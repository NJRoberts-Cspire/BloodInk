using Godot;
using BloodInk.Combat;
using BloodInk.Stealth;

namespace BloodInk.Enemies;

/// <summary>
/// Guard enemy — a stealth-aware enemy with a DetectionSensor, patrol route support,
/// and AI states that handle investigation, searching, and alert communication.
/// Extends EnemyBase with all the wiring needed for the stealth system.
/// </summary>
public partial class GuardEnemy : EnemyBase
{
    [ExportGroup("Guard")]
    [Export] public float PatrolSpeed { get; set; } = 40f;
    [Export] public float AlertedSpeed { get; set; } = 70f;
    [Export] public float ChaseSpeed { get; set; } = 90f;

    /// <summary>Time spent investigating a noise or last-seen position.</summary>
    [Export] public float InvestigateTime { get; set; } = 5f;

    /// <summary>Time spent searching after losing the player.</summary>
    [Export] public float SearchTime { get; set; } = 8f;

    /// <summary>Distance at which this guard yells to alert nearby guards.</summary>
    [Export] public float AlertCallRadius { get; set; } = 200f;

    // ─── Node References ──────────────────────────────────────────

    public DetectionSensor? Sensor { get; private set; }
    public PatrolRoute? Patrol { get; private set; }

    /// <summary>The direction the guard is currently facing (for vision cone).</summary>
    public Vector2 GuardFacingDirection { get; set; } = Vector2.Down;

    /// <summary>Whether this guard has called for backup during current alert.</summary>
    public bool HasCalledBackup { get; set; } = false;

    protected override void EnemyReady()
    {
        Sensor = GetNodeOrNull<DetectionSensor>("DetectionSensor");
        Patrol = GetNodeOrNull<PatrolRoute>("PatrolRoute");

        // Register sensor with the noise propagator.
        if (Sensor != null)
        {
            NoisePropagator.Instance?.RegisterSensor(Sensor);

            // Wire sensor signals.
            Sensor.PlayerDetected += OnPlayerDetected;
            Sensor.PlayerLost += OnPlayerLost;
            Sensor.NoiseHeard += OnNoiseHeard;
        }

        // Add to "guards" group for inter-guard communication.
        AddToGroup("Guards");
    }

    public override void _ExitTree()
    {
        if (Sensor != null)
            NoisePropagator.Instance?.UnregisterSensor(Sensor);
    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);

        // Keep sensor facing direction synced to actual movement direction.
        if (Sensor != null)
        {
            if (Velocity.LengthSquared() > 1f)
                GuardFacingDirection = Velocity.Normalized();
            Sensor.FacingDirection = GuardFacingDirection;
            Sensor.QueueRedraw(); // Update debug draw.

            // Flip sprite based on horizontal velocity.
            if (Mathf.Abs(Velocity.X) > 1f)
                AnimPlayer.FlipH = Velocity.X < 0;
        }
    }

    // ─── Signal Handlers ──────────────────────────────────────────

    private void OnPlayerDetected(Node2D player, int awarenessLevel)
    {
        Target = player;

        // Report detection to mission alert system.
        if ((AwarenessLevel)awarenessLevel >= AwarenessLevel.Alerted)
            Stealth.MissionAlertManager.Instance?.ReportDetection();

        // Alert nearby guards when first engaged.
        if ((AwarenessLevel)awarenessLevel == AwarenessLevel.Engaged && !HasCalledBackup)
        {
            CallBackup();
        }
    }

    private void OnPlayerLost()
    {
        // Don't clear target — let the search state use last known position.
    }

    private void OnNoiseHeard(Vector2 noisePosition)
    {
        // Handled by the AI states checking Sensor.HasPendingNoise.
    }

    // ─── Guard Communication ──────────────────────────────────────

    /// <summary>
    /// Alert nearby guards to the player's position.
    /// </summary>
    public void CallBackup()
    {
        HasCalledBackup = true;

        // Use noise propagator to alert nearby guards.
        NoisePropagator.Instance?.PropagateNoise(GlobalPosition, AlertCallRadius);

        // Also directly alert guards in the group within radius.
        foreach (var node in GetTree().GetNodesInGroup("Guards"))
        {
            if (node is GuardEnemy guard && guard != this)
            {
                float dist = GlobalPosition.DistanceTo(guard.GlobalPosition);
                if (dist <= AlertCallRadius && guard.Sensor != null)
                {
                    guard.Sensor.ForceEngage();
                    guard.Target = Target;
                    GD.Print($"Guard {Name} called backup to {guard.Name}");
                }
            }
        }
    }

    /// <summary>Update facing direction based on velocity.</summary>
    public void UpdateFacingFromVelocity()
    {
        if (Velocity.LengthSquared() > 1f)
        {
            GuardFacingDirection = Velocity.Normalized();
        }
    }

    protected override void OnDied()
    {
        // Guards drop ink and can trigger narrative events.
        // Noise from death (body hitting the ground).
        NoisePropagator.Instance?.PropagateNoise(GlobalPosition, 60f);

        base.OnDied();
    }
}
