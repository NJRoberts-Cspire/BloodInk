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
using BloodInk.UI;
using BloodInk.World;

namespace BloodInk.Missions;

/// <summary>
/// Goldmanor — Lord Cowl's estate. The first assassination mission.
/// Three zones: Gardens (entry) → Main Hall → Lord Cowl's Quarters.
/// Built procedurally from tile maps.
/// </summary>
public partial class GoldmanorLevel : MissionLevelBase
{
    // ─── Map Layout ──────────────────────────────────────────────
    // Each zone is a block of ASCII tiles.
    // . = stone floor, w = wood floor, # = wall, ~ = shadow, , = grass
    // p = path, c = carpet

    private static readonly string[] GardenMap = {
        ",,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,",
        ",,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,",
        ",,~,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,~,,,,,",
        ",,,,,,,,,,,,pppppppppppppppppppppppppppppppppppppp,,,,,,,,,,,,,,",
        ",,,,,,,,,,,pp..............................................pp,,",
        ",,,,,,,,,,pp................................................pp,",
        ",,,,,,,,,pp..................................................pp",
        ",,,,,,,,pp.................,,,,,,,,,,................,,,,.....pp",
        ",,,,,,,pp..........,,,,,,,,,,,,,,,,,,,,,,,......,,,,,,,,,.....p",
        ",,,,,,pp..........,,,,,,~,,,,,,,,,,,,~,,,,,....,,,,,,,,~,,....p",
        ",,,,,pp...........,,,,,,,,,,,,,,,,,,,,,,,,,....,,,,,,,,,,,,....,p",
        ",,,,,p............,,,,,,,,,,,,,,,,,,,,,,,,,....,,,,,,,,,,,,....,p",
        ",,,,,p............,,,,,,,,,,,,,,pppppp,,,,,....,,,,,,,,,,,....,p",
        ",,~,,p............,,,,,,,,,,,,pp......pp,,,....,,,,,,~,,,,....p",
        ",,,,,p............,,,,,,,,,,pp..........pp,,....,,,,,,,,,,,....p",
        ",,,,,p.............,,,,,,,,p..............p,....,,,,,,,,,,,....p",
        ",,,,,pp.............,,,,,,p................p....,,,,,,,,,,,....p",
        ",,,,,,pp.............,,,,,p................p.....,,,,,,,,,,....p",
        ",,,,,,,pp............,,,,,p................p......,,,,,,,,,....p",
        ",,,,,,,,pp...........,,,,,p................p.......,,,,,,,,....p",
        ",,,,,,,,,pp..........,,,,,,p..............p,........,,,,,,....,p",
        ",,,,,,,,,,pp..........,,,,,pp..........pp,..........,,,,,,....,p",
        ",,,,,,,,,,,pp..........,,,,,pp........pp,............,,,,....,p",
        ",,,,,,,,,,,,pp..........,,,,,,pp....pp,,......................,p",
        ",,,,,,,,,,,,,pp..........,,,,,,,pppp,,,,......................,p",
        ",,,,,,,,,,,,,,pp..............................................p",
        ",,,,,,,,,,,,,,,pp............................................pp",
        ",,,,,,,,,,,,,,,,pp..........................................pp,",
        ",,,,,~,,,,,,,,,,pppppppppppppppppp....pppppppppppppppppppppp,,,",
        ",,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,",
        ",,~,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,~,,,,,",
        ",,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,",
        ",,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,",
    };

    private static readonly string[] HallMap = {
        "################################################################",
        "#wwwwww#........#wwwwwwww#..........#wwwwwwww#........#wwwwwwww#",
        "#wwwwww#........#wwwwwwww#..........#wwwwwwww#........#wwwwwwww#",
        "#wwwwww#........#wwwwwwww#..........#wwwwwwww#........#wwwwwwww#",
        "#wwwwww#........#wwwwwwww#..........#wwwwwwww#........#wwwwwwww#",
        "#wwwwww....cc....wwwwwwww#....cc....#wwwwwwww....cc....wwwwwwww#",
        "#wwwwww....cc....wwwwwwww#....cc....#wwwwwwww....cc....wwwwwwww#",
        "#wwwwww#........#wwwwwwww#..........#wwwwwwww#........#wwwwwwww#",
        "#wwwwww#........#wwwwwwww#..........#wwwwwwww#........#wwwwwwww#",
        "#wwwwww#........#........#..........#........#........#wwwwwwww#",
        "#wwwwww#........#........#..........#........#........#wwwwwwww#",
        "#......#........#........#cccccccccc#........#........#........#",
        "#......#........#........#cccccccccc#........#........#........#",
        "#................................................................................#",
        "#................................................................................#",
        "#......#........#........#cccccccccc#........#........#........#",
        "#......#........#........#cccccccccc#........#........#........#",
        "#wwwwww#........#........#..........#........#........#wwwwwwww#",
        "#wwwwww#........#........#..........#........#........#wwwwwwww#",
        "#wwwwww#........#wwwwwwww#..........#wwwwwwww#........#wwwwwwww#",
        "#wwwwww#........#wwwwwwww#..........#wwwwwwww#........#wwwwwwww#",
        "#wwwwww....cc....wwwwwwww#....cc....#wwwwwwww....cc....wwwwwwww#",
        "#wwwwww....cc....wwwwwwww#....cc....#wwwwwwww....cc....wwwwwwww#",
        "#wwwwww#........#wwwwwwww#..........#wwwwwwww#........#wwwwwwww#",
        "#wwwwww#...cc...#wwwwwwww#..........#wwwwwwww#...cc...#wwwwwwww#",
        "#wwwwww#...cc...#wwwwwwww#..........#wwwwwwww#...cc...#wwwwwwww#",
        "#wwwwww#........#wwwwwwww#..........#wwwwwwww#........#wwwwwwww#",
        "#wwwwww#........#wwwwwwww#..........#wwwwwwww#........#wwwwwwww#",
        "################################################################",
    };

    private static readonly string[] QuartersMap = {
        "################################################################",
        "#cccccccccc#wwwwwwww#........#wwwwwwww#~~wwwwwwwwww#wwwwwwwwww#",
        "#cccccccccc#wwwwwwww#........#wwwwwwww#~~wwwwwwwwww#wwwwwwwwww#",
        "#cccccccccc#wwwwwwww#........#wwwwwwww#wwwwwwwwwwww#wwwwwwwwww#",
        "#cccccccccc#wwwwwwww#........#wwwwwwww#wwwwwwwwwwww#wwwwwwwwww#",
        "#cccccccccc....wwwww#........#wwwwwwww#wwwwwwwwwwww....wwwwwww#",
        "#cccccccccc....wwwww#........#wwwwwwww#wwwwwwwwwwww....wwwwwww#",
        "#cccccccccc#wwwwwwww#........#wwwwwwww#wwwwwwwwwwww#wwwwwwwwww#",
        "#cccccccccc#wwwwwwww#........#........#wwwwwwwwwwww#wwwwwwwwww#",
        "#cccccccccc#wwwwwwww#........#........#wwwwwwwwwwww#wwwwwwwwww#",
        "#..........#wwwwwwww#........#........#wwwwwwwwwwww#..........#",
        "#..........#wwwwwwww#........#........#wwwwwwwwwwww#..........#",
        "#..................................................................#",
        "#..................................................................#",
        "#..........#wwwwwwww#........#........#wwwwwwwwwwww#..........#",
        "#..........#wwwwwwww#........#........#wwwwwwwwwwww#..........#",
        "#cccccccccc#wwwwwwww#........#........#wwwwwwwwwwww#wwwwwwwwww#",
        "#cccccccccc#wwwwwwww#........#........#wwwwwwwwwwww#wwwwwwwwww#",
        "#cccccccccc#wwwwwwww....wwwww#wwwwwwww....wwwwwwwww#wwwwwwwwww#",
        "#cccccccccc#wwwwwwww....wwwww#wwwwwwww....wwwwwwwww#wwwwwwwwww#",
        "#cccccccccc#wwwwwwww#wwwwwwww#wwwwwwww#~~wwwwwwwwww#wwwwwwwwww#",
        "#cccccccccc#wwwwwwww#wwwwwwww#wwwwwwww#~~wwwwwwwwww#wwwwwwwwww#",
        "#cccccccccc#wwwwwwww#wwwwwwww#wwwwwwww#wwwwwwwwwwww#wwwwwwwwww#",
        "#cccccccccc#wwwwwwww#wwwwwwww#wwwwwwww#wwwwwwwwwwww#wwwwwwwwww#",
        "################################################################",
    };

    // ─── Zone offsets (world positions) ──────────────────────────
    private static readonly Vector2 GardenOffset = new(0, 864);   // Gardens below (29 rows × 16 + spacing)
    private static readonly Vector2 HallOffset = new(0, 0);       // Hall in middle
    private static readonly Vector2 QuartersOffset = new(0, -528); // Quarters above (25 rows × 16 + spacing)

    public override void _Ready()
    {
        PlaceholderSprites.CreateAll();

        BuildGardens();
        BuildMainHall();
        BuildQuarters();
        SpawnPlayer(GardenOffset + new Vector2(512, 460));
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

        // Area zone — covers full garden.
        AddAreaZone(gardenRoot, "Goldmanor Gardens", new Vector2(512, 264), new Vector2(1024, 528));

        // Shadow zones — dark hedge corners and alcoves.
        AddShadowZone(gardenRoot, new Vector2(48, 48), new Vector2(64, 64));
        AddShadowZone(gardenRoot, new Vector2(960, 48), new Vector2(64, 64));
        AddShadowZone(gardenRoot, new Vector2(48, 460), new Vector2(64, 64));
        AddShadowZone(gardenRoot, new Vector2(960, 460), new Vector2(64, 64));
        AddShadowZone(gardenRoot, new Vector2(160, 144), new Vector2(48, 48));
        AddShadowZone(gardenRoot, new Vector2(800, 144), new Vector2(48, 48));
        AddShadowZone(gardenRoot, new Vector2(480, 340), new Vector2(40, 40));

        // Hiding spots — plenty for multiple stealth routes.
        AddHidingSpot(gardenRoot, "Dense Bush", new Vector2(64, 200));
        AddHidingSpot(gardenRoot, "Hedge Alcove", new Vector2(944, 200));
        AddHidingSpot(gardenRoot, "Rose Trellis", new Vector2(300, 100));
        AddHidingSpot(gardenRoot, "Overgrown Arch", new Vector2(700, 100));
        AddHidingSpot(gardenRoot, "Tool Shed", new Vector2(160, 380));
        AddHidingSpot(gardenRoot, "Fountain Shadow", new Vector2(512, 260));
        AddHidingSpot(gardenRoot, "Vine Alcove", new Vector2(840, 380));

        // 4 garden patrol guards — longer routes across the expanded grounds.
        AddGuard(gardenRoot, "GardenGuard1", new Vector2(280, 120), new Vector2[]
        {
            new(0, 0), new(0, 200), new(200, 200), new(200, 0)
        });
        AddGuard(gardenRoot, "GardenGuard2", new Vector2(720, 120), new Vector2[]
        {
            new(0, 0), new(0, 200), new(-200, 200), new(-200, 0)
        });
        AddGuard(gardenRoot, "GardenGuard3", new Vector2(512, 380), new Vector2[]
        {
            new(-180, 0), new(180, 0)
        });
        AddGuard(gardenRoot, "GardenGuard4", new Vector2(160, 300), new Vector2[]
        {
            new(0, 0), new(120, 0), new(120, 140), new(0, 140)
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

        // Area zone — covers full hall.
        AddAreaZone(hallRoot, "Main Hall", new Vector2(512, 232), new Vector2(1024, 464), isRestricted: true);

        // Shadow zones — dark wings and servant corridors.
        AddShadowZone(hallRoot, new Vector2(64, 64), new Vector2(48, 48));
        AddShadowZone(hallRoot, new Vector2(960, 64), new Vector2(48, 48));
        AddShadowZone(hallRoot, new Vector2(64, 400), new Vector2(48, 48));
        AddShadowZone(hallRoot, new Vector2(960, 400), new Vector2(48, 48));
        AddShadowZone(hallRoot, new Vector2(256, 232), new Vector2(40, 32));
        AddShadowZone(hallRoot, new Vector2(768, 232), new Vector2(40, 32));

        // Hiding spots — multiple options throughout the hall.
        AddHidingSpot(hallRoot, "Under Table", new Vector2(360, 100));
        AddHidingSpot(hallRoot, "Behind Pillar", new Vector2(160, 200));
        AddHidingSpot(hallRoot, "Servant Passage", new Vector2(80, 300));
        AddHidingSpot(hallRoot, "Banquet Alcove", new Vector2(640, 100));
        AddHidingSpot(hallRoot, "Wine Rack", new Vector2(900, 300));

        // 5 hall guards — patrols leave gaps for sneaking through corridors.
        AddGuard(hallRoot, "HallGuard1", new Vector2(300, 80), new Vector2[]
        {
            new(0, 0), new(0, 180), new(200, 180), new(200, 0)
        });
        AddGuard(hallRoot, "HallGuard2", new Vector2(700, 80), new Vector2[]
        {
            new(0, 0), new(0, 180), new(-200, 180), new(-200, 0)
        });
        AddGuard(hallRoot, "HallGuard3", new Vector2(120, 232), new Vector2[]
        {
            new(0, -100), new(0, 100)
        });
        AddGuard(hallRoot, "HallGuard4", new Vector2(900, 232), new Vector2[]
        {
            new(0, -100), new(0, 100)
        });
        AddGuard(hallRoot, "HallGuard5", new Vector2(512, 360), new Vector2[]
        {
            new(-200, 0), new(200, 0)
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

        // Area zone — covers full quarters.
        AddAreaZone(quartersRoot, "Lord Cowl's Quarters", new Vector2(512, 200), new Vector2(1024, 400), isRestricted: true);

        // Shadow zones — dark alcoves and corners.
        AddShadowZone(quartersRoot, new Vector2(64, 48), new Vector2(48, 48));
        AddShadowZone(quartersRoot, new Vector2(960, 48), new Vector2(48, 48));
        AddShadowZone(quartersRoot, new Vector2(480, 100), new Vector2(40, 32));
        AddShadowZone(quartersRoot, new Vector2(64, 340), new Vector2(48, 48));
        AddShadowZone(quartersRoot, new Vector2(960, 340), new Vector2(48, 48));

        // Hiding spots — options to approach Cowl from multiple angles.
        AddHidingSpot(quartersRoot, "Wardrobe", new Vector2(80, 80));
        AddHidingSpot(quartersRoot, "Behind Curtains", new Vector2(900, 80));
        AddHidingSpot(quartersRoot, "Under Desk", new Vector2(320, 300));
        AddHidingSpot(quartersRoot, "Dark Alcove", new Vector2(700, 300));

        // 3 elite guards — tougher, patrolling the private quarters.
        AddGuard(quartersRoot, "EliteGuard1", new Vector2(300, 100), new Vector2[]
        {
            new(0, -50), new(0, 100), new(150, 100), new(150, -50)
        }, elite: true);
        AddGuard(quartersRoot, "EliteGuard2", new Vector2(720, 100), new Vector2[]
        {
            new(0, -50), new(0, 100), new(-150, 100), new(-150, -50)
        }, elite: true);
        AddGuard(quartersRoot, "EliteGuard3", new Vector2(512, 280), new Vector2[]
        {
            new(-120, 0), new(120, 0)
        }, elite: true);

        // LORD COWL — pacing in his study.
        SpawnLordCowl(quartersRoot);
    }

    // ═════════════════════════════════════════════════════════════
    //  LORD COWL
    // ═════════════════════════════════════════════════════════════

    private void SpawnLordCowl(Node2D parent)
    {
        // Use actual C# types so GetNode<T>/GetOwner<T> work at runtime.
        var cowl = new GuardEnemy { Name = "LordCowl" };
        cowl.Position = new Vector2(512, 160);
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
        patrol.Waypoints = new Vector2[] { new(0, 0), new(100, 0), new(100, 60), new(0, 60) };
        cowl.AddChild(patrol);

        // State machine with guard states.
        var stateMachine = new StateMachine { Name = "StateMachine" };
        cowl.AddChild(stateMachine);
        AddGuardStates(stateMachine);

        // Tag for identification.
        cowl.SetMeta("target_id", "cowl");
        cowl.SetMeta("is_boss", true);

        // Set Owner on all descendants BEFORE AddChild so GetOwner<GuardEnemy>()
        // works in Init() when _Ready fires.
        SetOwnerRecursive(cowl, cowl);
        parent.AddChild(cowl);

        // Wire death to mission complete.
        health.Died += () => OnCowlKilled(cowl);
    }

    private void OnCowlKilled(Node2D cowl)
    {
        GD.Print("═══ LORD COWL SLAIN ═══");
        GD.Print("\"I never saw the dark as empty. I thought it was full of things that loved me.\"");

        // Register the kill with KingdomState.
        var gm = GameManager.Instance;
        string rewardText = "";
        if (gm != null)
        {
            var killed = gm.Kingdoms[0].KillTarget("cowl");

            // Award ink.
            if (killed != null)
            {
                gm.InkInventory?.AddInk(killed.InkDrop, killed.InkAmount);
                rewardText = $"Blood-Ink Acquired: {killed.InkAmount}× {killed.InkDrop} Grade\nNew Tattoo Available: Shadow Step";
                GD.Print($"Blood-Ink acquired: {killed.InkAmount}x {killed.InkDrop} grade!");
            }
        }

        // Populate MissionComplete screen data.
        UI.MissionComplete.TargetText = "Lord Harlan Cowl\nGovernor of the Greenhold\nEdictbearer";
        UI.MissionComplete.WhisperText = "\"I never saw the dark as empty.\nI thought it was full of things that loved me.\"";
        UI.MissionComplete.RewardText = rewardText;

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
}
