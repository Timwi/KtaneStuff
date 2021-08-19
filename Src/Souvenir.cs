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

    static class Souvenir
    {
        public static void DoModels()
        {
            File.WriteAllText(@"D:\c\KTANE\Souvenir\Assets\Models\AnswerHighlightVeryLong.obj", GenerateObjFile(AnswerHighlight(.39), "AnswerHighlightVeryLong"));
            File.WriteAllText(@"D:\c\KTANE\Souvenir\Assets\Models\AnswerHighlightLong.obj", GenerateObjFile(AnswerHighlight(.21), "AnswerHighlightLong"));
            File.WriteAllText(@"D:\c\KTANE\Souvenir\Assets\Models\AnswerHighlightShort.obj", GenerateObjFile(AnswerHighlight(.15), "AnswerHighlightShort"));
        }

        private static IEnumerable<Pt[]> AnswerHighlight(double width)
        {
            var height = .08;
            var padding = .02;
            var right = width - height / 2;
            return
                Enumerable.Range(0, 37)
                    .Select(i => i * 180 / 36 + 90)
                    .Select(angle => new { Inner = pt((height - padding) / 2 * cos(angle), 0, (height - padding) / 2 * sin(angle)), Outer = pt(height / 2 * cos(angle), 0, height / 2 * sin(angle)) })
                    .SelectConsecutivePairs(false, (i1, i2) => new[] { i1.Inner, i1.Outer, i2.Outer, i2.Inner })
                .Concat(new[] { pt(0, 0, -height / 2), pt(right, 0, -height / 2), pt(right, 0, -(height - padding) / 2), pt(0, 0, -(height - padding) / 2) })
                .Concat(new[] { pt(0, 0, (height - padding) / 2), pt(right, 0, (height - padding) / 2), pt(right, 0, height / 2), pt(0, 0, height / 2) })
                .Concat(new[] { pt(right, 0, -height / 2), pt(right, 0, height / 2), pt(right - height / 3, 0, 0) });
        }

        public static void GenerateGrid()
        {
            (from x in Enumerable.Range(0, 7)
             from y in Enumerable.Range(0, 7)
             select (x, y))
             .ParallelForEach(Environment.ProcessorCount, tup =>
             {
                 var (x, y) = tup;
                 var svg = XDocument.Parse(File.ReadAllText(@"D:\c\KTANE\Souvenir\DataFiles\Grid.svg"));
                 var rect = svg.Descendants().First(e => e.Attribute("id")?.Value == "square");
                 rect.Attribute("x").Value = (x * 10 + 2.25).ToString();
                 rect.Attribute("y").Value = (y * 10 + 2.25).ToString();
                 var path = $@"D:\c\KTANE\Souvenir\DataFiles\temp-{x}-{y}.svg";
                 File.WriteAllText(path, svg.ToString(SaveOptions.DisableFormatting));
                 var cmd = $@"D:\Inkscape\bin\inkscape.exe --export-type=""png"" ""--export-filename=D:\c\KTANE\Souvenir\Assets\Sprites\Tiles7x7\{(char) ('A' + x)}{y + 1}.png"" --export-width=270 --export-height=270 ""{path}""";
                 CommandRunner.RunRaw(cmd).Go();
                 File.Delete(path);
             });
        }
    }
}
