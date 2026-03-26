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
    [Export] public float PatrolSpeed { get; set; } = 55f;
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

        // Wire sensor signals.
        // Note: DetectionSensor._Ready() already registers itself with NoisePropagator.
        if (Sensor != null)
        {
            Sensor.PlayerDetected += OnPlayerDetected;
            Sensor.PlayerLost += OnPlayerLost;
            Sensor.NoiseHeard += OnNoiseHeard;
        }

        // Add to "guards" group for inter-guard communication.
        AddToGroup("Guards");
    }

    public override void _ExitTree()
    {
        // DetectionSensor._ExitTree() handles its own NoisePropagator unregistration.
        // Disconnect signals to prevent stale callbacks after the guard is freed.
        if (Sensor != null)
        {
            Sensor.PlayerDetected -= OnPlayerDetected;
            Sensor.PlayerLost -= OnPlayerLost;
            Sensor.NoiseHeard -= OnNoiseHeard;
        }
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

        // Do NOT call CallBackup() here. When awareness reaches Engaged the state
        // machine transitions to Chase, which redirects to the Backup state on first
        // detection. GuardBackupState.Enter() calls CallBackup() at the right moment
        // (after the guard stops and plays the callout animation). Calling it here
        // would set HasCalledBackup = true before Chase checks it, permanently
        // skipping the Backup state.
    }

    private void OnPlayerLost()
    {
        // Don't clear target — let the search state use last known position.
    }

    private void OnNoiseHeard(Vector2 noisePosition)
    {
        // Handled by the AI states checking Sensor.HasPendingNoise.
    }

    // ─── Corpse Discovery ─────────────────────────────────────────

    /// <summary>
    /// Called by <see cref="Stealth.CorpseMarker"/> via signal when this guard
    /// discovers a body. Handles sensor alert boost and mission reporting.
    /// </summary>
    /// <param name="_discoverer">The guard that found the body (this guard).</param>
    /// <param name="corpsePosition">World position of the discovered corpse.</param>
    public void OnCorpseDiscovered(Node2D _discoverer, Vector2 corpsePosition)
    {
        // Boost sensor awareness so the guard immediately investigates the corpse location.
        if (Sensor != null)
        {
            Sensor.LastHeardNoisePosition = corpsePosition;
            Sensor.HasPendingNoise = true;
            Sensor.OnNoiseAtPosition(corpsePosition, 60f);
        }

        // Report corpse to mission alert system.
        Stealth.MissionAlertManager.Instance?.ReportCorpseFound();
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

        // Directly alert guards whose sensors are within radius — spatial query
        // via NoisePropagator avoids scanning every node in the "Guards" group.
        var propagator = NoisePropagator.Instance;
        if (propagator != null)
        {
            foreach (var sensor in propagator.GetSensorsInRadius(GlobalPosition, AlertCallRadius))
            {
                var guard = sensor.GetParent<GuardEnemy>();
                if (guard == null || guard == this) continue;

                sensor.ForceEngage();
                guard.Target = Target;
                GD.Print($"Guard {Name} called backup to {guard.Name}");
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
