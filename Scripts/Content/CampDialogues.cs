using BloodInk.Dialogue;

namespace BloodInk.Content;

/// <summary>
/// All camp NPC dialogue for Act 1 (pre-first-mission briefing).
/// Built in code until .tres dialogue editor exists.
/// </summary>
public static class CampDialogues
{
    // ═════════════════════════════════════════════════════════════
    //  NEEDLEWISE — The Tattoo Shaman (secret creator of the Edict)
    // ═════════════════════════════════════════════════════════════

    public static DialogueData NeedlewiseAct1() => new()
    {
        ConversationId = "needlewise_act1",
        Lines = new DialogueLine[]
        {
            new() { Id = "1", Speaker = "Needlewise", Text = "Ah, Vetch. Sit. Let me look at those hands.", IsEntry = true, NextLineId = "2" },
            new() { Id = "2", Speaker = "Needlewise", Text = "Steady enough. Good. The ink doesn't forgive trembling.", NextLineId = "3" },
            new() { Id = "3", Speaker = "Needlewise", Text = "The first mark will be Cowl's. Governor of the Greenhold. His blood carries the oldest anchor.", NextLineId = "4" },
            new() { Id = "4", Speaker = "Needlewise", Text = "Kill him, bring me the blood, and I'll give you something worth the pain.", NextLineId = "5" },
            new() { Id = "5", Speaker = "Vetch", Text = "...", Choices = new[]
            {
                "What will the tattoo do?|6a",
                "How do I find him?|6b",
                "I'm ready.|6c"
            }},
            new() { Id = "6a", Speaker = "Needlewise", Text = "It will let you step through shadows. The curse turned against itself — a controlled infection.", NextLineId = "6a2" },
            new() { Id = "6a2", Speaker = "Needlewise", Text = "It will hurt. Everything worth having does.", NextLineId = "7" },
            new() { Id = "6b", Speaker = "Needlewise", Text = "Goldmanor. A sprawling estate in the Greenhold's heart. Rukh has the details. Talk to him.", NextLineId = "7" },
            new() { Id = "6c", Speaker = "Needlewise", Text = "Patience. The Hollow Hand strike when ready, not when eager.", NextLineId = "7" },
            new() { Id = "7", Speaker = "Needlewise", Text = "When you return... the ink will be waiting. Now go prepare.", SetFlag = "needlewise_briefed" },
        }
    };

    public static DialogueData NeedlewisePostMission() => new()
    {
        ConversationId = "needlewise_post_cowl",
        Lines = new DialogueLine[]
        {
            new() { Id = "1", Speaker = "Needlewise", Text = "You have it. I can smell it — thick, old blood. Edictbearer blood.", IsEntry = true, NextLineId = "2" },
            new() { Id = "2", Speaker = "Needlewise", Text = "Sit still. This will feel... incorrect.", NextLineId = "3" },
            new() { Id = "3", Speaker = "", Text = "The needle bites. The ink burns cold. You see flashes — a garden, sunlight, a man who never looked down.", NextLineId = "4" },
            new() { Id = "4", Speaker = "Needlewise", Text = "Shadow Step. You can walk between darknesses now.", NextLineId = "5" },
            new() { Id = "5", Speaker = "Needlewise", Text = "One anchor broken. Five remain. The Edict weakens... but it won't die easy.", SetFlag = "tattoo_shadow_step_applied", Event = "apply_tattoo:shadow_step" },
        }
    };

    // ═════════════════════════════════════════════════════════════
    //  RUKH — The Handler / Intelligence
    // ═════════════════════════════════════════════════════════════

    public static DialogueData RukhAct1() => new()
    {
        ConversationId = "rukh_act1",
        Lines = new DialogueLine[]
        {
            new() { Id = "1", Speaker = "Rukh", Text = "Vetch. Good — I was about to send for you.", IsEntry = true, NextLineId = "2" },
            new() { Id = "2", Speaker = "Rukh", Text = "Lord Harlan Cowl. Governor of the Greenhold. Sits in Goldmanor like a fat spider.", NextLineId = "3" },
            new() { Id = "3", Speaker = "Rukh", Text = "My people inside say he takes an evening walk on the west balcony. Alone. Every night.", NextLineId = "4" },
            new() { Id = "4", Speaker = "Rukh", Text = "The manor gardens have shadow cover. The servant entrance is usually unwatched after sundown.", NextLineId = "5" },
            new() { Id = "5", Speaker = "Vetch", Text = "...", Choices = new[]
            {
                "What about guards?|6a",
                "Any other way in?|6b",
                "Good enough. I'll manage.|6c"
            }},
            new() { Id = "6a", Speaker = "Rukh", Text = "Greenguard. Well-drilled, but predictable. They patrol in fixed routes. Watch, learn, then move.", NextLineId = "7" },
            new() { Id = "6b", Speaker = "Rukh", Text = "Wine cellar. There's a delivery entrance on the east side. Less guarded, but the hallway is narrow.", NextLineId = "7" },
            new() { Id = "6c", Speaker = "Rukh", Text = "That's the Vetch I remember. Just... come back alive. We can't afford to lose another.", NextLineId = "7" },
            new() { Id = "7", Speaker = "Rukh", Text = "When you're ready, check the Mission Board. I'll have the routes marked.", SetFlag = "rukh_briefed" },
        }
    };

    // ═════════════════════════════════════════════════════════════
    //  GRAEL — The Warrior (growing hostility)
    // ═════════════════════════════════════════════════════════════

    public static DialogueData GraelAct1() => new()
    {
        ConversationId = "grael_act1",
        Lines = new DialogueLine[]
        {
            new() { Id = "1", Speaker = "Grael", Text = "*sharpening a blade* Another night of skulking, is it?", IsEntry = true, NextLineId = "2" },
            new() { Id = "2", Speaker = "Grael", Text = "My Irontusk would have stormed Goldmanor by dawn. Fifty warriors. No survivors.", NextLineId = "3" },
            new() { Id = "3", Speaker = "Grael", Text = "Instead, we send one half-dead orc with a knife. The Hollow Hand way.", NextLineId = "4" },
            new() { Id = "4", Speaker = "Vetch", Text = "...", Choices = new[]
            {
                "Your way would get us all killed.|5a",
                "We don't have fifty warriors.|5b",
                "*Say nothing.*|5c"
            }},
            new() { Id = "5a", Speaker = "Grael", Text = "*grunt* Better to die fighting than to die... quietly. Like cowards.", NextLineId = "6" },
            new() { Id = "5b", Speaker = "Grael", Text = "No. We don't. Because they've been dying for a century while your Hand did nothing.", NextLineId = "6" },
            new() { Id = "5c", Speaker = "Grael", Text = "*long stare* ...Fine. Go play shadow. But when the real war comes, you'd better be ready to bleed for it.", NextLineId = "6" },
            new() { Id = "6", Speaker = "Grael", Text = "Bring me Cowl's sword. If you find one. I want to see what kind of steel they arm these humans with." },
        }
    };

    // ═════════════════════════════════════════════════════════════
    //  SENNA — The Heart (emotional support, orc history)
    // ═════════════════════════════════════════════════════════════

    public static DialogueData SennaAct1() => new()
    {
        ConversationId = "senna_act1",
        Lines = new DialogueLine[]
        {
            new() { Id = "1", Speaker = "Senna", Text = "Vetch. Come sit by the fire a moment.", IsEntry = true, NextLineId = "2" },
            new() { Id = "2", Speaker = "Senna", Text = "I've been teaching the young ones the old counting songs. They don't know them anymore.", NextLineId = "3" },
            new() { Id = "3", Speaker = "Senna", Text = "Three generations and we've forgotten how to count past ten in our own tongue.", NextLineId = "4" },
            new() { Id = "4", Speaker = "Senna", Text = "When you go to Goldmanor... remember what you're fighting for. Not just survival.", NextLineId = "5" },
            new() { Id = "5", Speaker = "Vetch", Text = "...", Choices = new[]
            {
                "What am I fighting for, then?|6a",
                "I remember.|6b",
                "I just need to kill one man.|6c"
            }},
            new() { Id = "6a", Speaker = "Senna", Text = "Memory. A future where an orc child can grow tall enough for proper tusks. That's worth one lord's blood.", NextLineId = "7" },
            new() { Id = "6b", Speaker = "Senna", Text = "*soft smile* Good. Hold onto that. The ink changes things, Vetch. Don't let it change that.", NextLineId = "7" },
            new() { Id = "6c", Speaker = "Senna", Text = "*quiet* It's never just one man. It's what he carries. What he chose to keep carrying.", NextLineId = "7" },
            new() { Id = "7", Speaker = "Senna", Text = "Come back safe. The fire will be here." },
        }
    };
}
