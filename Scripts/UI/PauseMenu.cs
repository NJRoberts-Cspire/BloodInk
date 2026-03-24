using Godot;

namespace BloodInk.UI;

/// <summary>
/// In-game pause menu overlay.
/// Resume, Save, Load, Settings, Quit to Menu.
/// </summary>
public partial class PauseMenu : Control
{
    private Button? _resumeBtn;
    private Button? _saveBtn;
    private Button? _loadBtn;
    private Button? _settingsBtn;
    private Button? _resetPuzzleBtn;
    private Button? _quitBtn;
    private SettingsPanel? _settingsPanel;

    private PackedScene? _settingsScene;
    private PackedScene SettingsScene =>
        _settingsScene ??= GD.Load<PackedScene>("res://Scenes/UI/SettingsPanel.tscn");

    public override void _Ready()
    {
        _resumeBtn      = GetNodeOrNull<Button>("Panel/VBox/ResumeButton");
        _saveBtn        = GetNodeOrNull<Button>("Panel/VBox/SaveButton");
        _loadBtn        = GetNodeOrNull<Button>("Panel/VBox/LoadButton");
        _settingsBtn    = GetNodeOrNull<Button>("Panel/VBox/SettingsButton");
        _resetPuzzleBtn = GetNodeOrNull<Button>("Panel/VBox/ResetPuzzleButton");
        _quitBtn        = GetNodeOrNull<Button>("Panel/VBox/QuitButton");

        _resumeBtn?.Connect("pressed",      Callable.From(OnResume));
        _saveBtn?.Connect("pressed",        Callable.From(OnSave));
        _loadBtn?.Connect("pressed",        Callable.From(OnLoad));
        _settingsBtn?.Connect("pressed",    Callable.From(OnSettings));
        _resetPuzzleBtn?.Connect("pressed", Callable.From(OnResetPuzzle));
        _quitBtn?.Connect("pressed",        Callable.From(OnQuitToMenu));

        Visible = false;
        ProcessMode = ProcessModeEnum.Always;

        // Pre-instantiate the settings panel (hidden by default).
        _settingsPanel = SettingsScene.Instantiate<SettingsPanel>();
        _settingsPanel.Closed += OnSettingsClosed;
        AddChild(_settingsPanel);
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event.IsActionPressed("pause"))
        {
            if (Visible)
                OnResume();
            else
                ShowMenu();
            GetViewport().SetInputAsHandled();
        }
    }

    public void ShowMenu()
    {
        Visible = true;
        Core.GameManager.Instance?.SetPaused(true);
    }

    private void OnResume()
    {
        Visible = false;
        Core.GameManager.Instance?.SetPaused(false);
    }

    private void OnSave()
    {
        Core.GameManager.Instance?.Save("slot1");
        GD.Print("Game saved.");
    }

    private void OnLoad()
    {
        var scenePath = Core.GameManager.Instance?.Load("slot1");
        GD.Print("Game loaded.");
        Core.GameManager.Instance?.SetPaused(false);
        GetTree().Paused = false;
        // Navigate to the scene that was active when the player saved.
        if (!string.IsNullOrEmpty(scenePath))
            GetTree().ChangeSceneToFile(scenePath);
        else
            GetTree().ReloadCurrentScene();
    }

    private void OnSettings()
    {
        if (_settingsPanel == null) return;
        _settingsPanel.Visible = true;
    }

    private void OnSettingsClosed()
    {
        if (_settingsPanel != null)
            _settingsPanel.Visible = false;
    }

    private void OnResetPuzzle()
    {
        // Find every PushBlock in the current scene and return it to its spawn position.
        int count = 0;
        foreach (var node in GetTree().GetNodesInGroup("PushBlocks"))
        {
            if (node is Interaction.PushBlock block)
            {
                block.Reset();
                count++;
            }
        }

        // Also scan the scene tree directly in case blocks weren't added to the group.
        if (count == 0)
        {
            foreach (var node in GetTree().CurrentScene?.FindChildren("*", "PushBlock", true, false)
                                 ?? new Godot.Collections.Array<Node>())
            {
                if (node is Interaction.PushBlock block)
                    block.Reset();
            }
        }

        GD.Print($"[PauseMenu] Puzzle reset — {count} block(s) returned to start.");
        OnResume();
    }

    private void OnQuitToMenu()
    {
        Core.GameManager.Instance?.SetPaused(false);
        GetTree().Paused = false;
        GetTree().ChangeSceneToFile("res://Scenes/UI/MainMenu.tscn");
    }
}
