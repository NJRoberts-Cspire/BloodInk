using Godot;
using System.Linq;

namespace BloodInk.UI;

/// <summary>
/// End-of-demo screen shown after Lord Cowl is killed and the player returns to camp.
/// Displays a thank-you message, a few play-session stats, and a Steam wishlist
/// placeholder button.
/// Triggered by the "show_demo_end" dialogue event in CampScene.
/// </summary>
public partial class DemoEnd : CanvasLayer
{
    private const string SteamUrl = "https://store.steampowered.com"; // placeholder

    public override void _Ready()
    {
        Layer = 30;
        ProcessMode = ProcessModeEnum.Always;

        BuildUI();
    }

    private void BuildUI()
    {
        // Fullscreen black background.
        var bg = new ColorRect { Color = new Color(0, 0, 0, 0.92f) };
        bg.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
        AddChild(bg);

        // Centered VBox.
        var vbox = new VBoxContainer
        {
            CustomMinimumSize = new Vector2(600, 0),
        };
        vbox.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.Center);
        vbox.OffsetLeft  = -300;
        vbox.OffsetRight =  300;
        vbox.OffsetTop   = -200;
        vbox.OffsetBottom = 200;
        vbox.AddThemeConstantOverride("separation", 18);
        AddChild(vbox);

        // ── Thank-you header ───────────────────────────────────
        var header = new Label
        {
            Text                = "Thanks for playing the BloodInk Demo",
            HorizontalAlignment = HorizontalAlignment.Center,
        };
        header.AddThemeFontSizeOverride("font_size", 32);
        header.AddThemeColorOverride("font_color", new Color(0.9f, 0.75f, 0.4f));
        vbox.AddChild(header);

        // ── Divider ───────────────────────────────────────────
        vbox.AddChild(new HSeparator());

        // ── Stats ─────────────────────────────────────────────
        var gm = Core.GameManager.Instance;

        int missionsCompleted = 0;
        int targetsKilled     = 0;
        if (gm != null)
        {
            var kingdom = gm.Kingdoms[0];
            if (kingdom != null)
            {
                var killed = kingdom.GetKilledTargets().ToList();
                targetsKilled     = killed.Count;
                missionsCompleted = killed.Count; // One mission per kill in the demo.
            }
        }

        int tattoosApplied = gm?.TattooSystem?.TotalTattoosApplied ?? 0;

        AddStat(vbox, "Missions Completed",  missionsCompleted.ToString());
        AddStat(vbox, "Targets Killed",      targetsKilled.ToString());
        AddStat(vbox, "Tattoos Applied",     tattoosApplied.ToString());

        // ── Divider ───────────────────────────────────────────
        vbox.AddChild(new HSeparator());

        // ── Wishlist button ───────────────────────────────────
        var wishlistBtn = new Button
        {
            Text              = "Wishlist on Steam",
            CustomMinimumSize = new Vector2(260, 48),
        };
        wishlistBtn.AddThemeFontSizeOverride("font_size", 20);
        wishlistBtn.Pressed += OnWishlistPressed;

        var wishlistCenter = new CenterContainer();
        wishlistCenter.AddChild(wishlistBtn);
        vbox.AddChild(wishlistCenter);

        // ── Quit button ───────────────────────────────────────
        var quitBtn = new Button
        {
            Text              = "Quit",
            CustomMinimumSize = new Vector2(260, 48),
        };
        quitBtn.AddThemeFontSizeOverride("font_size", 20);
        quitBtn.Pressed += OnQuitPressed;

        var quitCenter = new CenterContainer();
        quitCenter.AddChild(quitBtn);
        vbox.AddChild(quitCenter);

        // Fade in.
        Modulate = new Color(1, 1, 1, 0);
        var tween = CreateTween().SetProcessMode(Tween.TweenProcessMode.Always);
        tween.TweenProperty(this, "modulate:a", 1.0f, 0.8f);
    }

    private static void AddStat(VBoxContainer parent, string label, string value)
    {
        var hbox = new HBoxContainer();
        hbox.AddThemeConstantOverride("separation", 12);

        var lbl = new Label { Text = label };
        lbl.AddThemeFontSizeOverride("font_size", 20);
        lbl.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;

        var val = new Label
        {
            Text                = value,
            HorizontalAlignment = HorizontalAlignment.Right,
        };
        val.AddThemeFontSizeOverride("font_size", 20);
        val.AddThemeColorOverride("font_color", new Color(0.9f, 0.75f, 0.4f));

        hbox.AddChild(lbl);
        hbox.AddChild(val);
        parent.AddChild(hbox);
    }

    private void OnWishlistPressed()
    {
        OS.ShellOpen(SteamUrl);
    }

    private void OnQuitPressed()
    {
        GetTree().Quit();
    }
}
