using Godot;
using System;
using BloodInk.Core;

namespace BloodInk.Stealth;

/// <summary>
/// Tracks the player's current stealth profile — visibility, noise output,
/// whether they're in cover/shadow, and movement mode. Attached to the player.
/// Enemy detection systems read from this to determine if they can see/hear the player.
/// </summary>
public partial class StealthProfile : Node
{
    [Signal] public delegate void VisibilityChangedEventHandler(int level);
    [Signal] public delegate void CrouchToggleEventHandler(bool isCrouching);
    [Signal] public delegate void NoiseEmittedEventHandler(int noiseType, float radius);

    // ─── Current State ────────────────────────────────────────────

    /// <summary>Current visibility level (driven by movement + environment).</summary>
    public VisibilityLevel Visibility { get; private set; } = VisibilityLevel.Normal;

    /// <summary>Whether the player is crouching / sneaking.</summary>
    public bool IsCrouching { get; private set; } = false;

    /// <summary>Whether the player is currently inside a shadow zone.</summary>
    public bool IsInShadow { get; set; } = false;

    /// <summary>Whether the player is behind cover.</summary>
    public bool IsInCover { get; set; } = false;

    /// <summary>Number of shadow/cover zones currently overlapping the player.</summary>
    public int ShadowZoneCount { get; set; } = 0;

    /// <summary>Number of cover zones overlapping the player.</summary>
    public int CoverZoneCount { get; set; } = 0;

    /// <summary>Whether the player is in a restricted zone (trespassing).</summary>
    public bool IsInRestrictedZone { get; set; } = false;

    // ─── Visibility Modifiers (from tattoos, items, etc.) ─────────
    /// <summary>Flat modifier subtracted from visibility detection. From tattoo stealth bonus etc.</summary>
    public float StealthModifier { get; set; } = 0f;

    /// <summary>Multiplier on noise radius. Below 1 = quieter. From tattoo/equipment.</summary>
    public float NoiseMultiplier { get; set; } = 1f;

    // ─── Movement speed modifiers ─────────────────────────────────
    /// <summary>Speed multiplier when crouching.</summary>
    [Export] public float CrouchSpeedMultiplier { get; set; } = 0.45f;

    /// <summary>Speed when the player is considered "running" for noise purposes.</summary>
    [Export] public float RunSpeedThreshold { get; set; } = 100f;

    // ─── Owner reference ──────────────────────────────────────────
    private CharacterBody2D? _owner;

    public override void _Ready()
    {
        _owner = GetParent<CharacterBody2D>();
    }

    public override void _PhysicsProcess(double delta)
    {
        UpdateVisibility();
    }

    // ─── Crouch ───────────────────────────────────────────────────

    public void SetCrouching(bool crouching)
    {
        if (IsCrouching == crouching) return;
        IsCrouching = crouching;
        EmitSignal(SignalName.CrouchToggle, crouching);
    }

    public void ToggleCrouch() => SetCrouching(!IsCrouching);

    // ─── Visibility Calculation ───────────────────────────────────

    private void UpdateVisibility()
    {
        IsInShadow = ShadowZoneCount > 0;
        IsInCover = CoverZoneCount > 0;

        var oldVis = Visibility;
        float speed = _owner?.Velocity.Length() ?? 0f;

        if (IsInCover || (IsInShadow && IsCrouching && speed < 5f))
        {
            Visibility = VisibilityLevel.Hidden;
        }
        else if (IsInShadow || IsCrouching)
        {
            Visibility = VisibilityLevel.Low;
        }
        else if (speed > RunSpeedThreshold || IsInRestrictedZone)
        {
            Visibility = VisibilityLevel.Exposed;
        }
        else
        {
            Visibility = VisibilityLevel.Normal;
        }

        if (Visibility != oldVis)
            EmitSignal(SignalName.VisibilityChanged, (int)Visibility);
    }

    // ─── Noise Generation ─────────────────────────────────────────

    /// <summary>
    /// Get the current noise radius in pixels based on movement state.
    /// Enemies within this radius can "hear" the player.
    /// </summary>
    public float GetCurrentNoiseRadius()
    {
        float speed = _owner?.Velocity.Length() ?? 0f;

        float baseRadius;
        if (speed < 5f)
            baseRadius = 0f;        // Standing still = silent.
        else if (IsCrouching)
            baseRadius = 30f;       // Crouch-walk = very small radius.
        else if (speed > RunSpeedThreshold)
            baseRadius = 150f;      // Running = large radius.
        else
            baseRadius = 70f;       // Normal walk.

        return baseRadius * NoiseMultiplier;
    }

    /// <summary>
    /// Get the noise type based on current movement.
    /// </summary>
    public NoiseType GetCurrentNoiseType()
    {
        float speed = _owner?.Velocity.Length() ?? 0f;
        if (speed < 5f) return NoiseType.Silent;
        if (IsCrouching) return NoiseType.Footstep;
        if (speed > RunSpeedThreshold) return NoiseType.Loud;
        return NoiseType.Movement;
    }

    /// <summary>
    /// Emit a one-shot noise event (combat, breaking something, etc.).
    /// Propagates via the NoiseEmitted signal.
    /// </summary>
    public void EmitNoise(NoiseType type, float radiusOverride = -1f)
    {
        float radius = radiusOverride > 0 ? radiusOverride : type switch
        {
            NoiseType.Silent => 0f,
            NoiseType.Footstep => 40f,
            NoiseType.Movement => 80f,
            NoiseType.Loud => 180f,
            NoiseType.Alarm => 400f,
            _ => 80f
        };

        radius *= NoiseMultiplier;
        EmitSignal(SignalName.NoiseEmitted, (int)type, radius);
    }

    // ─── Detection Helpers ────────────────────────────────────────

    /// <summary>
    /// Returns the effective detection range multiplier for enemies checking this player.
    /// 0.0 = undetectable, 1.0 = normal, >1.0 = easier to detect.
    /// </summary>
    public float GetDetectionMultiplier()
    {
        float baseMult = Visibility switch
        {
            VisibilityLevel.Hidden => 0.0f,
            VisibilityLevel.Low => 0.4f,
            VisibilityLevel.Normal => 1.0f,
            VisibilityLevel.Exposed => 1.5f,
            _ => 1.0f
        };

        // Tattoo stealth bonus reduces effective detection.
        float tattoStealth = Core.GameManager.Instance?.TattooSystem?.StealthBonus ?? 0f;
        float totalStealth = StealthModifier + Mathf.Clamp(tattoStealth, 0f, 0.9f);
        baseMult = Math.Max(0f, baseMult - totalStealth);
        return baseMult;
    }
}
