using Godot;

namespace BloodInk.Abilities;

/// <summary>
/// Enemy Sense — pulse that highlights all enemies within range through walls.
/// Active for 5s on a 10s cooldown. Enemies glow visible through terrain.
/// </summary>
public partial class EnemySenseAbility : AbilityBase
{
    [Export] public float SenseRange { get; set; } = 300f;
    [Export] public float Duration { get; set; } = 5f;

    private bool _isSensing;
    private float _senseTimer;

    public override void _Ready()
    {
        AbilityId = "enemy_sense";
        Cooldown = 10f;
    }

    public override void _Process(double delta)
    {
        base._Process(delta);

        if (_isSensing)
        {
            _senseTimer -= (float)delta;
            if (_senseTimer <= 0f)
                EndSense();
        }
    }

    protected override void Activate()
    {
        _isSensing = true;
        _senseTimer = Duration;
        HighlightNearbyEnemies();
        GD.Print($"[EnemySense] Sensing enemies within {SenseRange}px for {Duration}s.");
    }

    private void HighlightNearbyEnemies()
    {
        if (Owner2D == null) return;
        var origin = Owner2D.GlobalPosition;

        foreach (var node in GetTree().GetNodesInGroup("Enemy"))
        {
            if (node is Node2D enemy)
            {
                float dist = origin.DistanceTo(enemy.GlobalPosition);
                if (dist <= SenseRange)
                {
                    // Modulate sprite red to indicate detected — visual only
                    var sprite = enemy.GetNodeOrNull<AnimatedSprite2D>("AnimatedSprite2D");
                    if (sprite != null)
                        sprite.Modulate = new Color(2f, 0.3f, 0.3f, 1f);
                }
            }
        }
    }

    private void EndSense()
    {
        _isSensing = false;

        // Restore normal modulation
        foreach (var node in GetTree().GetNodesInGroup("Enemy"))
        {
            if (node is Node2D enemy)
            {
                var sprite = enemy.GetNodeOrNull<AnimatedSprite2D>("AnimatedSprite2D");
                if (sprite != null)
                    sprite.Modulate = Colors.White;
            }
        }

        GD.Print("[EnemySense] Sense expired.");
        ExpireAbility();
    }

    public bool IsSensing => _isSensing;
}
