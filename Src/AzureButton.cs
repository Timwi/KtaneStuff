using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Media;
using KtaneStuff.Modeling;
using RT.Util.ExtensionMethods;

namespace KtaneStuff
{
    using static Md;

    static class AzureButton
    {
        public static void GenerateArrows()
        {
            // Generates all possible arrows that don’t intersect with themselves
            var dxs = new[] { 0, 1, 1, 1, 0, -1, -1, -1 };
            var dys = new[] { -1, -1, 0, 1, 1, 1, 0, -1 };
            const int maxArrowLength = 3;

            IEnumerable<(Coord coord, int dir)[]> generateArrows((Coord coord, int dir)[] sofar)
            {
                if (sofar.Length == maxArrowLength)
                {
                    yield return sofar;
                    yield break;
                }

                var start = sofar.Length == 0 ? new Coord(4, 4, 0, 0) : sofar.Last().coord;
                for (var dir = 0; dir < 8; dir++)
                {
                    var next = start.AddWrap(dxs[dir], dys[dir]);
                    if (next.Index != 0 && !sofar.Any(tup => tup.coord == next))
                        foreach (var result in generateArrows(sofar.Insert(sofar.Length, (next, dir))))
                            yield return result;
                }
            }
            var allArrows = generateArrows(new (Coord coord, int dir)[0]).ToArray();

            Console.WriteLine(allArrows.Length);

            foreach (var arrow in allArrows)
            {
                var objName = $"Arrow-{arrow.Select(c => c.dir).JoinString()}";

                const double torusRadius = .4;
                var faces = LooseModels.Torus(torusRadius, .075, 32).ToList();

                var x = 0;
                var y = 0;
                int dir = -1;
                for (var line = 0; line < arrow.Length; line++)
                {
                    var start = line == 0 ? new Coord(4, 4, 0, 0) : arrow[line - 1].coord;
                    var end = arrow[line].coord;

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