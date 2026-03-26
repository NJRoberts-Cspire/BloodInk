using Godot;
using BloodInk.Stealth;
using BloodInk.UI;

namespace BloodInk.Interaction;

/// <summary>
/// A door that can be opened/closed. Optionally locked (requires a key).
/// Opening/closing doors generates noise.
/// </summary>
public partial class Door : Interactable
{
    [Export] public bool IsOpen { get; set; } = false;
    [Export] public bool IsLocked { get; set; } = false;
    [Export] public string RequiredKeyId { get; set; } = "";

    /// <summary>Noise radius when opening/closing.</summary>
    [Export] public float NoiseRadius { get; set; } = 60f;

    /// <summary>The collision body that blocks passage when closed.</summary>
    private StaticBody2D? _collisionBody;
    private Sprite2D? _sprite;

    protected override void InteractableReady()
    {
        _collisionBody = GetNodeOrNull<StaticBody2D>("StaticBody2D");
        _sprite = GetNodeOrNull<Sprite2D>("Sprite2D");
        ActionVerb = IsOpen ? "Close" : "Open";
        UpdateDoorState();
    }

    public override void OnInteract(Node2D interactor)
    {
        if (IsLocked)
        {
            // Check player inventory for the required key.
            var inventory = PlayerInventory.Instance;
            if (inventory != null && !string.IsNullOrEmpty(RequiredKeyId) && inventory.HasKey(RequiredKeyId))
            {
                Unlock();
                GD.Print($"Door unlocked with key: {RequiredKeyId}");
            }
            else
            {
                GD.Print($"Door is locked. Requires key: {RequiredKeyId}");
                GameHUD.ShowLockedMessage(GetTree(), PuzzleUtils.HumanizeId(RequiredKeyId));
                return;
            }
        }

        IsOpen = !IsOpen;
        ActionVerb = IsOpen ? "Close" : "Open";
        UpdateDoorState();

        // Door noise.
        NoisePropagator.Instance?.PropagateNoise(GlobalPosition, NoiseRadius);

        base.OnInteract(interactor);
        GD.Print($"Door {(IsOpen ? "opened" : "closed")}.");
    }

    public void Unlock()
    {
        IsLocked = false;
        GD.Print("Door unlocked.");
    }

    private void UpdateDoorState()
    {
        if (_collisionBody != null)
        {
            // Disable collision when open.
            _collisionBody.ProcessMode = IsOpen ? ProcessModeEnum.Disabled : ProcessModeEnum.Inherit;
        }

        // Visual feedback — could swap sprite frame, rotate, etc.
        if (_sprite != null)
        {
            _sprite.Frame = IsOpen ? 1 : 0;
        }
    }

    public override string GetPromptText()
    {
        if (IsLocked && !string.IsNullOrEmpty(RequiredKeyId))
        {
            var inv = PlayerInventory.Instance;
            if (inv == null || !inv.HasKey(RequiredKeyId))
                return $"[E] Locked (need {PuzzleUtils.HumanizeId(RequiredKeyId)})";
        }
        return base.GetPromptText();
    }
}
