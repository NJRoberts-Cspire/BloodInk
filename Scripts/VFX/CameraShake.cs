using Godot;

namespace BloodInk.VFX;

/// <summary>
/// Camera shake system. Attach to a Camera2D (or access via singleton).
/// Uses a trauma/decay model: add trauma on hits, trauma² drives shake intensity.
/// </summary>
public partial class CameraShake : Camera2D
{
    /// <summary>Maximum pixel offset for shake.</summary>
    [Export] public float MaxOffset { get; set; } = 8f;

    /// <summary>Maximum rotation for shake (degrees).</summary>
    [Export] public float MaxRoll { get; set; } = 2f;

    /// <summary>How fast trauma decays per second.</summary>
    [Export] public float TraumaDecayRate { get; set; } = 1.5f;

    /// <summary>Noise frequency — higher = more jittery.</summary>
    [Export] public float NoiseSpeed { get; set; } = 30f;

    public static CameraShake? Instance { get; private set; }

    private float _trauma;
    private float _noiseY;

    public override void _Ready()
    {
        Instance = this;
    }

    public override void _Process(double delta)
    {
        if (_trauma <= 0)
        {
            Offset = Vector2.Zero;
            Rotation = 0;
            return;
        }

        _trauma = Mathf.Max(_trauma - TraumaDecayRate * (float)delta, 0);

        _noiseY += NoiseSpeed * (float)delta;
        float shake = _trauma * _trauma; // Quadratic falloff.

        Offset = new Vector2(
            MaxOffset * shake * (float)(Mathf.Sin(_noiseY * 1.3f + 0.7f)),
            MaxOffset * shake * (float)(Mathf.Cos(_noiseY * 1.7f + 1.1f))
        );

        Rotation = Mathf.DegToRad(MaxRoll * shake * (float)Mathf.Sin(_noiseY * 2.3f));
    }

    // ─── Public API ───────────────────────────────────────────────

    /// <summary>Add trauma (0-1). Values are clamped and stack additively.</summary>
    public void AddTrauma(float amount)
    {
        _trauma = Mathf.Min(_trauma + amount, 1f);
    }

    /// <summary>Shortcut: light shake (footstep, minor hit).</summary>
    public void ShakeLight() => AddTrauma(0.15f);

    /// <summary>Shortcut: medium shake (player hit, attack landing).</summary>
    public void ShakeMedium() => AddTrauma(0.35f);

    /// <summary>Shortcut: heavy shake (stealth kill, boss attack, explosion).</summary>
    public void ShakeHeavy() => AddTrauma(0.6f);

    /// <summary>Shortcut: screen-filling shake (death, phase transition).</summary>
    public void ShakeExtreme() => AddTrauma(0.9f);
}
