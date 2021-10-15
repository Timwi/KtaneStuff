using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BlueButtonLib;
using RT.Util;
using RT.Util.ExtensionMethods;
using Words;

namespace KtaneStuff
{
    static class NavyButton
    {
        public static void Experiment()
        {
            //for (var seed = 0; seed < 1000; seed++)
            //{
            //    var rnd = new Random(seed);
            //    var edgework = Edgework.Generate(5, 5, false, rnd);
            //    var puzzle = NavyButtonPuzzle.Create(edgework.SerialNumber, rnd);
            //    Console.WriteLine($"Seed {seed,4} = Serial number {edgework.SerialNumber} = {puzzle.NumSolutions} solutions");
            //    //var tt = new TextTable { ColumnSpacing = 1 };
            //    //for (var x = 0; x < puzzle.Size; x++)
            //    //    for (var y = 0; y < puzzle.Size; y++)
            //    //        tt.SetCell(2 * x, 2 * y, puzzle.Board[x + puzzle.Size * y].ToString());
            //    //foreach (var (cell, ltDir) in puzzle.Constraints)
            //    //    tt.SetCell(2 * (cell % puzzle.Size) + NavyButtonPuzzle.Dxs[ltDir], 2 * (cell / puzzle.Size) + NavyButtonPuzzle.Dys[ltDir], "∧>∨<".Substring(ltDir, 1));
            //    //tt.WriteToConsole();
            //}

            //Console.WriteLine("Generating Latin squares...");
            //var allLatinSquares = NavyButtonPuzzle.FindSolutions(new int?[NavyButtonPuzzle._sz * NavyButtonPuzzle._sz], null, null).ToArray();
            //Console.WriteLine(allLatinSquares.Length);
            //Console.WriteLine();

            var n = new Dictionary<string, int>();
            for (var seed = 0; seed < 1000; seed++)
            {
                var minPuzzle = NavyButtonPuzzle.Create(new Random(seed + 1000), 1);
                var iter = 1;
                while (minPuzzle.Constraints.Length >= 6)
                {
                    iter++;
                    minPuzzle = NavyButtonPuzzle.Create(new Random(seed + 1000), iter);
                }
                Console.WriteLine($"Seed {seed}: {iter} iterations");


                //n.IncSafe($"{new[] { puzzle1.Constraints.Length, puzzle2.Constraints.Length }.Order().JoinString(",")}");
                //var tt = new TextTable { ColumnSpacing = 1 };
                //for (var x = 0; x < puzzle.Size; x++)
                //    for (var y = 0; y < puzzle.Size; y++)
                //        tt.SetCell(2 * x, 2 * y, ".");
                //var already = new Dictionary<int, string>();
                //foreach (var (sm, la) in puzzle.Constraints)
                //{
                //    var ltDir = Enumerable.Range(0, 4).First(i => NavyButtonPuzzle.Dxs[i] == sm % puzzle.Size - la % puzzle.Size && NavyButtonPuzzle.Dys[i] == sm / puzzle.Size - la / puzzle.Size);
                //    tt.SetCell(2 * (la % puzzle.Size) + NavyButtonPuzzle.Dxs[ltDir], 2 * (la / puzzle.Size) + NavyButtonPuzzle.Dys[ltDir], "^>v<".Substring(ltDir, 1));
                //}
                //tt.WriteToConsole();

                //Console.ReadLine();

                //for (var x = 0; x < puzzle.Size; x++)
                //    for (var y = 0; y < puzzle.Size; y++)
                //        tt.SetCell(2 * x, 2 * y, puzzle.Board[x + puzzle.Size * y].ToString());
                //tt.WriteToConsole();
                //Console.WriteLine();
            }
            Console.WriteLine(n.OrderBy(p => p.Key).Select(p => $"{p.Key} constraints = {p.Value} times").JoinString("\n"));
        }

        private static void NavyButtonLength5Experiments(int[][] allLatinSquares)
        {
            var words = File.ReadAllLines(@"D:\temp\temp.txt").Where(w => w.Length == 5).ToHashSet();
            Console.WriteLine($"{words.Count} words in word list");

            var perms4 = Enumerable.Range(0, 4).Permutations().Select(p => p.JoinString()).ToArray();
            char perm2Ltr(string perm)
            {
                var reduced = perm.Select(ch => perm.Count(ch2 => ch2 < ch).ToString()).JoinString();
                var permIx = perms4.IndexOf(reduced);
                if (permIx == 22)
                    return perm.Contains('0') && perm.Contains('1') ? 'Q' : 'J';
                if (permIx == 23)
                    return perm.Contains('0') && perm.Contains('1') ? 'V' : 'X';
                return "ABCDEFGHIKLMNOPRSTUWYZ"[permIx];    // JQVX removed
            }

            File.WriteAllLines(@"D:\temp\temp.txt", Enumerable.Range(0, 5).Permutations().Select(p => p.Take(4).JoinString()).Select(p => $"{p}={perm2Ltr(p)}"));
            System.Diagnostics.Debugger.Break();
            var validWords = new HashSet<string>();
            for (var lsqIx = 0; lsqIx < allLatinSquares.Length; lsqIx++)
            {
                var latinSquare = allLatinSquares[lsqIx];
                var rows = Enumerable.Range(0, 5).Select(row => latinSquare.Subarray(5 * row, 5).JoinString()).ToArray();
                for (var shifts = 0; shifts < 5 * 5 * 5 * 5 * 5; shifts++)
                {
                    var shift1 = shifts % 5;
                    var shift2 = (shifts / 5) % 5;
                    var shift3 = (shifts / 5 / 5) % 5;
                    var shift4 = (shifts / 5 / 5 / 5) % 5;
                    var shift5 = shifts / 5 / 5 / 5 / 5;

                    var row1 = (rows[0].Substring(shift1) + rows[0].Substring(0, shift1)).Substring(1);
                    var row2 = (rows[1].Substring(shift2) + rows[1].Substring(0, shift2)).Substring(1);
                    var row3 = (rows[2].Substring(shift3) + rows[2].Substring(0, shift3)).Substring(1);
                    var row4 = (rows[3].Substring(shift4) + rows[3].Substring(0, shift4)).Substring(1);
                    var row5 = (rows[4].Substring(shift5) + rows[4].Substring(0, shift5)).Substring(1);

                    var word = $"{perm2Ltr(row1)}{perm2Ltr(row2)}{perm2Ltr(row3)}{perm2Ltr(row4)}{perm2Ltr(row5)}";
                    if (words.Contains(word))
                        validWords.Add(word);
                }
                Console.Write($"Latin square #{lsqIx}: {validWords.Count} words\r");
            }
            Console.WriteLine();
            //Console.WriteLine($"Valid words: {validWords.JoinString("\n")}");
            Console.WriteLine($"Valid words: {validWords.Count}");
            File.WriteAllLines(@"D:\temp\temp2.txt", validWords);
        }

        public static void NavyButtonLength4Experiments()
        {
            Console.WriteLine("Generating Latin squares...");
            var allLatinSquares = NavyButtonPuzzle.FindSolutions(new int?[NavyButtonPuzzle._sz * NavyButtonPuzzle._sz], null, null).ToArray();

            int numBits(int n) => n == 0 ? 0 : 1 + numBits(n & (n - 1));
            var directions = Enumerable.Range(0, 9).Select(i => (dx: i % 3 - 1, dy: i / 3 - 1)).Where(tup => tup.dx != 0 || tup.dy != 0).ToArray();
            var allStencils = Enumerable.Range(0, 1 << 8).Where(n => numBits(n) == 3).Select(bits => Enumerable.Range(0, 8).Where(bit => (bits & (1 << bit)) != 0).Select(bit => directions[bit]).ToArray()).ToArray();

            var allWords = new Data().allWords[0].Concat(NavyButtonPuzzle._words4).ToHashSet();
            var numAllWords = allWords.Count;
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
                var rnd = new Random(seed);
                var stencils = allStencils.ToArray();
                stencils.Shuffle(rnd);

                var requiredStencils = Ut.ReduceRequiredSet(Enumerable.Range(0, stencils.Length), test: state =>
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
                    return validWords.Count == 456976;// numAllWords;
                });

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
    }
}