using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RT.Util;
using RT.Util.ExtensionMethods;

namespace KtaneStuff
{
    static class PatternCube
    {
        sealed class FaceSymbol : IEquatable<FaceSymbol>
        {
            public char Symbol;
            public int Orientation;
            public bool Equals(FaceSymbol other) => other != null && other.Symbol == Symbol && other.Orientation == Orientation;
        }
        sealed class HalfCube
        {
            public FaceSymbol Top, Left, Front;
        }
        sealed class FaceInfo : IEquatable<FaceInfo>
        {
            public int Face;
            public int Orientation;
            public bool Equals(FaceInfo other) => other != null && other.Face == Face && other.Orientation == Orientation;
        }
        sealed class Net : IEquatable<Net>
        {
            public FaceInfo[,] Faces;

            public bool Equals(Net other)
            {
                if (other == null)
                    return false;
                if ((Faces == null) != (other.Faces == null))
                    return false;
                if (Faces == null)
                    return true;
                if (other.Faces.GetLength(0) != Faces.GetLength(0) || other.Faces.GetLength(1) != Faces.GetLength(1))
                    return false;

                for (int i = Faces.GetLength(0) - 1; i >= 0; i--)
                    for (int j = other.Faces.GetLength(1) - 1; j >= 0; j--)
                        if (!Equals(Faces[i, j], other.Faces[i, j]))
                            return false;
                return true;
            }

            public string ToString((char symbol, int orientation)[] faceInfos, bool[] showSymbol, bool[] showOrientation)
            {
                if (faceInfos == null || faceInfos.Length != 6 || showSymbol == null || showSymbol.Length != 6 || showOrientation == null || showOrientation.Length != 6)
                    throw new ArgumentException();
                var sb = new StringBuilder();
                var w = Faces.GetLength(0);
                var h = Faces.GetLength(1);
                for (int y = 0; y < 2 * h + 1; y++)
                {
                    var fy = y / 2;
                    for (int x = 0; x < 2 * w + 1; x++)
                    {
                        var fx = x / 2;
                        var i = 0;
                        if (fx > 0 && fy > 0 && Faces[fx - 1, fy - 1] != null)
                            i |= 1;
                        if (fx < w && fy > 0 && Faces[fx, fy - 1] != null)
                            i |= 2;
                        if (fx > 0 && fy < h && Faces[fx - 1, fy] != null)
                            i |= 4;
                        if (fx < w && fy < h && Faces[fx, fy] != null)
                            i |= 8;
                        // Corner
                        if (x % 2 == 0 && y % 2 == 0)
                            sb.Append(" ┘└┴┐┤┼┼┌┼├┼┬┼┼┼"[i]);
                        // Horizontal line
                        else if (y % 2 == 0)
                            sb.Append(new string("  ──  ──────────"[i], 4));
                        // Vertical line
                        else if (x % 2 == 0)
                            sb.Append("    ││││││││││││"[i]);
                        // Face
                        else
                            sb.Append(Faces[fx, fy] == null ? "    " : $" {(showSymbol[Faces[fx, fy].Face] ? faceInfos[Faces[fx, fy].Face].symbol : ' ')}{"NESW "[showOrientation[Faces[fx, fy].Face] ? (faceInfos[Faces[fx, fy].Face].orientation + Faces[fx, fy].Orientation) % 4 : 4]} ");
                    }
                    sb.Append("\n");
                }
                return sb.ToString();
            }
        }

        private static readonly HalfCube[] _group1 = /*Diag-g1-start*/@"X2,C2,B1;B0,D1,C2;C2,A3,D1;Z1,D0,X0;X2,C0,A1;A2,C3,Y2;C1,D0,Y2;Y0,D0,X3;X0,Z2,C1;X1,C1,D0;X2,C3,Y0;D3,A0,B3;D1,B2,Y0;Y2,B0,A2;X1,A2,D2;C3,B3,A1;Z1,X3,B1;Y3,C1,B0;X1,A3,B1;Z1,A2,X0;Y0,X1,A1;X1,Y2,B0;A0,Y3,D0;D0,B0,X0"/*Diag-g1-end*/
            .Split(';').Select(hci => hci.Split(',').Select(fc => new FaceSymbol { Symbol = fc[0], Orientation = fc[1] - '0' }).ToArray()).Select(arr => new HalfCube { Top = arr[0], Left = arr[1], Front = arr[2] }).ToArray();
        private static readonly HalfCube[] _group2 = /*Diag-g2-start*/@"X0,E3,Y1;G2,E2,Z1;G1,X1,F0;F1,Z1,H2;Z3,F1,E2;H1,G1,F1;Z2,H3,G2;H1,E3,F0;G1,Z1,F1;Y3,Z3,E1;Z3,Y3,H1;G0,Z2,Y0;G1,E2,H3;E0,X2,G1;Y1,X3,G3;X3,H1,F0;Z0,H2,E3;E0,F0,G1;H3,X2,E2;G1,H3,X0;X1,E0,F0;X0,F0,Y2;X3,H3,Y0;F1,Y2,Z0"/*Diag-g2-end*/
            .Split(';').Select(hci => hci.Split(',').Select(fc => new FaceSymbol { Symbol = fc[0], Orientation = fc[1] - '0' }).ToArray()).Select(arr => new HalfCube { Top = arr[0], Left = arr[1], Front = arr[2] }).ToArray();

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

        public static void GenerateManualAndCode()
        {
            // Generate all the nets
            var nets = new HashSet<Net>();
            for (int face0Orientation = 0; face0Orientation < 4; face0Orientation++)
                recurse(nets, new List<(int x, int y, int face, int orientation)> { (0, 0, 0, face0Orientation) });

            // Generate diagrams in the manual
            var allowableGroup1Combinations = ",X,Y,XY,XZ,XYZ".Split(',').SelectMany(str => "ABCD".Subsequences(3 - str.Length, 3 - str.Length).Select(ss => str + ss.JoinString())).ToArray();
            var allowableGroup2Combinations = ",X,Z,XY,YZ,XYZ".Split(',').SelectMany(str => "EFGH".Subsequences(3 - str.Length, 3 - str.Length).Select(ss => str + ss.JoinString())).ToArray();

            var rnd = new Random(47);
            List<((char symbol, int orientation) top, (char symbol, int orientation) left, (char symbol, int orientation) front)> generateCubes(char[][] combinations) =>
                combinations.Select(combination => ((combination[0], rnd.Next(0, 4)), (combination[1], rnd.Next(0, 4)), (combination[2], rnd.Next(0, 4)))).ToList();

            var group1Arrangements = generateCubes(allowableGroup1Combinations.Select(c => c.ToArray().Shuffle(rnd)).ToArray().Shuffle(rnd)).ToList().Shuffle(rnd);
            var group2Arrangements = generateCubes(allowableGroup2Combinations.Select(c => c.ToArray().Shuffle(rnd)).ToArray().Shuffle(rnd)).ToList().Shuffle(rnd);

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
            //var path = @"D:\c\KTANE\KtaneStuff\Src\PatternCube.cs"; // TODO: change to module
            //Utils.ReplaceInFile(path, "/*Diag-g1-" + "start*/", "/*Diag-g1-" + "end*/", $@"@""{group1Arrangements.Select(arr => new[] { arr.top, arr.left, arr.front }.Select(elem => elem.symbol + "" + elem.orientation).JoinString(",")).JoinString(";")}""");
            //Utils.ReplaceInFile(path, "/*Diag-g2-" + "start*/", "/*Diag-g2-" + "end*/", $@"@""{group2Arrangements.Select(arr => new[] { arr.top, arr.left, arr.front }.Select(elem => elem.symbol + "" + elem.orientation).JoinString(",")).JoinString(";")}""");

            // Generate a puzzle
            var puzRnd = new Random(6);
            var net = nets.PickRandom(puzRnd);
            var faceGivenByLine = Enumerable.Range(0, net.Faces.GetLength(1)).SelectMany(y => Enumerable.Range(0, net.Faces.GetLength(0)).Select(x => (x, y))).First(tup => net.Faces[tup.x, tup.y] != null).Apply(tup => net.Faces[tup.x, tup.y].Face);
            var (arrangement1, arrangement2) = puzRnd.Next(0, 2) == 0 ? (group1Arrangements, group2Arrangements) : (group2Arrangements, group1Arrangements);
            var (top, left, front) = arrangement1.PickRandom(puzRnd);
            var symbolsAlready = new[] { top.symbol, left.symbol, front.symbol };
            var (fa, fb, fc) = arrangement2.Where(ag => !new[] { ag.top.symbol, ag.left.symbol, ag.front.symbol }.Intersect(symbolsAlready).Any()).PickRandom(puzRnd);
            (char symbol, int orientation) right, back, bottom;
            switch (puzRnd.Next(0, 3))
            {
                case 0:
                    back = (fc.symbol, (fc.orientation + 3) % 4);
                    right = (fa.symbol, (fa.orientation + 3) % 4);
                    bottom = (fb.symbol, (fb.orientation + 3) % 4);
                    break;
                case 1:
                    back = (fa.symbol, fa.orientation);
                    right = (fb.symbol, (fb.orientation + 1) % 4);
                    bottom = (fc.symbol, fc.orientation);
                    break;
                default:
                    back = (fb.symbol, (fb.orientation + 2) % 4);
                    right = (fc.symbol, (fc.orientation + 2) % 4);
                    bottom = (fa.symbol, (fa.orientation + 1) % 4);
                    break;
            }
            var faceGivenFully = (new[] { 0, 1, 4 }.Contains(faceGivenByLine) ? new[] { 2, 3, 5 } : new[] { 0, 1, 4 }).PickRandom(puzRnd);
            var allFaces = new[] { top, front, right, back, left, bottom };
            Console.WriteLine(net.ToString(allFaces, Ut.NewArray(6, face => face == faceGivenByLine || face == faceGivenFully), Ut.NewArray(6, face => face == faceGivenFully)));
            Console.WriteLine($"Symbols are: {allFaces.Select(f => f.symbol).ToArray().Shuffle(puzRnd).JoinString(", ")}");
            Console.WriteLine("Press Enter to view solution");
            Console.ReadLine();
            Console.WriteLine(net.ToString(new[] { top, front, right, back, left, bottom }, Ut.NewArray(6, _ => true), Ut.NewArray(6, _ => true)));
        }

        private static readonly int[] _xDelta = new[] { 0, 1, 0, -1 };
        private static readonly int[] _yDelta = new[] { -1, 0, 1, 0 };

        private static void recurse(HashSet<Net> nets, List<(int x, int y, int face, int orientation)> sofar)
        {
            if (sofar.Count == 6)
            {
                var minX = sofar.Min(k => k.x);
                var maxX = sofar.Max(k => k.x);
                var minY = sofar.Min(k => k.y);
                var maxY = sofar.Max(k => k.y);

                // Discard nets that have four faces in a 2×2 square
                for (int sx = minX; sx <= maxX; sx++)
                    for (int sy = minY; sy <= maxY; sy++)
                        if (sofar.Any(t => t.x == sx && t.y == sy) && sofar.Any(t => t.x == sx + 1 && t.y == sy) && sofar.Any(t => t.x == sx && t.y == sy + 1) && sofar.Any(t => t.x == sx + 1 && t.y == sy + 1))
                            return;

                var arr = new FaceInfo[maxX - minX + 1, maxY - minY + 1];
                for (int sy = minY; sy <= maxY; sy++)
                    for (int sx = minX; sx <= maxX; sx++)
                    {
                        var ix = sofar.IndexOf(tuple => tuple.x == sx && tuple.y == sy);
                        arr[sx - minX, sy - minY] = ix == -1 ? null : new FaceInfo { Face = sofar[ix].face, Orientation = sofar[ix].orientation };
                    }
                nets.Add(new Net { Faces = arr });
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
                    recurse(nets, newSofar);
                }
            }
        }
    }
}