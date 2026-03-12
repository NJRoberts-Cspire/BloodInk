using Godot;

namespace BloodInk.Stealth;

/// <summary>
/// Marker left at a dead enemy's position instead of immediately QueueFree-ing.
/// Guards who spot a corpse raise the mission alert level and become suspicious.
/// Visible as a blood pool sprite on the ground.
///
/// Spawned by EnemyBase.OnDied() instead of (or after) QueueFree.
/// </summary>
public partial class CorpseMarker : Area2D
{
    /// <summary>Whether this corpse has already been discovered by a guard.</summary>
    public bool IsDiscovered { get; private set; }

    /// <summary>How long until the corpse auto-frees (prevents unbounded growth). 0 = never.</summary>
    [Export] public float Lifetime { get; set; } = 120f;

    /// <summary>Noise radius when a guard discovers the corpse.</summary>
    [Export] public float DiscoveryNoiseRadius { get; set; } = 100f;

    private float _age;

    public override void _Ready()
    {
        // Corpse is on the world layer, detectable by guards.
        CollisionLayer = 0;
        CollisionMask = 1 << 2; // Enemy layer — guards walk over it.
        Monitoring = true;
        Monitorable = false;
        ZIndex = -1;

        // Visual blood pool.
        var sprite = new Sprite2D();
        var tex = Tools.PlaceholderSprites.Get("blood_pool");
        if (tex != null)
            sprite.Texture = tex;
        else
        {
            // Fallback: small red circle.
            sprite.Texture = Tools.PlaceholderSprites.Get("particle");
            sprite.Modulate = new Color(0.5f, 0f, 0f, 0.6f);
            sprite.Scale = new Vector2(2f, 1.2f);
        }
        AddChild(sprite);

        // Detection shape — circle.
        var shape = new CollisionShape2D();
        shape.Shape = new CircleShape2D { Radius = 20f };
        AddChild(shape);

        BodyEntered += OnBodyEntered;
    }

    public override void _Process(double delta)
    {
        if (Lifetime > 0)
        {
            _age += (float)delta;
            if (_age >= Lifetime)
                QueueFree();
        }
    }

    private void OnBodyEntered(Node2D body)
    {
        if (IsDiscovered) return;
        if (body is not Enemies.GuardEnemy guard) return;

        IsDiscovered = true;

        // Guard reacts — investigate the corpse location.
        if (guard.Sensor != null)
        {
            guard.Sensor.LastHeardNoisePosition = GlobalPosition;
            guard.Sensor.HasPendingNoise = true;
            // Boost awareness via noise to trigger investigation.
            guard.Sensor.OnNoiseAtPosition(GlobalPosition, 60f);
        }

        // Raise mission alert.
        MissionAlertManager.Instance?.ReportCorpseFound();

        // Propagate noise so nearby guards also react.
        NoisePropagator.Instance?.PropagateNoise(GlobalPosition, DiscoveryNoiseRadius);

        GD.Print($"[Corpse] Body discovered at {GlobalPosition} by {guard.Name}");
    }

    /// <summary>
    /// Factory: spawn a corpse marker at the given position.
    /// </summary>
    public static CorpseMarker? SpawnAt(Node parent, Vector2 position)
    {
        if (parent == null) return null;

        var marker = new CorpseMarker { GlobalPosition = position };
        parent.CallDeferred("add_child", marker);
        return marker;
    }
}
