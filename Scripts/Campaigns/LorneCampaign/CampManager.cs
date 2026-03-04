using Godot;
using System;
using System.Collections.Generic;

namespace BloodInk.Campaigns.Lorne;

/// <summary>
/// Manages the camp during Lorne's campaign — resource management,
/// rest cycles, and the tension between pushing forward vs. maintaining
/// Lorne's ability to work. The camp is where crafting, rest, and
/// tattoo application happen between missions.
/// </summary>
public partial class CampManager : Node
{
    [Signal] public delegate void CampPhaseChangedEventHandler(int phase);
    [Signal] public delegate void SuppliesChangedEventHandler(int food, int medicine);
    [Signal] public delegate void CampEventOccurredEventHandler(string eventText);

    /// <summary>Current camp phase / day counter.</summary>
    public int Day { get; private set; } = 1;

    /// <summary>Food supplies. Consumed each day. At 0, morale drops.</summary>
    public int Food { get; set; } = 20;

    /// <summary>Medicine supplies. Used for treating injuries and tremor herbs.</summary>
    public int Medicine { get; set; } = 10;

    /// <summary>Camp morale. Affects crafting focus and available options.</summary>
    public int Morale { get; set; } = 70;

    /// <summary>Whether enemies know the camp location (raises danger).</summary>
    public bool CampDiscovered { get; set; } = false;

    /// <summary>Danger level if discovered. Increases each day camp stays put.</summary>
    public int DangerLevel { get; set; } = 0;

    /// <summary>Reference to LorneCampaign tremor system.</summary>
    public TremorSystem? Tremor { get; set; }

    /// <summary>Reference to crafting system.</summary>
    public CraftingSystem? Crafting { get; set; }

    private readonly Random _rng = new();

    // ─── Day Cycle ────────────────────────────────────────────────

    /// <summary>
    /// Advance to the next day. Consumes food, applies events, advances tremor.
    /// </summary>
    public void AdvanceDay()
    {
        Day++;

        // Consume food.
        Food = Math.Max(0, Food - 3);
        if (Food <= 0)
        {
            Morale = Math.Max(0, Morale - 15);
            EmitSignal(SignalName.CampEventOccurred, "No food remaining. Camp morale plummets.");
        }

        // Danger escalation if discovered.
        if (CampDiscovered)
        {
            DangerLevel += 10;
            if (DangerLevel >= 80)
                EmitSignal(SignalName.CampEventOccurred, "The enemy is closing in. You must move camp or fight.");
        }

        // Random camp events.
        RollCampEvent();

        // Advance tremor system.
        Tremor?.AdvanceTurn();

        EmitSignal(SignalName.CampPhaseChanged, Day);
        EmitSignal(SignalName.SuppliesChanged, Food, Medicine);
        GD.Print($"Day {Day} — Food: {Food}, Medicine: {Medicine}, Morale: {Morale}, Danger: {DangerLevel}");
    }

    // ─── Camp Actions ─────────────────────────────────────────────

    /// <summary>
    /// Lorne rests for the day. Reduces tremor, restores some morale.
    /// </summary>
    public void Rest()
    {
        Tremor?.Rest();
        Morale = Math.Min(100, Morale + 5);
        GD.Print("Lorne rests. Tremor eases, morale improves slightly.");
    }

    /// <summary>
    /// Use medicine to make calming herb tea. Reduces tremor more than rest.
    /// </summary>
    public bool UseHerbalRemedy()
    {
        if (Medicine < 3)
        {
            GD.PrintErr("Not enough medicine for herbal remedy.");
            return false;
        }

        Medicine -= 3;
        Tremor?.UseHerbs();
        EmitSignal(SignalName.SuppliesChanged, Food, Medicine);
        return true;
    }

    /// <summary>
    /// Forage for supplies. Takes a day action, returns food and possibly materials.
    /// </summary>
    public void Forage()
    {
        int foodFound = _rng.Next(2, 6);
        Food += foodFound;

        // Chance of finding crafting materials.
        if (_rng.NextDouble() < 0.4)
        {
            int herbs = _rng.Next(1, 3);
            Crafting?.AddMaterial(MaterialType.HerbBundle, herbs);
            GD.Print($"Foraged: +{foodFound} food, +{herbs} herb bundles.");
        }
        else
        {
            GD.Print($"Foraged: +{foodFound} food.");
        }

        // Foraging risks discovery.
        if (!CampDiscovered && _rng.NextDouble() < 0.1)
        {
            CampDiscovered = true;
            EmitSignal(SignalName.CampEventOccurred, "Patrols spotted near camp during foraging. Camp location compromised.");
        }

        EmitSignal(SignalName.SuppliesChanged, Food, Medicine);
    }

    /// <summary>
    /// Move camp to a new location. Resets danger, costs food, costs a day.
    /// </summary>
    public void RelocateCamp()
    {
        int foodCost = 5;
        if (Food < foodCost)
        {
            GD.PrintErr("Not enough food to relocate camp.");
            return;
        }

        Food -= foodCost;
        CampDiscovered = false;
        DangerLevel = 0;
        Tremor?.ResetHerbTolerance();

        // Moving is stressful — slight tremor increase.
        Tremor?.OnDelicateWork(2);

        EmitSignal(SignalName.CampEventOccurred, "Camp relocated. Danger reduced, herb tolerance reset.");
        EmitSignal(SignalName.SuppliesChanged, Food, Medicine);
        GD.Print("Camp relocated successfully.");
    }

    // ─── Random Events ────────────────────────────────────────────

    private void RollCampEvent()
    {
        double roll = _rng.NextDouble();

        if (roll < 0.05)
        {
            // Rare positive event.
            Medicine += 5;
            EmitSignal(SignalName.CampEventOccurred, "A sympathetic healer passes through and leaves supplies.");
        }
        else if (roll < 0.1)
        {
            // Negative event.
            Food = Math.Max(0, Food - 3);
            EmitSignal(SignalName.CampEventOccurred, "Vermin got into the food stores.");
        }
        else if (roll < 0.13 && !CampDiscovered)
        {
            CampDiscovered = true;
            EmitSignal(SignalName.CampEventOccurred, "A patrol stumbled across signs of the camp.");
        }
        else if (roll < 0.16)
        {
            Morale = Math.Min(100, Morale + 10);
            EmitSignal(SignalName.CampEventOccurred, "A quiet night. Dreams of home. Morale rises.");
        }
        // Else: nothing notable happens.
    }

    // ─── Serialization ────────────────────────────────────────────

    public Dictionary<string, object> Serialize()
    {
        return new Dictionary<string, object>
        {
            ["day"] = Day,
            ["food"] = Food,
            ["medicine"] = Medicine,
            ["morale"] = Morale,
            ["discovered"] = CampDiscovered,
            ["danger"] = DangerLevel
        };
    }

    public void Deserialize(Dictionary<string, object> data)
    {
        if (data.TryGetValue("day", out var d) && d is int di) Day = di;
        if (data.TryGetValue("food", out var f) && f is int fi) Food = fi;
        if (data.TryGetValue("medicine", out var m) && m is int mi) Medicine = mi;
        if (data.TryGetValue("morale", out var mo) && mo is int moi) Morale = moi;
        if (data.TryGetValue("discovered", out var disc) && disc is bool db) CampDiscovered = db;
        if (data.TryGetValue("danger", out var dng) && dng is int dngi) DangerLevel = dngi;
    }
}
