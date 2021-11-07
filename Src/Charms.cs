using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using RT.Util;
using RT.Util.Consoles;
using RT.Util.ExtensionMethods;

namespace KtaneStuff
{
    static class Charms
    {
        private const int _w = 10;
        private const int _h = 10;
        private const int _numColors = 5;

        private static Polyomino[] GetAllPolyominoes()
        {
            var basePolyominoes = Ut.NewArray(
                // domino
                "##",

                // triominoes
                "###",
                "##,#"

            //// tetrominoes
            //"####",     // I
            //"##,##",    // O
            //"###,#",    // L
            //"##,.##",   // S
            //"###,.#"   // T

            //// pentominoes
            //".##,##,.#",    // F
            //"#####",        // I
            //"####,#",       // L
            //"##,.###",      // N
            //"##,###",       // P
            //"###,.#,.#",    // T
            //"###,#.#",      // U
            //"###,#,#",      // V
            //".##,##,#",     // W
            //".#,###,.#",    // X
            //"####,.#",      // Y
            //"##,.#,.##"     // Z
            );

            return basePolyominoes
                .Select(p => new Polyomino(p))
                .SelectMany(p => new[] { p, p.RotateClockwise(), p.RotateClockwise().RotateClockwise(), p.RotateClockwise().RotateClockwise().RotateClockwise() })
                .SelectMany(p => new[] { p, p.Reflect() })
                .Distinct()
                .Where(poly => !poly.Cells.Any(c => c.X >= _w || c.Y >= _h)
                    && !poly.Cells.Any(c =>
                        (!poly.Has(c.X + 1, c.Y) && poly.Has((c.X + 1) % _w, c.Y)) ||
                        (!poly.Has(c.X - 1, c.Y) && poly.Has((c.X + _w - 1) % _w, c.Y)) ||
                        (!poly.Has(c.X, c.Y + 1) && poly.Has(c.X, (c.Y + 1) % _h)) ||
                        (!poly.Has(c.X, c.Y - 1) && poly.Has(c.X, (c.Y + _h - 1) % _h))))
                .ToArray();
        }

        private static PolyominoPlacement[] GetAllPolyominoPlacements() =>
            (from poly in GetAllPolyominoes() from place in Enumerable.Range(0, _w * _h) select new PolyominoPlacement(poly, new Coord(_w, _h, place))).ToArray();

        public static void GenerateDiagram()
        {
            for (var seed = 47; seed < 48; seed++)
            {
                Console.WriteLine($"Trying seed: {seed}");
                var rnd = new Random(seed);
                var polys = GetAllPolyominoPlacements().Shuffle(rnd);


                Console.WriteLine("Start polyomino placement solver");
                var c = 0;
                foreach (var solution in polyominoRecurse(new int[_w * _h], 0, polys, rnd))
                {
                    Console.WriteLine($"Examining solution #{c}");
                    OutputGrid(solution);
                    Console.WriteLine();
                    var numPolyominoes = solution.Max() + 1;
                    foreach (var coloration in colorationRecurse(new int?[numPolyominoes], solution))
                    {
                        OutputGrid(solution, coloration);
                        Debugger.Break();
                    }
                    c++;
                }
            }
        }

        private static void OutputGrid(int[] solution)
        {
            for (var y = 0; y < _h; y++)
                ConsoleUtil.WriteLine(Enumerable.Range(0, _w).Select(x => "██".Color((ConsoleColor) (solution[x + _w * y] % 15 + 1))).JoinColoredString());
        }

        private static void OutputGrid(int[] solution, int[] coloration)
        {
            for (var y = 0; y < _h; y++)
                ConsoleUtil.WriteLine(Enumerable.Range(0, _w).Select(x => "██".Color((ConsoleColor) (coloration[solution[x + _w * y]] + 1))).JoinColoredString());
        }

        private static IEnumerable<int[]> polyominoRecurse(int[] sofar, int polyIx, PolyominoPlacement[] possible, Random rnd)
        {
            for (var x = 0; x < _w; x++)
            {
                var hs = new HashSet<int>();
                var n = 0;
                for (var y = 0; y < _h; y++)
                    if (sofar[x + _w * y] == 0 || hs.Add(sofar[x + _w * y]))
                        n++;
                if (n < _numColors)
                    yield break;
            }
            for (var y = 0; y < _h; y++)
            {
                var hs = new HashSet<int>();
                var n = 0;
                for (var x = 0; x < _w; x++)
                    if (sofar[x + _w * y] == 0 || hs.Add(sofar[x + _w * y]))
                        n++;
                if (n < _numColors)
                    yield break;
            }

            var ix = -1;
            PolyominoPlacement[] poss = null;
            for (var i = 0; i < _w * _h; i++)
            {
                if (sofar[i] > 0)
                    continue;

                var ps = possible.Where(p => p.Covers(new Coord(_w, _h, i))).ToArray();
                if (ps.Length == 0)
                    yield break;
                if (poss == null || poss.Length > ps.Length)
                {
                    ix = i;
                    poss = ps;
                    if (ps.Length == 1)
                        goto shortcut;
                }
            }

            if (ix == -1)
            {
                yield return sofar.Select(i => i - 1).ToArray();
                yield break;
            }

            shortcut:

            var offset = rnd.Next(0, poss.Length);
            for (var candidateIx = 0; candidateIx < poss.Length; candidateIx++)
            {
                var candidate = poss[(candidateIx + offset) % poss.Length];
                foreach (var cell in candidate.Polyomino.Cells)
                    sofar[candidate.Place.AddWrap(cell).Index] = polyIx;
                var newPossible = possible
                    .Where(p => p.Polyomino.Cells.All(c => sofar[p.Place.AddWrap(c).Index] == 0))
                    .ToArray();

                foreach (var solution in polyominoRecurse(sofar, polyIx + 1, newPossible, rnd))
                    yield return solution;

                foreach (var cell in candidate.Polyomino.Cells)
                    sofar[candidate.Place.AddWrap(cell).Index] = 0;
            }
        }

        private static IEnumerable<int[]> colorationRecurse(int?[] sofar, int[] grid)
        {
            var polyIx = -1;
            List<int> possibleColors = null;
            for (var pIx = 0; pIx < sofar.Length; pIx++)
            {
                if (sofar[pIx] != null)
                    continue;

                var poss = Enumerable.Range(0, _numColors).ToList();

                var colsConsidered = new bool[_w];
                var rowsConsidered = new bool[_h];
                foreach (var cell in Coord.Cells(_w, _h))
                    if (grid[cell.Index] == pIx)
                    {
                        // Can’t have the same color touching orthogonally
                        poss.RemoveAll(color => cell.OrthogonalNeighborsWrap.Any(n => sofar[grid[n.Index]] == color));

                        // Do not re-use a color in a column if that column does not have enough polyominoes in it to fill the rest
                        if (!colsConsidered[cell.X])
                        {
                            colsConsidered[cell.X] = true;
                            var colorsAlreadyInColumn = Enumerable.Range(0, _h).Where(y => sofar[grid[cell.X + _w * y]] != null).Select(y => sofar[grid[cell.X + _w * y]].Value).Distinct().ToArray();
                            var unassignedPolysInColumn = Enumerable.Range(0, _h).Where(y => sofar[grid[cell.X + _w * y]] == null).Select(y => grid[cell.X + _w * y]).Distinct().Count();
                            if (colorsAlreadyInColumn.Length + unassignedPolysInColumn < _numColors)
                                yield break;
                            if (colorsAlreadyInColumn.Length + unassignedPolysInColumn == _numColors)
                                poss.RemoveRange(colorsAlreadyInColumn);
                        }

                        // Do not re-use a color in a row if that row does not have enough polyominoes in it to fill the rest
                        if (!rowsConsidered[cell.Y])
                        {
                            rowsConsidered[cell.Y] = true;
                            var colorsAlreadyInRow = Enumerable.Range(0, _w).Where(x => sofar[grid[x + _w * cell.Y]] != null).Select(x => sofar[grid[x + _w * cell.Y]].Value).Distinct().ToArray();
                            var unassignedPolysInRow = Enumerable.Range(0, _w).Where(x => sofar[grid[x + _w * cell.Y]] == null).Select(x => grid[x + _w * cell.Y]).Distinct().Count();
                            if (colorsAlreadyInRow.Length + unassignedPolysInRow < _numColors)
                                yield break;
                            if (colorsAlreadyInRow.Length + unassignedPolysInRow == _numColors)
                                poss.RemoveRange(colorsAlreadyInRow);
                        }
                    }

                if (poss.Count == 0)
                    yield break;
                if (possibleColors == null || possibleColors.Count > poss.Count)
                {
                    polyIx = pIx;
                    possibleColors = poss;
                    if (poss.Count == 1)
                        goto shortcut;
                }
            }

            if (polyIx == -1)
            {
                yield return sofar.Select(i => i.Value).ToArray();
                yield break;
            }

            shortcut:
            foreach (var color in possibleColors)
            {
                sofar[polyIx] = color;
                foreach (var solution in colorationRecurse(sofar, grid))
                    yield return solution;
            }
            sofar[polyIx] = null;
        }
    }
}