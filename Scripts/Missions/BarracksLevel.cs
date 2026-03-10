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
/// Barracks — Captain Thorne's stronghold. A heavily-guarded military fort
/// with a training yard, bunk rooms, and Thorne's trophy room.
/// Three zones: Training Yard → Barracks Interior → Trophy Room.
/// Difficulty: 7 — many guards, tight corridors, hard-hitting elites.
/// </summary>
public partial class BarracksLevel : MissionLevelBase
{
    // ─── Map Layout ──────────────────────────────────────────────

    // Training Yard — open ground with weapon racks, stone border
    private static readonly string[] YardMap = {
        "################################################################",
        "#pppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppp#",
        "#pp,,,,,,pppppppppp,,,,,,,,pppppppppppp,,,,,,,,pppppppppp,,pp#",
        "#pp,,,,,,pppppppppp,,,,,,,,pppppppppppp,,,,,,,,pppppppppp,,pp#",
        "#pp,,,,,,pppppppppp,,,,,,,,pppppppppppp,,,,,,,,pppppppppp,,pp#",
        "#pp,,,,,,pppppppppp,,,,,,,,pppppppppppp,,,,,,,,pppppppppp,,pp#",
        "#pp,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,pp#",
        "#pp,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,pp#",
        "#pp,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,pp#",
        "#pp,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,pp#",
        "#pp,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,pp#",
        "#pp,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,pp#",
        "#pp,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,pp#",
        "#pp,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,pp#",
        "#pp,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,pp#",
        "#pp,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,pp#",
        "#pp,,,,,,pppppppppp,,,,,,,,pppppppppppp,,,,,,,,pppppppppp,,pp#",
        "#pp,,,,,,pppppppppp,,,,,,,,pppppppppppp,,,,,,,,pppppppppp,,pp#",
        "#pp,,,,,,pppppppppp,,,,,,,,pppppppppppp,,,,,,,,pppppppppp,,pp#",
        "#pp,,,,,,pppppppppp,,,,,,,,pppppppppppp,,,,,,,,pppppppppp,,pp#",
        "#pppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppp#",
        "#pppppppppppppppppppppppppp....pppppppppppppppppppppppppppppppp#",
        "################################################################",
    };

    // Barracks Interior — bunks, mess hall, corridors
    private static readonly string[] InteriorMap = {
        "################################################################",
        "#wwwww#......#......#wwwwwwwww#wwwww#......#......#wwwwwwwwww#",
        "#wwwww#......#......#wwwwwwwww#wwwww#......#......#wwwwwwwwww#",
        "#wwwww#......#......#wwwwwwwww#wwwww#......#......#wwwwwwwwww#",
        "#wwwww#......#......#wwwwwwwww#wwwww#......#......#wwwwwwwwww#",
        "#wwwww.............................................wwwwwwwwww#",
        "#wwwww.............................................wwwwwwwwww#",
        "#wwwww.............................................wwwwwwwwww#",
        "#wwwww.............................................wwwwwwwwww#",
        "#wwwww#......#......#wwwwwwwww#wwwww#......#......#wwwwwwwwww#",
        "#wwwww#......#......#wwwwwwwww#wwwww#......#......#wwwwwwwwww#",
        "#wwwww#......#......#wwwwwwwww#wwwww#......#......#wwwwwwwwww#",
        "#wwwww#......#......#wwwwwwwww#wwwww#......#......#wwwwwwwwww#",
        "#wwwww.............................................wwwwwwwwww#",
        "#wwwww.............................................wwwwwwwwww#",
        "#wwwww.............................................wwwwwwwwww#",
        "#wwwww.............................................wwwwwwwwww#",
        "#wwwww#......#......#wwwwwwwww#wwwww#......#......#wwwwwwwwww#",
        "#wwwww#......#......#wwwwwwwww#wwwww#......#......#wwwwwwwwww#",
        "#wwwww#..~~..#..~~..#wwwwwwwww#wwwww#..~~..#..~~..#wwwwwwwwww#",
        "#wwwww#......#......#wwwwwwwww#wwwww#......#......#wwwwwwwwww#",
        "#wwwww#......#......#wwwwwwwww#wwwww#......#......#wwwwwwwwww#",
        "################################################################",
    };

    // Trophy Room — Captain Thorne's private sanctum
    private static readonly string[] TrophyMap = {
        "################################################################",
        "#cccccccccccc#~~~~#cccccccccccc#cccccccccccc#~~~~#cccccccccccc#",
        "#cccccccccccc#~~~~#cccccccccccc#cccccccccccc#~~~~#cccccccccccc#",
        "#cccccccccccc#~~~~#cccccccccccc#cccccccccccc#~~~~#cccccccccccc#",
        "#cccccccccccc#....#cccccccccccc#cccccccccccc#....#cccccccccccc#",
        "#cccccccccccc......cccccccccccc.cccccccccccc......cccccccccccc#",
        "#cccccccccccc......cccccccccccc.cccccccccccc......cccccccccccc#",
        "#cccccccccccc......cccccccccccc.cccccccccccc......cccccccccccc#",
        "#cccccccccccc......cccccccccccc.cccccccccccc......cccccccccccc#",
        "#cccccccccccc#....#cccccccccccc#cccccccccccc#....#cccccccccccc#",
        "#cccccccccccc#~~~~#cccccccccccc#cccccccccccc#~~~~#cccccccccccc#",
        "#cccccccccccc#~~~~#cccccccccccc#cccccccccccc#~~~~#cccccccccccc#",
        "#cccccccccccc#~~~~#cccccccccccc#cccccccccccc#~~~~#cccccccccccc#",
        "################################################################",
    };

    private static readonly Vector2 YardOffset = new(0, 624);
    private static readonly Vector2 InteriorOffset = new(0, 0);
    private static readonly Vector2 TrophyOffset = new(0, -384);

    public override void _Ready()
    {
        PlaceholderSprites.CreateAll();

        BuildYard();
        BuildInterior();
        BuildTrophyRoom();
        SpawnPlayer(YardOffset + new Vector2(512, 320));
        SetupHUD();
        RegisterTargets();

        GD.Print("═══ BARRACKS LOADED ═══");
    }

    // ═════════════════════════════════════════════════════════════
    //  ZONE 1: TRAINING YARD (open, high security, 3 guards)
    // ═════════════════════════════════════════════════════════════

    private void BuildYard()
    {
        var root = new Node2D { Name = "TrainingYard" };
        root.Position = YardOffset;
        AddChild(root);

        MapBuilder.Build(root, YardMap);

        // Area zone — full expanded yard.
        AddAreaZone(root, "Training Yard", new Vector2(512, 184), new Vector2(1024, 368));

        // Shadow behind the equipment racks.
        AddShadowZone(root, new Vector2(56, 56), new Vector2(32, 48));
        AddShadowZone(root, new Vector2(960, 56), new Vector2(32, 48));
        AddShadowZone(root, new Vector2(56, 280), new Vector2(32, 48));
        AddShadowZone(root, new Vector2(960, 280), new Vector2(32, 48));
        AddShadowZone(root, new Vector2(400, 160), new Vector2(32, 32));

        // Hiding spots — weapon racks, crates, training dummies.
        AddHidingSpot(root, "Weapon Rack", new Vector2(80, 120));
        AddHidingSpot(root, "Supply Crates", new Vector2(920, 120));
        AddHidingSpot(root, "Training Dummy", new Vector2(350, 200));
        AddHidingSpot(root, "Equipment Shed", new Vector2(650, 200));
        AddHidingSpot(root, "Hay Bales", new Vector2(200, 300));

        // 5 guards patrolling the open yard — the most dangerous zone.
        AddGuard(root, "YardGuard1", new Vector2(200, 100), new Vector2[]
        {
            new(0, 0), new(180, 0), new(180, 100), new(0, 100)
        });
        AddGuard(root, "YardGuard2", new Vector2(700, 100), new Vector2[]
        {
            new(0, 0), new(-180, 0), new(-180, 100), new(0, 100)
        });
        AddGuard(root, "YardGuard3", new Vector2(512, 260), new Vector2[]
        {
            new(-200, 0), new(200, 0)
        });
        AddGuard(root, "YardGuard4", new Vector2(320, 180), new Vector2[]
        {
            new(0, 0), new(0, 80), new(80, 80), new(80, 0)
        });
        AddGuard(root, "YardGuard5", new Vector2(800, 180), new Vector2[]
        {
            new(0, 0), new(-100, 0), new(-100, -60), new(0, -60)
        });
    }

    // ═════════════════════════════════════════════════════════════
    //  ZONE 2: BARRACKS INTERIOR (tight corridors, 3 guards)
    // ═════════════════════════════════════════════════════════════

    private void BuildInterior()
    {
        var root = new Node2D { Name = "BarracksInterior" };
        root.Position = InteriorOffset;
        AddChild(root);

        MapBuilder.Build(root, InteriorMap);

        // Area zone — full expanded interior.
        AddAreaZone(root, "Barracks", new Vector2(512, 184), new Vector2(1024, 368),
            isRestricted: true);

        // Shadow in bunk corners.
        AddShadowZone(root, new Vector2(120, 312), new Vector2(24, 24));
        AddShadowZone(root, new Vector2(288, 312), new Vector2(24, 24));
        AddShadowZone(root, new Vector2(620, 312), new Vector2(24, 24));
        AddShadowZone(root, new Vector2(788, 312), new Vector2(24, 24));
        AddShadowZone(root, new Vector2(512, 100), new Vector2(32, 24));

        // Hiding spots — under bunks and in supply rooms.
        AddHidingSpot(root, "Under Bunks", new Vector2(80, 80));
        AddHidingSpot(root, "Supply Room", new Vector2(900, 184));
        AddHidingSpot(root, "Mess Table", new Vector2(400, 280));
        AddHidingSpot(root, "Locker Room", new Vector2(700, 80));
        AddHidingSpot(root, "Behind Barrels", new Vector2(250, 184));

        // 5 interior guards — 3 patrol, 2 elite standing.
        AddGuard(root, "BunkGuard1", new Vector2(200, 80), new Vector2[]
        {
            new(0, 0), new(0, 140), new(120, 140), new(120, 0)
        });
        AddGuard(root, "BunkGuard2", new Vector2(700, 80), new Vector2[]
        {
            new(0, 0), new(0, 140), new(-120, 140), new(-120, 0)
        });
        AddGuard(root, "BunkGuard3", new Vector2(512, 260), new Vector2[]
        {
            new(-150, 0), new(150, 0)
        });
        AddGuard(root, "CorridorElite1", new Vector2(350, 100), new Vector2[]
        {
            new(-80, 0), new(80, 0)
        }, elite: true);
        AddGuard(root, "CorridorElite2", new Vector2(750, 260), new Vector2[]
        {
            new(-80, 0), new(80, 0)
        }, elite: true);
    }

    // ═════════════════════════════════════════════════════════════
    //  ZONE 3: TROPHY ROOM (Captain Thorne + 2 elites)
    // ═════════════════════════════════════════════════════════════

    private void BuildTrophyRoom()
    {
        var root = new Node2D { Name = "TrophyRoom" };
        root.Position = TrophyOffset;
        AddChild(root);

        MapBuilder.Build(root, TrophyMap);

        // Area zone — full expanded trophy room.
        AddAreaZone(root, "Trophy Room", new Vector2(512, 112), new Vector2(1024, 224),
            isRestricted: true);

        // Heavy shadow around the trophies.
        AddShadowZone(root, new Vector2(224, 24), new Vector2(64, 32));
        AddShadowZone(root, new Vector2(736, 24), new Vector2(64, 32));
        AddShadowZone(root, new Vector2(224, 176), new Vector2(64, 32));
        AddShadowZone(root, new Vector2(736, 176), new Vector2(64, 32));
        AddShadowZone(root, new Vector2(480, 100), new Vector2(48, 32));

        // Hiding spots — trophy cases and tapestries.
        AddHidingSpot(root, "Trophy Case", new Vector2(80, 112));
        AddHidingSpot(root, "Tapestry", new Vector2(920, 112));
        AddHidingSpot(root, "Weapon Display", new Vector2(350, 40));
        AddHidingSpot(root, "Dark Alcove", new Vector2(700, 180));

        // 3 elite bodyguards — the toughest protection in the game.
        AddGuard(root, "Bodyguard1", new Vector2(280, 112), new Vector2[]
        {
            new(0, -50), new(0, 50)
        }, elite: true);
        AddGuard(root, "Bodyguard2", new Vector2(740, 112), new Vector2[]
        {
            new(0, -50), new(0, 50)
        }, elite: true);
        AddGuard(root, "Bodyguard3", new Vector2(512, 60), new Vector2[]
        {
            new(-100, 0), new(100, 0)
        }, elite: true);

        // Captain Thorne is the big-bad of the Barracks.
        SpawnCaptainThorne(root);
    }

    // ═════════════════════════════════════════════════════════════
    //  CAPTAIN THORNE
    // ═════════════════════════════════════════════════════════════

    private void SpawnCaptainThorne(Node2D parent)
    {
        var thorne = new GuardEnemy { Name = "CaptainThorne" };
        thorne.Position = new Vector2(512, 112);
        thorne.CollisionLayer = 1 << 2;
        thorne.CollisionMask = 1;
        thorne.MoveSpeed = 40f;
        thorne.PatrolSpeed = 25f;
        thorne.AlertedSpeed = 60f;
        thorne.ChaseSpeed = 80f;
        thorne.DetectRange = 150f;
        thorne.AttackRange = 30f;

        var sprite = new AnimatedSprite2D { Name = "AnimatedSprite2D" };
        sprite.SpriteFrames = CreateGuardSpriteFrames("guard");
        thorne.AddChild(sprite);

        var bodyShape = new CollisionShape2D();
        bodyShape.Shape = new RectangleShape2D { Size = new Vector2(12, 16) };
        thorne.AddChild(bodyShape);

        var hurtbox = new Hurtbox { Name = "Hurtbox" };
        hurtbox.CollisionLayer = 0;
        hurtbox.CollisionMask = 1 << 3;
        var hurtShape = new CollisionShape2D { Name = "HurtboxShape" };
        hurtShape.Shape = new RectangleShape2D { Size = new Vector2(14, 16) };
        hurtbox.AddChild(hurtShape);
        thorne.AddChild(hurtbox);

        var hitbox = new Hitbox { Name = "Hitbox" };
        hitbox.CollisionLayer = 1 << 4;
        hitbox.CollisionMask = 0;
        hitbox.Damage = 3;
        hitbox.KnockbackForce = new Vector2(100f, 0f);
        var hitShape = new CollisionShape2D { Name = "HitboxShape" };
        hitShape.Shape = new RectangleShape2D { Size = new Vector2(18, 14) };
        hitbox.AddChild(hitShape);
        thorne.AddChild(hitbox);

        var health = new HealthComponent { Name = "HealthComponent" };
        health.MaxHealth = 8;
        thorne.AddChild(health);

        var sensor = new DetectionSensor { Name = "DetectionSensor" };
        sensor.ViewDistance = 150f;
        sensor.ViewAngle = 80f;
        sensor.CloseDetectRadius = 35f;
        sensor.AwarenessGainRate = 50f;
        thorne.AddChild(sensor);

        var patrol = new PatrolRoute { Name = "PatrolRoute" };
        patrol.Waypoints = new Vector2[] { new(0, 0), new(60, 0), new(60, 30), new(0, 30) };
        thorne.AddChild(patrol);

        var stateMachine = new StateMachine { Name = "StateMachine" };
        thorne.AddChild(stateMachine);
        AddGuardStates(stateMachine);

        thorne.SetMeta("target_id", "thorne");

        SetOwnerRecursive(thorne, thorne);
        parent.AddChild(thorne);

        health.Died += () => OnTargetKilled("thorne", 0,
            "Captain Thorne\nCommander of the Greenguard",
            "\"He went down swinging. They all do. …Why didn't I?\"");
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

    // ═════════════════════════════════════════════════════════════
    //  HELPER: HARDER GUARDS (difficulty 7 — custom elite stats)
    // ═════════════════════════════════════════════════════════════

    /// <summary>
    /// Barracks override: elite guards are faster, hit harder, and see further
    /// than the standard base-class elites (difficulty 7 vs standard 4-5).
    /// </summary>
    protected override void AddGuard(Node2D parent, string name, Vector2 pos, Vector2[] waypoints, bool elite = false)
    {
        var guard = new GuardEnemy { Name = name };
        guard.Position = pos;
        guard.CollisionLayer = 1 << 2;
        guard.CollisionMask = 1;
        guard.PatrolSpeed = elite ? 55f : 40f;
        guard.AlertedSpeed = elite ? 90f : 70f;
        guard.ChaseSpeed = elite ? 110f : 90f;
        guard.DetectRange = elite ? 150f : 100f;
        guard.AttackRange = elite ? 30f : 25f;

        var sprite = new AnimatedSprite2D { Name = "AnimatedSprite2D" };
        sprite.SpriteFrames = CreateGuardSpriteFrames(elite ? "guard_alert" : "guard");
        guard.AddChild(sprite);

        var bodyShape = new CollisionShape2D();
        bodyShape.Shape = new RectangleShape2D { Size = new Vector2(10, 14) };
        guard.AddChild(bodyShape);

        var hurtbox = new Hurtbox { Name = "Hurtbox" };
        hurtbox.CollisionLayer = 0;
        hurtbox.CollisionMask = 1 << 3;
        var hurtShape = new CollisionShape2D { Name = "HurtboxShape" };
        hurtShape.Shape = new RectangleShape2D { Size = new Vector2(12, 14) };
        hurtbox.AddChild(hurtShape);
        guard.AddChild(hurtbox);

        var hitbox = new Hitbox { Name = "Hitbox" };
        hitbox.CollisionLayer = 1 << 4;
        hitbox.CollisionMask = 0;
        hitbox.Damage = elite ? 2 : 1;
        hitbox.KnockbackForce = new Vector2(80f, 0f);
        var hitShape = new CollisionShape2D { Name = "HitboxShape" };
        hitShape.Shape = new RectangleShape2D { Size = new Vector2(16, 12) };
        hitbox.AddChild(hitShape);
        guard.AddChild(hitbox);

        var health = new HealthComponent { Name = "HealthComponent" };
        health.MaxHealth = elite ? 5 : 3;
        guard.AddChild(health);

        var sensor = new DetectionSensor { Name = "DetectionSensor" };
        sensor.ViewDistance = elite ? 150f : 120f;
        sensor.ViewAngle = 55f;
        guard.AddChild(sensor);

        var patrol = new PatrolRoute { Name = "PatrolRoute" };
        patrol.Waypoints = waypoints;
        guard.AddChild(patrol);

        var stateMachine = new StateMachine { Name = "StateMachine" };
        guard.AddChild(stateMachine);
        AddGuardStates(stateMachine);

        SetOwnerRecursive(guard, guard);
        parent.AddChild(guard);
    }
}
