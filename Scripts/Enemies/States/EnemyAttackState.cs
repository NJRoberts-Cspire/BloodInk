using Godot;
using BloodInk.Core;

namespace BloodInk.Enemies.States;

/// <summary>
/// Enemy attack state. Enables hitbox for a duration, then returns to Chase/Idle.
/// </summary>
public partial class EnemyAttackState : State
{
    [Export] public float AttackDuration { get; set; } = 0.4f;
    [Export] public float CooldownDuration { get; set; } = 0.6f;

    private EnemyBase _enemy = null!;
    private float _timer;
    private bool _hitboxActive;

    public override void Init()
    {
        _enemy = GetOwner<EnemyBase>();
    }

    public override void Enter()
    {
        _timer = AttackDuration;
        _hitboxActive = true;
        _enemy.Velocity = Vector2.Zero;
        _enemy.Hitbox.Monitoring = true;

        if (_enemy.Target is Node2D target)
        {
            var dir = (_enemy.Target!.GlobalPosition - _enemy.GlobalPosition).Normalized();
            _enemy.Hitbox.Position = dir * 18f;
        }

        _enemy.AnimPlayer.Play("attack");
    }

    public override void PhysicsUpdate(double delta)
    {
        _timer -= (float)delta;
        _enemy.ApplyKnockback(delta);
        _enemy.MoveAndSlide();

        // Turn off hitbox after attack window, then wait for cooldown.
        if (_hitboxActive && _timer <= 0)
        {
            _enemy.Hitbox.Monitoring = false;
            _hitboxActive = false;
            _timer = CooldownDuration;
        }
        else if (!_hitboxActive && _timer <= 0)
        {
            Machine?.TransitionTo("Chase");
        }
    }

    public override void Exit()
    {
        _enemy.Hitbox.Monitoring = false;
        _hitboxActive = false;
    }
}
