using Godot;
using System.Text;
using BloodInk.Campaigns.Lorne;
using BloodInk.Core;

namespace BloodInk.UI;

/// <summary>
/// Lorne's Crafting Panel — shows known recipes, materials, tremor status,
/// and lets the player attempt to craft consumables.
/// Opened by the "open_crafting" dialogue event from Lorne.
/// </summary>
public partial class CraftingPanel : Control
{
    [Signal] public delegate void PanelClosedEventHandler();

    private VBoxContainer? _recipeList;
    private Panel? _detailPanel;
    private Label? _detailName;
    private RichTextLabel? _detailDesc;
    private Button? _craftButton;
    private RichTextLabel? _statusLabel;
    private Label? _tremorLabel;
    private Label? _materialsLabel;

    private Label? _resultBanner;
    private ColorRect? _resultFlash;

    private CraftingRecipe? _selectedRecipe;

    public override void _Ready()
    {
        BuildUI();
        Visible = false;
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (!Visible) return;
        if (@event.IsActionPressed("pause") || @event.IsActionPressed("interact"))
        {
            Close();
            GetViewport().SetInputAsHandled();
        }
    }

    public void Open()
    {
        Visible = true;
        GameManager.Instance?.SetPaused(true);
        ProcessMode = ProcessModeEnum.Always;
        Refresh();
    }

    public void Close()
    {
        Visible = false;
        GameManager.Instance?.SetPaused(false);
        EmitSignal(SignalName.PanelClosed);
    }

    private void Refresh()
    {
        var gm = GameManager.Instance;
        var crafting = gm?.Crafting;
        var tremor = gm?.Tremor;

        // Tremor display
        if (_tremorLabel != null && tremor != null)
        {
            _tremorLabel.Text = $"Tremor: {tremor.TremorLevel:F0}/100  |  Steadiness: {tremor.Steadiness:F0}";
        }

        // Materials display
        if (_materialsLabel != null && crafting != null)
        {
            var sb = new StringBuilder("Materials: ");
            bool any = false;
            foreach (MaterialType mat in System.Enum.GetValues<MaterialType>())
            {
                int qty = crafting.GetMaterialCount(mat);
                if (qty > 0)
                {
                    if (any) sb.Append(", ");
                    sb.Append($"{mat}: {qty}");
                    any = true;
                }
            }
            if (!any) sb.Append("(none)");
            _materialsLabel.Text = sb.ToString();
        }

        // Recipe list
        if (_recipeList == null) return;
        foreach (var child in _recipeList.GetChildren()) child.QueueFree();

        if (crafting == null)
        {
            _recipeList.AddChild(new Label { Text = "(Crafting unavailable)" });
            return;
        }

        bool anyRecipe = false;
        foreach (var recipe in crafting.GetKnownRecipes())
        {
            anyRecipe = true;
            bool canCraft = crafting.HasMaterials(recipe);
            var btn = new Button
            {
                Text = $"{recipe.ResultName}{(canCraft ? "" : " [need mats]")}",
                Disabled = false
            };
            btn.SizeFlagsHorizontal = SizeFlags.ExpandFill;
            CraftingRecipe captured = recipe;
            btn.Pressed += () => SelectRecipe(captured);
            _recipeList.AddChild(btn);
        }

        if (!anyRecipe)
            _recipeList.AddChild(new Label { Text = "(No recipes known yet)" });
    }

    private void SelectRecipe(CraftingRecipe recipe)
    {
        _selectedRecipe = recipe;
        if (_detailPanel != null) _detailPanel.Visible = true;
        if (_detailName != null) _detailName.Text = recipe.ResultName;

        if (_detailDesc != null)
        {
            var sb = new StringBuilder();
            sb.AppendLine(recipe.Description);
            sb.AppendLine();
            sb.Append("Requires: ");
            for (int i = 0; i < recipe.RequiredMaterialTypes.Length; i++)
            {
                var mat = (MaterialType)recipe.RequiredMaterialTypes[i];
                int qty = i < recipe.RequiredMaterialCounts.Length ? recipe.RequiredMaterialCounts[i] : 1;
                if (i > 0) sb.Append(", ");
                sb.Append($"{qty}× {mat}");
            }
            sb.AppendLine();
            sb.AppendLine($"Difficulty: {recipe.Difficulty}/10  |  Steadiness needed: {recipe.SteadinessRequired}");
            _detailDesc.Text = sb.ToString();
        }

        var gm = GameManager.Instance;
        bool canCraft = gm?.Crafting?.HasMaterials(recipe) ?? false;
        if (_craftButton != null)
        {
            _craftButton.Disabled = !canCraft;
            _craftButton.Text = canCraft ? "Craft" : "Missing Materials";
        }
    }

    private void OnCraftPressed()
    {
        if (_selectedRecipe == null) return;
        var gm = GameManager.Instance;
        if (gm?.Crafting == null) return;

        var quality = gm.Crafting.Craft(_selectedRecipe.Id);

        // ── Result banner (large quality name) ──────────────────────
        string bannerText;
        Color  bannerColor;
        string statusText;

        switch (quality)
        {
            case CraftQuality.Masterwork:
                bannerText  = "★ MASTERWORK";
                bannerColor = new Color(1f, 0.85f, 0.15f);
                statusText  = $"[color=#ffd924]Masterwork {_selectedRecipe.ResultName}! Lorne's hands were steady today.[/color]";
                break;
            case CraftQuality.Standard:
                bannerText  = "CRAFTED";
                bannerColor = new Color(0.35f, 0.85f, 0.45f);
                statusText  = $"[color=#58d975]{_selectedRecipe.ResultName} crafted. Clean work.[/color]";
                break;
            case CraftQuality.Flawed:
                bannerText  = "FLAWED";
                bannerColor = new Color(0.9f, 0.75f, 0.2f);
                statusText  = $"[color=#e6bf35]{_selectedRecipe.ResultName} crafted, but flawed. The tremor showed.[/color]";
                break;
            case CraftQuality.Ruined:
                bannerText  = "RUINED";
                bannerColor = new Color(0.85f, 0.2f, 0.15f);
                statusText  = "[color=#d93525]Ruined. The tremor took it. Materials lost.[/color]";
                break;
            default:
                bannerText  = "FAILED";
                bannerColor = new Color(0.85f, 0.2f, 0.15f);
                statusText  = "[color=#d93525]Crafting failed — check materials and tremor level.[/color]";
                break;
        }

        if (_resultBanner != null)
        {
            _resultBanner.Text    = bannerText;
            _resultBanner.Modulate = new Color(bannerColor.R, bannerColor.G, bannerColor.B, 1f);
            // Fade the banner out after 2 s.
            var tween = _resultBanner.CreateTween();
            tween.TweenProperty(_resultBanner, "modulate:a", 1f, 0.1f);
            tween.TweenInterval(1.8f);
            tween.TweenProperty(_resultBanner, "modulate:a", 0f, 0.6f);
        }

        if (_resultFlash != null)
        {
            _resultFlash.Color = new Color(bannerColor.R, bannerColor.G, bannerColor.B, 0.25f);
            _resultFlash.Visible = true;
            var tween = _resultFlash.CreateTween();
            tween.TweenProperty(_resultFlash, "modulate:a", 0f, 0.5f);
            tween.TweenCallback(Callable.From(() => _resultFlash.Visible = false));
        }

        if (_statusLabel != null)
            _statusLabel.Text = statusText;

        Refresh();
    }

    private void BuildUI()
    {
        SetAnchorsPreset(LayoutPreset.FullRect);

        var bg = new ColorRect { Color = new Color(0.04f, 0.03f, 0.06f, 0.92f) };
        bg.SetAnchorsPreset(LayoutPreset.FullRect);
        AddChild(bg);

        // Title row
        var titleLabel = new Label { Text = "Lorne's Workshop", Position = new Vector2(10, 6), Size = new Vector2(500, 24) };
        AddChild(titleLabel);

        _tremorLabel = new Label { Position = new Vector2(520, 6), Size = new Vector2(480, 24) };
        AddChild(_tremorLabel);

        var closeBtn = new Button { Text = "Close [E]", Position = new Vector2(1160, 4), Size = new Vector2(108, 28) };
        closeBtn.Pressed += Close;
        AddChild(closeBtn);

        // Materials row
        _materialsLabel = new Label { Position = new Vector2(10, 36), Size = new Vector2(1260, 24) };
        AddChild(_materialsLabel);

        var sep = new HSeparator { Position = new Vector2(0, 64), Size = new Vector2(1280, 4) };
        AddChild(sep);

        // Left — recipe list
        var recipeTitle = new Label { Text = "Known Recipes:", Position = new Vector2(10, 72), Size = new Vector2(380, 24) };
        AddChild(recipeTitle);

        var scroll = new ScrollContainer { Position = new Vector2(10, 100), Size = new Vector2(380, 580) };
        _recipeList = new VBoxContainer();
        _recipeList.SizeFlagsHorizontal = SizeFlags.ExpandFill;
        scroll.AddChild(_recipeList);
        AddChild(scroll);

        // Right — detail panel
        _detailPanel = new Panel { Position = new Vector2(410, 72), Size = new Vector2(858, 608), Visible = false };
        AddChild(_detailPanel);

        _detailName = new Label { Position = new Vector2(8, 8), Size = new Vector2(840, 28) };
        _detailPanel.AddChild(_detailName);

        _detailDesc = new RichTextLabel { Position = new Vector2(8, 44), Size = new Vector2(840, 260), BbcodeEnabled = false };
        _detailPanel.AddChild(_detailDesc);

        _craftButton = new Button { Text = "Craft", Position = new Vector2(8, 316), Size = new Vector2(160, 40) };
        _craftButton.Pressed += OnCraftPressed;
        _detailPanel.AddChild(_craftButton);

        // Flash overlay — briefly colours the whole detail panel on craft result.
        _resultFlash = new ColorRect
        {
            Position = Vector2.Zero,
            Size     = new Vector2(858, 608),
            Color    = new Color(0, 0, 0, 0),
            Visible  = false,
            ZIndex   = 5,
        };
        _detailPanel.AddChild(_resultFlash);

        // Large quality-name banner, centred above the status label.
        _resultBanner = new Label
        {
            Text                = "",
            Position            = new Vector2(8, 360),
            Size                = new Vector2(840, 36),
            HorizontalAlignment = HorizontalAlignment.Center,
            Modulate            = new Color(1, 1, 1, 0),
            ZIndex              = 6,
        };
        _resultBanner.AddThemeFontSizeOverride("font_size", 22);
        _detailPanel.AddChild(_resultBanner);

        _statusLabel = new RichTextLabel { Position = new Vector2(8, 404), Size = new Vector2(840, 100), BbcodeEnabled = true };
        _detailPanel.AddChild(_statusLabel);
    }
}
