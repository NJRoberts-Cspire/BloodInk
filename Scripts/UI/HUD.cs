using Godot;
using BloodInk.Combat;
using BloodInk.Core;
using BloodInk.Stealth;

namespace BloodInk.UI;

/// <summary>
/// Heads-up display showing player health, state, and stealth status.
/// Wire the player's HealthComponent signal to this.
/// </summary>
public partial class HUD : CanvasLayer
{
    private Label _healthLabel = null!;
    private Label _stateLabel = null!;
    private TextureProgressBar? _healthBar;
    private StateMachine? _stateMachine;
    private StealthProfile? _stealthProfile;

    public override void _Ready()
    {
        _healthLabel = GetNode<Label>("HealthLabel");
        _stateLabel = GetNodeOrNull<Label>("StateLabel") ?? CreateLabel("StateLabel", 26);
        _healthBar = GetNodeOrNull<TextureProgressBar>("HealthBar");
    }

    public override void _Process(double delta)
    {
        // Update state label every frame.
        if (_stateMachine?.CurrentState != null)
        {
            string stateName = _stateMachine.CurrentState.Name;
            string stealthText = "";
            if (_stealthProfile != null)
            {
                stealthText = $" | Stealth: {_stealthProfile.Visibility}";
                if (_stealthProfile.IsCrouching)
                    stealthText += " [Crouching]";
            }
            _stateLabel.Text = $"State: {stateName}{stealthText}";
        }
    }

    public void OnHealthChanged(int current, int max)
    {
        _healthLabel.Text = $"HP: {current}/{max}";
        // Color-code health.
        float ratio = max > 0 ? (float)current / max : 0;
        _healthLabel.AddThemeColorOverride("font_color",
            ratio > 0.5f ? Colors.White :
            ratio > 0.25f ? Colors.Yellow :
            Colors.Red);

        if (_healthBar != null)
        {
            _healthBar.MaxValue = max;
            _healthBar.Value = current;
        }
    }

    public void SetStateMachine(StateMachine sm)
    {
        _stateMachine = sm;
    }

    public void SetStealthProfile(StealthProfile sp)
    {
        _stealthProfile = sp;
    }

    private Label CreateLabel(string name, int yOffset)
    {
        var label = new Label
        {
            Name = name,
            Position = new Vector2(8, yOffset),
            Size = new Vector2(300, 20),
        };
        AddChild(label);
        return label;
    }
}
