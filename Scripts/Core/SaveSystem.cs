using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

using FileAccess = Godot.FileAccess;

namespace BloodInk.Core;

/// <summary>
/// Handles saving and loading all game state to/from disk.
/// Uses Godot's user:// directory for cross-platform save files.
/// </summary>
public partial class SaveSystem : Node
{
    [Signal] public delegate void GameSavedEventHandler(string slotName);
    [Signal] public delegate void GameLoadedEventHandler(string slotName);

    /// <summary>Number of save slots available.</summary>
    public const int MaxSlots = 3;

    /// <summary>Save file directory.</summary>
    private const string SaveDir = "user://saves/";

    // ─── Save ─────────────────────────────────────────────────────

    /// <summary>
    /// Save the entire game state to a named slot.
    /// All systems contribute their serialization data.
    /// </summary>
    public void SaveGame(string slotName, Dictionary<string, Dictionary<string, object>> systemsData)
    {
        // Ensure directory exists.
        DirAccess.MakeDirRecursiveAbsolute(SaveDir);

        string filePath = $"{SaveDir}{slotName}.json";

        var saveData = new Dictionary<string, object>
        {
            ["version"] = 1,
            ["timestamp"] = Time.GetDatetimeStringFromSystem(),
            ["systems"] = systemsData
        };

        string json = JsonSerializer.Serialize(saveData, new JsonSerializerOptions
        {
            WriteIndented = true
        });

        using var file = FileAccess.Open(filePath, FileAccess.ModeFlags.Write);
        if (file == null)
        {
            GD.PrintErr($"Failed to open save file: {filePath} — {FileAccess.GetOpenError()}");
            return;
        }

        file.StoreString(json);
        EmitSignal(SignalName.GameSaved, slotName);
        GD.Print($"Game saved to slot '{slotName}'.");
    }

    // ─── Load ─────────────────────────────────────────────────────

    /// <summary>
    /// Load game state from a named slot.
    /// Returns the deserialized systems data, or null on failure.
    /// </summary>
    public Dictionary<string, Dictionary<string, object>>? LoadGame(string slotName)
    {
        string filePath = $"{SaveDir}{slotName}.json";

        if (!FileAccess.FileExists(filePath))
        {
            GD.PrintErr($"Save file not found: {filePath}");
            return null;
        }

        using var file = FileAccess.Open(filePath, FileAccess.ModeFlags.Read);
        if (file == null)
        {
            GD.PrintErr($"Failed to open save file: {filePath} — {FileAccess.GetOpenError()}");
            return null;
        }

        string json = file.GetAsText();

        try
        {
            var saveData = JsonSerializer.Deserialize<Dictionary<string, object>>(json);
            if (saveData == null) return null;

            // Extract systems data.
            if (saveData.TryGetValue("systems", out var systems))
            {
                // Re-deserialize the systems portion specifically.
                string systemsJson = JsonSerializer.Serialize(systems);
                var result = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, object>>>(systemsJson);

                // System.Text.Json deserializes values as JsonElement, not int/bool/string.
                // Unbox them to their actual CLR types so downstream 'is int' checks work.
                if (result != null)
                {
                    foreach (var key in result.Keys)
                    {
                        var inner = result[key];
                        var unboxed = new Dictionary<string, object>(inner.Count);
                        foreach (var kv in inner)
                            unboxed[kv.Key] = UnboxJsonElement(kv.Value);
                        result[key] = unboxed;
                    }
                }

                EmitSignal(SignalName.GameLoaded, slotName);
                GD.Print($"Game loaded from slot '{slotName}'.");
                return result;
            }
        }
        catch (JsonException ex)
        {
            GD.PrintErr($"Failed to parse save file: {ex.Message}");
        }

        return null;
    }

    // ─── Slot Management ──────────────────────────────────────────

    /// <summary>
    /// Convert System.Text.Json JsonElement values to their CLR equivalents
    /// so that downstream code using 'is int', 'is bool', etc. works correctly.
    /// </summary>
    private static object UnboxJsonElement(object val)
    {
        if (val is not JsonElement je) return val;

        return je.ValueKind switch
        {
            JsonValueKind.Number => je.TryGetInt32(out var i) ? (object)i
                                  : je.TryGetSingle(out var f) ? (object)f
                                  : je.GetDouble(),
            JsonValueKind.True => (object)true,
            JsonValueKind.False => (object)false,
            JsonValueKind.String => (object)(je.GetString() ?? ""),
            JsonValueKind.Array => je.EnumerateArray()
                                     .Select(e => UnboxJsonElement(e))
                                     .ToList(),
            JsonValueKind.Object => je.EnumerateObject()
                                      .ToDictionary(p => p.Name, p => UnboxJsonElement(p.Value)),
            _ => val
        };
    }

    /// <summary>Check if a save slot exists.</summary>
    public static bool SaveExists(string slotName)
    {
        return FileAccess.FileExists($"{SaveDir}{slotName}.json");
    }

    /// <summary>Delete a save slot.</summary>
    public void DeleteSave(string slotName)
    {
        string filePath = $"{SaveDir}{slotName}.json";
        if (FileAccess.FileExists(filePath))
        {
            DirAccess.RemoveAbsolute(filePath);
            GD.Print($"Save slot '{slotName}' deleted.");
        }
    }

    /// <summary>Get save file info (timestamp, etc.) without fully loading.</summary>
    public Dictionary<string, string>? GetSaveInfo(string slotName)
    {
        string filePath = $"{SaveDir}{slotName}.json";
        if (!FileAccess.FileExists(filePath)) return null;

        using var file = FileAccess.Open(filePath, FileAccess.ModeFlags.Read);
        if (file == null) return null;

        string json = file.GetAsText();
        try
        {
            var data = JsonSerializer.Deserialize<Dictionary<string, object>>(json);
            if (data == null) return null;

            var info = new Dictionary<string, string>
            {
                ["slot"] = slotName,
                ["timestamp"] = data.TryGetValue("timestamp", out var ts) ? ts.ToString() ?? "" : "",
                ["version"] = data.TryGetValue("version", out var v) ? v.ToString() ?? "1" : "1"
            };
            return info;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>List all existing save slots.</summary>
    public List<string> GetSaveSlots()
    {
        var slots = new List<string>();
        var dir = DirAccess.Open(SaveDir);
        if (dir == null) return slots;

        dir.ListDirBegin();
        while (true)
        {
            string fileName = dir.GetNext();
            if (string.IsNullOrEmpty(fileName)) break;
            if (fileName.EndsWith(".json"))
                slots.Add(fileName.Replace(".json", ""));
        }
        dir.ListDirEnd();
        return slots;
    }
}
