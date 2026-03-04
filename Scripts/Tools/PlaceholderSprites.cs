using Godot;
using System.Collections.Generic;

namespace BloodInk.Tools;

/// <summary>
/// Generates simple placeholder textures at runtime for the vertical slice.
/// All sprites are small pixel-art-friendly colored rects/shapes.
/// Call CreateAll() early (e.g. from GameManager) to populate the cache.
/// </summary>
public static class PlaceholderSprites
{
    private static readonly Dictionary<string, ImageTexture> _cache = new();
    private static bool _created = false;

    /// <summary>Retrieve a cached texture by name.</summary>
    public static ImageTexture? Get(string name) =>
        _cache.TryGetValue(name, out var tex) ? tex : null;

    /// <summary>Generate all placeholder textures. Safe to call multiple times.</summary>
    public static void CreateAll()
    {
        if (_created) return;
        _created = true;

        // ─── Player (green orc) ──────────────────────
        CreateRect("player", 12, 16, new Color(0.25f, 0.55f, 0.2f));     // body
        CreateRect("player_crouch", 12, 10, new Color(0.2f, 0.45f, 0.15f));

        // ─── Guard enemies ───────────────────────────
        CreateRect("guard", 12, 16, new Color(0.7f, 0.15f, 0.1f));       // red
        CreateRect("guard_alert", 12, 16, new Color(1.0f, 0.3f, 0.1f));  // brighter when alert

        // ─── Lord Cowl boss ──────────────────────────
        CreateRect("cowl", 14, 18, new Color(0.55f, 0.1f, 0.5f));        // purple, larger

        // ─── NPCs ────────────────────────────────────
        CreateRect("npc_needlewise", 12, 16, new Color(0.35f, 0.6f, 0.55f)); // teal
        CreateRect("npc_grael", 14, 18, new Color(0.5f, 0.35f, 0.2f));       // brown, big
        CreateRect("npc_rukh", 12, 14, new Color(0.4f, 0.4f, 0.5f));         // grey
        CreateRect("npc_senna", 10, 14, new Color(0.6f, 0.5f, 0.3f));        // warm gold

        // ─── Props ───────────────────────────────────
        CreateRect("campfire", 10, 10, new Color(0.9f, 0.5f, 0.15f));
        CreateRect("mission_board", 14, 16, new Color(0.45f, 0.3f, 0.15f));
        CreateRect("ink_tent", 20, 18, new Color(0.15f, 0.15f, 0.25f));
        CreateRect("door", 16, 4, new Color(0.4f, 0.25f, 0.12f));
        CreateRect("barrel", 8, 10, new Color(0.35f, 0.22f, 0.1f));
        CreateRect("crate", 10, 10, new Color(0.4f, 0.3f, 0.15f));

        // ─── Tiles (16×16 building blocks) ───────────
        CreateRect("tile_floor_stone", 16, 16, new Color(0.3f, 0.3f, 0.32f));
        CreateRect("tile_floor_wood", 16, 16, new Color(0.4f, 0.28f, 0.15f));
        CreateRect("tile_wall", 16, 16, new Color(0.2f, 0.2f, 0.22f));
        CreateRect("tile_wall_top", 16, 16, new Color(0.15f, 0.15f, 0.18f));
        CreateRect("tile_grass", 16, 16, new Color(0.18f, 0.32f, 0.12f));
        CreateRect("tile_path", 16, 16, new Color(0.35f, 0.3f, 0.2f));
        CreateRect("tile_carpet", 16, 16, new Color(0.5f, 0.15f, 0.1f));
        CreateRect("tile_shadow", 16, 16, new Color(0.05f, 0.05f, 0.08f, 0.6f));

        GD.Print("PlaceholderSprites: All sprites generated.");
    }

    // ─── Helpers ──────────────────────────────────────────────────

    private static void CreateRect(string name, int w, int h, Color color)
    {
        var img = Image.CreateEmpty(w, h, false, Image.Format.Rgba8);
        img.Fill(color);

        // Add a 1px darker border for visibility.
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
