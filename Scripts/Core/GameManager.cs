using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using BloodInk.Tools;

namespace BloodInk.Core;

/// <summary>
/// Autoload singleton managing global game state: pausing, scene transitions,
/// and references to all persistent gameplay systems.
/// Add this as an Autoload in Project Settings → Autoload with name "GameManager".
/// </summary>
public partial class GameManager : Node
{
    public static GameManager? Instance { get; private set; }

    public bool IsPaused { get; private set; }

    // ─── System References ────────────────────────────────────────
    // These are created as child nodes so they persist across scene changes.

    public Ink.TattooSystem? TattooSystem { get; private set; }
    public Ink.InkInventory? InkInventory { get; private set; }
    public BloodEchoes.BloodEchoManager? EchoManager { get; private set; }
    public Campaigns.CampaignManager? CampaignManager { get; private set; }
    public Campaigns.Rukh.SpyNetworkManager? SpyNetwork { get; private set; }
    public Campaigns.Grael.WarbandManager? Warband { get; private set; }
    public Campaigns.Lorne.CraftingSystem? Crafting { get; private set; }
    public Campaigns.Lorne.TremorSystem? Tremor { get; private set; }
    public Campaigns.Lorne.CampManager? Camp { get; private set; }
    public Progression.PlayerChoices? Choices { get; private set; }
    public Progression.NewGamePlus? NewGamePlus { get; private set; }
    public SaveSystem? SaveSystem { get; private set; }

    /// <summary>Kingdom states indexed 0-5.</summary>
    public Progression.KingdomState[] Kingdoms { get; private set; } = new Progression.KingdomState[6];

    public override void _Ready()
    {
        Instance = this;
        ProcessMode = ProcessModeEnum.Always;
        InitializeSystems();
    }

    /// <summary>
    /// Create and wire all persistent gameplay systems as child nodes.
    /// </summary>
    private void InitializeSystems()
    {
        // Generate placeholder sprites first so all scenes can use them.
        PlaceholderSprites.CreateAll();

        // Core systems.
        InkInventory = new Ink.InkInventory();
        TattooSystem = new Ink.TattooSystem();
        EchoManager = new BloodEchoes.BloodEchoManager();
        CampaignManager = new Campaigns.CampaignManager();
        Choices = new Progression.PlayerChoices();
        NewGamePlus = new Progression.NewGamePlus();
        SaveSystem = new SaveSystem();

        AddChild(InkInventory);
        AddChild(TattooSystem);
        AddChild(EchoManager);
        AddChild(CampaignManager);
        AddChild(Choices);
        AddChild(NewGamePlus);
        AddChild(SaveSystem);

        // Rukh's spy network.
        SpyNetwork = new Campaigns.Rukh.SpyNetworkManager();
        AddChild(SpyNetwork);

        // Grael's warband.
        Warband = new Campaigns.Grael.WarbandManager();
        AddChild(Warband);

        // Lorne's crafting + tremor + camp.
        Tremor = new Campaigns.Lorne.TremorSystem();
        Crafting = new Campaigns.Lorne.CraftingSystem { Tremor = Tremor };
        Camp = new Campaigns.Lorne.CampManager { Tremor = Tremor, Crafting = Crafting };
        AddChild(Tremor);
        AddChild(Crafting);
        AddChild(Camp);

        // Noise propagation for stealth hearing.
        var noiseProp = new Stealth.NoisePropagator();
        AddChild(noiseProp);

        // Kingdom states.
        string[] kingdomNames = { "Ashenmarch", "Veilgard", "Thornwall", "Duskhollow", "Irontide", "The Pale" };
        for (int i = 0; i < 6; i++)
        {
            Kingdoms[i] = new Progression.KingdomState
            {
                KingdomIndex = i,
                KingdomName = kingdomNames[i]
            };
            AddChild(Kingdoms[i]);

            // Wire kingdom completion to campaign unlock checks.
            int idx = i;
            Kingdoms[i].KingdomCompleted += (int ki) =>
            {
                CampaignManager?.OnKingdomCompleted(ki);
            };
        }

        // Wire tattoo application to echo unlocks.
        TattooSystem.TattooApplied += (tattooId, slot) =>
        {
            // Check if this tattoo has an associated Blood Echo.
            var tattoo = Content.TattooRegistry.FindById(tattooId);
            if (tattoo != null && !string.IsNullOrEmpty(tattoo.BloodEchoId))
            {
                EchoManager?.UnlockEcho(tattoo.BloodEchoId);
            }
        };

        // Register all crafting recipes.
        foreach (var recipe in Content.CraftingRecipeRegistry.GetAll())
            Crafting?.RegisterRecipe(recipe);

        // Register all echo definitions.
        EchoManager?.RegisterEchoes(Content.EchoRegistry.GetAll());

        GD.Print("BloodInk: All gameplay systems initialized.");

        // Apply NG+ modifications if this is a subsequent cycle.
        if (NewGamePlus.IsNewGamePlus)
        {
            NewGamePlus.ApplyToNewGame(TattooSystem, Choices, Tremor);
        }
    }

    /// <summary>
    /// Toggle pause state. Called by PauseMenu — do NOT also handle "pause"
    /// input here to avoid double-toggling.
    /// </summary>
    public void TogglePause()
    {
        SetPaused(!IsPaused);
    }

    /// <summary>Explicitly set the paused state.</summary>
    public void SetPaused(bool paused)
    {
        IsPaused = paused;
        GetTree().Paused = IsPaused;
        GD.Print(IsPaused ? "Game Paused" : "Game Resumed");
    }

    public void ChangeScene(string scenePath)
    {
        GetTree().ChangeSceneToFile(scenePath);
    }

    // ─── Save / Load ──────────────────────────────────────────────

    /// <summary>
    /// Save all system state to a named slot.
    /// </summary>
    public void Save(string slotName = "slot1")
    {
        var data = new Dictionary<string, Dictionary<string, object>>
        {
            ["ink"] = InkInventory?.Serialize().ToDictionary(kv => kv.Key, kv => (object)kv.Value) ?? new(),
            ["tattoos"] = TattooSystem?.Serialize() ?? new(),
            ["echoes"] = EchoManager?.Serialize() ?? new(),
            ["campaigns"] = CampaignManager?.Serialize() ?? new(),
            ["spyNetwork"] = SpyNetwork?.Serialize() ?? new(),
            ["warband"] = Warband?.Serialize() ?? new(),
            ["crafting"] = Crafting?.Serialize() ?? new(),
            ["tremor"] = Tremor?.Serialize() ?? new(),
            ["camp"] = Camp?.Serialize() ?? new(),
            ["choices"] = Choices?.Serialize() ?? new(),
            ["ngPlus"] = NewGamePlus?.Serialize() ?? new()
        };

        // Save dialogue flags.
        var dialogueFlags = Dialogue.DialogueManager.Instance?.ExportFlags();
        if (dialogueFlags != null && dialogueFlags.Count > 0)
            data["dialogueFlags"] = dialogueFlags.ToDictionary(kv => kv.Key, kv => (object)kv.Value);

        // Save current scene path for proper scene restoration on load.
        data["meta"] = new Dictionary<string, object>
        {
            ["scene"] = GetTree().CurrentScene?.SceneFilePath ?? ""
        };

        // Add kingdom states.
        for (int i = 0; i < 6; i++)
            data[$"kingdom_{i}"] = Kingdoms[i]?.Serialize() ?? new();

        SaveSystem?.SaveGame(slotName, data);
    }

    /// <summary>
    /// Load all system state from a named slot.
    /// </summary>
    public void Load(string slotName = "slot1")
    {
        var data = SaveSystem?.LoadGame(slotName);
        if (data == null) return;

        if (data.TryGetValue("ink", out var inkData))
        {
            var intData = inkData.ToDictionary(kv => kv.Key, kv => Convert.ToInt32(kv.Value));
            InkInventory?.Deserialize(intData);
        }
        if (data.TryGetValue("tattoos", out var tatData)) TattooSystem?.Deserialize(tatData);
        if (data.TryGetValue("echoes", out var echoData)) EchoManager?.Deserialize(echoData);
        if (data.TryGetValue("campaigns", out var campData)) CampaignManager?.Deserialize(campData);
        if (data.TryGetValue("tremor", out var tremData)) Tremor?.Deserialize(tremData);
        if (data.TryGetValue("camp", out var campMgrData)) Camp?.Deserialize(campMgrData);
        if (data.TryGetValue("choices", out var choiceData)) Choices?.Deserialize(choiceData);
        if (data.TryGetValue("ngPlus", out var ngData)) NewGamePlus?.Deserialize(ngData);

        // Restore subsystems that were previously skipped.
        if (data.TryGetValue("warband", out var warbandData)) Warband?.Deserialize(warbandData);
        if (data.TryGetValue("spyNetwork", out var spyData)) SpyNetwork?.Deserialize(spyData);
        if (data.TryGetValue("crafting", out var craftData)) Crafting?.Deserialize(craftData);

        for (int i = 0; i < 6; i++)
            if (data.TryGetValue($"kingdom_{i}", out var kData)) Kingdoms[i]?.Deserialize(kData);

        // Restore dialogue flags.
        if (data.TryGetValue("dialogueFlags", out var dfData))
        {
            var stringFlags = dfData.ToDictionary(kv => kv.Key, kv => kv.Value?.ToString() ?? "");
            Dialogue.DialogueManager.Instance?.ImportFlags(stringFlags);
        }
    }

    // ─── NG+ Transition ───────────────────────────────────────────

    /// <summary>
    /// Trigger the Second Mark — snapshot current state and begin NG+.
    /// </summary>
    public void BeginNewGamePlus()
    {
        if (TattooSystem == null || Choices == null || NewGamePlus == null) return;

        // Collect killed Edictbearer IDs.
        var edictbearerKills = new List<string>();
        foreach (var kingdom in Kingdoms)
        {
            if (kingdom?.EdictbearerSlain == true)
            {
                foreach (var target in kingdom.GetKilledTargets())
                {
                    if (target.IsEdictbearer)
                        edictbearerKills.Add(target.Id);
                }
            }
        }

        NewGamePlus.SnapshotForNewGamePlus(Choices, TattooSystem, edictbearerKills);
        GD.Print("THE SECOND MARK — New Game+ initiated.");

        // Reset systems and reload with NG+ applied.
        // (Full reset + re-init would happen via scene reload.)
    }
}
