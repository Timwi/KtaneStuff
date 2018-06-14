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

    static class Lasers
    {
        public static void MakeTextures()
        {
            var bmp = new Bitmap(@"D:\c\KTANE\Lasers\Data\Metal2.jpg");

            for (int hatchIx = 1; hatchIx <= 9; hatchIx++)
                foreach (var left in new[] { true, false })
                {
                    GraphicsUtil.DrawBitmap(250, 500, g =>
                    {
                        g.DrawImage(bmp, -200 * ((hatchIx - 1) % 3), -200 * ((hatchIx - 1) / 3));
                        using (var tr = new GraphicsTransformer(g).Translate(left ? 0 : -250, 0))
                        {
                            g.FillRectangle(Brushes.Black, 200, 0, 100, 500);
                            for (int i = 0; i < 5; i++)
                                g.FillPolygon(new SolidBrush(GraphicsUtil.FromHsv(60, .9, .7)), new[] { new Point(200, 0), new Point(300, -100), new Point(300, 0), new Point(200, 100) }.Select(p => { p.Offset(0, 200 * i + 30); return p; }).ToArray());
                        }
                        using (var tr2 = new GraphicsTransformer(g).Scale(.9, 1).Translate(left ? 255 : 5, 0))
                            g.DrawString(hatchIx.ToString(), new Font("Federal Escort Laser", 420f, FontStyle.Regular), new SolidBrush(GraphicsUtil.FromHsv(230, .1, .9)), 0, 250, new StringFormat { LineAlignment = StringAlignment.Center, Alignment = StringAlignment.Center });
                    }).Save($@"D:\c\KTANE\Lasers\Assets\Textures\Hatch-{hatchIx}-{(left ? "left" : "right")}.png");
                }
        }

        public static void MakeModels()
        {
            File.WriteAllText($@"D:\c\KTANE\Lasers\Assets\Models\HatchLeft.obj", GenerateObjFile(Hatch(left: true), "HatchLeft"));
            File.WriteAllText($@"D:\c\KTANE\Lasers\Assets\Models\HatchRight.obj", GenerateObjFile(Hatch(left: false), "HatchRight"));
        }

        private static IEnumerable<VertexInfo[]> Hatch(bool left)
        {
            var path = new[] { p(0, 0), p(1, 0), p(1, 2), p(0, 2) }.Select(pt => left ? pt : pt - p(1, 0));
            var flatSideNormals = true;
            const double depth = .1;

            // Walls
            foreach (var face in path
                .SelectConsecutivePairs(true, (p1, p2) => new { P1 = p1, P2 = p2 })
                .Where(inf => inf.P1 != inf.P2)
                .SelectConsecutivePairs(true, (q1, q2) => new { P = q1.P2, N1 = (pt(0, 1, 0) * pt(q1.P2.X - q1.P1.X, 0, q1.P2.Y - q1.P1.Y)), N2 = (pt(0, 1, 0) * pt(q2.P2.X - q2.P1.X, 0, q2.P2.Y - q2.P1.Y)) })
                .SelectConsecutivePairs(true, (p1, p2) => Ut.NewArray(
                    pt(p1.P.X, depth, p1.P.Y).WithNormal(flatSideNormals ? p1.N2 : p1.N1 + p1.N2).WithTexture(0, 0),
                    pt(p2.P.X, depth, p2.P.Y).WithNormal(flatSideNormals ? p1.N2 : p2.N1 + p2.N2).WithTexture(0, 0),
                    pt(p2.P.X, 0, p2.P.Y).WithNormal(flatSideNormals ? p1.N2 : p2.N1 + p2.N2).WithTexture(0, 0),
                    pt(p1.P.X, 0, p1.P.Y).WithNormal(flatSideNormals ? p1.N2 : p1.N1 + p1.N2).WithTexture(0, 0))))
                yield return face;

            // Front face
            yield return path.Select(p => pt(p.X, depth, p.Y).WithNormal(0, 1, 0).WithTexture(left ? 1 - p.X : -p.X, p.Y / 2)).Reverse().ToArray();

            // Back face
            yield return path.Select(p => pt(p.X, 0, p.Y).WithNormal(0, -1, 0).WithTexture(left ? 1 - p.X : -p.X, p.Y / 2)).ToArray();
        }

        private static int[] _lastTwoColumns = new[] { 1, 2, 4, 5, 7, 8 };

        public static void Analyze()
        {
            var hatchValidInFirstStage = new int[9];

            var count = 0;
            foreach (var laserOrder in Enumerable.Range(0, 9).Permutations().Select(perm => perm.ToArray()))
            {
                foreach (var timeRoot in Enumerable.Range(1, 9))
                    foreach (var moduleParity in new[] { 0, 1 })
                    {
                        //if (!VerifySolutionExists(laserOrder, rowRoot, columnRoot, timeRoot, moduleParity))
                        //    Console.WriteLine($"No valid solution for {laserOrder.JoinString(",")} with timeRoot={timeRoot} && moduleParity={moduleParity}");

                        var emptyList = new List<int>();
                        var list = new List<int>();
                        for (int hatch = 0; hatch < 9; hatch++)
                        {
                            list.Clear();
                            list.Add(hatch);
                            if (IsValid(hatch, 0, laserOrder, timeRoot, moduleParity, emptyList) && VerifySolutionExists(laserOrder, timeRoot, moduleParity, list))
                                hatchValidInFirstStage[hatch]++;
                        }
                        count++;
                    }
                if (count % 10000 == 0)
                    Console.WriteLine($"{count} permutations analyzed.");
            }
            Console.WriteLine($"Total: {count}");
            for (int i = 0; i < 9; i++)
                Console.WriteLine($"Hatch #{i + 1} = {hatchValidInFirstStage[i]}");
        }

        private static bool VerifySolutionExists(int[] laserOrder, int timeRoot, int moduleParity, List<int> hatchesAlreadyPressed)
        {
            int[] anyValidSolution(int stage)
            {
                if (stage == 7)
                    return hatchesAlreadyPressed.ToArray();

                for (int hatch = 0; hatch < 9; hatch++)
                    if (IsValid(hatch, stage, laserOrder, timeRoot, moduleParity, hatchesAlreadyPressed))
                    {
                        hatchesAlreadyPressed.Add(hatch);
                        var result = anyValidSolution(stage + 1);
                        hatchesAlreadyPressed.RemoveAt(hatchesAlreadyPressed.Count - 1);
                        if (result != null)
                            return result;
                    }
                return null;
            }
            return anyValidSolution(hatchesAlreadyPressed.Count) != null;
        }

        private static bool IsValid(int hatch, int stage, int[] laserOrder, int timeRoot, int moduleParity, List<int> hatchesAlreadyPressed)
        {
            switch (stage)
            {
                case 0:
                    return hatch / 3 != laserOrder.IndexOf((laserOrder[0] + laserOrder[1] + laserOrder[2] - 1) % 9 + 1) / 3;
                case 1:
                    return !hatchesAlreadyPressed.Contains(hatch) && (
                        hatch % 3 == hatchesAlreadyPressed[0] % 3 ? Math.Abs(hatch / 3 - hatchesAlreadyPressed[0] / 3) != 1 :
                        Math.Abs(hatch % 3 - hatchesAlreadyPressed[0] % 3) == 1 ? hatch / 3 != hatchesAlreadyPressed[0] / 3 : true);
                case 2:
                    return !hatchesAlreadyPressed.Contains(hatch) && hatch % 3 != laserOrder.IndexOf((_lastTwoColumns.Sum(x => laserOrder[x]) - 1) % 9 + 1) % 3;
                case 3:
                    return !hatchesAlreadyPressed.Contains(hatch) && Math.Abs(hatch % 3 - hatchesAlreadyPressed[2] % 3) == 1 && Math.Abs(hatch / 3 - hatchesAlreadyPressed[2] / 3) == 1;
                case 4:
                    return !hatchesAlreadyPressed.Contains(hatch) && hatch % 3 != laserOrder.IndexOf(timeRoot) % 3 && hatch / 3 != laserOrder.IndexOf(timeRoot) / 3;
                case 5:
                    return !hatchesAlreadyPressed.Contains(hatch) && laserOrder[hatch] % 2 != moduleParity;
                case 6:
                    return !hatchesAlreadyPressed.Contains(hatch) && (Math.Abs(hatch % 3 - hatchesAlreadyPressed[4] % 3) > 1 || Math.Abs(hatch / 3 - hatchesAlreadyPressed[4] / 3) > 1);
            }
            return false;
        }
    }
}