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
        public static void Experiment()
        {
            var rnd = new Random(547);
            var puzzle = NavyButtonPuzzle.Create(rnd);
            static int sqr(int v) => v * v;
            static int dist((int sm, int la) p1, (int sm, int la) p2)
            {
                var (sm1, la1) = p1;
                var x1 = (sm1 % 4) + (la1 % 4);
                var y1 = (sm1 / 4) + (la1 / 4);
                var (sm2, la2) = p2;
                var x2 = (sm2 % 4) + (la2 % 4);
                var y2 = (sm2 / 4) + (la2 / 4);
                return (sqr(x2 - x1) + sqr(y2 - y1)) / 2;
            }

            void outputTable(Func<int, int, ConsoleColoredString> cell)
            {
                var tt = new TextTable { ColumnSpacing = 1 };
                for (var x = 0; x < puzzle.Constraints.Length; x++)
                    tt.SetCell(x + 1, 0, (x + 1).ToString().Color(ConsoleColor.White), alignment: HorizontalTextAlignment.Right);
                for (var y = 0; y < puzzle.Constraints.Length; y++)
                    tt.SetCell(0, y + 1, (y + 1).ToString().Color(ConsoleColor.White), alignment: HorizontalTextAlignment.Right);
                for (var x = 0; x < puzzle.Constraints.Length; x++)
                    for (var y = 0; y < puzzle.Constraints.Length; y++)
                        tt.SetCell(x + 1, y + 1, cell(x, y), alignment: HorizontalTextAlignment.Right);
                tt.WriteToConsole();
                Console.WriteLine();
            }
            outputTable((x, y) => dist(puzzle.Constraints[x], puzzle.Constraints[y]).ToString().Color(ConsoleColor.Magenta));
            outputTable((x, y) => (dist(puzzle.Constraints[x], puzzle.Constraints[y]) % 3).ToString().Color(ConsoleColor.Cyan));
            outputTable((x, y) => (dist(puzzle.Constraints[x], puzzle.Constraints[y]) % 5).ToString().Color(ConsoleColor.Green));
            outputTable((x, y) => (dist(puzzle.Constraints[x], puzzle.Constraints[y]) % 7).ToString().Color(ConsoleColor.Yellow));
            outputTable((x, y) => (dist(puzzle.Constraints[x], puzzle.Constraints[y]) % 9).ToString().Color(ConsoleColor.Gray));
        }

        public static void Stencil_FindPossibleWords()
        {
            var allLatinSquares = NavyButtonPuzzle.FindSolutions(new int?[NavyButtonPuzzle._sz * NavyButtonPuzzle._sz], null, null).ToArray();

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
            var allLatinSquares = NavyButtonPuzzle.FindSolutions(new int?[NavyButtonPuzzle._sz * NavyButtonPuzzle._sz], null, null).ToArray();

            var allWords = new Data().allWords[0].Concat(NavyButtonPuzzle._words4).ToHashSet();
            var currentBest = int.MaxValue;
            var lockObj = new object();

            // Best seed so far (for wordlist): 87 (requires 21)

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
            var allLatinSquares = NavyButtonPuzzle.FindSolutions(new int?[NavyButtonPuzzle._sz * NavyButtonPuzzle._sz], null, null).ToArray();
            var allWords = new Data().allWords[0].Concat(NavyButtonPuzzle._words4).ToHashSet();
            var stencils = FindRequiredStencilsForSeed(87, allLatinSquares, allWords);
            foreach (var stencil in stencils)
            {
                Console.WriteLine(Enumerable.Range(0, 3).Select(row => Enumerable.Range(0, 3).Select(col => col == 1 && row == 1 ? "░░" : stencil.Contains((col - 1, row - 1)) ? "██" : "  ").JoinString()).JoinString("\n"));
                Console.WriteLine();
            }
            Console.WriteLine(stencils.Length);
        }

        private static (int dx, int dy)[][] FindRequiredStencilsForSeed(int seed, int[][] allLatinSquares, HashSet<string> allWords)
        {
            int numBits(int n) => n == 0 ? 0 : 1 + numBits(n & (n - 1));
            var directions = Enumerable.Range(0, 9).Select(i => (dx: i % 3 - 1, dy: i / 3 - 1)).Where(tup => tup.dx != 0 || tup.dy != 0).ToArray();
            var stencils = Enumerable.Range(0, 1 << 8).Where(n => numBits(n) == 3).Select(bits => Enumerable.Range(0, 8).Where(bit => (bits & (1 << bit)) != 0).Select(bit => directions[bit]).ToArray()).ToArray();
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