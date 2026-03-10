using Godot;
using System;
using System.Collections.Generic;
using BloodInk.Ink;
using BloodInk.Content;

namespace BloodInk.UI;

/// <summary>
/// The Ink Tent UI — where the Needlewise applies tattoos.
/// Shows available tattoos organized by body slot, ink costs,
/// temperament effects, and stat previews. Handles the apply flow.
/// </summary>
public partial class InkTentPanel : Control
{
    [Signal] public delegate void TattooSelectedEventHandler(string tattooId);
    [Signal] public delegate void PanelClosedEventHandler();

    // ─── Node references ──────────────────────────────────────────
    private VBoxContainer? _slotList;
    private VBoxContainer? _tattooList;
    private Panel? _detailPanel;
    private Label? _detailName;
    private RichTextLabel? _detailDesc;
    private Label? _detailCost;
    private Label? _detailStats;
    private RichTextLabel? _detailWhisper;
    private Button? _applyButton;
    private Label? _inkMajorLabel;
    private Label? _inkLesserLabel;
    private Label? _inkTraceLabel;

    private TattooData? _selectedTattoo;
    private TattooSlot _currentSlot = TattooSlot.Arms_Shadow;

    // ─── Tattoo data ──────────────────────────────────────────────
    private TattooData[] _allTattoos = Array.Empty<TattooData>();

    public override void _Ready()
    {
        // Wire up node references.
        _slotList = GetNodeOrNull<VBoxContainer>("HSplit/SlotList");
        _tattooList = GetNodeOrNull<VBoxContainer>("HSplit/TattooList");
        _detailPanel = GetNodeOrNull<Panel>("HSplit/DetailPanel");
        _detailName = GetNodeOrNull<Label>("HSplit/DetailPanel/Name");
        _detailDesc = GetNodeOrNull<RichTextLabel>("HSplit/DetailPanel/Description");
        _detailCost = GetNodeOrNull<Label>("HSplit/DetailPanel/Cost");
        _detailStats = GetNodeOrNull<Label>("HSplit/DetailPanel/Stats");
        _detailWhisper = GetNodeOrNull<RichTextLabel>("HSplit/DetailPanel/Whisper");
        _applyButton = GetNodeOrNull<Button>("HSplit/DetailPanel/ApplyButton");
        _inkMajorLabel = GetNodeOrNull<Label>("InkBar/MajorInk");
        _inkLesserLabel = GetNodeOrNull<Label>("InkBar/LesserInk");
        _inkTraceLabel = GetNodeOrNull<Label>("InkBar/TraceInk");

        _applyButton?.Connect("pressed", Callable.From(OnApplyPressed));

        Visible = false;
        _allTattoos = TattooRegistry.GetAll();

        BuildSlotButtons();
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

    // ─── Open / Close ─────────────────────────────────────────────

    private bool _wasPausedBeforeOpen;

    public void Open()
    {
        Visible = true;
        _wasPausedBeforeOpen = GetTree().Paused;
        Core.GameManager.Instance?.SetPaused(true);
        ProcessMode = ProcessModeEnum.Always;
        RefreshInkDisplay();
        ShowSlot(_currentSlot);
    }

    public void Close()
    {
        Visible = false;
        // Restore prior pause state instead of unconditionally unpausing.
        if (!_wasPausedBeforeOpen)
            Core.GameManager.Instance?.SetPaused(false);
        EmitSignal(SignalName.PanelClosed);
    }

    // ─── Slot Selection ───────────────────────────────────────────

    private void BuildSlotButtons()
    {
        if (_slotList == null) return;

        foreach (var child in _slotList.GetChildren())
            child.QueueFree();

        var slotNames = new Dictionary<TattooSlot, string>
        {
            { TattooSlot.Arms_Shadow,   "Arms — Shadow Marks" },
            { TattooSlot.Chest_Fang,    "Chest — Fang Lines" },
            { TattooSlot.Legs_Vein,     "Legs — Vein Scripts" },
            { TattooSlot.Head_Skull,    "Head — Skull Wards" },
            { TattooSlot.Back_Spine,    "Back — Spine Chains" },
            { TattooSlot.Hands_Whisper, "Hands — Whisper Rings" }
        };

        foreach (var (slot, name) in slotNames)
        {
            var btn = new Button { Text = name };
            TattooSlot s = slot; // capture
            btn.Pressed += () => ShowSlot(s);
            btn.SizeFlagsHorizontal = SizeFlags.ExpandFill;
            _slotList.AddChild(btn);
        }
    }

    private void ShowSlot(TattooSlot slot)
    {
        _currentSlot = slot;
        if (_tattooList == null) return;

        foreach (var child in _tattooList.GetChildren())
            child.QueueFree();

        var gm = Core.GameManager.Instance;
        var tattooSystem = gm?.TattooSystem;

        foreach (var tattoo in _allTattoos)
        {
            if (tattoo.Slot != slot) continue;

            // Check if the required kingdom Edictbearer has been killed (for Major grade).
            bool locked = false;
            if (tattoo.RequiredKingdomIndex >= 0 && gm != null)
            {
                locked = !(gm.Kingdoms[tattoo.RequiredKingdomIndex]?.EdictbearerSlain ?? false);
            }

            bool applied = tattooSystem?.HasTattoo(tattoo.Id) ?? false;

            var btn = new Button();
            btn.Text = applied ? $"✓ {tattoo.DisplayName}" : locked ? $"🔒 {tattoo.DisplayName}" : tattoo.DisplayName;
            btn.Disabled = locked;

            string id = tattoo.Id;
            btn.Pressed += () => SelectTattoo(id);
            btn.SizeFlagsHorizontal = SizeFlags.ExpandFill;
            _tattooList.AddChild(btn);
        }
    }

    // ─── Tattoo Detail ────────────────────────────────────────────

    private void SelectTattoo(string tattooId)
    {
        _selectedTattoo = Array.Find(_allTattoos, t => t.Id == tattooId);
        if (_selectedTattoo == null) return;

        EmitSignal(SignalName.TattooSelected, tattooId);
        ShowDetail(_selectedTattoo);
    }

    private void ShowDetail(TattooData tattoo)
    {
        if (_detailName != null)
            _detailName.Text = tattoo.DisplayName;
        if (_detailDesc != null)
            _detailDesc.Text = tattoo.Description;
        if (_detailCost != null)
            _detailCost.Text = $"Cost: {tattoo.InkCost} {tattoo.RequiredGrade} Ink";

        // Stat preview.
        var stats = new List<string>();
        if (tattoo.StealthBonus != 0) stats.Add($"Stealth: {tattoo.StealthBonus:+0.##;-0.##}");
        if (tattoo.DamageBonus != 0) stats.Add($"Damage: {tattoo.DamageBonus:+0.##;-0.##}");
        if (tattoo.SpeedBonus != 0) stats.Add($"Speed: {tattoo.SpeedBonus:+0.##;-0.##}");
        if (tattoo.HealthBonus != 0) stats.Add($"Health: {tattoo.HealthBonus:+0.##;-0.##}");
        if (tattoo.ResistanceBonus != 0) stats.Add($"Resistance: {tattoo.ResistanceBonus:+0.##;-0.##}");
        if (tattoo.DetectionRadiusModifier != 0) stats.Add($"Detection: {tattoo.DetectionRadiusModifier:+0.##;-0.##}");
        if (tattoo.GrantsActiveAbility) stats.Add("★ Grants Active Ability");

        if (_detailStats != null)
            _detailStats.Text = stats.Count > 0 ? string.Join("\n", stats) : "Passive effect only.";

        if (_detailWhisper != null)
            _detailWhisper.Text = !string.IsNullOrEmpty(tattoo.WhisperText)
                ? $"[i]\"...{tattoo.WhisperText}\"[/i]"
                : "";

        // Can we apply?
        var gm = Core.GameManager.Instance;
        bool alreadyApplied = gm?.TattooSystem?.HasTattoo(tattoo.Id) ?? false;
        bool canAfford = gm?.InkInventory?.CanAfford(tattoo.RequiredGrade, tattoo.InkCost) ?? false;

        if (_applyButton != null)
        {
            _applyButton.Disabled = alreadyApplied || !canAfford;
            _applyButton.Text = alreadyApplied ? "Already Applied" : canAfford ? "Apply Tattoo" : "Not Enough Ink";
        }
    }

    // ─── Apply ────────────────────────────────────────────────────

    private void OnApplyPressed()
    {
        if (_selectedTattoo == null) return;

        var gm = Core.GameManager.Instance;
        if (gm?.TattooSystem == null || gm.InkInventory == null) return;

        bool success = gm.TattooSystem.ApplyTattoo(_selectedTattoo, gm.InkInventory);
        if (success)
        {
            GD.Print($"Tattoo '{_selectedTattoo.DisplayName}' applied by the Needlewise.");
            RefreshInkDisplay();
            ShowSlot(_currentSlot); // Refresh list to show checkmark.
            ShowDetail(_selectedTattoo); // Refresh detail to show "Already Applied".
        }
    }

    // ─── Ink Display ──────────────────────────────────────────────

    private void RefreshInkDisplay()
    {
        var inv = Core.GameManager.Instance?.InkInventory;
        if (inv == null) return;

        if (_inkMajorLabel != null) _inkMajorLabel.Text = $"Major: {inv.GetInk(InkGrade.Major)}";
        if (_inkLesserLabel != null) _inkLesserLabel.Text = $"Lesser: {inv.GetInk(InkGrade.Lesser)}";
        if (_inkTraceLabel != null) _inkTraceLabel.Text = $"Trace: {inv.GetInk(InkGrade.Trace)}";
    }
}
