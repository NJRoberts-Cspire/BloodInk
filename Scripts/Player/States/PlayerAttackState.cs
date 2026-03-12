using Godot;
using BloodInk.Core;
using BloodInk.VFX;

namespace BloodInk.Player.States;

/// <summary>
/// Melee attack state. Enables the sword hitbox for a fixed duration, then returns to Idle.
/// </summary>
public partial class PlayerAttackState : State
{
    [Export] public float AttackDuration { get; set; } = 0.35f;

    /// <summary>Cooldown after attack ends before another attack is allowed (seconds).</summary>
    [Export] public float AttackCooldown { get; set; } = 0.2f;

    private PlayerController _player = null!;
    private float _timer;

    /// <summary>Shared cooldown tracker — checked by IdleState/MoveState. Reset on Init().</summary>
    public static float CooldownRemaining { get; set; } = 0f;

    public override void Init()
    {
        _player = GetOwner<PlayerController>();
        CooldownRemaining = 0f;
    }

    public override void Enter()
    {
        _timer = AttackDuration;
        _player.Velocity = Vector2.Zero;

        // Apply tattoo damage bonus.
        int baseDamage = 1;
        float dmgBonus = Core.GameManager.Instance?.TattooSystem?.DamageBonus ?? 0f;
        _player.SwordHitbox.Damage = (int)Mathf.Max(1, baseDamage * (1f + dmgBonus));

        // Position hitbox in facing direction.
        _player.SwordHitbox.Position = _player.FacingDirection * 20f;
        _player.SwordHitbox.Monitoring = true;
        _player.UpdateAnimation("attack");

        // Slash VFX.
        SlashArc.SpawnAt(
            _player.GetTree().CurrentScene,
            _player.GlobalPosition + _player.FacingDirection * 16f,
            _player.FacingDirection);

        // Camera nudge.
        CameraShake.Instance?.ShakeLight();

        // Attack swing sound.
        Audio.AudioManager.Instance?.PlaySFX("res://Assets/Audio/SFX/sword_swing.wav");
    }

    public override void PhysicsUpdate(double delta)
    {
        _timer -= (float)delta;
        _player.ApplyKnockback(delta);
        _player.MoveAndSlide();

        // Tick the other ability's cooldown so it's ready when this attack ends.
        PlayerDodgeState.CooldownRemaining = Mathf.Max(0, PlayerDodgeState.CooldownRemaining - (float)delta);
        _player.TickInputBuffer((float)delta);

        if (_timer <= 0)
        {
            Machine?.TransitionTo("Idle");
        }
    }

    /// <summary>Buffer inputs pressed during the attack so they execute on exit.</summary>
    public override void HandleInput(InputEvent @event)
    {
        if (@event.IsActionPressed("attack"))
            _player.BufferInput("attack");
        else if (@event.IsActionPressed("dodge"))
            _player.BufferInput("dodge");
        else if (@event.IsActionPressed("crouch"))
            _player.BufferInput("crouch");
    }

    public override void Exit()
    {
        _player.SwordHitbox.Monitoring = false;
        CooldownRemaining = AttackCooldown;
    }
}
