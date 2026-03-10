using Godot;
using BloodInk.Core;

namespace BloodInk.Player.States;

/// <summary>
/// Player move state. 8-directional movement, transitions to Idle, Attack, or Dodge.
/// </summary>
public partial class PlayerMoveState : State
{
    private PlayerController _player = null!;

    public override void Init()
    {
        _player = GetOwner<PlayerController>();
    }

    public override void Enter()
    {
        _player.UpdateAnimation("run");
    }

    public override void PhysicsUpdate(double delta)
    {
        // Tick down ability cooldowns.
        PlayerAttackState.CooldownRemaining = Mathf.Max(0, PlayerAttackState.CooldownRemaining - (float)delta);
        PlayerDodgeState.CooldownRemaining = Mathf.Max(0, PlayerDodgeState.CooldownRemaining - (float)delta);

        var input = _player.GetInputVector();
        _player.ApplyKnockback(delta);

        // Apply tattoo speed bonus.
        float effectiveSpeed = _player.MoveSpeed * (1f + (Core.GameManager.Instance?.TattooSystem?.SpeedBonus ?? 0f));
        _player.ApplyMovement(input, effectiveSpeed, delta);
        _player.MoveAndSlide();

        if (input != Vector2.Zero)
            _player.UpdateAnimation("run");
        else
            Machine?.TransitionTo("Idle");
    }

    public override void HandleInput(InputEvent @event)
    {
        if (@event.IsActionPressed("attack") && PlayerAttackState.CooldownRemaining <= 0)
            Machine?.TransitionTo("Attack");
        else if (@event.IsActionPressed("dodge") && PlayerDodgeState.CooldownRemaining <= 0)
            Machine?.TransitionTo("Dodge");
        else if (@event.IsActionPressed("crouch"))
            Machine?.TransitionTo("Crouch");
    }
}
