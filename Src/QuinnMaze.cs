using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using RT.Serialization;
using RT.TagSoup;
using RT.Util;
using RT.Util.Consoles;
using RT.Util.ExtensionMethods;

namespace KtaneStuff
{
    static class QuinnMaze
    {
        public static void Experiment()
        {
            var c = 0;
            foreach (var solution in FindMazes(new int?[_w * _h], new List<(int cell, int nonwallMask)[]>(), new[] { 9, 1, 3, 8, 0, 2, 12, 4, 6 }))
            {
                Console.WriteLine(c);
                Console.WriteLine(solution.Split(_w).Select(row => row.Select(e => "    ^  ── └─ v  │  ┌─ ├─── ─┘ ────┴──┐ ─┤ ─┬──┼─".Substring(3 * ((~e) & 0xf), 3)).JoinString()).JoinString("\n"));
                c++;
            }
            Console.WriteLine($"Found {c} mazes.");
        }

        private static string _debug_output(int[] grid, int? w = null) =>
            _debug_output(grid.SelectNullable().ToArray(), w);
        private static string _debug_output(int?[] grid, int? w = null) =>
            grid.Split(w ?? _w).Select(row => row.Select(e => e == null ? " ? " : "    ^  ── └─ v  │  ┌─ ├─── ─┘ ────┴──┐ ─┤ ─┬──┼─".Substring(3 * ((~e.Value) & 0xf), 3)).JoinString()).JoinString("\n");

        private const int _w = 3;
        private const int _h = 3;

        private static IEnumerable<int[]> FindMazes(int?[] sofar, List<(int cell, int nonwallMask)[]> regions, int[] outerWalls)
        {
            var ix = -1;
            int[] possibilities = null;

            for (var i = 0; i < sofar.Length; i++)
            {
                if (sofar[i] != null)
                    continue;
                var c = new Coord(_w, _h, i);
                var poss = Enumerable.Range(0, 1 << 4).Where(walls =>
                {
                    if (walls == 0 || sofar.Contains(walls))
                        return false;
                    for (var dir = 0; dir < 4; dir++)
                        if (c.Neighbor((GridDirection) (2 * dir)) is Coord nb ? (sofar[nb.Index] != null && ((sofar[nb.Index].Value & (1 << ((dir + 2) % 4))) != 0) != ((walls & (1 << dir)) != 0)) : ((walls & (1 << dir)) != (outerWalls[i] & (1 << dir))))
                            return false;
                    foreach (var reg in regions)
                        if (reg.Length == 1 && reg[0].cell == i && (reg[0].nonwallMask & walls) != 0)
                            return false;
                    return true;
                }).ToArray();
                if (poss.Length == 0)
                    yield break;
                if (possibilities == null || poss.Length < possibilities.Length)
                {
                    ix = i;
                    possibilities = poss;
                    if (poss.Length == 1)
                        goto shortcut;
                }
            }

            if (ix == -1)
                throw new InvalidOperationException();

            shortcut:
            var cell = new Coord(_w, _h, ix);
            foreach (var walls in possibilities)
            {
                sofar[ix] = walls;
                var newRegions = new List<(int cell, int nonwallMask)[]>();
                var myRegion = Enumerable.Range(0, 4)
                    .Where(dir => (walls & (1 << dir)) == 0 && cell.Neighbor((GridDirection) (2 * dir)) is Coord neigh && sofar[neigh.Index] == null)
                    .Select(dir => (cell: cell.Neighbor((GridDirection) (2 * dir)).Value.Index, nonwallMask: 1 << ((dir + 2) % 4)))
                    .ToList();
                foreach (var region in regions)
                {
                    if (!(region.FirstOrNull(tup => tup.cell == ix) is (int rc, int nonwallMask)))
                        newRegions.Add(region);
                    else if ((walls & nonwallMask) != 0)
                        newRegions.Add(region.Where(tup => tup.cell != ix).ToArray());
                    else
                    {
                        foreach (var tup in region)
                            if (tup.cell != ix)
                            {
                                var p = myRegion.IndexOf(t => t.cell == tup.cell);
                                if (p == -1)
                                    myRegion.Add(tup);
                                else
                                    myRegion[p] = (tup.cell, tup.nonwallMask | myRegion[p].nonwallMask);
                            }
                    }
                }
                if (myRegion.Count == 0)
                {
                    if (!sofar.Any(s => s == null))
                        yield return sofar.Select(s => s.Value).ToArray();
                    continue;
                }
                newRegions.Add(myRegion.ToArray());
                foreach (var solution in FindMazes(sofar, newRegions, outerWalls))
                    yield return solution;
            }
            sofar[ix] = null;
        }

        public static void SudokuExperiment()
        {
            Console.WriteLine("Finding Sudoku...");
            var rnd = new Random(47);
            //var randomSudoku = FindSudokuMazes(new int?[9 * 9], new List<(int cell, int nonwallMask)[]>(), new (int mask, int value)[9 * 9], rnd).FirstOrDefault();
            //ClassifyJson.SerializeToFile(randomSudoku, @"D:\temp\temp.json");
            var randomSudoku = ClassifyJson.DeserializeFile<int[]>(@"D:\temp\temp.json");

            Console.WriteLine("Creating array...");
            var allValues = Enumerable.Range(0, 9 * 9 * 2).Select(i => (cell: i / 2, mask: 1 << (i % 2), value: randomSudoku[i / 2] & (1 << (i % 2)))).ToArray();

            var lines = new StringBuilder();
            for (var x = 0; x <= 9; x++)
                for (var y = 0; y <= 9; y++)
                {
                    if (x < 9 && ((randomSudoku[x + 9 * (y % 9)] & 1) != 0))
                        lines.Append($"<path d='M{x} {y}h1' />");
                    if (y < 9 && ((randomSudoku[(x % 9) + 9 * y] & (1 << 3)) != 0))
                        lines.Append($"<path d='M{x} {y}v1' />");
                }
            var frame = $"<path stroke-width='.1' stroke='#ccc' d='M0 0h9v9H0zM3 0v9M6 0v9M0 3h9M0 6h9' />";
            var frameThin = $"<path stroke-width='.05' stroke='#ccc' d='{Enumerable.Range(0, 9).Where(i => i % 3 != 0).Select(i => $"M{i} 0v9M0 {i}h9").JoinString()}' />";
            var svg = $"<svg xmlns='http://www.w3.org/2000/svg' viewBox='-1 -1 11 11' fill='none' stroke-width='.05' stroke='black' stroke-linecap='round'>{frame}{frameThin}{lines}</svg>";
            File.WriteAllText(@"D:\Daten\Puzzles\Logic puzzles\Sudoku Maze (solution).svg", svg);

            var isGiven = new int[9 * 9];
            var givenValues = new int[9 * 9];
            foreach (var givenIx in new[] { 61, 131, 130, 143, 57, 135, 36, 79, 26, 14, 91, 101, 53, 11, 64, 96, 10, 152, 125, 139, 27, 55, 80, 47, 40, 142, 31, 124, 104, 85, 84, 20, 37, 111, 4, 63, 66, 136, 0 })
            {
                Console.WriteLine($"{allValues[givenIx].mask} = {allValues[givenIx].value}");
                isGiven[allValues[givenIx].cell] |= allValues[givenIx].mask;
                givenValues[allValues[givenIx].cell] |= allValues[givenIx].value;
            }
            var grayLines = new StringBuilder();
            var blackLines = new StringBuilder();
            for (var x = 0; x <= 9; x++)
                for (var y = 0; y <= 9; y++)
                {
                    var ix = (x % 9) + 9 * (y % 9);
                    var ixr = ((x + 8) % 9) + 9 * (y % 9);
                    if (x < 9 && ((isGiven[ix] & 1) == 0 || (givenValues[ix] & 1) != 0))
                        ((isGiven[ix] & 1) != 0 ? blackLines : grayLines).Append($"<path d='M{x} {y}h1' stroke-width='{(y % 3 == 0 ? ".1" : ".05")}' stroke='{((isGiven[ix] & 1) != 0 ? "black" : "#ccc")}' />");
                    if (y < 9 && ((isGiven[ixr] & 2) == 0 || (givenValues[ixr] & 2) != 0))
                        ((isGiven[ixr] & 2) != 0 ? blackLines : grayLines).Append($"<path d='M{x} {y}v1' stroke-width='{(x % 3 == 0 ? ".1" : ".05")}' stroke='{((isGiven[ixr] & 2) != 0 ? "black" : "#ccc")}' />");
                }
            svg = $"<svg xmlns='http://www.w3.org/2000/svg' viewBox='-1 -1 11 11' fill='none' stroke-width='.05' stroke='black' stroke-linecap='round'>{grayLines}{blackLines}</svg>";
            File.WriteAllText(@"D:\Daten\Puzzles\Logic puzzles\Sudoku Maze (puzzle).svg", svg);
            //GenerateGivens(rnd, allValues);
        }

        private static void GenerateGivens(Random rnd, (int cell, int mask, int value)[] allValues)
        {
            Console.WriteLine("Generating givens...");
            var givensIxs = Ut.ReduceRequiredSet(Enumerable.Range(0, allValues.Length).ToArray().Shuffle(rnd), skipConsistencyTest: true, test: state =>
            {
                Console.WriteLine(Enumerable.Range(0, allValues.Length).Select(i => state.SetToTest.Contains(i) ? "█" : "░").JoinString());
                var givens = new (int mask, int value)[9 * 9];
                foreach (var ix in state.SetToTest)
                {
                    var (cell, mask, value) = allValues[ix];
                    givens[cell] = (mask: givens[cell].mask | mask, value: givens[cell].value | value);
                }
                return !FindSudokuMazes(new int?[9 * 9], new List<(int cell, int nonwallMask)[]>(), givens).Skip(1).Any();
            });

            ConsoleUtil.WriteParagraphs(givensIxs.JoinString(", "));
            File.WriteAllText(@"D:\temp\sudoku-maze-output.txt", givensIxs.JoinString(", "));
            Console.ReadLine();
        }

        private static IEnumerable<int[]> FindSudokuMazes(int?[] sofar, List<(int cell, int nonwallMask)[]> areas, (int mask, int value)[] givens, Random rnd = null)
        {
            var ix = -1;
            int[] possibilities = null;

            for (var i = 0; i < sofar.Length; i++)
            {
                if (sofar[i] != null)
                    continue;
                var c = new Coord(9, 9, i);
                var poss = Enumerable.Range(0, 1 << 4).Where(walls =>
                {
                    if ((walls & givens[i].mask) != givens[i].value)
                        return false;
                    if (Enumerable.Range(0, 9).Any(x => sofar[x + 9 * (i / 9)] == walls) ||
                        Enumerable.Range(0, 9).Any(y => sofar[(i % 9) + 9 * y] == walls) ||
                        Enumerable.Range(0, 9).Any(c => sofar[3 * ((i % 9) / 3) + c % 3 + 9 * (3 * ((i / 9) / 3) + c / 3)] == walls))
                        return false;
                    for (var dir = 0; dir < 4; dir++)
                    {
                        var nb = c.NeighborWrap((GridDirection) (2 * dir));
                        if ((sofar[nb.Index] != null && ((sofar[nb.Index].Value & (1 << ((dir + 2) % 4))) != 0) != ((walls & (1 << dir)) != 0)))
                            return false;
                    }
                    foreach (var area in areas)
                        if (area.Length == 1 && area[0].cell == i && (area[0].nonwallMask & walls) != 0)
                            return false;
                    return true;
                }).ToArray();
                if (poss.Length == 0)
                    yield break;
                if (possibilities == null || poss.Length < possibilities.Length)
                {
                    ix = i;
                    possibilities = poss;
                    if (poss.Length == 1)
                        goto shortcut;
                }
            }

            if (ix == -1)
                throw new InvalidOperationException();

            shortcut:
            var cell = new Coord(9, 9, ix);
            var offset = rnd == null ? 0 : rnd.Next(0, possibilities.Length);
            for (var possIx = 0; possIx < possibilities.Length; possIx++)
            {
                var walls = possibilities[(possIx + offset) % possibilities.Length];
                sofar[ix] = walls;
                var newAreas = new List<(int cell, int nonwallMask)[]>();
                var myArea = Enumerable.Range(0, 4)
                    .Where(dir => (walls & (1 << dir)) == 0 && cell.Neighbor((GridDirection) (2 * dir)) is Coord neigh && sofar[neigh.Index] == null)
                    .Select(dir => (cell: cell.Neighbor((GridDirection) (2 * dir)).Value.Index, nonwallMask: 1 << ((dir + 2) % 4)))
                    .ToList();
                foreach (var area in areas)
                {
                    if (!(area.FirstOrNull(tup => tup.cell == ix) is (int rc, int nonwallMask)))
                        newAreas.Add(area);
                    else if ((walls & nonwallMask) != 0)
                        newAreas.Add(area.Where(tup => tup.cell != ix).ToArray());
                    else
                    {
                        foreach (var tup in area)
                            if (tup.cell != ix)
                            {
                                var p = myArea.IndexOf(t => t.cell == tup.cell);
                                if (p == -1)
                                    myArea.Add(tup);
                                else
                                    myArea[p] = (tup.cell, tup.nonwallMask | myArea[p].nonwallMask);
                            }
                    }
                }
                if (myArea.Count == 0)
                {
                    if (!sofar.Any(s => s == null))
                        yield return sofar.Select(s => s.Value).ToArray();
                    continue;
                }
                newAreas.Add(myArea.ToArray());
                foreach (var solution in FindSudokuMazes(sofar, newAreas, givens, rnd))
                    yield return solution;
            }
            sofar[ix] = null;
        }
    }
}