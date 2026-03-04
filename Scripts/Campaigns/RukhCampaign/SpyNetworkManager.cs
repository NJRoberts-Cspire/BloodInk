using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BloodInk.Campaigns.Rukh;

/// <summary>
/// Mission types an agent can be assigned.
/// </summary>
public enum MissionType
{
    GatherIntel,
    MarkTarget,
    CreateDiversion,
    SabotageDefences,
    EscortAsset,
    Extraction
}

/// <summary>
/// Manages Rukh's spy network: agents, missions, intel gathering.
/// Rukh's campaign plays as a strategic layer where you assign agents
/// to gather intel that benefits Vetch's infiltrations.
/// </summary>
public partial class SpyNetworkManager : Node
{
    [Signal] public delegate void AgentRecruitedEventHandler(string agentId);
    [Signal] public delegate void AgentCompromisedEventHandler(string agentId);
    [Signal] public delegate void AgentLostEventHandler(string agentId);
    [Signal] public delegate void IntelGatheredEventHandler(string intelId);
    [Signal] public delegate void MissionCompleteEventHandler(string agentId, int missionType);

    /// <summary>All agents in the network.</summary>
    private readonly Dictionary<string, AgentData> _agents = new();

    /// <summary>All gathered intel.</summary>
    private readonly Dictionary<string, IntelData> _intel = new();

    /// <summary>Pending missions: agentId → mission type.</summary>
    private readonly Dictionary<string, MissionType> _activeMissions = new();

    /// <summary>Turns remaining on active missions.</summary>
    private readonly Dictionary<string, int> _missionTimers = new();

    /// <summary>Rukh's network heat per kingdom. Higher = agents more likely to be caught.</summary>
    private readonly Dictionary<int, float> _kingdomHeat = new();

    private readonly Random _rng = new();

    // ─── Agent Management ─────────────────────────────────────────

    public void RecruitAgent(AgentData agent)
    {
        _agents[agent.Id] = agent;
        EmitSignal(SignalName.AgentRecruited, agent.Id);
        GD.Print($"Spy recruited: {agent.CodeName} (Cover: {agent.CoverRole}, Kingdom {agent.KingdomIndex})");
    }

    public AgentData? GetAgent(string id) =>
        _agents.TryGetValue(id, out var a) ? a : null;

    public IEnumerable<AgentData> GetAgentsInKingdom(int kingdomIndex) =>
        _agents.Values.Where(a => a.KingdomIndex == kingdomIndex && !a.IsCompromised);

    public IEnumerable<AgentData> GetAvailableAgents() =>
        _agents.Values.Where(a => !a.IsCompromised && !a.IsOnMission);

    // ─── Mission Assignment ───────────────────────────────────────

    /// <summary>
    /// Assign an agent to a mission. Returns false if agent is unavailable.
    /// Mission duration is based on type and agent skill.
    /// </summary>
    public bool AssignMission(string agentId, MissionType mission)
    {
        if (!_agents.TryGetValue(agentId, out var agent)) return false;
        if (agent.IsCompromised || agent.IsOnMission) return false;

        agent.IsOnMission = true;
        _activeMissions[agentId] = mission;

        // Higher skill = faster completion. Base 3 turns, -1 per 30 skill points.
        int baseTurns = mission switch
        {
            MissionType.GatherIntel => 2,
            MissionType.MarkTarget => 3,
            MissionType.CreateDiversion => 2,
            MissionType.SabotageDefences => 4,
            MissionType.EscortAsset => 3,
            MissionType.Extraction => 1,
            _ => 3
        };
        int skillBonus = agent.SkillLevel / 40; // 0-2 turns reduced
        _missionTimers[agentId] = Math.Max(1, baseTurns - skillBonus);

        GD.Print($"Agent {agent.CodeName} assigned to {mission} ({_missionTimers[agentId]} turns)");
        return true;
    }

    // ─── Turn Processing ──────────────────────────────────────────

    /// <summary>
    /// Advance one "turn" in the spy network. Call this when Vetch completes
    /// a mission or moves between kingdoms.
    /// </summary>
    public void AdvanceTurn()
    {
        var completedAgents = new List<string>();

        foreach (var (agentId, turnsLeft) in _missionTimers.ToList())
        {
            _missionTimers[agentId] = turnsLeft - 1;
            if (_missionTimers[agentId] <= 0)
                completedAgents.Add(agentId);
        }

        foreach (var agentId in completedAgents)
            ResolveMission(agentId);

        // Decay heat naturally each turn.
        foreach (var key in _kingdomHeat.Keys.ToList())
            _kingdomHeat[key] = Math.Max(0f, _kingdomHeat[key] - 5f);
    }

    private void ResolveMission(string agentId)
    {
        if (!_activeMissions.TryGetValue(agentId, out var mission)) return;
        var agent = _agents[agentId];

        // Success chance: skill + loyalty vs kingdom heat.
        float heat = GetKingdomHeat(agent.KingdomIndex);
        float successChance = (agent.SkillLevel + agent.Loyalty) / 200f - heat / 200f;
        successChance = Math.Clamp(successChance, 0.1f, 0.95f);

        bool success = _rng.NextDouble() < successChance;

        if (success)
        {
            OnMissionSuccess(agent, mission);
        }
        else
        {
            OnMissionFailure(agent, mission);
        }

        agent.IsOnMission = false;
        _activeMissions.Remove(agentId);
        _missionTimers.Remove(agentId);

        EmitSignal(SignalName.MissionComplete, agentId, (int)mission);
    }

    private void OnMissionSuccess(AgentData agent, MissionType mission)
    {
        GD.Print($"Mission SUCCESS: {agent.CodeName} completed {mission}");

        switch (mission)
        {
            case MissionType.GatherIntel:
                var intel = GenerateIntel(agent);
                _intel[intel.Id] = intel;
                EmitSignal(SignalName.IntelGathered, intel.Id);
                break;

            case MissionType.MarkTarget:
                GD.Print($"  Target marked in kingdom {agent.KingdomIndex}");
                break;

            case MissionType.CreateDiversion:
                GD.Print($"  Diversion created — guard alert temporarily lowered");
                ModifyKingdomHeat(agent.KingdomIndex, -15f);
                break;

            case MissionType.SabotageDefences:
                GD.Print($"  Defences sabotaged in kingdom {agent.KingdomIndex}");
                break;

            case MissionType.Extraction:
                GD.Print($"  Asset extracted from kingdom {agent.KingdomIndex}");
                break;
        }

        // Skill improves on success.
        agent.SkillLevel = Math.Min(100, agent.SkillLevel + 5);
    }

    private void OnMissionFailure(AgentData agent, MissionType mission)
    {
        GD.Print($"Mission FAILED: {agent.CodeName} failed {mission}");

        // Chance of being compromised on failure.
        float compromiseChance = 0.3f + GetKingdomHeat(agent.KingdomIndex) / 200f;
        if (_rng.NextDouble() < compromiseChance)
        {
            agent.IsCompromised = true;
            EmitSignal(SignalName.AgentCompromised, agent.Id);
            GD.Print($"  AGENT COMPROMISED: {agent.CodeName}'s cover is blown!");

            // Raise heat in the kingdom.
            ModifyKingdomHeat(agent.KingdomIndex, 20f);
        }

        // Loyalty drops on failure.
        agent.Loyalty = Math.Max(0, agent.Loyalty - 10);

        // Very low loyalty = agent disappears (defects or captured).
        if (agent.Loyalty <= 10)
        {
            agent.IsCompromised = true;
            EmitSignal(SignalName.AgentLost, agent.Id);
            GD.Print($"  AGENT LOST: {agent.CodeName} has vanished.");
        }
    }

    // ─── Intel Generation ─────────────────────────────────────────

    private IntelData GenerateIntel(AgentData agent)
    {
        var types = Enum.GetValues<IntelType>();
        var type = types[_rng.Next(types.Length)];

        var intel = new IntelData
        {
            Id = $"intel_{agent.KingdomIndex}_{_intel.Count}",
            Type = type,
            KingdomIndex = agent.KingdomIndex,
            Summary = $"{type} gathered by {agent.CodeName}",
            FullText = $"Intelligence report from agent {agent.CodeName} regarding {type} in kingdom {agent.KingdomIndex}.",
            IsVerified = agent.SkillLevel >= 60,
            EffectKey = $"{type.ToString().ToLower()}_k{agent.KingdomIndex}",
            SourceAgentId = agent.Id
        };

        GD.Print($"  Intel gathered: {intel.Summary} (Verified: {intel.IsVerified})");
        return intel;
    }

    // ─── Kingdom Heat ─────────────────────────────────────────────

    public float GetKingdomHeat(int kingdomIndex) =>
        _kingdomHeat.TryGetValue(kingdomIndex, out var h) ? h : 0f;

    public void ModifyKingdomHeat(int kingdomIndex, float delta)
    {
        if (!_kingdomHeat.ContainsKey(kingdomIndex))
            _kingdomHeat[kingdomIndex] = 0f;
        _kingdomHeat[kingdomIndex] = Math.Clamp(_kingdomHeat[kingdomIndex] + delta, 0f, 100f);
    }

    // ─── Query Intel ──────────────────────────────────────────────

    public IEnumerable<IntelData> GetIntelForKingdom(int kingdomIndex) =>
        _intel.Values.Where(i => i.KingdomIndex == kingdomIndex);

    public IEnumerable<IntelData> GetVerifiedIntel() =>
        _intel.Values.Where(i => i.IsVerified);

    public bool HasIntelOfType(int kingdomIndex, IntelType type) =>
        _intel.Values.Any(i => i.KingdomIndex == kingdomIndex && i.Type == type);

    // ─── Serialization ────────────────────────────────────────────

    public Dictionary<string, object> Serialize()
    {
        var agentStates = new Dictionary<string, Dictionary<string, object>>();
        foreach (var (id, agent) in _agents)
        {
            agentStates[id] = new Dictionary<string, object>
            {
                ["skill"] = agent.SkillLevel,
                ["loyalty"] = agent.Loyalty,
                ["compromised"] = agent.IsCompromised,
                ["onMission"] = agent.IsOnMission,
                ["kingdom"] = agent.KingdomIndex
            };
        }

        return new Dictionary<string, object>
        {
            ["agents"] = agentStates,
            ["intelCount"] = _intel.Count,
            ["heat"] = new Dictionary<int, float>(_kingdomHeat)
        };
    }
}
