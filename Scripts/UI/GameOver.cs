using Godot;

namespace BloodInk.UI;

/// <summary>
/// Game Over screen — shown when the player dies.
/// Options: Retry (reload current mission) or Quit to Menu.
/// </summary>
public partial class GameOver : Control
{
    /// <summary>The scene to retry (set before transitioning here).</summary>
    public static string LastMissionScene { get; set; } = "";

    public override void _Ready()
    {
        // Build UI.
        var bg = new ColorRect
        {
            Color = new Color(0.05f, 0.01f, 0.01f, 1f),
            LayoutMode = 1
        };
        bg.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
        AddChild(bg);

        var vbox = new VBoxContainer
        {
            LayoutMode = 1,
        };
        vbox.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
        vbox.AddThemeConstantOverride("separation", 20);
        AddChild(vbox);

        vbox.AddChild(new Control { CustomMinimumSize = new Vector2(0, 80) });

        var title = new Label
        {
            Text = "T H E   E D I C T   E N D U R E S",
            HorizontalAlignment = HorizontalAlignment.Center
        };
        title.AddThemeColorOverride("font_color", new Color(0.6f, 0.12f, 0.08f));
        title.AddThemeFontSizeOverride("font_size", 18);
        vbox.AddChild(title);

        var subtitle = new Label
        {
            Text = "Vetch falls. The orcs' last hope bleeds out on foreign stone.",
            HorizontalAlignment = HorizontalAlignment.Center
        };
        subtitle.AddThemeColorOverride("font_color", new Color(0.5f, 0.4f, 0.35f));
        subtitle.AddThemeFontSizeOverride("font_size", 10);
        vbox.AddChild(subtitle);

        vbox.AddChild(new Control { CustomMinimumSize = new Vector2(0, 40) });

        var retryBtn = new Button
        {
            Text = "Try Again",
            CustomMinimumSize = new Vector2(180, 36),
            SizeFlagsHorizontal = SizeFlags.ShrinkCenter
        };
        retryBtn.Pressed += OnRetry;
        vbox.AddChild(retryBtn);

        var menuBtn = new Button
        {
            Text = "Quit to Menu",
            CustomMinimumSize = new Vector2(180, 36),
            SizeFlagsHorizontal = SizeFlags.ShrinkCenter
        };
        menuBtn.Pressed += OnQuitToMenu;
        vbox.AddChild(menuBtn);

        // Fade in.
        Modulate = new Color(1, 1, 1, 0);
        var tween = CreateTween();
        tween.TweenProperty(this, "modulate:a", 1.0f, 1.5f);
    }

    private void OnRetry()
    {
        if (!string.IsNullOrEmpty(LastMissionScene))
        {
            GetTree().Paused = false;
            GetTree().ChangeSceneToFile(LastMissionScene);
        }
        else
        {
            // Fallback — go to camp.
            GetTree().Paused = false;
            GetTree().ChangeSceneToFile("res://Scenes/World/Camp.tscn");
        }
    }

    private void OnQuitToMenu()
    {
        GetTree().Paused = false;
        GetTree().ChangeSceneToFile("res://Scenes/UI/MainMenu.tscn");
    }
}
