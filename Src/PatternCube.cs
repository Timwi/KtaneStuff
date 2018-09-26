using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using KtaneStuff.Modeling;
using RT.Util;
using RT.Util.Drawing;
using RT.Util.ExtensionMethods;
using RT.Util.Geometry;

namespace KtaneStuff
{
    using static Md;

    static class PatternCube
    {
        struct FaceSymbol : IEquatable<FaceSymbol>
        {
            public char Symbol { get; private set; }
            public int Orientation { get; private set; }
            public bool Equals(FaceSymbol other) => other.Symbol == Symbol && other.Orientation == Orientation;
            public FaceSymbol(char symbol, int orientation) { Symbol = symbol; Orientation = orientation; }
            public override int GetHashCode() => Ut.ArrayHash(Symbol, Orientation);
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

        public static void GeneratePngs()
        {
            var tasks = new List<(string from, string to)>();
            //CommandRunner.Run(@"D:\Tools\pngcrf.bat", @"Symbols.png").WithWorkingDirectory(@"D:\c\KTANE\PatternCube\Data").Go();
            using (var bmp = new Bitmap(@"D:\c\KTANE\PatternCube\Data\Symbols.png"))
            {
                var w = bmp.Width / 4;
                var h = bmp.Height / 3;
                for (int y = 0; y < 3; y++)
                {
                    for (int x = 0; x < (y == 2 ? 3 : 4); x++)
                    {
                        var ch = "ABCDEFGHXYZ"[y * 4 + x];
                        var path1 = $@"D:\c\KTANE\PatternCube\Assets\Textures\Symbol{ch}-tmp.png";
                        var path2 = $@"D:\c\KTANE\PatternCube\Assets\Textures\Symbol{ch}.png";
                        GraphicsUtil.DrawBitmap(w, h, g =>
                        {
                            g.Clear(Color.FromArgb(1, 255, 255, 255));
                            g.DrawImage(bmp, -x * w, -y * h);
                        }).Save(path1);
                        tasks.Add((path1, path2));
                    }
                }
            }
            tasks.ParallelForEach(tsk =>
            {
                CommandRunner.Run(@"D:\Tools\pngcr.bat", tsk.from, tsk.to).Go();
                File.Delete(tsk.from);
            });
        }

        sealed class FaceInfo : IEquatable<FaceInfo>
        {
            public int Face { get; private set; }
            public int Orientation { get; private set; }
            public bool Equals(FaceInfo other) => other != null && other.Face == Face && other.Orientation == Orientation;
            public FaceInfo(int face, int orientation) { Face = face; Orientation = orientation; }
            public override int GetHashCode() => Ut.ArrayHash(Face, Orientation);
        }
        sealed class Net : IEquatable<Net>
        {
            public FaceInfo[,] Faces { get; private set; }
            public Net(FaceInfo[,] faces) { Faces = faces ?? throw new ArgumentNullException(nameof(faces)); }
            public bool Equals(Net other)
            {
                if (other.Faces.GetLength(0) != Faces.GetLength(0) || other.Faces.GetLength(1) != Faces.GetLength(1))
                    return false;
                for (int i = Faces.GetLength(0) - 1; i >= 0; i--)
                    for (int j = other.Faces.GetLength(1) - 1; j >= 0; j--)
                        if (Faces[i, j] != null && !Faces[i, j].Equals(other.Faces[i, j]))
                            return false;
                return true;
            }
            public override int GetHashCode() => Ut.ArrayHash(Faces.GetLength(0), Faces.GetLength(1), Faces.Cast<object>().ToArray());

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
                    return $"{Faces.GetLength(0)}|{Faces.GetLength(1)}|{Enumerable.Range(0, Faces.GetLength(1)).Select(y => Enumerable.Range(0, Faces.GetLength(0)).Select(x => Faces[x, y] == null ? "-" : $"{Faces[x, y].Face},{Faces[x, y].Orientation}").JoinString("/")).JoinString(";")}";
                }
            }

            public string ID { get { return Enumerable.Range(0, Faces.GetLength(1)).Select(y => Enumerable.Range(0, Faces.GetLength(0)).Select(x => Faces[x, y] == null ? "_" : "X").JoinString()).JoinString("-"); } }
        }

        private static readonly List<(int fromFace, int toFace, int direction, int orientation)> _transitions;
        static PatternCube()
        {
            _transitions = new List<(int fromFace, int toFace, int direction, int orientation)>
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
            foreach (var (fromFace, toFace, direction, orientation) in _transitions.ToArray())
                _transitions.Add((toFace, fromFace, (direction + 6 - orientation) % 4, (4 - orientation) % 4));
        }

        public static void GenerateManualCodeAndModels()
        {
            // Generate all the nets
            var nets = new HashSet<Net>();
            for (int face0Orientation = 0; face0Orientation < 4; face0Orientation++)
                recurse(nets, new List<(int x, int y, int face, int orientation)> { (0, 0, 0, face0Orientation) });

            foreach (var geom in nets.Select(n => $"{n.Faces.GetLength(0)} × {n.Faces.GetLength(1)}").Distinct())
                Console.WriteLine(geom);

            // Generate diagrams in the manual
            var allowableGroup1Combinations = ",X,Y,XY,XZ".Split(',').SelectMany(str => "ABCD".Subsequences(3 - str.Length, 3 - str.Length).Select(ss => str + ss.JoinString())).ToArray();
            var allowableGroup2Combinations = ",X,Z,XY,YZ".Split(',').SelectMany(str => "EFGH".Subsequences(3 - str.Length, 3 - str.Length).Select(ss => str + ss.JoinString())).ToArray();

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
            Utils.ReplaceInFile(path, "/*Nets-start*/", "/*Nets-end*/", $@"@""{nets.Select(nt => nt.Code).JoinString("&")}""");

            // Generate module backing models for every unique shape of net
            // (outline vertices are sorted clockwise)
            var outline = @"-0.854439,0.15,-0.256271;-0.854439,0.15,-0.854439;-0.256271,0.15,-0.854439;0.256273,0.15,-0.854439;0.364376,0.15,-0.854439;0.444206,0.15,-0.774025;0.443619,0.15,-0.693596;0.524693,0.15,-0.545002;0.672195,0.15,-0.46653;0.784569,0.15,-0.472279;0.861069,0.15,-0.396061;0.854439,0.15,-0.256271;0.854439,0.15,0.256271;0.854439,0.15,0.854439;0.256273,0.15,0.854439;-0.256273,0.15,0.854439;-0.854439,0.15,0.854439;-0.854439,0.15,0.256271"
                .Split(';').Select(str => str.Split(',').Select(v => double.Parse(v)).ToArray()).Select((double[] arr) => pt(arr[0], arr[1], arr[2])).ToArray();

            const double x1 = -.8;
            const double x2 = .8;
            const double y1 = -.8;
            const double y2 = .8;
            const double w = (x2 - x1) / 5.5;
            const double h = (y2 - y1) / 5.5;
            var already = new HashSet<string>();
            foreach (var net in nets)
            {
                if (!already.Add(net.ID))
                    continue;

                var polygons = new List<PointD[]> { outline.Select(pt => p(pt.X, pt.Z)).ToArray() };
                var holeSpacing = (y2 - y1 - 5 * h) / 4;
                for (int i = 0; i < 5; i++)
                {
                    var y = y1 + i * (h + holeSpacing);
                    polygons.Add(new[] { p(x1, y), p(x1, y + h), p(x1 + w, y + h), p(x1 + w, y) });
                }
                var polys = Utils.BoolsToPaths(Ut.NewArray(net.Faces.GetLength(0), net.Faces.GetLength(1), (x, y) => net.Faces[x, y] != null));
                polygons.AddRange(polys.Select(poly => poly.Select(pt => p(x1 + w * (pt.X + 1.5), y1 + h * (pt.Y + 5.5 - net.Faces.GetLength(1)))).Reverse().ToArray()));

                var tri = polygons.Triangulate().Select(poly => poly.Select(p => pt(p.X, .15, p.Y).WithTexture(new PointD(.4771284794 * p.X + .46155, -.4771284794 * p.Y + .5337373145))).Reverse().ToArray()).ToArray();
                File.WriteAllText($@"D:\c\KTANE\PatternCube\Assets\Models\ModuleFront_{net.ID}.obj", GenerateObjFile(tri, $@"ModuleFront_{net.ID}", AutoNormal.Flat));
            }

            File.WriteAllText(@"D:\c\KTANE\PatternCube\Assets\Models\Frame.obj", GenerateObjFile(frame(w), "Frame"));
        }

        private static IEnumerable<VertexInfo[]> frame(double w)
        {
            var xs = new[] { -w / 2, w / 2, w / 2, -w / 2 };
            var ys = new[] { -w / 2, -w / 2, w / 2, w / 2 };
            var coords = Enumerable.Range(0, 4).Reverse().Select(ix => (xs[ix], ys[ix])).Select(m => new (double d, double h)[] { (1, .15), (.95, .155), (.9, .155), (.85, .14) }.Select(tup => pt(m.Item1 * tup.d, tup.h, m.Item2 * tup.d)).ToArray()).ToArray();
            return CreateMesh(true, false, coords, flatNormals: true);
        }

        private static void GeneratePuzzle(HashSet<Net> nets, List<HalfCube> group1Arrangements, List<HalfCube> group2Arrangements)
        {
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

                // Discard nets that are 5×2
                if (maxX - minX == 4)
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
                foreach (var (fromFace, toFace, direction, newOrientation) in _transitions)
                {
                    var ix = sofar.IndexOf(tp => tp.face == fromFace);
                    if (ix == -1)
                        continue;
                    if (sofar.Any(tp => tp.face == toFace))
                        continue;
                    var (x, y, face, oldOrientation) = sofar[ix];
                    var newX = x + _xDelta[(direction + oldOrientation) % 4];
                    var newY = y + _yDelta[(direction + oldOrientation) % 4];
                    if (sofar.Any(tp => tp.x == newX && tp.y == newY))
                        continue;
                    var newSofar = sofar.ToList();
                    newSofar.Add((newX, newY, toFace, (newOrientation + oldOrientation) % 4));
                    recurse(nets, newSofar);
                }
            }
        }
    }
}