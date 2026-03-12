using Godot;
using BloodInk.Core;

namespace BloodInk.Player.States;

/// <summary>
/// Player idle state. Waits for input to transition to Move, Attack, or Dodge.
/// </summary>
public partial class PlayerIdleState : State
{
    private PlayerController _player = null!;

    public override void Init()
    {
        _player = GetOwner<PlayerController>();
    }

    public override void Enter()
    {
        _player.UpdateAnimation("idle");

        // Consume any buffered input from the previous state (e.g. attack pressed during dodge).
        if (Machine != null && _player.TryConsumeBuffer(Machine))
            return;
    }

    public override void PhysicsUpdate(double delta)
    {
        // Tick down ability cooldowns.
        PlayerAttackState.CooldownRemaining = Mathf.Max(0, PlayerAttackState.CooldownRemaining - (float)delta);
        PlayerDodgeState.CooldownRemaining = Mathf.Max(0, PlayerDodgeState.CooldownRemaining - (float)delta);
        _player.TickInputBuffer((float)delta);

        _player.ApplyKnockback(delta);
        _player.ApplyMovement(Vector2.Zero, 0, delta);
        _player.MoveAndSlide();

        var input = _player.GetInputVector();
        if (input != Vector2.Zero)
        {
            Machine?.TransitionTo("Move");
            return;
        }
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
