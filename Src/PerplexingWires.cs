using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using KtaneStuff.Modeling;
using RT.Util.ExtensionMethods;
using RT.Util.Geometry;

namespace KtaneStuff
{
    using static Md;

    public static class PerplexingWires
    {
        public static void DoModels()
        {
            File.WriteAllText(@"D:\c\KTANE\PerplexingWires\Assets\Models\Strip1.obj", GenerateObjFile(Strip(1.25), "Strip1"));
            File.WriteAllText(@"D:\c\KTANE\PerplexingWires\Assets\Models\Strip2.obj", GenerateObjFile(Strip(.8), "Strip2"));
            File.WriteAllText(@"D:\c\KTANE\PerplexingWires\Assets\Models\Strip3.obj", GenerateObjFile(Strip(.5), "Strip3"));
            File.WriteAllText(@"D:\c\KTANE\PerplexingWires\Assets\Models\Connector.obj", GenerateObjFile(Connector(), "Connector"));
        }

        private static IEnumerable<Pt[]> Connector()
        {
            IEnumerable<Pt[]> circularSweep(IEnumerable<PointD> shape, int numSteps, double startAngle = 0, bool closed = false)
            {
                Pt rotate(PointD p, double angle) => pt(p.X * cos(angle), p.Y, p.X * sin(angle));
                return Enumerable.Range(0, numSteps).Select(i => (360 * i / numSteps) + startAngle)
                    .SelectConsecutivePairs(true, (angle1, angle2) => shape
                        .SelectConsecutivePairs(closed, (p1, p2) => new[] { rotate(p1, angle1), rotate(p1, angle2), rotate(p2, angle2), rotate(p2, angle1) }))
                    .SelectMany(x => x);
            }

            const double innerRadius = .1;
            const double outerRadius = .15;
            const double height = .1;
            var boxes = circularSweep(new[] { /*p(0, 0), */p(innerRadius, 0), p(innerRadius, height), p(outerRadius, height), p(outerRadius, 0) }, 6, 0);
            return boxes;
        }

        private static IEnumerable<VertexInfo[]> Strip(double wd /* Actual width is 2×(wd + bvx) */)
        {
            const double hg = .15; // Actual height is 2×(hg + bvy)
            const double dp = -.2; // Depth
            const double bvx = .15; // Bevel size X
            const double bvy = .1; // Bevel size Y

            var a = pt(-wd - bvx, 0, -hg - bvy);
            var b = pt(wd + bvx, 0, -hg - bvy);
            var c = pt(-wd, -dp, -hg);
            var d = pt(wd, -dp, -hg);
            var e = pt(-wd, -dp, hg);
            var f = pt(wd, -dp, hg);
            var g = pt(-wd - bvx, 0, hg + bvy);
            var h = pt(wd + bvx, 0, hg + bvy);

            yield return new[] { c, d, b, a }.Select(v => v.WithNormal(-(b - a) * (c - a))).ToArray();  // Top
            yield return new[] { d, f, h, b }.Select(v => v.WithNormal(-(h - b) * (d - b))).ToArray();  // Right
            yield return new[] { g, h, f, e }.Select(v => v.WithNormal(-(g - h) * (f - h))).ToArray();  // Bottom
            yield return new[] { g, e, c, a }.Select(v => v.WithNormal(-(g - a) * (c - a))).ToArray();  // Left
            yield return new[] { e, f, d, c }.Select(v => v.WithNormal(0, 1, 0)).ToArray();  // Front face

            var vector = -(b - a) * (c - a);
            var x = vector.X;
            var y = vector.Y;
            var z = vector.Z;
            var r = Math.Sqrt(x * x + y * y + z * z);
            var t = Math.Atan2(y, x) / Math.PI * 180;
            var p = Math.Acos(z / r) / Math.PI * 180;

            Console.WriteLine($"For width {wd}, t={t}, p={p}");

        }
    }
}