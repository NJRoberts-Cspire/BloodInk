using Godot;
using BloodInk.Core;
using BloodInk.Stealth;

namespace BloodInk.Enemies.States;

/// <summary>
/// Guard is alerted — has spotted the player but hasn't reached full engagement.
/// Moves cautiously toward the last known position, weapon drawn.
/// Higher detection gain rate in this state.
/// </summary>
public partial class GuardAlertState : State
{
    private GuardEnemy _guard = null!;
    private float _alertTimer;

    public override void Init()
    {
        _guard = GetOwner<GuardEnemy>();
    }

    public override void Enter()
    {
        _guard.AnimPlayer.Play("run");
        _alertTimer = 4f; // Max time in alert before de-escalating.
        GD.Print($"Guard {_guard.Name} is ALERTED!");
    }

    public override void PhysicsUpdate(double delta)
    {
        if (_guard.Sensor == null)
        {
            Machine?.TransitionTo("Patrol");
            return;
        }

        // Escalate to chase if fully engaged.
        if (_guard.Sensor.CurrentAwareness >= AwarenessLevel.Engaged)
        {
            Machine?.TransitionTo("Chase");
            return;
        }

        // De-escalate if awareness drops.
        if (_guard.Sensor.CurrentAwareness <= AwarenessLevel.Unaware)
        {
            Machine?.TransitionTo("Patrol");
            return;
        }

        _alertTimer -= (float)delta;

        // Move toward last known position cautiously.
        var target = _guard.Sensor.LastKnownPlayerPosition;
        var toTarget = target - _guard.GlobalPosition;
        float dist = toTarget.Length();

        if (dist > 10f)
        {
            var dir = toTarget.Normalized();
            _guard.Velocity = _guard.Velocity.MoveToward(
                dir * _guard.AlertedSpeed,
                _guard.Acceleration * (float)delta
            );
            _guard.GuardFacingDirection = dir;
            _guard.AnimPlayer.FlipH = dir.X < 0;
        }
        else
        {
            _guard.Velocity = _guard.Velocity.MoveToward(Vector2.Zero, _guard.Friction * (float)delta);
        }

        _guard.MoveAndSlide();

        // Timeout — switch to investigate.
        if (_alertTimer <= 0f)
        {
            Machine?.TransitionTo("Investigate");
        }
    }
}
