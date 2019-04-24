using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using KtaneStuff.Modeling;
using RT.Util;
using RT.Util.Drawing;
using RT.Util.Geometry;

namespace KtaneStuff
{
    using static Md;

    static class SimonSpins
    {
        public static void DoModels()
        {
            File.WriteAllText($@"D:\c\KTANE\SimonSpins\Assets\Models\PaddleHeadCircle.obj", GenerateObjFile(PaddleHead(36, 0, Normal.Average), "PaddleHeadCircle"));
            File.WriteAllText($@"D:\c\KTANE\SimonSpins\Assets\Models\PaddleHeadSquare.obj", GenerateObjFile(PaddleHead(4, 45, Normal.Mine), "PaddleHeadSquare"));
            File.WriteAllText($@"D:\c\KTANE\SimonSpins\Assets\Models\PaddleHeadPentagon.obj", GenerateObjFile(PaddleHead(5, 90, Normal.Mine), "PaddleHeadPentagon"));
            File.WriteAllText($@"D:\c\KTANE\SimonSpins\Assets\Models\HighlightCircle.obj", GenerateObjFile(Enumerable.Range(0, 36).Select(i => i * 360 / 36).Select(angle => p(cos(angle), sin(angle))).Extrude(1, true, true), "HighlightCircle"));
            File.WriteAllText($@"D:\c\KTANE\SimonSpins\Assets\Models\HighlightSquare.obj", GenerateObjFile(new[] { p(-1, -1), p(1, -1), p(1, 1), p(-1, 1) }.Extrude(1, true, true), "HighlightSquare"));
            File.WriteAllText($@"D:\c\KTANE\SimonSpins\Assets\Models\HighlightPentagon.obj", GenerateObjFile(Enumerable.Range(0, 5).Select(i => i * 360 / 5 - 90).Select(angle => p(cos(angle), sin(angle))).Extrude(1, true, true), "HighlightPentagon"));
            File.WriteAllText($@"D:\c\KTANE\SimonSpins\Assets\Models\FaceCircle.obj", GenerateObjFile(LooseModels.Disc(12), "FaceCircle"));
            File.WriteAllText($@"D:\c\KTANE\SimonSpins\Assets\Models\FacePentagon.obj", GenerateObjFile(LooseModels.Disc(5, addAngle: -90), "FacePentagon"));
            File.WriteAllText($@"D:\c\KTANE\SimonSpins\Assets\Models\Pivot.obj", GenerateObjFile(Pivot(), "Pivot"));
            File.WriteAllText($@"D:\c\KTANE\SimonSpins\Assets\Models\Protrusion.obj", GenerateObjFile(Protrusion(), "Protrusion"));
        }

        private static IEnumerable<VertexInfo[]> Pivot()
        {
            const double r = .1;
            const double h = .5;
            const int revSteps = 36;

            return CreateMesh(true, false,
                Enumerable.Range(0, revSteps)
                    .Select(i => i * 360 / revSteps)
                    .Reverse()
                    .Select(angle => new[] { pt(-r, 0, 0) }
                        .Concat(Enumerable.Range(0, revSteps).Select(j => j * 90 / (revSteps - 1)).Select(th => pt(-r * cos(th), h + r * sin(th), 0)))
                        .Select(p => p.RotateY(angle))
                        .Select((p, ix) =>
                            ix == 0 ? p.WithMeshInfo(Normal.Average, Normal.Average, Normal.Mine, Normal.Mine) :
                            ix == revSteps ? p.WithMeshInfo(0, 1, 0) : p.WithMeshInfo(Normal.Average, Normal.Average, Normal.Average, Normal.Average))
                        .ToArray())
                    .ToArray());
        }

        private static IEnumerable<VertexInfo[]> PaddleHead(int revSteps, int addAngle, Normal normal)
        {
            const double r = 1;
            const double h = .21;
            const double t = .1;
            const int bevelRevSteps = 6;

            var pts = new List<(PointD point, PointD? normalOverride, Normal? normal)>();
            pts.Add((p(0, 0), p(0, -1), null));
            pts.Add((p(-r, 0), null, Normal.Mine));
            for (var i = 0; i < bevelRevSteps; i++)
            {
                var angle = 180 * i / bevelRevSteps;
                pts.Add((p(-r + t - t * cos(angle), h + t * sin(angle)), null, Normal.Average));
            }
            pts.Add((p(-r + 2 * t, h), null, Normal.Mine));

            return CreateMesh(true, false, Enumerable.Range(0, revSteps)
                .Select(i => 360 * i / revSteps + addAngle)
                .Reverse()
                .Select(angle => pts
                    .Select(inf => pt(inf.point.X * cos(angle), inf.point.Y, inf.point.X * sin(angle))
                        .Apply(p => inf.normalOverride == null
                            ? p.WithMeshInfo(normal, normal, inf.normal.Value, inf.normal.Value)
                            : p.WithMeshInfo(inf.normalOverride.Value.X * cos(angle), inf.normalOverride.Value.Y, inf.normalOverride.Value.X * sin(angle))))
                    .ToArray())
                .ToArray());
        }

        private static IEnumerable<VertexInfo[]> Protrusion()
        {
            const int revSteps = 36;
            const int revSteps2 = 12;
            const double r = 1;
            const double h = 2;
            return CreateMesh(true, false, Enumerable.Range(0, revSteps).Select(i => i * 360 / revSteps).Reverse()
                .Select(angle => Enumerable.Range(0, revSteps2).Select(i => i * 90 / (revSteps2 - 1))
                    .Select(angle2 => pt(r * cos(angle2), h * sin(angle2), 0).RotateY(angle))
                    .ToArray())
                .ToArray());
        }

        public static void DoSymbols()
        {
#pragma warning disable CS0162 // Unreachable code detected
            if (false)
                using (var bmp = new Bitmap(@"D:\c\KTANE\SimonSpins\Data\Symbols.png"))
                {
                    var w = bmp.Width / 12;
                    var h = bmp.Height / 9;
                    for (var x = 0; x < 12; x++)
                        for (var y = 0; y < 9; y++)
                            GraphicsUtil.DrawBitmap(w, h, g => { g.Clear(Color.Transparent); g.DrawImage(bmp, -w * x, -h * y); }).Save($@"D:\c\KTANE\SimonSpins\Assets\Textures\Symbol-{x}-{y}.png");
                }
            if (true)
                using (var bmp = new Bitmap(@"D:\c\KTANE\SimonSpins\Data\BackSymbols.png"))
                {
                    var w = bmp.Width / 3;
                    var h = bmp.Height / 3;
                    for (var x = 0; x < 3; x++)
                        for (var y = 0; y < 3; y++)
                            GraphicsUtil.DrawBitmap(w, h, g => { g.Clear(Color.Transparent); g.DrawImage(bmp, -w * x, -h * y); }).Save($@"D:\c\KTANE\SimonSpins\Assets\Textures\BackSymbol-{x}-{y}.png");
                }
#pragma warning restore CS0162 // Unreachable code detected
        }
    }
}