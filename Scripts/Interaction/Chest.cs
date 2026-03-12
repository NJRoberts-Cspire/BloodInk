using Godot;

namespace BloodInk.Interaction;

/// <summary>
/// A lootable container. Can hold keys, ink, items, or gadgets.
/// Zelda-style: walk up, press interact, lid opens, item pops out.
/// Can optionally require a key to open.
/// </summary>
public partial class Chest : Interactable
{
    [Signal] public delegate void ChestOpenedEventHandler(string itemId);

    /// <summary>What this chest contains: "key", "ink", "item", "gadget".</summary>
    [Export] public string ContentsType { get; set; } = "key";

    /// <summary>The item/key ID to grant.</summary>
    [Export] public string ContentsId { get; set; } = "";

    /// <summary>Display name of the contents (for UI feedback).</summary>
    [Export] public string ContentsName { get; set; } = "Item";

    /// <summary>Quantity (for stackable items).</summary>
    [Export] public int ContentsQuantity { get; set; } = 1;

    /// <summary>If set, a key is required to open this chest.</summary>
    [Export] public string RequiredKeyId { get; set; } = "";

    /// <summary>Whether this chest has been opened.</summary>
    public bool IsOpened { get; private set; } = false;

    /// <summary>Color when closed.</summary>
    [Export] public Color ClosedColor { get; set; } = new(0.55f, 0.35f, 0.1f, 1.0f);

    /// <summary>Color when open.</summary>
    [Export] public Color OpenedColor { get; set; } = new(0.35f, 0.25f, 0.1f, 0.7f);

    private ColorRect? _bodyVisual;
    private ColorRect? _lidVisual;

    protected override void InteractableReady()
    {
        ActionVerb = "Open";
        OneShot = true;

        // Chest body.
        _bodyVisual = new ColorRect
        {
            Color = ClosedColor,
            Position = new Vector2(-8, -6),
            Size = new Vector2(16, 12),
            ZIndex = 1
        };
        AddChild(_bodyVisual);

        // Lid (lighter color).
        _lidVisual = new ColorRect
        {
            Color = new Color(ClosedColor.R * 1.2f, ClosedColor.G * 1.2f, ClosedColor.B * 0.8f, 1.0f),
            Position = new Vector2(-8, -10),
            Size = new Vector2(16, 4),
            ZIndex = 2
        };
        AddChild(_lidVisual);

        // Lock indicator if locked.
        if (!string.IsNullOrEmpty(RequiredKeyId))
        {
            var lockIcon = new ColorRect
            {
                Color = new Color(0.8f, 0.7f, 0.2f, 1.0f),
                Position = new Vector2(-2, -3),
                Size = new Vector2(4, 4),
                ZIndex = 3
            };
            AddChild(lockIcon);
        }
    }

    public override void OnInteract(Node2D interactor)
    {
        if (IsOpened) return;

        // Check for required key.
        if (!string.IsNullOrEmpty(RequiredKeyId))
        {
            var inv = PlayerInventory.Instance;
            if (inv == null || !inv.HasKey(RequiredKeyId))
            {
                GD.Print($"[Chest] Locked! Requires key: {RequiredKeyId}");
                return;
            }
        }

        IsOpened = true;
        GrantContents();
        UpdateVisual();
        EmitSignal(SignalName.ChestOpened, ContentsId);
        base.OnInteract(interactor);

        GD.Print($"[Chest] Opened! Got: {ContentsName} x{ContentsQuantity}");
    }

    private void GrantContents()
    {
        var inv = PlayerInventory.Instance;
        if (inv == null)
        {
            GD.PrintErr("[Chest] No PlayerInventory found!");
            return;
        }

        switch (ContentsType.ToLower())
        {
            case "key":
                inv.AddKey(ContentsId);
                break;
            case "ink":
                // Grant ink through GameManager.
                var gm = Core.GameManager.Instance;
                gm?.InkInventory?.AddInk(
                    ContentsId.Contains("major") || ContentsId.Contains("blood") ? Ink.InkGrade.Major :
                    ContentsId.Contains("lesser") ? Ink.InkGrade.Lesser :
                    Ink.InkGrade.Trace,
                    ContentsQuantity);
                break;
            default:
                inv.AddItem(ContentsId, ContentsQuantity);
                break;
        }

        // Floating text feedback.
        SpawnPickupText();
    }

    private void SpawnPickupText()
    {
        var label = new Label
        {
            Text = $"Got: {ContentsName}",
            Position = new Vector2(-40, -30),
            ZIndex = 100
        };
        label.AddThemeColorOverride("font_color", new Color(1f, 1f, 0.5f, 1f));
        AddChild(label);

        // Float up and fade.
        var tween = CreateTween();
        tween.SetParallel(true);
        tween.TweenProperty(label, "position:y", -50f, 1.2f);
        tween.TweenProperty(label, "modulate:a", 0f, 1.2f);
        tween.Chain().TweenCallback(Callable.From(() => label.QueueFree()));
    }

    private void UpdateVisual()
    {
        if (_bodyVisual != null)
            _bodyVisual.Color = OpenedColor;

        if (_lidVisual != null)
        {
            // Lid opens — moves up and fades.
            _lidVisual.Position = new Vector2(-10, -16);
            _lidVisual.Color = new Color(_lidVisual.Color, 0.5f);
        }
    }

    public override string GetPromptText()
    {
        if (IsOpened) return "[E] (Empty)";
        if (!string.IsNullOrEmpty(RequiredKeyId))
        {
            var inv = PlayerInventory.Instance;
            if (inv == null || !inv.HasKey(RequiredKeyId))
                return $"[E] Locked (need {PuzzleUtils.HumanizeId(RequiredKeyId)})";
        }
        return base.GetPromptText();
    }
}
