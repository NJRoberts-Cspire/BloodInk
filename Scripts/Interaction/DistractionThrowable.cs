using Godot;

namespace BloodInk.Interaction;

/// <summary>
/// Throwable distraction item. The player can throw it to create noise at
/// a target position, luring guards away from their patrol routes.
///
/// Usage: Throw via player input → projectile arc → lands → propagates noise.
/// Supports multiple distraction types (rock, bone chimes, coin).
/// </summary>
public partial class DistractionThrowable : CharacterBody2D
{
    /// <summary>Name of this distraction type (for UI/inventory).</summary>
    [Export] public string DistractionName { get; set; } = "Rock";

    /// <summary>Noise radius on impact.</summary>
    [Export] public float NoiseRadius { get; set; } = 120f;

    /// <summary>Travel speed in pixels/second.</summary>
    [Export] public float ThrowSpeed { get; set; } = 300f;

    /// <summary>Maximum travel distance before landing.</summary>
    [Export] public float MaxDistance { get; set; } = 200f;

    /// <summary>Whether the distraction makes repeated noise (bone chimes).</summary>
    [Export] public bool IsRepeating { get; set; } = false;

    /// <summary>Interval between repeated noises (if IsRepeating).</summary>
    [Export] public float RepeatInterval { get; set; } = 2f;

    /// <summary>Total duration for repeating distractions.</summary>
    [Export] public float RepeatDuration { get; set; } = 8f;

    private Vector2 _direction;
    private float _distanceTraveled;
    private bool _landed;
    private float _repeatTimer;
    private float _totalRepeatTime;

    public override void _Ready()
    {
        CollisionLayer = 0;
        CollisionMask = 1; // World layer — stop on walls.
        ZIndex = 5;

        // Simple visual.
        var sprite = new Sprite2D();
        var tex = Tools.PlaceholderSprites.Get("particle");
        if (tex != null)
        {
            sprite.Texture = tex;
            sprite.Modulate = new Color(0.5f, 0.4f, 0.3f);
            sprite.Scale = new Vector2(0.6f, 0.6f);
        }
        AddChild(sprite);

        // Tiny collision for wall detection.
        var shape = new CollisionShape2D();
        shape.Shape = new CircleShape2D { Radius = 3f };
        AddChild(shape);
    }

    /// <summary>Launch the throwable in a direction.</summary>
    public void Launch(Vector2 direction)
    {
        _direction = direction.Normalized();
    }

    public override void _PhysicsProcess(double delta)
    {
        if (!_landed)
        {
            Velocity = _direction * ThrowSpeed;
            var collision = MoveAndSlide();

            _distanceTraveled += ThrowSpeed * (float)delta;

            // Land on wall collision or max distance.
            if (GetSlideCollisionCount() > 0 || _distanceTraveled >= MaxDistance)
            {
                Land();
            }
        }
        else if (IsRepeating)
        {
            _totalRepeatTime += (float)delta;
            if (_totalRepeatTime >= RepeatDuration)
            {
                QueueFree();
                return;
            }

            _repeatTimer += (float)delta;
            if (_repeatTimer >= RepeatInterval)
            {
                _repeatTimer = 0;
                Stealth.NoisePropagator.Instance?.PropagateNoise(GlobalPosition, NoiseRadius);
            }
        }
    }

    private void Land()
    {
        _landed = true;
        Velocity = Vector2.Zero;

        // Impact noise.
        Stealth.NoisePropagator.Instance?.PropagateNoise(GlobalPosition, NoiseRadius);

        // Play impact sound.
        Audio.AudioManager.Instance?.PlaySFX("res://Assets/Audio/SFX/rock_impact.wav");

        if (!IsRepeating)
        {
            // One-shot: free after a brief delay (so the noise propagates).
            var timer = GetTree().CreateTimer(0.5);
            timer.Timeout += QueueFree;
        }
    }

    /// <summary>
    /// Factory: create and throw a distraction from the player's position.
    /// </summary>
    public static void Throw(Node parent, Vector2 origin, Vector2 direction,
                              float noiseRadius = 120f, bool repeating = false)
    {
        if (parent == null) return;

        var throwable = new DistractionThrowable
        {
            GlobalPosition = origin,
            NoiseRadius = noiseRadius,
            IsRepeating = repeating,
            RepeatInterval = repeating ? 2f : 0f,
            RepeatDuration = repeating ? 8f : 0f,
        };
        parent.AddChild(throwable);
        throwable.Launch(direction);
    }
}
