using Godot;
using System;
using System.Linq;
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
        CallDeferred(MethodName.PlaceServantDisguise); // After zones are in tree
        SpawnPlayer(GardenOffset + new Vector2(1280, 740));
        SetupHUD();
        SetupCheckpointRespawn();
        CallDeferred(MethodName.ShowGardenHintOnce);

        // Checkpoint 1 — player enters the Main Hall zone.
        AddCheckpoint(this, 1, new Vector2(1280, 50), 2560f, new Vector2(1280, 200));

        // Checkpoint 2 — player enters Lord Cowl's Quarters.
        AddCheckpoint(this, 2, new Vector2(1280, -1020), 2560f, new Vector2(1280, -900));

        // Camera bounds encompass all three zones (Quarters top → Garden bottom).
        SetCameraLimits(0, -1040, 2560, 2240);

        GD.Print("═══ GOLDMANOR LOADED ═══");
        GD.Print("  MECHANIC: Find the Servant Livery — non-elite guards half-blind while worn.");
        GD.Print("  PUZZLE GUIDE:");
        GD.Print("  Gardens: Find the Garden Key in a chest behind breakable bushes.");
        GD.Print("           Push blocks onto both floor switches to open the Hall gate.");
        GD.Print("  Hall:    Pull both levers (East & West wings) to open the Quarters gate.");
        GD.Print("           Find the Master Key in the locked chest (requires Servant Key).");
        GD.Print("  Quarters: Use Master Key on Cowl's study door. Breakable wall = secret path.");
    }

    // ─── First-visit hint ─────────────────────────────────────────

    private const string HintFlag = "goldmanor_garden_hint_shown";

    /// <summary>
    /// Shows a one-time entry hint via the HUD toast. The flag is stored in
    /// DialogueManager so it survives retries within the same session and is
    /// exported/imported with save data.
    /// </summary>
    private void ShowGardenHintOnce()
    {
        var dm = Dialogue.DialogueManager.Instance;
        if (dm == null || dm.HasFlag(HintFlag)) return;

        dm.SetFlag(HintFlag);

        var hud = GetTree().GetFirstNodeInGroup("GameHUD") as UI.GameHUD;
        hud?.ShowHint("The gardens are less guarded at the edges. Move slowly.", 4f);
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

        // ═══ PUZZLE 1: GARDEN GATE ═════════════════════════════════
        // Two floor switches (one on each side of the garden) must both
        // be held down to open the gate into the Main Hall.
        // Player pushes blocks onto them — classic Zelda push-block puzzle.

        var gardenGate = AddPuzzleGate(gardenRoot, "Hall Entrance Gate",
            new Vector2(1280, 60), requiredConditions: 2, stayOpen: true, width: 48, height: 12);

        var switchWest = AddFloorSwitch(gardenRoot, "West Garden Plate",
            new Vector2(640, 480), stayPressed: true);
        var switchEast = AddFloorSwitch(gardenRoot, "East Garden Plate",
            new Vector2(1920, 480), stayPressed: true);

        gardenGate.LinkSwitch(switchWest);
        gardenGate.LinkSwitch(switchEast);

        // Push blocks near the switches — player must push them onto the plates.
        AddPushBlock(gardenRoot, "West Stone", new Vector2(640, 432));
        AddPushBlock(gardenRoot, "East Stone", new Vector2(1920, 432));

        // ═══ PUZZLE 2: HIDDEN KEY CHEST ════════════════════════════
        // A breakable hedge wall hides a chest containing the Servant Key
        // (used later in the Main Hall to access a locked passage).
        AddBreakableWall(gardenRoot, "Cracked Hedge", new Vector2(400, 300),
            hitsRequired: 2, width: 16, height: 16);
        AddKeyChest(gardenRoot, "Hidden Garden Chest", new Vector2(400, 340),
            "servant_key", "Servant Passage Key");

        // ═══ BONUS: OPTIONAL INK CHEST ═════════════════════════════
        // Reward for exploring — trace ink hidden in the far east corner.
        AddItemChest(gardenRoot, "Gardener's Stash", new Vector2(2400, 680),
            "ink", "trace_ink", "Trace Ink", quantity: 1);

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

        // ═══ PUZZLE 3: LOCKED SERVANT PASSAGE ═════════════════════
        // The servant passage shortcut through the Hall is locked.
        // Requires the Servant Key found in the Garden's hidden chest.
        AddLockedDoor(hallRoot, "Servant Door", new Vector2(120, 352),
            "servant_key", isVertical: true);

        // ═══ PUZZLE 4: DUAL-LEVER GATE ════════════════════════════
        // Two levers — one in the west wing, one in the east wing —
        // must both be pulled to open the reinforced gate to the Quarters.
        // Guards patrol near each lever, forcing the player to plan timing.

        var quartersGate = AddPuzzleGate(hallRoot, "Quarters Gate",
            new Vector2(1280, 40), requiredConditions: 2, stayOpen: true, width: 48, height: 12);

        var leverWest = AddLever(hallRoot, "West Wing Lever",
            new Vector2(300, 500), oneWay: true);
        var leverEast = AddLever(hallRoot, "East Wing Lever",
            new Vector2(2260, 500), oneWay: true);

        quartersGate.LinkLever(leverWest);
        quartersGate.LinkLever(leverEast);

        // ═══ PUZZLE 5: BREAKABLE WALL SHORTCUT ════════════════════
        // A cracked section in the east wall reveals a shortcut
        // past the guarded central corridor — risky but fast.
        AddBreakableWall(hallRoot, "Cracked Hall Wall", new Vector2(2000, 352),
            hitsRequired: 2, width: 16, height: 32);

        // ═══ PUZZLE 6: MASTER KEY IN LOCKED CHEST ═════════════════
        // The Master Key (needed for Cowl's study) is in a locked chest
        // hidden in the wine cellar area. The chest requires the Servant Key.
        AddItemChest(hallRoot, "Wine Cellar Strongbox", new Vector2(2400, 140),
            "key", "master_key", "Master Key", requiredKeyId: "servant_key");

        // ═══ BONUS: INK CHEST BEHIND PUSH BLOCK ══════════════════
        // A push block in the dining area hides a lesser ink chest.
        AddPushBlock(hallRoot, "Dining Stone Block", new Vector2(800, 352));
        AddItemChest(hallRoot, "Hidden Dining Cache", new Vector2(800, 400),
            "ink", "lesser_ink", "Lesser Ink", quantity: 1);

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

        // ═══ PUZZLE 7: MASTER-LOCKED STUDY DOOR ═══════════════════
        // The direct path to Cowl's study requires the Master Key
        // found in the Main Hall's locked chest.
        AddLockedDoor(quartersRoot, "Cowl's Study Door", new Vector2(1280, 192),
            "master_key");

        // ═══ PUZZLE 8: SECRET PASSAGE (BREAKABLE WALL) ════════════
        // An alternate approach — a cracked wall on the west side
        // reveals a hidden passage directly into Cowl's study.
        // Hitting the wall makes noise (alerting nearby guards),
        // creating a risk/reward tradeoff.
        AddBreakableWall(quartersRoot, "Cracked Study Wall", new Vector2(960, 200),
            hitsRequired: 3, width: 16, height: 16);

        // ═══ PUZZLE 9: FLOOR SWITCH TRAP ROOM ═════════════════════
        // Before the study, a room with a floor switch puzzle.
        // Step on the switch to temporarily open a barred passage.
        // The switch releases when you step off — you must be quick.
        var trapGate = AddPuzzleGate(quartersRoot, "Barred Passage",
            new Vector2(1100, 300), requiredConditions: 1, stayOpen: false,
            isVertical: true, width: 16, height: 32);

        var trapSwitch = AddFloorSwitch(quartersRoot, "Floor Trigger",
            new Vector2(1100, 400), stayPressed: false);

        trapGate.LinkSwitch(trapSwitch);

        // A push block nearby — push it onto the switch to hold it down permanently.
        AddPushBlock(quartersRoot, "Chapel Stone", new Vector2(1100, 448));

        // ═══ BONUS: TREASURE ROOM ═════════════════════════════════
        // A locked chest in the trophy gallery with major ink — big reward.
        AddItemChest(quartersRoot, "Cowl's Private Collection", new Vector2(2360, 120),
            "ink", "lesser_ink", "Lesser Ink", quantity: 2);

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

    // ═════════════════════════════════════════════════════════════
    //  UNIQUE MECHANIC: SERVANT DISGUISE
    //  A chest in the groundskeeper shed contains a Servant Livery.
    //  While the player holds it, non-elite guards in unrestricted areas
    //  halve their detection awareness gain — they see a servant,
    //  not an intruder. Elite guards and restricted zones ignore it.
    //  The disguise is one-time; dropping into combat removes it.
    // ═════════════════════════════════════════════════════════════

    private bool _disguiseActive;

    private void PlaceServantDisguise()
    {
        var gardens = GetNodeOrNull<Node2D>("Gardens");
        if (gardens == null) return;

        // Servant livery chest hidden in the groundskeeper shed.
        var chest = AddItemChest(gardens, "Servant Livery Chest",
            new Vector2(700, 720),
            "consumable", "servant_livery", "Servant Livery", quantity: 1);

        // When the player opens this chest, activate the disguise.
        chest.Opened += OnServantLiveryPickedUp;
    }

    private void OnServantLiveryPickedUp()
    {
        if (_disguiseActive) return;
        _disguiseActive = true;

        // Reduce awareness gain on all non-elite guards currently in the scene.
        ApplyDisguiseToGuards(active: true);

        // Also subscribe to the alert manager — if a full alarm fires, the
        // disguise is blown (guards recognise the ruse under full alarm).
        var alertMgr = Stealth.MissionAlertManager.Instance;
        if (alertMgr != null)
            alertMgr.AlertLevelChanged += OnAlertWhileDisguised;

        GD.Print("[Goldmanor] Servant Livery equipped — non-elite guards partially blind.");
    }

    private void OnAlertWhileDisguised(int level)
    {
        if (level >= 3 && _disguiseActive)
            RemoveDisguise();
    }

    private void RemoveDisguise()
    {
        if (!_disguiseActive) return;
        _disguiseActive = false;

        ApplyDisguiseToGuards(active: false);

        var alertMgr = Stealth.MissionAlertManager.Instance;
        if (alertMgr != null)
            alertMgr.AlertLevelChanged -= OnAlertWhileDisguised;

        GD.Print("[Goldmanor] Disguise blown — guards are now fully alert.");
    }

    private void ApplyDisguiseToGuards(bool active)
    {
        // Walk every guard in the scene; halve (or restore) awareness gain
        // only on non-elite guards that are NOT inside a restricted zone.
        float multiplier = active ? 0.5f : 2.0f; // Apply/undo

        foreach (var guard in GetTree().GetNodesInGroup("Enemy").OfType<GuardEnemy>())
        {
            // Skip elites — they see through any disguise.
            if (guard.DetectRange >= 130f) continue;

            var sensor = guard.GetNodeOrNull<DetectionSensor>("DetectionSensor");
            if (sensor != null)
                sensor.AwarenessGainRate *= multiplier;
        }
    }
}
