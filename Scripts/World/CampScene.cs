using Godot;
using BloodInk.Content;
using BloodInk.Core;
using BloodInk.Interaction;
using BloodInk.Tools;

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

    /// <summary>Whether the Needlewise has been revealed (Act 5+).</summary>
    public bool NeedlewiseRevealed { get; set; } = false;

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

        // Wire NPC dialogues.
        WireNpcDialogue();

        // Wire mission board.
        WireMissionBoard();

        // Wire NPC placeholder sprites.
        ApplyNpcSprites();

        // Instantiate HUD if not present.
        SpawnHUD();

        // Ensure shapes exist for interactable collisions.
        EnsureCollisionShapes();

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
    }

    private void SpawnHUD()
    {
        // Prevent duplicate HUDs on camp re-entry.
        if (GetNodeOrNull("GameHUD") != null) return;
        var hud = GD.Load<PackedScene>("res://Scenes/UI/GameHUD.tscn");
        if (hud != null)
            AddChild(hud.Instantiate());
    }

    private void WireNpcDialogue()
    {
        // Check if Cowl has been killed.
        bool cowlDead = GameManager.Instance?.Kingdoms[0]?.IsTargetKilled("cowl") ?? false;

        SetNpcDialogue("Needlewise/NeedlewiseInteract",
            cowlDead ? CampDialogues.NeedlewisePostMission() : CampDialogues.NeedlewiseAct1());
        SetNpcDialogue("Grael/GraelInteract", CampDialogues.GraelAct1());
        SetNpcDialogue("Rukh/RukhInteract", CampDialogues.RukhAct1());
        SetNpcDialogue("Senna/SennaInteract", CampDialogues.SennaAct1());
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
    /// Update all camp NPCs with the correct dialogue for the current act.
    /// Called on enter and after major events.
    /// </summary>
    public void RefreshCampState()
    {
        GD.Print($"Camp refreshed — Act {CurrentAct}");

        // Reveal Needlewise once any kingdom has been completed.
        if (!NeedlewiseRevealed && GameManager.Instance != null)
        {
            foreach (var kingdom in GameManager.Instance.Kingdoms)
            {
                if (kingdom != null && kingdom.IsCompleted)
                {
                    NeedlewiseRevealed = true;
                    break;
                }
            }
        }

        SetNpcVisible("Needlewise", NeedlewiseRevealed);
        SetNpcVisible("Grael", true);
        SetNpcVisible("Rukh", true);
        SetNpcVisible("Senna", true);
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
}
