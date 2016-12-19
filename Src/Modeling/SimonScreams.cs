using System;
using System.Collections.Generic;
using System.IO;
using RT.Util;

namespace KtaneStuff.Modeling
{
    using static Md;

    static class SimonScreams
    {
        public static void Do()
        {
            File.WriteAllText(@"D:\c\KTANE\SimonScreams\Assets\Models\Button.obj", GenerateObjFile(Button(), "Button"));
            //File.WriteAllText(@"D:\c\KTANE\SimonScreams\Assets\Models\ButtonHighlight.obj", GenerateObjFile(ButtonHighlight(), "ButtonHighlight"));
        }

        private static IEnumerable<VertexInfo[]> Button()
        {
            var height = .04;
            var fh = height * .4;
            var innerRadius = 0.3;
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
            return CreateMesh(false, false, Ut.NewArray(bézierSteps, bézierSteps, (x, y) => new MeshVertexInfo(patchPiece[x][y], x == bézierSteps - 1 ? Normal.Mine : Normal.Average, x == 0 ? Normal.Mine : Normal.Average, y == bézierSteps - 1 ? Normal.Mine : Normal.Average, y == 0 ? Normal.Mine : Normal.Average)));
        }

        private static IEnumerable<VertexInfo[]> ButtonHighlight()
        {
            var width = .11;
            var innerWidth = .09;
            return CreateMesh(false, true, Ut.NewArray(2, 4, (i, j) => (j * 90 + 45).Apply(angle => (i == 0 ? innerWidth : width).Apply(radius => pt(radius * cos(angle), 0, radius * sin(angle))))));
        }
    }
}
