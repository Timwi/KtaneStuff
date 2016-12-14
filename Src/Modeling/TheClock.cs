using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using RT.KitchenSink;
using RT.Util.ExtensionMethods;
using RT.Util.Geometry;

namespace KtaneStuff.Modeling
{
    using RT.Util;
    using static Md;

    static class TheClock
    {
        public static void Do()
        {
            File.WriteAllText(@"D:\c\KTANE\TheClock\Assets\Models\ClockFrame.obj", GenerateObjFile(ClockFrame(), "ClockFrame"));
            File.WriteAllText(@"D:\c\KTANE\TheClock\Assets\Models\Clockface.obj", GenerateObjFile(Clockface(), "Clockface"));
            File.WriteAllText(@"D:\c\KTANE\TheClock\Assets\Models\ClockfaceBackground.obj", GenerateObjFile(Disc(90), "ClockfaceBackground"));

            //var files = new DirectoryInfo(@"D:\c\KTANE\TheClock\Assets\Models").EnumerateFiles("Arabic*.obj").Concat(new DirectoryInfo(@"D:\c\KTANE\TheClock\Assets\Models").EnumerateFiles("Roman*.obj")).ToArray();
            //foreach (var file in files)
            //    file.Delete();

            //var numbersFonts = new[] {
            //    "Agency FB", "Baskerville Old Face", "Bodoni MT Condensed", "Century", "Century Gothic",
            //    "Copperplate Gothic Light", "Edwardian Script ITC;DejaVu Serif", "Felix Titling", "Gill Sans MT Ext Condensed Bold", "Harrington",
            //    "Jokerman;Klotz", "Modern No. 20", "Niagara Solid", "Old English Text MT;Ostrich Sans Heavy", "Onyx",
            //    "P22 Johnston Underground", "Parchment;Perg", "Poor Richard", "Proxima Nova ExCn Th", "Stencil", "Vladimir Script;Bauhaus 93" };
            //Enumerable.Range(0, numbersFonts.Length).ParallelForEach(1, ix =>
            //{
            //    var fonts = numbersFonts[ix].Contains(';') ? numbersFonts[ix].Split(';') : new[] { numbersFonts[ix], numbersFonts[ix] };
            //    File.WriteAllText($@"D:\c\KTANE\TheClock\Assets\Models\Arabic{ix}.obj", GenerateObjFile(Numbers(fonts[0], "12,1,2,3,4,5,6,7,8,9,10,11".Split(',')), $"Arabic{ix}"));
            //    File.WriteAllText($@"D:\c\KTANE\TheClock\Assets\Models\Roman{ix}.obj", GenerateObjFile(Numbers(fonts[1], "XII,I,II,III,IV,V,VI,VII,VIII,IX,X,XI".Split(',')), $"Roman{ix}"));
            //    lock (numbersFonts)
            //        Console.WriteLine(numbersFonts[ix] + " done");
            //});
        }

        class PathStuff
        {
            public List<DecodeSvgPath.PathPiece> Path;
            public double X, Y, W, H;
        }

        private static IEnumerable<VertexInfo[]> Numbers(string fontFamily, string[] numbers)
        {
            const double depth = .005;
            const double bevelRadius = .005;
            const int revSteps = 2;

            const double numberRadius = .8;
            const double bézierSmoothness = .001;

            //const double maxWidth = .25;
            //const double maxHeight = .2;
            const double maxArea = .045;

            var stuffs = new List<PathStuff>();
            var factor = 2d;

            using (var bmp = new Bitmap(1024, 768, PixelFormat.Format32bppArgb))
                for (int i = 0; i < 12; i++)
                {
                    var angle = i * 360 / 12 - 90;
                    var gp = new GraphicsPath();
                    gp.AddString(numbers[i], new FontFamily(fontFamily), 0, .25f, new PointF(0, 0), new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center });
                    var path = new List<DecodeSvgPath.PathPiece>();
                    for (int j = 0; j < gp.PointCount; j++)
                    {
                        var type =
                            ((PathPointType) gp.PathTypes[j]).HasFlag(PathPointType.Bezier) ? DecodeSvgPath.PathPieceType.Curve :
                            ((PathPointType) gp.PathTypes[j]).HasFlag(PathPointType.Line) ? DecodeSvgPath.PathPieceType.Line : DecodeSvgPath.PathPieceType.Move;
                        if (type == DecodeSvgPath.PathPieceType.Curve)
                        {
                            path.Add(new DecodeSvgPath.PathPiece(DecodeSvgPath.PathPieceType.Curve, gp.PathPoints.Subarray(j, 3).Select(p => new PointD(p)).ToArray()));
                            j += 2;
                        }
                        else
                            path.Add(new DecodeSvgPath.PathPiece(type, gp.PathPoints.Subarray(j, 1).Select(p => new PointD(p)).ToArray()));

                        if (((PathPointType) gp.PathTypes[j]).HasFlag(PathPointType.CloseSubpath))
                            path.Add(DecodeSvgPath.PathPiece.End);
                    }

                    var x1 = path.Where(p => p.Type != DecodeSvgPath.PathPieceType.End).SelectMany(p => p.Points).Min(p => p.X);
                    var x2 = path.Where(p => p.Type != DecodeSvgPath.PathPieceType.End).SelectMany(p => p.Points).Max(p => p.X);
                    var y1 = path.Where(p => p.Type != DecodeSvgPath.PathPieceType.End).SelectMany(p => p.Points).Min(p => p.Y);
                    var y2 = path.Where(p => p.Type != DecodeSvgPath.PathPieceType.End).SelectMany(p => p.Points).Max(p => p.Y);
                    var w = x2 - x1;
                    var h = y2 - y1;
                    factor = Math.Min(factor, maxArea / w / h);
                    stuffs.Add(new PathStuff { Path = path, X = x1, Y = y1, W = w, H = h });
                }

            for (int i = 0; i < 12; i++)
            {
                var angle = i * 360 / 12 - 90;
                var path = stuffs[i].Path.Select(pth => pth.Select(p => new PointD((p.X - stuffs[i].X - stuffs[i].W / 2) * factor + numberRadius * cos(angle), (p.Y - stuffs[i].Y - stuffs[i].H / 2) * factor + numberRadius * sin(angle)))).ToList();
                var furthest = path.Where(p => p.Type != DecodeSvgPath.PathPieceType.End).SelectMany(p => p.Points).Max(p => p.Distance());
                var newRadius = numberRadius - (furthest - numberRadius);
                path = stuffs[i].Path.Select(pth => pth.Select(p => new PointD((p.X - stuffs[i].X - stuffs[i].W / 2) * factor + newRadius * cos(angle), (p.Y - stuffs[i].Y - stuffs[i].H / 2) * factor + newRadius * sin(angle)))).ToList();

                var reverse = false;
                var stuff = DecodeSvgPath.Do(path, bézierSmoothness);
                PointD[][] ts;
                try { ts = Triangulate(stuff).ToArray(); }
                catch (InvalidOperationException)
                {
                    ts = Triangulate(stuff.Select(st => st.Reverse())).ToArray();
                    reverse = true;
                }
                foreach (var t in ts)
                    yield return t.Select(p => pt(-p.X, depth, -p.Y).WithNormal(0, 1, 0)).Reverse().ToArray();

                foreach (var c in stuff)
                    foreach (var b in BevelFromCurve((reverse ? c.Reverse() : c).Select(p => pt(-p.X, depth, -p.Y)), bevelRadius, revSteps))
                        yield return b;
            }
        }

        private static IEnumerable<VertexInfo[]> Clockface()
        {
            const double hourTicksWidth = .01;
            const double hourTicksInnerRadius = .85;
            const double hourTicksOuterRadius = 1;
            const double minuteTicksWidth = .005;
            const double minuteTicksInnerRadius = .9;
            const double minuteTicksOuterRadius = 1;

            double depth = .01;
            double bevelRadius = .01;
            int revSteps = 3;

            for (int i = 0; i < 60; i++)
            {
                var angle = i * 360 / 60;
                var ticksWidth = i % (60 / 12) == 0 ? hourTicksWidth : minuteTicksWidth;
                var innerRadius = i % (60 / 12) == 0 ? hourTicksInnerRadius : minuteTicksInnerRadius;
                var outerRadius = i % (60 / 12) == 0 ? hourTicksOuterRadius : minuteTicksOuterRadius;
                var front = new[] { pt(ticksWidth, depth, innerRadius), pt(-ticksWidth, depth, innerRadius), pt(-ticksWidth, depth, outerRadius), pt(ticksWidth, depth, outerRadius) }.Select(p => p.RotateY(angle)).ToArray();
                yield return front.Select(p => p.WithNormal(0, 1, 0)).ToArray();
                foreach (var b in BevelFromCurve(front.Reverse(), bevelRadius, revSteps))
                    yield return b;
            }
        }

        private static IEnumerable<VertexInfo[]> ClockFrame()
        {
            const double depth = .05;
            const double bevelRadius = .05;
            const int revSteps = 4;
            const int circleSteps = 90;
            const double circleInnerRadius = 1;
            const double circleOuterRadius = 1.02;
            const double nodeRadius = .03;

            var outerCurve = Enumerable.Range(0, circleSteps).Select(i => i * 360 / circleSteps).Select(angle => new PointD(cos(angle), sin(angle)).Apply(p => new { Inner = circleInnerRadius * p, Outer = circleOuterRadius * p })).ToArray();
            foreach (var b in BevelFromCurve(outerCurve.Reverse().Select(c => pt(c.Inner.X, depth, c.Inner.Y)), bevelRadius, revSteps))
                yield return b;
            foreach (var b in BevelFromCurve(outerCurve.Select(c => pt(c.Outer.X, depth, c.Outer.Y)), bevelRadius, revSteps))
                yield return b;
            foreach (var c in outerCurve.SelectConsecutivePairs(true, (p1, p2) => new[] { p1.Outer, p1.Inner, p2.Inner, p2.Outer }.Select(p => pt(p.X, depth, p.Y).WithNormal(0, 1, 0)).ToArray()))
                yield return c;

            var nodeCurve = Enumerable.Range(0, circleSteps).Select(i => i * 360 / circleSteps).Select(angle => new PointD(cos(angle), sin(angle)) * nodeRadius).ToArray();
            foreach (var b in BevelFromCurve(nodeCurve.Select(c => pt(c.X, depth, c.Y)), bevelRadius, revSteps))
                yield return b;
            foreach (var c in nodeCurve.SelectConsecutivePairs(true, (p1, p2) => new[] { p(0, 0), p2, p1 }.Select(p => pt(p.X, depth, p.Y).WithNormal(0, 1, 0)).ToArray()))
                yield return c;
        }
    }
}
