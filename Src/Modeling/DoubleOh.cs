using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using RT.Util;
using RT.Util.ExtensionMethods;

namespace KtaneStuff.Modeling
{
    using System.Drawing;
    using RT.KitchenSink;
    using RT.Util.Drawing;
    using RT.Util.Geometry;
    using static Md;

    static class DoubleOh
    {
        public static void Do()
        {
            foreach (var inf in new[] { SvgInfo.SingleArrow, SvgInfo.DoubleArrow, SvgInfo.Submit })
                File.WriteAllText($@"D:\c\KTANE\DoubleOh\Assets\Models\Button{inf.Name}.obj", GenerateObjFile(Button(inf.Svg), $"Button{inf.Name}"));
            File.WriteAllText($@"D:\c\KTANE\DoubleOh\Assets\Models\ButtonTop.obj", GenerateObjFile(Button(null), $"ButtonTop"));

            const double f = .3;
            const int size = 800;
            GraphicsUtil.DrawBitmap(size, size, g =>
            {
                g.Clear(Color.White);
                for (int i = 0; i < 10; i++)
                {
                    var u = i / 10.0;
                    for (int j = 0; j <= 10; j++)
                    {
                        var v = j / 10.0;
                        g.DrawString($"a{i}{j}", new Font("Calibri", 12f, FontStyle.Regular), Brushes.Black, (new PointD((v + (1 - 2 * f) * (1 - v)) * u + ((1 - v) * f), (1 - v) * f) * size).ToPointF());
                    }
                }

                for (int i = 0; i < 10; i++)
                {
                    var u = i / 10.0;
                    for (int j = 0; j <= 10; j++)
                    {
                        var v = j / 10.0;
                        g.DrawString($"b{i}{j}", new Font("Calibri", 12f, FontStyle.Regular), Brushes.Black, (new PointD(1 - (1 - v) * f, (v + (1 - 2 * f) * (1 - v)) * u + ((1 - v) * f)) * size).ToPointF());
                    }
                }

                for (int i = 0; i < 10; i++)
                {
                    var u = i / 10.0;
                    for (int j = 0; j <= 10; j++)
                    {
                        var v = j / 10.0;
                        g.DrawString($"c{i}{j}", new Font("Calibri", 12f, FontStyle.Regular), Brushes.Black, (new PointD(1 - (v + (1 - 2 * f) * (1 - v)) * u - ((1 - v) * f), 1 - (1 - v) * f) * size).ToPointF());
                    }
                }

                for (int i = 0; i < 10; i++)
                {
                    var u = i / 10.0;
                    for (int j = 0; j <= 10; j++)
                    {
                        var v = j / 10.0;
                        g.DrawString($"d{i}{j}", new Font("Calibri", 12f, FontStyle.Regular), Brushes.Black, (new PointD((1 - v) * f, 1 - (v + (1 - 2 * f) * (1 - v)) * u - ((1 - v) * f)) * size).ToPointF());
                    }
                }

            }).Save(@"D:\daten\upload\scr.png");
        }

        sealed class SvgInfo
        {
            public string Name { get; private set; }
            public string Svg { get; private set; }
            public static SvgInfo SingleArrow = new SvgInfo { Name = "SingleArrow", Svg = "M 5,0 8,3 H 6 V 7 H 8 L 5,10 2,7 H 4 V 3 H 2 z" };
            public static SvgInfo DoubleArrow = new SvgInfo { Name = "DoubleArrow", Svg = "M 5,0 8,3 6.75,3 6.75,7 8,7 5,10 2,7 3.25,7 3.25,3 2,3 z M 4.75,7 5.25,7 5.25,3 4.75,3 z" };
            public static SvgInfo Submit = new SvgInfo { Name = "Submit", Svg = "M 5,0 10,5 5,10 0,5 5,0 z M 5,3 3,5 5,7 7,5 5,3 z M 5,4 6,5 5,6 4,5 5,4 z" };
        }

        public static IEnumerable<VertexInfo[]> Button(string svg)
        {
            const double radius1 = .6;
            const double radius2 = .8;
            const double radius3 = .9;
            const double radius4 = 1.0;

            const double h = 0.6;
            const double y1 = h;
            const double y2 = h;
            const double y3 = h / 2;
            const double y4 = 0;

            const double bevelRadius = .05;

            const double cf = .2;
            const int bézierSteps = 16;
            const int roundSteps = 3;
            const double bézierSmoothness = .001;

            const double textureFrame = .4;

            var patch = BézierPatch(
                pt(0, y1, radius1), pt(radius1 * cf, y1, radius1), pt(radius1, y1, radius1 * cf), pt(radius1, y1, 0),
                pt(0, y2, radius2), pt(radius2 * cf, y2, radius2), pt(radius2, y2, radius2 * cf), pt(radius2, y2, 0),
                pt(0, y3, radius3), pt(radius3 * cf, y3, radius3), pt(radius3, y3, radius3 * cf), pt(radius3, y3, 0),
                pt(0, y4, radius4), pt(radius4 * cf, y4, radius4), pt(radius4, y4, radius4 * cf), pt(radius4, y4, 0),
                bézierSteps);

            var patch2 = patch.Select((arr, j) => (j / (double) patch.Length).Apply(v =>
                arr.Skip(1).Select((vx, i) => (i / (double) arr.Length).Apply(u => new { Vertex = vx, Texture = p((v + (1 - 2 * textureFrame) * (1 - v)) * u + ((1 - v) * textureFrame), (1 - v) * textureFrame) }))
                    .Concat(arr.Skip(1).Select((vx, i) => (i / (double) arr.Length).Apply(u => new { Vertex = vx.RotateY(-90), Texture = p(1 - (1 - v) * textureFrame, (v + (1 - 2 * textureFrame) * (1 - v)) * u + ((1 - v) * textureFrame)) })))
                    .Concat(arr.Skip(1).Select((vx, i) => (i / (double) arr.Length).Apply(u => new { Vertex = vx.RotateY(-180), Texture = p(1 - (v + (1 - 2 * textureFrame) * (1 - v)) * u - ((1 - v) * textureFrame), 1 - (1 - v) * textureFrame) })))
                    .Concat(arr.Skip(1).Select((vx, i) => (i / (double) arr.Length).Apply(u => new { Vertex = vx.RotateY(-270), Texture = p((1 - v) * textureFrame, 1 - (v + (1 - 2 * textureFrame) * (1 - v)) * u - ((1 - v) * textureFrame)) })))
                    .ToArray()))
                .ToArray();

            var patch3 = Ut.NewArray(patch2.Length, patch2[0].Length, (i, j) =>
                (
                    i == 0 ? new MeshVertexInfo(patch2[i][j].Vertex, pt(0, 1, 0)) :
                    i == patch2.Length - 1 ? new MeshVertexInfo(patch2[i][j].Vertex, Normal.Mine, Normal.Mine, Normal.Average, Normal.Average) :
                    new MeshVertexInfo(patch2[i][j].Vertex, Normal.Average, Normal.Average, Normal.Average, Normal.Average)
                )
                    .WithTexture(patch2[i][j].Texture));

            var outlineRaw = patch2[0].Select(pt => new { Point = p(pt.Vertex.X, pt.Vertex.Z), Texture = pt.Texture }).ToArray();

            var texturize = Ut.Lambda((PointD pt) =>
            {
                var ix = outlineRaw.IndexOf(inf => inf.Point == pt);
                if (ix != -1)
                    return outlineRaw[ix].Texture;
                var pp = pt + p(0, radius1);
                return p(
                    (1 - pp.LengthProjectedOnto(p(-1, 1)) / Math.Sqrt(2) / radius1) * (1 - 2 * textureFrame) + textureFrame,
                    (1 - pp.LengthProjectedOnto(p(1, 1)) / Math.Sqrt(2) / radius1) * (1 - 2 * textureFrame) + textureFrame);
            });

            if (svg == null)
                return Triangulate(outlineRaw.Select(inf => inf.Point))
                    .Select(f => f.Select(p => pt(p.X, h - bevelRadius, p.Y).WithNormal(0, 1, 0).WithTexture(new PointD(p.X + 1, p.Y + 1) / 2)).ToArray());

            var svgPolygons = DecodeSvgPath.Do(svg, bézierSmoothness).Select(poly => poly.Select(pt => (pt - p(5, 5)) * .11).ToArray()).ToArray();
            var outline = Triangulate(outlineRaw.Select(inf => inf.Point).ToArray().Concat(svgPolygons))
                .Select(f => f.Select(p => pt(p.X, patch2[0][0].Vertex.Y, p.Y).WithNormal(0, 1, 0).WithTexture(texturize(p))).Reverse().ToArray());

            return CreateMesh(false, true, patch3)
                .Concat(outline)
                .Concat(svgPolygons.SelectMany(poly => BevelFromCurve(poly.Reverse().Select(p => pt(p.X, h, p.Y)), bevelRadius, roundSteps, Normal.Mine)).Select(face => face.Select(vi => vi.WithTexture(texturize(p(vi.Location.X, vi.Location.Z)))).ToArray()))
            //    .Select(arr => arr.Select(vi => vi.WithTexture(p(vi.Location.X + 1, vi.Location.Z + 1) / 2)).ToArray())
            ;
        }

        private static VertexInfo round(VertexInfo p) { return new VertexInfo(round(p.Location), p.Normal?.Apply(n => round(n)), p.Texture); }
        private static Pt round(Pt p) { return pt(Math.Round(p.X * 1000) / 1000, Math.Round(p.Y * 1000) / 1000, Math.Round(p.Z * 1000) / 1000); }
    }
}
