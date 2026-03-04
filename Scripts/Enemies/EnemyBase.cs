using Godot;
using BloodInk.Combat;

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

    /// <summary>Reference to the player — set by the world or via detection.</summary>
    public Node2D? Target { get; set; }

    public override void _Ready()
    {
        AnimPlayer = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
        Hurtbox = GetNode<Hurtbox>("Hurtbox");
        Hitbox = GetNode<Hitbox>("Hitbox");
        Health = GetNode<HealthComponent>("HealthComponent");

        Hitbox.Source = this;
        Hitbox.Monitoring = false;

        Hurtbox.Hurt += OnHurt;
        Health.Died += OnDied;

        EnemyReady();
    }

    /// <summary>Override in subclasses for additional setup.</summary>
    protected virtual void EnemyReady() { }

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
    }

    protected virtual void OnDied()
    {
        // Spawn death VFX before removing.
        VFX.DeathEffect.SpawnAt(
            GetTree().CurrentScene,
            GlobalPosition,
            GetNodeOrNull<AnimatedSprite2D>("AnimatedSprite2D"));
        VFX.BloodSplatter.SpawnHeavy(
            GetTree().CurrentScene,
            GlobalPosition, Vector2.Up);

        QueueFree();
    }
}
