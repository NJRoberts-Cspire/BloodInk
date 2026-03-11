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

    // ═════════════════════════════════════════════════════════════
    //  POST-MISSION VARIANTS (react to individual target kills)
    // ═════════════════════════════════════════════════════════════

    /// <summary>Needlewise reacts to ANY non-Cowl target being killed (generic progress).</summary>
    public static DialogueData NeedlewiseProgress() => new()
    {
        ConversationId = "needlewise_progress",
        Lines = new DialogueLine[]
        {
            new() { Id = "1", Speaker = "Needlewise", Text = "You carry blood on you. I can smell it even in the rain.", IsEntry = true, NextLineId = "2" },
            new() { Id = "2", Speaker = "Needlewise", Text = "Lesser ink, but useful. Every drop weakens the Edict's grip on its anchors.", NextLineId = "3" },
            new() { Id = "3", Speaker = "Needlewise", Text = "Bring me more. Or better — bring me an Edictbearer. That's where the real power sleeps.", NextLineId = "4" },
            new() { Id = "4", Speaker = "Needlewise", Text = "Cowl is the Greenhold's anchor. Kill him and the whole kingdom's Edict frays." },
        }
    };

    /// <summary>Rukh reacts to non-Cowl kills. Mentions specific targets if dead.</summary>
    public static DialogueData RukhProgress(bool thorneDead, bool marenDead, bool blessingDead) => new()
    {
        ConversationId = "rukh_progress",
        Lines = new DialogueLine[]
        {
            new()
            {
                Id = "1", Speaker = "Rukh", IsEntry = true, NextLineId = "2",
                Text = thorneDead
                    ? "Thorne's dead. Good — his patrols were the biggest threat to our routes."
                    : marenDead
                        ? "Maren's labor camp is disrupted. The orcs there may find their way to us."
                        : blessingDead
                            ? "The Chapel's quiet now. Blessing won't be 'purifying' anyone else."
                            : "Word is spreading. The Greenhold knows something is hunting."
            },
            new() { Id = "2", Speaker = "Rukh", Text = "The board has updated intel. More targets, more opportunities.", NextLineId = "3" },
            new() { Id = "3", Speaker = "Rukh", Text = "But don't lose sight of the prize — Cowl is the Edictbearer. Everything else is preparation." },
        }
    };

    /// <summary>Rukh after Cowl is killed.</summary>
    public static DialogueData RukhPostCowl() => new()
    {
        ConversationId = "rukh_post_cowl",
        Lines = new DialogueLine[]
        {
            new() { Id = "1", Speaker = "Rukh", Text = "*quiet satisfaction* The Greenhold has no governor.", IsEntry = true, NextLineId = "2" },
            new() { Id = "2", Speaker = "Rukh", Text = "My contacts say the human garrison is in disarray. They're fighting over who gives orders now.", NextLineId = "3" },
            new() { Id = "3", Speaker = "Rukh", Text = "There are still targets in the Greenhold if you want to press the advantage.", NextLineId = "4" },
            new() { Id = "4", Speaker = "Rukh", Text = "Or... the Needlewise may have thoughts on our next kingdom. One anchor down. Five to go." },
        }
    };

    /// <summary>Senna reacts to any kills. Reflects the moral weight.</summary>
    public static DialogueData SennaPostMission(int totalKills, bool cowlDead) => new()
    {
        ConversationId = "senna_post_mission",
        Lines = new DialogueLine[]
        {
            new()
            {
                Id = "1", Speaker = "Senna", IsEntry = true, NextLineId = "2",
                Text = cowlDead
                    ? "I heard the news. The lord of Goldmanor is dead."
                    : totalKills == 1
                        ? "They said someone died tonight. By your hand."
                        : $"That's {totalKills} now. The killing is adding up, Vetch."
            },
            new()
            {
                Id = "2", Speaker = "Senna", NextLineId = "3",
                Text = cowlDead
                    ? "I won't celebrate a death. Even his. But the children breathe easier already."
                    : "Every death has weight. Don't let yourself forget that."
            },
            new() { Id = "3", Speaker = "Senna", Text = "Rest, if you can. The fire is low but it's warm." },
        }
    };

    /// <summary>Grael reacts to kills — grudging respect growing.</summary>
    public static DialogueData GraelProgress(int totalKills) => new()
    {
        ConversationId = "grael_progress",
        Lines = new DialogueLine[]
        {
            new()
            {
                Id = "1", Speaker = "Grael", IsEntry = true, NextLineId = "2",
                Text = totalKills >= 3
                    ? "*grudging nod* You're racking up a count. Almost respectable."
                    : "*looks up from blade* You killed one. Congratulations. There are more."
            },
            new() { Id = "2", Speaker = "Grael", Text = "Still think a proper war party would be faster. But... fine. Your way is working. For now." },
        }
    };

    /// <summary>Grael after Cowl — impressed despite himself.</summary>
    public static DialogueData GraelPostCowl() => new()
    {
        ConversationId = "grael_post_cowl",
        Lines = new DialogueLine[]
        {
            new() { Id = "1", Speaker = "Grael", Text = "*standing* You killed the governor. Alone. In his own house.", IsEntry = true, NextLineId = "2" },
            new() { Id = "2", Speaker = "Grael", Text = "*long pause* ...That's an Irontusk thing to do.", NextLineId = "3" },
            new() { Id = "3", Speaker = "Grael", Text = "Don't let it go to your head. There are five more kingdoms. And they'll be ready for you now." },
        }
    };

    // ═════════════════════════════════════════════════════════════
    //  LORNE — Vetch's Sister / Crafter (the ticking clock)
    // ═════════════════════════════════════════════════════════════

    /// <summary>Lorne's initial dialogue — establishes her personality and the crafting offer.</summary>
    public static DialogueData LorneAct1() => new()
    {
        ConversationId = "lorne_act1",
        Lines = new DialogueLine[]
        {
            new() { Id = "1", Speaker = "Lorne", Text = "*carving a small bone figure, hands trembling* Oh good, you're not dead. I was about to repurpose your bedroll.", IsEntry = true, NextLineId = "2" },
            new() { Id = "2", Speaker = "Lorne", Text = "*holds up a bone needle, slightly crooked* See this? Used to make these straight. Now they've got... character.", NextLineId = "3" },
            new() { Id = "3", Speaker = "Lorne", Text = "I can still craft, Vet. My hands shake but my head works fine. Probably. Don't ask Senna.", NextLineId = "4" },
            new() { Id = "4", Speaker = "Vetch", Text = "...", Choices = new[]
            {
                "What can you make?|5a",
                "You should rest.|5b",
                "Show me what you've got.|5c"
            }},
            new() { Id = "5a", Speaker = "Lorne", Text = "Salves. Poisons. Smoke bombs. Needles for the Needlewise. Things that keep you alive so I don't have to be sad.", NextLineId = "6" },
            new() { Id = "5b", Speaker = "Lorne", Text = "*flat stare* I rest when I'm dead. Which, at this rate, is Thursday. So let me work while I can.", NextLineId = "6" },
            new() { Id = "5c", Speaker = "Lorne", Text = "Eager. I like that. Come sit. Bring materials and I'll make something that doesn't involve stabbing for once.", NextLineId = "6" },
            new() { Id = "6", Speaker = "Lorne", Text = "Bring me herbs, bone, metal — whatever you scavenge out there. I'll turn it into something useful.", NextLineId = "7" },
            new() { Id = "7", Speaker = "Lorne", Text = "Just... come back in one piece, Vet. I'm running out of jokes to tell Senna and she's starting to worry.", SetFlag = "lorne_crafting_unlocked" },
        }
    };

    /// <summary>Lorne after some kills — she's noticed the toll.</summary>
    public static DialogueData LorneProgress(int totalKills) => new()
    {
        ConversationId = "lorne_progress",
        Lines = new DialogueLine[]
        {
            new()
            {
                Id = "1", Speaker = "Lorne", IsEntry = true, NextLineId = "2",
                Text = totalKills >= 3
                    ? "*coughing* You smell like blood again. At least it's not yours this time. ...It's not yours, right?"
                    : "*looking up from workbench* Back already? Let me guess — someone's dead and you need me to make something."
            },
            new() { Id = "2", Speaker = "Lorne", Text = "My hands are worse today. But I carved you something. *slides a tiny bone animal across the table* Don't look at it too closely. The legs are uneven.", NextLineId = "3" },
            new() { Id = "3", Speaker = "Vetch", Text = "...", Choices = new[]
            {
                "It's perfect.|4a",
                "I need supplies crafted.|4b",
                "How are you feeling?|4c"
            }},
            new() { Id = "4a", Speaker = "Lorne", Text = "*quiet smile* Liar. But thanks. ...Go on. I know you've got somewhere to be.", NextLineId = "5" },
            new() { Id = "4b", Speaker = "Lorne", Text = "Straight to business. Very heroic. Fine — let me see what you've brought.", NextLineId = "craft" },
            new() { Id = "4c", Speaker = "Lorne", Text = "Like a masterwork in progress. Mostly cracks and held together by spite. *pause* I'm fine, Vet. Go save the world and let me work.", NextLineId = "5" },
            new() { Id = "5", Speaker = "Lorne", Text = "If you've got materials, I can craft. Otherwise, get out of my workshop before I throw a bone needle at you." },
            new() { Id = "craft", Speaker = "Lorne", Text = "Right. Let's see what we can make with this mess.", Event = "open_crafting" },
        }
    };

    /// <summary>Lorne after Cowl is killed — the Edict weakened, she feels it.</summary>
    public static DialogueData LornePostCowl() => new()
    {
        ConversationId = "lorne_post_cowl",
        Lines = new DialogueLine[]
        {
            new() { Id = "1", Speaker = "Lorne", Text = "*sitting very still, hands in lap* Something changed. I felt it.", IsEntry = true, NextLineId = "2" },
            new() { Id = "2", Speaker = "Lorne", Text = "The ache in my bones — it's... less. Like someone loosened a knot I didn't know was there.", NextLineId = "3" },
            new() { Id = "3", Speaker = "Lorne", Text = "You killed the Edictbearer. I won't pretend to understand how that works. But my hands shook less this morning.", NextLineId = "4" },
            new() { Id = "4", Speaker = "Lorne", Text = "*holding up a bone carving — a small bird, precisely detailed* Look. Straight lines again. Almost.", NextLineId = "5" },
            new() { Id = "5", Speaker = "Vetch", Text = "...", Choices = new[]
            {
                "One down. Five to go.|6a",
                "I'm glad it helped.|6b",
                "Need anything crafted?|6c"
            }},
            new() { Id = "6a", Speaker = "Lorne", Text = "Five more. Great. I'll start carving a whole menagerie. *dry laugh, then cough* ...Worth it, though. Keep going.", NextLineId = "7" },
            new() { Id = "6b", Speaker = "Lorne", Text = "*looks away* Don't get sentimental. You'll ruin my reputation as the camp's resident cynic.", NextLineId = "7" },
            new() { Id = "6c", Speaker = "Lorne", Text = "Always. Bring your materials — now that my hands work again, the quality should be better.", NextLineId = "craft" },
            new() { Id = "7", Speaker = "Lorne", Text = "Come find me when you've got materials. I can actually make things properly now. ...Mostly." },
            new() { Id = "craft", Speaker = "Lorne", Text = "*flexes fingers* Alright. Let's make something worth the blood you paid for it.", Event = "open_crafting" },
        }
    };
}
