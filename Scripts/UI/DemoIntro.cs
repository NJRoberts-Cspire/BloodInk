using Godot;
using BloodInk.Dialogue;

namespace BloodInk.UI;

/// <summary>
/// Fullscreen narration sequence shown before the player's first visit to camp.
/// Displays 5 text cards on a black background, each fading in then out,
/// then transitions to the Camp scene.
/// Press UI_cancel at any time to skip straight to Camp.
/// </summary>
public partial class DemoIntro : Node
{
    private static readonly string[] Cards = new[]
    {
        "Eighty years ago, the Needlewise cursed the orc people. They called it the Edict.",
        "Orc blood grows thin. Orc bones grow brittle. The human kingdoms call it mercy — an end to a savage race.",
        "You are Vetch. Hollow Hand. Assassin.",
        "Your clan collects ink from the dead — human and orc alike. Tattoos carry the strength of those who no longer need it.",
        "There are six Edictbearers. Six humans who maintain the curse. Kill them. Break the Edict. Save your people."
    };

    private const string CampScenePath = "res://Scenes/World/Camp.tscn";
    private const float FadeTime   = 0.6f;   // Seconds to fade a card in or out.
    private const float HoldTime   = 2.8f;   // Seconds a card stays fully visible.

    private CanvasLayer? _layer;
    private ColorRect?   _background;
    private Label?       _label;

    private int  _currentCard = 0;
    private bool _skipping    = false;

    public override void _Ready()
    {
        // Build the scene tree procedurally — no .tscn needed.
        _layer = new CanvasLayer { Layer = 20 };
        AddChild(_layer);

        _background = new ColorRect
        {
            Color = Colors.Black,
            AnchorLeft   = 0, AnchorTop    = 0,
            AnchorRight  = 1, AnchorBottom = 1,
            OffsetLeft   = 0, OffsetTop    = 0,
            OffsetRight  = 0, OffsetBottom = 0,
        };
        _background.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
        _layer.AddChild(_background);

        _label = new Label
        {
            AutowrapMode         = TextServer.AutowrapMode.WordSmart,
            HorizontalAlignment  = HorizontalAlignment.Center,
            VerticalAlignment    = VerticalAlignment.Center,
            Modulate             = new Color(1, 1, 1, 0),
        };
        _label.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
        // Inset the label so it doesn't sit edge-to-edge.
        _label.OffsetLeft  =  120;
        _label.OffsetRight = -120;
        _label.AddThemeFontSizeOverride("font_size", 28);
        _layer.AddChild(_label);

        ShowCard(_currentCard);
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event.IsActionPressed("ui_cancel"))
            Skip();
    }

    // ─── Card Sequencing ─────────────────────────────────────────

    private void ShowCard(int index)
    {
        if (_skipping || _label == null) return;
        if (index >= Cards.Length) { GoToCamp(); return; }

        _label.Text = Cards[index];

        var tween = CreateTween().SetProcessMode(Tween.TweenProcessMode.Always);
        // Fade in.
        tween.TweenProperty(_label, "modulate:a", 1.0f, FadeTime);
        // Hold.
        tween.TweenInterval(HoldTime);
        // Fade out.
        tween.TweenProperty(_label, "modulate:a", 0.0f, FadeTime);
        // Advance.
        tween.TweenCallback(Callable.From(AdvanceCard));
    }

    private void AdvanceCard()
    {
        _currentCard++;
        ShowCard(_currentCard);
    }

    private void Skip()
    {
        if (_skipping) return;
        _skipping = true;
        GoToCamp();
    }

    // ─── Camp Transition ─────────────────────────────────────────

    private void GoToCamp()
    {
        // Mark intro as seen so CampScene doesn't trigger it again.
        var choices = Core.GameManager.Instance?.Choices;
        choices?.MakeChoice(DialogueFlags.SawIntro, 1);

        // Fade to black then load Camp.
        VFX.ScreenTransition.Instance?.FadeToBlack(0.5f);

        var tree = GetTree();
        var timer = tree.CreateTimer(0.6f);
        timer.Timeout += () => tree.ChangeSceneToFile(CampScenePath);
    }
}
