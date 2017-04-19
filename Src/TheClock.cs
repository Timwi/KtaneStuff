using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
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
            Line,
            Arrow,
            Spade
        }

        public static void DoModels()
        {
            File.WriteAllText(@"D:\c\KTANE\TheClock\Assets\Models\ClockFrame.obj", GenerateObjFile(ClockFrame(), "ClockFrame"));
            File.WriteAllText(@"D:\c\KTANE\TheClock\Assets\Models\Clockface.obj", GenerateObjFile(Clockface(), "Clockface"));
            File.WriteAllText(@"D:\c\KTANE\TheClock\Assets\Models\ClockfaceBackground.obj", GenerateObjFile(Disc(90), "ClockfaceBackground"));
            File.WriteAllText(@"D:\c\KTANE\TheClock\Assets\Models\SecondHand.obj", GenerateObjFile(Cylinder(-.9 / 6, .9, .01), "SecondHand"));

            GenerateNumerals();
            GenerateHands();
        }

        private static void GenerateHands()
        {
            foreach (var minute in new[] { false, true })
            {
                foreach (var style in new[] { HandStyle.Line, HandStyle.Arrow, HandStyle.Spade })
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
            const double bevelRadius = .01;
            const int revSteps = 3;
            double backLength = length / 6;
            const double bézierSmoothness = .001;

            PointD[] curve = null;
            switch (style)
            {
                case HandStyle.Line:
                    curve = Ut.NewArray(
                        new[] { p(-thickness / 2, -backLength), p(thickness / 2, -backLength), p(thickness / 2, length), p(-thickness / 2, length) },
                        new[] { p(-thickness * 3 / 2, -backLength), p(thickness * 3 / 2, -backLength), p(-thickness / 100, length), p(thickness / 100, length) }
                    )[design];
                    break;

                case HandStyle.Arrow:
                    curve = Ut.NewArray(
                        new[] { p(-thickness * 2, -backLength), p(0, -backLength * 4 / 5), p(thickness * 2, -backLength), p(thickness / 5, length * .85), p(thickness * 4, length * .77), p(0, length), p(-thickness * 4, length * .77), p(-thickness / 5, length * .85) },
                        new[] { p(-thickness / 2, -backLength), p(thickness / 2, -backLength), p(thickness / 2, length * .85), p(thickness * 3, length * .85), p(0, length), p(-thickness * 3, length * .85), p(-thickness / 2, length * .85) }
                    )[design];
                    break;

                case HandStyle.Spade:
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

            foreach (var t in Triangulate(new[] { curve }).Select(arr => arr.Select(p => pt(p.X, depth + elevation, p.Y)).ToArray()))
                yield return t.Select(p => p.WithNormal(0, 1, 0)).Reverse().ToArray();
            foreach (var b in BevelFromCurve(curve.Select(p => pt(p.X, depth, p.Y)), bevelRadius, revSteps))
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

        public static void ClockDiagram()
        {
            var multipliers = new[] { 3, 5, 2, 2 };
            var factor = 1;
            var startAngle = 3.0;
            var svg = new StringBuilder();
            var text = new StringBuilder();
            var circ = 0;
            var innerCirc = 1;

            var writingSvg = XDocument.Parse(File.ReadAllText(@"D:\Daten\Upload\TheAuthorOfOZ\Writing.svg")).Root;
            var writingG = writingSvg.Elements().FirstOrDefault(e => e.Name.LocalName == "g");
            var getPieces = Ut.Lambda((string id, double dx, double dy) => DecodeSvgPath.DecodePieces(writingG.Elements().Single(e => e.Name.LocalName == "path" && e.Attribute("id").Value == id).Attributes().Single(a => a.Name.LocalName == "d").Value).Select(piece => piece.Select(p => new PointD(p.X + dx, p.Y + dy) / 35)).ToList());
            var writings = new[] { getPieces("Arabic", -50, -90 - 952.36218), getPieces("Roman", -50, -60 - 952.36218), getPieces("None", -50, -30 - 952.36218) };

            foreach (var mul in multipliers)
            {
                factor *= mul;
                svg.Append(mkCircle(0, 0, circ + innerCirc, "none", "#000", .05));
                startAngle += 180.0 / factor;
                for (int i = 0; i < factor; i++)
                {
                    var angle = (i * 360 / factor + startAngle) % 360;
                    svg.Append($"<line x1='0' y1='{circ + innerCirc}' x2='0' y2='{circ + 1 + innerCirc}' transform='rotate({angle})' fill='none' stroke='#000' stroke-width='.03' />");

                    switch (circ)
                    {
                        case 0:
                            var writing = writings[i % writings.Length];
                            angle = (angle + 150) % 360;
                            if (angle > 180)
                                svg.Append($"<path class='writing' d='{writing.Select(piece => piece.Select(p => (circ + .2 + innerCirc).Apply(r => new PointD((-p.Y + r) * cos(angle + p.X * 30), (-p.Y + r) * sin(angle + p.X * 30))))).JoinString(" ")}' fill='#000' stroke='none' />");
                            else
                                svg.Append($"<path class='writing' d='{writing.Select(piece => piece.Select(p => (circ + .7 + innerCirc).Apply(r => new PointD((p.Y + r) * cos(angle - p.X * 30), (p.Y + r) * sin(angle - p.X * 30))))).JoinString(" ")}' fill='#000' stroke='none' />");
                            break;

                        case 1:
                            {
                                var lbls = new[] { "BW", "B", "G", "Y", "R", "P", "G", "BW", "R", "B", "Y", "P", "R", "G", "B" };
                                if (angle < 90 || angle >= 270)
                                    text.Append($"<text x='0' y='{-circ - .325 - innerCirc}' transform='rotate({angle})' fill='#000' font-size='.5' text-anchor='middle' font-family='Special Elite'>{lbls[(i + 3) % lbls.Length]}</text>");
                                else
                                    text.Append($"<text x='0' y='{circ + .675 + innerCirc}' transform='rotate({angle + 180})' fill='#000' font-size='.5' text-anchor='middle' font-family='Special Elite'>{lbls[(i + 3) % lbls.Length]}</text>");
                            }
                            break;

                        case 2:
                            {
                                var lbls = new[] { "Lin", "Arw", "Spd" };
                                if (angle < 90 || angle >= 270)
                                    text.Append($"<text x='{circ + .5 + innerCirc}' y='0' transform='rotate({angle + 2})' fill='#000' font-size='.4' text-anchor='middle' font-family='Special Elite'>{lbls[i % lbls.Length]}</text>");
                                else
                                    text.Append($"<text x='{-circ - .5 - innerCirc}' y='0' transform='rotate({angle - 2 + 180})' fill='#000' font-size='.4' text-anchor='middle' font-family='Special Elite'>{lbls[i % lbls.Length]}</text>");
                            }
                            break;

                        case 3:
                            {
                                var lbls = new[] { "Ma", "Un", "Ab" };
                                if (angle < 90 || angle >= 270)
                                    text.Append($"<text x='{circ + .5 + innerCirc}' y='.0' transform='rotate({angle + 1 + 3})' fill='#000' font-size='.3' text-anchor='middle' font-family='Special Elite'>{lbls[i % lbls.Length]}</text>");
                                else
                                    text.Append($"<text x='{-circ - .5 - innerCirc}' y='.0' transform='rotate({angle - 1 + 180 + 3})' fill='#000' font-size='.3' text-anchor='middle' font-family='Special Elite'>{lbls[i % lbls.Length]}</text>");
                            }
                            break;
                    }
                }
                circ++;
            }
            svg.Append(mkCircle(0, 0, circ + innerCirc, "none", "#000", .05));

            for (int i = 0; i < 60; i++)
                if (i % 5 == 0)
                    svg.Append($"<line x1='0' y1='{circ + innerCirc}' x2='0' y2='{circ + 1 + innerCirc}' transform='rotate({i * 360 / 60})' fill='none' stroke='#000' stroke-width='.2' />");
                else
                    svg.Append($"<line x1='0' y1='{circ + .5 + innerCirc}' x2='0' y2='{circ + 1 + innerCirc}' transform='rotate({i * 360 / 60})' fill='none' stroke='#000' stroke-width='.1' />");
            circ++;

            svg.Append(mkCircle(0, 0, .1));

            File.WriteAllText(@"D:\c\KTANE\TheClock\Manual\img\The Clock\Wheel Chart.svg", $"<svg xmlns='http://www.w3.org/2000/svg' viewBox='{-circ - innerCirc - .2} {-circ - innerCirc - .2} {2 * (circ + innerCirc) + .4} {2 * (circ + innerCirc) + .4}'>{svg}<g>{text}</g></svg>");
        }

        private static string mkCircle(double cx, double cy, double r, string fill = null, string stroke = null, double? strokeWidth = null)
        {
            // Amazingly, Google Chrome fails to print SVGs with <circle>! To work around that, simulate a circle using Bézier curves
            double bf = r * (Math.Sqrt(2) - 1) * 4 / 3;
            return $"<path d='M{cx + r},{cy} C{cx + r},{cy + bf} {cx + bf},{cy + r} {cx},{cy + r} {cx - bf},{cy + r} {cx - r},{cy + bf} {cx - r},{cy} {cx - r},{cy - bf} {cx - bf},{cy - r} {cx},{cy - r} {cx + bf},{cy - r} {cx + r},{cy - bf} {cx + r},{cy}' {fill?.Apply(f => $"fill='{f}' ")}{stroke?.Apply(s => $"stroke='{s}' ")}{strokeWidth?.Apply(sw => $"stroke-width='{sw}' ")}/>";
        }
    }
}
