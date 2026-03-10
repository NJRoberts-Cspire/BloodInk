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
/// Labor Camp — Reeve Maren's domain. An orc labor camp on the edges of the
/// Greenhold where captives toil in quarry tunnels.
/// Three zones: Quarry Yard → Tunnel Passage → Maren's Office.
/// Difficulty: 5 — moderate guard presence, tight corridors in tunnels.
/// </summary>
public partial class LaborCampLevel : MissionLevelBase
{
    // ─── Map Layout ──────────────────────────────────────────────

    // Quarry Yard — open area with tool shacks, wagons, shadow edges
    private static readonly string[] YardMap = {
        ",,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,",
        ",,~,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,~,,,,",
        ",,,,,,,,,,,,,,,pppppppp,,,,,,,,pppppppp,,,,,,,,,,,,,,,,,,,,,,,,,",
        ",,,,,,,,,,,,,,pp......pp,,,,pp..........pp,,,,,,,,,,,,,,,,,,,,,,",
        ",,,,,,,,,,,,,p..........p,,p..............p,,,,,,,,,,,,,,,,,,,,,",
        ",,,,,,,,,,,,p............pp................p,,,,,,,,,,,,,,,,,,,,",
        ",,,,,,,,,,,p..............................p,,,,,,,,,,,,,,,,,,,,,",
        ",,,,,,,,,,p................................p,,,,,,,,,,,,,,,,,,,,",
        ",,,,,,,,,p..................................p,,,,,,,,,,,,,,,,,,,",
        ",,,,,,,,p....................................p,,,,,,,,,,,,,,,,,,",
        ",,,,,,,,p....................................p,,,,,,,,,,,,,,,,,,",
        ",,,,,,,p......................................p,,,,,,,,,,,,,,,,,",
        ",,,,,,,p......................................p,,,,,,,,,,,,,,,,,",
        ",,,,,,p........................................p,,,,,,,,,,,,,,,,",
        ",,,,,,p........................................p,,,,,,,,,,,,,,,,",
        ",,,,,,,p......................................p,,,,,,,,,,,,,,,,,",
        ",,,,,,,p......................................p,,,,,,,,,,,,,,,,,",
        ",,,,,,,,p....................................p,,,,,,,,,,,,,,,,,,",
        ",,,,,,,,p....................................p,,,,,,,,,,,,,,,,,,",
        ",,,,,,,,,p..................................p,,,,,,,,,,,,,,,,,,,",
        ",,,,,,,,,,p................................p,,,,,,,,,,,,,,,,,,,,",
        ",,,,,,,,,,,p..............................p,,,,,,,,,,,,,,,,,,,,,",
        ",,,,,,,,,,,,p............pp................p,,,,,,,,,,,,,,,,,,,,",
        ",,,,,,,,,,,,,p..........p,,p..............p,,,,,,,,,,,,,,,,,,,,,",
        ",,,,,,,,,,,,,,pp......pp,,,,pp..........pp,,,,,,,,,,,,,,,,,,,,,,",
        ",,,,,,,,,,,,,,,pppppppp,,,,,,,,pppppppp,,,,,,,,,,,,,,,,,,,,,,,,,",
        ",,~,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,~,,,,",
        ",,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,",
    };

    // Tunnel Passage — narrow corridors with hiding alcoves
    private static readonly string[] TunnelMap = {
        "################################################################",
        "#......~~#......#~~.........#......#~~.........#......~~........#",
        "#........#......#..........#........#..........#...............#",
        "#........#......#..........#........#..........#...............#",
        "#........#......#..........#........#..........#...............#",
        "#..............#...........#.................#.................#",
        "#..............#...........#.................#.................#",
        "#..............#...........#.................#.................#",
        "#..............#...........#.................#.................#",
        "#........#.....#..........#........#..........#...............#",
        "#........#.....#..........#........#..........#...............#",
        "#........#.....#..........#........#..........#...............#",
        "#........#.....#..........#........#..........#...............#",
        "#..............#...........#.................#.................#",
        "#..............#...........#.................#.................#",
        "#..............#...........#.................#.................#",
        "#..............#...........#.................#.................#",
        "#........#.....#..........#........#..........#...............#",
        "#........#.....#..........#........#..........#...............#",
        "#........#.....#..........#........#..........#...............#",
        "#......~~#.....#~~.........#......~~#~~.........#......~~......#",
        "################################################################",
    };

    // Maren's Office — walled compound with study and side rooms
    private static readonly string[] OfficeMap = {
        "################################################################",
        "#wwwwwwww#cccccccc#wwwwwwwwwwww#wwwwwwww#cccccccc#wwwwwwwwwwww#",
        "#wwwwwwww#cccccccc#wwwwwwwwwwww#wwwwwwww#cccccccc#wwwwwwwwwwww#",
        "#wwwwwwww#cccccccc#wwwwwwwwwwww#wwwwwwww#cccccccc#wwwwwwwwwwww#",
        "#wwwwwwww....cc....wwwwwwwwwwww#wwwwwwww....cc....wwwwwwwwwwww#",
        "#wwwwwwww....cc....wwwwwwwwwwww.wwwwwwww....cc....wwwwwwwwwwww#",
        "#wwwwwwww....cc....wwwwwwwwwwww.wwwwwwww....cc....wwwwwwwwwwww#",
        "#wwwwwwww....cc....wwwwwwwwwwww#wwwwwwww....cc....wwwwwwwwwwww#",
        "#wwwwwwww#cccccccc#wwwwwwwwwwww#wwwwwwww#cccccccc#wwwwwwwwwwww#",
        "#wwwwwwww#cccccccc#wwwwwwwwwwww#wwwwwwww#cccccccc#wwwwwwwwwwww#",
        "#wwwwwwww#cccccccc#wwwwwwwwwwww#wwwwwwww#cccccccc#wwwwwwwwwwww#",
        "#wwwwwwww#cccccccc#wwwwwwwwwwww#wwwwwwww#cccccccc#wwwwwwwwwwww#",
        "#wwwwwwww#cccccccc#wwwwwwwwwwww#wwwwwwww#cccccccc#wwwwwwwwwwww#",
        "#wwwwwwww#cccccccc#~~wwwwwwwwww#wwwwwwww#cccccccc#~~wwwwwwwwww#",
        "#wwwwwwww#cccccccc#~~wwwwwwwwww#wwwwwwww#cccccccc#~~wwwwwwwwww#",
        "################################################################",
    };

    // Zone offsets
    private static readonly Vector2 YardOffset = new(0, 608);
    private static readonly Vector2 TunnelOffset = new(0, 0);
    private static readonly Vector2 OfficeOffset = new(0, -368);

    public override void _Ready()
    {
        PlaceholderSprites.CreateAll();

        BuildYard();
        BuildTunnels();
        BuildOffice();
        SpawnPlayer(YardOffset + new Vector2(512, 380));
        SetupHUD();
        RegisterTargets();

        GD.Print("═══ LABOR CAMP LOADED ═══");
    }

    // ═════════════════════════════════════════════════════════════
    //  ZONE 1: QUARRY YARD (entry, open area, 2 patrol guards)
    // ═════════════════════════════════════════════════════════════

    private void BuildYard()
    {
        var root = new Node2D { Name = "QuarryYard" };
        root.Position = YardOffset;
        AddChild(root);

        MapBuilder.Build(root, YardMap);

        // Area zone — full expanded yard.
        AddAreaZone(root, "Quarry Yard", new Vector2(512, 224), new Vector2(1024, 448));

        // Shadow zones in the quarry corners and edges.
        AddShadowZone(root, new Vector2(40, 24), new Vector2(48, 40));
        AddShadowZone(root, new Vector2(960, 24), new Vector2(48, 40));
        AddShadowZone(root, new Vector2(40, 420), new Vector2(48, 40));
        AddShadowZone(root, new Vector2(960, 420), new Vector2(48, 40));
        AddShadowZone(root, new Vector2(300, 140), new Vector2(40, 32));
        AddShadowZone(root, new Vector2(700, 140), new Vector2(40, 32));

        // Hiding spots — tool shacks, wagons, crates.
        AddHidingSpot(root, "Tool Shack", new Vector2(120, 200));
        AddHidingSpot(root, "Supply Wagon", new Vector2(880, 200));
        AddHidingSpot(root, "Rock Pile", new Vector2(350, 100));
        AddHidingSpot(root, "Ore Cart", new Vector2(650, 100));
        AddHidingSpot(root, "Crate Stack", new Vector2(512, 320));

        // 4 yard patrol guards — wider patrols across the expanded yard.
        AddGuard(root, "YardGuard1", new Vector2(300, 140), new Vector2[]
        {
            new(0, 0), new(200, 0), new(200, 120), new(0, 120)
        });
        AddGuard(root, "YardGuard2", new Vector2(700, 140), new Vector2[]
        {
            new(0, 0), new(-200, 0), new(-200, 120), new(0, 120)
        });
        AddGuard(root, "YardGuard3", new Vector2(512, 280), new Vector2[]
        {
            new(-150, 0), new(150, 0)
        });
        AddGuard(root, "YardGuard4", new Vector2(200, 320), new Vector2[]
        {
            new(0, 0), new(100, 0), new(100, 80), new(0, 80)
        });
    }

    // ═════════════════════════════════════════════════════════════
    //  ZONE 2: TUNNEL PASSAGE (tight, 2 guards, hiding alcoves)
    // ═════════════════════════════════════════════════════════════

    private void BuildTunnels()
    {
        var root = new Node2D { Name = "TunnelPassage" };
        root.Position = TunnelOffset;
        AddChild(root);

        MapBuilder.Build(root, TunnelMap);

        // Area zone — spans the full expanded tunnel network.
        AddAreaZone(root, "Mining Tunnels", new Vector2(512, 176), new Vector2(1024, 352),
            isRestricted: true);

        // Shadow alcoves for stealth throughout the tunnels.
        AddShadowZone(root, new Vector2(120, 24), new Vector2(32, 24));
        AddShadowZone(root, new Vector2(120, 320), new Vector2(32, 24));
        AddShadowZone(root, new Vector2(520, 24), new Vector2(32, 24));
        AddShadowZone(root, new Vector2(520, 320), new Vector2(32, 24));
        AddShadowZone(root, new Vector2(900, 24), new Vector2(32, 24));
        AddShadowZone(root, new Vector2(900, 320), new Vector2(32, 24));

        // Hiding spots — rubble and support beams throughout.
        AddHidingSpot(root, "Rubble Pile", new Vector2(256, 176));
        AddHidingSpot(root, "Support Beam", new Vector2(600, 80));
        AddHidingSpot(root, "Collapsed Tunnel", new Vector2(850, 176));
        AddHidingSpot(root, "Mine Cart", new Vector2(400, 280));

        // 4 tunnel guards — patrols in the corridors leave windows for sneaking.
        AddGuard(root, "TunnelGuard1", new Vector2(200, 80), new Vector2[]
        {
            new(0, 0), new(0, 160)
        });
        AddGuard(root, "TunnelGuard2", new Vector2(450, 120), new Vector2[]
        {
            new(0, 0), new(0, -60), new(80, -60), new(80, 0)
        });
        AddGuard(root, "TunnelGuard3", new Vector2(700, 80), new Vector2[]
        {
            new(0, 0), new(0, 160)
        });
        AddGuard(root, "TunnelGuard4", new Vector2(900, 200), new Vector2[]
        {
            new(0, 0), new(-100, 0), new(-100, 80), new(0, 80)
        });
    }

    // ═════════════════════════════════════════════════════════════
    //  ZONE 3: MAREN'S OFFICE (the target + 2 elite guards)
    // ═════════════════════════════════════════════════════════════

    private void BuildOffice()
    {
        var root = new Node2D { Name = "MarenOffice" };
        root.Position = OfficeOffset;
        AddChild(root);

        MapBuilder.Build(root, OfficeMap);

        // Area zone — covers expanded office compound.
        AddAreaZone(root, "Reeve Maren's Office", new Vector2(512, 128),
            new Vector2(1024, 256), isRestricted: true);

        // Shadow in corner storage rooms.
        AddShadowZone(root, new Vector2(430, 216), new Vector2(32, 32));
        AddShadowZone(root, new Vector2(940, 216), new Vector2(32, 32));
        AddShadowZone(root, new Vector2(64, 32), new Vector2(40, 32));

        // Hiding spots — filing cabinets and desk alcoves.
        AddHidingSpot(root, "Filing Cabinet", new Vector2(80, 64));
        AddHidingSpot(root, "Under Desk", new Vector2(600, 64));
        AddHidingSpot(root, "Storage Closet", new Vector2(880, 128));

        // 2 elite guards — one at each wing entrance.
        AddGuard(root, "OfficeElite1", new Vector2(300, 100), new Vector2[]
        {
            new(0, -40), new(0, 60)
        }, elite: true);
        AddGuard(root, "OfficeElite2", new Vector2(720, 100), new Vector2[]
        {
            new(0, -40), new(0, 60)
        }, elite: true);

        // Reeve Maren — seated in the central carpeted study.
        SpawnReeveMaren(root);
    }

    // ═════════════════════════════════════════════════════════════
    //  REEVE MAREN
    // ═════════════════════════════════════════════════════════════

    private void SpawnReeveMaren(Node2D parent)
    {
        var maren = new GuardEnemy { Name = "ReeveMaren" };
        maren.Position = new Vector2(512, 128);
        maren.CollisionLayer = 1 << 2;
        maren.CollisionMask = 1;
        maren.MoveSpeed = 30f;
        maren.PatrolSpeed = 20f;
        maren.AlertedSpeed = 45f;
        maren.ChaseSpeed = 55f;
        maren.DetectRange = 110f;
        maren.AttackRange = 25f;

        // Sprite.
        var sprite = new AnimatedSprite2D { Name = "AnimatedSprite2D" };
        sprite.SpriteFrames = CreateGuardSpriteFrames("guard");
        maren.AddChild(sprite);

        // Body collision.
        var bodyShape = new CollisionShape2D();
        bodyShape.Shape = new RectangleShape2D { Size = new Vector2(10, 14) };
        maren.AddChild(bodyShape);

        // Hurtbox.
        var hurtbox = new Hurtbox { Name = "Hurtbox" };
        hurtbox.CollisionLayer = 0;
        hurtbox.CollisionMask = 1 << 3;
        var hurtShape = new CollisionShape2D { Name = "HurtboxShape" };
        hurtShape.Shape = new RectangleShape2D { Size = new Vector2(12, 14) };
        hurtbox.AddChild(hurtShape);
        maren.AddChild(hurtbox);

        // Hitbox.
        var hitbox = new Hitbox { Name = "Hitbox" };
        hitbox.CollisionLayer = 1 << 4;
        hitbox.CollisionMask = 0;
        hitbox.Damage = 1;
        hitbox.KnockbackForce = new Vector2(60f, 0f);
        var hitShape = new CollisionShape2D { Name = "HitboxShape" };
        hitShape.Shape = new RectangleShape2D { Size = new Vector2(14, 12) };
        hitbox.AddChild(hitShape);
        maren.AddChild(hitbox);

        // Health (difficulty 5: moderate HP).
        var health = new HealthComponent { Name = "HealthComponent" };
        health.MaxHealth = 5;
        maren.AddChild(health);

        // Detection.
        var sensor = new DetectionSensor { Name = "DetectionSensor" };
        sensor.ViewDistance = 110f;
        sensor.ViewAngle = 60f;
        sensor.CloseDetectRadius = 30f;
        sensor.AwarenessGainRate = 40f;
        maren.AddChild(sensor);

        // Patrol — pacing behind his desk.
        var patrol = new PatrolRoute { Name = "PatrolRoute" };
        patrol.Waypoints = new Vector2[] { new(0, 0), new(80, 0), new(80, 50), new(0, 50) };
        maren.AddChild(patrol);

        // State machine.
        var stateMachine = new StateMachine { Name = "StateMachine" };
        maren.AddChild(stateMachine);
        AddGuardStates(stateMachine);

        maren.SetMeta("target_id", "maren");

        SetOwnerRecursive(maren, maren);
        parent.AddChild(maren);

        health.Died += () => OnTargetKilled("maren", 0,
            "Reeve Maren\nOverseer of the Labor Camps",
            "\"The dogs didn't bark. That's what I can't understand.\"");
    }

    // ═════════════════════════════════════════════════════════════
    //  TARGETS
    // ═════════════════════════════════════════════════════════════

    private void RegisterTargets()
    {
        var gm = GameManager.Instance;
        if (gm == null) return;
        foreach (var target in GreenholdTargets.GetAll())
            gm.Kingdoms[0].RegisterTarget(target);
    }

}
