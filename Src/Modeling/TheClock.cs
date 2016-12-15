using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using RT.KitchenSink;
using RT.Util;
using RT.Util.ExtensionMethods;
using RT.Util.Geometry;
using RT.Util.Serialization;

namespace KtaneStuff.Modeling
{
    using static Md;

    static class TheClock
    {
        public static void Do()
        {
            File.WriteAllText(@"D:\c\KTANE\TheClock\Assets\Models\ClockFrame.obj", GenerateObjFile(ClockFrame(), "ClockFrame"));
            File.WriteAllText(@"D:\c\KTANE\TheClock\Assets\Models\Clockface.obj", GenerateObjFile(Clockface(), "Clockface"));
            File.WriteAllText(@"D:\c\KTANE\TheClock\Assets\Models\ClockfaceBackground.obj", GenerateObjFile(Disc(90), "ClockfaceBackground"));

            var oldNumberData = new Dictionary<string, double>();
            try { oldNumberData = ClassifyJson.DeserializeFile<Dictionary<string, double>>(@"D:\temp\TheClockNumberData.json"); } catch { }

            var numberData = new Dictionary<string, double>()
            {
                { "AAgency FB", 1.2 },
                { "RAgency FB", 1.0 },
                { "ABaskerville Old Face|.1|.09", 1.2 },
                { "RBaskerville Old Face", 0.7 },
                { "ABodoni MT Condensed|.07|.06", 1.6 },
                { "RBodoni MT Condensed", 1.2 },
                { "ACentury|.11|.1", 1.1 },
                { "RCentury", 0.7 },
                { "ACentury Gothic|.1|.07", 1.1 },
                { "RCentury Gothic", 0.9 },
                { "AHarrington|.09|.06", 1.4 },
                { "RCopperplate Gothic Light", 0.9 },
                { "AEdwardian Script ITC|.09|.06", 1.4 },
                { "RDejaVu Serif", 0.7 },
                { "AFelix Titling", 1.25 },
                { "RFelix Titling", 0.85 },
                { "AGill Sans MT Ext Condensed Bold", 1.4 },
                { "RGill Sans MT Ext Condensed Bold", 1.1 },
                { "AJokerman", 0.9 },
                { "RKlotz", 0.943 },
                { "AModern No. 20|.1|.08", 1.27 },
                { "RModern No. 20", 0.7 },
                { "ANiagara Solid", 1.6 },
                { "RNiagara Solid", 1.35 },
                { "AOld English Text MT|.1|.075", 1.2 },
                { "ROstrich Sans Heavy", 1.15 },
                { "AOnyx", 1.5 },
                { "ROnyx", 1.136 },
                { "AP22 Johnston Underground|.08|.05", 1.5 },
                { "RP22 Johnston Underground", 0.9 },
                { "AParchment", 2.75 },
                { "RPerg", 0.9 },
                { "APoor Richard", 1.184 },
                { "RPoor Richard", 0.85 },
                { "AProxima Nova ExCn Th|.06|.05", 1.6 },
                { "RProxima Nova ExCn Th", 1.2 },
                { "AStencil|.11|.09", 1.0 },
                { "RStencil", 0.8 },
                { "AVladimir Script|.08|.06", 1.25 },
                { "RBauhaus 93", 0.9 }
            };
            var indexes = new Dictionary<string, int>();
            var arabicIx = 0;
            var romanIx = 0;
            foreach (var key in numberData.Keys)
                if (key.StartsWith("A"))
                    indexes[key] = arabicIx++;
                else
                    indexes[key] = romanIx++;

            numberData.ParallelForEach(1, kvp =>
            {
                if (oldNumberData != null && oldNumberData.ContainsKey(kvp.Key) && kvp.Value == oldNumberData[kvp.Key])
                    return;
                var arabic = kvp.Key.StartsWith("A");
                var fontFamily = kvp.Key.Substring(1);
                double[] spacing = null;
                var pieces = fontFamily.Split('|');
                if (pieces.Length == 3)
                {
                    fontFamily = pieces[0];
                    spacing = pieces.Skip(1).Select(double.Parse).ToArray();
                }
                var factor = kvp.Value;
                var objName = $"{(arabic ? "Arabic" : "Roman")}{indexes[kvp.Key]}";

                File.WriteAllText($@"D:\c\KTANE\TheClock\Assets\Models\{objName}.obj", GenerateObjFile(Numbers(fontFamily, arabic, factor, spacing), objName));
                lock (indexes)
                    Console.WriteLine($"{fontFamily} {(arabic ? "Arabic" : "Roman")} done");
            });

            ClassifyJson.SerializeToFile(numberData, @"D:\temp\TheClockNumberData.json");
        }

        class PathStuff
        {
            public List<DecodeSvgPath.PathPiece> Path;
            public double X, Y, W, H;
        }

        private static IEnumerable<VertexInfo[]> Numbers(string fontFamily, bool arabic, double factor, double[] spacing)
        {
            const double depth = .0001;// .005;
            //const double bevelRadius = .005;
            //const int revSteps = 2;

            const double numberRadius = .775;
            const double bézierSmoothness = .001;

            var numbers = arabic ? "1|2,1,2,3,4,5,6,7,8,9,1|0,1|1".Split(',') : "XII,I,II,III,IV,V,VI,VII,VIII,IX,X,XI".Split(',');
            var stuffs = new List<PathStuff>();

            using (var bmp = new Bitmap(1024, 768, PixelFormat.Format32bppArgb))
                for (int i = 0; i < 12; i++)
                {
                    var angle = i * 360 / 12 - 90;
                    var gp = new GraphicsPath();
                    var x = 0.0;
                    foreach (var piece in spacing == null ? new[] { numbers[i].Replace("|", "") } : numbers[i].Split('|'))
                    {
                        gp.AddString(piece, new FontFamily(fontFamily), 0, .25f, new PointF((float) x, 0), new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center });
                        if (spacing != null && arabic && (i == 0 || i >= 10))
                            x += (i == 0 || i == 10) ? spacing[0] : spacing[1];
                    }
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
                    stuffs.Add(new PathStuff { Path = path, X = x1, Y = y1, W = w, H = h });
                }

            for (int i = 0; i < 12; i++)
            {
                var angle = i * 360 / 12 - 90;
                var path = stuffs[i].Path.Select(pth => pth.Select(p => new PointD((p.X - stuffs[i].X - stuffs[i].W / 2) * factor + numberRadius * cos(angle), (p.Y - stuffs[i].Y - stuffs[i].H / 2) * factor + numberRadius * sin(angle)))).ToList();
                var furthest = path.Where(p => p.Type != DecodeSvgPath.PathPieceType.End).SelectMany(p => p.Points).Max(p => p.Distance());
                var newRadius = numberRadius - (furthest - numberRadius);
                path = stuffs[i].Path.Select(pth => pth.Select(p => new PointD((p.X - stuffs[i].X - stuffs[i].W / 2) * factor + newRadius * cos(angle), (p.Y - stuffs[i].Y - stuffs[i].H / 2) * factor + newRadius * sin(angle)))).ToList();

                //var reverse = false;
                var stuff = DecodeSvgPath.Do(path, bézierSmoothness);
                PointD[][] ts;
                try { ts = Triangulate(stuff).ToArray(); }
                catch (InvalidOperationException)
                {
                    ts = Triangulate(stuff.Select(st => st.Reverse())).ToArray();
                    //reverse = true;
                }
                foreach (var t in ts)
                    yield return t.Select(p => pt(-p.X, depth, -p.Y).WithNormal(0, 1, 0)).Reverse().ToArray();

                //foreach (var c in stuff)
                //    foreach (var b in BevelFromCurve((reverse ? c.Reverse() : c).Select(p => pt(-p.X, depth, -p.Y)), bevelRadius, revSteps))
                //        yield return b;
            }
        }

        private static IEnumerable<VertexInfo[]> Clockface()
        {
            const double hourTicksWidth = .015;
            const double hourTicksInnerRadius = .825;
            const double hourTicksOuterRadius = .925;
            const double minuteTicksWidth = .01;
            const double minuteTicksInnerRadius = .85;
            const double minuteTicksOuterRadius = .9;

            double depth = .0001;   // .01
            //double bevelRadius = .01;
            //int revSteps = 3;

            for (int i = 0; i < 60; i++)
            {
                var angle = i * 360 / 60;
                var ticksWidth = i % (60 / 12) == 0 ? hourTicksWidth : minuteTicksWidth;
                var innerRadius = i % (60 / 12) == 0 ? hourTicksInnerRadius : minuteTicksInnerRadius;
                var outerRadius = i % (60 / 12) == 0 ? hourTicksOuterRadius : minuteTicksOuterRadius;
                var front = new[] { pt(ticksWidth, depth, innerRadius), pt(-ticksWidth, depth, innerRadius), pt(-ticksWidth, depth, outerRadius), pt(ticksWidth, depth, outerRadius) }.Select(p => p.RotateY(angle)).ToArray();
                yield return front.Select(p => p.WithNormal(0, 1, 0)).ToArray();
                //foreach (var b in BevelFromCurve(front.Reverse(), bevelRadius, revSteps))
                //    yield return b;
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
