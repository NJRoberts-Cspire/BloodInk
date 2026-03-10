using Godot;
using BloodInk.Core;

namespace BloodInk.Enemies.States;

/// <summary>
/// Enemy chase state. Moves toward the player, attacks when close enough.
/// </summary>
public partial class EnemyChaseState : State
{
    private EnemyBase _enemy = null!;

    public override void Init()
    {
        _enemy = GetOwner<EnemyBase>();
    }

    public override void Enter()
    {
        _enemy.PlayAnimation("run");
    }

    public override void PhysicsUpdate(double delta)
    {
        _enemy.ApplyKnockback(delta);

        var dist = _enemy.DistanceToTarget();

        if (dist > _enemy.DetectRange * 1.5f)
        {
            Machine?.TransitionTo("Idle");
            return;
        }

        if (dist <= _enemy.AttackRange)
        {
            Machine?.TransitionTo("Attack");
            return;
        }

        var dir = _enemy.DirectionToTarget();
        _enemy.Velocity = _enemy.Velocity.MoveToward(
            dir * _enemy.MoveSpeed,
            _enemy.Acceleration * (float)delta
        );
        _enemy.AnimPlayer.FlipH = dir.X < 0;
        _enemy.MoveAndSlide();
    }
}
