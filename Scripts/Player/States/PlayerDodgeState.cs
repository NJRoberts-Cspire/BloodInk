using Godot;
using BloodInk.Core;
using BloodInk.VFX;

namespace BloodInk.Player.States;

/// <summary>
/// Dodge / dash state. The player is invincible and moves quickly in the facing direction.
/// </summary>
public partial class PlayerDodgeState : State
{
    [Export] public float DodgeDuration { get; set; } = 0.25f;

    /// <summary>Cooldown after dodge ends before another dodge is allowed (seconds).</summary>
    [Export] public float DodgeCooldown { get; set; } = 0.3f;

    private PlayerController _player = null!;
    private float _timer;

    /// <summary>Shared cooldown tracker — set on exit, checked by IdleState/MoveState. Reset on Init().</summary>
    public static float CooldownRemaining { get; set; } = 0f;

    public override void Init()
    {
        _player = GetOwner<PlayerController>();
        CooldownRemaining = 0f;
    }

    private GhostTrail? _ghostTrail;
    private bool _wasInvincibleBeforeDodge;

    public override void Enter()
    {
        _timer = DodgeDuration;
        // Remember if player was already invincible (e.g. post-hit iframes).
        _wasInvincibleBeforeDodge = _player.Hurtbox.IsInvincible;
        _player.Hurtbox.IsInvincible = true;
        _player.Velocity = _player.FacingDirection * _player.DodgeSpeed;
        _player.UpdateAnimation("dodge");

        // Dodge VFX: ghost trail + dust puff at start.
        _ghostTrail = _player.GetNodeOrNull<GhostTrail>("GhostTrail");
        _ghostTrail?.StartTrail();
        var scene = _player.GetTree()?.CurrentScene;
        if (scene != null) DustPuff.SpawnAt(scene, _player.GlobalPosition, 1.5f);
    }

    public override void PhysicsUpdate(double delta)
    {
        _timer -= (float)delta;
        _player.MoveAndSlide();

        // Tick the other ability's cooldown so it's ready when this dodge ends.
        PlayerAttackState.CooldownRemaining = Mathf.Max(0, PlayerAttackState.CooldownRemaining - (float)delta);
        _player.TickInputBuffer((float)delta);

        if (_timer <= 0)
        {
            Machine?.TransitionTo("Idle");
        }
    }

    /// <summary>Buffer inputs pressed during dodge so they execute on exit.</summary>
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
        // Only clear invincibility if it wasn't already active before dodge.
        if (!_wasInvincibleBeforeDodge)
            _player.Hurtbox.IsInvincible = false;
        _ghostTrail?.StopTrail();
        CooldownRemaining = DodgeCooldown;

        // Landing dust.
        var scene = _player.GetTree()?.CurrentScene;
        if (scene != null) DustPuff.SpawnAt(scene, _player.GlobalPosition, 0.8f);
    }
}
