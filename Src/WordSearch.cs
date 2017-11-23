using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using KtaneStuff.Modeling;
using RT.Util;
using RT.Util.Drawing;
using RT.Util.ExtensionMethods;

namespace KtaneStuff
{
    using static Md;

    static class WordSearch
    {
        public static void DoModels()
        {
            File.WriteAllText(@"D:\c\KTANE\WordSearch\Assets\Models\Screen.obj", GenerateObjFile(Screen(), "Screen"));
            File.WriteAllText(@"D:\c\KTANE\WordSearch\Assets\Models\ScreenFrame.obj", GenerateObjFile(ScreenFrame(), "ScreenFrame"));
            File.WriteAllText(@"D:\c\KTANE\WordSearch\Assets\Models\Highlight.obj", GenerateObjFile(Highlight(), "Highlight"));
            File.WriteAllText(@"D:\c\KTANE\WordSearch\Assets\Models\Torus.obj", GenerateObjFile(Torus(.2, .03, 36), "Torus"));
            File.WriteAllText(@"D:\c\KTANE\WordSearch\Assets\Models\Decoration.obj", GenerateObjFile(Decoration(), "Decoration"));
        }

        public static void DoPngs()
        {
            const int padding = 10;
            var threads = new List<Thread>();
            for (int i = 3; i <= 6; i++)
                foreach (var diagonal in new[] { false, true })
                {
                    var j = i;
                    threads.Add(new Thread(() =>
                    {
                        var length = diagonal ? j * Math.Sqrt(2) : j;
                        var path1 = $@"D:\temp\temp{j}{diagonal}.png";
                        var path2 = $@"D:\c\KTANE\WordSearch\Assets\Images\{(diagonal ? "diag" : "str")}{j}.png";
                        GraphicsUtil.DrawBitmap((int) (length * 100), 100, g =>
                        {
                            g.Clear(Color.Transparent);
                            var path = GraphicsUtil.RoundedRectangle(new RectangleF(padding, padding, (int) (length * 100 - 2 * padding), 100 - 2 * padding), 45, tolerant: true);
                            //g.FillPath(Brushes.Teal, path);
                            g.DrawPath(new Pen(Color.Yellow, 10f), path);
                        }).Save(path1);
                        CommandRunner.Run("pngcr.bat", path1, path2).Go();
                        File.Delete(path1);
                    }));
                }
            foreach (var thr in threads)
                thr.Start();
            foreach (var thr in threads)
                thr.Join();
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
            var topFacePart3 = grooveX.Select(pt => p(pt.X, pt.Y + grooveHeight)).Concat(new[] { p(-w / 2, r), p(w / 2, r) }).Triangulate().Select(f => f.Select(p => pt(p.X, 0, p.Y).WithNormal(0, 1, 0)).ToArray()).ToArray();
            var topFacePart4 = grooveX.Select(pt => p(pt.X, -pt.Y - grooveHeight)).Reverse().Concat(new[] { p(w / 2, -r), p(-w / 2, -r) }).Triangulate().Select(f => f.Select(p => pt(p.X, 0, p.Y).WithNormal(0, 1, 0)).ToArray()).ToArray();

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

        public static void DoManual()
        {
            var rnd = new Random(1);
            var letters = Enumerable.Range('A', 26).Select(i => (char) i).ToList().Shuffle(rnd);
            var s = "";
            var wordPairs = "Hotel/Done;Search/Quebec;Add/Check;Sierra/Find;Finish/East;Port/Color;Boom/Submit;Line/Blue;Kaboom/Echo;Panic/False;Manual/Alarm;Decoy/Call;See/Twenty;India/North;Number/Look;Zulu/Green;Victor/Xray;Delta/Yes;Help/Locate;Romeo/Beep;True/Expert;Mike/Edge;Found/Red;Bombs/Word;Work/Unique;Test/Jinx;Golf/Letter;Talk/Six;Bravo/Serial;Seven/Timer;Module/Spell;List/Tango;Yankee/Solve;Chart/Oscar;Math/Next;Read/Listen;Lima/Four;Count/Office"
                .Split(';').Select(pairStr => pairStr.Split('/').Apply(arr => new { One = arr[0], Two = arr[1] })).ToArray();
            for (int i = 0; i < wordPairs.Length; i++)
            {
                var x = (i < 5 ? i + 1 : i >= 33 ? i - 32 : (i - 5) % 7);
                var y = (i < 5 ? 0 : i >= 33 ? 5 : (i + 2) / 7);
                s += $"<div class='box' style='left: {x * 25}mm; top: {y * 25}mm;'><div class='content'>{wordPairs[i].One.ToUpperInvariant()}\n—\n{wordPairs[i].Two.ToUpperInvariant()}</div></div>";
            }

            for (int i = 0; i < letters.Count; i++)
            {
                var x = (i < 4 ? i + 1 : i >= 22 ? i - 21 : (i - 4) % 6) + 1;
                var y = (i < 4 ? 0 : i >= 22 ? 4 : (i - 4) / 6 + 1) + 1;
                s += $"<div class='letter' style='left: {x * 25}mm; top: {y * 25}mm;' data-letter='{letters[i]}'></div>";
            }

            s += $"<div class='box hint' style='left: 0; top: 0;'><div class='content'>[even]\n—\n[odd]</div></div>";

            var path = @"D:\c\KTANE\WordSearch\Manual\Word Search.html";
            File.WriteAllText(path, Regex.Replace(File.ReadAllText(path), @"(?<=<!--##-->).*(?=<!--###-->)", s, RegexOptions.Singleline));
        }
    }
}
