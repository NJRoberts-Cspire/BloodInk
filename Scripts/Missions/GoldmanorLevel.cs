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

    // Garden: ~160 wide × 50 tall — sprawling estate grounds with hedge maze,
    // ornamental pond, servant paths, groundskeeper shed, and perimeter wall.
    private static readonly string[] GardenMap = {
        ",,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,",
        ",,~,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,~,,,,",
        ",,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,",
        ",,,,,,,,,,,,,,,,pppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppp,,,,,,,,,,,,,,",
        ",,,,,,,,,,,,,,,pp......................................................................................................................pp,,,,,,,,,,,,,",
        ",,,,,,,,,,,,,,pp........................................................................................................................pp,,,,,,,,,,,,",
        ",,,,,,,,,,,,,pp..........................................................................................................................pp,,,,,,,,,,,",
        ",,,,,,,,,,,,pp............................................................................................................................pp,,,,,,,,,,",
        ",,,,,,,,,,,pp..............,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,.................,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,..............pp,,,,,,,,,,,",
        ",,,,,,,,,,pp...............,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,.................,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,...............pp,,,,,,,,,,",
        ",,,,,,,,,pp................,,,,~,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,..................,,,,~,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,................pp,,,,,,,,,",
        ",,,,,,,,pp.................,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,...................,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,.................pp,,,,,,,,",
        ",,,,,,,pp..................,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,....................,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,..................pp,,,,,,,",
        ",,,,,,pp...................,,,,,,,,,,,,,,,,,,pppppppppppppppp,,,,,,,.....................,,,,,,,,,,,pppppppppppppppp,,,,,,,,,...................pp,,,,,,",
        ",,,,,pp....................,,,,,,,,,,,,,,,,pp................pp,,,,,,....................,,,,,,,,pp................pp,,,,,,,....................pp,,,,,",
        ",,~,,pp....................,,,,,,,,,,,,,,pp..................pp,,,,,,.....................,,,,,,pp..................pp,,,,,,....................pp,,~,,",
        ",,,,,pp....................,,,,,,,,,,,,,pp....................pp,,,,,.....................,,,,,pp....................pp,,,,,....................pp,,,,,",
        ",,,,,pp....................,,,,,,~,,,,,pp......................pp,,,,....................,,,,pp......................pp,,,,....................pp,,,,,",
        ",,,,,pp....................,,,,,,,,,,,,p........................p,,,,....................,,,,p........................p,,,,....................pp,,,,,",
        ",,,,,pp....................,,,,,,,,,,,,p........................p,,,,....................,,,,p........................p,,,,....................pp,,,,,",
        ",,,,,pp....................,,,,,,,,,,,,p........................p,,,,....................,,,,p........................p,,,,....................pp,,,,,",
        ",,,,,pp....................,,,,,,,,,,,,p........................p,,,,....................,,,,p........................p,,,,....................pp,,,,,",
        ",,,,,pp....................,,,,,,,,,,,,pp......................pp,,,,,....................,,,pp......................pp,,,,,....................pp,,,,,",
        ",,,,,,pp...................,,,,,,,,,,,,,,pp..................pp,,,,,,,.....................,,,,pp..................pp,,,,,,,...................pp,,,,,,",
        ",,,,,,,pp..................,,,,,,,,,,,,,,,,pp..............pp,,,,,,,,,....................,,,,,,pp..............pp,,,,,,,,,.................pp,,,,,,,",
        ",,,,,,,,pp.................,,,,,,,,,,,,,,,,,,pp..........pp,,,,,,,,,,,....................,,,,,,,pp..........pp,,,,,,,,,,,................pp,,,,,,,,",
        ",,,,,,,,,pp................,,,,,,,,,,,,,,,,,,,,pp......pp,,,,,,,,,,,,,...................,,,,,,,,,,pp......pp,,,,,,,,,,,,,...............pp,,,,,,,,,",
        ",,,,,,,,,,pp...............,,,,,,,,,,,,,,,,,,,,,,,pppp,,,,,,,,,,,,,,,,,..................,,,,,,,,,,,,,pppp,,,,,,,,,,,,,,,,..............pp,,,,,,,,,,",
        ",,,,,,,,,,,pp..............,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,.................,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,.............pp,,,,,,,,,,,",
        ",,,,,,,,,,,,pp.............,,,,,,,,,,~,,,,,,,,,,,,,,,,,,,,,,,~,,,,,,,,,..............,,,,,,,,,,~,,,,,,,,,,,,,,,,,,,,,~,,,,,,,........pp,,,,,,,,,,,,",
        ",,,,,,,,,,,,,pp..............................................................................................................pp,,,,,,,,,,,,,",
        ",,,,,,,,,,,,,,pp............................................................................................................pp,,,,,,,,,,,,,,",
        ",,,,,,,,,,,,,,,pp..........................................................................................................pp,,,,,,,,,,,,,,,",
        ",,,,,,,,,,,,,,,,pp........................................................................................................pp,,,,,,,,,,,,,,,,",
        ",,,,,,,,,,,,,,,,pp........................................................................................................pp,,,,,,,,,,,,,,,,",
        ",,,,,,,,,,,,,,,pp..............,,,,,,,,........,,,,,,,,........,,,,,,,,........,,,,,,,,........,,,,,,,,........,,,,,,,,........pp,,,,,,,,,,,,,,,",
        ",,,,,,,,,,,,,,pp...............,,,,,,,,.........,,,,,,,,........,,,,,,,,........,,,,,,,,........,,,,,,,,........,,,,,,,,........pp,,,,,,,,,,,,,,",
        ",,,,,,,,,,,,,pp................,,,,,,,,..........,,,,,,,,........,,,,,,,,........,,,,,,,,........,,,,,,,,........,,,,,,,,........pp,,,,,,,,,,,,,",
        ",,,,,,,,,,,,pp.................,,,,,,,,...........,,,,,,,,........,,,,,,,,........,,,,,,,,........,,,,,,,,........,,,,,,,,........pp,,,,,,,,,,,,",
        ",,,,,,,,,,,pp................................................................................pp....pppppppppppppppppppppp........pp,,,,,,,,,,,",
        ",,,,,,,,,,pp.................................................................................pp..................................pp,,,,,,,,,,",
        ",,,,,,,,,pp.....................................................................................................................pp,,,,,,,,,",
        ",,,,,,,,pp.....................................................................................................................pp,,,,,,,,",
        ",,,,,,,pp.......................................................................................................................pp,,,,,,,",
        ",,,,,,pp.........................................................................................................................pp,,,,,,",
        ",,,,,pp...........................................................................................................................pp,,,,,",
        ",,~,,pppppppppppppppppppppppppppppppppppppppppppppp....pppppppppppppppppppppppppppppppppp....pppppppppppppppppppppppppppppppppppppppp,,~,,",
        ",,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,",
        ",,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,",
        ",,~,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,~,,,,",
    };

    // Main Hall: ~160 wide × 50 tall — grand entrance hall with dining rooms,
    // servant passages, kitchens, wine cellar stairs, reception chambers, and a
    // long carpeted processional aisle. Rooms have purpose: people live here.
    private static readonly string[] HallMap = {
        "################################################################################################################################################################",
        "#wwwwww#........#wwwwwwww#..........#wwwwwwww#........#wwwwwwww#wwwwwwww#........#..........#wwwwwwww#........#wwwwwwww#..........#wwwwwwww#........#wwwwwwww#",
        "#wwwwww#........#wwwwwwww#..........#wwwwwwww#........#wwwwwwww#wwwwwwww#........#..........#wwwwwwww#........#wwwwwwww#..........#wwwwwwww#........#wwwwwwww#",
        "#wwwwww#........#wwwwwwww#..........#wwwwwwww#........#wwwwwwww#wwwwwwww#........#..........#wwwwwwww#........#wwwwwwww#..........#wwwwwwww#........#wwwwwwww#",
        "#wwwwww#........#wwwwwwww#..........#wwwwwwww#........#wwwwwwww#wwwwwwww#........#..........#wwwwwwww#........#wwwwwwww#..........#wwwwwwww#........#wwwwwwww#",
        "#wwwwww....cc....wwwwwwww#....cc....#wwwwwwww....cc....wwwwwwww#wwwwwwww....cc....wwwwwwwwww....cc....wwwwwwww....cc....wwwwwwwwww....cc....wwwwwwww#",
        "#wwwwww....cc....wwwwwwww#....cc....#wwwwwwww....cc....wwwwwwww#wwwwwwww....cc....wwwwwwwwww....cc....wwwwwwww....cc....wwwwwwwwww....cc....wwwwwwww#",
        "#wwwwww#........#wwwwwwww#..........#wwwwwwww#........#wwwwwwww#wwwwwwww#........#..........#wwwwwwww#........#wwwwwwww#..........#wwwwwwww#........#wwwwwwww#",
        "#wwwwww#........#wwwwwwww#..........#wwwwwwww#........#wwwwwwww#wwwwwwww#........#..........#wwwwwwww#........#wwwwwwww#..........#wwwwwwww#........#wwwwwwww#",
        "#wwwwww#........#........#..........#........#........#wwwwwwww#........#........#..........#........#........#........#..........#........#........#wwwwwwww#",
        "#wwwwww#........#........#..........#........#........#wwwwwwww#........#........#..........#........#........#........#..........#........#........#wwwwwwww#",
        "#......#........#........#cccccccccc#........#........#........#........#........#cccccccccc#........#........#........#cccccccccc#........#........#........#",
        "#......#........#........#cccccccccc#........#........#........#........#........#cccccccccc#........#........#........#cccccccccc#........#........#........#",
        "#......................................................................................................................................................................................................................................#",
        "#......................................................................................................................................................................................................................................#",
        "#......................................................................................................................................................................................................................................#",
        "#......#........#........#cccccccccc#........#........#........#........#........#cccccccccc#........#........#........#cccccccccc#........#........#........#",
        "#......#........#........#cccccccccc#........#........#........#........#........#cccccccccc#........#........#........#cccccccccc#........#........#........#",
        "#wwwwww#........#........#..........#........#........#wwwwwwww#........#........#..........#........#........#........#..........#........#........#wwwwwwww#",
        "#wwwwww#........#........#..........#........#........#wwwwwwww#........#........#..........#........#........#........#..........#........#........#wwwwwwww#",
        "#wwwwww#........#wwwwwwww#..........#wwwwwwww#........#wwwwwwww#wwwwwwww#........#..........#wwwwwwww#........#wwwwwwww#..........#wwwwwwww#........#wwwwwwww#",
        "#wwwwww#........#wwwwwwww#..........#wwwwwwww#........#wwwwwwww#wwwwwwww#........#..........#wwwwwwww#........#wwwwwwww#..........#wwwwwwww#........#wwwwwwww#",
        "#wwwwww....cc....wwwwwwww#....cc....#wwwwwwww....cc....wwwwwwww#wwwwwwww....cc....wwwwwwwwww....cc....wwwwwwww....cc....wwwwwwwwww....cc....wwwwwwww#",
        "#wwwwww....cc....wwwwwwww#....cc....#wwwwwwww....cc....wwwwwwww#wwwwwwww....cc....wwwwwwwwww....cc....wwwwwwww....cc....wwwwwwwwww....cc....wwwwwwww#",
        "#wwwwww#........#wwwwwwww#..........#wwwwwwww#........#wwwwwwww#wwwwwwww#........#..........#wwwwwwww#........#wwwwwwww#..........#wwwwwwww#........#wwwwwwww#",
        "#wwwwww#...cc...#wwwwwwww#..........#wwwwwwww#...cc...#wwwwwwww#wwwwwwww#...cc...#..........#wwwwwwww#...cc...#wwwwwwww#..........#wwwwwwww#...cc...#wwwwwwww#",
        "#wwwwww#...cc...#wwwwwwww#..........#wwwwwwww#...cc...#wwwwwwww#wwwwwwww#...cc...#..........#wwwwwwww#...cc...#wwwwwwww#..........#wwwwwwww#...cc...#wwwwwwww#",
        "#wwwwww#........#wwwwwwww#..........#wwwwwwww#........#wwwwwwww#wwwwwwww#........#..........#wwwwwwww#........#wwwwwwww#..........#wwwwwwww#........#wwwwwwww#",
        "#wwwwww#........#wwwwwwww#..........#wwwwwwww#........#wwwwwwww#wwwwwwww#........#..........#wwwwwwww#........#wwwwwwww#..........#wwwwwwww#........#wwwwwwww#",
        "#......#..~~....#........#..........#........#....~~..#........#........#..~~....#..........#........#....~~..#........#..........#........#..~~....#........#",
        "#......#........#........#..........#........#........#........#........#........#..........#........#........#........#..........#........#........#........#",
        "#......#........#........#..........#........#........#........#........#........#..........#........#........#........#..........#........#........#........#",
        "#......#........#........#..........#........#........#........#........#........#..........#........#........#........#..........#........#........#........#",
        "#......#........#........#..........#........#........#........#........#........#..........#........#........#........#..........#........#........#........#",
        "#..............................................................................................................................................................................................................................................#",
        "#..............................................................................................................................................................................................................................................#",
        "#......#........#........#..........#........#........#........#........#........#..........#........#........#........#..........#........#........#........#",
        "#......#........#........#..........#........#........#........#........#........#..........#........#........#........#..........#........#........#........#",
        "#......#........#........#..........#........#........#........#........#........#..........#........#........#........#..........#........#........#........#",
        "#......#..~~....#........#~~........#........#....~~..#........#........#..~~....#~~........#........#....~~..#........#~~........#........#..~~....#........#",
        "#wwwwww#........#wwwwwwww#..........#wwwwwwww#........#wwwwwwww#wwwwwwww#........#..........#wwwwwwww#........#wwwwwwww#..........#wwwwwwww#........#wwwwwwww#",
        "#wwwwww#........#wwwwwwww#..........#wwwwwwww#........#wwwwwwww#wwwwwwww#........#..........#wwwwwwww#........#wwwwwwww#..........#wwwwwwww#........#wwwwwwww#",
        "#wwwwww#........#wwwwwwww#..........#wwwwwwww#........#wwwwwwww#wwwwwwww#........#..........#wwwwwwww#........#wwwwwwww#..........#wwwwwwww#........#wwwwwwww#",
        "#wwwwww#........#wwwwwwww#..........#wwwwwwww#........#wwwwwwww#wwwwwwww#........#..........#wwwwwwww#........#wwwwwwww#..........#wwwwwwww#........#wwwwwwww#",
        "################################################################################################################################################################",
    };

    // Lord Cowl's Quarters: ~160 wide × 40 tall — private wing with study, bedroom,
    // trophy gallery, wine cellar, private chapel, servant stairs, wardrobe, balcony.
    private static readonly string[] QuartersMap = {
        "################################################################################################################################################################",
        "#cccccccccc#wwwwwwww#........#wwwwwwww#~~wwwwwwwwww#wwwwwwwwww#cccccccccc#wwwwwwww#........#wwwwwwww#~~wwwwwwwwww#wwwwwwwwww#cccccccccc#wwwwwwww#........#wwwwww#",
        "#cccccccccc#wwwwwwww#........#wwwwwwww#~~wwwwwwwwww#wwwwwwwwww#cccccccccc#wwwwwwww#........#wwwwwwww#~~wwwwwwwwww#wwwwwwwwww#cccccccccc#wwwwwwww#........#wwwwww#",
        "#cccccccccc#wwwwwwww#........#wwwwwwww#wwwwwwwwwwww#wwwwwwwwww#cccccccccc#wwwwwwww#........#wwwwwwww#wwwwwwwwwwww#wwwwwwwwww#cccccccccc#wwwwwwww#........#wwwwww#",
        "#cccccccccc#wwwwwwww#........#wwwwwwww#wwwwwwwwwwww#wwwwwwwwww#cccccccccc#wwwwwwww#........#wwwwwwww#wwwwwwwwwwww#wwwwwwwwww#cccccccccc#wwwwwwww#........#wwwwww#",
        "#cccccccccc....wwwww#........#wwwwwwww#wwwwwwwwwwww....wwwwwww#cccccccccc....wwwww#........#wwwwwwww#wwwwwwwwwwww....wwwwwww#cccccccccc....wwwww#........#wwwwww#",
        "#cccccccccc....wwwww#........#wwwwwwww#wwwwwwwwwwww....wwwwwww#cccccccccc....wwwww#........#wwwwwwww#wwwwwwwwwwww....wwwwwww#cccccccccc....wwwww#........#wwwwww#",
        "#cccccccccc#wwwwwwww#........#wwwwwwww#wwwwwwwwwwww#wwwwwwwwww#cccccccccc#wwwwwwww#........#wwwwwwww#wwwwwwwwwwww#wwwwwwwwww#cccccccccc#wwwwwwww#........#wwwwww#",
        "#cccccccccc#wwwwwwww#........#........#wwwwwwwwwwww#wwwwwwwwww#cccccccccc#wwwwwwww#........#........#wwwwwwwwwwww#wwwwwwwwww#cccccccccc#wwwwwwww#........#......#",
        "#cccccccccc#wwwwwwww#........#........#wwwwwwwwwwww#wwwwwwwwww#cccccccccc#wwwwwwww#........#........#wwwwwwwwwwww#wwwwwwwwww#cccccccccc#wwwwwwww#........#......#",
        "#..........#wwwwwwww#........#........#wwwwwwwwwwww#..........#..........#wwwwwwww#........#........#wwwwwwwwwwww#..........#..........#wwwwwwww#........#......#",
        "#..........#wwwwwwww#........#........#wwwwwwwwwwww#..........#..........#wwwwwwww#........#........#wwwwwwwwwwww#..........#..........#wwwwwwww#........#......#",
        "#..............................................................................................................................................................................................................................................#",
        "#..............................................................................................................................................................................................................................................#",
        "#..............................................................................................................................................................................................................................................#",
        "#..............................................................................................................................................................................................................................................#",
        "#..........#wwwwwwww#........#........#wwwwwwwwwwww#..........#..........#wwwwwwww#........#........#wwwwwwwwwwww#..........#..........#wwwwwwww#........#......#",
        "#..........#wwwwwwww#........#........#wwwwwwwwwwww#..........#..........#wwwwwwww#........#........#wwwwwwwwwwww#..........#..........#wwwwwwww#........#......#",
        "#cccccccccc#wwwwwwww#........#........#wwwwwwwwwwww#wwwwwwwwww#cccccccccc#wwwwwwww#........#........#wwwwwwwwwwww#wwwwwwwwww#cccccccccc#wwwwwwww#........#......#",
        "#cccccccccc#wwwwwwww#........#........#wwwwwwwwwwww#wwwwwwwwww#cccccccccc#wwwwwwww#........#........#wwwwwwwwwwww#wwwwwwwwww#cccccccccc#wwwwwwww#........#......#",
        "#cccccccccc#wwwwwwww....wwwww#wwwwwwww....wwwwwwwww#wwwwwwwwww#cccccccccc#wwwwwwww....wwwww#wwwwwwww....wwwwwwwww#wwwwwwwwww#cccccccccc#wwwwwwww....wwwww#......#",
        "#cccccccccc#wwwwwwww....wwwww#wwwwwwww....wwwwwwwww#wwwwwwwwww#cccccccccc#wwwwwwww....wwwww#wwwwwwww....wwwwwwwww#wwwwwwwwww#cccccccccc#wwwwwwww....wwwww#......#",
        "#cccccccccc#wwwwwwww#wwwwwwww#wwwwwwww#~~wwwwwwwwww#wwwwwwwwww#cccccccccc#wwwwwwww#wwwwwwww#wwwwwwww#~~wwwwwwwwww#wwwwwwwwww#cccccccccc#wwwwwwww#wwwwwwww#......#",
        "#cccccccccc#wwwwwwww#wwwwwwww#wwwwwwww#~~wwwwwwwwww#wwwwwwwwww#cccccccccc#wwwwwwww#wwwwwwww#wwwwwwww#~~wwwwwwwwww#wwwwwwwwww#cccccccccc#wwwwwwww#wwwwwwww#......#",
        "#cccccccccc#wwwwwwww#wwwwwwww#wwwwwwww#wwwwwwwwwwww#wwwwwwwwww#cccccccccc#wwwwwwww#wwwwwwww#wwwwwwww#wwwwwwwwwwww#wwwwwwwwww#cccccccccc#wwwwwwww#wwwwwwww#wwwwww#",
        "#cccccccccc#wwwwwwww#wwwwwwww#wwwwwwww#wwwwwwwwwwww#wwwwwwwwww#cccccccccc#wwwwwwww#wwwwwwww#wwwwwwww#wwwwwwwwwwww#wwwwwwwwww#cccccccccc#wwwwwwww#wwwwwwww#wwwwww#",
        "#......#........#........#..........#........#........#........#........#........#..........#........#........#........#..........#........#........#........#",
        "#......#........#........#..........#........#........#........#........#........#..........#........#........#........#..........#........#........#........#",
        "#......#........#........#..........#........#........#........#........#........#..........#........#........#........#..........#........#........#........#",
        "#..............................................................................................................................................................................................................................................#",
        "#..............................................................................................................................................................................................................................................#",
        "#......#........#........#..........#........#........#........#........#........#..........#........#........#........#..........#........#........#........#",
        "#......#........#........#..........#........#........#........#........#........#..........#........#........#........#..........#........#........#........#",
        "#......#..~~....#........#~~........#........#....~~..#........#........#..~~....#~~........#........#....~~..#........#~~........#........#..~~....#........#",
        "#wwwwww#........#wwwwwwww#..........#wwwwwwww#........#wwwwwwww#wwwwwwww#........#..........#wwwwwwww#........#wwwwwwww#..........#wwwwwwww#........#wwwwwwww#",
        "#wwwwww#........#wwwwwwww#..........#wwwwwwww#........#wwwwwwww#wwwwwwww#........#..........#wwwwwwww#........#wwwwwwww#..........#wwwwwwww#........#wwwwwwww#",
        "#wwwwww#........#wwwwwwww#..........#wwwwwwww#........#wwwwwwww#wwwwwwww#........#..........#wwwwwwww#........#wwwwwwww#..........#wwwwwwww#........#wwwwwwww#",
        "#wwwwww#........#wwwwwwww#..........#wwwwwwww#........#wwwwwwww#wwwwwwww#........#..........#wwwwwwww#........#wwwwwwww#..........#wwwwwwww#........#wwwwwwww#",
        "################################################################################################################################################################",
    };

    // Zone offsets — larger maps need more vertical spacing.
    private static readonly Vector2 GardenOffset = new(0, 1440);    // 50 rows × 16 + gap
    private static readonly Vector2 HallOffset = new(0, 0);         // Hall in middle
    private static readonly Vector2 QuartersOffset = new(0, -1040); // 38 rows × 16 + gap

    public override void _Ready()
    {
        PlaceholderSprites.CreateAll();

        BuildGardens();
        BuildMainHall();
        BuildQuarters();
        SpawnPlayer(GardenOffset + new Vector2(1280, 740));
        SetupHUD();

        // Camera bounds encompass all three zones (Quarters top → Garden bottom).
        SetCameraLimits(0, -1040, 2560, 2240);

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

        // Area zone — covers the full expanded garden estate.
        AddAreaZone(gardenRoot, "Goldmanor Gardens", new Vector2(1280, 400), new Vector2(2560, 800));

        // Shadow zones — dark hedge corners, alcoves, tree canopy shadows.
        AddShadowZone(gardenRoot, new Vector2(96, 80), new Vector2(80, 80));
        AddShadowZone(gardenRoot, new Vector2(2464, 80), new Vector2(80, 80));
        AddShadowZone(gardenRoot, new Vector2(96, 720), new Vector2(80, 80));
        AddShadowZone(gardenRoot, new Vector2(2464, 720), new Vector2(80, 80));
        AddShadowZone(gardenRoot, new Vector2(400, 200), new Vector2(64, 64));
        AddShadowZone(gardenRoot, new Vector2(2160, 200), new Vector2(64, 64));
        AddShadowZone(gardenRoot, new Vector2(1200, 500), new Vector2(56, 56));
        AddShadowZone(gardenRoot, new Vector2(640, 400), new Vector2(48, 48));
        AddShadowZone(gardenRoot, new Vector2(1920, 400), new Vector2(48, 48));
        AddShadowZone(gardenRoot, new Vector2(900, 600), new Vector2(64, 48));
        AddShadowZone(gardenRoot, new Vector2(1700, 600), new Vector2(64, 48));

        // Hiding spots — gardens, sheds, hedges, fountain, statuary.
        AddHidingSpot(gardenRoot, "Dense Bush", new Vector2(160, 320));
        AddHidingSpot(gardenRoot, "Hedge Alcove", new Vector2(2400, 320));
        AddHidingSpot(gardenRoot, "Rose Trellis", new Vector2(500, 160));
        AddHidingSpot(gardenRoot, "Overgrown Arch", new Vector2(1800, 160));
        AddHidingSpot(gardenRoot, "Tool Shed", new Vector2(320, 580));
        AddHidingSpot(gardenRoot, "Fountain Shadow", new Vector2(1280, 380));
        AddHidingSpot(gardenRoot, "Vine Alcove", new Vector2(2100, 580));
        AddHidingSpot(gardenRoot, "Groundskeeper Shed", new Vector2(700, 680));
        AddHidingSpot(gardenRoot, "Compost Heap", new Vector2(1900, 680));
        AddHidingSpot(gardenRoot, "Gazebo", new Vector2(1000, 260));
        AddHidingSpot(gardenRoot, "Statuary Grove", new Vector2(1600, 260));
        AddHidingSpot(gardenRoot, "Garden Gate", new Vector2(1280, 120));

        // 8 garden patrol guards — longer routes across the expanded grounds.
        AddGuard(gardenRoot, "GardenGuard1", new Vector2(500, 200), new Vector2[]
        {
            new(0, 0), new(0, 300), new(300, 300), new(300, 0)
        });
        AddGuard(gardenRoot, "GardenGuard2", new Vector2(2060, 200), new Vector2[]
        {
            new(0, 0), new(0, 300), new(-300, 300), new(-300, 0)
        });
        AddGuard(gardenRoot, "GardenGuard3", new Vector2(1280, 600), new Vector2[]
        {
            new(-400, 0), new(400, 0)
        });
        AddGuard(gardenRoot, "GardenGuard4", new Vector2(320, 480), new Vector2[]
        {
            new(0, 0), new(200, 0), new(200, 200), new(0, 200)
        });
        AddGuard(gardenRoot, "GardenGuard5", new Vector2(2240, 480), new Vector2[]
        {
            new(0, 0), new(-200, 0), new(-200, 200), new(0, 200)
        });
        AddGuard(gardenRoot, "GardenGuard6", new Vector2(800, 360), new Vector2[]
        {
            new(0, 0), new(0, 160), new(160, 160), new(160, 0)
        });
        AddGuard(gardenRoot, "GardenGuard7", new Vector2(1760, 360), new Vector2[]
        {
            new(0, 0), new(0, 160), new(-160, 160), new(-160, 0)
        });
        AddGuard(gardenRoot, "GardenGuard8", new Vector2(1280, 200), new Vector2[]
        {
            new(-200, 0), new(200, 0), new(200, 120), new(-200, 120)
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

        // Area zone — covers the full expanded hall with wings.
        AddAreaZone(hallRoot, "Main Hall", new Vector2(1280, 352), new Vector2(2560, 704), isRestricted: true);

        // Shadow zones — dark wings, servant corridors, alcoves.
        AddShadowZone(hallRoot, new Vector2(96, 96), new Vector2(64, 64));
        AddShadowZone(hallRoot, new Vector2(2464, 96), new Vector2(64, 64));
        AddShadowZone(hallRoot, new Vector2(96, 600), new Vector2(64, 64));
        AddShadowZone(hallRoot, new Vector2(2464, 600), new Vector2(64, 64));
        AddShadowZone(hallRoot, new Vector2(500, 352), new Vector2(56, 40));
        AddShadowZone(hallRoot, new Vector2(2060, 352), new Vector2(56, 40));
        AddShadowZone(hallRoot, new Vector2(1280, 100), new Vector2(48, 48));
        AddShadowZone(hallRoot, new Vector2(1280, 600), new Vector2(48, 48));
        AddShadowZone(hallRoot, new Vector2(800, 500), new Vector2(40, 40));
        AddShadowZone(hallRoot, new Vector2(1760, 500), new Vector2(40, 40));

        // Hiding spots — many options in the large hall.
        AddHidingSpot(hallRoot, "Under Table", new Vector2(600, 140));
        AddHidingSpot(hallRoot, "Behind Pillar", new Vector2(300, 300));
        AddHidingSpot(hallRoot, "Servant Passage", new Vector2(120, 460));
        AddHidingSpot(hallRoot, "Banquet Alcove", new Vector2(1600, 140));
        AddHidingSpot(hallRoot, "Wine Rack", new Vector2(2400, 460));
        AddHidingSpot(hallRoot, "Kitchen Pantry", new Vector2(400, 560));
        AddHidingSpot(hallRoot, "Fireplace Nook", new Vector2(1280, 280));
        AddHidingSpot(hallRoot, "Coat Room", new Vector2(800, 100));
        AddHidingSpot(hallRoot, "Linen Closet", new Vector2(1760, 100));
        AddHidingSpot(hallRoot, "Scullery", new Vector2(2200, 560));

        // 10 hall guards — patrols leave gaps for sneaking through corridors.
        AddGuard(hallRoot, "HallGuard1", new Vector2(500, 120), new Vector2[]
        {
            new(0, 0), new(0, 280), new(300, 280), new(300, 0)
        });
        AddGuard(hallRoot, "HallGuard2", new Vector2(2060, 120), new Vector2[]
        {
            new(0, 0), new(0, 280), new(-300, 280), new(-300, 0)
        });
        AddGuard(hallRoot, "HallGuard3", new Vector2(200, 352), new Vector2[]
        {
            new(0, -160), new(0, 160)
        });
        AddGuard(hallRoot, "HallGuard4", new Vector2(2360, 352), new Vector2[]
        {
            new(0, -160), new(0, 160)
        });
        AddGuard(hallRoot, "HallGuard5", new Vector2(1280, 560), new Vector2[]
        {
            new(-400, 0), new(400, 0)
        });
        AddGuard(hallRoot, "HallGuard6", new Vector2(800, 200), new Vector2[]
        {
            new(0, 0), new(200, 0), new(200, 200), new(0, 200)
        });
        AddGuard(hallRoot, "HallGuard7", new Vector2(1760, 200), new Vector2[]
        {
            new(0, 0), new(-200, 0), new(-200, 200), new(0, 200)
        });
        AddGuard(hallRoot, "HallGuard8", new Vector2(1280, 200), new Vector2[]
        {
            new(-150, 0), new(150, 0)
        });
        AddGuard(hallRoot, "HallGuard9", new Vector2(600, 500), new Vector2[]
        {
            new(0, 0), new(150, 0), new(150, 100), new(0, 100)
        });
        AddGuard(hallRoot, "HallGuard10", new Vector2(1960, 500), new Vector2[]
        {
            new(0, 0), new(-150, 0), new(-150, 100), new(0, 100)
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

        // Area zone — covers full expanded quarters.
        AddAreaZone(quartersRoot, "Lord Cowl's Quarters", new Vector2(1280, 312), new Vector2(2560, 624), isRestricted: true);

        // Shadow zones — dark alcoves, corners, passage shadows.
        AddShadowZone(quartersRoot, new Vector2(96, 64), new Vector2(64, 64));
        AddShadowZone(quartersRoot, new Vector2(2464, 64), new Vector2(64, 64));
        AddShadowZone(quartersRoot, new Vector2(1200, 140), new Vector2(56, 40));
        AddShadowZone(quartersRoot, new Vector2(96, 540), new Vector2(64, 64));
        AddShadowZone(quartersRoot, new Vector2(2464, 540), new Vector2(64, 64));
        AddShadowZone(quartersRoot, new Vector2(640, 300), new Vector2(48, 48));
        AddShadowZone(quartersRoot, new Vector2(1920, 300), new Vector2(48, 48));
        AddShadowZone(quartersRoot, new Vector2(1280, 500), new Vector2(56, 40));

        // Hiding spots — multiple approach vectors to Cowl.
        AddHidingSpot(quartersRoot, "Wardrobe", new Vector2(120, 120));
        AddHidingSpot(quartersRoot, "Behind Curtains", new Vector2(2440, 120));
        AddHidingSpot(quartersRoot, "Under Desk", new Vector2(500, 480));
        AddHidingSpot(quartersRoot, "Dark Alcove", new Vector2(2060, 480));
        AddHidingSpot(quartersRoot, "Wine Cellar Stairs", new Vector2(320, 300));
        AddHidingSpot(quartersRoot, "Trophy Case", new Vector2(960, 120));
        AddHidingSpot(quartersRoot, "Servant Stairwell", new Vector2(1600, 120));
        AddHidingSpot(quartersRoot, "Private Chapel Pew", new Vector2(800, 480));
        AddHidingSpot(quartersRoot, "Balcony Overhang", new Vector2(1760, 480));

        // 6 elite guards — tougher, patrolling the private quarters.
        AddGuard(quartersRoot, "EliteGuard1", new Vector2(500, 160), new Vector2[]
        {
            new(0, -80), new(0, 160), new(250, 160), new(250, -80)
        }, elite: true);
        AddGuard(quartersRoot, "EliteGuard2", new Vector2(2060, 160), new Vector2[]
        {
            new(0, -80), new(0, 160), new(-250, 160), new(-250, -80)
        }, elite: true);
        AddGuard(quartersRoot, "EliteGuard3", new Vector2(1280, 440), new Vector2[]
        {
            new(-250, 0), new(250, 0)
        }, elite: true);
        AddGuard(quartersRoot, "EliteGuard4", new Vector2(800, 300), new Vector2[]
        {
            new(0, -100), new(0, 100)
        }, elite: true);
        AddGuard(quartersRoot, "EliteGuard5", new Vector2(1760, 300), new Vector2[]
        {
            new(0, -100), new(0, 100)
        }, elite: true);
        AddGuard(quartersRoot, "EliteGuard6", new Vector2(1280, 160), new Vector2[]
        {
            new(-150, 0), new(150, 0), new(150, 100), new(-150, 100)
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
        cowl.Position = new Vector2(1280, 240);
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

        // Wire death to mission complete via the shared base handler.
        health.Died += () => OnTargetKilled("cowl", 0,
            "Lord Harlan Cowl\nGovernor of the Greenhold\nEdictbearer",
            "\"I never saw the dark as empty. I thought it was full of things that loved me.\"");
    }
}
