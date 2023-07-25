using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
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
            const double p2Factor = .75;
            const double p3Factor = .9;
            const double pw = 18;
            const double ph = 24;

            var denomsAtRadius = new[] { 4, 8, 12, 24 };
            var offsetsAtRadius = new CircleFraction[] { CircleFraction.Zero, new CircleFraction(1, 16), CircleFraction.Zero, new CircleFraction(1, 48) };

            var circularCells = Enumerable.Range(0, 4).SelectMany(radius => denomsAtRadius[radius].Apply(d => Enumerable.Range(0, d).Select(i =>
                new CircularCell(radius, new CircleFraction(i, d) + offsetsAtRadius[radius], new CircleFraction(i + 1, d) + offsetsAtRadius[radius]))));

            var info = Ut.NewArray<(string name, Type cellType, Type vertexType, IEnumerable<object> cells)>(
                ("Hex", typeof(Hex), typeof(Hex.Vertex), Hex.LargeHexagon(4).Cast<object>()),
                ("Cairo", typeof(Cairo), typeof(Cairo.Vertex), Cairo.Rectangle(4, 4).Cast<object>()),
                ("Penrose P3", typeof(P3Wrapper), typeof(P3Wrapper.P3Vertex), Enumerable.Range(0, 6)
                    .Select(a => new Penrose(Penrose.Kind.ThickRhomb, default, 2 * a))
                    .SelectMany(c => c.DeflatedTiles).Distinct().SelectMany(c => c.DeflatedTiles).Distinct().SelectMany(c => c.DeflatedTiles).Distinct()
                    .Where(c => c.Vertices.All(v => v.Point.Distance < ph / 6 / p3Factor)).Select(c => new P3Wrapper(c)).Cast<object>()),
                ("Square", typeof(RT.Coordinates.Coord), typeof(RT.Coordinates.Coord.Vertex), RT.Coordinates.Coord.Rectangle(9, 8).Cast<object>()),
                ("OctoCell", typeof(OctoCell), typeof(OctoCell.Vertex), OctoCell.Rectangle(7, 6).Cast<object>()),
                ("Rhombihexadel", typeof(Rhombihexadel), typeof(Rhombihexadel.Vertex), Rhombihexadel.LargeHexagon(2).Cast<object>()),
                ("Floret", typeof(Floret), typeof(Floret.Vertex), Floret.LargeHexagon(2).Cast<object>()),
                ("Circular", typeof(CircularCell), typeof(CircularCell.Vertex), circularCells.Cast<object>()),
                ("Tri", typeof(Tri), typeof(Tri.Vertex), Enumerable.Range(0, 6).SelectMany(y => Enumerable.Range(0, 11).Select(x => new Tri(x, y))).Cast<object>()),
                ("Penrose P2", typeof(Penrose), typeof(Penrose.Vertex), Enumerable.Range(0, 6)
                    .Select(a => new Penrose(Penrose.Kind.Dart, new Penrose.Vector(-1, 1, -1, 1).DivideByPhi.Rotate(2 * a), 2 * a))
                    .SelectMany(c => c.DeflatedTiles).Distinct().SelectMany(c => c.DeflatedTiles).Distinct().SelectMany(c => c.DeflatedTiles).Distinct().SelectMany(c => c.DeflatedTiles).Distinct()
                    .Where(c => c.Vertices.All(v => v.Point.Distance < ph / 6 / p2Factor)).Cast<object>()),
                ("Kite", typeof(Kite), typeof(Kite.Vertex), Kite.LargeHexagon(2).Cast<object>()),
                ("Rhomb", typeof(Rhomb), typeof(Rhomb.Vertex), Rhomb.LargeHexagon(3).Cast<object>())
            );
            var allCells = info.SelectMany(inf => inf.cells).ToArray();

            foreach (var cell in allCells)
            {
                var realCell = cell is P3Wrapper p ? p.Tile : cell;
                var str = realCell.ToString();
                var parsed = GridUtils.Parse(str);
                if (!parsed.Equals(realCell))
                    Debugger.Break();
            }

            IEnumerable<object> GetNeighbors(object cell) => cell is CircularCell cc ? cc.FindNeighbors(circularCells) : ((INeighbor<object>) cell).Neighbors;
            var structure = new Structure<object>(allCells, getNeighbors: GetNeighbors);

            object parse(string str) { var cell = GridUtils.Parse(str); return (cell is Penrose p && (p.TileKind == Penrose.Kind.ThickRhomb || p.TileKind == Penrose.Kind.ThinRhomb)) ? new P3Wrapper(p) : cell; }
            void addLink(string str1, string str2) => structure.AddLink(parse(str1), parse(str2));

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
            addLink("C(0,0)", "P(2,0,1,-1)/2/1");
            addLink("C(0,1)", "P(2,0,1,-1)/3/2");
            addLink("C(0,2)", "P(2,1,2,0)/2/8");
            addLink("C(0,3)", "P(2,1,2,0)/2/6");
            addLink("C(0,4)", "P(1,2,1,1)/3/6");
            addLink("C(0,5)", "P(0,1,1,1)/2/3");
            addLink("C(0,6)", "P(0,1,1,1)/3/4");
            addLink("M(0,-2)/2", "P(-3,0,-2,2)/2/0");
            addLink("F(0,-1)/0", "P(-2,1,-1,2)/3/6");
            addLink("F(0,-1)/1", "P(-2,1,-1,2)/2/5");
            addLink("F(1,-1)/5", "P(-2,1,0,3)/3/8");
            addLink("F(1,-1)/0", "P(-2,2,0,3)/2/8");
            addLink("P(-2,2,0,3)/2/0", "C(3;41/48→43/48)");
            addLink("C(0,7)", "F(1,-1)/1");
            addLink("C(3;43/48→15/16)", "C(1,7)");
            addLink("C(3;15/16→47/48)", "C(2,7)");
            addLink("C(3;47/48→1/48)", "C(3,7)");
            addLink("C(3;1/48→1/16)", "C(4,7)");
            addLink("C(3;1/16→5/48)", "C(5,7)");
            addLink("C(3;5/48→7/48)", "C(6,7)");
            addLink("C(3;7/48→3/16)", "C(7,7)");
            addLink("C(3;3/16→11/48)", "C(8,7)");
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
            addLink("P(0,-2,0,-2)/0/2", "O(6,5)");
            addLink("F(0,-1)/4", "M(1,-2)/5");
            addLink("F(-1,0)/0", "M(1,-2)/3");
            addLink("F(-1,0)/5", "M(1,-2)/2");
            addLink("M(2,-2)/5", "F(-1,0)/4");
            addLink("M(2,-2)/4", "F(-1,0)/3");
            addLink("M(2,-1)/5", "F(-1,1)/5");
            addLink("T(10,0)", "M(0,1)/4");
            addLink("M(0,1)/3", "P(1,-1,0,-2)/0/6");
            addLink("M(0,1)/2", "P(1,-1,0,-2)/0/2");
            addLink("P(2,0,0,-2)/0/6", "M(0,1)/1");
            addLink("M(1,0)/3", "P(2,0,0,-2)/0/4");
            addLink("P(2,0,1,-1)/0/8", "M(1,0)/2");
            addLink("P(2,0,1,-1)/0/4", "M(1,0)/1");
            addLink("K(-1,1)/5", "M(2,-1)/4");
            addLink("K(-1,0)/4", "M(1,-1)/1");
            addLink("C(3;13/16→41/48)", "F(1,-1)/2");
            addLink("C(3;37/48→13/16)", "F(1,0)/0");
            addLink("C(3;35/48→37/48)", "F(1,0)/1");
            addLink("C(3;11/16→35/48)", "F(1,0)/2");
            addLink("F(1,0)/3", "C(3;31/48→11/16)");
            addLink("F(0,1)/1", "C(3;29/48→31/48)");
            addLink("F(-1,1)/4", "P(2,0,2,0)/0/8");
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
            addLink("P(0,-2,0,-2)/0/4", "T(10,2)");
            addLink("T(10,3)", "P(-1,-1,-1,0)/0/0");
            addLink("T(10,4)", "P(-1,-1,-1,0)/0/4");
            addLink("T(10,5)", "P(-2,0,-2,2)/0/0");
            addLink("K(-1,1)/4", "P(2,0,2,0)/0/6");
            addLink("K(-1,1)/3", "P(0,1,1,1)/0/0");
            addLink("K(0,1)/4", "P(0,1,1,1)/0/6");
            addLink("K(0,1)/3", "P(-2,2,0,2)/0/0");
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
                v is RT.Coordinates.Coord.Vertex ? (v.Point - mid) * .95 + mid :
                v is Cairo.Vertex ? (v.Point + new PointD(.5, 0) - mid) * .9 + mid :
                v is OctoCell.Vertex ? v.Point * 1.25 + new PointD(.125, .25) :
                v is Tri.Vertex ? (v.Point + new PointD(.75, 0.10288568297002608956324573161177) - mid) * .975 + mid :
                v is Kite.Vertex ? v.Point.RotateDeg(30) * 1.5 + mid :
                v is Rhomb.Vertex ? v.Point.RotateDeg(30) * .95 + mid :
                v is Rhombihexadel.Vertex ? v.Point.RotateDeg(30) * .9 + mid :
                v is Penrose.Vertex ? v.Point * p2Factor + mid :
                v is P3Wrapper.P3Vertex ? v.Point * p3Factor + mid :
                v is CircularCell.Vertex ? v.Point + mid :
                v.Point
            ) + info.IndexOf(tup => tup.vertexType.Equals(v.GetType())).Apply(ix => new PointD(pw / 2 * (ix % 4), ph / 3 * (ix / 4)));

            IEnumerable<Link<Vertex>> GetEdges(object c) => c is CircularCell cc ? cc.FindEdges(circularCells) : ((IHasSvgGeometry) c).Edges;

            PointD? GetCenter(object c) => (
                    c is Hex h ? h.Center.RotateDeg(30) * 1.4 + mid :
                    c is Floret f ? f.Center.RotateDeg(-30) + mid :
                    c is RT.Coordinates.Coord co ? (co.Center - mid) * .95 + mid :
                    c is Cairo ca ? (ca.Center + new PointD(.5, 0) - mid) * .9 + mid :
                    c is OctoCell o ? o.Center * 1.25 + new PointD(.125, .25) :
                    c is Tri t ? (t.Center + new PointD(.75, 0.10288568297002608956324573161177) - mid) * .975 + mid :
                    c is Kite k ? k.Center.RotateDeg(30) * 1.5 + mid :
                    c is Rhomb r ? r.Center.RotateDeg(30) * .95 + mid :
                    c is Rhombihexadel rh ? rh.Center.RotateDeg(30) * .9 + mid :
                    c is Penrose p ? p.Center * p2Factor + mid :
                    c is P3Wrapper pe ? pe.Center * p3Factor + mid :
                    c is CircularCell cc ? cc.Center + mid :
                    null
                ) + info.IndexOf(tup => tup.cellType.Equals(c.GetType())).Apply(ix => new PointD(pw / 2 * (ix % 4), ph / 3 * (ix / 4)));

            var rnd = new Random(347);

            string encode(int ix) => $"{(char) ('A' + ix / 26)}{(char) ('A' + ix % 26)}";
            var manualSvg = structure.Svg(new SvgInstructions
            {
                GetVertexPoint = GetVertexPoint,
                GetEdges = GetEdges,
                GetCenter = GetCenter,

                SvgAttributes = null,
                PerCell = c => $"<text y='.14' id='cell-label-{allCells.IndexOf(c)}'>{encode(allCells.IndexOf(c))}</text>",

                PassagesSeparate = true,
                PassagesPaths = (d, c1, c2) => $"<path id='wall-{allCells.IndexOf(c1)}-{allCells.IndexOf(c2)}' class='wall' d='{d}' />",
                BridgeSvg = (c1, p1, c2, p2) => $"<g id='bridge-{allCells.IndexOf(c1)}-{allCells.IndexOf(c2)}' class='bridge'>{SvgInstructions.DrawBridge(p1, p2)}</g>"
            });

            var highlightables = allCells.Select(cell => $"<path d='{GridUtils.SvgEdgesPath(GetEdges(cell), GetVertexPoint)}' class='highlightable' data-cell='{cell}' />").JoinString();
            Utils.ReplaceInFile(@"D:\c\KTANE\Public\HTML\Crazy Maze.html", "<!--%%-->", "<!--%%%-->", manualSvg);
            Utils.ReplaceInFile(@"D:\c\KTANE\Public\HTML\Crazy Maze.html", "<!--@@-->", "<!--@@@-->", highlightables);
            Utils.ReplaceInFile(@"D:\c\KTANE\Public\HTML\Crazy Maze.html", "<!--##-->", "<!--###-->", highlightables);
            Utils.ReplaceInFile(@"D:\c\KTANE\Public\HTML\Crazy Maze.html", "/*!*/", "/*!!*/", allCells.Length.ToString());
            var dic = Ut.NewArray(allCells.Length, _ => new List<int>());
            foreach (var lnk in structure.Links)
                foreach (var cell in lnk)
                    dic[allCells.IndexOf(cell)].Add(allCells.IndexOf(lnk.Other(cell)));
            Utils.ReplaceInFile(@"D:\c\KTANE\Public\HTML\Crazy Maze.html", "/*@*/", "/*@@*/", dic.Select(kvp => $@"[{kvp.Order().JoinString(",")}]").JoinString(","));

            // Find bounds of each cells
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

            var transitions = new Dictionary<int, List<string>>();
            var bridgeTransitions = new Dictionary<int, int>();
            foreach (var (lnkCell1, lnkCell2) in structure.Links)
                foreach (var (fromC, toC) in new[] { (lnkCell1, lnkCell2), (lnkCell2, lnkCell1) })
                {
                    var (min, max) = getBounds(GetEdges(fromC));
                    var fromCellIx = allCells.IndexOf(fromC);
                    var toCellIx = allCells.IndexOf(toC);
                    var fromEs = GetEdges(fromC).ToArray();
                    var toEs = GetEdges(toC).ToArray();
                    var fromEIx = fromEs.IndexOf(toEs.Contains);
                    var toEIx = toEs.IndexOf(fromEs.Contains);
                    if ((fromEIx == -1) != (toEIx == -1))
                        Debugger.Break();
                    if (fromEIx == -1)
                        bridgeTransitions[fromCellIx] = toCellIx;
                    else
                    {
                        var (v1, v2) = fromEs[fromEIx];
                        var next = fromEs[(fromEIx + 1) % fromEs.Length];
                        if (next.Contains(v1) == next.Contains(v2))
                            Debugger.Break();
                        if (next.Contains(v1))
                            (v1, v2) = (v2, v1);

                        PointD midP;
                        double angle;
                        if (v1 is CircularCell.Vertex cv1 && v2 is CircularCell.Vertex cv2 && cv1.Radius == cv2.Radius)
                        {
                            var offset = GetVertexPoint(cv1) - cv1.Point;
                            var cv1Pos = cv1.Position + 0d;
                            var cv2Pos = cv2.Position + 0d;
                            if (cv2Pos < cv1Pos)
                                cv2Pos += 1;
                            var midPos = (cv1Pos + cv2Pos) / 2;
                            midP = offset + new PointD(
                                cv1.Radius * Math.Cos(Math.PI * (2d * midPos - .5)),
                                cv1.Radius * Math.Sin(Math.PI * (2d * midPos - .5)));
                            angle = 360 * midPos;
                        }
                        else
                        {
                            var p1 = GetVertexPoint(v1);
                            var p2 = GetVertexPoint(v2);
                            midP = (p1 + p2) / 2;
                            angle = Math.Atan2(p2.Y - p1.Y, p2.X - p1.X) * 180 / Math.PI;
                        }
                        midP -= (max + min) / 2;
                        midP *= 0.128572 / cellSize;
                        transitions.AddSafe(fromCellIx, $"new Transition({toCellIx}, {midP.X}f, {midP.Y}f, {angle}f)");
                    }
                }
            if (transitions.Keys.Any(k => k != 0 && !transitions.ContainsKey(k - 1)))
                Debugger.Break();
            Utils.ReplaceInFile(@"D:\c\KTANE\CrazyMaze\Assets\CellTransitions.cs", "/*!*/", "/*!!*/",
                Enumerable.Range(0, allCells.Length).Select(cellIx => $"new CellTransitions({(bridgeTransitions.TryGetValue(cellIx, out var bt) ? bt.ToString() : "null")}, new[] {{ {transitions[cellIx].JoinString(", ")} }})").JoinString(",\r\n"));

            // Generate sprites
            Enumerable.Range(0, allCells.Length).ParallelForEach(Environment.ProcessorCount, (cellIx, proc) =>
            {
                if (File.Exists($@"D:\c\KTANE\CrazyMaze\Assets\CellSprites\cell-{cellIx}.png"))
                    return;
                const int pngSize = 500;
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
                var cmd = $@"D:\Inkscape\bin\inkscape.exe ""--export-filename=D:\c\KTANE\CrazyMaze\Assets\CellSprites\cell-{cellIx}.png"" --export-width={pngSize} --export-height={pngSize} ""D:\temp\temp-{proc}.svg""";
                lock (allCells)
                    Console.WriteLine(cmd);
                CommandRunner.RunRaw(cmd).Go();
            });
        }

        class P3Wrapper : INeighbor<P3Wrapper>, INeighbor<object>, IHasSvgGeometry, IEquatable<P3Wrapper>
        {
            public Penrose Tile { get; private set; }
            public P3Wrapper(Penrose tile) { Tile = tile; }

            public bool Equals(P3Wrapper other) => other.Tile.Equals(Tile);
            public override bool Equals(object obj) => obj is P3Wrapper other && Equals(other);
            public override int GetHashCode() => unchecked(~Tile.GetHashCode());
            public IEnumerable<P3Wrapper> Neighbors => Tile.Neighbors.Select(n => new P3Wrapper(n));
            IEnumerable<object> INeighbor<object>.Neighbors => Neighbors.Cast<object>();
            public IEnumerable<Link<Vertex>> Edges => Tile.Edges.Select(e => new Link<Vertex>(new P3Vertex((Penrose.Vertex) e.Apart(out var other)), new P3Vertex((Penrose.Vertex) other)));
            public PointD Center => Tile.Center;
            public override string ToString() => Tile.ToString();

            public class P3Vertex : Vertex
            {
                public P3Vertex(Penrose.Vertex pv) { P3V = pv; }
                public Penrose.Vertex P3V { get; private set; }

                public override PointD Point => P3V.Point;
                public override bool Equals(Vertex other) => other is P3Vertex p && p.P3V.Equals(P3V);
                public override bool Equals(object obj) => obj is P3Vertex p && p.P3V.Equals(P3V);
                public override int GetHashCode() => unchecked(~P3V.GetHashCode());
            }
        }
    }
}