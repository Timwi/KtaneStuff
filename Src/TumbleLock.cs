using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KtaneStuff.Modeling;
using System.IO;
using RT.Util.Geometry;
using System.Drawing;
using RT.Util.Drawing;
using RT.Util.ExtensionMethods;

namespace KtaneStuff
{
    using RT.Util.Serialization;
    using static Md;

    static class TumbleLock
    {
        public static void DoModels()
        {
            File.WriteAllText(@"D:\c\KTANE\TumbleLock\Assets\Models\Cylinder-1.obj", GenerateObjFile(Cylinder(5, 6, 45), "Cylinder"));
        }

        private static IEnumerable<double> range(double from, double to, int steps) => Enumerable.Range(0, steps).Select(i => from + (to - from) * i / steps);

        private static IEnumerable<PointD> CylinderPolygon(double inner, double outer, double marble, double trapFillet, int trap)
        {
            //const double inner = 5;
            //const double outer = 6;
            //const double marble = .4;
            //const double trap = 45;     // where the trap is on the ring
            double w = (outer - inner) / 2;
            double m = (outer + inner) / 2;
            double gapAngle = arcsin(marble / m) + arcsin(w / m);
            double trapAngle = arcsin(marble / m) + arcsin(trapFillet / m);
            double t1 = trap - trapAngle;
            double t2 = trap + trapAngle;

            var semiCircleUnderGap = range(0, 180, 12).Select(i => p(w * cos(i - gapAngle) + m * cos(-gapAngle), w * sin(i - gapAngle) + m * sin(-gapAngle)));
            var innerRim = range(-gapAngle, -360 + gapAngle, 60).Select(i => p(inner * cos(i), inner * sin(i)));
            var semiCircleAboveGap = range(-180, 0, 12).Select(i => p(w * cos(i + gapAngle) + m * cos(gapAngle), w * sin(i + gapAngle) + m * sin(gapAngle)));
            var outerRimToTrap = range(gapAngle, t1, (trap + 5) / 6).Select(i => p(outer * cos(i), outer * sin(i)));
            var trapFillet1 = range(0, 90, 6).Select(i => p(trapFillet * cos(i + t1) + (outer - trapFillet) * cos(t1), trapFillet * sin(i + t1) + (outer - trapFillet) * sin(t1)));
            var trapBottom = range(270, 90, 12).Select(i => p(marble * cos(i + trap) + (outer - trapFillet) * cos(trap), marble * sin(i + trap) + (outer - trapFillet) * sin(trap)));
            var trapFillet2 = range(-90, 0, 6).Select(i => p(trapFillet * cos(i + t2) + (outer - trapFillet) * cos(t2), trapFillet * sin(i + t2) + (outer - trapFillet) * sin(t2)));
            var outerRimFromTrap = range(t2, 360 - gapAngle, (360 - trap) / 6).Select(i => p(outer * cos(i), outer * sin(i)));

            return new[] { semiCircleUnderGap, innerRim, semiCircleAboveGap, outerRimToTrap, trapFillet1, trapBottom, trapFillet2, outerRimFromTrap }.SelectMany(p => p);
        }

        private static IEnumerable<VertexInfo[]> Cylinder(double inner, double outer, int trap)
        {
            const double height = 1;
            const double bevelSize = .075;
            const double marble = .375;
            const double bevelRatio = .7;

            var bottom = CylinderPolygon(inner, outer, marble, marble, trap).Select(p => pt(p.X, 0, p.Y).WithMeshInfo(Normal.Mine, Normal.Mine, Normal.Average, Normal.Average)).ToArray();
            var bevelBottom = CylinderPolygon(inner, outer, marble, marble, trap).Select(p => pt(p.X, height, p.Y).WithMeshInfo(Normal.Mine, Normal.Theirs, Normal.Average, Normal.Average)).ToArray();
            var bevelMiddle = CylinderPolygon(inner + bevelSize * (1 - bevelRatio), outer - bevelSize * (1 - bevelRatio), marble + bevelSize * (1 - bevelRatio), marble - bevelSize * (1 - bevelRatio), trap).Select(p => pt(p.X, height + bevelSize * bevelRatio, p.Y).WithMeshInfo(Normal.Average, Normal.Average, Normal.Average, Normal.Average)).ToArray();
            var bevelTop = CylinderPolygon(inner + bevelSize, outer - bevelSize, marble + bevelSize, marble - bevelSize, trap).Select(p => pt(p.X, height + bevelSize, p.Y).WithMeshInfo(0, 1, 0)).ToArray();

            var mesh = CreateMesh(false, true, new[] { bottom, bevelBottom, bevelMiddle, bevelTop });
            var topFace = Md.Triangulate(CylinderPolygon(inner + bevelSize, outer - bevelSize, marble + bevelSize, marble - bevelSize, trap).Reverse()).Select(face => face.Select(p => pt(p.X, height + bevelSize, p.Y).WithNormal(0, 1, 0)).ToArray()).ToArray();
            return mesh.Concat(topFace);
        }

        public static void Do()
        {
            const double inner = 5;
            const double outer = 6;
            const double bevelSize = .075;
            const double marble = .375;
            const int trap = 45;

            var polygon = CylinderPolygon(inner + bevelSize, outer - bevelSize, marble + bevelSize, marble - bevelSize, trap);
            var triangulated = polygon.Reverse().Triangulate();
            //foreach (var tri in triangulated)
            //    yield return tri.Select(p => pt(p.X, 0, p.Y).WithNormal(0, 1, 0)).ToArray();

            GraphicsUtil.DrawBitmap(1000, 1000, g =>
            {
                g.Clear(Color.Transparent);
                using (var tr = new GraphicsTransformer(g).Scale(75, 75).Translate(500, 500))
                {
                    foreach (var point in polygon)
                    {
                        //g.DrawLine(Pens.Black, (point - new PointD(.1, .1)).ToPointF(), (point + new PointD(.1, .1)).ToPointF());
                        //g.DrawLine(Pens.Black, (point + new PointD(.1, -.1)).ToPointF(), (point + new PointD(-.1, .1)).ToPointF());
                    }
                    foreach (var tup in polygon.ConsecutivePairs(true))
                        g.DrawLine(new Pen(Brushes.Black, .2f), tup.Item1.ToPointF(), tup.Item2.ToPointF());

                    foreach (var tri in triangulated)
                        g.FillPolygon(Brushes.CornflowerBlue, tri.Select(v => v.ToPointF()).ToArray());
                }
            }).Save(@"D:\temp\temp.png");
        }
    }
}
