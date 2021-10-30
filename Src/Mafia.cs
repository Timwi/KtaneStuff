using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using KtaneStuff.Modeling;
using RT.KitchenSink;
using RT.Util;
using RT.Util.Drawing;
using RT.Util.ExtensionMethods;
using RT.Util.Geometry;

namespace KtaneStuff
{
    using static Md;

    public static class Mafia
    {
        public static void DoModels()
        {
            File.WriteAllText(@"D:\c\KTANE\Mafia\Assets\Models\StickFigure.obj", GenerateObjFile(StickFigure(), "StickFigure"));
            File.WriteAllText(@"D:\c\KTANE\Mafia\Assets\Models\StickFigureHighlight.obj", GenerateObjFile(StickFigureHighlight(), "StickFigureHighlight"));
            DoGallows();
        }

        private static void DoGallows()
        {
            IEnumerable<PointD> decode(string str) => DecodeSvgPath.DecodePieces(str).SelectMany(ps => (ps.Points ?? new PointD[0]));
            var data = decode(@"M 40,822.375 40,872.375 70,872.375 70,852.375 87.5,852.375 140,922.375 140,1002.375 100,1002.375 100,1032.375 210,1032.375 210,1002.375 170,1002.375 170,822.375 z M 125,852.375 140,852.375 140,872.375 z")
                .Concat(decode(@"M 45,827.375 45,867.375 65,867.375 65,847.375 90,847.375 145,920.71875 145,1007.375 105,1007.375 105,1027.375 205,1027.375 205,1007.375 165,1007.375 165,827.375 z M 115,847.375 145,847.375 145,887.375 z"))
                .Select(p => new PointD(p.X, p.Y - 802.36218))
                .ToArray();

            //GraphicsUtil.DrawBitmap(500, 500, g =>
            //{
            //    g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
            //    g.Clear(Color.White);
            //    using (var tr = new GraphicsTransformer(g).Scale(2, 2))
            //    {
            //        for (int i = 0; i < data.Length; i++)
            //            if (!new[] { 31, 15, 12, 28 }.Contains(i))
            //                g.DrawLine(new Pen(Color.CornflowerBlue, .5f), data[i].ToPointF(), data[(i + 1) % data.Length].ToPointF());
            //        for (int i = 0; i < data.Length; i++)
            //            g.DrawString(((char) (i < 10 ? '0' + i : 'A' + i - 10)).ToString(), new Font("Niagara Solid", 10f, FontStyle.Bold), Brushes.Black, (float) data[i].X, (float) data[i].Y, new StringFormat { LineAlignment = StringAlignment.Center, Alignment = StringAlignment.Center });
            //    }
            //}).Save(@"D:\temp\temp.png");

            var minX = data.Min(p => p.X);
            var maxX = data.Max(p => p.X);
            var minY = data.Min(p => p.Y);
            var maxY = data.Max(p => p.Y);

            void mkFile(string file, string objName, string code)
            {
                File.WriteAllText(file, GenerateObjFile(
                    code.Split(',').Select(str => str
                        .Select(ch => ch <= '9' ? ch - '0' : ch - 'A' + 10)
                        .Select(ix => pt(data[ix].X, ix >= 16 ? 5 : 0, data[ix].Y).WithTexture((data[ix].X - minX) / (maxX - minX), (data[ix].Y - minY) / (maxY - minY)))
                        .Reverse().ToArray()), 
                    objName, 
                    AutoNormal.FlatIfAbsent));
            }

            mkFile(@"D:\c\KTANE\Mafia\Assets\Models\GallowsFront.obj", "GallowsFront", @"GSUJ,GJIH,KTVL,USRM,NQPO");
            mkFile(@"D:\c\KTANE\Mafia\Assets\Models\GallowsBevel.obj", "GallowsBevel", @"0CSG,CBRS,BAQR,QA9P,OP98,7NO8,76MN,5LM6,4KL5,JK43,J32I,HI21,0GH1,TUED,EUVF,DFVT");
        }

        private static IEnumerable<VertexInfo[]> StickFigure()
        {
            const int revSteps = 72;
            const double middleRadius = 2;
            const double depth = .2;
            const double width = .3;
            const double innerRadius = middleRadius - width;
            const double outerRadius = middleRadius + width;

            foreach (var face in CreateMesh(true, false, Enumerable.Range(0, revSteps)
                .Select(i => 360.0 * i / revSteps)
                .Select(angle => Ut.NewArray(
                    pt(innerRadius * cos(angle), 0, innerRadius * sin(angle) + 5).WithMeshInfo(-cos(angle), 0, -sin(angle)),
                    pt(middleRadius * cos(angle), depth, middleRadius * sin(angle) + 5).WithMeshInfo(0, 1, 0),
                    pt(outerRadius * cos(angle), 0, outerRadius * sin(angle) + 5).WithMeshInfo(cos(angle), 0, sin(angle))))
                .ToArray()))
                yield return face;

            var lines = Ut.NewArray(
                new { From = p(0, 3), To = p(0, -1) },
                new { From = p(0, 2), To = p(-3, 0) },
                new { From = p(0, 2), To = p(3, 0) },
                new { From = p(0, -1), To = p(-2, -6) },
                new { From = p(0, -1), To = p(2, -6) });
            foreach (var line in lines)
            {
                var p1 = pt(line.From.X, 0, line.From.Y);
                var p2 = pt(line.To.X, 0, line.To.Y);
                var vector = ((p2 - p1) * pt(0, 1, 0)).Normalize() * width;
                yield return Ut.NewArray((p1 + vector).WithNormal((p2 - p1) * pt(0, 1, 0)), (p2 + vector).WithNormal((p2 - p1) * pt(0, 1, 0)), p2.Add(y: depth).WithNormal(pt(0, 1, 0)), p1.Add(y: depth).WithNormal(pt(0, 1, 0)));
                yield return Ut.NewArray(p1.Add(y: depth).WithNormal(pt(0, 1, 0)), p2.Add(y: depth).WithNormal(pt(0, 1, 0)), (p2 - vector).WithNormal(-(p2 - p1) * pt(0, 1, 0)), (p1 - vector).WithNormal(-(p2 - p1) * pt(0, 1, 0)));
            }
        }

        private static IEnumerable<VertexInfo[]> StickFigureHighlight()
        {
            const int revSteps = 72;
            const double middleRadius = 2;
            const double width = .75;
            const double innerRadius = middleRadius - width;
            const double outerRadius = middleRadius + width;

            foreach (var face in CreateMesh(true, false, Enumerable.Range(0, revSteps)
                .Select(i => 360.0 * i / revSteps)
                .Select(angle => Ut.NewArray(
                    pt(outerRadius * cos(angle), 0, outerRadius * sin(angle) + 5).WithMeshInfo(0, 1, 0),
                    pt(innerRadius * cos(angle), 0, innerRadius * sin(angle) + 5).WithMeshInfo(0, 1, 0)))
                .ToArray()))
                yield return face;

            var lines = Ut.NewArray(
                new { From = p(0, 3), To = p(0, -1) },
                new { From = p(0, 2), To = p(-3.3, -.2) },
                new { From = p(0, 2), To = p(3.3, -.2) },
                new { From = p(0, -1), To = p(-2.2, -6.5) },
                new { From = p(0, -1), To = p(2.2, -6.5) });
            foreach (var line in lines)
            {
                var p1 = pt(line.From.X, 0, line.From.Y);
                var p2 = pt(line.To.X, 0, line.To.Y);
                var vector = ((p2 - p1) * pt(0, 1, 0)).Normalize() * width;
                yield return Ut.NewArray((p1 + vector).WithNormal(pt(0, 1, 0)), (p1 - vector).WithNormal(pt(0, 1, 0)), (p2 - vector).WithNormal(pt(0, 1, 0)), (p2 + vector).WithNormal(pt(0, 1, 0)));
            }
        }
    }
}