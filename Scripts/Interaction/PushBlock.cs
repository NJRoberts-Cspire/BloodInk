using Godot;

namespace BloodInk.Interaction;

/// <summary>
/// A block the player can push by walking into it. Classic Zelda push block.
/// Moves in the direction the player pushes — snaps to grid after each push.
/// Can activate FloorSwitches when pushed onto them.
/// </summary>
public partial class PushBlock : CharacterBody2D
{
    /// <summary>Grid size for snapping movement.</summary>
    [Export] public int GridSize { get; set; } = 16;

    /// <summary>Push speed in pixels/sec.</summary>
    [Export] public float PushSpeed { get; set; } = 60f;

    /// <summary>Time the player must press against the block before it moves.</summary>
    [Export] public float PushDelay { get; set; } = 0.3f;

    /// <summary>Color of the block.</summary>
    [Export] public Color BlockColor { get; set; } = new(0.4f, 0.35f, 0.3f, 1.0f);

    private Vector2 _targetPosition;
    private bool _isMoving = false;
    private float _pushTimer = 0f;
    private Vector2 _pushDirection = Vector2.Zero;
    private Node2D? _pusher = null;

    public override void _Ready()
    {
        // Place on push-block layer (layer 7) so switches detect it.
        CollisionLayer = (1 << 6) | 1; // PushBlock layer + World layer.
        CollisionMask = 1;              // Collide with world.

        _targetPosition = Position;

        // Snap to grid on spawn.
        Position = SnapToGrid(Position);
        _targetPosition = Position;

        // Visual.
        var visual = new ColorRect
        {
            Color = BlockColor,
            Position = new Vector2(-GridSize / 2f, -GridSize / 2f),
            Size = new Vector2(GridSize, GridSize),
            ZIndex = 1
        };
        AddChild(visual);

        // Stone marking on top to indicate pushable.
        var marking = new ColorRect
        {
            Color = new Color(BlockColor.R * 0.7f, BlockColor.G * 0.7f, BlockColor.B * 0.7f, 1.0f),
            Position = new Vector2(-GridSize / 4f, -GridSize / 4f),
            Size = new Vector2(GridSize / 2f, GridSize / 2f),
            ZIndex = 2
        };
        AddChild(marking);

        // Collision shape.
        var bodyShape = new CollisionShape2D();
        bodyShape.Shape = new RectangleShape2D { Size = new Vector2(GridSize - 2, GridSize - 2) };
        AddChild(bodyShape);
    }

    public override void _PhysicsProcess(double delta)
    {
        if (_isMoving)
        {
            // Slide toward target.
            var direction = (_targetPosition - Position).Normalized();
            float dist = Position.DistanceTo(_targetPosition);

            if (dist < 1f)
            {
                Position = _targetPosition;
                _isMoving = false;
                Velocity = Vector2.Zero;
                GD.Print($"[PushBlock] {Name} settled at {Position}");
            }
            else
            {
                Velocity = direction * PushSpeed;
                MoveAndSlide();

                // If we hit something, stop.
                if (GetSlideCollisionCount() > 0)
                {
                    Position = SnapToGrid(Position);
                    _targetPosition = Position;
                    _isMoving = false;
                    Velocity = Vector2.Zero;
                }
            }
        }
    }

    /// <summary>
    /// Called by player movement when they push against this block.
    /// Direction should be a cardinal unit vector.
    /// </summary>
    public void TryPush(Vector2 direction, Node2D pusher, float delta)
    {
        if (_isMoving) return;

        // Normalize to cardinal direction.
        var cardinal = SnapToCardinal(direction);
        if (cardinal == Vector2.Zero) return;

        if (_pushDirection != cardinal || _pusher != pusher)
        {
            _pushDirection = cardinal;
            _pusher = pusher;
            _pushTimer = 0f;
        }

        _pushTimer += (float)delta;

        if (_pushTimer >= PushDelay)
        {
            // Check if target position is free.
            var target = SnapToGrid(Position + cardinal * GridSize);

            // Raycast to check for walls.
            var spaceState = GetWorld2D().DirectSpaceState;
            var query = PhysicsRayQueryParameters2D.Create(
                Position, target, 1); // World layer mask.
            query.Exclude = new Godot.Collections.Array<Rid> { GetRid() };

            var result = spaceState.IntersectRay(query);
            if (result.Count == 0)
            {
                _targetPosition = target;
                _isMoving = true;
                _pushTimer = 0f;
                GD.Print($"[PushBlock] {Name} pushed {cardinal} → {target}");
            }
            else
            {
                _pushTimer = 0f; // Reset — can't push that way.
            }
        }
    }

    /// <summary>Reset push timer when player stops pushing.</summary>
    public void StopPushing()
    {
        _pushTimer = 0f;
        _pushDirection = Vector2.Zero;
        _pusher = null;
    }

    private Vector2 SnapToGrid(Vector2 pos)
    {
        return new Vector2(
            Mathf.Round(pos.X / GridSize) * GridSize,
            Mathf.Round(pos.Y / GridSize) * GridSize
        );
    }

    private static Vector2 SnapToCardinal(Vector2 dir)
    {
        if (dir.LengthSquared() < 0.1f) return Vector2.Zero;

        if (Mathf.Abs(dir.X) > Mathf.Abs(dir.Y))
            return new Vector2(Mathf.Sign(dir.X), 0);
        else
            return new Vector2(0, Mathf.Sign(dir.Y));
    }
}
