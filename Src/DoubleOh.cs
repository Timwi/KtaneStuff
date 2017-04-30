using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using KtaneStuff.Modeling;
using RT.KitchenSink;
using RT.Util;
using RT.Util.ExtensionMethods;
using RT.Util.Geometry;

namespace KtaneStuff
{
    using static Md;

    static class DoubleOh
    {
        public static void DoModels()
        {
            foreach (var inf in new[] { SvgInfo.SingleArrow, SvgInfo.DoubleArrow, SvgInfo.Submit })
                File.WriteAllText($@"D:\c\KTANE\DoubleOh\Assets\Models\Button{inf.Name}.obj", GenerateObjFile(Button(inf.Svg), $"Button{inf.Name}"));
            File.WriteAllText(@"D:\c\KTANE\DoubleOh\Assets\Models\ButtonTop.obj", GenerateObjFile(Button(null), "ButtonTop"));
            File.WriteAllText(@"D:\c\KTANE\DoubleOh\Assets\Models\ButtonHighlight.obj", GenerateObjFile(Button("Highlight"), "ButtonHighlight"));
            File.WriteAllText(@"D:\c\KTANE\DoubleOh\Assets\Models\ButtonCollider.obj", GenerateObjFile(Box().Select(face => face.Select(p => new Pt(p.X * .85, p.Y * .65, p.Z * .85).RotateY(45)).ToArray()), "ButtonCollider"));
            File.WriteAllText(@"D:\c\KTANE\DoubleOh\Assets\Models\Frame.obj", GenerateObjFile(Frame(), "Frame"));

            //var g = XDocument.Parse(File.ReadAllText(@"D:\c\KTANE\DoubleOh\Assets\Sources\Segments.svg")).Root.Elements().FirstOrDefault(e => e.Name.LocalName == "g");
            //foreach (var path in g.Elements().Where(e => e.Name.LocalName == "path"))
            //{
            //    // -982.36218
            //    var svg = DecodeSvgPath.DecodePieces(path.Attributes().FirstOrDefault(a => a.Name.LocalName == "d").Value).Select(piece => piece.Select(p => p + new PointD(0, -982.36218)));
            //    Console.WriteLine(svg.JoinString(" "));
            //}

            var segments = Ut.NewArray(
                @"M2.5, 3.1875 0.5, 5.1875 0.5, 29.78122 5, 34.28122 9.5, 29.78122 9.5, 10.18746 z",
                @"M37.5, 3.1875 30.5, 10.18746 30.5, 29.78122 35, 34.28122 39.5, 29.78122 39.5, 5.1875 z",
                @"M5.1875, 0.5 3.1875, 2.5 10.21875, 9.5 29.78125, 9.5 36.8125, 2.5 34.8125, 0.5 z",
                @"M5, 35.68752 0.5, 40.18752 0.5, 64.78122 2.5, 66.78122 9.5, 59.78122 9.5, 40.18752 z",
                @"M35, 35.68752 30.5, 40.18752 30.5, 59.78122 37.5, 66.78122 39.5, 64.78122 39.5, 40.18752 z",
                @"M10.1875, 30.5 5.6875, 35 10.21875, 39.5 29.78125, 39.5 34.3125, 35 29.8125, 30.5 z",
                @"M10.1875, 60.5 3.1875, 67.5 5.21875, 69.5 34.78125, 69.5 36.8125, 67.5 29.8125, 60.5 z"
            );
            for (int i = 0; i < segments.Length; i++)
                File.WriteAllText($@"D:\c\KTANE\DoubleOh\Assets\Models\Segment{i}.obj", GenerateObjFile(Triangulate(DecodeSvgPath.Do(segments[i], 1)).Select(arr => arr.Select(p => pt(p.X, 0, p.Y).WithNormal(0, 1, 0)).Reverse().ToArray()), $"Segment{i}"));
        }

        private static MeshVertexInfo[] bpa(double x, double y, double z, Normal befX, Normal afX, Normal befY, Normal afY) { return new[] { pt(x, y, z, befX, afX, befY, afY).WithTexture((x + 1) / 2, (z + 1) / 2) }; }
        private static IEnumerable<VertexInfo[]> Frame()
        {
            var depth = .06;
            var béFac = depth * .55;
            var ratio = .6;
            var th = .2;

            return CreateMesh(true, true,
                Bézier(p(th, 0), p(th, béFac), p(th - depth + béFac, depth), p(th - depth, depth), 20).Select((p, first, last) => new BevelPoint(p.X, p.Y, first || last ? Normal.Mine : Normal.Average, first || last ? Normal.Mine : Normal.Average)).Concat(
                Bézier(p(depth, depth), p(depth - béFac, depth), p(0, béFac), p(0, -.2), 20).Select((p, first, last) => new BevelPoint(p.X, p.Y, first || last ? Normal.Mine : Normal.Average, first || last ? Normal.Mine : Normal.Average)))
                .Select(bi => Ut.NewArray(
                    // Bottom right
                    bpa(-1 + bi.Into, bi.Y, -ratio + bi.Into, bi.Before, bi.After, Normal.Mine, Normal.Mine),

                    // Top right
                    bpa(-1 + bi.Into, bi.Y, ratio - bi.Into, bi.Before, bi.After, Normal.Mine, Normal.Mine),

                    // Top left
                    bpa(1 - bi.Into, bi.Y, ratio - bi.Into, bi.Before, bi.After, Normal.Mine, Normal.Mine),

                    // Bottom left
                    bpa(1 - bi.Into, bi.Y, -ratio + bi.Into, bi.Before, bi.After, Normal.Mine, Normal.Mine),

                    null
                ).Where(x => x != null).SelectMany(x => x).ToArray()).ToArray());
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

            if (svg == "Highlight")
            {
                const double f = 1.1;
                return patch2.Last()
                    .Select(p => new { Point1 = pt(p.Vertex.X, y4, p.Vertex.Z), Point2 = pt(p.Vertex.X * f, y1, p.Vertex.Z * f) })
                    .SelectManyConsecutivePairs(true, (i1, i2) => new[] { new[] { i1.Point1, i1.Point2, i2.Point2, i2.Point1 }, new[] { i1.Point2, i1.Point1, i2.Point1, i2.Point2 } })
                    .Select(arr => arr.Select(p => new VertexInfo(p, null)).ToArray());
            }

            var svgPolygons = DecodeSvgPath.Do(svg, bézierSmoothness).Select(poly => poly.Select(pt => (pt - p(5, 5)) * .11).ToArray()).ToArray();
            var outline = Triangulate(outlineRaw.Select(inf => inf.Point).ToArray().Concat(svgPolygons))
                .Select(f => f.Select(p => pt(p.X, patch2[0][0].Vertex.Y, p.Y).WithNormal(0, 1, 0).WithTexture(texturize(p))).Reverse().ToArray());

            return CreateMesh(false, true, patch3)
                .Concat(outline)
                .Concat(svgPolygons.SelectMany(poly => BevelFromCurve(poly.Reverse().Select(p => pt(p.X, h, p.Y)), bevelRadius, roundSteps, Normal.Mine)).Select(face => face.Select(vi => vi.WithTexture(texturize(p(vi.Location.X, vi.Location.Z)))).ToArray()));
        }

        private static VertexInfo round(VertexInfo p) { return new VertexInfo(round(p.Location), p.Normal?.Apply(n => round(n)), p.Texture); }
        private static Pt round(Pt p) { return pt(Math.Round(p.X * 1000) / 1000, Math.Round(p.Y * 1000) / 1000, Math.Round(p.Z * 1000) / 1000); }
    }
}
