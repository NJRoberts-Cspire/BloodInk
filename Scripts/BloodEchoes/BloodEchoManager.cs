using Godot;
using System.Collections.Generic;
using System.Linq;

namespace BloodInk.BloodEchoes;

/// <summary>
/// Manages Blood Echo availability, playback state, and completion tracking.
/// Autoload or attach to a persistent node.
/// </summary>
public partial class BloodEchoManager : Node
{
    [Signal] public delegate void EchoUnlockedEventHandler(string echoId);
    [Signal] public delegate void EchoCompletedEventHandler(string echoId);
    [Signal] public delegate void EchoStartedEventHandler(string echoId);

    /// <summary>All registered echo definitions, keyed by ID.</summary>
    private readonly Dictionary<string, EchoData> _allEchoes = new();

    /// <summary>Set of echo IDs the player has unlocked (via Major tattoos).</summary>
    private readonly HashSet<string> _unlockedEchoes = new();

    /// <summary>Set of echo IDs the player has completed.</summary>
    private readonly HashSet<string> _completedEchoes = new();

    /// <summary>Currently playing echo ID, or null.</summary>
    public string? ActiveEchoId { get; private set; }

    /// <summary>Scene path to return to after an echo ends.</summary>
    private string _returnScenePath = "";

    // ─── Registration ─────────────────────────────────────────────

    /// <summary>Register an echo definition. Call during game init or from a data loader.</summary>
    public void RegisterEcho(EchoData echo)
    {
        _allEchoes[echo.Id] = echo;
    }

    /// <summary>Register multiple echoes at once.</summary>
    public void RegisterEchoes(IEnumerable<EchoData> echoes)
    {
        foreach (var echo in echoes)
            RegisterEcho(echo);
    }

    // ─── Unlock & Query ───────────────────────────────────────────

    /// <summary>Unlock an echo by ID (typically called when a Major tattoo is applied).</summary>
    public void UnlockEcho(string echoId)
    {
        if (!_allEchoes.ContainsKey(echoId))
        {
            GD.PrintErr($"BloodEchoManager: Unknown echo ID '{echoId}'.");
            return;
        }

        if (_unlockedEchoes.Add(echoId))
        {
            EmitSignal(SignalName.EchoUnlocked, echoId);
            GD.Print($"Blood Echo unlocked: {_allEchoes[echoId].DisplayName}");
        }
    }

    /// <summary>Check if an echo is available to play.</summary>
    public bool IsEchoUnlocked(string echoId) => _unlockedEchoes.Contains(echoId);

    /// <summary>Check if an echo has been completed.</summary>
    public bool IsEchoCompleted(string echoId) => _completedEchoes.Contains(echoId);

    /// <summary>Get all unlocked echoes.</summary>
    public IEnumerable<EchoData> GetUnlockedEchoes() =>
        _unlockedEchoes.Where(id => _allEchoes.ContainsKey(id)).Select(id => _allEchoes[id]);

    /// <summary>Get all unlocked but not yet completed echoes.</summary>
    public IEnumerable<EchoData> GetAvailableEchoes() =>
        GetUnlockedEchoes().Where(e => !_completedEchoes.Contains(e.Id));

    /// <summary>Get echo data by ID.</summary>
    public EchoData? GetEchoData(string echoId) =>
        _allEchoes.TryGetValue(echoId, out var data) ? data : null;

    // ─── Playback ─────────────────────────────────────────────────

    /// <summary>
    /// Start playing an echo. Transitions to the echo's scene.
    /// Saves the current scene path so we can return after.
    /// </summary>
    public void StartEcho(string echoId)
    {
        if (!_unlockedEchoes.Contains(echoId))
        {
            GD.PrintErr($"Echo '{echoId}' is not unlocked.");
            return;
        }

        var echo = _allEchoes[echoId];
        if (string.IsNullOrEmpty(echo.ScenePath))
        {
            GD.PrintErr($"Echo '{echoId}' has no scene path set.");
            return;
        }

        ActiveEchoId = echoId;
        _returnScenePath = GetTree().CurrentScene?.SceneFilePath ?? "";

        EmitSignal(SignalName.EchoStarted, echoId);
        GD.Print($"Starting Blood Echo: {echo.DisplayName} ({echo.Genre})");
        GD.Print($"Whisper: \"{echo.WhisperText}\"");

        GetTree().ChangeSceneToFile(echo.ScenePath);
    }

    /// <summary>
    /// Called when an echo scene completes. Marks it done and returns to the camp.
    /// </summary>
    public void CompleteEcho()
    {
        if (ActiveEchoId == null) return;

        _completedEchoes.Add(ActiveEchoId);
        var echo = _allEchoes[ActiveEchoId];

        EmitSignal(SignalName.EchoCompleted, ActiveEchoId);
        GD.Print($"Blood Echo completed: {echo.DisplayName}");

        if (echo.IntelUnlocked.Length > 0)
            GD.Print($"Intel unlocked: {string.Join(", ", echo.IntelUnlocked)}");

        ActiveEchoId = null;

        // Return to camp / previous scene.
        if (!string.IsNullOrEmpty(_returnScenePath))
            GetTree().ChangeSceneToFile(_returnScenePath);
    }

    /// <summary>Abort an echo without completing it.</summary>
    public void AbortEcho()
    {
        if (ActiveEchoId == null) return;

        GD.Print($"Blood Echo aborted: {_allEchoes[ActiveEchoId].DisplayName}");
        ActiveEchoId = null;

        if (!string.IsNullOrEmpty(_returnScenePath))
            GetTree().ChangeSceneToFile(_returnScenePath);
    }

    // ─── Serialization ────────────────────────────────────────────

    public Dictionary<string, object> Serialize()
    {
        return new Dictionary<string, object>
        {
            ["unlocked"] = _unlockedEchoes.ToList(),
            ["completed"] = _completedEchoes.ToList()
        };
    }

    public void Deserialize(Dictionary<string, object> data)
    {
        _unlockedEchoes.Clear();
        _completedEchoes.Clear();

        if (data.TryGetValue("unlocked", out var unlocked) && unlocked is List<object> uList)
            foreach (var id in uList) { if (id is string s) _unlockedEchoes.Add(s); }
        if (data.TryGetValue("completed", out var completed) && completed is List<object> cList)
            foreach (var id in cList) { if (id is string s) _completedEchoes.Add(s); }
    }
}
