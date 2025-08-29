using System.Text.RegularExpressions;
using RT.Util.ExtensionMethods;

namespace KtaneStuff;

internal static class PigpenCipher
{
    public static void Do()
    {
        var englishWords = File.ReadLines(@"D:\Daten\Unsorted\english-frequency.txt")
            .Select(line => new { Line = line, Match = line.RegexMatch(@"^\d+ ([a-z]+) \w+ \d+$", out var m) ? m : null })
            .Where(obj => obj.Match != null)
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
                for (var i = 0; i < arr.Length; i++)
                    for (var j = i + 1; j < arr.Length; j++)
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
