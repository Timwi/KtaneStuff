using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using RT.Util;
using RT.Util.Collections;
using RT.Util.Consoles;
using RT.Util.Drawing;
using RT.Util.ExtensionMethods;
using RT.Util.Geometry;
using RT.Util.Serialization;

namespace KtaneStuff
{
    static partial class Ktane
    {
        public enum Marking
        {
            None,
            Circle,
            SquareNS,
            SquareNeSw,
            SquareNwSe,
            TriangleUp,
            TriangleDown,
            TriangleLeft,
            TriangleRight
        }

        private static Marking[] _markingsSquare = new[] { Marking.SquareNeSw, Marking.SquareNS, Marking.SquareNwSe };
        private static Marking[] _markingsTriangle1 = new[] { Marking.TriangleUp, Marking.TriangleDown };
        private static Marking[] _markingsTriangle2 = new[] { Marking.TriangleLeft, Marking.TriangleRight };
        public static Marking Rotate(this Marking marking, int rotation)
        {
            switch (marking)
            {
                case Marking.None: return Marking.None;
                case Marking.Circle: return Marking.Circle;

                case Marking.SquareNeSw:
                case Marking.SquareNwSe:
                case Marking.SquareNS:
                    return _markingsSquare[(_markingsSquare.IndexOf(marking) + (rotation % 6 + 6) % 6) % 3];

                case Marking.TriangleUp:
                case Marking.TriangleDown:
                    return _markingsTriangle1[(_markingsTriangle1.IndexOf(marking) + (rotation % 6 + 6) % 6) % 2];

                case Marking.TriangleLeft:
                case Marking.TriangleRight:
                    return _markingsTriangle2[(_markingsTriangle2.IndexOf(marking) + (rotation % 6 + 6) % 6) % 2];

                default:
                    throw new ArgumentException("Invalid marking.", nameof(marking));
            }
        }
        public static Marking Mirror(this Marking marking, bool doMirror)
        {
            if (!doMirror)
                return marking;
            switch (marking)
            {
                case Marking.SquareNeSw: return Marking.SquareNwSe;
                case Marking.SquareNwSe: return Marking.SquareNeSw;
                case Marking.TriangleUp: return Marking.TriangleDown;
                case Marking.TriangleDown: return Marking.TriangleUp;
                default: return marking;
            }
        }
        public static string ToMarkingString(this IEnumerable<Marking> markings) => markings.Select(m => m == Marking.None ? "•" : ((int) m).ToString()).JoinString();

        public sealed class HexamazeInfo : ICloneable
        {
            public int Size = 12;
            public int SubmazeSize = 12;
            public Dictionary<Hex, bool[]> Walls = new Dictionary<Hex, bool[]>();
            public Dictionary<Hex, Marking> Markings = new Dictionary<Hex, Marking>();
            public bool HasWall(Hex hex, int n) => Walls.Get(n < 3 ? hex : hex.Neighbors[n], new[] { false, false, false })[n % 3];

            public string CreateSvg(Tuple<IEnumerable<Hex>, string>[] fills = null)
            {
                double hexWidth = 72;

                var fillsSvg = new StringBuilder();
                var nonWallsSvg = new StringBuilder();
                var wallsSvg = new StringBuilder();
                var markingsSvg = new StringBuilder();

                if (fills != null)
                    foreach (var fill in fills)
                        foreach (var fillHex in fill.Item1)
                            fillsSvg.Append($"<polygon style='fill:#{fill.Item2}' points='{fillHex.GetPolygon(hexWidth).Select(p => $"{p.X},{p.Y}").JoinString(" ")}' />");

                foreach (var hex in Hex.LargeHexagon(Size + 1))
                {
                    var poly = hex.GetPolygon(hexWidth);
                    for (int n = 0; n < 3; n++)
                        if (((hex.Distance < Size || hex.Neighbors[n].Distance < Size)) && Walls[hex][n])
                            (Walls[hex][n] ? wallsSvg : nonWallsSvg).Append($"<line class='{(Walls[hex][n] ? "wall" : "no-wall")}' x1='{poly[n].X}' y1='{poly[n].Y}' x2='{poly[n + 1].X}' y2='{poly[n + 1].Y}' />");
                    if (hex.Distance < Size)
                    {
                        var p = hex.GetCenter(hexWidth);
                        markingsSvg.Append($"<circle class='marking none' cx='{p.X}' cy='{p.Y}' r='{hexWidth / 12}' />");
                        markingsSvg.Append(Markings.Get(hex, Marking.None).getMarkingSvg(hexWidth, p.X, p.Y, useCssClass: true));
                    }
                }

                var w = Hex.LargeWidth(Size) * hexWidth;
                var h = Hex.LargeHeight(Size) * hexWidth * Hex.WidthToHeight;
                return $@"<svg class='hexamaze' viewBox='{-w / 2 - 5} {-h / 2 - 5} {w + 10} {h + 10}'>{fillsSvg}{nonWallsSvg}{wallsSvg}{markingsSvg}</svg>";
            }

            public HexamazeInfo Clone(bool skipMarkings = false)
            {
                return new HexamazeInfo
                {
                    Markings = skipMarkings ? null : Markings.ToDictionary(),
                    Size = Size,
                    SubmazeSize = SubmazeSize,
                    Walls = Walls.Select(kvp => Ut.KeyValuePair(kvp.Key, kvp.Value.ToArray())).ToDictionary()
                };
            }

            object ICloneable.Clone()
            {
                return Clone();
            }
        }

        private static string getMarkingSvg(this Marking marking, double hexWidth, double x, double y, bool useCssClass = false, bool useAttributes = false, double strokeWidth = 2, bool includeDot = false)
        {
            var attr = useAttributes ? $" fill='none' stroke='#f0faff' stroke-width='{strokeWidth}'" : null;
            var dot = includeDot ? $"<circle{(useAttributes ? " fill='#f0faff'" : null)}{(useCssClass ? " class='dot'" : null)} cx='{x}' cy='{y}' r='{hexWidth / 20}' />" : null;
            switch (marking)
            {
                case Marking.Circle:
                    return $"{dot}<circle{attr}{(useCssClass ? " class='marking circle'" : null)} cx='{x}' cy='{y}' r='{hexWidth / 4}' />";
                case Marking.SquareNS:
                    return $"{dot}<polygon{attr}{(useCssClass ? " class='marking square-ns'" : null)} points='{Enumerable.Range(0, 4).Select(i => (i + .5) * Math.PI / 2).Select(angle => $"{x + hexWidth * .3 * Math.Cos(angle)},{y + hexWidth * .3 * Math.Sin(angle)}").JoinString(" ")}' />";
                case Marking.SquareNeSw:
                    return $"{dot}<polygon{attr}{(useCssClass ? " class='marking square-ne-sw'" : null)} points='{Enumerable.Range(0, 4).Select(i => (i - 1.0 / 6) * Math.PI / 2).Select(angle => $"{x + hexWidth * .3 * Math.Cos(angle)},{y + hexWidth * .3 * Math.Sin(angle)}").JoinString(" ")}' />";
                case Marking.SquareNwSe:
                    return $"{dot}<polygon{attr}{(useCssClass ? " class='marking square-nw-se'" : null)} points='{Enumerable.Range(0, 4).Select(i => (i + 1.0 / 6) * Math.PI / 2).Select(angle => $"{x + hexWidth * .3 * Math.Cos(angle)},{y + hexWidth * .3 * Math.Sin(angle)}").JoinString(" ")}' />";
                case Marking.TriangleUp:
                    return $"{dot}<polygon{attr}{(useCssClass ? " class='marking square-nw-se'" : null)} points='{Enumerable.Range(0, 3).Select(i => (i + .25) * 2 * Math.PI / 3).Select(angle => $"{x + hexWidth * .325 * Math.Cos(angle)},{y + hexWidth * .325 * Math.Sin(angle)}").JoinString(" ")}' />";
                case Marking.TriangleDown:
                    return $"{dot}<polygon{attr}{(useCssClass ? " class='marking square-nw-se'" : null)} points='{Enumerable.Range(0, 3).Select(i => (i + .75) * 2 * Math.PI / 3).Select(angle => $"{x + hexWidth * .325 * Math.Cos(angle)},{y + hexWidth * .325 * Math.Sin(angle)}").JoinString(" ")}' />";
                case Marking.TriangleLeft:
                    return $"{dot}<polygon{attr}{(useCssClass ? " class='marking square-nw-se'" : null)} points='{Enumerable.Range(0, 3).Select(i => (i + .5) * 2 * Math.PI / 3).Select(angle => $"{x + hexWidth * .325 * Math.Cos(angle)},{y + hexWidth * .325 * Math.Sin(angle)}").JoinString(" ")}' />";
                case Marking.TriangleRight:
                    return $"{dot}<polygon{attr}{(useCssClass ? " class='marking square-nw-se'" : null)} points='{Enumerable.Range(0, 3).Select(i => i * 2 * Math.PI / 3).Select(angle => $"{x + hexWidth * .325 * Math.Cos(angle)},{y + hexWidth * .325 * Math.Sin(angle)}").JoinString(" ")}' />";
            }
            return dot;
        }

        public static HexamazeInfo GenerateHexamaze()
        {
            const int mazeSize = 12;
            const int smallMazeSize = 4;

            var rnd = new Random(26);   // This random seed gives the most walls (all but 501)
            var totalWallsRemoved = 0;
            var walls = new AutoDictionary<Hex, bool[]>(_ => new bool[3] { true, true, true });
            var removeWall = Ut.Lambda((Hex hex, int n) => { walls[n < 3 ? hex : hex.Neighbors[n]][n % 3] = false; totalWallsRemoved++; });
            var hasWall = Ut.Lambda((Hex hex, int n) => walls[n < 3 ? hex : hex.Neighbors[n]][n % 3]);
            var stack = new Stack<Hex>();
            Hex curHex = new Hex(0, 0);
            stack.Push(curHex);
            var taken = new HashSet<Hex> { curHex };
            var markings = new HashSet<Hex>();

            // Step 1: generate a single giant maze
            while (true)
            {
                var neighbors = curHex.Neighbors;
                var availableNeighborIndices = neighbors.SelectIndexWhere(n => !taken.Contains(n) && n.Distance < mazeSize).ToArray();
                if (availableNeighborIndices.Length == 0)
                {
                    if (stack.Count == 0)
                        break;
                    curHex = stack.Pop();
                    continue;
                }
                var nextNeighborIndex = availableNeighborIndices[rnd.Next(availableNeighborIndices.Length)];
                removeWall(curHex, nextNeighborIndex);
                stack.Push(curHex);
                curHex = neighbors[nextNeighborIndex];
                taken.Add(curHex);
            }

            // Step 2: Go through all submazes and make sure they’re all connected and all have at least one exit on each side
            while (true)
            {
                var candidateCounts = new Dictionary<Tuple<Hex, int>, int>();

                foreach (var centerHex in Hex.LargeHexagon(mazeSize - smallMazeSize + 1))
                {
                    var filled = new HashSet<Hex> { centerHex };
                    var queue = filled.ToQueue();
                    var edgesReachable = new bool[6];

                    // Flood-fill as much of the maze as possible
                    while (queue.Count > 0)
                    {
                        var hex = queue.Dequeue();
                        var ns = hex.Neighbors;
                        for (int n = 0; n < 6; n++)
                        {
                            var offset = ns[n] - centerHex;
                            if (offset.Distance < smallMazeSize && !hasWall(hex, n) && filled.Add(ns[n]))
                                queue.Enqueue(ns[n]);
                            if (offset.Distance == smallMazeSize && !hasWall(hex, n))
                                foreach (var edge in offset.GetEdges(smallMazeSize))
                                    edgesReachable[edge] = true;
                        }
                    }

                    var isHexAllFilled = filled.Count >= 3 * smallMazeSize * (smallMazeSize - 1) + 1;
                    var areAllEdgesReachable = !edgesReachable.Contains(false);

                    if (!isHexAllFilled || !areAllEdgesReachable)
                    {
                        // Consider removing a random wall
                        var candidates1 = filled.SelectMany(fh => fh.Neighbors.Select((th, n) => new { FromHex = fh, Direction = n, ToHex = th, Offset = th - centerHex }))
                            .Where(inf =>
                                (inf.Offset.Distance < smallMazeSize && hasWall(inf.FromHex, inf.Direction) && !filled.Contains(inf.ToHex)) ||
                                (inf.Offset.Distance == smallMazeSize && inf.Offset.GetEdges(smallMazeSize).Any(e => !edgesReachable[e])))
                            .ToArray();
                        foreach (var candidate in candidates1)
                            candidateCounts.IncSafe(Tuple.Create(candidate.Direction < 3 ? candidate.FromHex : candidate.ToHex, candidate.Direction % 3));
                        if (candidates1[0].Offset.Distance < smallMazeSize)
                        {
                            filled.Add(candidates1[0].ToHex);
                            queue.Enqueue(candidates1[0].ToHex);
                        }
                        else
                            foreach (var edge in candidates1[0].Offset.GetEdges(smallMazeSize))
                                edgesReachable[edge] = true;
                    }
                }

                if (candidateCounts.Count == 0)
                    break;

                //*
                // Remove one wall out of the “most wanted”
                var topScores = candidateCounts.Values.Distinct().Order().TakeLast(1).ToArray();
                var candidates2 = candidateCounts.Where(kvp => topScores.Contains(kvp.Value)).ToArray();
                var randomCandidate = candidates2[rnd.Next(candidates2.Length)];
                removeWall(randomCandidate.Key.Item1, randomCandidate.Key.Item2);
                /*/
                // Remove any one wall
                var candidates2 = candidateCounts.Keys.ToArray();
                var randomCandidate = candidates2[rnd.Next(candidates2.Length)];
                removeWall(randomCandidate.Item1, randomCandidate.Item2);
                /**/

                Console.Write($"Walls removed: {totalWallsRemoved}  \r");
            }

            // Step 3: Put as many walls back in as possible
            var missingWalls = walls.SelectMany(kvp => kvp.Value.Select((w, i) => new { Hex = kvp.Key, Index = i, IsWall = w })).Where(inf => !inf.IsWall).ToList();
            while (missingWalls.Count > 0)
            {
                var randomMissingWallIndex = rnd.Next(missingWalls.Count);
                var randomMissingWall = missingWalls[randomMissingWallIndex];
                missingWalls.RemoveAt(randomMissingWallIndex);
                walls[randomMissingWall.Hex][randomMissingWall.Index] = true;

                bool possible = true;
                foreach (var centerHex in Hex.LargeHexagon(mazeSize - smallMazeSize + 1))
                {
                    var filled = new HashSet<Hex> { centerHex };
                    var queue = filled.ToQueue();
                    var edgesReachable = new bool[6];

                    // Flood-fill as much of the maze as possible
                    while (queue.Count > 0)
                    {
                        var hex = queue.Dequeue();
                        var ns = hex.Neighbors;
                        for (int n = 0; n < 6; n++)
                        {
                            var offset = ns[n] - centerHex;
                            if (offset.Distance < smallMazeSize && !hasWall(hex, n) && filled.Add(ns[n]))
                                queue.Enqueue(ns[n]);
                            if (offset.Distance == smallMazeSize && !hasWall(hex, n))
                                foreach (var edge in offset.GetEdges(smallMazeSize))
                                    edgesReachable[edge] = true;
                        }
                    }

                    if (filled.Count < 3 * smallMazeSize * (smallMazeSize - 1) + 1 || edgesReachable.Contains(false))
                    {
                        // This wall cannot be added, take it back out.
                        walls[randomMissingWall.Hex][randomMissingWall.Index] = false;
                        possible = false;
                        break;
                    }
                }

                if (possible)
                {
                    totalWallsRemoved--;
                    Console.Write($"Walls removed: {totalWallsRemoved}  \r");
                }
            }

            Console.WriteLine();

            return new HexamazeInfo
            {
                Size = mazeSize,
                SubmazeSize = smallMazeSize,
                Markings = markings.Select((h, ix) => new { Hex = h, Index = ix }).ToDictionary(inf => inf.Hex, inf => (Marking) (inf.Index)),
                Walls = walls.ToDictionary()
            };
        }

        public static HexamazeInfo GenerateHexamazeMarkings(HexamazeInfo maze)
        {
            var rnd = new Random(192);  // This seed gives the fewest markings (18)

            maze = maze.Clone();
            var size = maze.Size;
            var smallSize = maze.SubmazeSize;

            maze.Markings = new Dictionary<Hex, Marking>();

            // Step 1: Put random markings in until there are no more ambiguities
            while (!areMarkingsUnique(maze))
            {
                var availableHexes = Hex.LargeHexagon(size).Where(h => !maze.Markings.ContainsKey(h)).ToArray();
                var randomHex = availableHexes[rnd.Next(availableHexes.Length)];
                maze.Markings[randomHex] = (Marking) (rnd.Next(6) + 1);
            }

            // Step 2: Find markings to remove again
            var removableMarkings = maze.Markings.ToList();
            while (removableMarkings.Count > 0)
            {
                var tryRemoveIndex = rnd.Next(removableMarkings.Count);
                var tryRemove = removableMarkings[tryRemoveIndex];
                removableMarkings.RemoveAt(tryRemoveIndex);
                maze.Markings.Remove(tryRemove.Key);
                if (!areMarkingsUnique(maze))
                {
                    // No longer unique — put it back in
                    maze.Markings.Add(tryRemove.Key, tryRemove.Value);
                }
            }

            return maze;
        }

        private static bool areMarkingsUnique(HexamazeInfo maze)
        {
            var ambig = 1;
            var size = maze.Size;
            var smallSize = maze.SubmazeSize;
            var unique = new Dictionary<string, List<Tuple<Hex, int>>>();
            foreach (var centerHex in Hex.LargeHexagon(size - smallSize + 1))
            {
                for (int rotation = 0; rotation < 6; rotation++)
                {
                    var markingsStr = Hex.LargeHexagon(smallSize).Select(h => h.Rotate(rotation) + centerHex).Select(h => maze.Markings.Get(h, Marking.None).Rotate(-rotation)).ToMarkingString();
                    List<Tuple<Hex, int>> uniqs;
                    if (unique.TryGetValue(markingsStr, out uniqs) && uniqs.Count > 0)
                    {
                        var fills1 = Tuple.Create(Hex.LargeHexagon(smallSize).Select(h => h.Rotate(uniqs[0].Item2) + uniqs[0].Item1), "fed");
                        var fills2 = Tuple.Create(Hex.LargeHexagon(smallSize).Select(h => h.Rotate(rotation) + centerHex), "def");
                        ambig++;
                        File.WriteAllText($@"D:\c\KTANE\HTML\Hexamaze{ambig}.html", Regex.Replace(File.ReadAllText(@"D:\c\KTANE\HTML\Hexamaze.html"), @"\A(.*<!--##-->\s*).*?(?=\s*<!--###-->)", options: RegexOptions.Singleline, evaluator: m => m.Groups[1].Value + maze.CreateSvg(new[] { fills1, fills2 })));
                        //return false;
                    }
                    unique.AddSafe(markingsStr, Tuple.Create(centerHex, rotation));
                }
            }

            //File.WriteAllText(path, Regex.Replace(File.ReadAllText(path), @"\A(.*<!--##-->\s*).*?(?=\s*<!--###-->)", options: RegexOptions.Singleline, evaluator: m => m.Groups[1].Value + maze.CreateSvg()));

            foreach (var nonUnique in unique.Where(k => k.Value.Count > 1))
                Console.WriteLine($"{nonUnique.Key} = {nonUnique.Value.Select(tup => $"{tup.Item1}/{(6 - tup.Item2) % 6}").JoinString(", ")}");
            return unique.All(kvp => kvp.Value.Count <= 1);
        }

        private static bool reallocateMarkings(string jsonFile, HexamazeInfo maze)
        {
            // Seed 808 gives my favourite arrangement so far
            for (int seed = 808; seed < 1000; seed++)
            {
                if (seed % 10 == 0)
                    Console.WriteLine($"Seed {seed}");
                var rnd = new Random(seed);
                var alloc = new[] {
                    Marking.Circle, Marking.Circle, Marking.Circle, Marking.Circle,
                    Marking.SquareNeSw, Marking.SquareNeSw, Marking.SquareNS, Marking.SquareNS,
                    Marking.SquareNwSe, Marking.SquareNwSe,
                    Marking.TriangleDown, Marking.TriangleDown, Marking.TriangleLeft, Marking.TriangleLeft,
                    Marking.TriangleRight, Marking.TriangleRight, Marking.TriangleUp, Marking.TriangleUp
                }.Shuffle(rnd);
                var markings = maze.Markings.Keys.ToArray();
                for (int i = 0; i < markings.Length; i++)
                    maze.Markings[markings[i]] = alloc[i];
                if (areMarkingsUnique(maze))
                    return true;
            }
            return false;
        }

        public static void GenerateMarkingsSvg()
        {
            var declarations = new List<string>();
            string lineDeclaration = null;
            var pngcrs = new List<Thread>();
            foreach (var obj in EnumStrong.GetValues<Marking>().Cast<object>().Concat("Line"))
            {
                var marking = obj as Marking?;
                var name = obj.ToString();

                File.WriteAllText($@"D:\temp\temp.svg", marking != null
                    ? $"<svg xmlns='http://www.w3.org/2000/svg' viewBox='-.5 -.5 1 1'>{marking.Value.getMarkingSvg(1, 0, 0, useAttributes: true, strokeWidth: .02, includeDot: true)}</svg>"
                    : $"<svg xmlns='http://www.w3.org/2000/svg' viewBox='-.75 -.75 1.5 1.5'>{new Hex(0, 0).GetPolygon(1).Apply(ps => $"<line x1='{ps[0].X}' y1='{ps[0].Y}' x2='{ps[1].X}' y2='{ps[1].Y}' stroke-width='.1' stroke='#f0faff' stroke-linecap='round' />")}</svg>");
                CommandRunner.RunRaw($@"D:\Inkscape\InkscapePortable.exe -z -f D:\temp\temp.svg -w {(marking == null ? 150 : 100)} -e D:\temp\temp-{name}.png").Go();
                var thr = new Thread(() =>
                {
                    CommandRunner.RunRaw($@"pngcr D:\temp\temp-{name}.png D:\temp\temp-{name}.cr.png").Go();
                    if (marking != null)
                        lock (declarations)
                            declarations.Add($@"{{ Marking.{name}, new byte[] {{ {File.ReadAllBytes($@"D:\temp\temp-{name}.cr.png").JoinString(", ")} }} }}");
                    else
                        lineDeclaration = $@"new byte[] {{ {File.ReadAllBytes($@"D:\temp\temp-{name}.cr.png").JoinString(", ")} }}";
                    File.Delete($@"D:\temp\temp-{name}.png");
                    File.Delete($@"D:\temp\temp-{name}.cr.png");
                });
                thr.Start();
                pngcrs.Add(thr);
            }
            foreach (var th in pngcrs)
                th.Join();

            if (declarations.Count > 0)
                File.WriteAllText(@"D:\c\KTANE\Hexamaze\Assets\MarkingPngs.cs", $@"
using System.Collections.Generic;
namespace Hexamaze {{
    public static class MarkingPngs {{
        public static Dictionary<Marking, byte[]> RawBytes = new Dictionary<Marking, byte[]> {{
            {declarations.JoinString(",\r\n            ")}
        }};
        public static byte[] LineRawBytes = {lineDeclaration};
    }}
}}");
        }

        public static void DoHexamazeStuff()
        {
            var jsonFile = @"D:\c\KTANE\KtaneStuff\Hexamaze.json";
            var maze = ClassifyJson.DeserializeFile<HexamazeInfo>(jsonFile);
            const double hexWidth = 72;

            Console.WriteLine(areMarkingsUnique(maze));

            //// Create the PNG for the Paint.NET layer
            //var lhw = Hex.LargeWidth(4) * hexWidth;
            //var lhh = Hex.LargeHeight(4) * hexWidth * Hex.WidthToHeight;
            //GraphicsUtil.DrawBitmap((int) lhw, (int) lhh, g =>
            //{
            //    g.Clear(Color.Transparent);
            //    g.FillPolygon(new SolidBrush(Color.FromArgb(10, 104, 255)), Hex.LargeHexagonOutline(4, hexWidth).Select(p => new PointD(p.X + lhw / 2, p.Y + lhh / 2).ToPointF()).ToArray());
            //}).Save(@"D:\temp\temp.png");

            foreach (var path in new[] { @"D:\c\KTANE\HTML\Hexamaze.html", @"D:\c\KTANE\Hexamaze\Manual\Hexamaze.html" })
            {
                // Create the color chart (top-left of the manual page)
                File.WriteAllText(path, Regex.Replace(File.ReadAllText(path), @"(?<=<!--%%-->).*(?=<!--%%%-->)", options: RegexOptions.Singleline, replacement: $@"
                <svg class='legend' viewBox='-325 -375 650 750'>
                    {"red,yellow,green,cyan,blue,pink".Split(',').Select((color, i) => $"<g class='label' transform='rotate({330 + 60 * i})'><text text-anchor='middle' y='-280'>{color}</text><path d='M-124.7-150v-200M124.7-150v-200' /></g>").JoinString()}
                    <polygon class='outline' points='{Hex.LargeHexagonOutline(4, hexWidth).Select(p => $"{p.X},{p.Y}").JoinString(" ")}' />
                    {Hex.LargeHexagon(4).Select(h => h.GetCenter(hexWidth)).Select(p => $"<circle class='dot' cx='{p.X}' cy='{p.Y}' r='{hexWidth / 12}' />").JoinString()}
                </svg>"));

                // Create the main maze in the manual page
                File.WriteAllText(path, Regex.Replace(File.ReadAllText(path), @"(?<=<!--##-->).*(?=<!--###-->)", maze.CreateSvg(), RegexOptions.Singleline));
            }

            //// Save the JSON
            //ClassifyJson.SerializeToFile(maze, jsonFile);
        }

        public static void DoHexamazeComponentSvg()
        {
            const double hexWidth = 48;
            var path = @"D:\c\KTANE\Hexamaze\Manual\img\Component\Hexamaze.svg";
            File.WriteAllText(path, Regex.Replace(File.ReadAllText(path), @"(?<=<!--##-->).*(?=<!--###-->)", options: RegexOptions.Singleline, replacement: $@"
                <polygon fill='#000' points='{Hex.LargeHexagonOutline(4, hexWidth).Select(p => $"{p.X + 174},{p.Y + 174}").JoinString(" ")}' />
                {Hex.LargeHexagon(4)
                    .Select(h => new { Point = h.GetCenter(hexWidth) + new PointD(174, 174) })
                    .Select(inf => $"<circle cx='{inf.Point.X}' cy='{inf.Point.Y}' r='{hexWidth / 12}' fill='#fff' />")
                    .JoinString()}
                {Enumerable.Range(0, 6).Select(i => i * 60).Select(angle => $@"<path fill='#000' d='M0-161l30 10h-60' transform='translate(174, 174) rotate({angle})'/>").JoinString()}
            "));
        }
    }
}
