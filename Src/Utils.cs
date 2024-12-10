using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Text.RegularExpressions;
using RT.KitchenSink;
using RT.TagSoup;
using RT.Util;
using RT.Util.ExtensionMethods;
using RT.Util.Geometry;

namespace KtaneStuff
{
    static class Utils
    {
        public static HTML CreateManualPage(string moduleName, object[] pages, string onTheSubjectOf = null, string css = null, bool omitDiagram = false)
        {
            return new HTML(
                new HEAD(
                    new TITLE(moduleName, " — Keep Talking and Nobody Explodes Mod"),
                    new META { httpEquiv = "Content-Type", content = "text/html; charset=UTF-8" },
                    new META { name = "viewport", content = "initial-scale=1" },
                    new LINK { rel = "stylesheet", type = "text/css", href = "css/normalize.css" },
                    new LINK { rel = "stylesheet", type = "text/css", href = "css/main.css" },
                    new LINK { rel = "stylesheet", type = "text/css", href = "css/font.css" },
                    new SCRIPT { src = "js/highlighter.js" },
                    css.NullOr(c => new STYLELiteral(c))),
                new BODY(
                    pages.Select((page, pageIx) => new DIV { class_ = "section" }._(
                        new DIV { class_ = $"page page-bg-0{Rnd.Next(1, 8)}" }._(
                            new DIV { class_ = "page-header" }._(
                                new SPAN { class_ = "page-header-doc-title" }._("Keep Talking and Nobody Explodes Mod"),
                                new SPAN { class_ = "page-header-section-title" }._(moduleName)),
                            new DIV { class_ = "page-content" }._(
                                pageIx > 0 ? null : Ut.NewArray<object>(
                                    omitDiagram ? null : new IMG { src = $"img/Component/{moduleName}.svg", class_ = "diagram" },
                                    new H2($"On the Subject of {onTheSubjectOf ?? moduleName}")),
                                page),
                            new DIV { class_ = "page-footer relative-footer" }._($"Page {pageIx + 1} of {pages.Length}"))))));
        }

        public static string Create2DMazeSvg(bool[][] nWalls, bool[][] wWalls, char[][] labels = null, bool highlightCorridors = false, bool frame = false, bool omitAxes = false, string extra = null)
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
            return $@"<svg viewBox='{-offset} {-offset} {25 * xSize + 5 + offset} {25 * ySize + 5 + offset}' class='tmaze'>{extra +
                // For the “lines” (cheat sheet only) below
                (labels == null || !highlightCorridors ? null : $"<defs><clipPath id='mainmaze'><rect x='0' y='0' width='{25 * xSize + 5}' height='{25 * ySize + 5}' /></clipPath></defs>") +
                // Polygons
                $"<path d='{BoolsToPaths(Ut.NewArray(2 * xSize + 1, 2 * ySize + 1, isOn)).Select(poly => poly.Select((p, ix) => (ix == 0 ? "M" : ix == 1 ? "L" : "") + getCoord(p)).JoinString(" ") + "z").JoinString(" ")}' />" +
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
                    .JoinString())}</svg>";
        }

        private enum Direction { Up, Down, Left, Right }

        /// <summary>
        ///     Generates the "outline" of an area given by a two-dimensional boolean array. If there are several disjoint
        ///     regions, several separate outlines are generated.</summary>
        /// <param name="input">
        ///     The input array to generate the outline from.</param>
        /// <returns>
        ///     An array of paths, where each path is an array of points. The co-ordinates of the points are the indexes in
        ///     the input array.</returns>
        /// <example>
        ///     <para>
        ///         An input array full of booleans set to false generates an empty output array.</para>
        ///     <para>
        ///         An input array full of booleans set to true generates a single output path which describes the complete
        ///         rectangle.</para></example>
        public static Point[][] BoolsToPaths(bool[][] input)
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

        public static string ToBinary(this BigInteger num, int sepAt)
        {
            if (num.IsZero)
                return "0";
            var str = new StringBuilder();
            var ix = 0;
            while (!num.IsZero)
            {
                str.Append((char) ('0' + (int) (num & 1)));
                num >>= 1;
                ix++;
                if (ix % sepAt == 0)
                    str.Append('|');
            }
            return str.ToString();
        }

        public static string Reverse(this string str)
        {
            if (str.Length == 0)
                return str;
            var arr = new char[str.Length];
            for (int i = 0; i < str.Length; i++)
                arr[str.Length - 1 - i] = str[i];
            return new string(arr);
        }

        public static void ReplaceInFile(this string path, string startMarker, string endMarker, string newText)
        {
            File.WriteAllText(path, Regex.Replace(File.ReadAllText(path),
                $@"(?<={Regex.Escape(startMarker)})(\r?\n)*( *).*?(\r?\n *)?(?={Regex.Escape(endMarker)})",
                m => m.Groups[1].Value + newText.Unindent().Indent(m.Groups[2].Length) + m.Groups[3].Value,
                RegexOptions.Singleline));
        }

        public static int IncSafe<K1, K2>(this IDictionary<K1, Dictionary<K2, int>> dic, K1 key1, K2 key2, int amount = 1)
        {
            if (dic == null)
                throw new ArgumentNullException("dic");
            if (key1 == null)
                throw new ArgumentNullException(nameof(key1), "Null values cannot be used for keys in dictionaries.");
            if (key2 == null)
                throw new ArgumentNullException(nameof(key2), "Null values cannot be used for keys in dictionaries.");
            if (!dic.ContainsKey(key1))
                dic[key1] = new Dictionary<K2, int>();
            if (!dic[key1].ContainsKey(key2))
                return (dic[key1][key2] = amount);
            else
                return (dic[key1][key2] = dic[key1][key2] + amount);
        }

        public static IEnumerable<DecodeSvgPath.PathPiece> FontToSvgPath(string text, string fontFamily, double emSize)
        {
            var gp = new GraphicsPath();
            gp.AddString(text, new FontFamily(fontFamily), 0, (float) emSize, new PointF(0, 0), new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center });

            var points = gp.PathPoints;
            var types = gp.PathTypes.Select(b => (PathPointType) b).ToArray();

            var path = new List<DecodeSvgPath.PathPiece>();
            for (int j = 0; j < gp.PointCount; j++)
            {
                var type =
                    types[j].HasFlag(PathPointType.Bezier) ? DecodeSvgPath.PathPieceType.Curve :
                    types[j].HasFlag(PathPointType.Line) ? DecodeSvgPath.PathPieceType.Line : DecodeSvgPath.PathPieceType.Move;
                if (type == DecodeSvgPath.PathPieceType.Curve)
                {
                    yield return new DecodeSvgPath.PathPiece(DecodeSvgPath.PathPieceType.Curve, points.Subarray(j, 3).Select(p => new PointD(p)).ToArray());
                    j += 2;
                }
                else
                    yield return new DecodeSvgPath.PathPiece(type, points.Subarray(j, 1).Select(p => new PointD(p)).ToArray());

                if (types[j].HasFlag(PathPointType.CloseSubpath))
                    yield return DecodeSvgPath.PathPiece.End;
            }
        }

        public static void SvgToPng(string outputFile, string svg, int width)
        {
            var path = Path.GetTempFileName();
            File.WriteAllText(path, svg);
            var cmd = $@"D:\Inkscape\bin\inkscape.exe --export-type=""png"" ""--export-filename={outputFile}"" --export-width={width} ""{path}""";
            CommandRunner.RunRaw(cmd).Go();
            File.Delete(path);
        }

        /// <summary>
        ///     Brings the elements of the given list into a random order.</summary>
        /// <typeparam name="T">
        ///     Type of the list.</typeparam>
        /// <param name="list">
        ///     List to shuffle.</param>
        /// <param name="rnd">
        ///     Random number generator.</param>
        /// <returns>
        ///     The list operated on.</returns>
        public static T Shuffle<T>(this T list, MonoRandom rnd = null) where T : IList
        {
            if (list == null)
                throw new ArgumentNullException(nameof(list));
            for (int j = list.Count; j >= 1; j--)
            {
                int item = rnd == null ? Rnd.Next(0, j) : rnd.Next(0, j);
                if (item < j - 1)
                    (list[j - 1], list[item]) = (list[item], list[j - 1]);
            }
            return list;
        }
    }
}
