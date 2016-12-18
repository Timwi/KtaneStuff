using System.Collections.Generic;
using System.IO;
using System.Linq;
using RT.Util;
using RT.Util.ExtensionMethods;

namespace KtaneStuff.Modeling
{
    using System;
    using System.Drawing;
    using System.Threading;
    using RT.Util.Drawing;
    using static Md;

    static class WordSearch
    {
        public static void Do()
        {
            File.WriteAllText(@"D:\c\KTANE\WordSearch\Assets\Models\Screen.obj", GenerateObjFile(Screen(), "Screen"));
            File.WriteAllText(@"D:\c\KTANE\WordSearch\Assets\Models\ScreenFrame.obj", GenerateObjFile(ScreenFrame(), "ScreenFrame"));
            File.WriteAllText(@"D:\c\KTANE\WordSearch\Assets\Models\Highlight.obj", GenerateObjFile(Highlight(), "Highlight"));
            File.WriteAllText(@"D:\c\KTANE\WordSearch\Assets\Models\Torus.obj", GenerateObjFile(Torus(.2, .03, 36), "Torus"));
            File.WriteAllText(@"D:\c\KTANE\WordSearch\Assets\Models\Decoration.obj", GenerateObjFile(Decoration(), "Decoration"));

            // Highlight PNGs
            const int padding = 10;
            var threads = new List<Thread>();
            var declarations = new List<string>();
            for (int i = 3; i <= 6; i++)
                foreach (var diagonal in new[] { false, true })
                {
                    var j = i;
                    threads.Add(new Thread(() =>
                    {
                        var length = diagonal ? j * Math.Sqrt(2) : j;
                        var path1 = $@"D:\temp\temp{j}{diagonal}.png";
                        var path2 = $@"D:\temp\temp{j}{diagonal}.cr.png";
                        GraphicsUtil.DrawBitmap((int) (length * 100), 100, g =>
                        {
                            g.Clear(Color.Transparent);
                            var path = GraphicsUtil.RoundedRectangle(new RectangleF(padding, padding, (int) (length * 100 - 2 * padding), 100 - 2 * padding), 45, tolerant: true);
                            //g.FillPath(Brushes.Teal, path);
                            g.DrawPath(new Pen(Color.Yellow, 10f), path);
                        }).Save(path1);
                        CommandRunner.Run("pngcr.bat", path1, path2).Go();
                        lock (declarations)
                            declarations.Add($@"{{ ""{(diagonal ? "diag" : "str")}{j}"", new byte[] {{ {File.ReadAllBytes(path2).JoinString(",")} }} }}");
                        File.Delete(path1);
                        File.Delete(path2);
                    }));
                }
            foreach (var thr in threads)
                thr.Start();
            foreach (var thr in threads)
                thr.Join();

            File.WriteAllText(@"D:\c\KTANE\WordSearch\Assets\RawPngs.cs", $@"
using System.Collections.Generic;
namespace WordSearch {{
    public static class Pngs {{
        public static Dictionary<string, byte[]> RawData = new Dictionary<string, byte[]> {{
            {declarations.Order().JoinString(",\r\n            ")}
        }};
    }}
}}
");
        }

        private static IEnumerable<VertexInfo[]> Decoration()
        {
            const int steps = 36;
            const double w = 1.4;
            const double r = .1;
            const double bevelR = .05;

            var outerCurve = Enumerable.Range(0, steps + 1).Select(i => i * 180 / steps - 90).Select(angle => pt(w / 2 + r * cos(angle), 0, r * sin(angle)))
                .Concat(Enumerable.Range(0, steps + 1).Select(i => i * 180 / steps + 90).Select(angle => pt(-w / 2 + r * cos(angle), 0, r * sin(angle))))
                .ToArray();

            const double bevelWidth = .2;
            const double xf = bevelWidth * .25;
            const double bevelHeight = 0;
            const double grooveHeight = .02;
            const double ghf = grooveHeight * .4;
            const int bézierSteps = 10;

            // Cannot use SmoothBézier because need consistent number of points
            var grooveYHalf = Ut.Lambda((double holeSize) => Bézier(p(-holeSize - grooveHeight, bevelHeight), p(-holeSize - grooveHeight + ghf, bevelHeight), p(-holeSize - xf / 2, bevelHeight - grooveHeight + ghf), p(-holeSize, bevelHeight - grooveHeight), bézierSteps));
            var grooveY = Ut.Lambda((double holeSize) => grooveYHalf(holeSize).Concat(grooveYHalf(holeSize).Select(pt => p(-pt.X, pt.Y)).Reverse()));

            const double xyInner = .0001;
            const double xyOuter = .0275;
            const double holeWidth = xyOuter - xyInner;

            var grooveX = Enumerable.Range(-2, 4)
                .SelectMany(i =>
                    Bézier(p(-.32 * i, xyOuter), p(-.32 * i - holeWidth, xyOuter), p(-.32 * i - holeWidth, xyInner), p(-.32 * i - 2 * holeWidth, xyInner), bézierSteps)
                        .Concat(Bézier(p(-.32 * i - .32 + 2 * holeWidth, xyInner), p(-.32 * i - .32 + holeWidth, xyInner), p(-.32 * i - .32 + holeWidth, xyOuter), p(-.32 * i - .32, xyOuter), bézierSteps)));

            var grooveMainPart = CreateMesh(false, false, grooveX.Select(b => grooveY(b.Y).Select(p => pt(b.X, p.Y, p.X)).ToArray()).ToArray());

            var grooveRound1 = CreateMesh(false, false,
                Enumerable.Range(0, steps + 1).Select(i => i * 180 / steps + 90).Reverse().Select(angle =>
                    grooveYHalf(xyOuter).Select(p => pt(p.X * cos(angle) + .64, p.Y, p.X * sin(angle))).ToArray()).ToArray());
            var grooveRound2 = CreateMesh(false, false,
                Enumerable.Range(0, steps + 1).Select(i => i * 180 / steps - 90).Reverse().Select(angle =>
                    grooveYHalf(xyOuter).Select(p => pt(p.X * cos(angle) - .64, p.Y, p.X * sin(angle))).ToArray()).ToArray());

            var topFacePart1 = CreateMesh(false, false, Enumerable.Range(0, steps + 1)
                .Select(i => i * 180 / steps - 90)
                .Select(angle => new[] { pt((xyOuter + grooveHeight) * cos(angle) + .64, 0, (xyOuter + grooveHeight) * sin(angle)), pt(w / 2 + r * cos(angle), 0, r * sin(angle)) })
                .ToArray());
            var topFacePart2 = CreateMesh(false, false, Enumerable.Range(0, steps + 1)
                .Select(i => i * 180 / steps + 90)
                .Select(angle => new[] { pt((xyOuter + grooveHeight) * cos(angle) - .64, 0, (xyOuter + grooveHeight) * sin(angle)), pt(-w / 2 + r * cos(angle), 0, r * sin(angle)) })
                .ToArray());
            var topFacePart3 = Triangulate(grooveX.Select(pt => p(pt.X, pt.Y + grooveHeight)).Concat(new[] { p(-w / 2, r), p(w / 2, r) })).Select(f => f.Select(p => pt(p.X, 0, p.Y).WithNormal(0, 1, 0)).ToArray()).ToArray();
            var topFacePart4 = Triangulate(grooveX.Select(pt => p(pt.X, -pt.Y - grooveHeight)).Reverse().Concat(new[] { p(w / 2, -r), p(-w / 2, -r) })).Select(f => f.Select(p => pt(p.X, 0, p.Y).WithNormal(0, 1, 0)).ToArray()).ToArray();

            return BevelFromCurve(outerCurve, bevelR, steps)
                .Concat(grooveMainPart).Concat(grooveRound1).Concat(grooveRound2)
                .Concat(topFacePart1).Concat(topFacePart2).Concat(topFacePart3).Concat(topFacePart4);
        }

        private static IEnumerable<Pt[]> Screen()
        {
            yield return new[] { pt(-1, 0, -1), pt(-1, 0, 1), pt(1, 0, 1), pt(1, 0, -1) };
        }

        private static IEnumerable<VertexInfo[]> ScreenFrame()
        {
            const double w = 1;
            const double h = 1;
            const double r = .05;
            const double bevelWidth = .2;
            const double xf = bevelWidth * .25;
            const double bevelHeight = .05;
            const double yf = bevelHeight * .4;
            const double grooveHeight = .02;
            const double ghf = grooveHeight * .4;
            const int bézierSteps = 10;
            const int roundSteps = 12;

            // Cannot use SmoothBézier because need consistent number of points
            var xy = Ut.Lambda((double holeSize) =>
                Bézier(p(0, 0), p(xf, yf), p(bevelHeight - yf, bevelHeight), p(bevelHeight, bevelHeight), bézierSteps)
                    .Concat(Bézier(p(bevelWidth / 2 - holeSize - grooveHeight, bevelHeight), p(bevelWidth / 2 - holeSize - grooveHeight + ghf, bevelHeight), p(bevelWidth / 2 - holeSize - xf / 2, bevelHeight - grooveHeight + ghf), p(bevelWidth / 2 - holeSize, bevelHeight - grooveHeight), bézierSteps))
                    .Concat(Bézier(p(bevelWidth / 2 + holeSize, bevelHeight - grooveHeight), p(bevelWidth / 2 + holeSize + xf / 2, bevelHeight - grooveHeight + ghf), p(bevelWidth / 2 + holeSize + grooveHeight - ghf, bevelHeight), p(bevelWidth / 2 + holeSize + grooveHeight, bevelHeight), bézierSteps))
                    .Concat(Bézier(p(bevelWidth - bevelHeight, bevelHeight), p(bevelWidth - bevelHeight + yf, bevelHeight), p(bevelWidth - yf, yf), p(bevelWidth, 0), bézierSteps))
                    .ToArray());

            const double xyInner = .0001;
            const double xyOuter = .0275;
            const double holeWidth = xyOuter - xyInner;

            var xyz =
                // Top-left corner
                Enumerable.Range(0, roundSteps + 1).Select(i => i * 90 / roundSteps).Select(angle => xy(xyInner).Select(p => pt(w - r + (p.X + r) * cos(angle), p.Y, h - r + (p.X + r) * sin(angle))).ToArray())
                    // Top edge
                    .Concat(
                        Enumerable.Range(-2, 5).SelectMany(i =>
                            Bézier(p(2 * holeWidth, xyInner), p(holeWidth, xyInner), p(holeWidth, xyOuter), p(0, xyOuter), bézierSteps)
                                .Concat(Bézier(p(0, xyOuter), p(-holeWidth, xyOuter), p(-holeWidth, xyInner), p(-2 * holeWidth, xyInner), bézierSteps).Skip(1))
                                .Select(b => xy(b.Y).Select(p => pt(b.X - .32 * i, p.Y, h + p.X)).ToArray())))
                    // Top-right corner
                    .Concat(Enumerable.Range(0, roundSteps + 1).Select(i => i * 90 / roundSteps + 90).Select(angle => xy(xyInner).Select(p => pt(-w + r + (p.X + r) * cos(angle), p.Y, h - r + (p.X + r) * sin(angle))).ToArray()))
                    // Right edge
                    .Concat(
                        Enumerable.Range(-2, 5).SelectMany(i =>
                            Bézier(p(2 * holeWidth, xyInner), p(holeWidth, xyInner), p(holeWidth, xyOuter), p(0, xyOuter), bézierSteps)
                                .Concat(Bézier(p(0, xyOuter), p(-holeWidth, xyOuter), p(-holeWidth, xyInner), p(-2 * holeWidth, xyInner), bézierSteps).Skip(1))
                                .Select(b => xy(b.Y).Select(p => pt(-w - p.X, p.Y, b.X - .32 * i)).ToArray())))
                    // Bottom-right corner
                    .Concat(Enumerable.Range(0, roundSteps + 1).Select(i => i * 90 / roundSteps + 180).Select(angle => xy(xyInner).Select(p => pt(-w + r + (p.X + r) * cos(angle), p.Y, -h + r + (p.X + r) * sin(angle))).ToArray()))
                    // Bottom-left corner
                    .Concat(Enumerable.Range(0, roundSteps + 1).Select(i => i * 90 / roundSteps + 270).Select(angle => xy(xyInner).Select(p => pt(w - r + (p.X + r) * cos(angle), p.Y, -h + r + (p.X + r) * sin(angle))).ToArray()))
                .Select((arr, ix1) => arr.Select((p, ix2) => pt(p.X, p.Y, p.Z, Normal.Average, Normal.Average, ix2 == 0 || ix2 == arr.Length - 1 ? Normal.Mine : Normal.Average, ix2 == 0 || ix2 == arr.Length - 1 ? Normal.Mine : Normal.Average)).ToArray())
                .ToArray();

            return CreateMesh(true, false, xyz);
        }

        private static IEnumerable<Pt[]> Highlight()
        {
            var outerRadius = 1;
            var innerRadius = .8;
            var roundSteps = 72;
            return
                Enumerable.Range(0, roundSteps).Select(i => i * 360 / roundSteps).SelectConsecutivePairs(true, (angle1, angle2) => Ut.NewArray(
                    pt(innerRadius * cos(angle1), 0, innerRadius * sin(angle1)),
                    pt(outerRadius * cos(angle1), 0, outerRadius * sin(angle1)),
                    pt(outerRadius * cos(angle2), 0, outerRadius * sin(angle2)),
                    pt(innerRadius * cos(angle2), 0, innerRadius * sin(angle2))));
        }
    }
}
