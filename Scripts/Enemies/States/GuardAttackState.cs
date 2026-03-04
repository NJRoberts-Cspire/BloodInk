using Godot;
using BloodInk.Core;
using BloodInk.Stealth;

namespace BloodInk.Enemies.States;

/// <summary>
/// Guard attack state — stealth-aware version.
/// Enables hitbox, performs attack, then transitions back to Chase.
/// If the player becomes hidden during the attack cooldown, may switch to Search.
/// </summary>
public partial class GuardAttackState : State
{
    [Export] public float AttackDuration { get; set; } = 0.4f;
    [Export] public float CooldownDuration { get; set; } = 0.5f;

    private GuardEnemy _guard = null!;
    private float _timer;
    private bool _hitboxActive;

    public override void Init()
    {
        _guard = GetOwner<GuardEnemy>();
    }

    public override void Enter()
    {
        _timer = AttackDuration;
        _hitboxActive = true;
        _guard.Velocity = Vector2.Zero;
        _guard.Hitbox.Monitoring = true;

        // Position hitbox toward target.
        if (_guard.Target is Node2D target)
        {
            var dir = (target.GlobalPosition - _guard.GlobalPosition).Normalized();
            _guard.Hitbox.Position = dir * 18f;
            _guard.GuardFacingDirection = dir;
        }

        _guard.AnimPlayer.Play("attack");

        // Attacking makes noise.
        NoisePropagator.Instance?.PropagateNoise(_guard.GlobalPosition, 100f);
    }

    public override void PhysicsUpdate(double delta)
    {
        _timer -= (float)delta;
        _guard.ApplyKnockback(delta);
        _guard.MoveAndSlide();

        if (_hitboxActive && _timer <= 0)
        {
            _guard.Hitbox.Monitoring = false;
            _hitboxActive = false;
            _timer = CooldownDuration;
        }
        else if (!_hitboxActive && _timer <= 0)
        {
            // After cooldown, decide next state.
            if (_guard.Sensor?.CurrentAwareness >= AwarenessLevel.Engaged)
                Machine?.TransitionTo("Chase");
            else if (_guard.Sensor?.CurrentAwareness >= AwarenessLevel.Suspicious)
                Machine?.TransitionTo("Search");
            else
                Machine?.TransitionTo("Patrol");
        }
    }

    public override void Exit()
    {
        _guard.Hitbox.Monitoring = false;
        _hitboxActive = false;
    }
}
