using Godot;
using BloodInk.Stealth;

namespace BloodInk.Interaction;

/// <summary>
/// A hiding spot the player can enter to become fully hidden.
/// While inside, the player can't move but is invisible to guards.
/// Examples: closets, barrels, tall grass, under tables.
/// </summary>
public partial class HidingSpot : Interactable
{
    [Signal] public delegate void PlayerHidEventHandler();
    [Signal] public delegate void PlayerUnhidEventHandler();

    /// <summary>Whether a player is currently hidden inside.</summary>
    public bool IsOccupied { get; private set; } = false;

    /// <summary>The player currently hiding.</summary>
    private Node2D? _hiddenPlayer;

    protected override void InteractableReady()
    {
        ActionVerb = "Hide in";
        TreeExiting += OnTreeExiting;
    }

    /// <summary>Auto-unhide the player if this node is freed while occupied.</summary>
    private void OnTreeExiting()
    {
        if (IsOccupied) UnhidePlayer();
    }

    public override void OnInteract(Node2D interactor)
    {
        if (!IsOccupied)
        {
            HidePlayer(interactor);
        }
        else if (_hiddenPlayer == interactor)
        {
            UnhidePlayer();
        }
    }

    private void HidePlayer(Node2D player)
    {
        IsOccupied = true;
        _hiddenPlayer = player;

        // Move player to hiding spot position.
        player.GlobalPosition = GlobalPosition;

        // Make player invisible and undetectable.
        if (player is CharacterBody2D body)
        {
            body.SetCollisionLayerValue(2, false); // Remove from player layer.
        }

        var stealth = player.GetNodeOrNull<StealthProfile>("StealthProfile");
        if (stealth != null)
        {
            stealth.CoverZoneCount += 100; // Guaranteed hidden.
        }

        // Hide the player sprite.
        var sprite = player.GetNodeOrNull<AnimatedSprite2D>("AnimatedSprite2D");
        if (sprite != null)
            sprite.Visible = false;

        // Disable state machine so the player can't attack/dodge while hidden.
        var stateMachine = player.GetNodeOrNull<Core.StateMachine>("StateMachine");
        if (stateMachine != null)
            stateMachine.ProcessMode = ProcessModeEnum.Disabled;

        ActionVerb = "Exit";
        EmitSignal(SignalName.PlayerHid);
        GD.Print($"Player hidden in {DisplayName}.");
    }

    private void UnhidePlayer()
    {
        if (_hiddenPlayer == null) return;

        // Restore player near the hiding spot (offset slightly) instead of the
        // pre-hide position, which could be far away or inside a wall.
        _hiddenPlayer.GlobalPosition = GlobalPosition + new Vector2(0, 16);

        if (_hiddenPlayer is CharacterBody2D body)
        {
            body.SetCollisionLayerValue(2, true);
        }

        var stealth = _hiddenPlayer.GetNodeOrNull<StealthProfile>("StealthProfile");
        if (stealth != null)
        {
            stealth.CoverZoneCount = System.Math.Max(0, stealth.CoverZoneCount - 100);
        }

        var sprite = _hiddenPlayer.GetNodeOrNull<AnimatedSprite2D>("AnimatedSprite2D");
        if (sprite != null)
            sprite.Visible = true;

        // Re-enable state machine.
        var stateMachine = _hiddenPlayer.GetNodeOrNull<Core.StateMachine>("StateMachine");
        if (stateMachine != null)
            stateMachine.ProcessMode = ProcessModeEnum.Inherit;

        IsOccupied = false;
        _hiddenPlayer = null;
        ActionVerb = "Hide in";
        EmitSignal(SignalName.PlayerUnhid);
        GD.Print($"Player emerged from {DisplayName}.");
    }

    public override string GetPromptText()
    {
        return IsOccupied ? "[E] Exit hiding spot" : base.GetPromptText();
    }
}
