using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using RT.Coordinates;
using RT.Util;
using RT.Util.ExtensionMethods;

namespace KtaneStuff
{
    static class PolygonalMaze
    {
        public static void Experiment()
        {
            var cells = Ut.NewArray(
                new CircularCell(0, new RationalMod1(0, 1), new RationalMod1(1, 3)),
                new CircularCell(0, new RationalMod1(1, 3), new RationalMod1(2, 3)),
                new CircularCell(0, new RationalMod1(2, 3), new RationalMod1(3, 3)),
                new CircularCell(1, new RationalMod1(0, 1), new RationalMod1(1, 9))
            );

            Console.WriteLine(cells[3]);
            Console.WriteLine();
            foreach (var neigh in cells[3].FindNeighbors(cells))
                Console.WriteLine(neigh);
        }

        public static void Generate()
        {
            const double p2Factor = .75;
            const double p3Factor = .75;
            const double pw = 18;
            const double ph = 24;

            var denomsAtRadius = new[] { 4, 8, 12, 24 };
            var offsetsAtRadius = new RationalMod1[] { RationalMod1.Zero, new RationalMod1(1, 16), RationalMod1.Zero, new RationalMod1(1, 48) };
            int denomAtRadius(int radius) => denomsAtRadius[radius];//(int) Math.Round((2 * radius + 1) * Math.PI);

            var circularCells = Enumerable.Range(0, 4).SelectMany(radius => denomAtRadius(radius).Apply(d => Enumerable.Range(0, d).Select(i =>
                new CircularCell(radius, new RationalMod1(i, d) + offsetsAtRadius[radius], new RationalMod1(i + 1, d) + offsetsAtRadius[radius]))));

            var info = Ut.NewArray<(string name, Type cellType, Type vertexType, IEnumerable<object> cells)>(
                ("Hex", typeof(Hex), typeof(Hex.Vertex), Hex.LargeHexagon(4).Cast<object>()),
                ("Cairo", typeof(Cairo), typeof(Cairo.Vertex), Cairo.Rectangle(4, 4).Cast<object>()),
                ("Penrose P3", typeof(P3Wrapper), typeof(P3Wrapper.P3Vertex), Enumerable.Range(0, 6)
                    .Select(a => new Penrose(Penrose.Kind.ThickRhomb, default, 2 * a))
                    .SelectMany(c => c.DeflatedTiles).Distinct().SelectMany(c => c.DeflatedTiles).Distinct().SelectMany(c => c.DeflatedTiles).Distinct().SelectMany(c => c.DeflatedTiles).Distinct()
                    .Where(c => c.Vertices.All(v => v.Point.Distance < ph / 6 / p3Factor)).Select(c => new P3Wrapper(c)).Cast<object>()),
                ("Square", typeof(RT.Coordinates.Coord), typeof(RT.Coordinates.Coord.Vertex), RT.Coordinates.Coord.Rectangle(9, 8).Cast<object>()),
                ("OctoCell", typeof(OctoCell), typeof(OctoCell.Vertex), OctoCell.Rectangle(7, 6).Cast<object>()),
                ("Rhombihexadel", typeof(Rhombihexadel), typeof(Rhombihexadel.Vertex), Rhombihexadel.LargeHexagon(2).Cast<object>()),
                ("Floret", typeof(Floret), typeof(Floret.Vertex), Floret.LargeHexagon(2).Cast<object>()),
                ("Circular", typeof(CircularCell), typeof(CircularCell.Vx), circularCells.Cast<object>()),
                ("Tri", typeof(Tri), typeof(Tri.Vertex), Tri.LargeHexagon(3).Cast<object>()),
                ("Penrose P2", typeof(Penrose), typeof(Penrose.Vertex), Enumerable.Range(0, 6)
                    .Select(a => new Penrose(Penrose.Kind.Dart, new Penrose.Vector(-1, 1, -1, 1).DivideByPhi.Rotate(2 * a), 2 * a))
                    .SelectMany(c => c.DeflatedTiles).Distinct().SelectMany(c => c.DeflatedTiles).Distinct().SelectMany(c => c.DeflatedTiles).Distinct().SelectMany(c => c.DeflatedTiles).Distinct()
                    .Where(c => c.Vertices.All(v => v.Point.Distance < ph / 6 / p2Factor)).Cast<object>()),
                ("Kite", typeof(Kite), typeof(Kite.Vertex), Kite.LargeHexagon(2).Cast<object>()),
                ("Rhomb", typeof(Rhomb), typeof(Rhomb.Vertex), Rhomb.LargeHexagon(3).Cast<object>())
            );
            var allCells = info.SelectMany(inf => inf.cells).ToArray();

            var structure = new Structure<object>(allCells, getNeighbors: obj => obj is CircularCell cc ? cc.FindNeighbors(circularCells) : ((INeighbor<object>) obj).Neighbors);

            void connectStr(int[] arr1, int[] arr2) { for (var i = 0; i < arr1.Length; i++) structure.AddLink(allCells[arr2[i]], allCells[arr1[i]]); }
            void connect(string str1, string str2) => connectStr(str1.Split(',').Select(int.Parse).ToArray(), str2.Split(',').Select(int.Parse).ToArray());

            connect("0,1,2,3,8,14,21", "37,40,53,56,69,72,85");    // Hex ⇒ Cairo
            connect("263,265,267,36,32,27", "33,34,35,269,271,273");   // Hex ⇒ OctoCell
            connect("88,87,92,91,96,95,100,99", "365,366,367,359,346,347,336,337");    // Cairo ⇒ Rhombihexadel
            connect("275,288,301,314,327,334", "364,389,388,386,385,395");  // OctoCell ⇒ Rhombihexadel
            connect("50,51,66,67,82,83,98", "106,140,147,165,167,168,175");    // OctoCell ⇒ Penrose P3
            connect("108,126,148,149,151,152,159,176", "191,200,209,218,227,236,245,254");     // Penrose P3 ⇒ Square
            connect("189,188,187,180,179,177", "401,396,397,407,402,403");  // Penrose P3 ⇒ Floret
            connect("255,256,257,258,259,260,261,262", "482,483,484,485,462,463,464,465");  // Square ⇒ Circular
            connect("404,420,421,422,423,433", "481,480,479,478,477,476");  // Floret ⇒ Circular
            connect("338,349,350,351,352,353,375", "400,408,413,412,411,431,430");  // Rhombihexadel ⇒ Floret
            connect("328,329,330,331,332,333", "500,486,488,492,496,498");  // OctoCell ⇒ Tri
            connect("516,538,539,517,499", "583,582,566,565,575");  // Tri ⇒ Penrose P2
            connect("393,392,391,379,378,377", "579,580,593,594,587,588");  // Rhombihexadel ⇒ Penrose P2
            connect("429,428,436,435,434", "612,607,599,600,595");  // Floret ⇒ Kite
            connect("591,590,548,549,572", "611,610,629,628,634");  // Penrose P2 ⇒ Kite
            connect("475,474,473,472,471,470", "660,658,648,646,639,637");  // Circular ⇒ Rhomb
            connect("596,606,601,602,619,620", "675,687,686,690,689,693");  // Kite ⇒ Rhomb

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
                v is CircularCell.Vx ? v.Point + mid :
                v.Point
            ) + info.IndexOf(tup => tup.vertexType.Equals(v.GetType())).Apply(ix => new PointD(pw / 2 * (ix % 4), ph / 3 * (ix / 4)));

            IEnumerable<Link<Vertex>> GetEdges(object c) => c is CircularCell cc ? cc.FindEdges(circularCells) : ((IHasSvgGeometry) c).Edges;

            var svg = structure.Svg(new SvgInstructions
            {
                GetVertexPoint = GetVertexPoint,
                GetEdges = GetEdges,
                GetCenter = c => (
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
                    ) + info.IndexOf(tup => tup.cellType.Equals(c.GetType())).Apply(ix => new PointD(pw / 2 * (ix % 4), ph / 3 * (ix / 4))),
                SvgAttributes = "xmlns='http://www.w3.org/2000/svg' viewBox='-1 -1 38 26' xviewBox='{0} {1} {2} {3}' font-size='.2' text-anchor='middle'",
                ExtraSvg1 = "<g fill='none' stroke='black' stroke-width='.05'><rect x='0' y='0' width='18' height='24' /><rect x='18' y='0' width='18' height='24' /></g>" +
                    "<path fill='none' stroke='black' stroke-width='.02' d='M0 8h36M0 16h36M9 0v24M27 0v24' />",
                PerCell = c => $"<circle r='.1' fill='black' fill-opacity='.2' />",
                PassagesSeparate = true,
                PassagesPaths = (d, c1, c2) => $"<path data-from='{allCells.IndexOf(c1)}' data-to='{allCells.IndexOf(c2)}' d='{d}' fill='none' stroke-width='.02' stroke='#aaa' stroke-dasharray='.1' />",
                ExtraSvg4 = info.Select((tup, ix) => $"<text x='{(ix % 4) * pw / 2 + .1}' y='{(ix / 4) * ph / 3 + (.5 * .7) + .1}' font-size='.5' text-anchor='start' stroke='hsl(60, 80%, 90%)' stroke-width='.1' stroke-linejoin='round' paint-order='stroke'>{tup.name}</text>").JoinString() +
                    allCells.Select((cell, ix) => $"<path d='{GridUtils.SvgEdgesPath(GetEdges(cell), GetVertexPoint)}' class='highlightable' fill='transparent' data-cell='{ix}' />").JoinString()
            });

            File.WriteAllText(@"D:\temp\temp.html", $@"
                <html>
                    <head>
                        <title>Polygonal Maze planning</title>
                        <style>
                            svg {{ width: 100%; }}
                        </style>
                    </head>
                    <body>
                        {svg}
                        <script>
                            let ids = [];
                            Array.from(document.querySelectorAll('svg path.highlightable')).forEach(cell => {{
                                let id = +cell.dataset.cell;
                                cell.onclick = function()
                                {{
                                    ids.push(id);
                                    console.log(ids.join(','));
                                }};
                            }});
                        </script>
                    </body>
                </html>
            ");
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

        class CircularCell : IEquatable<CircularCell>
        {
            public CircularCell(int radius, RationalMod1 start, RationalMod1 end)
            {
                if (start == end)
                    throw new ArgumentException("‘start’ and ‘end’ can’t be equal.", nameof(end));
                Radius = radius;
                Start = start;
                End = end;
            }

            public int Radius { get; private set; }
            public RationalMod1 Start { get; private set; }
            public RationalMod1 End { get; private set; }

            public override string ToString() => $"({Radius};{Start}→{End})";
            public override bool Equals(object obj) => obj is CircularCell other && other.Radius == Radius && other.Start == Start && other.End == End;
            public bool Equals(CircularCell other) => other.Radius == Radius && other.Start == Start && other.End == End;
            public override int GetHashCode() => unchecked(Radius * 235794631 + Start.GetHashCode() * 484361 + End.GetHashCode());

            public IEnumerable<CircularCell> FindNeighbors(IEnumerable<CircularCell> cells) => cells.Where(cell =>
                // Cell clockwise from this
                (cell.Radius == Radius && cell.Start == End) ||
                // Cell counter-clockwise from this
                (cell.Radius == Radius && cell.End == Start) ||

                // Cells on a neighboring ring
                ((cell.Radius == Radius + 1 || cell.Radius + 1 == Radius) &&
                    (cell.Start.IsStrictlyBetween(Start, End) || cell.End.IsStrictlyBetween(Start, End) || Start.IsStrictlyBetween(cell.Start, cell.End) || End.IsStrictlyBetween(cell.Start, cell.End) || (cell.Start == Start && cell.End == End)))
            );

            public IEnumerable<Link<Vertex>> FindEdges(IEnumerable<CircularCell> cells)
            {
                var vertices = new List<Vertex> { new Vx(Radius, Start), new Vx(Radius + 1, Start) };

                var outerNotches = new HashSet<RationalMod1>();
                var innerNotches = new HashSet<RationalMod1>();
                foreach (var neighbor in FindNeighbors(cells))
                {
                    if ((neighbor.Radius == Radius + 1 || neighbor.Radius + 1 == Radius) && neighbor.Start.IsStrictlyBetween(Start, End))
                        (neighbor.Radius == Radius + 1 ? outerNotches : innerNotches).Add(neighbor.Start);
                    if ((neighbor.Radius == Radius + 1 || neighbor.Radius + 1 == Radius) && neighbor.End.IsStrictlyBetween(Start, End))
                        (neighbor.Radius == Radius + 1 ? outerNotches : innerNotches).Add(neighbor.End);
                }
                foreach (var notch in outerNotches.Order())
                    vertices.Add(new Vx(Radius + 1, notch));
                vertices.Add(new Vx(Radius + 1, End));
                if (Radius > 0)
                {
                    vertices.Add(new Vx(Radius, End));
                    foreach (var notch in innerNotches.OrderByDescending(r => r))
                        vertices.Add(new Vx(Radius, notch));
                }
                return vertices.MakeEdges();
            }

            public PointD Center => new PointD(0, -Radius - .5).Rotate(Math.PI * (Start + (Start < End ? 0 : 1) + End));

            public class Vx : Vertex
            {
                public Vx(int radius, RationalMod1 pos)
                {
                    Radius = radius;
                    Position = radius == 0 ? new RationalMod1(0, 1) : pos;
                }

                private static int gcd(int a, int b)
                {
                    while (a != 0 && b != 0)
                    {
                        if (a > b)
                            a %= b;
                        else
                            b %= a;
                    }

                    return a | b;
                }

                public int Radius { get; private set; }
                public RationalMod1 Position { get; private set; }

                public override PointD Point => new(
                    Radius * Math.Cos(Math.PI * (2d * Position - .5)),
                    Radius * Math.Sin(Math.PI * (2d * Position - .5)));

                public override bool Equals(Vertex other) => other is Vx vx && vx.Radius == Radius && vx.Position == Position;
                public override bool Equals(object obj) => obj is Vx vx && vx.Radius == Radius && vx.Position == Position;
                public override int GetHashCode() => unchecked(Radius * 235794653 + Position.GetHashCode());

                public override string ToString() => $"{Radius};{Position}";

                public override string SvgPathFragment(Vertex from, Func<Vertex, PointD> getVertexPoint, bool isLast)
                {
                    if (from is not Vx v || v.Radius != Radius)
                        return base.SvgPathFragment(from, getVertexPoint, isLast);

                    var gc = gcd(Position.D, v.Position.D);
                    var n1 = Position.N * v.Position.D / gc;
                    var n2 = v.Position.N * Position.D / gc;
                    var cd = Position.D * v.Position.D / gc;

                    var p = getVertexPoint(this);
                    return $"A {Radius} {Radius} 0 0 {((n2 > n1 ? n1 + cd - n2 < n2 - n1 : n2 + cd - n1 > n1 - n2) ? "1" : "0")} {p.X} {p.Y}";
                }
            }
        }

        struct RationalMod1 : IEquatable<RationalMod1>, IComparable<RationalMod1>
        {
            public int N { get; private set; }
            public int D { get; private set; }
            public RationalMod1(int n, int d)
            {
                if (n % d == 0)
                {
                    N = 0;
                    D = 1;
                }
                else
                {
                    var divisor = gcd(Math.Abs(n), Math.Abs(d));
                    D = d / divisor;
                    var num = n / divisor;
                    N = (num % D + D) % D;
                }
            }

            public static readonly RationalMod1 Zero = new RationalMod1(0, 1);

            private static int gcd(int a, int b)
            {
                while (a != 0 && b != 0)
                {
                    if (a > b)
                        a %= b;
                    else
                        b %= a;
                }

                return a | b;
            }

            public static RationalMod1 operator +(RationalMod1 a, RationalMod1 b) => new(a.N * b.D + b.N * a.D, a.D * b.D);
            public static RationalMod1 operator -(RationalMod1 a, RationalMod1 b) => new(a.N * b.D - b.N * a.D, a.D * b.D);
            public static RationalMod1 operator *(RationalMod1 a, RationalMod1 b) => new(a.N * b.N, a.D * b.D);
            public static RationalMod1 operator /(RationalMod1 a, RationalMod1 b) => new(a.N * b.D, a.D * b.N);
            public static bool operator <(RationalMod1 a, RationalMod1 b) => a.N * b.D < b.N * a.D;
            public static bool operator >(RationalMod1 a, RationalMod1 b) => a.N * b.D > b.N * a.D;
            public static bool operator <=(RationalMod1 a, RationalMod1 b) => a.N * b.D <= b.N * a.D;
            public static bool operator >=(RationalMod1 a, RationalMod1 b) => a.N * b.D >= b.N * a.D;
            public static bool operator ==(RationalMod1 a, RationalMod1 b) => a.N * b.D == b.N * a.D;
            public static bool operator !=(RationalMod1 a, RationalMod1 b) => a.N * b.D != b.N * a.D;
            public bool IsStrictlyBetween(RationalMod1 min, RationalMod1 max) => max > min ? (this > min && this < max) : (this > min || this < max);
            public bool IsBetweenInclusive(RationalMod1 min, RationalMod1 max) => max > min ? (this >= min && this <= max) : (this >= min || this <= max);

            public static double operator +(RationalMod1 a, double b) => b + ((double) a.N / a.D);
            public static double operator +(double b, RationalMod1 a) => b + ((double) a.N / a.D);
            public static double operator -(RationalMod1 a, double b) => ((double) a.N / a.D) - b;
            public static double operator -(double b, RationalMod1 a) => b - ((double) a.N / a.D);
            public static double operator *(RationalMod1 a, double b) => b * a.N / a.D;
            public static double operator *(double b, RationalMod1 a) => b * a.N / a.D;
            public static double operator /(RationalMod1 a, double b) => (double) a.N / a.D / b;
            public static double operator /(double b, RationalMod1 a) => b * a.D / a.N;

            public int CompareTo(RationalMod1 other) => (N * other.D).CompareTo(other.N * D);
            public bool Equals(RationalMod1 other) => other.N == N && other.D == D;
            public override bool Equals(object obj) => obj is RationalMod1 other && other.N == N && other.D == D;
            public override int GetHashCode() => unchecked(N * 28935701 + D);
            public override string ToString() => $"{N}/{D}";
        }
    }
}