using Godot;
using BloodInk.Core;
using BloodInk.Stealth;

namespace BloodInk.Player.States;

/// <summary>
/// Crouch / sneak state. Player moves at reduced speed, generates less noise,
/// and has lower visibility. Can transition to stealth kills and attacks.
/// Toggle crouch via the "crouch" input action.
/// </summary>
public partial class PlayerCrouchState : State
{
    private PlayerController _player = null!;
    private StealthProfile? _stealth;

    public override void Init()
    {
        _player = GetOwner<PlayerController>();
    }

    public override void Enter()
    {
        _stealth = _player.GetNodeOrNull<StealthProfile>("StealthProfile");
        _stealth?.SetCrouching(true);
        _player.UpdateAnimation("crouch_idle");
    }

    public override void PhysicsUpdate(double delta)
    {
        var input = _player.GetInputVector();
        float crouchSpeed = _player.MoveSpeed * (_stealth?.CrouchSpeedMultiplier ?? 0.45f);

        _player.ApplyKnockback(delta);
        _player.ApplyMovement(input, crouchSpeed, delta);
        _player.MoveAndSlide();

        if (input != Vector2.Zero)
            _player.UpdateAnimation("crouch_walk");
        else
            _player.UpdateAnimation("crouch_idle");
    }

    public override void HandleInput(InputEvent @event)
    {
        // Toggle crouch off — return to idle/move.
        if (@event.IsActionPressed("crouch"))
        {
            Machine?.TransitionTo("Idle");
            return;
        }

        // Attack while crouched = stealth kill attempt.
        if (@event.IsActionPressed("attack"))
        {
            Machine?.TransitionTo("StealthKill");
            return;
        }

        // Dodge still available while crouched (a quick roll).
        if (@event.IsActionPressed("dodge"))
        {
            Machine?.TransitionTo("Dodge");
        }
    }

    public override void Exit()
    {
        _stealth?.SetCrouching(false);
    }
}
