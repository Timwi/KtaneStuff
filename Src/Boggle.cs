using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using RT.Util.ExtensionMethods;

namespace KtaneStuff
{
    static class Boggle
    {
        sealed class SolutionInfo : IEquatable<SolutionInfo>
        {
            public string[] Coordinates;
            public string Word;

            public bool Equals(SolutionInfo other) => other != null && other.Word == Word;
            public override int GetHashCode() => Word.GetHashCode();
        }

        public static void MakeCheatSheet()
        {
            var fullboard = new[] { "ISHNFYETRT", "PWTETEITRA", "OOQEAWXAME", "CAHRZRCHDR", "MPJEKDWSOO", "USODTDETIH", "ISIAELURMO", "LEDEOCOGTU", "RNCEAYNBIN", "ATELBATWSQ" };
            var words = File.ReadAllLines(@"D:\Daten\sowpods.txt").ToHashSet();
            var prefixes = new HashSet<string>();
            foreach (var w in words)
                for (int i = 1; i < w.Length; i++)
                    prefixes.Add(w.Substring(0, i));

            void gen(int x, int y, string wordPrefix, List<string> coordsPrefix, int taken, HashSet<SolutionInfo> output, string[] board)
            {
                if (x < 0 || x >= 4 || y < 0 || y >= 4)
                    return;
                if ((taken & (1 << (4 * x + y))) != 0)
                    return;
                taken |= (1 << (4 * x + y));
                wordPrefix += board[y][x];
                if (board[y][x] == 'Q')
                    wordPrefix += 'U';
                if (!prefixes.Contains(wordPrefix))
                    return;
                coordsPrefix.Add(((char) ('A' + x)) + "" + (y + 1));
                if (words.Contains(wordPrefix))
                    output.Add(new SolutionInfo { Word = wordPrefix, Coordinates = coordsPrefix.ToArray() });
                for (var i = -1; i <= 1; i++)
                    for (var j = -1; j <= 1; j++)
                        gen(x + i, y + j, wordPrefix, coordsPrefix, taken, output, board);
                coordsPrefix.RemoveAt(coordsPrefix.Count - 1);
            }

            Console.WriteLine("<table>");
            for (int y = 0; y < 7; y++)
            {
                Console.WriteLine("<tr>");
                for (int x = 0; x < 7; x++)
                {
                    var output = new HashSet<SolutionInfo>();
                    var partialBoard = fullboard.Subarray(y, 4).Select(row => row.Substring(x, 4)).ToArray();
                    for (int i = 0; i < 4; i++)
                        for (int j = 0; j < 4; j++)
                            gen(i, j, "", new List<string>(), 0, output, partialBoard);

                    var outputSorted = output.OrderByDescending(w => w.Word.Length).ToArray();
                    List<SolutionInfo> solutions = null;
                    var scoreFromLength = new[] { 0, 0, 0, 1, 1, 2, 3, 5, 11 };

                    for (int len = 1; len <= 3; len++)
                    {
                        var candidate = outputSorted.Subsequences(len, len).Where(sols => sols.Sum(sol => scoreFromLength[sol.Word.Length]) >= 5).MinElementOrDefault(sols => sols.Sum(sol => sol.Coordinates.Length));
                        if (candidate != null)
                        {
                            solutions = candidate.ToList();
                            break;
                        }
                    }

                    Console.WriteLine($"<td>{solutions.Select(sol => $"{sol.Coordinates.JoinString("/")} ({sol.Word})").JoinString("<br>")}</td>");
                }
                Console.WriteLine("</tr>");
            }
            Console.WriteLine("</table>");
        }
    }
}