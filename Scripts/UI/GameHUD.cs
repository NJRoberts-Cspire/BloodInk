using Godot;
using BloodInk.Combat;
using BloodInk.Stealth;
using BloodInk.Interaction;

namespace BloodInk.UI;

/// <summary>
/// Upgraded HUD — health, stealth indicator, interact prompt,
/// ink totals, area name, and alert state display.
/// </summary>
public partial class GameHUD : CanvasLayer
{
    // ─── Health ───────────────────────────────────────────────────
    private Label? _healthLabel;
    private TextureProgressBar? _healthBar;

    // ─── Stealth ──────────────────────────────────────────────────
    private Label? _stealthLabel;
    private TextureProgressBar? _stealthBar;

    // ─── Interact prompt ──────────────────────────────────────────
    private Label? _interactPrompt;

    // ─── Area name ────────────────────────────────────────────────
    private Label? _areaLabel;
    private float _areaFadeTimer;

    // ─── Ink totals ───────────────────────────────────────────────
    private Label? _inkLabel;

    // ─── Alert indicator ──────────────────────────────────────────
    private Label? _alertLabel;

    // ─── Crouch indicator ─────────────────────────────────────────
    private Label? _crouchLabel;

    public override void _Ready()
    {
        _healthLabel = GetNodeOrNull<Label>("TopLeft/HealthLabel");
        _healthBar = GetNodeOrNull<TextureProgressBar>("TopLeft/HealthBar");
        _stealthLabel = GetNodeOrNull<Label>("TopLeft/StealthLabel");
        _stealthBar = GetNodeOrNull<TextureProgressBar>("TopLeft/StealthBar");
        _interactPrompt = GetNodeOrNull<Label>("BottomCenter/InteractPrompt");
        _areaLabel = GetNodeOrNull<Label>("TopCenter/AreaLabel");
        _inkLabel = GetNodeOrNull<Label>("TopRight/InkLabel");
        _alertLabel = GetNodeOrNull<Label>("TopRight/AlertLabel");
        _crouchLabel = GetNodeOrNull<Label>("TopLeft/CrouchLabel");

        if (_interactPrompt != null) _interactPrompt.Visible = false;
        if (_areaLabel != null) _areaLabel.Modulate = new Color(1, 1, 1, 0);

        ConnectSignals();
    }

    public override void _Process(double delta)
    {
        UpdateStealthIndicator();
        UpdateAreaFade(delta);
    }

    // ─── Signal Connections ───────────────────────────────────────

    private void ConnectSignals()
    {
        // InteractionManager prompts.
        var im = InteractionManager.Instance;
        if (im != null)
        {
            im.InteractPromptChanged += OnInteractPromptChanged;
            im.InteractPromptHidden += OnInteractPromptHidden;
        }
        else
        {
            // Deferred connection — manager might init after us.
            CallDeferred(MethodName.ConnectInteractionManager);
        }
    }

    private void ConnectInteractionManager()
    {
        var im = InteractionManager.Instance;
        if (im == null) return;
        im.InteractPromptChanged += OnInteractPromptChanged;
        im.InteractPromptHidden += OnInteractPromptHidden;
    }

    // ─── Health ───────────────────────────────────────────────────

    public void OnHealthChanged(int current, int max)
    {
        if (_healthLabel != null)
            _healthLabel.Text = $"HP: {current}/{max}";
        if (_healthBar != null)
        {
            _healthBar.MaxValue = max;
            _healthBar.Value = current;
        }
    }

    // ─── Stealth Indicator ────────────────────────────────────────

    private void UpdateStealthIndicator()
    {
        var player = GetTree().GetFirstNodeInGroup("Player") as Node2D;
        if (player == null) return;

        var stealth = player.GetNodeOrNull<StealthProfile>("StealthProfile");
        if (stealth == null) return;

        string visText = stealth.Visibility switch
        {
            VisibilityLevel.Hidden => "[HIDDEN]",
            VisibilityLevel.Low => "[Low Profile]",
            VisibilityLevel.Normal => "[Visible]",
            VisibilityLevel.Exposed => "[EXPOSED]",
            _ => ""
        };

        Color visColor = stealth.Visibility switch
        {
            VisibilityLevel.Hidden => new Color(0.2f, 0.6f, 0.2f),
            VisibilityLevel.Low => new Color(0.4f, 0.7f, 0.3f),
            VisibilityLevel.Normal => new Color(0.9f, 0.9f, 0.5f),
            VisibilityLevel.Exposed => new Color(1f, 0.3f, 0.2f),
            _ => Colors.White
        };

        if (_stealthLabel != null)
        {
            _stealthLabel.Text = visText;
            _stealthLabel.Modulate = visColor;
        }

        if (_crouchLabel != null)
        {
            _crouchLabel.Visible = stealth.IsCrouching;
            _crouchLabel.Text = "🔽 Crouching";
        }
    }

    // ─── Interact Prompt ──────────────────────────────────────────

    private void OnInteractPromptChanged(string promptText)
    {
        if (_interactPrompt != null)
        {
            _interactPrompt.Text = promptText;
            _interactPrompt.Visible = true;
        }
    }

    private void OnInteractPromptHidden()
    {
        if (_interactPrompt != null)
            _interactPrompt.Visible = false;
    }

    // ─── Area Name ────────────────────────────────────────────────

    public void ShowAreaName(string areaName)
    {
        if (_areaLabel == null) return;
        _areaLabel.Text = areaName;
        _areaLabel.Modulate = new Color(1, 1, 1, 1);
        _areaFadeTimer = 3f; // Show for 3 seconds then fade.
    }

    private void UpdateAreaFade(double delta)
    {
        if (_areaLabel == null || _areaFadeTimer <= 0) return;

        _areaFadeTimer -= (float)delta;
        if (_areaFadeTimer <= 0)
        {
            // Fade out over 1 second.
            var tween = CreateTween();
            tween.TweenProperty(_areaLabel, "modulate:a", 0.0f, 1.0f);
        }
    }

    // ─── Alert ────────────────────────────────────────────────────

    public void ShowAlertLevel(string level)
    {
        if (_alertLabel != null)
            _alertLabel.Text = level;
    }

    // ─── Ink Totals ───────────────────────────────────────────────

    public void UpdateInkDisplay(int major, int lesser, int trace)
    {
        if (_inkLabel != null)
            _inkLabel.Text = $"Ink  M:{major}  L:{lesser}  T:{trace}";
    }
}
