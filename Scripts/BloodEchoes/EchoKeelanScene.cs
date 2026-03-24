using Godot;

namespace BloodInk.BloodEchoes;

/// <summary>
/// Echo: "The Undrowned's Keel" — Admiral Keelan's first dive.
/// Genre: Walking sim. Underwater / deep-dark environment, slow movement.
/// Keelan was lowered into black water alongside orcs he would later drown
/// by the hundreds. He was afraid, once.
/// </summary>
public partial class EchoKeelanScene : BloodEchoScene
{
    public override void _Ready()
    {
        EchoId = "echo_keelan";
        EchoTitle = "The Undrowned's Keel";
        EdictbearerName = "Admiral Voss Keelan — The Drench";
        WhisperText = "They sink so quietly. Like they were always meant to be down there.";
        NarrationText = "Keelan descends into black water. Young, afraid. "
                      + "Below him, shapes move in the dark. He does not know yet what he will become.";
        base._Ready();
    }

    protected override void BuildEchoWorld()
    {
        // Deep underwater — dark blue, light filtering from above
        var bg = new ColorRect
        {
            Color = new Color(0.04f, 0.08f, 0.18f, 1f),
            AnchorRight = 1, AnchorBottom = 1
        };
        AddChild(bg);

        // Light shaft from above
        var lightShaft = new ColorRect
        {
            Color = new Color(0.2f, 0.35f, 0.55f, 0.3f),
            Position = new Vector2(540, 0),
            Size = new Vector2(200, 720)
        };
        AddChild(lightShaft);

        // Rope descending from above
        var rope = new ColorRect
        {
            Color = new Color(0.55f, 0.45f, 0.28f, 0.8f),
            Position = new Vector2(635, 0),
            Size = new Vector2(6, 500)
        };
        AddChild(rope);

        // Young Keelan figure — clinging to rope
        var keelanFigure = new ColorRect
        {
            Color = new Color(0.35f, 0.30f, 0.45f, 1f),
            Position = new Vector2(626, 220),
            Size = new Vector2(14, 50)
        };
        AddChild(keelanFigure);

        // Shadowy orc shapes — barely visible in the deep
        for (int i = 0; i < 5; i++)
        {
            var shadow = new ColorRect
            {
                Color = new Color(0.08f, 0.10f, 0.16f, 0.6f),
                Position = new Vector2(100 + i * 220, 450 + (i % 2) * 60),
                Size = new Vector2(18, 40)
            };
            AddChild(shadow);
        }

        SetNarration("The water is cold. Something moves below.");

        // Trigger on descent — walk down
        var triggerArea = new Area2D { Name = "DepthTrigger" };
        triggerArea.Position = new Vector2(640, 500);
        var shape = new CollisionShape2D();
        shape.Shape = new RectangleShape2D { Size = new Vector2(400, 80) };
        triggerArea.AddChild(shape);
        triggerArea.BodyEntered += (body) =>
        {
            if (body.IsInGroup("Player"))
                TriggerWhisperReveal();
        };
        AddChild(triggerArea);
    }
}
