using Godot;

namespace BloodInk.VFX;

/// <summary>
/// Death effect for enemies — flash, expand, then dissolve into particles.
/// Replaces the abrupt QueueFree() with a dramatic visual.
/// </summary>
public partial class DeathEffect : Node2D
{
    /// <summary>Duration of the death effect.</summary>
    private const float EffectDuration = 0.6f;

    /// <summary>Spawn a death effect at an enemy's position, replacing its visual.</summary>
    public static void SpawnAt(Node parent, Vector2 worldPos, AnimatedSprite2D? sourceSprite = null)
    {
        if (parent == null) return;
        var effect = new DeathEffect { GlobalPosition = worldPos, ZIndex = 5 };
        parent.AddChild(effect);

        // Spawn a frozen frame of the enemy if we have the sprite.
        if (sourceSprite?.SpriteFrames != null)
        {
            var ghost = new Sprite2D
            {
                Texture = sourceSprite.SpriteFrames.GetFrameTexture(
                    sourceSprite.Animation, sourceSprite.Frame),
                FlipH = sourceSprite.FlipH,
                Modulate = new Color(1f, 0.3f, 0.2f, 1f), // Red flash.
            };
            effect.AddChild(ghost);

            // Flash white → expand + fade.
            var tween = effect.CreateTween();
            tween.TweenProperty(ghost, "modulate", Colors.White, 0.08f);
            tween.TweenProperty(ghost, "modulate:a", 0.0f, EffectDuration - 0.08f)
                 .SetEase(Tween.EaseType.In);
            tween.Parallel().TweenProperty(ghost, "scale",
                new Vector2(1.5f, 1.5f), EffectDuration)
                 .SetEase(Tween.EaseType.Out);
        }

        // Spawn dissolve particles (small colored squares spreading out).
        SpawnDissolveParticles(effect, worldPos);

        // Clean up after effect completes.
        var cleanup = effect.CreateTween();
        cleanup.TweenInterval(EffectDuration + 0.2f);
        cleanup.TweenCallback(Callable.From(() => effect.QueueFree()));
    }

    private static void SpawnDissolveParticles(Node parent, Vector2 center)
    {
        int count = 10;
        for (int i = 0; i < count; i++)
        {
            float angle = (float)GD.RandRange(0, Mathf.Tau);
            float dist = (float)GD.RandRange(8, 28);
            float size = (float)GD.RandRange(1.5, 3.5);
            float lifetime = (float)GD.RandRange(0.3, 0.7);

            Color color = (i % 3) switch
            {
                0 => new Color(0.8f, 0.15f, 0.1f, 1f),  // Red.
                1 => new Color(0.2f, 0.2f, 0.2f, 1f),    // Dark grey.
                _ => new Color(0.5f, 0.35f, 0.15f, 1f),   // Brown.
            };

            var particle = new ColorRect
            {
                Position = center - new Vector2(size / 2, size / 2),
                Size = new Vector2(size, size),
                Color = color,
                ZIndex = 6,
                MouseFilter = Control.MouseFilterEnum.Ignore,
            };

            parent.AddChild(particle);

            var endPos = center + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * dist;

            var tween = particle.CreateTween();
            tween.SetParallel(true);
            tween.TweenProperty(particle, "position", endPos, lifetime)
                 .SetEase(Tween.EaseType.Out)
                 .SetTrans(Tween.TransitionType.Quad);
            tween.TweenProperty(particle, "modulate:a", 0.0f, lifetime);
            tween.TweenProperty(particle, "rotation",
                (float)GD.RandRange(-Mathf.Pi, Mathf.Pi), lifetime);
            tween.SetParallel(false);
            tween.TweenCallback(Callable.From(() => particle.QueueFree()));
        }
    }
}
