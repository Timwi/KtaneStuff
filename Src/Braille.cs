using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using RT.Util;
using RT.Util.ExtensionMethods;

namespace KtaneStuff
{
    public static class Braille
    {
        public static void FindWords()
        {
            var forbidden = @"catherine,katherine,collins,france,french,steven,surrey,tories,edward,sterling,martin,forget,forgot".Split(',').ToHashSet();

            var brailleRaw = @"
a=1 b=12 c=14
d=145 e=15 f=124 g=1245 h=125 i=24 j=245
k=13 l=123 m=134 n=1345 o=135 p=1234 q=12345 r=1235 s=234 t=2345 u=136 v=1236 x=1346 y=13456 z=1356
and=12346 for=123456 of=12356 the=2346 with=23456
ch=16 gh=126 sh=146 th=1456 wh=156 ed=1246 er=12456 ou=1256 ow=246 w=2456
-ea-=2 -bb-=23 -cc-=25 en=26 -ff-=235 -gg-=2356 in=35 st=34 ar=345
-ing=346
"
                .Split(new[] { '\r', '\n', ' ' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(bit => Regex.Match(bit, @"^(-)?(\w+)(-)?=(\d+)$"))
                .Where(m => m.Success)
                .Select(m => new { Bit = m.Groups[2].Value, CanBeInitial = !m.Groups[1].Success, CanBeFinal = !m.Groups[3].Success, Dots = m.Groups[4].Value.Aggregate(0, (prev, next) => prev | (1 << (next - '1'))) })
                .ToArray();
            var braille = brailleRaw.ToDictionary(inf => inf.Bit, inf => inf);
            var brailleRegex = new Regex($@"({braille.OrderByDescending(kvp => kvp.Key.Length).Select(kvp => $"{(kvp.Value.CanBeInitial ? "" : "(?!^)")}{kvp.Key}{(kvp.Value.CanBeFinal ? "" : "(?!$)")}").JoinString("|")})", RegexOptions.Compiled);

            var numBCols = 10;
            var numBRows = (braille.Count + numBCols - 1) / numBCols;
            Utils.ReplaceInFile(@"D:\c\KTANE\Public\HTML\Braille.html", "<!--#-->", "<!--##-->",
                Enumerable.Range(0, numBRows).Select(row => "<tr>" + Enumerable.Range(0, numBCols)
                    .Select(col => /*row + numBRows * col*/col + numBCols * row)
                    .Select(ix => $"<td><svg class='braille-pattern' viewBox='-.5 -.5 4 6'>{Enumerable.Range(0, 6).Select(b => $"<circle cx='{(b / 3) * 2 + .5}' cy='{(b % 3) * 2 + .5}' r='.5' fill='{((brailleRaw[ix].Dots & (1 << b)) != 0 ? "black" : "white")}' stroke='{((brailleRaw[ix].Dots & (1 << b)) != 0 ? "none" : "#ccc")}' stroke-width='.1' />").JoinString()}</svg><div>{brailleRaw[ix].Bit}</div>")
                    .JoinString()).JoinString("\n"));

            var words = File.ReadAllLines(@"D:\Daten\Unsorted\english-frequency.txt")
                .Select(line => Regex.Match(line, @"^(\d+) ([a-z]+) "))
                .Where(m => m.Success && int.Parse(m.Groups[1].Value) >= 725)
                .Select(m => m.Groups[2].Value)
                .Where(w => w.Length >= 6 && !forbidden.Contains(w))
                .Distinct()
                .Select(w => new { Word = w, Braille = brailleRegex.Matches(w).Cast<Match>().Select(m => braille[m.Value].Dots).ToArray() })
                .Where(inf => inf.Braille.Length >= 4 && inf.Braille.Length <= 4)
                .OrderByDescending(inf => inf.Braille.Length)
                .Select((inf, ix) => new { Word = inf.Word, Braille = inf.Braille, Index = ix })
                .ToArray();

            var maxDifferences = 2;
            var pairs = words.UniquePairs()
                .Where(pair =>
                {
                    if (pair.Item1.Braille.Length != pair.Item2.Braille.Length)
                        return false;
                    var differencesFound = 0;
                    for (int i = 0; i < pair.Item1.Braille.Length; i++)
                    {
                        differencesFound += countBits(pair.Item1.Braille[i] ^ pair.Item2.Braille[i]);
                        if (differencesFound > maxDifferences)
                            return false;
                    }
                    if (pair.Item1.Word.EndsWith("s") && pair.Item2.Word == pair.Item1.Word.Remove(pair.Item1.Word.Length - 1) + "ing")
                        return false;
                    if (pair.Item2.Word.EndsWith("s") && pair.Item1.Word == pair.Item2.Word.Remove(pair.Item2.Word.Length - 1) + "ing")
                        return false;
                    return true;
                })
                .ToArray();
            var candidateWords = pairs.SelectMany(pair => new[] { pair.Item1.Word, pair.Item2.Word }).Distinct().ToArray();
            var candidateNumbers = Ut.NewArray(candidateWords.Length, ix => new List<int> { 1, 2, 3, 4 });
            var numbers = new int[candidateWords.Length];
            var rnd = new Random(2);
            candidateWords.Shuffle(rnd);
            var used = new int[4];
            for (int i = 0; i < candidateWords.Length; i++)
            {
                do
                {
                    numbers[i] = candidateNumbers[i][rnd.Next(0, candidateNumbers[i].Count)];

                    // This fail-safe is only necessary for some random seeds. At the time of writing, it does not trigger.
                    if (Enumerable.Range(0, 4).All(ix => used[ix] >= 25 || !candidateNumbers[i].Contains(ix + 1)))
                        break;
                }
                while (used[numbers[i] - 1] >= 25);
                used[numbers[i] - 1]++;
                foreach (var pair in pairs)
                    if (pair.Item1.Word == candidateWords[i])
                        candidateNumbers[candidateWords.IndexOf(pair.Item2.Word)].Remove(numbers[i]);
                    else if (pair.Item2.Word == candidateWords[i])
                        candidateNumbers[candidateWords.IndexOf(pair.Item1.Word)].Remove(numbers[i]);
            }

            var assoc = candidateWords.Order().Zip(numbers, (w, n) => new { Word = w, Number = n }).ToArray();
            var numCols = 5;
            var numRows = (assoc.Length + numCols - 1) / numCols;
            Utils.ReplaceInFile(@"D:\c\KTANE\Public\HTML\Braille.html", "<!--%-->", "<!--%%-->",
                Enumerable.Range(0, numRows)
                    .Select(row => "<tr>" + Enumerable.Range(0, numCols).Select(col => row + numRows * col).Select(ix => ix < assoc.Length ? $"<th>{assoc[ix].Word}<td>{assoc[ix].Number}" : null).JoinString())
                    .JoinString("\n"));

            Console.WriteLine(candidateWords.Length);
        }

        private static int countBits(int num)
        {
            if (num < 0)
                throw new ArgumentException("Negative numbers won’t work here.", "num");
            var bits = 0;
            while (num > 0)
            {
                bits += (num & 1);
                num >>= 1;
            }
            return bits;
        }
    }
}