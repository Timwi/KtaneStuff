using System;
using System.Linq;
using RT.Util;
using RT.Util.ExtensionMethods;
using System.IO;
using System.Xml.Linq;
using System.Text.RegularExpressions;
using RT.KitchenSink;
using RT.Util.Geometry;

namespace KtaneStuff
{
    internal class WorldMazes
    {
        public static void DACH_GenerateSymbols()
        {
            var xml = XDocument.Parse(Regex.Match(File.ReadAllText(@"D:\c\KTANE\Public\HTML\DACH Maze.html"), @"(?<=<!--%%-->\s*).*?(?=\s*<!--%%%-->)", RegexOptions.Singleline).Value);
            var parents = new[] { "borders-thick", "borders-thin" };
            var shapesG = new[] { "border-shapes-big", "border-shapes-small" }.Select(b => xml.Descendants().First(e => e.AttributeI("id") != null && e.AttributeI("id").Value == b)).ToArray();
            foreach (var sg in shapesG)
                sg.RemoveNodes();
            XName n(string name) => XName.Get(name, xml.Root.Name.NamespaceName);

            foreach (var border in xml.Descendants())
            {
                if (border.Name.LocalName != "path" || border.AttributeI("id") == null || !Regex.IsMatch(border.AttributeI("id").Value, @"^(..)-(..)$"))
                    continue;
                var (pointSum, numPts) = DecodeSvgPath.Do(border.AttributeI("d").Value, .1).SelectMany(x => x).Aggregate((p: new PointD(0, 0), n: 0), (p, n) => (p.p + n, p.n + 1));
                var midPoint = pointSum / numPts;
                var ix = parents.IndexOf(border.Parent.AttributeI("id").Value);
                if (ix == 1)
                    midPoint *= 2.4;
                shapesG[ix].Add(new XElement(n("path"), new XAttribute("d", "M -8,-8 H 8 V 8 H -8 Z"), new XAttribute("transform", $"translate({midPoint.X:0.#}, {midPoint.Y:0.#})"), new XAttribute("id", $"{border.AttributeI("id").Value}-sh")));
            }
            File.WriteAllText(@"D:\temp\temp.svg", xml.ToString(SaveOptions.DisableFormatting));
        }
    }
}