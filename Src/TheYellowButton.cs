﻿using System.IO;
using System.Linq;
using KtaneStuff.Modeling;

namespace KtaneStuff
{
    using static Md;

    static class TheYellowButton
    {
        public static void MakeModels()
        {
            const int numVertices = 12;
            const double ir = .5;       // inner radius
            const double or = .7; // outer radius

            File.WriteAllText(@"D:\c\KTANE\BunchOfButtons\Assets\Modules\Yellow\Assets\Segment.obj",
                GenerateObjFile(
                    CreateMesh(false, false,
                        Enumerable.Range(0, numVertices)
                            .Select(i => new[] { p(ir, 0), p(or, 0) }.Select(p => pt(p.X, p.Y, 0).RotateY(45.0 * i / (numVertices - 1)).WithMeshInfo(0, 1, 0)).ToArray())
                            .ToArray()),
                    "Segment"));
        }
    }
}
