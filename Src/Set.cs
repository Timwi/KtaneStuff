using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using KtaneStuff.Modeling;
using RT.Util;
using RT.Util.Drawing;
using RT.Util.ExtensionMethods;

namespace KtaneStuff
{
    using static Md;

    static class Set
    {
        public static void MakeGraphics()
        {
            var pngCrushers = new List<Action>();
            foreach (var selected in new[] { true, false })
                using (var srcBmp = new Bitmap($@"D:\c\KTANE\Set\Data\Symbols{(selected ? " selected" : "")}.png"))
                {
                    var w = srcBmp.Width / 9;
                    var h = srcBmp.Height / 9;
                    for (int x = 0; x < 3; x++)
                        for (int y = 0; y < 3; y++)
                            for (int s = 0; s < 3; s++)
                                for (int r = 0; r < 3; r++)
                                {
                                    var tempPath = Path.GetTempFileName();
                                    var destPath = $@"D:\c\KTANE\Set\Assets\Textures\Icon{(selected ? "Sel" : "")}{(char) ('A' + x)}{(char) ('1' + y)}{(char) ('a' + s)}{(char) ('1' + r)}.png";
                                    GraphicsUtil.DrawBitmap(w, h, g =>
                                    {
                                        g.Clear(Color.Transparent);
                                        g.DrawImage(srcBmp, -w * (3 * s + x), -h * (3 * r + y));
                                    }).Save(tempPath);
                                    pngCrushers.Add(() =>
                                    {
                                        lock (pngCrushers)
                                            Console.WriteLine("Crushing: " + destPath);
                                        CommandRunner.Run("pngcr", tempPath, destPath).Go();
                                        File.Delete(tempPath);
                                        lock (pngCrushers)
                                            Console.WriteLine("Done: " + destPath);
                                    });
                                }
                }
            Ut.ParallelForEach(pngCrushers, 4, a => a());
        }

        public static void DoModels()
        {
            File.WriteAllText(@"D:\c\KTANE\Set\Assets\Models\CardHighlight.obj", GenerateObjFile(CardHighlight(), "CardHighlight"));
            File.WriteAllText(@"D:\c\KTANE\Set\Assets\Models\CardSelection.obj", GenerateObjFile(CardSelection(), "CardSelection"));
        }

        private static IEnumerable<Pt[]> CardHighlight()
        {
            const int revSteps = 8;
            const double innerRadius = .17;
            const double outerRadius = .25;
            const double displacement = .29;

            Pt rPt(double radius, double angle, int quadrant) => pt(radius * cos(angle) + displacement * new[] { 1, -1, -1, 1 }[quadrant], 0, radius * sin(angle) + displacement * new[] { 1, 1, -1, -1 }[quadrant]);

            var infs = Enumerable.Range(0, 4)
                .SelectMany(q => Enumerable.Range(0, revSteps).Select(i => i * 90.0 / (revSteps - 1)).Select(angle => new { Angle = angle, Quadrant = q }))
                .SelectConsecutivePairs(true, (inf1, inf2) => new { A1 = inf1.Angle, Q1 = inf1.Quadrant, A2 = inf2.Angle, Q2 = inf2.Quadrant });

            foreach (var inf in infs)
                yield return new Pt[] { rPt(innerRadius, inf.A1 + 90 * inf.Q1, inf.Q1), rPt(outerRadius, inf.A1 + 90 * inf.Q1, inf.Q1), rPt(outerRadius, inf.A2 + 90 * inf.Q2, inf.Q2), rPt(innerRadius, inf.A2 + 90 * inf.Q2, inf.Q2) };
        }

        private static IEnumerable<VertexInfo[]> CardSelection()
        {
            const int revSteps = 8;
            const double radius = .19;
            const double displacement = .3;
            const double depth = .05;

            Pt rPt(double rds, double angle, int quadrant, double dpth) => pt(rds * cos(angle) + displacement * new[] { 1, -1, -1, 1 }[quadrant], dpth, rds * sin(angle) + displacement * new[] { 1, 1, -1, -1 }[quadrant]);

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
}