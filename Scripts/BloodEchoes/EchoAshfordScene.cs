using Godot;

namespace BloodInk.BloodEchoes;

/// <summary>
/// Echo: "First Blood" — Huntmaster Ashford's first orc hunt at age fourteen.
/// Genre: Stealth. Player must track through forest without being seen by the 'target'.
/// The prey begged in a language she almost understood.
/// </summary>
public partial class EchoAshfordScene : BloodEchoScene
{
    public override void _Ready()
    {
        EchoId = "echo_ashford";
        EchoTitle = "First Blood";
        EdictbearerName = "Huntmaster General Brielle Ashford — The Verdancy";
        WhisperText = "The best prey is the kind that almost gets away.";
        NarrationText = "Forest. Fourteen years old. Your mother handed you the bow. Follow the tracks.";
        base._Ready();
    }

    protected override void BuildEchoWorld()
    {
        // Dense forest — dark greens, dappled light
        var bg = new ColorRect
        {
            Color = new Color(0.10f, 0.14f, 0.08f, 1f),
            AnchorRight = 1, AnchorBottom = 1
        };
        AddChild(bg);

        // Tree trunks — scattered across scene
        var treePositions = new Vector2[]
        {
            new(80, 100), new(240, 200), new(400, 80), new(560, 300),
            new(720, 140), new(900, 260), new(1050, 100), new(1180, 320),
            new(160, 450), new(480, 520), new(700, 480), new(960, 500),
        };

        foreach (var tpos in treePositions)
        {
            var trunk = new ColorRect
            {
                Color = new Color(0.22f, 0.17f, 0.10f, 1f),
                Position = tpos,
                Size = new Vector2(18, 60)
            };
            AddChild(trunk);

            var canopy = new ColorRect
            {
                Color = new Color(0.12f, 0.20f, 0.08f, 0.7f),
                Position = tpos + new Vector2(-24, -40),
                Size = new Vector2(66, 50)
            };
            AddChild(canopy);

            // Tree trunks are solid
            var body = new StaticBody2D { Position = tpos + new Vector2(9, 30) };
            var shape = new CollisionShape2D();
            shape.Shape = new RectangleShape2D { Size = new Vector2(18, 60) };
            body.AddChild(shape);
            AddChild(body);
        }

        // Shadow zones — under canopies
        AddShadowZone(new Vector2(300, 300), new Vector2(200, 120));
        AddShadowZone(new Vector2(800, 200), new Vector2(180, 100));
        AddShadowZone(new Vector2(500, 450), new Vector2(220, 100));

        // Orc figure (the prey) — moves slowly across screen
        var orcFigure = new ColorRect
        {
            Name = "OrcFigure",
            Color = new Color(0.28f, 0.35f, 0.22f, 1f),
            Position = new Vector2(200, 380),
            Size = new Vector2(16, 50)
        };
        AddChild(orcFigure);

        // Paw-print trail markers
        for (int i = 0; i < 6; i++)
        {
            var track = new ColorRect
            {
                Color = new Color(0.40f, 0.35f, 0.25f, 0.6f),
                Position = new Vector2(180 + i * 120, 420 + (i % 2) * 10),
                Size = new Vector2(8, 8)
            };
            AddChild(track);
        }

        SetNarration("Follow the tracks. Stay hidden. Your hands are steady.");

        // Trigger — reaching end of track
        var trigger = new Area2D { Name = "TrailEndTrigger" };
        trigger.Position = new Vector2(1000, 380);
        var tShape = new CollisionShape2D();
        tShape.Shape = new RectangleShape2D { Size = new Vector2(120, 120) };
        trigger.AddChild(tShape);
        trigger.BodyEntered += (body) =>
        {
            if (body.IsInGroup("Player"))
                TriggerWhisperReveal();
        };
        AddChild(trigger);
    }

    private void AddShadowZone(Vector2 pos, Vector2 size)
    {
        var zone = new Stealth.ShadowZone { Name = "ShadowZone" };
        zone.Position = pos;
        zone.CollisionLayer = 0;
        zone.CollisionMask = 1 << 1;
        var shape = new CollisionShape2D();
        shape.Shape = new RectangleShape2D { Size = size };
        zone.AddChild(shape);
        var vis = new ColorRect { Color = new Color(0.02f, 0.04f, 0.02f, 0.4f), Position = -size / 2, Size = size };
        zone.AddChild(vis);
        AddChild(zone);
    }
}
