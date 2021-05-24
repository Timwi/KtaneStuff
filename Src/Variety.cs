using System.Collections.Generic;
using System.IO;
using System.Linq;
using KtaneStuff.Modeling;

namespace KtaneStuff
{
    using static Md;

    static class Variety
    {
        public static void DoModels()
        {
            File.WriteAllText(@"D:\c\KTANE\Variety\Assets\Items\Wires\WireBase.obj", GenerateObjFile(Base(), "WireBase"));
        }

        private static IEnumerable<VertexInfo[]> Base()
        {
            const double height = .2;
            const double holeI = .08;
            const double holeO = .1;
            const double holeDepth = .5;

            var curve = new[] { p(0, height - holeDepth), p(holeI, height), p(holeO, height), p(holeO, 0) };

            return CreateMesh(true, false, Enumerable.Range(0, 4)
                .Select(i => i * 360.0 / 4)
                .Select(angle => curve
                    .Select((p, i) => { var pr = pt(p.X, p.Y, 0).RotateY(angle); return pr.WithMeshInfo(Normal.Average, Normal.Average, Normal.Mine, Normal.Mine); })
                    .ToArray())
                .ToArray());
        }
    }
}