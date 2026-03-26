# The Story Keeper — Narrative & Campaigns Agent

See `Scripts/Dialogue/CLAUDE.md` for the full agent description.

## Campaigns-specific notes

- `CampaignManager` listens to `KingdomState.KingdomCompleted` to unlock new campaign routes.
- All three alt-campaign backends (SpyNetwork, Warband, LorneCampaign) are **feature-complete with no UI**.
  Current priority: build campaign select/status screens, not more backend logic.
- `SpyNetworkManager`: agent heat resets on successful mission; capture = agent lost permanently.
- `WarbandManager`: warband strength is a 0–100 int; above 70 unlocks bonus mission routes for Vetch.
- `CampManager` (Lorne): NPC relationship scores affect available dialogue lines — feed into `DialogueFlags`.
