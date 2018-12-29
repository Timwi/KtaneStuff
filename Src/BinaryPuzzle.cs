using System.Collections.Generic;
using System.IO;
using System.Linq;
using KtaneStuff.Modeling;
using RT.Util;
using RT.Util.ExtensionMethods;

namespace KtaneStuff
{
    using static Md;

    static class BinaryPuzzle
    {
        public static void DoModels()
        {
            File.WriteAllText(@"D:\c\KTANE\BinaryPuzzle\Assets\Models\Button.obj", GenerateObjFile(Button(.8, new (double x, double y, double z, double uv)[] { (1.05, 0, 1.05, 0), (.9, .7, .9, .165), (.7, .8, .7, .215) }), "Button"));
            File.WriteAllText(@"D:\c\KTANE\BinaryPuzzle\Assets\Models\BigButton.obj", GenerateObjFile(Button(.8, new (double x, double y, double z, double uv)[] { (5.05, 0, 1.25, 0), (4.9, .7, 1.1, .165), (4.7, .8, .9, .215) }), "BigButton"));
        }

        private static IEnumerable<VertexInfo[]> Button(double top, (double x, double y, double z, double uv)[] arr)
        {
            return CreateMesh(true, false, Ut.NewArray(
                arr.Select(inf => pt(-inf.x, inf.y, inf.z).WithMeshInfo(Normal.Mine, Normal.Mine, Normal.Mine, Normal.Mine).WithTexture(inf.uv, 1 - inf.uv)).Concat(pt(0, top, 0).WithMeshInfo(0, 1, 0).WithTexture(.5, .5)).ToArray(),
                arr.Select(inf => pt(inf.x, inf.y, inf.z).WithMeshInfo(Normal.Mine, Normal.Mine, Normal.Mine, Normal.Mine).WithTexture(1 - inf.uv, 1 - inf.uv)).Concat(pt(0, top, 0).WithMeshInfo(0, 1, 0).WithTexture(.5, .5)).ToArray(),
                arr.Select(inf => pt(inf.x, inf.y, -inf.z).WithMeshInfo(Normal.Mine, Normal.Mine, Normal.Mine, Normal.Mine).WithTexture(1 - inf.uv, inf.uv)).Concat(pt(0, top, 0).WithMeshInfo(0, 1, 0).WithTexture(.5, .5)).ToArray(),
                arr.Select(inf => pt(-inf.x, inf.y, -inf.z).WithMeshInfo(Normal.Mine, Normal.Mine, Normal.Mine, Normal.Mine).WithTexture(inf.uv, inf.uv)).Concat(pt(0, top, 0).WithMeshInfo(0, 1, 0).WithTexture(.5, .5)).ToArray()
            ));
        }
    }
}