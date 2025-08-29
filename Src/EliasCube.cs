using RT.Modeling;
using static RT.Modeling.Md;

namespace KtaneStuff;

internal static class EliasCube
{
    public static void DoModels() => File.WriteAllText(@"D:\c\KTANE\EliasCube\Assets\Circle.obj", GenerateObjFile(circle(), "Circle"));

    private static IEnumerable<VertexInfo[]> circle()
    {
        const int revSteps = 72;
        return CreateMesh(true, false, Enumerable.Range(0, revSteps).Select(i => i * 360.0 / revSteps).Select(angle => new[] { pt(cos(angle), 0, sin(angle)), 1.5 * pt(cos(angle), 0, sin(angle)) }.Select(p => p.WithMeshInfo(0, 1, 0)).ToArray()).ToArray());
    }
}
