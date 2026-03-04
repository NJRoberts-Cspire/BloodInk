using Godot;

namespace BloodInk.VFX;

/// <summary>
/// Blood splatter effect — spawns small blood particle sprites on hit.
/// Called when damage is dealt. Directional splatter based on knockback.
/// </summary>
public partial class BloodSplatter : Node
{
    /// <summary>Number of blood particles per splatter.</summary>
    [Export] public int ParticleCount { get; set; } = 6;

    /// <summary>How far particles travel (pixels).</summary>
    [Export] public float SplatterDistance { get; set; } = 20f;

    /// <summary>How long particles persist before fading.</summary>
    [Export] public float Lifetime { get; set; } = 1.2f;

    /// <summary>Spread angle in degrees from the knockback direction.</summary>
    [Export] public float SpreadAngle { get; set; } = 45f;

    /// <summary>Blood colors — randomly selected per particle for variety.</summary>
    private static readonly Color[] BloodColors = new[]
    {
        new Color(0.6f, 0.05f, 0.05f),   // Dark red.
        new Color(0.7f, 0.1f, 0.08f),    // Crimson.
        new Color(0.5f, 0.02f, 0.02f),   // Very dark (orc blood).
        new Color(0.45f, 0.08f, 0.12f),  // Maroon.
    };

    // ─── Static Spawn API ─────────────────────────────────────────

    /// <summary>Spawn a directional blood splatter at worldPos.</summary>
    public static void Spawn(Node parent, Vector2 worldPos, Vector2 knockbackDir, int intensity = 6)
    {
        if (knockbackDir == Vector2.Zero)
            knockbackDir = Vector2.Up;

        float baseAngle = knockbackDir.Angle();

        for (int i = 0; i < intensity; i++)
        {
            float angle = baseAngle + (float)GD.RandRange(
                Mathf.DegToRad(-45), Mathf.DegToRad(45));
            float dist = (float)GD.RandRange(4, 20);
            float size = (float)GD.RandRange(1, 3);
            float lifetime = (float)GD.RandRange(0.6, 1.4);

            var color = BloodColors[GD.RandRange(0, BloodColors.Length - 1)];

            var particle = new ColorRect
            {
                Position = worldPos,
                Size = new Vector2(size, size),
                Color = color,
                ZIndex = 1,
                MouseFilter = Control.MouseFilterEnum.Ignore,
            };

            parent.AddChild(particle);

            var endPos = worldPos + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * dist;

            var tween = particle.CreateTween();
            tween.SetParallel(true);
            tween.TweenProperty(particle, "position", endPos, lifetime * 0.3f)
                 .SetEase(Tween.EaseType.Out)
                 .SetTrans(Tween.TransitionType.Quad);
            tween.TweenProperty(particle, "modulate:a", 0.0f, lifetime)
                 .SetDelay(lifetime * 0.5f);
            tween.SetParallel(false);
            tween.TweenCallback(Callable.From(() => particle.QueueFree()));
        }
    }

    /// <summary>Spawn a heavy splatter (stealth kill / death).</summary>
    public static void SpawnHeavy(Node parent, Vector2 worldPos, Vector2 dir)
    {
        Spawn(parent, worldPos, dir, 14);
    }
}
