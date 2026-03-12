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

    // Quarry Yard — sprawling open area with tool shacks, wagons, captive pens, quarry pits, overseer platforms
    private static readonly string[] YardMap = {
        ",,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,",
        ",,~,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,~,,,,,,",
        ",,,,,,,,,,,,,,,pppppppp,,,,,,,,pppppppp,,,,,,,,,,,,,,,,,,pppppppppppppppppp,,,,,,,,,,,,,,,,,,pppppppp,,,,,,,,pppppppp,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,",
        ",,,,,,,,,,,,,,pp......pp,,,,pp..........pp,,,,,,,,,,,,ppp..................ppp,,,,,,,,,,,,,,pp......pp,,,,pp..........pp,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,",
        ",,,,,,,,,,,,,p..........p,,p..............p,,,,,,,,,,pp......................pp,,,,,,,,,,,,p..........p,,p..............p,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,",
        ",,,,,,,,,,,,p............pp................p,,,,,,,,pp........................pp,,,,,,,,,,p............pp................p,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,",
        ",,,,,,,,,,,p..............................p,,,,,,,,p..........................p,,,,,,,,,p..............................p,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,",
        ",,,,,,,,,,p................................p,,,,,,p............................p,,,,,,,,p................................p,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,",
        ",,,,,,,,,p..................................p,,,,p..............................p,,,,,,p..................................p,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,",
        ",,,,,,,,p....................................p,,p................................p,,,,p....................................p,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,",
        ",,,,,,,,p....................................pppp................................pppp......................................p,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,",
        ",,,,,,,p.......................................................................................................p.........p,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,",
        ",,,,,,,p.......................................................................................................p.........p,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,",
        ",,,,,,p........................................................................................................p..........p,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,",
        ",,,,,,p........................................................................................................p..........p,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,",
        ",,,,,,,p.......................................................................................................p.........p,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,",
        ",,,,,,,p.......................................................................................................p.........p,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,",
        ",,,,,,,,p....................................pppp................................pppp......................................p,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,",
        ",,,,,,,,p....................................p,,p................................p,,,,p....................................p,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,",
        ",,,,,,,,,p..................................p,,,,p..............................p,,,,,,p..................................p,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,",
        ",,,,,,,,,,p................................p,,,,,,p............................p,,,,,,,,p................................p,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,",
        ",,,,,,,,,,,p..............................p,,,,,,,,p..........................p,,,,,,,,,p..............................p,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,",
        ",,,,,,,,,,,,p............pp................p,,,,,,,,pp........................pp,,,,,,,,,,p............pp................p,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,",
        ",,,,,,,,,,,,,p..........p,,p..............p,,,,,,,,,,pp......................pp,,,,,,,,,,,,p..........p,,p..............p,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,",
        ",,,,,,,,,,,,,,pp......pp,,,,pp..........pp,,,,,,,,,,,,ppp..................ppp,,,,,,,,,,,,,,pp......pp,,,,pp..........pp,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,",
        ",,,,,,,,,,,,,,,pppppppp,,,,,,,,pppppppp,,,,,,,,,,,,,,,,,,pppppppppppppppppp,,,,,,,,,,,,,,,,,,pppppppp,,,,,,,,pppppppp,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,",
        ",,~,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,~,,,,,,",
        ",,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,",
        ",,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,",
        ",,~,,,,,,,,,,,,,,,,,,,,,~,,,,,,,,,,,,,,,,,,,,~,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,~,,,,,,,,,,,,,,,,,,,,~,,,,,,,,,,,,,,,,,,,,~,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,",
        ",,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,",
        ",,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,",
        ",,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,",
        ",,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,",
    };

    // Tunnel Passage — extended mining network with alcoves, support beams, mine cart tracks, and intersections
    private static readonly string[] TunnelMap = {
        "################################################################################################################################################################",
        "#......~~#......#~~.........#......#~~.........#......~~........#......~~#......#~~.........#......#~~.........#......~~........#......~~#......#~~.........#......#",
        "#........#......#..........#........#..........#...............#........#......#..........#........#..........#...............#........#......#..........#........#",
        "#........#......#..........#........#..........#...............#........#......#..........#........#..........#...............#........#......#..........#........#",
        "#........#......#..........#........#..........#...............#........#......#..........#........#..........#...............#........#......#..........#........#",
        "#..............#...........#.................#.................#..............#...........#.................#.................#..............#...........#..........#",
        "#..............#...........#.................#.................#..............#...........#.................#.................#..............#...........#..........#",
        "#..............#...........#.................#.................#..............#...........#.................#.................#..............#...........#..........#",
        "#..............#...........#.................#.................#..............#...........#.................#.................#..............#...........#..........#",
        "#........#.....#..........#........#..........#...............#........#.....#..........#........#..........#...............#........#.....#..........#..........#",
        "#........#.....#..........#........#..........#...............#........#.....#..........#........#..........#...............#........#.....#..........#..........#",
        "#........#.....#..........#........#..........#...............#........#.....#..........#........#..........#...............#........#.....#..........#..........#",
        "#........#.....#..........#........#..........#...............#........#.....#..........#........#..........#...............#........#.....#..........#..........#",
        "#..............#...........#.................#.................#..............#...........#.................#.................#..............#...........#..........#",
        "#..............#...........#.................#.................#..............#...........#.................#.................#..............#...........#..........#",
        "#..............#...........#.................#.................#..............#...........#.................#.................#..............#...........#..........#",
        "#..............#...........#.................#.................#..............#...........#.................#.................#..............#...........#..........#",
        "#........#.....#..........#........#..........#...............#........#.....#..........#........#..........#...............#........#.....#..........#..........#",
        "#........#.....#..........#........#..........#...............#........#.....#..........#........#..........#...............#........#.....#..........#..........#",
        "#........#.....#..........#........#..........#...............#........#.....#..........#........#..........#...............#........#.....#..........#..........#",
        "#..............#...........#.................#.................#..............#...........#.................#.................#..............#...........#..........#",
        "#..............#...........#.................#.................#..............#...........#.................#.................#..............#...........#..........#",
        "#........#.....#..........#........#..........#...............#........#.....#..........#........#..........#...............#........#.....#..........#..........#",
        "#........#.....#..........#........#..........#...............#........#.....#..........#........#..........#...............#........#.....#..........#..........#",
        "#......~~#.....#~~.........#......~~#~~.........#......~~......#......~~#.....#~~.........#......~~#~~.........#......~~......#......~~#.....#~~.........#......~~#",
        "################################################################################################################################################################",
    };

    // Maren's Office — expanded compound with study, trophy display, guard quarters, records room, storage
    private static readonly string[] OfficeMap = {
        "################################################################################################################################################################",
        "#wwwwwwww#cccccccc#wwwwwwwwwwww#wwwwwwww#cccccccc#wwwwwwwwwwww#wwwwwwww#cccccccc#wwwwwwwwwwww#wwwwwwww#cccccccc#wwwwwwwwwwww#wwwwwwww#cccccccc#wwwwwwwwwwwwwwww#",
        "#wwwwwwww#cccccccc#wwwwwwwwwwww#wwwwwwww#cccccccc#wwwwwwwwwwww#wwwwwwww#cccccccc#wwwwwwwwwwww#wwwwwwww#cccccccc#wwwwwwwwwwww#wwwwwwww#cccccccc#wwwwwwwwwwwwwwww#",
        "#wwwwwwww#cccccccc#wwwwwwwwwwww#wwwwwwww#cccccccc#wwwwwwwwwwww#wwwwwwww#cccccccc#wwwwwwwwwwww#wwwwwwww#cccccccc#wwwwwwwwwwww#wwwwwwww#cccccccc#wwwwwwwwwwwwwwww#",
        "#wwwwwwww....cc....wwwwwwwwwwww#wwwwwwww....cc....wwwwwwwwwwww#wwwwwwww....cc....wwwwwwwwwwww#wwwwwwww....cc....wwwwwwwwwwww#wwwwwwww....cc....wwwwwwwwwwwwwwww#",
        "#wwwwwwww....cc....wwwwwwwwwwww.wwwwwwww....cc....wwwwwwwwwwww.wwwwwwww....cc....wwwwwwwwwwww.wwwwwwww....cc....wwwwwwwwwwww.wwwwwwww....cc....wwwwwwwwwwwwwwww#",
        "#wwwwwwww....cc....wwwwwwwwwwww.wwwwwwww....cc....wwwwwwwwwwww.wwwwwwww....cc....wwwwwwwwwwww.wwwwwwww....cc....wwwwwwwwwwww.wwwwwwww....cc....wwwwwwwwwwwwwwww#",
        "#wwwwwwww....cc....wwwwwwwwwwww#wwwwwwww....cc....wwwwwwwwwwww#wwwwwwww....cc....wwwwwwwwwwww#wwwwwwww....cc....wwwwwwwwwwww#wwwwwwww....cc....wwwwwwwwwwwwwwww#",
        "#wwwwwwww#cccccccc#wwwwwwwwwwww#wwwwwwww#cccccccc#wwwwwwwwwwww#wwwwwwww#cccccccc#wwwwwwwwwwww#wwwwwwww#cccccccc#wwwwwwwwwwww#wwwwwwww#cccccccc#wwwwwwwwwwwwwwww#",
        "#wwwwwwww#cccccccc#wwwwwwwwwwww#wwwwwwww#cccccccc#wwwwwwwwwwww#wwwwwwww#cccccccc#wwwwwwwwwwww#wwwwwwww#cccccccc#wwwwwwwwwwww#wwwwwwww#cccccccc#wwwwwwwwwwwwwwww#",
        "#wwwwwwww#cccccccc#wwwwwwwwwwww#wwwwwwww#cccccccc#wwwwwwwwwwww#wwwwwwww#cccccccc#wwwwwwwwwwww#wwwwwwww#cccccccc#wwwwwwwwwwww#wwwwwwww#cccccccc#wwwwwwwwwwwwwwww#",
        "#wwwwwwww#cccccccc#wwwwwwwwwwww#wwwwwwww#cccccccc#wwwwwwwwwwww#wwwwwwww#cccccccc#wwwwwwwwwwww#wwwwwwww#cccccccc#wwwwwwwwwwww#wwwwwwww#cccccccc#wwwwwwwwwwwwwwww#",
        "#wwwwwwww#cccccccc#wwwwwwwwwwww#wwwwwwww#cccccccc#wwwwwwwwwwww#wwwwwwww#cccccccc#wwwwwwwwwwww#wwwwwwww#cccccccc#wwwwwwwwwwww#wwwwwwww#cccccccc#wwwwwwwwwwwwwwww#",
        "#wwwwwwww....cc....wwwwwwwwwwww#wwwwwwww....cc....wwwwwwwwwwww#wwwwwwww....cc....wwwwwwwwwwww#wwwwwwww....cc....wwwwwwwwwwww#wwwwwwww....cc....wwwwwwwwwwwwwwww#",
        "#wwwwwwww....cc....wwwwwwwwwwww.wwwwwwww....cc....wwwwwwwwwwww.wwwwwwww....cc....wwwwwwwwwwww.wwwwwwww....cc....wwwwwwwwwwww.wwwwwwww....cc....wwwwwwwwwwwwwwww#",
        "#wwwwwwww....cc....wwwwwwwwwwww#wwwwwwww....cc....wwwwwwwwwwww#wwwwwwww....cc....wwwwwwwwwwww#wwwwwwww....cc....wwwwwwwwwwww#wwwwwwww....cc....wwwwwwwwwwwwwwww#",
        "#wwwwwwww#cccccccc#wwwwwwwwwwww#wwwwwwww#cccccccc#wwwwwwwwwwww#wwwwwwww#cccccccc#wwwwwwwwwwww#wwwwwwww#cccccccc#wwwwwwwwwwww#wwwwwwww#cccccccc#wwwwwwwwwwwwwwww#",
        "#wwwwwwww#cccccccc#wwwwwwwwwwww#wwwwwwww#cccccccc#wwwwwwwwwwww#wwwwwwww#cccccccc#wwwwwwwwwwww#wwwwwwww#cccccccc#wwwwwwwwwwww#wwwwwwww#cccccccc#wwwwwwwwwwwwwwww#",
        "#wwwwwwww#cccccccc#wwwwwwwwwwww#wwwwwwww#cccccccc#wwwwwwwwwwww#wwwwwwww#cccccccc#wwwwwwwwwwww#wwwwwwww#cccccccc#wwwwwwwwwwww#wwwwwwww#cccccccc#wwwwwwwwwwwwwwww#",
        "#wwwwwwww#cccccccc#~~wwwwwwwwww#wwwwwwww#cccccccc#~~wwwwwwwwww#wwwwwwww#cccccccc#~~wwwwwwwwww#wwwwwwww#cccccccc#~~wwwwwwwwww#wwwwwwww#cccccccc#~~wwwwwwwwwwwwww#",
        "#wwwwwwww#cccccccc#~~wwwwwwwwww#wwwwwwww#cccccccc#~~wwwwwwwwww#wwwwwwww#cccccccc#~~wwwwwwwwww#wwwwwwww#cccccccc#~~wwwwwwwwww#wwwwwwww#cccccccc#~~wwwwwwwwwwwwww#",
        "################################################################################################################################################################",
    };

    // Zone offsets
    private static readonly Vector2 YardOffset = new(0, 1056);
    private static readonly Vector2 TunnelOffset = new(0, 0);
    private static readonly Vector2 OfficeOffset = new(0, -544);

    public override void _Ready()
    {
        PlaceholderSprites.CreateAll();

        BuildYard();
        BuildTunnels();
        BuildOffice();
        SpawnPlayer(YardOffset + new Vector2(1280, 480));
        SetupHUD();

        // Camera bounds encompass all three zones (Office top → Yard bottom).
        SetCameraLimits(0, -544, 2560, 1600);

        GD.Print("═══ LABOR CAMP LOADED ═══");
        GD.Print("  PUZZLE GUIDE:");
        GD.Print("  Yard:    Break the wooden fence to find the Tunnel Key chest.");
        GD.Print("           Push the ore block onto the floor switch to open the tunnel gate.");
        GD.Print("  Tunnels: Use Tunnel Key on the locked shaft door.");
        GD.Print("           Pull lever + push block onto switch to open the Foreman's gate.");
        GD.Print("           Break collapsed wall for shortcut. Find Foreman Key in chest.");
        GD.Print("  Office:  Use Foreman Key on Maren's door. Push block puzzle for bonus ink.");
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
        AddAreaZone(root, "Quarry Yard", new Vector2(1280, 272), new Vector2(2560, 544));

        // Shadow zones in the quarry corners and edges.
        AddShadowZone(root, new Vector2(40, 24), new Vector2(48, 40));
        AddShadowZone(root, new Vector2(2520, 24), new Vector2(48, 40));
        AddShadowZone(root, new Vector2(40, 480), new Vector2(48, 40));
        AddShadowZone(root, new Vector2(2520, 480), new Vector2(48, 40));
        AddShadowZone(root, new Vector2(600, 180), new Vector2(40, 32));
        AddShadowZone(root, new Vector2(1960, 180), new Vector2(40, 32));
        AddShadowZone(root, new Vector2(1280, 140), new Vector2(40, 32));
        AddShadowZone(root, new Vector2(800, 380), new Vector2(40, 32));
        AddShadowZone(root, new Vector2(1760, 380), new Vector2(40, 32));

        // Hiding spots — tool shacks, wagons, crates, quarry equipment.
        AddHidingSpot(root, "Tool Shack", new Vector2(240, 260));
        AddHidingSpot(root, "Supply Wagon", new Vector2(2320, 260));
        AddHidingSpot(root, "Rock Pile", new Vector2(700, 140));
        AddHidingSpot(root, "Ore Cart", new Vector2(1860, 140));
        AddHidingSpot(root, "Crate Stack", new Vector2(1280, 400));
        AddHidingSpot(root, "Captive Pen", new Vector2(480, 380));
        AddHidingSpot(root, "Overseer Platform", new Vector2(1080, 200));
        AddHidingSpot(root, "Crane Base", new Vector2(1480, 200));
        AddHidingSpot(root, "Rubble Heap", new Vector2(2080, 380));
        AddHidingSpot(root, "Quarry Ledge", new Vector2(400, 160));

        // 7 yard patrol guards — wider patrols across the expanded quarry.
        AddGuard(root, "YardGuard1", new Vector2(600, 180), new Vector2[]
        {
            new(0, 0), new(300, 0), new(300, 160), new(0, 160)
        });
        AddGuard(root, "YardGuard2", new Vector2(1960, 180), new Vector2[]
        {
            new(0, 0), new(-300, 0), new(-300, 160), new(0, 160)
        });
        AddGuard(root, "YardGuard3", new Vector2(1280, 360), new Vector2[]
        {
            new(-300, 0), new(300, 0)
        });
        AddGuard(root, "YardGuard4", new Vector2(400, 380), new Vector2[]
        {
            new(0, 0), new(160, 0), new(160, 100), new(0, 100)
        });
        AddGuard(root, "YardGuard5", new Vector2(2160, 380), new Vector2[]
        {
            new(0, 0), new(-160, 0), new(-160, 100), new(0, 100)
        });
        AddGuard(root, "YardGuard6", new Vector2(1000, 240), new Vector2[]
        {
            new(-100, 0), new(100, 0)
        });
        AddGuard(root, "YardGuard7", new Vector2(1560, 240), new Vector2[]
        {
            new(-100, 0), new(100, 0)
        });

        // ── Puzzle 1: Breakable wooden fence hides Tunnel Key ──
        AddBreakableWall(root, "WoodenFence", new Vector2(240, 300), hitsRequired: 1, width: 20f, height: 16f);
        AddKeyChest(root, "TunnelKeyChest", new Vector2(240, 340), "tunnel_key", "Tunnel Key");

        // ── Puzzle 2: Push ore block onto switch to open tunnel entrance gate ──
        var yardSwitch = AddFloorSwitch(root, "OreSwitch", new Vector2(1280, 300), stayPressed: true);
        var tunnelGate = AddPuzzleGate(root, "TunnelEntranceGate", new Vector2(1280, 200), requiredConditions: 1, stayOpen: true, isVertical: false, width: 40f, height: 16f);
        tunnelGate.LinkSwitch(yardSwitch);
        AddPushBlock(root, "OreBlock", new Vector2(1380, 300));

        // ── Bonus: Hidden supplies ──
        AddItemChest(root, "ToolShackChest", new Vector2(2320, 300), "ink", "trace_ink", "Trace Ink");
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
        AddAreaZone(root, "Mining Tunnels", new Vector2(1280, 208), new Vector2(2560, 416),
            isRestricted: true);

        // Shadow alcoves for stealth throughout the expanded tunnels.
        AddShadowZone(root, new Vector2(120, 24), new Vector2(32, 24));
        AddShadowZone(root, new Vector2(120, 388), new Vector2(32, 24));
        AddShadowZone(root, new Vector2(520, 24), new Vector2(32, 24));
        AddShadowZone(root, new Vector2(520, 388), new Vector2(32, 24));
        AddShadowZone(root, new Vector2(900, 24), new Vector2(32, 24));
        AddShadowZone(root, new Vector2(900, 388), new Vector2(32, 24));
        AddShadowZone(root, new Vector2(1280, 100), new Vector2(40, 24));
        AddShadowZone(root, new Vector2(1660, 24), new Vector2(32, 24));
        AddShadowZone(root, new Vector2(1660, 388), new Vector2(32, 24));
        AddShadowZone(root, new Vector2(2040, 24), new Vector2(32, 24));
        AddShadowZone(root, new Vector2(2040, 388), new Vector2(32, 24));
        AddShadowZone(root, new Vector2(2440, 24), new Vector2(32, 24));
        AddShadowZone(root, new Vector2(2440, 388), new Vector2(32, 24));

        // Hiding spots — rubble, support beams, mine carts throughout the network.
        AddHidingSpot(root, "Rubble Pile", new Vector2(400, 208));
        AddHidingSpot(root, "Support Beam", new Vector2(900, 100));
        AddHidingSpot(root, "Collapsed Tunnel", new Vector2(1600, 208));
        AddHidingSpot(root, "Mine Cart", new Vector2(700, 350));
        AddHidingSpot(root, "Ore Chute", new Vector2(1100, 100));
        AddHidingSpot(root, "Tool Cache", new Vector2(1900, 340));
        AddHidingSpot(root, "Rock Overhang", new Vector2(2200, 140));
        AddHidingSpot(root, "Ventilation Shaft", new Vector2(1280, 300));

        // 7 tunnel guards — patrols in the corridors leave windows for sneaking.
        AddGuard(root, "TunnelGuard1", new Vector2(380, 100), new Vector2[]
        {
            new(0, 0), new(0, 220)
        });
        AddGuard(root, "TunnelGuard2", new Vector2(800, 160), new Vector2[]
        {
            new(0, 0), new(0, -80), new(120, -80), new(120, 0)
        });
        AddGuard(root, "TunnelGuard3", new Vector2(1200, 100), new Vector2[]
        {
            new(0, 0), new(0, 220)
        });
        AddGuard(root, "TunnelGuard4", new Vector2(1600, 260), new Vector2[]
        {
            new(0, 0), new(-160, 0), new(-160, 100), new(0, 100)
        });
        AddGuard(root, "TunnelGuard5", new Vector2(2000, 100), new Vector2[]
        {
            new(0, 0), new(0, 200)
        });
        AddGuard(root, "TunnelGuard6", new Vector2(2400, 200), new Vector2[]
        {
            new(-120, 0), new(120, 0)
        });
        AddGuard(root, "TunnelGuard7", new Vector2(1280, 350), new Vector2[]
        {
            new(-200, 0), new(200, 0)
        });

        // ── Puzzle 3: Locked shaft door requires Tunnel Key ──
        AddLockedDoor(root, "ShaftDoor", new Vector2(400, 208), "tunnel_key", isVertical: true);

        // ── Puzzle 4: Lever + push block switch to open Foreman's passage ──
        var tunnelLever = AddLever(root, "TunnelLever", new Vector2(900, 60), oneWay: false);
        var tunnelSwitch = AddFloorSwitch(root, "TunnelSwitch", new Vector2(1600, 300), stayPressed: true);
        var foremanGate = AddPuzzleGate(root, "ForemanGate", new Vector2(2000, 208), requiredConditions: 2, stayOpen: true, isVertical: true, width: 16f, height: 40f);
        foremanGate.LinkLever(tunnelLever);
        foremanGate.LinkSwitch(tunnelSwitch);
        AddPushBlock(root, "MinecartBlock", new Vector2(1500, 300));

        // ── Puzzle 5: Breakable collapsed wall (shortcut) ──
        AddBreakableWall(root, "CollapsedTunnel", new Vector2(1600, 208), hitsRequired: 2, width: 16f, height: 20f);

        // ── Puzzle 6: Foreman Key chest deeper in tunnel ──
        AddKeyChest(root, "ForemanKeyChest", new Vector2(2400, 340), "foreman_key", "Foreman Key");
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
        AddAreaZone(root, "Reeve Maren's Office", new Vector2(1280, 176),
            new Vector2(2560, 352), isRestricted: true);

        // Shadow in corner storage rooms and corridors.
        AddShadowZone(root, new Vector2(430, 320), new Vector2(32, 32));
        AddShadowZone(root, new Vector2(940, 320), new Vector2(32, 32));
        AddShadowZone(root, new Vector2(64, 32), new Vector2(40, 32));
        AddShadowZone(root, new Vector2(1700, 320), new Vector2(32, 32));
        AddShadowZone(root, new Vector2(2200, 320), new Vector2(32, 32));
        AddShadowZone(root, new Vector2(2496, 32), new Vector2(40, 32));
        AddShadowZone(root, new Vector2(1280, 120), new Vector2(40, 32));

        // Hiding spots — filing cabinets, desk alcoves, guard quarters, records room.
        AddHidingSpot(root, "Filing Cabinet", new Vector2(160, 80));
        AddHidingSpot(root, "Under Desk", new Vector2(1100, 80));
        AddHidingSpot(root, "Storage Closet", new Vector2(2400, 176));
        AddHidingSpot(root, "Records Room", new Vector2(600, 280));
        AddHidingSpot(root, "Guard Quarters", new Vector2(1800, 80));
        AddHidingSpot(root, "Trophy Alcove", new Vector2(1400, 280));
        AddHidingSpot(root, "Supply Locker", new Vector2(2100, 280));

        // 4 elite guards — one at each wing entrance plus roaming.
        AddGuard(root, "OfficeElite1", new Vector2(500, 140), new Vector2[]
        {
            new(0, -60), new(0, 80)
        }, elite: true);
        AddGuard(root, "OfficeElite2", new Vector2(2060, 140), new Vector2[]
        {
            new(0, -60), new(0, 80)
        }, elite: true);
        AddGuard(root, "OfficeElite3", new Vector2(1280, 280), new Vector2[]
        {
            new(-200, 0), new(200, 0)
        }, elite: true);
        AddGuard(root, "OfficeElite4", new Vector2(1700, 80), new Vector2[]
        {
            new(-120, 0), new(120, 0)
        }, elite: true);

        // ── Puzzle 7: Locked office door requires Foreman Key ──
        AddLockedDoor(root, "MarenDoor", new Vector2(1280, 320), "foreman_key");

        // ── Puzzle 8: Push block onto switch for records room ──
        var officeSwitch = AddFloorSwitch(root, "RecordsSwitch", new Vector2(600, 240), stayPressed: true);
        var recordsGate = AddPuzzleGate(root, "RecordsGate", new Vector2(600, 300), requiredConditions: 1, stayOpen: true, isVertical: false, width: 32f, height: 16f);
        recordsGate.LinkSwitch(officeSwitch);
        AddPushBlock(root, "FilingBlock", new Vector2(700, 240));

        // ── Puzzle 9: Breakable wall revealing secret intel cache ──
        AddBreakableWall(root, "OfficeWall", new Vector2(2400, 140), hitsRequired: 2, width: 16f, height: 20f);
        AddItemChest(root, "IntelInkChest", new Vector2(2440, 140), "ink", "blood_ink", "Blood Ink");

        // Reeve Maren — seated in the central carpeted study.
        SpawnReeveMaren(root);
    }

    // ═════════════════════════════════════════════════════════════
    //  REEVE MAREN
    // ═════════════════════════════════════════════════════════════

    private void SpawnReeveMaren(Node2D parent)
    {
        var maren = new GuardEnemy { Name = "ReeveMaren" };
        maren.Position = new Vector2(1280, 176);
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

}
