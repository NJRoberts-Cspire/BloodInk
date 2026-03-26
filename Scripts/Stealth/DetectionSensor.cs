using Godot;
using BloodInk.Abilities;

namespace BloodInk.Stealth;

/// <summary>
/// Attached to an enemy. Handles vision cone raycasting and noise detection
/// to determine if the enemy can see or hear the player.
/// Works with the player's StealthProfile to account for shadows, cover, and crouching.
/// </summary>
public partial class DetectionSensor : Node2D
{
    [Signal] public delegate void PlayerDetectedEventHandler(Node2D player, int awarenessLevel);
    [Signal] public delegate void PlayerLostEventHandler();
    [Signal] public delegate void NoiseHeardEventHandler(Vector2 noisePosition);

    // ─── Configuration ────────────────────────────────────────────

    [ExportGroup("Vision")]
    /// <summary>Max distance the enemy can see (in pixels).</summary>
    [Export] public float ViewDistance { get; set; } = 110f;

    /// <summary>Vision cone half-angle in degrees.</summary>
    [Export] public float ViewAngle { get; set; } = 55f;

    /// <summary>Close-range detection radius (360° awareness, e.g. bump detection).</summary>
    [Export] public float CloseDetectRadius { get; set; } = 25f;

    /// <summary>Number of raycasts across the vision cone.</summary>
    [Export] public int VisionRayCount { get; set; } = 8;

    [ExportGroup("Hearing")]
    /// <summary>Max distance the enemy can hear noise.</summary>
    [Export] public float HearingRange { get; set; } = 150f;

    /// <summary>Hearing sensitivity multiplier. Higher = hears further.</summary>
    [Export] public float HearingSensitivity { get; set; } = 1.0f;

    [ExportGroup("Awareness")]
    /// <summary>How fast awareness builds when player is visible (per second).</summary>
    [Export] public float AwarenessGainRate { get; set; } = 22f;

    /// <summary>How fast awareness decays when player is NOT visible (per second).</summary>
    [Export] public float AwarenessDecayRate { get; set; } = 23f;

    /// <summary>Awareness threshold to become Suspicious.</summary>
    [Export] public float SuspiciousThreshold { get; set; } = 25f;

    /// <summary>Awareness threshold to become Alerted.</summary>
    [Export] public float AlertedThreshold { get; set; } = 60f;

    /// <summary>Awareness threshold to become Engaged (full detection).</summary>
    [Export] public float EngagedThreshold { get; set; } = 85f;

    // ─── Runtime State ────────────────────────────────────────────

    /// <summary>Current awareness value 0-100.</summary>
    public float Awareness { get; private set; } = 0f;

    /// <summary>Current awareness level (derived from awareness value).</summary>
    public AwarenessLevel CurrentAwareness { get; private set; } = AwarenessLevel.Unaware;

    /// <summary>Whether the player is currently visible to this enemy.</summary>
    public bool CanSeePlayer { get; private set; } = false;

    /// <summary>Last known player position (for investigation).</summary>
    public Vector2 LastKnownPlayerPosition { get; set; } = Vector2.Zero;

    /// <summary>Last heard noise position.</summary>
    public Vector2 LastHeardNoisePosition { get; set; } = Vector2.Zero;

    /// <summary>Whether a noise was heard this frame (consumed by AI states).</summary>
    public bool HasPendingNoise { get; set; } = false;

    /// <summary>The enemy's facing direction (set by the enemy controller).</summary>
    public Vector2 FacingDirection { get; set; } = Vector2.Down;

    // ─── Internal ─────────────────────────────────────────────────
    private Node2D? _player;
    private StealthProfile? _playerStealth;

    public override void _Ready()
    {
        // Find player in the scene tree. Deferred to allow scene to finish loading.
        CallDeferred(MethodName.FindPlayer);

        // Register with noise propagation so guards can hear sounds.
        NoisePropagator.Instance?.RegisterSensor(this);
    }

    public override void _ExitTree()
    {
        NoisePropagator.Instance?.UnregisterSensor(this);
    }

    private void FindPlayer()
    {
        _player = GetTree().GetFirstNodeInGroup("Player") as Node2D;
        if (_player != null)
            _playerStealth = _player.GetNodeOrNull<StealthProfile>("StealthProfile");
    }

    public override void _PhysicsProcess(double delta)
    {
        if (_player == null || !IsInstanceValid(_player))
        {
            FindPlayer();
            return;
        }

        CanSeePlayer = CheckVision();
        UpdateAwareness((float)delta);
    }

    // ─── Vision ───────────────────────────────────────────────────

    private bool CheckVision()
    {
        if (_player == null) return false;

        // MaskOfAsh ability: disguised player cannot be detected by vision.
        var mask = _player.GetNodeOrNull<MaskOfAshAbility>("mask_of_ash");
        if (mask?.IsMasked == true) return false;

        var toPlayer = _player.GlobalPosition - GlobalPosition;
        float distance = toPlayer.Length();

        // Player's stealth affects our effective detection ranges.
        float detectionMult = _playerStealth?.GetDetectionMultiplier() ?? 1.0f;

        // Close-range detection (360°, reduced by stealth).
        if (distance <= CloseDetectRadius * Mathf.Max(0.3f, detectionMult))
        {
            // Even hidden players can be bumped into at very close range.
            if (detectionMult > 0f || distance < CloseDetectRadius * 0.3f)
            {
                return HasLineOfSight(toPlayer, distance);
            }
        }

        // Cone detection.
        float effectiveRange = ViewDistance * detectionMult;
        if (distance > effectiveRange) return false;

        // Check angle.
        float angle = Mathf.RadToDeg(FacingDirection.AngleTo(toPlayer));
        if (Mathf.Abs(angle) > ViewAngle) return false;

        // Raycast to check for walls.
        return HasLineOfSight(toPlayer, distance);
    }

    private bool HasLineOfSight(Vector2 toPlayer, float distance)
    {
        // Use Godot's physics for line-of-sight check.
        var spaceState = GetWorld2D().DirectSpaceState;
        var query = PhysicsRayQueryParameters2D.Create(
            GlobalPosition,
            GlobalPosition + toPlayer.Normalized() * distance,
            1 // World collision layer only.
        );
        query.CollideWithAreas = false;
        query.CollideWithBodies = true;

        var result = spaceState.IntersectRay(query);
        if (result.Count == 0) return true; // No wall hit = clear line of sight.

        // If the hit point is past the player, we can still see them.
        var hitPos = (Vector2)result["position"];
        return GlobalPosition.DistanceTo(hitPos) >= distance - 2f;
    }

    // ─── Awareness ────────────────────────────────────────────────

    private void UpdateAwareness(float delta)
    {
        var oldLevel = CurrentAwareness;

        if (CanSeePlayer)
        {
            // Gain awareness based on visibility level.
            float detectionMult = _playerStealth?.GetDetectionMultiplier() ?? 1.0f;
            float gain = AwarenessGainRate * detectionMult * delta;
            Awareness = Mathf.Min(100f, Awareness + gain);

            // Track last known position.
            if (_player != null)
                LastKnownPlayerPosition = _player.GlobalPosition;
        }
        else
        {
            // Decay awareness when player isn't visible.
            // Slower decay at higher awareness levels (Alerted, Searching, or Engaged).
            float decayMult = CurrentAwareness >= AwarenessLevel.Searching ? 0.5f : 1f;
            Awareness = Mathf.Max(0f, Awareness - AwarenessDecayRate * decayMult * delta);
        }

        // Determine awareness level from value using exported thresholds.
        if (Awareness >= EngagedThreshold)
            CurrentAwareness = AwarenessLevel.Engaged;
        else if (Awareness >= AlertedThreshold)
            CurrentAwareness = AwarenessLevel.Alerted;
        else if (Awareness >= SuspiciousThreshold)
            CurrentAwareness = AwarenessLevel.Suspicious;
        else
            CurrentAwareness = AwarenessLevel.Unaware;

        // If the player is not visible and was previously at Alerted or higher
        // (including already Searching), preserve the Searching state until awareness
        // fully decays below the suspicious threshold. This prevents a guard from
        // silently dropping back to Suspicious/Alerted mid-search.
        if (!CanSeePlayer && Awareness > SuspiciousThreshold
            && oldLevel >= AwarenessLevel.Alerted)
        {
            CurrentAwareness = AwarenessLevel.Searching;
        }

        // Notify on level change.
        if (CurrentAwareness != oldLevel)
        {
            if (CurrentAwareness == AwarenessLevel.Engaged)
                EmitSignal(SignalName.PlayerDetected, _player!, (int)CurrentAwareness);
            else if (CurrentAwareness == AwarenessLevel.Unaware && oldLevel != AwarenessLevel.Unaware)
                EmitSignal(SignalName.PlayerLost);
        }
    }

    // ─── Noise Handling ───────────────────────────────────────────

    /// <summary>
    /// Called externally when a noise occurs. Checks if this enemy can hear it.
    /// </summary>
    public void OnNoiseAtPosition(Vector2 noisePosition, float noiseRadius)
    {
        float distance = GlobalPosition.DistanceTo(noisePosition);
        float effectiveHearing = HearingRange * HearingSensitivity;

        if (distance <= noiseRadius && distance <= effectiveHearing)
        {
            LastHeardNoisePosition = noisePosition;
            HasPendingNoise = true;

            // Boost awareness from noise.
            float noiseAwareness = Mathf.Max(5f, (1f - distance / noiseRadius) * 30f);
            Awareness = Mathf.Min(100f, Awareness + noiseAwareness);

            EmitSignal(SignalName.NoiseHeard, noisePosition);
        }
    }

    /// <summary>Immediately set awareness to full (e.g. alarm triggered).</summary>
    public void ForceEngage()
    {
        Awareness = 100f;
        CurrentAwareness = AwarenessLevel.Engaged;
        if (_player != null)
        {
            LastKnownPlayerPosition = _player.GlobalPosition;
            EmitSignal(SignalName.PlayerDetected, _player, (int)AwarenessLevel.Engaged);
        }
    }

    /// <summary>Reset awareness (e.g. after investigation timeout).</summary>
    public void ResetAwareness()
    {
        var wasAware = CurrentAwareness != AwarenessLevel.Unaware;
        Awareness = 0f;
        CurrentAwareness = AwarenessLevel.Unaware;
        CanSeePlayer = false;
        HasPendingNoise = false;

        // Notify listeners that we've lost the player.
        if (wasAware)
            EmitSignal(SignalName.PlayerLost);
    }

    // ─── Debug Drawing ────────────────────────────────────────────

    public override void _Draw()
    {
        if (!OS.IsDebugBuild()) return;

        // Draw vision cone.
        float halfAngle = Mathf.DegToRad(ViewAngle);
        float facingAngle = FacingDirection.Angle();

        var coneColor = CurrentAwareness switch
        {
            AwarenessLevel.Engaged => new Color(1, 0, 0, 0.15f),
            AwarenessLevel.Alerted => new Color(1, 0.5f, 0, 0.15f),
            AwarenessLevel.Suspicious => new Color(1, 1, 0, 0.1f),
            AwarenessLevel.Searching => new Color(1, 0.3f, 1, 0.15f),
            _ => new Color(0, 1, 0, 0.07f)
        };

        // Simple triangle approximation of the cone.
        int segments = 12;
        for (int i = 0; i < segments; i++)
        {
            float a1 = facingAngle - halfAngle + (2 * halfAngle * i / segments);
            float a2 = facingAngle - halfAngle + (2 * halfAngle * (i + 1) / segments);
            var p1 = new Vector2(Mathf.Cos(a1), Mathf.Sin(a1)) * ViewDistance;
            var p2 = new Vector2(Mathf.Cos(a2), Mathf.Sin(a2)) * ViewDistance;
            DrawTriangle(new[] { Vector2.Zero, p1, p2 }, coneColor);
        }
    }

    private void DrawTriangle(Vector2[] points, Color color)
    {
        DrawPolygon(points, new[] { color, color, color });
    }
}
