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

    private static readonly Vector2 YardOffset = new(0, 1120);
    private static readonly Vector2 ApproachOffset = new(0, 0);
    private static readonly Vector2 PitOffset = new(0, -720);

    public override void _Ready()
    {
        PlaceholderSprites.CreateAll();

        BuildFarmYard();
        BuildBarnApproach();
        BuildFightingPit();

        SpawnPlayer(YardOffset + new Vector2(640, 180));
        SetupHUD();
        SetCameraLimits(0, -720, 1280, 1500);

        GD.Print("═══ ROOTWARDEN FARM LOADED ═══");
        GD.Print("  Target: Silas Rootwarden — farm owner & fighting ring operator.");
        GD.Print("  Optional target. Difficulty 4. Barn has heavy guard presence.");
    }

    // ─── Zone Builders ────────────────────────────────────────────

    private void BuildFarmYard()
    {
        var zone = new Node2D { Name = "FarmYardZone" };
        zone.Position = YardOffset;
        AddChild(zone);

        BuildTileMap(zone, YardMap, new Vector2(0, 0));
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

        BuildTileMap(zone, BarnApproachMap, new Vector2(0, 0));
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

        BuildTileMap(zone, PitMap, new Vector2(0, 0));
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
                    '#' => new Color(0.30f, 0.22f, 0.14f, 1f),
                    '.' => new Color(0.45f, 0.38f, 0.28f, 1f),
                    'w' => new Color(0.42f, 0.32f, 0.20f, 1f),
                    'p' => new Color(0.40f, 0.36f, 0.28f, 1f),
                    '~' => new Color(0.10f, 0.15f, 0.10f, 0.5f),
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
