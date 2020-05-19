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

        private const int sw = 2 * HexamazeInfo.Size + 1;

        public static void GenerateMazeAndOutputToConsole()
        {
            var start = DateTime.UtcNow;

            var rnd = new MonoRandom(2);
            var inf = GenerateHexamaze(rnd);
            var w = 6 * HexamazeInfo.Size - 2;
            var h = 4 * HexamazeInfo.Size - 1;
            var chs = new char[w, h];
            for (var x = 0; x < w; x++)
                for (var y = 0; y < h; y++)
                    chs[x, y] = ' ';

            // Walls
            for (var ix = 0; ix < inf.Walls.Length; ix++)
            {
                if (!inf.Walls[ix])
                    continue;

                var dir = ix % 3;
                var r = (ix / 3) % sw - HexamazeInfo.Size;
                var q = (ix / 3) / sw - HexamazeInfo.Size;
                switch (dir)
                {
                    // NW wall
                    case 0:
                        chs[w / 2 + 3 * q - 2, h / 2 + q + 2 * r] = '/';
                        break;

                    // N wall
                    case 1:
                    {
                        chs[w / 2 + 3 * q - 1, h / 2 + q + 2 * r - 1] = '_';
                        chs[w / 2 + 3 * q, h / 2 + q + 2 * r - 1] = '_';
                    }
                    break;

                    // NE wall
                    case 2:
                        chs[w / 2 + 3 * q + 1, h / 2 + q + 2 * r] = '\\';
                        break;
                }
            }

            // Markings
            foreach (var hex in Hex.LargeHexagon(HexamazeInfo.Size))
            {
                var m = inf.Markings[markingIndex(hex)];
                if (m != Marking.None)
                {
                    chs[w / 2 + 3 * hex.Q - 1, h / 2 + hex.Q + 2 * hex.R] = " (/\\<|{"[(int) m];
                    chs[w / 2 + 3 * hex.Q, h / 2 + hex.Q + 2 * hex.R] = " )\\/|>}"[(int) m];
                }
            }
            for (var y = 0; y < h; y++)
                Console.WriteLine(Enumerable.Range(0, w).Select(x => chs[x, y]).JoinString());

            Console.WriteLine($"{inf.Markings.Count(m => m != Marking.None)} markings");
            Console.WriteLine($"Took {(DateTime.UtcNow - start).TotalSeconds:0.#}sec.");
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

        private static readonly Marking[] _markingsTriangle1 = new[] { Marking.TriangleUp, Marking.TriangleDown };
        private static readonly Marking[] _markingsTriangle2 = new[] { Marking.TriangleLeft, Marking.TriangleRight };
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

        public sealed class HexamazeInfo
        {
            public const int Size = 12;
            public const int SubmazeSize = 4;

            // Walls are at NW=0, N=1, and NE=2. The other walls must be obtained from the neighbouring hexes.
            public bool[] Walls = new bool[3 * sw * sw];
            public bool HasWall(Hex hex, int dir) => dir < 3 ? Walls[3 * (2 * Size) * hex.Q + 3 * hex.R + dir] : HasWall(hex.GetNeighbor(dir), dir - 3);

            public Marking[] Markings = new Marking[sw * sw];
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

        private static int markingIndex(Hex h)
        {
            return (h.Q + HexamazeInfo.Size) * sw + h.R + HexamazeInfo.Size;
        }

        private static int wallIndex(Hex hex, int dir)
        {
            return dir < 3
                ? 3 * sw * (hex.Q + HexamazeInfo.Size) + 3 * (hex.R + HexamazeInfo.Size) + dir
                : wallIndex(hex.GetNeighbor(dir), dir - 3);
        }

        public static HexamazeInfo GenerateHexamaze(MonoRandom rnd)
        {
            // PART 1: GENERATE MAZE (walls)
            var walls = Ut.NewArray(3 * sw * sw, ix =>
            {
                var r = (ix / 3) % sw - HexamazeInfo.Size;
                var q = (ix / 3) / sw - HexamazeInfo.Size;
                var h = new Hex(q, r);
                return h.Distance < HexamazeInfo.Size || h.GetNeighbor(ix % 3).Distance < HexamazeInfo.Size;
            });
            var allWalls = (bool[]) walls.Clone();

            var stack = new Stack<Hex>();
            var curHex = new Hex(0, 0);
            stack.Push(curHex);
            var taken = new HashSet<Hex> { curHex };

            // Step 1.1: generate a single giant maze
            while (true)
            {
                var neighbors = curHex.Neighbors;
                var availableNeighborIndices = neighbors.SelectIndexWhere(n => !taken.Contains(n) && n.Distance < HexamazeInfo.Size).ToArray();
                if (availableNeighborIndices.Length == 0)
                {
                    if (stack.Count == 0)
                        break;
                    curHex = stack.Pop();
                    continue;
                }
                var dir = availableNeighborIndices[rnd.Next(availableNeighborIndices.Length)];
                walls[wallIndex(curHex, dir)] = false;
                stack.Push(curHex);
                curHex = neighbors[dir];
                taken.Add(curHex);
            }

            // Step 1.2: Go through all submazes and make sure they’re all connected and all have at least one exit on each side
            // This is parallelizable and uses multiple threads
            while (true)
            {
                var candidateCounts = new Dictionary<int, int>();

                Hex.LargeHexagon(HexamazeInfo.Size - HexamazeInfo.SubmazeSize + 1).ParallelForEach(Environment.ProcessorCount, centerHex =>
                {
                    var validity = DetermineSubmazeValidity(walls, centerHex);
                    if (validity.IsValid)
                        return;

                    // Find out which walls might benefit from removing
                    foreach (var fh in validity.Filled)
                    {
                        var neighbors = fh.Neighbors;
                        for (var dir = 0; dir < neighbors.Length; dir++)
                        {
                            var th = neighbors[dir];
                            var offset = th - centerHex;
                            if ((offset.Distance < HexamazeInfo.SubmazeSize && walls[wallIndex(fh, dir)] && !validity.Filled.Contains(th)) ||
                                (offset.Distance == HexamazeInfo.SubmazeSize && offset.GetEdges(HexamazeInfo.SubmazeSize).Any(e => !validity.EdgesReachable[e])))
                                lock (candidateCounts)
                                    candidateCounts.IncSafe(wallIndex(fh, dir));
                        }
                    }
                });

                if (candidateCounts.Count == 0)
                    break;

                // Remove one wall out of the “most wanted”
                var topScore = 0;
                var topScorers = new List<int>();
                foreach (var kvp in candidateCounts)
                    if (kvp.Value > topScore)
                    {
                        topScore = kvp.Value;
                        topScorers.Clear();
                        topScorers.Add(kvp.Key);
                    }
                    else if (kvp.Value == topScore)
                        topScorers.Add(kvp.Key);
                var randomCandidate = topScorers[rnd.Next(topScorers.Count)];
                walls[randomCandidate] = false;
            }

            // Step 1.3: Put as many walls back in as possible
            var missingWalls = Enumerable.Range(0, allWalls.Length).Where(ix => allWalls[ix] && !walls[ix]).ToList();
            while (missingWalls.Count > 0)
            {
                var randomMissingWallIndex = rnd.Next(missingWalls.Count);
                var randomMissingWall = missingWalls[randomMissingWallIndex];
                missingWalls.RemoveAt(randomMissingWallIndex);
                walls[randomMissingWall] = true;

                foreach (var centerHex in rnd.ShuffleFisherYates(Hex.LargeHexagon(HexamazeInfo.Size - HexamazeInfo.SubmazeSize + 1).ToList()))
                {
                    if (!DetermineSubmazeValidity(walls, centerHex).IsValid)
                    {
                        // This wall cannot be added, take it back out.
                        walls[randomMissingWall] = false;
                        break;
                    }
                }
            }

            // PART 2: GENERATE MARKINGS
            tryAgain:
            var markings = new Marking[sw * sw];
            // List Circle and Hexagon twice so that triangles don’t completely dominate the distribution
            var allowedMarkings = new[] { Marking.Circle, Marking.Circle, Marking.Hexagon, Marking.Hexagon, Marking.TriangleDown, Marking.TriangleLeft, Marking.TriangleRight, Marking.TriangleUp };

            // Step 2.1: Put random markings in until there are no more ambiguities
            while (!areMarkingsUnique(markings))
            {
                var availableHexes = Hex.LargeHexagon(HexamazeInfo.Size)
                    .Where(h => markings[markingIndex(h)] == Marking.None && h.Neighbors.SelectMany(n => n.Neighbors).All(h => h.Distance >= HexamazeInfo.Size || markings[markingIndex(h)] == Marking.None))
                    .ToArray();
                if (availableHexes.Length == 0)
                    goto tryAgain;
                var randomHex = availableHexes[rnd.Next(availableHexes.Length)];
                markings[markingIndex(randomHex)] = allowedMarkings[rnd.Next(0, allowedMarkings.Length)];
            }

            // Step 2.2: Find markings to remove again
            var removableMarkings = markings.SelectIndexWhere(m => m != Marking.None).ToList();
            while (removableMarkings.Count > 0)
            {
                var tryRemoveIndex = rnd.Next(removableMarkings.Count);
                var tryRemove = removableMarkings[tryRemoveIndex];
                removableMarkings.RemoveAt(tryRemoveIndex);
                var prevMarking = markings[tryRemove];
                markings[tryRemove] = Marking.None;
                if (!areMarkingsUnique(markings))
                {
                    // No longer unique — put it back in
                    markings[tryRemove] = prevMarking;
                }
            }

            return new HexamazeInfo { Walls = walls, Markings = markings };
        }

        private sealed class SubmazeValidity
        {
            public HashSet<Hex> Filled;
            public bool[] EdgesReachable;
            public bool IsValid;
        }

        private static SubmazeValidity DetermineSubmazeValidity(bool[] walls, Hex centerHex)
        {
            bool hasWall(Hex hex, int dir) => dir < 3 ? walls[3 * sw * (hex.Q + HexamazeInfo.Size) + 3 * (hex.R + HexamazeInfo.Size) + dir] : hasWall(hex.GetNeighbor(dir), dir - 3);
            var ret = new SubmazeValidity();
            ret.Filled = new HashSet<Hex> { centerHex };
            var q = ret.Filled.ToQueue();
            ret.EdgesReachable = new bool[6];

            // Flood-fill as much of the maze as possible
            while (q.Count > 0)
            {
                var hex = q.Dequeue();
                var neighbors = hex.Neighbors;
                for (int dir = 0; dir < 6; dir++)
                {
                    var offset = neighbors[dir] - centerHex;
                    if (offset.Distance < HexamazeInfo.SubmazeSize && !hasWall(hex, dir) && ret.Filled.Add(neighbors[dir]))
                        q.Enqueue(neighbors[dir]);
                    if (offset.Distance == HexamazeInfo.SubmazeSize && !hasWall(hex, dir))
                        foreach (var edge in offset.GetEdges(HexamazeInfo.SubmazeSize))
                            ret.EdgesReachable[edge] = true;
                }
            }

            ret.IsValid =
                // All hexes filled?
                (ret.Filled.Count >= 3 * HexamazeInfo.SubmazeSize * (HexamazeInfo.SubmazeSize - 1) + 1) &&
                // All edges reachable?
                !ret.EdgesReachable.Contains(false);
            return ret;
        }

        private static bool areMarkingsUnique(Marking[] markings)
        {
            var unique = new HashSet<string>();
            foreach (var centerHex in Hex.LargeHexagon(HexamazeInfo.Size - HexamazeInfo.SubmazeSize + 1))
            {
                for (int rotation = 0; rotation < 6; rotation++)
                {
                    var markingsStr = Hex.LargeHexagon(HexamazeInfo.SubmazeSize)
                        .Select(h => h.Rotate(rotation) + centerHex)
                        .Select(h => markings[markingIndex(h)].Rotate(-rotation))
                        .ToMarkingString();
                    if (!unique.Add(markingsStr))
                        return false;
                }
            }
            return true;
        }

        public static void GenerateMarkingsTextures()
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

        public static void WriteMazeInManual()
        {
            const double hexSizeFactor = 72;
            foreach (var path in new[] { @"D:\c\KTANE\Public\HTML\Hexamaze.html", @"D:\c\KTANE\Hexamaze\Manual\Hexamaze.html" })
            {
                // Create the color chart (top-left of the manual page)
                Utils.ReplaceInFile(path, "<!--%%-->", "<!--%%%-->", $@"
                    <svg class='legend' viewBox='-325 -375 650 750'>
                        {"red,yellow,green,cyan,blue,pink".Split(',').Select((color, i) => $"<g class='label' transform='rotate({330 + 60 * i})'><text text-anchor='middle' y='-280'>{color}</text><path d='M-124.7-150v-200M124.7-150v-200' /></g>").JoinString()}
                        <polygon class='outline' points='{Hex.LargeHexagonOutline(4, hexSizeFactor).Select(p => $"{p.X},{p.Y}").JoinString(" ")}' />
                        {Hex.LargeHexagon(4).Select(h => h.GetCenter(hexSizeFactor)).Select(p => $"<circle class='dot' cx='{p.X}' cy='{p.Y}' r='{hexSizeFactor / 12}' />").JoinString()}
                    </svg>");

                // Create the main maze in the manual page
                var wallsSvg = new StringBuilder();
                var markingsSvg = new StringBuilder();

                foreach (var hex in Hex.LargeHexagon(HexamazeInfo.Size + 1))
                {
                    var poly = hex.GetPolygon(hexSizeFactor);
                    for (int dir = 0; dir < 3; dir++)
                        if (hex.Distance < HexamazeInfo.Size || hex.GetNeighbor(dir).Distance < HexamazeInfo.Size)
                            wallsSvg.Append($"<line class='wall' id='wall-{hex.Q}-{hex.R}-{dir}' x1='{poly[dir].X}' y1='{poly[dir].Y}' x2='{poly[dir + 1].X}' y2='{poly[dir + 1].Y}' />");
                    if (hex.Distance < HexamazeInfo.Size)
                    {
                        var p = hex.GetCenter(hexSizeFactor);
                        markingsSvg.Append($"<circle class='dot' cx='{p.X}' cy='{p.Y}' r='{hexSizeFactor / 12}' />");
                        markingsSvg.Append($"<path class='marking' id='marking-{hex.Q}-{hex.R}' />");
                    }
                }

                var w = Hex.LargeWidth(HexamazeInfo.Size) * hexSizeFactor;
                var h = Hex.LargeHeight(HexamazeInfo.Size) * hexSizeFactor;
                Utils.ReplaceInFile(path, "<!--##-->", "<!--###-->", $@"<svg class='hexamaze' viewBox='{-w / 2 - 5} {-h / 2 - 5} {w + 10} {h + 10}'>{wallsSvg}{markingsSvg}</svg>");
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
