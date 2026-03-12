using Godot;
using BloodInk.Content;
using BloodInk.Core;
using BloodInk.Interaction;
using BloodInk.Tools;
using BloodInk.VFX;
using System.Linq;

namespace BloodInk.World;

/// <summary>
/// Root script for the Ashwild Camp — the player's base between missions.
/// Manages which NPCs are available, camp state (act progression),
/// and transitions back to missions. Spawns the player and wires dialogue.
/// </summary>
public partial class CampScene : Node2D
{
    [Signal] public delegate void CampEnteredEventHandler(int actNumber);
    [Signal] public delegate void CampExitedEventHandler();

    /// <summary>Current act (1-6). Determines which NPCs have new dialogue.</summary>
    [Export] public int CurrentAct { get; set; } = 1;

    /// <summary>Whether the Needlewise's true identity has been revealed (Act 5+).</summary>
    public bool NeedlewiseIdentityRevealed { get; set; } = false;

    private Node2D? _player;

    public override void _Ready()
    {
        PlaceholderSprites.CreateAll();

        // Spawn player if not already present.
        _player = GetTree().GetFirstNodeInGroup("Player") as Node2D;
        if (_player == null)
        {
            SpawnPlayer();
        }

        // Wire NPC dialogues based on current game state.
        WireNpcDialogue();

        // Wire mission board.
        WireMissionBoard();

        // Wire NPC placeholder sprites.
        ApplyNpcSprites();

        // Instantiate HUD if not present.
        SpawnHUD();

        // Ensure shapes exist for interactable collisions.
        EnsureCollisionShapes();

        // Listen for dialogue events (crafting, tattoo application, etc.).
        WireDialogueEvents();

        EmitSignal(SignalName.CampEntered, CurrentAct);
        RefreshCampState();
    }

    private void SpawnPlayer()
    {
        var playerScene = GD.Load<PackedScene>("res://Scenes/Player/Player.tscn");
        if (playerScene == null) return;

        _player = playerScene.Instantiate<Node2D>();
        var spawnPoint = GetNodeOrNull<Node2D>("SpawnPoint");
        if (spawnPoint != null)
            _player.Position = spawnPoint.Position;
        else
            _player.Position = new Vector2(0, 80);
        AddChild(_player);

        // Set camera limits to the camp bounds (-640,-480 to 640,480).
        CallDeferred(nameof(ApplyCampCameraLimits));
    }

    private void ApplyCampCameraLimits()
    {
        if (_player == null) return;
        var cam = _player.GetNodeOrNull<VFX.CameraShake>("Camera2D");
        cam?.SetLimits(-640, -480, 640, 480);
    }

    private void SpawnHUD()
    {
        // Prevent duplicate HUDs on camp re-entry.
        if (GetNodeOrNull("GameHUD") != null) return;
        var hud = GD.Load<PackedScene>("res://Scenes/UI/GameHUD.tscn");
        if (hud != null)
            AddChild(hud.Instantiate());
    }

    // ─── NPC Dialogue Wiring ──────────────────────────────────────

    private void WireNpcDialogue()
    {
        var gm = GameManager.Instance;
        if (gm == null)
        {
            // Fallback — no game state, use Act 1 defaults.
            SetNpcDialogue("Needlewise/NeedlewiseInteract", CampDialogues.NeedlewiseAct1());
            SetNpcDialogue("Grael/GraelInteract", CampDialogues.GraelAct1());
            SetNpcDialogue("Rukh/RukhInteract", CampDialogues.RukhAct1());
            SetNpcDialogue("Senna/SennaInteract", CampDialogues.SennaAct1());
            SetNpcDialogue("Lorne/LorneInteract", CampDialogues.LorneAct1());
            return;
        }

        var kingdom = gm.Kingdoms[0];
        bool cowlDead = kingdom?.IsTargetKilled("cowl") ?? false;
        bool thorneDead = kingdom?.IsTargetKilled("thorne") ?? false;
        bool marenDead = kingdom?.IsTargetKilled("maren") ?? false;
        bool blessingDead = kingdom?.IsTargetKilled("blessing") ?? false;
        int totalKills = kingdom?.GetKilledTargets().Count() ?? 0;

        // ── Needlewise ──
        if (cowlDead)
            SetNpcDialogue("Needlewise/NeedlewiseInteract", CampDialogues.NeedlewisePostMission());
        else if (totalKills > 0)
            SetNpcDialogue("Needlewise/NeedlewiseInteract", CampDialogues.NeedlewiseProgress());
        else
            SetNpcDialogue("Needlewise/NeedlewiseInteract", CampDialogues.NeedlewiseAct1());

        // ── Rukh ──
        if (totalKills > 0 && !cowlDead)
            SetNpcDialogue("Rukh/RukhInteract", CampDialogues.RukhProgress(thorneDead, marenDead, blessingDead));
        else if (cowlDead)
            SetNpcDialogue("Rukh/RukhInteract", CampDialogues.RukhPostCowl());
        else
            SetNpcDialogue("Rukh/RukhInteract", CampDialogues.RukhAct1());

        // ── Senna ──
        if (totalKills > 0)
            SetNpcDialogue("Senna/SennaInteract", CampDialogues.SennaPostMission(totalKills, cowlDead));
        else
            SetNpcDialogue("Senna/SennaInteract", CampDialogues.SennaAct1());

        // ── Grael ──
        if (cowlDead)
            SetNpcDialogue("Grael/GraelInteract", CampDialogues.GraelPostCowl());
        else if (totalKills > 0)
            SetNpcDialogue("Grael/GraelInteract", CampDialogues.GraelProgress(totalKills));
        else
            SetNpcDialogue("Grael/GraelInteract", CampDialogues.GraelAct1());

        // ── Lorne ──
        if (cowlDead)
            SetNpcDialogue("Lorne/LorneInteract", CampDialogues.LornePostCowl());
        else if (totalKills > 0)
            SetNpcDialogue("Lorne/LorneInteract", CampDialogues.LorneProgress(totalKills));
        else
            SetNpcDialogue("Lorne/LorneInteract", CampDialogues.LorneAct1());
    }

    private void SetNpcDialogue(string path, Dialogue.DialogueData data)
    {
        var npc = GetNodeOrNull(path) as DialogueNPC;
        if (npc != null)
        {
            npc.SetDialogue(data);
        }
    }

    private void WireMissionBoard()
    {
        var oldBoard = GetNodeOrNull<Area2D>("MissionBoard");
        if (oldBoard == null) return;

        // Replace the generic Area2D with an actual MissionBoard instance.
        var pos = oldBoard.Position;
        var missionBoard = new MissionBoard { Name = "MissionBoard" };
        missionBoard.Position = pos;
        missionBoard.CollisionLayer = 1 << 5; // Interactable.
        missionBoard.CollisionMask = 1 << 1;  // Player.
        missionBoard.DisplayName = "Mission Board";

        // Move collision children from old node to new one.
        // Snapshot to avoid mutating during iteration.
        var children = new System.Collections.Generic.List<Node>(oldBoard.GetChildren());
        foreach (var child in children)
        {
            oldBoard.RemoveChild(child);
            missionBoard.AddChild(child);
        }

        RemoveChild(oldBoard);
        oldBoard.QueueFree();
        AddChild(missionBoard);
    }

    private void ApplyNpcSprites()
    {
        SetNpcSprite("Needlewise/NeedlewiseSprite", "npc_needlewise");
        SetNpcSprite("Grael/GraelSprite", "npc_grael");
        SetNpcSprite("Rukh/RukhSprite", "npc_rukh");
        SetNpcSprite("Senna/SennaSprite", "npc_senna");
        SetNpcSprite("Lorne/LorneSprite", "npc_lorne");
    }

    private void SetNpcSprite(string path, string textureName)
    {
        var sprite = GetNodeOrNull<AnimatedSprite2D>(path);
        if (sprite == null) return;

        var tex = PlaceholderSprites.Get(textureName);
        if (tex == null) return;

        var frames = new SpriteFrames();
        frames.AddAnimation("idle");
        frames.SetAnimationSpeed("idle", 1);
        frames.SetAnimationLoop("idle", true);
        frames.AddFrame("idle", tex);
        sprite.SpriteFrames = frames;
        sprite.Play("idle");
    }

    private void EnsureCollisionShapes()
    {
        // Ensure all NPC interaction areas and the MissionBoard/InkTent have collision shapes.
        EnsureShape("Needlewise/NeedlewiseInteract/NeedlewiseShape", new Vector2(24, 24));
        EnsureShape("Grael/GraelInteract/GraelShape", new Vector2(24, 24));
        EnsureShape("Rukh/RukhInteract/RukhShape", new Vector2(24, 24));
        EnsureShape("Senna/SennaInteract/SennaShape", new Vector2(24, 24));
        EnsureShape("Lorne/LorneInteract/LorneShape", new Vector2(24, 24));
        EnsureShape("MissionBoard/MissionBoardShape", new Vector2(24, 24));
        EnsureShape("InkTent/InkTentShape", new Vector2(32, 24));
    }

    private void EnsureShape(string path, Vector2 size)
    {
        var shapeNode = GetNodeOrNull<CollisionShape2D>(path);
        if (shapeNode != null && shapeNode.Shape == null)
        {
            shapeNode.Shape = new RectangleShape2D { Size = size };
        }
    }

    /// <summary>
    /// Update all camp NPCs with the correct state for the current act.
    /// Called on enter and after major events.
    /// </summary>
    public void RefreshCampState()
    {
        GD.Print($"Camp refreshed — Act {CurrentAct}");

        // The Needlewise is always physically present (she's the camp's tattoo shaman).
        // NeedlewiseIdentityRevealed tracks whether her SECRET identity is known (Act 5+).
        SetNpcVisible("Needlewise", true);
        SetNpcVisible("Grael", true);
        SetNpcVisible("Rukh", true);
        SetNpcVisible("Senna", true);
        SetNpcVisible("Lorne", true);

        // Log progress for player awareness.
        var gm = GameManager.Instance;
        if (gm != null)
        {
            var kingdom = gm.Kingdoms[0];
            int killed = kingdom?.GetKilledTargets().Count() ?? 0;
            int total = kingdom?.GetLivingTargets().Count() ?? 0;
            total += killed;
            GD.Print($"  Greenhold progress: {killed}/{total} targets eliminated");
            if (kingdom?.EdictbearerSlain == true)
                GD.Print("  Edictbearer SLAIN — the Edict weakens.");
        }
    }

    private void SetNpcVisible(string npcName, bool visible)
    {
        var npc = GetNodeOrNull<Node2D>(npcName);
        if (npc == null) return;
        npc.Visible = visible;
        // Also disable processing so invisible NPCs can't be interacted with.
        npc.ProcessMode = visible ? ProcessModeEnum.Inherit : ProcessModeEnum.Disabled;
    }

    /// <summary>Deploy to a mission scene.</summary>
    public void DeployToMission(string missionScenePath)
    {
        EmitSignal(SignalName.CampExited);
        GetTree().ChangeSceneToFile(missionScenePath);
    }

    // ─── Dialogue Event Handling ────────────────────────────────

    public override void _ExitTree()
    {
        var dm = Dialogue.DialogueManager.Instance;
        if (dm != null)
            dm.DialogueEventFired -= OnDialogueEvent;
    }

    private void WireDialogueEvents()
    {
        var dm = Dialogue.DialogueManager.Instance;
        if (dm == null) return;
        // Prevent stacking handlers on repeated camp loads — disconnect first.
        dm.DialogueEventFired -= OnDialogueEvent;
        dm.DialogueEventFired += OnDialogueEvent;
    }

    private void OnDialogueEvent(string eventKey)
    {
        switch (eventKey)
        {
            case "open_crafting":
                CallDeferred(nameof(OpenCraftingPanel));
                break;
        }
    }

    private void OpenCraftingPanel()
    {
        var gm = GameManager.Instance;
        if (gm?.Crafting == null)
        {
            GD.Print("CampScene: Crafting system not available.");
            return;
        }

        // Check if Ink Tent panel scene exists.
        // The InkTentPanel doubles as Lorne's crafting workspace.
        var panelScene = GD.Load<PackedScene>("res://Scenes/UI/InkTentPanel.tscn");
        if (panelScene == null)
        {
            GD.Print("CampScene: InkTentPanel.tscn not found — crafting UI not yet built.");
            return;
        }

        // Prevent duplicates.
        if (GetNodeOrNull("CraftingPanel") != null) return;

        var panel = panelScene.Instantiate<Control>();
        panel.Name = "CraftingPanel";
        AddChild(panel);
        GD.Print("CampScene: Crafting panel opened.");
    }
}
