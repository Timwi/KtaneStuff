using System.Collections.Generic;
using System.IO;
using System.Linq;
using KtaneStuff.Modeling;
using RT.Util;
using RT.Util.ExtensionMethods;

namespace KtaneStuff
{
    using static Md;

    static class RegularCrazyTalk
    {
        public static void DoModels()
        {
            File.WriteAllText(@"D:\c\KTANE\RegularCrazyTalk\Assets\Models\Frame.obj", GenerateObjFile(Frame(new (double x, double y, double z, double uv)[] { (1, 0, 1.34, 0), (.95, .03, 1.29, .02), (.91, .03, 1.25, .04), (.9, 0, 1.24, .06) }), "Frame"));
            File.WriteAllText(@"D:\c\KTANE\RegularCrazyTalk\Assets\Models\SmallFrame.obj", GenerateObjFile(Frame(new (double x, double y, double z, double uv)[] { (.25, 0, .34, 0), (.20, .03, .29, .02), (.16, .03, .25, .04), (.15, 0, .24, .06) }), "SmallFrame"));
            File.WriteAllText(@"D:\c\KTANE\RegularCrazyTalk\Assets\Models\ButtonUp.obj", GenerateObjFile(Button(3, 90), "ButtonUp"));
            File.WriteAllText(@"D:\c\KTANE\RegularCrazyTalk\Assets\Models\ButtonDown.obj", GenerateObjFile(Button(3, 270), "ButtonDown"));
            File.WriteAllText(@"D:\c\KTANE\RegularCrazyTalk\Assets\Models\ButtonHighlight.obj", GenerateObjFile(new[] { Enumerable.Range(0, 3).Select(i => pt(0, 0, 1).RotateY(i * 120).WithNormal(0, 1, 0)).ToArray() }, "ButtonHighlight"));
        }

        private static IEnumerable<VertexInfo[]> Button(int revSteps, double extraAngle)
        {
            var arr = new (double x, double y)[] { (.25, 0), (.20, .03) };
            const double top = .03;
            return CreateMesh(true, false, Enumerable.Range(0, revSteps).Select(i => i * 360 / revSteps + extraAngle).Select(angle =>
                arr.Select(inf => pt(inf.x, inf.y, 0).RotateY(angle).WithMeshInfo(Normal.Mine, Normal.Mine, Normal.Mine, Normal.Mine).Apply(v => v.WithTexture((v.Location.X + 1) / 2, (v.Location.Z + 1) / 2))).Concat(pt(0, top, 0).WithMeshInfo(0, 1, 0).WithTexture(.5, .5)).ToArray()
            ).Reverse().ToArray());
        }

        private static IEnumerable<VertexInfo[]> Frame((double x, double y, double z, double uv)[] arr)
        {
            return CreateMesh(true, false, Ut.NewArray(
                arr.Select(inf => pt(-inf.x, inf.y, inf.z).WithMeshInfo(Normal.Mine, Normal.Mine, Normal.Mine, Normal.Mine).WithTexture(inf.uv, 1 - inf.uv)).ToArray(),
                arr.Select(inf => pt(inf.x, inf.y, inf.z).WithMeshInfo(Normal.Mine, Normal.Mine, Normal.Mine, Normal.Mine).WithTexture(1 - inf.uv, 1 - inf.uv)).ToArray(),
                arr.Select(inf => pt(inf.x, inf.y, -inf.z).WithMeshInfo(Normal.Mine, Normal.Mine, Normal.Mine, Normal.Mine).WithTexture(1 - inf.uv, inf.uv)).ToArray(),
                arr.Select(inf => pt(-inf.x, inf.y, -inf.z).WithMeshInfo(Normal.Mine, Normal.Mine, Normal.Mine, Normal.Mine).WithTexture(inf.uv, inf.uv)).ToArray()
            ));
        }
    }
}