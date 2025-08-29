using System.Text.RegularExpressions;
using RT.Json;
using RT.Modeling;
using RT.Util.Consoles;
using RT.Util.ExtensionMethods;
using static RT.Modeling.Md;

namespace KtaneStuff;

internal static class Souvenir
{
    public static void DoModels()
    {
        File.WriteAllText(@"D:\c\KTANE\Souvenir\Assets\Models\AnswerHighlightVeryLong.obj", GenerateObjFile(answerHighlight(.39), "AnswerHighlightVeryLong"));
        File.WriteAllText(@"D:\c\KTANE\Souvenir\Assets\Models\AnswerHighlightLong.obj", GenerateObjFile(answerHighlight(.21), "AnswerHighlightLong"));
        File.WriteAllText(@"D:\c\KTANE\Souvenir\Assets\Models\AnswerHighlightShort.obj", GenerateObjFile(answerHighlight(.15), "AnswerHighlightShort"));
    }

    private static IEnumerable<Pt[]> answerHighlight(double width)
    {
        var height = .08;
        var padding = .02;
        var right = width - height / 2;
        return
            Enumerable.Range(0, 37)
                .Select(i => i * 180 / 36 + 90)
                .Select(angle => new { Inner = pt((height - padding) / 2 * cos(angle), 0, (height - padding) / 2 * sin(angle)), Outer = pt(height / 2 * cos(angle), 0, height / 2 * sin(angle)) })
                .SelectConsecutivePairs(false, (i1, i2) => new[] { i1.Inner, i1.Outer, i2.Outer, i2.Inner })
            .Concat([pt(0, 0, -height / 2), pt(right, 0, -height / 2), pt(right, 0, -(height - padding) / 2), pt(0, 0, -(height - padding) / 2)])
            .Concat([pt(0, 0, (height - padding) / 2), pt(right, 0, (height - padding) / 2), pt(right, 0, height / 2), pt(0, 0, height / 2)])
            .Concat([pt(right, 0, -height / 2), pt(right, 0, height / 2), pt(right - height / 3, 0, 0)]);
    }

    public static void FindHangingCoroutinesInLogfiles()
    {
        //foreach (var file in new DirectoryInfo(@"F:\KtaneLogfiles").EnumerateFiles("*.txt", SearchOption.TopDirectoryOnly))
        //foreach (var file in new DirectoryInfo(@"D:\temp").EnumerateFiles("*.txt", SearchOption.TopDirectoryOnly))
        foreach (var file in new DirectoryInfo(@"C:\Users\Timwi\AppData\LocalLow\Steel Crate Games\Keep Talking and Nobody Explodes").EnumerateFiles("*.txt", SearchOption.TopDirectoryOnly))
        {
            var contents = File.ReadLines(file.FullName);
            var running = new Dictionary<string, int>();
            foreach (var line in contents)
                if (line.RegexMatch(@"^[<‹]Souvenir #\d+[>›] Module (\w+): (Start processing|Finished processing)\.$", out var m))
                    running.IncSafe(m.Groups[1].Value, m.Groups[2].Value.Equals("Start processing") ? 1 : -1);
            foreach (var kvp in running)
                if (kvp.Value != 0)
                    ConsoleUtil.WriteLine($"{file.Name.Color(ConsoleColor.Red)}: {kvp.Key.Color(ConsoleColor.Cyan)} is {kvp.Value.ToString().Color(ConsoleColor.Magenta)}", null);
        }
    }

    public static void UpdateJs()
    {
        var file = File.ReadAllLines(@"D:\c\KTANE\Public\HTML\js\Modules\Souvenir.js");
        var modules = Ktane.GetLiveJson().Where(md => md["Type"].GetString() != "Appendix").ToDictionary(md => md["Name"].GetString(), md => (id: md["ModuleID"].GetString(), filename: md.Safe["FileName"]?.GetString() ?? md["Name"].GetString()));
        for (var i = 0; i < file.Length; i++)
            if (file[i].RegexMatch(@"name: ""([^""]*)"",\tid: ""\?""", out var m) && modules.Get(m.Groups[1].Value.Replace("’", "'"), null) is { } tup)
            {
                file[i] = file[i].Remove(m.Index, m.Length).Insert(m.Index, $"name: \"{m.Groups[1].Value}\",\tid: \"{tup.id}\"");
                var json = JsonDict.Parse(File.ReadAllText($@"D:\c\KTANE\Public\JSON\{tup.filename}.json"));
                json["Souvenir"] = new JsonDict { ["Status"] = "Supported" };
                File.WriteAllText($@"D:\c\KTANE\Public\JSON\{tup.filename}.json", json.ToStringIndented());
            }
        File.WriteAllLines(@"D:\c\KTANE\Public\HTML\js\Modules\Souvenir.js", file);
    }
}
