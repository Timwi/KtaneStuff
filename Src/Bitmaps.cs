using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using KtaneStuff.Modeling;
using RT.Util.ExtensionMethods;

namespace KtaneStuff
{
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
