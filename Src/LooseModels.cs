using System.Collections.Generic;
using System.Linq;
using KtaneStuff.Modeling;
using RT.Util;
using RT.Util.ExtensionMethods;
using RT.Util.Geometry;

namespace KtaneStuff
{
    using static Md;

    static class LooseModels
    {
        public static IEnumerable<VertexInfo[]> Tube(int revSteps, double innerRadius, double outerRadius, double length)
        {
            var ir = innerRadius;
            var or = outerRadius;
            return CreateMesh(true, true, Enumerable.Range(0, revSteps).Select(i => i * 360.0 / revSteps).Select(angle =>
                new Pt[] { pt(ir * cos(angle), ir * sin(angle), -length), pt(or * cos(angle), or * sin(angle), -length), pt(or * cos(angle), or * sin(angle), length), pt(ir * cos(angle), ir * sin(angle), length) }
                    .Select(p => p.WithMeshInfo(Normal.Average, Normal.Average, Normal.Mine, Normal.Mine)).ToArray()).ToArray());
        }

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

        public static VertexInfo[][] Disc(int numVertices = 20, bool reverse = false, double addAngle = 0)
        {
            return Enumerable.Range(0, numVertices)
                .Select(i => new PointD(cos(360.0 * i / numVertices + addAngle), sin(360.0 * i / numVertices + addAngle)))
                .SelectConsecutivePairs(true, (p1, p2) => new[] { pt(p1.X, 0, p1.Y), pt(p2.X, 0, p2.Y), pt(0, 0, 0) }.Apply(arr => reverse ? arr : arr.ReverseInplace()).Select(p => p.WithNormal(0, 1, 0).WithTexture((p.X + 1) / 2, (p.Z + 1) / 2)).ToArray())
                .ToArray();
        }

        public static VertexInfo[][] Annulus(double innerRadius, double outerRadius, int numVertices = 20, bool reverse = false)
        {
            return Enumerable.Range(0, numVertices)
                .Select(i => new PointD(cos(360.0 * i / numVertices), sin(360.0 * i / numVertices)))
                .SelectConsecutivePairs(true, (p1, p2) =>
                {
                    var arr = new[] { pt(outerRadius * p1.X, 0, outerRadius * p1.Y), pt(outerRadius * p2.X, 0, outerRadius * p2.Y), pt(innerRadius * p2.X, 0, innerRadius * p2.Y), pt(innerRadius * p1.X, 0, innerRadius * p1.Y) };
                    if (reverse)
                        arr.ReverseInplace();
                    return arr.Select(p => p.WithNormal(0, 1, 0)).ToArray();
                })
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
    }
}