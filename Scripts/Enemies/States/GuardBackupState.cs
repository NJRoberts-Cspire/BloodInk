using Godot;
using BloodInk.Core;
using BloodInk.Stealth;

namespace BloodInk.Enemies.States;

/// <summary>
/// Guard calls for reinforcements — briefly stops, shouts (makes noise), and
/// alerts all nearby guards via NoisePropagator and direct sensor ForceEngage.
/// After calling backup, transitions directly to Chase to pursue the player.
///
/// This state is entered from Chase when the guard first spots the player and
/// hasn't yet called for backup. It is a short interrupt — typically 0.5–1s —
/// before the guard resumes pursuit.
/// </summary>
public partial class GuardBackupState : State
{
    /// <summary>How long the guard pauses to "call out" before chasing.</summary>
    [Export] public float CalloutDuration { get; set; } = 0.8f;

    private GuardEnemy _guard = null!;
    private float _timer;

    public override void Init()
    {
        _guard = GetOwner<GuardEnemy>();
    }

    public override void Enter()
    {
        _timer = CalloutDuration;

        // Stop moving and play an alert animation while calling out.
        _guard.Velocity = Vector2.Zero;
        _guard.PlayAnimation("idle");

        // Call backup immediately on entering this state.
        if (!_guard.HasCalledBackup)
            _guard.CallBackup();

        GD.Print($"Guard {_guard.Name} is calling for BACKUP!");
    }

    public override void PhysicsUpdate(double delta)
    {
        _guard.ApplyKnockback(delta);

        // Stay still during the callout.
        _guard.Velocity = _guard.Velocity.MoveToward(Vector2.Zero, _guard.Friction * (float)delta);
        _guard.MoveAndSlide();

        // If we've lost the player while calling, transition to Search instead.
        if (_guard.Sensor != null)
        {
            if (_guard.Sensor.CurrentAwareness <= AwarenessLevel.Unaware)
            {
                Machine?.TransitionTo("Patrol");
                return;
            }
            if (_guard.Sensor.CurrentAwareness == AwarenessLevel.Searching)
            {
                Machine?.TransitionTo("Search");
                return;
            }
        }

        _timer -= (float)delta;
        if (_timer <= 0f)
        {
            // Callout complete — pursue the player.
            Machine?.TransitionTo("Chase");
        }
    }

    public override void Exit()
    {
        // Ensure backup flag is set even if state was interrupted.
        _guard.HasCalledBackup = true;
    }
}
