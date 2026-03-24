using Godot;
using System.Text;
using BloodInk.Campaigns.Grael;
using BloodInk.Core;

namespace BloodInk.UI;

/// <summary>
/// Grael's Warband Panel — shows recruited warriors, their stats, morale,
/// renown, and allows training sessions using war supplies.
/// Opened by the "open_warband" dialogue event from Grael.
/// </summary>
public partial class WarbandPanel : Control
{
    [Signal] public delegate void PanelClosedEventHandler();

    private VBoxContainer? _warriorList;
    private Panel? _detailPanel;
    private Label? _detailName;
    private RichTextLabel? _detailInfo;
    private VBoxContainer? _trainButtons;
    private RichTextLabel? _statusLabel;
    private Label? _renownLabel;

    private WarriorData? _selectedWarrior;

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
        var warband = gm?.Warband;

        if (_renownLabel != null)
        {
            if (warband != null)
                _renownLabel.Text = $"Renown: {warband.Renown}  |  Size: {warband.LivingCount}/{warband.MaxWarbandSize}  |  Morale: {warband.AverageMorale:F0}/100  |  Supplies: {warband.WarSupplies}";
            else
                _renownLabel.Text = "Warband unavailable";
        }

        if (_warriorList == null) return;
        foreach (var child in _warriorList.GetChildren()) child.QueueFree();

        if (warband == null)
        {
            _warriorList.AddChild(new Label { Text = "(Warband unavailable)" });
            return;
        }

        bool anyWarrior = false;
        foreach (var warrior in warband.GetLivingWarriors())
        {
            anyWarrior = true;
            var btn = new Button
            {
                Text = $"{warrior.Name} [{warrior.Role}]  M:{warrior.Morale}"
            };
            btn.SizeFlagsHorizontal = SizeFlags.ExpandFill;
            WarriorData captured = warrior;
            btn.Pressed += () => SelectWarrior(captured);
            _warriorList.AddChild(btn);
        }

        if (!anyWarrior)
        {
            _warriorList.AddChild(new Label { Text = "(No warriors recruited)" });
            _warriorList.AddChild(new Label { Text = "Talk to Grael after missions" });
            _warriorList.AddChild(new Label { Text = "to unlock warrior recruitment." });
        }
    }

    private void SelectWarrior(WarriorData warrior)
    {
        _selectedWarrior = warrior;
        if (_detailPanel != null) _detailPanel.Visible = true;
        if (_detailName != null)
            _detailName.Text = $"{warrior.Name}  — {warrior.Role}";

        if (_detailInfo != null)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"Strength:  {warrior.Strength}/100");
            sb.AppendLine($"Endurance: {warrior.Endurance}/100");
            sb.AppendLine($"Morale:    {warrior.Morale}/100");
            sb.AppendLine($"Raids survived: {warrior.RaidsSurvived}");
            if (!string.IsNullOrEmpty(warrior.Lore))
            {
                sb.AppendLine();
                sb.AppendLine(warrior.Lore);
            }
            _detailInfo.Text = sb.ToString();
        }

        if (_trainButtons == null) return;
        foreach (var child in _trainButtons.GetChildren()) child.QueueFree();

        var gm = GameManager.Instance;
        if (gm?.Warband == null) return;

        _trainButtons.AddChild(new Label { Text = "Train (costs 10 supplies):" });

        foreach (var stat in new[] { "strength", "endurance", "morale" })
        {
            var btn = new Button
            {
                Text = $"Train {stat}",
                Disabled = gm.Warband.WarSupplies < 10
            };
            btn.SizeFlagsHorizontal = SizeFlags.ExpandFill;
            string capturedStat = stat;
            btn.Pressed += () => TrainWarrior(warrior, capturedStat);
            _trainButtons.AddChild(btn);
        }
    }

    private void TrainWarrior(WarriorData warrior, string stat)
    {
        var warband = GameManager.Instance?.Warband;
        if (warband == null) return;

        bool ok = warband.TrainWarrior(warrior.Id, stat);

        if (_statusLabel != null)
            _statusLabel.Text = ok
                ? $"[color=green]{warrior.Name} trained in {stat}.[/color]"
                : $"[color=red]Not enough supplies (need 10).[/color]";

        Refresh();
        SelectWarrior(warrior);
    }

    private void BuildUI()
    {
        SetAnchorsPreset(LayoutPreset.FullRect);

        var bg = new ColorRect { Color = new Color(0.06f, 0.04f, 0.03f, 0.92f) };
        bg.SetAnchorsPreset(LayoutPreset.FullRect);
        AddChild(bg);

        var title = new Label { Text = "Grael's Warband", Position = new Vector2(10, 6), Size = new Vector2(500, 24) };
        AddChild(title);

        _renownLabel = new Label { Position = new Vector2(200, 6), Size = new Vector2(900, 24) };
        AddChild(_renownLabel);

        var closeBtn = new Button { Text = "Close [E]", Position = new Vector2(1160, 4), Size = new Vector2(108, 28) };
        closeBtn.Pressed += Close;
        AddChild(closeBtn);

        var sep = new HSeparator { Position = new Vector2(0, 36), Size = new Vector2(1280, 4) };
        AddChild(sep);

        var warriorTitle = new Label { Text = "Warriors:", Position = new Vector2(10, 44), Size = new Vector2(380, 24) };
        AddChild(warriorTitle);

        var scroll = new ScrollContainer { Position = new Vector2(10, 72), Size = new Vector2(380, 608) };
        _warriorList = new VBoxContainer();
        _warriorList.SizeFlagsHorizontal = SizeFlags.ExpandFill;
        scroll.AddChild(_warriorList);
        AddChild(scroll);

        _detailPanel = new Panel { Position = new Vector2(410, 44), Size = new Vector2(858, 636), Visible = false };
        AddChild(_detailPanel);

        _detailName = new Label { Position = new Vector2(8, 8), Size = new Vector2(840, 28) };
        _detailPanel.AddChild(_detailName);

        _detailInfo = new RichTextLabel { Position = new Vector2(8, 44), Size = new Vector2(840, 220), BbcodeEnabled = false };
        _detailPanel.AddChild(_detailInfo);

        var trainTitle = new Label { Text = "Training:", Position = new Vector2(8, 276), Size = new Vector2(840, 24) };
        _detailPanel.AddChild(trainTitle);

        _trainButtons = new VBoxContainer { Position = new Vector2(8, 304), Size = new Vector2(440, 180) };
        _detailPanel.AddChild(_trainButtons);

        _statusLabel = new RichTextLabel { Position = new Vector2(8, 500), Size = new Vector2(840, 80), BbcodeEnabled = true };
        _detailPanel.AddChild(_statusLabel);
    }
}
