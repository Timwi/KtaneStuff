using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KtaneStuff.Modeling;
using System.IO;
    using RT.Util.Geometry;

namespace KtaneStuff
{
    using static Md;

    static class TumbleLock
    {
        public static void DoModels()
        {
            File.WriteAllText(@"D:\c\KTANE\TumbleLock\Assets\Models\Cylinder-1.obj", GenerateObjFile(Cylinder(), "Cylinder"));
        }

        private static IEnumerable<double> range(double from, double to, int steps) => Enumerable.Range(0, steps).Select(i => from + (to - from) * i / steps);

        private static IEnumerable<VertexInfo[]> Cylinder()
        {
            const double inner = 5;
            const double outer = 6;
            const double marble = .5;
            const double trapAngle = 45;
            const double w = (outer - inner) / 2;
            const double m = (outer + inner) / 2;
            double gapAngle = arcsin(marble / m) + arcsin(w / m);
            double t1 = trapAngle - gapAngle;
            double t2 = trapAngle + gapAngle;
            const int steps = 10;
            
            var semiCircleUnderGap = range(0, 180,steps ).Select(i => p(w * cos(i-gapAngle) + m * cos(-gapAngle), w * sin(i-gapAngle) + m * sin(-gapAngle)));
            var innerRim = range(-gapAngle, -360 + gapAngle, steps).Select(i => p(inner * cos(i), inner * sin(i)));
            var semiCircleAboveGap = range(-180, 0, steps).Select(i => p(w * cos(i+gapAngle) + m * cos(gapAngle), w * sin(i+gapAngle) + m * sin(gapAngle)));
            var outerRimToTrap = range(gapAngle, t1, steps).Select(i => p(outer * cos(i), outer * sin(i)));
            var trapSide1 = range(0, 90, steps).Select(i => p(w * cos(i+t1) + m * cos(t1), w * sin(i+t1) + m * sin(t1)));
            var trapBottom = range(270, 90, steps).Select(i => p(marble * cos(i + trapAngle) + m * cos(trapAngle), marble * sin(i + trapAngle) + m * sin(trapAngle)));
            var trapSide2 = range(-90, 0, steps).Select(i => p(w * cos(i + t2) + m * cos(t2), w * sin(i + t2) + m * sin(t2)));
            var outerRimFromTrap = range(t2, 360 - gapAngle, steps).Select(i => p(outer * cos(i), outer * sin(i)));

            var polygon = new[] { semiCircleUnderGap, innerRim, semiCircleAboveGap, outerRimToTrap, trapSide1, trapBottom, trapSide2, outerRimFromTrap }
                .SelectMany(p => p).Reverse();
            foreach (var tri in Triangulate.Delaunay(polygon))
                yield return tri.Vertices.Select(p => pt(p.X, 0, p.Y).WithNormal(0,1,0)).ToArray();
        }
    }
}
