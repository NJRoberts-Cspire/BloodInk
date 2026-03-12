using Godot;

namespace BloodInk.Stealth;

/// <summary>
/// Defines a floor surface type for a map area. When the player overlaps,
/// it sets the footstep audio surface AND applies a noise radius multiplier
/// to the player's StealthProfile, so stone floors are mechanically louder
/// than grass and affect enemy detection range.
/// Place as Area2D children in rooms/corridors.
/// </summary>
public partial class SurfaceZone : Area2D
{
    /// <summary>Surface type name — matches FootstepPlayer surface names.</summary>
    [Export] public string SurfaceType { get; set; } = "stone";

    /// <summary>
    /// Noise multiplier for detection while on this surface.
    /// grass=0.5, carpet=0.6, wood=1.0, stone=1.3, metal=1.5.
    /// </summary>
    [Export(PropertyHint.Range, "0.1,3.0,0.05")]
    public float NoiseMultiplier { get; set; } = 1.0f;

    private Audio.FootstepPlayer? _footsteps;
    private StealthProfile? _stealth;
    private float _previousMult = 1f;
    private bool _playerInside;

    public override void _Ready()
    {
        CollisionLayer = 0;
        CollisionMask = 1 << 1; // Player layer.
        Monitoring = true;
        Monitorable = false;

        BodyEntered += OnBodyEntered;
        BodyExited += OnBodyExited;
    }

    private void OnBodyEntered(Node2D body)
    {
        if (!body.IsInGroup("Player") || body is not CharacterBody2D player) return;
        _playerInside = true;

        _footsteps = player.GetNodeOrNull<Audio.FootstepPlayer>("FootstepPlayer");
        _footsteps?.SetSurface(SurfaceType);

        _stealth = player.GetNodeOrNull<StealthProfile>("StealthProfile");
        if (_stealth != null)
        {
            _previousMult = _stealth.NoiseMultiplier;
            _stealth.NoiseMultiplier = NoiseMultiplier;
        }
    }

    private void OnBodyExited(Node2D body)
    {
        if (!body.IsInGroup("Player")) return;
        _playerInside = false;

        // Restore to default rather than the stacked previous value —
        // overlapping surface zones use last-entered priority.
        _footsteps?.SetSurface("default");
        if (_stealth != null)
            _stealth.NoiseMultiplier = 1f;

        _footsteps = null;
        _stealth = null;
    }
}
