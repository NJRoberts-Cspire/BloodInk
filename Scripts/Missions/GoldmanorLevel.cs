using Godot;
using System;
using BloodInk.Combat;
using BloodInk.Content;
using BloodInk.Core;
using BloodInk.Enemies;
using BloodInk.Enemies.States;
using BloodInk.Interaction;
using BloodInk.Stealth;
using BloodInk.Tools;
using BloodInk.World;

namespace BloodInk.Missions;

/// <summary>
/// Goldmanor — Lord Cowl's estate. The first assassination mission.
/// Three zones: Gardens (entry) → Main Hall → Lord Cowl's Quarters.
/// Built procedurally from tile maps.
/// </summary>
public partial class GoldmanorLevel : Node2D
{
    // ─── Map Layout ──────────────────────────────────────────────
    // Each zone is a block of ASCII tiles.
    // . = stone floor, w = wood floor, # = wall, ~ = shadow, , = grass
    // p = path, c = carpet

    private static readonly string[] GardenMap = {
        ",,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,",
        ",,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,",
        ",,,,~,,,,,,,,,pppp,,,,,,,,,,~,,,,,,",
        ",,,,,,,,,,,,,pp....pp,,,,,,,,,,,,,,,",
        ",,,,,,,,,,,,p......p,,,,,,,,,,,,,,,,",
        ",,,,,,,,,,,p........p,,,,,,,,,,,,,,,",
        ",,,,,,,,,,p..........p,,,,,,,,,,,,,,",
        ",,,,,,,,,p............p,,,,,,,,,,,,,",
        ",,,,,,,,p..............p,,,,,,,,,,,,",
        ",,~,,,,,p..............p,,,,,,~,,,,,",
        ",,,,,,,,p..............p,,,,,,,,,,,,",
        ",,,,,,,,,p............p,,,,,,,,,,,,,",
        ",,,,,,,,,,p..........p,,,,,,,,,,,,,,",
        ",,,,,,,,,,,p........p,,,,,,,,,,,,,,,",
        ",,,,,,,,,,,,p......p,,,,,,,,,,,,,,,,",
        ",,,,,~,,,,,,,pp..pp,,,,,,,,,~,,,,,,",
        ",,,,,,,,,,,,,,pppp,,,,,,,,,,,,,,,,,,",
        ",,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,",
    };

    private static readonly string[] HallMap = {
        "####################################",
        "#wwwwwwwwww#........#wwwwwwwwwwwwww#",
        "#wwwwwwwwww#........#wwwwwwwwwwwwww#",
        "#wwwwwwwwww#........#wwwwwwwwwwwwww#",
        "#wwwwwwwwww....cc....wwwwwwwwwwwwww#",
        "#wwwwwwwwww....cc....wwwwwwwwwwwwww#",
        "#wwwwwwwwww#........#wwwwwwwwwwwwww#",
        "#wwwwwwwwww#........#wwwwwwwwwwwwww#",
        "#wwwwwwwwww#...cc...#wwwwwwwwwwwwww#",
        "#wwwwwwwwww#...cc...#wwwwwwwwwwwwww#",
        "#wwwwwwwwww#........#wwwwwwwwwwwwww#",
        "#wwwwwwwwww#........#wwwwwwwwwwwwww#",
        "####################################",
    };

    private static readonly string[] QuartersMap = {
        "####################################",
        "#cccccccccc#wwwwwwww#~~wwwwwwwwwwww#",
        "#cccccccccc#wwwwwwww#~~wwwwwwwwwwww#",
        "#cccccccccc#wwwwwwww#wwwwwwwwwwwwww#",
        "#cccccccccc....wwwww#wwwwwwwwwwwwww#",
        "#cccccccccc....wwwww#wwwwwwwwwwwwww#",
        "#cccccccccc#wwwwwwww#wwwwwwwwwwwwww#",
        "#cccccccccc#wwwwwwww....wwwwwwwwwww#",
        "#cccccccccc#wwwwwwww....wwwwwwwwwww#",
        "#cccccccccc#wwwwwwww#wwwwwwwwwwwwww#",
        "#cccccccccc#wwwwwwww#wwwwwwwwwwwwww#",
        "####################################",
    };

    // ─── Zone offsets (world positions) ──────────────────────────
    private static readonly Vector2 GardenOffset = new(0, 288);  // Gardens below
    private static readonly Vector2 HallOffset = new(0, 0);      // Hall in middle
    private static readonly Vector2 QuartersOffset = new(0, -208); // Quarters above

    public override void _Ready()
    {
        PlaceholderSprites.CreateAll();

        BuildGardens();
        BuildMainHall();
        BuildQuarters();
        SpawnPlayer();
        SetupHUD();
        RegisterTargets();

        GD.Print("═══ GOLDMANOR LOADED ═══");
    }

    // ═════════════════════════════════════════════════════════════
    //  ZONE 1: GARDENS (entry point, 2 patrolling guards)
    // ═════════════════════════════════════════════════════════════

    private void BuildGardens()
    {
        var gardenRoot = new Node2D { Name = "Gardens" };
        gardenRoot.Position = GardenOffset;
        AddChild(gardenRoot);

        MapBuilder.Build(gardenRoot, GardenMap);

        // Area zone.
        AddAreaZone(gardenRoot, "Goldmanor Gardens", new Vector2(288, 144), new Vector2(576, 288));

        // Shadow zones in corners.
        AddShadowZone(gardenRoot, new Vector2(80, 40), new Vector2(48, 48));
        AddShadowZone(gardenRoot, new Vector2(480, 40), new Vector2(48, 48));
        AddShadowZone(gardenRoot, new Vector2(80, 248), new Vector2(48, 48));
        AddShadowZone(gardenRoot, new Vector2(480, 248), new Vector2(48, 48));

        // Hiding spots (bushes).
        AddHidingSpot(gardenRoot, "Dense Bush", new Vector2(64, 144));
        AddHidingSpot(gardenRoot, "Hedge", new Vector2(512, 144));

        // 2 garden patrol guards — waypoints are offsets from guard's local position.
        AddGuard(gardenRoot, "GardenGuard1", new Vector2(200, 100), new Vector2[]
        {
            new(0, 0), new(0, 100), new(150, 100), new(150, 0)
        });
        AddGuard(gardenRoot, "GardenGuard2", new Vector2(400, 180), new Vector2[]
        {
            new(0, 0), new(0, -100), new(-220, -100), new(-220, 0)
        });
    }

    // ═════════════════════════════════════════════════════════════
    //  ZONE 2: MAIN HALL (3 guards, dining area, servant passages)
    // ═════════════════════════════════════════════════════════════

    private void BuildMainHall()
    {
        var hallRoot = new Node2D { Name = "MainHall" };
        hallRoot.Position = HallOffset;
        AddChild(hallRoot);

        MapBuilder.Build(hallRoot, HallMap);

        // Area zone.
        AddAreaZone(hallRoot, "Main Hall", new Vector2(288, 104), new Vector2(576, 208), isRestricted: true);

        // Shadow zone in left wing.
        AddShadowZone(hallRoot, new Vector2(80, 104), new Vector2(48, 32));

        // Hiding spot — under banquet table.
        AddHidingSpot(hallRoot, "Under Table", new Vector2(288, 104));

        // 3 hall guards — waypoints are local offsets.
        AddGuard(hallRoot, "HallGuard1", new Vector2(200, 50), new Vector2[]
        {
            new(0, 0), new(0, 110), new(180, 110), new(180, 0)
        });
        AddGuard(hallRoot, "HallGuard2", new Vector2(100, 104), new Vector2[]
        {
            new(0, -64), new(0, 66)
        });
        AddGuard(hallRoot, "HallGuard3", new Vector2(470, 104), new Vector2[]
        {
            new(0, -64), new(0, 66)
        });
    }

    // ═════════════════════════════════════════════════════════════
    //  ZONE 3: LORD COWL'S QUARTERS (the target + 2 elite guards)
    // ═════════════════════════════════════════════════════════════

    private void BuildQuarters()
    {
        var quartersRoot = new Node2D { Name = "Quarters" };
        quartersRoot.Position = QuartersOffset;
        AddChild(quartersRoot);

        MapBuilder.Build(quartersRoot, QuartersMap);

        // Area zone.
        AddAreaZone(quartersRoot, "Lord Cowl's Quarters", new Vector2(288, 88), new Vector2(576, 176), isRestricted: true);

        // Shadow zone in the dark alcove.
        AddShadowZone(quartersRoot, new Vector2(400, 32), new Vector2(32, 32));

        // Hiding spot — wardrobe.
        AddHidingSpot(quartersRoot, "Wardrobe", new Vector2(48, 48));

        // 2 elite guards (tougher) — waypoints are local offsets.
        AddGuard(quartersRoot, "EliteGuard1", new Vector2(240, 80), new Vector2[]
        {
            new(0, -40), new(0, 60), new(100, 60), new(100, -40)
        }, elite: true);
        AddGuard(quartersRoot, "EliteGuard2", new Vector2(460, 80), new Vector2[]
        {
            new(0, -40), new(0, 60)
        }, elite: true);

        // LORD COWL — pacing in his study (left room with carpet).
        SpawnLordCowl(quartersRoot);
    }

    // ═════════════════════════════════════════════════════════════
    //  LORD COWL
    // ═════════════════════════════════════════════════════════════

    private void SpawnLordCowl(Node2D parent)
    {
        // Use actual C# types so GetNode<T>/GetOwner<T> work at runtime.
        var cowl = new GuardEnemy { Name = "LordCowl" };
        cowl.Position = new Vector2(80, 80);
        cowl.CollisionLayer = 1 << 2;  // Enemy layer (layer 3).
        cowl.CollisionMask = 1;          // World.
        cowl.MoveSpeed = 35f;
        cowl.PatrolSpeed = 25f;
        cowl.AlertedSpeed = 50f;
        cowl.ChaseSpeed = 65f;
        cowl.DetectRange = 150f;
        cowl.AttackRange = 30f;

        // Sprite.
        var sprite = new AnimatedSprite2D { Name = "AnimatedSprite2D" };
        var frames = CreateGuardSpriteFrames("cowl");
        sprite.SpriteFrames = frames;
        cowl.AddChild(sprite);

        // Collision body.
        var bodyShape = new CollisionShape2D();
        bodyShape.Shape = new RectangleShape2D { Size = new Vector2(12, 16) };
        cowl.AddChild(bodyShape);

        // Hurtbox.
        var hurtbox = new Hurtbox { Name = "Hurtbox" };
        hurtbox.CollisionLayer = 0;
        hurtbox.CollisionMask = 1 << 3; // PlayerHitbox layer.
        var hurtShape = new CollisionShape2D { Name = "HurtboxShape" };
        hurtShape.Shape = new RectangleShape2D { Size = new Vector2(14, 18) };
        hurtbox.AddChild(hurtShape);
        cowl.AddChild(hurtbox);

        // Hitbox.
        var hitbox = new Hitbox { Name = "Hitbox" };
        hitbox.CollisionLayer = 1 << 4; // EnemyHitbox layer.
        hitbox.CollisionMask = 0;
        hitbox.Damage = 2;
        hitbox.KnockbackForce = new Vector2(120f, 0f);
        var hitShape = new CollisionShape2D { Name = "HitboxShape" };
        hitShape.Shape = new RectangleShape2D { Size = new Vector2(20, 16) };
        hitbox.AddChild(hitShape);
        cowl.AddChild(hitbox);

        // Health (boss — higher HP).
        var health = new HealthComponent { Name = "HealthComponent" };
        health.MaxHealth = 8;
        cowl.AddChild(health);

        // Detection sensor.
        var sensor = new DetectionSensor { Name = "DetectionSensor" };
        sensor.ViewDistance = 150f;
        sensor.ViewAngle = 65f;
        sensor.CloseDetectRadius = 35f;
        sensor.AwarenessGainRate = 50f;
        cowl.AddChild(sensor);

        // Patrol route — pacing in study.
        var patrol = new PatrolRoute { Name = "PatrolRoute" };
        patrol.Waypoints = new Vector2[] { new(0, 0), new(60, 0), new(60, 40), new(0, 40) };
        cowl.AddChild(patrol);

        // State machine with guard states.
        var stateMachine = new StateMachine { Name = "StateMachine" };
        cowl.AddChild(stateMachine);
        AddGuardStates(stateMachine);

        // Tag for identification.
        cowl.SetMeta("target_id", "cowl");
        cowl.SetMeta("is_boss", true);

        parent.AddChild(cowl);

        // Set Owner on all descendants so GetOwner<GuardEnemy>() works in states.
        SetOwnerRecursive(cowl, cowl);

        // Wire death to mission complete.
        health.Died += () => OnCowlKilled(cowl);
    }

    private void OnCowlKilled(Node2D cowl)
    {
        GD.Print("═══ LORD COWL SLAIN ═══");
        GD.Print("\"I never saw the dark as empty. I thought it was full of things that loved me.\"");

        // Register the kill with KingdomState.
        var gm = GameManager.Instance;
        if (gm != null)
        {
            var target = GreenholdTargets.LordHarlanCowl();
            gm.Kingdoms[0].RegisterTarget(target);
            var killed = gm.Kingdoms[0].KillTarget("cowl");

            // Award ink.
            if (killed != null)
            {
                gm.InkInventory?.AddInk(killed.InkDrop, killed.InkAmount);
                GD.Print($"Blood-Ink acquired: {killed.InkAmount}x {killed.InkDrop} grade!");
            }
        }

        // Show mission complete after a delay.
        var timer = GetTree().CreateTimer(2.0f);
        timer.Timeout += ShowMissionComplete;
    }

    private void ShowMissionComplete()
    {
        // Transition to mission complete screen.
        GetTree().ChangeSceneToFile("res://Scenes/UI/MissionComplete.tscn");
    }

    // ═════════════════════════════════════════════════════════════
    //  PLAYER SPAWN
    // ═════════════════════════════════════════════════════════════

    private void SpawnPlayer()
    {
        var playerScene = GD.Load<PackedScene>("res://Scenes/Player/Player.tscn");
        if (playerScene == null)
        {
            GD.PrintErr("Cannot load Player.tscn!");
            return;
        }

        var player = playerScene.Instantiate<CharacterBody2D>();
        player.Position = GardenOffset + new Vector2(288, 260); // Bottom of garden.
        AddChild(player);

        // Assign placeholder sprite color.
        CallDeferred(MethodName.ApplyPlayerSprite, player);
    }

    private void ApplyPlayerSprite(CharacterBody2D player)
    {
        var sprite = player.GetNodeOrNull<AnimatedSprite2D>("AnimatedSprite2D");
        if (sprite != null)
        {
            var tex = PlaceholderSprites.Get("player");
            if (tex != null)
            {
                // Create a simple SpriteFrames with our placeholder.
                var frames = new SpriteFrames();
                frames.AddAnimation("idle");
                frames.SetAnimationSpeed("idle", 1);
                frames.SetAnimationLoop("idle", true);
                frames.AddFrame("idle", tex);

                frames.AddAnimation("run");
                frames.SetAnimationSpeed("run", 8);
                frames.SetAnimationLoop("run", true);
                frames.AddFrame("run", tex);

                frames.AddAnimation("attack");
                frames.SetAnimationSpeed("attack", 12);
                frames.SetAnimationLoop("attack", false);
                frames.AddFrame("attack", tex);

                frames.AddAnimation("dodge");
                frames.SetAnimationSpeed("dodge", 10);
                frames.SetAnimationLoop("dodge", false);
                frames.AddFrame("dodge", tex);

                frames.AddAnimation("death");
                frames.SetAnimationSpeed("death", 1);
                frames.SetAnimationLoop("death", false);
                frames.AddFrame("death", tex);

                frames.AddAnimation("crouch_idle");
                frames.SetAnimationSpeed("crouch_idle", 1);
                frames.SetAnimationLoop("crouch_idle", true);
                var crouchTex = PlaceholderSprites.Get("player_crouch") ?? tex;
                frames.AddFrame("crouch_idle", crouchTex);

                frames.AddAnimation("crouch_walk");
                frames.SetAnimationSpeed("crouch_walk", 8);
                frames.SetAnimationLoop("crouch_walk", true);
                frames.AddFrame("crouch_walk", crouchTex);

                sprite.SpriteFrames = frames;
                sprite.Play("idle");
            }
        }
    }

    // ═════════════════════════════════════════════════════════════
    //  HUD SETUP
    // ═════════════════════════════════════════════════════════════

    private void SetupHUD()
    {
        var hud = GD.Load<PackedScene>("res://Scenes/UI/GameHUD.tscn");
        if (hud != null)
        {
            AddChild(hud.Instantiate());
        }
    }

    // ═════════════════════════════════════════════════════════════
    //  TARGET REGISTRATION
    // ═════════════════════════════════════════════════════════════

    private void RegisterTargets()
    {
        var gm = GameManager.Instance;
        if (gm == null) return;

        // Register all Greenhold targets.
        foreach (var target in GreenholdTargets.GetAll())
        {
            gm.Kingdoms[0].RegisterTarget(target);
        }
    }

    // ═════════════════════════════════════════════════════════════
    //  HELPER FACTORIES
    // ═════════════════════════════════════════════════════════════

    private void AddGuard(Node2D parent, string name, Vector2 pos, Vector2[] waypoints, bool elite = false)
    {
        // Use actual C# types so GetNode<T>/GetOwner<T> work at runtime.
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

        // State machine with guard states.
        var stateMachine = new StateMachine { Name = "StateMachine" };
        guard.AddChild(stateMachine);
        AddGuardStates(stateMachine);

        parent.AddChild(guard);

        // Set Owner on all descendants so GetOwner<GuardEnemy>() works in states.
        SetOwnerRecursive(guard, guard);
    }

    /// <summary>Create all 6 guard AI states and add to the state machine.</summary>
    private void AddGuardStates(StateMachine stateMachine)
    {
        stateMachine.AddChild(new GuardPatrolState { Name = "Patrol" });
        stateMachine.AddChild(new GuardInvestigateState { Name = "Investigate" });
        stateMachine.AddChild(new GuardAlertState { Name = "Alert" });
        stateMachine.AddChild(new GuardChaseState { Name = "Chase" });
        stateMachine.AddChild(new GuardSearchState { Name = "Search" });
        stateMachine.AddChild(new GuardAttackState { Name = "Attack" });
    }

    /// <summary>Recursively set Owner on all descendants so GetOwner works.</summary>
    private void SetOwnerRecursive(Node node, Node owner)
    {
        foreach (var child in node.GetChildren())
        {
            child.Owner = owner;
            SetOwnerRecursive(child, owner);
        }
    }

    /// <summary>Create SpriteFrames with placeholder textures for a guard.</summary>
    private SpriteFrames CreateGuardSpriteFrames(string textureName)
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

    private void SetPatrolWaypoints(CharacterBody2D guard, Vector2[] waypoints)
    {
        var patrol = guard.GetNodeOrNull<PatrolRoute>("PatrolRoute");
        if (patrol != null && waypoints.Length > 0)
        {
            patrol.Waypoints = waypoints;
        }
    }

    private void AddShadowZone(Node2D parent, Vector2 pos, Vector2 size)
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

    private void AddAreaZone(Node2D parent, string areaName, Vector2 center, Vector2 size, bool isRestricted = false)
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

    private void AddHidingSpot(Node2D parent, string spotName, Vector2 pos)
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
}
