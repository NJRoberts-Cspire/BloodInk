using Godot;

namespace BloodInk.VFX;

/// <summary>
/// Melee slash arc VFX — a crescent swipe rendered as a tween-animated sprite
/// in the attack direction. Spawned by attack states.
/// </summary>
public partial class SlashArc : Node2D
{
    private const float SlashDuration = 0.2f;

    /// <summary>
    /// Spawn a slash arc effect at the given position, sweeping in the facing direction.
    /// Uses a ColorRect fan since we don't have slash sprites yet.
    /// </summary>
    public static void SpawnAt(Node parent, Vector2 worldPos, Vector2 direction, bool isHeavy = false)
    {
        var arc = new Node2D
        {
            GlobalPosition = worldPos,
            Rotation = direction.Angle(),
            ZIndex = 8,
        };
        parent.AddChild(arc);

        int segments = isHeavy ? 5 : 3;
        float arcSpread = isHeavy ? 1.2f : 0.8f; // radians
        float length = isHeavy ? 22f : 16f;
        Color color = isHeavy
            ? new Color(0.9f, 0.2f, 0.15f, 0.8f)  // Red for heavy.
            : new Color(0.8f, 0.8f, 0.9f, 0.7f);   // White for normal.

        for (int i = 0; i < segments; i++)
        {
            float t = (float)i / (segments - 1) - 0.5f; // -0.5 to 0.5.
            float angle = t * arcSpread;

            var line = new ColorRect
            {
                Size = new Vector2(length, 1.5f),
                Position = Vector2.Zero,
                Rotation = angle,
                PivotOffset = new Vector2(0, 0.75f),
                Color = color,
                MouseFilter = Control.MouseFilterEnum.Ignore,
            };
            arc.AddChild(line);
        }

        // Animate: quick scale-up then fade.
        arc.Scale = new Vector2(0.3f, 0.6f);
        var tween = arc.CreateTween();
        tween.SetParallel(true);
        tween.TweenProperty(arc, "scale", new Vector2(1.2f, 1.0f), SlashDuration * 0.4f)
             .SetEase(Tween.EaseType.Out);
        tween.TweenProperty(arc, "modulate:a", 0.0f, SlashDuration)
             .SetDelay(SlashDuration * 0.3f);
        tween.SetParallel(false);
        tween.TweenCallback(Callable.From(() => arc.QueueFree()));
    }
}
