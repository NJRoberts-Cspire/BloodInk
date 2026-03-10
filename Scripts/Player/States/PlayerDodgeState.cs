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

    public override void Enter()
    {
        _timer = DodgeDuration;
        _player.Hurtbox.IsInvincible = true;
        _player.Velocity = _player.FacingDirection * _player.DodgeSpeed;
        _player.UpdateAnimation("dodge");

        // Dodge VFX: ghost trail + dust puff at start.
        _ghostTrail = _player.GetNodeOrNull<GhostTrail>("GhostTrail");
        _ghostTrail?.StartTrail();
        DustPuff.SpawnAt(_player.GetTree().CurrentScene, _player.GlobalPosition, 1.5f);
    }

    public override void PhysicsUpdate(double delta)
    {
        _timer -= (float)delta;
        _player.MoveAndSlide();

        if (_timer <= 0)
        {
            Machine?.TransitionTo("Idle");
        }
    }

    public override void Exit()
    {
        _player.Hurtbox.IsInvincible = false;
        _ghostTrail?.StopTrail();
        CooldownRemaining = DodgeCooldown;

        // Landing dust.
        DustPuff.SpawnAt(_player.GetTree().CurrentScene, _player.GlobalPosition, 0.8f);
    }
}
