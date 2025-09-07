using System.Text.RegularExpressions;
using RT.Serialization;
using RT.TagSoup;
using RT.Util.Consoles;
using RT.Util.ExtensionMethods;

namespace KtaneStuff;

internal static class Tennis
{
    public static void GetPlayers()
    {
        var h = new HttpClient();
        h.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:142.0) Gecko/20100101 Firefox/142.0");
        var allData = new Dictionary<string, Dictionary<string, Dictionary<string, Dictionary<string, int>>>>();
        foreach (var tournament in new[] { "Wimbledon Championships", "US Open", "French Open" })
        {
            foreach (var isMale in new[] { true, false })
            {
                foreach (var year in Enumerable.Range(1968, 2024 - 1968 + 1))
                {
                    if (tournament == "Wimbledon Championships" && year == 2020)
                        continue;   // Wimbledon 2020 was canceled
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
                            var url = $@"https://en.wikipedia.org/w/index.php?title={year}_{tournament.Replace(' ', '_')}_%E2%80%93_{(isMale ? "Men" : "Women")}%27s_singles&action=raw";
                            ConsoleUtil.WriteLineFmt($"{url:r}");
                            data = h.GetStringAsync(url).Result;
                            write = true;
                        }
                        catch (Exception e)
                        {
                            ConsoleUtil.WriteLine($"{year.ToString().Color(ConsoleColor.Green)} {tournament.Color(ConsoleColor.Green)}: {e.Message.Color(ConsoleColor.Magenta)} {e.GetType().FullName.Color(ConsoleColor.Red)}", ConsoleColor.DarkRed);
                            continue;
                        }
                    }

                    if (!data.RegexMatch(@"^===.*(Finals|Final Eight).*===", RegexOptions.Multiline | RegexOptions.IgnoreCase, out var m1))
                    {
                        ConsoleUtil.WriteLine($"{year.ToString().Color(ConsoleColor.Cyan)} {tournament.Color(ConsoleColor.Cyan)}: {"Finals section not found".Color(ConsoleColor.Magenta)}", ConsoleColor.Cyan);
                        continue;
                    }
                    var subdata = data.Substring(m1.Index);
                    if (!subdata.RegexMatch(@"^\}\}", RegexOptions.Multiline, out var m2))
                    {
                        ConsoleUtil.WriteLine($"{year.ToString().Color(ConsoleColor.Cyan)} {tournament.Color(ConsoleColor.Cyan)}: {"Could not find end of table.".Color(ConsoleColor.Magenta)}", ConsoleColor.Cyan);
                        continue;
                    }
                    subdata = subdata.Substring(0, m2.Index);
                    foreach (var encounter in subdata.Replace("\r", "").Split('\n')
                        .Select(line => new
                        {
                            Line = line,
                            Match = line.RegexMatch(@"^\|\s*RD(\d+)-team(\d+)\s*=\s*(?:\{\{flagicon\|[ \w]*(?:\|\d+)?\}\}\s*|'''|\{\{nowrap\|)*(?:\[\[)?(.*?)((?:\]\]\s*|'''\s*|\}\}\s*)*)( \(''\[\[|$)", RegexOptions.IgnoreCase, out var m) ? m : null
                        })
                        .Where(line => line.Match != null)
                        .Select(line => new { line.Line, Round = int.Parse(line.Match.Groups[1].Value), Place = int.Parse(line.Match.Groups[2].Value) - 1, Name = line.Match.Groups[3].Value, Suffix = line.Match.Groups[4].Value, IsWinner = line.Match.Groups[4].Value.Contains("'''") })
                        .Select(line => new { line.Line, line.Round, line.Place, Name = (line.Name.Contains('|') ? line.Name.Remove(line.Name.IndexOf('|')) : line.Name).Replace(" (tennis)", "").Replace(" (tennis player)", ""), line.Suffix, line.IsWinner })
                        .GroupBy(line => line.Round * 100 + (line.Place >> 1)))
                    {
                        var winner = modify(encounter.First(g => g.IsWinner).Name);
                        var loser = modify(encounter.First(g => !g.IsWinner).Name);
                        if (winner == null || loser == null)
                            continue;
                        allData.incSafe(tournament, isMale ? "Men" : "Women", winner, loser);
                    }
                    if (write)
                        File.WriteAllText(path, data);
                }
            }
        }

        ClassifyJson.SerializeToFile(allData, $@"D:\c\KTANE\KtaneStuff\DataFiles\Tennis\All encouters.json");
        var allPlayers = allData.SelectMany(kvp => kvp.Value).SelectMany(kvp => kvp.Value.Keys)
            .Concat(allData.SelectMany(kvp => kvp.Value).SelectMany(kvp => kvp.Value).SelectMany(kvp => kvp.Value.Keys))
            .Distinct().Order().ToArray();
        var shortNames = File.ReadLines(@"D:\c\KTANE\KtaneStuff\DataFiles\Tennis\All names.txt").Select(line => line.Split('=')).ToDictionary(arr => arr[0], arr => arr[1]);
        foreach (var newPlayer in allPlayers)
            if (!shortNames.ContainsKey(newPlayer))
                shortNames.Add(newPlayer, newPlayer.RegexMatch(@" (\p{Lu}[-’\p{L}]+)$", out var m) ? m.Groups[1].Value : newPlayer);
        var allNamesStr = shortNames.OrderBy(kvp => kvp.Key).Select(kvp => $"{kvp.Key}={kvp.Value}").JoinString(Environment.NewLine);
        File.WriteAllText($@"D:\c\KTANE\KtaneStuff\DataFiles\Tennis\All names.txt", allNamesStr);

        Utils.ReplaceInFile(@"D:\c\KTANE\Tennis\Assets\Data.cs", "#region auto-generated", "#endregion // auto-generated", $"""
            private const string ShortNamesRaw = @"{allNamesStr}";

            """ +
            new[] { ("Wimbledon Championships", "wimbledon"), ("US Open", "usOpen"), ("French Open", "frenchOpen") }.SelectMany(tournament =>
                new[] { ("Men", "Mens"), ("Women", "Womens") }.Select(gender => $$"""
                static Dictionary<string, Dictionary<string, int>> {{tournament.Item2}}{{gender.Item2}}()
                {
                    return new Dictionary<string, Dictionary<string, int>>
                    {
                        {{allData[tournament.Item1][gender.Item1].Select(kvp => $@"{{ ""{kvp.Key}"", new Dictionary<string, int> {{ {kvp.Value.Select(kvp2 => $@"{{ ""{kvp2.Key}"", {kvp2.Value} }}").JoinString(", ")} }} }}").JoinString(",\r\n        ")}}
                    };
                }

                """)).JoinString());

    }

    private static string modify(string name) => name switch
    {
        "Alexander Vladimirovich Volkov" => "Alexander Volkov",
        "Christophe Roger-Vasselin" => null,
        "Magdaléna Rybáriková" => null,
        "Chris Evert-Lloyd" => "Chris Evert",
        "Arantxa Sánchez Vicario" => "Arantxa Sánchez",
        "Evonne Goolagong Cawley" => "Evonne Goolagong",
        "Helga Niessen Masthoff" => "Helga Niessen",
        "Justine Henin-Hardenne" => "Justine Henin",
        "Mary Joe Fernandez" => "Mary Joe Fernández",
        "Rosie Casals" => "Rosemary Casals",
        "Víctor Pecci, Sr." => "Víctor Pecci",
        "Anastasia Pavlyuchenkova" => null,
        "Coco Vandeweghe" => "CoCo Vandeweghe",
        "Brenda Schultz-McCarthy" => null,
        "Hans-Jürgen Pohmann" => null,
        "Jaime Fillol Sr." => "Jaime Fillol",
        "Judy Tegart Dalton" => "Judy Tegart",
        "Judy Tegart-Dalton" => "Judy Tegart",
        "Kerry Melville Reid" => "Kerry Reid",
        "Kerry Melville" => "Kerry Reid",
        "Lina Krasnoroutskaya" => null,
        "Odile De Roubin" => "Odile de Roubin",
        "Richard Pancho Gonzales" => "Pancho Gonzales",
        "Wojtek Fibak" => "Wojciech Fibak",
        "Beatriz Haddad Maia" => "Beatriz Maia",
        "Tomás Martín Etcheverry" => "Tomás Etcheverry",
        "Alejandro Davidovich Fokina" => "Alejandro Fokina",
        _ => name.Replace("'", "’"),
    };

    private static int incSafe<K1, K2, K3, K4>(this Dictionary<K1, Dictionary<K2, Dictionary<K3, Dictionary<K4, int>>>> dic, K1 key1, K2 key2, K3 key3, K4 key4, int amount = 1)
    {
        if (dic == null)
            throw new ArgumentNullException(nameof(dic));
        if (key1 == null)
            throw new ArgumentNullException(nameof(key1), "Null values cannot be used for keys in dictionaries.");
        if (key2 == null)
            throw new ArgumentNullException(nameof(key2), "Null values cannot be used for keys in dictionaries.");
        if (key3 == null)
            throw new ArgumentNullException(nameof(key3), "Null values cannot be used for keys in dictionaries.");
        if (key4 == null)
            throw new ArgumentNullException(nameof(key4), "Null values cannot be used for keys in dictionaries.");
        if (!dic.ContainsKey(key1))
            dic[key1] = [];
        if (!dic[key1].ContainsKey(key2))
            dic[key1][key2] = [];
        if (!dic[key1][key2].ContainsKey(key3))
            dic[key1][key2][key3] = [];
        return dic[key1][key2][key3].TryGetValue(key4, out var value)
            ? (dic[key1][key2][key3][key4] = value + amount)
            : (dic[key1][key2][key3][key4] = amount);
    }

    public static void TryNames()
    {
        var allNames = File.ReadAllLines(@"D:\c\KTANE\KtaneStuff\DataFiles\Tennis\All names.txt").Select(l => l.Split('=')).Select(arr => arr[0]).OrderByDescending(l => l.Length).ToArray();
        Console.WriteLine(allNames.Take(20).JoinString("\n"));
    }
}
