using Godot;
using BloodInk.Combat;
using BloodInk.Tools;

namespace BloodInk.Enemies;

/// <summary>
/// Base enemy controller. Extend this for specific enemy types (Slime, Skeleton, etc).
/// </summary>
public partial class EnemyBase : CharacterBody2D
{
    [ExportGroup("Stats")]
    [Export] public float MoveSpeed { get; set; } = 60f;
    [Export] public float Friction { get; set; } = 400f;
    [Export] public float Acceleration { get; set; } = 500f;
    [Export] public float DetectRange { get; set; } = 100f;
    [Export] public float AttackRange { get; set; } = 25f;

    public AnimatedSprite2D AnimPlayer { get; private set; } = null!;
    public Hurtbox Hurtbox { get; private set; } = null!;
    public Hitbox Hitbox { get; private set; } = null!;
    public HealthComponent Health { get; private set; } = null!;

    public Vector2 KnockbackVelocity { get; set; }

    /// <summary>Hitstun invincibility duration — prevents stunlock.</summary>
    [Export] public float HitstunDuration { get; set; } = 0.25f;

    /// <summary>Reference to the player — set by the world or via detection.</summary>
    public Node2D? Target { get; set; }

    /// <summary>Safe animation play — falls back to idle if animation doesn't exist.</summary>
    public void PlayAnimation(string animName)
    {
        if (AnimPlayer.SpriteFrames != null && AnimPlayer.SpriteFrames.HasAnimation(animName))
            AnimPlayer.Play(animName);
        else if (AnimPlayer.SpriteFrames != null && AnimPlayer.SpriteFrames.HasAnimation("idle"))
            AnimPlayer.Play("idle");
    }

    public override void _Ready()
    {
        AnimPlayer = GetNodeOrNull<AnimatedSprite2D>("AnimatedSprite2D")!;
        Hurtbox = GetNodeOrNull<Hurtbox>("Hurtbox")!;
        Hitbox = GetNodeOrNull<Hitbox>("Hitbox")!;
        Health = GetNodeOrNull<HealthComponent>("HealthComponent")!;

        if (AnimPlayer == null || Hurtbox == null || Hitbox == null || Health == null)
        {
            GD.PrintErr($"EnemyBase '{Name}': missing required child nodes (AnimatedSprite2D, Hurtbox, Hitbox, or HealthComponent).");
            SetPhysicsProcess(false);
            return;
        }

        Hitbox.Source = this;
        Hitbox.Monitoring = false;

        Hurtbox.Hurt += OnHurt;
        Health.Died += OnDied;

        // Apply placeholder sprites if no textures are loaded.
        ApplyPlaceholderSprites();

        EnemyReady();
    }

    /// <summary>Override in subclasses for additional setup.</summary>
    protected virtual void EnemyReady() { }

    /// <summary>Apply placeholder sprite frames if none are set.</summary>
    private void ApplyPlaceholderSprites()
    {
        if (AnimPlayer.SpriteFrames == null ||
            AnimPlayer.SpriteFrames.GetFrameCount("idle") == 0 ||
            AnimPlayer.SpriteFrames.GetFrameTexture("idle", 0) == null)
        {
            // Try guard frames first for GuardEnemy types, fall back to slime.
            string preferredKey = this is GuardEnemy ? "guard_frames" : "slime_frames";
            var placeholder = PlaceholderSprites.GetFrames(preferredKey)
                           ?? PlaceholderSprites.GetFrames("slime_frames");
            if (placeholder != null)
            {
                AnimPlayer.SpriteFrames = placeholder;
                AnimPlayer.Play("idle");
            }
        }
    }

    public void ApplyKnockback(double delta)
    {
        KnockbackVelocity = KnockbackVelocity.MoveToward(Vector2.Zero, Friction * (float)delta);
        Velocity += KnockbackVelocity;
    }

    /// <summary>Distance to current target, or float.MaxValue if no target.</summary>
    public float DistanceToTarget()
    {
        if (Target == null) return float.MaxValue;
        return GlobalPosition.DistanceTo(Target.GlobalPosition);
    }

    /// <summary>Direction toward current target.</summary>
    public Vector2 DirectionToTarget()
    {
        if (Target == null) return Vector2.Zero;
        return (Target.GlobalPosition - GlobalPosition).Normalized();
    }

    private void OnHurt(int damage, Vector2 knockback)
    {
        Health.TakeDamage(damage);
        KnockbackVelocity = knockback;

        // Hitstun invincibility — prevents infinite stunlock from attack spam.
        Hurtbox.IsInvincible = true;
        var tree = GetTree();
        var timer = tree.CreateTimer(HitstunDuration, false);
        timer.Timeout += () => { if (IsInstanceValid(this) && IsInsideTree()) Hurtbox.IsInvincible = false; };
    }

    protected virtual void OnDied()
    {
        // Disable combat and processing before deferred removal.
        Hitbox.Monitoring = false;
        Hurtbox.IsInvincible = true;
        Hurtbox.Monitorable = false;
        Hurtbox.SetDeferred("monitoring", false);
        Hurtbox.Hurt -= OnHurt;
        SetPhysicsProcess(false);
        SetProcess(false);

        var sm = GetNodeOrNull<Core.StateMachine>("StateMachine");
        if (sm != null) sm.ProcessMode = ProcessModeEnum.Disabled;

        // Spawn death VFX before removing.
        VFX.DeathEffect.SpawnAt(
            GetTree().CurrentScene,
            GlobalPosition,
            GetNodeOrNull<AnimatedSprite2D>("AnimatedSprite2D"));
        VFX.BloodSplatter.SpawnHeavy(
            GetTree().CurrentScene,
            GlobalPosition, Vector2.Up);

        // Leave a corpse marker that guards can discover.
        Stealth.CorpseMarker.SpawnAt(GetTree().CurrentScene, GlobalPosition);

        QueueFree();
    }
}
