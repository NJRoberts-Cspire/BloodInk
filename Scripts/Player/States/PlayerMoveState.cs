using Godot;
using BloodInk.Core;
using BloodInk.Interaction;

namespace BloodInk.Player.States;

/// <summary>
/// Player move state. 8-directional movement, transitions to Idle, Attack, or Dodge.
/// Detects PushBlock collisions and calls TryPush so the player can shove blocks.
/// </summary>
public partial class PlayerMoveState : State
{
    private PlayerController _player = null!;

    /// <summary>Tracks the block the player is actively pushing (if any).</summary>
    private PushBlock? _activePushBlock;

    public override void Init()
    {
        _player = GetOwner<PlayerController>();
    }

    public override void Enter()
    {
        _player.UpdateAnimation("run");

        // Consume any buffered input from the previous state (e.g. attack pressed during dodge).
        if (Machine != null && _player.TryConsumeBuffer(Machine))
            return;
    }

    public override void Exit()
    {
        // Stop any push-in-progress when leaving the move state.
        if (_activePushBlock != null && IsInstanceValid(_activePushBlock))
            _activePushBlock.StopPushing();
        _activePushBlock = null;
    }

    public override void PhysicsUpdate(double delta)
    {
        // Tick down ability cooldowns.
        PlayerAttackState.CooldownRemaining = Mathf.Max(0, PlayerAttackState.CooldownRemaining - (float)delta);
        PlayerDodgeState.CooldownRemaining = Mathf.Max(0, PlayerDodgeState.CooldownRemaining - (float)delta);
        _player.TickInputBuffer((float)delta);

        var input = _player.GetInputVector();
        _player.ApplyKnockback(delta);

        // Apply tattoo speed bonus.
        float effectiveSpeed = _player.MoveSpeed * (1f + (Core.GameManager.Instance?.TattooSystem?.SpeedBonus ?? 0f));
        _player.ApplyMovement(input, effectiveSpeed, delta);
        _player.MoveAndSlide();

        // ── Push-block detection ────────────────────────────────
        PushBlock? touchedBlock = null;

        for (int i = 0; i < _player.GetSlideCollisionCount(); i++)
        {
            var col = _player.GetSlideCollision(i);
            if (col.GetCollider() is PushBlock pb)
            {
                touchedBlock = pb;
                pb.TryPush(_player.FacingDirection, _player, (float)delta);
                break; // only push one block at a time
            }
        }

        // If we stopped touching the block, reset its push timer.
        if (touchedBlock == null && _activePushBlock != null)
        {
            _activePushBlock.StopPushing();
        }
        _activePushBlock = touchedBlock;

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
