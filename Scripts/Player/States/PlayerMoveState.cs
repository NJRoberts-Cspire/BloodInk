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
        var input = _player.GetInputVector();
        _player.ApplyKnockback(delta);
        _player.ApplyMovement(input, _player.MoveSpeed, delta);
        _player.MoveAndSlide();

        if (input != Vector2.Zero)
            _player.UpdateAnimation("run");
        else
            Machine?.TransitionTo("Idle");
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
