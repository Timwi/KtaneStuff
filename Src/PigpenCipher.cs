using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using RT.Util.ExtensionMethods;

namespace KtaneStuff
{
    static class PigpenCipher
    {
        public static void Do()
        {
            //            var rnd = new Random(51);
            //            var numbers = Enumerable.Repeat(Enumerable.Range(1, 4), 25).SelectMany(x => x).ToArray().Shuffle(rnd);
            //            File.WriteAllText(@"D:\temp\temp.html", $@"
            //<table>
            //    {numbers.Split(10).Select(row => $@"<tr>{row.Select(value => $"<td>{value}</td>").JoinString()}</tr>").JoinString("\r\n    ")}
            //</table>");

            var englishWords = File.ReadLines(@"D:\Daten\Unsorted\english-frequency.txt")
                .Select(line => new { Line = line, Match = Regex.Match(line, @"^\d+ ([a-z]+) \w+ \d+$") })
                .Where(obj => obj.Match.Success)
                .Select(obj => obj.Match.Groups[1].Value.ToUpperInvariant())
                .ToHashSet();

            var wordLength = 6;
            var groups = new[] { "ACGI", "BFHD", "JLRP", "KOQM", "STUV", "WXYZ" };
            var groupList = File.ReadLines(@"D:\Daten\sowpods.txt")
                .Where(w => w.Length == wordLength/* && englishWords.Contains(w)*/)
                //.Where(w => !w.Contains('E') && !w.Contains('N'))
                .GroupBy(w => w.Select(ch =>
                {
                    var ix = groups.IndexOf(g => g.Contains(ch));
                    return ix == -1 ? ch : (char) ('0' + ix);
                }).JoinString())
                //.Where(g => g.Key[5] != 'N' && g.Key[4] != 'E')
                .Where(g => g.Count() >= 2)
                .OrderByDescending(g => g.Count())
                .Select(g =>
                {
                    var arr = g.ToArray();
                    for (int i = 0; i < arr.Length; i++)
                        for (int j = i + 1; j < arr.Length; j++)
                        {
                            var c = Enumerable.Range(0, arr[0].Length).Count(ix => arr[i][ix] == arr[j][ix]);
                            if (c <= 1)
                                return new { Words = new[] { arr[i], arr[j] }, CommonLetters = c };
                        }
                    return null;
                })
                .Where(x => x != null)
                .OrderBy(x => x.CommonLetters)
                .ToArray();

            foreach (var group in groupList)//.Take(10))
                Console.WriteLine($"{group.Words.JoinString(", ")} ({group.CommonLetters})");
            Console.WriteLine(groupList.Length);
        }
    }
}