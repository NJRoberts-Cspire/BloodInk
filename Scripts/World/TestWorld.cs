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
    }
}
