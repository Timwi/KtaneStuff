using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using RT.Util;
using RT.Util.ExtensionMethods;
using RT.Util.Text;

namespace KtaneStuff
{
    using static Modeling.Md;

    static partial class Ktane
    {
        public static void SimonScreamsGenerateLargeTable()
        {
            var grids = new List<int[][]>();
            for (int i = 0; i < 3; i++)
            {
                var grid = Ut.NewArray(6, 6, (_, __) => Rnd.Next(6));
                fill(grid, 0, 0);
                grids.Add(grid);
            }

            Console.WriteLine(Enumerable.Range(0, 6).Select(row => Enumerable.Range(0, 6).Select(col => $"<td>{grids.Select(g => "ACDEFH"[g[col][row]]).JoinString()}").JoinString()).JoinString(Environment.NewLine));
        }

        public static void SimonScreamsGenerateSmallTable()
        {
            var grid = Ut.NewArray(6, 6, (_, __) => Rnd.Next(6));
            fill(grid, 0, 0);
            Console.WriteLine(Enumerable.Range(0, 6).Select(row => Enumerable.Range(0, 6).Select(col => $"<td>{"ROYGBP"[grid[col][row]]}").JoinString()).JoinString(Environment.NewLine));
        }

        public static void SimonScreamsSvg()
        {
            var path = @"D:\c\KTANE\HTML\img\Component\Simon Screams.svg";
            File.WriteAllText(path, Regex.Replace(File.ReadAllText(path), @"(?<=<!--##-->).*(?=<!--###-->)",
                $@"<g transform='translate(174,174) scale(50)' fill='none' stroke='#000' stroke-width='.04' stroke-linejoin='round'>{
                    Enumerable.Range(0, 6).Select(i => i * 360 / 6).Select(angle =>
                        $"<path d='M0,0 L{1.25 * cos(30)},{1.25 * sin(30)} 3,0 {1.25 * cos(-30)},{1.25 * sin(-30)} z' transform='rotate({angle})' />"
                    ).JoinString()
                }</g>",
                RegexOptions.Singleline));
        }

        private static bool fill(int[][] grid, int x, int y)
        {
            if (x == 6)
            {
                x = 0;
                y++;
            }
            if (y == 6)
                return true;

            var offset = grid[x][y];
            for (int j = 0; j < 6; j++)
            {
                var i = (j + offset) % 6;
                for (int xx = x - 1; xx >= 0; xx--)
                    if (grid[xx][y] == i)
                        goto nope;
                for (int yy = y - 1; yy >= 0; yy--)
                    if (grid[x][yy] == i)
                        goto nope;
                grid[x][y] = i;
                if (fill(grid, x + 1, y))
                    return true;
                nope:;
            }
            return false;
        }

        public static void SimonScreamsSimulation()
        {
            var criteria1 = Ut.NewArray(
                new { Name = "If three adjacent colors flashed in counter clockwise order", Criterion = Ut.Lambda((int[] seq, int[] rgb, int orange, int purple) => Enumerable.Range(0, seq.Length - 2).Any(ix => seq[ix + 1] == (seq[ix] + 5) % 6 && seq[ix + 2] == (seq[ix] + 4) % 6)) },
                new { Name = "Otherwise, if orange flashed more than twice", Criterion = Ut.Lambda((int[] seq, int[] rgb, int orange, int purple) => seq.Count(n => n == orange) > 2) },
                new { Name = "Otherwise, if two adjacent colors didn’t flash", Criterion = Ut.Lambda((int[] seq, int[] rgb, int orange, int purple) => Enumerable.Range(0, 6).Any(color => !seq.Contains(color) && !seq.Contains((color + 1) % 6))) },
                new { Name = "Otherwise, if exactly two colors flashed exactly twice", Criterion = Ut.Lambda((int[] seq, int[] rgb, int orange, int purple) => seq.GroupBy(n => n).Where(g => g.Count() == 2).Count() == 2) },
                new { Name = "Otherwise, if the number of colors that flashed is even", Criterion = Ut.Lambda((int[] seq, int[] rgb, int orange, int purple) => seq.Distinct().Count() % 2 == 0) },
                new { Name = "Otherwise", Criterion = Ut.Lambda((int[] seq, int[] rgb, int orange, int purple) => true) }
            );
            var criteria2 = Ut.NewArray(
                new { Name = "If two opposite colors didn’t flash", Criterion = Ut.Lambda((int[] seq, int[] rgb, int orange, int purple) => Enumerable.Range(0, 3).Any(col => !seq.Contains(col) && !seq.Contains(col + 3))) },
                new { Name = "Otherwise, if at most one of red, green and blue flashed", Criterion = Ut.Lambda((int[] seq, int[] rgb, int orange, int purple) => rgb.Count(color => seq.Contains(color)) <= 1) },
                new { Name = "Otherwise, if three adjacent colors flashed in clockwise order", Criterion = Ut.Lambda((int[] seq, int[] rgb, int orange, int purple) => Enumerable.Range(0, seq.Length - 2).Any(ix => seq[ix + 1] == (seq[ix] + 1) % 6 && seq[ix + 2] == (seq[ix] + 2) % 6)) },
                new { Name = "Otherwise, if a color flashed, then an adjacent color, then the first again", Criterion = Ut.Lambda((int[] seq, int[] rgb, int orange, int purple) => Enumerable.Range(0, seq.Length - 2).Any(ix => seq[ix + 2] == seq[ix] && (seq[ix + 1] == (seq[ix] + 1) % 6 || seq[ix + 1] == (seq[ix] + 5) % 6))) },
                new { Name = "Otherwise, if two adjacent colors flashed in clockwise order", Criterion = Ut.Lambda((int[] seq, int[] rgb, int orange, int purple) => Enumerable.Range(0, seq.Length - 1).Any(ix => seq[ix + 1] == (seq[ix] + 1) % 6)) },
                new { Name = "Otherwise", Criterion = Ut.Lambda((int[] seq, int[] rgb, int orange, int purple) => true) }
            );

            const int numIterations = 1000000;

            var dic = new Dictionary<string, Dictionary<int, int>>();
            int numStages = 0;
            for (int i = 0; i < numIterations; i++)
            {
                var colors = Enumerable.Range(0, 6).ToList().Shuffle();
                var rgb = colors.Take(3).ToArray();
                var orange = colors[3];
                var purple = colors[4];
                var seqs = generateSequences();
                numStages = seqs.Length;
                for (int seqIx = 0; seqIx < seqs.Length; seqIx++)
                {
                    var seq = seqs[seqIx];
                    string c1 = "Otherwise";
                    foreach (var cr in criteria1)
                        if (cr.Criterion(seq, rgb, orange, purple))
                        {
                            c1 = cr.Name;
                            break;
                        }
                    string c2 = "Otherwise";
                    foreach (var cr in criteria2)
                        if (cr.Criterion(seq, rgb, orange, purple))
                        {
                            c2 = cr.Name;
                            break;
                        }
                    dic.IncSafe($"{c1}×{c2}", seqIx);
                }
            }

            for (int stage = 0; stage < numStages; stage++)
            {
                var tt = new TextTable { ColumnSpacing = 3, RowSpacing = 1, HorizontalRules = true, VerticalRules = true, MaxWidth = 125 };
                var col = 1;
                foreach (var cri1 in criteria1)
                    tt.SetCell(col++, 0, cri1.Name);
                tt.SetCell(col++, 0, "Total");

                var row = 1;
                foreach (var cri2 in criteria2)
                {
                    tt.SetCell(0, row, cri2.Name);
                    col = 1;
                    foreach (var cri1 in criteria1)
                        tt.SetCell(col++, row, $"{(dic.Get($"{cri1.Name}×{cri2.Name}", stage, 0) / (double) numIterations) * 100:0.000}%", alignment: HorizontalTextAlignment.Right);
                    tt.SetCell(col, row, $"{(criteria1.Sum(cri1 => dic.Get($"{cri1.Name}×{cri2.Name}", stage, 0)) / (double) numIterations) * 100:0.000}%", alignment: HorizontalTextAlignment.Right);
                    row++;
                }

                tt.SetCell(0, row, "Total");
                col = 1;
                foreach (var cri1 in criteria1)
                    tt.SetCell(col++, row, $"{(criteria2.Sum(cri2 => dic.Get($"{cri1.Name}×{cri2.Name}", stage, 0)) / (double) numIterations) * 100:0.000}%", alignment: HorizontalTextAlignment.Right);

                tt.WriteToConsole();
                Console.WriteLine(new string('─', 125));
            }
        }

        private static int IncSafe<K1, K2>(this IDictionary<K1, Dictionary<K2, int>> dic, K1 key1, K2 key2, int amount = 1)
        {
            if (dic == null)
                throw new ArgumentNullException("dic");
            if (key1 == null)
                throw new ArgumentNullException(nameof(key1), "Null values cannot be used for keys in dictionaries.");
            if (key2 == null)
                throw new ArgumentNullException(nameof(key2), "Null values cannot be used for keys in dictionaries.");
            if (!dic.ContainsKey(key1))
                dic[key1] = new Dictionary<K2, int>();
            if (!dic[key1].ContainsKey(key2))
                return (dic[key1][key2] = amount);
            else
                return (dic[key1][key2] = dic[key1][key2] + amount);
        }

        private static int[][] generateSequences()
        {
            var seq = generateSequence().ToArray();
            var arr = new int[3][];
            var len = 6;
            for (int stage = 0; stage < 3; stage++)
            {
                arr[stage] = seq.Subarray(0, len);
                len += Rnd.Next(3) + 1;
            }
            return arr;
        }

        private static IEnumerable<int> generateSequence()
        {
            var last = Rnd.Next(6);
            yield return last;
            var num = 12;
            for (int i = 1; i < num; i++)
            {
                var next = Rnd.Next(5);
                if (next >= last)
                    next++;
                yield return next;
                last = next;
            }
        }
    }
}
