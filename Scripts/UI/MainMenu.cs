using Godot;

namespace BloodInk.UI;

/// <summary>
/// Main menu: New Game, Continue, Settings, Quit.
/// Set as the main scene or loaded on game launch.
/// </summary>
public partial class MainMenu : Control
{
    private Button? _newGameBtn;
    private Button? _continueBtn;
    private Button? _settingsBtn;
    private Button? _quitBtn;
    private SettingsPanel? _settingsPanel;

    private PackedScene? _settingsScene;
    private PackedScene SettingsScene =>
        _settingsScene ??= GD.Load<PackedScene>("res://Scenes/UI/SettingsPanel.tscn");

    public override void _Ready()
    {
        _newGameBtn = GetNodeOrNull<Button>("VBox/NewGameButton");
        _continueBtn = GetNodeOrNull<Button>("VBox/ContinueButton");
        _settingsBtn = GetNodeOrNull<Button>("VBox/SettingsButton");
        _quitBtn = GetNodeOrNull<Button>("VBox/QuitButton");

        _newGameBtn?.Connect("pressed", Callable.From(OnNewGame));
        _continueBtn?.Connect("pressed", Callable.From(OnContinue));
        _settingsBtn?.Connect("pressed", Callable.From(OnSettings));
        _quitBtn?.Connect("pressed", Callable.From(OnQuit));

        // Disable continue if no save exists.
        if (_continueBtn != null)
            _continueBtn.Disabled = !Core.SaveSystem.SaveExists("slot1");

        // Pre-instantiate the settings panel (hidden by default).
        _settingsPanel = SettingsScene.Instantiate<SettingsPanel>();
        _settingsPanel.Closed += OnSettingsClosed;
        AddChild(_settingsPanel);
    }

    private void OnNewGame()
    {
        GetTree().ChangeSceneToFile("res://Scenes/World/TestWorld.tscn");
    }

    private void OnContinue()
    {
        Core.GameManager.Instance?.Load("slot1");
        // After loading, transition to the appropriate scene.
        // For now, go to camp.
        GetTree().ChangeSceneToFile("res://Scenes/World/Camp.tscn");
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

    private void OnQuit()
    {
        GetTree().Quit();
    }
}
