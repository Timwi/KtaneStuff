using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using RT.Util;
using RT.Util.Consoles;
using RT.Util.ExtensionMethods;
using RT.Util.Serialization;

namespace KtaneStuff
{
    static class Tennis
    {
        public static void GetPlayers()
        {
            var h = new HClient();
            var allData = new Dictionary<string, Dictionary<string, Dictionary<string, Dictionary<string, int>>>>();
            foreach (var tournament in new[] { "Wimbledon Championships", "US Open", "French Open" })
            {
                foreach (var isMale in new[] { true, false })
                {
                    foreach (var year in Enumerable.Range(1968, 2017 - 1968 + 1))
                    {
                        var path = $@"D:\c\KTANE\KtaneStuff\DataFiles\Tennis\{tournament} {year}{(isMale ? "" : " (W)")}.txt";
                        var write = false;
                        string data;
                        if (File.Exists(path))
                            data = File.ReadAllText(path);
                        else
                        {
                            try
                            {
                                Console.WriteLine($"Downloading: {tournament} {year} ({(isMale ? "M" : "W")})");
                                var raw = h.Get($@"https://en.wikipedia.org/w/index.php?title={year}_{tournament.Replace(' ', '_')}_%E2%80%93_{(isMale ? "Men" : "Women")}%27s_Singles&action=raw");
                                data = raw.DataString;
                                write = true;
                            }
                            catch (Exception e)
                            {
                                ConsoleUtil.WriteLine($"{year.ToString().Color(ConsoleColor.Green)} {tournament.Color(ConsoleColor.Green)}: {e.Message.Color(ConsoleColor.Magenta)} {e.GetType().FullName.Color(ConsoleColor.Red)}", ConsoleColor.DarkRed);
                                continue;
                            }
                        }

                        var m = Regex.Match(data, @"^===.*(Finals|Final Eight).*===", RegexOptions.Multiline | RegexOptions.IgnoreCase);
                        if (!m.Success)
                        {
                            ConsoleUtil.WriteLine($"{year.ToString().Color(ConsoleColor.Cyan)} {tournament.Color(ConsoleColor.Cyan)}: {"Finals section not found".Color(ConsoleColor.Magenta)}", ConsoleColor.Cyan);
                            continue;
                        }
                        var subdata = data.Substring(m.Index);
                        m = Regex.Match(subdata, @"^\}\}", RegexOptions.Multiline);
                        if (!m.Success)
                        {
                            ConsoleUtil.WriteLine($"{year.ToString().Color(ConsoleColor.Cyan)} {tournament.Color(ConsoleColor.Cyan)}: {"Could not find end of table.".Color(ConsoleColor.Magenta)}", ConsoleColor.Cyan);
                            continue;
                        }
                        subdata = subdata.Substring(0, m.Index);
                        foreach (var encounter in subdata.Replace("\r", "").Split('\n')
                            .Select(line => new { Line = line, Match = Regex.Match(line, @"^\|\s*RD(\d+)-team(\d+)\s*=\s*(?:\{\{flagicon\|[ \w]+(?:\|\d+)?\}\}\s*|'''|\{\{nowrap\|)*(?:\[\[)?(.*?)((?:\]\]\s*|'''\s*|\}\}\s*)*)$", RegexOptions.IgnoreCase) })
                            .Where(line => line.Match.Success)
                            .Select(line => new { line.Line, Round = int.Parse(line.Match.Groups[1].Value), Place = int.Parse(line.Match.Groups[2].Value) - 1, Name = line.Match.Groups[3].Value, Suffix = line.Match.Groups[4].Value, IsWinner = line.Match.Groups[4].Value.Contains("'''") })
                            .Select(line => new { line.Line, line.Round, line.Place, Name = (line.Name.Contains('|') ? line.Name.Remove(line.Name.IndexOf('|')) : line.Name).Replace(" (tennis)", "").Replace(" (tennis player)", ""), line.Suffix, line.IsWinner })
                            .GroupBy(line => line.Round * 100 + (line.Place >> 1)))
                        {
                            Clipboard.SetText($@"https://en.wikipedia.org/w/index.php?title={year}_{tournament.Replace(' ', '_')}_%E2%80%93_{(isMale ? "Men" : "Women")}%27s_Singles&action=edit&section=4");
                            var winner = modify(encounter.First(g => g.IsWinner).Name);
                            var loser = modify(encounter.First(g => !g.IsWinner).Name);
                            if (winner == null || loser == null)
                                continue;
                            allData.IncSafe(tournament, isMale ? "Men" : "Women", winner, loser);
                        }
                        if (write)
                            File.WriteAllText(path, data);
                    }
                }
            }

            Utils.ReplaceInFile(@"D:\c\KTANE\Tennis\Assets\Data.cs", "// Start auto-generated", "// End auto-generated",
                new[] { ("Wimbledon Championships", "wimbledon"), ("US Open", "usOpen"), ("French Open", "frenchOpen") }.SelectMany(tournament =>
                    new[] { ("Men", "Mens"), ("Women", "Womens") }.Select(gender => $@"
static Dictionary<string, Dictionary<string, int>> {tournament.Item2}{gender.Item2}()
{{
    return new Dictionary<string, Dictionary<string, int>>
    {{
        {allData[tournament.Item1][gender.Item1].Select(kvp => $@"{{ ""{kvp.Key}"", new Dictionary<string, int> {{ {kvp.Value.Select(kvp2 => $@"{{ ""{kvp2.Key}"", {kvp2.Value} }}").JoinString(", ")} }} }}").JoinString(",\r\n        ")}
    }};
}}
")).JoinString());

            ClassifyJson.SerializeToFile(allData, $@"D:\c\KTANE\KtaneStuff\DataFiles\Tennis\All encouters.json");
            var allPlayers = allData.SelectMany(kvp => kvp.Value).SelectMany(kvp => kvp.Value.Keys)
                .Concat(allData.SelectMany(kvp => kvp.Value).SelectMany(kvp => kvp.Value).SelectMany(kvp => kvp.Value.Keys))
                .Distinct().Order().ToArray();
            File.WriteAllText($@"D:\c\KTANE\KtaneStuff\DataFiles\Tennis\All names2.txt", allPlayers.Select(p => { Match m; return $"{p}={((m = Regex.Match(p, @" (\p{Lu}[-\p{L}]+)$")).Success ? m.Groups[1].Value : p)}"; }).JoinString(Environment.NewLine));
        }

        private static string modify(string name)
        {
            switch (name)
            {
                case "Alexander Vladimirovich Volkov": return "Alexander Volkov";
                case "Christophe Roger-Vasselin": return null;
                case "Magdaléna Rybáriková": return null;
                case "Chris Evert-Lloyd": return "Chris Evert";
                case "Arantxa Sánchez Vicario": return "Arantxa Sánchez";
                case "Evonne Goolagong Cawley": return "Evonne Goolagong";
                case "Helga Niessen Masthoff": return "Helga Niessen";
                case "Justine Henin-Hardenne": return "Justine Henin";
                case "Mary Joe Fernandez": return "Mary Joe Fernández";
                case "Rosie Casals": return "Rosemary Casals";
                case "Víctor Pecci, Sr.": return "Víctor Pecci";
                case "Anastasia Pavlyuchenkova": return null;
                case "Coco Vandeweghe": return "CoCo Vandeweghe";
                case "Brenda Schultz-McCarthy": return null;
                case "Hans-Jürgen Pohmann": return null;
                case "Jaime Fillol Sr.": return "Jaime Fillol";
                case "Judy Tegart Dalton": return "Judy Tegart";
                case "Judy Tegart-Dalton": return "Judy Tegart";
                case "Kerry Melville Reid": return "Kerry Reid";
                case "Kerry Melville": return "Kerry Reid";
                case "Lina Krasnoroutskaya": return null;
                case "Odile De Roubin": return "Odile de Roubin";
                case "Richard Pancho Gonzales": return "Pancho Gonzales";
                case "Wojtek Fibak": return "Wojciech Fibak";
            }
            return name.Replace("'", "’");
        }

        static int IncSafe<K1, K2, K3, K4>(this IDictionary<K1, Dictionary<K2, Dictionary<K3, Dictionary<K4, int>>>> dic, K1 key1, K2 key2, K3 key3, K4 key4, int amount = 1)
        {
            if (dic == null)
                throw new ArgumentNullException("dic");
            if (key1 == null)
                throw new ArgumentNullException(nameof(key1), "Null values cannot be used for keys in dictionaries.");
            if (key2 == null)
                throw new ArgumentNullException(nameof(key2), "Null values cannot be used for keys in dictionaries.");
            if (key3 == null)
                throw new ArgumentNullException(nameof(key3), "Null values cannot be used for keys in dictionaries.");
            if (key4 == null)
                throw new ArgumentNullException(nameof(key4), "Null values cannot be used for keys in dictionaries.");
            if (!dic.ContainsKey(key1))
                dic[key1] = new Dictionary<K2, Dictionary<K3, Dictionary<K4, int>>>();
            if (!dic[key1].ContainsKey(key2))
                dic[key1][key2] = new Dictionary<K3, Dictionary<K4, int>>();
            if (!dic[key1][key2].ContainsKey(key3))
                dic[key1][key2][key3] = new Dictionary<K4, int>();
            if (!dic[key1][key2][key3].ContainsKey(key4))
                return (dic[key1][key2][key3][key4] = amount);
            else
                return (dic[key1][key2][key3][key4] = dic[key1][key2][key3][key4] + amount);
        }

        public static void TryNames()
        {
            var allNames = File.ReadAllLines(@"D:\c\KTANE\KtaneStuff\DataFiles\Tennis\All names.txt").Select(l => l.Split('=')).Select(arr => arr[0]).OrderByDescending(l => l.Length).ToArray();
            Console.WriteLine(allNames.Take(20).JoinString("\n"));
        }
    }
}