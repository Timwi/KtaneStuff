using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using RT.KitchenSink;
using RT.Util;
using RT.Util.ExtensionMethods;
using RT.Util.Geometry;

namespace KtaneStuff.Modeling
{
    public static partial class Md
    {
        public static IEnumerable<VertexInfo[]> Extrude(this IEnumerable<PointD> polygon, double depth, bool includeBackFace = false, bool flatSideNormals = false) =>
            Extrude(new[] { polygon }, depth, includeBackFace, flatSideNormals);
        public static IEnumerable<VertexInfo[]> Extrude(this IEnumerable<IEnumerable<PointD>> polygons, double depth, bool includeBackFace = false, bool flatSideNormals = false) =>
            extrudeImpl(polygons.Select(poly => poly.Select(p => (pt: p, normalBefore: (object) flatSideNormals, normalAfter: (object) flatSideNormals))), depth, includeBackFace).SelectMany(x => x);

        public static IEnumerable<VertexInfo[]> Extrude(this IEnumerable<DecodeSvgPath.PathPiece> pieces, double depth, double smoothness, bool includeBackFace = false)
        {
            if (pieces == null)
                throw new ArgumentNullException(nameof(pieces));
            if (smoothness <= 0)
                throw new ArgumentOutOfRangeException(nameof(smoothness), "The smoothness parameter cannot be zero or negative.");

            var convertedPaths = new List<IEnumerable<(PointD pt, object normalBefore, object normalAfter)>>();

            foreach (var subpathGroup in pieces.Split(pp => pp.Type == DecodeSvgPath.PathPieceType.End))
            {
                var subpath = subpathGroup.ToArray();
                if (subpath.Length == 0)
                    continue;
                var list = new List<(PointD pt, object normalBefore, object normalAfter)>();
                var lastPoint = subpath.Last().Points.Last();
                Pt nrml(PointD p) => pt(0, 1, 0) * pt(p.X, 0, p.Y);
                foreach (var segment in subpath)
                {
                    if (segment.Type == DecodeSvgPath.PathPieceType.Move || segment.Type == DecodeSvgPath.PathPieceType.Line)
                    {
                        if (list.Count > 0)
                            list[list.Count - 1] = (list[list.Count - 1].pt, list[list.Count - 1].normalBefore, true);
                        list.AddRange(segment.Points.Select((p, first, last) => (pt: p, normalBefore: (object) true, normalAfter: (object) true)));
                        lastPoint = segment.Points.Last();
                    }

                    else if (segment.Type == DecodeSvgPath.PathPieceType.Curve)
                    {
                        for (int i = 0; i < segment.Points.Length; i += 3)
                        {
                            var start = lastPoint;
                            var c1 = segment.Points[i];
                            var c2 = segment.Points[i + 1];
                            var end = segment.Points[i + 2];
                            list[list.Count - 1] = (list[list.Count - 1].pt, list[list.Count - 1].normalBefore, nrml(c1 - start));
                            list.AddRange(GeomUt.SmoothBézier(lastPoint, c1, c2, end, smoothness)
                                .Skip(1).Select((p, first, last) => (pt: p, normalBefore: last ? (object) nrml(end - c2) : false, normalAfter: (object) last)));
                            lastPoint = end;
                        }
                    }

                    else if (segment.Type == DecodeSvgPath.PathPieceType.Arc && segment is DecodeSvgPath.PathPieceArc arc && arc.Points.Length == 1)
                    {
                        var p1 = lastPoint.Rotated(arc.XAxisRotation * Math.PI / 180);
                        var p2 = arc.Points[0].Rotated(arc.XAxisRotation * Math.PI / 180);
                        var a = arc.RX;
                        var b = arc.RY;

                        var r1 = (p1.X - p2.X) / (2 * a);
                        var r2 = (p1.Y - p2.Y) / (2 * b);
                        var lambda = Math.Pow(r1, 2) + Math.Pow(r2, 2);
                        if (lambda > 1)
                        {
                            a *= Math.Sqrt(lambda);
                            b *= Math.Sqrt(lambda);
                            r1 = (p1.X - p2.X) / (2 * a);
                            r2 = (p1.Y - p2.Y) / (2 * b);
                            lambda = 1;
                        }
                        var a1 = Math.Atan2(-r1, r2);
                        var a2 = Math.Asin(Math.Sqrt(lambda));
                        var t1 = a1 + a2;
                        var t2 = a1 - a2;

                        var result = GeomUt.SmoothArc(new PointD(p1.X - a * Math.Cos(t1), p1.Y - b * Math.Sin(t1)), a, b, t1, arc.LargeArcFlag ? t2 + 2 * Math.PI : t2, smoothness);
                        if (arc.SweepFlag ^ arc.LargeArcFlag)
                            result = result.Select(p => p1 + p2 - p).Reverse();
                        result = result.Select(p => p.Rotated(-arc.XAxisRotation * Math.PI / 180));
                        list.AddRange(result.Skip(1).Select((p, first, last) => (pt: last ? arc.Points[0] : p, normalBefore: (object) false, normalAfter: (object) last)));
                        lastPoint = arc.Points[0];
                    }

                    else
                        throw new NotImplementedException();
                }
                if (list[list.Count - 1].pt == list[0].pt)
                {
                    list[0] = (list[0].pt, list[list.Count - 1].normalBefore, list[0].normalAfter);
                    list.RemoveAt(list.Count - 1);
                }
                convertedPaths.Add(list);
            }

            return extrudeImpl(convertedPaths, depth, includeBackFace).SelectMany(x => x);
        }

        // The “normalBefore/After” object in the tuple can be one of the following:
        //  • true: calculate a flat normal
        //  • false: calculate a smooth normal (average between previous and next faces)
        //  • Pt: actual normal
        private static IEnumerable<IEnumerable<VertexInfo[]>> extrudeImpl(IEnumerable<IEnumerable<(PointD pt, object normalBefore, object normalAfter)>> polygons, double depth, bool includeBackFace)
        {
            // Walls
            foreach (var path in polygons)
                yield return path
                    .SelectConsecutivePairs(true, (p1, p2) => new { P1 = p1, P2 = p2 })
                    .Where(inf => inf.P1 != inf.P2)
                    .SelectConsecutivePairs(true, (q1, q2) => new
                    {
                        P = q1.P2,
                        N1 = pt(0, 1, 0) * pt(q1.P2.pt.X - q1.P1.pt.X, 0, q1.P2.pt.Y - q1.P1.pt.Y),
                        N2 = pt(0, 1, 0) * pt(q2.P2.pt.X - q2.P1.pt.X, 0, q2.P2.pt.Y - q2.P1.pt.Y)
                    })
                    .SelectConsecutivePairs(true, (p1, p2) => Ut.NewArray(
                        pt(p1.P.pt.X, depth, p1.P.pt.Y).WithNormal(p1.P.normalAfter is Pt n1 ? n1 : (bool) p1.P.normalAfter ? p1.N2 : p1.N1 + p1.N2),
                        pt(p2.P.pt.X, depth, p2.P.pt.Y).WithNormal(p2.P.normalBefore is Pt n2 ? n2 : (bool) p2.P.normalBefore ? p2.N1 : p2.N1 + p2.N2),
                        pt(p2.P.pt.X, 0, p2.P.pt.Y).WithNormal(p2.P.normalBefore is Pt n3 ? n3 : (bool) p2.P.normalBefore ? p2.N1 : p2.N1 + p2.N2),
                        pt(p1.P.pt.X, 0, p1.P.pt.Y).WithNormal(p1.P.normalAfter is Pt n4 ? n4 : (bool) p1.P.normalAfter ? p1.N2 : p1.N1 + p1.N2)));

            // Front face
            yield return Triangulate(polygons.Select(poly => poly.Select(tup => tup.pt))).Select(ps => ps.Select(p => pt(p.X, depth, p.Y).WithNormal(0, 1, 0)).Reverse().ToArray());
            // Back face
            if (includeBackFace)
                yield return Triangulate(polygons.Select(poly => poly.Select(tup => tup.pt))).Select(ps => ps.Select(p => pt(p.X, 0, p.Y).WithNormal(0, -1, 0)).ToArray());
        }

        public static string GenerateObjFile(IEnumerable<Pt[]> faces, string objectName = null, AutoNormal autoNormal = AutoNormal.None)
        {
            return GenerateObjFile(faces.Select(face => face.Select(p => new VertexInfo(p, null)).ToArray()).ToArray(), objectName, autoNormal);
        }

        public static string GenerateObjFile(IEnumerable<VertexInfo[]> faces, string objectName = null, AutoNormal autoNormal = AutoNormal.None, bool uniqueVertices = false)
        {
            var facesProcessed = faces;
            if (autoNormal != AutoNormal.None)
                facesProcessed = faces.Select(face => face.Select(p => new VertexInfo(p.Location, (autoNormal == AutoNormal.FlatOverride ? null : p.Normal) ?? (autoNormal == AutoNormal.FlatIfAbsent && face.Length >= 3 ? ((face[2].Location - face[1].Location) * (face[0].Location - face[1].Location)).Normalize() : (Pt?) null), p.Texture)).ToArray());

            var facesArr = facesProcessed.ToArray();

            var vertices = new List<string>();
            var normals = new List<string>();
            var textures = new List<string>();
            var facesStrs = new List<string>();
            foreach (var face in facesArr)
            {
                var faceStr = new List<string>();
                foreach (var vertex in face)
                {
                    var v = vertex.Location;
                    var def = $"v {v.X:R} {v.Y:R} {v.Z:R}";
                    var ix = vertices.IndexOf(def);
                    if (uniqueVertices || ix == -1)
                    {
                        ix = vertices.Count;
                        vertices.Add($"v {v.X:R} {v.Y:R} {v.Z:R}");
                    }
                    var str = (ix + 1).ToString();
                    if (vertex.Texture != null)
                    {
                        var t = vertex.Texture.Value;
                        textures.Add($"vt {t.X:R} {t.Y:R}");
                        str += $"/{textures.Count}";
                    }
                    if (vertex.Normal != null)
                    {
                        var n = vertex.Normal.Value;
                        normals.Add($"vn {n.X:R} {n.Y:R} {n.Z:R}");
                        if (vertex.Texture == null)
                            str += "/";
                        str += $"/{normals.Count}";
                    }
                    faceStr.Add(str);
                }
                facesStrs.Add($"f {faceStr.JoinString(" ")}");
            }
            return $"o {objectName}\r\n{vertices.JoinString("\r\n")}\r\n{normals.JoinString("\r\n")}\r\n{textures.JoinString("\r\n")}\r\ng {objectName}\r\n{facesStrs.JoinString("\r\n")}";
        }

        public static IEnumerable<Pt> Bézier(Pt start, Pt control1, Pt control2, Pt end, int steps)
        {
            return Enumerable.Range(0, steps)
                .Select(i => (double) i / (steps - 1))
                .Select(t => pow(1 - t, 3) * start + 3 * pow(1 - t, 2) * t * control1 + 3 * (1 - t) * t * t * control2 + pow(t, 3) * end);
        }

        public static IEnumerable<PointD> Bézier(PointD start, PointD control1, PointD control2, PointD end, int steps)
        {
            return Enumerable.Range(0, steps)
                .Select(i => (double) i / (steps - 1))
                .Select(t => pow(1 - t, 3) * start + 3 * pow(1 - t, 2) * t * control1 + 3 * (1 - t) * t * t * control2 + pow(t, 3) * end);
        }

        public static Pt[][] BézierPatch(Pt p00, Pt p10, Pt p20, Pt p30, Pt p01, Pt p11, Pt p21, Pt p31, Pt p02, Pt p12, Pt p22, Pt p32, Pt p03, Pt p13, Pt p23, Pt p33, int steps)
        {
            return Ut.NewArray(steps, steps, (a, b) =>
            {
                var u = (double) a / (steps - 1);
                return bé(bé(p00, p01, p02, p03, u), bé(p10, p11, p12, p13, u), bé(p20, p21, p22, p23, u), bé(p30, p31, p32, p33, u), (double) b / (steps - 1));
            });
        }

        public static Pt[][] BézierPatch(Pt[][] controlPoints, int steps)
        {
            return Ut.NewArray(steps, steps, (a, b) => ((double) a / (steps - 1)).Apply(u => ((double) b / (steps - 1)).Apply(v =>
                bé(
                    bé(controlPoints[0][0], controlPoints[1][0], controlPoints[2][0], controlPoints[3][0], u),
                    bé(controlPoints[0][1], controlPoints[1][1], controlPoints[2][1], controlPoints[3][1], u),
                    bé(controlPoints[0][2], controlPoints[1][2], controlPoints[2][2], controlPoints[3][2], u),
                    bé(controlPoints[0][3], controlPoints[1][3], controlPoints[2][3], controlPoints[3][3], u),
                    v))));
        }

        private static Pt bé(Pt start, Pt c1, Pt c2, Pt end, double t) => Math.Pow((1 - t), 3) * start + 3 * (1 - t) * (1 - t) * t * c1 + 3 * (1 - t) * t * t * c2 + Math.Pow(t, 3) * end;
        private static PointD bé(PointD start, PointD c1, PointD c2, PointD end, double t) => Math.Pow((1 - t), 3) * start + 3 * (1 - t) * (1 - t) * t * c1 + 3 * (1 - t) * t * t * c2 + Math.Pow(t, 3) * end;

        public static IEnumerable<PointD> SmoothBézier(PointD start, PointD c1, PointD c2, PointD end, double smoothness)
        {
            yield return start;

            var stack = new Stack<Tuple<double, double>>();
            stack.Push(Tuple.Create(0d, 1d));

            while (stack.Count > 0)
            {
                var elem = stack.Pop();
                var p1 = bé(start, c1, c2, end, elem.Item1);
                var p2 = bé(start, c1, c2, end, elem.Item2);
                var midT = (elem.Item1 + elem.Item2) / 2;
                var midCurve = bé(start, c1, c2, end, midT);
                var dist = new EdgeD(p1, p2).Distance(midCurve);
                if (dist <= smoothness)
                    yield return p2;
                else
                {
                    stack.Push(Tuple.Create(midT, elem.Item2));
                    stack.Push(Tuple.Create(elem.Item1, midT));
                }
            }
        }

        public static IEnumerable<VertexInfo[]> CreateMesh(bool closedX, bool closedY, Pt[][] pts, bool flatNormals = false)
        {
            return CreateMesh(closedX, closedY, pts
                .Select((arr, x, xFirst, xLast) => arr
                    .Select((p, y, yFirst, yLast) => pt(p.X, p.Y, p.Z,
                        (xLast && !closedX) || flatNormals ? Normal.Mine : Normal.Average,
                        (xFirst && !closedX) || flatNormals ? Normal.Mine : Normal.Average,
                        (yLast && !closedY) || flatNormals ? Normal.Mine : Normal.Average,
                        (yFirst && !closedY) || flatNormals ? Normal.Mine : Normal.Average).WithTexture((arr.Length - 1 - y) / (double) (arr.Length - 1), x / (double) (pts.Length - 1)))
                    .ToArray())
                .ToArray());
        }

        static Pt getPt(MeshVertexInfo[][] pts, int x, int y) => pts[(x + pts.Length) % pts.Length][(y + pts[0].Length) % pts[0].Length].Location;
        static Pt ifZero(this Pt pt, Pt alt) { return Math.Abs(pt.X) <= double.Epsilon && Math.Abs(pt.Y) <= double.Epsilon && Math.Abs(pt.Z) <= double.Epsilon ? alt : pt; }

        public static IEnumerable<VertexInfo[]> CreateMesh(bool closedX, bool closedY, MeshVertexInfo[][] pts)
        {
            var normals = Ut.NewArray(pts.Length, pts[0].Length, (x, y) =>
            {
                if (pts[x][y].NormalOverride != null)
                    return Ut.NewArray(9, _ => pts[x][y].NormalOverride.Value);

                var nrmls = new Pt[9];
                nrmls[0] = (getPt(pts, x, y) - getPt(pts, x - 1, y)).ifZero(getPt(pts, x, y) - getPt(pts, x - 1, y - 1)) * (getPt(pts, x, y) - getPt(pts, x, y - 1)).ifZero(getPt(pts, x, y) - getPt(pts, x - 1, y - 1));
                nrmls[2] = (getPt(pts, x + 1, y) - getPt(pts, x, y)).ifZero(getPt(pts, x + 1, y + 1) - getPt(pts, x, y)) * (getPt(pts, x, y) - getPt(pts, x, y - 1)).ifZero(getPt(pts, x, y) - getPt(pts, x - 1, y - 1));
                nrmls[6] = (getPt(pts, x, y) - getPt(pts, x - 1, y)).ifZero(getPt(pts, x, y) - getPt(pts, x - 1, y - 1)) * (getPt(pts, x, y + 1) - getPt(pts, x, y)).ifZero(getPt(pts, x + 1, y + 1) - getPt(pts, x, y));
                nrmls[8] = (getPt(pts, x + 1, y) - getPt(pts, x, y)).ifZero(getPt(pts, x + 1, y + 1) - getPt(pts, x, y)) * (getPt(pts, x, y + 1) - getPt(pts, x, y)).ifZero(getPt(pts, x + 1, y + 1) - getPt(pts, x, y));

                nrmls[1] = nrmls[0] + nrmls[2];
                nrmls[3] = nrmls[0] + nrmls[6];
                nrmls[5] = nrmls[2] + nrmls[8];
                nrmls[7] = nrmls[6] + nrmls[8];
                nrmls[4] = nrmls[3] + nrmls[5];
                return nrmls;
            });

            return Enumerable.Range(0, pts.Length)
                .SelectManyConsecutivePairs(closedX, (i1, i2) => Enumerable.Range(0, pts[0].Length)
                    .SelectConsecutivePairs(closedY, (j1, j2) => Ut.NewArray(
                        new VertexInfo(pts[i1][j1].Location, normals[i1][j1][(int) pts[i1][j1].NormalAfterX + 3 * (int) pts[i1][j1].NormalAfterY].Normalize(),
                            pts[i1][j1].TextureAfter?.X ?? pts[i1][j1].Texture?.X, pts[i1][j1].TextureAfter?.Y ?? pts[i1][j1].Texture?.Y),
                        new VertexInfo(pts[i2][j1].Location, normals[i2][j1][(2 - (int) pts[i2][j1].NormalBeforeX) + 3 * (int) pts[i2][j1].NormalAfterY].Normalize(),
                            pts[i2][j1].Texture?.X, pts[i2][j1].TextureAfter?.Y ?? pts[i2][j1].Texture?.Y),
                        new VertexInfo(pts[i2][j2].Location, normals[i2][j2][(2 - (int) pts[i2][j2].NormalBeforeX) + 3 * (2 - (int) pts[i2][j2].NormalBeforeY)].Normalize(),
                            pts[i2][j2].Texture?.X, pts[i2][j2].Texture?.Y),
                        new VertexInfo(pts[i1][j2].Location, normals[i1][j2][(int) pts[i1][j2].NormalAfterX + 3 * (2 - (int) pts[i1][j2].NormalBeforeY)].Normalize(),
                            pts[i1][j2].TextureAfter?.X ?? pts[i1][j2].Texture?.X, pts[i1][j2].Texture?.Y))
                        .SelectConsecutivePairs(true, (vi1, vi2) => vi1.Location == vi2.Location ? null : vi1.Nullable())
                        .WhereNotNull()
                        .ToArray()
                    ));
        }

        public static IEnumerable<VertexInfo[]> BevelFromCurve(IEnumerable<Pt> points, double radius, int revSteps, Normal normal = Normal.Average)
        {
            return BevelFromCurve(points, radius, radius, revSteps, normal);
        }

        public static IEnumerable<VertexInfo[]> BevelFromCurve(IEnumerable<Pt> points, double radiusOut, double radiusDown, int revSteps, Normal normal = Normal.Average)
        {
            var pts = points.RemoveConsecutiveDuplicates(true).ToArray();
            var radiusManip = Ut.Lambda((double a, double b, double t) => a * b / Math.Sqrt(Math.Pow(b * cos(t), 2) + Math.Pow(a * sin(t), 2)));
            return CreateMesh(true, false, pts
                .Select((p, ix) => new
                {
                    AxisStart = p,
                    AxisEnd = p + (pts[(ix + 1) % pts.Length] - p).Normalize() + (p - pts[(ix - 1 + pts.Length) % pts.Length]).Normalize()
                })
                .Select(inf => Enumerable.Range(0, revSteps)
                    .Select(i => -90.0 * i / (revSteps - 1))
                    .Select(angle => inf.AxisStart.Add(y: radiusManip(radiusDown, radiusOut, angle)).Rotate(inf.AxisStart, inf.AxisEnd, angle).Add(y: -radiusDown))
                    .Select((p, isFirst, isLast) => isFirst ? new MeshVertexInfo(pt(p.X, p.Y, p.Z), pt(0, 1, 0)) : pt(p.X, p.Y, p.Z, normal, normal, isLast ? Normal.Mine : Normal.Average, Normal.Average))
                    .ToArray())
                .ToArray());
        }

        public static IEnumerable<VertexInfo[]> TubeFromCurve(IEnumerable<Pt> points, double radius, int revSteps)
        {
            var pts = (points as Pt[]) ?? points.ToArray();
            var normals = new Pt[pts.Length];
            normals[0] = ((pts[1] - pts[0]) * pt(0, 1, 0)).Apply(n => n.Length > .001 ? n : (pts[1] - pts[0]) * pt(1, 0, 0)).Normalize() * radius;
            for (int i = 1; i < pts.Length - 1; i++)
                normals[i] = normals[i - 1].ProjectOntoPlane((pts[i + 1] - pts[i]).Normalize() + (pts[i] - pts[i - 1]).Normalize()).Normalize() * radius;
            normals[pts.Length - 1] = normals[pts.Length - 2].ProjectOntoPlane(pts[pts.Length - 1] - pts[pts.Length - 2]).Normalize() * radius;

            var axes = pts.Select((p, i) =>
                i == 0 ? new { Start = pts[0], End = pts[1] } :
                i == pts.Length - 1 ? new { Start = pts[pts.Length - 2], End = pts[pts.Length - 1] } :
                new { Start = p, End = p + (pts[i + 1] - p).Normalize() + (p - pts[i - 1]).Normalize() }).ToArray();

            return CreateMesh(false, true, Enumerable.Range(0, pts.Length)
                .Select(ix => new { Axis = axes[ix], Perp = pts[ix] + normals[ix], Point = pts[ix] })
                .Select(inf => Enumerable.Range(0, revSteps)
                    .Select(i => 360 * i / revSteps)
                    .Select(angle => inf.Perp.Rotate(inf.Axis.Start, inf.Axis.End, angle))
                    .Reverse().ToArray())
                .ToArray());
        }

        public static double pi = Math.PI;
        public static double sin(double x) => Math.Sin(x * pi / 180);
        public static double cos(double x) => Math.Cos(x * pi / 180);
        public static double tan(double x) => Math.Tan(x * pi / 180);
        public static double arcsin(double x) => Math.Asin(x) / Math.PI * 180;
        public static double arccos(double x) => Math.Acos(x) / Math.PI * 180;
        public static double pow(double x, double y) => Math.Pow(x, y);
        public static Pt pt(double x, double y, double z) => new Pt(x, y, z);
        public static Pt ptp(double r, double a, double y) => pt(r * cos(a), y, r * sin(a));
        public static MeshVertexInfo pt(double x, double y, double z, Normal befX, Normal afX, Normal befY, Normal afY) => new MeshVertexInfo(new Pt(x, y, z), befX, afX, befY, afY);
        public static MeshVertexInfo pt(double x, double y, double z, Pt normalOverride) => new MeshVertexInfo(new Pt(x, y, z), normalOverride);
        public static PointD p(double x, double y) => new PointD(x, y);
        public static VertexInfo Move(this VertexInfo vi, double x = 0, double y = 0, double z = 0) { return new VertexInfo(vi.Location.Add(x, y, z), vi.Normal, vi.Texture); }

        public static Pt[] MoveX(this Pt[] face, double x) { return face.Select(p => p.Add(x: x)).ToArray(); }
        public static Pt[] MoveY(this Pt[] face, double y) { return face.Select(p => p.Add(y: y)).ToArray(); }
        public static Pt[] MoveZ(this Pt[] face, double z) { return face.Select(p => p.Add(z: z)).ToArray(); }
        public static Pt[] Move(this Pt[] face, Pt by) { return face.Select(p => p + by).ToArray(); }

        public static IEnumerable<Pt> MoveX(this IEnumerable<Pt> face, double x) { return face.Select(p => p.Add(x: x)); }
        public static IEnumerable<Pt> MoveY(this IEnumerable<Pt> face, double y) { return face.Select(p => p.Add(y: y)); }
        public static IEnumerable<Pt> MoveZ(this IEnumerable<Pt> face, double z) { return face.Select(p => p.Add(z: z)); }
        public static IEnumerable<Pt> Move(this IEnumerable<Pt> face, Pt by) { return face.Select(p => p + by); }

        public static Pt[][] MoveX(this Pt[][] faces, double x) { return faces.Select(face => MoveX(face, x)).ToArray(); }
        public static Pt[][] MoveY(this Pt[][] faces, double y) { return faces.Select(face => MoveY(face, y)).ToArray(); }
        public static Pt[][] MoveZ(this Pt[][] faces, double z) { return faces.Select(face => MoveZ(face, z)).ToArray(); }
        public static Pt[][] Move(this Pt[][] faces, Pt by) { return faces.Select(face => Move(face, by)).ToArray(); }

        public static IEnumerable<Pt[]> MoveX(this IEnumerable<Pt[]> faces, double x) { return faces.Select(face => MoveX(face, x)); }
        public static IEnumerable<Pt[]> MoveY(this IEnumerable<Pt[]> faces, double y) { return faces.Select(face => MoveY(face, y)); }
        public static IEnumerable<Pt[]> MoveZ(this IEnumerable<Pt[]> faces, double z) { return faces.Select(face => MoveZ(face, z)); }
        public static IEnumerable<Pt[]> Move(this IEnumerable<Pt[]> faces, Pt by) { return faces.Select(face => Move(face, by)); }

        public static Pt RotateX(this Pt p, double angle) { return pt(p.X, p.Y * cos(angle) - p.Z * sin(angle), p.Y * sin(angle) + p.Z * cos(angle)); }
        public static Pt RotateY(this Pt p, double angle) { return pt(p.X * cos(angle) - p.Z * sin(angle), p.Y, p.X * sin(angle) + p.Z * cos(angle)); }
        public static Pt RotateZ(this Pt p, double angle) { return pt(p.X * cos(angle) - p.Y * sin(angle), p.X * sin(angle) + p.Y * cos(angle), p.Z); }

        public static VertexInfo RotateX(this VertexInfo vi, double angle) { return new VertexInfo(vi.Location.RotateX(angle), vi.Normal?.RotateX(angle), vi.Texture); }
        public static VertexInfo RotateY(this VertexInfo vi, double angle) { return new VertexInfo(vi.Location.RotateY(angle), vi.Normal?.RotateY(angle), vi.Texture); }
        public static VertexInfo RotateZ(this VertexInfo vi, double angle) { return new VertexInfo(vi.Location.RotateZ(angle), vi.Normal?.RotateZ(angle), vi.Texture); }

        public static Pt[] RotateX(this Pt[] face, double angle) { return face.Select(p => RotateX(p, angle)).ToArray(); }
        public static Pt[] RotateY(this Pt[] face, double angle) { return face.Select(p => RotateY(p, angle)).ToArray(); }
        public static Pt[] RotateZ(this Pt[] face, double angle) { return face.Select(p => RotateZ(p, angle)).ToArray(); }

        public static IEnumerable<Pt> RotateX(this IEnumerable<Pt> face, double angle) { return face.Select(p => RotateX(p, angle)); }
        public static IEnumerable<Pt> RotateY(this IEnumerable<Pt> face, double angle) { return face.Select(p => RotateY(p, angle)); }
        public static IEnumerable<Pt> RotateZ(this IEnumerable<Pt> face, double angle) { return face.Select(p => RotateZ(p, angle)); }

        public static Pt[][] RotateX(this Pt[][] faces, double angle) { return faces.Select(face => RotateX(face, angle)).ToArray(); }
        public static Pt[][] RotateY(this Pt[][] faces, double angle) { return faces.Select(face => RotateY(face, angle)).ToArray(); }
        public static Pt[][] RotateZ(this Pt[][] faces, double angle) { return faces.Select(face => RotateZ(face, angle)).ToArray(); }

        public static IEnumerable<Pt[]> RotateX(this IEnumerable<Pt[]> faces, double angle) { return faces.Select(face => RotateX(face, angle)); }
        public static IEnumerable<Pt[]> RotateY(this IEnumerable<Pt[]> faces, double angle) { return faces.Select(face => RotateY(face, angle)); }
        public static IEnumerable<Pt[]> RotateZ(this IEnumerable<Pt[]> faces, double angle) { return faces.Select(face => RotateZ(face, angle)); }

        public static IEnumerable<TResult> SelectManyConsecutivePairs<T, TResult>(this IEnumerable<T> source, bool closed, Func<T, T, IEnumerable<TResult>> selector) => source.SelectConsecutivePairs(closed, selector).SelectMany(x => x);
        public static IEnumerable<T> RemoveConsecutiveDuplicates<T>(this IEnumerable<T> source, bool closed) where T : IEquatable<T>
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            return removeConsecutiveDuplicatesImpl(closed, source);
        }
        private static IEnumerable<T> removeConsecutiveDuplicatesImpl<T>(bool closed, IEnumerable<T> source) where T : IEquatable<T>
        {
            using (var e = source.GetEnumerator())
            {
                if (!e.MoveNext())
                    yield break;
                T first = e.Current;
                if (!closed)
                    yield return first;
                T last = first;
                while (e.MoveNext())
                {
                    if (!e.Current.Equals(last))
                        yield return e.Current;
                    last = e.Current;
                }
                if (closed && !first.Equals(last))
                    yield return first;
            }
        }

        public static IEnumerable<TResult> Select<T, TResult>(this IEnumerable<T> source, Func<T, bool, bool, TResult> selector)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            if (selector == null)
                throw new ArgumentNullException(nameof(selector));
            return selectIterator(source, (elem, ix, f, l) => selector(elem, f, l));
        }

        public static IEnumerable<TResult> Select<T, TResult>(this IEnumerable<T> source, Func<T, int, bool, bool, TResult> selector)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            if (selector == null)
                throw new ArgumentNullException(nameof(selector));
            return selectIterator(source, selector);
        }

        private static IEnumerable<TResult> selectIterator<T, TResult>(IEnumerable<T> source, Func<T, int, bool, bool, TResult> selector)
        {
            var index = 0;
            var isFirst = true;
            T elem;
            using (var e = source.GetEnumerator())
            {
                if (!e.MoveNext())
                    yield break;
                elem = e.Current;
                while (e.MoveNext())
                {
                    yield return selector(elem, index, isFirst, false);
                    isFirst = false;
                    elem = e.Current;
                    index++;
                }
                yield return selector(elem, index, isFirst, true);
            }
        }

        public static IEnumerable<PointD[]> Triangulate(this IEnumerable<PointD> face)
        {
            var pgon = face.RemoveConsecutiveDuplicates(true).ToList();

            while (pgon.Count > 3)
            {
                // Find an ear
                int bi = pgon.Count - 1;
                PointD a, b, c;
                while (true)
                {
                    a = pgon[(bi + pgon.Count - 1) % pgon.Count];
                    b = pgon[bi];
                    c = pgon[(bi + 1) % pgon.Count];

                    // If the angle ABC is concave, the triangle is not an ear.
                    if ((b.X - a.X) * (c.Y - a.Y) - (b.Y - a.Y) * (c.X - a.X) < 0)
                    {
                        // If any other point lies inside the triangle ABC, it is not an ear.
                        var isEar = true;
                        var triangle = new PolygonD(a, b, c);
                        for (int i = 0; i < pgon.Count - 3 && isEar; i++)
                            if (!triangle.Vertices.Contains(pgon[(bi + 2 + i) % pgon.Count]) && triangle.ContainsPoint(pgon[(bi + 2 + i) % pgon.Count]))
                                isEar = false;
                        if (isEar)
                            break;
                    }
                    bi--;
                    if (bi < 0)
                        throw new InvalidOperationException();
                }

                yield return new[] { a, b, c };
                pgon.RemoveAt(bi);
            }

            yield return pgon.ToArray();
        }

        public static PointD[][] Triangulate(this IEnumerable<IEnumerable<PointD>> polygons, bool failNegative = false)
        {
            var result = new List<PointD[]>();
            var remaining = polygons.Select(p => new PolygonD(p.RemoveConsecutiveDuplicates(true))).ToList();
            while (remaining.Count > 0)
            {
                var polyIx = remaining.IndexOf(poly => poly.Area() > 0);
                if (polyIx == -1)
                {
                    if (!failNegative)
                        return Triangulate(polygons.Select(poly => poly.Reverse()), failNegative: true);
                    throw new InvalidOperationException("There are only negative polygons left.");
                }

                var polygon = remaining[polyIx];
                remaining.RemoveAt(polyIx);
                int holeIx;
                while ((holeIx = remaining.IndexOf(poly => poly.Area() < 0 && poly.Vertices.Any(polygon.ContainsPoint))) != -1)
                {
                    // This polygon has a hole in the shape of another polygon.
                    var hole = remaining[holeIx];
                    remaining.RemoveAt(holeIx);

                    // Find a pair of adjacent points on the hole and a closeby pair of adjacent points on the polygon where we can “cut through”
                    var candidate = hole.Vertices
                        .SelectMany((v, i) => polygon.Vertices.Select((v2, i2) => new { Vertex = v, Index = i, Nearest = new { Vertex = v2, Index = i2 } }))
                        .Where(inf => !new EdgeD(inf.Vertex, inf.Nearest.Vertex).IntersectsWith(new EdgeD(hole.Vertices[(inf.Index + 1) % hole.Vertices.Count], polygon.Vertices[(inf.Nearest.Index + polygon.Vertices.Count - 1) % polygon.Vertices.Count]), true))

                        .Where(inf => !hole.ToEdges().Any(e => new EdgeD(inf.Vertex, inf.Nearest.Vertex).IntersectsWith(e, true)))
                        .Where(inf => !polygon.ToEdges().Any(e => new EdgeD(inf.Vertex, inf.Nearest.Vertex).IntersectsWith(e, true)))
                        .Where(inf => remaining.All(rem => !rem.ToEdges().Any(e => new EdgeD(inf.Vertex, inf.Nearest.Vertex).IntersectsWith(e, true))))

                        .Where(inf => !hole.ToEdges().Any(e => new EdgeD(hole.Vertices[(inf.Index + 1) % hole.Vertices.Count], polygon.Vertices[(inf.Nearest.Index + polygon.Vertices.Count - 1) % polygon.Vertices.Count]).IntersectsWith(e, true)))
                        .Where(inf => !polygon.ToEdges().Any(e => new EdgeD(hole.Vertices[(inf.Index + 1) % hole.Vertices.Count], polygon.Vertices[(inf.Nearest.Index + polygon.Vertices.Count - 1) % polygon.Vertices.Count]).IntersectsWith(e, true)))
                        .Where(inf => remaining.All(rem => !rem.ToEdges().Any(e => new EdgeD(hole.Vertices[(inf.Index + 1) % hole.Vertices.Count], polygon.Vertices[(inf.Nearest.Index + polygon.Vertices.Count - 1) % polygon.Vertices.Count]).IntersectsWith(e, true))))

                        .MinElement(inf => inf.Vertex.Distance(inf.Nearest.Vertex));

                    // Create the quadrilateral that “cuts through”
                    result.Add(new[] { candidate.Vertex, hole.Vertices[(candidate.Index + 1) % hole.Vertices.Count], polygon.Vertices[(candidate.Nearest.Index + polygon.Vertices.Count - 1) % polygon.Vertices.Count] });
                    result.Add(new[] { candidate.Vertex, polygon.Vertices[(candidate.Nearest.Index + polygon.Vertices.Count - 1) % polygon.Vertices.Count], candidate.Nearest.Vertex });

                    // Fix up the current polygon
                    polygon.Vertices.InsertRange(candidate.Nearest.Index, hole.Vertices.Skip(candidate.Index + 1).Concat(hole.Vertices.Take(candidate.Index + 1)));
                }

                // We should have a holeless polygon — triangulate that
                var pgon = polygon.Vertices;
                while (pgon.Count > 3)
                {
                    // Find an ear
                    int bi = pgon.Count - 1;
                    PointD a, b, c;
                    while (true)
                    {
                        a = pgon[(bi + pgon.Count - 1) % pgon.Count];
                        b = pgon[bi];
                        c = pgon[(bi + 1) % pgon.Count];

                        // If the angle ABC is concave, the triangle is not an ear.
                        if ((b.X - a.X) * (c.Y - a.Y) - (b.Y - a.Y) * (c.X - a.X) > 0)
                        {
                            // If any other point lies inside the triangle ABC, it is not an ear.
                            var isEar = true;
                            var triangle = new PolygonD(a, b, c);
                            for (int i = 0; i < pgon.Count - 3 && isEar; i++)
                                if (!triangle.Vertices.Contains(pgon[(bi + 2 + i) % pgon.Count]) && triangle.ContainsPoint(pgon[(bi + 2 + i) % pgon.Count]))
                                    isEar = false;
                            if (isEar)
                                break;
                        }
                        bi--;
                        if (bi < 0)
                            throw new InvalidOperationException();
                    }

                    result.Add(new[] { a, b, c });
                    pgon.RemoveAt(bi);
                }

                result.Add(pgon.ToArray());
            }
            return result.ToArray();
        }

        public static string PathToSvg(IEnumerable<PointD> ptsArr)
        {
            var minX = ptsArr.Min(p => p.X);
            var maxX = ptsArr.Max(p => p.X);
            var minY = ptsArr.Min(p => p.Y);
            var maxY = ptsArr.Max(p => p.Y);
            return $@"
                <svg xmlns='http://www.w3.org/2000/svg' viewBox='{minX} {minY} {maxX - minX} {maxY - minY}'>
                    <path d='{ptsArr.Select((p, i) => $"{(i == 0 ? "M" : i == 1 ? "L" : "")}{p.X},{p.Y}").JoinString(" ")} z' stroke='#000' stroke-width='.01' fill='none' />
                </svg>
            ";
        }

        public static IEnumerable<VertexInfo[]> ExtrudedText(string text, string fontFamily, double extrusionDepth, bool includeBackFace = false, double bézierSmoothness = .05)
        {
            var gp = new GraphicsPath();
            gp.AddString(text, new FontFamily(fontFamily), (int) FontStyle.Regular, 12f, new PointF(0, 0), new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center });

            var path = new List<DecodeSvgPath.PathPiece>();
            for (int j = 0; j < gp.PointCount; j++)
            {
                var type =
                    ((PathPointType) gp.PathTypes[j]).HasFlag(PathPointType.Bezier) ? DecodeSvgPath.PathPieceType.Curve :
                    ((PathPointType) gp.PathTypes[j]).HasFlag(PathPointType.Line) ? DecodeSvgPath.PathPieceType.Line : DecodeSvgPath.PathPieceType.Move;
                if (type == DecodeSvgPath.PathPieceType.Curve)
                {
                    path.Add(new DecodeSvgPath.PathPiece(DecodeSvgPath.PathPieceType.Curve, gp.PathPoints.Subarray(j, 3).Select(p => new PointD(p)).ToArray()));
                    j += 2;
                }
                else
                    path.Add(new DecodeSvgPath.PathPiece(type, gp.PathPoints.Subarray(j, 1).Select(p => new PointD(p)).ToArray()));

                if (((PathPointType) gp.PathTypes[j]).HasFlag(PathPointType.CloseSubpath))
                    path.Add(DecodeSvgPath.PathPiece.End);
            }

            return DecodeSvgPath.Do(path, bézierSmoothness).Extrude(extrusionDepth);
        }

        public static IEnumerable<VertexInfo[]> FlatText(string text, string fontFamily, double bézierSmoothness = .05)
        {
            var gp = new GraphicsPath();
            gp.AddString(text, new FontFamily(fontFamily), (int) FontStyle.Regular, 12f, new PointF(0, 0), new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center });

            var path = new List<DecodeSvgPath.PathPiece>();
            for (int j = 0; j < gp.PointCount; j++)
            {
                var type =
                    ((PathPointType) gp.PathTypes[j]).HasFlag(PathPointType.Bezier) ? DecodeSvgPath.PathPieceType.Curve :
                    ((PathPointType) gp.PathTypes[j]).HasFlag(PathPointType.Line) ? DecodeSvgPath.PathPieceType.Line : DecodeSvgPath.PathPieceType.Move;
                if (type == DecodeSvgPath.PathPieceType.Curve)
                {
                    path.Add(new DecodeSvgPath.PathPiece(DecodeSvgPath.PathPieceType.Curve, gp.PathPoints.Subarray(j, 3).Select(p => new PointD(p)).ToArray()));
                    j += 2;
                }
                else
                    path.Add(new DecodeSvgPath.PathPiece(type, gp.PathPoints.Subarray(j, 1).Select(p => new PointD(p)).ToArray()));

                if (((PathPointType) gp.PathTypes[j]).HasFlag(PathPointType.CloseSubpath))
                    path.Add(DecodeSvgPath.PathPiece.End);
            }

            return DecodeSvgPath.Do(path, bézierSmoothness).Triangulate().Select(ps => ps.Select(p => pt(p.X, 0, p.Y).WithNormal(0, 1, 0)).Reverse().ToArray());
        }

        public static VertexInfo[] FlatNormals(this Pt[] polygon)
        {
            var normal = (polygon[2] - polygon[1]) * (polygon[0] - polygon[1]);
            return polygon.Select(f => f.WithNormal(normal)).ToArray();
        }

        public static IEnumerable<VertexInfo[]> DoubleSidedFlatNormals(this Pt[] polygon)
        {
            yield return polygon.FlatNormals();
            yield return polygon.Reverse().ToArray().FlatNormals();
        }

        public static (string name, Pt[][] polygons) ParseObjFile(string file)
        {
            var vertices = new List<Pt>();
            var textures = new List<PointD>();
            var normals = new List<Pt>();
            var faces = new List<Tuple<int, int, int>[]>();
            string name = null;

            foreach (var line in File.ReadAllLines(file))
            {
                Match m;
                if ((m = Regex.Match(line, @"^v (-?\d*\.?\d+(?:[eE][-+]?\d+)?) (-?\d*\.?\d+(?:[eE][-+]?\d+)?) (-?\d*\.?\d+(?:[eE][-+]?\d+)?)$")).Success)
                    vertices.Add(new Pt(double.Parse(m.Groups[1].Value), double.Parse(m.Groups[2].Value), double.Parse(m.Groups[3].Value)));
                else if ((m = Regex.Match(line, @"^vt (-?\d*\.?\d+(?:[eE][-+]?\d+)?) (-?\d*\.?\d+(?:[eE][-+]?\d+)?)$")).Success)
                    textures.Add(new PointD(double.Parse(m.Groups[1].Value), double.Parse(m.Groups[2].Value)));
                else if ((m = Regex.Match(line, @"^vn (-?\d*\.?\d+(?:[eE][-+]?\d+)?) (-?\d*\.?\d+(?:[eE][-+]?\d+)?) (-?\d*\.?\d+(?:[eE][-+]?\d+)?)$")).Success)
                    normals.Add(new Pt(double.Parse(m.Groups[1].Value), double.Parse(m.Groups[2].Value), double.Parse(m.Groups[3].Value)));
                else if (line.StartsWith("v"))
                    System.Diagnostics.Debugger.Break();
                else if (line.StartsWith("f "))
                    faces.Add(line.Substring(2).Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
                        .Select(s =>
                        {
                            var ixs = s.Split(new[] { '/' });
                            var vix = int.Parse(ixs[0]) - 1;
                            var tix = ixs.Length > 1 && ixs[1].Length > 0 ? int.Parse(ixs[1]) - 1 : -1;
                            var nix = ixs.Length > 2 && ixs[2].Length > 0 ? int.Parse(ixs[2]) - 1 : -1;
                            return Tuple.Create(vix, tix, nix);
                        }).ToArray());
                else if (line.StartsWith("o "))
                    name = line.Substring(2);
            }

            return (name, faces.Select(f => f.Select(p => vertices[p.Item1]).ToArray()).ToArray());
        }

        public static VertexInfo[][] Texturize(this VertexInfo[][] mesh, double baseY, double rMult)
        {
            var textureTmps = mesh.Select(arr => arr.Select(pt => p(pt.Location.X, pt.Location.Z) * ((pt.Location.Y - baseY) * rMult + 1)).ToArray()).ToArray();

            var minU = textureTmps.Min(arr => arr.Min(p => p.X));
            var maxU = textureTmps.Max(arr => arr.Max(p => p.X));
            var minV = textureTmps.Min(arr => arr.Min(p => p.Y));
            var maxV = textureTmps.Max(arr => arr.Max(p => p.Y));

            static double lm(double min, double max, double val) => (val - min) / (max - min);
            return mesh.Select((arr, i) => arr.Select((p, j) => p.WithTexture(lm(minU, maxU, textureTmps[i][j].X), lm(minV, maxV, textureTmps[i][j].Y))).ToArray()).ToArray();
        }
    }
}
