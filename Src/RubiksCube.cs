using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using KtaneStuff.Modeling;
using RT.KitchenSink;
using RT.Util.ExtensionMethods;
using RT.Util.Geometry;

namespace KtaneStuff
{
    using static Md;

    static class RubiksCube
    {
        public static void DoModels()
        {
            File.WriteAllText(@"D:\c\KTANE\RubiksCube\Assets\Models\Cubelet.obj", GenerateObjFile(Cubelet(), "Cubelet"));
            File.WriteAllText(@"D:\c\KTANE\RubiksCube\Assets\Models\Sticker.obj", GenerateObjFile(Sticker(), "Sticker"));
            File.WriteAllText(@"D:\c\KTANE\RubiksCube\Assets\Models\Reset.obj", GenerateObjFile(Reset(), "Reset"));
            File.WriteAllText(@"D:\c\KTANE\RubiksCube\Assets\Models\ArrowNSWE.obj", GenerateObjFile(Shape(ArrowNSWE), "ArrowNSWE"));
            File.WriteAllText(@"D:\c\KTANE\RubiksCube\Assets\Models\Arrow.obj", GenerateObjFile(Shape(Arrow), "Arrow"));
        }

        const string ArrowNSWE = @"5,0 7,2 6,2 6,4 8,4 8,3 10,5 8,7 8,6 6,6 6,8 7,8 5,10 3,8 4,8 4,6 2,6 2,7 0,5 2,3 2,4 4,4 4,2 3,2";
        const string Arrow = @"5,0 8,4 6,4 6,10 4,10 4,4 2,4";

        private static IEnumerable<VertexInfo[]> Cubelet()
        {
            const int roundSteps = 4;
            const int degree = 10;

            return CreateMesh(false, true, anglesFull(roundSteps, 2).Concat(180)
                .Select(a => a - 90)
                .Select(a => p(cos(a), sin(a)))
                .Select(a => a / Math.Pow(Math.Pow(a.X, degree) + Math.Pow(a.Y, degree), 1.0 / degree))
                .Select((a, aIx) => anglesFull(roundSteps, 4)
                    .Select(b => p(cos(b), sin(b)))
                    .Select(b => b / Math.Pow(Math.Pow(b.X, degree) + Math.Pow(b.Y, degree), 1.0 / degree))
                    .Select(b => pt(a.X * b.X, a.Y, a.X * b.Y))
                    .Select(b => aIx == 0 ? b.WithMeshInfo(0, -1, 0) : aIx == 4 * roundSteps ? b.WithMeshInfo(0, 1, 0) : b.WithMeshInfo(Normal.Average, Normal.Average, Normal.Average, Normal.Average))
                    .ToArray())
                .ToArray());
        }

        private static IEnumerable<double> angles(int steps) =>
            Enumerable.Range(0, steps).Select(x => x * 45.0 / steps).Select(x => -Math.Pow(x - 45, 2) / 45 + 45).Concat(
            Enumerable.Range(0, steps).Select(x => x * 45.0 / steps + 45).Select(x => Math.Pow(x - 45, 2) / 45 + 45));
        private static IEnumerable<double> anglesFull(int steps, int num) => Enumerable.Range(0, num).SelectMany(i => angles(steps).Select(a => a + 90 * i));

        private static IEnumerable<VertexInfo[]> Sticker()
        {
            const int roundSteps = 6;
            const int degree = 6;
            const double depth = 1.001;
            const double depth2 = 0;

            var poly = anglesFull(roundSteps, 4)
                .Select(b => p(cos(b), sin(b)))
                .Select(b => b / Math.Pow(Math.Pow(b.X, degree) + Math.Pow(b.Y, degree), 1.0 / degree) * .80)
                .Reverse()
                .ToArray();

            // Walls
            foreach (var wall in poly
                .SelectConsecutivePairs(true, (p1, p2) => new { P1 = p1, P2 = p2 })
                .Where(inf => inf.P1 != inf.P2)
                .SelectConsecutivePairs(true, (q1, q2) => new { P = q1.P2, N = (pt(0, 1, 0) * pt(q1.P2.X - q1.P1.X, 0, q1.P2.Y - q1.P1.Y)) + (pt(0, 1, 0) * pt(q2.P2.X - q2.P1.X, 0, q2.P2.Y - q2.P1.Y)) })
                .SelectConsecutivePairs(true, (p1, p2) => new[] { pt(p1.P.X, depth2, p1.P.Y).WithNormal(p1.N), pt(p2.P.X, depth2, p2.P.Y).WithNormal(p2.N), pt(p2.P.X, depth, p2.P.Y).WithNormal(p2.N), pt(p1.P.X, depth, p1.P.Y).WithNormal(p1.N) }))
                yield return wall;

            // Front face
            foreach (var frontFace in poly.SelectConsecutivePairs(true, (p1, p2) => new[] { pt(0, depth, 0).WithNormal(0, 1, 0), pt(p1.X, depth, p1.Y).WithNormal(0, 1, 0), pt(p2.X, depth, p2.Y).WithNormal(0, 1, 0) }))
                yield return frontFace;
        }

        private static IEnumerable<VertexInfo[]> Shape(string vertices)
        {
            return Triangulate(vertices.Split(' ').Reverse().Select(coord => coord.Split(',').Select(int.Parse).ToArray()).Select(c => p(c[0] - 5, c[1] - 5) / 6.0)).Select(poly => poly.Select(p => pt(p.X, 0, p.Y).WithNormal(0, 1, 0)).ToArray()).ToArray();
        }

        private static IEnumerable<VertexInfo[]> Reset()
        {
            using (var bmp = new Bitmap(8, 8, PixelFormat.Format24bppRgb))
            using (var g = Graphics.FromImage(bmp))
            {
                var gp = new GraphicsPath();
                gp.AddString("RESET", new FontFamily("Agency FB"), (int) FontStyle.Regular, 12f, new PointF(0, 0), new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center });

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

                return Extrude(DecodeSvgPath.Do(path, .05), 4);
            }
        }
    }
}