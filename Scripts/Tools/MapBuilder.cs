using Godot;
using System.Collections.Generic;

namespace BloodInk.Tools;

/// <summary>
/// Procedurally builds a simple TileMapLayer from a string map.
/// Characters in the map string represent tile types:
///   . = floor (stone)
///   w = wooden floor
///   # = wall
///   ^ = wall top (visual only, above walkable floor)
///   ~ = shadow zone (dark overlay)
///   , = grass
///   p = path
///   c = carpet
///   (space) = empty
/// </summary>
public static class MapBuilder
{
    private const int TILE_SIZE = 16;

    /// <summary>
    /// Build the tilemap from a string grid and add it to the parent.
    /// Returns walls as StaticBody2D children for collision.
    /// </summary>
    public static void Build(Node2D parent, string[] rows)
    {
        PlaceholderSprites.CreateAll();

        int mapH = rows.Length;
        int mapW = 0;
        foreach (var r in rows)
            if (r.Length > mapW) mapW = r.Length;

        // Create floor layer.
        for (int y = 0; y < mapH; y++)
        {
            for (int x = 0; x < rows[y].Length; x++)
            {
                char c = rows[y][x];
                string? texName = CharToTile(c);
                if (texName == null) continue;

                var tex = PlaceholderSprites.Get(texName);
                if (tex == null) continue;

                var sprite = new Sprite2D
                {
                    Texture = tex,
                    Position = new Vector2(x * TILE_SIZE + TILE_SIZE / 2, y * TILE_SIZE + TILE_SIZE / 2),
                    ZIndex = -10
                };
                parent.AddChild(sprite);

                // Wall collision.
                if (c == '#')
                {
                    var body = new StaticBody2D();
                    body.Position = sprite.Position;
                    body.CollisionLayer = 1; // World layer.
                    body.CollisionMask = 0;

                    var shape = new CollisionShape2D();
                    var rect = new RectangleShape2D { Size = new Vector2(TILE_SIZE, TILE_SIZE) };
                    shape.Shape = rect;
                    body.AddChild(shape);
                    parent.AddChild(body);
                }

                // Shadow zone — create functional ShadowZone Area2D.
                if (c == '~')
                {
                    var shadow = new Stealth.ShadowZone();
                    shadow.Position = sprite.Position;
                    var shadowShape = new CollisionShape2D();
                    shadowShape.Shape = new RectangleShape2D { Size = new Vector2(TILE_SIZE, TILE_SIZE) };
                    shadow.AddChild(shadowShape);
                    parent.AddChild(shadow);
                }
            }
        }
    }

    private static string? CharToTile(char c) => c switch
    {
        '.' => "tile_floor_stone",
        'w' => "tile_floor_wood",
        '#' => "tile_wall",
        '^' => "tile_wall_top",
        '~' => "tile_shadow",
        ',' => "tile_grass",
        'p' => "tile_path",
        'c' => "tile_carpet",
        ' ' => null,
        _ => "tile_floor_stone"
    };

    /// <summary>Get world position for a tile coordinate.</summary>
    public static Vector2 TileToWorld(int x, int y) =>
        new Vector2(x * TILE_SIZE + TILE_SIZE / 2, y * TILE_SIZE + TILE_SIZE / 2);

    /// <summary>Get world position for a tile coordinate (Vector2I).</summary>
    public static Vector2 TileToWorld(Vector2I tile) =>
        TileToWorld(tile.X, tile.Y);
}
