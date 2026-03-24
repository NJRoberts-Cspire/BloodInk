using Godot;
using BloodInk.Abilities;
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

        // Any attack (even stealth) breaks a disguise.
        foreach (var child in _player.GetChildren())
        {
            if (child is MaskOfAshAbility mask)
            {
                mask.BreakMask();
                break;
            }
        }

        // Determine if this qualifies as a stealth kill.
        _isStealthKill = CheckStealthKillConditions();

        // Stop movement during the kill.
        _player.Velocity = Vector2.Zero;

        if (_isStealthKill)
        {
            // Silent kill — no noise, big damage.
            _player.SwordHitbox.Damage = StealthDamage;
            _player.SwordHitbox.IsStealthKill = true;
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
            // Apply tattoo damage bonus like a normal attack.
            float dmgBonus = Core.GameManager.Instance?.TattooSystem?.DamageBonus ?? 0f;
            _player.SwordHitbox.Damage = (int)Mathf.Max(1, 1 * (1f + dmgBonus));
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

    /// <summary>Brief window during which the lethal hitbox is active.</summary>
    private const float HitboxActiveWindow = 0.15f;

    public override void PhysicsUpdate(double delta)
    {
        _timer -= (float)delta;
        _player.MoveAndSlide();

        // Tick cooldowns and input buffer during the kill animation.
        PlayerAttackState.CooldownRemaining = Mathf.Max(0, PlayerAttackState.CooldownRemaining - (float)delta);
        PlayerDodgeState.CooldownRemaining = Mathf.Max(0, PlayerDodgeState.CooldownRemaining - (float)delta);
        _player.TickInputBuffer((float)delta);

        // Disable hitbox after the brief active window to prevent multi-kills.
        if (_timer <= KillDuration - HitboxActiveWindow && _player.SwordHitbox.Monitoring)
        {
            _player.SwordHitbox.Monitoring = false;
        }

        if (_timer <= 0)
        {
            // Return to Crouch state (still sneaking after the kill).
            Machine?.TransitionTo("Crouch");
        }
    }

    /// <summary>Buffer inputs pressed during the stealth kill so they execute on exit.</summary>
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
        // Restore regular damage and stealth-kill flag.
        _player.SwordHitbox.Damage = 1;
        _player.SwordHitbox.IsStealthKill = false;
        // Set attack cooldown so stealth kills can't be spammed.
        PlayerAttackState.CooldownRemaining = 0.4f;
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
        if (result.Count == 0) return false; // No enemy in range — can't backstab nothing.

        var enemy = result["collider"].As<Node2D>();
        if (enemy == null) return false; // Can't identify enemy — no backstab.

        // Check if we're behind the enemy.
        // Prefer DetectionSensor.FacingDirection (set by AI); fall back to velocity.
        Vector2 enemyFacing = Vector2.Down;
        var sensor = enemy.GetNodeOrNull<Stealth.DetectionSensor>("DetectionSensor");
        if (sensor != null)
        {
            enemyFacing = sensor.FacingDirection;
        }
        else if (enemy is CharacterBody2D body && body.Velocity.Length() > 5f)
        {
            enemyFacing = body.Velocity.Normalized();
        }

        // The player's approach direction (from player toward enemy).
        var approachDir = (enemy.GlobalPosition - _player.GlobalPosition).Normalized();

        // If the approach direction aligns with the enemy's facing (we're behind them),
        // the dot product of approach and enemyFacing should be positive.
        float dot = approachDir.Dot(enemyFacing);
        float angle = Mathf.RadToDeg(Mathf.Acos(Mathf.Clamp(dot, -1f, 1f)));

        return angle < BackstabAngle;
    }
}
