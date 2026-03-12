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
        // Tick down ability cooldowns (must happen in every movement state).
        PlayerAttackState.CooldownRemaining = Mathf.Max(0, PlayerAttackState.CooldownRemaining - (float)delta);
        PlayerDodgeState.CooldownRemaining = Mathf.Max(0, PlayerDodgeState.CooldownRemaining - (float)delta);
        _player.TickInputBuffer((float)delta);

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

        // Attack while crouched = stealth kill attempt (respects attack cooldown).
        if (@event.IsActionPressed("attack") && PlayerAttackState.CooldownRemaining <= 0)
        {
            Machine?.TransitionTo("StealthKill");
            return;
        }

        // Dodge still available while crouched (respects dodge cooldown).
        if (@event.IsActionPressed("dodge") && PlayerDodgeState.CooldownRemaining <= 0)
        {
            Machine?.TransitionTo("Dodge");
        }
    }

    public override void Exit()
    {
        _stealth?.SetCrouching(false);
    }
}
