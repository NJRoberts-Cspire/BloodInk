using Godot;

namespace BloodInk.BloodEchoes;

/// <summary>
/// Echo: "The Sermon of Ash" — Ashen Father Morvain's ordination.
/// Genre: Walking sim. Volcanic ash landscape, temple approach.
/// He knelt until his knees bled. He heard a voice. It might have been his own.
/// </summary>
public partial class EchoMorvainScene : BloodEchoScene
{
    public override void _Ready()
    {
        EchoId = "echo_morvain";
        EchoTitle = "The Sermon of Ash";
        EdictbearerName = "Ashen Father Morvain — The Crucible of the Named";
        WhisperText = "For the children who will never have to fight.";
        NarrationText = "Ash falls like snow. A temple at the end of the path. Walk forward.";
        base._Ready();
    }

    protected override void BuildEchoWorld()
    {
        // Volcanic ash landscape — grey-white ground, dim orange sky
        var sky = new ColorRect
        {
            Color = new Color(0.25f, 0.15f, 0.08f, 1f),
            AnchorRight = 1,
            AnchorBottom = 0.6f
        };
        AddChild(sky);

        var ground = new ColorRect
        {
            Color = new Color(0.45f, 0.44f, 0.42f, 1f),
            AnchorLeft = 0, AnchorTop = 0.6f,
            AnchorRight = 1, AnchorBottom = 1
        };
        AddChild(ground);

        // Ash path to temple
        var path = new ColorRect
        {
            Color = new Color(0.55f, 0.53f, 0.50f, 1f),
            Position = new Vector2(540, 400),
            Size = new Vector2(200, 320)
        };
        AddChild(path);

        // Temple pillars
        for (int i = 0; i < 5; i++)
        {
            var pillar = new ColorRect
            {
                Color = new Color(0.32f, 0.30f, 0.28f, 1f),
                Position = new Vector2(550 + i * 40, 100),
                Size = new Vector2(18, 300)
            };
            AddChild(pillar);

            var body = new StaticBody2D { Position = new Vector2(559 + i * 40, 250) };
            var shape = new CollisionShape2D();
            shape.Shape = new RectangleShape2D { Size = new Vector2(18, 300) };
            body.AddChild(shape);
            AddChild(body);
        }

        // Temple roof
        var roof = new ColorRect
        {
            Color = new Color(0.28f, 0.26f, 0.24f, 1f),
            Position = new Vector2(530, 80),
            Size = new Vector2(220, 30)
        };
        AddChild(roof);

        // Kneeling Morvain figure inside temple
        var morvain = new ColorRect
        {
            Color = new Color(0.20f, 0.18f, 0.22f, 1f),
            Position = new Vector2(625, 200),
            Size = new Vector2(14, 36)
        };
        AddChild(morvain);

        // Falling ash particles (static dots)
        var rng = new RandomNumberGenerator();
        rng.Seed = 12345;
        for (int i = 0; i < 80; i++)
        {
            var ash = new ColorRect
            {
                Color = new Color(0.88f, 0.86f, 0.84f, rng.RandfRange(0.3f, 0.8f)),
                Position = new Vector2(rng.RandfRange(0, 1280), rng.RandfRange(0, 720)),
                Size = new Vector2(3, 3)
            };
            AddChild(ash);
        }

        SetNarration("The ash is warm. Walk to the temple.");

        // Trigger — entering temple
        var trigger = new Area2D { Name = "TempleTrigger" };
        trigger.Position = new Vector2(640, 250);
        var tShape = new CollisionShape2D();
        tShape.Shape = new RectangleShape2D { Size = new Vector2(200, 200) };
        trigger.AddChild(tShape);
        trigger.BodyEntered += (body) =>
        {
            if (body.IsInGroup("Player"))
                TriggerWhisperReveal();
        };
        AddChild(trigger);
    }
}
