using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using AngleSharp.Css.Dom;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using CsQuery;
using RT.Json;
using RT.TagSoup;
using RT.Util;
using RT.Util.Consoles;
using RT.Util.ExtensionMethods;
using RT.Util.Text;

namespace KtaneStuff;

internal static class Ktane
{
    public static void CountWords()
    {
        var entities = "ensp=\u2002,emsp=\u2003,nbsp=\u00a0,ge=≥,gt=>,lt=<,le=≤,amp=&,shy=\u00ad,mdash=—,trade=™,ohm=Ω,ldquo=“,rdquo=”,horbar=―,rarr=→,uarr=↑,darr=↓,larr=←,times=×,quot=\",phi=φ,rsquo=’"
            .Split(',')
            .Select(p => p.Split('='))
            .ToDictionary(p => $"&{p[0]};", p => p[1]);

        var words =
            new DirectoryInfo(@"D:\c\KTANE\Public\HTML")
                .EnumerateFiles("*.html", SearchOption.TopDirectoryOnly)
                .Where(file => !file.Name.Contains('('))
                .Select(file => File.ReadAllText(file.FullName))
                .Select(text => text.RegexReplace(@"\A.*(?=<body)", "", RegexOptions.Singleline))
                .Select(text => text.RegexReplace(@"<style>([^<]*)</style>", "", RegexOptions.Singleline))
                .Select(text => text.RegexReplace(@"<[^>]+>", " ", RegexOptions.Singleline))
                .Select(text => text.RegexReplace(@"&\w+;", m => entities[m.Value], RegexOptions.Singleline))
                .Select(text => text.Replace("Keep Talking and Nobody Explodes Mod", ""))
                .Select(text => text.Replace("Keep Talking and Nobody Explodes v. 1", ""))
                .SelectMany(text =>
                {
                    var matches = text.RegexMatches(@"[-'’\w]+", RegexOptions.Singleline);
                    //var tmp = matches.Select(m => m.Value.ToUpperInvariant()).Distinct().Order().ToArray();
                    //if (text.Contains("Alchemy") || text.Contains("Lightspeed") || text.Contains("Manometers"))
                    //{
                    //    Console.WriteLine(tmp.Contains("ACTIVATE"));
                    //}
                    return matches;
                })
                //.Where(m => m.Value.All(ch => (ch >= '0' && ch <= '9') || ch == '.'))
                .Select(m => m.Value.ToUpperInvariant())
                .Where(m => m.All(ch => ch >= 'A' && ch <= 'Z'))
                .Where(word => word.Length >= 3)
                //.Where(word => Indicator.WellKnown.Concat(new[] { "NLL" }).Contains(word.ToUpperInvariant()))
                .GroupBy(word => word, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(gr => gr.Key, gr => gr.Count(), StringComparer.OrdinalIgnoreCase);

        var tt = new TextTable { ColumnSpacing = 2 };
        var row = 0;
        foreach (var kvp in words.Where(p => p.Value == 2).OrderBy(p => p.Key))
        //.OrderByDescending(p => p.Value).Take(100))
        {
            tt.SetCell(0, row, $"{row + 1}.".ToString().Color(ConsoleColor.Blue), alignment: HorizontalTextAlignment.Right);
            tt.SetCell(1, row, kvp.Value.ToString().Color(ConsoleColor.White), alignment: HorizontalTextAlignment.Right);
            tt.SetCell(2, row, kvp.Key.Color(ConsoleColor.Green));
            tt.SetCell(3, row, new string('█', kvp.Value));
            row++;
        }
        tt.WriteToConsole();
    }

    public static void FindUnsubscribedModules()
    {
        var exclusionList = new[] { "Accumulation", "Blackjack", "Button Sequence", "Complicated Buttons", "Countdown", "Dragon Energy", "Encrypted Morse", "FizzBuzz", "Font Select", "Game of Life Cruel", "Game of Life Simple", "Guitar Chords", "Ice Cream", "Micro-Modules", "Pigpen Rotations", "Scripting", "Seven Wires", "Sink", "Street Fighter", "Symbolic Password", "The Crystal Maze", "The Festive Jukebox", "The Hangover", "The Jukebox", "The Stopwatch", "The Swan", "The Time Keeper", "Third Base", "Timing is Everything", "Unfair Cipher", "Wire Spaghetti" };
        var installedWorkshopIds = new DirectoryInfo(@"D:\Steam\steamapps\workshop\content\341800").EnumerateDirectories().Select(d => d.Name).ToHashSet();
        var localIds = new DirectoryInfo(@"D:\Steam\steamapps\common\Keep Talking and Nobody Explodes\mods").EnumerateDirectories().Select(d => d.Name).ToHashSet();
        localIds.AddRange(new DirectoryInfo(@"D:\Steam\steamapps\common\Keep Talking and Nobody Explodes\tmpmods").EnumerateDirectories().Select(d => d.Name));
        localIds.AddRange(localIds.Select(i => i.RegexReplace(@"Module$", "")).ToArray());
        foreach (var module in Ktane.GetLiveJson())
        {
            if (module["SteamID"] == null || module["Type"].GetString() != "Regular")
                continue;
            if (!installedWorkshopIds.Contains(module["SteamID"].GetString()) && !localIds.Contains(module["ModuleID"].GetString()) && !localIds.Contains(module["ModuleID"].GetString().RegexReplace(@"Module$", "")))
                Console.WriteLine($"Not subscribed: {module["Name"].GetString()}");
        }
    }

    public static void CodeSizeAnalysis()
    {
        var additional = new Dictionary<string, string[]>
        {
            { "Hexamaze", new[] { "Hex.cs" } },
            { "No particular module", new string[] { "Edgework.cs", "EliasCube.cs", "GridGenerator.cs", "Ktane.cs", "MonoRandom.cs", "MorseTable.cs", "Simulations.cs", "Translatable.cs", "Utils.cs", @"Modeling\AutoNormal.cs", @"Modeling\BevelPoint.cs", @"Modeling\GaussianBlur.cs", @"Modeling\Md.cs", @"Modeling\MeshVertexInfo.cs", @"Modeling\Normal.cs", @"Modeling\Pt.cs", @"Modeling\VertexInfo.cs" } }
        };

        var allExtraFiles = new DirectoryInfo($@"D:\c\KTANE\KtaneStuff\Src").EnumerateFiles("*.cs", SearchOption.AllDirectories).ToList();
        var list = GetLiveJson()
            .Where(el => el["Author"].GetString().Contains("Timwi") && !new[] { "Lasers", "Dr. Doctor", "3D Tunnels", "Cursed Double-Oh" }.Contains(el["Name"].GetString()))
            .Concat((JsonValue) null)
            .Select(el =>
            {
                var name = el == null ? "No particular module" : el["Name"].GetString();
                var id = el == null ? "No particular module" : el["ModuleID"].GetString();
                var dir = $@"D:\c\KTANE\{name.Replace(" ", "")}";
                if (!Directory.Exists(dir))
                    dir = $@"D:\c\KTANE\{id.RegexReplace(@"Module$", "")}";
                var bytes = 0L;
                if (Directory.Exists(dir))
                {
                    foreach (var file in new DirectoryInfo(Path.Combine(dir, "Assets")).EnumerateFiles("*.cs", SearchOption.AllDirectories))
                        if (!new[] { "Editor", "KMScripts", "Plugins", "Shaders", "TestHarness" }.Any(d => file.FullName.Contains($@"Assets\{d}")) && !new[] { "KMBombInfoExtensions.cs", "Ut.cs", "DummyModule.cs", "Data.cs", "ExtensionMethods.cs" }.Contains(file.Name))
                        {
                            Console.WriteLine(file.FullName);
                            bytes += new FileInfo(file.FullName).Length;
                        }
                    Console.WriteLine($"{name} = {bytes}");
                }

                var extra = $@"D:\c\KTANE\KtaneStuff\Src\{name.Replace(" ", "")}.cs";
                if (!File.Exists(extra))
                    extra = $@"D:\c\KTANE\KtaneStuff\Src\{id.RegexReplace(@"Module$", "")}.cs";
                var extraLoc = 0L;
                if (File.Exists(extra))
                {
                    extraLoc = new FileInfo(extra).Length;
                    allExtraFiles.RemoveAll(f => f.FullName.Equals(extra, StringComparison.InvariantCultureIgnoreCase));
                }
                foreach (var adtnl in additional.Get(name, []))
                {
                    extraLoc += new FileInfo($@"D:\c\KTANE\KtaneStuff\Src\{adtnl}").Length;
                    allExtraFiles.RemoveAll(f => f.Name.Equals(adtnl, StringComparison.InvariantCultureIgnoreCase));
                }

                var manual = el == null ? null : File.ReadAllText($@"D:\c\KTANE\Public\HTML\{name}.html");
                var manualBytes = el == null ? 0 : manual.RegexMatches(@"<script>\s*(.*?)\s*</script>", RegexOptions.Singleline).Select(m => (long) m.Groups[1].Length).Sum();

                return new { Module = name, LOC = bytes, LOC2 = extraLoc, Manual = manualBytes };
            })
            .OrderByDescending(inf => inf.LOC + inf.LOC2 + inf.Manual)
            .ToList();
        Console.WriteLine(allExtraFiles.Select(f => f.FullName).JoinString("\n"));
        list.Add(new { Module = "No particular module", LOC = 0L, LOC2 = allExtraFiles.Select(f => f.Length).Sum(), Manual = 0L });

        // Copy Excel data to clipboard
        Clipboard.SetText(list.Select(inf => $"{inf.Module}\t{inf.LOC}\t{inf.LOC2}\t{inf.Manual}").JoinString("\n"));
    }

    public static void UpdateModuleIconsHtml()
    {
        var list = GetLiveJson()
            .Where(el => el["Type"].GetString() is "Regular" or "Needy")
            .OrderBy(el => DateTime.Parse(el["Published"].GetString()))
            .Select(el => $@"<div class='module' data-module='{el["Name"].GetString().HtmlEscape()}' data-type='{el["Type"].GetString().HtmlEscape()}'></div>")
            .ToList();
        var n = list.Count;
        var w = (int) Math.Ceiling(Math.Sqrt(n));
        while (n > 1)
        {
            while (n % w != 0)
                w++;
            if ((double) w * w / n < 1.6)
                break;
            n--;
            w = (int) Math.Ceiling(Math.Sqrt(n));
        }
        list = list.Take(n).OrderBy(x => Rnd.NextDouble()).ToList();

        Utils.ReplaceInFile(@"D:\Daten\Upload\KTANE\Modules.html", "<!--%%-->", "<!--%%%-->", list.JoinString("\n"));
        Utils.ReplaceInFile(@"D:\Daten\Upload\KTANE\Modules.html", "/*w_s*/", "/*w_e*/", $"{35 * w}px");
    }

    public static JsonList GetLiveJson() => JsonDict.Parse(new HttpClient().GetStringAsync(@"https://ktane.timwi.de/json/raw").Result)["KtaneModules"].GetList();

    private static void allComponentSvgsExperimentForTabletop(IEnumerable<string> moduleNames)
    {
        const int w = 348;
        var chunkIx = 0;
        foreach (var chunk in moduleNames.Where(m =>
        {
            var ex = File.Exists(Path.Combine(@"D:\c\KTANE\Public\HTML\img\Component", $"{m}.svg"));
            if (!ex)
                Console.WriteLine("Skipping: " + m);
            return ex;
        }).Split(99))
        {
            chunkIx++;
            var svgs = chunk.Select(m => new { Svg = XDocument.Parse(File.ReadAllText($@"D:\c\KTANE\Public\HTML\img\Component\{m}.svg")).Root, Name = m }).ToArray();
            var defs = new List<XElement>();
            foreach (var svg in svgs)
                foreach (var def in svg.Svg.ElementsI("defs"))
                    foreach (var elem in def.Elements())
                    {
                        var oldId = elem.AttributeI("id").Value;
                        var oldVal = $"url(#{oldId})";
                        var newId = "d" + defs.Count;
                        var newVal = $"url(#{newId})";
                        foreach (var elem2 in svg.Svg.Descendants())
                            foreach (var attr2 in elem2.Attributes())
                                attr2.Value = attr2.Value.Replace(oldVal, newVal);
                        elem.AttributeI("id").Value = newId;
                        defs.Add(elem);
                    }
            static XName n(string name) => XName.Get(name, "http://www.w3.org/2000/svg");
            File.WriteAllText($@"D:\Daten\Upload\Tabletop Simulator\KTANE\Modules-{chunkIx}.svg",
                new XElement(n("svg"), new XAttribute("viewBox", $"0 0 {w * 10} {w * 10}"),
                    new XElement(n("defs"), defs),
                    svgs.Select((svg, ix) =>
                    {
                        var attrs = svg.Svg.Attributes().Where(a => !"xmlns,viewBox".Contains(a.Name.LocalName)).ToList();
                        var tr = attrs.FirstOrDefault(a => a.Name.LocalName == "transform");
                        var trV = $"translate({w * (ix % 10)}, {w * (ix / 10)})";
                        if (tr == null)
                        {
                            tr = new XAttribute("transform", trV);
                            attrs.Add(tr);
                        }
                        else
                            tr.Value = trV + " " + tr.Value;
                        attrs.Insert(0, new XAttribute("data-name", svg.Name));
                        return new XElement(n("g"), attrs, svg.Svg.Elements().Where(e => e.Name.LocalName != "defs"));
                    })
                )
                    .ToString());
        }
    }

    public static string GenerateModuleStatistics<TKey>(Func<DateTime, TKey> grouping, TKey[] bucketNames, bool cumulative)
    {
        var numModules = new int[bucketNames.Length];
        var numRegular = new int[bucketNames.Length];

        foreach (var module in Ktane.GetLiveJson().Where(m => m["Type"].GetString() != "Widget"))
        {
            var bucket = bucketNames.IndexOf(grouping(DateTime.Parse(module["Published"].GetString())));
            if (bucket == -1)
                Debugger.Break();
            numModules[bucket]++;
            if (module["Type"].GetString() == "Regular")
                numRegular[bucket]++;
        }

        if (cumulative)
        {
            for (var i = 1; i < bucketNames.Length; i++)
            {
                numModules[i] += numModules[i - 1];
                numRegular[i] += numRegular[i - 1];
            }
        }

        return bucketNames.Select((bck, ix) => $"{bck}\t{numRegular[ix]}\t{numModules[ix]}").JoinString("\n");
    }

    public static void FindApostrophes()
    {
        foreach (var f in new DirectoryInfo(@"D:\c\KTANE\Public\HTML").EnumerateFiles("*.html"))
        {
            var raw = File.ReadAllText(f.FullName);
            var q = CQ.CreateDocument(raw);
            foreach (var elem in q["script"])
                elem.Remove();
            var text = q["body"].Text();
            if (text.IndexOf('\"') is int p && p >= 0)
                ConsoleUtil.WriteLine($"{text.SubstringSafe(p - 20, 40).CLiteralEscape().Color(ConsoleColor.Yellow)} — {f.FullName.Color(ConsoleColor.Cyan)}", null);
        }
    }

    public static void FindQuotes()
    {
        foreach (var f in new DirectoryInfo(@"D:\c\KTANE\Public\HTML").EnumerateFiles("*.html"))
        {
            Console.WriteLine(f.FullName);
            var html = File.ReadAllText(f.FullName);
            var sb = new StringBuilder();
            var inScript = false;
            var inStyle = false;
            foreach (Match m in html.RegexMatches(@"<(?<end>/)?(?<tag>[-a-zA-Z0-9_:]+)[^>]*>|""(?<q1>[^""”<>]*)""|“(?<q2>[^""”<>]*)""|""(?<q3>[^""”<>]*)”|\r?\n|."))
            {
                if (m.Groups["tag"].Success)
                {
                    if (m.Groups["tag"].Value == "script")
                        inScript = !m.Groups["end"].Success;
                    else if (m.Groups["tag"].Value == "style")
                        inStyle = !m.Groups["end"].Success;
                    sb.Append(m.Value);
                }
                else if (m.Groups["q1"].Success)
                    sb.Append(inScript || inStyle ? m.Value : $"“{m.Groups["q1"].Value}”");
                else if (m.Groups["q2"].Success)
                    sb.Append(inScript || inStyle ? m.Value : $"“{m.Groups["q2"].Value}”");
                else if (m.Groups["q3"].Success)
                    sb.Append(inScript || inStyle ? m.Value : $"“{m.Groups["q3"].Value}”");
                else
                    sb.Append(m.Value);
            }
            var newHtml = sb.ToString();
            if (newHtml != html)
                File.WriteAllText(f.FullName, sb.ToString());
        }
    }

    public static void FindCsDeQuotes()
    {
        foreach (var lang in "Deutsch,Čeština".Split(','))
            foreach (var f in new DirectoryInfo(@"D:\c\KTANE\Public\HTML").EnumerateFiles($"*{lang}*.html"))
            {
                Console.WriteLine(f.FullName);
                var html = File.ReadAllText(f.FullName);
                if (html.Contains('„'))
                    continue;
                if (!html.Contains('“'))
                    continue;
                html = html.Replace('“', '„').Replace('”', '“');
                File.WriteAllText(f.FullName, html);
            }
    }

    public static void FixLangAttributes()
    {
        var langCodes = new Dictionary<string, string>
        {
            ["Čeština"] = "cs",
            ["Deutsch"] = "de",
            ["English"] = "en",
            ["Français"] = "fr",
            ["日本語"] = "ja",
            ["Español"] = "es",
            ["Български"] = "bg",
            ["Magyar"] = "hu",
            ["العربية"] = "ar",
            ["ภาษาไทย"] = "th",
            ["Nederlands"] = "nl",
            ["Italiano"] = "it",
            ["Polski"] = "pl",
            ["简体中文"] = "zh-CN",
            ["Português"] = "pt",
            ["繁體中文"] = "zh-TW",
            ["Frysk"] = "fy",
            ["Svenska"] = "sv",
            ["Valencià"] = "ca",
            ["Norsk"] = "no",
            ["עברית"] = "he",
            ["Türkçe"] = "tr",
            ["Русский"] = "ru",
        };
        foreach (var f in new DirectoryInfo(@"D:\c\KTANE\Public\HTML").EnumerateFiles($"*.html"))
        {
            Console.Write($"{f.Name}   \r");
            var html = File.ReadAllText(f.FullName);
            if (html.Contains("<html lang="))
                continue;

            var langName = f.Name.RegexMatch(@"translated \((.*) —", out var m) ? m.Groups[1].Value : "English";
            if (!langCodes.ContainsKey(langName))
            {
                Clipboard.SetText(langName);
                Debugger.Break();
            }
            var langCode = langCodes[langName];
            if (!html.RegexMatch(@"<html>", out var tag))
            {
                Clipboard.SetText(f.FullName);
                Debugger.Break();
            }
            html = $@"{html.Substring(0, tag.Index)}<html lang=""{langCode}"">{html.Substring(tag.Index + tag.Length)}";
            File.WriteAllText(f.FullName, html);
        }
    }

    internal static void FixLinksInManuals()
    {
        foreach (var file in new DirectoryInfo(@"D:\c\KTANE\Public\HTML").EnumerateFiles("*.html", SearchOption.AllDirectories))
        {
            tryAgain:
            Console.WriteLine(file.Name);
            var html = File.ReadAllText(file.FullName);
            foreach (var match in html.RegexMatches(@"href='([^']*)'|href=""([^""]*)""", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
            {
                var url = (match.Groups[1].Success ? match.Groups[1].Value : match.Groups[2].Value).Replace("&#39;", "'").RegexReplace(@"^(\./|\.\./HTML/|/HTML/)", "");
                if (!url.Contains('%'))
                    continue;
                static string valid(string xyz) => Directory.EnumerateFiles(@"D:\c\KTANE\Public\HTML", xyz).FirstOrDefault().NullOr(p => PathUtil.ToggleRelative(@"D:\c\KTANE\Public\HTML", p));

                if (!url.EndsWith(".html") || url.StartsWith("http") || (url.StartsWith("../") && !url.StartsWith("../HTML/")))
                    continue;

                url = url.UrlUnescape();
                foreach (var alternate in Ut.NewArray(
                    url.Replace(" &amp; ", ", "),
                    url.Replace(".html", "*.html"),
                    url.RegexReplace(@"^(.*) \([^\)]*\)", m => $"{m.Groups[1].Value}*")
                ))
                    if (valid(alternate) is { } newFileName)
                    {
                        File.WriteAllText(file.FullName, html.Remove(match.Index, match.Length).Insert(match.Index, $@"href='{newFileName.HtmlEscape()}'"));
                        goto tryAgain;
                    }

                ConsoleUtil.WriteLine(url.Color(ConsoleColor.Red));
                Debugger.Break();
            }
        }
    }

    public static void FindStaleLogfiles()
    {
        var logDir = @"F:\KtaneLogfiles";
        var canDel = new List<string>();
        foreach (var logFile in new DirectoryInfo(logDir).GetFiles("*.txt"))
        {
            var any = false;
            int? inEvent = null;
            var eventSb = new StringBuilder();
            var anySuccessfulBombs = false;
            foreach (var line in File.ReadLines(logFile.FullName))
            {
                if (inEvent == null && line.RegexMatch(@"^\[Tweaks\] LFAEvent (\d+)$", out var mEv) && int.TryParse(mEv.Groups[1].Value, out var evLines))
                {
                    inEvent = evLines;
                    eventSb.Clear();
                }
                else if (inEvent != null)
                {
                    eventSb.Append(line);
                    inEvent = inEvent.Value - 1;
                    if (inEvent == 0)
                    {
                        inEvent = null;
                        var json = JsonValue.Parse(eventSb.ToString());
                        if (!json.TryGetValue("type", out var typeVal) || typeVal.GetStringSafe() is not { } type)
                            continue;
                        if (type is "ROUND_START")
                            any = true;
                        else if (type is "BOMB_SOLVE")
                            anySuccessfulBombs = true;
                    }
                }
            }
            if (any && !anySuccessfulBombs)
            {
                ConsoleUtil.WriteLineFmt($"I think we can delete {logFile.Name:G}");
                canDel.Add(logFile.Name);
                //File.Delete(logFile.FullName);
            }
            else
                ConsoleUtil.WriteLineFmt($"I think we shall keep {logFile.Name:M}");
        }
        Console.WriteLine();
        //File.WriteAllLines(@"E:\KtaneLogfiles\New\Ktane Logfiles we can delete.txt", canDel);
    }

    public static void DeleteOldUselessLogfiles()
    {
        const string logDir = @"E:\KtaneLogfiles";

        var canDelete = File.ReadAllLines(@"E:\KtaneLogfiles\Ktane Logfiles we can delete.txt").ToHashSet();
        var today = DateTime.UtcNow.Date;
        //var rawOutput = CommandRunner.Run("7z", "l", Path.Combine(logDir, $"Ktane Logfiles {prefix}.7z")).GoGetOutputText();
        //File.WriteAllLines($@"E:\KtaneLogfiles\Temp\Deleted logfiles {prefix}.txt",
        foreach (var file in new DirectoryInfo(logDir).EnumerateFiles("*.txt"))
            if (today - file.LastWriteTimeUtc > TimeSpan.FromDays(365) && canDelete.Contains(file.Name))
            {
                Console.WriteLine(file.Name);
                File.AppendAllLines($@"E:\KtaneLogfiles\Temp\Deleted logfiles {file.Name[0..2]}.txt", [file.Name]);
                File.Delete(file.FullName);
            }
    }

    public static void LogfilesTemp()
    {
        var lockObj = new object();
        void process7z(HashSet<string> h, string output)
        {
            var idsGleaned = output.Split('\n')
                .Select(line => line.RegexMatch(@"^\d{4}-\d{2}-\d{2} \d{2}:\d{2}:\d{2} .....\s*(?:\d+\s*){1,2}([0-9a-f]{40})\.txt\s*$", RegexOptions.IgnoreCase, out var m) ? m.Groups[1].Value.ToLowerInvariant() : null)
                .Where(id => id != null)
                .ToArray();
            lock (lockObj)
                h.AddRange(idsGleaned);
        }
        void processDir(HashSet<string> h, string output)
        {
            var idsGleaned = output.Split('\n')
                .Select(line => line.RegexMatch(@"^\d\d/\d\d/\d\d\d\d  \d\d:\d\d\s*[\d,]*\s*([0-9a-f]{40})\.txt\s*$", RegexOptions.IgnoreCase, out var m) ? m.Groups[1].Value.ToLowerInvariant() : null)
                .Where(id => id != null)
                .ToArray();
            lock (lockObj)
            {
                Console.WriteLine($"dir: {idsGleaned.Length}");
                h.AddRange(idsGleaned);
            }
        }

        // Topaz
        var topazIds = new HashSet<string>();
        process7z(topazIds, CommandRunner.Run(["7z", "l", @"F:\KtaneLogfiles\KtaneLogfiles_new.7z"]).OutputNothing().GoGetOutputText());
        Enumerable.Range(0, 256).ParallelForEach(Environment.ProcessorCount, i =>
        {
            var prefix = i.ToString("x2");
            lock (lockObj)
                Console.WriteLine($"{prefix} — {topazIds.Count}");
            process7z(topazIds, CommandRunner.Run(["7z", "l", Path.Combine(@"F:\KtaneLogfilesBackup\2026-04-06", $"Ktane Logfiles {prefix}.7z")]).OutputNothing().GoGetOutputText());
        });
        processDir(topazIds, File.ReadAllText(@"F:\KtaneLogfiles\KtaneLogfiles_TOPAZ.txt"));
        Console.WriteLine(topazIds.Count);

        //// Sapphire
        //var sapphireIds = new HashSet<string>();
        //foreach (var file in new DirectoryInfo(@"F:\KtaneLogfiles\KtaneLogfiles_SAPPHIRE_7zl").EnumerateFiles("*.txt"))
        //    process7z(sapphireIds, File.ReadAllText(file.FullName));
        //Console.WriteLine(sapphireIds.Count);
        //processDir(topazIds, File.ReadAllText(@"F:\KtaneLogfiles\KtaneLogfiles_SAPPHIRE.txt"));
        //Console.WriteLine(sapphireIds.Count);

        // Missing?
        Console.WriteLine("MISSING in clipboard");

        var sam = File.ReadLines(@"E:\KtaneLogfiles\Sam_verify.txt").Select(l => l.PadLeft(40, '0')).ToHashSet();
        var recovered = File.ReadLines(@"F:\KtaneLogfiles\KtaneLogfiles_SAPPHIRE_7zl\recover.txt").Select(l => l.Replace(".txt", "")).ToHashSet();
        Clipboard.SetText(sam.Except(topazIds).Except(recovered).JoinString("\n"));
    }

    internal struct CssFontResults
    {
        public (string family, string url)[] GoodFontFamilies;
        public (string family, string url)[] BrokenFontFamilies;
        public (string selector, string family)[] ReferencedFonts;
        public ICssStyleSheet StyleSheet;
    }
    private static readonly HashSet<string> _builtinFontFamilies = @"serif,sans-serif,monospace,cursive,fantasy,system-ui,ui-serif,ui-sans-serif,ui-monospace,ui-rounded,math,fangsong,inherit,initial,revert,revert-layer,unset".Split(',').ToHashSet();

    public static void FindBrokenFontUsage()
    {
        var alreadyCss = new Dictionary<string, CssFontResults>();

        IEnumerable<string> parseFontFamilies(string rawPropertyValue) =>
            from match in rawPropertyValue.RegexMatches(@"\s*(?:""(?<dq>[^""]+)""|'(?<sq>[^']+)'|(?<raw>[^,]+))\s*")
            select (match.Groups["dq"].Success ? match.Groups["dq"].Value : match.Groups["sq"].Success ? match.Groups["sq"].Value : match.Groups["raw"].Value).Trim();

        CssFontResults dealWithStyleSheet(string css, bool isFilePath, string relativePath)
        {
            if (isFilePath && alreadyCss.TryGetValue(css, out var result))
                return result;
            ICssStyleSheet styleSheet;
            if (isFilePath)
            {
                using var cssFile = File.OpenRead(css);
                styleSheet = new AngleSharp.Css.Parser.CssParser().ParseStyleSheet(cssFile);
            }
            else
                styleSheet = new AngleSharp.Css.Parser.CssParser().ParseStyleSheet(css);

            var brokenFontFamilies = new List<(string fontFamily, string url)>();
            var goodFontFamilies = new List<(string fontFamily, string url)>();

            string sanitize(string fontName) =>
                fontName.RegexMatch(@"^""([^""]+)""$", out var dq) ? dq.Groups[1].Value.Trim() :
                fontName.RegexMatch(@"^'([^']+)'$", out var sq) ? sq.Groups[1].Value.Trim() : fontName.Trim();

            foreach (var (fontFamily, url, exists) in
                from fontFaceDeclaration in styleSheet.Rules.OfType<ICssFontFaceRule>()
                from match in fontFaceDeclaration.Source.RegexMatches(@"\burl\(\s*(?:'(?<m1>[^']+)'|""(?<m2>[^""]+)""|(?<m0>[^\)]+))\s*\)")
                let url = match.Groups["m0"].Success ? match.Groups["m0"].Value : match.Groups["m1"].Success ? match.Groups["m1"].Value : match.Groups["m2"].Value
                let absolutePath = Path.Combine(relativePath, url)
                select (sanitize(fontFaceDeclaration.Family), url, File.Exists(absolutePath)))
            {
                (exists ? goodFontFamilies : brokenFontFamilies).Add((fontFamily, url));
            }

            var referencedFonts = (
                from rule in styleSheet.Rules.OfType<ICssStyleRule>()
                from declaration in rule.Style
                where declaration.Name == "font-family"
                from fontFamily in parseFontFamilies(declaration.Value)
                where !_builtinFontFamilies.Contains(fontFamily)
                select (rule.SelectorText, fontFamily)).ToArray();

            result = new CssFontResults
            {
                GoodFontFamilies = goodFontFamilies.ToArray(),
                BrokenFontFamilies = brokenFontFamilies.ToArray(),
                ReferencedFonts = referencedFonts,
                StyleSheet = styleSheet
            };

            if (isFilePath)
                alreadyCss[css] = result;
            return result;
        }

        var output = new StringBuilder();

        foreach (var htmlFileInfo in new DirectoryInfo(@"D:\c\KTANE\Public\HTML").EnumerateFiles("*.html", SearchOption.AllDirectories))
        {
            ConsoleUtil.WriteLineFmt($"{PathUtil.ToggleRelative(@"D:\c\KTANE\Public\HTML", htmlFileInfo.FullName):Y}");
            using var htmlFile = htmlFileInfo.OpenRead();
            var htmlParsed = new AngleSharp.Html.Parser.HtmlParser().ParseDocument(htmlFile);
            var styleSheets = new List<CssFontResults>();
            foreach (var linkTag in htmlParsed.QuerySelectorAll("link[rel=stylesheet]"))
            {
                var cssLink = linkTag.GetAttribute("href");
                var cssFullPath = Path.Combine(htmlFileInfo.DirectoryName, cssLink.UrlUnescape());
                styleSheets.Add(dealWithStyleSheet(cssFullPath, isFilePath: true, relativePath: Path.GetDirectoryName(cssFullPath)));
            }
            foreach (var styleTag in htmlParsed.QuerySelectorAll("style"))
                styleSheets.Add(dealWithStyleSheet(styleTag.TextContent, isFilePath: false, relativePath: htmlFileInfo.DirectoryName));

            var brokenFontFamilies = (from styleSheet in styleSheets from tup in styleSheet.BrokenFontFamilies select tup.family).ToHashSet();
            var selectorsThatReferenceBrokenFontFamilies = (from styleSheet in styleSheets from tup in styleSheet.ReferencedFonts where brokenFontFamilies.Contains(tup.family) select tup).ToArray();
            foreach (var (selector, fontFamily) in selectorsThatReferenceBrokenFontFamilies)
                if (htmlParsed.QuerySelector(selector) is { } matchingTag)
                    output.Append($"{htmlFileInfo.Name}\t{fontFamily}\t{matchingTag.TagName}\t{matchingTag.ClassName}\t{selector}\n");

            foreach (var elem in htmlParsed.Descendants<IHtmlElement>())
                if (elem.GetAttribute("style") is { } styleAttr)
                    foreach (var declaration in new AngleSharp.Css.Parser.CssParser().ParseDeclaration(styleAttr))
                        if (declaration.Name == "font-family")
                            foreach (var fontFamily in parseFontFamilies(declaration.Value))
                                if (brokenFontFamilies.Contains(fontFamily))
                                    output.Append($"{htmlFileInfo.Name}\t{fontFamily}\t{elem.TagName}\t{elem.ClassName}\t(style attribute)\n");
        }

        File.WriteAllText(@"D:\temp\temp.txt", output.ToString());
    }
}

internal static class ExtensionMethods
{
    public static void AddSafe<K1, K2, K3, K4, V>(this IDictionary<K1, Dictionary<K2, Dictionary<K3, Dictionary<K4, V>>>> dic, K1 key1, K2 key2, K3 key3, K4 key4, V value)
    {
        ArgumentNullException.ThrowIfNull(dic);
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

        dic[key1][key2][key3][key4] = value;
    }
}
