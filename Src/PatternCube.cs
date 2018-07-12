using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using RT.Util.ExtensionMethods;

namespace KtaneStuff
{
    static class PatternCube
    {
        private static readonly List<(int face1, int face2, int direction, int orientation)> _transitions;
        static PatternCube()
        {
            _transitions = new List<(int face1, int face2, int direction, int orientation)>
            {
                (0, 1, 2, 0),
                (0, 2, 1, 3),
                (0, 3, 0, 2),
                (0, 4, 3, 1),
                (1, 2, 1, 0),
                (1, 4, 3, 0),
                (1, 5, 2, 2),
                (2, 3, 1, 0),
                (2, 5, 2, 1),
                (3, 4, 1, 0),
                (3, 5, 2, 0),
                (4, 5, 2, 3)
            };
            foreach (var (face1, face2, direction, orientation) in _transitions.ToArray())
                _transitions.Add((face2, face1, (direction + 6 - orientation) % 4, (4 - orientation) % 4));
        }

        private static int __debug_counter;
        public static void Experiment()
        {
            __debug_counter = 0;
            var results = new HashSet<string>();
            for (int face0Orientation = 0; face0Orientation < 4; face0Orientation++)
                recurse(results, new List<(int x, int y, int face, int orientation)> { (0, 0, 0, face0Orientation) });
            File.WriteAllText(@"D:\temp\PatternCubeNets.txt", results.JoinString("\n\n"));
            Console.WriteLine(__debug_counter);
            Console.WriteLine(results.Count);

            var allowableTopCombinations = ",X,Y,XY,XZ,XYZ".Split(',').SelectMany(str => "ABCD".Subsequences(3 - str.Length, 3 - str.Length).Select(ss => str + ss.JoinString())).ToArray();
            var allowableBottomCombinations = ",X,Z,XY,YZ,XYZ".Split(',').SelectMany(str => "EFGH".Subsequences(3 - str.Length, 3 - str.Length).Select(ss => str + ss.JoinString())).ToArray();

            Console.WriteLine($"{allowableTopCombinations.JoinString(" // ")} = {allowableTopCombinations.Length}");
            Console.WriteLine($"{allowableBottomCombinations.JoinString(" // ")} = {allowableBottomCombinations.Length}");
        }

        private static readonly int[] _xDelta = new[] { 0, 1, 0, -1 };
        private static readonly int[] _yDelta = new[] { -1, 0, 1, 0 };

        private static void recurse(HashSet<string> results, List<(int x, int y, int face, int orientation)> sofar)
        {
            if (sofar.Count == 6)
            {
                var sb = new StringBuilder();
                var minX = sofar.Min(k => k.x);
                var maxX = sofar.Max(k => k.x);
                var minY = sofar.Min(k => k.y);
                var maxY = sofar.Max(k => k.y);

                for (int sx = minX; sx <= maxX; sx++)
                    for (int sy = minY; sy <= maxY; sy++)
                        if (sofar.Any(t => t.x == sx && t.y == sy) && sofar.Any(t => t.x == sx + 1 && t.y == sy) && sofar.Any(t => t.x == sx && t.y == sy + 1) && sofar.Any(t => t.x == sx + 1 && t.y == sy + 1))
                            return;

                for (int sy = minY; sy <= maxY; sy++)
                    sb.AppendLine(Enumerable.Range(minX, maxX - minX + 1).Select(sx =>
                    {
                        var ix = sofar.IndexOf(tuple => tuple.x == sx && tuple.y == sy);
                        return ix == -1 ? "   " : $"{sofar[ix].face}{"NESW"[sofar[ix].orientation]} ";
                    }).JoinString());
                if (!results.Add(sb.ToString()))
                    __debug_counter++;
            }
            else
            {
                foreach (var (face1, face2, direction, newOrientation) in _transitions)
                {
                    var ix = sofar.IndexOf(tp => tp.face == face1);
                    if (ix == -1)
                        continue;
                    if (sofar.Any(tp => tp.face == face2))
                        continue;
                    var (x, y, face, oldOrientation) = sofar[ix];
                    var newX = x + _xDelta[(direction + oldOrientation) % 4];
                    var newY = y + _yDelta[(direction + oldOrientation) % 4];
                    if (sofar.Any(tp => tp.x == newX && tp.y == newY))
                        continue;
                    var newSofar = sofar.ToList();
                    newSofar.Add((newX, newY, face2, (newOrientation + oldOrientation) % 4));
                    recurse(results, newSofar);
                }
            }
        }
    }
}