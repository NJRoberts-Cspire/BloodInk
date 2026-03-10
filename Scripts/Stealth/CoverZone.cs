using Godot;

namespace BloodInk.Stealth;

/// <summary>
/// An Area2D zone that provides hard cover — makes the player Hidden.
/// Place behind walls, crates, pillars, tall grass, etc.
/// The player's StealthProfile reads CoverZoneCount to determine visibility.
/// </summary>
[GlobalClass]
public partial class CoverZone : Area2D
{
    /// <summary>Whether this cover can be destroyed (e.g. crates).</summary>
    [Export] public bool IsDestructible { get; set; } = false;

    /// <summary>Health of this cover if destructible.</summary>
    [Export] public int Health { get; set; } = 30;

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
        if (body.GetNodeOrNull<StealthProfile>("StealthProfile") is StealthProfile profile)
        {
            profile.CoverZoneCount++;
        }
    }

    private void OnBodyExited(Node2D body)
    {
        if (body.GetNodeOrNull<StealthProfile>("StealthProfile") is StealthProfile profile)
        {
            profile.CoverZoneCount = Mathf.Max(0, profile.CoverZoneCount - 1);
        }
    }

    /// <summary>Damage this cover piece. When health reaches 0, it's destroyed.</summary>
    public void TakeDamage(int damage)
    {
        if (!IsDestructible) return;
        Health -= damage;
        if (Health <= 0)
        {
            GD.Print("Cover destroyed!");
            // Decrement CoverZoneCount for any players still inside before freeing.
            foreach (var body in GetOverlappingBodies())
            {
                if (body.GetNodeOrNull<StealthProfile>("StealthProfile") is StealthProfile profile)
                {
                    profile.CoverZoneCount = Mathf.Max(0, profile.CoverZoneCount - 1);
                }
            }
            QueueFree();
        }
    }
}
