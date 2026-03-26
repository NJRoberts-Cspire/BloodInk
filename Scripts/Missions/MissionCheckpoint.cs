using Godot;

namespace BloodInk.Missions;

/// <summary>
/// Invisible zone-boundary checkpoint. Place at the transition between two zones.
/// When the player walks through, emits CheckpointReached once and deactivates.
/// No visual, no sound — completely transparent to the player.
/// </summary>
public partial class MissionCheckpoint : Area2D
{
    [Signal]
    public delegate void CheckpointReachedEventHandler(int checkpointIndex, Vector2 respawnPos);

    /// <summary>Monotonically increasing index — higher index = further into the mission.</summary>
    public int CheckpointIndex { get; set; }

    /// <summary>
    /// World-space position the player will respawn at if they die after hitting this checkpoint.
    /// Should be just inside the zone this checkpoint guards — not the boundary itself.
    /// </summary>
    public Vector2 RespawnPos { get; set; }

    private bool _triggered;

    public override void _Ready()
    {
        // Invisible — no collision layer for rendering, player mask only.
        CollisionLayer = 0;
        CollisionMask  = 1 << 1; // Player layer.
        Monitorable    = false;
        Monitoring     = true;

        // Full-width trigger: caller sets the shape size via AddCheckpoint helper.
        BodyEntered += OnBodyEntered;
    }

    private void OnBodyEntered(Node2D body)
    {
        if (_triggered) return;
        if (!body.IsInGroup("Player")) return;

        _triggered = true;
        SetDeferred(Area2D.PropertyName.Monitoring, false);

        EmitSignal(SignalName.CheckpointReached, CheckpointIndex, RespawnPos);
        GD.Print($"[Checkpoint {CheckpointIndex}] Reached — respawn locked to {RespawnPos}.");
    }
}
