using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;

namespace KtaneStuff.Modeling
{
    using static Md;

    static class OnlyConnect
    {
        public static void Do()
        {
            File.WriteAllText(@"D:\c\KTANE\OnlyConnect\Assets\Misc\Button.obj", GenerateObjFile(Button(), "Button"));
        }

        private static IEnumerable<VertexInfo[]> Button()
        {
            const double width = 1;
            const double height = .6;
            const double bf = .01;
            const double depth = .05;
            const double middleF = .25;
            const int bézierSteps = 24;

            var patch = BézierPatch(
                pt(-width / 2, 0, -height / 2), pt(-width / 2 + bf, 0, -height / 2 - bf), pt(width / 2 - bf, 0, -height / 2 - bf), pt(width / 2, 0, -height / 2),
                pt(-width / 2 - bf, 0, -height / 2 + bf), pt(-width * middleF, depth, -height * middleF), pt(width * middleF, depth, -height * middleF), pt(width / 2 + bf, 0, -height / 2 + bf),
                pt(-width / 2 - bf, 0, height / 2 - bf), pt(-width * middleF, depth, height * middleF), pt(width * middleF, depth, height * middleF), pt(width / 2 + bf, 0, height / 2 - bf),
                pt(-width / 2, 0, height / 2), pt(-width / 2 + bf, 0, height / 2 + bf), pt(width / 2 - bf, 0, height / 2 + bf), pt(width / 2, 0, height / 2),
                bézierSteps);
            return CreateMesh(false, false, patch)
            .Select(arr => arr.Select(val => val.WithTexture((-val.Location.X + width / 2 + bf) / (width + 2 * bf), (val.Location.Z + height / 2 + bf) / (height + 2 * bf))).ToArray())
            .ToArray()
            ;
        }
    }
}
