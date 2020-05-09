using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using KtaneStuff.Modeling;
using RT.KitchenSink;
using RT.Serialization;
using RT.Util;
using RT.Util.Collections;
using RT.Util.Consoles;
using RT.Util.ExtensionMethods;
using RT.Util.Geometry;

namespace KtaneStuff
{
    using static Md;

    static class Hexamaze
    {
        public static void DoModels()
        {
            File.WriteAllText(@"D:\c\KTANE\Hexamaze\Assets\Models\Screen.obj", GenerateObjFile(Screen(), "Screen"));
            File.WriteAllText(@"D:\c\KTANE\Hexamaze\Assets\Models\ScreenFrame.obj", GenerateObjFile(ScreenFrame(), "ScreenFrame"));
            File.WriteAllText(@"D:\c\KTANE\Hexamaze\Assets\Models\Button.obj", GenerateObjFile(Button(), "Button"));
            File.WriteAllText(@"D:\c\KTANE\Hexamaze\Assets\Models\Pawn.obj", GenerateObjFile(Pawn(), "Pawn"));
        }

        private static IEnumerable<Pt[]> Screen()
        {
            return Hex.LargeHexagonOutline(sideLength: 4, hexWidth: 1)
                .SelectConsecutivePairs(true, (p1, p2) => new[] { pt(0, 0, 0), pt(p2.X, 0, p2.Y), pt(p1.X, 0, p1.Y) });
        }

        private static IEnumerable<VertexInfo[]> ScreenFrame()
        {
            var arr = Ut.NewArray(
                new { Outline = Hex.LargeHexagonOutline(sideLength: 4, hexWidth: 1, expand: .0).InsertKinks(.0).ToArray(), Y = 0d },
                new { Outline = Hex.LargeHexagonOutline(sideLength: 4, hexWidth: 1, expand: .05).InsertKinks(.05).ToArray(), Y = .1 },
                new { Outline = Hex.LargeHexagonOutline(sideLength: 4, hexWidth: 1, expand: .2).InsertKinks(.35).ToArray(), Y = .1 },
                new { Outline = Hex.LargeHexagonOutline(sideLength: 4, hexWidth: 1, expand: .25).InsertKinks(.4).ToArray(), Y = 0d }
            ).Reverse().ToArray();
            return CreateMesh(false, true, Ut.NewArray(arr.Length, arr[0].Outline.Length, (x, y) => pt(arr[x].Outline[y].X, arr[x].Y, arr[x].Outline[y].Y, Normal.Mine, Normal.Mine, Normal.Mine, Normal.Mine)));
        }

        private static IEnumerable<PointD> InsertKinks(this IEnumerable<PointD> src, double expand)
        {
            expand += 3.5 * Hex.WidthToHeight;
            var i = 0;
            foreach (var elem in src)
            {
                yield return elem;
                i++;
                if (i % 7 == 0)
                    yield return new PointD(expand * cos(60 * (i / 7) - 90), expand * sin(60 * (i / 7) - 90));
            }
        }

        private static IEnumerable<VertexInfo[]> Button()
        {
            var f = .0035;
            var h = .0075;
            var w = .12;
            return CreateMesh(false, true,
                new BevelPoint(0, 0, Normal.Mine, Normal.Mine).Concat(
                Bézier(p(w, 0), p(w, f), p(w - h + f, h), p(w - h, h), 20).Select((p, first, last) => new BevelPoint(p.X, p.Y, first || last ? Normal.Mine : Normal.Average, first || last ? Normal.Mine : Normal.Average)))
                .Concat(new BevelPoint(0, h, Normal.Mine, Normal.Mine))
                .Select(bi =>
                    new[] { new { Angle = 90, Dist = .1 }, new { Angle = 190, Dist = .5 }, new { Angle = -10, Dist = .5 } }
                        .Select(info => pt(bi.X * info.Dist * cos(info.Angle), bi.Y, bi.X * info.Dist * sin(info.Angle), bi.Before, bi.After, Normal.Mine, Normal.Mine))
                        .ToArray()
                ).ToArray());
        }

        private static IEnumerable<VertexInfo[]> Pawn()
        {
            return CreateMesh(false, true, DecodeSvgPath.Do(@"M 13,0 C 19,0 22,3 22,9 22,15 19,14 19,20 19,28 26,30 26,48 26,58 20,57 13,57", .1)
                .FirstOrDefault()
                .SelectConsecutivePairs(true, (p1, p2) => p1 == p2 ? (PointD?) null : p1)
                .Where(p => p != null)
                .Select(p => p.Value + new PointD(-13, -57))
                .Select(p => Enumerable.Range(0, 72).Select(i => i * 360 / 72).Select(angle => pt(p.X * cos(angle), -p.Y, p.X * sin(angle))).ToArray())
                .Reverse()
                .ToArray());
        }

        public enum Marking
        {
            None,
            Circle,
            TriangleUp,
            TriangleDown,
            TriangleLeft,
            TriangleRight,
            Hexagon
        }

        private static Marking[] _markingsTriangle1 = new[] { Marking.TriangleUp, Marking.TriangleDown };
        private static Marking[] _markingsTriangle2 = new[] { Marking.TriangleLeft, Marking.TriangleRight };
        private static Marking Rotate(this Marking marking, int rotation)
        {
            switch (marking)
            {
                case Marking.None: return Marking.None;
                case Marking.Circle: return Marking.Circle;
                case Marking.Hexagon: return Marking.Hexagon;

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
        private static string ToMarkingString(this IEnumerable<Marking> markings) => markings.Select(m => m == Marking.None ? "•" : ((int) m).ToString()).JoinString();

        public sealed class HexamazeInfo : ICloneable
        {
            public int Size = 12;
            public int SubmazeSize = 12;
            // The bool[] has three elements: wall at NW, N, and NE. The other walls must be obtained from the neighbouring hexes.
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
                case Marking.TriangleUp:
                    return $"{dot}<polygon{attr}{(useCssClass ? " class='marking triangle-up'" : null)} points='{Enumerable.Range(0, 3).Select(i => (i + .25) * 2 * Math.PI / 3).Select(angle => $"{x + hexWidth * .325 * Math.Cos(angle)},{y + hexWidth * .325 * Math.Sin(angle)}").JoinString(" ")}' />";
                case Marking.TriangleDown:
                    return $"{dot}<polygon{attr}{(useCssClass ? " class='marking triangle-down'" : null)} points='{Enumerable.Range(0, 3).Select(i => (i + .75) * 2 * Math.PI / 3).Select(angle => $"{x + hexWidth * .325 * Math.Cos(angle)},{y + hexWidth * .325 * Math.Sin(angle)}").JoinString(" ")}' />";
                case Marking.TriangleLeft:
                    return $"{dot}<polygon{attr}{(useCssClass ? " class='marking triangle-left'" : null)} points='{Enumerable.Range(0, 3).Select(i => (i + .5) * 2 * Math.PI / 3).Select(angle => $"{x + hexWidth * .325 * Math.Cos(angle)},{y + hexWidth * .325 * Math.Sin(angle)}").JoinString(" ")}' />";
                case Marking.TriangleRight:
                    return $"{dot}<polygon{attr}{(useCssClass ? " class='marking triangle-right'" : null)} points='{Enumerable.Range(0, 3).Select(i => i * 2 * Math.PI / 3).Select(angle => $"{x + hexWidth * .325 * Math.Cos(angle)},{y + hexWidth * .325 * Math.Sin(angle)}").JoinString(" ")}' />";
                case Marking.Hexagon:
                    return $"{dot}<polygon{attr}{(useCssClass ? " class='marking hexagon'" : null)} points='{Enumerable.Range(0, 6).Select(i => i * Math.PI / 3).Select(angle => $"{x + hexWidth * .325 * Math.Cos(angle)},{y + hexWidth * .325 * Math.Sin(angle)}").JoinString(" ")}' />";
            }
            return dot;
        }

        public static HexamazeInfo GenerateHexamaze()
        {
            const int mazeSize = 12;
            const int smallMazeSize = 4;

            var rnd = new Random(26);   // This random seed gives the most walls (all but 501)
            var totalWallsRemoved = 0;
            var walls = new AutoDictionary<Hex, bool[]>(hex =>
            {
                return new bool[3] {
                    hex.Distance < mazeSize || hex.Neighbors[0].Distance < mazeSize,
                    hex.Distance < mazeSize || hex.Neighbors[1].Distance < mazeSize,
                    hex.Distance < mazeSize || hex.Neighbors[2].Distance < mazeSize
                };
            });
            var removeWall = Ut.Lambda((Hex hex, int n) => { walls[n < 3 ? hex : hex.Neighbors[n]][n % 3] = false; totalWallsRemoved++; });
            var hasWall = Ut.Lambda((Hex hex, int n) => walls[n < 3 ? hex : hex.Neighbors[n]][n % 3]);
            var stack = new Stack<Hex>();
            var curHex = new Hex(0, 0);
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

                // Remove one wall out of the “most wanted”
                var topScores = candidateCounts.Values.Distinct().Order().TakeLast(1).ToArray();
                var candidates2 = candidateCounts.Where(kvp => topScores.Contains(kvp.Value)).ToArray();

                //*
                var randomCandidate = candidates2[rnd.Next(candidates2.Length)];
                removeWall(randomCandidate.Key.Item1, randomCandidate.Key.Item2);
                /*/
                foreach (var candidate in candidates2)
                    removeWall(candidate.Key.Item1, candidate.Key.Item2);
                /**/

                Console.Write($"Walls removed: {totalWallsRemoved}  \r");
            }

            // Step 3: Put as many walls back in as possible
            var missingWalls = walls
                .SelectMany(kvp => kvp.Value.Select((w, i) => (hex: kvp.Key, index: i, isWall: w)))
                .Where(inf => !inf.isWall && (inf.hex.Distance < mazeSize || inf.hex.Neighbors[inf.index].Distance < mazeSize))
                .ToList();
            while (missingWalls.Count > 0)
            {
                var randomMissingWallIndex = rnd.Next(missingWalls.Count);
                var randomMissingWall = missingWalls[randomMissingWallIndex];
                missingWalls.RemoveAt(randomMissingWallIndex);
                if (randomMissingWall.hex.Q == 12 && randomMissingWall.hex.R == -10 && randomMissingWall.index > 0)
                    System.Diagnostics.Debugger.Break();
                walls[randomMissingWall.hex][randomMissingWall.index] = true;

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
                        // These walls cannot be added, take them back out.
                        walls[randomMissingWall.hex][randomMissingWall.index] = false;
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
                Markings = markings.Select((h, ix) => new { Hex = h, Index = ix }).ToDictionary(inf => inf.Hex, inf => (Marking) inf.Index),
                Walls = walls.ToDictionary()
            };
        }

        public static HexamazeInfo GenerateMarkings(HexamazeInfo origMaze)
        {
            var seed = 146;     // 19 markings with initial hexagon in the center
            var rnd = new Random(seed);

            var maze = origMaze.Clone();
            var size = maze.Size;
            var smallSize = maze.SubmazeSize;

            tryAgain:
            maze.Markings = new Dictionary<Hex, Marking>();
            // List Circle and Hexagon twice so that triangles don’t completely dominate the distribution
            var allowedMarkings = new[] { Marking.Circle, Marking.Circle, Marking.Hexagon, Marking.Hexagon, Marking.TriangleDown, Marking.TriangleLeft, Marking.TriangleRight, Marking.TriangleUp };

            // Put a hexagon in the center
            maze.Markings.Add(new Hex(0, 0), Marking.Hexagon);

            // Step 1: Put random markings in until there are no more ambiguities
            while (!areMarkingsUnique(maze))
            {
                var availableHexes = Hex.LargeHexagon(size).Where(h => !maze.Markings.ContainsKey(h) && !h.Neighbors.SelectMany(n => n.Neighbors).Any(maze.Markings.ContainsKey)).ToArray();
                if (availableHexes.Length == 0)
                {
                    Console.WriteLine("Markings are wonky. Trying again.");
                    goto tryAgain;
                }
                var randomHex = availableHexes[rnd.Next(availableHexes.Length)];
                maze.Markings[randomHex] = allowedMarkings.PickRandom(rnd);
            }

            Console.WriteLine($"{maze.Markings.Count(kvp => kvp.Value != Marking.None)} markings");

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

            Console.WriteLine($"{maze.Markings.Count(kvp => kvp.Value != Marking.None)} markings");

            //    var msg = "{0/White} = {1/Green}{2/Magenta}".Color(null).Fmt(seed, maze.Markings.Count, maze.Markings.Count < lowestMarkings ? " — NEW BEST" : maze.Markings.Count == lowestMarkings ? " — TIED" : "");
            //    if (maze.Markings.Count <= lowestMarkings)
            //    {
            //        lowestMarkings = maze.Markings.Count;
            //        bestMaze = maze;
            //        WriteMazeInManual(bestMaze);
            //    }
            //    ConsoleUtil.WriteLine(msg);

            return maze;
        }

        private static bool areMarkingsUnique(HexamazeInfo maze, bool saveFiles = false)
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
                    if (unique.TryGetValue(markingsStr, out var uniqs) && uniqs.Count > 0)
                    {
                        if (saveFiles)
                        {
                            var fills1 = Tuple.Create(Hex.LargeHexagon(smallSize).Select(h => h.Rotate(uniqs[0].Item2) + uniqs[0].Item1), "fed");
                            var fills2 = Tuple.Create(Hex.LargeHexagon(smallSize).Select(h => h.Rotate(rotation) + centerHex), "def");
                            ambig++;
                            File.WriteAllText($@"D:\c\KTANE\HTML\Hexamaze{ambig}.html", Regex.Replace(File.ReadAllText(@"D:\c\KTANE\HTML\Hexamaze.html"), @"\A(.*<!--##-->\s*).*?(?=\s*<!--###-->)", options: RegexOptions.Singleline, evaluator: m => m.Groups[1].Value + maze.CreateSvg(new[] { fills1, fills2 })));
                        }
                        else
                            return false;
                    }
                    unique.AddSafe(markingsStr, Tuple.Create(centerHex, rotation));
                }
            }

            if (!saveFiles)
                return true;

            foreach (var nonUnique in unique.Where(k => k.Value.Count > 1))
                Console.WriteLine($"{nonUnique.Key} = {nonUnique.Value.Select(tup => $"{tup.Item1}/{(6 - tup.Item2) % 6}").JoinString(", ")}");
            return unique.All(kvp => kvp.Value.Count <= 1);
        }

        public static void GenerateMarkingsSvg()
        {
            var pngcrs = new List<Thread>();
            foreach (var obj in new object[] { "Line", Marking.None, Marking.Circle, Marking.Hexagon, Marking.TriangleDown, Marking.TriangleLeft, Marking.TriangleRight, Marking.TriangleUp })
            {
                var marking = obj as Marking?;
                var name = obj.ToString();

                File.WriteAllText($@"D:\temp\temp.svg", marking != null
                    ? $"<svg xmlns='http://www.w3.org/2000/svg' viewBox='-.5 -.5 1 1'>{marking.Value.getMarkingSvg(1, 0, 0, useAttributes: true, strokeWidth: .02, includeDot: true)}</svg>"
                    : $"<svg xmlns='http://www.w3.org/2000/svg' viewBox='-.75 -.75 1.5 1.5'>{new Hex(0, 0).GetPolygon(1).Apply(ps => $"<line x1='{ps[1].X}' y1='{ps[1].Y}' x2='{ps[2].X}' y2='{ps[2].Y}' stroke-width='.1' stroke='#f0faff' stroke-linecap='round' />")}</svg>");
                CommandRunner.RunRaw($@"D:\Inkscape\InkscapePortable.exe -z -f D:\temp\temp.svg -w {(marking == null ? 150 : 100)} -e D:\temp\temp-{name}.png").Go();
                var thr = new Thread(() =>
                {
                    CommandRunner.RunRaw($@"pngcr D:\temp\temp-{name}.png D:\c\KTANE\Hexamaze\Assets\Images\{name}.png").Go();
                    File.Delete($@"D:\temp\temp-{name}.png");
                });
                thr.Start();
                pngcrs.Add(thr);
            }
            foreach (var th in pngcrs)
                th.Join();
        }

        public static void DoStuff()
        {
            var jsonFile = @"D:\c\KTANE\KtaneStuff\DataFiles\Hexamaze\Hexamaze.json";
            var maze = ClassifyJson.DeserializeFile<HexamazeInfo>(jsonFile);

            Console.WriteLine(areMarkingsUnique(maze, saveFiles: true));

            //var dic = new Dictionary<string, int>();
            //var triangles = new[] { Marking.TriangleDown, Marking.TriangleLeft, Marking.TriangleRight, Marking.TriangleUp };
            //var trianglesV = new[] { Marking.TriangleLeft, Marking.TriangleRight };
            //var trianglesE = new[] { Marking.TriangleDown, Marking.TriangleUp };
            //foreach (var center in Hex.LargeHexagon(9))
            //{
            //    var countC = Hex.LargeHexagon(4).Select(h => maze.Markings.Get(center + h, Marking.None)).Count(m => m == Marking.Circle);
            //    var countH = Hex.LargeHexagon(4).Select(h => maze.Markings.Get(center + h, Marking.None)).Count(m => m == Marking.Hexagon);
            //    var countTV = Hex.LargeHexagon(4).Select(h => maze.Markings.Get(center + h, Marking.None)).Count(m => trianglesV.Contains(m));
            //    var countTE = Hex.LargeHexagon(4).Select(h => maze.Markings.Get(center + h, Marking.None)).Count(m => trianglesE.Contains(m));
            //    dic.IncSafe(new[] { countC != 0 ? countC + " circles" : null, countH != 0 ? countH + " hexagons" : null, countTV != 0 ? countTV + " vertex triangles" : null, countTE != 0 ? countTE + " edge triangles" : null }.Where(x => x != null).JoinString(", "));
            //}
            //foreach (var kvp in dic.OrderBy(k => k.Value))
            //    Console.WriteLine($"{kvp.Key} = {kvp.Value} times ({kvp.Value / (double) 217 * 100:0.00}%)");

            //// Create the PNG for the Paint.NET layer
            //const double hexWidth = 72;
            //var lhw = Hex.LargeWidth(4) * hexWidth;
            //var lhh = Hex.LargeHeight(4) * hexWidth * Hex.WidthToHeight;
            //GraphicsUtil.DrawBitmap((int) lhw, (int) lhh, g =>
            //{
            //    g.Clear(Color.Transparent);
            //    g.FillPolygon(new SolidBrush(Color.FromArgb(10, 104, 255)), Hex.LargeHexagonOutline(4, hexWidth).Select(p => new PointD(p.X + lhw / 2, p.Y + lhh / 2).ToPointF()).ToArray());
            //}).Save(@"D:\temp\temp.png");

            maze = GenerateMarkings(maze);
            WriteMazeInManual(maze);

            // Save the JSON
            //ClassifyJson.SerializeToFile(maze, jsonFile);
        }

        private static void WriteMazeInManual(HexamazeInfo maze)
        {
            const double hexWidth = 72;
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
        }

        public static void DoComponentSvg()
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
