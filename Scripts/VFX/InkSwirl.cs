using Godot;

namespace BloodInk.VFX;

/// <summary>
/// Ink swirl VFX — plays when a tattoo is applied at the Ink Tent.
/// Spiralling ink-colored particles converge on a body-part location.
/// </summary>
public partial class InkSwirl : Node2D
{
    /// <summary>Duration of the full swirl effect.</summary>
    private const float SwirlDuration = 1.2f;

    /// <summary>Number of ink particles.</summary>
    private const int ParticleCount = 16;

    /// <summary>Starting radius of the particle ring.</summary>
    private const float StartRadius = 40f;

    /// <summary>
    /// Spawn an ink-swirl effect at a world position.
    /// Color reflects the tattoo's ink type.
    /// </summary>
    public static void SpawnAt(Node parent, Vector2 worldPos, Color inkColor)
    {
        var swirl = new InkSwirl { GlobalPosition = worldPos, ZIndex = 15 };
        parent.AddChild(swirl);

        for (int i = 0; i < ParticleCount; i++)
        {
            float startAngle = (float)i / ParticleCount * Mathf.Tau;
            float delay = (float)i / ParticleCount * 0.3f; // Staggered start.

            var startPos = new Vector2(
                Mathf.Cos(startAngle) * StartRadius,
                Mathf.Sin(startAngle) * StartRadius
            );

            float size = (float)GD.RandRange(1.5, 3);

            // Slightly vary the ink color per particle.
            var c = inkColor;
            c.R += (float)GD.RandRange(-0.1, 0.1);
            c.G += (float)GD.RandRange(-0.1, 0.1);
            c.B += (float)GD.RandRange(-0.05, 0.05);

            var particle = new ColorRect
            {
                Position = startPos,
                Size = new Vector2(size, size),
                Color = c,
                MouseFilter = Control.MouseFilterEnum.Ignore,
            };
            swirl.AddChild(particle);

            // Spiral inward: converge to center while rotating.
            var tween = particle.CreateTween();
            tween.TweenInterval(delay);

            float duration = SwirlDuration - delay;
            float midAngle = startAngle + Mathf.Pi; // Half revolution.
            var midPos = new Vector2(
                Mathf.Cos(midAngle) * StartRadius * 0.4f,
                Mathf.Sin(midAngle) * StartRadius * 0.4f
            );

            tween.TweenProperty(particle, "position", midPos, duration * 0.6f)
                 .SetEase(Tween.EaseType.InOut)
                 .SetTrans(Tween.TransitionType.Sine);
            tween.TweenProperty(particle, "position", Vector2.Zero, duration * 0.4f)
                 .SetEase(Tween.EaseType.In)
                 .SetTrans(Tween.TransitionType.Back);
            tween.Parallel().TweenProperty(particle, "modulate:a", 0.0f, duration * 0.3f)
                 .SetDelay(duration * 0.7f);
        }

        // Final flash at center when particles converge.
        var flash = swirl.CreateTween();
        flash.TweenInterval(SwirlDuration * 0.85f);
        flash.TweenCallback(Callable.From(() =>
        {
            var glow = new ColorRect
            {
                Position = new Vector2(-6, -6),
                Size = new Vector2(12, 12),
                Color = Colors.White,
                MouseFilter = Control.MouseFilterEnum.Ignore,
            };
            swirl.AddChild(glow);

            var gt = glow.CreateTween();
            gt.TweenProperty(glow, "modulate:a", 0.0f, 0.3f);
            gt.TweenProperty(glow, "scale", new Vector2(3, 3), 0.3f);
        }));

        // Cleanup.
        var cleanup = swirl.CreateTween();
        cleanup.TweenInterval(SwirlDuration + 0.5f);
        cleanup.TweenCallback(Callable.From(() => swirl.QueueFree()));
    }

    /// <summary>Get ink color by tattoo slot/type.</summary>
    public static Color GetInkColor(string slotName)
    {
        return slotName.ToLower() switch
        {
            "shadow" or "arms" => new Color(0.15f, 0.1f, 0.25f),    // Deep purple.
            "fang" or "chest"  => new Color(0.7f, 0.12f, 0.08f),    // Blood red.
            "vein" or "legs"   => new Color(0.1f, 0.3f, 0.6f),      // Blue.
            "skull" or "head"  => new Color(0.6f, 0.55f, 0.4f),     // Bone/gold.
            "spine" or "back"  => new Color(0.3f, 0.3f, 0.3f),      // Iron grey.
            "whisper" or "hands" => new Color(0.35f, 0.5f, 0.3f),   // Moss green.
            _ => new Color(0.2f, 0.05f, 0.05f)                       // Default dark ink.
        };
    }
}
