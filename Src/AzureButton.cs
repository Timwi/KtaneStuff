using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Media;
using BlueButtonLib;
using KtaneStuff.Modeling;
using RT.Util.ExtensionMethods;

namespace KtaneStuff
{
    using static Md;

    static class AzureButton
    {
        public static void GenerateArrows()
        {
            var dxs = new[] { 0, 1, 1, 1, 0, -1, -1, -1 };
            var dys = new[] { -1, -1, 0, 1, 1, 1, 0, -1 };

            foreach (var arrow in AzureButtonArrowInfo.AllArrows.Where(a => a.Directions[0].IsBetween(0, 1)))
            {
                var objName = arrow.ModelName;

                const double torusRadius = .4;
                var faces = LooseModels.Torus(torusRadius, .075, 32).ToList();

                var x = 0;
                var y = 0;
                int dir = -1;
                for (var line = 0; line < arrow.Directions.Length; line++)
                {
                    var start = line == 0 ? new BlueButtonLib.Coord(4, 4, 0, 0) : arrow.Coordinates[line - 1];
                    var end = arrow.Coordinates[line];

                    int d(int x1, int x2) => x1 == 0 && x2 == 3 ? -1 : x1 == 3 && x2 == 0 ? 1 : x2 - x1;
                    dir = Enumerable.Range(0, 8).First(dir => dxs[dir] == d(start.X, end.X) && dys[dir] == d(start.Y, end.Y));
                    faces.AddRange(LooseModels.Cylinder(line == 0 ? torusRadius : 0, dir % 2 != 0 ? Math.Sqrt(2) : 1, .05, 32).Select(face => face.Select(v => v.RotateY(dir * -45).Move(x: x, z: -y)).ToArray()));
                    if (line > 0)
                        faces.AddRange(LooseModels.Sphere(.1).Select(face => face.Select(v => v.Move(x: x, z: -y)).ToArray()));
                    x += dxs[dir];
                    y += dys[dir];
                }
                faces.AddRange(LooseModels.Cone(-.25, .25, .15, 32).Select(face => face.Select(p => p.RotateY(dir * -45).Move(x: x, z: -y)).ToArray()));
                File.WriteAllText($@"D:\c\KTANE\BunchOfButtons\Assets\Modules\Azure\Assets\Arrows\{objName}.obj", GenerateObjFile(faces, objName));
            }
        }
    }
}