using Godot;
using System.Text;
using BloodInk.Campaigns.Rukh;
using BloodInk.Core;

namespace BloodInk.UI;

/// <summary>
/// Rukh's Spy Network Panel — shows active agents, their missions, intel gathered,
/// and allows assigning available agents to new missions.
/// Opened by the "open_spy_network" dialogue event from Rukh.
/// </summary>
public partial class SpyNetworkPanel : Control
{
    [Signal] public delegate void PanelClosedEventHandler();

    private VBoxContainer? _agentList;
    private Panel? _detailPanel;
    private Label? _detailName;
    private RichTextLabel? _detailInfo;
    private VBoxContainer? _missionButtons;
    private RichTextLabel? _statusLabel;
    private RichTextLabel? _heatLabel;
    private RichTextLabel? _intelLabel;

    private AgentData? _selectedAgent;

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
        var network = gm?.SpyNetwork;

        if (_heatLabel != null)
        {
            if (network == null)
            {
                _heatLabel.Text = "[color=gray]Heat: —[/color]";
            }
            else
            {
                var heatParts = new System.Text.StringBuilder();
                heatParts.Append("[b]Kingdom Heat:[/b]  ");
                for (int k = 0; k < 6; k++)
                {
                    float h = network.GetKingdomHeat(k);
                    string col = h >= 70 ? "red" : h >= 40 ? "yellow" : "green";
                    heatParts.Append($"[color={col}]K{k}:{h:F0}[/color]  ");
                }
                _heatLabel.Text = heatParts.ToString();
            }
        }

        // ── Gathered Intel summary ────────────────────────────────────────
        if (_intelLabel != null)
        {
            if (network == null)
            {
                _intelLabel.Text = "";
            }
            else
            {
                var verified = new System.Collections.Generic.List<Campaigns.Rukh.IntelData>(network.GetVerifiedIntel());
                if (verified.Count == 0)
                {
                    _intelLabel.Text = "[color=gray](No verified intel yet)[/color]";
                }
                else
                {
                    var sb = new System.Text.StringBuilder();
                    foreach (var intel in verified)
                        sb.AppendLine($"[color=cyan]★[/color] {intel.Summary}");
                    _intelLabel.Text = sb.ToString();
                }
            }
        }

        if (_agentList == null) return;
        foreach (var child in _agentList.GetChildren()) child.QueueFree();

        if (network == null)
        {
            _agentList.AddChild(new Label { Text = "(Spy network unavailable)" });
            return;
        }

        bool anyAgent = false;
        foreach (var agent in network.GetAllAgents())
        {
            anyAgent = true;
            string status = agent.IsCompromised ? " [BLOWN]" :
                            agent.IsOnMission   ? " [ON MISSION]" : " [AVAILABLE]";
            var btn = new Button
            {
                Text = $"{agent.CodeName}{status}",
                Disabled = agent.IsCompromised
            };
            btn.SizeFlagsHorizontal = SizeFlags.ExpandFill;
            AgentData captured = agent;
            btn.Pressed += () => SelectAgent(captured);
            _agentList.AddChild(btn);
        }

        if (!anyAgent)
        {
            _agentList.AddChild(new Label { Text = "(No agents recruited)" });
            _agentList.AddChild(new Label { Text = "Talk to Rukh after missions" });
            _agentList.AddChild(new Label { Text = "to unlock agent recruitment." });
        }
    }

    private void SelectAgent(AgentData agent)
    {
        _selectedAgent = agent;
        if (_detailPanel != null) _detailPanel.Visible = true;
        if (_detailName != null)
            _detailName.Text = $"{agent.CodeName}  (Kingdom {agent.KingdomIndex})";

        if (_detailInfo != null)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"Cover: {agent.CoverRole}");
            sb.AppendLine($"Skill: {agent.SkillLevel}/100   Loyalty: {agent.Loyalty}/100");
            sb.AppendLine($"Status: {(agent.IsCompromised ? "COMPROMISED" : agent.IsOnMission ? "On Mission" : "Available")}");
            if (!string.IsNullOrEmpty(agent.BackgroundLore))
            {
                sb.AppendLine();
                sb.AppendLine(agent.BackgroundLore);
            }
            _detailInfo.Text = sb.ToString();
        }

        if (_missionButtons == null) return;
        foreach (var child in _missionButtons.GetChildren()) child.QueueFree();

        if (agent.IsCompromised || agent.IsOnMission)
        {
            _missionButtons.AddChild(new Label
            {
                Text = agent.IsCompromised
                    ? "Agent blown — cannot assign missions."
                    : "Agent already on a mission."
            });
            return;
        }

        var missionLabel = new Label { Text = "Assign Mission:" };
        _missionButtons.AddChild(missionLabel);

        foreach (MissionType mType in System.Enum.GetValues<MissionType>())
        {
            var btn = new Button { Text = mType.ToString() };
            btn.SizeFlagsHorizontal = SizeFlags.ExpandFill;
            MissionType captured = mType;
            btn.Pressed += () => AssignMission(agent, captured);
            _missionButtons.AddChild(btn);
        }
    }

    private void AssignMission(AgentData agent, MissionType mission)
    {
        var network = GameManager.Instance?.SpyNetwork;
        if (network == null) return;

        bool ok = network.AssignMission(agent.Id, mission);

        if (_statusLabel != null)
            _statusLabel.Text = ok
                ? $"[color=green]{agent.CodeName} assigned: {mission}.[/color]"
                : $"[color=red]Could not assign — agent unavailable.[/color]";

        Refresh();
        if (_detailPanel != null) _detailPanel.Visible = false;
    }

    private void BuildUI()
    {
        SetAnchorsPreset(LayoutPreset.FullRect);

        var bg = new ColorRect { Color = new Color(0.04f, 0.04f, 0.07f, 0.92f) };
        bg.SetAnchorsPreset(LayoutPreset.FullRect);
        AddChild(bg);

        var title = new Label { Text = "Rukh's Spy Network", Position = new Vector2(10, 6), Size = new Vector2(500, 24) };
        AddChild(title);

        _heatLabel = new RichTextLabel
        {
            BbcodeEnabled = true,
            Position = new Vector2(500, 4),
            Size = new Vector2(640, 28),
            ScrollActive = false,
            FitContent = true,
        };
        AddChild(_heatLabel);

        var closeBtn = new Button { Text = "Close [E]", Position = new Vector2(1160, 4), Size = new Vector2(108, 28) };
        closeBtn.Pressed += Close;
        AddChild(closeBtn);

        var sep = new HSeparator { Position = new Vector2(0, 36), Size = new Vector2(1280, 4) };
        AddChild(sep);

        var agentTitle = new Label { Text = "Agents:", Position = new Vector2(10, 44), Size = new Vector2(380, 24) };
        AddChild(agentTitle);

        var scroll = new ScrollContainer { Position = new Vector2(10, 72), Size = new Vector2(380, 420) };
        _agentList = new VBoxContainer();
        _agentList.SizeFlagsHorizontal = SizeFlags.ExpandFill;
        scroll.AddChild(_agentList);
        AddChild(scroll);

        var intelTitle = new Label { Text = "Verified Intel:", Position = new Vector2(10, 500), Size = new Vector2(380, 24) };
        AddChild(intelTitle);

        var intelScroll = new ScrollContainer { Position = new Vector2(10, 524), Size = new Vector2(380, 156) };
        _intelLabel = new RichTextLabel
        {
            BbcodeEnabled = true,
            SizeFlagsHorizontal = SizeFlags.ExpandFill,
            SizeFlagsVertical = SizeFlags.ExpandFill,
            FitContent = false,
        };
        intelScroll.AddChild(_intelLabel);
        AddChild(intelScroll);

        _detailPanel = new Panel { Position = new Vector2(410, 44), Size = new Vector2(858, 636), Visible = false };
        AddChild(_detailPanel);

        _detailName = new Label { Position = new Vector2(8, 8), Size = new Vector2(840, 28) };
        _detailPanel.AddChild(_detailName);

        _detailInfo = new RichTextLabel { Position = new Vector2(8, 44), Size = new Vector2(840, 200), BbcodeEnabled = false };
        _detailPanel.AddChild(_detailInfo);

        var missionTitle = new Label { Text = "Missions:", Position = new Vector2(8, 256), Size = new Vector2(840, 24) };
        _detailPanel.AddChild(missionTitle);

        var missionScroll = new ScrollContainer { Position = new Vector2(8, 284), Size = new Vector2(840, 240) };
        _missionButtons = new VBoxContainer();
        _missionButtons.SizeFlagsHorizontal = SizeFlags.ExpandFill;
        missionScroll.AddChild(_missionButtons);
        _detailPanel.AddChild(missionScroll);

        _statusLabel = new RichTextLabel { Position = new Vector2(8, 536), Size = new Vector2(840, 60), BbcodeEnabled = true };
        _detailPanel.AddChild(_statusLabel);
    }
}
