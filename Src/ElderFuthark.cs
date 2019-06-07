using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using RT.Util.ExtensionMethods;

namespace KtaneStuff
{
    static class ElderFuthark
    {
        public static void ReplaceRunesInManual()
        {
            var svg = XDocument.Parse(File.ReadAllText(@"D:\c\KTANE\KtaneStuff\DataFiles\ElderFuthark\Runes.svg"));
            var paths = svg.Root.ElementsI("path").ToArray();
            var chars = "abcdefghijylmnopzrstuvx";
            Console.WriteLine(paths.Length);
            Console.WriteLine(chars.Length);

            if (paths.Length != 23 || chars.Length != 23)
                Debugger.Break();

            for (int i = 0; i < 23; i++)
                Utils.ReplaceInFile(@"D:\c\KTANE\Public\HTML\Elder Futhark.html", $"<!--rune-{chars[i]}-->", $"<!--/rune-{chars[i]}-->", $@"<svg viewBox='{7.5 + 10 * i} 7 5 6'><path d='{paths[i].AttributeI("d").Value}' /></svg>");
        }
    }
}