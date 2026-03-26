using Godot;
using BloodInk.Content;
using BloodInk.Core;
using BloodInk.Interaction;
using BloodInk.Stealth;
using BloodInk.Tools;

namespace BloodInk.Missions;

/// <summary>
/// Rootwarden Farm — Silas Rootwarden's property. He runs an underground orc-fighting
/// ring in his barn. Three zones: Farm Yard (entry) → Barn Approach → Fighting Pit.
/// Difficulty: 4 — mixed guards and spectators, tight corridors in barn. Optional target.
/// </summary>
public partial class RootwardenFarmLevel : MissionLevelBase
{
    // ─── Map Layout ──────────────────────────────────────────────
    // . = dirt floor, # = wood/stone wall, ~ = shadow, , = grass/mud
    // w = wooden planks, p = packed earth path, b = hay bale (cover)

    // Farm Yard — fields, well, tool sheds, animal pens, muddy paths
    private static readonly string[] YardMap = {
        ",,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,",
        ",,~,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,~,,",
        ",,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,",
        "#################################################################################",
        "#.......................................................................................#",
        "#...pppp....................................................................................................pppp...#",
        "#...pppp....................................................................................................pppp...#",
        "#...pppp......~~~~~~.............................................~~~~~~......pppp...#",
        "#...pppp......~~~~~~.............................................~~~~~~......pppp...#",
        "#...pppp....................................................................................................pppp...#",
        "#............................................................................................................#",
        "#...............................................................................................................#",
        "#............................................................................................................#",
        "#...pppp....................................................................................................pppp...#",
        "#...pppp......~~~~~~.............................................~~~~~~......pppp...#",
        "#...pppp......~~~~~~.............................................~~~~~~......pppp...#",
        "#...pppp....................................................................................................pppp...#",
        "#.......................................................................................#",
        "#################################################################################",
        ",,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,",
        ",,~,,,,,,,,,,,,,,,,,,,~,,,,,,,,,,,,,,,,,,~,,,,,,,,,,,,,,,,,,,~,,,,,,,,,,,,,,,,,~,,,,",
    };

    // Barn Approach — exterior of the barn, hay bales, fence, guard post, torch light
    private static readonly string[] BarnApproachMap = {
        "#################################################################################",
        "#wwwwwwwww#........#wwwwwwwww#..........#wwwwwwwww#........#wwwwwwwww#..........#",
        "#wwwwwwwww#........#wwwwwwwww#..........#wwwwwwwww#........#wwwwwwwww#..........#",
        "#wwwwwwwww#........#wwwwwwwww#..........#wwwwwwwww#........#wwwwwwwww#..........#",
        "#wwwwwwwww....pp....wwwwwwwww#....pp....#wwwwwwwww....pp....wwwwwwwww#....pp....#",
        "#wwwwwwwww....pp....wwwwwwwww#....pp....#wwwwwwwww....pp....wwwwwwwww#....pp....#",
        "#wwwwwwwww#........#wwwwwwwww#..........#wwwwwwwww#........#wwwwwwwww#..........#",
        "#wwwwwwwww#........#wwwwwwwww#..........#wwwwwwwww#........#wwwwwwwww#..........#",
        "#......#..~~....#........#..........#........#....~~..#........#........#..~~....#",
        "#......#........#........#..........#........#........#........#........#........#",
        "#...........................................................................#",
        "#...........................................................................#",
        "#...........................................................................#",
        "#...........................................................................#",
        "#......#........#........#..........#........#........#........#........#",
        "#......#..~~....#........#..........#........#....~~..#........#........#",
        "#wwwwwwwww#........#wwwwwwwww#..........#wwwwwwwww#........#wwwwwwwww#..........#",
        "#wwwwwwwww#........#wwwwwwwww#..........#wwwwwwwww#........#wwwwwwwww#..........#",
        "#wwwwwwwww....pp....wwwwwwwww#....pp....#wwwwwwwww....pp....wwwwwwwww#....pp....#",
        "#wwwwwwwww....pp....wwwwwwwww#....pp....#wwwwwwwww....pp....wwwwwwwww#....pp....#",
        "#wwwwwwwww#........#wwwwwwwww#..........#wwwwwwwww#........#wwwwwwwww#..........#",
        "#################################################################################",
    };

    // Fighting Pit — inside the barn. Pit in center, tiered seating, cages along walls
    private static readonly string[] PitMap = {
        "################################################################################",
        "#wwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwww#",
        "#wwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwww#",
        "#wwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwww#",
        "#wwwwwww#...................................................................#wwwwwww#",
        "#wwwwwww#...................................................................#wwwwwww#",
        "#wwwwwww#...................................................................#wwwwwww#",
        "#wwwwwww#...................................................................#wwwwwww#",
        "#wwwwwww#.....................~~....~~....~~.........................#wwwwwww#",
        "#wwwwwww#.....................~~....~~....~~.........................#wwwwwww#",
        "#wwwwwww#...................................................................#wwwwwww#",
        "#wwwwwww#...................................................................#wwwwwww#",
        "#wwwwwww#...................................................................#wwwwwww#",
        "#wwwwwww#...................................................................#wwwwwww#",
        "#wwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwww#",
        "#wwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwww#",
        "#wwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwww#",
        "################################################################################",
    };

    // ─── Root Cellar — fourth zone ────────────────────────────────
    // Below the fighting pit: Rootwarden's private cellar where he keeps the
    // orc fighting contracts, his cut of the prize money, and escape tunnels.
    // Unique mechanic: CROWD NOISE. A noise pulse fires every ~8 seconds when
    // the crowd cheers — during this window, guard detection range is halved
    // and the player can move freely without risking alert. A visible "pulse"
    // of dark color washes over the screen to cue the player.
    private static readonly string[] CellarMap = {
        "################################################################################",
        "#..........#wwwwwwww#..........#wwwwwwww#..........#wwwwwwww#..........#wwwwww#",
        "#..........#wwwwwwww#..........#wwwwwwww#..........#wwwwwwww#..........#wwwwww#",
        "#..........#........#..........#........#..........#........#..........#......#",
        "#..~~......#........#..........#........#..........#........#..~~......#......#",
        "#..~~......................................................................#......#",
        "#..........................................................................#......#",
        "#..........................................................................#......#",
        "#########..........####...####...####...####...####...####...####..#######",
        "#########..........####...####...####...####...####...####...####..#######",
        "#..........................................................................#......#",
        "#..........................................................................#......#",
        "#..~~......................................................................#......#",
        "#..~~......#........#..........#........#..........#........#..~~......#......#",
        "#..........#........#..........#........#..........#........#..........#......#",
        "#..........#wwwwwwww#..........#wwwwwwww#..........#wwwwwwww#..........#wwwwww#",
        "#..........#wwwwwwww#..........#wwwwwwww#..........#wwwwwwww#..........#wwwwww#",
        "################################################################################",
    };

    private static readonly Vector2 YardOffset     = new(0, 1120);
    private static readonly Vector2 ApproachOffset = new(0, 0);
    private static readonly Vector2 PitOffset      = new(0, -720);
    private static readonly Vector2 CellarOffset   = new(0, -1310); // Below the pit

    // ─── Crowd Noise State ────────────────────────────────────────
    // When _crowdNoisActive is true, guard detection range is halved.
    private bool _crowdNoiseActive;
    private readonly System.Collections.Generic.List<Enemies.GuardEnemy> _allGuards = new();

    public override void _Ready()
    {
        PlaceholderSprites.CreateAll();

        BuildFarmYard();
        BuildBarnApproach();
        BuildFightingPit();
        BuildRootCellar();

        SpawnPlayer(YardOffset + new Vector2(640, 180));
        SetupHUD();
        SetupCheckpointRespawn();

        // Checkpoint 1 — player enters Barn Approach zone.
        AddCheckpoint(this, 1, new Vector2(640, 50), 1280f, new Vector2(640, 200));

        // Checkpoint 2 — player enters the Fighting Pit zone.
        AddCheckpoint(this, 2, new Vector2(640, -700), 1280f, new Vector2(640, -600));

        // Checkpoint 3 — player descends into Rootwarden's Root Cellar.
        AddCheckpoint(this, 3, new Vector2(640, -1290), 1280f, new Vector2(640, -1180));

        // Expand camera bounds to include root cellar.
        SetCameraLimits(0, -1670, 1280, 1500);

        GD.Print("═══ ROOTWARDEN FARM LOADED ═══");
        GD.Print("  Target: Silas Rootwarden — farm owner & fighting ring operator.");
        GD.Print("  Optional target. Difficulty 4. Four zones: Yard → Approach → Pit → Cellar.");
        GD.Print("  MECHANIC: Crowd noise pulses every ~8 s — guard detection halved during cheers.");
    }

    // ─── Zone Builders ────────────────────────────────────────────

    private void BuildFarmYard()
    {
        var zone = new Node2D { Name = "FarmYardZone" };
        zone.Position = YardOffset;
        AddChild(zone);

        MapBuilder.Build(zone, YardMap);
        AddAreaZone(zone, "Farm Yard", new Vector2(640, 280), new Vector2(1280, 400));

        // Shadow — hedgerow corners, tool shed back
        AddShadowZone(zone, new Vector2(60, 200), new Vector2(80, 220));
        AddShadowZone(zone, new Vector2(1220, 200), new Vector2(80, 220));
        AddShadowZone(zone, new Vector2(640, 60), new Vector2(300, 80));

        // Hiding spot — tool shed, hay bale
        AddHidingSpot(zone, "Tool Shed", new Vector2(160, 230));
        AddHidingSpot(zone, "Hay Bale", new Vector2(640, 240));

        // Two field hands — unarmed, will panic and flee not fight
        AddGuard(zone, "FarmHand1", new Vector2(400, 220),
            new[] { new Vector2(200, 220), new Vector2(600, 220) });
        AddGuard(zone, "FarmHand2", new Vector2(900, 220),
            new[] { new Vector2(700, 220), new Vector2(1100, 220) });
    }

    private void BuildBarnApproach()
    {
        var zone = new Node2D { Name = "BarnApproachZone" };
        zone.Position = ApproachOffset;
        AddChild(zone);

        MapBuilder.Build(zone, BarnApproachMap);
        AddAreaZone(zone, "Barn Approach", new Vector2(640, 300), new Vector2(1280, 400));
        AddAreaZone(zone, "Guard Post", new Vector2(640, 100), new Vector2(200, 160), isRestricted: true);

        // Shadow — fence corners, between bales
        AddShadowZone(zone, new Vector2(100, 200), new Vector2(100, 200));
        AddShadowZone(zone, new Vector2(1180, 200), new Vector2(100, 200));
        AddShadowZone(zone, new Vector2(400, 300), new Vector2(80, 100));
        AddShadowZone(zone, new Vector2(880, 300), new Vector2(80, 100));

        // Hiding — hay bale stack, cart
        AddHidingSpot(zone, "Hay Stack", new Vector2(200, 280));
        AddHidingSpot(zone, "Feed Cart", new Vector2(640, 320));

        // Barn entrance key in guard post chest
        AddKeyChest(zone, "Guard Post Chest", new Vector2(640, 100), "barn_key", "Barn Key");

        // Locked barn door
        AddLockedDoor(zone, "Barn Door", new Vector2(640, 170), "barn_key");

        // Guards — two sentries at barn entrance
        AddGuard(zone, "BarnSentry1", new Vector2(500, 250),
            new[] { new Vector2(300, 250), new Vector2(640, 170) });
        AddGuard(zone, "BarnSentry2", new Vector2(780, 250),
            new[] { new Vector2(640, 170), new Vector2(980, 250) });
        AddGuard(zone, "PatrolGuard", new Vector2(200, 350),
            new[] { new Vector2(100, 350), new Vector2(1180, 350), new Vector2(640, 200) });
    }

    private void BuildFightingPit()
    {
        var zone = new Node2D { Name = "FightingPitZone" };
        zone.Position = PitOffset;
        AddChild(zone);

        MapBuilder.Build(zone, PitMap);
        AddAreaZone(zone, "Fighting Pit", new Vector2(640, 250), new Vector2(1280, 400));
        AddAreaZone(zone, "Viewing Gallery", new Vector2(640, 80), new Vector2(1000, 160), isRestricted: true);
        AddAreaZone(zone, "Orc Cages", new Vector2(100, 250), new Vector2(160, 300), isRestricted: false);

        // Shadow — under bleachers, behind cages
        AddShadowZone(zone, new Vector2(80, 200), new Vector2(120, 220));
        AddShadowZone(zone, new Vector2(1200, 200), new Vector2(120, 220));
        AddShadowZone(zone, new Vector2(640, 350), new Vector2(200, 80));

        // Hiding — under bleachers, cage shadow
        AddHidingSpot(zone, "Bleacher Shadow", new Vector2(100, 200));
        AddHidingSpot(zone, "Cage Corner", new Vector2(1190, 200));

        // Elite guard — pit enforcer, stays near Rootwarden
        AddGuard(zone, "PitEnforcer", new Vector2(700, 250),
            new[] { new Vector2(600, 250), new Vector2(800, 250) }, elite: true);
        AddGuard(zone, "PitGuard1", new Vector2(350, 250),
            new[] { new Vector2(200, 200), new Vector2(500, 300) });
        AddGuard(zone, "PitGuard2", new Vector2(950, 250),
            new[] { new Vector2(800, 200), new Vector2(1100, 300) });

        // Silas Rootwarden — wired to fire kill event on death
        SpawnRootwardenTarget(zone, new Vector2(640, 120));
    }

    private void SpawnRootwardenTarget(Node2D parent, Vector2 pos)
    {
        var rootwarden = new Enemies.EnemyBase { Name = "SilasRootwarden" };
        rootwarden.Position = pos;
        rootwarden.CollisionLayer = 1 << 2;
        rootwarden.CollisionMask = 1;

        var sprite = new AnimatedSprite2D { Name = "AnimatedSprite2D" };
        sprite.SpriteFrames = CreateGuardSpriteFrames("npc_civilian");
        rootwarden.AddChild(sprite);

        var bodyShape = new CollisionShape2D();
        bodyShape.Shape = new RectangleShape2D { Size = new Vector2(10, 14) };
        rootwarden.AddChild(bodyShape);

        // Rootwarden is drunk after events — slightly more health
        var health = new Combat.HealthComponent { Name = "HealthComponent", MaxHealth = 3 };
        health.Died += OnRootwardenDied;
        rootwarden.AddChild(health);

        var hurtbox = new Combat.Hurtbox { Name = "Hurtbox" };
        hurtbox.CollisionLayer = 0;
        hurtbox.CollisionMask = 1 << 3;
        var hurtShape = new CollisionShape2D { Name = "HurtboxShape" };
        hurtShape.Shape = new RectangleShape2D { Size = new Vector2(12, 14) };
        hurtbox.AddChild(hurtShape);
        rootwarden.AddChild(hurtbox);

        parent.AddChild(rootwarden);
    }

    private void OnRootwardenDied()
    {
        OnTargetKilled(
            targetId: "rootwarden",
            kingdomIndex: 0,
            targetDisplayName: "Silas Rootwarden",
            whisperText: "\"It's entertainment. They were going to die anyway.\"");
    }

    // ─── Root Cellar Zone ─────────────────────────────────────────

    private void BuildRootCellar()
    {
        var zone = new Node2D { Name = "RootCellarZone" };
        zone.Position = CellarOffset;
        AddChild(zone);

        MapBuilder.Build(zone, CellarMap);
        AddAreaZone(zone, "Root Cellar", new Vector2(640, 260), new Vector2(1280, 480));
        AddAreaZone(zone, "Contracts Vault", new Vector2(640, 130), new Vector2(400, 200), isRestricted: true);

        // Shadow — wine rack alcoves, vaulted ceiling corners
        AddShadowZone(zone, new Vector2(60, 230), new Vector2(100, 200));
        AddShadowZone(zone, new Vector2(1220, 230), new Vector2(100, 200));
        AddShadowZone(zone, new Vector2(640, 360), new Vector2(320, 60));

        // Hiding spots — barrel rows, root pile, wine racks
        AddHidingSpot(zone, "Barrel Row", new Vector2(180, 230));
        AddHidingSpot(zone, "Wine Rack", new Vector2(1100, 230));
        AddHidingSpot(zone, "Root Pile", new Vector2(640, 350));

        // Locked vault — fighting contracts and prize ledger inside
        AddLockedDoor(zone, "Contracts Vault Door", new Vector2(640, 130), "vault_key");
        AddKeyChest(zone, "Pit Master Key Chest", new Vector2(400, 280), "vault_key", "Pit Master Key");
        AddItemChest(zone, "Prize Ledger", new Vector2(640, 100),
            "consumable", "fighting_contracts", "Fighting Contracts", 1, "vault_key");

        // Escape tunnel — breakable wall at the far end of the cellar
        AddBreakableWall(zone, "Escape Tunnel Wall", new Vector2(1230, 260), hitsRequired: 2, width: 16f, height: 128f);

        // Guards — two contract enforcers who stay in the cellar
        var g1 = SpawnAndTrackGuard(zone, "CellarEnforcer1", new Vector2(300, 260),
            new[] { new Vector2(150, 260), new Vector2(500, 260) });
        var g2 = SpawnAndTrackGuard(zone, "CellarEnforcer2", new Vector2(980, 260),
            new[] { new Vector2(780, 260), new Vector2(1130, 260) }, elite: true);

        _allGuards.Add(g1);
        _allGuards.Add(g2);

        // Start crowd noise cycle — fires from the pit above
        StartCrowdNoiseCycle();
    }

    // ─── Crowd Noise Mechanic ─────────────────────────────────────

    private void StartCrowdNoiseCycle()
    {
        // Crowd cheers every 8 seconds, noise lasts 3 seconds.
        var timer = new Timer { WaitTime = 8.0, Autostart = true, OneShot = false };
        timer.Timeout += OnCrowdNoisePulse;
        AddChild(timer);
    }

    private async void OnCrowdNoisePulse()
    {
        if (_crowdNoiseActive) return;

        _crowdNoiseActive = true;

        // Halve all guard detection ranges.
        foreach (var g in _allGuards)
        {
            var sensor = g.GetNodeOrNull<Stealth.DetectionSensor>("DetectionSensor");
            if (sensor != null) sensor.ViewDistance *= 0.5f;
        }

        // Visual cue — brief dark overlay pulse so the player knows the window is open.
        var layer = new CanvasLayer { Layer = 9 };
        var rect  = new ColorRect { Color = new Color(0f, 0f, 0f, 0.0f) };
        rect.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
        rect.LayoutMode = 1;
        layer.AddChild(rect);
        AddChild(layer);

        var tween = CreateTween().SetLoops(1);
        tween.TweenProperty(rect, "color:a", 0.25f, 0.3f);
        tween.TweenProperty(rect, "color:a", 0.0f,  0.3f);
        tween.TweenCallback(Callable.From(() => layer.QueueFree()));

        GD.Print("[Crowd Noise] Detection window OPEN — 3 seconds.");

        await ToSignal(GetTree().CreateTimer(3.0), SceneTreeTimer.SignalName.Timeout);

        // Restore guard detection ranges.
        foreach (var g in _allGuards)
        {
            var sensor = g.GetNodeOrNull<Stealth.DetectionSensor>("DetectionSensor");
            if (sensor != null) sensor.ViewDistance *= 2.0f;
        }

        _crowdNoiseActive = false;
        GD.Print("[Crowd Noise] Detection window CLOSED.");
    }

    private Enemies.GuardEnemy SpawnAndTrackGuard(Node2D parent, string name, Vector2 pos,
        Vector2[] waypoints, bool elite = false)
    {
        AddGuard(parent, name, pos, waypoints, elite);
        return parent.GetNode<Enemies.GuardEnemy>(name);
    }

}
