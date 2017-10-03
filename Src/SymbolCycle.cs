using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using KtaneStuff.Modeling;
using RT.Util;
using RT.Util.Geometry;

namespace KtaneStuff
{
    using static Md;

    public static class SymbolCycle
    {
        public static void DoModels()
        {
            File.WriteAllText(@"D:\c\KTANE\SymbolCycle\Assets\Models\Plate.obj", GenerateObjFile(fixTextures(Plate()), "Plate"));
        }

        private static IEnumerable<VertexInfo[]> Plate()
        {
            var outline = @"(-0.854439, 0.15, -0.854439)
(-0.256271, 0.15, -0.854439)
(0.256273, 0.15, -0.854439)
(0.364376, 0.15, -0.854439)
(0.444206, 0.15, -0.774025)
(0.443619, 0.15, -0.693596)
(0.524693, 0.15, -0.545002)
(0.672195, 0.15, -0.46653)
(0.784569, 0.15, -0.472279)
(0.861069, 0.15, -0.396061)
(0.854439, 0.15, -0.256271)
(0.854439, 0.15, 0.256271)
(0.854439, 0.15, 0.854439)
(0.256273, 0.15, 0.854439)
(-0.256273, 0.15, 0.854439)
(-0.854439, 0.15, 0.854439)
(-0.854439, 0.15, 0.256271)
(-0.854439, 0.15, -0.256271)".Replace("\r", "").Split('\n').Select(v => Regex.Match(v, @"^\((-?\d*\.?\d+), (-?\d*\.?\d+), (-?\d*\.?\d+)\)$")).Select(m => p(double.Parse(m.Groups[1].Value), double.Parse(m.Groups[3].Value))).ToArray();

            const int revSteps = 6;
            const double innerRadius = .04;
            const double outerRadius = .07;
            const double displacement = .18;
            const double innerDepth = .1;
            const double outerDepth = .15;

            Pt rPt(double rds, double angle, int quadrant, double dpth) => pt(rds * cos(angle) + displacement * new[] { 1, -1, -1, 1 }[quadrant], dpth, rds * sin(angle) + displacement * new[] { 1, 1, -1, -1 }[quadrant]);

            var infs = Enumerable.Range(0, 4)
                .SelectMany(q => Enumerable.Range(0, revSteps).Select(i => i * 90.0 / (revSteps - 1)).Select(angle => new { Angle = angle, Quadrant = q }));

            List<IEnumerable<PointD>> platePoly = new List<IEnumerable<PointD>> { outline };
            for (int x = 0; x < 2; x++)
                for (int y = 0; y < 2; y++)
                {
                    var dx = x * .6 - .5;
                    var dy = y * .6 - .5;

                    foreach (var face in CreateMesh(true, false, infs.Select(inf => Ut.NewArray(
                        rPt(innerRadius, inf.Angle + 90 * inf.Quadrant, inf.Quadrant, innerDepth).Add(x: dx, z: dy).WithMeshInfo(Normal.Average, Normal.Average, Normal.Mine, Normal.Mine),
                        rPt(outerRadius, inf.Angle + 90 * inf.Quadrant, inf.Quadrant, outerDepth).Add(x: dx, z: dy).WithMeshInfo(0, 1, 0)
                    )).ToArray()))
                        yield return face;

                    platePoly.Add(infs.Reverse().Select(inf => rPt(outerRadius, inf.Angle + 90 * inf.Quadrant, inf.Quadrant, outerDepth).Apply(pt => p(pt.X + dx, pt.Z + dy))).ToArray());
                }

            foreach (var face in platePoly.Triangulate())
                yield return face.Reverse().Select(p => pt(p.X, .15, p.Y).WithNormal(0, 1, 0)).ToArray();
        }

        private static IEnumerable<VertexInfo[]> fixTextures(IEnumerable<VertexInfo[]> original)
        {
            return original.Select(o => o.Select(v => v.WithTexture(new PointD(.4771284794 * v.Location.X + .46155, -.4771284794 * v.Location.Z + .5337373145))).ToArray());
        }
    }
}