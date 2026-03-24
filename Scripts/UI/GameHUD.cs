using Godot;
using BloodInk.Abilities;
using BloodInk.Combat;
using BloodInk.Ink;
using BloodInk.Interaction;
using BloodInk.Stealth;

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

    // ─── Mask of Ash indicator ─────────────────────────────────────
    private Label? _maskLabel;
    private TextureProgressBar? _maskBar;

    // ─── Ink conflict indicator ────────────────────────────────────
    private Label? _inkConflictLabel;

    // ─── Blood Echo unlock toast ───────────────────────────────────
    private Label? _echoToastLabel;
    private Tween? _echoToastTween;

    // ─── Cached references ────────────────────────────────────────
    private Node2D? _cachedPlayer;
    private StealthProfile? _cachedStealth;

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
        _maskLabel       = GetNodeOrNull<Label>("TopLeft/MaskLabel");
        _maskBar         = GetNodeOrNull<TextureProgressBar>("TopLeft/MaskBar");
        _inkConflictLabel = GetNodeOrNull<Label>("TopLeft/InkConflictLabel");

        if (_interactPrompt   != null) _interactPrompt.Visible   = false;
        if (_areaLabel        != null) _areaLabel.Modulate        = new Color(1, 1, 1, 0);
        if (_maskLabel        != null) _maskLabel.Visible         = false;
        if (_maskBar          != null) _maskBar.Visible           = false;
        if (_inkConflictLabel != null) _inkConflictLabel.Visible  = false;

        // Blood Echo toast — built programmatically so it works without a scene file.
        _echoToastLabel = GetNodeOrNull<Label>("Center/EchoToastLabel");
        if (_echoToastLabel == null)
        {
            _echoToastLabel = new Label
            {
                Name                  = "EchoToastLabel",
                HorizontalAlignment   = HorizontalAlignment.Center,
                VerticalAlignment     = VerticalAlignment.Center,
                AutowrapMode          = TextServer.AutowrapMode.Off,
                Modulate              = new Color(1, 1, 1, 0), // Start invisible.
                ZIndex                = 10,
                // Anchored to the lower-center of the screen.
                LayoutMode            = 1,
                AnchorsPreset         = (int)LayoutPreset.CenterBottom,
                OffsetBottom          = -60,
                OffsetTop             = -90,
                OffsetLeft            = -220,
                OffsetRight           = 220,
            };
            AddChild(_echoToastLabel);
        }

        // Connect TattooSystem conflict signal so we can show an immediate flash.
        var tattoo = Core.GameManager.Instance?.TattooSystem;
        if (tattoo != null)
            tattoo.InkConflictTriggered += OnInkConflictTriggered;

        // Connect MissionAlertManager so the alert label reflects lockdown state.
        ConnectAlertManager();

        // Connect BloodEchoManager so we show a toast when a memory unlocks.
        var echoMgr = Core.GameManager.Instance?.EchoManager;
        if (echoMgr != null)
            echoMgr.EchoUnlocked += OnEchoUnlocked;

        ConnectSignals();
    }

    public override void _ExitTree()
    {
        var im = InteractionManager.Instance;
        if (im != null)
        {
            im.InteractPromptChanged -= OnInteractPromptChanged;
            im.InteractPromptHidden -= OnInteractPromptHidden;
        }

        var ink = Core.GameManager.Instance?.InkInventory;
        if (ink != null)
            ink.InkChanged -= OnInkChanged;
    }

    public override void _Process(double delta)
    {
        UpdateStealthIndicator();
        UpdateAreaFade(delta);
        UpdateMaskIndicator();
        UpdateInkConflictIndicator();
        UpdateAlertGuardCount();
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

        // Ink inventory display.
        var ink = Core.GameManager.Instance?.InkInventory;
        if (ink != null)
        {
            ink.InkChanged += OnInkChanged;
            // Show current totals immediately.
            UpdateInkDisplay(
                ink.GetInk(InkGrade.Major),
                ink.GetInk(InkGrade.Lesser),
                ink.GetInk(InkGrade.Trace));
        }
    }

    private void OnInkChanged(int grade, int amount)
    {
        var ink = Core.GameManager.Instance?.InkInventory;
        if (ink == null) return;
        UpdateInkDisplay(
            ink.GetInk(InkGrade.Major),
            ink.GetInk(InkGrade.Lesser),
            ink.GetInk(InkGrade.Trace));
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
        // Use cached references; re-lookup only if invalidated.
        if (_cachedPlayer == null || !IsInstanceValid(_cachedPlayer))
        {
            _cachedPlayer = GetTree().GetFirstNodeInGroup("Player") as Node2D;
            _cachedStealth = _cachedPlayer?.GetNodeOrNull<StealthProfile>("StealthProfile");
        }
        if (_cachedPlayer == null || _cachedStealth == null) return;

        var stealth = _cachedStealth;

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

    // ─── Ink Conflict Indicator ───────────────────────────────────

    private float _inkConflictFlashTimer;

    private void OnInkConflictTriggered(string conflictType)
    {
        // Flash the label for 2 seconds whenever a new conflict event fires.
        _inkConflictFlashTimer = 2f;
    }

    /// <summary>
    /// Polls TattooSystem each frame. Shows a persistent label whenever Shadow+Fang
    /// (Ink Bleed) or Root+Bone (Ink Calm) conflicts are active, with severity colour.
    /// </summary>
    private void UpdateInkConflictIndicator()
    {
        if (_inkConflictLabel == null) return;

        var tattoo = Core.GameManager.Instance?.TattooSystem;
        if (tattoo == null) { _inkConflictLabel.Visible = false; return; }

        float bleed = tattoo.GetInkBleedChance();   // 0 → ~0.3
        float calm  = tattoo.GetInkCalmPenalty();   // 0 → ~0.4

        bool hasBleed = bleed > 0f;
        bool hasCalm  = calm  > 0f;

        if (!hasBleed && !hasCalm)
        {
            _inkConflictLabel.Visible = false;
            return;
        }

        _inkConflictLabel.Visible = true;

        var parts = new System.Collections.Generic.List<string>();
        if (hasBleed)
        {
            int pct = Mathf.RoundToInt(bleed * 100f);
            parts.Add($"INK BLEED {pct}%");
        }
        if (hasCalm)
        {
            int pct = Mathf.RoundToInt(calm * 100f);
            parts.Add($"INK CALM {pct}%");
        }

        _inkConflictLabel.Text = string.Join("  |  ", parts);

        // Pulse colour: orange normally, red flash on trigger event.
        if (_inkConflictFlashTimer > 0f)
        {
            _inkConflictFlashTimer -= (float)GetProcessDeltaTime();
            _inkConflictLabel.Modulate = new Color(1f, 0.15f, 0.1f);
        }
        else
        {
            _inkConflictLabel.Modulate = new Color(1f, 0.6f, 0.1f);
        }
    }

    // ─── Mask of Ash Indicator ────────────────────────────────────

    /// <summary>
    /// Polls the player's MaskOfAshAbility each frame. Shows a duration bar
    /// and a "MASK BREAKING" warning when under 5 s remain.
    /// Falls back gracefully when no mask ability is present.
    /// </summary>
    private void UpdateMaskIndicator()
    {
        if (_maskLabel == null && _maskBar == null) return;

        // Re-use the already-cached player reference.
        var mask = _cachedPlayer?.GetNodeOrNull<MaskOfAshAbility>("MaskOfAshAbility");

        if (mask == null || !mask.IsMasked)
        {
            if (_maskLabel != null) _maskLabel.Visible = false;
            if (_maskBar   != null) _maskBar.Visible   = false;
            return;
        }

        float remaining = mask.MaskTimeRemaining;
        float duration  = mask.Duration;
        bool  breaking  = remaining < 5f;

        if (_maskLabel != null)
        {
            _maskLabel.Visible = true;
            _maskLabel.Text    = breaking ? "!! MASK BREAKING !!" : $"DISGUISE  {remaining:F0}s";
            _maskLabel.Modulate = breaking
                ? new Color(1f, 0.25f, 0.1f)   // urgent red
                : new Color(0.8f, 0.75f, 0.4f); // warm gold
        }

        if (_maskBar != null)
        {
            _maskBar.Visible  = true;
            _maskBar.MaxValue = duration;
            _maskBar.Value    = remaining;
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

    private Tween? _areaFadeTween;

    public void ShowAreaName(string areaName)
    {
        if (_areaLabel == null) return;
        _areaFadeTween?.Kill();
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
            _areaFadeTween?.Kill();
            _areaFadeTween = CreateTween();
            _areaFadeTween.TweenProperty(_areaLabel, "modulate:a", 0.0f, 1.0f);
        }
    }

    // ─── Alert ────────────────────────────────────────────────────

    public void ShowAlertLevel(string level)
    {
        if (_alertLabel != null)
            _alertLabel.Text = level;
    }

    private void ConnectAlertManager()
    {
        var am = Stealth.MissionAlertManager.Instance;
        if (am != null)
            am.AlertLevelChanged += OnAlertLevelChanged;
        else
            CallDeferred(MethodName.ConnectAlertManager); // Retry next frame if not ready.
    }

    /// <summary>
    /// Appends the live count of alerted/engaged guards to the alert label every frame.
    /// Keeps the label accurate even as guards lose/regain the player between events.
    /// </summary>
    private void UpdateAlertGuardCount()
    {
        if (_alertLabel == null) return;

        var am = Stealth.MissionAlertManager.Instance;
        if (am == null || am.AlertLevel == 0) return;

        int alerted = 0;
        foreach (var node in GetTree().GetNodesInGroup("Guards"))
        {
            if (node is Enemies.GuardEnemy g &&
                g.Sensor != null &&
                g.Sensor.CurrentAwareness >= Stealth.AwarenessLevel.Alerted)
            {
                alerted++;
            }
        }

        if (alerted > 0)
        {
            // Append count only — the text/color set by OnAlertLevelChanged is preserved.
            string levelText = am.AlertLevel switch
            {
                1 => "! Suspicious",
                2 => "!! ALERTED",
                3 => "!!! HUNTED",
                _ => "!!!! SIEGE"
            };
            _alertLabel.Text = $"{levelText}  [{alerted} guard{(alerted == 1 ? "" : "s")}]";
        }
    }

    private void OnAlertLevelChanged(int oldLevel, int newLevel)
    {
        if (_alertLabel == null) return;

        string text = newLevel switch
        {
            0 => "",
            1 => "! Suspicious",
            2 => "!! ALERTED",
            3 => "!!! HUNTED",
            _ => "!!!! SIEGE"
        };

        Color col = newLevel switch
        {
            0 => Colors.White,
            1 => new Color(1f, 0.9f, 0.3f),
            2 => new Color(1f, 0.55f, 0.1f),
            _ => new Color(1f, 0.15f, 0.1f)
        };

        _alertLabel.Text     = text;
        _alertLabel.Modulate = col;
        _alertLabel.Visible  = newLevel > 0;
    }

    // ─── Ink Totals ───────────────────────────────────────────────

    public void UpdateInkDisplay(int major, int lesser, int trace)
    {
        if (_inkLabel != null)
            _inkLabel.Text = $"Ink  M:{major}  L:{lesser}  T:{trace}";
    }

    // ─── Blood Echo Unlock Toast ──────────────────────────────────

    private void OnEchoUnlocked(string echoId)
    {
        var echo = Core.GameManager.Instance?.EchoManager?.GetEchoData(echoId);
        string name = echo?.DisplayName ?? echoId;
        ShowEchoToast($"MEMORY UNLOCKED\n\"{name}\"");
    }

    /// <summary>
    /// Display a short notification in the lower-center of the screen,
    /// fading in over 0.3 s, staying for 3.5 s, then fading out over 1 s.
    /// </summary>
    private void ShowEchoToast(string message)
    {
        if (_echoToastLabel == null) return;

        _echoToastLabel.Text    = message;
        _echoToastLabel.Modulate = new Color(0.75f, 0.55f, 1f, 0f); // soft purple, transparent

        _echoToastTween?.Kill();
        _echoToastTween = CreateTween();
        _echoToastTween.TweenProperty(_echoToastLabel, "modulate:a", 1f,  0.3f);
        _echoToastTween.TweenInterval(3.5f);
        _echoToastTween.TweenProperty(_echoToastLabel, "modulate:a", 0f,  1.0f);
    }
}
