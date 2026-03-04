using Godot;

namespace BloodInk.Stealth;

/// <summary>
/// An Area2D zone that reduces player visibility when inside.
/// Place these in the level (under shadows, dark alleys, foliage, etc.).
/// The player's StealthProfile reads ShadowZoneCount to determine visibility.
/// </summary>
[GlobalClass]
public partial class ShadowZone : Area2D
{
    /// <summary>
    /// How much this zone reduces detection. Stacks with other zones.
    /// Not used directly yet — the binary shadow count is enough for now.
    /// </summary>
    [Export] public float DarknessLevel { get; set; } = 1.0f;

    public override void _Ready()
    {
        // Ensure we detect the player (layer 2).
        CollisionLayer = 0;
        CollisionMask = 1 << 1; // Player is on layer 2.
        Monitoring = true;
        Monitorable = false;

        BodyEntered += OnBodyEntered;
        BodyExited += OnBodyExited;
    }

    private void OnBodyEntered(Node2D body)
    {
        if (body.GetNodeOrNull<StealthProfile>("StealthProfile") is StealthProfile profile)
        {
            profile.ShadowZoneCount++;
        }
    }

    private void OnBodyExited(Node2D body)
    {
        if (body.GetNodeOrNull<StealthProfile>("StealthProfile") is StealthProfile profile)
        {
            profile.ShadowZoneCount = Mathf.Max(0, profile.ShadowZoneCount - 1);
        }
    }
}
