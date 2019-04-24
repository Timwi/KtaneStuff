using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using RT.Util;
using RT.Util.ExtensionMethods;

namespace KtaneStuff
{
    static class MorseTable
    {
        public static void Generate()
        {
            var path = @"D:\c\KTANE\HTML\Morsematics cheat sheet (Rexkix).html";
            File.WriteAllText(path, Regex.Replace(File.ReadAllText(path), @"(?<=<!--##-->).*(?=<!--###-->)",
                Generate(sortByLetter: false, clip: 0) + Generate(sortByLetter: true, clip: 0), RegexOptions.Singleline));
        }

        public static string Generate(bool sortByLetter, int clip = 10)
        {
            var codes = new Dictionary<string, string> {
                { "T", "−" },
                { "M", "−−" },
                { "O", "−−−" },
                { "G", "−−·" },
                { "Q", "−−·−" },
                { "Z", "−−··" },
                { "N", "−·" },
                { "K", "−·−" },
                { "Y", "−·−−" },
                { "C", "−·−·" },
                { "D", "−··" },
                { "X", "−··−" },
                { "B", "−···" },
                { "E", "·" },
                { "A", "·−" },
                { "W", "·−−" },
                { "J", "·−−−" },
                { "P", "·−−·" },
                { "R", "·−·" },
                { "L", "·−··" },
                { "I", "··" },
                { "U", "··−" },
                { "F", "··−·" },
                { "S", "···" },
                { "V", "···−" },
                { "H", "····" }
            };

            var fibo = new[] { 1, 2, 3, 5, 8, 13, 21 };
            var primes = new[] { 2, 3, 5, 7, 11, 13, 17, 19, 23 };
            var squares = new[] { 1, 4, 9, 16, 25 };

            var calculate = Ut.Lambda((int v, int frst) => (v * (frst + (fibo.Contains(v) ? 2 : 0) + (primes.Contains(v) ? -1 : 0) + (squares.Contains(v) ? -1 : 0)) + 3 * 26 + clip) % 26 - clip);

            var arr = codes
                .OrderBy(kvp => sortByLetter ? kvp.Key : kvp.Value, StringComparer.OrdinalIgnoreCase)
                .Select(kvp => $"<th class='letter'>{kvp.Key}<td class='number'>{(kvp.Key[0] - 'A' + 1).Apply(v => $"{v}<td class='number'>{calculate(v, -1)}<td class='number'>{calculate(v, 1)}")}<td><svg class='morse' viewBox='0 0 {kvp.Value.Sum(ch => ch == '−' ? 4 : 2)} 1'>{kvp.Value.Aggregate(new { X = 0, Str = "" }, (prev, ch) => ch == '−' ? new { X = prev.X + 4, Str = prev.Str + $"<rect x='{prev.X}' y='0' width='3' height='1'/>" } : new { X = prev.X + 2, Str = prev.Str + $"<circle cx='{prev.X + .5}' cy='.5' r='.5' />" }).Str}</svg>")
                .ToArray();

            return $"<table class='morse'><tr class='top'><th class='letter'><th><th>∣b<th>∤b<th>Morse code<th class='letter'><th><th>∣b<th>∤b<th>Morse code</tr>{Enumerable.Range(0, 13).Select(row => $"<tr>{arr[row]}{arr[row + 13]}</tr>").JoinString()}</table>";
        }
    }
}
