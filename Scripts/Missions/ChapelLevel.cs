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
/// Chapel — Sister Blessing's domain. A walled religious compound with a
/// public nave, a restricted vestry, and a hidden relic chamber below.
/// Three zones: Chapel Grounds → Nave & Vestry → Relic Chamber.
/// Difficulty: 4 — few guards but tight passages underground.
/// </summary>
public partial class ChapelLevel : MissionLevelBase
{
    // ─── Map Layout ──────────────────────────────────────────────

    // Chapel Grounds — walled garden with a stone path to the entrance
    private static readonly string[] GroundsMap = {
        ",,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,",
        ",,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,",
        ",,,,,,,,,,,,,,,,,pppppppppppppp,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,",
        ",,,,,,,,,,,,,,,,pp............pp,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,",
        ",,,,,,,,,,,,,,,pp..............pp,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,",
        ",,,,,,,,,,,,,,pp................pp,,,,,,,,,,,,,,,,,,,,,,,,,,,,,",
        ",,,,,,,,,,,,,pp..................pp,,,,,,,,,,,,,,,,,,,,,,,,,,,,",
        ",,,,,,,,,,,,pp....................pp,,,,,,,,,,,,,,,,,,,,,,,,,,,",
        ",,,,,,,,,,,pp......................pp,,,,,,,,,,,,,,,,,,,,,,,,,,",
        ",,,,,,,,,,pp........................pp,,,,,,,,,,,,,,,,,,,,,,,,,",
        ",,,,,,,,,pp..........................pp,,,,,,,,,,,,,,,,,,,,,,,,",
        ",,,,,,,,pp............................pp,,,,,,,,,,,,,,,,,,,,,,,",
        ",,,,,,,,p..............................p,,,,,,,,,,,,,,,,,,,,,,,",
        ",,,,,,,,p..............................p,,,,,,,,,,,,,,,,,,,,,,,",
        ",,,,,,,pp..............................pp,,,,,,,,,,,,,,,,,,,,,,",
        ",,,,,,,p................................p,,,,,,,,,,,,,,,,,,,,,,",
        ",,,,,,,p................................p,,,,,,,,,,,,,,,,,,,,,,",
        ",,,,,,,pp..............................pp,,,,,,,,,,,,,,,,,,,,,,",
        ",,,,,,,,p..............................p,,,,,,,,,,,,,,,,,,,,,,,",
        ",,,,,,,,p..............................p,,,,,,,,,,,,,,,,,,,,,,,",
        ",,,,,,,,pp............................pp,,,,,,,,,,,,,,,,,,,,,,,",
        ",,,,,,,,,pp..........................pp,,,,,,,,,,,,,,,,,,,,,,,,",
        ",,,,,,,,,,pp........................pp,,,,,,,,,,,,,,,,,,,,,,,,,",
        ",,,,,,,,,,,pp......................pp,,,,,,,,,,,,,,,,,,,,,,,,,,",
        ",,,,,,,,,,,,pp....................pp,,,,,,,,,,,,,,,,,,,,,,,,,,,",
        ",,,,,,,,,,,,,pp..................pp,,,,,,,,,,,,,,,,,,,,,,,,,,,,",
        ",,~,,,,,,,,,,,,pppppp....pppppp,,,,,,,,,,,,,~,,,,,,,,,,,,,,,,",
        ",,,,,,,,,,,,,,,,,,,,,pppp,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,",
        ",,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,",
        ",,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,",
    };

    // Nave & Vestry — interior with carpeted aisle and side rooms
    private static readonly string[] NaveMap = {
        "################################################################",
        "#wwwwww#cccccccccc#wwwwwwww#wwwwww#cccccccccc#wwwwwwww#wwwwwww#",
        "#wwwwww#cccccccccc#wwwwwwww#wwwwww#cccccccccc#wwwwwwww#wwwwwww#",
        "#wwwwww#cccccccccc#wwwwwwww#wwwwww#cccccccccc#wwwwwwww#wwwwwww#",
        "#wwwwww#cccccccccc#wwwwwwww#wwwwww#cccccccccc#wwwwwwww#wwwwwww#",
        "#wwwwww...cccccc...wwwwwwww.wwwwww...cccccc...wwwwwwww.wwwwwww#",
        "#wwwwww...cccccc...wwwwwwww.wwwwww...cccccc...wwwwwwww.wwwwwww#",
        "#wwwwww...cccccc...wwwwwwww.wwwwww...cccccc...wwwwwwww.wwwwwww#",
        "#wwwwww...cccccc...wwwwwwww.wwwwww...cccccc...wwwwwwww.wwwwwww#",
        "#wwwwww#cccccccccc#wwwwwwww#wwwwww#cccccccccc#wwwwwwww#wwwwwww#",
        "#wwwwww#cccccccccc#wwwwwwww#wwwwww#cccccccccc#wwwwwwww#wwwwwww#",
        "#wwwwww#cccccccccc#wwwwwwww#wwwwww#cccccccccc#wwwwwwww#wwwwwww#",
        "#wwwwww#cccccccccc#wwwwwwww#wwwwww#cccccccccc#wwwwwwww#wwwwwww#",
        "#wwwwww#cccccccccc#wwwwwwww#wwwwww#cccccccccc#wwwwwwww#wwwwwww#",
        "#wwwwww#cccccccccc#wwwwwwww#wwwwww#cccccccccc#wwwwwwww#wwwwwww#",
        "#wwwwww#cccccccccc#wwwwwwww#wwwwww#cccccccccc#wwwwwwww#wwwwwww#",
        "#wwwwww#cccccccccc#wwwwwwww#wwwwww#cccccccccc#wwwwwwww#wwwwwww#",
        "#wwwwww#cccccccccc#~~wwwwww#wwwwww#cccccccccc#~~wwwwww#wwwwwww#",
        "#wwwwww#cccccccccc#~~wwwwww#wwwwww#cccccccccc#~~wwwwww#wwwwwww#",
        "#wwwwww#cccccccccc#wwwwwwww#wwwwww#cccccccccc#wwwwwwww#wwwwwww#",
        "#wwwwww#cccccccccc#wwwwwwww#wwwwww#cccccccccc#wwwwwwww#wwwwwww#",
        "#wwwwww#cccccccccc#wwwwwwww#wwwwww#cccccccccc#wwwwwwww#wwwwwww#",
        "#wwwwww#cccccccccc#wwwwwwww#wwwwww#cccccccccc#wwwwwwww#wwwwwww#",
        "################################################################",
    };

    // Relic Chamber — underground, dark, cramped stone corridors
    private static readonly string[] RelicMap = {
        "################################################################",
        "#......~~#........#~~.....#........#~~.....#........#~~........#",
        "#........#........#.......#........#.......#........#..........#",
        "#........#........#.......#........#.......#........#..........#",
        "#........#........#.......#........#.......#........#..........#",
        "#.............##..........#.............##..........#..........#",
        "#.............##..........#.............##..........#..........#",
        "#.............##..........#.............##..........#..........#",
        "#.............##..........#.............##..........#..........#",
        "#........#........#.......#........#.......#........#..........#",
        "#........#........#.......#........#.......#........#..........#",
        "#........#........#.......#........#.......#........#..........#",
        "#........#........#.......#........#.......#........#..........#",
        "#.............##..........#.............##..........#..........#",
        "#.............##..........#.............##..........#..........#",
        "#........#........#.......#........#.......#........#..........#",
        "#........#........#.......#........#.......#........#..........#",
        "#......~~#........#~~.....#........#~~.....#........#~~........#",
        "################################################################",
    };

    private static readonly Vector2 GroundsOffset = new(0, 688);
    private static readonly Vector2 NaveOffset = new(0, 0);
    private static readonly Vector2 RelicOffset = new(0, -400);

    public override void _Ready()
    {
        PlaceholderSprites.CreateAll();

        BuildGrounds();
        BuildNave();
        BuildRelicChamber();
        SpawnPlayer(GroundsOffset + new Vector2(440, 430));
        SetupHUD();
        RegisterTargets();

        GD.Print("═══ CHAPEL LOADED ═══");
    }

    // ═════════════════════════════════════════════════════════════
    //  ZONE 1: CHAPEL GROUNDS (low security, 1 patrol guard)
    // ═════════════════════════════════════════════════════════════

    private void BuildGrounds()
    {
        var root = new Node2D { Name = "ChapelGrounds" };
        root.Position = GroundsOffset;
        AddChild(root);

        MapBuilder.Build(root, GroundsMap);

        // Area zone — full expanded grounds.
        AddAreaZone(root, "Chapel Grounds", new Vector2(512, 240), new Vector2(1024, 480));

        // Shadow at edges and hedgerows.
        AddShadowZone(root, new Vector2(40, 416), new Vector2(40, 40));
        AddShadowZone(root, new Vector2(720, 416), new Vector2(40, 40));
        AddShadowZone(root, new Vector2(200, 100), new Vector2(40, 40));
        AddShadowZone(root, new Vector2(680, 100), new Vector2(40, 40));
        AddShadowZone(root, new Vector2(440, 200), new Vector2(40, 32));

        // Hiding spots — cemetery, graves, bushes.
        AddHidingSpot(root, "Hedgerow", new Vector2(120, 200));
        AddHidingSpot(root, "Gravestone", new Vector2(660, 200));
        AddHidingSpot(root, "Cemetery Tree", new Vector2(300, 140));
        AddHidingSpot(root, "Stone Bench", new Vector2(550, 340));
        AddHidingSpot(root, "Overgrown Arch", new Vector2(440, 100));

        // 3 gate guards — wider patrols across the grounds.
        AddGuard(root, "GateGuard1", new Vector2(350, 160), new Vector2[]
        {
            new(0, 0), new(150, 0), new(150, 120), new(0, 120)
        });
        AddGuard(root, "GateGuard2", new Vector2(550, 280), new Vector2[]
        {
            new(0, 0), new(-120, 0), new(-120, -100), new(0, -100)
        });
        AddGuard(root, "GateGuard3", new Vector2(300, 360), new Vector2[]
        {
            new(-80, 0), new(100, 0)
        });
    }

    // ═════════════════════════════════════════════════════════════
    //  ZONE 2: NAVE & VESTRY (restricted, 2 guards)
    // ═════════════════════════════════════════════════════════════

    private void BuildNave()
    {
        var root = new Node2D { Name = "NaveVestry" };
        root.Position = NaveOffset;
        AddChild(root);

        MapBuilder.Build(root, NaveMap);

        // Area zone — full expanded nave.
        AddAreaZone(root, "Chapel Nave", new Vector2(512, 192), new Vector2(1024, 384),
            isRestricted: true);

        // Shadow zones — dark vestry corners.
        AddShadowZone(root, new Vector2(396, 280), new Vector2(32, 32));
        AddShadowZone(root, new Vector2(900, 280), new Vector2(32, 32));
        AddShadowZone(root, new Vector2(64, 48), new Vector2(40, 32));
        AddShadowZone(root, new Vector2(960, 48), new Vector2(40, 32));

        // Hiding spots — pews, confessionals, behind pillars.
        AddHidingSpot(root, "Confessional", new Vector2(80, 80));
        AddHidingSpot(root, "Behind Altar", new Vector2(400, 40));
        AddHidingSpot(root, "Pew Alcove", new Vector2(700, 160));
        AddHidingSpot(root, "Vestry Closet", new Vector2(920, 80));

        // 4 nave guards — patrols with openings.
        AddGuard(root, "NaveGuard1", new Vector2(300, 80), new Vector2[]
        {
            new(0, 0), new(0, 180)
        });
        AddGuard(root, "NaveGuard2", new Vector2(700, 80), new Vector2[]
        {
            new(0, 0), new(0, 180)
        });
        AddGuard(root, "NaveGuard3", new Vector2(512, 300), new Vector2[]
        {
            new(-180, 0), new(180, 0)
        });
        AddGuard(root, "NaveGuard4", new Vector2(160, 192), new Vector2[]
        {
            new(0, -80), new(0, 80)
        });
    }

    // ═════════════════════════════════════════════════════════════
    //  ZONE 3: RELIC CHAMBER (Sister Blessing + the relic)
    // ═════════════════════════════════════════════════════════════

    private void BuildRelicChamber()
    {
        var root = new Node2D { Name = "RelicChamber" };
        root.Position = RelicOffset;
        AddChild(root);

        MapBuilder.Build(root, RelicMap);

        // Area zone — full expanded relic chamber.
        AddAreaZone(root, "Relic Chamber", new Vector2(512, 152), new Vector2(1024, 304),
            isRestricted: true);

        // Heavy shadow underground — many dark alcoves.
        AddShadowZone(root, new Vector2(120, 24), new Vector2(32, 24));
        AddShadowZone(root, new Vector2(120, 280), new Vector2(32, 24));
        AddShadowZone(root, new Vector2(520, 24), new Vector2(32, 24));
        AddShadowZone(root, new Vector2(520, 280), new Vector2(32, 24));
        AddShadowZone(root, new Vector2(900, 24), new Vector2(32, 24));
        AddShadowZone(root, new Vector2(900, 280), new Vector2(32, 24));

        // Hiding spots — sarcophagi and rubble piles.
        AddHidingSpot(root, "Sarcophagus", new Vector2(80, 100));
        AddHidingSpot(root, "Rubble Nook", new Vector2(400, 60));
        AddHidingSpot(root, "Stone Column", new Vector2(700, 200));
        AddHidingSpot(root, "Collapsed Wall", new Vector2(950, 100));

        // 2 elite guards protecting the relic.
        AddGuard(root, "RelicGuard1", new Vector2(350, 100), new Vector2[]
        {
            new(0, -40), new(0, 80), new(100, 80), new(100, -40)
        }, elite: true);
        AddGuard(root, "RelicGuard2", new Vector2(700, 100), new Vector2[]
        {
            new(0, -40), new(0, 80)
        }, elite: true);

        // Sister Blessing — praying near the relic in the center.
        SpawnSisterBlessing(root);
    }

    // ═════════════════════════════════════════════════════════════
    //  SISTER BLESSING
    // ═════════════════════════════════════════════════════════════

    private void SpawnSisterBlessing(Node2D parent)
    {
        var blessing = new GuardEnemy { Name = "SisterBlessing" };
        blessing.Position = new Vector2(512, 152);
        blessing.CollisionLayer = 1 << 2;
        blessing.CollisionMask = 1;
        blessing.MoveSpeed = 25f;
        blessing.PatrolSpeed = 15f;
        blessing.AlertedSpeed = 35f;
        blessing.ChaseSpeed = 45f;
        blessing.DetectRange = 90f;
        blessing.AttackRange = 20f;

        var sprite = new AnimatedSprite2D { Name = "AnimatedSprite2D" };
        sprite.SpriteFrames = CreateGuardSpriteFrames("guard");
        blessing.AddChild(sprite);

        var bodyShape = new CollisionShape2D();
        bodyShape.Shape = new RectangleShape2D { Size = new Vector2(10, 14) };
        blessing.AddChild(bodyShape);

        var hurtbox = new Hurtbox { Name = "Hurtbox" };
        hurtbox.CollisionLayer = 0;
        hurtbox.CollisionMask = 1 << 3;
        var hurtShape = new CollisionShape2D { Name = "HurtboxShape" };
        hurtShape.Shape = new RectangleShape2D { Size = new Vector2(12, 14) };
        hurtbox.AddChild(hurtShape);
        blessing.AddChild(hurtbox);

        var hitbox = new Hitbox { Name = "Hitbox" };
        hitbox.CollisionLayer = 1 << 4;
        hitbox.CollisionMask = 0;
        hitbox.Damage = 1;
        hitbox.KnockbackForce = new Vector2(40f, 0f);
        var hitShape = new CollisionShape2D { Name = "HitboxShape" };
        hitShape.Shape = new RectangleShape2D { Size = new Vector2(12, 10) };
        hitbox.AddChild(hitShape);
        blessing.AddChild(hitbox);

        var health = new HealthComponent { Name = "HealthComponent" };
        health.MaxHealth = 4;
        blessing.AddChild(health);

        var sensor = new DetectionSensor { Name = "DetectionSensor" };
        sensor.ViewDistance = 90f;
        sensor.ViewAngle = 70f;
        sensor.CloseDetectRadius = 25f;
        sensor.AwarenessGainRate = 35f;
        blessing.AddChild(sensor);

        var patrol = new PatrolRoute { Name = "PatrolRoute" };
        patrol.Waypoints = new Vector2[] { new(0, 0), new(60, 0), new(60, -30), new(0, -30) };
        blessing.AddChild(patrol);

        var stateMachine = new StateMachine { Name = "StateMachine" };
        blessing.AddChild(stateMachine);
        AddGuardStates(stateMachine);

        blessing.SetMeta("target_id", "blessing");

        SetOwnerRecursive(blessing, blessing);
        parent.AddChild(blessing);

        health.Died += () => OnTargetKilled("blessing", 0,
            "Sister Blessing\nHead of the Greenhold Chapel",
            "\"We prayed for them too. I want you to know that.\"");
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
