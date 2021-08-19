using System;
using System.Text;
using System.Windows.Forms;
using RT.Util;
using RT.Util.ExtensionMethods;

namespace KtaneStuff
{
    static class Codenames
    {
        public static void CreateSheet()
        {
            var boardInfs = Ut.NewArray<(string board, string name)>(
                ("KBRNBRNNNBBBNRNBRNRBRNRRB", "Cold/Food/Defusal"),
                ("BRBNBNBBNRRRNBBBNRRNNKRRN", "Puzzles/Language/Maritime"),
                ("BNRBNBBNNRRNKBRBNNRRBNRRB", "Video Games/Plants/Music"),
                ("BRKNNBRNBRBBNNRNBRNBRRNBR", "Math/History/Body"),
                ("RBBRNBNBRRNKNBBRNNRNRBNRB", "Politics/Sports/Tabletop"));

            var readingOrders = Ut.NewArray<Func<int, int>>(
                // normal reading order
                i => (i + 1) % 25,
                // TR down, then left
                i => (i / 5 == 4 ? (i % 5 + 24) : i + 5) % 25,
                // BR left, then up
                i => (i + 24) % 25,
                // BL up, then right
                i => (i / 5 == 0 ? i % 5 + 21 : i + 20) % 25
            );

            var sb = new StringBuilder();
            foreach (var (board, name) in boardInfs)
                foreach (var step in readingOrders)
                {
                    var b1 = new StringBuilder();
                    var b2 = new StringBuilder();
                    for (int i = 0, cell = 0; i < 25; i++, cell = step(cell))
                    {
                        b1.Append(board[cell] == 'B' ? 'I' : board[cell] == 'R' ? 'I' : board[cell] == 'N' ? 'K' : board[cell]);
                        b2.Append(board[cell]);
                    }
                    sb.AppendLine($"{b1}\t{b2}\t{name.Split('/').JoinString("\t")}");
                }
            Clipboard.SetText(sb.ToString());
        }
    }
}