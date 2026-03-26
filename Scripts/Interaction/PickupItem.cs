using Godot;

namespace BloodInk.Interaction;

/// <summary>
/// A collectible item in the world. Picking it up emits a signal
/// with the item ID that the inventory/game systems can respond to.
/// </summary>
public partial class PickupItem : Interactable
{
    [Signal] public delegate void ItemPickedUpEventHandler(string itemId, string itemType);

    /// <summary>Unique ID for this item type.</summary>
    [Export] public string ItemId { get; set; } = "";

    /// <summary>Category: "ink", "key", "material", "lore", "gadget".</summary>
    [Export] public string ItemType { get; set; } = "item";

    /// <summary>How many of this item to grant.</summary>
    [Export] public int Quantity { get; set; } = 1;

    protected override void InteractableReady()
    {
        ActionVerb = "Pick up";
        OneShot = true;
    }

    public override void OnInteract(Node2D interactor)
    {
        EmitSignal(SignalName.ItemPickedUp, ItemId, ItemType);
        GD.Print($"Picked up: {DisplayName} x{Quantity} ({ItemType})");

        // Keys are registered directly into the player inventory so Door can check them.
        if (ItemType == "key" && !string.IsNullOrEmpty(ItemId))
            PlayerInventory.Instance?.AddKey(ItemId);

        base.OnInteract(interactor);

        // Despawn the item.
        QueueFree();
    }
}
