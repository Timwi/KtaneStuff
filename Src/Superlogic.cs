using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Threading;
using KtaneStuff.Modeling;
using RT.KitchenSink.Fonts;
using RT.Util;
using RT.Util.Drawing;

namespace KtaneStuff
{
    using static Md;

    static class Superlogic
    {
        public static void FindFonts()
        {
            foreach (var ff in FontUtil.GetFontFamiliesContaining("∧∨⊻|↓↔→←".Select(ch => (int) ch).ToArray()))
                Console.WriteLine(ff);
        }

        public static void DoModels()
        {
            // Models
            File.WriteAllText(@"D:\c\KTANE\Superlogic\Assets\Models\Button.obj", GenerateObjFile(Button(), "Button"));
            File.WriteAllText(@"D:\c\KTANE\Superlogic\Assets\Models\ButtonDepressed.obj", GenerateObjFile(Button(true), "ButtonDepressed"));
            File.WriteAllText(@"D:\c\KTANE\Superlogic\Assets\Models\ButtonHighlight.obj", GenerateObjFile(ButtonHighlight(), "ButtonHighlight"));
        }

        public static void DoTextures()
        {
            const bool doPngCrush = true;

            // Textures
            var filesToDelete = new List<string>();
            var threads = new List<Thread>();
            foreach (var (color, colorName) in new[] { (0xff469A47, "green"), (0xffCE323C, "red") })
            {
                foreach (var ch in "ABCD ")
                {
                    var name = ch == ' ' ? "space" : ch.ToString();
                    var offset =
                        ch == 'A' ? 0 :
                        ch == 'B' ? 8 :
                        ch == 'C' ? -4 :
                        ch == 'D' ? 8 :
                        0;
                    const int w = 512;
                    var path1 = $@"D:\c\KTANE\Superlogic\Assets\Textures\{name}-{colorName}-tmp.png";
                    var path2 = $@"D:\c\KTANE\Superlogic\Assets\Textures\{name}-{colorName}.png";
                    GraphicsUtil.DrawBitmap(w, w, g =>
                    {
                        g.Clear(Color.FromArgb(unchecked((int) color)));
                        if (ch != ' ')
                        {
                            using (var font = new Font("Work Sans SemiBold", 384, FontStyle.Regular))
                            {
                                var gp = new GraphicsPath();
                                gp.AddString(ch.ToString(), font.FontFamily, 0, 384f * g.DpiX / 72f, new PointF(0, 0), new StringFormat { LineAlignment = StringAlignment.Center, Alignment = StringAlignment.Center });
                                var points = gp.PathPoints;
                                var minX = points.Min(p => p.X);
                                var maxX = points.Max(p => p.X);
                                var minY = points.Min(p => p.Y);
                                var maxY = points.Max(p => p.Y);
                                using (var tr = new GraphicsTransformer(g).Translate(-(minX + maxX) / 2 + w / 2 + offset, -(minY + maxY) / 2 + w / 2))
                                    g.FillPath(Brushes.Black, gp);
                            }
                        }
                    }).Save(doPngCrush ? path1 : path2);
                    if (doPngCrush)
                    {
                        var thr = new Thread(() =>
                        {
                            CommandRunner.Run("pngcr", path1, path2).WithWorkingDirectory(Path.GetDirectoryName(path1)).Go();
                        });
                        threads.Add(thr);
                        thr.Start();
                        filesToDelete.Add(path1);
                    }
                }
            }
            foreach (var thr in threads)
                thr.Join();
            foreach (var file in filesToDelete)
                File.Delete(file);
        }

        private static IEnumerable<VertexInfo[]> Button(bool depressed = false)
        {
            var w = .1;
            var c = .05;
            var cf = .55 * c;
            var h = .03;
            var hf = h * .55;
            var wf = .55 * c;
            var wc = w - c;
            var bézierSteps = 20;

            if (depressed)
                h = .001f;

            var faces = new List<VertexInfo[]>();

            var sidePiece = BézierPatch(
                   pt(wc, h, -wc), pt(0, h, -wc), pt(0, h, -wc), pt(-wc, h, -wc),
                   pt(wc, h, -wc - wf), pt(0, h, -wc - wf), pt(0, h, -wc - wf), pt(-wc, h, -wc - wf),
                   pt(wc, hf, -w), pt(0, hf, -w), pt(0, hf, -w), pt(-wc, hf, -w),
                   pt(wc, 0, -w), pt(0, 0, -w), pt(0, 0, -w), pt(-wc, 0, -w),
                   bézierSteps);

            foreach (var angle in new[] { 0, 90, 180, 270 })
                foreach (var face in CreateMesh(false, false, Ut.NewArray(bézierSteps, bézierSteps, (u, v) => new MeshVertexInfo(
                    sidePiece[u][v].RotateY(angle),
                    u == bézierSteps - 1 ? Normal.Mine : Normal.Average,
                    u == 0 ? Normal.Mine : Normal.Average,
                    v == bézierSteps - 1 ? Normal.Mine : Normal.Average,
                    v == 0 ? Normal.Mine : Normal.Average))
                ))
                    faces.Add(face);

            var cornerPiece = BézierPatch(
                    pt(wc, h, -wc), pt(wc, h, -wc), pt(wc, h, -wc), pt(wc, h, -wc),
                    pt(wc + wf, h, -wc), pt(wc + wf, h, -wc - wf), pt(wc + wf, h, -wc - wf), pt(wc, h, -wc - wf),
                    pt(w, hf, -wc), pt(w, hf, -wc - cf), pt(wc + cf, hf, -w), pt(wc, hf, -w),
                    pt(w, 0, -wc), pt(w, 0, -wc - cf), pt(wc + cf, 0, -w), pt(wc, 0, -w),
                    bézierSteps);

            foreach (var angle in new[] { 0, 90, 180, 270 })
                foreach (var face in CreateMesh(false, false, Ut.NewArray(bézierSteps, bézierSteps, (u, v) =>
                u == 0 ? new MeshVertexInfo(cornerPiece[u][v].RotateY(angle), pt(0, 1, 0)) :
                new MeshVertexInfo(
                    cornerPiece[u][v].RotateY(angle),
                    u == bézierSteps - 1 ? Normal.Mine : Normal.Average,
                    u == 0 ? Normal.Mine : Normal.Average,
                    v == bézierSteps - 1 ? Normal.Mine : Normal.Average,
                    v == 0 ? Normal.Mine : Normal.Average))
                ))
                    faces.Add(face);

            faces.Add(new[] { pt(-wc, h, -wc), pt(-wc, h, wc), pt(wc, h, wc), pt(wc, h, -wc) }.Select(p => p.WithNormal(0, 1, 0)).ToArray());

            return faces.Select(f => f.Select(v => v.WithTexture(1 - (v.Location.X + w) / (2 * w), (v.Location.Z + w) / (2 * w)).Move(x: -w)).ToArray());
        }

        private static IEnumerable<VertexInfo[]> ButtonHighlight()
        {
            const int revSteps = 6;
            const double innerRadius = .05;
            const double outerRadius = .075;
            const double displacement = .05;
            const double depth = 0;

            Pt rPt(double rds, double angle, int quadrant, double dpth, double dx, double dy) => pt(rds * cos(angle) + dx * new[] { 1, -1, -1, 1 }[quadrant], dpth, rds * sin(angle) + dy * new[] { 1, 1, -1, -1 }[quadrant]);

            var infs = Enumerable.Range(0, 4)
                .SelectMany(q => Enumerable.Range(0, revSteps).Select(i => i * 90.0 / (revSteps - 1)).Select(angle => new { Angle = angle, Quadrant = q }));

            foreach (var face in CreateMesh(true, false, infs.Select(inf => Ut.NewArray(
                rPt(innerRadius, inf.Angle + 90 * inf.Quadrant, inf.Quadrant, depth, displacement, displacement).WithMeshInfo(Normal.Average, Normal.Average, Normal.Mine, Normal.Mine),
                rPt(outerRadius, inf.Angle + 90 * inf.Quadrant, inf.Quadrant, depth, displacement, displacement).WithMeshInfo(0, 1, 0)
            )).ToArray()))
                yield return face.Reverse().Select(v => v.Move(x: -.1)).ToArray();
        }
    }
}
