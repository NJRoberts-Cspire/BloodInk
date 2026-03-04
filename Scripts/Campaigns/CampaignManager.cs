using Godot;
using System.Collections.Generic;
using System.Linq;

namespace BloodInk.Campaigns;

/// <summary>
/// Manages campaign unlocks, active campaign state, and completion tracking.
/// Should be an autoload or attached to a persistent node.
/// </summary>
public partial class CampaignManager : Node
{
    [Signal] public delegate void CampaignUnlockedEventHandler(int hand);
    [Signal] public delegate void CampaignStartedEventHandler(int hand);
    [Signal] public delegate void CampaignCompletedEventHandler(int hand);

    /// <summary>Registered campaign definitions.</summary>
    private readonly Dictionary<CampaignHand, CampaignData> _campaigns = new();

    /// <summary>Set of unlocked campaign hands.</summary>
    private readonly HashSet<CampaignHand> _unlockedCampaigns = new() { CampaignHand.Vetch };

    /// <summary>Set of completed campaign hands.</summary>
    private readonly HashSet<CampaignHand> _completedCampaigns = new();

    /// <summary>Currently active campaign hand.</summary>
    public CampaignHand ActiveCampaign { get; private set; } = CampaignHand.Vetch;

    // ─── Registration ─────────────────────────────────────────────

    public void RegisterCampaign(CampaignData data)
    {
        _campaigns[data.Hand] = data;
    }

    // ─── Unlock Logic ─────────────────────────────────────────────

    /// <summary>
    /// Called when Vetch completes a kingdom. Checks if any campaign unlocks.
    /// </summary>
    public void OnKingdomCompleted(int kingdomIndex)
    {
        foreach (var (hand, data) in _campaigns)
        {
            if (hand == CampaignHand.Vetch) continue;
            if (_unlockedCampaigns.Contains(hand)) continue;
            if (data.UnlockAfterKingdom == kingdomIndex)
            {
                _unlockedCampaigns.Add(hand);
                EmitSignal(SignalName.CampaignUnlocked, (int)hand);
                GD.Print($"Campaign unlocked: {data.DisplayName}");
                if (!string.IsNullOrEmpty(data.UnlockLore))
                    GD.Print($"  \"{data.UnlockLore}\"");
            }
        }
    }

    // ─── Query ────────────────────────────────────────────────────

    public bool IsCampaignUnlocked(CampaignHand hand) => _unlockedCampaigns.Contains(hand);
    public bool IsCampaignCompleted(CampaignHand hand) => _completedCampaigns.Contains(hand);
    public CampaignData? GetCampaignData(CampaignHand hand) =>
        _campaigns.TryGetValue(hand, out var d) ? d : null;

    public IEnumerable<CampaignData> GetUnlockedCampaigns() =>
        _unlockedCampaigns.Where(h => _campaigns.ContainsKey(h)).Select(h => _campaigns[h]);

    // ─── Playback ─────────────────────────────────────────────────

    /// <summary>Switch to a campaign. Stores active hand and loads starting scene.</summary>
    public void StartCampaign(CampaignHand hand)
    {
        if (!_unlockedCampaigns.Contains(hand))
        {
            GD.PrintErr($"Campaign {hand} is not unlocked.");
            return;
        }

        var data = _campaigns[hand];
        if (string.IsNullOrEmpty(data.StartScenePath))
        {
            GD.PrintErr($"Campaign {hand} has no start scene.");
            return;
        }

        ActiveCampaign = hand;
        EmitSignal(SignalName.CampaignStarted, (int)hand);
        GD.Print($"Starting campaign: {data.DisplayName}");

        GetTree().ChangeSceneToFile(data.StartScenePath);
    }

    /// <summary>Mark the active campaign as complete.</summary>
    public void CompleteCampaign()
    {
        _completedCampaigns.Add(ActiveCampaign);
        EmitSignal(SignalName.CampaignCompleted, (int)ActiveCampaign);

        var data = _campaigns[ActiveCampaign];
        GD.Print($"Campaign completed: {data.DisplayName}");

        if (data.GrantsUniqueTattoo && !string.IsNullOrEmpty(data.CompletionTattooId))
            GD.Print($"  Unique tattoo earned: {data.CompletionTattooId}");
    }

    /// <summary>Return to Vetch's main campaign.</summary>
    public void ReturnToMainCampaign()
    {
        if (_campaigns.TryGetValue(CampaignHand.Vetch, out var vetch))
        {
            ActiveCampaign = CampaignHand.Vetch;
            // Caller is responsible for scene transition.
        }
    }

    // ─── Serialization ────────────────────────────────────────────

    public Dictionary<string, object> Serialize()
    {
        return new Dictionary<string, object>
        {
            ["active"] = (int)ActiveCampaign,
            ["unlocked"] = _unlockedCampaigns.Select(h => (int)h).ToList(),
            ["completed"] = _completedCampaigns.Select(h => (int)h).ToList()
        };
    }

    public void Deserialize(Dictionary<string, object> data)
    {
        if (data.TryGetValue("active", out var active) && active is int a)
            ActiveCampaign = (CampaignHand)a;

        if (data.TryGetValue("unlocked", out var unlocked) && unlocked is List<int> uList)
            foreach (var h in uList) _unlockedCampaigns.Add((CampaignHand)h);

        if (data.TryGetValue("completed", out var completed) && completed is List<int> cList)
            foreach (var h in cList) _completedCampaigns.Add((CampaignHand)h);
    }
}
