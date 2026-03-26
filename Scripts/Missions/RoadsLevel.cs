using Godot;
using BloodInk.Content;
using BloodInk.Core;
using BloodInk.Interaction;
using BloodInk.Stealth;
using BloodInk.Tools;

namespace BloodInk.Missions;

/// <summary>
/// The Roads — hunting The Assessor, a traveling tax collector and slave appraiser.
/// Three zones: Country Road (entry) → Market Crossroads → The Assessor's Wagon.
/// Difficulty: 3 — light guard escort, open terrain. Optional target.
/// </summary>
public partial class RoadsLevel : MissionLevelBase
{
    // ─── Map Layout ──────────────────────────────────────────────
    // . = dirt road, # = stone wall/fence, ~ = shadow under trees
    // , = grass/scrubland, p = packed earth path, w = wooden planks
    // T = tree trunk (impassable), b = brush/undergrowth

    // Country Road — long dirt track with ditches, hedgerows, crofters' fields
    private static readonly string[] RoadMap = {
        ",,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,",
        ",,~,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,~,,",
        ",,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,",
        ",,,,,,,,,,,,,,~,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,~,,,,,,,,,,,,,,,,,,,,,,,",
        ",,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,",
        "#############################################################################################################################################################",
        "#............................................................................................................................................................................................................................#",
        "#............................................................................................................................................................................................................................#",
        "#............................................................................................................................................................................................................................#",
        "#............................................................................................................................................................................................................................#",
        "#...........................................................................ppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppp......#",
        "#....................pppppppppppppppppppppppppppppppppppppppppppppppppppppp.................................................................p...............#",
        "#.................pp..............................................................................................................pp.....p.................#",
        "#...............pp..............................................................................................................pp.......p...............#",
        "#..............p................................................................................................................p.........p..............#",
        "#..............p................................................................................................................p.........p..............#",
        "#..............p................................................................................................................p.........p..............#",
        "#..............pp..............................................................................................................pp.........p.............#",
        "#...............pp............................................................................................................pp..........p.............#",
        "#.................pp........................................................................................................pp...........p.............#",
        "#...................ppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppp.......pppppppp..............p.............#",
        "#...........................................................................................................................................................................................................p...........#",
        "#...........................................................................................................................................................................................................p...........#",
        "#............................................................................................................................................................................................................p..........#",
        "#............................................................................................................................................................................................................................#",
        "#############################################################################################################################################################",
        ",,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,",
        ",,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,",
        ",,~,,,,,,,,,,,,,,,,~,,,,,,,,,,,,,,,,,,~,,,,,,,,,,,,,,,,,,,,~,,,,,,,,,,,,,,,,,,~,,,,,,,,,,,,,,,,,,,~,,,,,,,,,,,,,,,,,,~,,,,,,,,,,,,,,,,,,~,,,,,,,,,,,,,~,,,,",
    };

    // Market Crossroads — wider junction with stalls, well, pillory, inn yard
    private static readonly string[] CrossroadsMap = {
        "#############################################################################################################################################################",
        "#wwwwwwwww#........#wwwwwwwww#..........#wwwwwwwww#........#wwwwwwwww#........#wwwwwwwww#..........#wwwwwwwww#........#wwwwwwwww#..........#wwwwwwwww#",
        "#wwwwwwwww#........#wwwwwwwww#..........#wwwwwwwww#........#wwwwwwwww#........#wwwwwwwww#..........#wwwwwwwww#........#wwwwwwwww#..........#wwwwwwwww#",
        "#wwwwwwwww#........#wwwwwwwww#..........#wwwwwwwww#........#wwwwwwwww#........#wwwwwwwww#..........#wwwwwwwww#........#wwwwwwwww#..........#wwwwwwwww#",
        "#wwwwwwwww....pppp....wwwwwwww#....pppp....wwwwwwwww....pppp....wwwwwwww....pppp....wwwwwwwww....pppp....wwwwwwwww....pppp....wwwwwwwww....pppp#",
        "#wwwwwwwww....pppp....wwwwwwww#....pppp....wwwwwwwww....pppp....wwwwwwww....pppp....wwwwwwwww....pppp....wwwwwwwww....pppp....wwwwwwwww....pppp#",
        "#wwwwwwwww#........#wwwwwwwww#..........#wwwwwwwww#........#wwwwwwwww#........#wwwwwwwww#..........#wwwwwwwww#........#wwwwwwwww#..........#wwwwwwwww#",
        "#wwwwwwwww#........#wwwwwwwww#..........#wwwwwwwww#........#wwwwwwwww#........#wwwwwwwww#..........#wwwwwwwww#........#wwwwwwwww#..........#wwwwwwwww#",
        "#......#........#........#..........#........#........#........#........#........#..........#........#........#........#..........#........#........#",
        "#......#........#........#..........#........#........#........#........#........#..........#........#........#........#..........#........#........#",
        "#......#..~~....#........#..........#........#....~~..#........#........#..~~....#..........#........#....~~..#........#..........#........#..~~....#",
        "#..............................................................................................................................................................................................................................................#",
        "#..............................................................................................................................................................................................................................................#",
        "#..............................................................................................................................................................................................................................................#",
        "#..............................................................................................................................................................................................................................................#",
        "#..............................................................................................................................................................................................................................................#",
        "#..............................................................................................................................................................................................................................................#",
        "#......#..~~....#........#..........#........#....~~..#........#........#..~~....#..........#........#....~~..#........#..........#........#..~~....#",
        "#......#........#........#..........#........#........#........#........#........#..........#........#........#........#..........#........#........#",
        "#wwwwwwwww#........#wwwwwwwww#..........#wwwwwwwww#........#wwwwwwwww#........#wwwwwwwww#..........#wwwwwwwww#........#wwwwwwwww#..........#wwwwwwwww#",
        "#wwwwwwwww#........#wwwwwwwww#..........#wwwwwwwww#........#wwwwwwwww#........#wwwwwwwww#..........#wwwwwwwww#........#wwwwwwwww#..........#wwwwwwwww#",
        "#wwwwwwwww....pppp....wwwwwwww#....pppp....wwwwwwwww....pppp....wwwwwwww....pppp....wwwwwwwww....pppp....wwwwwwwww....pppp....wwwwwwwww....pppp#",
        "#wwwwwwwww....pppp....wwwwwwww#....pppp....wwwwwwwww....pppp....wwwwwwww....pppp....wwwwwwwww....pppp....wwwwwwwww....pppp....wwwwwwwww....pppp#",
        "#wwwwwwwww#........#wwwwwwwww#..........#wwwwwwwww#........#wwwwwwwww#........#wwwwwwwww#..........#wwwwwwwww#........#wwwwwwwww#..........#wwwwwwwww#",
        "#############################################################################################################################################################",
    };

    // The Assessor's Wagon — final zone: camp/wagon area, portable office, records tent
    private static readonly string[] WagonMap = {
        "#############################################################################################################################################################",
        "#..........#wwwwwwww#..........#wwwwwwww#..........#wwwwwwww#..........#wwwwwwww#..........#wwwwwwww#..........#wwwwwwww#..........#wwwwwwww#..........#",
        "#..........#wwwwwwww#..........#wwwwwwww#..........#wwwwwwww#..........#wwwwwwww#..........#wwwwwwww#..........#wwwwwwww#..........#wwwwwwww#..........#",
        "#..........#wwwwwwww#..........#wwwwwwww#..........#wwwwwwww#..........#wwwwwwww#..........#wwwwwwww#..........#wwwwwwww#..........#wwwwwwww#..........#",
        "#..........#wwwwwwww#..........#wwwwwwww#..........#wwwwwwww#..........#wwwwwwww#..........#wwwwwwww#..........#wwwwwwww#..........#wwwwwwww#..........#",
        "#...........................................................................................................................................................................................................#",
        "#...........................................................................................................................................................................................................#",
        "#...........................................................................................................................................................................................................#",
        "#...........................................................................................................................................................................................................#",
        "#...........................................................................................................................................................................................................#",
        "#...........................................................................................................................................................................................................#",
        "#...........................................................................................................................................................................................................#",
        "#..........#wwwwwwww#..........#wwwwwwww#..........#..~~....#..........#....~~..#..........#wwwwwwww#..........#wwwwwwww#..........#wwwwwwww#..........#",
        "#..........#wwwwwwww#..........#wwwwwwww#..........#..~~....#..........#....~~..#..........#wwwwwwww#..........#wwwwwwww#..........#wwwwwwww#..........#",
        "#..........#........#..........#........#..........#........#..........#........#..........#........#..........#........#..........#........#..........#",
        "#..........#........#..........#........#..........#........#..........#........#..........#........#..........#........#..........#........#..........#",
        "#...........................................................................................................................................................................................................#",
        "#...........................................................................................................................................................................................................#",
        "#...........................................................................................................................................................................................................#",
        "#...........................................................................................................................................................................................................#",
        "#############################################################################################################################################################",
    };

    // ─── Toll Bridge Guard Post — fourth zone added to extend level length ──
    // A narrow fortified crossing with a gate mechanism. The Assessor's wagon
    // must pass through here — it's the one fixed choke point on the entire road.
    // Unique mechanic: guard patrol SCHEDULES. Guards shift routes every 30 seconds
    // based on an alternating watch-pattern, forcing the player to observe timing.
    private static readonly string[] TollBridgeMap = {
        "#############################################################################################################################################################",
        "#................#wwwwwwww#..........#wwwwwwww#..........#wwwwwwww#..........#wwwwwwww#..........#wwwwwwww#..........#wwwwwwww#..........#wwwwwwww#........#",
        "#................#wwwwwwww#..........#wwwwwwww#..........#wwwwwwww#..........#wwwwwwww#..........#wwwwwwww#..........#wwwwwwww#..........#wwwwwwww#........#",
        "#................#wwwwwwww#..........#wwwwwwww#..........#wwwwwwww#..........#wwwwwwww#..........#wwwwwwww#..........#wwwwwwww#..........#wwwwwwww#........#",
        "#..~~............#........#..........#........#..........#........#..........#........#..........#........#..........#........#..........#........#..~~....#",
        "#................................................................................................................................................................................................................#",
        "#................................................................................................................................................................................................................#",
        "#####.....#######################################################################################################################################.....#####",
        "#####.....#######################################################################################################################################.....#####",
        "#####.....##~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~##.....#####",
        "#####.....##~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~##.....#####",
        "#####.....##~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~##.....#####",
        "#####.....#######################################################################################################################################.....#####",
        "#####.....#######################################################################################################################################.....#####",
        "#................................................................................................................................................................................................................#",
        "#................................................................................................................................................................................................................#",
        "#..~~............#........#..........#........#..........#........#..........#........#..........#........#..........#........#..........#........#..~~....#",
        "#................#wwwwwwww#..........#wwwwwwww#..........#wwwwwwww#..........#wwwwwwww#..........#wwwwwwww#..........#wwwwwwww#..........#wwwwwwww#........#",
        "#................#wwwwwwww#..........#wwwwwwww#..........#wwwwwwww#..........#wwwwwwww#..........#wwwwwwww#..........#wwwwwwww#..........#wwwwwwww#........#",
        "#................#wwwwwwww#..........#wwwwwwww#..........#wwwwwwww#..........#wwwwwwww#..........#wwwwwwww#..........#wwwwwwww#..........#wwwwwwww#........#",
        "#############################################################################################################################################################",
    };

    private static readonly Vector2 RoadOffset      = new(0, 1200);
    private static readonly Vector2 CrossroadsOffset = new(0, 0);
    private static readonly Vector2 WagonOffset      = new(0, -800);
    private static readonly Vector2 TollBridgeOffset = new(0, -1280); // Below the wagon zone

    // ─── Patrol Schedule State ────────────────────────────────────
    // Guards alternate between Watch A and Watch B every 30 seconds.
    // Watch A: spread patrol covering both flanks.
    // Watch B: converged patrol guarding the gate directly.
    private readonly Vector2[][] _watchA =
    {
        new[] { new Vector2(300,  200), new Vector2(800,  200) },   // Left flank sweep
        new[] { new Vector2(2200, 200), new Vector2(1700, 200) },   // Right flank sweep
        new[] { new Vector2(1280, 150), new Vector2(1280, 350) },   // Gate centre
    };
    private readonly Vector2[][] _watchB =
    {
        new[] { new Vector2(200, 300), new Vector2(600, 100) },     // Tight gate left
        new[] { new Vector2(2360, 300), new Vector2(1960, 100) },   // Tight gate right
        new[] { new Vector2(900, 250), new Vector2(1660, 250) },    // Wide crossing sweep
    };
    private Enemies.GuardEnemy[] _scheduleGuards = System.Array.Empty<Enemies.GuardEnemy>();
    private int _watchPhase; // 0 = Watch A, 1 = Watch B

    public override void _Ready()
    {
        PlaceholderSprites.CreateAll();

        BuildRoad();
        BuildCrossroads();
        BuildWagonCamp();
        BuildTollBridge();

        SpawnPlayer(RoadOffset + new Vector2(800, 180));
        SetupHUD();
        SetupCheckpointRespawn();

        // Checkpoint 1 — player enters Market Crossroads zone.
        AddCheckpoint(this, 1, new Vector2(1280, 50), 2560f, new Vector2(800, 200));

        // Checkpoint 2 — player enters the Assessor's Wagon zone.
        AddCheckpoint(this, 2, new Vector2(1280, -780), 2560f, new Vector2(800, -680));

        // Checkpoint 3 — player enters the Toll Bridge Guard Post.
        AddCheckpoint(this, 3, new Vector2(1280, -1260), 2560f, new Vector2(1280, -1150));

        // Expand camera to include toll bridge zone below the wagon.
        SetCameraLimits(0, -1640, 2720, 1680);

        GD.Print("═══ THE ROADS LOADED ═══");
        GD.Print("  Target: The Assessor — traveling tax collector & slave appraiser.");
        GD.Print("  Optional target. Difficulty 3. Four zones including Toll Bridge.");
        GD.Print("  MECHANIC: Guard patrol schedules rotate every 30 s — observe before moving.");
    }

    // ─── Zone Builders ────────────────────────────────────────────

    private void BuildRoad()
    {
        var zone = new Node2D { Name = "RoadZone" };
        zone.Position = RoadOffset;
        AddChild(zone);

        MapBuilder.Build(zone, RoadMap);
        AddAreaZone(zone, "Country Road", new Vector2(800, 300), new Vector2(1600, 400));
        AddAreaZone(zone, "Roadside Ditch", new Vector2(200, 200), new Vector2(200, 300), isRestricted: false);

        // Shadow spots — ditch, hedge, treeline
        AddShadowZone(zone, new Vector2(120, 200), new Vector2(80, 240));
        AddShadowZone(zone, new Vector2(2600, 200), new Vector2(80, 240));
        AddShadowZone(zone, new Vector2(800, 60), new Vector2(400, 80));

        // Entry chest — distraction stone
        AddItemChest(zone, "Traveler's Pack", new Vector2(180, 190),
            "consumable", "stone", "Throwing Stone", 2);

        // Two light guards — road escorts traveling with the wagon
        AddGuard(zone, "RoadGuard1", new Vector2(600, 300),
            new[] { new Vector2(400, 300), new Vector2(1200, 300) });
        AddGuard(zone, "RoadGuard2", new Vector2(1400, 280),
            new[] { new Vector2(1200, 280), new Vector2(1800, 280) });
    }

    private void BuildCrossroads()
    {
        var zone = new Node2D { Name = "CrossroadsZone" };
        zone.Position = CrossroadsOffset;
        AddChild(zone);

        MapBuilder.Build(zone, CrossroadsMap);
        AddAreaZone(zone, "Market Crossroads", new Vector2(1360, 200), new Vector2(2720, 400));
        AddAreaZone(zone, "Inn Yard", new Vector2(400, 200), new Vector2(600, 300), isRestricted: true);

        // Market stalls — shadow between stalls
        AddShadowZone(zone, new Vector2(400, 200), new Vector2(60, 200));
        AddShadowZone(zone, new Vector2(900, 200), new Vector2(60, 200));
        AddShadowZone(zone, new Vector2(1400, 200), new Vector2(60, 200));
        AddShadowZone(zone, new Vector2(1900, 200), new Vector2(60, 200));

        // Hiding spots — barrels, cart shadow, well
        AddHidingSpot(zone, "Barrel Stack", new Vector2(500, 150));
        AddHidingSpot(zone, "Market Cart", new Vector2(1100, 220));
        AddHidingSpot(zone, "Well Shadow", new Vector2(1360, 200));

        // Guards — two innkeepers' muscle, one roving patrol
        AddGuard(zone, "MarketGuard1", new Vector2(600, 200),
            new[] { new Vector2(300, 200), new Vector2(900, 200) });
        AddGuard(zone, "MarketGuard2", new Vector2(2000, 200),
            new[] { new Vector2(1600, 200), new Vector2(2400, 200) });
        AddGuard(zone, "RoamingGuard", new Vector2(1360, 100),
            new[] { new Vector2(200, 100), new Vector2(2500, 100), new Vector2(1360, 300) });

        // Records chest — unlockable intel
        AddKeyChest(zone, "Assessor's Ledger", new Vector2(2200, 180), "ledger_key", "Ledger Key");
    }

    private void BuildWagonCamp()
    {
        var zone = new Node2D { Name = "WagonZone" };
        zone.Position = WagonOffset;
        AddChild(zone);

        MapBuilder.Build(zone, WagonMap);
        AddAreaZone(zone, "Wagon Camp", new Vector2(1360, 200), new Vector2(2720, 400));
        AddAreaZone(zone, "Assessor's Tent", new Vector2(1360, 120), new Vector2(400, 200), isRestricted: true);

        // Shadow under wagon and tent edges
        AddShadowZone(zone, new Vector2(700, 200), new Vector2(120, 100));
        AddShadowZone(zone, new Vector2(2100, 200), new Vector2(120, 100));
        AddShadowZone(zone, new Vector2(1360, 50), new Vector2(200, 80));

        // Hiding spots
        AddHidingSpot(zone, "Wagon Underside", new Vector2(700, 200));
        AddHidingSpot(zone, "Supply Crates", new Vector2(1700, 250));

        // Elite guard — personal bodyguard
        AddGuard(zone, "BodyGuard", new Vector2(1360, 300),
            new[] { new Vector2(1200, 300), new Vector2(1520, 300) }, elite: true);
        AddGuard(zone, "CampSentinel", new Vector2(800, 200),
            new[] { new Vector2(600, 150), new Vector2(600, 280), new Vector2(800, 280) });

        // The Assessor — wired to fire kill event on death
        SpawnAssessorTarget(zone, new Vector2(1360, 150));
    }

    private void SpawnAssessorTarget(Node2D parent, Vector2 pos)
    {
        // The Assessor: frail civilian-class enemy — dies in 1 stealth hit, 2 normal
        var assessor = new Enemies.EnemyBase { Name = "TheAssessor" };
        assessor.Position = pos;
        assessor.CollisionLayer = 1 << 2;
        assessor.CollisionMask = 1;

        var sprite = new AnimatedSprite2D { Name = "AnimatedSprite2D" };
        sprite.SpriteFrames = CreateGuardSpriteFrames("npc_civilian");
        assessor.AddChild(sprite);

        var bodyShape = new CollisionShape2D();
        bodyShape.Shape = new RectangleShape2D { Size = new Vector2(10, 14) };
        assessor.AddChild(bodyShape);

        var health = new Combat.HealthComponent { Name = "HealthComponent", MaxHealth = 2 };
        health.Died += OnAssessorDied;
        assessor.AddChild(health);

        var hurtbox = new Combat.Hurtbox { Name = "Hurtbox" };
        hurtbox.CollisionLayer = 0;
        hurtbox.CollisionMask = 1 << 3;
        var hurtShape = new CollisionShape2D { Name = "HurtboxShape" };
        hurtShape.Shape = new RectangleShape2D { Size = new Vector2(12, 14) };
        hurtbox.AddChild(hurtShape);
        assessor.AddChild(hurtbox);

        parent.AddChild(assessor);
    }

    private void OnAssessorDied()
    {
        OnTargetKilled(
            targetId: "assessor",
            kingdomIndex: 0,
            targetDisplayName: "The Assessor",
            whisperText: "\"I just wrote down what they told me to write.\"");
    }

    // ─── Toll Bridge Guard Post ───────────────────────────────────

    private void BuildTollBridge()
    {
        var zone = new Node2D { Name = "TollBridgeZone" };
        zone.Position = TollBridgeOffset;
        AddChild(zone);

        MapBuilder.Build(zone, TollBridgeMap);
        AddAreaZone(zone, "Toll Bridge", new Vector2(1280, 200), new Vector2(2560, 400));
        AddAreaZone(zone, "Gate House", new Vector2(1280, 280), new Vector2(500, 180), isRestricted: true);

        // Shadow — guardhouse alcoves and bridge underside shadows
        AddShadowZone(zone, new Vector2(80, 130), new Vector2(120, 120));
        AddShadowZone(zone, new Vector2(2480, 130), new Vector2(120, 120));
        AddShadowZone(zone, new Vector2(400, 300), new Vector2(200, 80));
        AddShadowZone(zone, new Vector2(1900, 300), new Vector2(200, 80));
        AddShadowZone(zone, new Vector2(1280, 310), new Vector2(600, 60)); // Bridge underdeck shadow

        // Hiding spots — guard house alcoves, supply pile, ditch
        AddHidingSpot(zone, "Guard House Alcove", new Vector2(120, 130));
        AddHidingSpot(zone, "Far Alcove", new Vector2(2440, 130));
        AddHidingSpot(zone, "Supply Pile", new Vector2(600, 250));
        AddHidingSpot(zone, "Road Ditch", new Vector2(380, 310));

        // Toll gate — locked; lever inside the guardhouse raises it
        var gate = AddPuzzleGate(zone, "Toll Gate", new Vector2(1280, 330), requiredConditions: 1,
            isVertical: false, width: 80f, height: 16f);
        var gateLever = AddLever(zone, "Gate Lever", new Vector2(1280, 270), oneWay: true);
        gateLever.Toggled += (on) => { if (on) gate.RegisterConditionMet(); };

        // Breakable barrier — side path through collapsed stone abutment
        AddBreakableWall(zone, "Collapsed Abutment", new Vector2(200, 330), hitsRequired: 2, width: 32f, height: 16f);

        // Key chest — bridge toll records (contains intel about Assessor's route)
        AddItemChest(zone, "Toll Ledger", new Vector2(1280, 140),
            "consumable", "intel_scroll", "Bridge Toll Records", 1);

        // Schedule-driven guards: stored so the timer can swap their waypoints
        var guard0 = SpawnScheduleGuard(zone, "BridgeGuard1", new Vector2(500, 200));
        var guard1 = SpawnScheduleGuard(zone, "BridgeGuard2", new Vector2(2060, 200));
        var guard2 = SpawnScheduleGuard(zone, "BridgeGuard3", new Vector2(1280, 200), elite: true);
        _scheduleGuards = new[] { guard0, guard1, guard2 };

        // Apply initial Watch A routes immediately.
        ApplyWatchRoutes(_watchA);

        // Two crossbowmen on upper guardhouse ledges — cannot be bypassed from front.
        AddCrossbowman(zone, "TollCrossbowman1", new Vector2(300, 80));
        AddCrossbowman(zone, "TollCrossbowman2", new Vector2(2260, 80));

        // Start the 30-second patrol rotation timer.
        var timer = new Timer { WaitTime = 30.0, Autostart = true, OneShot = false };
        timer.Timeout += OnPatrolScheduleRotate;
        AddChild(timer);
    }

    private Enemies.GuardEnemy SpawnScheduleGuard(Node2D parent, string name, Vector2 pos, bool elite = false)
    {
        // Use AddGuard then retrieve the node so we can store a reference.
        AddGuard(parent, name, pos, System.Array.Empty<Vector2>(), elite);
        return parent.GetNode<Enemies.GuardEnemy>(name);
    }

    private void ApplyWatchRoutes(Vector2[][] routes)
    {
        for (int i = 0; i < _scheduleGuards.Length && i < routes.Length; i++)
        {
            var patrol = _scheduleGuards[i].GetNodeOrNull<Enemies.PatrolRoute>("PatrolRoute");
            if (patrol != null)
                patrol.Waypoints = routes[i];
        }
    }

    private void OnPatrolScheduleRotate()
    {
        _watchPhase = 1 - _watchPhase; // Toggle 0 ↔ 1
        ApplyWatchRoutes(_watchPhase == 0 ? _watchA : _watchB);
        GD.Print($"[Toll Bridge] Patrol rotated to Watch {(_watchPhase == 0 ? 'A' : 'B')}.");
    }

}
