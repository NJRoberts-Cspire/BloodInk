using Godot;
using BloodInk.Tools;

namespace BloodInk.Enemies;

/// <summary>
/// A ranged enemy that keeps its distance and fires crossbow bolts at the player.
/// Uses a simple inline state machine (no DetectionSensor — sight is distance-based).
///
/// States
/// ───────
///  Idle      → player enters AggroRange  → Reposition
///  Reposition→ within PreferredRange and has line-of-sight → Attack
///  Attack    → fires bolt, enters Cooldown
///  Cooldown  → waits AttackCooldown seconds → back to Reposition
///  Flee      → player too close (FleeRange) → back away until at PreferredRange
/// </summary>
public partial class CrossbowEnemy : EnemyBase
{
    /// <summary>Distance at which the crossbowman first notices the player.</summary>
    [Export] public float AggroRange { get; set; } = 180f;

    /// <summary>Ideal distance to maintain from the player.</summary>
    [Export] public float PreferredRange { get; set; } = 110f;

    /// <summary>If the player gets closer than this the crossbowman backs away.</summary>
    [Export] public float FleeRange { get; set; } = 60f;

    /// <summary>Seconds between shots.</summary>
    [Export] public float AttackCooldown { get; set; } = 1.8f;

    /// <summary>Wind-up time before the bolt actually fires (telegraphs the attack).</summary>
    [Export] public float WindupTime { get; set; } = 0.5f;

    /// <summary>Bolt speed in pixels/second.</summary>
    [Export] public float BoltSpeed { get; set; } = 200f;

    /// <summary>Bolt damage.</summary>
    [Export] public int BoltDamage { get; set; } = 1;

    // ─── Internal state machine ────────────────────────────────────

    private enum CbState { Idle, Reposition, Windup, Cooldown, Flee }
    private CbState _state = CbState.Idle;
    private float _stateTimer;

    // Telegraph tint during windup.
    private static readonly Color WindupTint = new(1f, 0.6f, 0.1f, 1f);

    protected override void EnemyReady()
    {
        // Apply crossbowman placeholder sprite if none are set.
        var frames = PlaceholderSprites.GetFrames("crossbowman_frames")
                  ?? PlaceholderSprites.GetFrames("guard_frames");
        if (frames != null && AnimPlayer.SpriteFrames == null)
        {
            AnimPlayer.SpriteFrames = frames;
            AnimPlayer.Play("idle");
        }

        MoveSpeed = 55f;   // Slower than a melee guard — relies on keeping distance.
        DetectRange = AggroRange;
        AttackRange = PreferredRange;
    }

    public override void _PhysicsProcess(double delta)
    {
        ApplyKnockback(delta);

        switch (_state)
        {
            case CbState.Idle:      TickIdle(delta);       break;
            case CbState.Reposition:TickReposition(delta); break;
            case CbState.Windup:    TickWindup(delta);     break;
            case CbState.Cooldown:  TickCooldown(delta);   break;
            case CbState.Flee:      TickFlee(delta);       break;
        }

        MoveAndSlide();
    }

    // ─── State ticks ──────────────────────────────────────────────

    private void TickIdle(double delta)
    {
        PlayAnimation("idle");
        Velocity = Velocity.MoveToward(Vector2.Zero, Friction * (float)delta);

        if (Target == null) return;
        if (DistanceToTarget() <= AggroRange)
            SetState(CbState.Reposition);
    }

    private void TickReposition(double delta)
    {
        if (Target == null) { SetState(CbState.Idle); return; }

        float dist = DistanceToTarget();

        // Player left aggro range — go idle.
        if (dist > AggroRange * 1.3f)
        {
            SetState(CbState.Idle);
            return;
        }

        // Too close — flee first.
        if (dist < FleeRange)
        {
            SetState(CbState.Flee);
            return;
        }

        // At preferred range and has rough line-of-sight — begin windup.
        if (dist <= PreferredRange && dist >= FleeRange)
        {
            SetState(CbState.Windup);
            return;
        }

        // Move toward preferred range.
        var dir = DirectionToTarget();
        Velocity = Velocity.MoveToward(dir * MoveSpeed, Acceleration * (float)delta);
        AnimPlayer.FlipH = dir.X < 0;
        PlayAnimation("run");
    }

    private void TickWindup(double delta)
    {
        // Stand still and telegraph.
        Velocity = Velocity.MoveToward(Vector2.Zero, Friction * (float)delta);

        _stateTimer -= (float)delta;
        if (_stateTimer <= 0f)
            FireBolt();
    }

    private void TickCooldown(double delta)
    {
        Velocity = Velocity.MoveToward(Vector2.Zero, Friction * (float)delta);
        PlayAnimation("idle");

        _stateTimer -= (float)delta;
        if (_stateTimer <= 0f)
            SetState(CbState.Reposition);
    }

    private void TickFlee(double delta)
    {
        if (Target == null) { SetState(CbState.Idle); return; }

        float dist = DistanceToTarget();
        if (dist >= PreferredRange)
        {
            SetState(CbState.Reposition);
            return;
        }

        // Back away from player.
        var dir = -DirectionToTarget();
        Velocity = Velocity.MoveToward(dir * MoveSpeed, Acceleration * (float)delta);
        AnimPlayer.FlipH = dir.X < 0;
        PlayAnimation("run");
    }

    // ─── Helpers ──────────────────────────────────────────────────

    private void SetState(CbState next)
    {
        _state = next;
        _stateTimer = next switch
        {
            CbState.Windup   => WindupTime,
            CbState.Cooldown => AttackCooldown,
            _ => 0f,
        };

        if (next == CbState.Windup)
        {
            PlayAnimation("attack");
            Modulate = WindupTint;
        }
        else
        {
            Modulate = Colors.White;
        }
    }

    private void FireBolt()
    {
        Modulate = Colors.White;

        if (Target != null && IsInstanceValid(Target))
        {
            // Spawn the bolt as a sibling so it isn't parented to this enemy.
            var parent = GetParent() ?? GetTree().CurrentScene;
            CrossbowBolt.Spawn(parent, GlobalPosition, Target.GlobalPosition,
                BoltSpeed, BoltDamage);
        }

        SetState(CbState.Cooldown);
    }

    protected override void OnDied()
    {
        Stealth.NoisePropagator.Instance?.PropagateNoise(GlobalPosition, 50f);
        base.OnDied();
    }
}
