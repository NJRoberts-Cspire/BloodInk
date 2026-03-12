using Godot;
using BloodInk.Core;
using BloodInk.Stealth;

namespace BloodInk.Enemies.States;

/// <summary>
/// Guard attack state — stealth-aware version with attack telegraph.
/// Shows a brief windup (red tint) before the hitbox activates, giving players
/// a chance to dodge. After the attack, transitions based on awareness level.
/// </summary>
public partial class GuardAttackState : State
{
    [Export] public float AttackDuration { get; set; } = 0.4f;
    [Export] public float CooldownDuration { get; set; } = 0.5f;

    /// <summary>Windup time before the hitbox activates. Gives players time to react.</summary>
    [Export] public float WindupDuration { get; set; } = 0.2f;

    private GuardEnemy _guard = null!;
    private float _timer;
    private bool _hitboxActive;

    private enum Phase { Windup, Attack, Cooldown }
    private Phase _phase;

    /// <summary>Colour tint used during windup to telegraph the attack.</summary>
    private static readonly Color WindupTint = new(1f, 0.3f, 0.3f, 1f);

    public override void Init()
    {
        _guard = GetOwner<GuardEnemy>();
    }

    public override void Enter()
    {
        _hitboxActive = false;
        _guard.Velocity = Vector2.Zero;

        // Position hitbox toward target.
        if (_guard.Target != null && IsInstanceValid(_guard.Target) && _guard.Target is Node2D target)
        {
            var dir = (target.GlobalPosition - _guard.GlobalPosition).Normalized();
            _guard.Hitbox.Position = dir * 18f;
            _guard.GuardFacingDirection = dir;
        }

        // Start windup phase — telegraph with a red flash.
        _phase = Phase.Windup;
        _timer = WindupDuration;
        _guard.AnimPlayer.Play("attack");
        _guard.Modulate = WindupTint;
    }

    public override void PhysicsUpdate(double delta)
    {
        _timer -= (float)delta;
        _guard.ApplyKnockback(delta);
        _guard.MoveAndSlide();

        switch (_phase)
        {
            case Phase.Windup:
                if (_timer <= 0)
                {
                    // Windup finished — activate hitbox and make noise.
                    _phase = Phase.Attack;
                    _timer = AttackDuration;
                    _hitboxActive = true;
                    _guard.Hitbox.Monitoring = true;
                    _guard.Modulate = Colors.White;

                    // Attacking makes noise (moved here from Enter so it fires with the actual strike).
                    NoisePropagator.Instance?.PropagateNoise(_guard.GlobalPosition, 100f);
                }
                break;

            case Phase.Attack:
                if (_timer <= 0)
                {
                    _guard.Hitbox.Monitoring = false;
                    _hitboxActive = false;
                    _phase = Phase.Cooldown;
                    _timer = CooldownDuration;
                }
                break;

            case Phase.Cooldown:
                if (_timer <= 0)
                {
                    // After cooldown, decide next state.
                    if (_guard.Sensor?.CurrentAwareness >= AwarenessLevel.Engaged)
                        Machine?.TransitionTo("Chase");
                    else if (_guard.Sensor?.CurrentAwareness >= AwarenessLevel.Suspicious)
                        Machine?.TransitionTo("Search");
                    else
                        Machine?.TransitionTo("Patrol");
                }
                break;
        }
    }

    public override void Exit()
    {
        _guard.Hitbox.Monitoring = false;
        _hitboxActive = false;
        _guard.Modulate = Colors.White; // Ensure tint is restored if interrupted.
    }
}
