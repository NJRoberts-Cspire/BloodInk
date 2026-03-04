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
    }

    private void OnNewGame()
    {
        GetTree().ChangeSceneToFile("res://Scenes/World/Camp.tscn");
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
        // TODO: Open settings panel (volume, controls, display).
        GD.Print("Settings not yet implemented.");
    }

    private void OnQuit()
    {
        GetTree().Quit();
    }
}
