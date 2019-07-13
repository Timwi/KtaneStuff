using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Xml.Linq;
using KtaneStuff.Modeling;
using RT.KitchenSink;
using RT.Util;
using RT.Util.Drawing;
using RT.Util.ExtensionMethods;
using RT.Util.Geometry;

namespace KtaneStuff
{
    using static Md;

    static class OneHundredAndOneDalmatians
    {
        private static readonly (double d, double a, int r)[][] _patterns;

        static OneHundredAndOneDalmatians()
        {
            var already = new HashSet<string>();
            var number = 1;

            // Amount of variety
            var iv = 5;
            var v1 = 4;
            var v2 = 4;
            var v3 = 4;
            var v4 = 4;

            var patterns = new List<(double d, double a, int r)[]>();
            while (number < iv * v1 * v2 * v3 + iv * v1 * v4)
            {
                var stage2 = number >= iv * v1 * v2 * v3;
                var inner = number % iv;
                var c1 = (number / iv) % v1;
                var c2 = stage2 ? 0 : (number / (iv * v1)) % v2;
                var c3 = stage2 ? 0 : (number / (iv * v1 * v2)) % v3;
                var c4 = stage2 ? ((number - iv * v1 * v2 * v3) / (iv * v1)) % v4 : 0;
                number++;

                if (new[] { inner, c1, c2, c3, c4 }.Count(x => x != 0) < 3)
                    continue;

                var key = stage2 ? $"{inner},{c1},{c4}" : $"{inner},{c1},{c2},{c3}";
                if (already.Contains(key) ||
                    (stage2 ? already.Contains($"{inner},{c4},{c1}") : (already.Contains($"{inner},{c2},{c3},{c1}") || already.Contains($"{inner},{c3},{c1},{c2}"))))
                    continue;
                already.Add(key);

                var radiusDist = 5;
                var pattern = new List<(double d, double a, int r)>();
                if (inner != 0)
                    pattern.Add((0, 0, inner));
                if (c1 != 0)
                    pattern.Add((radiusDist, 0, c1));
                if (c2 != 0)
                    pattern.Add((radiusDist, 120, c2));
                if (c3 != 0)
                    pattern.Add((radiusDist, 240, c3));
                if (c4 != 0)
                    pattern.Add((radiusDist, 90, c4));
                patterns.Add(pattern.ToArray());
            }
            _patterns = patterns.ToArray();
        }

        public static void DoManual()
        {
            // Generate 
            var rnd = new MonoRandom(1);
            var skip = rnd.Next(100);
            while (skip-- > 0)
                rnd.NextDouble();
            var patterns = _patterns.Select((p, i) => (p, i)).ToArray();
            rnd.ShuffleFisherYates(patterns);
            var svgs = new List<string>();
            for (int i = 0; i < 101; i++)
            {
                var rotation = 360 * rnd.NextDouble();
                var pattern = patterns[i].p.Select(c => (x: c.d * cos(c.a + rotation), y: c.d * sin(c.a + rotation), c.r)).ToArray();
                var mx = (pattern.Min(c => c.x - c.r) + pattern.Max(c => c.x + c.r)) / 2;
                var my = (pattern.Min(c => c.y - c.r) + pattern.Max(c => c.y + c.r)) / 2;
                svgs.Add($@"<!--{patterns[i].i}--><svg xmlns='http://www.w3.org/2000/svg' viewBox='{mx - 8} {my - 8} 16 16'>{pattern.Select(c => $"<circle cx='{c.x:0.###}' cy='{c.y:0.###}' r='{c.r}' />").JoinString()}</svg>");
            }
            var names = File.ReadAllLines(@"D:\c\KTANE\101Dalmatians\Data\Names.txt");
            var cols = 11;
            Utils.ReplaceInFile(@"D:\c\KTANE\Public\HTML\101 Dalmatians.html", "<!--%%-->", "<!--%%%-->",
                Enumerable.Range(0, (101 + cols - 1) / cols).Select(row => $@"<tr>{Enumerable.Range(0, cols)
                    .Select(col => col + cols * row >= 101 ? null : $"<td><div class='pattern'>{svgs[col + cols * row]}</div><div class='name'>{names[col + cols * row]}</div></td>")
                    .JoinString()}</tr>").JoinString(Environment.NewLine));
        }

        public static void MakeTextures()
        {
            var threads = new List<Thread>();
            var rnd = new Random(47);
            for (int i = 0; i < _patterns.Length; i++)
            {
                var bmp = MakeTexture(i, rnd);
                var old = $@"D:\c\KTANE\101Dalmatians\Assets\Textures\Fur{i}_tmp.png";
                var newf = $@"D:\c\KTANE\101Dalmatians\Assets\Textures\Fur{i}.png";
                bmp.Save(old);
                while (threads.Count >= 2)
                    Thread.Sleep(100);
                Thread thr = null;
                var j = i;
                thr = new Thread(() =>
                {
                    lock (threads)
                        Console.WriteLine($"Fur {j} START");
                    CommandRunner.Run(@"pngcr", old, newf).OutputNothing().Go();
                    File.Delete(old);
                    lock (threads)
                    {
                        threads.Remove(thr);
                        Console.WriteLine($"Fur {j} DONE");
                    }
                });
                thr.Start();
                lock (threads)
                    threads.Add(thr);
            }
            while (threads.Count > 0)
                Thread.Sleep(100);
        }

        private static Bitmap MakeTexture(int patternIx, Random rnd)
        {
            var patternRotation = rnd.NextDouble() * 360;
            var pattern = _patterns[patternIx].Select(p => (x: p.d * cos(p.a + patternRotation), y: p.d * sin(p.a + patternRotation), p.r)).ToArray();
            var mx = (pattern.Min(c => c.x - c.r) + pattern.Max(c => c.x + c.r)) / 2;
            var my = (pattern.Min(c => c.y - c.r) + pattern.Max(c => c.y + c.r)) / 2;

            const int w = 200;
            const int h = w;
            const int wContrast = 152;
            const int bContrast = 64;
            return GraphicsUtil.DrawBitmap(w, h, g =>
            {
                g.Clear(Color.GhostWhite);
                for (int y = h; y >= -10; y -= 2)
                {
                    for (int x = 0; x < w; x += 2)
                    {
                        var pt = new PointD(x * 18d / w - 9, y * 18d / h - 9);
                        var inside = pattern.Any(p => pt.Distance(new PointD(p.x - mx, p.y - my)) < p.r);
                        var angle = rnd.NextDouble() * 45 - 45 * .5;
                        using (var tr = new GraphicsTransformer(g).RotateAt((float) angle, x, y))
                        {
                            var c = inside ? rnd.Next(0, bContrast) : 255 - rnd.Next(0, wContrast);
                            g.DrawLine(new Pen(Color.FromArgb(c, c, c), .5f), x, y, x, y + 10);
                        }
                    }
                }
            });
        }

        public static void DoModels()
        {
            File.WriteAllText(@"D:\c\KTANE\101Dalmatians\Assets\Models\Disc.obj", GenerateObjFile(Disc(), "Disc"));
            File.WriteAllText(@"D:\c\KTANE\101Dalmatians\Assets\Models\Frame1.obj", GenerateObjFile(Frame1(), "Frame1"));
            var (frame, background) = Frame2();
            File.WriteAllText(@"D:\c\KTANE\101Dalmatians\Assets\Models\Frame2.obj", GenerateObjFile(frame, "Frame2"));
            File.WriteAllText(@"D:\c\KTANE\101Dalmatians\Assets\Models\Frame2Background.obj", GenerateObjFile(background, "Frame2Background"));
            File.WriteAllText(@"D:\c\KTANE\101Dalmatians\Assets\Models\ArrowHighlight.obj", GenerateObjFile(ArrowHighlight(), "ArrowHighlight"));
            File.WriteAllText(@"D:\c\KTANE\101Dalmatians\Assets\Models\AnnulusHighlight.obj", GenerateObjFile(AnnulusHighlight(), "AnnulusHighlight"));
        }

        private static IEnumerable<VertexInfo[]> AnnulusHighlight()
        {
            const int steps = 60;
            const double ir = 1.05;
            const double or = 1.3;
            return Enumerable.Range(0, steps).Select(i => i * 360 / steps).ConsecutivePairs(closed: true).Select(pair => new[] { ptp(ir, pair.Item1, 0), ptp(or, pair.Item1, 0), ptp(or, pair.Item2, 0), ptp(ir, pair.Item2, 0) }.Select(p=>p.WithNormal(0,1,0)).ToArray());
        }

        private static IEnumerable<VertexInfo[]> ArrowHighlight()
        {
            return new[] { p(-5, 0), p(-1, -4), p(-1, -2), p(4, -2), p(4, 2), p(-1, 2), p(-1, 4) }.Reverse().Triangulate().Select(tr => tr.Select(p => pt(p.X, 0, p.Y).WithNormal(0, 1, 0)).Reverse().ToArray()).ToArray();
        }

        public static IEnumerable<VertexInfo[]> Frame1()
        {
            const double r1 = 1;
            const double r2 = 1.01;
            const double r3 = 1.07;
            const double r4 = 1.1;
            const double h = .05;
            const int steps = 60;

            return CreateMesh(true, false,
                Enumerable.Range(0, steps).Select(i => i * 360 / steps).Select(angle => Ut.NewArray(
                    ptp(r1, angle, 0).WithMeshInfo(Normal.Average, Normal.Average, Normal.Mine, Normal.Mine),
                    ptp(r2, angle, h).WithMeshInfo(Normal.Average, Normal.Average, Normal.Average, Normal.Average),
                    ptp(r3, angle, h).WithMeshInfo(Normal.Average, Normal.Average, Normal.Average, Normal.Average),
                    ptp(r4, angle, 0).WithMeshInfo(Normal.Average, Normal.Average, Normal.Mine, Normal.Mine)
                )).ToArray());
        }

        public static (IEnumerable<VertexInfo[]> frame, IEnumerable<VertexInfo[]> background) Frame2()
        {
            const double smoothness = .1;
            const double height = 7.5;
            var xml = XDocument.Parse(File.ReadAllText(@"D:\c\KTANE\101Dalmatians\Data\Design.svg"));
            PointD[] getPoints(string id)
            {
                var points = DecodeSvgPath.Do(xml.Root.ElementI("g").ElementsI("path").Where(e => e.AttributeI("id").Value == id).Single().AttributeI("d").Value, smoothness).Single().Select(p => (p + new PointD(0, -217.625)) / 79.38 * 300 - new PointD(150, 254)).ToArray();
                var index = points.IndexOf(p => p.X > -.1 && p.X < .1 && p.Y < 0);
                return points.Subarray(index).Concat(points.Subarray(0, index)).ToArray();
            }

            // Make them all counter-clockwise
            var inner = getPoints("inner");
            var middle = getPoints("middle");
            var outer = getPoints("outside").Reverse().ToArray();

            IEnumerable<T[]> make<T>(PointD[] inr, PointD[] outr, Func<(PointD p, bool ou)[], T[]> prcPoint)
            {
                var oIx = 0;
                var iIx = 1;
                while (iIx < inr.Length)
                {
                    yield return prcPoint(new[] { (outr[oIx], true), (inr[iIx - 1], false), (inr[iIx], false) }.ReverseInplace());
                    var nm = outr.MinIndex(m => m.Distance(inr[iIx]));
                    while (oIx < nm)
                    {
                        yield return prcPoint(new[] { (outr[oIx], true), (inr[iIx], false), (outr[oIx + 1], true) }.ReverseInplace());
                        oIx++;
                    }
                    iIx++;
                }
                yield return prcPoint(new[] { (outr[oIx], true), (inr[inr.Length - 1], false), (inr[0], false) }.ReverseInplace());
                while (oIx < outr.Length)
                {
                    yield return prcPoint(new[] { (outr[oIx], true), (inr[0], false), (outr[(oIx + 1) % outr.Length], true) }.ReverseInplace());
                    oIx++;
                }
            }

            var tries = new List<(Pt point, Pt normal)[]>();

            tries.AddRange(make(inner, middle, ((PointD p, bool ou)[] arr) =>
            {
                var points = arr.Select(inf => pt(inf.p.X, inf.ou ? height : 0, inf.p.Y)).ToArray();
                var normals = arr.Select(inf => inf.ou ? pt(0, 1, 0) : (points[1] - points[0]) * (points[2] - points[1])).ToArray();
                return Enumerable.Range(0, arr.Length).Select(ix => (points[ix], normals[ix])).ToArray();
            }));
            tries.AddRange(make(middle, outer, ((PointD p, bool ou)[] arr) =>
            {
                var points = arr.Select(inf => pt(inf.p.X, inf.ou ? 0 : height, inf.p.Y)).ToArray();
                var normals = arr.Select(inf => inf.ou ? (points[1] - points[0]) * (points[2] - points[1]) : pt(0, 1, 0)).ToArray();
                return Enumerable.Range(0, arr.Length).Select(ix => (points[ix], normals[ix])).ToArray();
            }));

            var results = new List<VertexInfo[]>();
            foreach (var tri in tries)
            {
                var result = new VertexInfo[tri.Length];
                for (int i = 0; i < tri.Length; i++)
                    result[i] = tri[i].point.WithNormal(tries.SelectMany(x => x).Where(p => p.point.Distance(tri[i].point) < .001).ToArray().Apply(arr => arr.Aggregate(pt(0, 0, 0), (prev, next) => prev + next.normal) / arr.Length));
                results.Add(result);
            }

            var minX = inner.Min(p => p.X);
            var minY = inner.Min(p => p.Y);
            var maxX = inner.Max(p => p.X);
            var maxY = inner.Max(p => p.Y);
            return (results, inner.Triangulate().Select(face => face.Select(p => pt(p.X, 0, p.Y).WithNormal(0, 1, 0).WithTexture((p.X - minX) / (maxX - minX), (p.Y - minY) / (maxY - minY))).ToArray()).ToArray());
        }

        public static IEnumerable<VertexInfo[]> Disc()
        {
            const int steps = 60;
            return Enumerable.Range(0, steps).Select(i => i * 360 / steps).ConsecutivePairs(closed: true)
                .Select(angles => new[] { pt(0, 0, 0), ptp(1, angles.Item2, 0), ptp(1, angles.Item1, 0) }.Select(p => p.WithNormal(0, 1, 0).WithTexture(-(p.X + 1) / 2, (p.Z + 1) / 2)).ToArray())
                .ToArray();
        }
    }
}