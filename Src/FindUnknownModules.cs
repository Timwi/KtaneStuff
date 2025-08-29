using System.Text.RegularExpressions;
using RT.Json;
using RT.Modeling;
using RT.Util.ExtensionMethods;

namespace KtaneStuff;

internal static class FindUnknownModules
{
    public static void Do()
    {
        var modules = Ktane.GetLiveJson().Where(m => m["Type"].GetString() is not "Widget" and not "Holdable").Select(m => m["ModuleID"].GetString()).ToHashSet();
        var ids = new HashSet<string>();
        var allFiles = new DirectoryInfo(@"D:\KTANELogfiles").EnumerateFiles("*.txt").ToArray();

        for (var fileIx = 0; fileIx < allFiles.Length; fileIx++)
        {
            Console.Write($"Processing file {fileIx}/{allFiles.Length} \r");
            var file = allFiles[fileIx];
            var content = File.ReadAllLines(file.FullName);
            var blah = content.Select((line, ix) => new { Match = line.RegexMatch(@"^\[Tweaks\] LFABombInfo (\d+)$", out var m) ? m : null, Index = ix })
                .Where(inf => inf.Match.Success)
                .Select(inf => new { StartIndex = inf.Index + 1, Length = int.Parse(inf.Match.Groups[1].Value) });
            foreach (var inf in blah)
            {
                var json = JsonValue.Parse(content.Subarray(inf.StartIndex, inf.Length).JoinString());
                foreach (var kvp in json["ids"].GetDict())
                    if (!modules.Contains(kvp.Key))
                        if (ids.Add(kvp.Key))
                        {
                            Console.Write($"                                             \r");
                            Console.WriteLine(kvp.Key);
                        }
            }
        }
        Console.WriteLine();
        Console.WriteLine($"Found {ids.Count} unknown IDs.");
        File.WriteAllLines("module-ids-output.txt", ids);
    }
}
