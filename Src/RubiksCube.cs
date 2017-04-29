using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using KtaneStuff.Modeling;
using RT.Util;
using RT.Util.ExtensionMethods;

namespace KtaneStuff
{
    using static Md;

    static class RubiksCube
    {
        public static void DoModels()
        {
            File.WriteAllText(@"D:\c\KTANE\RubiksCube\Assets\Models\Cubelet.obj", GenerateObjFile(Cubelet(), "Cubelet"));
        }

        private static IEnumerable<VertexInfo[]> Cubelet()
        {
            const int roundSteps = 48;
            return CreateMesh(false, true, anglesFull(roundSteps, 2).Concat(180d)
                .Select((a, aIx) => anglesFull(roundSteps, 4)
                    .Select((b, bIx) => distort(new Pt(0, 1, 0).RotateZ(a).RotateY(b))
                        .Apply(p => aIx == 0 ? p.WithMeshInfo(0, 1, 0) : aIx == 2 * roundSteps ? p.WithMeshInfo(0, -1, 0) : p.WithMeshInfo(Normal.Average, Normal.Average, Normal.Average, Normal.Average)))
                    .Reverse()
                    .ToArray())
                .ToArray());
        }

        private static double _45to8 = Math.Pow(45, 8);
        private static IEnumerable<double> angles(int steps) => Enumerable.Range(0, steps).Select(x => x * 90.0 / steps).Select(x => Math.Pow(x - 45, 9) / _45to8 + 45);
        private static IEnumerable<double> anglesFull(int steps, int num) => Enumerable.Range(0, num).SelectMany(i => angles(steps).Select(a => a + 90 * i));

        private static Pt distort(Pt p) => p / Math.Pow(Math.Pow(p.X, 10) + Math.Pow(p.Y, 10) + Math.Pow(p.Z, 10), 1.0 / 10);
    }
}
