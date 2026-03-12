using Godot;

namespace BloodInk.UI;

/// <summary>
/// Unified settings panel with tabbed sections:
///   Audio   – Master / Music / SFX / Ambient / UI volume sliders
///   Display – Window mode, resolution, VSync
///   Controls – Embedded KeybindSettings panel
/// All settings persist to user://settings.cfg.
/// </summary>
public partial class SettingsPanel : Control
{
    [Signal]
    public delegate void ClosedEventHandler();

    private const string ConfigPath = "user://settings.cfg";

    // ─── Audio widgets ──────────────────────────────────────────
    private HSlider? _masterSlider;
    private HSlider? _musicSlider;
    private HSlider? _sfxSlider;
    private HSlider? _ambientSlider;
    private HSlider? _uiSlider;

    // ─── Display widgets ────────────────────────────────────────
    private OptionButton? _windowModeOption;
    private OptionButton? _resolutionOption;
    private CheckButton? _vsyncToggle;

    // ─── Controls tab ────────────────────────────────────────────
    private KeybindSettings? _keybindSettings;
    // ─── Gameplay tab ────────────────────────────────────────────
    private HSlider? _screenShakeSlider;
    // ─── State ──────────────────────────────────────────────────
    private bool _loading;
    // ─── Tab buttons ─────────────────────────────────────────────
    private Button? _audioTabBtn;
    private Button? _displayTabBtn;
    private Button? _controlsTabBtn;
    private Button? _gameplayTabBtn;
    private Control? _audioTab;
    private Control? _displayTab;
    private Control? _controlsTab;
    private Control? _gameplayTab;

    private PackedScene? _keybindScene;
    private PackedScene KeybindScene =>
        _keybindScene ??= GD.Load<PackedScene>("res://Scenes/UI/KeybindSettings.tscn");

    // Common resolutions (render-size multipliers of the 640×360 base).
    private static readonly Vector2I[] Resolutions = new[]
    {
        new Vector2I(640, 360),
        new Vector2I(960, 540),
        new Vector2I(1280, 720),
        new Vector2I(1600, 900),
        new Vector2I(1920, 1080),
        new Vector2I(2560, 1440),
        new Vector2I(3840, 2160),
    };

    public override void _Ready()
    {
        BuildUi();
        LoadSettings();
        ShowTab("audio");
    }

    // ═════════════════════════════════════════════════════════════
    //  UI Construction
    // ═════════════════════════════════════════════════════════════

    private void BuildUi()
    {
        // Full-screen dark background
        var bg = new ColorRect
        {
            Color = new Color(0.05f, 0.02f, 0.08f, 0.97f),
            LayoutMode = 1,
            AnchorsPreset = (int)LayoutPreset.FullRect,
        };
        AddChild(bg);

        // Title
        var title = new Label
        {
            Text = "SETTINGS",
            HorizontalAlignment = HorizontalAlignment.Center,
            LayoutMode = 1,
            AnchorsPreset = (int)LayoutPreset.CenterTop,
            OffsetTop = 10,
            OffsetLeft = -120,
            OffsetRight = 120,
            OffsetBottom = 34,
        };
        AddChild(title);

        // ─── Tab bar ────────────────────────────────────────────
        var tabBar = new HBoxContainer
        {
            LayoutMode = 1,
            AnchorsPreset = (int)LayoutPreset.TopWide,
            OffsetTop = 38,
            OffsetBottom = 60,
            OffsetLeft = 40,
            OffsetRight = -40,
        };
        tabBar.AddThemeConstantOverride("separation", 6);
        AddChild(tabBar);

        _audioTabBtn = new Button { Text = "Audio", SizeFlagsHorizontal = SizeFlags.ExpandFill, ToggleMode = true };
        _displayTabBtn = new Button { Text = "Display", SizeFlagsHorizontal = SizeFlags.ExpandFill, ToggleMode = true };
        _gameplayTabBtn = new Button { Text = "Gameplay", SizeFlagsHorizontal = SizeFlags.ExpandFill, ToggleMode = true };
        _controlsTabBtn = new Button { Text = "Controls", SizeFlagsHorizontal = SizeFlags.ExpandFill, ToggleMode = true };
        _audioTabBtn.Pressed += () => ShowTab("audio");
        _displayTabBtn.Pressed += () => ShowTab("display");
        _gameplayTabBtn.Pressed += () => ShowTab("gameplay");
        _controlsTabBtn.Pressed += () => ShowTab("controls");
        tabBar.AddChild(_audioTabBtn);
        tabBar.AddChild(_displayTabBtn);
        tabBar.AddChild(_gameplayTabBtn);
        tabBar.AddChild(_controlsTabBtn);

        // ─── Content area ───────────────────────────────────────
        var contentArea = new Control
        {
            LayoutMode = 1,
            AnchorsPreset = (int)LayoutPreset.FullRect,
            OffsetTop = 66,
            OffsetBottom = -44,
            OffsetLeft = 40,
            OffsetRight = -40,
        };
        AddChild(contentArea);

        _audioTab = BuildAudioTab();
        _displayTab = BuildDisplayTab();
        _gameplayTab = BuildGameplayTab();
        _controlsTab = BuildControlsTab();
        contentArea.AddChild(_audioTab);
        contentArea.AddChild(_displayTab);
        contentArea.AddChild(_gameplayTab);
        contentArea.AddChild(_controlsTab);

        // ─── Bottom bar (Back) ──────────────────────────────────
        var bottomBar = new HBoxContainer
        {
            LayoutMode = 1,
            AnchorsPreset = (int)LayoutPreset.BottomWide,
            OffsetTop = -38,
            OffsetLeft = 40,
            OffsetRight = -40,
        };
        AddChild(bottomBar);

        var backBtn = new Button { Text = "Back", SizeFlagsHorizontal = SizeFlags.ExpandFill };
        backBtn.Pressed += OnBack;
        bottomBar.AddChild(backBtn);
    }

    // ─── Audio tab ──────────────────────────────────────────────

    private Control BuildAudioTab()
    {
        var scroll = new ScrollContainer
        {
            LayoutMode = 1,
            AnchorsPreset = (int)LayoutPreset.FullRect,
        };

        var vbox = new VBoxContainer();
        vbox.AddThemeConstantOverride("separation", 10);
        scroll.AddChild(vbox);

        var am = Audio.AudioManager.Instance;

        _masterSlider = AddVolumeRow(vbox, "Master Volume", am?.GetMasterVolume() ?? 1f);
        _musicSlider = AddVolumeRow(vbox, "Music Volume", am?.GetMusicVolume() ?? 0.7f);
        _sfxSlider = AddVolumeRow(vbox, "SFX Volume", am?.GetSFXVolume() ?? 0.8f);
        _ambientSlider = AddVolumeRow(vbox, "Ambient Volume", am?.GetAmbientVolume() ?? 0.6f);
        _uiSlider = AddVolumeRow(vbox, "UI Volume", 0.9f);

        _masterSlider.ValueChanged += v => { Audio.AudioManager.Instance?.SetMasterVolume((float)v); SaveSettings(); };
        _musicSlider.ValueChanged += v => { Audio.AudioManager.Instance?.SetMusicVolume((float)v); SaveSettings(); };
        _sfxSlider.ValueChanged += v => { Audio.AudioManager.Instance?.SetSFXVolume((float)v); SaveSettings(); };
        _ambientSlider.ValueChanged += v => { Audio.AudioManager.Instance?.SetAmbientVolume((float)v); SaveSettings(); };
        _uiSlider.ValueChanged += v => { Audio.AudioManager.Instance?.SetUIVolume((float)v); SaveSettings(); };

        return scroll;
    }

    private static HSlider AddVolumeRow(VBoxContainer parent, string label, float initial)
    {
        var row = new HBoxContainer();
        row.AddThemeConstantOverride("separation", 12);

        var lbl = new Label
        {
            Text = label,
            CustomMinimumSize = new Vector2(130, 0),
            SizeFlagsHorizontal = SizeFlags.ExpandFill,
        };
        row.AddChild(lbl);

        var slider = new HSlider
        {
            MinValue = 0,
            MaxValue = 1,
            Step = 0.01,
            Value = initial,
            CustomMinimumSize = new Vector2(160, 0),
            SizeFlagsHorizontal = SizeFlags.ExpandFill,
        };
        row.AddChild(slider);

        var pctLabel = new Label
        {
            Text = $"{(int)(initial * 100)}%",
            CustomMinimumSize = new Vector2(40, 0),
            HorizontalAlignment = HorizontalAlignment.Right,
        };
        slider.ValueChanged += v => pctLabel.Text = $"{(int)(v * 100)}%";
        row.AddChild(pctLabel);

        parent.AddChild(row);
        return slider;
    }

    // ─── Display tab ────────────────────────────────────────────

    private Control BuildDisplayTab()
    {
        var scroll = new ScrollContainer
        {
            LayoutMode = 1,
            AnchorsPreset = (int)LayoutPreset.FullRect,
        };

        var vbox = new VBoxContainer();
        vbox.AddThemeConstantOverride("separation", 10);
        scroll.AddChild(vbox);

        // Window mode
        {
            var row = new HBoxContainer();
            row.AddThemeConstantOverride("separation", 12);

            var lbl = new Label
            {
                Text = "Window Mode",
                CustomMinimumSize = new Vector2(130, 0),
                SizeFlagsHorizontal = SizeFlags.ExpandFill,
            };
            row.AddChild(lbl);

            _windowModeOption = new OptionButton
            {
                CustomMinimumSize = new Vector2(200, 0),
                SizeFlagsHorizontal = SizeFlags.ExpandFill,
            };
            _windowModeOption.AddItem("Windowed", 0);
            _windowModeOption.AddItem("Borderless Fullscreen", 1);
            _windowModeOption.AddItem("Exclusive Fullscreen", 2);

            // Set current
            var currentMode = DisplayServer.WindowGetMode();
            _windowModeOption.Selected = currentMode switch
            {
                DisplayServer.WindowMode.ExclusiveFullscreen => 2,
                DisplayServer.WindowMode.Fullscreen => 1,
                _ => 0,
            };
            _windowModeOption.ItemSelected += OnWindowModeChanged;
            row.AddChild(_windowModeOption);
            vbox.AddChild(row);
        }

        // Resolution
        {
            var row = new HBoxContainer();
            row.AddThemeConstantOverride("separation", 12);

            var lbl = new Label
            {
                Text = "Resolution",
                CustomMinimumSize = new Vector2(130, 0),
                SizeFlagsHorizontal = SizeFlags.ExpandFill,
            };
            row.AddChild(lbl);

            _resolutionOption = new OptionButton
            {
                CustomMinimumSize = new Vector2(200, 0),
                SizeFlagsHorizontal = SizeFlags.ExpandFill,
            };

            var currentSize = DisplayServer.WindowGetSize();
            int selectedIdx = 2; // default 1280x720
            for (int i = 0; i < Resolutions.Length; i++)
            {
                var r = Resolutions[i];
                _resolutionOption.AddItem($"{r.X} × {r.Y}", i);
                if (r == currentSize) selectedIdx = i;
            }
            _resolutionOption.Selected = selectedIdx;
            _resolutionOption.ItemSelected += OnResolutionChanged;
            row.AddChild(_resolutionOption);
            vbox.AddChild(row);
        }

        // VSync
        {
            var row = new HBoxContainer();
            row.AddThemeConstantOverride("separation", 12);

            var lbl = new Label
            {
                Text = "VSync",
                CustomMinimumSize = new Vector2(130, 0),
                SizeFlagsHorizontal = SizeFlags.ExpandFill,
            };
            row.AddChild(lbl);

            _vsyncToggle = new CheckButton
            {
                Text = "Enabled",
                ButtonPressed = DisplayServer.WindowGetVsyncMode() != DisplayServer.VSyncMode.Disabled,
                SizeFlagsHorizontal = SizeFlags.ExpandFill,
            };
            _vsyncToggle.Toggled += OnVSyncToggled;
            row.AddChild(_vsyncToggle);
            vbox.AddChild(row);
        }

        return scroll;
    }
    // ─── Gameplay tab ───────────────────────────────────────

    private Control BuildGameplayTab()
    {
        var scroll = new ScrollContainer
        {
            LayoutMode = 1,
            AnchorsPreset = (int)LayoutPreset.FullRect,
        };

        var vbox = new VBoxContainer();
        vbox.AddThemeConstantOverride("separation", 10);
        scroll.AddChild(vbox);

        // Screen Shake Intensity
        _screenShakeSlider = AddVolumeRow(vbox, "Screen Shake", VFX.CameraShake.IntensityMultiplier);
        _screenShakeSlider.ValueChanged += v =>
        {
            VFX.CameraShake.IntensityMultiplier = (float)v;
            SaveSettings();
        };

        return scroll;
    }
    // ─── Controls tab ───────────────────────────────────────────

    private Control BuildControlsTab()
    {
        // Embed the keybind panel directly as a child. Override its layout to fill the content area.
        _keybindSettings = KeybindScene.Instantiate<KeybindSettings>();
        _keybindSettings.LayoutMode = 1;
        _keybindSettings.AnchorsPreset = (int)LayoutPreset.FullRect;
        _keybindSettings.Visible = true;

        // When the keybind panel fires "Closed", just switch back to Audio tab.
        _keybindSettings.Closed += () => ShowTab("audio");

        return _keybindSettings;
    }

    // ═════════════════════════════════════════════════════════════
    //  Tab switching
    // ═════════════════════════════════════════════════════════════

    private void ShowTab(string tab)
    {
        if (_audioTab != null) _audioTab.Visible = tab == "audio";
        if (_displayTab != null) _displayTab.Visible = tab == "display";
        if (_gameplayTab != null) _gameplayTab.Visible = tab == "gameplay";
        if (_controlsTab != null) _controlsTab.Visible = tab == "controls";

        if (_audioTabBtn != null) _audioTabBtn.ButtonPressed = tab == "audio";
        if (_displayTabBtn != null) _displayTabBtn.ButtonPressed = tab == "display";
        if (_gameplayTabBtn != null) _gameplayTabBtn.ButtonPressed = tab == "gameplay";
        if (_controlsTabBtn != null) _controlsTabBtn.ButtonPressed = tab == "controls";
    }

    // ═════════════════════════════════════════════════════════════
    //  Callbacks
    // ═════════════════════════════════════════════════════════════

    private void OnWindowModeChanged(long index)
    {
        switch (index)
        {
            case 0: // Windowed
                DisplayServer.WindowSetMode(DisplayServer.WindowMode.Windowed);
                DisplayServer.WindowSetFlag(DisplayServer.WindowFlags.Borderless, false);
                // Restore resolution from dropdown.
                if (_resolutionOption != null)
                    OnResolutionChanged(_resolutionOption.Selected);
                break;
            case 1: // Borderless Fullscreen
                DisplayServer.WindowSetMode(DisplayServer.WindowMode.Fullscreen);
                break;
            case 2: // Exclusive Fullscreen
                DisplayServer.WindowSetMode(DisplayServer.WindowMode.ExclusiveFullscreen);
                break;
        }
        SaveSettings();
    }

    private void OnResolutionChanged(long index)
    {
        if (index < 0 || index >= Resolutions.Length) return;
        var res = Resolutions[index];
        DisplayServer.WindowSetSize(res);
        // Centre the window on the screen.
        var screenSize = DisplayServer.ScreenGetSize();
        var pos = (screenSize - res) / 2;
        DisplayServer.WindowSetPosition(pos);
        SaveSettings();
    }

    private void OnVSyncToggled(bool enabled)
    {
        DisplayServer.WindowSetVsyncMode(
            enabled ? DisplayServer.VSyncMode.Enabled : DisplayServer.VSyncMode.Disabled);
        SaveSettings();
    }

    private void OnBack()
    {
        SaveSettings();
        EmitSignal(SignalName.Closed);
    }

    // ═════════════════════════════════════════════════════════════
    //  Persistence
    // ═════════════════════════════════════════════════════════════

    private void SaveSettings()
    {
        if (_loading) return;

        var cfg = new ConfigFile();

        // Audio
        cfg.SetValue("audio", "master", _masterSlider?.Value ?? 1.0);
        cfg.SetValue("audio", "music", _musicSlider?.Value ?? 0.7);
        cfg.SetValue("audio", "sfx", _sfxSlider?.Value ?? 0.8);
        cfg.SetValue("audio", "ambient", _ambientSlider?.Value ?? 0.6);
        cfg.SetValue("audio", "ui", _uiSlider?.Value ?? 0.9);

        // Display
        cfg.SetValue("display", "window_mode", _windowModeOption?.Selected ?? 0);
        cfg.SetValue("display", "resolution", _resolutionOption?.Selected ?? 2);
        cfg.SetValue("display", "vsync", _vsyncToggle?.ButtonPressed ?? true);

        // Gameplay
        cfg.SetValue("gameplay", "screen_shake", _screenShakeSlider?.Value ?? 1.0);

        cfg.Save(ConfigPath);
    }

    private void LoadSettings()
    {
        _loading = true;
        var cfg = new ConfigFile();
        if (cfg.Load(ConfigPath) != Error.Ok) { _loading = false; return; }

        // Audio
        var am = Audio.AudioManager.Instance;
        if (cfg.HasSectionKey("audio", "master"))
        {
            float v = System.Convert.ToSingle(cfg.GetValue("audio", "master"));
            if (_masterSlider != null) _masterSlider.Value = v;
            am?.SetMasterVolume(v);
        }
        if (cfg.HasSectionKey("audio", "music"))
        {
            float v = System.Convert.ToSingle(cfg.GetValue("audio", "music"));
            if (_musicSlider != null) _musicSlider.Value = v;
            am?.SetMusicVolume(v);
        }
        if (cfg.HasSectionKey("audio", "sfx"))
        {
            float v = System.Convert.ToSingle(cfg.GetValue("audio", "sfx"));
            if (_sfxSlider != null) _sfxSlider.Value = v;
            am?.SetSFXVolume(v);
        }
        if (cfg.HasSectionKey("audio", "ambient"))
        {
            float v = System.Convert.ToSingle(cfg.GetValue("audio", "ambient"));
            if (_ambientSlider != null) _ambientSlider.Value = v;
            am?.SetAmbientVolume(v);
        }
        if (cfg.HasSectionKey("audio", "ui"))
        {
            float v = System.Convert.ToSingle(cfg.GetValue("audio", "ui"));
            if (_uiSlider != null) _uiSlider.Value = v;
            am?.SetUIVolume(v);
        }

        // Display
        if (cfg.HasSectionKey("display", "window_mode"))
        {
            int mode = System.Convert.ToInt32(cfg.GetValue("display", "window_mode"));
            if (_windowModeOption != null) _windowModeOption.Selected = mode;
            OnWindowModeChanged(mode);
        }
        if (cfg.HasSectionKey("display", "resolution"))
        {
            int idx = System.Convert.ToInt32(cfg.GetValue("display", "resolution"));
            if (_resolutionOption != null) _resolutionOption.Selected = idx;
            OnResolutionChanged(idx);
        }
        if (cfg.HasSectionKey("display", "vsync"))
        {
            bool on = System.Convert.ToBoolean(cfg.GetValue("display", "vsync"));
            if (_vsyncToggle != null) _vsyncToggle.ButtonPressed = on;
            OnVSyncToggled(on);
        }

        // Gameplay
        if (cfg.HasSectionKey("gameplay", "screen_shake"))
        {
            float v = System.Convert.ToSingle(cfg.GetValue("gameplay", "screen_shake"));
            if (_screenShakeSlider != null) _screenShakeSlider.Value = v;
            VFX.CameraShake.IntensityMultiplier = v;
        }

        _loading = false;
    }
}
