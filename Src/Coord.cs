using System;
using System.Collections.Generic;
using System.Linq;

namespace KtaneStuff
{
    public struct Coord : IEquatable<Coord>
    {
        public int Index { get; private set; }
        public int Width { get; private set; }
        public int Height { get; private set; }
        public readonly int X => Index % Width;
        public readonly int Y => Index / Width;

        public Coord(int width, int height, int index) { Width = width; Height = height; Index = index; }
        public Coord(int width, int height, int x, int y) : this(width, height, x + width * y) { }
        public override string ToString() => $"({X}, {Y})/({Width}×{Height})";

        public Coord AddXWrap(int dx) => new Coord(Width, Height, ((X + dx) % Width + Width) % Width, Y);
        public Coord AddYWrap(int dy) => new Coord(Width, Height, X, ((Y + dy) % Height + Height) % Height);
        public Coord AddWrap(int dx, int dy) => new Coord(Width, Height, ((X + dx) % Width + Width) % Width, ((Y + dy) % Height + Height) % Height);
        public Coord AddWrap(Coord c) => AddWrap(c.X, c.Y);

        public readonly bool Equals(Coord other) => other.Index == Index && other.Width == Width && other.Height == Height;
        public override bool Equals(object obj) => obj is Coord other && Equals(other);
        public override readonly int GetHashCode() => unchecked(Index * 1048583 + Width * 1031 + Height);

        public static bool operator ==(Coord one, Coord two) => one.Equals(two);
        public static bool operator !=(Coord one, Coord two) => !one.Equals(two);

        public static IEnumerable<Coord> Cells(int w, int h) => Enumerable.Range(0, w * h).Select(ix => new Coord(w, h, ix));
        public bool AdjacentToWrap(Coord other) => other == AddXWrap(1) || other == AddXWrap(-1) || other == AddYWrap(1) || other == AddYWrap(-1);
        public bool AdjacentTo(Coord other) =>
            (CanMoveBy(-1, 0) && AddXWrap(-1) == other) ||
            (CanMoveBy(1, 0) && AddXWrap(1) == other) ||
            (CanMoveBy(0, -1) && AddYWrap(-1) == other) ||
            (CanMoveBy(0, 1) && AddYWrap(1) == other);

        public readonly bool CanGoTo(GridDirection dir, int amount = 1) => dir switch
        {
            GridDirection.Up => Y >= amount,
            GridDirection.UpRight => Y >= amount && X < Width - amount,
            GridDirection.Right => X < Width - amount,
            GridDirection.DownRight => Y < Height - amount && X < Width - amount,
            GridDirection.Down => Y < Height - amount,
            GridDirection.DownLeft => Y < Height - amount && X >= amount,
            GridDirection.Left => X >= amount,
            GridDirection.UpLeft => X >= amount && Y >= amount,
            _ => throw new ArgumentOutOfRangeException(nameof(dir), "Invalid GridDirection enum value."),
        };

        public readonly bool CanMoveBy(int x, int y) => (X + x) >= 0 && (X + x) < Width && (Y + y) >= 0 && (Y + y) < Height;

        public Coord GoTo(GridDirection dir, int amount = 1) => !CanGoTo(dir, amount) ? throw new InvalidOperationException("Taking that many steps in that direction would go off the grid.") : GoToWrap(dir, amount);

        public Coord GoToWrap(GridDirection dir, int amount = 1) => dir switch
        {
            GridDirection.Up => AddWrap(0, -amount),
            GridDirection.UpRight => AddWrap(amount, -amount),
            GridDirection.Right => AddWrap(amount, 0),
            GridDirection.DownRight => AddWrap(amount, amount),
            GridDirection.Down => AddWrap(0, amount),
            GridDirection.DownLeft => AddWrap(-amount, amount),
            GridDirection.Left => AddWrap(-amount, 0),
            GridDirection.UpLeft => AddWrap(-amount, -amount),
            _ => throw new ArgumentOutOfRangeException(nameof(dir), "Invalid GridDirection enum value.")
        };

        public Coord Neighbor(GridDirection dir) => !CanGoTo(dir) ? throw new InvalidOperationException("The grid has no neighbor in that direction.") : NeighborWrap(dir);

        public Coord NeighborWrap(GridDirection dir) => dir switch
        {
            GridDirection.Up => AddWrap(0, -1),
            GridDirection.UpRight => AddWrap(1, -1),
            GridDirection.Right => AddWrap(1, 0),
            GridDirection.DownRight => AddWrap(1, 1),
            GridDirection.Down => AddWrap(0, 1),
            GridDirection.DownLeft => AddWrap(-1, 1),
            GridDirection.Left => AddWrap(-1, 0),
            GridDirection.UpLeft => AddWrap(-1, -1),
            _ => throw new ArgumentOutOfRangeException(nameof(dir), "Invalid GridDirection enum value.")
        };

        public IEnumerable<Coord> Neighbors
        {
            get
            {
                for (var i = 0; i < 8; i++)
                    if (CanGoTo((GridDirection) i))
                        yield return Neighbor((GridDirection) i);
            }
        }

        public IEnumerable<Coord> NeighborsWrap
        {
            get
            {
                for (var i = 0; i < 8; i++)
                    yield return NeighborWrap((GridDirection) i);
            }
        }

        public IEnumerable<Coord> OrthogonalNeighbors
        {
            get
            {
                for (var i = 0; i < 4; i++)
                    if (CanGoTo((GridDirection) (2 * i)))
                        yield return Neighbor((GridDirection) (2 * i));
            }
        }

        public IEnumerable<Coord> OrthogonalNeighborsWrap
        {
            get
            {
                for (var i = 0; i < 8; i += 2)
                    yield return NeighborWrap((GridDirection) i);
            }
        }
    }
}