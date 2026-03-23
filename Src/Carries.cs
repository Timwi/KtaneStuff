using System.Diagnostics;
using RT.Util;
using RT.Util.Consoles;
using RT.Util.ExtensionMethods;

namespace KtaneStuff;

static class Carries
{
    public static void DoAlgorithm()
    {
        const int numCols = 5;

        var word = "POW";
        var binary = word.Select(ch => Utils.ConvertToBase(ch - 'A' + 1, 2).PadLeft(numCols, '0')).ToArray();
        var powersOfTen = Enumerable.Range(0, numCols).Select(i => (int) Math.Pow(10, i + 1)).ToArray();

        IEnumerable<string[]> recurse(string[] soFar, Random rnd)
        {
            if (soFar.All(s => s.Length == numCols))
            {
                yield return soFar;
                yield break;
            }

            var row = Enumerable.Range(1, soFar.Length - 1).FirstOrDefault(i => soFar[i].Length < soFar[i - 1].Length);
            var place = soFar[row].Length;

            var digits = Enumerable.Range(0, 10).ToArray().Shuffle(rnd);
            foreach (var digit in digits)
            {
                var newSoFar = soFar.ToArray();
                newSoFar[row] = digit + newSoFar[row];

                // Check if adding the numbers so far actually produces the carry we expect
                if (row > 0)
                {
                    var prevSum = newSoFar.Take(row).Aggregate(0, (prev, next) => prev + int.Parse(next)) % powersOfTen[place];
                    if ((prevSum + int.Parse(newSoFar[row]) >= powersOfTen[place]) != (binary[row - 1][numCols - 1 - place] == '1'))
                        continue;
                }

                foreach (var result in recurse(newSoFar, rnd))
                    yield return result;
            }
        }

        var stats = new Dictionary<int, int>();
        for (var seed = 0; seed < 100; seed++)
        {
            var rnd = new Random(seed);
            var solution = recurse(Enumerable.Repeat("", binary.Length + 1).ToArray(), rnd).First();
            ConsoleUtil.WriteLineFmt($"{seed:Y} =\n    {solution.JoinString("\n    ")}");
            foreach (var str in solution)
                foreach (var ch in str)
                    stats.IncSafe(ch - '0');
        }
        foreach (var kvp in stats.OrderByDescending(k => k.Value))
            ConsoleUtil.WriteLineFmt($"{kvp.Key:G}: {kvp.Value:C}");
    }
}
