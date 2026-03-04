using Godot;
using BloodInk.Core;

namespace BloodInk.UI;

/// <summary>
/// Mission Complete screen — shown after a target is killed.
/// Displays results and transitions back to camp.
/// </summary>
public partial class MissionComplete : Control
{
    private Label? _titleLabel;
    private Label? _targetLabel;
    private Label? _whisperLabel;
    private Label? _rewardLabel;
    private Button? _continueBtn;

    public override void _Ready()
    {
        // Build the UI programmatically.
        var bg = new ColorRect
        {
            Color = new Color(0.02f, 0.02f, 0.05f, 1f),
            AnchorsPreset = (int)LayoutPreset.FullRect,
            LayoutMode = 1
        };
        bg.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
        AddChild(bg);

        var vbox = new VBoxContainer
        {
            LayoutMode = 1,
            SizeFlagsHorizontal = SizeFlags.ExpandFill
        };
        vbox.SetAnchorsAndOffsetsPreset(LayoutPreset.Center);
        vbox.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
        vbox.AddThemeConstantOverride("separation", 16);
        AddChild(vbox);

        // Spacer.
        vbox.AddChild(new Control { CustomMinimumSize = new Vector2(0, 60) });

        _titleLabel = new Label
        {
            Text = "T A R G E T   E L I M I N A T E D",
            HorizontalAlignment = HorizontalAlignment.Center
        };
        _titleLabel.AddThemeColorOverride("font_color", new Color(0.78f, 0.15f, 0.1f));
        _titleLabel.AddThemeFontSizeOverride("font_size", 20);
        vbox.AddChild(_titleLabel);

        vbox.AddChild(new Control { CustomMinimumSize = new Vector2(0, 20) });

        _targetLabel = new Label
        {
            Text = "Lord Harlan Cowl\nGovernor of the Greenhold\nEdictbearer",
            HorizontalAlignment = HorizontalAlignment.Center
        };
        _targetLabel.AddThemeFontSizeOverride("font_size", 14);
        vbox.AddChild(_targetLabel);

        vbox.AddChild(new Control { CustomMinimumSize = new Vector2(0, 20) });

        _whisperLabel = new Label
        {
            Text = "\"I never saw the dark as empty.\nI thought it was full of things that loved me.\"",
            HorizontalAlignment = HorizontalAlignment.Center
        };
        _whisperLabel.AddThemeColorOverride("font_color", new Color(0.6f, 0.4f, 0.4f));
        _whisperLabel.AddThemeFontSizeOverride("font_size", 10);
        vbox.AddChild(_whisperLabel);

        vbox.AddChild(new Control { CustomMinimumSize = new Vector2(0, 30) });

        _rewardLabel = new Label
        {
            Text = "Blood-Ink Acquired: 1× Major Grade\nNew Tattoo Available: Shadow Step",
            HorizontalAlignment = HorizontalAlignment.Center
        };
        _rewardLabel.AddThemeColorOverride("font_color", new Color(0.4f, 0.7f, 0.5f));
        _rewardLabel.AddThemeFontSizeOverride("font_size", 12);
        vbox.AddChild(_rewardLabel);

        vbox.AddChild(new Control { CustomMinimumSize = new Vector2(0, 40) });

        _continueBtn = new Button
        {
            Text = "Return to Camp",
            CustomMinimumSize = new Vector2(200, 40),
            SizeFlagsHorizontal = SizeFlags.ShrinkCenter
        };
        _continueBtn.Pressed += OnContinue;
        vbox.AddChild(_continueBtn);

        // Animate fade-in.
        Modulate = new Color(1, 1, 1, 0);
        var tween = CreateTween();
        tween.TweenProperty(this, "modulate:a", 1.0f, 2.0f);
    }

    private void OnContinue()
    {
        // Save progress.
        GameManager.Instance?.Save("slot1");

        // Return to camp.
        GetTree().ChangeSceneToFile("res://Scenes/World/Camp.tscn");
    }
}
