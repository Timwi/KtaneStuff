using System.Collections.Generic;
using System.IO;
using System.Linq;
using KtaneStuff.Modeling;
using RT.Util.ExtensionMethods;
using RT.Util.Geometry;

namespace KtaneStuff
{
    using static Md;

    static class Kudosudoku
    {
        public static void DoModels()
        {
            File.WriteAllText(@"D:\c\KTANE\Kudosudoku\Assets\Models\Background.obj", GenerateObjFile(Cleanup(Background()), "Background"));
            File.WriteAllText(@"D:\c\KTANE\Kudosudoku\Assets\Models\SubmitButton.obj", GenerateObjFile(SubmitButton(), "SubmitButton"));
            File.WriteAllText(@"D:\c\KTANE\Kudosudoku\Assets\Models\SquareHighlight.obj", GenerateObjFile(SquareHighlight(), "SquareHighlight"));
        }

        private static IEnumerable<VertexInfo[]> Cleanup(IEnumerable<Pt[]> x) => x
            .Select(face => face.Reverse().ToArray().FlatNormals().Select(v => v.WithTexture(new PointD(.4771284794 * v.Location.X + .46155, -.4771284794 * v.Location.Z + .5337373145))).ToArray())
            .ToArray();

        private static IEnumerable<Pt[]> Background()
        {
            const double h = .15;
            var outline = @"-0.854439,0.256271
-0.854439,0.854439
-0.256273,0.854439
0.256273,0.854439
0.854439,0.854439
0.854439,0.256271
0.854439,-0.256271
0.861069,-0.396061
0.784569,-0.472279
0.672195,-0.46653
0.524693,-0.545002
0.443619,-0.693596
0.444206,-0.774025
0.364376,-0.854439
0.256273,-0.854439
-0.256271,-0.854439
-0.854439,-0.854439
-0.854439,-0.256271".Replace("\r", "").Split('\n').Select(str => str.Split(',')).Select(arr => p(double.Parse(arr[0]), double.Parse(arr[1]))).ToArray();

            var cutOuts = new List<(RectangleD rect, double innerRadius, double depth)>();
            const double outset = 1.16;
            const double bevelWidth = (outset - 1) / 2 * .025 * 10;
            for (int x = 0; x < 4; x++)
                for (int y = 0; y < 4; y++)
                    cutOuts.Add(((new RectangleD(-1.8 + 1.2 * x - outset / 2, -1.8 + 1.2 * y - outset / 2, outset, outset) * .025 + new PointD(-.02, .02)) * 10, bevelWidth, .14));
            cutOuts.Add((new RectangleD(-.7, -.825, 1, .4), .02, .13));  // top panel: .96 × .36
            cutOuts.Add((new RectangleD(.425, -.3, .4, 1), .02, .13));   // right panel

            foreach (var (rect, innerRadius, depth) in cutOuts)
            {
                yield return new[] { pt(rect.Left, h, rect.Top), pt(rect.Right, h, rect.Top), pt(rect.Right - innerRadius, depth, rect.Top + innerRadius), pt(rect.Left + innerRadius, depth, rect.Top + innerRadius) };
                yield return new[] { pt(rect.Right, h, rect.Top), pt(rect.Right, h, rect.Bottom), pt(rect.Right - innerRadius, depth, rect.Bottom - innerRadius), pt(rect.Right - innerRadius, depth, rect.Top + innerRadius) };
                yield return new[] { pt(rect.Right, h, rect.Bottom), pt(rect.Left, h, rect.Bottom), pt(rect.Left + innerRadius, depth, rect.Bottom - innerRadius), pt(rect.Right - innerRadius, depth, rect.Bottom - innerRadius) };
                yield return new[] { pt(rect.Left, h, rect.Top), pt(rect.Left + innerRadius, depth, rect.Top + innerRadius), pt(rect.Left + innerRadius, depth, rect.Bottom - innerRadius), pt(rect.Left, h, rect.Bottom) };
            }

            var tr = new[] { outline }.Concat(cutOuts.Select(c => c.rect).Select(r => new[] { r.TopLeft, r.TopRight, r.BottomRight, r.BottomLeft })).Triangulate();
            foreach (var tri in tr)
                yield return tri.Select(p => pt(p.X, h, p.Y)).ToArray();
        }

        private static IEnumerable<VertexInfo[]> SubmitButton()
        {
            var steps = 30;

            foreach (var face in Enumerable.Range(0, steps).Select(i => i * 360 / steps).ConsecutivePairs(closed: true).Select(pair => new[] { ptp(1, pair.Item1, 1), ptp(1, pair.Item1, -1), ptp(1, pair.Item2, -1), ptp(1, pair.Item2, 1) }.ReverseInplace()))
                yield return face.FlatNormals().Select(v => v.WithTexture(0, 0)).ToArray();

            foreach (var face in Enumerable.Range(0, steps).Select(i => i * 360 / steps).ConsecutivePairs(closed: true).Select(pair => new[] { ptp(0, 0, 1), ptp(1, pair.Item1, 1), ptp(1, pair.Item2, 1) }.ReverseInplace()))
                yield return face.FlatNormals().Select(v => v.WithTexture((v.Location.X + 1) / 2, (1 - v.Location.Z) / 2)).ToArray();
        }

        private static IEnumerable<VertexInfo[]> SquareHighlight()
        {
            var w = 1.2;
            var h = .2;
            for (int i = 0; i < 4; i++)
                yield return new[] { pt(1, 0, -1), pt(w, h, -w), pt(w, h, w), pt(1, 0, 1) }.Select(p => p.RotateY(90 * i)).ToArray().FlatNormals();
        }
    }
}