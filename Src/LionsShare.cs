using System.Collections.Generic;
using System.IO;
using System.Linq;
using KtaneStuff.Modeling;
using RT.Util;
using RT.Util.Geometry;

namespace KtaneStuff
{
    using RT.Util.ExtensionMethods;
    using static Md;

    static class LionsShare
    {
        public static void DoModels()
        {
            for (int i = 1; i <= 100; i++)
                File.WriteAllText($@"D:\c\KTANE\LionsShare\Assets\Models\Slice{i}.obj", GenerateObjFile(Slice(i), $"Slice{i}"));
            File.WriteAllText($@"D:\c\KTANE\LionsShare\Assets\Models\Frame.obj", GenerateObjFile(Frame(), $"Frame"));
            File.WriteAllText($@"D:\c\KTANE\LionsShare\Assets\Models\Button.obj", GenerateObjFile(Button(), $"Button"));
        }

        private static IEnumerable<VertexInfo[]> Frame()
        {
            const double ir = 1;
            const double or = 1.1;
            const double height = .1;

            var points = Enumerable.Range(0, 100).Select(i =>
            {
                var angle = i * 3.6;
                var angle2 = (i + 1) * 3.6;
                return Ut.NewArray(
                    pt(ir * cos(angle), 0, ir * sin(angle)).WithMeshInfo(Normal.Average, Normal.Average, Normal.Mine, Normal.Mine),
                    pt(ir * cos(angle), height, ir * sin(angle)).WithMeshInfo(Normal.Average, Normal.Average, Normal.Mine, Normal.Mine),
                    pt(or * cos(angle), height, or * sin(angle)).WithMeshInfo(Normal.Average, Normal.Average, Normal.Mine, Normal.Mine),
                    pt(or * cos(angle), 0, or * sin(angle)).WithMeshInfo(Normal.Average, Normal.Average, Normal.Mine, Normal.Mine)
                );
            }).ToArray();
            return CreateMesh(true, false, points);
        }

        private static IEnumerable<VertexInfo[]> Slice(int portion)
        {
            const double h = .1;
            const double bv = .03;

            var rounded = false;

            if (rounded)
                yield return Ut.NewArray(
                    pt(0, 0, 0).WithNormal(0, 1, 0),
                    pt(cos(.5 * 3.6), 0, sin(.5 * 3.6)).WithNormal(0, 1, 0),
                    pt(cos(0), -bv, sin(0)).WithNormal(0, 0, -1),
                    pt(0, -bv, 0).WithNormal(0, 0, -1)
                );

            var stuff = (portion == 1 ? Enumerable.Empty<int>() : Enumerable.Range(1, portion - 2)).Select<int, (double angle1, double angle2, double h1, double h2, bool specialNormals)>(i => (i * 3.6, (i + 1) * 3.6, 0, 0, false)).ToList();
            if (portion > 1)
                stuff.Insert(0, (.5 * 3.6, 3.6, 0, 0, false));
            if (!rounded)
                stuff.Insert(0, (0, .5 * 3.6, -bv, 0, true));
            if (portion > 1)
                stuff.Add(((portion - 1) * 3.6, (portion - .5) * 3.6, 0, 0, false));
            if (!rounded)
                stuff.Add(((portion - .5) * 3.6, portion * 3.6, 0, -bv, true));

            if (rounded)
            {
                var bvAngle1 = (portion - .5) * 3.6;
                var bvAngle2 = portion * 3.6;
                yield return Ut.NewArray(
                    pt(0, -bv, 0).WithNormal(cos(bvAngle2 + 90), 0, sin(bvAngle2 + 90)),
                    pt(cos(bvAngle2), -bv, sin(bvAngle2)).WithNormal(cos(bvAngle2 + 90), 0, sin(bvAngle2 + 90)),
                    pt(cos(bvAngle1), 0, sin(bvAngle1)).WithNormal(0, 1, 0),
                    pt(0, 0, 0).WithNormal(0, 1, 0)
                );
            }

            foreach (var (angle1, angle2, h1, h2, specialNormals) in stuff)
            {
                yield return Ut.NewArray(
                    pt(0, 0, 0),
                    pt(cos(angle2), h2, sin(angle2)),
                    pt(cos(angle1), h1, sin(angle1))
                ).FlatNormals();
            }

            var angle = portion * 3.6;
            yield return new[] { pt(0, -h, 0), pt(0, 0, 0), pt(1, -bv, 0), pt(1, -h, 0) }.FlatNormals();
            yield return new[] { pt(0, 0, 0), pt(0, -h, 0), pt(cos(angle), -h, sin(angle)), pt(cos(angle), -bv, sin(angle)) }.FlatNormals();
        }

        private static IEnumerable<VertexInfo[]> SliceCollider(int portion)
        {
            var polygon = new List<PointD> { p(0, 0) };
            var numSegments = portion / 13 + 1;
            for (int i = 0; i <= numSegments; i++)
            {
                var angle = 360d * i / numSegments * portion / 100d;
                polygon.Add(p(cos(angle), sin(angle)));
            }
            return polygon.Extrude(.1, true, true);
        }

        private static IEnumerable<VertexInfo[]> Button()
        {
            const int steps = 24;
            const double height = .075;
            const double outerRadius = .08;
            const double bottomRadius = .1;
            const double bevelRadius = .02;

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
    }
}