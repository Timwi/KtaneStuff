using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using RT.KitchenSink;
using RT.Util;
using RT.Util.ExtensionMethods;
using RT.Util.Geometry;

namespace KtaneStuff
{
    using static Modeling.Md;

    static partial class Ktane
    {
        public static void ClockDiagram()
        {
            var multipliers = new[] { 3, 5, 2, 2 };
            var factor = 1;
            var startAngle = 0.0;
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
                                    text.Append($"<text x='{circ + .5 + innerCirc}' y='.0' transform='rotate({angle + 1 - 3})' fill='#000' font-size='.3' text-anchor='middle' font-family='Special Elite'>{lbls[i % lbls.Length]}</text>");
                                else
                                    text.Append($"<text x='{-circ - .5 - innerCirc}' y='.0' transform='rotate({angle - 1 + 180 - 3})' fill='#000' font-size='.3' text-anchor='middle' font-family='Special Elite'>{lbls[i % lbls.Length]}</text>");
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

            File.WriteAllText(@"D:\c\KTANE\HTML\img\The Clock\Wheel Chart.svg", $"<svg xmlns='http://www.w3.org/2000/svg' viewBox='{-circ - innerCirc - .2} {-circ - innerCirc - .2} {2 * (circ + innerCirc) + .4} {2 * (circ + innerCirc) + .4}'>{svg}<g>{text}</g></svg>");
        }

        private static string mkCircle(double cx, double cy, double r, string fill = null, string stroke = null, double? strokeWidth = null)
        {
            // Amazingly, Google Chrome fails to print SVGs with <circle>! To work around that, simulate a circle using Bézier curves
            double bf = r * (Math.Sqrt(2) - 1) * 4 / 3;
            return $"<path d='M{cx + r},{cy} C{cx + r},{cy + bf} {cx + bf},{cy + r} {cx},{cy + r} {cx - bf},{cy + r} {cx - r},{cy + bf} {cx - r},{cy} {cx - r},{cy - bf} {cx - bf},{cy - r} {cx},{cy - r} {cx + bf},{cy - r} {cx + r},{cy - bf} {cx + r},{cy}' {fill?.Apply(f => $"fill='{f}' ")}{stroke?.Apply(s => $"stroke='{s}' ")}{strokeWidth?.Apply(sw => $"stroke-width='{sw}' ")}/>";
        }
    }
}
