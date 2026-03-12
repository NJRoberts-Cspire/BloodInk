using Godot;
using BloodInk.Combat;
using BloodInk.Core;
using BloodInk.Enemies;
using BloodInk.Enemies.States;
using BloodInk.Interaction;
using BloodInk.Stealth;
using BloodInk.Tools;
using BloodInk.UI;
using BloodInk.World;

namespace BloodInk.Missions;

/// <summary>
/// Base class for procedurally-built mission levels.
/// Provides shared helper factories for guards, shadow zones, hiding spots,
/// area zones, player spawning, and HUD setup — eliminating ~200 lines of
/// duplication across each concrete level.
/// </summary>
public abstract partial class MissionLevelBase : Node2D
{
    // ─── Player Spawn ─────────────────────────────────────────────

    /// <summary>Instantiate the player scene at <paramref name="spawnPos"/>.</summary>
    protected void SpawnPlayer(Vector2 spawnPos)
    {
        var playerScene = GD.Load<PackedScene>("res://Scenes/Player/Player.tscn");
        if (playerScene == null)
        {
            GD.PrintErr("Cannot load Player.tscn!");
            return;
        }

        var player = playerScene.Instantiate<CharacterBody2D>();
        player.Position = spawnPos;

        // Ensure player has an inventory for keys and items.
        if (player.GetNodeOrNull<PlayerInventory>("PlayerInventory") == null)
        {
            player.AddChild(new PlayerInventory { Name = "PlayerInventory" });
        }

        AddChild(player);

        CallDeferred(MethodName.ApplyPlayerSprite, player);
    }

    // ─── Camera Limits ────────────────────────────────────────────

    /// <summary>
    /// Configure the player camera so it can't scroll past the map edges.
    /// Call after SpawnPlayer(). Pass the world-space bounding box that
    /// encompasses all zones of the level.
    /// </summary>
    protected void SetCameraLimits(int left, int top, int right, int bottom)
    {
        // The camera is a child of the player node which is a direct child of this level.
        CallDeferred(MethodName.ApplyCameraLimits, left, top, right, bottom);
    }

    private void ApplyCameraLimits(int left, int top, int right, int bottom)
    {
        foreach (var child in GetChildren())
        {
            if (child is CharacterBody2D player && player.IsInGroup("Player"))
            {
                var cam = player.GetNodeOrNull<VFX.CameraShake>("Camera2D");
                if (cam != null)
                {
                    cam.SetLimits(left, top, right, bottom);
                    return;
                }
            }
        }
    }

    /// <summary>Apply placeholder sprite frames to the player.</summary>
    protected void ApplyPlayerSprite(CharacterBody2D player)
    {
        var sprite = player.GetNodeOrNull<AnimatedSprite2D>("AnimatedSprite2D");
        if (sprite == null) return;

        var pFrames = PlaceholderSprites.GetFrames("player_frames");
        if (pFrames != null)
        {
            sprite.SpriteFrames = pFrames;
            sprite.Play("idle");
            return;
        }

        // Fallback: build from single texture.
        var tex = PlaceholderSprites.Get("player");
        if (tex == null) return;

        var frames = new SpriteFrames();
        var crouchTex = PlaceholderSprites.Get("player_crouch") ?? tex;

        foreach (var (anim, speed, loop, useCrouch) in new[]
        {
            ("idle",         1,  true,  false),
            ("run",          8,  true,  false),
            ("attack",      12,  false, false),
            ("dodge",       10,  false, false),
            ("death",        1,  false, false),
            ("crouch_idle",  1,  true,  true),
            ("crouch_walk",  8,  true,  true),
            ("stealth_kill",12,  false, true),
        })
        {
            frames.AddAnimation(anim);
            frames.SetAnimationSpeed(anim, speed);
            frames.SetAnimationLoop(anim, loop);
            frames.AddFrame(anim, useCrouch ? crouchTex : tex);
        }

        sprite.SpriteFrames = frames;
        sprite.Play("idle");
    }

    // ─── HUD Setup ────────────────────────────────────────────────

    /// <summary>Instantiate GameHUD and wire health display to the player.</summary>
    protected void SetupHUD()
    {
        var hudScene = GD.Load<PackedScene>("res://Scenes/UI/GameHUD.tscn");
        if (hudScene == null) return;

        var hudInstance = hudScene.Instantiate();
        AddChild(hudInstance);

        var player = GetTree().GetFirstNodeInGroup("Player") as CharacterBody2D;
        if (player != null && hudInstance is GameHUD gameHud)
        {
            var health = player.GetNodeOrNull<HealthComponent>("HealthComponent");
            if (health != null)
            {
                health.HealthChanged += gameHud.OnHealthChanged;
                gameHud.OnHealthChanged(health.CurrentHealth, health.MaxHealth);
            }
        }
    }

    // ─── Guard Factory ────────────────────────────────────────────

    /// <summary>
    /// Spawn a guard enemy with full collision, hitbox, hurtbox, detection, patrol, and state machine.
    /// </summary>
    protected virtual void AddGuard(Node2D parent, string name, Vector2 pos, Vector2[] waypoints, bool elite = false)
    {
        var guard = new GuardEnemy { Name = name };
        guard.Position = pos;
        guard.CollisionLayer = 1 << 2;  // Enemy layer (layer 3).
        guard.CollisionMask = 1;          // World.
        guard.PatrolSpeed = elite ? 50f : 40f;
        guard.AlertedSpeed = elite ? 80f : 70f;
        guard.ChaseSpeed = elite ? 100f : 90f;
        guard.DetectRange = elite ? 140f : 100f;
        guard.AttackRange = 25f;

        // Sprite placeholder.
        var sprite = new AnimatedSprite2D { Name = "AnimatedSprite2D" };
        sprite.SpriteFrames = CreateGuardSpriteFrames(elite ? "guard_alert" : "guard");
        guard.AddChild(sprite);

        // Body collision.
        var bodyShape = new CollisionShape2D();
        bodyShape.Shape = new RectangleShape2D { Size = new Vector2(10, 14) };
        guard.AddChild(bodyShape);

        // Hurtbox.
        var hurtbox = new Hurtbox { Name = "Hurtbox" };
        hurtbox.CollisionLayer = 0;
        hurtbox.CollisionMask = 1 << 3; // PlayerHitbox.
        var hurtShape = new CollisionShape2D { Name = "HurtboxShape" };
        hurtShape.Shape = new RectangleShape2D { Size = new Vector2(12, 14) };
        hurtbox.AddChild(hurtShape);
        guard.AddChild(hurtbox);

        // Hitbox.
        var hitbox = new Hitbox { Name = "Hitbox" };
        hitbox.CollisionLayer = 1 << 4; // EnemyHitbox.
        hitbox.CollisionMask = 0;
        hitbox.Damage = elite ? 2 : 1;
        hitbox.KnockbackForce = new Vector2(80f, 0f);
        var hitShape = new CollisionShape2D { Name = "HitboxShape" };
        hitShape.Shape = new RectangleShape2D { Size = new Vector2(16, 12) };
        hitbox.AddChild(hitShape);
        guard.AddChild(hitbox);

        // Health.
        var health = new HealthComponent { Name = "HealthComponent" };
        health.MaxHealth = elite ? 5 : 3;
        guard.AddChild(health);

        // Detection sensor.
        var sensor = new DetectionSensor { Name = "DetectionSensor" };
        sensor.ViewDistance = elite ? 140f : 120f;
        sensor.ViewAngle = 55f;
        guard.AddChild(sensor);

        // Patrol route.
        var patrol = new PatrolRoute { Name = "PatrolRoute" };
        patrol.Waypoints = waypoints;
        guard.AddChild(patrol);

        // State machine with 7 guard AI states.
        var stateMachine = new StateMachine { Name = "StateMachine" };
        guard.AddChild(stateMachine);
        AddGuardStates(stateMachine);

        // Set Owner on all descendants BEFORE AddChild so GetOwner<GuardEnemy>()
        // works in Init() when _Ready fires.
        SetOwnerRecursive(guard, guard);
        parent.AddChild(guard);
    }

    /// <summary>Create all 7 guard AI states and add to the state machine.</summary>
    protected static void AddGuardStates(StateMachine stateMachine)
    {
        stateMachine.AddChild(new GuardPatrolState { Name = "Patrol" });
        stateMachine.AddChild(new EnemyIdleState { Name = "Idle" });
        stateMachine.AddChild(new GuardInvestigateState { Name = "Investigate" });
        stateMachine.AddChild(new GuardAlertState { Name = "Alert" });
        stateMachine.AddChild(new GuardChaseState { Name = "Chase" });
        stateMachine.AddChild(new GuardSearchState { Name = "Search" });
        stateMachine.AddChild(new GuardAttackState { Name = "Attack" });
    }

    /// <summary>Recursively set Owner on all descendants so GetOwner works.</summary>
    protected static void SetOwnerRecursive(Node node, Node owner)
    {
        foreach (var child in node.GetChildren())
        {
            child.Owner = owner;
            SetOwnerRecursive(child, owner);
        }
    }

    /// <summary>Create SpriteFrames with placeholder textures for a guard.</summary>
    protected static SpriteFrames CreateGuardSpriteFrames(string textureName)
    {
        var tex = PlaceholderSprites.Get(textureName) ?? PlaceholderSprites.Get("guard")!;
        var frames = new SpriteFrames();

        foreach (var animName in new[] { "idle", "run", "walk", "attack" })
        {
            frames.AddAnimation(animName);
            frames.SetAnimationSpeed(animName, animName == "attack" ? 12 : 8);
            frames.SetAnimationLoop(animName, animName != "attack");
            frames.AddFrame(animName, tex);
        }

        return frames;
    }

    // ─── Environment Factories ────────────────────────────────────

    /// <summary>Add a shadow zone (stealth-friendly dark area).</summary>
    protected static void AddShadowZone(Node2D parent, Vector2 pos, Vector2 size)
    {
        var zone = new ShadowZone { Name = "ShadowZone" };
        zone.Position = pos;
        zone.CollisionLayer = 0;
        zone.CollisionMask = 1 << 1; // Player.

        var shape = new CollisionShape2D();
        shape.Shape = new RectangleShape2D { Size = size };
        zone.AddChild(shape);

        // Visual indicator.
        var visual = new ColorRect();
        visual.Color = new Color(0.05f, 0.05f, 0.1f, 0.4f);
        visual.Position = -size / 2;
        visual.Size = size;
        zone.AddChild(visual);

        parent.AddChild(zone);
    }

    /// <summary>Add a named area zone (for location tracking / restriction).</summary>
    protected static void AddAreaZone(Node2D parent, string areaName, Vector2 center, Vector2 size, bool isRestricted = false)
    {
        var zone = new AreaZone { Name = $"Area_{areaName.Replace(" ", "_")}" };
        zone.Position = center;
        zone.AreaName = areaName;
        zone.IsRestricted = isRestricted;
        zone.CollisionLayer = 0;
        zone.CollisionMask = 1 << 1; // Player.

        var shape = new CollisionShape2D();
        shape.Shape = new RectangleShape2D { Size = size };
        zone.AddChild(shape);
        parent.AddChild(zone);
    }

    /// <summary>Add a hiding spot (interactable stealth object).</summary>
    protected static void AddHidingSpot(Node2D parent, string spotName, Vector2 pos)
    {
        var spot = new HidingSpot { Name = $"HidingSpot_{spotName.Replace(" ", "_")}" };
        spot.Position = pos;
        spot.DisplayName = spotName;
        spot.CollisionLayer = 1 << 5; // Interactable.
        spot.CollisionMask = 1 << 1;  // Player.

        var shape = new CollisionShape2D();
        shape.Shape = new RectangleShape2D { Size = new Vector2(16, 16) };
        spot.AddChild(shape);

        // Visual — slightly darker square.
        var visual = new ColorRect();
        visual.Color = new Color(0.15f, 0.25f, 0.1f, 0.6f);
        visual.Position = new Vector2(-8, -8);
        visual.Size = new Vector2(16, 16);
        spot.AddChild(visual);

        parent.AddChild(spot);
    }

    // ─── Puzzle Element Factories ─────────────────────────────────

    /// <summary>Place a locked door that requires a specific key to open.</summary>
    protected static Door AddLockedDoor(Node2D parent, string name, Vector2 pos, string requiredKeyId, bool isVertical = false)
    {
        var door = new Door { Name = $"Door_{name.Replace(" ", "_")}" };
        door.Position = pos;
        door.IsLocked = true;
        door.RequiredKeyId = requiredKeyId;
        door.DisplayName = name;
        door.CollisionLayer = 1 << 5; // Interactable.
        door.CollisionMask = 1 << 1;  // Player.

        var shape = new CollisionShape2D();
        shape.Shape = new RectangleShape2D { Size = new Vector2(16, 16) };
        door.AddChild(shape);

        // Door collision body.
        var body = new StaticBody2D { Name = "StaticBody2D" };
        body.CollisionLayer = 1;
        body.CollisionMask = 0;
        var bodyShape = new CollisionShape2D();
        float w = isVertical ? 6 : 32;
        float h = isVertical ? 32 : 6;
        bodyShape.Shape = new RectangleShape2D { Size = new Vector2(w, h) };
        body.AddChild(bodyShape);
        door.AddChild(body);

        // Visual — door rectangle with lock indicator.
        float vw = isVertical ? 8 : 32;
        float vh = isVertical ? 32 : 8;
        var visual = new Sprite2D { Name = "Sprite2D" };
        var img = Image.CreateEmpty((int)vw, (int)vh, false, Image.Format.Rgba8);
        img.Fill(new Color(0.5f, 0.3f, 0.15f, 1.0f));
        // Lock indicator — small yellow square in center.
        int cx = (int)vw / 2 - 2; int cy = (int)vh / 2 - 2;
        for (int x = cx; x < cx + 4 && x < (int)vw; x++)
            for (int y = cy; y < cy + 4 && y < (int)vh; y++)
                img.SetPixel(x, y, new Color(0.8f, 0.7f, 0.2f, 1.0f));
        visual.Texture = ImageTexture.CreateFromImage(img);
        visual.ZIndex = 2;
        door.AddChild(visual);

        parent.AddChild(door);
        return door;
    }

    /// <summary>Place an unlocked door (can be opened freely).</summary>
    protected static Door AddDoor(Node2D parent, string name, Vector2 pos, bool isVertical = false)
    {
        var door = new Door { Name = $"Door_{name.Replace(" ", "_")}" };
        door.Position = pos;
        door.IsLocked = false;
        door.DisplayName = name;
        door.CollisionLayer = 1 << 5;
        door.CollisionMask = 1 << 1;

        var shape = new CollisionShape2D();
        shape.Shape = new RectangleShape2D { Size = new Vector2(16, 16) };
        door.AddChild(shape);

        var body = new StaticBody2D { Name = "StaticBody2D" };
        body.CollisionLayer = 1;
        body.CollisionMask = 0;
        var bodyShape = new CollisionShape2D();
        float w = isVertical ? 6 : 32;
        float h = isVertical ? 32 : 6;
        bodyShape.Shape = new RectangleShape2D { Size = new Vector2(w, h) };
        body.AddChild(bodyShape);
        door.AddChild(body);

        float vw = isVertical ? 8 : 32;
        float vh = isVertical ? 32 : 8;
        var visual = new Sprite2D { Name = "Sprite2D" };
        var img = Image.CreateEmpty((int)vw, (int)vh, false, Image.Format.Rgba8);
        img.Fill(new Color(0.45f, 0.3f, 0.15f, 1.0f));
        visual.Texture = ImageTexture.CreateFromImage(img);
        visual.ZIndex = 2;
        door.AddChild(visual);

        parent.AddChild(door);
        return door;
    }

    /// <summary>Place a chest containing a key.</summary>
    protected static Chest AddKeyChest(Node2D parent, string name, Vector2 pos, string keyId, string keyDisplayName = "Key")
    {
        var chest = new Chest { Name = $"Chest_{name.Replace(" ", "_")}" };
        chest.Position = pos;
        chest.DisplayName = name;
        chest.ContentsType = "key";
        chest.ContentsId = keyId;
        chest.ContentsName = keyDisplayName;
        chest.CollisionLayer = 1 << 5;
        chest.CollisionMask = 1 << 1;

        var shape = new CollisionShape2D();
        shape.Shape = new RectangleShape2D { Size = new Vector2(16, 16) };
        chest.AddChild(shape);

        parent.AddChild(chest);
        return chest;
    }

    /// <summary>Place a chest containing items or ink.</summary>
    protected static Chest AddItemChest(Node2D parent, string name, Vector2 pos,
        string contentsType, string contentsId, string contentsName, int quantity = 1,
        string requiredKeyId = "")
    {
        var chest = new Chest { Name = $"Chest_{name.Replace(" ", "_")}" };
        chest.Position = pos;
        chest.DisplayName = name;
        chest.ContentsType = contentsType;
        chest.ContentsId = contentsId;
        chest.ContentsName = contentsName;
        chest.ContentsQuantity = quantity;
        chest.RequiredKeyId = requiredKeyId;
        chest.CollisionLayer = 1 << 5;
        chest.CollisionMask = 1 << 1;

        var shape = new CollisionShape2D();
        shape.Shape = new RectangleShape2D { Size = new Vector2(16, 16) };
        chest.AddChild(shape);

        parent.AddChild(chest);
        return chest;
    }

    /// <summary>Place a floor switch / pressure plate.</summary>
    protected static FloorSwitch AddFloorSwitch(Node2D parent, string name, Vector2 pos, bool stayPressed = false)
    {
        var sw = new FloorSwitch { Name = $"Switch_{name.Replace(" ", "_")}" };
        sw.Position = pos;
        sw.StayPressed = stayPressed;

        parent.AddChild(sw);
        return sw;
    }

    /// <summary>Place an interactable lever.</summary>
    protected static Lever AddLever(Node2D parent, string name, Vector2 pos, bool oneWay = false)
    {
        var lever = new Lever { Name = $"Lever_{name.Replace(" ", "_")}" };
        lever.Position = pos;
        lever.OneWay = oneWay;
        lever.CollisionLayer = 1 << 5;
        lever.CollisionMask = 1 << 1;

        var shape = new CollisionShape2D();
        shape.Shape = new RectangleShape2D { Size = new Vector2(12, 12) };
        lever.AddChild(shape);

        parent.AddChild(lever);
        return lever;
    }

    /// <summary>Place a puzzle gate that opens when conditions are met.</summary>
    protected static PuzzleGate AddPuzzleGate(Node2D parent, string name, Vector2 pos,
        int requiredConditions = 1, bool stayOpen = true, bool isVertical = false,
        float width = 32f, float height = 16f)
    {
        var gate = new PuzzleGate { Name = $"Gate_{name.Replace(" ", "_")}" };
        gate.Position = pos;
        gate.RequiredConditions = requiredConditions;
        gate.StayOpen = stayOpen;
        gate.IsVertical = isVertical;
        gate.GateWidth = width;
        gate.GateHeight = height;

        parent.AddChild(gate);
        return gate;
    }

    /// <summary>Place a pushable block for puzzles.</summary>
    protected static PushBlock AddPushBlock(Node2D parent, string name, Vector2 pos)
    {
        var block = new PushBlock { Name = $"PushBlock_{name.Replace(" ", "_")}" };
        block.Position = pos;

        parent.AddChild(block);
        return block;
    }

    /// <summary>Place a breakable/cracked wall.</summary>
    protected static BreakableWall AddBreakableWall(Node2D parent, string name, Vector2 pos,
        int hitsRequired = 1, float width = 16f, float height = 16f)
    {
        var wall = new BreakableWall { Name = $"BreakableWall_{name.Replace(" ", "_")}" };
        wall.Position = pos;
        wall.HitsRequired = hitsRequired;
        wall.WallWidth = width;
        wall.WallHeight = height;

        parent.AddChild(wall);
        return wall;
    }

    // ─── Mission Flow Helpers ─────────────────────────────────────

    /// <summary>
    /// Common target kill handler — registers the kill, awards ink,
    /// tracks moral choices, unlocks Blood Echoes, populates
    /// MissionComplete screen, and transitions after delay.
    /// </summary>
    protected void OnTargetKilled(
        string targetId,
        int kingdomIndex,
        string targetDisplayName,
        string whisperText,
        float delaySeconds = 2.0f)
    {
        GD.Print($"═══ TARGET {targetId.ToUpper()} SLAIN ═══");
        GD.Print(whisperText);

        var gm = GameManager.Instance;
        string rewardText = "";
        if (gm != null)
        {
            var killed = gm.Kingdoms[kingdomIndex].KillTarget(targetId);
            if (killed != null)
            {
                // Award ink.
                gm.InkInventory?.AddInk(killed.InkDrop, killed.InkAmount);
                rewardText = $"Blood-Ink Acquired: {killed.InkAmount}× {killed.InkDrop} Grade";
                GD.Print($"Blood-Ink acquired: {killed.InkAmount}x {killed.InkDrop} grade!");

                // Track moral choices — optional kills increase cruelty.
                if (!killed.IsMandatory)
                    gm.Choices?.RecordOptionalKill();

                // Unlock Blood Echo for Edictbearer kills.
                if (killed.IsEdictbearer && !string.IsNullOrEmpty(killed.BloodEchoId))
                {
                    gm.EchoManager?.UnlockEcho(killed.BloodEchoId);
                    rewardText += "\nBlood Echo Unlocked";
                }
            }
        }

        MissionComplete.TargetText = targetDisplayName;
        MissionComplete.WhisperText = whisperText;
        MissionComplete.RewardText = rewardText;

        // Capture tree reference before timer to avoid ObjectDisposedException if node freed.
        var tree = GetTree();
        var timer = tree.CreateTimer(delaySeconds);
        timer.Timeout += () => tree.ChangeSceneToFile("res://Scenes/UI/MissionComplete.tscn");
    }
}
