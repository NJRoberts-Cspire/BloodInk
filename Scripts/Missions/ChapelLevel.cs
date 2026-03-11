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

    // Chapel Grounds — sprawling walled compound with graveyard, meditation garden, bell tower approach
    private static readonly string[] GroundsMap = {
        ",,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,",
        ",,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,",
        ",,,,~,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,pppppppp,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,~,,,,,,,,,,,,,,,",
        ",,,,,,,,,,,,,,,,pppppppppp,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,pp........pp,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,pppppppp,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,",
        ",,,,,,,,,,,,,,,pp..........pp,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,pp............pp,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,pp..........pp,,,,,,,,,,,,,,,,,,,,,,,,,,",
        ",,,,,,,,,,,,,,pp............pp,,,,,,,,,,,,,,,,ppppppp,,,,,,,,,,,,,,,,pp................pp,,,,,,,,,,,,,,,,ppppppp,,,,,,,,,,pp..............pp,,,,,,,,,,,,,,,,,,,,,,,,,",
        ",,,,,,,,,,,,,pp..............pp,,,,,,,,,,,,,ppp.....ppp,,,,,,,,,,,,,pp..................pp,,,,,,,,,,,,,ppp.....ppp,,,,,,pp................pp,,,,,,,,,,,,,,,,,,,,,,,,",
        ",,,,,,,,,,,,pp................pp,,,,,,,,,,ppp.........ppp,,,,,,,,,pp......................pp,,,,,,,,,,ppp.........ppp,pp..................pp,,,,,,,,,,,,,,,,,,,,,,,,,",
        ",,,,,,,,,,,pp..................pp,,,,,,,ppp.............ppp,,,,,,pp........................pp,,,,,,ppp.............ppp....................pp,,,,,,,,,,,,,,,,,,,,,,,,,,",
        ",,,,,,,,,,pp....................pp,,,,,pp.................pp,,,pp..........................pp,,,,,pp.................pp....................pp,,,,,,,,,,,,,,,,,,,,,,,,,,,",
        ",,,,,,,,,pp......................pp,,,pp...................pppp..............................pp,,pp...................pp..................pp,,,,,,,,,,,,,,,,,,,,,,,,,,,,",
        ",,,,,,,,pp........................pp,pp.....................pp................................pppp.....................pp................pp,,,,,,,,,,,,,,,,,,,,,,,,,,,,,",
        ",,,,,,,,p..........................ppp.......................................................................................pp..........pp,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,",
        ",,,,,,,,p..........................pp.........................................................................................pp......pp,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,",
        ",,,,,,,pp..........................p...........................................................................................pp....pp,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,",
        ",,,,,,,p...........................p.............................................................................................pppppp,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,",
        ",,,,,,,p...........................p...........................................................................................pp....pp,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,",
        ",,,,,,,pp..........................p.........................................................................................pp......pp,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,",
        ",,,,,,,,p..........................pp.......................................................................................pp..........pp,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,",
        ",,,,,,,,p..........................ppp.......................pp................................pppp.....................pp................pp,,,,,,,,,,,,,,,,,,,,,,,,,,,,,",
        ",,,,,,,,pp........................pp,pp.....................pppp..............................pp,,pp...................pp..................pp,,,,,,,,,,,,,,,,,,,,,,,,,,,,",
        ",,,,,,,,,pp......................pp,,,pp...................pp,,,pp..........................pp,,,,,pp.................pp....................pp,,,,,,,,,,,,,,,,,,,,,,,,,,,",
        ",,,,,,,,,,pp....................pp,,,,,ppp.............ppp,,,,,,pp........................pp,,,,,,ppp.............ppp....................pp,,,,,,,,,,,,,,,,,,,,,,,,,,,,",
        ",,,,,,,,,,,pp..................pp,,,,,,,,,ppp.........ppp,,,,,,,,,pp......................pp,,,,,,,,,,ppp.........ppp,pp..................pp,,,,,,,,,,,,,,,,,,,,,,,,,,,",
        ",,,,,,,,,,,,pp................pp,,,,,,,,,,,,ppp.....ppp,,,,,,,,,,,,,pp..................pp,,,,,,,,,,,,,ppp.....ppp,,,,,,pp................pp,,,,,,,,,,,,,,,,,,,,,,,,,,",
        ",,,,,,,,,,,,,pp..............pp,,,,,,,,,,,,,,,ppppppp,,,,,,,,,,,,,,,,pp................pp,,,,,,,,,,,,,,,,ppppppp,,,,,,,,,,pp..............pp,,,,,,,,,,,,,,,,,,,,,,,,,",
        ",,,,,,,,,,,,,,pp............pp,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,pp............pp,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,pp..........pp,,,,,,,,,,,,,,,,,,,,,,,,,,,,",
        ",,,,,,,,,,,,,,,pp..........pp,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,pp........pp,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,pppppppp,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,",
        ",,~,,,,,,,,,,,,,,pppppppppp,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,pppppppp,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,~,,,,,,,,,,,",
        ",,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,",
        ",,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,",
        ",,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,",
        ",,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,",
        ",,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,",
        ",,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,",
        ",,,,~,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,~,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,~,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,~,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,~,,,,,,,,,,,,",
        ",,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,",
        ",,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,",
        ",,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,",
        ",,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,",
    };

    // Nave & Vestry — grand interior with twin aisles, side chapels, confessionals, sacristy, clergy hall, scriptorium
    private static readonly string[] NaveMap = {
        "################################################################################################################################################################",
        "#wwwwww#cccccccccc#wwwwwwww#wwwwww#cccccccccc#wwwwwwww#wwwwwwww#cccccccccc#wwwwwwww#wwwwwwww#cccccccccc#wwwwwwww#wwwwwwww#cccccccccc#wwwwwwww#wwwwwwww#wwwwwww#",
        "#wwwwww#cccccccccc#wwwwwwww#wwwwww#cccccccccc#wwwwwwww#wwwwwwww#cccccccccc#wwwwwwww#wwwwwwww#cccccccccc#wwwwwwww#wwwwwwww#cccccccccc#wwwwwwww#wwwwwwww#wwwwwww#",
        "#wwwwww#cccccccccc#wwwwwwww#wwwwww#cccccccccc#wwwwwwww#wwwwwwww#cccccccccc#wwwwwwww#wwwwwwww#cccccccccc#wwwwwwww#wwwwwwww#cccccccccc#wwwwwwww#wwwwwwww#wwwwwww#",
        "#wwwwww#cccccccccc#wwwwwwww#wwwwww#cccccccccc#wwwwwwww#wwwwwwww#cccccccccc#wwwwwwww#wwwwwwww#cccccccccc#wwwwwwww#wwwwwwww#cccccccccc#wwwwwwww#wwwwwwww#wwwwwww#",
        "#wwwwww...cccccc...wwwwwwww.wwwwww...cccccc...wwwwwwww.wwwwwwww...cccccc...wwwwwwww.wwwwwwww...cccccc...wwwwwwww.wwwwwwww...cccccc...wwwwwwww.wwwwwwww.wwwwwww#",
        "#wwwwww...cccccc...wwwwwwww.wwwwww...cccccc...wwwwwwww.wwwwwwww...cccccc...wwwwwwww.wwwwwwww...cccccc...wwwwwwww.wwwwwwww...cccccc...wwwwwwww.wwwwwwww.wwwwwww#",
        "#wwwwww...cccccc...wwwwwwww.wwwwww...cccccc...wwwwwwww.wwwwwwww...cccccc...wwwwwwww.wwwwwwww...cccccc...wwwwwwww.wwwwwwww...cccccc...wwwwwwww.wwwwwwww.wwwwwww#",
        "#wwwwww...cccccc...wwwwwwww.wwwwww...cccccc...wwwwwwww.wwwwwwww...cccccc...wwwwwwww.wwwwwwww...cccccc...wwwwwwww.wwwwwwww...cccccc...wwwwwwww.wwwwwwww.wwwwwww#",
        "#wwwwww#cccccccccc#wwwwwwww#wwwwww#cccccccccc#wwwwwwww#wwwwwwww#cccccccccc#wwwwwwww#wwwwwwww#cccccccccc#wwwwwwww#wwwwwwww#cccccccccc#wwwwwwww#wwwwwwww#wwwwwww#",
        "#wwwwww#cccccccccc#wwwwwwww#wwwwww#cccccccccc#wwwwwwww#wwwwwwww#cccccccccc#wwwwwwww#wwwwwwww#cccccccccc#wwwwwwww#wwwwwwww#cccccccccc#wwwwwwww#wwwwwwww#wwwwwww#",
        "#wwwwww#cccccccccc#wwwwwwww#wwwwww#cccccccccc#wwwwwwww#wwwwwwww#cccccccccc#wwwwwwww#wwwwwwww#cccccccccc#wwwwwwww#wwwwwwww#cccccccccc#wwwwwwww#wwwwwwww#wwwwwww#",
        "#wwwwww#cccccccccc#wwwwwwww#wwwwww#cccccccccc#wwwwwwww#wwwwwwww#cccccccccc#wwwwwwww#wwwwwwww#cccccccccc#wwwwwwww#wwwwwwww#cccccccccc#wwwwwwww#wwwwwwww#wwwwwww#",
        "#wwwwww#cccccccccc#wwwwwwww#wwwwww#cccccccccc#wwwwwwww#wwwwwwww#cccccccccc#wwwwwwww#wwwwwwww#cccccccccc#wwwwwwww#wwwwwwww#cccccccccc#wwwwwwww#wwwwwwww#wwwwwww#",
        "#wwwwww#cccccccccc#wwwwwwww#wwwwww#cccccccccc#wwwwwwww#wwwwwwww#cccccccccc#wwwwwwww#wwwwwwww#cccccccccc#wwwwwwww#wwwwwwww#cccccccccc#wwwwwwww#wwwwwwww#wwwwwww#",
        "#wwwwww...cccccc...wwwwwwww.wwwwww...cccccc...wwwwwwww.wwwwwwww...cccccc...wwwwwwww.wwwwwwww...cccccc...wwwwwwww.wwwwwwww...cccccc...wwwwwwww.wwwwwwww.wwwwwww#",
        "#wwwwww...cccccc...wwwwwwww.wwwwww...cccccc...wwwwwwww.wwwwwwww...cccccc...wwwwwwww.wwwwwwww...cccccc...wwwwwwww.wwwwwwww...cccccc...wwwwwwww.wwwwwwww.wwwwwww#",
        "#wwwwww#cccccccccc#~~wwwwww#wwwwww#cccccccccc#~~wwwwww#wwwwwwww#cccccccccc#~~wwwwww#wwwwwwww#cccccccccc#~~wwwwww#wwwwwwww#cccccccccc#~~wwwwww#wwwwwwww#wwwwwww#",
        "#wwwwww#cccccccccc#~~wwwwww#wwwwww#cccccccccc#~~wwwwww#wwwwwwww#cccccccccc#~~wwwwww#wwwwwwww#cccccccccc#~~wwwwww#wwwwwwww#cccccccccc#~~wwwwww#wwwwwwww#wwwwwww#",
        "#wwwwww#cccccccccc#wwwwwwww#wwwwww#cccccccccc#wwwwwwww#wwwwwwww#cccccccccc#wwwwwwww#wwwwwwww#cccccccccc#wwwwwwww#wwwwwwww#cccccccccc#wwwwwwww#wwwwwwww#wwwwwww#",
        "#wwwwww#cccccccccc#wwwwwwww#wwwwww#cccccccccc#wwwwwwww#wwwwwwww#cccccccccc#wwwwwwww#wwwwwwww#cccccccccc#wwwwwwww#wwwwwwww#cccccccccc#wwwwwwww#wwwwwwww#wwwwwww#",
        "#wwwwww#cccccccccc#wwwwwwww#wwwwww#cccccccccc#wwwwwwww#wwwwwwww#cccccccccc#wwwwwwww#wwwwwwww#cccccccccc#wwwwwwww#wwwwwwww#cccccccccc#wwwwwwww#wwwwwwww#wwwwwww#",
        "#wwwwww#cccccccccc#wwwwwwww#wwwwww#cccccccccc#wwwwwwww#wwwwwwww#cccccccccc#wwwwwwww#wwwwwwww#cccccccccc#wwwwwwww#wwwwwwww#cccccccccc#wwwwwwww#wwwwwwww#wwwwwww#",
        "#wwwwww#cccccccccc#wwwwwwww#wwwwww#cccccccccc#wwwwwwww#wwwwwwww#cccccccccc#wwwwwwww#wwwwwwww#cccccccccc#wwwwwwww#wwwwwwww#cccccccccc#wwwwwwww#wwwwwwww#wwwwwww#",
        "#wwwwww...cccccc...wwwwwwww.wwwwww...cccccc...wwwwwwww.wwwwwwww...cccccc...wwwwwwww.wwwwwwww...cccccc...wwwwwwww.wwwwwwww...cccccc...wwwwwwww.wwwwwwww.wwwwwww#",
        "#wwwwww...cccccc...wwwwwwww.wwwwww...cccccc...wwwwwwww.wwwwwwww...cccccc...wwwwwwww.wwwwwwww...cccccc...wwwwwwww.wwwwwwww...cccccc...wwwwwwww.wwwwwwww.wwwwwww#",
        "#wwwwww#cccccccccc#wwwwwwww#wwwwww#cccccccccc#wwwwwwww#wwwwwwww#cccccccccc#wwwwwwww#wwwwwwww#cccccccccc#wwwwwwww#wwwwwwww#cccccccccc#wwwwwwww#wwwwwwww#wwwwwww#",
        "#wwwwww#cccccccccc#wwwwwwww#wwwwww#cccccccccc#wwwwwwww#wwwwwwww#cccccccccc#wwwwwwww#wwwwwwww#cccccccccc#wwwwwwww#wwwwwwww#cccccccccc#wwwwwwww#wwwwwwww#wwwwwww#",
        "#wwwwww#cccccccccc#wwwwwwww#wwwwww#cccccccccc#wwwwwwww#wwwwwwww#cccccccccc#wwwwwwww#wwwwwwww#cccccccccc#wwwwwwww#wwwwwwww#cccccccccc#wwwwwwww#wwwwwwww#wwwwwww#",
        "#wwwwww#cccccccccc#wwwwwwww#wwwwww#cccccccccc#wwwwwwww#wwwwwwww#cccccccccc#wwwwwwww#wwwwwwww#cccccccccc#wwwwwwww#wwwwwwww#cccccccccc#wwwwwwww#wwwwwwww#wwwwwww#",
        "#wwwwww#cccccccccc#wwwwwwww#wwwwww#cccccccccc#wwwwwwww#wwwwwwww#cccccccccc#wwwwwwww#wwwwwwww#cccccccccc#wwwwwwww#wwwwwwww#cccccccccc#wwwwwwww#wwwwwwww#wwwwwww#",
        "#wwwwww#cccccccccc#wwwwwwww#wwwwww#cccccccccc#wwwwwwww#wwwwwwww#cccccccccc#wwwwwwww#wwwwwwww#cccccccccc#wwwwwwww#wwwwwwww#cccccccccc#wwwwwwww#wwwwwwww#wwwwwww#",
        "#wwwwww#cccccccccc#wwwwwwww#wwwwww#cccccccccc#wwwwwwww#wwwwwwww#cccccccccc#wwwwwwww#wwwwwwww#cccccccccc#wwwwwwww#wwwwwwww#cccccccccc#wwwwwwww#wwwwwwww#wwwwwww#",
        "#wwwwww#cccccccccc#wwwwwwww#wwwwww#cccccccccc#wwwwwwww#wwwwwwww#cccccccccc#wwwwwwww#wwwwwwww#cccccccccc#wwwwwwww#wwwwwwww#cccccccccc#wwwwwwww#wwwwwwww#wwwwwww#",
        "################################################################################################################################################################",
    };

    // Relic Chamber — sprawling underground catacombs with burial niches, winding corridors, central shrine
    private static readonly string[] RelicMap = {
        "################################################################################################################################################################",
        "#......~~#........#~~.....#........#~~.....#........#~~........#......~~#........#~~.....#........#~~.....#........#~~........#........#~~.....#........#~~......#",
        "#........#........#.......#........#.......#........#..........#........#........#.......#........#.......#........#..........#........#.......#........#........#",
        "#........#........#.......#........#.......#........#..........#........#........#.......#........#.......#........#..........#........#.......#........#........#",
        "#........#........#.......#........#.......#........#..........#........#........#.......#........#.......#........#..........#........#.......#........#........#",
        "#.............##..........#.............##..........#..........#.............##..........#.............##..........#..........#.............##..........#..........#",
        "#.............##..........#.............##..........#..........#.............##..........#.............##..........#..........#.............##..........#..........#",
        "#.............##..........#.............##..........#..........#.............##..........#.............##..........#..........#.............##..........#..........#",
        "#.............##..........#.............##..........#..........#.............##..........#.............##..........#..........#.............##..........#..........#",
        "#........#........#.......#........#.......#........#..........#........#........#.......#........#.......#........#..........#........#.......#........#........#",
        "#........#........#.......#........#.......#........#..........#........#........#.......#........#.......#........#..........#........#.......#........#........#",
        "#........#........#.......#........#.......#........#..........#........#........#.......#........#.......#........#..........#........#.......#........#........#",
        "#........#........#.......#........#.......#........#..........#........#........#.......#........#.......#........#..........#........#.......#........#........#",
        "#.............##..........#.............##..........#..........#.............##..........#.............##..........#..........#.............##..........#..........#",
        "#.............##..........#.............##..........#..........#.............##..........#.............##..........#..........#.............##..........#..........#",
        "#........#........#.......#........#.......#........#..........#........#........#.......#........#.......#........#..........#........#.......#........#........#",
        "#........#........#.......#........#.......#........#..........#........#........#.......#........#.......#........#..........#........#.......#........#........#",
        "#........#........#.......#........#.......#........#..........#........#........#.......#........#.......#........#..........#........#.......#........#........#",
        "#........#........#.......#........#.......#........#..........#........#........#.......#........#.......#........#..........#........#.......#........#........#",
        "#.............##..........#.............##..........#..........#.............##..........#.............##..........#..........#.............##..........#..........#",
        "#.............##..........#.............##..........#..........#.............##..........#.............##..........#..........#.............##..........#..........#",
        "#........#........#.......#........#.......#........#..........#........#........#.......#........#.......#........#..........#........#.......#........#........#",
        "#........#........#.......#........#.......#........#..........#........#........#.......#........#.......#........#..........#........#.......#........#........#",
        "#......~~#........#~~.....#........#~~.....#........#~~........#......~~#........#~~.....#........#~~.....#........#~~........#........#~~.....#........#~~......#",
        "################################################################################################################################################################",
    };

    private static readonly Vector2 GroundsOffset = new(0, 1200);
    private static readonly Vector2 NaveOffset = new(0, 0);
    private static readonly Vector2 RelicOffset = new(0, -640);

    public override void _Ready()
    {
        PlaceholderSprites.CreateAll();

        BuildGrounds();
        BuildNave();
        BuildRelicChamber();
        SpawnPlayer(GroundsOffset + new Vector2(1280, 540));
        SetupHUD();
        RegisterTargets();

        // Camera bounds encompass all three zones (Relic top → Grounds bottom).
        SetCameraLimits(0, -640, 2560, 1840);

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
        AddAreaZone(root, "Chapel Grounds", new Vector2(1280, 320), new Vector2(2560, 640));

        // Shadow at edges, hedgerows, and among the graves.
        AddShadowZone(root, new Vector2(64, 32), new Vector2(40, 40));
        AddShadowZone(root, new Vector2(2496, 32), new Vector2(40, 40));
        AddShadowZone(root, new Vector2(64, 560), new Vector2(40, 40));
        AddShadowZone(root, new Vector2(2496, 560), new Vector2(40, 40));
        AddShadowZone(root, new Vector2(480, 140), new Vector2(40, 40));
        AddShadowZone(root, new Vector2(1280, 260), new Vector2(40, 32));
        AddShadowZone(root, new Vector2(2080, 140), new Vector2(40, 40));
        AddShadowZone(root, new Vector2(720, 480), new Vector2(40, 40));
        AddShadowZone(root, new Vector2(1840, 480), new Vector2(40, 40));

        // Hiding spots — cemetery, graves, bushes, memorial statues.
        AddHidingSpot(root, "Hedgerow", new Vector2(240, 300));
        AddHidingSpot(root, "Gravestone", new Vector2(1480, 300));
        AddHidingSpot(root, "Cemetery Tree", new Vector2(640, 180));
        AddHidingSpot(root, "Stone Bench", new Vector2(1080, 420));
        AddHidingSpot(root, "Overgrown Arch", new Vector2(880, 140));
        AddHidingSpot(root, "Memorial Statue", new Vector2(1680, 180));
        AddHidingSpot(root, "Collapsed Tomb", new Vector2(400, 480));
        AddHidingSpot(root, "Flower Garden", new Vector2(2160, 300));
        AddHidingSpot(root, "Bell Tower Base", new Vector2(1280, 100));
        AddHidingSpot(root, "Iron Gate", new Vector2(2000, 480));

        // 5 gate guards — wider patrols across the sprawling grounds.
        AddGuard(root, "GateGuard1", new Vector2(640, 200), new Vector2[]
        {
            new(0, 0), new(200, 0), new(200, 160), new(0, 160)
        });
        AddGuard(root, "GateGuard2", new Vector2(1920, 200), new Vector2[]
        {
            new(0, 0), new(-200, 0), new(-200, 160), new(0, 160)
        });
        AddGuard(root, "GateGuard3", new Vector2(600, 460), new Vector2[]
        {
            new(-120, 0), new(200, 0)
        });
        AddGuard(root, "GateGuard4", new Vector2(1960, 460), new Vector2[]
        {
            new(-200, 0), new(120, 0)
        });
        AddGuard(root, "GateGuard5", new Vector2(1280, 340), new Vector2[]
        {
            new(-300, 0), new(300, 0)
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
        AddAreaZone(root, "Chapel Nave", new Vector2(1280, 280), new Vector2(2560, 560),
            isRestricted: true);

        // Shadow zones — dark vestry corners and side chapel alcoves.
        AddShadowZone(root, new Vector2(396, 440), new Vector2(32, 32));
        AddShadowZone(root, new Vector2(900, 440), new Vector2(32, 32));
        AddShadowZone(root, new Vector2(1660, 440), new Vector2(32, 32));
        AddShadowZone(root, new Vector2(2164, 440), new Vector2(32, 32));
        AddShadowZone(root, new Vector2(64, 48), new Vector2(40, 32));
        AddShadowZone(root, new Vector2(2496, 48), new Vector2(40, 32));
        AddShadowZone(root, new Vector2(1280, 60), new Vector2(40, 32));
        AddShadowZone(root, new Vector2(640, 240), new Vector2(32, 32));
        AddShadowZone(root, new Vector2(1920, 240), new Vector2(32, 32));

        // Hiding spots — pews, confessionals, behind pillars, side chapels, sacristy.
        AddHidingSpot(root, "Confessional", new Vector2(160, 120));
        AddHidingSpot(root, "Behind Altar", new Vector2(800, 60));
        AddHidingSpot(root, "Pew Alcove", new Vector2(1400, 240));
        AddHidingSpot(root, "Vestry Closet", new Vector2(2400, 120));
        AddHidingSpot(root, "Side Chapel", new Vector2(400, 380));
        AddHidingSpot(root, "Scriptorium Desk", new Vector2(2000, 120));
        AddHidingSpot(root, "Choir Loft", new Vector2(1280, 440));
        AddHidingSpot(root, "Clergy Quarters", new Vector2(600, 60));
        AddHidingSpot(root, "Sacristy Cabinet", new Vector2(1800, 380));

        // 7 nave guards — patrols with openings along both aisles.
        AddGuard(root, "NaveGuard1", new Vector2(500, 120), new Vector2[]
        {
            new(0, 0), new(0, 280)
        });
        AddGuard(root, "NaveGuard2", new Vector2(1100, 120), new Vector2[]
        {
            new(0, 0), new(0, 280)
        });
        AddGuard(root, "NaveGuard3", new Vector2(1700, 120), new Vector2[]
        {
            new(0, 0), new(0, 280)
        });
        AddGuard(root, "NaveGuard4", new Vector2(2300, 120), new Vector2[]
        {
            new(0, 0), new(0, 280)
        });
        AddGuard(root, "NaveGuard5", new Vector2(1280, 460), new Vector2[]
        {
            new(-400, 0), new(400, 0)
        });
        AddGuard(root, "NaveGuard6", new Vector2(320, 280), new Vector2[]
        {
            new(0, -120), new(0, 120)
        });
        AddGuard(root, "NaveGuard7", new Vector2(2240, 280), new Vector2[]
        {
            new(0, -120), new(0, 120)
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
        AddAreaZone(root, "Relic Chamber", new Vector2(1280, 200), new Vector2(2560, 400),
            isRestricted: true);

        // Heavy shadow underground — many dark alcoves across the catacombs.
        AddShadowZone(root, new Vector2(120, 24), new Vector2(32, 24));
        AddShadowZone(root, new Vector2(120, 360), new Vector2(32, 24));
        AddShadowZone(root, new Vector2(520, 24), new Vector2(32, 24));
        AddShadowZone(root, new Vector2(520, 360), new Vector2(32, 24));
        AddShadowZone(root, new Vector2(900, 24), new Vector2(32, 24));
        AddShadowZone(root, new Vector2(900, 360), new Vector2(32, 24));
        AddShadowZone(root, new Vector2(1280, 100), new Vector2(40, 24));
        AddShadowZone(root, new Vector2(1660, 24), new Vector2(32, 24));
        AddShadowZone(root, new Vector2(1660, 360), new Vector2(32, 24));
        AddShadowZone(root, new Vector2(2040, 24), new Vector2(32, 24));
        AddShadowZone(root, new Vector2(2040, 360), new Vector2(32, 24));
        AddShadowZone(root, new Vector2(2440, 24), new Vector2(32, 24));
        AddShadowZone(root, new Vector2(2440, 360), new Vector2(32, 24));

        // Hiding spots — sarcophagi, rubble piles, burial niches, collapsed passages.
        AddHidingSpot(root, "Sarcophagus", new Vector2(160, 140));
        AddHidingSpot(root, "Rubble Nook", new Vector2(700, 80));
        AddHidingSpot(root, "Stone Column", new Vector2(1400, 260));
        AddHidingSpot(root, "Collapsed Wall", new Vector2(2360, 140));
        AddHidingSpot(root, "Burial Niche", new Vector2(400, 300));
        AddHidingSpot(root, "Bone Pile", new Vector2(1000, 200));
        AddHidingSpot(root, "Fallen Archway", new Vector2(1800, 100));
        AddHidingSpot(root, "Relic Pedestal", new Vector2(2100, 300));

        // 4 elite guards protecting the relic across the expanded catacombs.
        AddGuard(root, "RelicGuard1", new Vector2(600, 140), new Vector2[]
        {
            new(0, -60), new(0, 100), new(140, 100), new(140, -60)
        }, elite: true);
        AddGuard(root, "RelicGuard2", new Vector2(1960, 140), new Vector2[]
        {
            new(0, -60), new(0, 100)
        }, elite: true);
        AddGuard(root, "RelicGuard3", new Vector2(1280, 320), new Vector2[]
        {
            new(-200, 0), new(200, 0)
        }, elite: true);
        AddGuard(root, "RelicGuard4", new Vector2(2300, 200), new Vector2[]
        {
            new(-100, -60), new(100, 60)
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
        blessing.Position = new Vector2(1280, 200);
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
