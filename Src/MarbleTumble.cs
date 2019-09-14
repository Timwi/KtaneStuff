using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using KtaneStuff.Modeling;
using RT.Dijkstra;
using RT.Util;
using RT.Util.Drawing;
using RT.Util.ExtensionMethods;
using RT.Util.Geometry;

namespace KtaneStuff
{
    using static Md;

    static class MarbleTumble
    {
        const double _height = 1;
        const double _bevelSize = .11;
        const double _marbleSize = .38;
        const double _bevelRatio = .7;
        const double _bevMid = _bevelSize * _bevelRatio;
        const double _bevMid2 = _bevelSize * (1 - _bevelRatio);
        const double _notchDepth = .2;

        const int _numNotches = 10;
        static readonly int[] _gapTrapMins = new[] { 3, 2, 1, 1, 1 };
        static readonly int[] _gapTrapMaxs = new[] { 7, 8, 9, 9, 9 };

        public static void DoModels()
        {
            foreach (var file in new DirectoryInfo(@"D:\c\KTANE\MarbleTumble\Assets\Models").EnumerateFileSystemInfos("*.obj"))
                if (file.Name.StartsWith("Cylinder-") && file.Name.EndsWith(".obj"))
                    file.Delete();
            for (int r = 1; r <= 5; r++)
                for (int t = _gapTrapMins[r - 1]; t <= _gapTrapMaxs[r - 1]; t++)
                {
                    var (mainBody, notches) = Cylinder(r - .5, r + .5, (360 / _numNotches) * t, Enumerable.Range(1, 9).Where(n => n != t).ToArray(), n => r == 1 ? (Math.Abs(t - n) <= 1 ? .15 : n <= 1 || 10 - n <= 1 ? .105 : .15) : .15);
                    File.WriteAllText($@"D:\c\KTANE\MarbleTumble\Assets\Models\Cylinder-{r}-{t}.obj", GenerateObjFile(mainBody.Concat(notches), $"Cylinder_{r}_{t}"));
                    Console.WriteLine($"Generated: {r}-{t}");
                }
        }

        private static IEnumerable<double> range(double from, double to, int steps) => Enumerable.Range(0, steps).Select(i => from + (to - from) * i / steps);

        /// <summary>
        ///     Generates a polygon that describes the outline of a single cylinder.</summary>
        /// <param name="inner">
        ///     The radius of the inside (the “hole in the ring”).</param>
        /// <param name="outer">
        ///     The radius of the outside (the full size of the cylinder).</param>
        /// <param name="marble">
        ///     The radius of the marble holes (both gap and trap).</param>
        /// <param name="trapDepth">
        ///     How far the trap is inset into the cylinder. Must be greater than <paramref name="marble"/>.</param>
        /// <param name="trap">
        ///     The angle where the trap is on the cylinder. (The gap is always at 0°).</param>
        /// <param name="textureDist">
        ///     The distance from the original curve at which to calculate the UV coordinates.</param>
        /// <param name="notches">
        ///     List of positions (0–9) of notches on the surface of the cylinder.</param>
        /// <param name="getNotchSize">
        ///     Size of the notches (max .1 for inner cylinder)</param>
        /// <returns>
        ///     A sequence of points that describe the outline, decorated with texture coordinates.</returns>
        private static IEnumerable<(IEnumerable<(PointD p, PointD texture)> poly, PointD mid)> CylinderPolygonWithNotches(double inner, double outer, double marble, double trapDepth, int trap, double textureDist, int[] notches = null, Func<int, double> getNotchSize = null)
        {
            double w = (outer - inner) / 2;     // half the width of the cylinder’s bulk (same as radius of the semicircles on each side of the gap)
            double m = (outer + inner) / 2;     // distance from center to the middle of the bulk
            double gapAngle = arcsin(marble / m) + arcsin(w / m);               // angle between the middle of the gap and the centerpoint of the semicircles on either side of it
            double s = -(marble * marble / 2 - outer * trapDepth + trapDepth * trapDepth / 2) / (marble + outer);   // radius of the trap fillet
            double th = arcsin((marble + s) / (outer - s));  // Angle between the fillet’s top, the cylinder center, and the trap’s bottom

            (PointD p, PointD texture) mk(double rMin, double θMin, double rMaj, double θMaj) => (
                p(rMin * cos(θMin) + rMaj * cos(θMaj), rMin * sin(θMin) + rMaj * sin(θMaj)),
                p((rMin + textureDist) * cos(θMin) + rMaj * cos(θMaj), (rMin + textureDist) * sin(θMin) + rMaj * sin(θMaj)));

            var semiCircleUnderGap = range(0, 180, 12).Select(i => mk(w, i - gapAngle, m, -gapAngle));
            var innerRim = range(-gapAngle, -360 + gapAngle, 60).Select(i => mk(-w, i, m, i));
            var semiCircleAboveGap = range(-180, 0, 12).Select(i => mk(w, i + gapAngle, m, gapAngle));
            var outerRimToTrap = range(gapAngle, trap - th, (trap + 5) / 6).Select(i => mk(w, i, m, i));
            var trapFillet1 = range(-th, 90, 6).Select(i => mk(s, i + trap, outer - s, -th + trap));
            var trapBottom = range(270, 90, 12).Select(i => mk(marble, i + trap, outer - trapDepth, trap));
            var trapFillet2 = range(-90, th, 6).Select(i => mk(s, i + trap, outer - s, th + trap));
            var outerRimFromTrap = range(trap + th, 360 - gapAngle, (365 - trap) / 6).Select(i => mk(w, i, m, i));

            yield return (new[] { semiCircleUnderGap, innerRim, semiCircleAboveGap, outerRimToTrap, trapFillet1, trapBottom, trapFillet2, outerRimFromTrap }.SelectMany(p => p), default(PointD));

            if (notches != null)
            {
                foreach (var n in notches)
                {
                    var ns = getNotchSize == null ? .1 : getNotchSize(n);
                    yield return (new[] { p(-ns, m), p(0, m + 2 * ns), p(ns, m), p(0, m - 2 * ns) }.Select(p => p.Rotated((-n * .2 + .5) * Math.PI)).Select(p => (p, p)).ToArray(), p(0, m).Rotated((-n * .2 + .5) * Math.PI));
                }
            }
        }

        private static IEnumerable<(PointD p, PointD texture)> CylinderPolygon(double inner, double outer, double marble, double trapDepth, int trap, double textureDist)
        {
            return CylinderPolygonWithNotches(inner, outer, marble, trapDepth, trap, textureDist).First().poly;
        }

        private static (IEnumerable<VertexInfo[]> mainBody, IEnumerable<VertexInfo[]> notches) Cylinder(double inner, double outer, int trap, int[] notches, Func<int, double> getNotchSize)
        {
            var topFaceFull = CylinderPolygonWithNotches(inner + _bevelSize, outer - _bevelSize, _marbleSize + _bevelSize, _marbleSize - _bevelSize, trap, 0, notches, getNotchSize).ToArray();
            var topFaceOutline = topFaceFull.First();

            var bottom = CylinderPolygon(inner, outer, _marbleSize, _marbleSize, trap, 1).Select(p => pt(p.p.X, 0, p.p.Y).WithMeshInfo(Normal.Mine, Normal.Mine, Normal.Average, Normal.Average).WithTexture(p.texture)).ToArray();
            var bevelBottom = CylinderPolygon(inner, outer, _marbleSize, _marbleSize, trap, .1).Select(p => pt(p.p.X, _height, p.p.Y).WithMeshInfo(Normal.Mine, Normal.Theirs, Normal.Average, Normal.Average).WithTexture(p.texture)).ToArray();
            var bevelMiddle = CylinderPolygon(inner + _bevMid2, outer - _bevMid2, _marbleSize + _bevMid2, _marbleSize - _bevMid2, trap, .05).Select(p => pt(p.p.X, _height + _bevMid, p.p.Y).WithMeshInfo(Normal.Average, Normal.Average, Normal.Average, Normal.Average).WithTexture(p.texture)).ToArray();
            var bevelTop = topFaceOutline.poly.Select(p => pt(p.p.X, _height + _bevelSize, p.p.Y).WithMeshInfo(0, 1, 0).WithTexture(p.texture)).ToArray();

            PointD translateTexture(PointD orig) => p((orig.X + outer + 1) / (2 * outer + 2), (orig.Y + outer + 1) / (2 * outer + 2));
            VertexInfo[] translateTextures(VertexInfo[] orig) => orig.Select(v => v.WithTexture(translateTexture(v.Texture.Value))).ToArray();

            IEnumerable<VertexInfo[]> generateMainBody()
            {
                // Main body of the cylinder without the top face
                foreach (var f in CreateMesh(false, true, new[] { bottom, bevelBottom, bevelMiddle, bevelTop }))
                    yield return translateTextures(f);

                // Top face without the notches
                var topFacePoly = topFaceFull.Select(pl => pl.poly.Select(p => p.p).ToArray()).ToArray();
                VertexInfo[][] arr;
                try
                {
                    arr = topFacePoly
                        .Triangulate()
                        .Select(face => face.Select(p => pt(p.X, _height + _bevelSize, p.Y).WithNormal(0, 1, 0).WithTexture(topFaceFull.SelectMany(pl => pl.poly).MinElement(p2 => p2.p.Distance(p)).texture)).Reverse().ToArray())
                        .ToArray();
                }
                catch (Exception)
                {
                    makeTempPng(topFacePoly.SelectMany(x => x));
                    throw;
                }

                foreach (var f in arr)
                    yield return translateTextures(f);
            }

            IEnumerable<VertexInfo[]> generateNotches()
            {
                foreach (var (poly, mid) in topFaceFull.Skip(1))
                {
                    foreach (var f in CreateMesh(true, false, poly.Select(ptTx => new[] {
                        pt(ptTx.p.X, _height + _bevelSize, ptTx.p.Y).WithMeshInfo(Normal.Mine, Normal.Mine, Normal.Mine, Normal.Mine).WithTexture(ptTx.texture),
                        pt(mid.X, _height + _bevelSize - _notchDepth, mid.Y).WithMeshInfo(0, 1, 0).WithTexture(mid)
                    }).ToArray()))
                        yield return translateTextures(f);
                }
            }

            return (generateMainBody(), generateNotches());
        }

        public static void DoDebug()
        {
            makeTempPng(CylinderPolygon(.5, 1.5, _marbleSize, _marbleSize, 120, 0).Select(p => p.p));
        }

        private static void makeTempPng(IEnumerable<PointD> polygon)
        {
            const double po = .02;
            const float pw = .02f;

            GraphicsUtil.DrawBitmap(1000, 1000, g =>
            {
                g.Clear(Color.Transparent);
                using (var tr = new GraphicsTransformer(g).Scale(75, 75).Translate(500, 500))
                {
                    void drawPoint(PointD p, Brush brush)
                    {
                        g.DrawLine(new Pen(brush, pw), (p + new PointD(-po, -po)).ToPointF(), (p + new PointD(po, po)).ToPointF());
                        g.DrawLine(new Pen(brush, pw), (p + new PointD(po, -po)).ToPointF(), (p + new PointD(-po, po)).ToPointF());
                    }
                    foreach (var point in polygon)
                        drawPoint(point, Brushes.Black);
                }
            }).Save(@"D:\temp\temp.png");
        }

        public static void GenerateComponentSvg()
        {
            var path = @"D:\c\KTANE\Public\HTML\img\Component\Marble Tumble.svg";
            Utils.ReplaceInFile(path, @"<!--%%-->", @"<!--%%%-->", Enumerable.Range(0, 5).Select(ix => $@"<path d='M {
                (Rnd.Next(0, 10) * Math.PI / 5).Apply(angle =>
                    CylinderPolygon(ix + .5, ix + 1.5, _marbleSize, _marbleSize, (360 / _numNotches) * Rnd.Next(_gapTrapMins[ix], _gapTrapMaxs[ix] + 1), 0)
                        .Select(tup => tup.p.Rotated(angle))
                        .Select(p => p * 300 / 11 + new PointD(166, 348 - 166))
                        .Select(tup => $"{tup.X},{tup.Y}")
                        .JoinString(" "))
            } z' stroke-width='1' stroke='#000' fill='{Rnd.Next(64, 255).Apply(shade => $"#{shade.ToString("X2")}{shade.ToString("X2")}{shade.ToString("X2")}")}' />").JoinString());
        }

        public static void GenerateLogfileAnalyzerSvgs()
        {
            for (int cylIx = 0; cylIx < 5; cylIx++)
                for (int trapIx = _gapTrapMins[cylIx]; trapIx <= _gapTrapMaxs[cylIx]; trapIx++)
                {
                    var notches = Enumerable.Range(1, 9).Where(n => n != trapIx).ToArray();
                    foreach (var (name, colorLight, colorDark) in Ut.NewArray(
                        ("red", "ff8181", "eb4747"),
                        ("yellow", "eff09a", "bdc030"),
                        ("green", "81d682", "3d8f3f"),
                        ("blue", "8eb5ff", "185bd8"),
                        ("silver", "ccc", "787878")))
                    {
                        var cylinderPolygon = CylinderPolygon(cylIx + .5, cylIx + 1.5, _marbleSize, _marbleSize, (360 / _numNotches) * trapIx, 0).Select(tup => $"{tup.p.X},{tup.p.Y}").JoinString(" ");
                        var notchesSvg = notches.Select(n => $"<path transform='rotate({n * 360 / _numNotches - 90})' d='M0,{cylIx + .8} L.1,{cylIx + 1} 0,{cylIx + 1.2} -.1,{cylIx + 1}z' />").JoinString();
                        File.WriteAllText(
                            $@"D:\c\KTANE\Public\More\img\Marble Tumble\Cylinder-{cylIx}-{trapIx}-{name}.svg",
                            $@"<svg xmlns='http://www.w3.org/2000/svg' viewBox='-6 -6 12 12'><path d='M{cylinderPolygon}z' fill='#{colorLight}' stroke-width='.06' stroke='#000' /><g fill='rgba(0, 0, 0, .2)'>{notchesSvg}</g></svg>");
                    }
                }
        }

        sealed class DijNode : Node<int, string>
        {
            public int[] Rotations { get; private set; }
            public int[] Traps { get; private set; }
            public int[] ColorIxs { get; private set; }
            public int Marble { get; private set; }
            public int LastSec { get; private set; }

            public DijNode(int[] rotations, int[] traps, int[] colorIxs, int marble, int lastSec)
            {
                Rotations = rotations;
                Traps = traps;
                ColorIxs = colorIxs;
                Marble = marble;
                LastSec = lastSec;
            }

            private static readonly int[][] _rotationData = @"-1,1,-2,0,2;-2,1,2,-1,0;1,0,2,-2,-1;0,-1,-2,1,2;2,0,1,-1,-2;1,-2,-1,2,0;-2,2,0,1,-1;0,-1,1,2,-2;-1,2,0,-2,1;2,-2,-1,0,1".Split(';').Select(str => str.Split(',').Select(s => int.Parse(s)).ToArray()).ToArray();

            public override bool IsFinal => Marble == 0;

            public static int m(int v) => (v % 10 + 10) % 10;

            public override IEnumerable<Edge<int, string>> Edges
            {
                get
                {
                    if (Marble == 0 || m(Traps[Marble - 1] + Rotations[Marble - 1]) == (Marble == 5 ? 0 : m(Rotations[Marble])))
                        yield break;

                    for (int sec = 0; sec < 10; sec++)
                    {
                        if (sec != LastSec)
                        {
                            for (int times = 1; times <= 2; times++)
                            {
                                var newRotations = Ut.NewArray(5, i => Rotations[i] + times * _rotationData[sec][ColorIxs[i]]);
                                var newMarble = Marble;
                                while (newMarble > 0 && (newMarble == 5 ? 0 : m(newRotations[newMarble])) == m(newRotations[newMarble - 1]))
                                    newMarble--;
                                var weight = 60 * ((LastSec == -1) ? 1 : (sec < LastSec) ? (LastSec - sec) : (LastSec + 10 - sec));
                                if (Marble != newMarble && Marble - newMarble <= weight)
                                    weight /= (Marble - newMarble);
                                yield return new Edge<int, string>(weight, $@"{times}× at {sec}; rotations: {newRotations.JoinString(", ")}, marble: {newMarble}, weight: {weight}", new DijNode(newRotations, Traps, ColorIxs, newMarble, sec));
                            }
                        }
                    }
                }
            }

            public override bool Equals(Node<int, string> other)
            {
                if (!(other is DijNode oth))
                    return false;
                for (int i = 0; i < 5; i++)
                    if (m(Rotations[i]) != m(oth.Rotations[i]))
                        return false;
                return Marble == oth.Marble && LastSec == oth.LastSec;
            }

            public override int GetHashCode()
            {
                const int b = 378551;
                int a = 63689;
                int hash = 12;

                unchecked
                {
                    for (int i = 0; i < 5; i++)
                    {
                        hash = hash * a + m(Rotations[i]);
                        a = a * b;
                    }
                    return hash + Marble + 17 * LastSec;
                }
            }

            public override string ToString()
            {
                return $"Rotations: {Rotations.JoinString(", ")}; Marble: {Marble}";
            }
        }

        public static void Solver()
        {
            var colors = getStrArr("Colors? ");
            if (colors == null)
                return;
            var colorNames = "red,yellow,green,blue,silver".Split(',');
            if (colors.Any(c => !colorNames.Any(cn => cn.EqualsNoCase(c))))
                return;
            var colorIxs = colors.Select(c => colorNames.IndexOf(c, StringComparer.InvariantCultureIgnoreCase)).Reverse().ToArray();
            var gaps = getIntArr("Gaps? ");
            if (gaps == null)
                return;
            var traps = getIntArr("Traps? ");
            if (traps == null)
                return;

            try
            {
                var result = DijkstrasAlgorithm.Run(new DijNode(gaps.Reverse().ToArray(), gaps.Zip(traps, (g, t) => t - g).Reverse().ToArray(), colorIxs, 5, -1), 0, (a, b) => a + b, out var totalWeight);
                Console.WriteLine(result.JoinString("\n"));
                Console.WriteLine($"Total weight: {totalWeight}");
            }
            catch (DijkstraNoSolutionException<int, string> e)
            {
                Console.WriteLine("No solution found.");
                var all = new HashSet<string>();
                for (int a = 0; a < 10; a++)
                    for (int b = 0; b < 10; b++)
                        for (int c = 0; c < 10; c++)
                            for (int d = 0; d < 10; d++)
                                for (int f = 0; f < 10; f++)
                                    all.Add($"{a},{b},{c},{d},{f}");
                foreach (DijNode elem in e.VisitedNodes)
                {
                    if (!all.Remove(elem.Rotations.Select(r => (r % 10 + 10) % 10).JoinString(",")))
                        System.Diagnostics.Debugger.Break();
                    if (elem.Rotations.All(r => (r % 10 + 10) % 10 == 0) || elem.Marble != 5)
                        Console.WriteLine(elem);
                }
                Console.WriteLine($"Remaining:\n{all.JoinString("\n")}");
                System.Diagnostics.Debugger.Break();
            }
        }

        private static string[] getStrArr(string prompt)
        {
            Console.Write(prompt);
            var input = Console.ReadLine();
            if (input == null)
                return null;
            var spl = input.Split(',');
            if (spl.Length != 5)
                return null;
            return spl;
        }

        private static int[] getIntArr(string prompt)
        {
            var spl = getStrArr(prompt);
            if (spl == null || spl.Any(s => !int.TryParse(s, out _)))
                return null;
            return spl.Select(s => int.Parse(s)).ToArray();
        }
    }
}
