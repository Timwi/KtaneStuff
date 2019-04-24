using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using KtaneStuff.Modeling;
using RT.KitchenSink;
using RT.Util;
using RT.Util.ExtensionMethods;
using RT.Util.Geometry;
using RT.Util.Serialization;

namespace KtaneStuff
{
    using static Md;

    static class TheClock
    {
        enum HandStyle
        {
            Lines,
            Arrows,
            Spades
        }

        public static void DoModels()
        {
            File.WriteAllText(@"D:\c\KTANE\TheClock\Assets\Models\ClockFrame.obj", GenerateObjFile(ClockFrame(), "ClockFrame"));
            File.WriteAllText(@"D:\c\KTANE\TheClock\Assets\Models\Clockface.obj", GenerateObjFile(Clockface(), "Clockface"));
            File.WriteAllText(@"D:\c\KTANE\TheClock\Assets\Models\SecondHand.obj", GenerateObjFile(LooseModels.Cylinder(-.9 / 6, .9, .01, 8), "SecondHand"));
            File.WriteAllText(@"D:\c\KTANE\TheClock\Assets\Models\AM.obj", GenerateObjFile(AmPm("AM"), "AM"));
            File.WriteAllText(@"D:\c\KTANE\TheClock\Assets\Models\PM.obj", GenerateObjFile(AmPm("PM"), "PM"));
            File.WriteAllText(@"D:\c\KTANE\TheClock\Assets\Models\Knob.obj", GenerateObjFile(Knob(), "Knob"));

            GenerateNumerals();
            GenerateHands();
        }

        private static void GenerateHands()
        {
            foreach (var minute in new[] { false, true })
            {
                foreach (var style in new[] { HandStyle.Lines, HandStyle.Arrows, HandStyle.Spades })
                {
                    for (int design = 0; design < 2; design++)
                    {
                        double thickness = minute ? .01 : .015;
                        double length = minute ? .8 : .6;
                        double depth = .01;
                        double elevation = minute ? depth : 0;

                        var objName = $"{(minute ? "Minute" : "Hour")}Hand{style}{design}";

                        File.WriteAllText(
                            $@"D:\c\KTANE\TheClock\Assets\Models\{objName}.obj",
                            GenerateObjFile(Hand(style, design, thickness, length, depth, elevation), objName));
                    }
                }
            }
        }

        private static IEnumerable<VertexInfo[]> Hand(HandStyle style, int design, double thickness, double length, double depth, double elevation)
        {
            const double bevelRadiusOutward = .025;
            const double bevelRadiusDown = .01;
            const int revSteps = 45;
            double backLength = length / 6;
            const double bézierSmoothness = .001;

            PointD[] curve = null;
            switch (style)
            {
                case HandStyle.Lines:
                    curve = Ut.NewArray(
                        new[] { p(-thickness / 2, -backLength), p(thickness / 2, -backLength), p(thickness / 2, length), p(-thickness / 2, length) },
                        new[] { p(-thickness * 3 / 2, -backLength), p(thickness * 3 / 2, -backLength), p(-thickness / 100, length), p(thickness / 100, length) }
                    )[design];
                    break;

                case HandStyle.Arrows:
                    curve = Ut.NewArray(
                        new[] { p(-thickness * 2, -backLength), p(0, -backLength * 4 / 5), p(thickness * 2, -backLength), p(thickness / 5, length * .85), p(thickness * 4, length * .77), p(0, length), p(-thickness * 4, length * .77), p(-thickness / 5, length * .85) },
                        new[] { p(-thickness / 2, -backLength), p(thickness / 2, -backLength), p(thickness / 2, length * .85), p(thickness * 3, length * .85), p(0, length), p(-thickness * 3, length * .85), p(-thickness / 2, length * .85) }
                    )[design];
                    break;

                case HandStyle.Spades:
                    double spadeWidth = thickness * 3 / 2;
                    double spadeHeightF = 4;
                    curve = Ut.NewArray(
                        Enumerable.Range(0, 72).Select(i => i * (360 - 30) / 72 + 90 + 15).Select(angle => p(thickness * 3 * cos(angle), thickness * 3 * sin(angle) - backLength))
                            .Concat(SmoothBézier(p(spadeWidth, length * (1 - spadeHeightF * (1 - .91))), p(spadeWidth * 3, length * (1 - spadeHeightF * (1 - .9))), p(spadeWidth * 4, length * (1 - spadeHeightF * (1 - .91))), p(spadeWidth * 4, length * (1 - spadeHeightF * (1 - .93))), bézierSmoothness))
                            .Concat(SmoothBézier(p(spadeWidth * 4, length * (1 - spadeHeightF * (1 - .93))), p(spadeWidth * 4, length * (1 - spadeHeightF * (1 - .96))), p(spadeWidth, length * (1 - spadeHeightF * (1 - .96))), p(0, length), bézierSmoothness).Skip(1))
                            .Concat(SmoothBézier(p(0, length), p(-spadeWidth, length * (1 - spadeHeightF * (1 - .96))), p(-spadeWidth * 4, length * (1 - spadeHeightF * (1 - .96))), p(-spadeWidth * 4, length * (1 - spadeHeightF * (1 - .93))), bézierSmoothness).Skip(1))
                            .Concat(SmoothBézier(p(-spadeWidth * 4, length * (1 - spadeHeightF * (1 - .93))), p(-spadeWidth * 4, length * (1 - spadeHeightF * (1 - .91))), p(-spadeWidth * 3, length * (1 - spadeHeightF * (1 - .9))), p(-spadeWidth, length * (1 - spadeHeightF * (1 - .91))), bézierSmoothness).Skip(1))
                            .ToArray(),
                        SmoothBézier(p(spadeWidth * 2, -backLength * 1.5), p(spadeWidth / 2, backLength), p(spadeWidth / 2, 0), p(spadeWidth / 2, length * (1 - spadeHeightF * (1 - .91))), bézierSmoothness)
                            .Concat(SmoothBézier(p(spadeWidth / 2, length * (1 - spadeHeightF * (1 - .91))), p(spadeWidth * 3, length * (1 - spadeHeightF * (1 - .91))), p(spadeWidth * 3, length * (1 - spadeHeightF * (1 - .92))), p(spadeWidth * 3, length * (1 - spadeHeightF * (1 - .93))), bézierSmoothness).Skip(1))
                            .Concat(SmoothBézier(p(spadeWidth * 3, length * (1 - spadeHeightF * (1 - .93))), p(spadeWidth * 3, length * (1 - spadeHeightF * (1 - .95))), p(spadeWidth, length * (1 - spadeHeightF * (1 - .94))), p(0, length), bézierSmoothness).Skip(1))
                            .Concat(SmoothBézier(p(0, length), p(-spadeWidth, length * (1 - spadeHeightF * (1 - .94))), p(-spadeWidth * 3, length * (1 - spadeHeightF * (1 - .95))), p(-spadeWidth * 3, length * (1 - spadeHeightF * (1 - .93))), bézierSmoothness).Skip(1))
                            .Concat(SmoothBézier(p(-spadeWidth * 3, length * (1 - spadeHeightF * (1 - .93))), p(-spadeWidth * 3, length * (1 - spadeHeightF * (1 - .92))), p(-spadeWidth * 3, length * (1 - spadeHeightF * (1 - .91))), p(-spadeWidth / 2, length * (1 - spadeHeightF * (1 - .91))), bézierSmoothness).Skip(1))
                            .Concat(SmoothBézier(p(-spadeWidth / 2, length * (1 - spadeHeightF * (1 - .91))), p(-spadeWidth / 2, 0), p(-spadeWidth / 2, backLength), p(-spadeWidth * 2, -backLength * 1.5), bézierSmoothness))
                            .ToArray()
                    )[1 - design];
                    break;
            }

            if (curve == null)
                throw new InvalidOperationException();

            foreach (var t in new[] { curve }.Triangulate().Select(arr => arr.Select(p => pt(p.X, depth + elevation, p.Y)).ToArray()))
                yield return t.Select(p => p.WithNormal(0, 1, 0)).Reverse().ToArray();
            foreach (var b in BevelFromCurve(curve.Select(p => pt(p.X, depth, p.Y)), bevelRadiusOutward, bevelRadiusDown, revSteps, Normal.Mine))
                yield return b.Select(vi => new VertexInfo(vi.Location.Add(y: elevation), vi.Normal)).ToArray();
        }

        private static void GenerateNumerals()
        {
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
                indexes[key] = key.StartsWith("A") ? arabicIx++ : romanIx++;

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
                try { ts = stuff.Triangulate().ToArray(); }
                catch (InvalidOperationException)
                {
                    ts = stuff.Select(st => st.Reverse()).Triangulate().ToArray();
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
            const int circleSteps = 48;
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

        public static void GenerateComponentSvg()
        {
            double[] thickness = { .01, .015 };
            double[] length = { .8, .6 };
            double[] backLength = { length[0] / 6, length[1] / 6 };
            double[] angles = { 132, 12 };

            Utils.ReplaceInFile(@"D:\c\KTANE\HTML\img\Component\The Clock.svg", "<!--%%-->", "<!--%%%-->",
                $@"
                    {Enumerable.Range(-2, 5).Select(i => $"<line x1='0' y1='0' x2='1.1' y2='0' transform='rotate({20 * i})' stroke-width='.1' stroke='#000' />").JoinString()}
                    <circle cx='0' cy='0' r='1' fill='#fff' stroke='#000' stroke-width='.01' />
                    {Enumerable.Range(0, 2)
                        .Select(i => $"<path transform='rotate({-angles[i]})' d='M {new[] { p(-thickness[i] * 2, -backLength[i]), p(0, -backLength[i] * 4 / 5), p(thickness[i] * 2, -backLength[i]), p(thickness[i] / 5, length[i] * .85), p(thickness[i] * 4, length[i] * .77), p(0, length[i]), p(-thickness[i] * 4, length[i] * .77), p(-thickness[i] / 5, length[i] * .85) }.Select(p => $"{p.X},{p.Y}").JoinString(" ")} z' stroke='none' fill='#000' />")
                        .JoinString()
                    }
                    {Enumerable.Range(1, 12).Select(i => $"<path transform='translate({.8 * cos(i * 30 + 270)}, {.8 * sin(i * 30 + 270) + .05})' d='{Utils.FontToSvgPath(i.ToString(), "Agency FB", .35f).JoinString(" ")}' fill='#000' />").JoinString()}
                ");
        }

        public static void GenerateCharts()
        {
            // Minutes
            generateChart(@"D:\c\KTANE\HTML\img\The Clock\Minutes Chart.svg", "Minutes", 1, 1.24,
                new RingInfo { Labels = new[] { "Arrows", "Lines", "Spades" }, Horiz = true, FontSize = .8, HFactor = 32, InnerD = .35, OuterD = .6 },
                new RingInfo { Labels = new[] { "Red", "Green", "Blue", "Gold", "Black" }, Horiz = true, FontSize = .45, HFactor = 16, InnerD = .425, OuterD = .575 },
                new RingInfo { Labels = new[] { "B", "W" }, Horiz = true, FontSize = .5, HFactor = 20, InnerD = .4, OuterD = .6 },
                new RingInfo { Labels = new[] { "Ab", "Pr" }, Horiz = false, FontSize = .4, HFactor = 12, InnerD = .4, OuterD = .55 });

            // Hours
            generateChart(@"D:\c\KTANE\HTML\img\The Clock\Hours Chart.svg", "Hours", 1 / .8, 1,
                new RingInfo { Labels = new[] { "Arabic", "Roman", "None" }, Horiz = true, FontSize = .8, HFactor = 32, InnerD = .35, OuterD = .6 },
                new RingInfo { Labels = new[] { "Silver", "Gold" }, Horiz = true, FontSize = .5, HFactor = 20, InnerD = .4, OuterD = .55 },
                new RingInfo { Labels = new[] { "Matched", "Unmatched" }, Horiz = true, FontSize = .35, HFactor = 14, InnerD = .4, OuterD = .55 });
        }

        sealed class RingInfo
        {
            public string[] Labels;
            public bool Horiz;
            public double FontSize;
            public double HFactor;
            public double InnerD;
            public double OuterD;
        }

        static void generateChart(string outputFile, string centerText, double xSizeFac, double ySizeFac, params RingInfo[] rings)
        {
            var factor = 1;
            var svg = new StringBuilder();
            var text = new StringBuilder();
            var circ = 0;
            var innerCirc = 1;
            var offset = rings.Aggregate(180, (p, n) => p / n.Labels.Length);

            foreach (var ring in rings)
            {
                svg.Append(mkCircle(0, 0, circ + innerCirc, "none", "#000", .05));
                for (int i = 0; i < ring.Labels.Length * factor; i++)
                {
                    var lineAngle = (i * 360 / (ring.Labels.Length * factor) + offset) % 360;
                    svg.Append($"<line x1='0' y1='{circ + innerCirc}' x2='0' y2='{circ + 1 + innerCirc}' transform='rotate({lineAngle})' fill='none' stroke='#000' stroke-width='.03' />");
                    var textAngle = (lineAngle + 180 / ring.Labels.Length / factor) % 360;
                    var label = ring.Labels[i % ring.Labels.Length];

                    if (ring.Horiz)
                    {
                        var writing = Utils.FontToSvgPath(label, "Special Elite", ring.FontSize);
                        var textAngle2 = textAngle + 90;
                        if (textAngle < 90 || textAngle >= 270)
                            svg.Append($"<path class='writing' d='{writing.Select(piece => piece.Select(p => (circ + ring.OuterD + innerCirc).Apply(r => new PointD((r + p.Y) * cos(textAngle2 - p.X * ring.HFactor), (r + p.Y) * sin(textAngle2 - p.X * ring.HFactor))))).JoinString(" ")}' fill='#000' stroke='none' />");
                        else
                            svg.Append($"<path class='writing' d='{writing.Select(piece => piece.Select(p => (circ + ring.InnerD + innerCirc).Apply(r => new PointD((r - p.Y) * cos(textAngle2 + p.X * ring.HFactor), (r - p.Y) * sin(textAngle2 + p.X * ring.HFactor))))).JoinString(" ")}' fill='#000' stroke='none' />");
                    }
                    else
                    {
                        if (textAngle < 90 || textAngle >= 270)
                            text.Append($"<text x='{circ + .5 + innerCirc}' y='0' transform='rotate({textAngle + 1.5})' fill='#000' font-size='{ring.FontSize}' text-anchor='middle' font-family='Special Elite'>{ring.Labels[i % ring.Labels.Length]}</text>");
                        else
                            text.Append($"<text x='{-circ - .5 - innerCirc}' y='0' transform='rotate({textAngle - 1.5 + 180})' fill='#000' font-size='{ring.FontSize}' text-anchor='middle' font-family='Special Elite'>{ring.Labels[i % ring.Labels.Length]}</text>");
                    }
                }
                factor *= ring.Labels.Length;
                circ++;
            }
            svg.Append(mkCircle(0, 0, circ + innerCirc, "none", "#000", .05));

            for (int i = 0; i < 60; i++)
                if (i % 5 == 0)
                    svg.Append($"<line x1='0' y1='{circ + innerCirc}' x2='0' y2='{circ + 1 + innerCirc}' transform='rotate({i * 360 / 60})' fill='none' stroke='#000' stroke-width='.2' />");
                else
                    svg.Append($"<line x1='0' y1='{circ + .5 + innerCirc}' x2='0' y2='{circ + 1 + innerCirc}' transform='rotate({i * 360 / 60})' fill='none' stroke='#000' stroke-width='.1' />");
            circ++;

            svg.Append($"<text x='0' y='.1' fill='#000' font-size='.4' text-anchor='middle' font-family='Special Elite'>{centerText}</text>");

            var match = File.Exists(outputFile) ? Regex.Match(File.ReadAllText(outputFile), @"(?<=<!--%%-->).*(?=<!--%%%-->)", RegexOptions.Singleline) : null;
            File.WriteAllText(outputFile, $"<svg xmlns='http://www.w3.org/2000/svg' viewBox='{-circ - innerCirc - .2} {-circ - innerCirc - .2} {(2 * (circ + innerCirc) + .4) * xSizeFac} {(2 * (circ + innerCirc) + .4) * ySizeFac}'>{svg}{(text.Length > 0 ? $"<g>{text}</g>" : null)}<!--%%-->{(match != null && match.Success ? match.Value : null)}<!--%%%--></svg>");
        }

        private static string mkCircle(double cx, double cy, double r, string fill = null, string stroke = null, double? strokeWidth = null)
        {
            // Amazingly, Google Chrome fails to print SVGs with <circle>! To work around that, simulate a circle using Bézier curves
            double bf = r * (Math.Sqrt(2) - 1) * 4 / 3;
            return $"<path d='M{cx + r},{cy} C{cx + r},{cy + bf} {cx + bf},{cy + r} {cx},{cy + r} {cx - bf},{cy + r} {cx - r},{cy + bf} {cx - r},{cy} {cx - r},{cy - bf} {cx - bf},{cy - r} {cx},{cy - r} {cx + bf},{cy - r} {cx + r},{cy - bf} {cx + r},{cy}' {fill?.Apply(f => $"fill='{f}' ")}{stroke?.Apply(s => $"stroke='{s}' ")}{strokeWidth?.Apply(sw => $"stroke-width='{sw}' ")}/>";
        }

        private static IEnumerable<VertexInfo[]> AmPm(string text)
        {
            var faces = DecodeSvgPath.Do(Utils.FontToSvgPath(text, "Proxima Nova ExCn Rg", 1), .01).Triangulate();
            var minX = faces.Min(f => f.Min(p => p.X));
            var maxX = faces.Max(f => f.Max(p => p.X));
            var minY = faces.Min(f => f.Min(p => p.Y));
            var maxY = faces.Max(f => f.Max(p => p.Y));
            var cx = (minX + maxX) / 2;
            var cy = (minY + maxY) / 2;

            return faces.Select(parr => parr.Select(p => pt(p.X - cx, 0, p.Y - cy).WithNormal(0, 1, 0)).Reverse().ToArray());
        }

        private static IEnumerable<VertexInfo[]> Knob()
        {
            const int revSteps = 45;
            var middleBézier = SmoothBézier(p(-2, 1.5), p(-3.75, 1.5), p(-4, .75), p(-4, 0), .03).ToArray();

            return CreateMesh(true, false, Enumerable.Range(0, revSteps)
                .Select(i => -i * 360.0 / revSteps)
                .Select((angle, ix) =>
                    new[] { p(0, .6), p(-2, .6) }.Select(p => new { Point = p, AverageNormalX = true, AverageNormalY = false, NormalOverride = false })
                        .Concat(middleBézier.Select((pt, frst, lst) =>
                            pt.X > -3 ? new { Point = p(pt.X, pt.Y + (ix % 2 == 0 ? .1 : -.1)), AverageNormalX = false, AverageNormalY = false, NormalOverride = false } :
                            new { Point = pt, AverageNormalX = true, AverageNormalY = true, NormalOverride = lst }))
                        .Select(inf => pt(inf.Point.X, 0, inf.Point.Y).RotateX(angle).Apply(rotated =>
                            inf.NormalOverride ? rotated.WithMeshInfo(-1, 0, 0) : rotated.WithMeshInfo(inf.AverageNormalX ? Normal.Average : Normal.Mine, inf.AverageNormalX ? Normal.Average : Normal.Mine, inf.AverageNormalY ? Normal.Average : Normal.Mine, inf.AverageNormalY ? Normal.Average : Normal.Mine)))
                        .ToArray())
                .ToArray());
        }
    }
}
