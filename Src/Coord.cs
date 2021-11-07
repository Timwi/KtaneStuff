using System;
using System.Collections.Generic;
using System.Linq;
using RT.Util;

namespace KtaneStuff
{
    public struct Coord : IEquatable<Coord>
    {
        public int Index { get; private set; }
        public int Width { get; private set; }
        public int Height { get; private set; }
        public int X => Index % Width;
        public int Y => Index / Width;

        public Coord(int width, int height, int index) { Width = width; Height = height; Index = index; }
        public Coord(int width, int height, int x, int y) : this(width, height, x + width * y) { }
        public override string ToString() => $"({X}, {Y})/({Width}×{Height})";

        public Coord AddXWrap(int dx) => new Coord(Width, Height, ((X + dx) % Width + Width) % Width, Y);
        public Coord AddYWrap(int dy) => new Coord(Width, Height, X, ((Y + dy) % Height + Height) % Height);
        public Coord AddWrap(int dx, int dy) => new Coord(Width, Height, ((X + dx) % Width + Width) % Width, ((Y + dy) % Height + Height) % Height);
        public Coord AddWrap(Coord c) => AddWrap(c.X, c.Y);

        public bool Equals(Coord other) => other.Index == Index && other.Width == Width && other.Height == Height;
        public override bool Equals(object obj) => obj is Coord other && Equals(other);
        public override int GetHashCode() => unchecked(Index * 1048583 + Width * 1031 + Height);

        public static bool operator ==(Coord one, Coord two) => one.Equals(two);
        public static bool operator !=(Coord one, Coord two) => !one.Equals(two);

        public static IEnumerable<Coord> Cells(int w, int h) => Enumerable.Range(0, w * h).Select(ix => new Coord(w, h, ix));
        public bool AdjacentToWrap(Coord other) => other == AddXWrap(1) || other == AddXWrap(-1) || other == AddYWrap(1) || other == AddYWrap(-1);

        public bool CanGoTo(GridDirection dir) => dir switch
        {
            GridDirection.Up => Y > 0,
            GridDirection.UpRight => Y > 0 && X < Width - 1,
            GridDirection.Right => X < Width - 1,
            GridDirection.DownRight => Y < Height - 1 && X < Width - 1,
            GridDirection.Down => Y < Height - 1,
            GridDirection.DownLeft => Y < Height - 1 && X > 0,
            GridDirection.Left => X > 0,
            GridDirection.UpLeft => X > 0 && Y > 0,
            _ => throw new ArgumentOutOfRangeException(nameof(dir), "Invalid GridDirection enum value."),
        };

        public Coord? Neighbor(GridDirection dir) => CanGoTo(dir) ? NeighborWrap(dir).Nullable() : null;

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
                        yield return NeighborWrap((GridDirection) i);
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
                for (var i = 0; i < 8; i += 2)
                    if (CanGoTo((GridDirection) i))
                        yield return NeighborWrap((GridDirection) i);
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