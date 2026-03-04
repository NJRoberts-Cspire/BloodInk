using Godot;
using BloodInk.Core;
using BloodInk.Combat;
using BloodInk.Stealth;
using BloodInk.VFX;

namespace BloodInk.Player.States;

/// <summary>
/// Stealth kill state. Performs a high-damage silent attack when performed from
/// behind an enemy or while fully hidden. If conditions aren't met, falls back
/// to a regular (noisy) attack.
/// </summary>
public partial class PlayerStealthKillState : State
{
    /// <summary>Duration of the stealth kill animation.</summary>
    [Export] public float KillDuration { get; set; } = 0.6f;

    /// <summary>Damage dealt on a successful stealth kill (instant kill most enemies).</summary>
    [Export] public int StealthDamage { get; set; } = 999;

    /// <summary>Range ahead of the player to check for a valid target.</summary>
    [Export] public float LungeRange { get; set; } = 28f;

    /// <summary>Max angle offset from behind the enemy for a "backstab" (degrees).</summary>
    [Export] public float BackstabAngle { get; set; } = 75f;

    private PlayerController _player = null!;
    private StealthProfile? _stealth;
    private float _timer;
    private bool _isStealthKill;

    public override void Init()
    {
        _player = GetOwner<PlayerController>();
    }

    public override void Enter()
    {
        _stealth = _player.GetNodeOrNull<StealthProfile>("StealthProfile");
        _timer = KillDuration;

        // Determine if this qualifies as a stealth kill.
        _isStealthKill = CheckStealthKillConditions();

        // Stop movement during the kill.
        _player.Velocity = Vector2.Zero;

        if (_isStealthKill)
        {
            // Silent kill — no noise, big damage.
            _player.SwordHitbox.Damage = StealthDamage;
            _player.SwordHitbox.Position = _player.FacingDirection * LungeRange;
            _player.SwordHitbox.Monitoring = true;
            _player.UpdateAnimation("stealth_kill");

            // ─── Stealth Kill VFX ─────────────────────────────────
            CameraShake.Instance?.ShakeHeavy();
            HitStop.Instance?.FreezeHeavy();
            ScreenTransition.Instance?.FlashWhite(0.12f);
            SlashArc.SpawnAt(
                _player.GetTree().CurrentScene,
                _player.GlobalPosition + _player.FacingDirection * LungeRange,
                _player.FacingDirection, isHeavy: true);

            GD.Print("Stealth kill!");
        }
        else
        {
            // Not stealthy — just a regular attack that'll make noise.
            _player.SwordHitbox.Position = _player.FacingDirection * 20f;
            _player.SwordHitbox.Monitoring = true;
            _player.UpdateAnimation("attack");

            // This generates noise since it's not a clean stealth kill.
            _stealth?.EmitNoise(NoiseType.Loud);
            NoisePropagator.Instance?.PropagateNoise(
                _player.GlobalPosition, 120f);
            GD.Print("Failed stealth kill — too exposed or not behind target.");
        }
    }

    public override void PhysicsUpdate(double delta)
    {
        _timer -= (float)delta;
        _player.MoveAndSlide();

        if (_timer <= 0)
        {
            // Return to Crouch state (still sneaking after the kill).
            Machine?.TransitionTo("Crouch");
        }
    }

    public override void Exit()
    {
        _player.SwordHitbox.Monitoring = false;
        // Restore regular damage.
        _player.SwordHitbox.Damage = 1;
    }

    // ─── Condition Checks ─────────────────────────────────────────

    private bool CheckStealthKillConditions()
    {
        // Condition 1: Player must be at least Low visibility (crouching, shadow, etc.)
        if (_stealth == null) return false;
        if (_stealth.Visibility == VisibilityLevel.Exposed ||
            _stealth.Visibility == VisibilityLevel.Normal)
        {
            return false;
        }

        // Condition 2: Check if there's a valid target ahead in the facing direction
        // and the player is behind it (backstab) OR fully hidden.
        if (_stealth.Visibility == VisibilityLevel.Hidden)
        {
            // Hidden = always counts as stealth kill if target in range.
            return true;
        }

        // For Low visibility — must be behind the enemy.
        return CheckBackstabAngle();
    }

    private bool CheckBackstabAngle()
    {
        // Raycast forward to find nearest enemy.
        var spaceState = _player.GetWorld2D().DirectSpaceState;
        var from = _player.GlobalPosition;
        var to = from + _player.FacingDirection * LungeRange * 1.5f;

        var query = PhysicsRayQueryParameters2D.Create(from, to);
        query.CollisionMask = 1 << 2; // Enemies layer (layer 3).
        query.CollideWithAreas = false;
        query.CollideWithBodies = true;

        var result = spaceState.IntersectRay(query);
        if (result.Count == 0) return true; // No enemy to check angle against.

        var enemy = result["collider"].As<Node2D>();
        if (enemy == null) return true;

        // Check if we're behind the enemy.
        // Get the enemy's facing direction — default to their velocity or looking direction.
        Vector2 enemyFacing = Vector2.Down;
        if (enemy is CharacterBody2D body && body.Velocity.Length() > 5f)
        {
            enemyFacing = body.Velocity.Normalized();
        }

        // The player's approach direction (from player to enemy).
        var approachDir = (_player.GlobalPosition - enemy.GlobalPosition).Normalized();

        // If the approach direction aligns with the enemy's facing (we're behind them),
        // the dot product of approach and enemyFacing should be positive.
        float dot = approachDir.Dot(enemyFacing);
        float angle = Mathf.RadToDeg(Mathf.Acos(Mathf.Clamp(dot, -1f, 1f)));

        return angle < BackstabAngle;
    }
}
