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

        public static void GenerateManualAndCode()
        {
            var allowableGroup1Combinations = ",X,Y,XY,XZ,XYZ".Split(',').SelectMany(str => "ABCD".Subsequences(3 - str.Length, 3 - str.Length).Select(ss => str + ss.JoinString())).ToArray();
            var allowableGroup2Combinations = ",X,Z,XY,YZ,XYZ".Split(',').SelectMany(str => "EFGH".Subsequences(3 - str.Length, 3 - str.Length).Select(ss => str + ss.JoinString())).ToArray();

            Console.WriteLine($"Group 1: {allowableGroup1Combinations.JoinString(" // ")} = {allowableGroup1Combinations.Length}");
            Console.WriteLine($"Group 2: {allowableGroup2Combinations.JoinString(" // ")} = {allowableGroup2Combinations.Length}");

            var rnd = new Random(47);
            List<((char symbol, int orientation) top, (char symbol, int orientation) left, (char symbol, int orientation) front)> generateCubes(char[][] combinations) =>
                combinations.Select(combination => ((combination[0], rnd.Next(0, 4)), (combination[1], rnd.Next(0, 4)), (combination[2], rnd.Next(0, 4)))).ToList();

            var group1Arrangements = generateCubes(allowableGroup1Combinations.Select(c => c.ToArray().Shuffle(rnd)).ToArray().Shuffle(rnd)).ToList().Shuffle(rnd);
            var group2Arrangements = generateCubes(allowableGroup2Combinations.Select(c => c.ToArray().Shuffle(rnd)).ToArray().Shuffle(rnd)).ToList().Shuffle(rnd);

            // Generate diagrams in the manual
            foreach (var (startTag, endTag, arrangements) in new[] { (@"<!-- g1-s -->", @"<!-- g1-e -->", group1Arrangements), (@"<!-- g2-s -->", @"<!-- g2-e -->", group2Arrangements) })
                Utils.ReplaceInFile(@"D:\c\KTANE\Public\HTML\Pattern Cube.html", startTag, endTag,
                    arrangements.Select(cube => $@"
                        <div class='cube-box highlightable'>
                            <div class='cube-rotation'>
                                <div class='face front symbol-{cube.front.symbol} or-{cube.front.orientation}'></div>
                                <div class='face left symbol-{cube.left.symbol} or-{cube.left.orientation}'></div>
                                <div class='face top symbol-{cube.top.symbol} or-{cube.top.orientation}'></div>
                            </div>
                        </div>").Split(6).Select(chunk => $@"
                    <div class='cube-row'>{chunk.JoinString()}
                    </div>").JoinString("\n"));

            // Generate code file
            var path = @"D:\c\KTANE\KtaneStuff\Src\PatternCube.cs"; // TODO: change to module
            Utils.ReplaceInFile(path, "/*Diag-g1-" + "start*/", "/*Diag-g1-" + "end*/", $@"@""{group1Arrangements.Select(arr => new[] { arr.top, arr.left, arr.front }.Select(elem => elem.symbol + "" + elem.orientation).JoinString(",")).JoinString(";")}""");
            Utils.ReplaceInFile(path, "/*Diag-g2-" + "start*/", "/*Diag-g2-" + "end*/", $@"@""{group2Arrangements.Select(arr => new[] { arr.top, arr.left, arr.front }.Select(elem => elem.symbol + "" + elem.orientation).JoinString(",")).JoinString(";")}""");
        }

        sealed class FaceInfo
        {
            public char Symbol;
            public int Orientation;
        }
        sealed class HalfCubeInfo
        {
            public FaceInfo Top, Left, Front;
        }

        private static readonly HalfCubeInfo[] _group1 = /*Diag-g1-start*/@"X2,C2,B1;B0,D1,C2;C2,A3,D1;Z1,D0,X0;X2,C0,A1;A2,C3,Y2;C1,D0,Y2;Y0,D0,X3;X0,Z2,C1;X1,C1,D0;X2,C3,Y0;D3,A0,B3;D1,B2,Y0;Y2,B0,A2;X1,A2,D2;C3,B3,A1;Z1,X3,B1;Y3,C1,B0;X1,A3,B1;Z1,A2,X0;Y0,X1,A1;X1,Y2,B0;A0,Y3,D0;D0,B0,X0"/*Diag-g1-end*/
            .Split(';').Select(hci => hci.Split(',').Select(fc => new FaceInfo { Symbol = fc[0], Orientation = fc[1] - '0' }).ToArray()).Select(arr => new HalfCubeInfo { Top = arr[0], Left = arr[1], Front = arr[2] }).ToArray();
        private static readonly HalfCubeInfo[] _group2 = /*Diag-g2-start*/@"X0,E3,Y1;G2,E2,Z1;G1,X1,F0;F1,Z1,H2;Z3,F1,E2;H1,G1,F1;Z2,H3,G2;H1,E3,F0;G1,Z1,F1;Y3,Z3,E1;Z3,Y3,H1;G0,Z2,Y0;G1,E2,H3;E0,X2,G1;Y1,X3,G3;X3,H1,F0;Z0,H2,E3;E0,F0,G1;H3,X2,E2;G1,H3,X0;X1,E0,F0;X0,F0,Y2;X3,H3,Y0;F1,Y2,Z0"/*Diag-g2-end*/
            .Split(';').Select(hci => hci.Split(',').Select(fc => new FaceInfo { Symbol = fc[0], Orientation = fc[1] - '0' }).ToArray()).Select(arr => new HalfCubeInfo { Top = arr[0], Left = arr[1], Front = arr[2] }).ToArray();

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