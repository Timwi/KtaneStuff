using System;
using System.Collections.Generic;
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
            var entities = "ensp=\u2002,emsp=\u2003,nbsp=\u00a0,ge=≥,gt=>,lt=<,le=≤,amp=&,shy=\u00ad,mdash=—,trade=™,ohm=Ω,ldquo=“,rdquo=”,horbar=―,rarr=→,uarr=↑,darr=↓,larr=←,times=×"
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
                    .Where(m => m.Value.All(ch => ch >= 'A' && ch <= 'Z'))
                    .Select(m => m.Value.ToUpperInvariant())
                    //.Where(word => word.Length >= 2)
                    .Where(word => Indicator.WellKnown.Concat(new[] { "NLL" }).Contains(word.ToUpperInvariant()))
                    .GroupBy(word => word, StringComparer.OrdinalIgnoreCase)
                    .ToDictionary(gr => gr.Key, gr => gr.Count(), StringComparer.OrdinalIgnoreCase);

            var tt = new TextTable { ColumnSpacing = 2 };
            var row = 0;
            foreach (var kvp in words.OrderByDescending(p => p.Value).Take(100))
            {
                tt.SetCell(0, row, $"{row + 1}.".ToString().Color(ConsoleColor.Blue), alignment: HorizontalTextAlignment.Right);
                tt.SetCell(1, row, kvp.Value.ToString().Color(ConsoleColor.White), alignment: HorizontalTextAlignment.Right);
                tt.SetCell(2, row, kvp.Key.Color(ConsoleColor.Green));
                tt.SetCell(3, row, new string('█', kvp.Value));
                row++;
            }
            tt.WriteToConsole();
        }
    }

    static class ExtensionMethods
    {
        public static void AddSafe<K1, K2, K3, K4, V>(this IDictionary<K1, Dictionary<K2, Dictionary<K3, Dictionary<K4, V>>>> dic, K1 key1, K2 key2, K3 key3, K4 key4, V value)
        {
            if (dic == null)
                throw new ArgumentNullException("dic");
            if (key1 == null)
                throw new ArgumentNullException("key1", "Null values cannot be used for keys in dictionaries.");
            if (key2 == null)
                throw new ArgumentNullException("key2", "Null values cannot be used for keys in dictionaries.");
            if (key3 == null)
                throw new ArgumentNullException("key3", "Null values cannot be used for keys in dictionaries.");
            if (key4 == null)
                throw new ArgumentNullException("key4", "Null values cannot be used for keys in dictionaries.");

            if (!dic.ContainsKey(key1))
                dic[key1] = new Dictionary<K2, Dictionary<K3, Dictionary<K4, V>>>();
            if (!dic[key1].ContainsKey(key2))
                dic[key1][key2] = new Dictionary<K3, Dictionary<K4, V>>();
            if (!dic[key1][key2].ContainsKey(key3))
                dic[key1][key2][key3] = new Dictionary<K4, V>();

            dic[key1][key2][key3][key4] = value;
        }
    }
}
