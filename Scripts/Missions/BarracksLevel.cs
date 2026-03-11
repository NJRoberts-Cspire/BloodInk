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

    // Training Yard — ~160 wide × 40 tall — sprawling drill grounds with
    // weapon racks, supply wagons, archery range, stables, and stone perimeter.
    private static readonly string[] YardMap = {
        "################################################################################################################################################################",
        "#pppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppp#",
        "#pp,,,,,,pppppppppp,,,,,,,,pppppppppppp,,,,,,,,pppppppppp,,pppppppppp,,,,,,,,pppppppppppp,,,,,,,,pppppppppp,,,,,,,,pppppppppppp,,,,,,,,pppppppppp,,pp#",
        "#pp,,,,,,pppppppppp,,,,,,,,pppppppppppp,,,,,,,,pppppppppp,,pppppppppp,,,,,,,,pppppppppppp,,,,,,,,pppppppppp,,,,,,,,pppppppppppp,,,,,,,,pppppppppp,,pp#",
        "#pp,,,,,,pppppppppp,,,,,,,,pppppppppppp,,,,,,,,pppppppppp,,pppppppppp,,,,,,,,pppppppppppp,,,,,,,,pppppppppp,,,,,,,,pppppppppppp,,,,,,,,pppppppppp,,pp#",
        "#pp,,,,,,pppppppppp,,,,,,,,pppppppppppp,,,,,,,,pppppppppp,,pppppppppp,,,,,,,,pppppppppppp,,,,,,,,pppppppppp,,,,,,,,pppppppppppp,,,,,,,,pppppppppp,,pp#",
        "#pp,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,pp#",
        "#pp,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,pp#",
        "#pp,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,pp#",
        "#pp,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,pp#",
        "#pp,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,pp#",
        "#pp,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,pp#",
        "#pp,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,pp#",
        "#pp,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,pp#",
        "#pp,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,pp#",
        "#pp,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,pp#",
        "#pp,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,pp#",
        "#pp,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,pp#",
        "#pp,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,pp#",
        "#pp,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,pp#",
        "#pp,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,pp#",
        "#pp,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,pp#",
        "#pp,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,pp#",
        "#pp,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,pp#",
        "#pp,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,pp#",
        "#pp,,,,,,pppppppppp,,,,,,,,pppppppppppp,,,,,,,,pppppppppp,,pppppppppp,,,,,,,,pppppppppppp,,,,,,,,pppppppppp,,,,,,,,pppppppppppp,,,,,,,,pppppppppp,,pp#",
        "#pp,,,,,,pppppppppp,,,,,,,,pppppppppppp,,,,,,,,pppppppppp,,pppppppppp,,,,,,,,pppppppppppp,,,,,,,,pppppppppp,,,,,,,,pppppppppppp,,,,,,,,pppppppppp,,pp#",
        "#pp,,,,,,pppppppppp,,,,,,,,pppppppppppp,,,,,,,,pppppppppp,,pppppppppp,,,,,,,,pppppppppppp,,,,,,,,pppppppppp,,,,,,,,pppppppppppp,,,,,,,,pppppppppp,,pp#",
        "#pp,,,,,,pppppppppp,,,,,,,,pppppppppppp,,,,,,,,pppppppppp,,pppppppppp,,,,,,,,pppppppppppp,,,,,,,,pppppppppp,,,,,,,,pppppppppppp,,,,,,,,pppppppppp,,pp#",
        "#pppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppp#",
        "#pppppppppppppppppppppppppppppppppppppppppppppppp....pppppppppppppppppppppppppppppp....pppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppp#",
        "################################################################################################################################################################",
    };

    // Barracks Interior — ~160 wide × 40 tall — bunks, mess hall, armory,
    // kitchen, latrines, officer lounge, supply rooms, narrow corridors.
    private static readonly string[] InteriorMap = {
        "################################################################################################################################################################",
        "#wwwww#......#......#wwwwwwwww#wwwww#......#......#wwwwwwwwww#wwwww#......#......#wwwwwwwww#wwwww#......#......#wwwwwwwwww#wwwww#......#......#wwwwwwwww#",
        "#wwwww#......#......#wwwwwwwww#wwwww#......#......#wwwwwwwwww#wwwww#......#......#wwwwwwwww#wwwww#......#......#wwwwwwwwww#wwwww#......#......#wwwwwwwww#",
        "#wwwww#......#......#wwwwwwwww#wwwww#......#......#wwwwwwwwww#wwwww#......#......#wwwwwwwww#wwwww#......#......#wwwwwwwwww#wwwww#......#......#wwwwwwwww#",
        "#wwwww#......#......#wwwwwwwww#wwwww#......#......#wwwwwwwwww#wwwww#......#......#wwwwwwwww#wwwww#......#......#wwwwwwwwww#wwwww#......#......#wwwwwwwww#",
        "#wwwww.............................................wwwwwwwwww#wwwww.............................................wwwwwwwwww#wwwww...........................wwwwwwwww#",
        "#wwwww.............................................wwwwwwwwww#wwwww.............................................wwwwwwwwww#wwwww...........................wwwwwwwww#",
        "#wwwww.............................................wwwwwwwwww#wwwww.............................................wwwwwwwwww#wwwww...........................wwwwwwwww#",
        "#wwwww.............................................wwwwwwwwww#wwwww.............................................wwwwwwwwww#wwwww...........................wwwwwwwww#",
        "#wwwww#......#......#wwwwwwwww#wwwww#......#......#wwwwwwwwww#wwwww#......#......#wwwwwwwww#wwwww#......#......#wwwwwwwwww#wwwww#......#......#wwwwwwwww#",
        "#wwwww#......#......#wwwwwwwww#wwwww#......#......#wwwwwwwwww#wwwww#......#......#wwwwwwwww#wwwww#......#......#wwwwwwwwww#wwwww#......#......#wwwwwwwww#",
        "#wwwww#......#......#wwwwwwwww#wwwww#......#......#wwwwwwwwww#wwwww#......#......#wwwwwwwww#wwwww#......#......#wwwwwwwwww#wwwww#......#......#wwwwwwwww#",
        "#wwwww#......#......#wwwwwwwww#wwwww#......#......#wwwwwwwwww#wwwww#......#......#wwwwwwwww#wwwww#......#......#wwwwwwwwww#wwwww#......#......#wwwwwwwww#",
        "#wwwww.............................................wwwwwwwwww#wwwww.............................................wwwwwwwwww#wwwww...........................wwwwwwwww#",
        "#wwwww.............................................wwwwwwwwww#wwwww.............................................wwwwwwwwww#wwwww...........................wwwwwwwww#",
        "#wwwww.............................................wwwwwwwwww#wwwww.............................................wwwwwwwwww#wwwww...........................wwwwwwwww#",
        "#wwwww.............................................wwwwwwwwww#wwwww.............................................wwwwwwwwww#wwwww...........................wwwwwwwww#",
        "#wwwww#......#......#wwwwwwwww#wwwww#......#......#wwwwwwwwww#wwwww#......#......#wwwwwwwww#wwwww#......#......#wwwwwwwwww#wwwww#......#......#wwwwwwwww#",
        "#wwwww#......#......#wwwwwwwww#wwwww#......#......#wwwwwwwwww#wwwww#......#......#wwwwwwwww#wwwww#......#......#wwwwwwwwww#wwwww#......#......#wwwwwwwww#",
        "#wwwww#..~~..#..~~..#wwwwwwwww#wwwww#..~~..#..~~..#wwwwwwwwww#wwwww#..~~..#..~~..#wwwwwwwww#wwwww#..~~..#..~~..#wwwwwwwwww#wwwww#..~~..#..~~..#wwwwwwwww#",
        "#wwwww#......#......#wwwwwwwww#wwwww#......#......#wwwwwwwwww#wwwww#......#......#wwwwwwwww#wwwww#......#......#wwwwwwwwww#wwwww#......#......#wwwwwwwww#",
        "#wwwww.............................................wwwwwwwwww#wwwww.............................................wwwwwwwwww#wwwww...........................wwwwwwwww#",
        "#wwwww.............................................wwwwwwwwww#wwwww.............................................wwwwwwwwww#wwwww...........................wwwwwwwww#",
        "#wwwww.............................................wwwwwwwwww#wwwww.............................................wwwwwwwwww#wwwww...........................wwwwwwwww#",
        "#wwwww.............................................wwwwwwwwww#wwwww.............................................wwwwwwwwww#wwwww...........................wwwwwwwww#",
        "#wwwww#......#......#wwwwwwwww#wwwww#......#......#wwwwwwwwww#wwwww#......#......#wwwwwwwww#wwwww#......#......#wwwwwwwwww#wwwww#......#......#wwwwwwwww#",
        "#wwwww#......#......#wwwwwwwww#wwwww#......#......#wwwwwwwwww#wwwww#......#......#wwwwwwwww#wwwww#......#......#wwwwwwwwww#wwwww#......#......#wwwwwwwww#",
        "#wwwww#......#......#wwwwwwwww#wwwww#......#......#wwwwwwwwww#wwwww#......#......#wwwwwwwww#wwwww#......#......#wwwwwwwwww#wwwww#......#......#wwwwwwwww#",
        "#wwwww#......#......#wwwwwwwww#wwwww#......#......#wwwwwwwwww#wwwww#......#......#wwwwwwwww#wwwww#......#......#wwwwwwwwww#wwwww#......#......#wwwwwwwww#",
        "################################################################################################################################################################",
    };

    // Trophy Room — ~160 wide × 28 tall — Captain Thorne's private sanctum with
    // display cases, war trophies, private dining room, study, and armory.
    private static readonly string[] TrophyMap = {
        "################################################################################################################################################################",
        "#cccccccccccc#~~~~#cccccccccccc#cccccccccccc#~~~~#cccccccccccc#cccccccccccc#~~~~#cccccccccccc#cccccccccccc#~~~~#cccccccccccc#cccccccccccc#~~~~#cccccccccccc#",
        "#cccccccccccc#~~~~#cccccccccccc#cccccccccccc#~~~~#cccccccccccc#cccccccccccc#~~~~#cccccccccccc#cccccccccccc#~~~~#cccccccccccc#cccccccccccc#~~~~#cccccccccccc#",
        "#cccccccccccc#~~~~#cccccccccccc#cccccccccccc#~~~~#cccccccccccc#cccccccccccc#~~~~#cccccccccccc#cccccccccccc#~~~~#cccccccccccc#cccccccccccc#~~~~#cccccccccccc#",
        "#cccccccccccc#....#cccccccccccc#cccccccccccc#....#cccccccccccc#cccccccccccc#....#cccccccccccc#cccccccccccc#....#cccccccccccc#cccccccccccc#....#cccccccccccc#",
        "#cccccccccccc......cccccccccccc.cccccccccccc......cccccccccccc#cccccccccccc......cccccccccccc.cccccccccccc......cccccccccccc#cccccccccccc......cccccccccccc#",
        "#cccccccccccc......cccccccccccc.cccccccccccc......cccccccccccc#cccccccccccc......cccccccccccc.cccccccccccc......cccccccccccc#cccccccccccc......cccccccccccc#",
        "#cccccccccccc......cccccccccccc.cccccccccccc......cccccccccccc#cccccccccccc......cccccccccccc.cccccccccccc......cccccccccccc#cccccccccccc......cccccccccccc#",
        "#cccccccccccc......cccccccccccc.cccccccccccc......cccccccccccc#cccccccccccc......cccccccccccc.cccccccccccc......cccccccccccc#cccccccccccc......cccccccccccc#",
        "#cccccccccccc......cccccccccccc.cccccccccccc......cccccccccccc.cccccccccccc......cccccccccccc.cccccccccccc......cccccccccccc.cccccccccccc......cccccccccccc#",
        "#cccccccccccc......cccccccccccc.cccccccccccc......cccccccccccc.cccccccccccc......cccccccccccc.cccccccccccc......cccccccccccc.cccccccccccc......cccccccccccc#",
        "#cccccccccccc......cccccccccccc.cccccccccccc......cccccccccccc.cccccccccccc......cccccccccccc.cccccccccccc......cccccccccccc.cccccccccccc......cccccccccccc#",
        "#cccccccccccc......cccccccccccc.cccccccccccc......cccccccccccc.cccccccccccc......cccccccccccc.cccccccccccc......cccccccccccc.cccccccccccc......cccccccccccc#",
        "#cccccccccccc......cccccccccccc.cccccccccccc......cccccccccccc#cccccccccccc......cccccccccccc.cccccccccccc......cccccccccccc#cccccccccccc......cccccccccccc#",
        "#cccccccccccc......cccccccccccc.cccccccccccc......cccccccccccc#cccccccccccc......cccccccccccc.cccccccccccc......cccccccccccc#cccccccccccc......cccccccccccc#",
        "#cccccccccccc......cccccccccccc.cccccccccccc......cccccccccccc#cccccccccccc......cccccccccccc.cccccccccccc......cccccccccccc#cccccccccccc......cccccccccccc#",
        "#cccccccccccc......cccccccccccc.cccccccccccc......cccccccccccc#cccccccccccc......cccccccccccc.cccccccccccc......cccccccccccc#cccccccccccc......cccccccccccc#",
        "#cccccccccccc#....#cccccccccccc#cccccccccccc#....#cccccccccccc#cccccccccccc#....#cccccccccccc#cccccccccccc#....#cccccccccccc#cccccccccccc#....#cccccccccccc#",
        "#cccccccccccc#~~~~#cccccccccccc#cccccccccccc#~~~~#cccccccccccc#cccccccccccc#~~~~#cccccccccccc#cccccccccccc#~~~~#cccccccccccc#cccccccccccc#~~~~#cccccccccccc#",
        "#cccccccccccc#~~~~#cccccccccccc#cccccccccccc#~~~~#cccccccccccc#cccccccccccc#~~~~#cccccccccccc#cccccccccccc#~~~~#cccccccccccc#cccccccccccc#~~~~#cccccccccccc#",
        "#cccccccccccc#~~~~#cccccccccccc#cccccccccccc#~~~~#cccccccccccc#cccccccccccc#~~~~#cccccccccccc#cccccccccccc#~~~~#cccccccccccc#cccccccccccc#~~~~#cccccccccccc#",
        "################################################################################################################################################################",
    };

    private static readonly Vector2 YardOffset = new(0, 1008);
    private static readonly Vector2 InteriorOffset = new(0, 0);
    private static readonly Vector2 TrophyOffset = new(0, -640);

    public override void _Ready()
    {
        PlaceholderSprites.CreateAll();

        BuildYard();
        BuildInterior();
        BuildTrophyRoom();
        SpawnPlayer(YardOffset + new Vector2(1280, 420));
        SetupHUD();
        RegisterTargets();

        // Camera bounds encompass all three zones (Trophy top → Yard bottom).
        SetCameraLimits(0, -640, 2560, 1520);

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
        AddAreaZone(root, "Training Yard", new Vector2(1280, 264), new Vector2(2560, 528));

        // Shadow behind the equipment racks and stables.
        AddShadowZone(root, new Vector2(96, 80), new Vector2(48, 64));
        AddShadowZone(root, new Vector2(2464, 80), new Vector2(48, 64));
        AddShadowZone(root, new Vector2(96, 420), new Vector2(48, 64));
        AddShadowZone(root, new Vector2(2464, 420), new Vector2(48, 64));
        AddShadowZone(root, new Vector2(800, 220), new Vector2(40, 40));
        AddShadowZone(root, new Vector2(1760, 220), new Vector2(40, 40));
        AddShadowZone(root, new Vector2(1280, 100), new Vector2(48, 40));

        // Hiding spots — weapon racks, crates, training dummies, stables.
        AddHidingSpot(root, "Weapon Rack", new Vector2(160, 160));
        AddHidingSpot(root, "Supply Crates", new Vector2(2400, 160));
        AddHidingSpot(root, "Training Dummy", new Vector2(600, 300));
        AddHidingSpot(root, "Equipment Shed", new Vector2(1800, 300));
        AddHidingSpot(root, "Hay Bales", new Vector2(400, 420));
        AddHidingSpot(root, "Archery Range Cover", new Vector2(1000, 200));
        AddHidingSpot(root, "Stable Stall", new Vector2(1600, 200));
        AddHidingSpot(root, "Siege Engine", new Vector2(1280, 360));
        AddHidingSpot(root, "Watchtower Base", new Vector2(2200, 420));

        // 8 guards patrolling the open yard — the most dangerous zone.
        AddGuard(root, "YardGuard1", new Vector2(400, 140), new Vector2[]
        {
            new(0, 0), new(300, 0), new(300, 160), new(0, 160)
        });
        AddGuard(root, "YardGuard2", new Vector2(2160, 140), new Vector2[]
        {
            new(0, 0), new(-300, 0), new(-300, 160), new(0, 160)
        });
        AddGuard(root, "YardGuard3", new Vector2(1280, 400), new Vector2[]
        {
            new(-400, 0), new(400, 0)
        });
        AddGuard(root, "YardGuard4", new Vector2(640, 260), new Vector2[]
        {
            new(0, 0), new(0, 120), new(120, 120), new(120, 0)
        });
        AddGuard(root, "YardGuard5", new Vector2(1920, 260), new Vector2[]
        {
            new(0, 0), new(-150, 0), new(-150, -80), new(0, -80)
        });
        AddGuard(root, "YardGuard6", new Vector2(1000, 160), new Vector2[]
        {
            new(0, 0), new(200, 0), new(200, 140), new(0, 140)
        });
        AddGuard(root, "YardGuard7", new Vector2(1560, 160), new Vector2[]
        {
            new(0, 0), new(-200, 0), new(-200, 140), new(0, 140)
        });
        AddGuard(root, "YardGuard8", new Vector2(1280, 200), new Vector2[]
        {
            new(-120, 0), new(120, 0)
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
        AddAreaZone(root, "Barracks", new Vector2(1280, 240), new Vector2(2560, 480),
            isRestricted: true);

        // Shadow in bunk corners and corridors.
        AddShadowZone(root, new Vector2(200, 480), new Vector2(32, 32));
        AddShadowZone(root, new Vector2(500, 480), new Vector2(32, 32));
        AddShadowZone(root, new Vector2(1100, 480), new Vector2(32, 32));
        AddShadowZone(root, new Vector2(1460, 480), new Vector2(32, 32));
        AddShadowZone(root, new Vector2(1280, 140), new Vector2(40, 32));
        AddShadowZone(root, new Vector2(2000, 240), new Vector2(32, 32));
        AddShadowZone(root, new Vector2(600, 240), new Vector2(32, 32));

        // Hiding spots — under bunks, supply rooms, kitchen, armory.
        AddHidingSpot(root, "Under Bunks", new Vector2(160, 120));
        AddHidingSpot(root, "Supply Room", new Vector2(2400, 240));
        AddHidingSpot(root, "Mess Table", new Vector2(800, 380));
        AddHidingSpot(root, "Locker Room", new Vector2(1600, 120));
        AddHidingSpot(root, "Behind Barrels", new Vector2(500, 240));
        AddHidingSpot(root, "Kitchen Pantry", new Vector2(1000, 120));
        AddHidingSpot(root, "Armory Rack", new Vector2(2100, 120));
        AddHidingSpot(root, "Latrine Stall", new Vector2(300, 380));
        AddHidingSpot(root, "Officer Lounge", new Vector2(1900, 380));

        // 8 interior guards — 5 patrol, 3 elite standing.
        AddGuard(root, "BunkGuard1", new Vector2(400, 120), new Vector2[]
        {
            new(0, 0), new(0, 200), new(200, 200), new(200, 0)
        });
        AddGuard(root, "BunkGuard2", new Vector2(2160, 120), new Vector2[]
        {
            new(0, 0), new(0, 200), new(-200, 200), new(-200, 0)
        });
        AddGuard(root, "BunkGuard3", new Vector2(1280, 380), new Vector2[]
        {
            new(-300, 0), new(300, 0)
        });
        AddGuard(root, "BunkGuard4", new Vector2(800, 240), new Vector2[]
        {
            new(0, -100), new(0, 100)
        });
        AddGuard(root, "BunkGuard5", new Vector2(1760, 240), new Vector2[]
        {
            new(0, -100), new(0, 100)
        });
        AddGuard(root, "CorridorElite1", new Vector2(600, 160), new Vector2[]
        {
            new(-120, 0), new(120, 0)
        }, elite: true);
        AddGuard(root, "CorridorElite2", new Vector2(1960, 380), new Vector2[]
        {
            new(-120, 0), new(120, 0)
        }, elite: true);
        AddGuard(root, "CorridorElite3", new Vector2(1280, 160), new Vector2[]
        {
            new(-100, 0), new(100, 0)
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
        AddAreaZone(root, "Trophy Room", new Vector2(1280, 176), new Vector2(2560, 352),
            isRestricted: true);

        // Heavy shadow around the trophies.
        AddShadowZone(root, new Vector2(400, 40), new Vector2(80, 40));
        AddShadowZone(root, new Vector2(2160, 40), new Vector2(80, 40));
        AddShadowZone(root, new Vector2(400, 280), new Vector2(80, 40));
        AddShadowZone(root, new Vector2(2160, 280), new Vector2(80, 40));
        AddShadowZone(root, new Vector2(1200, 160), new Vector2(64, 40));
        AddShadowZone(root, new Vector2(800, 100), new Vector2(48, 40));
        AddShadowZone(root, new Vector2(1760, 100), new Vector2(48, 40));

        // Hiding spots — trophy cases, tapestries, weapon display, private study.
        AddHidingSpot(root, "Trophy Case", new Vector2(160, 176));
        AddHidingSpot(root, "Tapestry", new Vector2(2400, 176));
        AddHidingSpot(root, "Weapon Display", new Vector2(600, 60));
        AddHidingSpot(root, "Dark Alcove", new Vector2(1960, 280));
        AddHidingSpot(root, "Private Study Desk", new Vector2(1000, 60));
        AddHidingSpot(root, "War Banner", new Vector2(1560, 60));
        AddHidingSpot(root, "Armor Stand", new Vector2(800, 280));

        // 5 elite bodyguards — the toughest protection in the game.
        AddGuard(root, "Bodyguard1", new Vector2(500, 176), new Vector2[]
        {
            new(0, -80), new(0, 80)
        }, elite: true);
        AddGuard(root, "Bodyguard2", new Vector2(2060, 176), new Vector2[]
        {
            new(0, -80), new(0, 80)
        }, elite: true);
        AddGuard(root, "Bodyguard3", new Vector2(1280, 80), new Vector2[]
        {
            new(-200, 0), new(200, 0)
        }, elite: true);
        AddGuard(root, "Bodyguard4", new Vector2(800, 120), new Vector2[]
        {
            new(0, -60), new(0, 60)
        }, elite: true);
        AddGuard(root, "Bodyguard5", new Vector2(1760, 120), new Vector2[]
        {
            new(0, -60), new(0, 60)
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
        thorne.Position = new Vector2(1280, 176);
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
