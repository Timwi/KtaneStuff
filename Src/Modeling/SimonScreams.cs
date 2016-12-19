using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using RT.Util;

namespace KtaneStuff.Modeling
{
    using RT.Util.Geometry;
    using static Md;

    static class SimonScreams
    {
        public static void Do()
        {
            File.WriteAllText(@"D:\c\KTANE\SimonScreams\Assets\Models\Button.obj", GenerateObjFile(Button(), "Button"));
            File.WriteAllText(@"D:\c\KTANE\SimonScreams\Assets\Models\ButtonHighlight.obj", GenerateObjFile(ButtonHighlight(), "ButtonHighlight"));
        }

        private static IEnumerable<VertexInfo[]> Button()
        {
            var height = .2;
            var fh = height * .4;
            var innerRadius = 0.4;
            var outerRadius = 1.0;
            var fr = innerRadius * .1;
            var angle = 30.0;

            var bézierSteps = 20;
            var d = Math.Sqrt(innerRadius * innerRadius + outerRadius * outerRadius - 2 * innerRadius * outerRadius * cos(angle));
            var frix = outerRadius - (outerRadius - innerRadius * cos(angle)) / d * (d - fr);
            var friy = innerRadius * sin(angle) / d * (d - fr);
            var frox = outerRadius - (outerRadius - innerRadius * cos(angle)) / d * fr;
            var froy = innerRadius * sin(angle) / d * fr;

            var patchPiece = BézierPatch(
                pt(0, 0, 0), pt(fr * cos(-angle), fh, fr * sin(-angle)), pt((innerRadius - fr) * cos(-angle), fh, (innerRadius - fr) * sin(-angle)), pt(innerRadius * cos(-angle), 0, innerRadius * sin(-angle)),
                pt(fr * cos(angle), fh, fr * sin(angle)), pt(innerRadius / 2, height, 0), pt(innerRadius * cos(angle / 2), height, innerRadius * sin(angle / 2)), pt(frix, fh, -friy),
                pt((innerRadius - fr) * cos(angle), fh, (innerRadius - fr) * sin(angle)), pt(innerRadius * cos(angle / 2), height, innerRadius * sin(angle / 2)), pt((innerRadius + outerRadius) / 2, height, 0), pt(frox, fh, -froy),
                pt(innerRadius * cos(angle), 0, innerRadius * sin(angle)), pt(frix, fh, friy), pt(frox, fh, froy), pt(outerRadius, 0, 0),
                bézierSteps);
            return CreateMesh(false, false, Ut.NewArray(bézierSteps, bézierSteps, (x, y) => new MeshVertexInfo(patchPiece[x][y].Add(x: .03), x == bézierSteps - 1 ? Normal.Mine : Normal.Average, x == 0 ? Normal.Mine : Normal.Average, y == bézierSteps - 1 ? Normal.Mine : Normal.Average, y == 0 ? Normal.Mine : Normal.Average)));
        }

        private static IEnumerable<VertexInfo[]> ButtonHighlight()
        {
            var preRadius = .03;
            var innerRadius = .44;
            var outerRadius = 1.15;
            var angle = 32.0;

            var holeInnerRadius = .4;
            var holeOuterRadius = 1.0;
            var holeAngle = 30.0;

            return
                Triangulate(Ut.NewArray(
                    new[] { p(-preRadius, 0), p(innerRadius * cos(-angle), innerRadius * sin(-angle)), p(outerRadius, 0), p(innerRadius * cos(angle), innerRadius * sin(angle)) },
                    new[] { p(0, 0), p(holeInnerRadius * cos(holeAngle), holeInnerRadius * sin(holeAngle)), p(holeOuterRadius, 0), p(holeInnerRadius * cos(-holeAngle), holeInnerRadius * sin(-holeAngle)) }.Select(p => p + new PointD(.03, 0))
                ))
                    .Select(poly => poly.Select(p => pt(p.X, 0, p.Y).WithNormal(0, 1, 0)).ToArray());
        }
    }
}
