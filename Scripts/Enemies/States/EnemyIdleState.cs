using Godot;
using BloodInk.Core;

namespace BloodInk.Enemies.States;

/// <summary>
/// Enemy idle state. Waits until the player enters detection range.
/// </summary>
public partial class EnemyIdleState : State
{
    private EnemyBase _enemy = null!;

    public override void Init()
    {
        _enemy = GetOwner<EnemyBase>();
    }

    public override void Enter()
    {
        _enemy.PlayAnimation("idle");
    }

    public override void PhysicsUpdate(double delta)
    {
        _enemy.ApplyKnockback(delta);
        _enemy.Velocity = _enemy.Velocity.MoveToward(Vector2.Zero, _enemy.Friction * (float)delta);
        _enemy.MoveAndSlide();

        // Transition to Patrol if the enemy has a patrol route,
        // otherwise fall through to detection-based chase.
        if (_enemy.GetNodeOrNull("PatrolRoute") != null)
        {
            Machine?.TransitionTo("Patrol");
            return;
        }

        if (_enemy.DistanceToTarget() <= _enemy.DetectRange)
        {
            Machine?.TransitionTo("Chase");
        }
    }
}
