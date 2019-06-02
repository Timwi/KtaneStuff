using System;
using System.Linq;
using RT.Util.ExtensionMethods;

namespace KtaneStuff
{
    static class ModulesImage
    {
        public static void Update()
        {
            var modules = Ktane.GetLiveJson();
            var ms = modules.Where(m => m["Type"].GetString() != "Widget" /*&& m["Name"].GetString() != "Encrypted Equations"*/).ToArray().Shuffle();
            Utils.ReplaceInFile(@"D:\Daten\Upload\KTANE\Modules.html", "<!--%%-->", "<!--%%%-->", ms.Select(m => $"<div class='module' data-module='{m["Name"].GetString().HtmlEscape()}' data-type='{m["Type"].GetString().HtmlEscape()}'></div>").JoinString("\n"));
            Console.WriteLine(ms.Length);
        }
    }
}