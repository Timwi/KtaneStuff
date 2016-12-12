using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using RT.Util;
using RT.Util.ExtensionMethods;

namespace KtaneStuff
{
    static partial class Ktane
    {
        public static void Do3DMazeManual()
        {
            var output1 = new StringBuilder();
            var output2 = new StringBuilder();
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

                (mapIndex >= 6 ? output2 : output1).AppendLine($@"<div class='tmaze-outer'>{create2DMazeSvg(
                    nWalls.Select(row => row.Select(ch => ch == '1').ToArray()).ToArray(),
                    wWalls.Select(row => row.Select(ch => ch == '1').ToArray()).ToArray(),
                    labels.Select(str => str.ToCharArray()).ToArray(),
                    highlightCorridors: true)}</div>");
            }
            var alltext = File.ReadAllText(@"D:\c\KTANE\HTML\3D Maze cheat sheet (Timwi).html");
            alltext = Regex.Replace(alltext, @"(?<=<!-- ##\[## -->).*(?=<!-- ##\]## -->)", Environment.NewLine + output1.ToString(), RegexOptions.Singleline);
            alltext = Regex.Replace(alltext, @"(?<=<!-- ###\[### -->).*(?=<!-- ###\]### -->)", Environment.NewLine + output2.ToString(), RegexOptions.Singleline);
            File.WriteAllText(@"D:\c\KTANE\HTML\3D Maze cheat sheet (Timwi).html", alltext);
        }

        private static string create2DMazeSvg(bool[][] nWalls, bool[][] wWalls, char[][] labels = null, bool highlightCorridors = false, bool frame = false, bool omitAxes = false, string extra = null)
        {
            if (nWalls == null)
                throw new ArgumentNullException(nameof(nWalls));
            if (wWalls == null)
                throw new ArgumentNullException(nameof(wWalls));

            var ySize = nWalls.Length;
            if (wWalls.Length != ySize)
                throw new ArgumentException("wWalls has different length than nWalls.", nameof(wWalls));
            if (ySize == 0 || nWalls[0].Length == 0)
                throw new ArgumentException("Cannot have a zero-size maze.", nameof(nWalls));
            var xSize = nWalls[0].Length;
            if (nWalls.Any(arr => arr.Length != xSize))
                throw new ArgumentException("nWalls lengths are inconsistent.", nameof(nWalls));
            if (wWalls.Any(arr => arr.Length != xSize))
                throw new ArgumentException("wWalls lengths are inconsistent.", nameof(wWalls));

            var n = Ut.Lambda((int x, int y) => frame && y % ySize == 0 ? true : nWalls[(y + ySize) % ySize][(x + xSize) % xSize]);
            var w = Ut.Lambda((int x, int y) => frame && x % xSize == 0 ? true : wWalls[(y + ySize) % ySize][(x + xSize) % xSize]);

            var isCell = Ut.Lambda((int x, int y) => x % 2 != 0 && y % 2 != 0);
            var isOn = Ut.Lambda((int x, int y) => (x % 2 == 0)
                ? (y % 2 == 0)
                    ? (x != 0 && n(x / 2 - 1, y / 2)) || (x != xSize * 2 && n(x / 2, y / 2)) || (y != 0 && w(x / 2, y / 2 - 1)) || (y != ySize * 2 && w(x / 2, y / 2))
                    : w(x / 2, y / 2)
                : (y % 2 == 0)
                    ? n(x / 2, y / 2)
                    : false);

            var getX = Ut.Lambda((int gridX) => (gridX / 2) * 25 + (gridX % 2) * 5);
            var getY = Ut.Lambda((int gridY) => (gridY / 2) * 25 + (gridY % 2) * 5);
            var getCoord = Ut.Lambda((Point p) => $"{getX(p.X)},{getY(p.Y)}");

            var corridorsSvg = Ut.Lambda(() =>
            {
                var lines = new Dictionary<string, List<string>>();
                for (int x = 0; x < 8; x++)
                    for (int y = 0; y < 8; y++)
                        if (w(x, y))
                        {
                            var markings = "";
                            var startX = x;
                            var endX = x;
                            do
                            {
                                markings += labels[y][endX % 8].Apply(c => c == '*' ? ' ' : c);
                                endX++;
                            }
                            while (!w(endX, y));
                            if (markings.Count(ch => ch != ' ') >= 2)
                            {
                                var rect = $"<rect clip-path='url(#mainmaze)' x='{startX * 25 + 8}' y='{y * 25 + 8}' width='{(endX - startX) * 25 - 11}' height='14' rx='5' ry='5' class='{{0}}' />";
                                if (endX > 8)
                                    rect += $"<rect clip-path='url(#mainmaze)' x='{(startX - 8) * 25 + 8}' y='{y * 25 + 8}' width='{(endX - startX) * 25 - 11}' height='14' rx='5' ry='5' class='{{0}}' />";
                                lines.AddSafe(markings, rect);
                            }
                        }
                for (int x = 0; x < 8; x++)
                    for (int y = 0; y < 8; y++)
                        if (n(x, y))
                        {
                            var markings = "";
                            var startY = y;
                            var endY = y;
                            do
                            {
                                markings += labels[endY % 8][x].Apply(c => c == '*' ? ' ' : c);
                                endY++;
                            }
                            while (!n(x, endY));
                            if (markings.Count(ch => ch != ' ') >= 2)
                            {
                                var rect = $"<rect clip-path='url(#mainmaze)' x='{x * 25 + 8}' y='{startY * 25 + 8}' width='14' height='{(endY - startY) * 25 - 11}' rx='5' ry='5' class='{{0}}' />";
                                if (endY > 8)
                                    rect += $"<rect clip-path='url(#mainmaze)' x='{x * 25 + 8}' y='{(startY - 8) * 25 + 8}' width='14' height='{(endY - startY) * 25 - 11}' rx='5' ry='5' class='{{0}}' />";
                                lines.AddSafe(markings, rect);
                            }
                        }

                return lines.SelectMany(kvp => kvp.Value.Select(str => str.Fmt(kvp.Value.Count > 1 || lines.ContainsKey(kvp.Key.Reverse().JoinString()) ? "ambiguous" : "unique"))).JoinString();
            });

            var offset = omitAxes ? 0 : 14;
            return $@"<svg viewBox='{-offset} {-offset} {25 * xSize + 5 + offset} {25 * ySize + 5 + offset}' class='tmaze'>{
                extra +
                // For the “lines” (cheat sheet only) below
                (labels == null || !highlightCorridors ? null : $"<defs><clipPath id='mainmaze'><rect x='0' y='0' width='{25 * xSize + 5}' height='{25 * ySize + 5}' /></clipPath></defs>") +
                // Polygons
                $"<path d='{boolsToPaths(Ut.NewArray(2 * xSize + 1, 2 * ySize + 1, isOn)).Select(poly => poly.Select((p, ix) => (ix == 0 ? "M" : ix == 1 ? "L" : "") + getCoord(p)).JoinString(" ") + "z").JoinString(" ")}' />" +
                // “Lines” (cheat sheet only)
                (labels == null || !highlightCorridors ? null : corridorsSvg()) +
                // Column headers
                (omitAxes ? null : Enumerable.Range(0, xSize).Select(colIx => $"<text x='{colIx * 25 + 15}' y='-4' class='x-label'>{colIx}</text>").JoinString()) +
                // Row headers
                (omitAxes ? null : Enumerable.Range(0, ySize).Select(rowIx => $"<text x='-4' y='{rowIx * 25 + 20}' class='y-label'>{rowIx}</text>").JoinString()) +
                // Text objects
                (labels == null ? null : Enumerable.Range(0, 9)
                    .Select(rowIx => Enumerable.Range(0, 9)
                        .Select(colIx => colIx < 8 && rowIx < 8 && labels[rowIx][colIx] != ' ' ? $"<text class='cell' x='{25 * colIx + 15}' y='{25 * rowIx + (labels[rowIx][colIx] == '*' ? 25 : 22)}'>{labels[rowIx][colIx]}</text>" : null)
                        .JoinString())
                    .JoinString())
            }</svg>";
        }

        private enum Direction { Up, Down, Left, Right }

        /// <summary>Given a <see cref="MoveFinder"/> or <see cref="PushFinder"/>, generates the "outline" of the reachable area.
        /// If there are several disjoint regions, several separate outlines are generated.</summary>
        /// <param name="input">The input <see cref="MoveFinder"/> or <see cref="PushFinder"/> to generate the outline from.</param>
        /// <returns>An array of paths, where each path is an array of points. The co-ordinates of the points are the indexes in the input array.</returns>
        /// <example>
        /// <para>An input array full of booleans set to false generates an empty output array.</para>
        /// <para>An input array full of booleans set to true generates a single output path which describes the complete rectangle.</para>
        /// </example>
        private static Point[][] boolsToPaths(bool[][] input)
        {
            int width = input.Length;
            int height = input[0].Length;

            var results = new List<Point[]>();
            var visitedUpArrow = Ut.NewArray<bool>(width, height);

            for (int i = 0; i < width; i++)
                for (int j = 0; j < height; j++)
                    // every region must have at least one up arrow (left edge)
                    if (!visitedUpArrow[i][j] && input.Get(i, j) && !input.Get(i - 1, j))
                        results.Add(tracePolygon(input, i, j, visitedUpArrow));

            return results.ToArray();
        }

        private static T Get<T>(this T[][] arr, int i, int j)
        {
            return i < 0 || i >= arr.Length || j < 0 || j >= arr[i].Length ? default(T) : arr[i][j];
        }

        private static Point[] tracePolygon(bool[][] input, int i, int j, bool[][] visitedUpArrow)
        {
            var result = new List<Point>();
            var dir = Direction.Up;

            while (true)
            {
                // In each iteration of this loop, we move from the current edge to the next one.
                // We have to prioritise right-turns so that the diagonal-adjacent case is handled correctly.
                // Every time we take a 90° turn, we add the corner coordinate to the result list.
                // When we get back to the original edge, the polygon is complete.
                switch (dir)
                {
                    case Direction.Up:
                        // If we’re back at the beginning, we’re done with this polygon
                        if (visitedUpArrow[i][j])
                            return result.ToArray();

                        visitedUpArrow[i][j] = true;

                        if (!input.Get(i, j - 1))
                        {
                            result.Add(new Point(i, j));
                            dir = Direction.Right;
                        }
                        else if (input.Get(i - 1, j - 1))
                        {
                            result.Add(new Point(i, j));
                            dir = Direction.Left;
                            i--;
                        }
                        else
                            j--;
                        break;

                    case Direction.Down:
                        j++;
                        if (!input.Get(i - 1, j))
                        {
                            result.Add(new Point(i, j));
                            dir = Direction.Left;
                            i--;
                        }
                        else if (input.Get(i, j))
                        {
                            result.Add(new Point(i, j));
                            dir = Direction.Right;
                        }
                        break;

                    case Direction.Left:
                        if (!input.Get(i - 1, j - 1))
                        {
                            result.Add(new Point(i, j));
                            dir = Direction.Up;
                            j--;
                        }
                        else if (input.Get(i - 1, j))
                        {
                            result.Add(new Point(i, j));
                            dir = Direction.Down;
                        }
                        else
                            i--;
                        break;

                    case Direction.Right:
                        i++;
                        if (!input.Get(i, j))
                        {
                            result.Add(new Point(i, j));
                            dir = Direction.Down;
                        }
                        else if (input.Get(i, j - 1))
                        {
                            result.Add(new Point(i, j));
                            dir = Direction.Up;
                            j--;
                        }
                        break;
                }
            }
        }
    }
}
