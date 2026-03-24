using Godot;

namespace BloodInk.BloodEchoes;

/// <summary>
/// Echo: "Specimen Zero" — Provost Myre's first dissection.
/// Genre: Puzzle. Player navigates a cold laboratory, finding a way to open
/// the locked door. The 'specimen' is alive. Myre's hands don't shake.
/// </summary>
public partial class EchoMyreScene : BloodEchoScene
{
    public override void _Ready()
    {
        EchoId = "echo_myre";
        EchoTitle = "Specimen Zero";
        EdictbearerName = "Provost Ilian Myre — The Fane of Flensing";
        WhisperText = "I catalogued every sound they made. For science.";
        NarrationText = "A cold laboratory. Steel tables. A locked door. Find the key.";
        base._Ready();
    }

    protected override void BuildEchoWorld()
    {
        // Cold stone laboratory — pale grey-blue
        var bg = new ColorRect
        {
            Color = new Color(0.16f, 0.17f, 0.22f, 1f),
            AnchorRight = 1, AnchorBottom = 1
        };
        AddChild(bg);

        // Lab walls
        BuildWall(new Vector2(0, 0), new Vector2(1280, 20));
        BuildWall(new Vector2(0, 700), new Vector2(1280, 20));
        BuildWall(new Vector2(0, 0), new Vector2(20, 720));
        BuildWall(new Vector2(1260, 0), new Vector2(20, 720));

        // Central dividing wall with locked door
        BuildWall(new Vector2(580, 0), new Vector2(20, 300));
        BuildWall(new Vector2(580, 380), new Vector2(20, 340));

        // Steel examination tables
        BuildTable(new Vector2(150, 280));
        BuildTable(new Vector2(150, 420));
        BuildTable(new Vector2(900, 280));
        BuildTable(new Vector2(900, 420));

        // Key chest — on a table in the back room
        var keyChest = new Interaction.Chest
        {
            Name = "LabKeyChest",
            DisplayName = "Specimen Drawer",
            ContentsType = "key",
            ContentsId = "lab_key",
            ContentsName = "Dissection Key",
            Position = new Vector2(950, 350),
            CollisionLayer = 1 << 5,
            CollisionMask = 1 << 1
        };
        var chestShape = new CollisionShape2D();
        chestShape.Shape = new RectangleShape2D { Size = new Vector2(16, 16) };
        keyChest.AddChild(chestShape);
        AddChild(keyChest);

        // Locked lab door
        AddLockedDoor(this, "Lab Door", new Vector2(580, 340), "lab_key");

        // Shadow zones between tables
        AddShadowZone(this, new Vector2(300, 350), new Vector2(150, 100));
        AddShadowZone(this, new Vector2(800, 350), new Vector2(100, 100));

        // Myre figure — standing at far table (static, observing)
        var myre = new ColorRect
        {
            Color = new Color(0.22f, 0.20f, 0.30f, 1f),
            Position = new Vector2(880, 300),
            Size = new Vector2(12, 48)
        };
        AddChild(myre);

        SetNarration("Find the key. Open the door. She is not watching you.");

        // Trigger — reaching the locked door area opens whisper
        var trigger = new Area2D { Name = "DoorTrigger" };
        trigger.Position = new Vector2(650, 360);
        var tShape = new CollisionShape2D();
        tShape.Shape = new RectangleShape2D { Size = new Vector2(80, 80) };
        trigger.AddChild(tShape);
        trigger.BodyEntered += (body) =>
        {
            if (body.IsInGroup("Player"))
                TriggerWhisperReveal();
        };
        AddChild(trigger);
    }

    private void BuildWall(Vector2 pos, Vector2 size)
    {
        var wall = new ColorRect
        {
            Color = new Color(0.22f, 0.22f, 0.28f, 1f),
            Position = pos,
            Size = size
        };
        AddChild(wall);

        var body = new StaticBody2D { Position = pos + size / 2 };
        var shape = new CollisionShape2D();
        shape.Shape = new RectangleShape2D { Size = size };
        body.AddChild(shape);
        AddChild(body);
    }

    private void BuildTable(Vector2 pos)
    {
        var table = new ColorRect
        {
            Color = new Color(0.32f, 0.34f, 0.40f, 1f),
            Position = pos,
            Size = new Vector2(80, 40)
        };
        AddChild(table);
    }

    // Relay MissionLevelBase helpers for this echo puzzle
    private static void AddShadowZone(Node2D parent, Vector2 pos, Vector2 size)
    {
        var zone = new Stealth.ShadowZone { Name = "ShadowZone" };
        zone.Position = pos;
        zone.CollisionLayer = 0;
        zone.CollisionMask = 1 << 1;
        var shape = new CollisionShape2D();
        shape.Shape = new RectangleShape2D { Size = size };
        zone.AddChild(shape);
        var vis = new ColorRect { Color = new Color(0.05f, 0.05f, 0.1f, 0.4f), Position = -size / 2, Size = size };
        zone.AddChild(vis);
        parent.AddChild(zone);
    }

    private static void AddLockedDoor(Node2D parent, string name, Vector2 pos, string keyId)
    {
        var door = new Interaction.Door
        {
            Name = $"Door_{name.Replace(" ", "_")}",
            Position = pos,
            IsLocked = true,
            RequiredKeyId = keyId,
            DisplayName = name,
            CollisionLayer = 1 << 5,
            CollisionMask = 1 << 1
        };
        var shape = new CollisionShape2D();
        shape.Shape = new RectangleShape2D { Size = new Vector2(16, 16) };
        door.AddChild(shape);
        var body = new StaticBody2D { Name = "StaticBody2D" };
        body.CollisionLayer = 1;
        var bodyShape = new CollisionShape2D();
        bodyShape.Shape = new RectangleShape2D { Size = new Vector2(6, 32) };
        body.AddChild(bodyShape);
        door.AddChild(body);
        parent.AddChild(door);
    }
}
