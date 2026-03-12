using Godot;

namespace BloodInk.VFX;

/// <summary>
/// Dust puff effect — spawned on quick direction changes, dodge starts,
/// landings, and wall impacts.
/// </summary>
public partial class DustPuff : Node2D
{
    private const float PuffLifetime = 0.4f;
    private const int ParticleCount = 5;

    /// <summary>Spawn a small dust puff at worldPos.</summary>
    public static void SpawnAt(Node parent, Vector2 worldPos, float intensity = 1f)
    {
        if (parent == null) return;
        int count = Mathf.RoundToInt(ParticleCount * intensity);

        for (int i = 0; i < count; i++)
        {
            float angle = (float)GD.RandRange(0, Mathf.Tau);
            float dist = (float)GD.RandRange(3, 10) * intensity;
            float size = (float)GD.RandRange(1, 2.5);
            float lifetime = (float)GD.RandRange(0.2, PuffLifetime);

            var particle = new ColorRect
            {
                Position = worldPos,
                Size = new Vector2(size, size),
                Color = new Color(0.55f, 0.5f, 0.4f, 0.5f), // Dusty brown.
                ZIndex = 0,
                MouseFilter = Control.MouseFilterEnum.Ignore,
            };
            parent.AddChild(particle);

            var endPos = worldPos + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * dist;

            var tween = particle.CreateTween();
            tween.SetParallel(true);
            tween.TweenProperty(particle, "position", endPos, lifetime)
                 .SetEase(Tween.EaseType.Out);
            tween.TweenProperty(particle, "modulate:a", 0.0f, lifetime);
            tween.SetParallel(false);
            tween.TweenCallback(Callable.From(() => particle.QueueFree()));
        }
    }
}
