using System.Collections.Generic;
using System.IO;
using System.Linq;
using RT.Util;

namespace KtaneStuff.Modeling
{
    using System;
    using static Md;

    static class Coordinates
    {
        public static void Do()
        {
            File.WriteAllText(@"D:\c\KTANE\Coordinates\Assets\Misc\Button.obj", GenerateObjFile(Button(Enumerable.Range(0, 3).Select(i => i * 120 + 60).ToArray()), "Button"));
            File.WriteAllText(@"D:\c\KTANE\Coordinates\Assets\Misc\SubmitButton.obj", GenerateObjFile(Button(Enumerable.Range(0, 4).Select(i => i * 90 + 45).ToArray()), "SubmitButton"));
            File.WriteAllText(@"D:\c\KTANE\Coordinates\Assets\Misc\Screen.obj", GenerateObjFile(Screen(), "Screen"));
        }

        private static IEnumerable<VertexInfo[]> Screen()
        {
            const int xSteps = 72;
            const int ySteps = 36;

            const double width = .75;
            const double height = .3;
            const double roundingRadius = .04;

            return CreateMesh(true, false,
                Enumerable.Range(0, xSteps).Select(i => -i * 360.0 / xSteps).Select((angle, x) =>
                    Enumerable.Range(0, ySteps)
                        .Select(i => i * 180.0 / (ySteps - 1))
                        .Select(angle2 => pt(roundingRadius * cos(angle2), roundingRadius * sin(angle2), 0).RotateY(angle))
                        .Select(p => p + new[] { pt(width, 0, -height), pt(-width, 0, -height), pt(-width, 0, height), pt(width, 0, height) }[x * 4 / xSteps])
                        .Select(p => p.WithMeshInfo(Normal.Average, Normal.Average, Normal.Average, Normal.Average))
                        .Concat(new[] { pt(0, 0, 0).WithMeshInfo(0, 1, 0) })
                        .ToArray())
                    .ToArray());
        }

        private static IEnumerable<VertexInfo[]> Button(int[] angles)
        {
            const int xSteps = 72;
            const int ySteps = 36;

            const double baseHeightB = .04;
            const double baseHeight = .05;
            const double topHeight = .07;
            const double stemWidth = .02;
            const double stemWidthB = .03;
            const double buttonRadius = .1;
            const double roundingRadius = topHeight - baseHeight;

            return CreateMesh(true, false,
                Enumerable.Range(0, xSteps).Select(i => -i * 360.0 / xSteps).Select((angle, x) =>
                    Ut.NewArray(
                        pt(stemWidth * cos(angle), 0, stemWidth * sin(angle)).WithMeshInfo(Normal.Average, Normal.Average, Normal.Mine, Normal.Mine),
                        pt(stemWidth * cos(angle), baseHeightB, stemWidth * sin(angle)).WithMeshInfo(Normal.Average, Normal.Average, Normal.Average, Normal.Average),
                        pt(stemWidthB * cos(angle), baseHeight, stemWidthB * sin(angle)).WithMeshInfo(Normal.Average, Normal.Average, Normal.Average, Normal.Average)
                    )
                        .Concat(Enumerable.Range(0, ySteps)
                            .Select(i => i * 180.0 / (ySteps - 1) - 90.0)
                            .Select(angle2 => pt(roundingRadius * cos(angle2), roundingRadius * sin(angle2) + baseHeight + roundingRadius, 0).RotateY(angle))
                            .Select(p => p + pt(buttonRadius, 0, 0).RotateY(-angles[x * angles.Length / xSteps]))
                            .Select(p => p.WithMeshInfo(Normal.Average, Normal.Average, Normal.Average, Normal.Average)))
                        .Concat(new[] { pt(0, baseHeight + 2 * roundingRadius, 0).WithMeshInfo(0, 1, 0) })
                        .ToArray())
                    .ToArray());
        }
    }
}
