using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using RT.Util;
using RT.Util.Consoles;
using RT.Util.ExtensionMethods;

namespace KtaneStuff
{
    public sealed class GridGenerator
    {
        public static IEnumerable<int[]> GenerateGrid(int width, int height, int minNum, int maxNum, Func<int[], int, int, BigInteger> extraTaken = null, Random rnd = null)
        {
            rnd = rnd ?? new Random();
            var max = maxNum - minNum + 1;
            var generator = new GridGenerator(Ut.NewArray(width * height, _ => rnd.Next(max)), width, height, max, extraTaken);
            foreach (var result in generator.recurse(BigInteger.Zero, 0, 0))
                yield return Ut.NewArray(width * height, ix => generator._grid[ix] + minNum);
        }

        private GridGenerator(int[] grid, int width, int height, int max, Func<int[], int, int, BigInteger> extraTaken)
        {
            _grid = grid;
            _width = width;
            _height = height;
            _max = max;
            _maxPlusOne = max + 1;
            _extraTaken = extraTaken;

            _adders = Ut.NewArray(_width, _height, (x, y) =>
                // current row
                (((~(BigInteger.MinusOne << _width * _maxPlusOne)) / (~(BigInteger.MinusOne << _maxPlusOne))) << (_width * _maxPlusOne * y)) |
                // current column
                (((~(BigInteger.MinusOne << _width * _height * _maxPlusOne)) / (~(BigInteger.MinusOne << _width * _maxPlusOne))) << (_maxPlusOne * x)));

            _oneOfEach = (~(BigInteger.MinusOne << _width * _height * _maxPlusOne)) / (~(BigInteger.MinusOne << _maxPlusOne));
            _overhang = _oneOfEach << max;
        }

        private int _width, _height, _max, _maxPlusOne;
        private int[] _grid;
        private BigInteger[][] _adders;
        private BigInteger _oneOfEach, _overhang;
        private Func<int[], int, int, BigInteger> _extraTaken;
        private int _debugCounter;
        private static object[] _oneNull = new object[] { null };

        private IEnumerable<object> recurse(BigInteger taken, int x, int y)
        {
            if (y == _height)
                return _oneNull;
            if (x == _width)
                return recurse(taken, 0, y + 1);
            return recurseImpl(taken, x, y);
        }

        private IEnumerable<object> recurseImpl(BigInteger taken, int x, int y)
        {
            _debugCounter++;
            if (_debugCounter == 100000)
            {
                Console.Clear();
                ConsoleUtil.WriteLine(Enumerable.Range(0, 9).Select(row => _grid.Subarray(9 * row, 9).Select((i, col) => i.ToString().Color(row < y || (row == y && col < x) ? ConsoleColor.White : ConsoleColor.Red)).JoinColoredString(" ")).JoinColoredString("\n"));
                _debugCounter = 0;
            }

            var ix = y * _height + x;
            var offset = _grid[ix];

            for (int i = 0; i < _max; i++)
            {
                var j = (i + offset) % _max;
                if ((taken & (BigInteger.One << (j + _maxPlusOne * (x + _width * y)))) != 0)
                    continue;
                var newTaken = taken | (_adders[x][y] << j);
                if (_extraTaken != null)
                    newTaken |= _extraTaken(_grid, y * _width + x, j);
                if (((newTaken + _oneOfEach) & _overhang & (BigInteger.MinusOne << (_maxPlusOne * (x + 1 + _width * y)))) != 0)
                    continue;
                _grid[ix] = j;
                foreach (var result in recurse(newTaken, x + 1, y))
                    yield return result;
            }
        }
    }
}
