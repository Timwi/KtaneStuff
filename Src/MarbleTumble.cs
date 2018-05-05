using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using KtaneStuff.Modeling;
using RT.Util;
using RT.Util.Drawing;
using RT.Util.ExtensionMethods;
using RT.Util.Geometry;

namespace KtaneStuff
{
    using static Md;

    static class MarbleTumble
    {
        const double height = 1;
        const double bevelSize = .11;
        const double marble = .38;
        const double bevelRatio = .7;
        const double bevMid = bevelSize * bevelRatio;
        const double bevMid2 = bevelSize * (1 - bevelRatio);

        const int numNotches = 10;
        static int[] notchMins = new[] { 3, 2, 1, 1, 1 };
        static int[] notchMaxs = new[] { 7, 8, 9, 9, 9 };

        public static void DoModels()
        {
            foreach (var file in new DirectoryInfo(@"D:\c\KTANE\MarbleTumble\Assets\Models").EnumerateFileSystemInfos("*.obj"))
                if (file.Name.StartsWith("Cylinder-") && file.Name.EndsWith(".obj"))
                    file.Delete();
            for (int r = 1; r <= 5; r++)
                for (int t = notchMins[r - 1]; t <= notchMaxs[r - 1]; t++)
                    File.WriteAllText($@"D:\c\KTANE\MarbleTumble\Assets\Models\Cylinder-{r}-{t}.obj", GenerateObjFile(Cylinder(r - .5, r + .5, (360 / numNotches) * t), $"Cylinder_{r}_{t}"));
        }

        private static IEnumerable<double> range(double from, double to, int steps) => Enumerable.Range(0, steps).Select(i => from + (to - from) * i / steps);

        /// <summary>
        ///     Generates a polygon that describes the outline of a single tumble-lock cylinder.</summary>
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
        /// <returns>
        ///     A sequence of points that describe the outline, decorated with texture coordinates.</returns>
        private static IEnumerable<(PointD p, PointD texture)> CylinderPolygon(double inner, double outer, double marble, double trapDepth, int trap, double textureDist)
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

            return new[] { semiCircleUnderGap, innerRim, semiCircleAboveGap, outerRimToTrap, trapFillet1, trapBottom, trapFillet2, outerRimFromTrap }.SelectMany(p => p);
        }

        private static IEnumerable<VertexInfo[]> Cylinder(double inner, double outer, int trap)
        {
            var topFaceOutline = CylinderPolygon(inner + bevelSize, outer - bevelSize, marble + bevelSize, marble - bevelSize, trap, 0);

            var bottom = CylinderPolygon(inner, outer, marble, marble, trap, 1).Select(p => pt(p.p.X, 0, p.p.Y).WithMeshInfo(Normal.Mine, Normal.Mine, Normal.Average, Normal.Average).WithTexture(p.texture)).ToArray();
            var bevelBottom = CylinderPolygon(inner, outer, marble, marble, trap, .1).Select(p => pt(p.p.X, height, p.p.Y).WithMeshInfo(Normal.Mine, Normal.Theirs, Normal.Average, Normal.Average).WithTexture(p.texture)).ToArray();
            var bevelMiddle = CylinderPolygon(inner + bevMid2, outer - bevMid2, marble + bevMid2, marble - bevMid2, trap, .05).Select(p => pt(p.p.X, height + bevMid, p.p.Y).WithMeshInfo(Normal.Average, Normal.Average, Normal.Average, Normal.Average).WithTexture(p.texture)).ToArray();
            var bevelTop = topFaceOutline.Select(p => pt(p.p.X, height + bevelSize, p.p.Y).WithMeshInfo(0, 1, 0).WithTexture(p.texture)).ToArray();

            var mesh = CreateMesh(false, true, new[] { bottom, bevelBottom, bevelMiddle, bevelTop });
            try
            {
                var topFace = Md.Triangulate(topFaceOutline.Reverse().Select(p => p.p)).Select(face => face.Select(p => pt(p.X, height + bevelSize, p.Y).WithNormal(0, 1, 0).WithTexture(topFaceOutline.MinElement(p2 => p2.p.Distance(p)).texture)).ToArray()).ToArray();
                var allFaces = mesh.Concat(topFace);

                var allTextures = allFaces.SelectMany(f => f).Select(v => v.Texture.Value);
                var txMinX = -outer - 1;//allTextures.Min(p => p.X);
                var txMaxX = outer + 1;//allTextures.Max(p => p.X);
                var txMinY = -outer - 1;//allTextures.Min(p => p.Y);
                var txMaxY = outer + 1;//allTextures.Max(p => p.Y);
                PointD translateTexture(PointD orig) => p((orig.X - txMinX) / (txMaxX - txMinX), (orig.Y - txMinY) / (txMaxY - txMinY));
                return allFaces.Select(f => f.Select(v => v.WithTexture(translateTexture(v.Texture.Value))).ToArray());
            }
            catch (Exception e)
            {
                makeTempPng(topFaceOutline.Select(t => t.p));
                throw;
            }
        }

        public static void DoDebug()
        {
            makeTempPng(CylinderPolygon(.5, 1.5, marble, marble, 120, 0).Select(p => p.p));
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

                    //double inner = .5;
                    //double outer = 1.5;
                    //double trapFillet = marble;
                    //int trap = 120;
                    //double textureDist = 0;
                    //double w = (outer - inner) / 2;     // half the width of the cylinder’s bulk (same as radius of the semicircles on each side of the gap)
                    //double m = (outer + inner) / 2;     // distance from center to the middle of the bulk
                    //double gapAngle = arcsin(marble / m) + arcsin(w / m);               // angle between the middle of the gap and the centerpoint of the semicircles on either side of it
                    //double trapAngle = arcsin(marble / (outer - trapFillet)) + arcsin(trapFillet / (outer - trapFillet));   // angle between the middle of the trap and the centerpoint of the fillet
                    //double t1 = trap - trapAngle;       // angle of the centerpoint of the first trap fillet
                    //double t2 = trap + trapAngle;       // angle of the centerpoint of the other trap fillet

                    //(PointD p, PointD texture) mk(double rMin, double θMin, double rMaj, double θMaj) => (
                    //    p(rMin * cos(θMin) + rMaj * cos(θMaj), rMin * sin(θMin) + rMaj * sin(θMaj)),
                    //    p((rMin + textureDist) * cos(θMin) + rMaj * cos(θMaj), (rMin + textureDist) * sin(θMin) + rMaj * sin(θMaj)));

                    //var trapFillet1 = range(0, 90, 6 * 4).Select(i => mk(trapFillet, i + t1, outer - trapFillet, t1));
                    //var trapBottom = range(270, 90, 12 * 4).Select(i => mk(marble, i + trap, outer - trapFillet, trap));
                    //var trapFillet2 = range(-90, 0, 6 * 4).Select(i => mk(trapFillet, i + t2, outer - trapFillet, t2));

                    //var mp = mk(0, 0, outer - trapFillet, t1);
                    //drawPoint(mp.p, Brushes.Red);
                    //g.DrawLine(new Pen(Brushes.Red, .05f), new PointF(0, 0), mp.p.ToPointF());
                    //g.DrawLine(new Pen(Brushes.Red, .05f), mp.p.ToPointF(), mk(trapFillet, 0 + t1, outer - trapFillet, t1).p.ToPointF());
                    //g.DrawLine(new Pen(Brushes.Red, .05f), mp.p.ToPointF(), mk(trapFillet, 90 + t1, outer - trapFillet, t1).p.ToPointF());

                    //foreach (var tup in polygon.ConsecutivePairs(true))
                    //    g.DrawLine(new Pen(Brushes.Black, .2f), tup.Item1.ToPointF(), tup.Item2.ToPointF());

                    //foreach (var tri in triangulated)
                    //    g.FillPolygon(Brushes.CornflowerBlue, tri.Select(v => v.ToPointF()).ToArray());
                }
            }).Save(@"D:\temp\temp.png");
        }

        public static void GenerateSvg()
        {
            var path = @"D:\c\KTANE\Public\HTML\img\Component\Marble Tumble.svg";
            Utils.ReplaceInFile(path, @"<!--%%-->", @"<!--%%%-->", Enumerable.Range(0, 5).Select(ix => $@"<path d='M {
                (Rnd.Next(0, 10) * Math.PI / 5).Apply(angle =>
                    CylinderPolygon(ix + .5, ix + 1.5, marble, marble, (360 / numNotches) * Rnd.Next(notchMins[ix], notchMaxs[ix] + 1), 0)
                        .Select(tup => tup.p.Rotated(angle))
                        .Select(p => p * 300 / 11 + new PointD(166, 348 - 166))
                        .Select(tup => $"{tup.X},{tup.Y}")
                        .JoinString(" "))
            } z' stroke-width='1' stroke='#000' fill='{Rnd.Next(64, 255).Apply(shade => $"#{shade.ToString("X2")}{shade.ToString("X2")}{shade.ToString("X2")}")}' />").JoinString());
        }
    }
}
