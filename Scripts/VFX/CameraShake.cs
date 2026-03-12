using Godot;

namespace BloodInk.VFX;

/// <summary>
/// Camera shake system with smooth scrolling, look-ahead, and map limits.
/// Attach to a Camera2D that is a child of the player.
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

    // ─── Look-Ahead ──────────────────────────────────────────────

    /// <summary>How far ahead (px) the camera leads in the player's movement direction.</summary>
    [Export] public float LookAheadDistance { get; set; } = 40f;

    /// <summary>How quickly the look-ahead offset catches up (higher = snappier).</summary>
    [Export] public float LookAheadSmoothing { get; set; } = 3f;

    public static CameraShake? Instance { get; private set; }

    /// <summary>
    /// User-configurable intensity multiplier (0 = none, 1 = full).
    /// Set from SettingsPanel. Multiplied into shake offset and rotation.
    /// </summary>
    public static float IntensityMultiplier { get; set; } = 1f;

    private float _trauma;
    private float _noiseY;
    private Vector2 _lookAheadTarget;
    private Vector2 _lookAheadCurrent;

    public override void _Ready()
    {
        Instance = this;

        // Enable built-in smooth following so the camera glides toward the player.
        PositionSmoothingEnabled = true;
        PositionSmoothingSpeed = 5f;

        // Drag margins give a slight dead-zone so micro-movements don't jitter.
        DragHorizontalEnabled = true;
        DragVerticalEnabled = true;
        DragLeftMargin = 0.15f;
        DragRightMargin = 0.15f;
        DragTopMargin = 0.15f;
        DragBottomMargin = 0.15f;
    }

    public override void _ExitTree()
    {
        if (Instance == this) Instance = null;
    }

    public override void _Process(double delta)
    {
        float dt = (float)delta;

        // ── Look-Ahead ──────────────────────────────────────────
        var player = GetParentOrNull<CharacterBody2D>();
        if (player != null)
        {
            var vel = player.Velocity;
            if (vel.LengthSquared() > 100f) // Moving meaningfully
                _lookAheadTarget = vel.Normalized() * LookAheadDistance;
            else
                _lookAheadTarget = Vector2.Zero;
        }

        _lookAheadCurrent = _lookAheadCurrent.Lerp(_lookAheadTarget, LookAheadSmoothing * dt);

        // ── Shake ───────────────────────────────────────────────
        Vector2 shakeOffset = Vector2.Zero;
        float shakeRot = 0f;

        if (_trauma > 0)
        {
            _trauma = Mathf.Max(_trauma - TraumaDecayRate * dt, 0);
            _noiseY += NoiseSpeed * dt;
            float shake = _trauma * _trauma;

            shakeOffset = new Vector2(
                MaxOffset * shake * IntensityMultiplier * Mathf.Sin(_noiseY * 1.3f + 0.7f),
                MaxOffset * shake * IntensityMultiplier * Mathf.Cos(_noiseY * 1.7f + 1.1f)
            );
            shakeRot = Mathf.DegToRad(MaxRoll * shake * IntensityMultiplier * Mathf.Sin(_noiseY * 2.3f));
        }

        Offset = _lookAheadCurrent + shakeOffset;
        Rotation = shakeRot;
    }

    // ─── Camera Limits ────────────────────────────────────────────

    /// <summary>
    /// Set the camera's world-space bounding limits so it won't scroll past
    /// the edges of the map. Call this from the level script after building.
    /// </summary>
    public void SetLimits(int left, int top, int right, int bottom)
    {
        LimitLeft = left;
        LimitTop = top;
        LimitRight = right;
        LimitBottom = bottom;
        LimitSmoothed = true; // Smoothly approach the limit instead of hard-clamping.
    }

    /// <summary>Convenience overload using a Rect2I.</summary>
    public void SetLimits(Rect2I bounds)
    {
        SetLimits(bounds.Position.X, bounds.Position.Y,
                  bounds.Position.X + bounds.Size.X,
                  bounds.Position.Y + bounds.Size.Y);
    }

    // ─── Public Shake API ─────────────────────────────────────────

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
