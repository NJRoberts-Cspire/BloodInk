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

    private PlayerController _player = null!;
    private float _timer;

    public override void Init()
    {
        _player = GetOwner<PlayerController>();
    }

    public override void Enter()
    {
        _timer = AttackDuration;
        _player.Velocity = Vector2.Zero;

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
    }

    public override void PhysicsUpdate(double delta)
    {
        _timer -= (float)delta;
        _player.ApplyKnockback(delta);
        _player.MoveAndSlide();

        if (_timer <= 0)
        {
            Machine?.TransitionTo("Idle");
        }
    }

    public override void Exit()
    {
        _player.SwordHitbox.Monitoring = false;
    }
}
