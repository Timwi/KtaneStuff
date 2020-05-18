using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using KtaneStuff.Modeling;
using RT.Util;
using RT.Util.ExtensionMethods;
using RT.Util.Geometry;

namespace KtaneStuff
{
    using static Modeling.Md;

    static class ColorBraille
    {
        public static void FindWords()
        {
            var braille = Braille.BrailleRaw.ToDictionary(inf => inf.Bit, inf => inf);
            var brailleRegex = new Regex($@"({braille.OrderByDescending(kvp => kvp.Key.Length).Select(kvp => $"{(kvp.Value.CanBeInitial ? "" : "(?!^)")}{kvp.Key}{(kvp.Value.CanBeFinal ? "" : "(?!$)")}").JoinString("|")})", RegexOptions.Compiled);

            var words = File.ReadAllLines(@"D:\Daten\Wordlists\VeryCommonWords.txt")
                .Select(m => m.ToLowerInvariant())
                .Where(w => w.Length >= 5)
                .Distinct()
                .Select(w => new { Word = w, Braille = brailleRegex.Matches(w).Cast<Match>().Select(m => braille[m.Value].Dots).ToArray() })
                .Where(inf => inf.Braille.Length == 5)
                .OrderBy(inf => inf.Word)
                .ToArray();

            Utils.ReplaceInFile(@"D:\c\KTANE\ColorBraille\Assets\WordsData.cs", "//#words-start", "//#words-end",
                words.Select(inf => $@"{{ ""{inf.Word}"", new[] {{ {inf.Braille.JoinString(", ")} }} }}").JoinString(",\r\n"));
            Console.WriteLine(words.Length);
        }

        public static void DoModels()
        {
            const int rev = 32;
            const double radius = .075;
            const double bottom = .12;
            const double top = .15;

            var circles = new List<(double x, double y, PointD[] pts)>();
            for (var dx = 0; dx < 5; dx++)
                for (var dx2 = 0; dx2 < 2; dx2++)
                    for (var dy = 0; dy < 3; dy++)
                    {
                        var x = -.71 + (1.42 / 4 * dx) - .08 + .16 * dx2;
                        var y = -.16 + dy * .16;
                        circles.Add((x, y, Enumerable.Range(0, rev).Select(i => i * Math.PI * 2 / rev).Select(angle => new PointD(x, y) + radius * new PointD(angle)).ToArray()));
                    }

            // Part of the module plate surrounding the inset holes
            var outerShape = Ut.NewArray(
                new PointD(0.854439, 0.256271),
                new PointD(0.898415, 0.193691),
                new PointD(0.898415, -0.193691),
                new PointD(0.854439, -0.256271),
                new PointD(-0.854439, -0.256271),
                new PointD(-0.898415, -0.193691),
                new PointD(-0.898415, 0.193691),
                new PointD(-0.854439, 0.256271)
            );
            var aroundDiscs = new List<PointD[]> { outerShape };
            foreach (var (x, y, pts) in circles)
                aroundDiscs.Add(pts);

            static PointD texture(PointD p) => new PointD(.4771284794 * p.X + .46155, -.4771284794 * p.Y + .5337373145);

            // The inset holes themselves
            var vertexInfos = new List<VertexInfo[]>();
            foreach (var (x, y, pts) in circles)
            {
                vertexInfos.AddRange(CreateMesh(false, true, Ut.NewArray(2, rev, (d, ix) =>
                {
                    if (d == 0)
                        return new Pt(x, bottom, y).WithMeshInfo(0, 1, 0).WithTexture(texture(new PointD(x, y)));
                    return new Pt(pts[rev - 1 - ix].X, top, pts[rev - 1 - ix].Y).WithMeshInfo(Normal.Mine, Normal.Mine, Normal.Average, Normal.Average).WithTexture(texture(pts[rev - 1 - ix]));
                })));
            }

            File.WriteAllText(@"D:\c\KTANE\ColorBraille\Assets\Models\AroundDiscs.obj", GenerateObjFile(aroundDiscs.Triangulate()
                .Select(tri => tri.Select(p => pt(p.X, top, p.Y).WithNormal(0, 1, 0).WithTexture(texture(p))).Reverse().ToArray())
                .Concat(vertexInfos), @"AroundDiscs"));


            // Rounded cylinder
            const double b = .4;
            const double f = .55;
            var curve = new[] { (p(0, 0), (Normal?) null) }.Concat(Bézier(p(1 - b, 0), p(1 - b + f * b, 0), p(1, b - f * b), p(1, b), 3).Select(p => (p, (Normal?) Normal.Average))).Concat(new[] { (p(1, 1), (Normal?) Normal.Mine) }).ToArray();
            File.WriteAllText(@"D:\c\KTANE\ColorBraille\Assets\Models\RoundedCylinder.obj", GenerateObjFile(CreateMesh(false, true, Ut.NewArray(curve.Length, rev, (cIx, angle) =>
            {
                (var p, var normal) = curve[cIx];
                var r = p.X;
                var point = pt(r * cos(-angle * 360.0 / rev), -p.Y, r * sin(-angle * 360.0 / rev));
                if (normal == null)
                    return point.WithMeshInfo(0, 1, 0);
                return point.WithMeshInfo(normal.Value, normal.Value, Normal.Average, Normal.Average);
            })), "RoundedCylinder"));
        }
    }
}