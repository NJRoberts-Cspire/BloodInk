using Godot;
using BloodInk.Combat;
using BloodInk.Core;
using BloodInk.Enemies;
using BloodInk.Enemies.States;
using BloodInk.Interaction;
using BloodInk.Stealth;
using BloodInk.Tools;
using BloodInk.UI;
using BloodInk.World;

namespace BloodInk.Missions;

/// <summary>
/// Base class for procedurally-built mission levels.
/// Provides shared helper factories for guards, shadow zones, hiding spots,
/// area zones, player spawning, and HUD setup — eliminating ~200 lines of
/// duplication across each concrete level.
/// </summary>
public abstract partial class MissionLevelBase : Node2D
{
    // ─── Player Spawn ─────────────────────────────────────────────

    /// <summary>Instantiate the player scene at <paramref name="spawnPos"/>.</summary>
    protected void SpawnPlayer(Vector2 spawnPos)
    {
        var playerScene = GD.Load<PackedScene>("res://Scenes/Player/Player.tscn");
        if (playerScene == null)
        {
            GD.PrintErr("Cannot load Player.tscn!");
            return;
        }

        var player = playerScene.Instantiate<CharacterBody2D>();
        player.Position = spawnPos;
        AddChild(player);

        CallDeferred(MethodName.ApplyPlayerSprite, player);
    }

    /// <summary>Apply placeholder sprite frames to the player.</summary>
    protected void ApplyPlayerSprite(CharacterBody2D player)
    {
        var sprite = player.GetNodeOrNull<AnimatedSprite2D>("AnimatedSprite2D");
        if (sprite == null) return;

        var pFrames = PlaceholderSprites.GetFrames("player_frames");
        if (pFrames != null)
        {
            sprite.SpriteFrames = pFrames;
            sprite.Play("idle");
            return;
        }

        // Fallback: build from single texture.
        var tex = PlaceholderSprites.Get("player");
        if (tex == null) return;

        var frames = new SpriteFrames();
        var crouchTex = PlaceholderSprites.Get("player_crouch") ?? tex;

        foreach (var (anim, speed, loop, useCrouch) in new[]
        {
            ("idle",         1,  true,  false),
            ("run",          8,  true,  false),
            ("attack",      12,  false, false),
            ("dodge",       10,  false, false),
            ("death",        1,  false, false),
            ("crouch_idle",  1,  true,  true),
            ("crouch_walk",  8,  true,  true),
            ("stealth_kill",12,  false, true),
        })
        {
            frames.AddAnimation(anim);
            frames.SetAnimationSpeed(anim, speed);
            frames.SetAnimationLoop(anim, loop);
            frames.AddFrame(anim, useCrouch ? crouchTex : tex);
        }

        sprite.SpriteFrames = frames;
        sprite.Play("idle");
    }

    // ─── HUD Setup ────────────────────────────────────────────────

    /// <summary>Instantiate GameHUD and wire health display to the player.</summary>
    protected void SetupHUD()
    {
        var hudScene = GD.Load<PackedScene>("res://Scenes/UI/GameHUD.tscn");
        if (hudScene == null) return;

        var hudInstance = hudScene.Instantiate();
        AddChild(hudInstance);

        var player = GetTree().GetFirstNodeInGroup("Player") as CharacterBody2D;
        if (player != null && hudInstance is GameHUD gameHud)
        {
            var health = player.GetNodeOrNull<HealthComponent>("HealthComponent");
            if (health != null)
            {
                health.HealthChanged += gameHud.OnHealthChanged;
                gameHud.OnHealthChanged(health.CurrentHealth, health.MaxHealth);
            }
        }
    }

    // ─── Guard Factory ────────────────────────────────────────────

    /// <summary>
    /// Spawn a guard enemy with full collision, hitbox, hurtbox, detection, patrol, and state machine.
    /// </summary>
    protected virtual void AddGuard(Node2D parent, string name, Vector2 pos, Vector2[] waypoints, bool elite = false)
    {
        var guard = new GuardEnemy { Name = name };
        guard.Position = pos;
        guard.CollisionLayer = 1 << 2;  // Enemy layer (layer 3).
        guard.CollisionMask = 1;          // World.
        guard.PatrolSpeed = elite ? 50f : 40f;
        guard.AlertedSpeed = elite ? 80f : 70f;
        guard.ChaseSpeed = elite ? 100f : 90f;
        guard.DetectRange = elite ? 140f : 100f;
        guard.AttackRange = 25f;

        // Sprite placeholder.
        var sprite = new AnimatedSprite2D { Name = "AnimatedSprite2D" };
        sprite.SpriteFrames = CreateGuardSpriteFrames(elite ? "guard_alert" : "guard");
        guard.AddChild(sprite);

        // Body collision.
        var bodyShape = new CollisionShape2D();
        bodyShape.Shape = new RectangleShape2D { Size = new Vector2(10, 14) };
        guard.AddChild(bodyShape);

        // Hurtbox.
        var hurtbox = new Hurtbox { Name = "Hurtbox" };
        hurtbox.CollisionLayer = 0;
        hurtbox.CollisionMask = 1 << 3; // PlayerHitbox.
        var hurtShape = new CollisionShape2D { Name = "HurtboxShape" };
        hurtShape.Shape = new RectangleShape2D { Size = new Vector2(12, 14) };
        hurtbox.AddChild(hurtShape);
        guard.AddChild(hurtbox);

        // Hitbox.
        var hitbox = new Hitbox { Name = "Hitbox" };
        hitbox.CollisionLayer = 1 << 4; // EnemyHitbox.
        hitbox.CollisionMask = 0;
        hitbox.Damage = elite ? 2 : 1;
        hitbox.KnockbackForce = new Vector2(80f, 0f);
        var hitShape = new CollisionShape2D { Name = "HitboxShape" };
        hitShape.Shape = new RectangleShape2D { Size = new Vector2(16, 12) };
        hitbox.AddChild(hitShape);
        guard.AddChild(hitbox);

        // Health.
        var health = new HealthComponent { Name = "HealthComponent" };
        health.MaxHealth = elite ? 5 : 3;
        guard.AddChild(health);

        // Detection sensor.
        var sensor = new DetectionSensor { Name = "DetectionSensor" };
        sensor.ViewDistance = elite ? 140f : 120f;
        sensor.ViewAngle = 55f;
        guard.AddChild(sensor);

        // Patrol route.
        var patrol = new PatrolRoute { Name = "PatrolRoute" };
        patrol.Waypoints = waypoints;
        guard.AddChild(patrol);

        // State machine with 7 guard AI states.
        var stateMachine = new StateMachine { Name = "StateMachine" };
        guard.AddChild(stateMachine);
        AddGuardStates(stateMachine);

        // Set Owner on all descendants BEFORE AddChild so GetOwner<GuardEnemy>()
        // works in Init() when _Ready fires.
        SetOwnerRecursive(guard, guard);
        parent.AddChild(guard);
    }

    /// <summary>Create all 7 guard AI states and add to the state machine.</summary>
    protected static void AddGuardStates(StateMachine stateMachine)
    {
        stateMachine.AddChild(new GuardPatrolState { Name = "Patrol" });
        stateMachine.AddChild(new EnemyIdleState { Name = "Idle" });
        stateMachine.AddChild(new GuardInvestigateState { Name = "Investigate" });
        stateMachine.AddChild(new GuardAlertState { Name = "Alert" });
        stateMachine.AddChild(new GuardChaseState { Name = "Chase" });
        stateMachine.AddChild(new GuardSearchState { Name = "Search" });
        stateMachine.AddChild(new GuardAttackState { Name = "Attack" });
    }

    /// <summary>Recursively set Owner on all descendants so GetOwner works.</summary>
    protected static void SetOwnerRecursive(Node node, Node owner)
    {
        foreach (var child in node.GetChildren())
        {
            child.Owner = owner;
            SetOwnerRecursive(child, owner);
        }
    }

    /// <summary>Create SpriteFrames with placeholder textures for a guard.</summary>
    protected static SpriteFrames CreateGuardSpriteFrames(string textureName)
    {
        var tex = PlaceholderSprites.Get(textureName) ?? PlaceholderSprites.Get("guard")!;
        var frames = new SpriteFrames();

        foreach (var animName in new[] { "idle", "run", "walk", "attack" })
        {
            frames.AddAnimation(animName);
            frames.SetAnimationSpeed(animName, animName == "attack" ? 12 : 8);
            frames.SetAnimationLoop(animName, animName != "attack");
            frames.AddFrame(animName, tex);
        }

        return frames;
    }

    // ─── Environment Factories ────────────────────────────────────

    /// <summary>Add a shadow zone (stealth-friendly dark area).</summary>
    protected static void AddShadowZone(Node2D parent, Vector2 pos, Vector2 size)
    {
        var zone = new ShadowZone { Name = "ShadowZone" };
        zone.Position = pos;
        zone.CollisionLayer = 0;
        zone.CollisionMask = 1 << 1; // Player.

        var shape = new CollisionShape2D();
        shape.Shape = new RectangleShape2D { Size = size };
        zone.AddChild(shape);

        // Visual indicator.
        var visual = new ColorRect();
        visual.Color = new Color(0.05f, 0.05f, 0.1f, 0.4f);
        visual.Position = -size / 2;
        visual.Size = size;
        zone.AddChild(visual);

        parent.AddChild(zone);
    }

    /// <summary>Add a named area zone (for location tracking / restriction).</summary>
    protected static void AddAreaZone(Node2D parent, string areaName, Vector2 center, Vector2 size, bool isRestricted = false)
    {
        var zone = new AreaZone { Name = $"Area_{areaName.Replace(" ", "_")}" };
        zone.Position = center;
        zone.AreaName = areaName;
        zone.IsRestricted = isRestricted;
        zone.CollisionLayer = 0;
        zone.CollisionMask = 1 << 1; // Player.

        var shape = new CollisionShape2D();
        shape.Shape = new RectangleShape2D { Size = size };
        zone.AddChild(shape);
        parent.AddChild(zone);
    }

    /// <summary>Add a hiding spot (interactable stealth object).</summary>
    protected static void AddHidingSpot(Node2D parent, string spotName, Vector2 pos)
    {
        var spot = new HidingSpot { Name = $"HidingSpot_{spotName.Replace(" ", "_")}" };
        spot.Position = pos;
        spot.DisplayName = spotName;
        spot.CollisionLayer = 1 << 5; // Interactable.
        spot.CollisionMask = 1 << 1;  // Player.

        var shape = new CollisionShape2D();
        shape.Shape = new RectangleShape2D { Size = new Vector2(16, 16) };
        spot.AddChild(shape);

        // Visual — slightly darker square.
        var visual = new ColorRect();
        visual.Color = new Color(0.15f, 0.25f, 0.1f, 0.6f);
        visual.Position = new Vector2(-8, -8);
        visual.Size = new Vector2(16, 16);
        spot.AddChild(visual);

        parent.AddChild(spot);
    }

    // ─── Mission Flow Helpers ─────────────────────────────────────

    /// <summary>
    /// Common target kill handler — registers the kill, awards ink,
    /// populates MissionComplete screen, and transitions after delay.
    /// </summary>
    protected void OnTargetKilled(
        string targetId,
        int kingdomIndex,
        string targetDisplayName,
        string whisperText,
        float delaySeconds = 2.0f)
    {
        GD.Print($"═══ TARGET {targetId.ToUpper()} SLAIN ═══");
        GD.Print(whisperText);

        var gm = GameManager.Instance;
        string rewardText = "";
        if (gm != null)
        {
            var killed = gm.Kingdoms[kingdomIndex].KillTarget(targetId);
            if (killed != null)
            {
                gm.InkInventory?.AddInk(killed.InkDrop, killed.InkAmount);
                rewardText = $"Blood-Ink Acquired: {killed.InkAmount}× {killed.InkDrop} Grade";
                GD.Print($"Blood-Ink acquired: {killed.InkAmount}x {killed.InkDrop} grade!");
            }
        }

        MissionComplete.TargetText = targetDisplayName;
        MissionComplete.WhisperText = whisperText;
        MissionComplete.RewardText = rewardText;

        var timer = GetTree().CreateTimer(delaySeconds);
        timer.Timeout += () => GetTree().ChangeSceneToFile("res://Scenes/UI/MissionComplete.tscn");
    }
}
