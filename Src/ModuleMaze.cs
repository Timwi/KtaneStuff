using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Xml.Linq;
using RT.TagSoup;
using RT.Util.Consoles;
using RT.Util.ExtensionMethods;

namespace KtaneStuff
{
    static class ModuleMaze
    {
        public static void UpdatePngs()
        {
            var moduleNames = File.ReadAllLines(@"D:\c\KTANE\ModuleMaze\DataFiles\Module names.txt");
            var modules = Ktane.GetLiveJson();
            var wrong = moduleNames.FirstOrDefault(m => !modules.Any(m2 => m2["Name"].GetString() == m) || !File.Exists($@"D:\c\KTANE\Public\Icons\{Regex.Replace(m, @"\?$", "")}.png"));
            if (wrong != null)
                Debugger.Break();

            updateModuleBitmap(@"D:\c\KTANE\ModuleMaze\Assets\Icons\Modules.png", 3, moduleNames);
            updateModuleBitmap(@"D:\c\KTANE\ModuleMaze\Assets\Icons\ModulesO.png", 1, moduleNames);
        }

        public static void UpdateManual()
        {
            var moduleNames = File.ReadAllLines(@"D:\c\KTANE\ModuleMaze\DataFiles\Module names.txt");
            foreach (var f in new[] { @"Module Maze", @"Module Maze with index (Timwi)" })
            {
                Utils.ReplaceInFile($@"D:\c\KTANE\Public\HTML\{f}.html", "<!--%%-->", "<!--%%%-->",
                    Enumerable.Range(0, 400).Select(ix => $@"<image xlink:href='../Icons/{moduleNames[ix].Replace("'", "%27").Replace("?", "")}.png' width='1' height='1' x='{ix % 20}' y='{ix / 20}' preserveAspectRatio='none' opacity='.5' />").JoinString(Environment.NewLine));
                Utils.ReplaceInFile($@"D:\c\KTANE\Public\HTML\{f}.html", "<!--##-->", "<!--###-->",
                    Enumerable.Range(0, 400).Select(ix => $@"<rect class='highlightable' width='1' height='1' x='{ix % 20}' y='{ix / 20}' fill='transparent' />").JoinString(Environment.NewLine));
            }
            Utils.ReplaceInFile(@"D:\c\KTANE\Public\HTML\Module Maze with index (Timwi).html", "<!--@@-->", "<!--@@@-->",
                Enumerable.Range(0, 400).Select(ix => (Index: ix, Name: moduleNames[ix].Replace("'", "’"))).OrderBy(tup => Regex.Replace(tup.Name, "^The ", ""))
                    .Select(tup => $@"<li>{tup.Name.HtmlEscape()}: {(char) (tup.Index % 20 + 'A')}{tup.Index / 20 + 1}</li>").JoinString(Environment.NewLine));
        }

        private static void updateModuleBitmap(string filename, int size, string[] moduleNames)
        {
            using var bmp = new Bitmap(new MemoryStream(File.ReadAllBytes(filename)));
            for (var i = 0; i < 400; i++)
            {
                ConsoleUtil.WriteLine(moduleNames[i].Color(size == 1 ? ConsoleColor.Cyan : ConsoleColor.Yellow));
                using var icon = new Bitmap($@"D:\c\KTANE\Public\Icons\{Regex.Replace(moduleNames[i], @"\?$", "")}.png");
                for (var x = 0; x < 32; x++)
                    for (var y = 0; y < 32; y++)
                    {
                        var pix = icon.GetPixel(x, y);
                        for (var dx = 0; dx < size; dx++)
                            for (var dy = 0; dy < size; dy++)
                                bmp.SetPixel((32 * (i % 20) + x) * size + dx, (32 * (i / 20) + y) * size + dy, pix);
                    }
            }
            bmp.Save(filename);
        }

        public static void WriteConnections()
        {
            var moduleNames = File.ReadAllLines(@"D:\c\KTANE\ModuleMaze\DataFiles\Module names.txt");
            //Clipboard.SetText(Enumerable.Range(0, 400).Select(ix => $"    {21300000 + 2 * ix}: {moduleNames[ix]}").JoinString("\r\n"));
            var big = false;
            Clipboard.SetText(Enumerable.Range(0, 400).Select(ix => $@"
    - serializedVersion: 2
      name: {moduleNames[ix]}
      rect:
        serializedVersion: 2
        x: {(big ? 96 * (ix % 20) : 32 * (ix % 20))}
        y: {(big ? 1824 - 96 * (ix / 20) : 608 - 32 * (ix / 20))}
        width: {(big ? 96 : 32)}
        height: {(big ? 96 : 32)}
      alignment: 0
      pivot: {{x: 0.5, y: 0.5}}
      border: {{x: 0, y: 0, z: 0, w: 0}}
      outline: []
      physicsShape: []
      tessellationDetail: 0").JoinString());
        }
    }
}