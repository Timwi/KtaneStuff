using System.IO;
using System.Linq;
using KtaneStuff.Modeling;
using RT.Util.ExtensionMethods;

namespace KtaneStuff
{
    using static Md;

    static class TheWhiteButton
    {
        public static void MakeModels()
        {
            const int numVertices = 12;
            const double ir = .5;       // inner radius
            const double or = .7; // outer radius
            const double bw = .05;
            const double h = .03;

            var bézier = Bézier(p(0, 0), p(0, h * .5), p(bw * .5, h), p(bw, h), 6);

            File.WriteAllText(@"D:\c\KTANE\SingleSelectablePack\Assets\Modules\White\Assets\Segment.obj",
                GenerateObjFile(
                    CreateMesh(false, false,
                        Enumerable.Range(0, numVertices)
                            .Select(i => bézier.Select(pt => pt + p(ir, 0)).Concat(bézier.Reverse().Select(pt => p(or - pt.X, pt.Y))).Select(p => pt(p.X, p.Y, 0).RotateY(120.0 * i / (numVertices - 1))).ToArray())
                            .Select((pts, fst, lst) => pts.Select((pt, f, l) => pt.WithMeshInfo(fst || lst ? Normal.Mine : Normal.Average, fst || lst ? Normal.Mine : Normal.Average, f || l ? Normal.Mine : Normal.Average, f || l ? Normal.Mine : Normal.Average)).ToArray())
                            .ToArray()),
                    "Segment"));
        }
    }
}