using System.IO;
using System.Linq;
using RT.Util;
using RT.Util.ExtensionMethods;
using RT.Util.Geometry;

namespace KtaneStuff.Modeling
{
    using static Md;

    static class TheBulb
    {
        public static void Do()
        {
            var points = Ut.NewArray(
                new PointD(0, 0),
                new PointD(0, 1),
                new PointD(0, 4),
                new PointD(2, 8),
                new PointD(4, 12),
                new PointD(0, 12),
                new PointD(-2, 10),
                new PointD(-5, 7),
                new PointD(-6, 3),
                new PointD(-6, 0),
                new PointD(-6, -3),
                new PointD(-5, -7),
                new PointD(-2, -10),
                new PointD(0, -12),
                new PointD(4, -12),
                new PointD(2, -8),
                new PointD(0, -4),
                new PointD(0, -1)
            );
            const double radius = .5;

            var curve = Enumerable.Range(0, points.Length / 3)
                .Select(i => Enumerable.Range(0, 4).Select(j => points[(3 * i + j) % points.Length]).Select(p => pt(p.X, 0, p.Y)).ToArray())
                .SelectMany(pts => Bézier(pts[0], pts[1], pts[2], pts[3], 20))
                .ToArray();

            var bevel = BevelFromCurve(curve, radius, 12);
            var topFace = Enumerable.Range(0, curve.Length / 2)
                .Select(i => new { I1 = (curve.Length / 4 + i) % curve.Length, I2 = (curve.Length / 4 - 1 - i + curve.Length) % curve.Length })
                .Select(inf => new { I1 = curve[inf.I1].WithNormal(pt(0, 1, 0)), I2 = curve[inf.I2].WithNormal(pt(0, 1, 0)) })
                .SelectConsecutivePairs(false, (p1, p2) => new[] { p2.I1, p1.I1, p1.I2, p2.I2 })
                .ToArray();
            File.WriteAllText(@"D:\c\KTANE\TheBulb\Assets\Models\Button.obj", GenerateObjFile(bevel.Concat(topFace), "Button"));

            var btnHighlightFaces = Enumerable.Range(0, curve.Length / 2)
                .Select(i => new { I1 = (curve.Length / 4 + i) % curve.Length, I2 = (curve.Length / 4 - 1 - i + curve.Length) % curve.Length })
                .Select(inf => new { I1 = curve[inf.I1], I2 = curve[inf.I2] })
                .SelectConsecutivePairs(false, (p1, p2) => new[] { p1.I1, p2.I1, p2.I2, p1.I2 })
                .ToArray();
            File.WriteAllText(@"D:\c\KTANE\TheBulb\Assets\Models\ButtonHighlight.obj", GenerateObjFile(btnHighlightFaces, "ButtonHighlight"));

            const int bulbHighlightSteps = 72;
            const double bulbHighlightRadius = 1d;
            var bulbHighlightFaces = Enumerable.Range(0, bulbHighlightSteps)
                .Select(i => i * 360 / bulbHighlightSteps)
                .Select(angle => pt(bulbHighlightRadius * cos(angle), 0, bulbHighlightRadius * sin(angle)))
                .SelectConsecutivePairs(true, (p1, p2) => new[] { pt(0, 0, 0), p1, p2 })
                .ToArray();
            File.WriteAllText(@"D:\c\KTANE\TheBulb\Assets\Models\BulbHighlight.obj", GenerateObjFile(bulbHighlightFaces, "BulbHighlight"));

            const double w = .05;
            const double h = .2;
            File.WriteAllText(@"D:\c\KTANE\TheBulb\Assets\Models\I.obj", GenerateObjFile(new[] { new[] { pt(w / 2, 0, h / 2), pt(w / 2, 0, -h / 2), pt(-w / 2, 0, -h / 2), pt(-w / 2, 0, h / 2) } }, "IFace"));

            var oSteps = 36;
            var oFaces = Enumerable.Range(0, oSteps)
                .Select(i => i * 360 / oSteps)
                .SelectConsecutivePairs(true, (angle1, angle2) => new[] { pt(0, 0, h / 2).RotateY(angle2), pt(0, 0, h / 2).RotateY(angle1), pt(0, 0, h / 2 - w).RotateY(angle1), pt(0, 0, h / 2 - w).RotateY(angle2) });
            File.WriteAllText(@"D:\c\KTANE\TheBulb\Assets\Models\O.obj", GenerateObjFile(oFaces, "OFace"));
        }
    }
}
