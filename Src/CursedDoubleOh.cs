using RT.Dijkstra;
using RT.Util;
using RT.Util.ExtensionMethods;

namespace KtaneStuff;

internal static class CursedDoubleOh
{
    private enum buttonFunction
    {
        SmallLeft,
        SmallRight,
        SmallUp,
        SmallDown,
        LargeLeft,
        LargeRight,
        LargeUp,
        LargeDown
    }

    private static readonly int[] _grid = @"
            60 02 15 57 36 83 48 71 24
            88 46 31 70 22 64 07 55 13
            74 27 53 05 41 18 86 30 62
            52 10 04 43 85 37 61 28 76
            33 65 78 21 00 56 12 44 87
            47 81 26 68 14 72 50 03 35
            06 38 42 84 63 20 75 17 51
            25 73 67 16 58 01 34 82 40
            11 54 80 32 77 45 23 66 08".Trim().Replace("\r", "").Split([' ', '\n'], StringSplitOptions.RemoveEmptyEntries).Select(int.Parse).ToArray();

    public static void AnalysisABCD()
    {
        var fncs = EnumStrong.GetValues<buttonFunction>();
        var paths = new Dictionary<string, int>();
        for (var i = 0; i < 81; i++)
        {
            foreach (var a in fncs)
                foreach (var b in fncs)
                    if ((int) a >> 1 != (int) b >> 1)
                        foreach (var c in fncs)
                            if ((int) a >> 1 != (int) c >> 1 && (int) b >> 1 != (int) c >> 1)
                                foreach (var d in fncs)
                                    if ((int) a >> 1 != (int) d >> 1 && (int) b >> 1 != (int) d >> 1 && (int) c >> 1 != (int) d >> 1)
                                        paths.IncSafe(new[] { 0, 1, 2, 3, 0, 1, 2, 3, 0, 1, 2 }.Select(f => new { Fnc = new[] { a, b, c, d }[f], Name = (char) ('A' + f) })
                                            .Aggregate(new { Path = (_grid[i] / 10).ToString(), Pos = i }, (p, n) =>
                                            {
                                                var newPos = move(p.Pos, n.Fnc);
                                                return new { Path = p.Path + n.Name + (_grid[newPos] / 10), Pos = newPos };
                                            }).Path);
        }
        foreach (var kvp in paths.OrderByDescending(p => p.Value).Take(20))
            Console.WriteLine($"{kvp.Key} = {kvp.Value}");
        Console.WriteLine(paths.Count);
    }

    public static void AnalysisABAB()
    {
        var fncs = EnumStrong.GetValues<buttonFunction>();
        var paths = new Dictionary<string, int>();
        for (var i = 0; i < 81; i++)
        {
            foreach (var a in fncs)
                foreach (var b in fncs)
                    if ((int) a >> 1 != (int) b >> 1)
                        //foreach (var c in fncs)
                        //    if ((int) a >> 1 != (int) c >> 1 && (int) b >> 1 != (int) c >> 1)
                        paths.IncSafe(new[] { 0, 1, 0, 1 }.Select(f => new { Fnc = new[] { a, b }[f], Name = (char) ('A' + f) })
                            .Aggregate(new { Path = (_grid[i] / 10).ToString(), Pos = i }, (p, n) =>
                            {
                                var newPos = move(p.Pos, n.Fnc);
                                return new { Path = p.Path + n.Name + (_grid[newPos] / 10), Pos = newPos };
                            }).Path);
        }
        foreach (var kvp in paths.OrderByDescending(p => p.Value).Take(20))
            Console.WriteLine($"{kvp.Key} = {kvp.Value}");
        Console.WriteLine(paths.Count);
    }

    private static int move(int pos, buttonFunction fnc)
    {
        return fnc switch
        {
            buttonFunction.SmallLeft => (pos / 3) * 3 + (pos + 2) % 3,
            buttonFunction.SmallRight => (pos / 3) * 3 + (pos + 1) % 3,
            buttonFunction.SmallUp => (pos / 27) * 27 + (pos + 18) % 27,
            buttonFunction.SmallDown => (pos / 27) * 27 + (pos + 9) % 27,
            buttonFunction.LargeLeft => (pos / 9) * 9 + (pos + 6) % 9,
            buttonFunction.LargeRight => (pos / 9) * 9 + (pos + 3) % 9,
            buttonFunction.LargeUp => (pos + 54) % 81,
            buttonFunction.LargeDown => (pos + 27) % 81,
            _ => -1,
        };
    }

    private sealed class cdNode(int pos, buttonFunction[] btnFncs, buttonFunction last, HashSet<int> taken) : Node<int, int>
    {
        public int Pos { get; private set; } = pos;

        public override bool IsFinal => Pos == 4 + 9 * 4;
        public override IEnumerable<Edge<int, int>> Edges => Enumerable.Range(0, btnFncs.Length)
            .Where(i => btnFncs[i] != last)
            .Select(i => new { NewPos = move(Pos, btnFncs[i]), Ix = i })
            .Where(inf => !taken.Contains(inf.NewPos))
            .Select(inf => new Edge<int, int>(1, inf.Ix, new cdNode(inf.NewPos, btnFncs, btnFncs[inf.Ix], taken)));
        public override bool Equals(Node<int, int> other) => other is cdNode cd && cd.Pos == Pos;
        public override int GetHashCode() => Pos;
    }

    private static string findSolution(int position, HashSet<int> positionsTaken, buttonFunction[] btnFncs, buttonFunction last) =>
        DijkstrasAlgorithm.Run(new cdNode(position, btnFncs, last, positionsTaken), 0, (a, b) => a + b, out var totalWeight).Select(i => "ABCD"[i.Label]).JoinString();
}
