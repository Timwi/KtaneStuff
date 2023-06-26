using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using RT.Util;
using RT.Util.Consoles;
using RT.Util.ExtensionMethods;

namespace KtaneStuff
{
    public static class TpExtravaganza
    {
        public static void FindPopularKtaneTpModules()
        {
            string[] recentLogFiles;
            var recentPath = @"D:\temp\KTANE\recent_logfiles.txt";
            if (File.Exists(recentPath))
                recentLogFiles = File.ReadAllLines(recentPath);
            else
            {
                var list = new List<string>();
                foreach (var file in new DirectoryInfo(@"D:\temp\KTANE").EnumerateFiles("*.txt"))
                //if (File.GetLastWriteTimeUtc(file.FullName) >= DateTime.UtcNow.AddDays(-100))
                {
                    Console.Write($"{file.Name}\r");
                    if (File.ReadLines(file.FullName).Any(line => line.StartsWith("[TwitchPlays] [IRCConnection] [M]")))
                    {
                        ConsoleUtil.WriteLine($"{file.Name}: FOUND".Color(ConsoleColor.Yellow));
                        list.Add(file.Name);
                    }
                }
                recentLogFiles = list.ToArray();
                File.WriteAllLines(recentPath, recentLogFiles);
            }

            var popularity = new Dictionary<string, int>();
            foreach (var filename in recentLogFiles)
            {
                var log = File.ReadAllLines(Path.Combine(@"D:\temp\KTANE", filename));
                var bgFound = false;
                string[] moduleNames = null;
                foreach (var bg in log)
                    if (bg.StartsWith("[BombGenerator] Bomb component list:"))
                    {
                        if (bgFound)
                        {
                            ConsoleUtil.WriteLine($"{filename}: multiple bomb generator lines".Color(ConsoleColor.Red));
                            goto busted;
                        }
                        var m = Regex.Match(bg, @"^\[BombGenerator\] Bomb component list: RequiresTimerVisibility \[(.*)\], AnyFace: \[(.*)\]$");
                        if (!m.Success)
                        {
                            ConsoleUtil.WriteLine($"{filename}: bomb generator does not match regex".Color(ConsoleColor.Red));
                            goto busted;
                        }
                        bgFound = true;
                        moduleNames = m.Groups[1].Value.Apply(s => s.Length == 0 ? new string[0] : s.Split(", "))
                            .Concat(m.Groups[2].Value.Apply(s => s.Length == 0 ? new string[0] : s.Split(", ")))
                            .Select(s => Regex.Match(s, @"^(.*) \(.*\)$") is Match m && m.Success ? m.Groups[1].Value : s)
                            .ToArray();
                    }
                if (!bgFound)
                    goto busted;

                ConsoleUtil.WriteLine($"{filename}: Processing".Color(ConsoleColor.Green));
                var numMatches = 0;
                foreach (var chatLine in log)
                {
                    if (chatLine.StartsWith(@"[TwitchPlays] [IRCConnection] [M]"))
                    {
                        int[] moduleIds = null;
                        if (Regex.Match(chatLine, @"^\[TwitchPlays\] \[IRCConnection\] \[M\] .* \((?:#[0-9A-F]{6})?(?:, \d+)?\): !(\d+) +(?:claim|claimview|viewclaim|cv)$") is Match m && m.Success)
                            moduleIds = new[] { int.Parse(m.Groups[1].Value) };
                        else if (Regex.Match(chatLine, @"^\[TwitchPlays\] \[IRCConnection\] \[M\] .* \((?:#[0-9A-F]{6})?(?:, \d+)?\): !claim +([\d ]+)$") is Match m2 && m2.Success)
                            moduleIds = m2.Groups[1].Value.Split(' ').Select(int.Parse).ToArray();
                        if (moduleIds != null)
                        {
                            foreach (var moduleId in moduleIds)
                                if (moduleId >= 1 && moduleId <= moduleNames.Length)
                                    popularity.IncSafe(moduleNames[moduleId - 1]);
                            numMatches++;
                            if (numMatches >= 10)
                                break;
                        }
                    }
                }

                busted:;
            }

            File.WriteAllLines(@"D:\temp\KTANE\popularity.txt", popularity.OrderByDescending(v => v.Value).Select(v => $"{v.Value}\t{v.Key}"));
        }
    }
}