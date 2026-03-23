using System.Diagnostics;
using RT.Json;
using RT.Modeling;
using RT.Util;
using RT.Util.Consoles;
using RT.Util.ExtensionMethods;
using static RT.Modeling.Md;

namespace KtaneStuff;

internal static class Set
{
    public static void MakeGraphics()
    {
        var datas = new List<(double x, double w, double h, string d)>();
        var c = 0;
        RT.Xml.Xml.ParseFile(@"D:\c\KTANE\Set\Data\Symbols (new).svg").Root["path"].ToArray().ParallelForEach(Environment.ProcessorCount, (tag, proc) =>
        {
            if (proc == 0)
                lock (datas)
                    ConsoleUtil.WriteLineFmt($"{c:Y}/{18:M} = {(double) 100 * c / 18:G/0}{"%":G}");
            var id = tag["@id"].Value;

            var cmd = $@"D:\Inkscape\bin\inkscape.com ""D:\c\KTANE\Set\Data\Symbols (new).svg"" --query-id={id} -X -Y -W -H";
            var values = CommandRunner.RunRaw(cmd).OutputNothing().GoGetOutputText().Trim().Split('\n').Select(double.Parse).ToArray();
            var (x, y, w, h) = (values[0], values[1], values[2], values[3]);
            if (y > 50)
                return;

            var cmd2 = $@"D:\Inkscape\bin\inkscape.com ""D:\c\KTANE\Set\Data\Symbols (new).svg"" --export-id={id} --export-area={x}:{y}:{x + w}:{y + h} --export-dpi=96 --export-filename=D:\temp\temp{proc}.svg --export-type=svg --export-plain-svg --export-id-only";
            CommandRunner.RunRaw(cmd2).OutputNothing().Go();

            var pathData = RT.Xml.Xml.ParseFile($@"D:\temp\temp{proc}.svg").Root["path"]["@d"].Value;
            lock (datas)
            {
                datas.Add((x, w, h, pathData));
                c++;
            }

            File.Delete($@"D:\temp\temp{proc}.svg");
        });
        datas.SortBy(tup => tup.x);
        if (datas.Count != 18)
            Debugger.Break();
        Clipboard.SetText(datas.Select(tup => new JsonDict { ["d"] = tup.d, ["w"] = tup.w, ["h"] = tup.h }).ToJsonList().ToStringIndented());
    }

    public static void DoModels()
    {
        File.WriteAllText(@"D:\c\KTANE\Set\Assets\Models\CardHighlight.obj", GenerateObjFile(cardHighlight(), "CardHighlight"));
        File.WriteAllText(@"D:\c\KTANE\Set\Assets\Models\CardSelection.obj", GenerateObjFile(cardSelection(), "CardSelection"));
    }

    private static IEnumerable<Pt[]> cardHighlight()
    {
        const int revSteps = 8;
        const double innerRadius = .17;
        const double outerRadius = .25;
        const double displacement = .29;

        static Pt rPt(double radius, double angle, int quadrant) => pt(radius * cos(angle) + displacement * new[] { 1, -1, -1, 1 }[quadrant], 0, radius * sin(angle) + displacement * new[] { 1, 1, -1, -1 }[quadrant]);

        var infs = Enumerable.Range(0, 4)
            .SelectMany(q => Enumerable.Range(0, revSteps).Select(i => i * 90.0 / (revSteps - 1)).Select(angle => new { Angle = angle, Quadrant = q }))
            .SelectConsecutivePairs(true, (inf1, inf2) => new { A1 = inf1.Angle, Q1 = inf1.Quadrant, A2 = inf2.Angle, Q2 = inf2.Quadrant });

        foreach (var inf in infs)
            yield return new Pt[] { rPt(innerRadius, inf.A1 + 90 * inf.Q1, inf.Q1), rPt(outerRadius, inf.A1 + 90 * inf.Q1, inf.Q1), rPt(outerRadius, inf.A2 + 90 * inf.Q2, inf.Q2), rPt(innerRadius, inf.A2 + 90 * inf.Q2, inf.Q2) };
    }

    private static IEnumerable<VertexInfo[]> cardSelection()
    {
        const int revSteps = 8;
        const double radius = .19;
        const double displacement = .3;
        const double depth = .05;

        static Pt rPt(double rds, double angle, int quadrant, double dpth) => pt(rds * cos(angle) + displacement * new[] { 1, -1, -1, 1 }[quadrant], dpth, rds * sin(angle) + displacement * new[] { 1, 1, -1, -1 }[quadrant]);

        var infs = Enumerable.Range(0, 4)
            .SelectMany(q => Enumerable.Range(0, revSteps).Select(i => i * 90.0 / (revSteps - 1)).Select(angle => new { Angle = angle, Quadrant = q }));

        return CreateMesh(true, false, infs.Select(inf => Ut.NewArray(
            pt(0, depth, 0).WithMeshInfo(0, 1, 0),
            rPt(radius, inf.Angle + 90 * inf.Quadrant, inf.Quadrant, depth).WithMeshInfo(Normal.Average, Normal.Average, Normal.Mine, Normal.Mine),
            rPt(radius, inf.Angle + 90 * inf.Quadrant, inf.Quadrant, 0).WithMeshInfo(Normal.Average, Normal.Average, Normal.Mine, Normal.Mine),
            pt(0, 0, 0).WithMeshInfo(0, -1, 0)
        )).ToArray());
    }
}
