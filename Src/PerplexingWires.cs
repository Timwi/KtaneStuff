using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using KtaneStuff.Modeling;
using RT.Util.Drawing;
using RT.Util.ExtensionMethods;

namespace KtaneStuff
{
    using static Md;

    public static class PerplexingWires
    {
        public static void DoModels()
        {
            File.WriteAllText(@"D:\c\KTANE\PerplexingWires\Assets\Models\Strip1.obj", GenerateObjFile(Strip(1.25), "Strip1"));
            File.WriteAllText(@"D:\c\KTANE\PerplexingWires\Assets\Models\Strip2.obj", GenerateObjFile(Strip(.8), "Strip2"));
            File.WriteAllText(@"D:\c\KTANE\PerplexingWires\Assets\Models\Strip3.obj", GenerateObjFile(Strip(.5, depth: .1, bevelX: .1, bevelY: .075), "Strip3"));
            File.WriteAllText(@"D:\c\KTANE\PerplexingWires\Assets\Models\Connector.obj", GenerateObjFile(Connector(), "Connector"));
            File.WriteAllText(@"D:\c\KTANE\PerplexingWires\Assets\Models\LedFrame.obj", GenerateObjFile(LedFrame(), "LedFrame"));
            File.WriteAllText(@"D:\c\KTANE\PerplexingWires\Assets\Models\Led.obj", GenerateObjFile(Led(), "Led"));
        }

        private static IEnumerable<VertexInfo[]> Connector()
        {
            const double innerRadius = .1;
            const double outerRadius = .15;
            const double height = .1;
            const int numSteps = 6;
            return CreateMesh(false, true,
                new[] { p(outerRadius, 0), p(outerRadius, height), p(innerRadius, height), p(innerRadius, 0), p(0, 0) }
                    .Select(p => Enumerable.Range(0, numSteps)
                        .Select(i => (360 * i / numSteps))
                        .Select(angle => pt(p.X * cos(angle), p.Y, p.X * sin(angle)).WithMeshInfo(Normal.Mine, Normal.Mine, Normal.Mine, Normal.Mine))
                        .ToArray())
                    .ToArray());
        }

        private static IEnumerable<VertexInfo[]> Strip(double wd /* Actual width is 2×(wd + bvx) */, double depth = .2, double bevelX = .15, double bevelY = .1)
        {
            const double hg = .15; // Actual height is 2×(hg + bvy)

            var a = pt(-wd - bevelX, 0, -hg - bevelY);
            var b = pt(wd + bevelX, 0, -hg - bevelY);
            var c = pt(-wd, depth, -hg);
            var d = pt(wd, depth, -hg);
            var e = pt(-wd, depth, hg);
            var f = pt(wd, depth, hg);
            var g = pt(-wd - bevelX, 0, hg + bevelY);
            var h = pt(wd + bevelX, 0, hg + bevelY);

            yield return new[] { c, d, b, a }.Select(v => v.WithNormal(-(b - a) * (c - a))).ToArray();  // Top
            yield return new[] { d, f, h, b }.Select(v => v.WithNormal(-(h - b) * (d - b))).ToArray();  // Right
            yield return new[] { g, h, f, e }.Select(v => v.WithNormal(-(g - h) * (f - h))).ToArray();  // Bottom
            yield return new[] { g, e, c, a }.Select(v => v.WithNormal((g - a) * (c - a))).ToArray();  // Left
            yield return new[] { e, f, d, c }.Select(v => v.WithNormal(0, 1, 0)).ToArray();  // Front face

            var vector = -(b - a) * (c - a);
            var x = vector.X;
            var y = vector.Y;
            var z = vector.Z;
            var r = Math.Sqrt(x * x + y * y + z * z);
            var t = Math.Atan2(y, x) / Math.PI * 180;
            var p = Math.Acos(z / r) / Math.PI * 180;

            //Console.WriteLine($"For width {wd}, t={t}, p={p}");
        }

        public static void DoGraphics()
        {
            using (var bmp = new Bitmap(@"D:\c\KTANE\PerplexingWires\Data\Graphics.png"))
            {
                var w = bmp.Width / 5;
                var h = bmp.Height / 5;
                for (int x = 0; x < 5; x++)
                    for (int y = 0; y < 5; y++)
                        if (x < 2 || y > 0)
                            GraphicsUtil.DrawBitmap(w, h, g => { g.DrawImage(bmp, -w * x, -h * y); }).Save($@"D:\c\KTANE\PerplexingWires\Assets\Textures\T_{x}_{y}.png");
            }
        }

        private static IEnumerable<VertexInfo[]> LedFrame()
        {
            const double innerRadius = .1;
            const double outerRadius = .125;
            const double height = .025;
            const double height2 = .035;
            const int numSteps = 7;
            return CreateMesh(false, true,
               new[] { p(outerRadius, 0), p(outerRadius, height), p((outerRadius + innerRadius) / 2, height2), p(innerRadius, height), p(innerRadius, 0) }
                   .Select((p, pix) => Enumerable.Range(0, numSteps)
                       .Select(i => (360 * i / numSteps))
                       .Select(angle => pt(p.X * cos(angle), p.Y, p.X * sin(angle)).WithMeshInfo(pix == 2 ? Normal.Average : Normal.Mine, pix == 2 ? Normal.Average : Normal.Mine, Normal.Mine, Normal.Mine))
                       .ToArray())
                   .ToArray());
        }

        private static IEnumerable<VertexInfo[]> Led()
        {
            const double radius = .115;
            const int numSteps = 7;
            return CreateMesh(false, true,
               new[] { p(radius, 0), p(0, 0) }
                   .Select(p => Enumerable.Range(0, numSteps)
                       .Select(i => (360 * i / numSteps))
                       .Select(angle => pt(p.X * cos(angle), p.Y, p.X * sin(angle)).WithMeshInfo(Normal.Mine, Normal.Mine, Normal.Mine, Normal.Mine))
                       .ToArray())
                   .ToArray());
        }
    }
}