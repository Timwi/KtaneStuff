using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using RT.Util.ExtensionMethods;

namespace KtaneStuff
{
    static partial class ThreeDMaze
    {
        public static void Do3DMazeManual(bool cheatSheet)
        {
            var outputs = new List<string>();
            var headers = cheatSheet ? "ABC|ABD|ABH|ACD|ACH|ADH|BCD|BCH|BDH|CDH".Split('|').Select(h => $"<div class='header'>{h}</div>").ToArray() : null;
            for (int mapIndex = 0; mapIndex < 10; mapIndex++)
            {
                string[] labels, nWalls, wWalls;

                switch (mapIndex)
                {
                    // ABC
                    case 0:
                        labels = new string[] { "     A  ", " *A    B", "A  B C  ", " C  *  B", "    A   ", " B C  B ", "* C     ", "    A C " };
                        nWalls = new string[] { "11000110", "00001000", "01011100", "00100011", "10011001", "00000100", "00110010", "11000001" };
                        wWalls = new string[] { "10101001", "10100100", "00010001", "01010110", "01010001", "01100100", "01001101", "00101100" };
                        break;

                    // ABD
                    case 1:
                        labels = new string[] { "A  B  A*", "  D     ", "     D B", " A B    ", "  *   A ", "D   A   ", "  B  D  ", " D  *  B" };
                        nWalls = new string[] { "10100011", "01100100", "00011010", "00100000", "11100110", "00010100", "10100001", "00011000" };
                        wWalls = new string[] { "10000010", "11000100", "00010001", "01000100", "10010001", "00010100", "01000101", "00010100" };
                        break;

                    // ABH
                    case 2:
                        labels = new string[] { "B    A H", "* H     ", "B   B   ", "    * HA", " A H    ", "    A B ", " B   *  ", "A  H    " };
                        nWalls = new string[] { "11101011", "01011000", "10001101", "01010000", "00001000", "00110100", "00101110", "01100000" };
                        wWalls = new string[] { "10010000", "01000001", "00110011", "11010101", "11010010", "11100000", "00010101", "00000001" };
                        break;

                    // ACD
                    case 3:
                        labels = new string[] { "D       ", "  C D* C", " *   C  ", " A      ", "D  C D  ", "  A  * A", "   A  D ", "A    C  " };
                        nWalls = new string[] { "10111011", "01100110", "00000111", "01000000", "11101110", "01100100", "00010110", "01101100" };
                        wWalls = new string[] { "00001000", "11001100", "01110010", "10011001", "10001001", "01100101", "01010000", "10000010" };
                        break;

                    // ACH
                    case 4:
                        labels = new string[] { "H C   A ", "*   H   ", "      *C", " A   H  ", "C H C A ", " *     A", "   C H  ", "  A     " };
                        nWalls = new string[] { "00111100", "11010000", "01100110", "00000000", "01000100", "00000001", "11100010", "01011011" };
                        wWalls = new string[] { "10000110", "10010001", "01010101", "10000001", "11000000", "01010101", "10010000", "01000010" };
                        break;

                    // ADH
                    case 5:
                        labels = new string[] { "D D  *  ", "    H  A", " *H   A ", "A  D    ", "    HD  ", "* H    A", "D       ", "   A H  " };
                        nWalls = new string[] { "01110101", "00001110", "00000111", "01001100", "00110101", "11100000", "01100000", "11001000" };
                        wWalls = new string[] { "10100100", "11110000", "11011000", "01110000", "10000101", "10001110", "00011111", "10000101" };
                        break;

                    // BCD
                    case 6:
                        labels = new string[] { "     B  ", "C D   * ", " * B  C ", " C    B ", "    C  D", "B    D  ", " C  * D ", "D  B    " };
                        nWalls = new string[] { "01011110", "01101011", "01100100", "10000111", "10110011", "10011001", "01100010", "10111001" };
                        wWalls = new string[] { "10010000", "00001010", "11101000", "00001000", "00100010", "00010001", "00000110", "00100001" };
                        break;

                    // BCH
                    case 7:
                        labels = new string[] { "C   H   ", "  C    H", "  * B   ", "B  H*   ", " H   B C", "   *    ", "  B C   ", " C   H B" };
                        nWalls = new string[] { "10110011", "01010111", "10101100", "00000110", "01111110", "00100010", "10010100", "00000110" };
                        wWalls = new string[] { "10000100", "01001000", "01111001", "10001000", "10001000", "01101101", "01101001", "00010010" };
                        break;

                    // BDH
                    case 8:
                        labels = new string[] { "  D B  H", "   *  D ", "  H *  B", "D    B  ", "    D  H", "  B     ", "   H  H*", "D    B  " };
                        nWalls = new string[] { "00100001", "00010001", "11011000", "11011011", "00010011", "10000000", "01101100", "01001111" };
                        wWalls = new string[] { "01101000", "11010111", "00000110", "00100000", "00110100", "10001110", "11000000", "00011010" };
                        break;

                    // CDH
                    default:
                        labels = new string[] { "  H  D  ", "    C*  ", "   H   D", "H    D  ", "  C     ", "C  D C H", "*D  H * ", "       C" };
                        nWalls = new string[] { "01011010", "00100100", "00000000", "01000010", "01000010", "01100110", "01011010", "10100101" };
                        wWalls = new string[] { "11001001", "11110111", "00100010", "00011100", "10011100", "10001000", "11000001", "00101010" };
                        break;
                }

                outputs.Add($@"<div class='tmaze-outer'>{headers?[mapIndex]}{Utils.Create2DMazeSvg(
                    nWalls.Select(row => row.Select(ch => ch == '1').ToArray()).ToArray(),
                    wWalls.Select(row => row.Select(ch => ch == '1').ToArray()).ToArray(),
                    labels.Select(str => str.ToCharArray()).ToArray(),
                    highlightCorridors: cheatSheet)}</div>");
            }
            var path = cheatSheet ? @"D:\c\KTANE\Public\HTML\3D Maze embellished (Timwi).html" : @"D:\c\KTANE\Public\HTML\3D Maze.html";
            var alltext = File.ReadAllText(path);

            alltext = Regex.Replace(alltext, @"(?<=<!-- ##\[## -->).*(?=<!-- ##\]## -->)", @"
<table class='mazes-table'>
    <tr><td>{0}</td><td>{1}</td></tr>
    <tr><td>{2}</td><td>{3}</td></tr>
    <tr><td>{4}</td><td>{5}</td></tr>
</table>
".Fmt(outputs.ToArray()), RegexOptions.Singleline);

            alltext = Regex.Replace(alltext, @"(?<=<!-- ###\[### -->).*(?=<!-- ###\]### -->)", @"
<table class='mazes-table'>
    <tr><td>{6}</td><td>{7}</td></tr>
    <tr><td>{8}</td><td>{9}</td></tr>
</table>
".Fmt(outputs.ToArray()), RegexOptions.Singleline);

            File.WriteAllText(path, alltext);
        }
    }
}
