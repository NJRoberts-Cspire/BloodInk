using Godot;
using System.Collections.Generic;

namespace BloodInk.Interaction;

/// <summary>
/// Simple key/item inventory carried by the player.
/// Tracks keys, gadgets, and consumables by string ID.
/// Accessed by doors, chests, and puzzle elements to check requirements.
/// </summary>
public partial class PlayerInventory : Node
{
    [Signal] public delegate void ItemAddedEventHandler(string itemId, int newCount);
    [Signal] public delegate void ItemRemovedEventHandler(string itemId, int newCount);
    [Signal] public delegate void KeyAcquiredEventHandler(string keyId);

    /// <summary>Item ID → count.</summary>
    private readonly Dictionary<string, int> _items = new();

    /// <summary>Set of key IDs the player holds (keys are permanent, not consumed on use).</summary>
    private readonly HashSet<string> _keys = new();

    /// <summary>Singleton — one inventory per player, found via group.</summary>
    public static PlayerInventory? Instance { get; private set; }

    public override void _Ready()
    {
        Instance = this;
    }
    public override void _ExitTree()
    {
        if (Instance == this) Instance = null;
    }
    // ─── Keys ─────────────────────────────────────────────────────

    /// <summary>Add a key to the inventory.</summary>
    public void AddKey(string keyId)
    {
        if (string.IsNullOrEmpty(keyId)) return;
        _keys.Add(keyId);
        EmitSignal(SignalName.KeyAcquired, keyId);
        GD.Print($"[Inventory] Key acquired: {keyId}");
    }

    /// <summary>Check if player has a specific key.</summary>
    public bool HasKey(string keyId) => _keys.Contains(keyId);

    /// <summary>Consume a key (for single-use keys). Returns true if consumed.</summary>
    public bool ConsumeKey(string keyId)
    {
        if (!_keys.Contains(keyId)) return false;
        _keys.Remove(keyId);
        return true;
    }

    // ─── Items ────────────────────────────────────────────────────

    /// <summary>Add items to inventory.</summary>
    public void AddItem(string itemId, int quantity = 1)
    {
        if (string.IsNullOrEmpty(itemId)) return;
        _items.TryGetValue(itemId, out int current);
        _items[itemId] = current + quantity;
        EmitSignal(SignalName.ItemAdded, itemId, _items[itemId]);
        GD.Print($"[Inventory] +{quantity} {itemId} (total: {_items[itemId]})");
    }

    /// <summary>Check if player has at least <paramref name="quantity"/> of an item.</summary>
    public bool HasItem(string itemId, int quantity = 1)
    {
        _items.TryGetValue(itemId, out int current);
        return current >= quantity;
    }

    /// <summary>Consume items. Returns true if successful.</summary>
    public bool ConsumeItem(string itemId, int quantity = 1)
    {
        if (!HasItem(itemId, quantity)) return false;
        _items[itemId] -= quantity;
        if (_items[itemId] <= 0) _items.Remove(itemId);
        EmitSignal(SignalName.ItemRemoved, itemId, _items.GetValueOrDefault(itemId, 0));
        return true;
    }

    /// <summary>Get current count of an item.</summary>
    public int GetItemCount(string itemId) => _items.GetValueOrDefault(itemId, 0);

    // ─── Serialization ────────────────────────────────────────────

    public Dictionary<string, object> Serialize()
    {
        var data = new Dictionary<string, object>();
        var itemsCopy = new Dictionary<string, int>(_items);
        var keysList = new List<string>(_keys);
        data["items"] = itemsCopy;
        data["keys"] = keysList;
        return data;
    }

    public void Deserialize(Dictionary<string, object> data)
    {
        _items.Clear();
        _keys.Clear();

        if (data.TryGetValue("items", out var itemsObj) && itemsObj is Dictionary<string, int> items)
        {
            foreach (var kv in items) _items[kv.Key] = kv.Value;
        }

        if (data.TryGetValue("keys", out var keysObj) && keysObj is List<string> keys)
        {
            foreach (var k in keys) _keys.Add(k);
        }
    }
}
