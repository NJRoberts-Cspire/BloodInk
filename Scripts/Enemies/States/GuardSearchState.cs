using Godot;
using BloodInk.Core;
using BloodInk.Stealth;

namespace BloodInk.Enemies.States;

/// <summary>
/// Guard is searching — lost sight of the player but knows they were there.
/// Moves between the last known position and nearby areas, looking around.
/// After the search timer expires, de-escalates to Investigate or Patrol.
/// </summary>
public partial class GuardSearchState : State
{
    private GuardEnemy _guard = null!;
    private float _searchTimer;
    private Vector2 _searchCenter;
    private Vector2 _currentSearchPoint;
    private float _searchPointTimer;
    private int _searchPointsVisited;
    private readonly System.Random _rng = new();

    public override void Init()
    {
        _guard = GetOwner<GuardEnemy>();
    }

    public override void Enter()
    {
        _guard.AnimPlayer.Play("run");
        _searchTimer = _guard.SearchTime;
        _searchCenter = _guard.Sensor?.LastKnownPlayerPosition ?? _guard.GlobalPosition;
        _currentSearchPoint = _searchCenter;
        _searchPointTimer = 0f;
        _searchPointsVisited = 0;

        GD.Print($"Guard {_guard.Name} is SEARCHING near {_searchCenter}");
        PickNewSearchPoint();
    }

    public override void PhysicsUpdate(double delta)
    {
        _guard.ApplyKnockback(delta);

        // Re-engage if player spotted again.
        if (_guard.Sensor != null)
        {
            if (_guard.Sensor.CurrentAwareness >= AwarenessLevel.Engaged)
            {
                Machine?.TransitionTo("Chase");
                return;
            }

            // New noise during search redirects.
            if (_guard.Sensor.HasPendingNoise)
            {
                _searchCenter = _guard.Sensor.LastHeardNoisePosition;
                _guard.Sensor.HasPendingNoise = false;
                PickNewSearchPoint();
                _searchTimer = _guard.SearchTime; // Reset timer.
            }
        }

        _searchTimer -= (float)delta;

        // Move toward current search point.
        var toPoint = _currentSearchPoint - _guard.GlobalPosition;
        float dist = toPoint.Length();

        if (dist < 8f)
        {
            // Arrived — look around briefly then pick a new point.
            _guard.Velocity = _guard.Velocity.MoveToward(Vector2.Zero, _guard.Friction * (float)delta);
            _guard.MoveAndSlide();

            _searchPointTimer -= (float)delta;
            if (_searchPointTimer <= 0f)
            {
                _searchPointsVisited++;
                PickNewSearchPoint();
            }
        }
        else
        {
            var dir = toPoint.Normalized();
            _guard.Velocity = _guard.Velocity.MoveToward(
                dir * _guard.AlertedSpeed,
                _guard.Acceleration * (float)delta
            );
            _guard.GuardFacingDirection = dir;
            _guard.AnimPlayer.FlipH = dir.X < 0;
            _guard.MoveAndSlide();
            _guard.UpdateFacingFromVelocity();
        }

        // Search timeout.
        if (_searchTimer <= 0f || _searchPointsVisited >= 5)
        {
            GD.Print($"Guard {_guard.Name} gave up searching.");
            _guard.Sensor?.ResetAwareness();
            Machine?.TransitionTo("Patrol");
        }
    }

    private void PickNewSearchPoint()
    {
        // Random point within a radius of the search center.
        float angle = (float)(_rng.NextDouble() * Mathf.Tau);
        float radius = 30f + (float)(_rng.NextDouble() * 60f);
        _currentSearchPoint = _searchCenter + new Vector2(
            Mathf.Cos(angle) * radius,
            Mathf.Sin(angle) * radius
        );
        _searchPointTimer = 1.5f; // Time to look around at each point.
    }
}
