using System;
using System.Linq;
using RT.TagSoup;
using RT.Util.ExtensionMethods;

namespace KtaneStuff
{
    public static class ColorGenerator
    {
        public static void GenerateCheatSheet()
        {
            string ch(int n) => Enumerable.Range(0, 10).Where(v => v == n).Select(v => (char) ('0' + v)).Concat(Enumerable.Range(0, 26).Where(v => (v + 1) % 16 == n).Select(v => (char) ('A' + v))).JoinString();
            Utils.ReplaceInFile(@"D:\c\KTANE\Public\HTML\Color Generator cheat sheet (Timwi).html", "<!--%%-->", "<!--%%%-->",
                new TABLE { class_ = "color-generator" }._(
                    new TR(new TD { class_ = "corner" }, Enumerable.Range(0, 16).Select(i => new TH(ch(i)))),
                    Enumerable.Range(0, 16).Select(row => new TR(
                        new TH(ch(row)),
                        Enumerable.Range(0, 16).Select(col => new TD(col * 16 + row))))).ToString());
        }
    }
}