using Godot;
using System;
using System.Collections.Generic;

namespace BloodInk.Dialogue;

/// <summary>
/// A dialogue box UI that listens to DialogueManager signals and renders
/// the conversation: speaker name, portrait, text with typewriter effect,
/// and choice buttons.
/// </summary>
public partial class DialoguePanel : Control
{
    [Export] public float TypewriterSpeed { get; set; } = 30f; // chars per second

    // ─── Node refs (assigned in _Ready or via Export) ─────────────
    private Label? _speakerLabel;
    private RichTextLabel? _textLabel;
    private TextureRect? _portraitRect;
    private VBoxContainer? _choiceContainer;
    private Panel? _panelBg;

    private string _fullText = "";
    private int _visibleChars = 0;
    private float _charTimer = 0f;
    private bool _isTyping = false;
    private bool _waitingForChoice = false;

    // ─── Continue indicator ───────────────────────────────────────
    private Label? _continueHint; // "▼" blink shown when text is done
    private float _blinkTimer;

    // ─── Hold-to-skip ─────────────────────────────────────────────
    private float _holdTimer;
    private const float HoldSkipDuration = 1.0f; // seconds to hold before skipping
    private bool _holdingAdvance;

    public override void _Ready()
    {
        _panelBg = GetNodeOrNull<Panel>("PanelBg");
        _speakerLabel = GetNodeOrNull<Label>("PanelBg/SpeakerLabel");
        _textLabel = GetNodeOrNull<RichTextLabel>("PanelBg/TextLabel");
        _portraitRect = GetNodeOrNull<TextureRect>("PanelBg/Portrait");
        _choiceContainer = GetNodeOrNull<VBoxContainer>("PanelBg/ChoiceContainer");

        Visible = false;

        // Must process during pause so typewriter + input work while dialogue pauses the game.
        ProcessMode = ProcessModeEnum.Always;

        // Continue hint — try to get it from the scene, otherwise build it.
        _continueHint = GetNodeOrNull<Label>("PanelBg/ContinueHint");
        if (_continueHint == null && _panelBg != null)
        {
            _continueHint = new Label
            {
                Name = "ContinueHint",
                Text = "▼",
                HorizontalAlignment = HorizontalAlignment.Right,
                LayoutMode = 1,
                AnchorsPreset = (int)LayoutPreset.BottomRight,
                OffsetBottom = -6,
                OffsetRight  = -10,
                OffsetTop    = -28,
                OffsetLeft   = -40,
            };
            _continueHint.AddThemeColorOverride("font_color", new Color(0.7f, 0.5f, 0.3f));
            _continueHint.Visible = false;
            _panelBg.AddChild(_continueHint);
        }

        // Connect to DialogueManager signals (deferred — manager may load after us).
        CallDeferred(MethodName.ConnectToManager);
    }

    public override void _ExitTree()
    {
        // Disconnect from the singleton manager to prevent stale delegate crashes.
        var mgr = DialogueManager.Instance;
        if (mgr == null) return;
        mgr.LineDisplayed -= OnLineDisplayed;
        mgr.ChoicesPresented -= OnChoicesPresented;
        mgr.ConversationStarted -= OnConversationStarted;
        mgr.ConversationEnded -= OnConversationEnded;
    }

    private void ConnectToManager()
    {
        var mgr = DialogueManager.Instance;
        if (mgr == null)
        {
            GD.PrintErr("DialoguePanel: No DialogueManager instance found.");
            return;
        }

        mgr.LineDisplayed += OnLineDisplayed;
        mgr.ChoicesPresented += OnChoicesPresented;
        mgr.ConversationStarted += OnConversationStarted;
        mgr.ConversationEnded += OnConversationEnded;
    }

    public override void _Process(double delta)
    {
        if (!Visible) return;

        // ── Typewriter ──────────────────────────────────────────────
        if (_isTyping)
        {
            _charTimer += (float)delta;
            float interval = 1f / TypewriterSpeed;
            while (_charTimer >= interval && _visibleChars < _fullText.Length)
            {
                _visibleChars++;
                _charTimer -= interval;
            }

            if (_textLabel != null)
                _textLabel.VisibleCharacters = _visibleChars;

            if (_visibleChars >= _fullText.Length)
                _isTyping = false;
        }

        // ── Continue-hint blink (shown when idle, no choices) ────────
        if (_continueHint != null)
        {
            bool showHint = !_isTyping && !_waitingForChoice;
            if (showHint)
            {
                _blinkTimer += (float)delta;
                _continueHint.Visible = (int)(_blinkTimer * 2f) % 2 == 0;
            }
            else
            {
                _continueHint.Visible = false;
                _blinkTimer = 0f;
            }
        }

        // ── Hold-to-skip ────────────────────────────────────────────
        if (_holdingAdvance)
        {
            _holdTimer += (float)delta;
            if (_holdTimer >= HoldSkipDuration)
            {
                _holdingAdvance = false;
                _holdTimer = 0f;
                DialogueManager.Instance?.EndConversation();
            }
        }
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (!Visible) return;

        bool isAdvanceAction = @event.IsAction("interact") || @event.IsAction("attack");
        if (!isAdvanceAction) return;

        if (@event.IsPressed() && !@event.IsEcho())
        {
            if (_waitingForChoice) return; // choices need button press

            if (_isTyping)
            {
                // First press: complete the line instantly.
                _visibleChars = _fullText.Length;
                if (_textLabel != null)
                    _textLabel.VisibleCharacters = _visibleChars;
                _isTyping = false;
            }
            else
            {
                // Begin tracking hold for skip.
                _holdingAdvance = true;
                _holdTimer = 0f;
                DialogueManager.Instance?.Advance();
            }

            GetViewport().SetInputAsHandled();
        }
        else if (!@event.IsPressed())
        {
            // Button released before hold threshold — just a normal tap (already advanced above).
            _holdingAdvance = false;
            _holdTimer = 0f;
        }
    }

    // ─── Signal Handlers ──────────────────────────────────────────

    private void OnConversationStarted(string conversationId)
    {
        Visible = true;
        ClearChoices();
    }

    private void OnConversationEnded(string conversationId)
    {
        Visible = false;
        _isTyping = false;
        _waitingForChoice = false;
        _holdingAdvance = false;
        _holdTimer = 0f;
        if (_continueHint != null) _continueHint.Visible = false;
    }

    private void OnLineDisplayed(string speaker, string portrait, string text)
    {
        _waitingForChoice = false;
        ClearChoices();

        if (_speakerLabel != null)
            _speakerLabel.Text = string.IsNullOrEmpty(speaker) ? "" : speaker;

        // Portrait loading (if we have a portrait texture path mapping).
        if (_portraitRect != null)
        {
            if (!string.IsNullOrEmpty(portrait))
            {
                var tex = GD.Load<Texture2D>($"res://Assets/Portraits/{portrait}.png");
                _portraitRect.Texture = tex;
                _portraitRect.Visible = tex != null;
            }
            else
            {
                _portraitRect.Visible = false;
            }
        }

        // Begin typewriter.
        _fullText = text;
        _visibleChars = 0;
        _charTimer = 0f;
        _isTyping = true;

        if (_textLabel != null)
        {
            _textLabel.Text = text;
            _textLabel.VisibleCharacters = 0;
        }
    }

    private void OnChoicesPresented(string[] labels)
    {
        _waitingForChoice = true;
        ClearChoices();

        if (_choiceContainer == null) return;

        for (int i = 0; i < labels.Length; i++)
        {
            var btn = new Button();
            btn.Text = labels[i];
            int idx = i; // capture for lambda
            btn.Pressed += () => OnChoicePressed(idx);
            btn.SizeFlagsHorizontal = SizeFlags.ExpandFill;
            _choiceContainer.AddChild(btn);
        }
    }

    private void OnChoicePressed(int index)
    {
        _waitingForChoice = false;
        ClearChoices();
        DialogueManager.Instance?.SelectChoice(index);
    }

    private void ClearChoices()
    {
        if (_choiceContainer == null) return;
        foreach (var child in _choiceContainer.GetChildren())
            child.QueueFree();
    }
}
