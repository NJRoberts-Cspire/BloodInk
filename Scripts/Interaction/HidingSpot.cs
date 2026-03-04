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

    /// <summary>Stored player position to restore on exit.</summary>
    private Vector2 _storedPosition;

    protected override void InteractableReady()
    {
        ActionVerb = "Hide in";
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
        _storedPosition = player.GlobalPosition;

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

        ActionVerb = "Exit";
        EmitSignal(SignalName.PlayerHid);
        GD.Print($"Player hidden in {DisplayName}.");
    }

    private void UnhidePlayer()
    {
        if (_hiddenPlayer == null) return;

        // Restore player position and visibility.
        _hiddenPlayer.GlobalPosition = _storedPosition;

        if (_hiddenPlayer is CharacterBody2D body)
        {
            body.SetCollisionLayerValue(2, true);
        }

        var stealth = _hiddenPlayer.GetNodeOrNull<StealthProfile>("StealthProfile");
        if (stealth != null)
        {
            stealth.CoverZoneCount -= 100;
        }

        var sprite = _hiddenPlayer.GetNodeOrNull<AnimatedSprite2D>("AnimatedSprite2D");
        if (sprite != null)
            sprite.Visible = true;

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
