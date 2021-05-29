using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using KtaneStuff.Modeling;
using RT.Util;
using RT.Util.ExtensionMethods;
using RT.Util.Geometry;

namespace KtaneStuff
{
    using static Md;

    static class Variety
    {
        public static void DoModels()
        {
            File.WriteAllText(@"D:\c\KTANE\Variety\Assets\Items\Wire\WireBase.obj", GenerateObjFile(WireBase(), "WireBase"));
            File.WriteAllText(@"D:\c\KTANE\Variety\Assets\Items\Key\KeyBase1.obj", GenerateObjFile(KeyBase(1), "KeyBase1"));
            File.WriteAllText(@"D:\c\KTANE\Variety\Assets\Items\Key\KeyBase2.obj", GenerateObjFile(KeyBase(2), "KeyBase2"));

            File.WriteAllText($@"D:\c\KTANE\Variety\Assets\Items\Maze\Frame3x3.obj", GenerateObjFile(MazeFrame(MazeComponent.Frame, 3, 3), $"Frame3x3"));
            File.WriteAllText($@"D:\c\KTANE\Variety\Assets\Items\Maze\Back3x3.obj", GenerateObjFile(MazeFrame(MazeComponent.Back, 3, 3), $"Back3x3"));
            File.WriteAllText($@"D:\c\KTANE\Variety\Assets\Items\Maze\Button.obj", GenerateObjFile(MazeButton(), "Button"));
            File.WriteAllText($@"D:\c\KTANE\Variety\Assets\Items\Maze\ButtonHighlight.obj", GenerateObjFile(MazeButtonHighlight(), "ButtonHighlight"));
        }

        private static IEnumerable<VertexInfo[]> MazeButton()
        {
            const double w = .1;
            const double h = .05;
            const double ow = .15;
            const double oh = .07;
            var inner = new[] { p(-w, 0), p(w, 0), p(0, -h) }.Select(p => pt(p.X, .05, p.Y)).ToArray();
            var outer = new[] { p(-ow, 0), p(ow, 0), p(0, -oh) }.Select(p => pt(p.X, 0, p.Y)).ToArray();
            return CreateMesh(false, true, new[] { inner, outer }, flatNormals: true).Concat(inner.Select(p => p.WithNormal(0, 1, 0)).ToArray());
        }

        private static IEnumerable<VertexInfo[]> MazeButtonHighlight()
        {
            const double w = .15;
            const double h = .07;
            yield return new[] { p(-w, 0), p(0, -h), p(0, 0) }.Select(p => pt(p.X, .05, p.Y)).Select(p => p.WithNormal(0, 1, 0)).ToArray();
            yield return new[] { p(0, 0), p(0, -h), p(w, 0) }.Select(p => pt(p.X, .05, p.Y)).Select(p => p.WithNormal(0, 1, 0)).ToArray();
        }

        enum MazeComponent { Frame, Back }

        private static IEnumerable<VertexInfo[]> MazeFrame(MazeComponent which, int mazeWidth, int mazeHeight)
        {
            const double cw = .0125 / 2;
            const double ch = .0125 / 2;

            MeshVertexInfo[] frameImpl(Normal xNormal, double tx, double w, double h, double y, double bevel)
            {
                var bl = bevel * Math.Sqrt(2);
                var xl = w - 2 * bevel;
                var yl = h - 2 * bevel;
                var f = 4 * bl + 2 * xl + 2 * yl;
                bl /= f;
                xl /= f;
                yl /= f;
                return Ut.NewArray<(double? ty, Pt p)>(
                        (null, pt(-w + bevel, y, h)),
                        (xl, pt(w - bevel, y, h)),
                        (xl + bl, pt(w, y, h - bevel)),
                        (xl + yl + bl, pt(w, y, -h + bevel)),
                        (xl + yl + 2 * bl, pt(w - bevel, y, -h)),
                        (2 * xl + yl + 2 * bl, pt(-w + bevel, y, -h)),
                        (2 * xl + yl + 3 * bl, pt(-w, y, -h + bevel)),
                        (2 * xl + 2 * yl + 3 * bl, pt(-w, y, h - bevel)))
                    .Select(tup => new MeshVertexInfo(tup.p, xNormal, xNormal, Normal.Mine, Normal.Mine, new PointD(tx, tup.ty ?? 1), new PointD(tx, tup.ty ?? 0)))
                    .ToArray();
            }
            MeshVertexInfo[] frame(Normal xNormal, double ty, double margin, double y, double bevel) => frameImpl(xNormal, ty, mazeWidth * cw + margin, mazeHeight * ch + margin, y, bevel);

            return which switch
            {
                MazeComponent.Frame => CreateMesh(false, true, Ut.NewArray(
                    frame(Normal.Mine, 0, .11, 0, .015),
                    frame(Normal.Average, .6, .12, .02, .016),
                    frame(Normal.Mine, 1, .126, 0, .0166))),

                MazeComponent.Back => frame(Normal.Mine, 0, .11, 0, .015).SelectConsecutivePairs(true, (v1, v2) => new[] { pt(0, 0, 0).WithNormal(0, 1, 0), v1.Location.WithNormal(0, 1, 0), v2.Location.WithNormal(0, 1, 0) }).ToArray(),

                _ => throw new InvalidOperationException(),
            };
        }

        private static IEnumerable<VertexInfo[]> KeyBase(int which)
        {
            const double h = .7;
            const double w = .5;
            const double c1 = .3;
            const double c2 = .2;
            const double c3 = .3;
            const int rev = 32;

            switch (which)
            {
                case 1:
                    var bézier = SmoothBézier(p(c3, h), p(c3 + c2, h), p(w, c1), p(w, 0), .005).ToArray();
                    return CreateMesh(true, false, Enumerable.Range(0, rev)
                        .Select(i => i * 360.0 / rev)
                        .Select((angle, fa, la) => bézier
                            .Select((p, i, first, last) => pt(p.X, p.Y, 0).RotateY(angle).Apply(pr =>
                                first ? pr.WithMeshInfo(0, 1, 0) :
                                last ? pr.WithMeshInfo(Normal.Average, Normal.Average, Normal.Mine, Normal.Theirs) :
                                pr.WithMeshInfo(Normal.Average, Normal.Average, Normal.Average, Normal.Average))
                                .WithTexture(fa ? 1 : angle / 360.0, (double) i / (bézier.Length - 1), fa ? 0 : angle / 360.0, (double) i / (bézier.Length - 1)))
                            .ToArray())
                        .ToArray())
                        .SelectMany(arr => new[] { new[] { arr[0], arr[1], arr[3] }, new[] { arr[3], arr[1], arr[2] } });

                default:
                    const double slitW = .035;
                    const double slitH = .1;

                    var outline = Enumerable.Range(0, rev).Select(i => i * 360 / rev).Select(angle => pt(c3, h, 0).RotateY(angle)).Select(pt => p(pt.X, pt.Z)).ToArray();
                    var slit = new[] { p(-slitW, -slitH), p(slitW, -slitH), p(slitW, slitH), p(-slitW, slitH) }.ReverseInplace();
                    return new[] { outline, slit }.Triangulate().Select(tri => tri.Select(p => pt(p.X, h, p.Y).WithNormal(0, 1, 0).WithTexture((p.X + c3) / (2 * c3), (p.Y + c3) / (2 * c3))).Reverse().ToArray());
            }
        }

        private static IEnumerable<VertexInfo[]> WireBase()
        {
            const double height = .05;
            const double holeI = .03;
            const double holeO = .05;
            const double holeDepth = .05;

            var curve = new[] { p(0, height - holeDepth), p(holeI, height), p(holeO, height), p(holeO, 0) };

            return CreateMesh(true, false, Enumerable.Range(0, 4)
                .Select(i => i * 360.0 / 4)
                .Select(angle => curve
                    .Select(p => pt(p.X, p.Y, 0).RotateY(angle).Apply(pr => pr.WithMeshInfo(Normal.Average, Normal.Average, Normal.Mine, Normal.Mine)))
                    .ToArray())
                .ToArray());
        }
    }
}