using Godot;
using System.Collections.Generic;

namespace BloodInk.UI;

/// <summary>
/// Keybind remapping panel. Lists all gameplay input actions and lets the player
/// click a button then press a new key to rebind it. Persists changes to a config file.
/// </summary>
public partial class KeybindSettings : Control
{
    /// <summary>Emitted when the Back button is pressed so the parent menu can hide this panel.</summary>
    [Signal]
    public delegate void ClosedEventHandler();

    // Actions the player is allowed to rebind, mapped to friendly display names.
    private static readonly Dictionary<string, string> RebindableActions = new()
    {
        { "move_up",    "Move Up" },
        { "move_down",  "Move Down" },
        { "move_left",  "Move Left" },
        { "move_right", "Move Right" },
        { "attack",     "Attack" },
        { "dodge",      "Dodge" },
        { "interact",   "Interact" },
        { "crouch",     "Crouch" },
        { "ability",    "Use Ability" },
        { "pause",      "Pause" },
    };

    private const string ConfigPath = "user://keybinds.cfg";

    // Maps action name → the button widget showing its current key.
    private readonly Dictionary<string, Button> _bindButtons = new();

    // Stores the original (project-default) events so we can reset.
    private readonly Dictionary<string, Godot.Collections.Array<InputEvent>> _defaults = new();

    private string? _listeningAction;  // Which action we're currently waiting for a key press on.
    private Button? _listeningButton;

    private ScrollContainer? _scroll;

    public override void _Ready()
    {
        // Snapshot defaults before any overrides are applied.
        foreach (var action in RebindableActions.Keys)
        {
            if (InputMap.HasAction(action))
            {
                _defaults[action] = InputMap.ActionGetEvents(action);
            }
        }

        // Load any saved overrides.
        LoadKeybinds();

        BuildUi();
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (_listeningAction == null) return;

        // We only rebind keyboard keys, ignore mouse movement etc.
        if (@event is not InputEventKey keyEvent) return;
        if (!keyEvent.Pressed) return;

        // Cancel on Escape without rebinding.
        if (keyEvent.PhysicalKeycode == Key.Escape)
        {
            CancelListening();
            GetViewport().SetInputAsHandled();
            return;
        }

        RebindAction(_listeningAction, keyEvent);
        GetViewport().SetInputAsHandled();
    }

    // ─── UI construction ────────────────────────────────────────────

    private void BuildUi()
    {
        // Background dimmer
        var bg = new ColorRect
        {
            Color = new Color(0.05f, 0.02f, 0.08f, 0.95f),
            LayoutMode = 1,
            AnchorsPreset = (int)LayoutPreset.FullRect,
        };
        AddChild(bg);

        // Title
        var title = new Label
        {
            Text = "KEYBINDS",
            HorizontalAlignment = HorizontalAlignment.Center,
            LayoutMode = 1,
            AnchorsPreset = (int)LayoutPreset.CenterTop,
            OffsetTop = 16,
            OffsetLeft = -120,
            OffsetRight = 120,
            OffsetBottom = 40,
        };
        AddChild(title);

        // Scrollable container for the action rows
        _scroll = new ScrollContainer
        {
            LayoutMode = 1,
            AnchorsPreset = (int)LayoutPreset.FullRect,
            OffsetTop = 48,
            OffsetBottom = -48,
            OffsetLeft = 40,
            OffsetRight = -40,
        };
        AddChild(_scroll);

        var vbox = new VBoxContainer();
        vbox.AddThemeConstantOverride("separation", 6);
        _scroll.AddChild(vbox);

        foreach (var (action, displayName) in RebindableActions)
        {
            var row = new HBoxContainer();
            row.AddThemeConstantOverride("separation", 12);

            var label = new Label
            {
                Text = displayName,
                CustomMinimumSize = new Vector2(120, 0),
                SizeFlagsHorizontal = SizeFlags.ExpandFill,
            };
            row.AddChild(label);

            var btn = new Button
            {
                Text = GetKeyLabelForAction(action),
                CustomMinimumSize = new Vector2(120, 0),
                SizeFlagsHorizontal = SizeFlags.ExpandFill,
            };
            var capturedAction = action;
            btn.Pressed += () => StartListening(capturedAction, btn);
            row.AddChild(btn);

            vbox.AddChild(row);
            _bindButtons[action] = btn;
        }

        // Bottom bar: Reset Defaults + Back
        var bottomBar = new HBoxContainer
        {
            LayoutMode = 1,
            AnchorsPreset = (int)LayoutPreset.BottomWide,
            OffsetTop = -40,
            OffsetLeft = 40,
            OffsetRight = -40,
        };
        bottomBar.AddThemeConstantOverride("separation", 12);
        AddChild(bottomBar);

        var resetBtn = new Button { Text = "Reset Defaults", SizeFlagsHorizontal = SizeFlags.ExpandFill };
        resetBtn.Pressed += OnResetDefaults;
        bottomBar.AddChild(resetBtn);

        var backBtn = new Button { Text = "Back", SizeFlagsHorizontal = SizeFlags.ExpandFill };
        backBtn.Pressed += OnBack;
        bottomBar.AddChild(backBtn);
    }

    // ─── Listening / Rebinding ──────────────────────────────────────

    private void StartListening(string action, Button btn)
    {
        // If already listening on another button, reset its text first.
        CancelListening();

        _listeningAction = action;
        _listeningButton = btn;
        btn.Text = "< Press a key >";
    }

    private void CancelListening()
    {
        if (_listeningAction != null && _listeningButton != null)
        {
            _listeningButton.Text = GetKeyLabelForAction(_listeningAction);
        }
        _listeningAction = null;
        _listeningButton = null;
    }

    private void RebindAction(string action, InputEventKey newKey)
    {
        if (!InputMap.HasAction(action)) return;

        // Remove existing keyboard events but keep non-keyboard events (mouse, joypad).
        var existing = InputMap.ActionGetEvents(action);
        foreach (var ev in existing)
        {
            if (ev is InputEventKey)
                InputMap.ActionEraseEvent(action, ev);
        }

        // Create a clean key event using the physical keycode from the pressed key.
        var bindEvent = new InputEventKey
        {
            PhysicalKeycode = newKey.PhysicalKeycode,
            Keycode = newKey.Keycode,
            Unicode = newKey.Unicode,
        };
        InputMap.ActionAddEvent(action, bindEvent);

        // Update button text.
        if (_listeningButton != null)
            _listeningButton.Text = GetKeyLabelForAction(action);

        _listeningAction = null;
        _listeningButton = null;

        SaveKeybinds();
    }

    // ─── Reset ──────────────────────────────────────────────────────

    private void OnResetDefaults()
    {
        CancelListening();

        foreach (var (action, defaults) in _defaults)
        {
            if (!InputMap.HasAction(action)) continue;

            // Clear all current events.
            var current = InputMap.ActionGetEvents(action);
            foreach (var ev in current)
                InputMap.ActionEraseEvent(action, ev);

            // Re-add the original defaults.
            foreach (var ev in defaults)
                InputMap.ActionAddEvent(action, ev);
        }

        RefreshAllButtons();
        SaveKeybinds();
    }

    private void OnBack()
    {
        CancelListening();
        EmitSignal(SignalName.Closed);
    }

    // ─── Helpers ────────────────────────────────────────────────────

    private void RefreshAllButtons()
    {
        foreach (var (action, btn) in _bindButtons)
        {
            btn.Text = GetKeyLabelForAction(action);
        }
    }

    /// <summary>Returns a human-readable label for the first keyboard event bound to an action.</summary>
    private static string GetKeyLabelForAction(string action)
    {
        if (!InputMap.HasAction(action)) return "???";

        foreach (var ev in InputMap.ActionGetEvents(action))
        {
            if (ev is InputEventKey key)
            {
                // Prefer the physical keycode, fall back to logical.
                var code = key.PhysicalKeycode != Key.None ? key.PhysicalKeycode : key.Keycode;
                return OS.GetKeycodeString(code);
            }
        }
        return "Unbound";
    }

    // ─── Persistence ────────────────────────────────────────────────

    private void SaveKeybinds()
    {
        var config = new ConfigFile();

        foreach (var action in RebindableActions.Keys)
        {
            if (!InputMap.HasAction(action)) continue;

            foreach (var ev in InputMap.ActionGetEvents(action))
            {
                if (ev is InputEventKey key)
                {
                    config.SetValue("keybinds", action, (long)key.PhysicalKeycode);
                    break; // Save only the first keyboard binding.
                }
            }
        }

        config.Save(ConfigPath);
    }

    private void LoadKeybinds()
    {
        var config = new ConfigFile();
        if (config.Load(ConfigPath) != Error.Ok) return;

        foreach (var action in RebindableActions.Keys)
        {
            if (!config.HasSectionKey("keybinds", action)) continue;
            if (!InputMap.HasAction(action)) continue;

            var code = (Key)System.Convert.ToInt64(config.GetValue("keybinds", action));

            // Remove existing keyboard events.
            var existing = InputMap.ActionGetEvents(action);
            foreach (var ev in existing)
            {
                if (ev is InputEventKey)
                    InputMap.ActionEraseEvent(action, ev);
            }

            var bindEvent = new InputEventKey { PhysicalKeycode = code };
            InputMap.ActionAddEvent(action, bindEvent);
        }
    }
}
