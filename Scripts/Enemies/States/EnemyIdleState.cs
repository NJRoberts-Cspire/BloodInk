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
        _enemy.AnimPlayer.Play("idle");
    }

    public override void PhysicsUpdate(double delta)
    {
        _enemy.ApplyKnockback(delta);
        _enemy.Velocity = _enemy.Velocity.MoveToward(Vector2.Zero, _enemy.Friction * (float)delta);
        _enemy.MoveAndSlide();

        if (_enemy.DistanceToTarget() <= _enemy.DetectRange)
        {
            Machine?.TransitionTo("Chase");
        }
    }
}
