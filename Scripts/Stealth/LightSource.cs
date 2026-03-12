using Godot;

namespace BloodInk.Stealth;

/// <summary>
/// An extinguishable light source that affects gameplay stealth.
/// When lit: creates an illuminated area where the player is more visible.
/// When extinguished: spawns a ShadowZone in its place, providing concealment.
///
/// Player can interact to extinguish (snuff out torches, blow out candles).
/// Guards can relight extinguished sources if they patrol past them.
///
/// Place in corridors, rooms, and guard posts. The interaction radius
/// comes from the Interactable base class (Area2D collision shape).
/// </summary>
public partial class LightSource : Interaction.Interactable
{
    /// <summary>Whether this light is currently lit.</summary>
    [Export] public bool IsLit { get; set; } = true;

    /// <summary>Radius of the light effect (affects ShadowZone creation).</summary>
    [Export] public float LightRadius { get; set; } = 48f;

    /// <summary>Whether this light can be relit by guards.</summary>
    [Export] public bool CanBeRelit { get; set; } = true;

    /// <summary>Color of the light when lit.</summary>
    [Export] public Color LightColor { get; set; } = new Color(1f, 0.85f, 0.6f, 0.4f);

    /// <summary>Noise radius when extinguishing (slight hiss/puff).</summary>
    [Export] public float ExtinguishNoise { get; set; } = 30f;

    private PointLight2D? _light;
    private ShadowZone? _shadowZone;
    private Sprite2D? _lightSprite;

    protected override void InteractableReady()
    {
        DisplayName = "Torch";
        ActionVerb = "Extinguish";

        // Create the PointLight2D for visual lighting.
        _light = new PointLight2D
        {
            Color = LightColor,
            Energy = 0.8f,
            TextureScale = LightRadius / 64f, // Scale based on radius.
            BlendMode = Light2D.BlendModeEnum.Add,
            ShadowEnabled = true,
            ShadowColor = new Color(0, 0, 0, 0.5f),
        };

        // Use a built-in gradient texture for the light.
        var gradient = new GradientTexture2D();
        gradient.Width = 128;
        gradient.Height = 128;
        gradient.Fill = GradientTexture2D.FillEnum.Radial;
        gradient.FillFrom = new Vector2(0.5f, 0.5f);
        gradient.FillTo = new Vector2(0.5f, 0f);
        var grad = new Gradient();
        grad.SetColor(0, Colors.White);
        grad.SetColor(1, Colors.Transparent);
        gradient.Gradient = grad;
        _light.Texture = gradient;

        AddChild(_light);

        // Flame sprite visual.
        _lightSprite = new Sprite2D();
        var tex = Tools.PlaceholderSprites.Get("particle");
        if (tex != null)
        {
            _lightSprite.Texture = tex;
            _lightSprite.Modulate = new Color(1f, 0.6f, 0.1f, 0.9f);
            _lightSprite.Scale = new Vector2(0.5f, 0.7f);
            _lightSprite.Position = new Vector2(0, -6);
        }
        AddChild(_lightSprite);

        UpdateState();
    }

    public override void OnInteract(Node2D interactor)
    {
        if (!IsLit) return;

        Extinguish();
        base.OnInteract(interactor);
    }

    /// <summary>Extinguish this light source.</summary>
    public void Extinguish()
    {
        if (!IsLit) return;
        IsLit = false;

        // Small noise when extinguishing.
        if (ExtinguishNoise > 0)
            NoisePropagator.Instance?.PropagateNoise(GlobalPosition, ExtinguishNoise);

        UpdateState();
        GD.Print($"[Light] {Name} extinguished at {GlobalPosition}");
    }

    /// <summary>Relight this source (called by guards on patrol).</summary>
    public void Relight()
    {
        if (IsLit || !CanBeRelit) return;
        IsLit = true;
        UpdateState();
        GD.Print($"[Light] {Name} relit at {GlobalPosition}");
    }

    private void UpdateState()
    {
        if (_light != null)
            _light.Enabled = IsLit;

        if (_lightSprite != null)
            _lightSprite.Visible = IsLit;

        if (IsLit)
        {
            ActionVerb = "Extinguish";
            // Remove shadow zone when lit.
            if (_shadowZone != null && IsInstanceValid(_shadowZone))
            {
                _shadowZone.QueueFree();
                _shadowZone = null;
            }
        }
        else
        {
            ActionVerb = CanBeRelit ? "Relight" : "Extinguished";
            IsEnabled = false;

            // Create shadow zone in the now-dark area.
            _shadowZone = new ShadowZone();
            var shape = new CollisionShape2D();
            shape.Shape = new CircleShape2D { Radius = LightRadius };
            _shadowZone.AddChild(shape);
            _shadowZone.GlobalPosition = GlobalPosition;

            // Dark visual overlay.
            var overlay = new Sprite2D();
            var tex = Tools.PlaceholderSprites.Get("particle");
            if (tex != null)
            {
                overlay.Texture = tex;
                overlay.Modulate = new Color(0, 0, 0, 0.3f);
                overlay.Scale = new Vector2(LightRadius / 4f, LightRadius / 4f);
            }
            _shadowZone.AddChild(overlay);

            GetParent()?.CallDeferred("add_child", _shadowZone);
        }
    }

    public override string GetPromptText()
    {
        if (!IsLit && !CanBeRelit) return "Extinguished";
        if (!IsLit) return "[E] Relight Torch";
        return "[E] Extinguish Torch";
    }
}
