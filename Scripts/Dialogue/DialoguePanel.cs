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
        if (!_isTyping) return;

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

    public override void _UnhandledInput(InputEvent @event)
    {
        if (!Visible) return;

        if (@event.IsActionPressed("interact") || @event.IsActionPressed("attack"))
        {
            if (_waitingForChoice) return; // choices need button press

            if (_isTyping)
            {
                // Skip typewriter — show full text instantly.
                _visibleChars = _fullText.Length;
                if (_textLabel != null)
                    _textLabel.VisibleCharacters = _visibleChars;
                _isTyping = false;
            }
            else
            {
                DialogueManager.Instance?.Advance();
            }

            GetViewport().SetInputAsHandled();
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
