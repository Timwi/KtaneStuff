using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using KtaneStuff.Modeling;
using RT.KitchenSink;
using RT.Util.Consoles;
using RT.Util.ExtensionMethods;

namespace KtaneStuff
{
    using static Md;

    static class Quilting
    {
        public static void MakeModels()
        {
            foreach (var svgFile in new DirectoryInfo(@"D:\c\KTANE\Quilting\DataFiles").EnumerateFiles("*.svg"))
            {
                var doc = XDocument.Parse(File.ReadAllText(svgFile.FullName));
                var paths = doc.Root.ElementsI("path").ToArray();
                if (paths.Length != 20)
                    System.Diagnostics.Debugger.Break();
                for (var pathIx = 0; pathIx < paths.Length; pathIx++)
                {
                    var path = paths[pathIx];
                    var data = path.AttributeI("d").Value;

                    ConsoleUtil.WriteLine($"{svgFile.FullName.Color(ConsoleColor.Cyan)} — {path.AttributeI("id").Value.Color(ConsoleColor.Yellow)}", null);

                    //Utils.ReplaceInFile(@"D:\temp\temp.svg", "<!--#-->", "<!--##-->", $@"<path fill='none' stroke='black' stroke-width='2' d='{data}'/>");
                    //Utils.ReplaceInFile(@"D:\temp\temp.svg", "<!--%-->", "<!--%%-->", $@"<path fill='none' stroke='red' stroke-width='1' d='{DecodeSvgPath.Do(data, .1).Select(pol => $"M{pol.Select(p => $"{p.X} {p.Y}").JoinString(" ")}").JoinString()}z'/>");
                    File.WriteAllText($@"D:\c\KTANE\Quilting\Assets\Models\Patches\{Path.GetFileNameWithoutExtension(svgFile.Name)}-{pathIx}.obj", GenerateObjFile(DecodeSvgPath.DecodePieces(data).Extrude(1, .1, true)));
                }
            }
        }
    }
}