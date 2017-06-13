using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Xml.Linq;
using KtaneStuff.Modeling;
using RT.KitchenSink;
using RT.Util;
using RT.Util.ExtensionMethods;
using RT.Util.Geometry;

namespace KtaneStuff
{
    using static Md;

    static class Battleship
    {
        public static void DoModels()
        {
            File.WriteAllText(@"D:\c\KTANE\Battleship\Assets\Models\SquareHighlight.obj", GenerateObjFile(SquareHighlight(), "SquareHighlight"));
            File.WriteAllText(@"D:\c\KTANE\Battleship\Assets\Models\WaterHighlight.obj", GenerateObjFile(WaterHighlight(), "WaterHighlight"));
            File.WriteAllText(@"D:\c\KTANE\Battleship\Assets\Models\Button.obj", GenerateObjFile(Button(), "Button"));
            File.WriteAllText(@"D:\c\KTANE\Battleship\Assets\Models\ButtonHighlight.obj", GenerateObjFile(ButtonHighlight(), "ButtonHighlight"));

            foreach (var name in new string[] { "SqShipL", "SqShipF", "SqShipA" })
                File.WriteAllText($@"D:\c\KTANE\Battleship\Assets\Models\{name}.obj", GenerateObjFile(ShipPart(name), name));

            var path = @"D:\c\KTANE\Battleship\Assets\Misc\Radar.svg";
            File.WriteAllText(path, Regex.Replace(File.ReadAllText(path),
                @"(?<=<!--##-->).*(?=<!--###-->)",
                Enumerable.Range(0, 50).Select((i, f, l) => new { Angle = i * 2 + 140, Style = l ? "style='fill:rgb(42, 255, 0)'" : $"style='fill:rgb(32, 192, 0);fill-opacity:{i / 50.0}'" }).Select(inf => $"<path d='M50,50 L{50 + 40 * cos(inf.Angle)},{50 + 40 * sin(inf.Angle)} {50 + 40 * cos(inf.Angle + 2)},{50 + 40 * sin(inf.Angle + 2)}' {inf.Style} />").JoinString() +
                Enumerable.Range(0, 360 / 5).Select(i => i * 5).Select(i => $"<circle cx='{50 + 42.25 * cos(i)}' cy='{50 + 42.25 * sin(i)}' r='.5' fill='rgb(32, 192, 0)' stroke='none' />").JoinString(),
                RegexOptions.Singleline));
        }

        private static IEnumerable<Pt[]> ShipPart(string name)
        {
            const double depth1 = 3;
            const double depth2 = 3.5;

            var xml = XDocument.Parse(File.ReadAllText($@"D:\c\KTANE\Battleship\Assets\Misc\{name}.svg"));
            var pathElem = xml.Root.Elements().FirstOrDefault(el => el.Name.LocalName == "path");
            var path = pathElem.Attributes().FirstOrDefault(attr => attr.Name.LocalName == "d").Value;
            foreach (var polyOrig in DecodeSvgPath.Do(path, .25))
            {
                var poly = polyOrig.Select(p => (p - new PointD(50, 50)) / 50);
                foreach (var piece in poly.SelectConsecutivePairs(true, (p1, p2) => new[] { pt(p1.X, depth1, p1.Y), pt(p2.X, depth1, p2.Y), pt(p2.X, depth2, p2.Y), pt(p1.X, depth2, p1.Y) }))
                    yield return piece.Reverse().ToArray();
                foreach (var tri in new[] { poly }.Triangulate())
                    yield return tri.Select(p => pt(p.X, depth2, p.Y)).Reverse().ToArray();
            }
        }

        private static IEnumerable<Pt[]> SquareHighlight()
        {
            var x1 = -.7;
            var x2 = .3;
            var y1 = -.7;
            var y2 = .3;

            var oDepth = .16;
            var iDepth = .14;

            var padding = .03;
            var spacing = .02;

            var w = (x2 - x1 - 2 * padding - 5 * spacing) / 6;
            var h = (y2 - y1 - 2 * padding - 5 * spacing) / 6;

            var x = 1;
            var y = 1;

            var ix1 = x1 + padding + (w + spacing) * x;
            var iy1 = y1 + padding + (h + spacing) * y;
            var ix2 = ix1 + w;
            var iy2 = iy1 + h;

            var ox1 = ix1 - spacing;
            var oy1 = iy1 - spacing;
            var ox2 = ix2 + spacing;
            var oy2 = iy2 + spacing;

            var sqcx = (ix2 + ix1) / 2;
            var sqcy = (iy2 + iy1) / 2;
            var sqw = ix2 - ix1;

            return Ut.NewArray(
                new[] { pt(ox2, oDepth, oy1), pt(ox1, oDepth, oy1), pt(ix1, iDepth, iy1), pt(ix2, iDepth, iy1) },
                new[] { pt(ox2, oDepth, oy2), pt(ox2, oDepth, oy1), pt(ix2, iDepth, iy1), pt(ix2, iDepth, iy2) },
                new[] { pt(ox2, oDepth, oy2), pt(ix2, iDepth, iy2), pt(ix1, iDepth, iy2), pt(ox1, oDepth, oy2) },
                new[] { pt(ix1, iDepth, iy1), pt(ox1, oDepth, oy1), pt(ox1, oDepth, oy2), pt(ix1, iDepth, iy2) }
            )
                .Select(arr => arr.Select(p => (p - pt(sqcx, iDepth, sqcy)) / sqw).ToArray())
                .SelectMany(arr => new[] { arr, arr.Reverse().ToArray() });
        }

        private static IEnumerable<Pt[]> WaterHighlight()
        {
            var svg = @"M 35.8125,98.90625 35.84375,98.90625 C 48.276341,98.911781 57.763822,101.25244 74.3125,108.3125 80.957475,111.04867 89.78287,114.04254 92.53125,114.5625 L 92.59375,114.59375 92.65625,114.59375 C 103.18072,116.88168 119.73587,115.86712 127.875,112.75 L 127.875,112.78125 C 128.37066,112.55943 128.9303,112.31882 129.8125,112.25 130.83085,112.17055 132.48449,112.60948 133.46875,113.59375 135.43729,115.56228 135,116.9568 135,118.1875 135,119.62514 134.45953,121.42222 133.375,122.6875 132.29047,123.95278 130.94852,124.67389 129.5,125.25 126.60297,126.40223 122.87724,127.1063 117.09375,127.96875 L 117.0625,127.96875 117.03125,127.96875 C 108.7355,129.09533 103.5412,129.10051 94.65625,127.875 L 94.625,127.84375 94.5625,127.84375 C 87.692265,126.75324 79.271176,123.92143 63.375,117.28125 L 63.34375,117.28125 C 46.166654,110.08415 30.249317,109.19228 14.125,114.3125 12.594269,114.79988 10.330915,114.61182 8.875,113.375 7.4170859,112.13648 7.0007834,110.51688 6.96875,109.1875 6.9046832,106.52872 8.2155097,103.97793 10.9375,102.5625 L 10.96875,102.5625 10.96875,102.53125 C 13.708506,101.12223 17.179691,100.4304 21.53125,99.8125 25.869968,99.196428 30.898434,98.856414 35.8125,98.90625 z M 32.625,70.25 C 33.387682,70.24167 34.156065,70.24139 34.9375,70.25 35.830568,70.25984 36.71054,70.280072 37.625,70.3125 L 37.65625,70.3125 37.6875,70.3125 C 50.812019,70.922943 57.613492,72.564147 73.53125,79.3125 73.541395,79.316801 73.552347,79.308195 73.5625,79.3125 79.914899,81.949746 88.404393,84.978179 91.53125,85.71875 L 91.5625,85.71875 C 99.439321,87.665829 114.73703,87.63153 121.75,85.75 L 121.78125,85.71875 121.8125,85.71875 C 125.88904,84.661868 127.55305,83.923525 130.3125,84.15625 131.00236,84.214431 131.92067,84.369635 132.84375,85 133.76683,85.630365 134.44101,86.768141 134.6875,87.625 135.18049,89.338717 134.86202,90.084421 134.75,90.8125 134.48128,92.538171 132.93173,94.931067 131.1875,95.875 129.38133,96.852459 127.51067,97.297075 124.5,98.0625 L 124.4375,98.09375 124.40625,98.09375 C 114.90709,100.32885 100.00098,100.41783 90.25,98.3125 L 90.21875,98.3125 C 86.252297,97.405882 79.649595,95.198797 74.59375,93.09375 74.586775,93.090846 74.569469,93.096654 74.5625,93.09375 57.04579,85.928272 51.418488,84.115301 43.1875,83.0625 35.016781,82.056147 21.71872,83.258534 17.8125,84.75 15.51705,85.646945 13.873283,86.422631 11.1875,85.78125 9.8394881,85.459337 8.2028336,84.217405 7.59375,82.78125 7.0049387,81.392895 7.0906258,80.244995 7.25,79.25 7.2509131,79.243426 7.2490698,79.22531 7.25,79.21875 7.2545589,79.190503 7.2453361,79.184234 7.25,79.15625 7.6967677,76.284597 9.79538,74.799045 11.6875,73.8125 13.604066,72.81321 15.834382,72.134925 18.4375,71.59375 22.423524,70.765077 27.286225,70.30831 32.625,70.25 z M 36,41.40625 C 48.323433,41.451084 60.005646,44.790359 78.65625,52.625 96.073307,60.034466 110.8791,60.83208 128.125,55.5625 L 128.125,55.59375 C 128.6731,55.415367 129.30034,55.231393 130.15625,55.25 131.05863,55.269617 132.3098,55.621246 133.21875,56.40625 135.03666,57.976258 135,59.711806 135,60.90625 135,62.12446 134.69505,63.596187 133.875,64.84375 133.05495,66.091313 131.87855,66.971821 130.6875,67.5625 128.31067,68.741256 125.59128,69.226623 121.53125,69.84375 121.52229,69.845112 121.50897,69.842387 121.5,69.84375 111.2911,71.45998 106.78042,71.588739 99.03125,70.875 L 99,70.875 C 89.403997,69.936261 81.497216,67.637082 68.625,62.0625 L 68.59375,62.0625 C 63.15099,59.675324 55.023518,56.775914 51.25,55.78125 42.42082,53.596711 28.112542,53.517609 19.71875,55.75 L 19.71875,55.78125 C 16.796373,56.550297 14.99496,56.979398 13.1875,57.0625 12.28377,57.104051 11.26421,57.090314 10.0625,56.5 8.8958372,55.926902 7.9576949,54.672899 7.59375,53.75 L 7.5625,53.75 C 7.5563901,53.734386 7.5684974,53.734492 7.5625,53.71875 7.5577678,53.706226 7.5357549,53.668639 7.53125,53.65625 L 7.5625,53.65625 C 7.0852223,52.373723 6.9449279,50.69409 7.5,49.25 8.0678924,47.772556 9.1207505,46.732717 10.1875,46 12.320307,44.535042 14.797616,43.848704 18.0625,43.25 24.6369,42.037373 30.399323,41.385874 36,41.40625 z M 35.0625,13.09375 C 35.073088,13.093667 35.083167,13.09383 35.09375,13.09375 48.308225,12.994196 57.956619,15.243702 73.1875,21.84375 L 73.21875,21.84375 C 87.211604,28.008434 95.674895,29.989847 106.3125,30 L 106.34375,30 C 111.19351,29.901994 117.60777,29.349136 119.0625,29 L 119.125,29 119.1875,29 C 124.73516,27.850484 127.18663,27.17862 129.625,27.15625 130.23459,27.150657 130.90922,27.191323 131.78125,27.5 132.65328,27.808677 133.73112,28.583131 134.28125,29.5 135.38149,31.333738 135,32.292803 135,32.8125 135,34.1625 134.38449,35.969606 133.28125,37.125 132.17801,38.280394 130.94761,38.857335 129.59375,39.34375 126.89256,40.314238 123.33559,40.938767 117.65625,41.84375 117.64347,41.845907 117.63778,41.8416 117.625,41.84375 108.99109,43.296191 101.87535,43.190783 92.78125,41.4375 92.768697,41.435202 92.762547,41.439804 92.75,41.4375 85.308094,40.071259 78.877431,37.79268 63.25,31.28125 45.989199,24.049071 32.13203,23.236124 14.3125,28.4375 13.714932,28.608234 13.166775,28.791024 12.375,28.875 11.583225,28.958976 10.078908,28.98292 8.6875,27.78125 7.2960921,26.57958 7.1182514,25.046297 7.09375,24.25 7.0715777,23.529401 7.1423522,23.030717 7.21875,22.5 7.2267755,22.444249 7.2419143,22.40035 7.25,22.34375 7.4845844,20.670249 8.795709,18.273256 10.53125,17.21875 12.323659,16.129691 14.157333,15.661088 17,14.9375 L 17.03125,14.9375 17.03125,14.90625 C 21.602484,13.763441 28.286026,13.095824 35.0625,13.09375 z";
            return DecodeSvgPath.Do(svg, 1).Select(poly => poly.Select(pt => (pt - p(71, 71)) / 14.2)).Triangulate().Select(e => e.Select(p => pt(p.X, 0, p.Y)).ToArray());
        }

        private static IEnumerable<VertexInfo[]> Button()
        {
            const int steps = 24;
            const double height = .1;
            const double outerRadius = .08;
            const double bottomRadius = .1;
            const double bevelRadius = .01;

            return CreateMesh(true, false,
                Enumerable.Range(0, steps).Select(i => i * 360.0 / steps).Select(angle2 =>
                    new MeshVertexInfo(pt(0, height, 0), pt(0, 1, 0))
                        .Concat(Enumerable.Range(0, steps).Select(i => 90 - i * (80.0) / steps).Select(angle => pt(outerRadius - bevelRadius + bevelRadius * cos(angle), height - bevelRadius + bevelRadius * sin(angle), 0, Normal.Average, Normal.Average, Normal.Average, Normal.Average)))
                        .Concat(pt(bottomRadius, 0, 0, Normal.Average, Normal.Average, Normal.Mine, Normal.Mine))
                        .Select(p => p.NormalOverride != null ? new MeshVertexInfo(p.Location.RotateY(angle2), p.NormalOverride.Value) : new MeshVertexInfo(p.Location.RotateY(angle2), p.NormalBeforeX, p.NormalAfterX, p.NormalBeforeY, p.NormalAfterY))
                        .ToArray())
                    .ToArray())
                .Select(face => face.Select(vi => new VertexInfo(vi.Location, vi.Normal, new PointD((-vi.Location.X + bottomRadius) / (2 * bottomRadius), (vi.Location.Z + bottomRadius) / (2 * bottomRadius)))).ToArray());
        }

        private static IEnumerable<Pt[]> ButtonHighlight()
        {
            const int numVertices = 24;
            const double innerRadius = .8;
            const double outerRadius = 1;

            return Enumerable.Range(0, numVertices)
                .Select(i => new PointD(cos(360.0 * i / numVertices), sin(360.0 * i / numVertices)))
                .SelectConsecutivePairs(true, (p1, p2) => new[] { pt(p1.X, 0, p1.Y) * outerRadius, pt(p2.X, 0, p2.Y) * outerRadius, pt(p2.X, 0, p2.Y) * innerRadius, pt(p1.X, 0, p1.Y) * innerRadius })
                .ToArray();
        }

        public static void DoGraphics()
        {
            var declarations = new List<string>();
            var threads = new List<Thread>();
            foreach (var name in new string[] { "SqWater", "SqShip", "SqShipL", "SqShipR", "SqShipT", "SqShipB", "SqShipF", "SqShipA" })
            {
                var thr = new Thread(() =>
                {
                    File.WriteAllBytes($@"D:\temp\temp-{name}.png", File.ReadAllBytes($@"D:\c\KTANE\Battleship\Assets\Misc\{name}.svg.png"));
                    CommandRunner.RunRaw($@"pngcr D:\c\KTANE\Battleship\Assets\Misc\{name}.svg.png D:\c\KTANE\Battleship\Assets\Misc\{name}.png").Go();
                    lock (declarations)
                        declarations.Add($@"{{ ""{name}"", new byte[] {{ {File.ReadAllBytes($@"D:\c\KTANE\Battleship\Assets\Misc\{name}.png").JoinString(", ")} }} }}");
                });
                thr.Start();
                threads.Add(thr);
            }
            foreach (var th in threads)
                th.Join();

            if (declarations.Count > 0)
                File.WriteAllText(@"D:\c\KTANE\Battleship\Assets\RawPngs.cs", $@"
using System.Collections.Generic;
namespace Battleship {{
    public static class RawPngs {{
        public static Dictionary<string, byte[]> RawBytes = new Dictionary<string, byte[]> {{
            {declarations.JoinString(",\r\n            ")}
        }};
    }}
}}");
        }

        public static void GeneratePuzzle()
        {
            var theLog = new List<string>();
            var log = Ut.Lambda((string msg) =>
            {
                Console.WriteLine(msg);
                //File.AppendAllLines(@"D:\temp\temp.txt", new[] { msg });
                theLog.Add(msg);
            });

            var size = 6;
            var rnd = new Random(49);
            var nonUnique = 0;
            var hints = Enumerable.Range(0, size * size).ToList().Shuffle(rnd).Take(rnd.Next(2, 3)).ToArray();
            log($"There are {hints.Length} hints.");

            retry:
            var ships = new[] { rnd.Next(3, 5), rnd.Next(2, 4), rnd.Next(2, 4), rnd.Next(1, 4), rnd.Next(1, 3), 1 }.OrderByDescending(x => x).ToArray();
            log("Ships: " + ships.JoinString(", "));
            var anyHypothesis = false;

            //Console.WriteLine("Retrying.");
            var grid = Ut.NewArray(size, size, (x, y) => (bool?) null);

            var availableShips = ships.ToList();
            while (availableShips.Count > 0)
            {
                var ix = rnd.Next(availableShips.Count);
                var shipLen = availableShips[ix];
                availableShips.RemoveAt(ix);
                var positions = Enumerable.Range(0, size * size * 2).Select(i =>
                {
                    var horiz = (i & 1) == 1;
                    var x = (i >> 1) % size;
                    var y = (i >> 1) / size;
                    if (horiz && x + shipLen >= size || !horiz && y + shipLen >= size)
                        return null;
                    for (int j = 0; j < shipLen; j++)
                        if (grid[horiz ? x + j : x][horiz ? y : y + j] != null)
                            return null;
                    return new { X = x, Y = y, Horiz = horiz };
                }).Where(inf => inf != null).ToArray();
                if (positions.Length == 0)
                {
                    //log("Must retry.");
                    Console.WriteLine("Retrying because no fit.");
                    goto retry;
                }
                var pos = positions.PickRandom(rnd);
                for (int j = -1; j <= shipLen; j++)
                    if ((pos.Horiz ? pos.X + j : pos.Y + j).Apply(ps => ps >= 0 && ps < size))
                    {
                        if ((pos.Horiz ? pos.Y : pos.X) > 0)
                            grid[pos.Horiz ? pos.X + j : pos.X - 1][pos.Horiz ? pos.Y - 1 : pos.Y + j] = false;
                        grid[pos.Horiz ? pos.X + j : pos.X][pos.Horiz ? pos.Y : pos.Y + j] = j >= 0 && j < shipLen;
                        if ((pos.Horiz ? pos.Y : pos.X) < size - 1)
                            grid[pos.Horiz ? pos.X + j : pos.X + 1][pos.Horiz ? pos.Y + 1 : pos.Y + j] = false;
                    }
            }

            var rowCounts = Enumerable.Range(0, size).Select(row => Enumerable.Range(0, size).Count(col => grid[col][row] == true)).ToArray();
            var colCounts = Enumerable.Range(0, size).Select(col => Enumerable.Range(0, size).Count(row => grid[col][row] == true)).ToArray();
            var output = Ut.Lambda(() =>
            {
                log("   " + Enumerable.Range(0, size).Select(col => colCounts[col].ToString().PadLeft(2)).JoinString());
                log(Enumerable.Range(0, size).Select(row => rowCounts[row].ToString().PadLeft(3) + " " + Enumerable.Range(0, size).Select(col => hints.Contains(col + row * size) ? (grid[col][row] == true ? "% " : "• ") : grid[col][row] == null ? "? " : grid[col][row].Value ? "# " : "· ").JoinString()).JoinString("\n"));
            });

            log("Intended solution:");
            output();

            grid = Ut.NewArray(size, size, (x, y) => hints.Contains(x + y * size) ? grid[x][y] ?? false : (bool?) null);

            var rowsDone = new bool[size];
            var colsDone = new bool[size];
            var hypotheses = new[] { new { X = 0, Y = 0, Grid = (bool?[][]) null, RowsDone = (bool[]) null, ColsDone = (bool[]) null } }.ToStack();
            hypotheses.Pop();
            bool?[][] solution = null;

            nextIter:
            if (rowsDone.All(b => b) && colsDone.All(b => b))
                goto tentativeSolution;

            // Diagonal from a true is a false
            for (int c = 0; c < size; c++)
                for (int r = 0; r < size; r++)
                    if (grid[c][r] == true)
                    {
                        if (r > 0 && c > 0)
                            grid[c - 1][r - 1] = false;
                        if (r > 0 && c < size - 1)
                            grid[c + 1][r - 1] = false;
                        if (r < size - 1 && c > 0)
                            grid[c - 1][r + 1] = false;
                        if (r < size - 1 && c < size - 1)
                            grid[c + 1][r + 1] = false;
                    }

            // Check if a row can be filled in unambiguously
            for (int r = 0; r < size; r++)
                if (!rowsDone[r])
                {
                    var cnt = Enumerable.Range(0, size).Count(c => grid[c][r] != false);
                    if (cnt < rowCounts[r])
                    {
                        log($"Contradiction: row {r} has too few available spaces.");
                        goto contradiction;
                    }
                    if (cnt == rowCounts[r])
                    {
                        for (int c = 0; c < size; c++)
                            if (grid[c][r] == null)
                                grid[c][r] = true;
                        rowsDone[r] = true;
                        log($"Deduced row {r}");
                        goto nextIter;
                    }

                    cnt = Enumerable.Range(0, size).Count(c => grid[c][r] == true);
                    if (cnt > rowCounts[r])
                    {
                        log($"Contradiction: row {r} has too many assigned spaces.");
                        goto contradiction;
                    }
                    if (cnt == rowCounts[r])
                    {
                        for (int c = 0; c < size; c++)
                            if (grid[c][r] == null)
                                grid[c][r] = false;
                        rowsDone[r] = true;
                        log($"Deduced row {r}");
                        goto nextIter;
                    }
                }

            // Check if a column can be filled in unambiguously
            for (int c = 0; c < size; c++)
                if (!colsDone[c])
                {
                    var cnt = Enumerable.Range(0, size).Count(r => grid[c][r] != false);
                    if (cnt < colCounts[c])
                    {
                        log($"Contradiction: column {c} has too few available spaces.");
                        goto contradiction;
                    }
                    if (cnt == colCounts[c])
                    {
                        for (int r = 0; r < size; r++)
                            if (grid[c][r] == null)
                                grid[c][r] = true;
                        colsDone[c] = true;
                        log($"Deduced column {c}");
                        goto nextIter;
                    }

                    cnt = Enumerable.Range(0, size).Count(r => grid[c][r] == true);
                    if (cnt > colCounts[c])
                    {
                        log($"Contradiction: column {c} has too many assigned spaces.");
                        goto contradiction;
                    }
                    if (cnt == colCounts[c])
                    {
                        for (int r = 0; r < size; r++)
                            if (grid[c][r] == null)
                                grid[c][r] = false;
                        colsDone[c] = true;
                        log($"Deduced column {c}");
                        goto nextIter;
                    }
                }

            // No obvious deduction: Try a hypothesis
            log("No obvious deduction: trying a hypothesis");
            anyHypothesis = true;
            var unfinishedCol = colsDone.IndexOf(false);
            var unfinishedRow = grid[unfinishedCol].IndexOf((bool?) null);
            hypotheses.Push(new { X = unfinishedCol, Y = unfinishedRow, Grid = Ut.NewArray(size, size, (x, y) => grid[x][y]), RowsDone = (bool[]) rowsDone.Clone(), ColsDone = (bool[]) colsDone.Clone() });
            log($"Hypothesis is X={unfinishedCol} Y={unfinishedRow}, trying ship");
            grid[unfinishedCol][unfinishedRow] = true;
            goto nextIter;

            contradiction:
            if (hypotheses.Count == 0)
            {
                if (solution != null)
                    goto uniqueSolutionFound;
                log("This puzzle is impossible?");
                Debugger.Break();
                throw new InvalidOperationException();
            }
            var prevHypo = hypotheses.Pop();
            // Try the opposite hypothesis
            log($"Backtracking from hypothesis X={prevHypo.X} Y={prevHypo.Y}, trying water");
            grid = prevHypo.Grid;
            rowsDone = prevHypo.RowsDone;
            colsDone = prevHypo.ColsDone;
            grid[prevHypo.X][prevHypo.Y] = false;
            goto nextIter;

            tentativeSolution:
            if (!anyHypothesis)
            {
                log("Retrying because too trivial.");
                goto retry;
            }
            // Found a tentative solution. Check that it’s correct

            // Find unaccounted-for ships
            var unaccountedFor = ships.OrderByDescending(x => x).ToList();
            for (int x = 0; x < size; x++)
                for (int y = 0; y < size; y++)
                {
                    int? thisLen = null;
                    if (grid[x][y] == true && (x == 0 || grid[x - 1][y] == false) && (x == size - 1 || grid[x + 1][y] == false) && (y == 0 || grid[x][y - 1] == false) && (y == size - 1 || grid[x][y + 1] == false))
                        thisLen = 1;
                    if (thisLen == null && grid[x][y] == true && (x == 0 || grid[x - 1][y] == false))
                    {
                        var len = 0;
                        while (x + len < size && grid[x + len][y] == true)
                            len++;
                        if (len > 1 && (x + len == size || grid[x + len][y] == false))
                            thisLen = len;
                    }
                    if (thisLen == null && grid[x][y] == true && (y == 0 || grid[x][y - 1] == false))
                    {
                        var len = 0;
                        while (y + len < size && grid[x][y + len] == true)
                            len++;
                        if (len > 1 && (y + len == size || grid[x][y + len] == false))
                            thisLen = len;
                    }
                    if (thisLen != null)
                    {
                        if (!unaccountedFor.Remove(thisLen.Value))
                        {
                            log($"Too many length {thisLen.Value} ships.");
                            goto contradiction;
                        }
                    }
                }
            if (unaccountedFor.Count > 0)
            {
                log($"Ship lengths {unaccountedFor.JoinString(", ")} unaccounted for.");
                goto contradiction;
            }

            // Actually found a solution
            if (solution != null)
            {
                // The puzzle is not unique.
                log("Solution is not unique:");
                output();
                nonUnique++;
                goto retry;
            }

            log("Found a solution:");
            output();
            solution = Ut.NewArray(size, size, (i, j) => grid[i][j]);
            goto contradiction;

            uniqueSolutionFound:
            log("Unique solution found.");
            grid = solution;
            output();
            log("Non-unique puzzles I had to discard: " + nonUnique);
            File.WriteAllLines(@"D:\temp\temp.txt", theLog);

            grid = Ut.NewArray(size, size, (x, y) => hints.Contains(x + y * size) ? grid[x][y] ?? false : (bool?) null);
            Console.WriteLine();
            Console.WriteLine("PUZZLE FOR YOU:");
            Console.WriteLine("   " + Enumerable.Range(0, size).Select(col => colCounts[col].ToString().PadLeft(2)).JoinString());
            Console.WriteLine(Enumerable.Range(0, size).Select(row => rowCounts[row].ToString().PadLeft(3) + " " + Enumerable.Range(0, size).Select(col => hints.Contains(col + row * size) ? (grid[col][row] == true ? "% " : "• ") : grid[col][row] == null ? "? " : grid[col][row].Value ? "# " : "· ").JoinString()).JoinString("\n"));
            Console.WriteLine();
            Console.WriteLine(ships.JoinString(", "));
        }
    }
}
