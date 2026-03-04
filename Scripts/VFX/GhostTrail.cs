using Godot;

namespace BloodInk.VFX;

/// <summary>
/// Creates ghosting/afterimage trail during dodge.
/// Spawns semi-transparent copies of the player sprite that fade out.
/// Attach as child of the player CharacterBody2D.
/// </summary>
public partial class GhostTrail : Node2D
{
    /// <summary>How often to spawn a ghost (seconds).</summary>
    [Export] public float SpawnInterval { get; set; } = 0.04f;

    /// <summary>How long each ghost lasts before fully fading.</summary>
    [Export] public float GhostLifetime { get; set; } = 0.3f;

    /// <summary>Starting opacity of each ghost.</summary>
    [Export] public float StartAlpha { get; set; } = 0.5f;

    /// <summary>Tint color for the ghost sprites.</summary>
    [Export] public Color GhostTint { get; set; } = new(0.4f, 0.2f, 0.6f, 1f); // Purple-ish.

    private bool _active;
    private float _timer;
    private AnimatedSprite2D? _sourceSprite;

    public override void _Ready()
    {
        _sourceSprite = GetParent().GetNodeOrNull<AnimatedSprite2D>("AnimatedSprite2D");
    }

    public override void _Process(double delta)
    {
        if (!_active) return;

        _timer -= (float)delta;
        if (_timer <= 0)
        {
            _timer = SpawnInterval;
            SpawnGhost();
        }
    }

    /// <summary>Start spawning ghost afterimages.</summary>
    public void StartTrail()
    {
        _active = true;
        _timer = 0; // Spawn one immediately.
    }

    /// <summary>Stop spawning (existing ghosts still fade out).</summary>
    public void StopTrail()
    {
        _active = false;
    }

    private void SpawnGhost()
    {
        if (_sourceSprite == null) return;

        // Create a snapshot sprite at current position.
        var ghost = new Sprite2D
        {
            GlobalPosition = GlobalPosition,
            Texture = _sourceSprite.SpriteFrames?.GetFrameTexture(
                _sourceSprite.Animation, _sourceSprite.Frame),
            FlipH = _sourceSprite.FlipH,
            Modulate = new Color(GhostTint.R, GhostTint.G, GhostTint.B, StartAlpha),
            ZIndex = ZIndex - 1,
        };

        // Add to scene root so it stays in place.
        GetTree().CurrentScene.AddChild(ghost);

        // Fade out and free.
        var tween = ghost.CreateTween();
        tween.TweenProperty(ghost, "modulate:a", 0.0f, GhostLifetime)
             .SetEase(Tween.EaseType.In);
        tween.TweenProperty(ghost, "scale", new Vector2(1.1f, 1.1f), GhostLifetime);
        tween.TweenCallback(Callable.From(() => ghost.QueueFree()));
    }
}
