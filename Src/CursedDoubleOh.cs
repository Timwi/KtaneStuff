using System;
using System.Collections.Generic;
using System.Linq;
using RT.Dijkstra;
using RT.Util;
using RT.Util.ExtensionMethods;

namespace KtaneStuff
{
    static class CursedDoubleOh
    {
        enum ButtonFunction
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
            11 54 80 32 77 45 23 66 08".Trim().Replace("\r", "").Split(new[] { ' ', '\n' }, StringSplitOptions.RemoveEmptyEntries).Select(str => int.Parse(str)).ToArray();

        public static void AnalysisABCD()
        {
            var fncs = EnumStrong.GetValues<ButtonFunction>();
            var paths = new Dictionary<string, int>();
            for (int i = 0; i < 81; i++)
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
                                                    var newPos = Move(p.Pos, n.Fnc);
                                                    return new { Path = p.Path + n.Name + (_grid[newPos] / 10), Pos = newPos };
                                                }).Path);
            }
            foreach (var kvp in paths.OrderByDescending(p => p.Value).Take(20))
                Console.WriteLine($"{kvp.Key} = {kvp.Value}");
            Console.WriteLine(paths.Count);
        }

        public static void AnalysisABAB()
        {
            var fncs = EnumStrong.GetValues<ButtonFunction>();
            var paths = new Dictionary<string, int>();
            for (int i = 0; i < 81; i++)
            {
                foreach (var a in fncs)
                    foreach (var b in fncs)
                        if ((int) a >> 1 != (int) b >> 1)
                            //foreach (var c in fncs)
                            //    if ((int) a >> 1 != (int) c >> 1 && (int) b >> 1 != (int) c >> 1)
                            paths.IncSafe(new[] { 0, 1, 0, 1 }.Select(f => new { Fnc = new[] { a, b }[f], Name = (char) ('A' + f) })
                                .Aggregate(new { Path = (_grid[i] / 10).ToString(), Pos = i }, (p, n) =>
                                {
                                    var newPos = Move(p.Pos, n.Fnc);
                                    return new { Path = p.Path + n.Name + (_grid[newPos] / 10), Pos = newPos };
                                }).Path);
            }
            foreach (var kvp in paths.OrderByDescending(p => p.Value).Take(20))
                Console.WriteLine($"{kvp.Key} = {kvp.Value}");
            Console.WriteLine(paths.Count);
        }

        private static int Move(int pos, ButtonFunction fnc)
        {
            switch (fnc)
            {
                case ButtonFunction.SmallLeft: return (pos / 3) * 3 + (pos + 2) % 3;
                case ButtonFunction.SmallRight: return (pos / 3) * 3 + (pos + 1) % 3;
                case ButtonFunction.SmallUp: return (pos / 27) * 27 + (pos + 18) % 27;
                case ButtonFunction.SmallDown: return (pos / 27) * 27 + (pos + 9) % 27;
                case ButtonFunction.LargeLeft: return (pos / 9) * 9 + (pos + 6) % 9;
                case ButtonFunction.LargeRight: return (pos / 9) * 9 + (pos + 3) % 9;
                case ButtonFunction.LargeUp: return (pos + 54) % 81;
                case ButtonFunction.LargeDown: return (pos + 27) % 81;
            }
            return -1;
        }

        sealed class CDNode : Node<int, int>
        {
            public int Pos { get; private set; }
            private readonly ButtonFunction[] _btnFncs;
            private readonly ButtonFunction _last;
            private readonly HashSet<int> _taken;
            public CDNode(int pos, ButtonFunction[] btnFncs, ButtonFunction last, HashSet<int> taken)
            {
                Pos = pos;
                _btnFncs = btnFncs;
                _last = last;
                _taken = taken;
            }

            public override bool IsFinal => Pos == 4 + 9 * 4;
            public override IEnumerable<Edge<int, int>> Edges => Enumerable.Range(0, _btnFncs.Length)
                .Where(i => _btnFncs[i] != _last)
                .Select(i => new { NewPos = Move(Pos, _btnFncs[i]), Ix = i })
                .Where(inf => !_taken.Contains(inf.NewPos))
                .Select(inf => new Edge<int, int>(1, inf.Ix, new CDNode(inf.NewPos, _btnFncs, _btnFncs[inf.Ix], _taken)));
            public override bool Equals(Node<int, int> other) => other is CDNode cd ? cd.Pos == Pos : false;
            public override int GetHashCode() => Pos;
        }

        private static string FindSolution(int position, HashSet<int> positionsTaken, ButtonFunction[] btnFncs, ButtonFunction last) =>
            DijkstrasAlgorithm.Run(new CDNode(position, btnFncs, last, positionsTaken), 0, (a, b) => a + b, out var totalWeight).Select(i => "ABCD"[i.Label]).JoinString();
    }
}