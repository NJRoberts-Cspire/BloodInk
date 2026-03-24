using Godot;

namespace BloodInk.BloodEchoes;

/// <summary>
/// Base class for all Blood Echo scenes.
/// Handles the common UI overlay (title card, whisper text, fade in/out),
/// input to advance, and calling CompleteEcho() to return to camp.
///
/// Derived classes override BuildEchoWorld() to add scene-specific environment,
/// characters, and narration nodes.
/// </summary>
public abstract partial class BloodEchoScene : Node2D
{
    // ─── Configuration (set by derived classes) ───────────────────

    protected string EchoId { get; set; } = "";
    protected string EchoTitle { get; set; } = "Memory Fragment";
    protected string EdictbearerName { get; set; } = "Unknown";
    protected string WhisperText { get; set; } = "";
    protected string NarrationText { get; set; } = "";

    // ─── UI Nodes ────────────────────────────────────────────────

    private CanvasLayer? _ui;
    private Control? _titleCard;
    private Label? _titleLabel;
    private Label? _edictbearerLabel;
    private RichTextLabel? _narrationLabel;
    private Label? _whisperLabel;
    private Label? _continueHint;
    private ColorRect? _fadeRect;

    // ─── State ───────────────────────────────────────────────────

    private enum EchoPhase { FadeIn, TitleCard, Playing, WhisperReveal, FadeOut }
    private EchoPhase _phase = EchoPhase.FadeIn;
    private float _phaseTimer;
    private bool _inputReady;

    private const float FadeTime = 1.5f;
    private const float TitleHoldTime = 3.0f;
    private const float WhisperRevealTime = 4.0f;

    public override void _Ready()
    {
        BuildUI();
        BuildEchoWorld();
        SpawnEchoPlayer();
        SetupFadeIn();
    }

    private void SpawnEchoPlayer()
    {
        // Re-use the player scene if available; otherwise do nothing — echoes
        // can still be triggered by the player carried over from the previous scene.
        if (GetTree().GetFirstNodeInGroup("Player") != null) return;

        var playerScene = GD.Load<PackedScene>("res://Scenes/Player/Player.tscn");
        if (playerScene == null) return;

        var player = playerScene.Instantiate<CharacterBody2D>();
        // Place at world center — derived classes can override by moving the spawn
        player.Position = EchoPlayerSpawnPosition;
        AddChild(player);
    }

    /// <summary>Override in derived classes to set where the echo player spawns.</summary>
    protected virtual Vector2 EchoPlayerSpawnPosition => new(640, 500);

    public override void _UnhandledInput(InputEvent @event)
    {
        if (!_inputReady) return;

        if (@event.IsActionPressed("interact") || @event.IsActionPressed("attack") ||
            @event.IsActionPressed("dodge"))
        {
            AdvancePhase();
            GetViewport().SetInputAsHandled();
        }
    }

    public override void _Process(double delta)
    {
        _phaseTimer -= (float)delta;

        switch (_phase)
        {
            case EchoPhase.FadeIn:
                if (_fadeRect != null)
                    _fadeRect.Color = new Color(0, 0, 0, Mathf.Clamp(_phaseTimer / FadeTime, 0f, 1f));
                if (_phaseTimer <= 0f)
                    EnterPhase(EchoPhase.TitleCard);
                break;

            case EchoPhase.TitleCard:
                if (_phaseTimer <= 0f)
                {
                    if (_titleCard != null) _titleCard.Visible = false;
                    _inputReady = true;
                    EnterPhase(EchoPhase.Playing);
                }
                break;

            case EchoPhase.Playing:
                // Derived class drives this phase; player can press advance to skip to whisper
                break;

            case EchoPhase.WhisperReveal:
                if (_phaseTimer <= 0f)
                    EnterPhase(EchoPhase.FadeOut);
                break;

            case EchoPhase.FadeOut:
                if (_fadeRect != null)
                    _fadeRect.Color = new Color(0, 0, 0,
                        Mathf.Clamp(1f - (_phaseTimer / FadeTime), 0f, 1f));
                if (_phaseTimer <= 0f)
                    FinishEcho();
                break;
        }
    }

    // ─── Phase Management ─────────────────────────────────────────

    private void SetupFadeIn()
    {
        if (_fadeRect != null) _fadeRect.Color = Colors.Black;
        EnterPhase(EchoPhase.FadeIn);
    }

    private void EnterPhase(EchoPhase phase)
    {
        _phase = phase;
        _phaseTimer = phase switch
        {
            EchoPhase.FadeIn => FadeTime,
            EchoPhase.TitleCard => TitleHoldTime,
            EchoPhase.Playing => float.MaxValue,
            EchoPhase.WhisperReveal => WhisperRevealTime,
            EchoPhase.FadeOut => FadeTime,
            _ => 1f
        };

        if (phase == EchoPhase.TitleCard && _titleCard != null)
            _titleCard.Visible = true;

        if (phase == EchoPhase.WhisperReveal)
        {
            if (_narrationLabel != null) _narrationLabel.Visible = false;
            if (_whisperLabel != null)
            {
                _whisperLabel.Text = $"\"{WhisperText}\"";
                _whisperLabel.Visible = true;
            }
            if (_continueHint != null) _continueHint.Visible = true;
            _inputReady = true;
        }

        if (phase == EchoPhase.FadeOut && _fadeRect != null)
            _fadeRect.Color = Colors.Transparent;
    }

    private void AdvancePhase()
    {
        switch (_phase)
        {
            case EchoPhase.Playing:
                EnterPhase(EchoPhase.WhisperReveal);
                break;
            case EchoPhase.WhisperReveal:
                EnterPhase(EchoPhase.FadeOut);
                break;
        }
    }

    private void FinishEcho()
    {
        GD.Print($"[BloodEcho] {EchoId} complete — returning to camp.");
        Core.GameManager.Instance?.EchoManager?.CompleteEcho();
    }

    // ─── Echo World ──────────────────────────────────────────────

    /// <summary>
    /// Override to build the echo's visual environment, characters, and narration.
    /// Called after UI is ready.
    /// </summary>
    protected abstract void BuildEchoWorld();

    /// <summary>Trigger the whisper phase early (called by derived classes when scene moment is reached).</summary>
    protected void TriggerWhisperReveal()
    {
        if (_phase == EchoPhase.Playing)
            EnterPhase(EchoPhase.WhisperReveal);
    }

    /// <summary>Set the narration text shown during the Playing phase.</summary>
    protected void SetNarration(string text)
    {
        if (_narrationLabel != null)
        {
            _narrationLabel.Text = text;
            _narrationLabel.Visible = true;
        }
    }

    // ─── UI Builder ──────────────────────────────────────────────

    private void BuildUI()
    {
        _ui = new CanvasLayer { Name = "EchoUI", Layer = 10 };
        AddChild(_ui);

        // Background fade rect (full screen black)
        _fadeRect = new ColorRect
        {
            Name = "FadeRect",
            Color = Colors.Black,
            AnchorRight = 1,
            AnchorBottom = 1
        };
        _ui.AddChild(_fadeRect);

        // Title card panel
        _titleCard = new Control { Name = "TitleCard", Visible = false };
        _titleCard.SetAnchorsPreset(Control.LayoutPreset.FullRect);
        _ui.AddChild(_titleCard);

        var titleBg = new ColorRect
        {
            Color = new Color(0, 0, 0, 0.7f),
            AnchorRight = 1,
            AnchorBottom = 1
        };
        _titleCard.AddChild(titleBg);

        _titleLabel = new Label
        {
            Name = "EchoTitle",
            Text = $"BLOOD ECHO\n{EchoTitle}",
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            AnchorLeft = 0.1f, AnchorRight = 0.9f,
            AnchorTop = 0.35f, AnchorBottom = 0.55f
        };
        _titleCard.AddChild(_titleLabel);

        _edictbearerLabel = new Label
        {
            Name = "EdictbearerName",
            Text = EdictbearerName,
            HorizontalAlignment = HorizontalAlignment.Center,
            AnchorLeft = 0.1f, AnchorRight = 0.9f,
            AnchorTop = 0.56f, AnchorBottom = 0.65f
        };
        _titleCard.AddChild(_edictbearerLabel);

        // Narration overlay (lower third during Playing phase)
        _narrationLabel = new RichTextLabel
        {
            Name = "Narration",
            Visible = false,
            AnchorLeft = 0.05f, AnchorRight = 0.95f,
            AnchorTop = 0.78f, AnchorBottom = 0.94f,
            BbcodeEnabled = false
        };
        _ui.AddChild(_narrationLabel);

        // Whisper text (full screen, centered)
        _whisperLabel = new Label
        {
            Name = "WhisperText",
            Visible = false,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            AnchorLeft = 0.1f, AnchorRight = 0.9f,
            AnchorTop = 0.35f, AnchorBottom = 0.65f,
            AutowrapMode = TextServer.AutowrapMode.Word
        };
        _ui.AddChild(_whisperLabel);

        // Continue hint
        _continueHint = new Label
        {
            Name = "ContinueHint",
            Text = "[E / Attack] to continue",
            Visible = false,
            HorizontalAlignment = HorizontalAlignment.Center,
            AnchorLeft = 0.3f, AnchorRight = 0.7f,
            AnchorTop = 0.88f, AnchorBottom = 0.95f
        };
        _ui.AddChild(_continueHint);
    }
}
