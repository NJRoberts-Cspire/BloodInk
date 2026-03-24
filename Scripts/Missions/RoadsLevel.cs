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

    private static readonly Vector2 RoadOffset = new(0, 1200);
    private static readonly Vector2 CrossroadsOffset = new(0, 0);
    private static readonly Vector2 WagonOffset = new(0, -800);

    public override void _Ready()
    {
        PlaceholderSprites.CreateAll();

        BuildRoad();
        BuildCrossroads();
        BuildWagonCamp();

        SpawnPlayer(RoadOffset + new Vector2(800, 180));
        SetupHUD();
        SetCameraLimits(0, -800, 2720, 1680);

        GD.Print("═══ THE ROADS LOADED ═══");
        GD.Print("  Target: The Assessor — traveling tax collector & slave appraiser.");
        GD.Print("  Optional target. Difficulty 3. Light escort.");
    }

    // ─── Zone Builders ────────────────────────────────────────────

    private void BuildRoad()
    {
        var zone = new Node2D { Name = "RoadZone" };
        zone.Position = RoadOffset;
        AddChild(zone);

        BuildTileMap(zone, RoadMap, new Vector2(0, 0));
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

        BuildTileMap(zone, CrossroadsMap, new Vector2(0, 0));
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

        BuildTileMap(zone, WagonMap, new Vector2(0, 0));
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

    // ─── Tile Map Builder ─────────────────────────────────────────

    private static void BuildTileMap(Node2D parent, string[] map, Vector2 origin)
    {
        const float TileSize = 16f;

        for (int row = 0; row < map.Length; row++)
        {
            for (int col = 0; col < map[row].Length; col++)
            {
                char tile = map[row][col];
                var pos = origin + new Vector2(col * TileSize, row * TileSize);

                Color? color = tile switch
                {
                    '#' => new Color(0.28f, 0.24f, 0.18f, 1f),
                    '.' => new Color(0.55f, 0.48f, 0.38f, 1f),
                    'w' => new Color(0.48f, 0.36f, 0.22f, 1f),
                    'p' => new Color(0.50f, 0.44f, 0.34f, 1f),
                    '~' => null,
                    ',' => null,
                    _ => null
                };

                if (color == null) continue;

                var rect = new ColorRect();
                rect.Color = color.Value;
                rect.Position = pos;
                rect.Size = new Vector2(TileSize, TileSize);

                if (tile == '#')
                {
                    var body = new StaticBody2D();
                    body.Position = pos + new Vector2(TileSize / 2, TileSize / 2);
                    var shape = new CollisionShape2D();
                    shape.Shape = new RectangleShape2D { Size = new Vector2(TileSize, TileSize) };
                    body.AddChild(shape);
                    parent.AddChild(body);
                }

                parent.AddChild(rect);
            }
        }
    }
}
