using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using RT.KitchenSink;
using RT.Util;
using RT.Util.ExtensionMethods;
using RT.Util.Geometry;

namespace KtaneStuff.Modeling
{
    public static partial class Md
    {
        /// <summary>
        ///     Generates a cylinder oriented along the Z axis.</summary>
        /// <param name="startZ">
        ///     Z-coordinate of the “bottom” of the cylinder. This must be less than <paramref name="endZ"/>.</param>
        /// <param name="endZ">
        ///     Z-coordinate of the “top” of the cylinder. This must be greater than <paramref name="startZ"/>.</param>
        public static VertexInfo[][] Cylinder(double startZ, double endZ, double radius, int numVertices = 20)
        {
            if (startZ > endZ)
            {
                var t = startZ;
                startZ = endZ;
                endZ = t;
            }

            // Create a circle in X/Y space
            var circle = Enumerable.Range(0, numVertices)
                .Select(i => new PointD(radius * cos(360.0 * i / numVertices), radius * sin(360.0 * i / numVertices)));

            return Ut.NewArray(
                // Side wall
                Enumerable.Range(0, numVertices)
                    .Select(i => 360.0 * i / numVertices)
                    .Select(angle => new PointD(radius * cos(angle), radius * sin(angle)))
                    .SelectConsecutivePairs(true, (p1, p2) => new[] { pt(p1.X, p1.Y, startZ).WithNormal(p1.X, p1.Y, 0), pt(p2.X, p2.Y, startZ).WithNormal(p2.X, p2.Y, 0), pt(p2.X, p2.Y, endZ).WithNormal(p2.X, p2.Y, 0), pt(p1.X, p1.Y, endZ).WithNormal(p1.X, p1.Y, 0) }),
                // Caps
                new[] { circle.Reverse().Select(c => pt(c.X, c.Y, startZ).WithNormal(0, 0, -1)).ToArray() },
                new[] { circle.Select(c => pt(c.X, c.Y, endZ).WithNormal(0, 0, 1)).ToArray() }
            ).SelectMany(x => x).ToArray();
        }

        public static IEnumerable<VertexInfo[]> Torus(double outerRadius, double innerRadius, int steps, double startAngle = 0, double endAngle = 360)
        {
            return CreateMesh(true, true,
                Enumerable.Range(0, steps).Select(i => i * (endAngle - startAngle) / steps + startAngle).Select(angle1 =>
                    Enumerable.Range(0, steps).Select(i => i * 360 / steps).Select(angle2 =>
                        p(outerRadius + innerRadius * cos(angle2), innerRadius * sin(angle2))
                            .Apply(p => pt(p.X * cos(angle1), p.Y, p.X * sin(angle1), Normal.Average, Normal.Average, Normal.Average, Normal.Average)))
                        .ToArray())
                .Reverse()
                .ToArray());
        }

        public static IEnumerable<VertexInfo[]> Extrude(this IEnumerable<PointD> polygon, double depth, bool includeBackFace = false) => Extrude(new[] { polygon }, depth, includeBackFace);
        public static IEnumerable<VertexInfo[]> Extrude(this IEnumerable<IEnumerable<PointD>> polygons, double depth, bool includeBackFace = false) => extrudeImpl(polygons, depth, includeBackFace).SelectMany(x => x);

        private static IEnumerable<IEnumerable<VertexInfo[]>> extrudeImpl(IEnumerable<IEnumerable<PointD>> polygons, double depth, bool includeBackFace)
        {
            // Walls
            foreach (var path in polygons)
                yield return path
                    .SelectConsecutivePairs(true, (p1, p2) => new { P1 = p1, P2 = p2 })
                    .Where(inf => inf.P1 != inf.P2)
                    .SelectConsecutivePairs(true, (q1, q2) => new { P = q1.P2, N = (pt(0, 1, 0) * pt(q1.P2.X - q1.P1.X, 0, q1.P2.Y - q1.P1.Y)) + (pt(0, 1, 0) * pt(q2.P2.X - q2.P1.X, 0, q2.P2.Y - q2.P1.Y)) })
                    .SelectConsecutivePairs(true, (p1, p2) => new[] { pt(p1.P.X, depth, p1.P.Y).WithNormal(p1.N), pt(p2.P.X, depth, p2.P.Y).WithNormal(p2.N), pt(p2.P.X, 0, p2.P.Y).WithNormal(p2.N), pt(p1.P.X, 0, p1.P.Y).WithNormal(p1.N) });

            // Front face
            yield return Triangulate(polygons).Select(ps => ps.Select(p => pt(p.X, depth, p.Y).WithNormal(0, 1, 0)).Reverse().ToArray());
            // Back face
            if (includeBackFace)
                yield return Triangulate(polygons).Select(ps => ps.Select(p => pt(p.X, 0, p.Y).WithNormal(0, -1, 0)).ToArray());
        }

        public static VertexInfo[][] Disc(int numVertices = 20, bool reverse = false)
        {
            return Enumerable.Range(0, numVertices)
                .Select(i => new PointD(cos(360.0 * i / numVertices), sin(360.0 * i / numVertices)))
                .SelectConsecutivePairs(true, (p1, p2) => (reverse ? new[] { pt(p1.X, 0, p1.Y), pt(p2.X, 0, p2.Y), pt(0, 0, 0) } : new[] { pt(p1.X, 0, p1.Y), pt(0, 0, 0), pt(p2.X, 0, p2.Y) }).Select(p => p.WithNormal(0, 1, 0)).ToArray())
                .ToArray();
        }

        public static VertexInfo[][] Square(bool reverse = false)
        {
            var arr = new[] { pt(-1, 0, -1), pt(-1, 0, 1), pt(1, 0, 1), pt(1, 0, -1) }.Select(p => p.WithNormal(0, 1, 0));
            return new[] { reverse ? arr.Reverse().ToArray() : arr.ToArray() };
        }

        public static Pt[][] Box(bool reverse = false)
        {
            var arrs = Ut.NewArray(
                new[] { pt(-1, 1, -1), pt(-1, 1, 1), pt(1, 1, 1), pt(1, 1, -1) },
                new[] { pt(-1, -1, -1), pt(1, -1, 1), pt(-1, -1, 1), pt(1, -1, -1) },
                new[] { pt(-1, -1, 1), pt(1, 1, 1), pt(-1, 1, 1), pt(1, -1, 1) },
                new[] { pt(-1, -1, -1), pt(-1, 1, -1), pt(1, 1, -1), pt(1, -1, -1) },
                new[] { pt(1, -1, -1), pt(1, 1, 1), pt(1, -1, 1), pt(1, 1, -1) },
                new[] { pt(-1, -1, -1), pt(-1, -1, 1), pt(-1, 1, 1), pt(-1, 1, -1) }
            );
            return reverse ? arrs.Select(arr => arr.Reverse().ToArray()).ToArray() : arrs;
        }

        public static string GenerateObjFile(IEnumerable<Pt[]> faces, string objectName = null, AutoNormal autoNormal = AutoNormal.None)
        {
            return GenerateObjFile(faces.Select(face => face.Select(p => new VertexInfo(p, autoNormal == AutoNormal.Flat && face.Length >= 3 ? (face[2] - face[1]) * (face[0] - face[1]) : (Pt?) null, null)).ToArray()).ToArray(), objectName);
        }

        public static string GenerateObjFile(IEnumerable<VertexInfo[]> faces, string objectName = null)
        {
            var facesArr = faces.ToArray();
            var vertices = facesArr.SelectMany(f => f).Select(f => f.Location).Distinct().ToArray();
            var verticesLookup = vertices.Select((v, i) => Ut.KeyValuePair(v, i + 1)).ToDictionary();
            var normals = facesArr.SelectMany(f => f).Where(f => f.Normal != null).Select(f => f.Normal.Value).Distinct().ToArray();
            var normalsLookup = normals.Select((n, i) => Ut.KeyValuePair(n, i + 1)).ToDictionary();
            var textures = facesArr.SelectMany(f => f).Where(f => f.Texture != null).Select(f => f.Texture.Value).Distinct().ToArray();
            var s = new StringBuilder();
            if (objectName != null)
                s.AppendLine($"o {objectName}");
            foreach (var v in vertices)
                s.AppendLine($"v {v.X:R} {v.Y:R} {v.Z:R}");
            foreach (var n in normals)
                s.AppendLine($"vn {n.X:R} {n.Y:R} {n.Z:R}");
            foreach (var t in textures)
                s.AppendLine($"vt {t.X:R} {t.Y:R}");
            if (objectName != null)
                s.AppendLine($"g {objectName}");
            foreach (var f in facesArr)
                s.AppendLine($@"f {f.Select(vi =>
                    verticesLookup[vi.Location].Apply(v =>
                    vi.Texture.NullOr(t => textures.IndexOf(t) + 1).Apply(t =>
                    vi.Normal.NullOr(n => normalsLookup[n]).Apply(n =>
                    n == null ? t == null ? v.ToString() : $"{v}/{t}" : $"{v}/{t}/{n}")))).JoinString(" ")}");
            return s.ToString();
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

        public static IEnumerable<VertexInfo[]> CreateMesh(bool closedX, bool closedY, Pt[][] pts)
        {
            return CreateMesh(closedX, closedY, pts
                .Select((arr, x, xFirst, xLast) => arr
                    .Select((p, y, yFirst, yLast) => pt(p.X, p.Y, p.Z,
                        xLast && !closedX ? Normal.Mine : Normal.Average,
                        xFirst && !closedX ? Normal.Mine : Normal.Average,
                        yLast && !closedY ? Normal.Mine : Normal.Average,
                        yFirst && !closedY ? Normal.Mine : Normal.Average).WithTexture((arr.Length - 1 - y) / (double) (arr.Length - 1), x / (double) (pts.Length - 1)))
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
                        new VertexInfo(pts[i1][j1].Location, normals[i1][j1][(int) pts[i1][j1].NormalAfterX + 3 * (int) pts[i1][j1].NormalAfterY].Normalize(), pts[i1][j1].Texture),
                        new VertexInfo(pts[i2][j1].Location, normals[i2][j1][(2 - (int) pts[i2][j1].NormalBeforeX) + 3 * (int) pts[i2][j1].NormalAfterY].Normalize(), pts[i2][j1].Texture),
                        new VertexInfo(pts[i2][j2].Location, normals[i2][j2][(2 - (int) pts[i2][j2].NormalBeforeX) + 3 * (2 - (int) pts[i2][j2].NormalBeforeY)].Normalize(), pts[i2][j2].Texture),
                        new VertexInfo(pts[i1][j2].Location, normals[i1][j2][(int) pts[i1][j2].NormalAfterX + 3 * (2 - (int) pts[i1][j2].NormalBeforeY)].Normalize(), pts[i1][j2].Texture))
                        .SelectConsecutivePairs(true, (vi1, vi2) => vi1.Location == vi2.Location ? null : vi1.Nullable())
                        .Where(vi => vi != null)
                        .Select(vi => vi.Value)
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

        public static double pi = Math.PI;
        public static double sin(double x) => Math.Sin(x * pi / 180);
        public static double cos(double x) => Math.Cos(x * pi / 180);
        public static double tan(double x) => Math.Tan(x * pi / 180);
        public static double arcsin(double x) => Math.Asin(x) / Math.PI * 180;
        public static double arccos(double x) => Math.Acos(x) / Math.PI * 180;
        public static double pow(double x, double y) => Math.Pow(x, y);
        public static Pt pt(double x, double y, double z) => new Pt(x, y, z);
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
            using (var bmp = new Bitmap(8, 8, PixelFormat.Format24bppRgb))
            using (var g = Graphics.FromImage(bmp))
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
    }
}
