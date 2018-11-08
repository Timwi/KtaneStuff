using System;
using System.Linq;
using System.Windows.Forms;
using RT.Util;
using RT.Util.ExtensionMethods;

namespace KtaneStuff
{
    static class DividedSquares
    {
        public static void Do()
        {
            var rnd = new Random(47);
            var list = Enumerable.Range(0, 30).ToList().Shuffle(rnd);
            var table = Ut.NewArray(36, _ => -1);
            for (int y = 0; y < 6; y++)
                for (int x = 0; x < 6; x++)
                    if (x != y)
                    {
                        table[6 * y + x] = list[0];
                        list.RemoveAt(0);
                    }
            Clipboard.SetText(table.JoinString(","));
            Console.WriteLine(Enumerable.Range(0, 6).Select(row => Enumerable.Range(0, 6).Select(col => table[6 * row + col]).JoinString(", ")).JoinString("\n"));
        }
    }
}