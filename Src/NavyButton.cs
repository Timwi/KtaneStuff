using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BlueButtonLib;
using RT.Util;
using RT.Util.Consoles;
using RT.Util.ExtensionMethods;
using RT.Util.Text;
using Words;

namespace KtaneStuff
{
    static class NavyButton
    {
        private const int _sz = 4;

        public static void GenerateLatinSquaresAndStencils()
        {
            var allLatinSquares = FindSolutions(new int?[NavyButtonPuzzle._sz * NavyButtonPuzzle._sz], null, null).ToList();
            Console.WriteLine(allLatinSquares.Count);

            (int sm, int la, bool big)[] generateConstraints(int[] grid)
            {
                var upConstraints = Enumerable.Range(0, _sz * _sz).Where(ix => ix / _sz > 0 && grid[ix - _sz] < grid[ix]).Select(ix => (sm: ix - _sz, la: ix, false));
                var rightConstraints = Enumerable.Range(0, _sz * _sz).Where(ix => ix % _sz < _sz - 1 && grid[ix + 1] < grid[ix]).Select(ix => (sm: ix + 1, la: ix, false));
                var downConstraints = Enumerable.Range(0, _sz * _sz).Where(ix => ix / _sz < _sz - 1 && grid[ix + _sz] < grid[ix]).Select(ix => (sm: ix + _sz, la: ix, false));
                var leftConstraints = Enumerable.Range(0, _sz * _sz).Where(ix => ix % _sz > 0 && grid[ix - 1] < grid[ix]).Select(ix => (sm: ix - 1, la: ix, false));

                var upConstraints2 = Enumerable.Range(0, _sz * _sz).Where(ix => ix / _sz > 0 && grid[ix - _sz] < grid[ix] - 1).Select(ix => (sm: ix - _sz, la: ix, true));
                var rightConstraints2 = Enumerable.Range(0, _sz * _sz).Where(ix => ix % _sz < _sz - 1 && grid[ix + 1] < grid[ix] - 1).Select(ix => (sm: ix + 1, la: ix, true));
                var downConstraints2 = Enumerable.Range(0, _sz * _sz).Where(ix => ix / _sz < _sz - 1 && grid[ix + _sz] < grid[ix] - 1).Select(ix => (sm: ix + _sz, la: ix, true));
                var leftConstraints2 = Enumerable.Range(0, _sz * _sz).Where(ix => ix % _sz > 0 && grid[ix - 1] < grid[ix] - 1).Select(ix => (sm: ix - 1, la: ix, true));

                return upConstraints.Concat(rightConstraints).Concat(downConstraints).Concat(leftConstraints)
                    .Concat(upConstraints2).Concat(rightConstraints2).Concat(downConstraints2).Concat(leftConstraints2)
                    .ToArray();
            }

            bool evaluateConstraint(int[] grid, (int sm, int la, bool big) constraint) => constraint.big ? grid[constraint.sm] < grid[constraint.la] - 1 : grid[constraint.sm] < grid[constraint.la];

            allLatinSquares.RemoveAll(lsq =>
            {
                var constraints = generateConstraints(lsq);
                var num = allLatinSquares.Count(lsq2 => constraints.All(constr => evaluateConstraint(lsq2, constr)));
                return num > 1;
            });
            Console.WriteLine(allLatinSquares.Count);

            Utils.ReplaceInFile(@"D:\c\KTANE\BunchOfButtons\Lib\NavyButtonPuzzle.cs", "/*LSQstart*/", "/*LSQend*/",
                $@"""{allLatinSquares.Select(ia => ia.JoinString()).JoinString(",")}""");
            var allWords = new Data().allWords[0].Concat(NavyButtonPuzzle._words4).ToHashSet();
            Utils.ReplaceInFile(@"D:\c\KTANE\BunchOfButtons\Lib\NavyButtonPuzzle.cs", "/*stencils-start*/", "/*stencils-end*/",
                FindRequiredStencilsForSeed(2, allLatinSquares, allWords).Select(ta => $"new[] {{ {ta.Select(tup => $"({tup.dx}, {tup.dy})").JoinString(", ")} }}").JoinString(", "));
        }

        private static IEnumerable<int[]> FindSolutions(int?[] sofar, (int sm, int la)[] constraints, Random rnd)
        {
            var ix = -1;
            int[] best = null;
            for (var i = 0; i < sofar.Length; i++)
            {
                var x = i % _sz;
                var y = i / _sz;
                if (sofar[i] != null)
                    continue;
                var taken = new bool[_sz];
                // Same row
                for (var c = 0; c < _sz; c++)
                    if (sofar[c + _sz * y] is int v)
                        taken[v] = true;
                // Same column
                for (var r = 0; r < _sz; r++)
                    if (sofar[x + _sz * r] is int v)
                        taken[v] = true;
                // Constraints
                if (constraints != null)
                {
                    foreach (var (sm, la) in constraints)
                    {
                        if (i == sm && sofar[la] != null) // i is the cell with the smaller value, so it can’t be anything larger than la
                            for (var ov = sofar[la].Value; ov < _sz; ov++)
                                taken[ov] = true;
                        else if (i == la && sofar[sm] != null)  // i is the cell with the larger value, so it can’t be anything smaller than sm
                            for (var ov = sofar[sm].Value; ov >= 0; ov--)
                                taken[ov] = true;
                    }
                }
                var values = taken.SelectIndexWhere(b => !b).ToArray();
                if (values.Length == 0)
                    yield break;
                if (best == null || values.Length < best.Length)
                {
                    ix = i;
                    best = values;
                    if (values.Length == 1)
                        goto shortcut;
                }
            }

            if (ix == -1)
            {
                yield return sofar.Select(i => i.Value).ToArray();
                yield break;
            }

            shortcut:
            var offset = rnd == null ? 0 : rnd.Next(0, best.Length);
            for (var i = 0; i < best.Length; i++)
            {
                var value = best[(i + offset) % best.Length];
                sofar[ix] = value;
                foreach (var solution in FindSolutions(sofar, constraints, rnd))
                    yield return solution;
            }
            sofar[ix] = null;
        }

        public static void GeneratePuzzle()
        {
            var puzzle = NavyButtonPuzzle.Create(1);
            var tt = new TextTable { ColumnSpacing = 1 };
            for (var i = 0; i < 7 * 7; i++)
                tt.SetCell(i % 7, i / 7, " ");
            for (var i = 0; i < 16; i++)
                tt.SetCell(2 * (i % 4), 2 * (i / 4), "·");
            foreach (var (sm, la) in puzzle.Constraints)
            {
                var x = sm % 4 + la % 4;
                var y = sm / 4 + la / 4;
                tt.SetCell(x, y, sm % 4 < la % 4 ? "<" : sm % 4 > la % 4 ? ">" : sm / 4 < la / 4 ? "∧" : "∨");
            }
            tt.WriteToConsole();
        }

        public static void Stencil_FindPossibleWords()
        {
            var allLatinSquares = FindSolutions(new int?[NavyButtonPuzzle._sz * NavyButtonPuzzle._sz], null, null).ToArray();

            int numBits(int n) => n == 0 ? 0 : 1 + numBits(n & (n - 1));
            var directions = Enumerable.Range(0, 9).Select(i => (dx: i % 3 - 1, dy: i / 3 - 1)).Where(tup => tup.dx != 0 || tup.dy != 0).ToArray();
            var allStencils = Enumerable.Range(0, 1 << 8).Where(n => numBits(n) == 3).Select(bits => Enumerable.Range(0, 8).Where(bit => (bits & (1 << bit)) != 0).Select(bit => directions[bit]).ToArray()).ToArray();

            var validWords = new HashSet<string>();
            for (var lsqIx = 0; lsqIx < allLatinSquares.Length; lsqIx++)
            {
                Console.WriteLine($"LSq {lsqIx}/{allLatinSquares.Length}");
                var latinSquare = allLatinSquares[lsqIx];
                var threes = latinSquare.SelectIndexWhere(val => val == 3);
                var letters = threes.Select(ix => allStencils.Select(stencil =>
                {
                    var x = ix % 4;
                    var y = ix / 4;
                    var values = stencil.Select(tup => latinSquare[(x + tup.dx + 4) % 4 + 4 * ((y + tup.dy + 4) % 4)]);
                    return values.Contains(3) ? null : (int?) values.Aggregate(0, (p, n) => 3 * p + n);
                }).Where(i => i != null && i != 0).Select(i => (char) ('A' + i.Value - 1)).Distinct().Order().JoinString()).ToArray();

                validWords.AddRange(
                    from a in letters[0]
                    from b in letters[1]
                    from c in letters[2]
                    from d in letters[3]
                    select $"{a}{b}{c}{d}");
            }

            var invalidWords = (from a in Enumerable.Range('A', 26)
                                from b in Enumerable.Range('A', 26)
                                from c in Enumerable.Range('A', 26)
                                from d in Enumerable.Range('A', 26)
                                select $"{(char) a}{(char) b}{(char) c}{(char) d}").Except(validWords).ToArray();
            Console.WriteLine(invalidWords.JoinString("\n"));
        }

        public static void Stencil_ReduceRequired()
        {
            var allLatinSquares = NavyButtonPuzzle._latinSquares;

            var allWords = new Data().allWords[0].Concat(NavyButtonPuzzle._words4).ToHashSet();
            var currentBest = int.MaxValue;
            var lockObj = new object();

            Console.Clear();
            Enumerable.Range(0, 1000).ParallelForEach(Environment.ProcessorCount, (seed, proc) =>
            {
                lock (lockObj)
                {
                    Console.CursorLeft = 0;
                    Console.CursorTop = proc;
                    Console.WriteLine($"Proc {proc}: seed {seed}");
                }
                var requiredStencils = FindRequiredStencilsForSeed(seed, allLatinSquares, allWords);

                lock (lockObj)
                {
                    var req = requiredStencils.Count();
                    if (req < currentBest)
                    {
                        Console.CursorLeft = 0;
                        Console.CursorTop = Environment.ProcessorCount + 2;
                        Console.WriteLine($"Seed {seed} requires only {req}  ");
                        currentBest = req;
                    }
                }
            });
        }

        public static void List21Stencils()
        {
            var allLatinSquares = FindSolutions(new int?[NavyButtonPuzzle._sz * NavyButtonPuzzle._sz], null, null).ToArray();
            var allWords = new Data().allWords[0].Concat(NavyButtonPuzzle._words4).ToHashSet();
            var stencils = FindRequiredStencilsForSeed(2, allLatinSquares, allWords).OrderBy(tup => tup[0].dy).ThenBy(tup => tup[0].dx).ToArray();
            foreach (var stencil in stencils)
            {
                Console.WriteLine(Enumerable.Range(0, 3).Select(row => Enumerable.Range(0, 3).Select(col => col == 1 && row == 1 ? "░░" : stencil.Contains((col - 1, row - 1)) ? "██" : "  ").JoinString()).JoinString("\n"));
                Console.WriteLine();
            }
            Console.WriteLine(stencils.Length);
        }

        private static (int dx, int dy)[][] FindRequiredStencilsForSeed(int seed, IEnumerable<int[]> allLatinSquares, HashSet<string> allWords)
        {
            int numBits(int n) => n == 0 ? 0 : 1 + numBits(n & (n - 1));
            var directions = Enumerable.Range(0, 9).Select(i => (dx: i % 3 - 1, dy: i / 3 - 1)).Where(tup => tup.dx != 0 || tup.dy != 0).ToArray();
            var stencils = Enumerable.Range(0, 1 << 8).Where(n => /*!new[] { 69, 49, 162, 140 }.Contains(n) &&*/ numBits(n) == 3).Select(bits => Enumerable.Range(0, 8).Where(bit => (bits & (1 << bit)) != 0).Select(bit => directions[bit]).ToArray()).ToArray();
            var rnd = new Random(seed);

            var requiredStencils = Ut.ReduceRequiredSet(Enumerable.Range(0, stencils.Length).ToArray().Shuffle(rnd), test: state =>
            {
                var words = allWords.ToHashSet();
                var validWords = new HashSet<string>();
                foreach (var latinSquare in allLatinSquares)
                {
                    var threes = latinSquare.SelectIndexWhere(val => val == 3);
                    var letters = threes.Select(ix => state.SetToTest.Select(stencilIx =>
                    {
                        var x = ix % 4;
                        var y = ix / 4;
                        var values = stencils[stencilIx].Select(tup => latinSquare[(x + tup.dx + 4) % 4 + 4 * ((y + tup.dy + 4) % 4)]);
                        return values.Contains(3) ? null : (int?) values.Aggregate(0, (p, n) => 3 * p + n);
                    }).Where(i => i != null && i != 0).Select(i => (char) ('A' + i.Value - 1)).Distinct().Order().JoinString()).ToArray();

                    foreach (var word in words.ToArray())
                        if (Enumerable.Range(0, 4).All(ix => letters[ix].Contains(word[ix])))
                        {
                            validWords.Add(word);
                            words.Remove(word);
                        }
                }
                return validWords.Count == allWords.Count;
            });
            return requiredStencils.Select(req => stencils[req]).ToArray();
        }
    }
}