using System.Collections.Generic;
using System.IO;
using System.Linq;
using KtaneStuff.Modeling;
using RT.Util.Geometry;

namespace KtaneStuff
{
    using static Md;

    static class Yahtzee
    {
        public static void DoModels()
        {
            File.WriteAllText(@"D:\c\KTANE\Yahtzee\Assets\Models\RollButton.obj", GenerateObjFile(RollButton(), "RollButton"));
            File.WriteAllText(@"D:\c\KTANE\Yahtzee\Assets\Models\RollText.obj", GenerateObjFile(RollText(), "RollText"));
            File.WriteAllText(@"D:\c\KTANE\Yahtzee\Assets\Models\RollHighlight.obj", GenerateObjFile(RollHighlight(), "RollHighlight"));
        }

        private static IEnumerable<VertexInfo[]> RollButton()
        {
            var revSteps = 8;
            var curve = Enumerable.Range(0, revSteps + 1).Select(j => j * 90.0 / revSteps - 90).Select(angle => p(cos(angle), sin(angle) - 3))
                .Concat(Enumerable.Range(0, revSteps + 1).Select(j => j * 90.0 / revSteps).Select(angle => p(cos(angle), sin(angle) + 3)));
            return CreateMesh(false, false,
                Enumerable.Range(0, revSteps + 1).Select(i => i * 180.0 / revSteps)
                    .Select(angle => curve.Select(p => pt(cos(angle) * p.X, sin(angle) * p.X, p.Y)).ToArray())
                    .ToArray());
        }

        private static IEnumerable<VertexInfo[]> RollText()
        {
            return ExtrudedText("ROLL", "Showcard Gothic", 2.5);
        }

        private static IEnumerable<Pt[]> RollHighlight()
        {
            var revSteps = 24;
            IEnumerable<PointD> curve(double radius) =>
                Enumerable.Range(0, revSteps + 1).Select(j => j * 180.0 / revSteps + 90).Select(angle => p(radius * cos(angle) - 3, radius * sin(angle)))
                .Concat(Enumerable.Range(0, revSteps + 1).Select(j => j * 180.0 / revSteps - 90).Select(angle => p(radius * cos(angle) + 3, radius * sin(angle))));
            return
                new[] { curve(1.25), curve(1).Reverse() }
                .Triangulate()
                .Select(face => face.Select(p => pt(p.X, 0, p.Y)).Reverse().ToArray());
        }
    }
}