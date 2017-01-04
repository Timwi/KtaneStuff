using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using RT.Util;
using RT.Util.ExtensionMethods;

namespace KtaneStuff
{
    static partial class Ktane
    {
        public static void BattleshipGraphics()
        {
            var declarations = new List<string>();
            var threads = new List<Thread>();
            foreach (var name in new string[] { "SqWater", "SqShip", "SqShipL", "SqShipR", "SqShipT", "SqShipB", "SqShipF", "SqShipA" })
            {
                var thr = new Thread(() =>
                {
                    File.WriteAllBytes($@"D:\temp\temp-{name}.png", File.ReadAllBytes($@"D:\c\KTANE\Battleship\Assets\Misc\{name}.svg.png"));
                    CommandRunner.RunRaw($@"pngcr D:\c\KTANE\Battleship\Assets\Misc\{name}.svg.png D:\c\KTANE\Battleship\Assets\Misc\{name}.png").Go();
                    lock (declarations)
                        declarations.Add($@"{{ ""{name}"", new byte[] {{ {File.ReadAllBytes($@"D:\c\KTANE\Battleship\Assets\Misc\{name}.png").JoinString(", ")} }} }}");
                });
                thr.Start();
                threads.Add(thr);
            }
            foreach (var th in threads)
                th.Join();

            if (declarations.Count > 0)
                File.WriteAllText(@"D:\c\KTANE\Battleship\Assets\RawPngs.cs", $@"
using System.Collections.Generic;
namespace Battleship {{
    public static class RawPngs {{
        public static Dictionary<string, byte[]> RawBytes = new Dictionary<string, byte[]> {{
            {declarations.JoinString(",\r\n            ")}
        }};
    }}
}}");
        }

        public static void GenerateBattleshipPuzzle()
        {
            var theLog = new List<string>();
            var log = Ut.Lambda((string msg) =>
            {
                Console.WriteLine(msg);
                //File.AppendAllLines(@"D:\temp\temp.txt", new[] { msg });
                theLog.Add(msg);
            });

            var size = 6;
            var rnd = new Random(49);
            var nonUnique = 0;
            var hints = Enumerable.Range(0, size * size).ToList().Shuffle(rnd).Take(rnd.Next(2, 3)).ToArray();
            log($"There are {hints.Length} hints.");

            retry:
            var ships = new[] { rnd.Next(3, 5), rnd.Next(2, 4), rnd.Next(2, 4), rnd.Next(1, 4), rnd.Next(1, 3), 1 }.OrderByDescending(x => x).ToArray();
            log("Ships: " + ships.JoinString(", "));
            var anyHypothesis = false;

            //Console.WriteLine("Retrying.");
            var grid = Ut.NewArray(size, size, (x, y) => (bool?) null);

            var availableShips = ships.ToList();
            while (availableShips.Count > 0)
            {
                var ix = rnd.Next(availableShips.Count);
                var shipLen = availableShips[ix];
                availableShips.RemoveAt(ix);
                var positions = Enumerable.Range(0, size * size * 2).Select(i =>
                {
                    var horiz = (i & 1) == 1;
                    var x = (i >> 1) % size;
                    var y = (i >> 1) / size;
                    if (horiz && x + shipLen >= size || !horiz && y + shipLen >= size)
                        return null;
                    for (int j = 0; j < shipLen; j++)
                        if (grid[horiz ? x + j : x][horiz ? y : y + j] != null)
                            return null;
                    return new { X = x, Y = y, Horiz = horiz };
                }).Where(inf => inf != null).ToArray();
                if (positions.Length == 0)
                {
                    //log("Must retry.");
                    Console.WriteLine("Retrying because no fit.");
                    goto retry;
                }
                var pos = positions.PickRandom(rnd);
                for (int j = -1; j <= shipLen; j++)
                    if ((pos.Horiz ? pos.X + j : pos.Y + j).Apply(ps => ps >= 0 && ps < size))
                    {
                        if ((pos.Horiz ? pos.Y : pos.X) > 0)
                            grid[pos.Horiz ? pos.X + j : pos.X - 1][pos.Horiz ? pos.Y - 1 : pos.Y + j] = false;
                        grid[pos.Horiz ? pos.X + j : pos.X][pos.Horiz ? pos.Y : pos.Y + j] = j >= 0 && j < shipLen;
                        if ((pos.Horiz ? pos.Y : pos.X) < size - 1)
                            grid[pos.Horiz ? pos.X + j : pos.X + 1][pos.Horiz ? pos.Y + 1 : pos.Y + j] = false;
                    }
            }

            var rowCounts = Enumerable.Range(0, size).Select(row => Enumerable.Range(0, size).Count(col => grid[col][row] == true)).ToArray();
            var colCounts = Enumerable.Range(0, size).Select(col => Enumerable.Range(0, size).Count(row => grid[col][row] == true)).ToArray();
            var output = Ut.Lambda(() =>
            {
                log("   " + Enumerable.Range(0, size).Select(col => colCounts[col].ToString().PadLeft(2)).JoinString());
                log(Enumerable.Range(0, size).Select(row => rowCounts[row].ToString().PadLeft(3) + " " + Enumerable.Range(0, size).Select(col => hints.Contains(col + row * size) ? (grid[col][row] == true ? "% " : "• ") : grid[col][row] == null ? "? " : grid[col][row].Value ? "# " : "· ").JoinString()).JoinString("\n"));
            });

            log("Intended solution:");
            output();

            grid = Ut.NewArray(size, size, (x, y) => hints.Contains(x + y * size) ? grid[x][y] ?? false : (bool?) null);

            var rowsDone = new bool[size];
            var colsDone = new bool[size];
            var hypotheses = new[] { new { X = 0, Y = 0, Grid = (bool?[][]) null, RowsDone = (bool[]) null, ColsDone = (bool[]) null } }.ToStack();
            hypotheses.Pop();
            bool?[][] solution = null;

            nextIter:
            if (rowsDone.All(b => b) && colsDone.All(b => b))
                goto tentativeSolution;

            // Diagonal from a true is a false
            for (int c = 0; c < size; c++)
                for (int r = 0; r < size; r++)
                    if (grid[c][r] == true)
                    {
                        if (r > 0 && c > 0)
                            grid[c - 1][r - 1] = false;
                        if (r > 0 && c < size - 1)
                            grid[c + 1][r - 1] = false;
                        if (r < size - 1 && c > 0)
                            grid[c - 1][r + 1] = false;
                        if (r < size - 1 && c < size - 1)
                            grid[c + 1][r + 1] = false;
                    }

            // Check if a row can be filled in unambiguously
            for (int r = 0; r < size; r++)
                if (!rowsDone[r])
                {
                    var cnt = Enumerable.Range(0, size).Count(c => grid[c][r] != false);
                    if (cnt < rowCounts[r])
                    {
                        log($"Contradiction: row {r} has too few available spaces.");
                        goto contradiction;
                    }
                    if (cnt == rowCounts[r])
                    {
                        for (int c = 0; c < size; c++)
                            if (grid[c][r] == null)
                                grid[c][r] = true;
                        rowsDone[r] = true;
                        log($"Deduced row {r}");
                        goto nextIter;
                    }

                    cnt = Enumerable.Range(0, size).Count(c => grid[c][r] == true);
                    if (cnt > rowCounts[r])
                    {
                        log($"Contradiction: row {r} has too many assigned spaces.");
                        goto contradiction;
                    }
                    if (cnt == rowCounts[r])
                    {
                        for (int c = 0; c < size; c++)
                            if (grid[c][r] == null)
                                grid[c][r] = false;
                        rowsDone[r] = true;
                        log($"Deduced row {r}");
                        goto nextIter;
                    }
                }

            // Check if a column can be filled in unambiguously
            for (int c = 0; c < size; c++)
                if (!colsDone[c])
                {
                    var cnt = Enumerable.Range(0, size).Count(r => grid[c][r] != false);
                    if (cnt < colCounts[c])
                    {
                        log($"Contradiction: column {c} has too few available spaces.");
                        goto contradiction;
                    }
                    if (cnt == colCounts[c])
                    {
                        for (int r = 0; r < size; r++)
                            if (grid[c][r] == null)
                                grid[c][r] = true;
                        colsDone[c] = true;
                        log($"Deduced column {c}");
                        goto nextIter;
                    }

                    cnt = Enumerable.Range(0, size).Count(r => grid[c][r] == true);
                    if (cnt > colCounts[c])
                    {
                        log($"Contradiction: column {c} has too many assigned spaces.");
                        goto contradiction;
                    }
                    if (cnt == colCounts[c])
                    {
                        for (int r = 0; r < size; r++)
                            if (grid[c][r] == null)
                                grid[c][r] = false;
                        colsDone[c] = true;
                        log($"Deduced column {c}");
                        goto nextIter;
                    }
                }

            // No obvious deduction: Try a hypothesis
            log("No obvious deduction: trying a hypothesis");
            anyHypothesis = true;
            var unfinishedCol = colsDone.IndexOf(false);
            var unfinishedRow = grid[unfinishedCol].IndexOf((bool?) null);
            hypotheses.Push(new { X = unfinishedCol, Y = unfinishedRow, Grid = Ut.NewArray(size, size, (x, y) => grid[x][y]), RowsDone = (bool[]) rowsDone.Clone(), ColsDone = (bool[]) colsDone.Clone() });
            log($"Hypothesis is X={unfinishedCol} Y={unfinishedRow}, trying ship");
            grid[unfinishedCol][unfinishedRow] = true;
            goto nextIter;

            contradiction:
            if (hypotheses.Count == 0)
            {
                if (solution != null)
                    goto uniqueSolutionFound;
                log("This puzzle is impossible?");
                Debugger.Break();
                throw new InvalidOperationException();
            }
            var prevHypo = hypotheses.Pop();
            // Try the opposite hypothesis
            log($"Backtracking from hypothesis X={prevHypo.X} Y={prevHypo.Y}, trying water");
            grid = prevHypo.Grid;
            rowsDone = prevHypo.RowsDone;
            colsDone = prevHypo.ColsDone;
            grid[prevHypo.X][prevHypo.Y] = false;
            goto nextIter;

            tentativeSolution:
            if (!anyHypothesis)
            {
                log("Retrying because too trivial.");
                goto retry;
            }
            // Found a tentative solution. Check that it’s correct

            // Find unaccounted-for ships
            var unaccountedFor = ships.OrderByDescending(x => x).ToList();
            for (int x = 0; x < size; x++)
                for (int y = 0; y < size; y++)
                {
                    int? thisLen = null;
                    if (grid[x][y] == true && (x == 0 || grid[x - 1][y] == false) && (x == size - 1 || grid[x + 1][y] == false) && (y == 0 || grid[x][y - 1] == false) && (y == size - 1 || grid[x][y + 1] == false))
                        thisLen = 1;
                    if (thisLen == null && grid[x][y] == true && (x == 0 || grid[x - 1][y] == false))
                    {
                        var len = 0;
                        while (x + len < size && grid[x + len][y] == true)
                            len++;
                        if (len > 1 && (x + len == size || grid[x + len][y] == false))
                            thisLen = len;
                    }
                    if (thisLen == null && grid[x][y] == true && (y == 0 || grid[x][y - 1] == false))
                    {
                        var len = 0;
                        while (y + len < size && grid[x][y + len] == true)
                            len++;
                        if (len > 1 && (y + len == size || grid[x][y + len] == false))
                            thisLen = len;
                    }
                    if (thisLen != null)
                    {
                        if (!unaccountedFor.Remove(thisLen.Value))
                        {
                            log($"Too many length {thisLen.Value} ships.");
                            goto contradiction;
                        }
                    }
                }
            if (unaccountedFor.Count > 0)
            {
                log($"Ship lengths {unaccountedFor.JoinString(", ")} unaccounted for.");
                goto contradiction;
            }

            // Actually found a solution
            if (solution != null)
            {
                // The puzzle is not unique.
                log("Solution is not unique:");
                output();
                nonUnique++;
                goto retry;
            }

            log("Found a solution:");
            output();
            solution = Ut.NewArray(size, size, (i, j) => grid[i][j]);
            goto contradiction;

            uniqueSolutionFound:
            log("Unique solution found.");
            grid = solution;
            output();
            log("Non-unique puzzles I had to discard: " + nonUnique);
            File.WriteAllLines(@"D:\temp\temp.txt", theLog);

            grid = Ut.NewArray(size, size, (x, y) => hints.Contains(x + y * size) ? grid[x][y] ?? false : (bool?) null);
            Console.WriteLine();
            Console.WriteLine("PUZZLE FOR YOU:");
            Console.WriteLine("   " + Enumerable.Range(0, size).Select(col => colCounts[col].ToString().PadLeft(2)).JoinString());
            Console.WriteLine(Enumerable.Range(0, size).Select(row => rowCounts[row].ToString().PadLeft(3) + " " + Enumerable.Range(0, size).Select(col => hints.Contains(col + row * size) ? (grid[col][row] == true ? "% " : "• ") : grid[col][row] == null ? "? " : grid[col][row].Value ? "# " : "· ").JoinString()).JoinString("\n"));
            Console.WriteLine();
            Console.WriteLine(ships.JoinString(", "));
        }
    }
}
