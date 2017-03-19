using System.Collections.Generic;
using System.IO;
using System.Linq;
using KtaneStuff.Modeling;
using RT.Util;
using RT.Util.ExtensionMethods;

namespace KtaneStuff
{
    using static Md;

    static class LightCycle
    {
        public static void DoModels()
        {
            File.WriteAllText(@"D:\c\KTANE\LightCycle\Assets\Misc\Button.obj", GenerateObjFile(Button(), "Button"));
            File.WriteAllText(@"D:\c\KTANE\LightCycle\Assets\Misc\LedSocket.obj", GenerateObjFile(LedSocket(), "LedSocket"));
        }

        private static IEnumerable<VertexInfo[]> LedSocket()
        {
            return Cylinder(.01, .1, .1, 32);
        }

        private static IEnumerable<VertexInfo[]> Button()
        {
            const double oRadius = .1;
            const double iRadius = .06;
            const double baseF = .035;
            const double halfHeight = .05;
            const double hhFDown = .03;
            const double hhFUp = .04;
            const double height = .1;
            const double topF = .04;
            const double bézierSmoothness = .0001;
            const int roundSteps = 72;

            var curve = SmoothBézier(p(0, height), p(topF, height), p(iRadius, halfHeight + hhFUp), p(iRadius, halfHeight), bézierSmoothness)
                .Concat(SmoothBézier(p(iRadius, halfHeight), p(iRadius, halfHeight - hhFDown), p(oRadius, baseF), p(oRadius, 0), bézierSmoothness).Skip(1))
                .ToArray();

            return CreateMesh(true, false,
                Enumerable.Range(0, roundSteps).Select(i => 360.0 * i / roundSteps).Select(angle =>
                    curve.Select((p, fst, lst) => pt(p.X, p.Y, 0).RotateY(angle).Apply(np => fst ? np.WithMeshInfo(0, 1, 0) : np.WithMeshInfo(Normal.Average, Normal.Average, lst ? Normal.Mine : Normal.Average, lst ? Normal.Mine : Normal.Average))).ToArray()).ToArray());
        }
    }
}
