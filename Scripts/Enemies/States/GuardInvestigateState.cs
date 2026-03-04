using Godot;
using BloodInk.Core;
using BloodInk.Stealth;

namespace BloodInk.Enemies.States;

/// <summary>
/// Guard investigates a noise or brief sighting. Walks to the last known
/// position and looks around, then returns to patrol.
/// </summary>
public partial class GuardInvestigateState : State
{
    private GuardEnemy _guard = null!;
    private Vector2 _investigatePosition;
    private float _investigateTimer;
    private bool _arrived = false;
    private float _lookTimer = 0f;
    private int _lookPhase = 0;

    public override void Init()
    {
        _guard = GetOwner<GuardEnemy>();
    }

    public override void Enter()
    {
        _guard.AnimPlayer.Play("run");
        _arrived = false;
        _lookPhase = 0;
        _lookTimer = 0f;
        _investigateTimer = _guard.InvestigateTime;

        // Go to noise position or last known player position.
        var sensor = _guard.Sensor;
        if (sensor?.HasPendingNoise == true)
        {
            _investigatePosition = sensor.LastHeardNoisePosition;
            sensor.HasPendingNoise = false;
        }
        else if (sensor != null)
        {
            _investigatePosition = sensor.LastKnownPlayerPosition;
        }
        else
        {
            _investigatePosition = _guard.GlobalPosition;
        }

        GD.Print($"Guard {_guard.Name} investigating position {_investigatePosition}");
    }

    public override void PhysicsUpdate(double delta)
    {
        // Escalation check — if detection increases, transition up.
        if (_guard.Sensor != null)
        {
            if (_guard.Sensor.CurrentAwareness >= AwarenessLevel.Engaged)
            {
                Machine?.TransitionTo("Chase");
                return;
            }
            if (_guard.Sensor.CurrentAwareness >= AwarenessLevel.Alerted)
            {
                Machine?.TransitionTo("Alert");
                return;
            }

            // New noise redirects investigation.
            if (_guard.Sensor.HasPendingNoise)
            {
                _investigatePosition = _guard.Sensor.LastHeardNoisePosition;
                _guard.Sensor.HasPendingNoise = false;
                _arrived = false;
                _investigateTimer = _guard.InvestigateTime;
            }
        }

        _investigateTimer -= (float)delta;

        if (!_arrived)
        {
            // Walk to investigation position.
            var toTarget = _investigatePosition - _guard.GlobalPosition;
            float dist = toTarget.Length();

            if (dist < 8f)
            {
                _arrived = true;
                _lookTimer = 1.2f;
                _lookPhase = 0;
                _guard.AnimPlayer.Play("idle");
            }
            else
            {
                var dir = toTarget.Normalized();
                _guard.Velocity = _guard.Velocity.MoveToward(
                    dir * _guard.AlertedSpeed,
                    _guard.Acceleration * (float)delta
                );
                _guard.GuardFacingDirection = dir;
                _guard.AnimPlayer.FlipH = dir.X < 0;
                _guard.MoveAndSlide();
            }
        }
        else
        {
            // Look around at the investigation point.
            _guard.Velocity = _guard.Velocity.MoveToward(Vector2.Zero, _guard.Friction * (float)delta);
            _guard.MoveAndSlide();

            _lookTimer -= (float)delta;
            if (_lookTimer <= 0f)
            {
                _lookPhase++;
                _lookTimer = 1.0f;

                // Turn to look in different directions.
                _guard.GuardFacingDirection = _lookPhase switch
                {
                    1 => Vector2.Right,
                    2 => Vector2.Left,
                    3 => Vector2.Up,
                    _ => Vector2.Down
                };
            }
        }

        // Timeout — nothing found, return to patrol.
        if (_investigateTimer <= 0f)
        {
            _guard.Sensor?.ResetAwareness();
            Machine?.TransitionTo("Patrol");
        }
    }

    public override void Exit()
    {
        GD.Print($"Guard {_guard.Name} finished investigating.");
    }
}
