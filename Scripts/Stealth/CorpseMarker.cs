using Godot;

namespace BloodInk.Stealth;

/// <summary>
/// Marker left at a dead enemy's position instead of immediately QueueFree-ing.
/// Guards who spot a corpse raise the mission alert level and become suspicious.
/// Visible as a blood pool sprite on the ground.
///
/// Spawned by EnemyBase.OnDied() instead of (or after) QueueFree.
///
/// Architecture: CorpseMarker emits <see cref="BodyDiscovered"/> when a guard walks
/// over it. Alert logic (sensor manipulation, MissionAlertManager calls) must be
/// handled by the receiver — typically GuardEnemy — not here.
/// </summary>
public partial class CorpseMarker : Area2D
{
    /// <summary>
    /// Emitted when a guard first discovers this corpse.
    /// <param name="discoverer">The guard body node that walked over the corpse.</param>
    /// <param name="corpsePosition">World position of this corpse.</param>
    /// </summary>
    [Signal] public delegate void BodyDiscoveredEventHandler(Node2D discoverer, Vector2 corpsePosition);

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
        // Only react to enemies in the "Guards" group — avoids direct type coupling.
        if (!body.IsInGroup("Guards")) return;

        IsDiscovered = true;

        // Connect the BodyDiscovered signal to the guard's handler (if it has one)
        // before emitting, so the guard receives this specific discovery event.
        if (body.HasMethod("OnCorpseDiscovered"))
            Connect(SignalName.BodyDiscovered, new Callable(body, "OnCorpseDiscovered"),
                (uint)ConnectFlags.OneShot);

        // Propagate noise so nearby guards also react to the discovery location.
        NoisePropagator.Instance?.PropagateNoise(GlobalPosition, DiscoveryNoiseRadius);

        // Emit the signal — receivers handle their own alert logic.
        EmitSignal(SignalName.BodyDiscovered, body, GlobalPosition);

        GD.Print($"[Corpse] Body discovered at {GlobalPosition} by {body.Name}");
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
