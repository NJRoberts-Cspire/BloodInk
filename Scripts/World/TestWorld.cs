using Godot;

namespace BloodInk.World;

/// <summary>
/// Test world scene script. Spawns the player and wires up HUD + enemy targets.
/// </summary>
public partial class TestWorld : Node2D
{
    public override void _Ready()
    {
        // Wire player health to HUD.
        var player = GetNodeOrNull<Player.PlayerController>("Player");
        var hud = GetNodeOrNull<UI.HUD>("HUD");

        if (player != null && hud != null)
        {
            player.Health.HealthChanged += hud.OnHealthChanged;
            // Trigger initial display.
            hud.OnHealthChanged(player.Health.CurrentHealth, player.Health.MaxHealth);

            // Wire state machine to state label.
            var sm = player.GetNodeOrNull<Core.StateMachine>("StateMachine");
            if (sm != null)
            {
                hud.SetStateMachine(sm);
            }

            // Wire stealth profile to HUD.
            var stealth = player.GetNodeOrNull<Stealth.StealthProfile>("StealthProfile");
            if (stealth != null)
            {
                hud.SetStealthProfile(stealth);
            }
        }

        // Point all enemies at the player.
        if (player != null)
        {
            foreach (var child in GetChildren())
            {
                if (child is Enemies.EnemyBase enemy)
                {
                    enemy.Target = player;
                }
            }
        }

        // Make walls visible by adding ColorRects.
        AddWallVisual("WallTop", new Color(0.2f, 0.2f, 0.22f));
        AddWallVisual("WallBottom", new Color(0.2f, 0.2f, 0.22f));
        AddWallVisual("WallLeft", new Color(0.2f, 0.2f, 0.22f));
        AddWallVisual("WallRight", new Color(0.2f, 0.2f, 0.22f));

        // Add shadow zones for stealth testing.
        AddShadowZone(new Vector2(-200, -80), new Vector2(80, 60));
        AddShadowZone(new Vector2(150, 50), new Vector2(70, 70));
    }

    private void AddWallVisual(string wallNodeName, Color color)
    {
        var wall = GetNodeOrNull<StaticBody2D>(wallNodeName);
        if (wall == null) return;

        var shape = wall.GetNodeOrNull<CollisionShape2D>(wallNodeName + "Shape");
        if (shape?.Shape is not RectangleShape2D rect) return;

        var visual = new ColorRect
        {
            Color = color,
            Size = rect.Size,
            Position = -rect.Size / 2,
            ZIndex = -1,
        };
        wall.AddChild(visual);
    }

    private void AddShadowZone(Vector2 position, Vector2 size)
    {
        // Visual indicator.
        var visual = new ColorRect
        {
            Color = new Color(0.05f, 0.05f, 0.1f, 0.5f),
            Size = size,
            Position = position,
            ZIndex = -1,
            MouseFilter = Control.MouseFilterEnum.Ignore,
        };
        AddChild(visual);

        // Area2D that marks shadow zone for stealth system.
        var area = new Stealth.ShadowZone();
        area.Position = position + size / 2;
        var shape = new CollisionShape2D();
        var rectShape = new RectangleShape2D { Size = size };
        shape.Shape = rectShape;
        area.AddChild(shape);
        AddChild(area);
    }
}
