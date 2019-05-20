using System;
using System.Collections.Generic;
using System.Linq;
using RT.Util.Consoles;
using RT.Util.ExtensionMethods;

namespace KtaneStuff
{
    static class FactoryMaze
    {
        private static readonly int[,,] mazes = {
            { { 1, 2 }, { 2, 3 }, { 3, 4 }, { 4, 0 }, { 0, 1 } },
            { { 1, 3 }, { 2, 4 }, { 0, 3 }, { 1, 4 }, { 0, 2 } },
            { { 2, 3 }, { 0, 4 }, { 1, 4 }, { 1, 2 }, { 0, 3 } } };
        private static readonly int[,,] defaultKeys = {
            { { 2, 3, 0 }, { 4, 1, 2 }, { 0, 1, 3 }, { 3, 4, 99 }, { 1, 2, 99 } },
            { { 3, 4, 0 }, { 0, 1, 1 }, { 2, 0, 4 }, { 3, 1, 99 }, { 4, 0, 99 } },
            { { 4, 3, 0 }, { 0, 3, 2 }, { 2, 1, 3 }, { 1, 4, 99 }, { 4, 0, 99 } } };

        public static void CreateGoodsheet()
        {
            var dic = new Dictionary<string, HashSet<string>>();
            for (int maze = 0; maze < 3; maze++)
            {
                IEnumerable<(string, string)> recurse((int from, int to, bool key)[] sofar, int cur)
                {
                    var p = sofar.IndexOf(sf => sf.from == cur);
                    if (p != -1)
                    {
                        yield return ($"{maze + 1}/{sofar.Skip(p).Select(inf => inf.from + 1).JoinString()}", sofar.Skip(p).Select(inf => inf.key ? "+" : "-").JoinString());
                        yield break;
                    }

                    for (int i = 0; i < 2; i++)
                    {
                        var to = mazes[maze, cur, i];
                        foreach (var result in recurse(sofar.Append((cur, to, Enumerable.Range(0, 5).Any(j => (defaultKeys[maze, j, 0] == cur && defaultKeys[maze, j, 1] == to) || (defaultKeys[maze, j, 1] == cur && defaultKeys[maze, j, 0] == to)))).ToArray(), to))
                            yield return result;
                    }
                }

                string lexicographicallySmallestRotation(string input)
                {
                    var smallest = input;
                    for (int i = 0; i < input.Length; i++)
                    {
                        var newStr = input.Substring(i) + input.Substring(0, i);
                        if (newStr.CompareTo(smallest) < 0)
                            smallest = newStr;
                    }
                    return smallest;
                }

                for (int i = 0; i < 5; i++)
                    foreach (var (full, abbrev) in recurse(new (int, int, bool)[0], i))
                        if (abbrev == lexicographicallySmallestRotation(abbrev))
                            dic.AddSafe(abbrev, full);
            }

            foreach (var kvp in dic.OrderBy(k => k.Key.Length).ThenBy(k => k.Key))
            {
                ConsoleUtil.WriteLine(kvp.Key.Color(ConsoleColor.White));
                foreach (var elem in kvp.Value)
                    ConsoleUtil.WriteLine($" — {elem.Color(ConsoleColor.Green)}", null);
                Console.WriteLine();
            }
        }
    }
}