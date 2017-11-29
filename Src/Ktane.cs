using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using RT.Util.ExtensionMethods;
using RT.Util.Text;

namespace KtaneStuff
{
    partial class Program
    {
        static void KtaneCountWords()
        {
            var entities = "ensp =\u2002,nbsp=\u00a0,ge=≥,gt=>,lt=<,le=≤,amp=&,shy=\u00ad,mdash=—,trade=™,ohm=Ω,ldquo=“,rdquo=”,horbar=―,rarr=→,times=×"
                .Split(',')
                .Select(p => p.Split('='))
                .ToDictionary(p => $"&{p[0]};", p => p[1]);

            var words =
                new DirectoryInfo(@"D:\c\KTANE\Public\HTML")
                    .EnumerateFiles("*.html", SearchOption.TopDirectoryOnly)
                    .Where(file => !file.Name.Contains('('))
                    .Select(file => File.ReadAllText(file.FullName))
                    .Select(text => Regex.Replace(text, @"\A.*(?=<body)", "", RegexOptions.Singleline))
                    .Select(text => Regex.Replace(text, @"<style>([^<]*)</style>", "", RegexOptions.Singleline))
                    .Select(text => Regex.Replace(text, @"<[^>]+>", "", RegexOptions.Singleline))
                    .Select(text => Regex.Replace(text, @"&\w+;", m => entities[m.Value], RegexOptions.Singleline))
                    .Select(text => text.Replace("Keep Talking and Nobody Explodes Mod", ""))
                    .Select(text => text.Replace("Keep Talking and Nobody Explodes v. 1", ""))
                    .SelectMany(text => Regex.Matches(text, @"[-'’\w]+", RegexOptions.Singleline).Cast<Match>())
                    .Select(m => m.Value.ToLowerInvariant())
                    .Where(word => word.Length > 2)
                    .GroupBy(word => word, StringComparer.OrdinalIgnoreCase)
                    .ToDictionary(gr => gr.Key, gr => gr.Count(), StringComparer.OrdinalIgnoreCase);

            var tt = new TextTable { ColumnSpacing = 2 };
            var row = 0;
            foreach (var kvp in words.OrderByDescending(p => p.Value).Take(100))
            {
                tt.SetCell(0, row, $"{row + 1}.".ToString().Color(ConsoleColor.Blue), alignment: HorizontalTextAlignment.Right);
                tt.SetCell(1, row, kvp.Value.ToString().Color(ConsoleColor.White), alignment: HorizontalTextAlignment.Right);
                tt.SetCell(2, row, kvp.Key.Color(ConsoleColor.Green));
                row++;
            }
            tt.WriteToConsole();
        }
    }
}
