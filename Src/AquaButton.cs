using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using RT.Util;
using RT.Util.Consoles;
using RT.Util.ExtensionMethods;

namespace KtaneStuff
{
    static class AquaButton
    {
        public static bool[][] BrailleBits = @"
a=1 b=12 c=14
d=145 e=15 f=124 g=1245 h=125 i=24 j=245
k=13 l=123 m=134 n=1345 o=135 p=1234 q=12345 r=1235 s=234 t=2345 u=136 v=1236 w=2456 x=1346 y=13456 z=1356
"
            .Split(new[] { '\r', '\n', ' ' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(bit => Regex.Match(bit, @"^[a-z]=(\d+)$"))
            .Where(m => m.Success)
            .Select(m => Enumerable.Range(0, 6).Select(i => m.Groups[1].Value.Contains((char) ('1' + i))).ToArray())
            .ToArray();

        static string nonogramClue(IEnumerable<bool> src) => src.GroupConsecutive().Where(gr => gr.Key).Select(gr => gr.Count()).JoinString().PadLeft(1, '0');

        private static Dictionary<string, bool[][]> GetNonogramCombinations()
        {
            var dic = new Dictionary<string, List<bool[]>>();
            for (var i = 0; i < (1 << 6); i++)
            {
                var arr = Enumerable.Range(0, 6).Select(bit => (i & (1 << bit)) != 0).ToArray();
                dic.AddSafe(nonogramClue(arr), arr);
            }
            return dic.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.ToArray());
        }

        public static void Experiment()
        {
            var dic = GetNonogramCombinations();
            var possibilities = dic.Keys.ToArray();

            var data = new CipherModulesLib.Data();
            var rnd = new Random(6);
            while (true)
            {
                var word = data.ChooseWord(6, 6, rnd);
                var bitmap = new bool[6 * 6];
                for (var i = 0; i < word.Length; i++)
                    for (var dot = 0; dot < 6; dot++)
                        bitmap[2 * (i % 3) + (dot / 3) + 6 * (3 * (i / 3) + (dot % 3))] = BrailleBits[word[i] - 'A'][dot];

                //for (var row = 0; row < 6; row++)
                //    Console.WriteLine(Enumerable.Range(0, 6).Select(col => bitmap[col + 6 * row] ? "██" : "░░").JoinString());

                //Console.WriteLine();

                var nonogramClues = "";
                // columns
                for (var col = 0; col < 6; col++)
                    nonogramClues += nonogramClue(Enumerable.Range(0, 6).Select(y => bitmap[col + 6 * y]));
                // rows
                for (var row = 0; row < 6; row++)
                    nonogramClues += nonogramClue(Enumerable.Range(0, 6).Select(x => bitmap[x + 6 * row]));

                IEnumerable<string[]> recurse(string[] sofar, int[] remaining)
                {
                    if (remaining.Length == 0 && sofar.Length == 12)
                    {
                        yield return sofar;
                        yield break;
                    }
                    if (remaining.Length == 0 || sofar.Length == 12)
                        yield break;

                    foreach (var poss in possibilities)
                        if (remaining.Take(poss.Length).SequenceEqual(poss.Select(ch => ch - '0')))
                            foreach (var result in recurse(sofar.Insert(sofar.Length, poss), remaining.Remove(0, poss.Length)))
                                yield return result;
                }

                var total = 0;
                foreach (var combinations in recurse(new string[0], nonogramClues.Select(ch => ch - '0').ToArray()))
                {
                    var allSolutions = solveNonogram(combinations.Select(str => dic[str]).ToArray()).ToArray();
                    var numSolutions = allSolutions.Length;
                    //if (allSolutions.Length > 0)
                    //{
                    //    ConsoleUtil.WriteLine($"{combinations.JoinString(" | ")} = {numSolutions}".Color(numSolutions == 0 ? ConsoleColor.DarkRed : numSolutions == 1 ? ConsoleColor.Green : ConsoleColor.Magenta));
                    //    ConsoleUtil.WriteLine(outputGrid(allSolutions[0]) + "\n");
                    //}
                    total += numSolutions;
                }
                //ConsoleUtil.WriteLine($"{word} = {total}".Color(total == 0 ? ConsoleColor.DarkRed : total == 1 ? ConsoleColor.Green : ConsoleColor.Magenta));
                if (total == 1)
                {
                    Console.WriteLine(nonogramClues);
                    Console.ReadLine();
                    Console.WriteLine(word);
                    break;
                }
            }
        }

        public static void TestNonogramSolver()
        {
            var dic = GetNonogramCombinations();
            var c = 0;
            foreach (var solution in solveNonogram(new bool?[6 * 6], Ut.NewArray(
                dic["11"], dic["21"], dic["111"], dic["1"], dic["21"], dic["22"],
                dic["31"], dic["12"], dic["3"], dic["11"], dic["12"], dic["1"]
            )))
            {
                Console.WriteLine(solution.Split(6).Select(row => row.Select(c => c ? "██" : "░░").JoinString()).JoinString("\n"));
                Console.WriteLine();
                c++;
            }
            Console.WriteLine($"{c} solutions found.");
        }

        private static ConsoleColoredString outputGrid(bool[] grid) => outputGrid(grid.Select(g => g.Nullable()).ToArray());
        private static ConsoleColoredString outputGrid(bool?[] grid)
        {
            return Enumerable.Range(0, 6).Select(row =>
                Enumerable.Range(0, 6).Select(col => grid[col + 6 * row] == null ? "??".Color(ConsoleColor.Gray) : grid[col + 6 * row].Value ? "██".Color(ConsoleColor.Yellow) : "░░".Color(ConsoleColor.DarkGreen)).JoinColoredString()
            ).JoinColoredString("\n");
        }

        private static IEnumerable<bool[]> solveNonogram(bool[][][] rowColCombinations) => solveNonogram(new bool?[6 * 6], rowColCombinations);
        private static IEnumerable<bool[]> solveNonogram(bool?[] grid, bool[][][] rowColCombinations)
        {
            if (!grid.Contains(null))
            {
                // Check that the grid consists only of valid Braille letters
                //if (Enumerable.Range(0, 6).All(ltrIx => Enumerable.Range(0, 6).Select(dot => grid[2 * (ltrIx % 3) + (dot / 3) + 6 * (3 * (ltrIx / 3) + (dot % 3))].Value).Apply(brailleDots =>
                //    BrailleBits.Any(bb => brailleDots.SequenceEqual(bb)))))
                yield return grid.WhereNotNull().ToArray();
                yield break;
            }

            // Find the row or column with the fewest remaining combinations
            var bestRowCol = rowColCombinations.IndexOf(a => a != null);
            for (var i = bestRowCol + 1; i < rowColCombinations.Length; i++)
                if (rowColCombinations[i] != null && rowColCombinations[i].Length < rowColCombinations[bestRowCol].Length)
                    bestRowCol = i;

            foreach (var combination in rowColCombinations[bestRowCol])
            {
                var newRowColCombinations = rowColCombinations.ToArray();
                newRowColCombinations[bestRowCol] = null;

                // Reduce the number of combinations of all perpendicular lines
                if (bestRowCol < 6)
                {
                    // We filled in a column ⇒ reduce the number of combinations in the row clues
                    for (var row = 0; row < 6; row++)
                        if (rowColCombinations[row + 6] != null)
                            newRowColCombinations[row + 6] = rowColCombinations[row + 6].Where(b => b[bestRowCol] == combination[row]).ToArray();
                }
                else
                {
                    // We filled in a row ⇒ reduce the number of combinations in the column clues
                    for (var col = 0; col < 6; col++)
                        if (rowColCombinations[col] != null)
                            newRowColCombinations[col] = rowColCombinations[col].Where(b => b[bestRowCol - 6] == combination[col]).ToArray();
                }
                if (newRowColCombinations.Any(n => n != null && n.Length == 0))
                    continue;

                // Fill the relevant row/col of ‘grid’
                var newGrid = grid.ToArray();
                for (var i = 0; i < 6; i++)
                    newGrid[bestRowCol < 6 ? bestRowCol + 6 * i : i + 6 * (bestRowCol - 6)] = combination[i];

                foreach (var solution in solveNonogram(newGrid, newRowColCombinations))
                    yield return solution;
            }
        }
    }
}