using Godot;

namespace BloodInk.BloodEchoes;

/// <summary>
/// Echo: "The Governor's Garden" — Lord Cowl's last peaceful morning.
/// Genre: Walking sim. Player walks through Goldmanor's sunlit balcony
/// watching the wheat fields. Cowl never looked down at what lay beneath them.
/// </summary>
public partial class EchoCowlScene : BloodEchoScene
{
    public override void _Ready()
    {
        EchoId = "echo_cowl";
        EchoTitle = "The Governor's Garden";
        EdictbearerName = "Lord Harlan Cowl — Governor of the Greenhold";
        WhisperText = "I never saw the dark as empty. I thought it was full of things that loved me.";
        NarrationText = "Cowl watches his wheat fields from the balcony. The harvest looks good this year. "
                      + "Below the fields — bones. He has never wondered about the bones.";
        base._Ready();
    }

    protected override void BuildEchoWorld()
    {
        // Warm golden morning — sunlit balcony overlooking fields
        var bg = new ColorRect
        {
            Color = new Color(0.85f, 0.75f, 0.45f, 1f),
            AnchorRight = 1, AnchorBottom = 1
        };
        AddChild(bg);

        // Balcony railing
        var railing = new ColorRect
        {
            Color = new Color(0.55f, 0.40f, 0.22f, 1f),
            Position = new Vector2(0, 280),
            Size = new Vector2(1280, 16)
        };
        AddChild(railing);

        // Wheat field rows (horizontal bands of color)
        for (int i = 0; i < 8; i++)
        {
            var row = new ColorRect
            {
                Color = i % 2 == 0
                    ? new Color(0.72f, 0.60f, 0.25f, 1f)
                    : new Color(0.78f, 0.68f, 0.32f, 1f),
                Position = new Vector2(0, 296 + i * 12),
                Size = new Vector2(1280, 12)
            };
            AddChild(row);
        }

        // Cowl figure — standing at railing
        var cowlFigure = new ColorRect
        {
            Color = new Color(0.25f, 0.20f, 0.30f, 1f),
            Position = new Vector2(600, 230),
            Size = new Vector2(14, 50)
        };
        AddChild(cowlFigure);

        // Sparse narration — guide player toward the railing
        SetNarration("Walk to the balcony railing and look out at the fields.");

        // Simple walkable area — player can approach railing, triggering whisper reveal
        var triggerArea = new Area2D { Name = "RailingTrigger" };
        triggerArea.Position = new Vector2(640, 280);
        var shape = new CollisionShape2D();
        shape.Shape = new RectangleShape2D { Size = new Vector2(1280, 40) };
        triggerArea.AddChild(shape);
        triggerArea.BodyEntered += (body) =>
        {
            if (body.IsInGroup("Player"))
                TriggerWhisperReveal();
        };
        AddChild(triggerArea);
    }
}
