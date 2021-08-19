using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using KtaneStuff.Modeling;
using RT.Util;
using RT.Util.ExtensionMethods;

namespace KtaneStuff
{
    static class SimonSignals
    {
        public static void CreateArrows()
        {
            var svg = XDocument.Parse(File.ReadAllText(@"D:\c\KTANE\SimonSignals\Data\ArrowShapes.svg"));
            var colorNames = "red,pink,green,yellow,blue,cyan,gray,white".Split(',');
            var gradientColors = colorNames
                .Select(color => svg.Descendants().First(e => e.Attribute("id")?.Value == $"gr-{color}"))
                .Select(e =>
                {
                    var matches = e.Elements().Select(c => Regex.Match(c.Attribute("style").Value, @"^stop-color:(#[a-f0-9]{6});")).ToArray();
                    if (matches.Any(m => !m.Success))
                        Debugger.Break();
                    return matches.Select(m => m.Groups[1].Value).ToArray();
                })
                .ToArray();

            if (gradientColors.Length != 8)
                Debugger.Break();

            var shapes = Enumerable.Range(0, 8)
                .Select(i => svg.Descendants().First(e => e.Attribute("id")?.Value == $"ar-{i / 2}-{i % 2}").Attribute("d").Value)
                .ToArray();

            if (shapes.Length != 8)
                Debugger.Break();

            (from shape in Enumerable.Range(0, 8)
             from color in Enumerable.Range(0, 8)
             select (shape, color))
                .ParallelForEach(Environment.ProcessorCount, tup =>
                {
                    var (shape, color) = tup;
                    var template = XDocument.Parse(File.ReadAllText(@"D:\c\KTANE\SimonSignals\Data\ArrowShapes-template.svg"));
                    template.Root.Attribute("viewBox").Value = $"{10 * (shape / 2)} {10 * (shape % 2)} 10 10";
                    template.Descendants().First(e => e.Attribute("id")?.Value == "rad-gr").Attribute("cx").Value = $"{5 + 10 * (shape / 2)}";
                    template.Descendants().First(e => e.Attribute("id")?.Value == "rad-gr").Attribute("cy").Value = $"{5 + 10 * (shape % 2)}";
                    template.Descendants().First(e => e.Attribute("id")?.Value == "rad-gr").Attribute("fx").Value = $"{5 + 10 * (shape / 2)}";
                    template.Descendants().First(e => e.Attribute("id")?.Value == "rad-gr").Attribute("fy").Value = $"{5 + 10 * (shape % 2)}";
                    template.Descendants().First(e => e.Attribute("id")?.Value == "stop-1").Attribute("style").Value = $"stop-color:{gradientColors[color][0]};stop-opacity:1";
                    template.Descendants().First(e => e.Attribute("id")?.Value == "stop-2").Attribute("style").Value = $"stop-color:{gradientColors[color][1]};stop-opacity:1";
                    template.Descendants().First(e => e.Attribute("id")?.Value == "arrow").Attribute("d").Value = shapes[shape];
                    var tmpSvgPath = $@"D:\c\KTANE\SimonSignals\Data\ArrowShapes-tmp-{shape}-{color}.svg";
                    File.WriteAllText(tmpSvgPath, template.ToString());
                    var arrowIx = (color % 2) + 2 * (shape + 8 * (color >> 1));
                    var cmd = $@"D:\Inkscape\bin\inkscape.exe --export-type=""png"" ""--export-filename=D:\c\KTANE\SimonSignals\Assets\Textures\Arrow-{arrowIx}.png"" --export-width=320 --export-height=320 ""{tmpSvgPath}""";
                    CommandRunner.RunRaw(cmd).Go();
                    File.Delete(tmpSvgPath);
                });
        }
    }
}