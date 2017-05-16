using System.Linq;
using RT.TagSoup;

namespace KtaneStuff
{
    static class LedEncryption
    {
        public static void DoCheatSheet()
        {
            var values = new[] { 2, 3, 4, 5, 6, 7 };
            var names = new[] { "Red", "Green", "Blue", "Yellow", "Purple", "Orange" };

            Utils.ReplaceInFile(@"D:\c\KTANE\HTML\LED Encryption cheat sheet (Timwi).html", "<!--%%-->", "<!--%%%-->",
                new TABLE(
                    new TR(names.Select(n => new TH(n))),
                    Enumerable.Range(0, 26).Select(c => new TR(Enumerable.Range(0, values.Length).Select(i =>
                         new TD($"{(char) ('A' + c)}{(char) ('A' + (c * values[i]) % 26)}")
                    )))
                ).ToString());
        }
    }
}