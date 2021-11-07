using System;
using System.Collections.Generic;
using System.Linq;
using RT.Util;
using RT.Util.ExtensionMethods;

namespace KtaneStuff
{
    public class Polyomino : IEquatable<Polyomino>
    {
        private readonly int _w;
        private readonly int _h;
        private readonly bool[] _arr;

        private Polyomino() { }
        private Polyomino(int w, int h, bool[] arr) { _w = w; _h = h; _arr = arr; }

        public Polyomino(string description)
        {
            var strs = description.Split(',');
            _w = strs.Max(s => s.Length);
            _h = strs.Length;
            _arr = new bool[_w * _h];
            for (var y = 0; y < _h; y++)
                for (var x = 0; x < _w; x++)
                    _arr[x + _w * y] = strs[y].Length > x && strs[y][x] == '#';
        }

        public Polyomino RotateClockwise() => new Polyomino(_h, _w, Ut.NewArray(_h * _w, ix => _arr[(ix / _h) + _w * (_h - 1 - (ix % _h))]));
        public Polyomino Reflect() => new Polyomino(_w, _h, Ut.NewArray(_w * _h, ix => _arr[_w - 1 - (ix % _w) + _w * (ix / _w)]));

        public bool Has(int x, int y) => x >= 0 && x < _w && y >= 0 && y < _h && _arr[x + _w * y];
        public IEnumerable<Coord> Cells => _arr.SelectIndexWhere(b => b).Select(ix => new Coord(_w, _h, ix));

        public bool Equals(Polyomino other) => other._w == _w && other._h == _h && other._arr.SequenceEqual(_arr);
        public override bool Equals(object obj) => obj is Polyomino other && Equals(other);
        public override int GetHashCode() => _arr.Aggregate(_w * 37 + _h, (p, n) => unchecked((p << 1) | (n ? 1 : 0)));
        public static bool operator ==(Polyomino one, Polyomino two) => one.Equals(two);
        public static bool operator !=(Polyomino one, Polyomino two) => !one.Equals(two);

        public override string ToString() => Enumerable.Range(0, _h).Select(row => Enumerable.Range(0, _w).Select(col => _arr[col + _w * row] ? "██" : "░░").JoinString()).JoinString("|\n");
    }
}
