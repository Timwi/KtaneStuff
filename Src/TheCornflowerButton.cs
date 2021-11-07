using System.IO;
using System.Linq;
using KtaneStuff.Modeling;
using RT.KitchenSink;

namespace KtaneStuff
{
    using static Md;

    static class TheCornflowerButton
    {
        public static void MakeModels()
        {
            const int numVertices = 96;
            const double ir = .5;       // inner radius
            const double or = .7; // outer radius

            File.WriteAllText(@"D:\c\KTANE\BunchOfButtons\Assets\Modules\Cornflower\Assets\Annulus.obj",
                GenerateObjFile(
                    CreateMesh(true, false,
                        Enumerable.Range(0, numVertices)
                            .Select(i => new[] { p(ir, 0), p(or, 0) }.Select(p => pt(p.X, p.Y, 0).RotateY(360.0 * i / numVertices).WithMeshInfo(0, 1, 0)).ToArray())
                            .ToArray()),
                    "Annulus"));

            const string svgPath = "m 0.01929863,0.48908235 h -0.03970005 v -0.82653266 q -0.08160561,0.0584473 -0.15604317,0.10255846 l -0.006617,-0.00772 Q -0.08160561,-0.35454335 0,-0.48908238 q 0.08160561,0.13453903 0.18306128,0.24647105 l -0.006617,0.00772 Q 0.09924966,-0.2828624 0.0192982,-0.33744998 Z";

            File.WriteAllText(@"D:\c\KTANE\BunchOfButtons\Assets\Modules\Cornflower\Assets\Arrow.obj",
                GenerateObjFile(DecodeSvgPath.DecodePieces(svgPath).Extrude(.025, .1, true), "Arrow"));
            File.WriteAllText(@"D:\c\KTANE\BunchOfButtons\Assets\Modules\Cornflower\Assets\Node.obj",
                GenerateObjFile(LooseModels.Cylinder(0, .05, .075, 36), "Node"));
        }
    }
}