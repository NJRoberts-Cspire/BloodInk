using Godot;

namespace BloodInk.Abilities;

/// <summary>
/// Shadow Step — short-range teleport between shadow zones.
/// Costs 5s cooldown. Player must be in or near a shadow zone to activate.
/// Teleports to the nearest other shadow zone within range.
/// </summary>
public partial class ShadowStepAbility : AbilityBase
{
    [Export] public float TeleportRange { get; set; } = 200f;

    public override void _Ready()
    {
        AbilityId = "shadow_step";
        Cooldown = 5f;
        GD.Print("[ShadowStep] Ability ready.");
    }

    protected override void Activate()
    {
        var player = Owner2D;
        if (player == null) return;

        // Find all shadow zones in the scene
        var zones = GetTree().GetNodesInGroup("ShadowZone");
        Node2D? nearest = null;
        float nearestDist = float.MaxValue;

        foreach (var node in zones)
        {
            if (node is not Node2D zone) continue;
            float dist = player.GlobalPosition.DistanceTo(zone.GlobalPosition);
            if (dist > 16f && dist < TeleportRange && dist < nearestDist)
            {
                nearestDist = dist;
                nearest = zone;
            }
        }

        if (nearest != null)
        {
            player.GlobalPosition = nearest.GlobalPosition;
            GD.Print($"[ShadowStep] Teleported to {nearest.Name} at {nearest.GlobalPosition}");
            // Instant ability — notify listeners the effect is complete.
            ExpireAbility();
        }
        else
        {
            // No valid destination — refund the cooldown so the player is not penalised.
            CancelCooldown();
            GD.Print("[ShadowStep] No target shadow zone in range — cooldown refunded.");
        }
    }
}
