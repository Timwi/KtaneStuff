using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using KtaneStuff.Modeling;
using RT.Util;
using RT.Util.ExtensionMethods;

namespace KtaneStuff
{
    using static Md;

    static class Concentration
    {
        public static void CreateGraphics()
        {
            Enumerable.Range(1, 15).ParallelForEach(Environment.ProcessorCount, i =>
            {
                var xml = XDocument.Parse(File.ReadAllText(@"D:\c\KTANE\Concentration\Data\Card design.svg"));
                var elem = xml.Descendants().Where(e => e.AttributeI("id")?.Value == "text838").First();
                elem.Value = i.ToString();
                var tmpPath = $@"D:\c\KTANE\Concentration\Data\Card design-{i}.svg";
                File.WriteAllText(tmpPath, xml.ToString());

                var cmd = $@"D:\Inkscape\bin\inkscape.exe --export-type=""png"" ""--export-filename=D:\c\KTANE\Concentration\Assets\Textures\Card-{i}.png"" --export-width=340 --export-height=280 ""D:\c\KTANE\Concentration\Data\Card design-{i}.svg""";
                CommandRunner.RunRaw(cmd).Go();
                File.Delete($@"D:\c\KTANE\Concentration\Data\Card design-{i}.svg");
            });
        }

        public static void CreateHighlightModel()
        {
            const double w = 100;
            const double h = 100 * 0.0266 / 0.0323;
            const int rev = 9;    // revolutions per corner
            const double ir = 3d / 34 * 100; // inner radius
            const double or = ir + 2d / 34 * 100; // outer radius

            var polys = new List<VertexInfo[]>();
            for (var corner = 0; corner < 4; corner++)
            {
                var xf = new[] { 1, -1, -1, 1 }[corner];
                var yf = new[] { 1, 1, -1, -1 }[corner];
                for (var i = 0; i < rev; i++)
                {
                    var angle1 = Math.PI / 2 * ((double) i / rev + corner);
                    var angle2 = Math.PI / 2 * ((double) (i + 1) / rev + corner);
                    polys.Add(Ut.NewArray(
                        p(xf * (w / 2 - ir) + ir * Math.Cos(angle1), yf * (h / 2 - ir) + ir * Math.Sin(angle1)),
                        p(xf * (w / 2 - ir) + or * Math.Cos(angle1), yf * (h / 2 - ir) + or * Math.Sin(angle1)),
                        p(xf * (w / 2 - ir) + or * Math.Cos(angle2), yf * (h / 2 - ir) + or * Math.Sin(angle2)),
                        p(xf * (w / 2 - ir) + ir * Math.Cos(angle2), yf * (h / 2 - ir) + ir * Math.Sin(angle2))
                    )
                        .Select(p => pt(p.X, 0, p.Y).WithNormal(0, 1, 0))
                        .ToArray());
                }
                var xf2 = new[] { 1, -1, -1, 1 }[(corner + 1) % 4];
                var yf2 = new[] { 1, 1, -1, -1 }[(corner + 1) % 4];
                var angle = Math.PI / 2 * (1 + corner);
                polys.Add(Ut.NewArray(
                    p(xf * (w / 2 - ir) + ir * Math.Cos(angle), yf * (h / 2 - ir) + ir * Math.Sin(angle)),
                    p(xf * (w / 2 - ir) + or * Math.Cos(angle), yf * (h / 2 - ir) + or * Math.Sin(angle)),
                    p(xf2 * (w / 2 - ir) + or * Math.Cos(angle), yf2 * (h / 2 - ir) + or * Math.Sin(angle)),
                    p(xf2 * (w / 2 - ir) + ir * Math.Cos(angle), yf2 * (h / 2 - ir) + ir * Math.Sin(angle))
                )
                    .Select(p => pt(p.X, 0, p.Y).WithNormal(0, 1, 0))
                    .ToArray());
            }

            File.WriteAllText(@"D:\c\KTANE\Concentration\Assets\Models\CardHighlight.obj",
                GenerateObjFile(polys, "CardHighlight"));
        }
    }
}