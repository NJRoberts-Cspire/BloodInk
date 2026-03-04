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
    private Button? _quitBtn;
    private SettingsPanel? _settingsPanel;

    private static readonly PackedScene SettingsScene =
        GD.Load<PackedScene>("res://Scenes/UI/SettingsPanel.tscn");

    public override void _Ready()
    {
        _resumeBtn = GetNodeOrNull<Button>("Panel/VBox/ResumeButton");
        _saveBtn = GetNodeOrNull<Button>("Panel/VBox/SaveButton");
        _loadBtn = GetNodeOrNull<Button>("Panel/VBox/LoadButton");
        _settingsBtn = GetNodeOrNull<Button>("Panel/VBox/SettingsButton");
        _quitBtn = GetNodeOrNull<Button>("Panel/VBox/QuitButton");

        _resumeBtn?.Connect("pressed", Callable.From(OnResume));
        _saveBtn?.Connect("pressed", Callable.From(OnSave));
        _loadBtn?.Connect("pressed", Callable.From(OnLoad));
        _settingsBtn?.Connect("pressed", Callable.From(OnSettings));
        _quitBtn?.Connect("pressed", Callable.From(OnQuitToMenu));

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
        GetTree().Paused = true;
    }

    private void OnResume()
    {
        Visible = false;
        GetTree().Paused = false;
    }

    private void OnSave()
    {
        Core.GameManager.Instance?.Save("slot1");
        GD.Print("Game saved.");
    }

    private void OnLoad()
    {
        Core.GameManager.Instance?.Load("slot1");
        OnResume();
        GD.Print("Game loaded.");
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

    private void OnQuitToMenu()
    {
        GetTree().Paused = false;
        GetTree().ChangeSceneToFile("res://Scenes/UI/MainMenu.tscn");
    }
}
