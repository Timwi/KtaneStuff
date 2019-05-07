using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using RT.Util.ExtensionMethods;

namespace KtaneStuff
{
    static class ModuleMaze
    {
        public static void GenerateIndex()
        {
            var originalHtml = File.ReadAllText(@"D:\c\KTANE\Public\HTML\Module Maze.html");
            var svg = Regex.Match(originalHtml, @"<svg.*?</svg>", RegexOptions.Singleline);
            if (!svg.Success)
                Debugger.Break();
            var xml = XElement.Parse(svg.Value);

            var modules = xml.ElementI("g").ElementsI("image")
                .Select(m => (name: m.AttributeI("href").Value.Replace("../Icons/", "").Replace(".png", ""), x: (int) Math.Round(double.Parse(m.AttributeI("x")?.Value ?? "0") / 8.467), y: (int) Math.Round((double.Parse(m.AttributeI("y")?.Value ?? "0") - 127.667) / 8.467)))
                .OrderBy(tup => Regex.Replace(tup.name, @"^The ", "").ToLowerInvariant())
                .Select(tup => $"<li>{tup.name}: {(char) ('A' + tup.x)}{tup.y + 1}</li>")
                .ToArray();

            Utils.ReplaceInFile(@"D:\c\KTANE\Public\HTML\Module Maze with index (Timwi).html", "<!--%%-->", "<!--%%%-->", modules.JoinString(Environment.NewLine));
        }
    }
}