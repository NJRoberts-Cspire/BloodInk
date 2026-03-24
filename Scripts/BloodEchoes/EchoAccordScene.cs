using Godot;
using System.Collections.Generic;

namespace BloodInk.BloodEchoes;

/// <summary>
/// Echo: "The Unanimous Vote" — founding of the Council of Scales.
/// Genre: Dialogue. Six diplomats vote to distribute the Edict anchor.
/// They were afraid of one person holding that much power. They were right.
/// Player moves around the table listening to each diplomat's reasoning.
/// </summary>
public partial class EchoAccordScene : BloodEchoScene
{
    private readonly List<(string Name, string Quote, Vector2 Pos)> _diplomats = new()
    {
        ("Delegate Voss",    "\"If one hand holds it, it becomes a fist.\"",               new Vector2(300,  250)),
        ("Delegate Miren",   "\"Distribute it. Diffuse it. Contain it.\"",                  new Vector2(500,  150)),
        ("Delegate Harrow",  "\"We are building mercy, not a weapon.\"",                    new Vector2(750,  150)),
        ("Delegate Sule",    "\"The orcs will not outlast our agreement. They never do.\"", new Vector2(950,  250)),
        ("Delegate Tane",    "\"This is the only way to keep any one of us honest.\"",      new Vector2(950,  420)),
        ("The Gray Warden",  "\"Neutrality is a mask. We all wear one.\"",                  new Vector2(300,  420)),
    };

    private int _diplomatsHeard = 0;

    public override void _Ready()
    {
        EchoId = "echo_accord";
        EchoTitle = "The Unanimous Vote";
        EdictbearerName = "The Council of Scales — The Accord Spire";
        WhisperText = "Neutrality is a mask. We all wear one.";
        NarrationText = "Six delegates. One vote. Listen to each of them.";
        base._Ready();
    }

    protected override void BuildEchoWorld()
    {
        // Grand council chamber — dark stone, candlelight
        var bg = new ColorRect
        {
            Color = new Color(0.12f, 0.10f, 0.14f, 1f),
            AnchorRight = 1, AnchorBottom = 1
        };
        AddChild(bg);

        // Council table — long oval
        var table = new ColorRect
        {
            Color = new Color(0.30f, 0.22f, 0.12f, 1f),
            Position = new Vector2(280, 180),
            Size = new Vector2(720, 320)
        };
        AddChild(table);

        // Candle lights on table
        for (int i = 0; i < 6; i++)
        {
            var candle = new PointLight2D
            {
                Position = new Vector2(360 + i * 120, 340),
                Color = new Color(0.9f, 0.6f, 0.2f, 1f),
                Energy = 0.4f,
                TextureScale = 1.5f
            };
            AddChild(candle);
        }

        // Diplomat figures + interaction areas
        foreach (var (name, quote, pos) in _diplomats)
        {
            BuildDiplomat(name, quote, pos);
        }

        SetNarration("Approach each delegate to hear their reasoning.");
    }

    private void BuildDiplomat(string name, string quote, Vector2 pos)
    {
        // Delegate figure
        var figure = new ColorRect
        {
            Color = new Color(0.28f, 0.25f, 0.35f, 1f),
            Position = pos,
            Size = new Vector2(14, 46)
        };
        AddChild(figure);

        // Nameplate
        var nameLabel = new Label
        {
            Text = name,
            Position = pos + new Vector2(-30, -20),
            Scale = new Vector2(0.6f, 0.6f)
        };
        AddChild(nameLabel);

        // Interaction trigger
        var trigger = new Area2D { Name = $"Trigger_{name.Replace(" ", "_")}" };
        trigger.Position = pos + new Vector2(7, 23);
        var shape = new CollisionShape2D();
        shape.Shape = new CircleShape2D { Radius = 40f };
        trigger.AddChild(shape);

        bool heard = false;
        trigger.BodyEntered += (body) =>
        {
            if (body.IsInGroup("Player") && !heard)
            {
                heard = true;
                GD.Print($"[EchoAccord] {name}: {quote}");
                SetNarration(quote);
                _diplomatsHeard++;

                if (_diplomatsHeard >= _diplomats.Count)
                    TriggerWhisperReveal();
            }
        };
        AddChild(trigger);
    }
}
