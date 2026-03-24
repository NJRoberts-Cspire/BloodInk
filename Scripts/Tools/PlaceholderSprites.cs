using Godot;
using System.Collections.Generic;

namespace BloodInk.Tools;

/// <summary>
/// Loads sprites from the Calciumtrice dungeon tileset (CC-BY 3.0) for map
/// tiles and characters.  Falls back to coloured rectangles when the tileset
/// PNG is missing.  Call CreateAll() early (e.g. from GameManager).
/// </summary>
public static class PlaceholderSprites
{
    private static readonly Dictionary<string, ImageTexture> _cache = new();
    private static readonly Dictionary<string, SpriteFrames> _framesCache = new();
    private static bool _created = false;

    private const string TilesetPath = "res://Assets/Sprites/dungeon_tileset.png";
    private const string CharacterSheetPath = "res://Assets/Sprites/tiny16_characters.png";

    /// <summary>Retrieve a cached texture by name.</summary>
    public static ImageTexture? Get(string name) =>
        _cache.TryGetValue(name, out var tex) ? tex : null;

    /// <summary>Retrieve cached SpriteFrames by name.</summary>
    public static SpriteFrames? GetFrames(string name) =>
        _framesCache.TryGetValue(name, out var f) ? f : null;

    /// <summary>Generate all sprites. Safe to call multiple times.</summary>
    public static void CreateAll()
    {
        if (_created) return;
        _created = true;

        Image? tileset = LoadTilesetImage();

        if (tileset != null)
            CreateFromTileset(tileset);
        else
            CreateFallback();

        GD.Print("PlaceholderSprites: All sprites generated.");
    }

    // ═══════════════════════════════════════════════════════════════
    //  TILESET-BASED CREATION
    // ═══════════════════════════════════════════════════════════════

    private static void CreateFromTileset(Image ts)
    {
        GD.Print("PlaceholderSprites: Using Calciumtrice dungeon tileset.");

        // ─── Map tiles (16×16) ───────────────────────────────────
        ExtractTile(ts, "tile_floor_stone", 32, 128);   // light stone floor
        ExtractTile(ts, "tile_floor_wood",   0, 176);   // wooden planks
        ExtractTile(ts, "tile_wall",        32,  16);   // dark wall face
        ExtractTile(ts, "tile_wall_top",    32,  64);   // wall top edge
        ExtractTile(ts, "tile_path",        64, 128);   // stone floor variant
        ExtractTile(ts, "tile_carpet",      48, 176);   // wood variant stand-in
        // Grass & shadow have no dungeon equivalent – keep procedural.
        CreateRect("tile_grass",  16, 16, new Color(0.18f, 0.32f, 0.12f));
        CreateRect("tile_shadow", 16, 16, new Color(0.05f, 0.05f, 0.08f, 0.6f));

        // ─── Player (character sheet dark assassin, fallback to tileset) ───
        Image? earlyCharSheet = LoadCharacterSheet();
        if (earlyCharSheet != null)
        {
            ExtractTile(earlyCharSheet, "player",        144, 64);   // dark char, standing down
            ExtractTile(earlyCharSheet, "player_crouch", 160, 64);   // walk frame as crouch
            ExtractTile(earlyCharSheet, "guard",         144,  0);   // armoured char, standing down
            ExtractTile(earlyCharSheet, "guard_alert",   160,  0);   // armoured walk frame
        }
        else
        {
            ExtractTile(ts, "player",        32, 416);       // armed
            ExtractTile(ts, "player_crouch", 192, 416);      // unarmed variant
            ExtractTile(ts, "guard",       0, 416);
            ExtractTile(ts, "guard_alert", 16, 416);
        }

        // ─── Lord Cowl (dark rogue, row 26 col 6) ───────────────
        ExtractTile(ts, "cowl", 96, 416);

        // ─── NPCs – lore-specific colours, kept procedural ──────
        CreateRect("npc_needlewise", 12, 16, new Color(0.35f, 0.6f, 0.55f));
        CreateRect("npc_grael",     14, 18, new Color(0.5f, 0.35f, 0.2f));
        CreateRect("npc_rukh",      12, 14, new Color(0.4f, 0.4f, 0.5f));
        CreateRect("npc_senna",     10, 14, new Color(0.6f, 0.5f, 0.3f));
        CreateRect("npc_lorne",     10, 14, new Color(0.55f, 0.4f, 0.45f));

        // ─── Props – kept procedural for now ─────────────────────
        CreateRect("campfire",      10, 10, new Color(0.9f, 0.5f, 0.15f));
        CreateRect("mission_board", 14, 16, new Color(0.45f, 0.3f, 0.15f));
        CreateRect("ink_tent",      20, 18, new Color(0.15f, 0.15f, 0.25f));
        CreateRect("door",          16,  4, new Color(0.4f, 0.25f, 0.12f));
        CreateRect("barrel",         8, 10, new Color(0.35f, 0.22f, 0.1f));
        CreateRect("crate",         10, 10, new Color(0.4f, 0.3f, 0.15f));

        // ─── SpriteFrames (character sheet walk animations) ────
        Image? charSheet = LoadCharacterSheet();
        if (charSheet != null)
        {
            // Dark assassin (cols 9-11, rows 4-7) → player
            BuildCharacterWalkFrames(charSheet, "player_frames",
                colStart: 9, rowStart: 4,
                new[] { "idle", "run", "attack", "dodge", "hurt", "death",
                        "crouch_idle", "crouch_walk", "stealth_kill",
                        "attack_heavy", "thrust", "air_attack", "cast", "staff_attack" });

            // Grey armoured (cols 9-11, rows 0-3) → guards
            BuildCharacterWalkFrames(charSheet, "guard_frames",
                colStart: 9, rowStart: 0,
                new[] { "idle", "run", "walk", "attack", "patrol" });

            GD.Print("PlaceholderSprites: Character walk animations loaded from tiny16_characters.png");
        }
        else
        {
            // Fallback: single-frame from dungeon tileset
            BuildTilesetSpriteFrames(ts, "player_frames", 32, 416, 16, 16,
                new[] { "idle", "run", "attack", "dodge", "hurt", "death",
                        "crouch_idle", "crouch_walk", "stealth_kill",
                        "attack_heavy", "thrust", "air_attack", "cast", "staff_attack" });

            BuildTilesetSpriteFrames(ts, "guard_frames", 0, 416, 16, 16,
                new[] { "idle", "run", "attack", "patrol" });
        }

        BuildTilesetSpriteFrames(ts, "slime_frames", 128, 432, 16, 16,
            new[] { "idle", "run", "attack" });

        // Crossbow enemy — reuse a tileset character tile with a teal tint to distinguish it.
        if (charSheet != null)
            BuildCharacterWalkFrames(charSheet, "crossbowman_frames",
                colStart: 6, rowStart: 0,
                new[] { "idle", "run", "attack", "hurt", "death" });
        else
            BuildTilesetSpriteFrames(ts, "crossbowman_frames", 48, 416, 16, 16,
                new[] { "idle", "run", "attack", "hurt", "death" });
    }

    // ─── Tileset helpers ──────────────────────────────────────────

    private static Image? LoadTilesetImage()
    {
        // Try raw image load first (always available during development).
        var img = new Image();
        if (img.Load(TilesetPath) == Error.Ok)
            return img;

        // Fallback: try the Godot import pipeline (export builds).
        var tex = ResourceLoader.Load<Texture2D>(TilesetPath);
        if (tex != null)
            return tex.GetImage();

        GD.PrintErr($"PlaceholderSprites: tileset not found at {TilesetPath} – using fallback colours.");
        return null;
    }

    private static void ExtractTile(Image source, string name, int x, int y, int w = 16, int h = 16)
    {
        var region = source.GetRegion(new Rect2I(x, y, w, h));
        _cache[name] = ImageTexture.CreateFromImage(region);
    }

    private static Image? LoadCharacterSheet()
    {
        var img = new Image();
        if (img.Load(CharacterSheetPath) == Error.Ok)
            return img;

        var tex = ResourceLoader.Load<Texture2D>(CharacterSheetPath);
        if (tex != null)
            return tex.GetImage();

        GD.Print("PlaceholderSprites: Character sheet not found – using tileset fallback.");
        return null;
    }

    /// <summary>
    /// Build SpriteFrames with real 3-frame walk cycles from the Tiny 16
    /// character sheet (Sharm, CC-BY 3.0).  Layout: 3 cols per character
    /// (stand, walk1, walk2) x 4 rows (down, left, right, up).
    /// </summary>
    private static void BuildCharacterWalkFrames(Image sheet, string name,
        int colStart, int rowStart, string[] animNames)
    {
        var frames = new SpriteFrames();
        if (frames.HasAnimation("default"))
            frames.RemoveAnimation("default");

        // Extract 3 frames × 4 directions.
        var dirFrames = new ImageTexture[4, 3]; // [dir, frame]
        for (int dir = 0; dir < 4; dir++)
        {
            for (int f = 0; f < 3; f++)
            {
                int px = (colStart + f) * 16;
                int py = (rowStart + dir) * 16;
                var region = sheet.GetRegion(new Rect2I(px, py, 16, 16));
                dirFrames[dir, f] = ImageTexture.CreateFromImage(region);
            }
        }

        // Also create tinted variants for special animations.
        ImageTexture TintFrame(ImageTexture src, Color tint)
        {
            var img = src.GetImage();
            var copy = (Image)img.Duplicate();
            for (int y = 0; y < 16; y++)
            {
                for (int x = 0; x < 16; x++)
                {
                    var p = copy.GetPixel(x, y);
                    if (p.A > 0)
                        copy.SetPixel(x, y, new Color(
                            p.R * 0.4f + tint.R * 0.6f,
                            p.G * 0.4f + tint.G * 0.6f,
                            p.B * 0.4f + tint.B * 0.6f,
                            p.A));
                }
            }
            return ImageTexture.CreateFromImage(copy);
        }

        var attackTint  = new Color(0.9f, 0.25f, 0.15f);
        var dodgeTint   = new Color(0.3f, 0.5f, 0.9f);
        var hurtTint    = new Color(1f, 0.1f, 0.1f);
        var deathTint   = new Color(0.2f, 0.05f, 0.05f);
        var stealthTint = new Color(0.1f, 0.1f, 0.15f);

        foreach (var animName in animNames)
        {
            frames.AddAnimation(animName);
            bool isMovement = animName is "run" or "walk" or "patrol"
                                        or "crouch_walk";
            bool loops = animName is "idle" or "run" or "walk"
                                   or "crouch_idle" or "crouch_walk" or "patrol";
            frames.SetAnimationSpeed(animName, isMovement ? 6f : 8f);
            frames.SetAnimationLoop(animName, loops);

            switch (animName)
            {
                // Movement – use all 3 walk frames of the down direction.
                case "run":
                case "walk":
                case "patrol":
                    frames.AddFrame(animName, dirFrames[0, 0]);
                    frames.AddFrame(animName, dirFrames[0, 1]);
                    frames.AddFrame(animName, dirFrames[0, 2]);
                    break;

                case "crouch_walk":
                    frames.AddFrame(animName, dirFrames[0, 1]);
                    frames.AddFrame(animName, dirFrames[0, 2]);
                    break;

                // Idle / crouch idle – just standing frame.
                case "idle":
                case "crouch_idle":
                    frames.AddFrame(animName, dirFrames[0, 0]);
                    break;

                // Combat – tinted frames.
                case "attack":
                case "attack_heavy":
                case "thrust":
                case "air_attack":
                case "staff_attack":
                case "stealth_kill":
                    frames.AddFrame(animName, dirFrames[0, 0]);
                    frames.AddFrame(animName, TintFrame(dirFrames[0, 1],
                        animName == "stealth_kill" ? stealthTint : attackTint));
                    break;

                case "dodge":
                    frames.AddFrame(animName, TintFrame(dirFrames[0, 1], dodgeTint));
                    frames.AddFrame(animName, TintFrame(dirFrames[0, 2], dodgeTint));
                    break;

                case "hurt":
                    frames.AddFrame(animName, TintFrame(dirFrames[0, 0], hurtTint));
                    break;

                case "death":
                    frames.AddFrame(animName, TintFrame(dirFrames[0, 0], deathTint));
                    break;

                case "cast":
                    frames.AddFrame(animName, dirFrames[0, 0]);
                    frames.AddFrame(animName, dirFrames[0, 2]);
                    break;

                default:
                    frames.AddFrame(animName, dirFrames[0, 0]);
                    break;
            }
        }

        _framesCache[name] = frames;
    }

    /// <summary>
    /// Build SpriteFrames from a tileset character sprite.
    /// Frame 0 = base sprite, Frame 1 = 15 % darkened variant.
    /// </summary>
    private static void BuildTilesetSpriteFrames(Image tilesetImg, string name,
        int srcX, int srcY, int w, int h, string[] animNames)
    {
        var frames = new SpriteFrames();
        if (frames.HasAnimation("default"))
            frames.RemoveAnimation("default");

        var baseImg = tilesetImg.GetRegion(new Rect2I(srcX, srcY, w, h));
        var tex0 = ImageTexture.CreateFromImage(baseImg);

        // Pre-build a darkened variant for frame alternation.
        var darkImg = (Image)baseImg.Duplicate();
        for (int py = 0; py < h; py++)
        {
            for (int px = 0; px < w; px++)
            {
                var pixel = darkImg.GetPixel(px, py);
                if (pixel.A > 0)
                    darkImg.SetPixel(px, py,
                        new Color(pixel.R * 0.85f, pixel.G * 0.85f, pixel.B * 0.85f, pixel.A));
            }
        }
        var tex1 = ImageTexture.CreateFromImage(darkImg);

        foreach (var animName in animNames)
        {
            frames.AddAnimation(animName);
            frames.SetAnimationSpeed(animName, 8f);
            bool loops = animName is "idle" or "run" or "crouch_idle" or "crouch_walk" or "patrol";
            frames.SetAnimationLoop(animName, loops);
            frames.AddFrame(animName, tex0);
            frames.AddFrame(animName, tex1);
        }

        _framesCache[name] = frames;
    }

    // ═══════════════════════════════════════════════════════════════
    //  FALLBACK – coloured rectangles (no tileset available)
    // ═══════════════════════════════════════════════════════════════

    private static void CreateFallback()
    {
        GD.Print("PlaceholderSprites: Tileset missing – generating coloured rectangles.");

        // Player
        CreateRect("player",        12, 16, new Color(0.25f, 0.55f, 0.2f));
        CreateRect("player_crouch", 12, 10, new Color(0.2f, 0.45f, 0.15f));

        // Guards
        CreateRect("guard",       12, 16, new Color(0.7f, 0.15f, 0.1f));
        CreateRect("guard_alert", 12, 16, new Color(1.0f, 0.3f, 0.1f));

        // Boss
        CreateRect("cowl", 14, 18, new Color(0.55f, 0.1f, 0.5f));

        // NPCs
        CreateRect("npc_needlewise", 12, 16, new Color(0.35f, 0.6f, 0.55f));
        CreateRect("npc_grael",     14, 18, new Color(0.5f, 0.35f, 0.2f));
        CreateRect("npc_rukh",     12, 14, new Color(0.4f, 0.4f, 0.5f));
        CreateRect("npc_senna",    10, 14, new Color(0.6f, 0.5f, 0.3f));
        CreateRect("npc_lorne",    10, 14, new Color(0.55f, 0.4f, 0.45f));

        // Props
        CreateRect("campfire",      10, 10, new Color(0.9f, 0.5f, 0.15f));
        CreateRect("mission_board", 14, 16, new Color(0.45f, 0.3f, 0.15f));
        CreateRect("ink_tent",      20, 18, new Color(0.15f, 0.15f, 0.25f));
        CreateRect("door",          16,  4, new Color(0.4f, 0.25f, 0.12f));
        CreateRect("barrel",         8, 10, new Color(0.35f, 0.22f, 0.1f));
        CreateRect("crate",         10, 10, new Color(0.4f, 0.3f, 0.15f));

        // Tiles
        CreateRect("tile_floor_stone", 16, 16, new Color(0.3f, 0.3f, 0.32f));
        CreateRect("tile_floor_wood",  16, 16, new Color(0.4f, 0.28f, 0.15f));
        CreateRect("tile_wall",        16, 16, new Color(0.2f, 0.2f, 0.22f));
        CreateRect("tile_wall_top",    16, 16, new Color(0.15f, 0.15f, 0.18f));
        CreateRect("tile_grass",       16, 16, new Color(0.18f, 0.32f, 0.12f));
        CreateRect("tile_path",        16, 16, new Color(0.35f, 0.3f, 0.2f));
        CreateRect("tile_carpet",      16, 16, new Color(0.5f, 0.15f, 0.1f));
        CreateRect("tile_shadow",      16, 16, new Color(0.05f, 0.05f, 0.08f, 0.6f));

        // SpriteFrames
        BuildFallbackSpriteFrames("player_frames",
            new Color(0.25f, 0.55f, 0.2f), 12, 16,
            new[] { "idle", "run", "attack", "dodge", "hurt", "death",
                    "crouch_idle", "crouch_walk", "stealth_kill",
                    "attack_heavy", "thrust", "air_attack", "cast", "staff_attack" });

        BuildFallbackSpriteFrames("slime_frames",
            new Color(0.3f, 0.7f, 0.2f), 12, 10,
            new[] { "idle", "run", "attack" });

        BuildFallbackSpriteFrames("guard_frames",
            new Color(0.7f, 0.15f, 0.1f), 12, 16,
            new[] { "idle", "run", "attack", "patrol" });

        BuildFallbackSpriteFrames("crossbowman_frames",
            new Color(0.15f, 0.5f, 0.6f), 12, 16,
            new[] { "idle", "run", "attack", "hurt", "death" });
    }

    // ═══════════════════════════════════════════════════════════════
    //  SHARED HELPERS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>Build SpriteFrames from coloured rectangles (fallback path).</summary>
    private static void BuildFallbackSpriteFrames(string name, Color baseColor,
        int w, int h, string[] animNames)
    {
        var frames = new SpriteFrames();
        if (frames.HasAnimation("default"))
            frames.RemoveAnimation("default");

        foreach (var animName in animNames)
        {
            frames.AddAnimation(animName);
            frames.SetAnimationSpeed(animName, 8f);
            bool loops = animName is "idle" or "run" or "crouch_idle" or "crouch_walk" or "patrol";
            frames.SetAnimationLoop(animName, loops);

            Color frameColor = animName switch
            {
                "run" or "crouch_walk" or "patrol" => baseColor * 1.15f,
                "attack" or "attack_heavy" or "stealth_kill" => new Color(1f, 0.3f, 0.2f),
                "dodge" => new Color(0.3f, 0.5f, 0.9f),
                "hurt" => new Color(1f, 0.1f, 0.1f),
                "death" => new Color(0.3f, 0.1f, 0.1f),
                _ => baseColor,
            };
            frameColor.A = 1f;

            for (int f = 0; f < 2; f++)
            {
                var img = Image.CreateEmpty(w, h, false, Image.Format.Rgba8);
                Color c = f == 0 ? frameColor : frameColor * 0.85f;
                c.A = 1f;
                img.Fill(c);

                var border = c * 0.5f;
                border.A = 1f;
                for (int x = 0; x < w; x++)
                {
                    img.SetPixel(x, 0, border);
                    img.SetPixel(x, h - 1, border);
                }
                for (int y = 0; y < h; y++)
                {
                    img.SetPixel(0, y, border);
                    img.SetPixel(w - 1, y, border);
                }

                var tex = ImageTexture.CreateFromImage(img);
                frames.AddFrame(animName, tex);
            }
        }

        _framesCache[name] = frames;
    }

    private static void CreateRect(string name, int w, int h, Color color)
    {
        var img = Image.CreateEmpty(w, h, false, Image.Format.Rgba8);
        img.Fill(color);

        var border = color * 0.6f;
        border.A = color.A;
        for (int x = 0; x < w; x++)
        {
            img.SetPixel(x, 0, border);
            img.SetPixel(x, h - 1, border);
        }
        for (int y = 0; y < h; y++)
        {
            img.SetPixel(0, y, border);
            img.SetPixel(w - 1, y, border);
        }

        var tex = ImageTexture.CreateFromImage(img);
        _cache[name] = tex;
    }
}
