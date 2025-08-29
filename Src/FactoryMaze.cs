using RT.Util.Consoles;
using RT.Util.ExtensionMethods;

namespace KtaneStuff;

internal static class FactoryMaze
{
    private static readonly int[,,] _mazes = {
        { { 1, 2 }, { 2, 3 }, { 3, 4 }, { 4, 0 }, { 0, 1 } },
        { { 1, 3 }, { 2, 4 }, { 0, 3 }, { 1, 4 }, { 0, 2 } },
        { { 2, 3 }, { 0, 4 }, { 1, 4 }, { 1, 2 }, { 0, 3 } } };
    private static readonly int[,,] _defaultKeys = {
        { { 2, 3, 0 }, { 4, 1, 2 }, { 0, 1, 3 }, { 3, 4, 99 }, { 1, 2, 99 } },
        { { 3, 4, 0 }, { 0, 1, 1 }, { 2, 0, 4 }, { 3, 1, 99 }, { 4, 0, 99 } },
        { { 4, 3, 0 }, { 0, 3, 2 }, { 2, 1, 3 }, { 1, 4, 99 }, { 4, 0, 99 } } };

    public static void CreateGoodsheet()
    {
        var dic = new Dictionary<string, HashSet<string>>();
        for (var maze = 0; maze < 3; maze++)
        {
            IEnumerable<(string, string)> recurse((int from, int to, bool key)[] sofar, int cur)
            {
                var p = sofar.IndexOf(sf => sf.from == cur);
                if (p != -1)
                {
                    yield return ($"{maze + 1}/{sofar.Skip(p).Select(inf => inf.from + 1).JoinString()}", sofar.Skip(p).Select(inf => inf.key ? "+" : "-").JoinString());
                    yield break;
                }

                for (var i = 0; i < 2; i++)
                {
                    var to = _mazes[maze, cur, i];
                    foreach (var result in recurse(sofar.Append((cur, to, Enumerable.Range(0, 5).Any(j => (_defaultKeys[maze, j, 0] == cur && _defaultKeys[maze, j, 1] == to) || (_defaultKeys[maze, j, 1] == cur && _defaultKeys[maze, j, 0] == to)))).ToArray(), to))
                        yield return result;
                }
            }

            string lexicographicallySmallestRotation(string input)
            {
                var smallest = input;
                for (var i = 0; i < input.Length; i++)
                {
                    var newStr = string.Concat(input.AsSpan(i), input.AsSpan(0, i));
                    if (newStr.CompareTo(smallest) < 0)
                        smallest = newStr;
                }
                return smallest;
            }

            for (var i = 0; i < 5; i++)
                foreach (var (full, abbrev) in recurse([], i))
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
