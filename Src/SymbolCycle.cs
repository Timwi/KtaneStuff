using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using KtaneStuff.Modeling;
using RT.Util;
using RT.Util.Drawing;
using RT.Util.Geometry;

namespace KtaneStuff
{
    using static Md;

    public static class SymbolCycle
    {
        public static void DoModels()
        {
            File.WriteAllText(@"D:\c\KTANE\SymbolCycle\Assets\Models\Plate.obj", GenerateObjFile(fixTextures(Plate()), "Plate"));
            File.WriteAllText(@"D:\c\KTANE\SymbolCycle\Assets\Models\ScreenHighlight.obj", GenerateObjFile(ScreenHighlight(), "ScreenHighlight"));
            File.WriteAllText(@"D:\c\KTANE\SymbolCycle\Assets\Models\Switch.obj", GenerateObjFile(Switch(), "Switch"));
            File.WriteAllText(@"D:\c\KTANE\SymbolCycle\Assets\Models\SwitchCasing.obj", GenerateObjFile(SwitchCasing(), "SwitchCasing"));
        }

        private static IEnumerable<VertexInfo[]> Switch()
        {
            const int revSteps = 9;
            const double switchAngle = 30;
            const double extraAngle = 10;
            const double depth = .5;
            const double r = 1.5;
            const double bw = .1;

            // Outer frame (rounded part and front and back)
            foreach (var baseAngle in new[] { 0, 180 - extraAngle })
            {
                foreach (var face in CreateMesh(false, false, Enumerable.Range(0, revSteps).Select(i => (switchAngle + extraAngle) * i / (revSteps - 1) + baseAngle).Select(angle => Ut.NewArray(
                    pt(0, 0, -depth).WithMeshInfo(0, 0, -1),
                    pt(r * cos(angle), r * sin(angle), -depth).WithMeshInfo(0, 0, -1),
                    pt((r + bw) * cos(angle), (r + bw) * sin(angle), -depth + bw).WithMeshInfo(cos(angle), sin(angle), 0),
                    pt((r + bw) * cos(angle), (r + bw) * sin(angle), depth - bw).WithMeshInfo(cos(angle), sin(angle), 0),
                    pt(r * cos(angle), r * sin(angle), depth).WithMeshInfo(0, 0, 1),
                    pt(0, 0, depth).WithMeshInfo(0, 0, 1)
                )).ToArray()))
                    yield return face;
            }

            // Rest of the front plate
            yield return new[] { pt(0, 0, -depth).WithNormal(0, 0, -1), pt(r * cos(switchAngle + extraAngle), r * sin(switchAngle + extraAngle), -depth).WithNormal(0, 0, -1), pt(r * cos(180 - extraAngle), r * sin(180 - extraAngle), -depth).WithNormal(0, 0, -1) }.Reverse().ToArray();
            // Rest of the back plate
            yield return new[] { pt(0, 0, depth).WithNormal(0, 0, 1), pt(r * cos(switchAngle + extraAngle), r * sin(switchAngle + extraAngle), depth).WithNormal(0, 0, 1), pt(r * cos(180 - extraAngle), r * sin(180 - extraAngle), depth).WithNormal(0, 0, 1) };

            var bAngle = 270 - switchAngle / 2;
            var data = Ut.NewArray(
                Ut.NewArray(
                    pt(r + bw, 0, -depth + bw).WithMeshInfo(1, 0, 0),
                    pt(r, 0, -depth).WithMeshInfo(0, 0, -1),
                    pt(0, 0, -depth).WithMeshInfo(0, 0, -1),
                    pt(r * cos(switchAngle + 180), r * sin(switchAngle + 180), -depth).WithMeshInfo(0, 0, -1),
                    pt((r + bw) * cos(switchAngle + 180), (r + bw) * sin(switchAngle + 180), -depth + bw).WithMeshInfo(-cos(switchAngle), -sin(switchAngle), 0)
                ),
                Ut.NewArray(
                    pt(r + bw, 0, -depth + bw).WithMeshInfo(1, 0, 0),
                    pt(r, -bw, -depth + bw).WithMeshInfo(0, -1, 0),
                    pt(bw * cos(bAngle), bw * sin(bAngle), -depth + bw).WithMeshInfo(cos(bAngle), sin(bAngle), 0),
                    pt(r * cos(switchAngle + 180) + bw * cos(switchAngle - 90), r * sin(switchAngle + 180) + bw * sin(switchAngle - 90), -depth + bw).WithMeshInfo(cos(switchAngle - 90), sin(switchAngle - 90), 0),
                    pt((r + bw) * cos(switchAngle + 180), (r + bw) * sin(switchAngle + 180), -depth + bw).WithMeshInfo(cos(switchAngle + 180), sin(switchAngle + 180), 0)
                ),
                Ut.NewArray(
                    pt(r + bw, 0, depth - bw).WithMeshInfo(1, 0, 0),
                    pt(r, -bw, depth - bw).WithMeshInfo(0, -1, 0),
                    pt(bw * cos(bAngle), bw * sin(bAngle), depth - bw).WithMeshInfo(cos(bAngle), sin(bAngle), 0),
                    pt(r * cos(switchAngle + 180) + bw * cos(switchAngle - 90), r * sin(switchAngle + 180) + bw * sin(switchAngle - 90), depth - bw).WithMeshInfo(cos(switchAngle - 90), sin(switchAngle - 90), 0),
                    pt((r + bw) * cos(switchAngle + 180), (r + bw) * sin(switchAngle + 180), depth - bw).WithMeshInfo(cos(switchAngle + 180), sin(switchAngle + 180), 0)
                ),
                Ut.NewArray(
                    pt(r + bw, 0, depth - bw).WithMeshInfo(1, 0, 0),
                    pt(r, 0, depth).WithMeshInfo(0, 0, 1),
                    pt(0, 0, depth).WithMeshInfo(0, 0, 1),
                    pt(r * cos(switchAngle + 180), r * sin(switchAngle + 180), depth).WithMeshInfo(0, 0, 1),
                    pt((r + bw) * cos(switchAngle + 180), (r + bw) * sin(switchAngle + 180), depth - bw).WithMeshInfo(cos(switchAngle + 180), sin(switchAngle + 180), 0)
                )
            );

            foreach (var face in CreateMesh(false, false, data))
                yield return face;
        }

        public static void PngCrush()
        {
            var lck = new object();
            Enumerable.Range(0, 2252).ParallelForEach(8, i =>
            {
                lock (lck)
                    Console.WriteLine($"{i}/2252");
                CommandRunner.Run("pngcr", $@"D:\c\KTANE\SymbolCycle\Assets\Symbols\Icon{i}.png", $@"D:\c\KTANE\SymbolCycle\Assets\Symbols\cr\Icon{i}.png").OutputNothing().Go();
            });
        }

        private static IEnumerable<VertexInfo[]> SwitchCasing()
        {
            const int revSteps = 17;
            const double w = .95;     // width of the whole casing
            const double u = .8;   // width of the hole for the switch
            const double h = .45;    // height of the whole casing
            const double v = .25;    // height of the hole for the switch
            const double r = .25;
            const double bw = .05;
            const double depth = .2;
            const double ic = .05;
            const double c = .025;

            return CreateMesh(true, false, Ut.NewArray(
                Ut.NewArray(pt(u - ic, v, bw).WithNormal(0, -1, 0), pt(u - ic, v + bw, 0).WithNormal(0, 0, -1), pt(w - c, h, 0).WithNormal(0, 0, -1), pt(w - c, h + bw, bw).WithNormal(0, 1, 0), pt(w - c, h + bw, depth).WithNormal(0, 1, 0)),
                Ut.NewArray(pt(u, v - ic, bw).WithNormal(-1, 0, 0), pt(u + bw, v - ic, 0).WithNormal(0, 0, -1), pt(w, h - c, 0).WithNormal(0, 0, -1), pt(w + bw, h - c, bw).WithNormal(1, 0, 0), pt(w + bw, h - c, depth).WithNormal(1, 0, 0))
            ).Concat(Enumerable.Range(0, revSteps).Select(i => 180.0 * i / (revSteps - 1)).Select(angle => Ut.NewArray(
                pt(u, v / 2 - v * angle / 180, bw).WithNormal(-1, 0, 0),
                pt(u + bw, v / 2 - v * angle / 180, 0).WithNormal(0, 0, -1),
                pt(w + r * sin(angle), r * cos(angle), 0).WithNormal(0, 0, -1),
                pt(w + (r + bw) * sin(angle) + bw * (cos(2 * angle) + 1) / 2, (r + bw) * cos(angle), bw).WithNormal(sin(angle), cos(angle), 0),
                pt(w + (r + bw) * sin(angle) + bw * (cos(2 * angle) + 1) / 2, (r + bw) * cos(angle), depth).WithNormal(sin(angle), cos(angle), 0)))
            ).Concat(Ut.NewArray(
                Ut.NewArray(pt(u, -v + ic, bw).WithNormal(-1, 0, 0), pt(u + bw, -v + ic, 0).WithNormal(0, 0, -1), pt(w, -h + c, 0).WithNormal(0, 0, -1), pt(w + bw, -h + c, bw).WithNormal(1, 0, 0), pt(w + bw, -h + c, depth).WithNormal(1, 0, 0)),
                Ut.NewArray(pt(u - ic, -v, bw).WithNormal(0, 1, 0), pt(u - ic, -v - bw, 0).WithNormal(0, 0, -1), pt(w - c, -h, 0).WithNormal(0, 0, -1), pt(w - c, -h - bw, bw).WithNormal(0, -1, 0), pt(w - c, -h - bw, depth).WithNormal(0, -1, 0))
            )).Apply(arr => arr.Select(face => face.Select(vx => vx.Location.WithMeshInfo(vx.Normal.Value)).ToArray()).Concat(arr.Select(face => face.Select(vx => vx.RotateZ(180)).Select(vx => vx.Location.WithMeshInfo(vx.Normal.Value)).ToArray()))).Reverse().ToArray());
        }

        private static IEnumerable<VertexInfo[]> ScreenHighlight()
        {
            const int revSteps = 6;
            const double innerRadius = .07;
            const double outerRadius = .12;
            const double displacement = .25;
            const double depth = 0;

            Pt rPt(double rds, double angle, int quadrant, double dpth, double dx, double dy) => pt(rds * cos(angle) + dx * new[] { 1, -1, -1, 1 }[quadrant], dpth, rds * sin(angle) + dy * new[] { 1, 1, -1, -1 }[quadrant]);

            var infs = Enumerable.Range(0, 4)
                .SelectMany(q => Enumerable.Range(0, revSteps).Select(i => i * 90.0 / (revSteps - 1)).Select(angle => new { Angle = angle, Quadrant = q }));

            foreach (var face in CreateMesh(true, false, infs.Select(inf => Ut.NewArray(
                rPt(innerRadius, inf.Angle + 90 * inf.Quadrant, inf.Quadrant, depth, displacement, displacement).WithMeshInfo(Normal.Average, Normal.Average, Normal.Mine, Normal.Mine),
                rPt(outerRadius, inf.Angle + 90 * inf.Quadrant, inf.Quadrant, depth, displacement, displacement).WithMeshInfo(0, 1, 0)
            )).ToArray()))
                yield return face;
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
            const double displacement = .25;
            const double innerDepth = .1;
            const double outerDepth = .15;

            Pt rPt(double rds, double angle, int quadrant, double dpth, double dx, double dy) => pt(rds * cos(angle) + dx * new[] { 1, -1, -1, 1 }[quadrant], dpth, rds * sin(angle) + dy * new[] { 1, 1, -1, -1 }[quadrant]);

            var infs = Enumerable.Range(0, 4)
                .SelectMany(q => Enumerable.Range(0, revSteps).Select(i => i * 90.0 / (revSteps - 1)).Select(angle => new { Angle = angle, Quadrant = q }));

            List<IEnumerable<PointD>> platePoly = new List<IEnumerable<PointD>> { outline };
            for (int x = 0; x < 2; x++)
            {
                var dx = x * .8 - .4;
                var dy = .03;

                foreach (var face in CreateMesh(true, false, infs.Select(inf => Ut.NewArray(
                    rPt(innerRadius, inf.Angle + 90 * inf.Quadrant, inf.Quadrant, innerDepth, displacement, displacement).Add(x: dx, z: dy).WithMeshInfo(Normal.Average, Normal.Average, Normal.Mine, Normal.Mine),
                    rPt(outerRadius, inf.Angle + 90 * inf.Quadrant, inf.Quadrant, outerDepth, displacement, displacement).Add(x: dx, z: dy).WithMeshInfo(0, 1, 0)
                )).ToArray()))
                    yield return face;

                platePoly.Add(infs.Reverse().Select(inf => rPt(outerRadius, inf.Angle + 90 * inf.Quadrant, inf.Quadrant, outerDepth, displacement, displacement).Apply(pt => p(pt.X + dx, pt.Z + dy))).ToArray());
            }

            const double displacementX = .67;
            const double displacementY = .1;
            const double wx = 0;
            const double wy = .65;

            foreach (var face in CreateMesh(true, false, infs.Select(inf => Ut.NewArray(
                rPt(innerRadius, inf.Angle + 90 * inf.Quadrant, inf.Quadrant, innerDepth, displacementX, displacementY).Add(x: wx, z: wy).WithMeshInfo(Normal.Average, Normal.Average, Normal.Mine, Normal.Mine),
                rPt(outerRadius, inf.Angle + 90 * inf.Quadrant, inf.Quadrant, outerDepth, displacementX, displacementY).Add(x: wx, z: wy).WithMeshInfo(0, 1, 0)
            )).ToArray()))
                yield return face;

            platePoly.Add(infs.Reverse().Select(inf => rPt(outerRadius, inf.Angle + 90 * inf.Quadrant, inf.Quadrant, outerDepth, displacementX, displacementY).Apply(pt => p(pt.X + wx, pt.Z + wy))).ToArray());

            foreach (var face in platePoly.Triangulate())
                yield return face.Reverse().Select(p => pt(p.X, .15, p.Y).WithNormal(0, 1, 0)).ToArray();
        }

        public static void CreateIcons()
        {
            var chars = File.ReadAllText(@"D:\c\KTANE\SymbolCycle\Data\Symbols.txt");
            const int w = 256;
            const int h = 256;
            var i = 0;
            var j = 0;
            while (i < chars.Length)
            {
                Console.WriteLine($"{i}/{chars.Length}");
                var ch = char.ConvertFromUtf32(char.ConvertToUtf32(chars, i));
                i += ch.Length;

                GraphicsUtil.DrawBitmap(w, h, g =>
                {
                    g.Clear(Color.Transparent);

                    var gp = new GraphicsPath();
                    gp.AddString(ch, new FontFamily("🏸🏹🏺🕋🕌🕍🕎🕺🏏🏐🏑🏒🏓".Contains(ch) ? "Segoe UI Symbol" : "Symbola"), 0, 100, new PointF(0, 0), new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center });

                    var pts = gp.PathPoints;
                    float minX = pts[0].X, maxX = pts[0].X, minY = pts[0].Y, maxY = pts[0].Y;
                    foreach (var pt in pts)
                    {
                        minX = Math.Min(minX, pt.X);
                        maxX = Math.Max(maxX, pt.X);
                        minY = Math.Min(minY, pt.Y);
                        maxY = Math.Max(maxY, pt.Y);
                    }
                    var rect = GraphicsUtil.FitIntoMaintainAspectRatio(new Size((int) (maxX - minX), (int) (maxY - minY)), new Rectangle(1, 1, w - 2, h - 2));

                    using (var tr = new GraphicsTransformer(g).Translate(-minX, -minY).Scale(rect.Width / (maxX - minX), rect.Height / (maxY - minY)).Translate(rect.Left, rect.Top))
                        g.FillPath(Brushes.White, gp);
                }).Save($@"D:\c\KTANE\SymbolCycle\Assets\Symbols\Icon{j}.png");
                j++;
            }
        }

        private static IEnumerable<VertexInfo[]> fixTextures(IEnumerable<VertexInfo[]> original)
        {
            return original.Select(o => o.Select(v => v.WithTexture(new PointD(.4771284794 * v.Location.X + .46155, -.4771284794 * v.Location.Z + .5337373145))).ToArray());
        }
    }
}