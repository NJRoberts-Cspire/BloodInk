using Godot;
using BloodInk.Combat;

namespace BloodInk.UI;

/// <summary>
/// Heads-up display showing player health as hearts (or a bar).
/// Wire the player's HealthComponent signal to this.
/// </summary>
public partial class HUD : CanvasLayer
{
    private Label _healthLabel = null!;
    private TextureProgressBar? _healthBar;

    public override void _Ready()
    {
        _healthLabel = GetNode<Label>("HealthLabel");
        _healthBar = GetNodeOrNull<TextureProgressBar>("HealthBar");
    }

    public void OnHealthChanged(int current, int max)
    {
        _healthLabel.Text = $"HP: {current}/{max}";
        if (_healthBar != null)
        {
            _healthBar.MaxValue = max;
            _healthBar.Value = current;
        }
    }
}
