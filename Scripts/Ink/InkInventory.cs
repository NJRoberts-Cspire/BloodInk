using Godot;
using System.Collections.Generic;

namespace BloodInk.Ink;

/// <summary>
/// Tracks harvested ink from killed targets. Consumed when applying tattoos.
/// </summary>
public partial class InkInventory : Node
{
    [Signal] public delegate void InkChangedEventHandler(int grade, int amount);

    private readonly Dictionary<InkGrade, int> _ink = new()
    {
        { InkGrade.Major, 0 },
        { InkGrade.Lesser, 0 },
        { InkGrade.Trace, 0 }
    };

    /// <summary>Amount of ink of a specific grade.</summary>
    public int GetInk(InkGrade grade) => _ink[grade];

    /// <summary>Add ink after killing a target.</summary>
    public void AddInk(InkGrade grade, int amount)
    {
        _ink[grade] += amount;
        EmitSignal(SignalName.InkChanged, (int)grade, _ink[grade]);
        GD.Print($"Ink harvested: +{amount} {grade} (total: {_ink[grade]})");
    }

    /// <summary>Try to spend ink. Returns false if insufficient.</summary>
    public bool SpendInk(InkGrade grade, int amount)
    {
        if (_ink[grade] < amount) return false;
        _ink[grade] -= amount;
        EmitSignal(SignalName.InkChanged, (int)grade, _ink[grade]);
        return true;
    }

    public bool CanAfford(InkGrade grade, int amount) => _ink[grade] >= amount;

    /// <summary>Save state to a dictionary for serialization.</summary>
    public Dictionary<string, int> Serialize()
    {
        return new Dictionary<string, int>
        {
            ["Major"] = _ink[InkGrade.Major],
            ["Lesser"] = _ink[InkGrade.Lesser],
            ["Trace"] = _ink[InkGrade.Trace]
        };
    }

    public void Deserialize(Dictionary<string, int> data)
    {
        if (data.TryGetValue("Major", out var major)) _ink[InkGrade.Major] = major;
        if (data.TryGetValue("Lesser", out var lesser)) _ink[InkGrade.Lesser] = lesser;
        if (data.TryGetValue("Trace", out var trace)) _ink[InkGrade.Trace] = trace;
    }
}
