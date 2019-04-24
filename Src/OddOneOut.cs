using System.Collections.Generic;
using System.IO;
using System.Linq;
using KtaneStuff.Modeling;
using RT.Util.ExtensionMethods;
using RT.Util.Geometry;

namespace KtaneStuff
{
    using System.Drawing;
    using System.Drawing.Drawing2D;
    using RT.Util.Drawing;
    using static Md;

    static class OddOneOut
    {
        public static void DoModels()
        {
            File.WriteAllText(@"D:\c\KTANE\OddOneOut\Assets\Models\Button.obj", GenerateObjFile(Button(), "Button"));
            File.WriteAllText(@"D:\c\KTANE\OddOneOut\Assets\Models\ButtonHighlight.obj", GenerateObjFile(ButtonHighlight(), "ButtonHighlight"));
        }

        private static IEnumerable<VertexInfo[]> Button()
        {
            const int steps = 36;
            const double height = .05;
            const double outerRadius = .095;
            const double bottomRadius = .1;
            const double bevelRadius = .01;

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

        public static void RenderPigpen()
        {
            const int m = 23;
            const int w = 20;
            for (int letterIx = 0; letterIx < 26; letterIx++)
            {
                var letter = (char) ('A' + letterIx);
                GraphicsUtil.DrawBitmap(260, 260, g =>
                {
                    using (var pen = new Pen(Color.Black, w) { StartCap = LineCap.Round, EndCap = LineCap.Round })
                    {
                        if ("BEHCFIKNQLOR".Contains(letter))
                            g.DrawLine(pen, m, m, m, 260 - m);
                        if ("DEFGHIMNOPQR".Contains(letter))
                            g.DrawLine(pen, m, m, 260 - m, m);
                        if ("ADGBEHJMPKNQ".Contains(letter))
                            g.DrawLine(pen, 260 - m, m, 260 - m, 260 - m);
                        if ("ABCDEFJKLMNO".Contains(letter))
                            g.DrawLine(pen, m, 260 - m, 260 - m, 260 - m);

                        var ix = "SUVTWYZX".IndexOf(letter);
                        if (ix != -1)
                            using (var tr = new GraphicsTransformer(g).RotateAt(90 * ix, 130, 130))
                            {
                                g.DrawLine(pen, m, m, 130, 260 - m);
                                g.DrawLine(pen, 130, 260 - m, 260 - m, m);
                                if ("WYZX".Contains(letter))
                                    g.FillEllipse(Brushes.Black, 100, 60, 60, 60);
                            }
                        else if ("JKLMNOPQRWXYZ".Contains(letter))
                            g.FillEllipse(Brushes.Black, 100, 100, 60, 60);
                    }
                })
                    .Save($@"D:\c\KTANE\OddOneOut\Assets\Texture\Pigpen\Pigpen-{letter}.png");
            }
        }

        public static void RenderBraille()
        {
            var brailleLetters = @"1;12;14;145;15;124;1245;125;24;245;13;123;134;1345;135;1234;12345;1235;234;2345;136;1236;2456;1346;13456;1356".Split(';');
            for (int letter = 0; letter < 26; letter++)
            {
                GraphicsUtil.DrawBitmap(200, 300, g =>
                {
                    for (int dot = 0; dot < 6; dot++)
                        if (brailleLetters[letter].Contains((dot + 1).ToString()))
                            g.FillEllipse(Brushes.Black, new Rectangle(15 + (dot / 3) * 100, 15 + (dot % 3) * 100, 70, 70));
                }).Save($@"D:\c\KTANE\OddOneOut\Assets\Texture\Braille\Braille-{(char) (letter + 'A')}.png");
            }
        }

        private static IEnumerable<Pt[]> ButtonHighlight()
        {
            const int numVertices = 36;
            const double innerRadius = .8;
            const double outerRadius = 1;

            return Enumerable.Range(0, numVertices)
                .Select(i => new PointD(cos(360.0 * i / numVertices), sin(360.0 * i / numVertices)))
                .SelectConsecutivePairs(true, (p1, p2) => new[] { pt(p1.X, 0, p1.Y) * outerRadius, pt(p2.X, 0, p2.Y) * outerRadius, pt(p2.X, 0, p2.Y) * innerRadius, pt(p1.X, 0, p1.Y) * innerRadius })
                .ToArray();
        }
    }
}