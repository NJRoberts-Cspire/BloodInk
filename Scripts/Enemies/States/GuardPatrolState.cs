using Godot;
using BloodInk.Core;
using BloodInk.Stealth;

namespace BloodInk.Enemies.States;

/// <summary>
/// Guard patrols along a PatrolRoute. Transitions to Investigate on noise,
/// or Alert/Chase when the player is detected.
/// </summary>
public partial class GuardPatrolState : State
{
    private GuardEnemy _guard = null!;
    private float _waitTimer = 0f;
    private bool _isWaiting = false;
    private Vector2 _currentWaypoint;

    public override void Init()
    {
        _guard = GetOwner<GuardEnemy>();
    }

    public override void Enter()
    {
        _guard.AnimPlayer.Play("run");
        _isWaiting = false;
        _waitTimer = 0f;

        if (_guard.Patrol != null && _guard.Patrol.Count > 0)
        {
            _currentWaypoint = _guard.Patrol.GetCurrentWaypoint();
        }
        else
        {
            // No patrol route — just idle in place.
            Machine?.TransitionTo("Idle");
        }
    }

    public override void PhysicsUpdate(double delta)
    {
        // Check detection sensor for escalation.
        if (CheckDetection()) return;

        // Check for noise.
        if (CheckNoise()) return;

        if (_isWaiting)
        {
            _waitTimer -= (float)delta;
            _guard.Velocity = _guard.Velocity.MoveToward(Vector2.Zero, _guard.Friction * (float)delta);
            _guard.MoveAndSlide();

            if (_waitTimer <= 0f)
            {
                _isWaiting = false;
                _currentWaypoint = _guard.Patrol!.AdvanceToNext();
                _guard.AnimPlayer.Play("run");
            }
            return;
        }

        // Move toward current waypoint.
        var toWaypoint = _currentWaypoint - _guard.GlobalPosition;
        float dist = toWaypoint.Length();

        if (dist < 5f)
        {
            // Arrived at waypoint — wait.
            _isWaiting = true;
            _waitTimer = _guard.Patrol!.WaitTimeAtPoint;
            _guard.AnimPlayer.Play("idle");

            // Face a natural direction at the waypoint.
            return;
        }

        var dir = toWaypoint.Normalized();
        _guard.Velocity = _guard.Velocity.MoveToward(
            dir * _guard.PatrolSpeed,
            _guard.Acceleration * (float)delta
        );
        _guard.GuardFacingDirection = dir;
        _guard.AnimPlayer.FlipH = dir.X < 0;
        _guard.MoveAndSlide();
    }

    private bool CheckDetection()
    {
        if (_guard.Sensor == null) return false;

        return _guard.Sensor.CurrentAwareness switch
        {
            AwarenessLevel.Engaged => TransitionTo("Chase"),
            AwarenessLevel.Alerted => TransitionTo("Alert"),
            AwarenessLevel.Suspicious => TransitionTo("Investigate"),
            _ => false
        };
    }

    private bool CheckNoise()
    {
        if (_guard.Sensor?.HasPendingNoise == true)
        {
            _guard.Sensor.HasPendingNoise = false;
            return TransitionTo("Investigate");
        }
        return false;
    }

    private bool TransitionTo(string state)
    {
        Machine?.TransitionTo(state);
        return true;
    }
}
