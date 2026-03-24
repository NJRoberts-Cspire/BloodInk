using Godot;
using BloodInk.Combat;

namespace BloodInk.Enemies;

/// <summary>
/// A crossbow bolt fired by <see cref="CrossbowEnemy"/>.
/// Travels in a straight line and damages the player on contact.
/// Despawns after a set lifetime or on hitting a wall.
/// </summary>
public partial class CrossbowBolt : Area2D
{
    /// <summary>Pixels per second.</summary>
    public float Speed { get; set; } = 180f;

    /// <summary>Damage dealt on impact.</summary>
    public int Damage { get; set; } = 1;

    /// <summary>Knockback strength applied toward the bolt's travel direction.</summary>
    public float KnockbackStrength { get; set; } = 80f;

    /// <summary>Seconds before auto-despawn.</summary>
    public float Lifetime { get; set; } = 2.5f;

    private Vector2 _direction;
    private float _age;
    private bool _hit;

    public override void _Ready()
    {
        // Collide with the player hurtbox layer (layer 2) and walls (layer 1).
        CollisionLayer = 0;
        CollisionMask  = 0b110; // layers 2 (player hurtbox) + 1 (walls)

        // Build a small visible rectangle to represent the bolt.
        var rect = new ColorRect
        {
            Color = new Color(0.85f, 0.7f, 0.3f),
            Size  = new Vector2(6, 2),
            Position = new Vector2(-3, -1),
        };
        AddChild(rect);

        var shape = new CollisionShape2D();
        shape.Shape = new RectangleShape2D { Size = new Vector2(6, 2) };
        AddChild(shape);

        BodyEntered   += OnBodyEntered;
        AreaEntered   += OnAreaEntered;
    }

    /// <summary>Set the bolt's travel direction (should be normalised).</summary>
    public void Fire(Vector2 direction)
    {
        _direction = direction.Normalized();
        // Rotate the sprite to face travel direction.
        Rotation = _direction.Angle();
    }

    public override void _PhysicsProcess(double delta)
    {
        if (_hit) return;

        _age += (float)delta;
        if (_age >= Lifetime)
        {
            QueueFree();
            return;
        }

        Position += _direction * Speed * (float)delta;

        // Simple wall check via Godot's space state.
        var spaceState = GetWorld2D().DirectSpaceState;
        var query = PhysicsRayQueryParameters2D.Create(
            GlobalPosition,
            GlobalPosition + _direction * (Speed * (float)delta + 4f),
            collisionMask: 1); // walls only
        var result = spaceState.IntersectRay(query);
        if (result.Count > 0)
        {
            QueueFree();
        }
    }

    private void OnBodyEntered(Node body)
    {
        if (_hit) return;
        if (body is not CharacterBody2D) return;

        TryDamageHurtbox(body);
    }

    private void OnAreaEntered(Area2D area)
    {
        if (_hit) return;
        if (area is not Hurtbox hurtbox) return;
        if (hurtbox.IsInvincible) return;

        _hit = true;
        hurtbox.EmitSignal(Hurtbox.SignalName.Hurt, Damage, _direction * KnockbackStrength);
        QueueFree();
    }

    private void TryDamageHurtbox(Node body)
    {
        var hurtbox = body.GetNodeOrNull<Hurtbox>("Hurtbox");
        if (hurtbox == null || hurtbox.IsInvincible) return;

        _hit = true;
        hurtbox.EmitSignal(Hurtbox.SignalName.Hurt, Damage, _direction * KnockbackStrength);
        QueueFree();
    }

    /// <summary>
    /// Spawn a bolt at <paramref name="origin"/> aimed at <paramref name="target"/>,
    /// added as a child of <paramref name="parent"/> so it persists independently.
    /// </summary>
    public static CrossbowBolt Spawn(Node parent, Vector2 origin, Vector2 target,
        float speed = 180f, int damage = 1)
    {
        var bolt = new CrossbowBolt
        {
            GlobalPosition = origin,
            Speed = speed,
            Damage = damage,
        };
        parent.AddChild(bolt);
        bolt.Fire((target - origin).Normalized());
        return bolt;
    }
}
