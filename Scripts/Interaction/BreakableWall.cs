using Godot;
using BloodInk.Combat;

namespace BloodInk.Interaction;

/// <summary>
/// A cracked/breakable wall that can be destroyed by attacking it.
/// Zelda-style: see cracks in a wall, hit it, it crumbles to reveal a passage.
/// Uses a Hurtbox to receive damage from the player's hitbox.
/// </summary>
public partial class BreakableWall : StaticBody2D
{
    [Signal] public delegate void WallBrokenEventHandler();

    /// <summary>Number of hits required to break.</summary>
    [Export] public int HitsRequired { get; set; } = 1;

    /// <summary>Whether to generate noise when broken.</summary>
    [Export] public float NoiseRadius { get; set; } = 80f;

    /// <summary>Wall color.</summary>
    [Export] public Color WallColor { get; set; } = new(0.45f, 0.4f, 0.35f, 1.0f);

    /// <summary>Crack indicator color.</summary>
    [Export] public Color CrackColor { get; set; } = new(0.25f, 0.2f, 0.15f, 1.0f);

    /// <summary>Width (pixels).</summary>
    [Export] public float WallWidth { get; set; } = 16f;

    /// <summary>Height (pixels).</summary>
    [Export] public float WallHeight { get; set; } = 16f;

    private int _hitsTaken = 0;
    private ColorRect? _visual;

    public override void _Ready()
    {
        CollisionLayer = 1; // World.
        CollisionMask = 0;

        // Collision shape.
        var bodyShape = new CollisionShape2D();
        bodyShape.Shape = new RectangleShape2D { Size = new Vector2(WallWidth, WallHeight) };
        AddChild(bodyShape);

        // Visual — wall tile with crack lines.
        _visual = new ColorRect
        {
            Color = WallColor,
            Position = new Vector2(-WallWidth / 2, -WallHeight / 2),
            Size = new Vector2(WallWidth, WallHeight),
            ZIndex = 0
        };
        AddChild(_visual);

        // Crack indicator.
        var crack1 = new ColorRect
        {
            Color = CrackColor,
            Position = new Vector2(-2, -WallHeight / 2 + 2),
            Size = new Vector2(1, WallHeight - 4),
            ZIndex = 1
        };
        AddChild(crack1);

        var crack2 = new ColorRect
        {
            Color = CrackColor,
            Position = new Vector2(-WallWidth / 2 + 2, -1),
            Size = new Vector2(WallWidth - 4, 1),
            ZIndex = 1
        };
        AddChild(crack2);

        // Hurtbox to receive player attacks.
        var hurtbox = new Hurtbox { Name = "Hurtbox" };
        hurtbox.CollisionLayer = 0;
        hurtbox.CollisionMask = 1 << 3; // Player hitbox layer.
        var hurtShape = new CollisionShape2D { Name = "HurtboxShape" };
        hurtShape.Shape = new RectangleShape2D { Size = new Vector2(WallWidth + 4, WallHeight + 4) };
        hurtbox.AddChild(hurtShape);
        AddChild(hurtbox);

        // Wire damage — Hurtbox emits HitTaken which we handle.
        hurtbox.AreaEntered += OnAttackReceived;
    }

    private void OnAttackReceived(Area2D area)
    {
        if (area is not Hitbox) return;

        _hitsTaken++;
        GD.Print($"[BreakableWall] {Name} hit! ({_hitsTaken}/{HitsRequired})");

        // Visual feedback — flash.
        if (_visual != null)
        {
            var flashColor = new Color(1f, 1f, 1f, 0.9f);
            var originalColor = WallColor;
            _visual.Color = flashColor;

            var tween = CreateTween();
            tween.TweenProperty(_visual, "color", originalColor, 0.15f);
        }

        if (_hitsTaken >= HitsRequired)
        {
            Break();
        }
    }

    private void Break()
    {
        GD.Print($"[BreakableWall] {Name} BROKEN!");

        // Noise.
        Stealth.NoisePropagator.Instance?.PropagateNoise(GlobalPosition, NoiseRadius);

        // Crumble VFX — scatter small rectangles.
        SpawnDebris();

        EmitSignal(SignalName.WallBroken);
        QueueFree();
    }

    private void SpawnDebris()
    {
        var parent = GetParent();
        if (parent == null) return;

        var rng = new RandomNumberGenerator();
        rng.Randomize();

        for (int i = 0; i < 6; i++)
        {
            var debris = new ColorRect
            {
                Color = new Color(WallColor, 0.8f),
                Position = GlobalPosition + new Vector2(rng.RandfRange(-8, 8), rng.RandfRange(-8, 8)),
                Size = new Vector2(rng.RandfRange(2, 5), rng.RandfRange(2, 5)),
                ZIndex = 5
            };
            parent.AddChild(debris);

            // Fly outward and fade.
            var tween = debris.CreateTween();
            tween.SetParallel(true);
            tween.TweenProperty(debris, "position",
                debris.Position + new Vector2(rng.RandfRange(-20, 20), rng.RandfRange(-20, 20)),
                0.6f);
            tween.TweenProperty(debris, "modulate:a", 0f, 0.6f);
            tween.Chain().TweenCallback(Callable.From(() => debris.QueueFree()));
        }
    }
}
