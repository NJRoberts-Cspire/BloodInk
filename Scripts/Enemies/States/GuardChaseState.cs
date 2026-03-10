using Godot;
using BloodInk.Core;
using BloodInk.Stealth;

namespace BloodInk.Enemies.States;

/// <summary>
/// Guard is fully engaged — chasing and attacking the player.
/// In this state the guard moves at full chase speed and attacks on contact.
/// If the player escapes, transitions to Search.
/// </summary>
public partial class GuardChaseState : State
{
    private GuardEnemy _guard = null!;

    public override void Init()
    {
        _guard = GetOwner<GuardEnemy>();
    }

    public override void Enter()
    {
        _guard.AnimPlayer.Play("run");
        GD.Print($"Guard {_guard.Name} is CHASING!");

        // Call for backup on first engagement only.
        if (!_guard.HasCalledBackup)
            _guard.CallBackup();
    }

    public override void PhysicsUpdate(double delta)
    {
        _guard.ApplyKnockback(delta);

        // Check if we've lost the player.
        if (_guard.Sensor != null)
        {
            if (_guard.Sensor.CurrentAwareness == AwarenessLevel.Searching)
            {
                Machine?.TransitionTo("Search");
                return;
            }
            if (_guard.Sensor.CurrentAwareness <= AwarenessLevel.Unaware)
            {
                Machine?.TransitionTo("Patrol");
                return;
            }
        }

        // Attack range check.
        float dist = _guard.DistanceToTarget();
        if (dist <= _guard.AttackRange)
        {
            Machine?.TransitionTo("Attack");
            return;
        }

        // Chase the player / last known position.
        Vector2 chaseTarget;
        if (_guard.Target != null && _guard.Sensor?.CanSeePlayer == true)
        {
            chaseTarget = _guard.Target.GlobalPosition;
        }
        else
        {
            chaseTarget = _guard.Sensor?.LastKnownPlayerPosition ?? _guard.GlobalPosition;
        }

        var toTarget = chaseTarget - _guard.GlobalPosition;
        if (toTarget.LengthSquared() > 4f)
        {
            var dir = toTarget.Normalized();
            _guard.Velocity = _guard.Velocity.MoveToward(
                dir * _guard.ChaseSpeed,
                _guard.Acceleration * (float)delta
            );
            _guard.GuardFacingDirection = dir;
            _guard.AnimPlayer.FlipH = dir.X < 0;
        }

        _guard.MoveAndSlide();
        _guard.UpdateFacingFromVelocity();
    }
}
