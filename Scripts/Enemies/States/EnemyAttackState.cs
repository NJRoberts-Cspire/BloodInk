using Godot;
using BloodInk.Core;

namespace BloodInk.Enemies.States;

/// <summary>
/// Enemy attack state. Shows a brief telegraph/windup before enabling the hitbox,
/// then attacks for a duration with a post-attack cooldown.
/// </summary>
public partial class EnemyAttackState : State
{
    [Export] public float AttackDuration { get; set; } = 0.4f;
    [Export] public float CooldownDuration { get; set; } = 0.6f;

    /// <summary>Windup time before the hitbox activates. Gives players time to react.</summary>
    [Export] public float WindupDuration { get; set; } = 0.2f;

    private EnemyBase _enemy = null!;
    private float _timer;
    private bool _hitboxActive;

    private enum Phase { Windup, Attack, Cooldown }
    private Phase _phase;

    /// <summary>Colour tint used during windup to telegraph the attack.</summary>
    private static readonly Color WindupTint = new(1f, 0.3f, 0.3f, 1f);

    public override void Init()
    {
        _enemy = GetOwner<EnemyBase>();
    }

    public override void Enter()
    {
        _hitboxActive = false;
        _enemy.Velocity = Vector2.Zero;

        // Face toward target.
        if (_enemy.Target != null && IsInstanceValid(_enemy.Target) && _enemy.Target is Node2D target)
        {
            var dir = (target.GlobalPosition - _enemy.GlobalPosition).Normalized();
            _enemy.Hitbox.Position = dir * 18f;
        }

        // Start windup phase — telegraph with a red flash and "wind_up" or "attack" anim.
        _phase = Phase.Windup;
        _timer = WindupDuration;
        _enemy.PlayAnimation("attack"); // animation will hold first frames during windup
        _enemy.Modulate = WindupTint;
    }

    public override void PhysicsUpdate(double delta)
    {
        _timer -= (float)delta;
        _enemy.ApplyKnockback(delta);
        _enemy.MoveAndSlide();

        switch (_phase)
        {
            case Phase.Windup:
                if (_timer <= 0)
                {
                    // Windup finished — activate hitbox.
                    _phase = Phase.Attack;
                    _timer = AttackDuration;
                    _hitboxActive = true;
                    _enemy.Hitbox.Monitoring = true;
                    _enemy.Modulate = Colors.White;
                }
                break;

            case Phase.Attack:
                if (_timer <= 0)
                {
                    // Attack finished — enter cooldown.
                    _enemy.Hitbox.Monitoring = false;
                    _hitboxActive = false;
                    _phase = Phase.Cooldown;
                    _timer = CooldownDuration;
                }
                break;

            case Phase.Cooldown:
                if (_timer <= 0)
                {
                    Machine?.TransitionTo("Chase");
                }
                break;
        }
    }

    public override void Exit()
    {
        _enemy.Hitbox.Monitoring = false;
        _hitboxActive = false;
        _enemy.Modulate = Colors.White; // Ensure tint is restored if interrupted.
    }
}
