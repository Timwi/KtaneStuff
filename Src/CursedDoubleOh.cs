using System;
using System.Collections.Generic;
using System.Linq;
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

        public static void MakeCheatSheet()
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
                                            paths.IncSafe(new[] { a, b, c, d }.Select((f, ix) => new { Fnc = f, Name = (char) ('A' + ix) })
                                                .Aggregate(new { Path = (_grid[i] / 10).ToString(), Pos = i }, (p, n) =>
                                                {
                                                    var newPos = p.Pos;
                                                    switch (n.Fnc)
                                                    {
                                                        case ButtonFunction.SmallLeft: newPos = (newPos / 3) * 3 + (newPos + 2) % 3; break;
                                                        case ButtonFunction.SmallRight: newPos = (newPos / 3) * 3 + (newPos + 1) % 3; break;
                                                        case ButtonFunction.SmallUp: newPos = (newPos / 27) * 27 + (newPos + 18) % 27; break;
                                                        case ButtonFunction.SmallDown: newPos = (newPos / 27) * 27 + (newPos + 9) % 27; break;
                                                        case ButtonFunction.LargeLeft: newPos = (newPos / 9) * 9 + (newPos + 6) % 9; break;
                                                        case ButtonFunction.LargeRight: newPos = (newPos / 9) * 9 + (newPos + 3) % 9; break;
                                                        case ButtonFunction.LargeUp: newPos = (newPos + 54) % 81; break;
                                                        case ButtonFunction.LargeDown: newPos = (newPos + 27) % 81; break;
                                                    }
                                                    return new { Path = p.Path + n.Name + (_grid[newPos] / 10), Pos = newPos };
                                                }).Path);
            }
            foreach (var kvp in paths.OrderByDescending(p => p.Value).Take(20))
                Console.WriteLine($"{kvp.Key} = {kvp.Value}");
        }
    }
}