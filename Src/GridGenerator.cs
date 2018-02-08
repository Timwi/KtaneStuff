using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using RT.Util;
using RT.Util.Consoles;
using RT.Util.ExtensionMethods;

namespace KtaneStuff
{
    public sealed class GridGenerator
    {
        /// <summary>
        ///     Generates a Sudoku-like grid, distributing numbers in rows and columns.</summary>
        /// <param name="width">
        ///     Width of the grid.</param>
        /// <param name="height">
        ///     Height of the grid.</param>
        /// <param name="minNum">
        ///     Smallest number to be used in the grid.</param>
        /// <param name="maxNum">
        ///     Largest number to be used in the grid.</param>
        /// <param name="extraTaken">
        ///     Function that can specify additional constraints. Without this function, the standard constraints are only to
        ///     have unique numbers in each row or column. To implement a true Sudoku, an additional criterion has to be added
        ///     to constrain the numbers in each subgrid. The first parameter is the current (incomplete) grid. The second
        ///     parameter is the index at which a number has just been placed. The third parameter is the number placed, minus
        ///     <paramref name="minNum"/>. The return value is a bitfield specifying which grid locations can no longer hold
        ///     which values.</param>
        /// <param name="rnd">
        ///     Optional random number generator.</param>
        /// <returns>
        ///     A lazy enumerable containing all solutions. To find just one solution, enumerate only the first element.</returns>
        public static IEnumerable<int[]> GenerateGrid(int width, int height, int minNum, int maxNum, Func<int[], int, int, BigInteger> extraTaken = null, Random rnd = null)
        {
            var max = maxNum - minNum + 1;
            var generator = new GridGenerator(Ut.NewArray(width * height, _ => rnd == null ? Rnd.Next(max) : rnd.Next(max)), width, height, max, extraTaken);
            foreach (var result in generator.recurse(BigInteger.Zero, 0, 0))
                yield return Ut.NewArray(width * height, ix => generator._grid[ix] + minNum);
        }

        /// <summary>
        ///     Generates a Sudoku.</summary>
        /// <param name="size">
        ///     The size of each subgrid. For example, if <paramref name="size"/> is 3, the generated Sudoku is a standard
        ///     Sudoku with 9 rows, 9 columns and 9 subgrids.</param>
        /// <returns/>
        public static int[] GenerateSudoku(int size)
        {
            var sq = size * size;
            var block = (~(BigInteger.MinusOne << (size * (sq + 1)))) / (~(BigInteger.MinusOne << (sq + 1)));
            for (int i = 1; i < size; i++)
                block |= block << (sq * (sq + 1));
            return GenerateGrid(sq, sq, 1, sq, (grid, index, num) => block << (num + size * (sq + 1) * ((index % sq) / size) + size * sq * (sq + 1) * (index / (size * sq)))).First();
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
                var len = (_max + 1).ToString().Length;
                Console.Clear();
                ConsoleUtil.WriteLine(Enumerable.Range(0, _max).Select(row => _grid.Subarray(_max * row, _max).Select((i, col) => i.ToString().PadLeft(len).Color(row < y || (row == y && col < x) ? ConsoleColor.White : ConsoleColor.Red)).JoinColoredString(" ")).JoinColoredString("\n"));
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
