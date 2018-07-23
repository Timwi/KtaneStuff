using System;
using System.Collections.Generic;
using System.IO;
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
            public char Symbol { get; private set; }
            public int Orientation { get; private set; }
            public bool Equals(FaceSymbol other) => other != null && other.Symbol == Symbol && other.Orientation == Orientation;
            public FaceSymbol(char symbol, int orientation) { Symbol = symbol; Orientation = orientation; }
        }
        sealed class HalfCube
        {
            public FaceSymbol Top { get; private set; }
            public FaceSymbol Left { get; private set; }
            public FaceSymbol Front { get; private set; }
            public HalfCube(FaceSymbol top, FaceSymbol left, FaceSymbol front) { Top = top; Left = left; Front = front; }

            public static HalfCube Random(char[] combination, Random rnd) =>
                new HalfCube(new FaceSymbol(combination[0], rnd.Next(0, 4)), new FaceSymbol(combination[1], rnd.Next(0, 4)), new FaceSymbol(combination[2], rnd.Next(0, 4)));
        }
        sealed class FaceInfo : IEquatable<FaceInfo>
        {
            public int Face { get; private set; }
            public int Orientation { get; private set; }
            public bool Equals(FaceInfo other) => other != null && other.Face == Face && other.Orientation == Orientation;
            public FaceInfo(int face, int orientation) { Face = face; Orientation = orientation; }
        }
        sealed class Net : IEquatable<Net>
        {
            public FaceInfo[,] Faces { get; private set; }
            public Net(FaceInfo[,] faces) { Faces = faces; }
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

            public string ToString(FaceSymbol[] faceSymbols, bool[] showSymbol, bool[] showOrientation)
            {
                if (faceSymbols == null || faceSymbols.Length != 6 || showSymbol == null || showSymbol.Length != 6 || showOrientation == null || showOrientation.Length != 6)
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
                            sb.Append(Faces[fx, fy] == null ? "    " : $" {(showSymbol[Faces[fx, fy].Face] ? faceSymbols[Faces[fx, fy].Face].Symbol : ' ')}{"NESW "[showOrientation[Faces[fx, fy].Face] ? (faceSymbols[Faces[fx, fy].Face].Orientation + Faces[fx, fy].Orientation) % 4 : 4]} ");
                    }
                    sb.Append("\n");
                }
                return sb.ToString();
            }

            public string Code
            {
                get
                {
                    return $"new Net(new FaceInfo[{Faces.GetLength(0)}, {Faces.GetLength(1)}] {{ {Enumerable.Range(0, Faces.GetLength(0)).Select(x => $@"{{ {Enumerable.Range(0, Faces.GetLength(1)).Select(y => Faces[x, y] == null ? "null" : $"new FaceInfo({Faces[x, y].Face}, {Faces[x, y].Orientation})").JoinString(", ")} }}").JoinString(", ")} }})";
                }
            }
        }

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

            var netStrs = new HashSet<string>();
            foreach (var nt in nets)
                netStrs.Add(Enumerable.Range(0, nt.Faces.GetLength(1)).Select(y => Enumerable.Range(0, nt.Faces.GetLength(0)).Select(x => nt.Faces[x, y] == null ? " " : "#").JoinString()).JoinString("\n"));
            File.WriteAllText(@"D:\Daten\Upload\KTANE\Pattern Cube nets.txt", netStrs.JoinString("\n\n"));
            Console.WriteLine(netStrs.Count);

            // Generate diagrams in the manual
            var allowableGroup1Combinations = ",X,Y,XY,XZ,XYZ".Split(',').SelectMany(str => "ABCD".Subsequences(3 - str.Length, 3 - str.Length).Select(ss => str + ss.JoinString())).ToArray();
            var allowableGroup2Combinations = ",X,Z,XY,YZ,XYZ".Split(',').SelectMany(str => "EFGH".Subsequences(3 - str.Length, 3 - str.Length).Select(ss => str + ss.JoinString())).ToArray();

            var rnd = new Random(47);
            List<HalfCube> generateCubes(char[][] combinations) =>
                combinations.Select(combination => HalfCube.Random(combination, rnd)).ToList();

            var group1Arrangements = generateCubes(allowableGroup1Combinations.Select(c => c.ToArray().Shuffle(rnd)).ToArray().Shuffle(rnd)).ToList().Shuffle(rnd);
            var group2Arrangements = generateCubes(allowableGroup2Combinations.Select(c => c.ToArray().Shuffle(rnd)).ToArray().Shuffle(rnd)).ToList().Shuffle(rnd);

            foreach (var (startTag, endTag, arrangements) in new[] { (@"<!-- g1-s -->", @"<!-- g1-e -->", group1Arrangements), (@"<!-- g2-s -->", @"<!-- g2-e -->", group2Arrangements) })
                Utils.ReplaceInFile(@"D:\c\KTANE\Public\HTML\Pattern Cube.html", startTag, endTag,
                    arrangements.Select(cube =>
                        $"<div class='cube-box highlightable'><div class='cube-rotation'>" +
                            $"<div class='face front symbol-{cube.Front.Symbol} or-{cube.Front.Orientation}'></div>" +
                            $"<div class='face left symbol-{cube.Left.Symbol} or-{cube.Left.Orientation}'></div>" +
                            $"<div class='face top symbol-{cube.Top.Symbol} or-{cube.Top.Orientation}'></div>" +
                        $"</div></div>").Split(6).Select(chunk => $@"<div class='cube-row'>{chunk.JoinString()}</div>").JoinString("\n"));

            // Generate code file
            var path = @"D:\c\KTANE\PatternCube\Assets\Data.cs";
            Utils.ReplaceInFile(path, "/*Diag-g1-start*/", "/*Diag-g1-end*/", $@"@""{group1Arrangements.Select(halfCube => new[] { halfCube.Top, halfCube.Left, halfCube.Front }.Select(face => face.Symbol + "" + face.Orientation).JoinString(",")).JoinString(";")}""");
            Utils.ReplaceInFile(path, "/*Diag-g2-start*/", "/*Diag-g2-end*/", $@"@""{group2Arrangements.Select(halfCube => new[] { halfCube.Top, halfCube.Left, halfCube.Front }.Select(face => face.Symbol + "" + face.Orientation).JoinString(",")).JoinString(";")}""");
            Utils.ReplaceInFile(path, "// Nets-start", "// Nets-end", nets.Select(nt => nt.Code).JoinString(",\r\n") + "\r\n");

            // Generate a puzzle
            var puzRnd = new Random(700000);
            var net = nets.PickRandom(puzRnd);
            var faceGivenByLine = Enumerable.Range(0, net.Faces.GetLength(1)).SelectMany(y => Enumerable.Range(0, net.Faces.GetLength(0)).Select(x => (x, y))).First(tup => net.Faces[tup.x, tup.y] != null).Apply(tup => net.Faces[tup.x, tup.y].Face);
            var (arrangement1, arrangement2) = puzRnd.Next(0, 2) == 0 ? (group1Arrangements, group2Arrangements) : (group2Arrangements, group1Arrangements);
            var halfCube1 = arrangement1.PickRandom(puzRnd);
            var symbolsAlready = new[] { halfCube1.Top.Symbol, halfCube1.Left.Symbol, halfCube1.Front.Symbol };
            var halfCube2 = arrangement2.Where(ag => !new[] { ag.Top.Symbol, ag.Left.Symbol, ag.Front.Symbol }.Intersect(symbolsAlready).Any()).PickRandom(puzRnd);
            FaceSymbol right, back, bottom;
            switch (puzRnd.Next(0, 3))
            {
                case 0:
                    back = new FaceSymbol(halfCube2.Front.Symbol, (halfCube2.Front.Orientation + 3) % 4);
                    right = new FaceSymbol(halfCube2.Top.Symbol, (halfCube2.Top.Orientation + 3) % 4);
                    bottom = new FaceSymbol(halfCube2.Left.Symbol, (halfCube2.Left.Orientation + 3) % 4);
                    break;
                case 1:
                    back = new FaceSymbol(halfCube2.Top.Symbol, halfCube2.Top.Orientation);
                    right = new FaceSymbol(halfCube2.Left.Symbol, (halfCube2.Left.Orientation + 1) % 4);
                    bottom = new FaceSymbol(halfCube2.Front.Symbol, halfCube2.Front.Orientation);
                    break;
                default:
                    back = new FaceSymbol(halfCube2.Left.Symbol, (halfCube2.Left.Orientation + 2) % 4);
                    right = new FaceSymbol(halfCube2.Front.Symbol, (halfCube2.Front.Orientation + 2) % 4);
                    bottom = new FaceSymbol(halfCube2.Top.Symbol, (halfCube2.Top.Orientation + 1) % 4);
                    break;
            }
            var faceGivenFully = (new[] { 0, 1, 4 }.Contains(faceGivenByLine) ? new[] { 2, 3, 5 } : new[] { 0, 1, 4 }).PickRandom(puzRnd);
            var allFaces = new[] { halfCube1.Top, halfCube1.Front, right, back, halfCube1.Left, bottom };
            Console.WriteLine(net.ToString(allFaces, Ut.NewArray(6, face => face == faceGivenByLine || face == faceGivenFully), Ut.NewArray(6, face => face == faceGivenFully)));
            Console.WriteLine($"Symbols are: {allFaces.Select(f => f.Symbol).ToArray().Shuffle(puzRnd).JoinString(", ")}");
            Console.WriteLine("Press Enter to view solution");
            Console.ReadLine();
            Console.WriteLine(net.ToString(allFaces, Ut.NewArray(6, _ => true), Ut.NewArray(6, _ => true)));
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
                        arr[sx - minX, sy - minY] = ix == -1 ? null : new FaceInfo(sofar[ix].face, sofar[ix].orientation);
                    }
                nets.Add(new Net(arr));
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