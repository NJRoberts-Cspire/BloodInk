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
    }

    public override void PhysicsUpdate(double delta)
    {
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
        if (@event.IsActionPressed("attack"))
            Machine?.TransitionTo("Attack");
        else if (@event.IsActionPressed("dodge"))
            Machine?.TransitionTo("Dodge");
        else if (@event.IsActionPressed("crouch"))
            Machine?.TransitionTo("Crouch");
    }
}
