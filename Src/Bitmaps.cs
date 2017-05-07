using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using KtaneStuff.Modeling;
using RT.Util;
using RT.Util.Consoles;
using RT.Util.ExtensionMethods;
using RT.Util.Text;

namespace KtaneStuff
{
    using static Edgework;
    using static Md;

    static class Bitmaps
    {
        public static void DoModels()
        {
            File.WriteAllText(@"D:\c\KTANE\Bitmaps\Assets\Models\Screen.obj", GenerateObjFile(Screen(), "Screen"));
            File.WriteAllText(@"D:\c\KTANE\Bitmaps\Assets\Models\ScreenFrame.obj", GenerateObjFile(ScreenFrame(), "ScreenFrame"));
            File.WriteAllText(@"D:\c\KTANE\Bitmaps\Assets\Models\Button.obj", GenerateObjFile(Button(), "Button"));
            File.WriteAllText(@"D:\c\KTANE\Bitmaps\Assets\Models\ButtonHighlight.obj", GenerateObjFile(ButtonHighlight(), "ButtonHighlight"));
        }

        private static IEnumerable<Pt[]> Screen()
        {
            yield return new[] { pt(-1, 0, -1), pt(-1, 0, 1), pt(1, 0, 1), pt(1, 0, -1) };
        }

        private static IEnumerable<VertexInfo[]> ScreenFrame()
        {
            var h = .04;
            var f = h * .4;
            var w = .12;
            var size = 1.0;
            var diag = size * Math.Sqrt(2);
            var roundSteps = 9;
            var arr = SmoothBézier(p(0, 0), p(0, f), p(h - f, h), p(h, h), .0001)
                    .Select((p, first, last) => new BevelPoint(p.X, p.Y, first || last ? Normal.Mine : Normal.Average, first || last ? Normal.Mine : Normal.Average))
                    .Concat(SmoothBézier(p(w - h, h), p(w - h + f, h), p(w, f), p(w, 0), .0001)
                        .Select((p, first, last) => new BevelPoint(p.X, p.Y, first || last ? Normal.Mine : Normal.Average, first || last ? Normal.Mine : Normal.Average)))
                .Select(bi =>
                    Enumerable.Range(0, roundSteps).Select((i, ft, lt) => new { Normal = ft || lt ? Normal.Mine : Normal.Average, Angle = i * 90 / (roundSteps - 1), X = 1, Y = 1 }).Concat(
                    Enumerable.Range(0, roundSteps).Select((i, ft, lt) => new { Normal = ft || lt ? Normal.Mine : Normal.Average, Angle = i * 90 / (roundSteps - 1) + 90, X = -1, Y = 1 })).Concat(
                    Enumerable.Range(0, roundSteps).Select((i, ft, lt) => new { Normal = ft || lt ? Normal.Mine : Normal.Average, Angle = i * 90 / (roundSteps - 1) + 180, X = -1, Y = -1 })).Concat(
                    Enumerable.Range(0, roundSteps).Select((i, ft, lt) => new { Normal = ft || lt ? Normal.Mine : Normal.Average, Angle = i * 90 / (roundSteps - 1) + 270, X = 1, Y = -1 }))
                        .Select(inf => pt(size * inf.X + bi.Into * cos(inf.Angle), bi.Y, size * inf.Y + bi.Into * sin(inf.Angle), bi.Before, bi.After, inf.Normal, inf.Normal))
                        .Reverse()
                        .ToArray()
                ).ToArray();
            return CreateMesh(false, true, arr);
        }

        public static void Simulations()
        {
            const int iterations = 100000;
            const int quadrantCount = 5;

            var getQuadrantCounts = Ut.Lambda((bool[][] arr) =>
            {
                var qCounts = new int[4];
                for (int x = 0; x < 8; x++)
                    for (int y = 0; y < 8; y++)
                        if (arr[x][y])
                            qCounts[(y / 4) * 2 + (x / 4)]++;
                return qCounts;
            });

            var quadrantCountRule = Ut.Lambda((bool white) => new Tuple<string, Func<bool[][], Widget[], int>>(
                $"Exactly one quadrant has {quadrantCount} or fewer {(white ? "white" : "black")} pixels ⇒ number of {(white ? "white" : "black")} pixels in the other 3 quadrants",
                (arr, edgework) =>
                {
                    var qCounts = getQuadrantCounts(arr);
                    if ((white ? qCounts.Count(sum => sum <= quadrantCount) : qCounts.Count(sum => sum >= (16 - quadrantCount))) != 1)
                        return 0;
                    var qIx = (white ? qCounts.IndexOf(sum => sum <= quadrantCount) : qCounts.IndexOf(sum => sum >= (16 - quadrantCount))) + 1;
                    return (qCounts.Where((sum, ix) => ix != qIx).Sum() + 3) % 4 + 1;
                }));

            var totalCountRule = Ut.Lambda((int num, bool white) => new Tuple<string, Func<bool[][], Widget[], int>>(
                $"The entire bitmap has {num} or more {(white ? "white" : "black")} pixels ⇒ number of {(white ? "white" : "black")} pixels",
                (arr, edgework) =>
                {
                    var sum = 0;
                    for (int x = 0; x < 8; x++)
                        for (int y = 0; y < 8; y++)
                            sum += (arr[x][y] ^ white) ? 0 : 1;
                    if (sum >= num)
                        return ((sum + 3) % 4) + 1;
                    return 0;
                }));

            var rowColumnRule = new Tuple<string, Func<bool[][], Widget[], int>>(
                "Exactly one row or column is completely white or completely black ⇒ x- or y-coordinate",
                (arr, edgework) =>
                {
                    int answer = 0;
                    for (int x = 0; x < 8; x++)
                    {
                        var isWhite = arr[x][0];
                        for (int y = 1; y < 8; y++)
                            if (arr[x][y] != isWhite)
                                goto next;

                        if (answer != 0)
                            return 0;
                        // The coordinate is 0-based, but the answer needs to be 1-based.
                        answer = (x % 4) + 1;

                        next:;
                    }
                    for (int y = 0; y < 8; y++)
                    {
                        var isWhite = arr[0][y];
                        for (int x = 1; x < 8; x++)
                            if (arr[x][y] != isWhite)
                                goto next;

                        if (answer != 0)
                            return 0;
                        // The coordinate is 0-based, but the answer needs to be 1-based.
                        answer = (y % 4) + 1;

                        next:;
                    }
                    return answer;
                });

            var squareRule = new Tuple<string, Func<bool[][], Widget[], int>>(
                "There is a 3×3 square that is completely white or completely black ⇒ x-coordinate of center of first in reading order",
                (arr, edgework) =>
                {
                    for (int x = 1; x < 7; x++)
                        for (int y = 1; y < 7; y++)
                        {
                            var isWhite = arr[x][y];
                            for (int xx = -1; xx < 2; xx++)
                                for (int yy = -1; yy < 2; yy++)
                                    if (arr[x + xx][y + yy] != isWhite)
                                        goto next;
                            // x is 0-based, but the answer needs to be 1-based.
                            return (x % 4) + 1;
                            next:;
                        }
                    return 0;
                });

            var quadrantMajorityRule = Ut.Lambda((string name, Func<int, int, Widget[], bool> compare, Func<int, int, Widget[], bool[][], int> getAnswer) => new Tuple<string, Func<bool[][], Widget[], int>>(
                name,
                (arr, widgets) =>
                {
                    var quadrantCounts = new int[4];
                    for (int x = 0; x < 8; x++)
                        for (int y = 0; y < 8; y++)
                            if (arr[x][y])
                                quadrantCounts[(x / 4) * 2 + (y / 4)]++;
                    var w = quadrantCounts.Count(q => q > 8);
                    var b = quadrantCounts.Count(q => q < 8);
                    return compare(b, w, widgets) ? ((getAnswer(b, w, widgets, arr) + 3) % 4) + 1 : 0;
                }));

            var rules = Ut.NewArray(
                quadrantCountRule(true),
                quadrantMajorityRule("There are as many mostly-white quadrants as there are lit indicators ⇒ number of batteries", (b, w, widgets) => w == widgets.GetNumLitIndicators(), (b, w, widgets, arr) => widgets.GetNumBatteries()),
                rowColumnRule,
                quadrantMajorityRule("There are fewer mostly-white quadrants than mostly-black quadrants ⇒ number of mostly-black quadrants", (b, w, widgets) => w < b, (b, w, widgets, arr) => b),
                totalCountRule(36, true),
                quadrantMajorityRule("There are more mostly-white quadrants than mostly-black quadrants ⇒ smallest number of black in any quadrant", (b, w, widgets) => w > b, (b, w, widgets, arr) => 16 - getQuadrantCounts(arr).Max()),
                quadrantCountRule(false),
                quadrantMajorityRule("There are as many mostly-black quadrants as there are unlit indicators ⇒ number of ports", (b, w, widgets) => b == widgets.GetNumUnlitIndicators(), (b, w, widgets, arr) => widgets.GetNumPorts()),
                squareRule,
                quadrantMajorityRule("There are as many mostly-white quadrants as mostly-black quadrants ⇒ first numeric digit of the serial number", (b, w, widgets) => w == b, (b, w, widgets, arr) => Rnd.Next(0, 10)));

            var counts = new int[rules.Length, 4];
            for (int iter = 0; iter < iterations; iter++)
            {
                var widgets = GenerateWidgets();
                var bitmap = Ut.NewArray(8, 8, (_, __) => Rnd.Next(0, 2) == 0);
                var startRule = Rnd.Next(0, rules.Length);

                var answer = 0;
                string rule;
                int ruleIndex;
                for (int r = 0; r < rules.Length; r++)
                {
                    ruleIndex = (r + startRule) % rules.Length;
                    var tup = rules[ruleIndex];
                    answer = tup.Item2(bitmap, widgets);
                    if (answer != 0)
                    {
                        rule = tup.Item1;
                        goto found;
                    }
                }
                Console.WriteLine(rules.Select(r => r.Item2(bitmap, widgets)).JoinString(", "));
                System.Diagnostics.Debugger.Break();
                break;

                found:;
                counts[ruleIndex, answer - 1]++;
            }

            var tt = new TextTable { ColumnSpacing = 2 };
            var ruleCounts = new int[rules.Length];

            for (int a = 0; a < 4; a++)
            {
                tt.SetCell(a + 1, 0, (a + 1).ToString().Color(ConsoleColor.White), alignment: HorizontalTextAlignment.Right);
                tt.SetCell(a + 1, rules.Length + 1, ((Enumerable.Range(0, rules.Length).Sum(r => counts[r, a]) * 100 / (double) iterations).ToString("0.0") + "%").Color(ConsoleColor.Green), alignment: HorizontalTextAlignment.Right);
            }

            for (int r = 0; r < rules.Length; r++)
            {
                tt.SetCell(0, r + 1, rules[r].Item1.Color(ConsoleColor.Cyan).Apply(s => s.ColorSubstring(s.IndexOf('⇒'), ConsoleColor.DarkCyan)), alignment: HorizontalTextAlignment.Right);
                for (int a = 0; a < 4; a++)
                    tt.SetCell(a + 1, r + 1, ((counts[r, a] * 100 / (double) iterations).ToString("0.0") + "%").Color(ConsoleColor.Magenta), alignment: HorizontalTextAlignment.Right);
                tt.SetCell(5, r + 1, ((Enumerable.Range(0, 4).Sum(a => counts[r, a]) * 100 / (double) iterations).ToString("0.0") + "%").Color(ConsoleColor.White), alignment: HorizontalTextAlignment.Right);
                ruleCounts[r] += Enumerable.Range(0, 4).Sum(a => counts[r, a]);
            }

            tt.WriteToConsole();
            Console.WriteLine();
            ConsoleUtil.WriteLine("Ratio of most likely to least likely rule: {0/White:0.0}".Color(ConsoleColor.White).Fmt(ruleCounts.Max() / (double) ruleCounts.Min()));
        }

        private static IEnumerable<VertexInfo[]> Button()
        {
            var height = .02;
            var fh = height * .4;
            var bevelWidth = .0312;
            var fbw = bevelWidth * .6;
            var width = .09;
            var roundSteps = 9;
            var innerSize = (width - bevelWidth);
            var arr = SmoothBézier(p(0, height), p(fbw, height), p(bevelWidth, fh), p(bevelWidth, 0), .0001)
                .Select((p, first, last) => new BevelPoint(p.X, p.Y, first || last ? Normal.Mine : Normal.Average, first || last ? Normal.Mine : Normal.Average))
                .Select(bi =>
                    Enumerable.Range(0, roundSteps).Select((i, ft, lt) => new { Normal = ft || lt ? Normal.Mine : Normal.Average, Angle = i * 90 / (roundSteps - 1), X = 1, Y = 1 }).Concat(
                    Enumerable.Range(0, roundSteps).Select((i, ft, lt) => new { Normal = ft || lt ? Normal.Mine : Normal.Average, Angle = i * 90 / (roundSteps - 1) + 90, X = -1, Y = 1 })).Concat(
                    Enumerable.Range(0, roundSteps).Select((i, ft, lt) => new { Normal = ft || lt ? Normal.Mine : Normal.Average, Angle = i * 90 / (roundSteps - 1) + 180, X = -1, Y = -1 })).Concat(
                    Enumerable.Range(0, roundSteps).Select((i, ft, lt) => new { Normal = ft || lt ? Normal.Mine : Normal.Average, Angle = i * 90 / (roundSteps - 1) + 270, X = 1, Y = -1 }))
                        .Select(inf => pt(innerSize * inf.X + bi.Into * cos(inf.Angle), bi.Y, innerSize * inf.Y + bi.Into * sin(inf.Angle), bi.Before, bi.After, inf.Normal, inf.Normal))
                        .Reverse()
                        .ToArray()
                ).ToArray();
            return CreateMesh(false, true, arr)
                .Select(row => row.Select(vi => vi.Location.Y == height ? new VertexInfo(vi.Location, pt(0, 1, 0)) : vi).ToArray())
                .Concat(new[] { p(-innerSize, -innerSize), p(-innerSize, innerSize), p(innerSize, innerSize), p(innerSize, -innerSize) }.Select(p => new VertexInfo(pt(p.X, height, p.Y), pt(0, 1, 0))).ToArray());
        }

        private static IEnumerable<Pt[]> ButtonHighlight()
        {
            var bevelWidth = .0512;
            var width = .11;
            var roundSteps = 9;
            var innerSize = (width - bevelWidth);
            return
                Enumerable.Range(0, roundSteps).Select((i, ft, lt) => new { Angle = i * 90 / (roundSteps - 1), X = 1, Y = 1 }).Concat(
                Enumerable.Range(0, roundSteps).Select((i, ft, lt) => new { Angle = i * 90 / (roundSteps - 1) + 90, X = -1, Y = 1 })).Concat(
                Enumerable.Range(0, roundSteps).Select((i, ft, lt) => new { Angle = i * 90 / (roundSteps - 1) + 180, X = -1, Y = -1 })).Concat(
                Enumerable.Range(0, roundSteps).Select((i, ft, lt) => new { Angle = i * 90 / (roundSteps - 1) + 270, X = 1, Y = -1 }))
                    .Select(inf => pt(innerSize * inf.X + bevelWidth * cos(inf.Angle), 0, innerSize * inf.Y + bevelWidth * sin(inf.Angle)))
                    .SelectConsecutivePairs(true, (p1, p2) => new[] { pt(0, 0, 0), p1, p2 });
        }
    }
}
