using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using RT.Util.Consoles;
using RT.Util.ExtensionMethods;
using Tile = (int top, int right, int bottom, int left);

namespace KtaneStuff
{
    class MazeGenerator
    {
        private readonly int _size;
        private readonly MonoRandom _rand;
        public MazeGenerator(int size, MonoRandom rand)
        {
            _size = size;
            _rand = rand;
        }
        private bool[][] _visited;
        private char[] _charArr;

        public string GenerateMaze()
        {
            _visited = new bool[_size][];
            for (int i = 0; i < _visited.Length; i++)
                _visited[i] = new bool[_size];
            _charArr = Enumerable.Repeat('█', (_size * 2 + 1) * (_size * 2 + 1)).ToArray();
            for (int a = 0; a < _size; a++)
                for (int b = 0; b < _size; b++)
                    _charArr[(a * (_size * 2 + 1) * 2) + (b * 2) + _size * 2 + 2] = ' ';
            var x = _rand.Next(0, _size);
            var y = _rand.Next(0, _size);
            Generate(x, y);
            return new string(_charArr);
        }

        private void Generate(int x, int y)
        {
            _visited[x][y] = true;
            var arr = Enumerable.Range(0, 4).ToArray();
            _rand.ShuffleFisherYates(arr);
            var curPos = (x * (_size * 2 + 1) * 2) + (y * 2) + (_size * 2 + 2);
            for (int i = 0; i < 4; i++)
            {
                switch (arr[i])
                {
                    case 0:
                        if (y != 0 && !_visited[x][y - 1])
                        {
                            _charArr[curPos - 1] = ' ';
                            Generate(x, y - 1);
                        }
                        break;
                    case 1:
                        if (x != _size - 1 && !_visited[x + 1][y])
                        {
                            _charArr[curPos + (_size * 2 + 1)] = ' ';
                            Generate(x + 1, y);
                        }
                        break;
                    case 2:
                        if (y != _size - 1 && !_visited[x][y + 1])
                        {
                            _charArr[curPos + 1] = ' ';
                            Generate(x, y + 1);
                        }
                        break;
                    case 3:
                        if (x != 0 && !_visited[x - 1][y])
                        {
                            _charArr[curPos - (_size * 2 + 1)] = ' ';
                            Generate(x - 1, y);
                        }
                        break;
                }
            }
        }
    }

    static class OddWallOut
    {
        const int _size = 4;
        const int _sttpo = 2 * _size + 1;
        const int _numColors = 4;

        public static void Generate()
        {
            const bool toroidal = true;
            const bool outputAll = false;
            for (var seed = 0; seed < 500; seed++)
            {
                var rnd = new MonoRandom(seed);
                var gen = new MazeGenerator(_size, rnd);
                var maze = gen.GenerateMaze();
                var walls = Enumerable.Range(0, _sttpo * _sttpo / 2).Select(ix => 2 * ix + 1).Where(ix => (!toroidal || (ix % _sttpo != _sttpo - 1 && ix / _sttpo != _sttpo - 1)) && maze[ix] == '█').ToArray().Shuffle(rnd);
                var firstNonEdgeWall = walls.IndexOf(ix => ix % _sttpo != 0 && ix % _sttpo != _sttpo - 1 && ix / _sttpo != 0 && ix / _sttpo != _sttpo - 1);
                if (firstNonEdgeWall != 0)
                    (walls[0], walls[firstNonEdgeWall]) = (walls[firstNonEdgeWall], walls[0]);

                var wallColors = walls.Select(w => rnd.Next(0, _numColors)).ToArray();
                var specialWallColors = Enumerable.Range(0, _numColors).ToArray().Shuffle(rnd).Subarray(0, 2);
                int transformPosition(int pos) => (pos / _size * (_size * 2 + 1) * 2) + (pos % _size * 2) + _size * 2 + 2;
                var tiles = Enumerable.Range(0, _size * _size).Select(pos =>
                {
                    var cell = transformPosition(pos);
                    int toroidalled(int cl) => (cl % _sttpo) % (_sttpo - 1) + _sttpo * ((cl / _sttpo) % (_sttpo - 1));
                    int getColor(int cl, bool sec) => cl == walls[0] ? specialWallColors[sec ? 1 : 0] : walls.IndexOf(cl) is int p && p != -1 ? wallColors[p] : toroidal && walls.IndexOf(toroidalled(cl)) is int p2 && p2 != -1 ? wallColors[p2] : -1;
                    return (top: getColor(cell - _sttpo, false), right: getColor(cell + 1, false), bottom: getColor(cell + _sttpo, true), left: getColor(cell - 1, true));
                }).ToArray();

                if (outputAll)
                {
                    ConsoleUtil.WriteLine(" INTENDED MAZE: ".Color(ConsoleColor.White, ConsoleColor.DarkGreen));
                    Console.WriteLine();
                    ConsoleUtil.WriteLine(VisualizeMaze(tiles));
                    Console.WriteLine();
                }

                const int maxMismatchedWallsAllowed = 1;
                // Reconstruct the maze from the tiles to see if there is a unique solution with exactly one mismatching wall
                IEnumerable<ConsoleColoredString> constructMaze(Tile[] sofar, Tile[] remaining, int[][] disconnectedPieces, int mismatchedWallAlready)
                {
                    if (sofar.Length == _size * _size)
                    {
                        if (remaining.Length != 0)
                            Debugger.Break();
                        if (disconnectedPieces.Length == 1)
                            yield return VisualizeMaze(sofar);
                        yield break;
                    }

                    var cell = sofar.Length;
                    for (var i = 0; i < remaining.Length; i++)
                    {
                        var newTile = remaining[i];

                        // Make sure there is a wall around the edge of the maze
                        if ((cell % _size == 0 && newTile.left == -1) ||
                            (cell % _size == _size - 1 && newTile.right == -1) ||
                            (cell / _size == 0 && newTile.top == -1) ||
                            (cell / _size == _size - 1 && newTile.bottom == -1))
                            continue;

                        int above(int cell) => (cell - _size + _size * _size) % (_size * _size);
                        int left(int cell) => (cell % _size + _size - 1) % _size + _size * (cell / _size);
                        int below(int cell) => (cell + _size) % (_size * _size);
                        int right(int cell) => (cell % _size + 1) % _size + _size * (cell / _size);

                        // Make sure that walls/non-walls join up with each other ABOVE and LEFT
                        if (cell / _size != 0 && (sofar[above(cell)].bottom != -1) != (newTile.top != -1) || cell % _size != 0 && (sofar[left(cell)].right != -1) != (newTile.left != -1))
                            continue;
                        // If toroidal, make sure they join up BELOW and RIGHT
                        if (toroidal && (cell / _size == _size - 1 && (sofar[below(cell)].top != -1) != (newTile.bottom != -1) || cell % _size == _size - 1 && (sofar[right(cell)].left != -1) != (newTile.right != -1)))
                            continue;

                        var conflictsWithAbove = cell / _size != 0 && sofar[above(cell)].bottom != newTile.top;
                        var conflictsWithLeft = cell % _size != 0 && sofar[left(cell)].right != newTile.left;
                        var conflictsWithBelow = toroidal && cell / _size == _size - 1 && sofar[below(cell)].top != newTile.bottom;
                        var conflictsWithRight = toroidal && cell % _size == _size - 1 && sofar[right(cell)].left != newTile.right;

                        var newMismatches = mismatchedWallAlready + (conflictsWithAbove ? 1 : 0) + (conflictsWithLeft ? 1 : 0) + (conflictsWithBelow ? 1 : 0) + (conflictsWithRight ? 1 : 0);
                        if (newMismatches > maxMismatchedWallsAllowed)
                            continue;

                        var pieceAbove = (cell / _size == 0 && !toroidal) || newTile.top != -1 ? -1 : disconnectedPieces.IndexOf(dp => dp.Contains(above(cell)));
                        var pieceLeft = (cell % _size == 0 && !toroidal) || newTile.left != -1 ? -1 : disconnectedPieces.IndexOf(dp => dp.Contains(left(cell)));
                        var newDisconnectedPieces =
                            (pieceAbove != -1 && pieceLeft == pieceAbove) ? disconnectedPieces.Replace(pieceAbove, disconnectedPieces[pieceAbove].Append(cell)) :
                            (pieceAbove != -1 && pieceLeft != -1) ? disconnectedPieces.Remove(Math.Max(pieceLeft, pieceAbove), 1).Remove(Math.Min(pieceLeft, pieceAbove), 1)
                                .Append(disconnectedPieces[pieceAbove].Concat(disconnectedPieces[pieceLeft]).Append(cell)) :
                            (pieceAbove != -1) ? disconnectedPieces.Replace(pieceAbove, disconnectedPieces[pieceAbove].Append(cell)) :
                            (pieceLeft != -1) ? disconnectedPieces.Replace(pieceLeft, disconnectedPieces[pieceLeft].Append(cell)) :
                            disconnectedPieces.Append([cell]);

                        foreach (var solution in constructMaze(sofar.Append(newTile), remaining.Remove(i, 1), newDisconnectedPieces, newMismatches))
                            yield return solution;
                    }
                }

                tiles.Shuffle(rnd);

                if (outputAll)
                {
                    ConsoleUtil.WriteLine(" RECONSTRUCTED MAZES: ".Color(ConsoleColor.White, ConsoleColor.DarkRed));
                    Console.WriteLine();
                }

                var c = 0;
                foreach (var reconstructedMaze in constructMaze([], tiles, [], 0).Distinct())
                {
                    if (outputAll)
                    {
                        ConsoleUtil.WriteLine(reconstructedMaze);
                        Console.WriteLine();
                    }
                    c++;
                }
                ConsoleUtil.WriteLine($"Seed {seed.ToString().Color(ConsoleColor.White),5} = {c.ToString().Color(c == 1 ? ConsoleColor.Green : ConsoleColor.Magenta)}", null);
            }
        }

        private static readonly ConsoleColor[] _colors = [ConsoleColor.Red, ConsoleColor.Yellow, ConsoleColor.Green, ConsoleColor.Blue];

        private static Tile GetTile(Tile[] tiles, int ix) => ix < 0 || ix >= tiles.Length ? (-1, -1, -1, -1) : tiles[ix];
        private static ConsoleColoredString VisualizeMaze(Tile[] tiles) =>
            Enumerable.Range(0, (_size * 2 + 1) * (_size * 2 + 1)).Select(ix => (x: ix % (_size * 2 + 1), y: ix / (_size * 2 + 1)))
                .Select(c =>
                {
                    if (c.x % 2 == 0 && c.y % 2 == 0)
                        return "██".Color(ConsoleColor.DarkGray);
                    if (c.x % 2 == 1 && c.y % 2 == 1)
                        return "  ";
                    if (c.x % 2 == 0)
                    {
                        var rightColor = c.x >= 2 * _size ? -1 : GetTile(tiles, c.x / 2 + _size * (c.y / 2)).left;
                        var leftColor = c.x == 0 ? -1 : GetTile(tiles, c.x / 2 - 1 + _size * (c.y / 2)).right;
                        if (leftColor == -1 && rightColor == -1)
                            return "  ";
                        if (leftColor == -1 || rightColor == -1)
                            return "██".Color(_colors[leftColor == -1 ? rightColor : leftColor]);
                        return "█".Color(_colors[leftColor]) + "█".Color(_colors[rightColor]);
                    }
                    var bottomColor = c.y >= 2 * _size ? -1 : GetTile(tiles, c.x / 2 + _size * (c.y / 2)).top;
                    var topColor = c.y == 0 ? -1 : GetTile(tiles, c.x / 2 + _size * (c.y / 2 - 1)).bottom;
                    if (topColor == -1 && bottomColor == -1)
                        return "  ";
                    if (topColor == -1 || bottomColor == -1)
                        return "██".Color(_colors[topColor == -1 ? bottomColor : topColor]);
                    return "▀▀".Color(_colors[topColor], _colors[bottomColor]);
                })
                    .Split(2 * _size + 1)
                    .Select(chunk => chunk.JoinColoredString())
                    .JoinColoredString("\n");
    }
}