using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using KtaneStuff.Modeling;
using RT.Util;
using RT.Util.ExtensionMethods;
using RT.Util.Geometry;

namespace KtaneStuff
{
    using static Md;

    static class UltimateTicTacToe
    {
        public static void CreateModels()
        {
            File.WriteAllText(@"D:\c\KTANE\UltimateTicTacToe\Assets\Models\Surface.obj", GenerateObjFile(Surface(), "Surface"));
        }

        private static IEnumerable<VertexInfo[]> Surface()
        {
            const double sm = 0.005;     // small margin
            const double lm = 0.04;     // large margin
            const double w = 0.854439 - lm;
            const int revSteps = 3;
            const double radius = .02;  // bevel radius
            const bool open = false;
            const double extraWallDepth = 0;

            var totalMargin = 6 * sm + 2 * lm;
            var sq = (2 * w - totalMargin) / 9;
            static PointD tx(double x, double z) => new PointD(.4771284794 * x + .46155, -.4771284794 * z + .5337373145);

            // Step 1: generate the bevels for each square
            var allSquares = new List<PointD[]>();
            for (var x = 0; x < 9; x++)
                for (var y = 0; y < 9; y++)
                {
                    var sqx = -w + (x / 3) * (3 * sq + 2 * sm + lm) + (x % 3) * (sq + sm);
                    var sqy = -w + (y / 3) * (3 * sq + 2 * sm + lm) + (y % 3) * (sq + sm);
                    //var coords = new List<PointD> { p(sqx, sqy), p(sqx + sq, sqy), p(sqx + sq, sqy + sq), p(sqx, sqy + sq) };
                    var coords = new List<PointD> { p(sqx, sqy), p(sqx, sqy + sq), p(sqx + sq, sqy + sq), p(sqx + sq, sqy) };
                    allSquares.Add(coords.ToArray().Reverse().ToArray());

                    var pts = coords.Select(p => pt(p.X, 0.15, p.Y)).ToList();

                    static Pt mn(Pt p) => p.Normalize();

                    var nPts = pts
                        .Select((p, ix) => new
                        {
                            AxisStart = !open || ix != pts.Count - 1 ? p.Add(y: -radius) : pts[pts.Count - 2].Add(y: -radius),
                            AxisEnd = !open || (ix != 0 && ix != pts.Count - 1) ? p.Add(y: -radius) + mn(pts[(ix + 1) % pts.Count] - p) + mn(p - pts[(ix - 1 + pts.Count) % pts.Count]) :
                                ix == 0 ? pts[1].Add(y: -radius) : pts[pts.Count - 1].Add(y: -radius),
                            Perpendicular = pts[ix],
                            Center = pts[ix].Add(y: -radius)
                        })
                        .Select(inf => Enumerable.Range(0, revSteps)
                            .Select(i => -90 * i / (revSteps - 1))
                            .Select(angle => new { inf.Center, Angle = angle, Rotated = inf.Perpendicular.Rotate(inf.AxisStart, inf.AxisEnd, angle) })
                            .Apply(x => x.Concat(new { Center = inf.Center.Add(y: -extraWallDepth), Angle = -90, Rotated = inf.Perpendicular.Rotate(inf.AxisStart, inf.AxisEnd, -90).Add(y: -extraWallDepth) }))
                            .ToArray())
                        .ToArray();

                    var faces = Enumerable.Range(0, nPts.Length)
                        .SelectConsecutivePairs(!open, (i1, i2) => Enumerable.Range(0, nPts[0].Length)
                            .SelectConsecutivePairs(false, (j1, j2) => new[] { nPts[i1][j1], nPts[i2][j1], nPts[i2][j2], nPts[i1][j2] }
                                .Select(inf => new VertexInfo(
                                    loc: inf.Rotated,
                                    normal: pts[i2].Rotate(pts[i1].Add(y: -radius), pts[i2].Add(y: -radius), inf.Angle) - pts[i2].Add(y: -radius),
                                    texture: tx(inf.Rotated.X, inf.Rotated.Z))).ToArray()))
                        .SelectMany(x => x);
                    foreach (var face in faces)
                        yield return face;
                }

            var outline = Ut.NewArray(
                p(-0.854439, 0.256271),
                p(-0.854439, 0.854439),
                p(-0.256273, 0.854439),
                p(0.256273, 0.854439),
                p(0.854439, 0.854439),
                p(0.854439, 0.256271),
                p(0.854439, -0.256271),
                p(0.854439, -0.854439),
                p(0.69, -0.854439),
                p(0.256273, -0.854439),
                p(-0.256271, -0.854439),
                p(-0.854439, -0.854439),
                p(-0.854439, -0.256271));

            var triangles = allSquares.Concat(outline).Triangulate();
            foreach (var triangle in triangles)
                yield return triangle.Reverse().Select(v => pt(v.X, .15, v.Y).WithNormal(0, 1, 0).WithTexture(tx(v.X, v.Y))).ToArray();
        }
    }
}