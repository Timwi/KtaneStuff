using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using RT.Coordinates;
using RT.TagSoup;
using RT.Util;
using RT.Util.ExtensionMethods;

namespace KtaneStuff
{
    static class CrazyMaze
    {
        public static void Generate()
        {
            const double p3Factor = .9;
            const double pw = 18;
            const double ph = 24;
            const double chamfFactor = .7;
            var chamfOffset = new PointD(0, -.5);

            var denomsAtRadius = new[] { 3, 8, 12, 24 };
            var offsetsAtRadius = new CircleFraction[] { CircleFraction.Zero, new CircleFraction(1, 16), CircleFraction.Zero, new CircleFraction(1, 48) };

            var circularCells = Enumerable.Range(0, 4).SelectMany(radius => denomsAtRadius[radius].Apply(d => Enumerable.Range(0, d).Select(i =>
                new CircularCell(radius, new CircleFraction(i, d) + offsetsAtRadius[radius], new CircleFraction(i + 1, d) + offsetsAtRadius[radius]))));

            var info = Ut.NewArray<(string name, Type cellType, Type vertexType, IEnumerable<object> cells)>(
                ("Hex", typeof(Hex), typeof(Hex.Vertex), Hex.LargeHexagon(4).Cast<object>()),
                ("Cairo", typeof(Cairo), typeof(Cairo.Vertex), Cairo.Rectangle(4, 4).Cast<object>()),
                ("Penrose P3", typeof(Penrose), typeof(Penrose.Vertex), Enumerable.Range(0, 6)
                    .Select(a => new Penrose(Penrose.Kind.ThickRhomb, default, 2 * a))
                    .SelectMany(c => c.DeflatedTiles).Distinct().SelectMany(c => c.DeflatedTiles).Distinct().SelectMany(c => c.DeflatedTiles).Distinct()
                    .Where(c => c.Vertices.All(v => v.Point.Distance < ph / 6 / p3Factor)).Cast<object>()),
                ("Square", typeof(Square), typeof(Square.Vertex), Square.Rectangle(9, 8).Cast<object>()),
                ("OctoCell", typeof(OctoCell), typeof(OctoCell.Vertex), OctoCell.Rectangle(7, 6).Cast<object>()),
                ("Rhombihexadel", typeof(Rhombihexadel), typeof(Rhombihexadel.Vertex), Rhombihexadel.LargeHexagon(2).Cast<object>()),
                ("Floret", typeof(Floret), typeof(Floret.Vertex), Floret.LargeHexagon(2).Cast<object>()),
                ("Circular", typeof(CircularCell), typeof(CircularCell.Vertex), circularCells.Cast<object>()),
                ("Tri", typeof(Tri), typeof(Tri.Vertex), Enumerable.Range(0, 6).SelectMany(y => Enumerable.Range(0, 11).Select(x => new Tri(x, y))).Cast<object>()),
                ("Chamf", typeof(Chamf), typeof(Chamf.Vertex), Chamf.Rectangle(4, 4).Cast<object>()),
                ("Kite", typeof(Kite), typeof(Kite.Vertex), Kite.LargeHexagon(2).Cast<object>()),
                ("Rhomb", typeof(Rhomb), typeof(Rhomb.Vertex), Rhomb.LargeHexagon(3).Cast<object>())
            );
            var allCells = info.SelectMany(inf => inf.cells).ToArray();

            foreach (var cell in allCells)
            {
                var str = cell.ToString();
                var parsed = GridUtils.Parse(str);
                if (!parsed.Equals(cell))
                    Debugger.Break();
            }

            IEnumerable<object> GetNeighbors(object cell) => cell is CircularCell cc ? cc.FindNeighbors(circularCells) : ((INeighbor<object>) cell).Neighbors;
            var structure = new Structure<object>(allCells, getNeighbors: GetNeighbors);

            void addLink(string str1, string str2) => structure.AddLink(GridUtils.Parse(str1), GridUtils.Parse(str2));

            addLink("C(0,0)/3", "H(0,-3)");
            addLink("C(0,1)/0", "H(1,-3)");
            addLink("C(0,1)/3", "H(2,-3)");
            addLink("C(0,2)/0", "H(3,-3)");
            addLink("C(0,2)/3", "H(3,-2)");
            addLink("M(-2,1)/1", "H(3,-1)");
            addLink("H(-2,3)", "O(0,0)");
            addLink("H(-1,3)", "O(1,0)");
            addLink("O(2,0)", "H(0,3)");
            addLink("O(3,0)", "H(1,2)");
            addLink("O(4,0)", "H(2,1)");
            addLink("O(5,0)", "H(3,0)");
            addLink("P(1,-1,0,-2)/2/9", "C(3,0)/1");
            addLink("P(1,-2,-1,-2)/3/2", "C(3,0)/2");
            addLink("P(0,-2,-1,-2)/2/2", "C(3,1)/1");
            addLink("P(0,-2,-1,-2)/2/4", "C(3,1)/2");
            addLink("P(-1,-1,-1,0)/3/8", "C(3,2)/1");
            addLink("P(-1,-1,-1,0)/2/7", "C(3,2)/2");
            addLink("P(-3,0,-2,1)/3/0", "C(3,3)/1");
            addLink("C(0,3)/3", "O(6,0)");
            addLink("M(-2,0)/1", "C(0,3)/2");
            addLink("M(-1,-1)/4", "C(1,3)/3");
            addLink("M(-1,-1)/3", "C(1,3)/2");
            addLink("M(-1,-1)/1", "C(2,3)/3");
            addLink("M(0,-2)/4", "C(2,3)/2");
            addLink("M(0,-2)/3", "C(3,3)/3");
            addLink("F(0,-1)/5", "C(3,3)/2");
            addLink("S(0,0)", "P(2,0,1,-1)/2/1");
            addLink("S(0,1)", "P(2,0,1,-1)/3/2");
            addLink("S(0,2)", "P(2,1,2,0)/2/8");
            addLink("S(0,3)", "P(2,1,2,0)/2/6");
            addLink("S(0,4)", "P(1,2,1,1)/3/6");
            addLink("S(0,5)", "P(0,1,1,1)/2/3");
            addLink("S(0,6)", "P(0,1,1,1)/3/4");
            addLink("M(0,-2)/2", "P(-3,0,-2,2)/2/0");
            addLink("F(0,-1)/0", "P(-2,1,-1,2)/3/6");
            addLink("F(0,-1)/1", "P(-2,1,-1,2)/2/5");
            addLink("F(1,-1)/5", "P(-2,1,0,3)/3/8");
            addLink("F(1,-1)/0", "P(-2,2,0,3)/2/8");
            addLink("P(-2,2,0,3)/2/0", "C(3;41/48→43/48)");
            addLink("S(0,7)", "F(1,-1)/1");
            addLink("C(3;43/48→15/16)", "S(1,7)");
            addLink("C(3;15/16→47/48)", "S(2,7)");
            addLink("C(3;47/48→1/48)", "S(3,7)");
            addLink("C(3;1/48→1/16)", "S(4,7)");
            addLink("C(3;1/16→5/48)", "S(5,7)");
            addLink("C(3;5/48→7/48)", "S(6,7)");
            addLink("C(3;7/48→3/16)", "S(7,7)");
            addLink("C(3;3/16→11/48)", "S(8,7)");
            addLink("M(-1,0)/5", "O(6,1)");
            addLink("M(-2,1)/2", "O(6,2)");
            addLink("O(6,3)", "M(-1,1)/4");
            addLink("O(6,4)", "M(0,1)/5");
            addLink("O(0,5)", "T(0,0)");
            addLink("O(1,5)", "T(1,0)");
            addLink("O(2,5)", "T(3,0)");
            addLink("T(5,0)", "O(3,5)");
            addLink("T(7,0)", "O(4,5)");
            addLink("T(9,0)", "O(5,5)");
            addLink("H(0,0)/2", "O(6,5)");
            addLink("F(0,-1)/4", "M(1,-2)/5");
            addLink("F(-1,0)/0", "M(1,-2)/3");
            addLink("F(-1,0)/5", "M(1,-2)/2");
            addLink("M(2,-2)/5", "F(-1,0)/4");
            addLink("M(2,-2)/4", "F(-1,0)/3");
            addLink("M(2,-1)/5", "F(-1,1)/5");
            addLink("M(0,1)/4", "T(10,0)");
            addLink("M(0,1)/3", "H(0,0)/1");
            addLink("M(0,1)/2", "H(1,0)/1");
            addLink("M(1,0)/2", "H(2,0)/1");
            addLink("M(1,0)/1", "H(3,0)/1");
            addLink("K(-1,1)/5", "M(2,-1)/4");
            addLink("K(-1,0)/4", "M(1,-1)/1");
            addLink("C(3;13/16→41/48)", "F(1,-1)/2");
            addLink("C(3;37/48→13/16)", "F(1,0)/0");
            addLink("C(3;35/48→37/48)", "F(1,0)/1");
            addLink("C(3;11/16→35/48)", "F(1,0)/2");
            addLink("F(1,0)/3", "C(3;31/48→11/16)");
            addLink("F(0,1)/1", "C(3;29/48→31/48)");
            addLink("F(-1,1)/4", "H(4,0)/2");
            addLink("K(-1,0)/5", "F(-1,1)/3");
            addLink("K(-1,0)/0", "F(-1,1)/2");
            addLink("F(0,1)/4", "K(0,-1)/4");
            addLink("F(0,1)/3", "K(0,-1)/5");
            addLink("F(0,1)/2", "R(-2,0)/2");
            addLink("K(0,-1)/0", "C(3;9/16→29/48)");
            addLink("R(-2,0)/0", "C(3;25/48→9/16)");
            addLink("R(-1,-1)/2", "C(3;23/48→25/48)");
            addLink("R(-1,-1)/0", "C(3;7/16→23/48)");
            addLink("R(0,-2)/2", "C(3;19/48→7/16)");
            addLink("R(0,-2)/0", "C(3;17/48→19/48)");
            addLink("R(1,-2)/0", "C(3;5/16→17/48)");
            addLink("H(0,1)/2", "T(10,2)");
            addLink("T(10,3)", "H(0,2)/2");
            addLink("T(10,4)", "H(0,3)/2");
            addLink("T(10,5)", "H(0,4)/1");
            addLink("K(-1,1)/4", "H(4,1)/2");
            addLink("H(4,2)/2", "K(-1,1)/3");
            addLink("H(4,3)/2", "K(0,1)/4");
            addLink("H(3,4)/1", "K(0,1)/3");
            addLink("R(-2,1)/2", "K(0,-1)/1");
            addLink("R(-2,2)/2", "K(1,-1)/0");
            addLink("K(1,-1)/1", "R(-2,2)/1");
            addLink("K(1,-1)/2", "R(-1,2)/2");
            addLink("K(1,0)/0", "R(-1,2)/1");
            addLink("K(1,0)/1", "R(0,2)/2");

            var mid = new PointD(pw / 4, ph / 6);

            PointD GetVertexPoint(Vertex v) => (
                v is Hex.Vertex ? v.Point.RotateDeg(30) * 1.4 + mid :
                v is Floret.Vertex ? v.Point.RotateDeg(-30) + mid :
                v is Square.Vertex ? (v.Point - mid) * .95 + mid :
                v is Cairo.Vertex ? (v.Point + new PointD(.5, 0) - mid) * .9 + mid :
                v is OctoCell.Vertex ? v.Point * 1.25 + new PointD(.125, .25) :
                v is Tri.Vertex ? (v.Point + new PointD(.75, 0.10288568297002608956324573161177) - mid) * .975 + mid :
                v is Kite.Vertex ? v.Point.RotateDeg(30) * 1.5 + mid :
                v is Rhomb.Vertex ? v.Point.RotateDeg(30) * .95 + mid :
                v is Rhombihexadel.Vertex ? v.Point.RotateDeg(30) * .9 + mid :
                v is Penrose.Vertex ? v.Point * p3Factor + mid :
                v is CircularCell.Vertex ? v.Point + mid :
                v is Chamf.Vertex ? (v.Point - mid + chamfOffset) * chamfFactor + mid :
                v.Point
            ) + info.IndexOf(tup => tup.vertexType.Equals(v.GetType())).Apply(ix => new PointD(pw / 2 * (ix % 4), ph / 3 * (ix / 4)));

            IEnumerable<Link<Vertex>> GetEdges(object c) => c is CircularCell cc ? cc.FindEdges(circularCells) : ((IHasSvgGeometry) c).Edges;

            PointD? GetCenter(object c) => (
                    c is Hex h ? h.Center.RotateDeg(30) * 1.4 + mid :
                    c is Floret f ? f.Center.RotateDeg(-30) + mid :
                    c is Square co ? (co.Center - mid) * .95 + mid :
                    c is Cairo ca ? (ca.Center + new PointD(.5, 0) - mid) * .9 + mid :
                    c is OctoCell o ? o.Center * 1.25 + new PointD(.125, .25) :
                    c is Tri t ? (t.Center + new PointD(.75, 0.10288568297002608956324573161177) - mid) * .975 + mid :
                    c is Kite k ? k.Center.RotateDeg(30) * 1.5 + mid :
                    c is Rhomb r ? r.Center.RotateDeg(30) * .95 + mid :
                    c is Rhombihexadel rh ? rh.Center.RotateDeg(30) * .9 + mid :
                    c is Penrose p ? p.Center * p3Factor + mid :
                    c is CircularCell cc ? cc.Center + mid :
                    c is Chamf ch ? (ch.Center - mid + chamfOffset) * chamfFactor + mid :
                    null
                ) + info.IndexOf(tup => tup.cellType.Equals(c.GetType())).Apply(ix => new PointD(pw / 2 * (ix % 4), ph / 3 * (ix / 4)));

            string encode(int ix) => $"{(char) ('A' + ix / 26)}{(char) ('A' + ix % 26)}";
            var svgInstructions = new SvgInstructions
            {
                GetVertexPoint = GetVertexPoint,
                GetEdges = GetEdges,
                GetCenter = GetCenter,

                SvgAttributes = null,
                PerCell = c => $"<text y='.14' id='cell-label-{allCells.IndexOf(c)}'>{encode(allCells.IndexOf(c))}</text>",

                OutlinePath = d => $"<path d='{d}' class='outline' />",

                PassagesSeparate = true,
                PassagesPaths = (d, c1, c2) => $"<path id='wall-{allCells.IndexOf(c1)}-{allCells.IndexOf(c2)}' class='wall' d='{d}' />",
                BridgeSvg = (c1, c2, d) => $"<g id='bridge-{allCells.IndexOf(c1)}-{allCells.IndexOf(c2)}' class='bridge'><path d='{d}' class='bridge-out' /><path d='{d}' class='bridge-in' /></g>",

                Precision = 2
            };
            var manualSvg = structure.Svg(svgInstructions);

            var highlightables = allCells.Select(cell => $"<path d='{GridUtils.SvgEdgesPath(GetEdges(cell), GetVertexPoint, svgInstructions.Round)}' class='highlightable' />").JoinString();
            Utils.ReplaceInFile(@"D:\c\KTANE\Public\HTML\Crazy Maze.html", "<!--%%-->", "<!--%%%-->", manualSvg);
            Utils.ReplaceInFile(@"D:\c\KTANE\Public\HTML\Crazy Maze.html", "<!--@@-->", "<!--@@@-->", highlightables);
            Utils.ReplaceInFile(@"D:\c\KTANE\Public\HTML\Crazy Maze.html", "<!--##-->", "<!--###-->", highlightables);
            Utils.ReplaceInFile(@"D:\c\KTANE\Public\HTML\Crazy Maze.html", "/*!*/", "/*!!*/", allCells.Length.ToString());
            var dic = Ut.NewArray(allCells.Length, _ => new List<int>());
            foreach (var lnk in structure.Links)
                foreach (var cell in lnk)
                    dic[allCells.IndexOf(cell)].Add(allCells.IndexOf(lnk.Other(cell)));
            Utils.ReplaceInFile(@"D:\c\KTANE\Public\HTML\Crazy Maze.html", "/*@*/", "/*@@*/", dic.Select(kvp => $@"[{kvp.Order().JoinString(",")}]").JoinString(","));

            // Find bounds of each cell
            (PointD min, PointD max) getBounds(IEnumerable<Link<Vertex>> edges)
            {
                var minX = edges.SelectMany(lnk => lnk).Min(v => GetVertexPoint(v).X);
                var maxX = edges.SelectMany(lnk => lnk).Max(v => GetVertexPoint(v).X);
                var minY = edges.SelectMany(lnk => lnk).Min(v => GetVertexPoint(v).Y);
                var maxY = edges.SelectMany(lnk => lnk).Max(v => GetVertexPoint(v).Y);
                return (new PointD(minX, minY), new PointD(maxX, maxY));
            }

            double maxWidth = 0, maxHeight = 0;

            for (var i = 0; i < allCells.Length; i++)
            {
                var (min, max) = getBounds(GetEdges(allCells[i]));
                maxWidth = Math.Max(maxWidth, max.X - min.X);
                maxHeight = Math.Max(maxHeight, max.Y - min.Y);
            }
            var cellSize = Math.Max(maxWidth, maxHeight);

            var bridgeTransitions = new Dictionary<int, int>();
            foreach (var (lnkCell1, lnkCell2) in structure.Links)
                foreach (var (fromC, toC) in new[] { (lnkCell1, lnkCell2), (lnkCell2, lnkCell1) })
                    if (GetEdges(fromC).ToArray().IndexOf(GetEdges(toC).ToArray().Contains) == -1)
                        bridgeTransitions[allCells.IndexOf(fromC)] = allCells.IndexOf(toC);

            var transitions = new Dictionary<int, List<string>>();

            for (var cellIx = 0; cellIx < allCells.Length; cellIx++)
            {
                var (min, max) = getBounds(GetEdges(allCells[cellIx]));
                var edges = GetEdges(allCells[cellIx]).ToArray();
                var arrows = new List<string>();
                var fIx = 0;
                var fAngle = double.MaxValue;
                for (var edgeIx = 0; edgeIx < edges.Length; edgeIx++)
                {
                    var toCell = Enumerable.Range(0, allCells.Length).Where(ix => ix != cellIx && GetEdges(allCells[ix]).Contains(edges[edgeIx])).FirstOrNull();
                    var (v1, v2) = edges[edgeIx];
                    var p1 = GetVertexPoint(v1);
                    var p2 = GetVertexPoint(v2);
                    var center = GetCenter(allCells[cellIx]).Value;

                    var fAng = Math.Atan2(p1.X - center.X - 0.001, p1.Y - center.Y);    // swapping X and Y to get the -π/π threshold to point up
                    if (fAng < fAngle)
                    {
                        fAngle = fAng;
                        fIx = edgeIx;
                    }

                    PointD midP;
                    double angle;
                    if (v1 is CircularCell.Vertex cv1 && v2 is CircularCell.Vertex cv2 && cv1.Radius == cv2.Radius)
                    {
                        var offset = p1 - cv1.Point;

                        var gc = Ut.Gcd(cv2.Position.Denominator, cv1.Position.Denominator);
                        var n1 = cv2.Position.Numerator * cv1.Position.Denominator / gc;
                        var n2 = cv1.Position.Numerator * cv2.Position.Denominator / gc;
                        var cd = cv2.Position.Denominator * cv1.Position.Denominator / gc;

                        var nMin = Math.Min(n1, n2);
                        var nMax = Math.Max(n1, n2);
                        double midPos;
                        if (2 * (nMax - nMin) > cd)
                            midPos = (nMin + nMax + cd) * .5 / cd;
                        else
                            midPos = (nMin + nMax) * .5 / cd;

                        midP = offset + new PointD(
                            cv1.Radius * Math.Cos(Math.PI * (2d * midPos - .5)),
                            cv1.Radius * Math.Sin(Math.PI * (2d * midPos - .5)));
                    }
                    else
                        midP = (p1 + p2) / 2;
                    midP -= (max + min) / 2;
                    midP *= 0.128572 / cellSize;
                    angle = Math.Atan2(p2.Y - p1.Y, p2.X - p1.X) * 180 / Math.PI;
                    arrows.Add($"new Transition({toCell?.ToString() ?? "null"}, {midP.X}f, {midP.Y}f, {angle}f)");
                }
                if (fIx > 0)
                {
                    arrows.AddRange(arrows.Take(fIx).ToArray());
                    arrows.RemoveRange(0, fIx);
                }
                transitions[cellIx] = arrows;
            }

            if (transitions.Keys.Any(k => k != 0 && !transitions.ContainsKey(k - 1)))
                Debugger.Break();

            // Generate sprites
            var knownSpriteShapes = new Dictionary<string, (int shapeId, int representativeCellIx)>();
            var cellToSpriteShape = new int[allCells.Length];
            var nextSprite = 0;
            for (var cellIx = 0; cellIx < allCells.Length; cellIx++)
            {
                var edges = GetEdges(allCells[cellIx]).ToArray();
                var (min, max) = getBounds(edges);
                var midP = (min + max) / 2;

                // Deduplicate identical shapes
                var str = edges.ConsecutivePairs(true)
                    .Select(pair => GetVertexPoint(pair.Item1.Intersect(pair.Item2).First()) - midP)
                    .Select(v => $"({v.X:0.00},{v.Y:0.00})")
                    .JoinString();
                if (knownSpriteShapes.TryGetValue(str, out var tup))
                    cellToSpriteShape[cellIx] = tup.shapeId;
                else
                {
                    int shapeId = cellToSpriteShape[cellIx] = nextSprite;
                    knownSpriteShapes[str] = (shapeId, cellIx);
                    nextSprite++;
                }
            }

            knownSpriteShapes.Values.ParallelForEach(Environment.ProcessorCount, (tup, proc) =>
            {
                var (shapeId, cellIx) = tup;

                const int pngSize = 500;
                if (File.Exists($@"D:\c\KTANE\CrazyMaze\Assets\CellSprites\shape-{shapeId}.png"))
                    return;

                var edges = GetEdges(allCells[cellIx]).ToArray();
                var (min, max) = getBounds(edges);
                var midP = (min + max) / 2;
                double leftX = midP.X - cellSize / 2, topY = midP.Y - cellSize / 2;
                var strc = new Structure<object>(new[] { allCells[cellIx] }, getNeighbors: c => Enumerable.Empty<object>());
                File.WriteAllText($@"D:\temp\temp-{proc}.svg", strc.Svg(new SvgInstructions
                {
                    SvgAttributes = $"xmlns='http://www.w3.org/2000/svg' viewBox='{leftX} {topY} {cellSize} {cellSize}' font-size='.2' text-anchor='middle'",
                    OutlinePath = d => $"<path fill='white' stroke='none' d='{d}' />",
                    GetEdges = GetEdges,
                    GetCenter = GetCenter,
                    GetVertexPoint = GetVertexPoint
                }));
                var cmd = $@"D:\Inkscape\bin\inkscape.exe ""--export-filename=D:\c\KTANE\CrazyMaze\Assets\CellSprites\shape-{shapeId}.png"" --export-width={pngSize} --export-height={pngSize} ""D:\temp\temp-{proc}.svg""";
                lock (allCells)
                    Console.WriteLine(cmd);
                CommandRunner.RunRaw(cmd).Go();
            });

            Utils.ReplaceInFile(@"D:\c\KTANE\CrazyMaze\Assets\CellTransitions.cs", "/*!*/", "/*!!*/",
                Enumerable.Range(0, allCells.Length).Select(cellIx =>
                {
                    var bridgeValue = bridgeTransitions.TryGetValue(cellIx, out var bt) ? bt.ToString() : "null";
                    var transitionStrings = transitions[cellIx].JoinString(", ");
                    return $"new CellTransitions({bridgeValue}, {cellToSpriteShape[cellIx]}, new[] {{ {transitionStrings} }})";
                }).JoinString(",\r\n"));

            //*      — ## Test SVG output
            var rnd = new Random(347);
            var svg = structure.Svg(new SvgInstructions
            {
                GetVertexPoint = GetVertexPoint,
                GetEdges = GetEdges,
                GetCenter = GetCenter,
                SvgAttributes = "xmlns='http://www.w3.org/2000/svg' viewBox='-1 -1 38 26' font-size='.2' text-anchor='middle'",
                PerCell = c => $"<text font-size='.2' y='.07' text-anchor='middle' font-family='Agency FB' stroke='white' paint-order='stroke' stroke-width='.1'>{cellToSpriteShape[allCells.IndexOf(c)]}</text>",// $"<circle r='.1' fill='black' fill-opacity='.2' />",
                ExtraSvg1 = "<g fill='none' stroke='black' stroke-width='.05'><rect x='0' y='0' width='18' height='24' /><rect x='18' y='0' width='18' height='24' /></g>" +
                    "<path fill='none' stroke='black' stroke-width='.02' d='M0 8h36M0 16h36M9 0v24M27 0v24' />",
                ExtraSvg3 = info.Select((tup, ix) => $"<text x='{(ix % 4) * pw / 2 + .1}' y='{(ix / 4) * ph / 3 + (.5 * .7) + .1}' font-size='.5' text-anchor='start' stroke='hsl(60, 80%, 90%)' stroke-width='.1' stroke-linejoin='round' paint-order='stroke'>{tup.name} ({tup.cells.Count()})</text>").JoinString()
            });

            File.WriteAllText(@"D:\temp\temp.html", $@"
                <html>
                    <head>
                        <title>Crazy Maze planning</title>
                        <style>
                            svg {{ width: 100%; }}
                        </style>
                    </head>
                    <body>
                        {svg}
                    </body>
                </html>
            ");
            File.WriteAllText(@"D:\temp\temp.svg", svg);
            /**/
        }
    }
}