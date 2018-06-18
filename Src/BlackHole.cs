using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using RT.KitchenSink;
using RT.Util.Drawing;
using RT.Util.ExtensionMethods;
using RT.Util.Geometry;

namespace KtaneStuff
{
    static class BlackHole
    {
        public static void CreateGraphics()
        {
            const int w = 500;

            GraphicsUtil.DrawBitmap(w, w, g =>
            {
                g.Clear(Color.Transparent);

                const int padding = 50;
                var rect = new RectangleF(padding, padding, w - 2 * padding, w - 2 * padding);
                g.FillEllipse(Brushes.Black, rect);
                g.DrawEllipse(new Pen(Color.FromArgb(128, Color.CornflowerBlue), 12f), rect);
            }).Save(@"D:\c\KTANE\BlackHole\Assets\Textures\BlackHole.png");

            foreach (var color in new(Color color, string name)[] { (Color.Red, "red"), (Color.Orange, "orange"), (Color.Yellow, "yellow"), (Color.Green, "green"), (Color.Cyan, "cyan"), (Color.Blue, "blue"), (Color.Purple, "purple") })
            {
                GraphicsUtil.DrawBitmap(w, w, g =>
                {
                    g.Clear(Color.Transparent);

                    var svgData = @"M 240,612.36218 270,612.36218 C 250,587.36218 225,577.36218 200,587.36218 230,582.36218 240,597.36218 240,612.36218 z";
                    var polygons = DecodeSvgPath.Do(svgData, 1);
                    foreach (var polygon in polygons)
                        g.FillPolygon(new SolidBrush(Color.FromArgb(128, color.color)), polygon.Select(p => (p + new PointD(0, -552.36218)).ToPointF()).ToArray());

                }).Save($@"D:\c\KTANE\BlackHole\Assets\Textures\Swirl-{color.name}.png");
            }
        }

        internal static void Analyze()
        {
            int[][] grid = new[] { 3, 9, 1, 10, 2, 8, 6, 7, 5, 4, 6, 8, 5, 7, 4, 1, 2, 3, 9, 10, 8, 7, 4, 2, 1, 3, 5, 10, 6, 9, 9, 5, 10, 6, 3, 4, 7, 2, 1, 8, 1, 2, 6, 3, 5, 10, 9, 8, 4, 7, 4, 10, 7, 8, 9, 6, 3, 5, 2, 1, 7, 6, 3, 1, 8, 5, 4, 9, 10, 2, 2, 4, 9, 5, 10, 7, 1, 6, 8, 3, 5, 1, 8, 9, 7, 2, 10, 4, 3, 6, 10, 3, 2, 4, 6, 9, 8, 1, 7, 5 }
                .Split(10).Select(gr => gr.Select(i => i % 5).ToArray()).ToArray();

            for (int row = 0; row < 10; row++)
                Console.WriteLine($"Row {row} = {Enumerable.Range(0, 5).Select(i => $"{i}={Enumerable.Range(0, 10).Count(col => grid[row][col] == i)}").JoinString(", ")}");

            Console.WriteLine();
            for (int col = 0; col < 10; col++)
                Console.WriteLine($"Col {col} = {Enumerable.Range(0, 5).Select(i => $"{i}={Enumerable.Range(0, 10).Count(row => grid[row][col] == i)}").JoinString(", ")}");

            for (int row = 0; row < 10; row++)
                Console.WriteLine($"{Enumerable.Range(0, 10).Select(col => grid[row][col]).JoinString(" ")}");
            Console.WriteLine();
            Clipboard.SetText(grid.Select(g => $"<tr>{g.Select(i => $"<td>{i}</td>").JoinString()}</tr>").JoinString("\n"));
        }
    }
}