using System;
using System.Linq;
using RT.Util.ExtensionMethods;

namespace KtaneStuff
{
    static class Mineseeker
    {
        private static readonly int[][] colors = new int[][]
        {
            new int[] { 3, 7, 14, 14, 14, 11, 14, 14, 12, 13, 6, 2 },
            new int[] { 4, 6, 14, 11, 14, 3, 0, 14, 14, 5, 14, 9 },
            new int[] { 14, 1, 14, 14, 4, 13, 8, 14, 14, 7, 2, 10 },
            new int[] { 14, 14, 7, 12, 0, 1, 14, 6, 3, 1, 14, 14 },
            new int[] { 1, 8, 14, 6, 5, 13, 10, 14, 1, 0, 14, 14 },
            new int[] { 14, 9, 0, 14, 14, 14, 14, 8, 9, 14, 4, 1 },
            new int[] { 12, 13, 1, 3, 14, 14, 2, 11, 4, 14, 14, 14 },
            new int[] { 10, 14, 11, 14, 9, 6, 8, 5, 14, 14, 3, 14 },
            new int[] { 14, 14, 14, 13, 1, 5, 14, 9, 14, 14, 7, 8 },
            new int[] { 11, 14, 5, 10, 8, 14, 7, 14, 0, 2, 14, 14 },
            new int[] { 14, 2, 10, 3, 14, 14, 13, 7, 14, 14, 4, 12 },
            new int[] { 0, 14, 14, 4, 14, 14, 14, 12, 9, 10, 5, 6 }
        };
        private static readonly int[][] bombs = new int[][]
        {
            new int[] { 5, 4, 7, 7, 7, 3, 7, 7, 0, 1, 2, 6 },
            new int[] { 6, 5, 7, 3, 7, 1, 4, 7, 7, 2, 7, 0 },
            new int[] { 7, 1, 7, 7, 2, 0, 6, 7, 7, 3, 4, 5 },
            new int[] { 7, 7, 4, 2, 1, 6, 7, 0, 3, 5, 7, 7 },
            new int[] { 4, 2, 7, 0, 3, 5, 1, 7, 5, 6, 7, 7 },
            new int[] { 7, 0, 6, 7, 7, 7, 7, 2, 1, 7, 3, 4 },
            new int[] { 2, 3, 1, 5, 7, 7, 0, 6, 4, 7, 7, 7 },
            new int[] { 0, 7, 5, 7, 6, 2, 3, 4, 7, 7, 1, 7 },
            new int[] { 7, 7, 7, 5, 0, 4, 7, 1, 7, 7, 6, 3 },
            new int[] { 1, 7, 3, 6, 4, 7, 5, 7, 2, 0, 7, 7 },
            new int[] { 7, 6, 0, 4, 7, 7, 2, 3, 7, 7, 5, 1 },
            new int[] { 3, 7, 7, 1, 7, 7, 7, 5, 6, 4, 0, 2 }
        };
        private static readonly string[,] walls = new string[,]
        {
            { "R", "LR",  "LR",  "LR",  "LR",  "LDR",  "LR",  "L",  "D",  "R", "LR", "LD" },
            { "D", "DR", "LDR", "LDR", "L", "DU", "DR", "LR", "LDRU", "LDR", "L", "DU" },
            { "DU", "DRU", "LDRU", "LU", "R", "LDU", "DU", "D", "RU", "LDU", "D", "DU" },
            { "DU", "DRU", "LU", "R", "LD", "U", "DU", "DRU", "LD", "U", "DU", "DU" },
            { "DU", "DU", "R", "LD", "RU", "LD", "U", "DRU", "LDRU", "LR", "LU", "DU" },
            { "DU", "RU", "L", "DRU", "LD", "DU", "R", "LRU", "LU", "R", "LDR", "LDU" },
            { "DRU", "LDR", "LD", "RU", "LDU", "RU", "LR", "LD", "R", "LD", "RU", "LU" },
            { "DRU", "LRU", "LU", "D", "RU", "LDR", "LD", "RU", "LD", "RU", "LDR", "LD" },
            { "U", "DR", "LR", "LU", "D", "RU", "LRU", "L", "RU", "L", "DRU", "LDU" },
            { "DR", "LDU", "R", "LR", "LDU", "DR", "LR", "LD", "R", "LR", "LDRU", "LU" },
            { "DRU", "LDRU", "LR", "L", "DU", "U", "D", "RU", "LDR", "LD", "U", "D" },
            { "RU", "LU", "R", "LR", "LRU", "LR", "LRU", "L", "RU", "LRU", "LR", "LU" }
        };

        public static void DoManualTable()
        {
            Utils.ReplaceInFile(@"D:\c\KTANE\Public\HTML\Mineseeker.html",
                "<!--maze-start-->",
                "<!--maze-end-->",
                Enumerable.Range(0, 12).Select(r => $@"<tr>{Enumerable.Range(0, 12).Select(c => $@"<td class='{walls[r, c].ToLowerInvariant().JoinString(" ")}'>{(colors[r][c] == 14 ? null : $"<div class='color-{colors[r][c]} bomb-{bombs[r][c]}'></div>")}</td>").JoinString()}</tr>").JoinString("\r\n"));
        }
    }
}