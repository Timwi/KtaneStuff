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

        public static void GenerateNets()
        {
            var results = new HashSet<string>();
            for (int face0Orientation = 0; face0Orientation < 4; face0Orientation++)
                recurse(results, new List<(int x, int y, int face, int orientation)> { (0, 0, 0, face0Orientation) });
            File.WriteAllText(@"D:\temp\PatternCubeNets.txt", results.JoinString("\n\n"));
            Console.WriteLine(@"Nets written to D:\temp\PatternCubeNets.txt.");
            Console.WriteLine(results.Count);
        }

        public static void GenerateManual()
        {
            var allowableGroup1Combinations = ",X,Y,XY,XZ,XYZ".Split(',').SelectMany(str => "ABCD".Subsequences(3 - str.Length, 3 - str.Length).Select(ss => str + ss.JoinString())).ToArray();
            var allowableGroup2Combinations = ",X,Z,XY,YZ,XYZ".Split(',').SelectMany(str => "EFGH".Subsequences(3 - str.Length, 3 - str.Length).Select(ss => str + ss.JoinString())).ToArray();

            Console.WriteLine($"Group 1: {allowableGroup1Combinations.JoinString(" // ")} = {allowableGroup1Combinations.Length}");
            Console.WriteLine($"Group 2: {allowableGroup2Combinations.JoinString(" // ")} = {allowableGroup2Combinations.Length}");

            var rnd = new Random(47);
            List<((char symbol, int orientation) top, (char symbol, int orientation) left, (char symbol, int orientation) front)> generateCubes(char[][] combinations) =>
                combinations.Select(combination => ((combination[0], rnd.Next(0, 4)), (combination[1], rnd.Next(0, 4)), (combination[2], rnd.Next(0, 4)))).ToList();

            foreach (var (startTag, endTag, combinations) in new[] { (@"<!-- g1-s -->", @"<!-- g1-e -->", allowableGroup1Combinations), (@"<!-- g2-s -->", @"<!-- g2-e -->", allowableGroup2Combinations) })
                Utils.ReplaceInFile(@"D:\c\KTANE\Public\HTML\Pattern Cube.html", startTag, endTag,
                    generateCubes(combinations.Select(c => c.ToArray().Shuffle(rnd)).ToArray().Shuffle(rnd)).ToList().Shuffle(rnd).Select(cube => $@"
                        <div class='cube-box highlightable'>
                            <div class='cube-rotation'>
                                <div class='face front symbol-{cube.front.symbol} or-{cube.front.orientation}'></div>
                                <div class='face left symbol-{cube.left.symbol} or-{cube.left.orientation}'></div>
                                <div class='face top symbol-{cube.top.symbol} or-{cube.top.orientation}'></div>
                            </div>
                        </div>").Split(6).Select(chunk => $@"
                    <div class='cube-row'>{chunk.JoinString()}
                    </div>").JoinString("\n"));

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
                results.Add(sb.ToString());
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