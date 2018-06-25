using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using RT.KitchenSink;
using RT.Util;
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

            var thrs = new List<Thread>();

            void pngcr(string path1, string path2)
            {
                var thr = new Thread(() =>
                {
                    CommandRunner.Run("pngcr", path1, path2).OutputNothing().Go();
                    File.Delete(path1);
                    Console.WriteLine(path2);
                });
                thr.Start();
                thrs.Add(thr);
            }

            var bhPath1 = @"D:\c\KTANE\BlackHole\Assets\Textures\BlackHole-tmp.png";
            var bhPath2 = @"D:\c\KTANE\BlackHole\Assets\Textures\BlackHole.png";
            GraphicsUtil.DrawBitmap(w, w, g =>
            {
                g.Clear(Color.Transparent);

                const int padding = 50;
                var rect = new RectangleF(padding, padding, w - 2 * padding, w - 2 * padding);
                g.FillEllipse(Brushes.Black, rect);
                g.DrawEllipse(new Pen(Color.FromArgb(128, Color.CornflowerBlue), 12f), rect);
            }).Save(bhPath1);
            pngcr(bhPath1, bhPath2);

            var colorInfos = new(string colorCode, string name)[] { ("e70909ff", "red"), ("ed800cff", "orange"), ("deda16ff", "yellow"), ("17b129ff", "green"), ("10a0a8ff", "cyan"), ("2826ffff", "blue"), ("bb0db0ff", "purple") };

            foreach (var (colorCode, name) in colorInfos)
            {
                var color = Color.FromArgb(128, Convert.ToInt32(colorCode.Substring(0, 2), 16), Convert.ToInt32(colorCode.Substring(2, 2), 16), Convert.ToInt32(colorCode.Substring(4, 2), 16));
                var path1 = $@"D:\c\KTANE\BlackHole\Assets\Textures\Swirl-{name}-tmp.png";
                var path2 = $@"D:\c\KTANE\BlackHole\Assets\Textures\Swirl-{name}.png";
                GraphicsUtil.DrawBitmap(70, 32, g =>
                {
                    g.Clear(Color.Transparent);

                    using (var sb = new SolidBrush(color))
                    {
                        var svgData = @"M 240,612.36218 270,612.36218 C 250,587.36218 225,577.36218 200,587.36218 230,582.36218 240,597.36218 240,612.36218 z";
                        var polygons = DecodeSvgPath.Do(svgData, 1);
                        foreach (var polygon in polygons)
                            g.FillPolygon(sb, polygon.Select(p => (p + new PointD(0, -552.36218) - new PointD(201, 31)).ToPointF()).ToArray());
                    }
                }).Save(path1);
                pngcr(path1, path2);
            }

            using (var bmp = new Bitmap($@"D:\c\KTANE\BlackHole\Data\Symbols-white.png"))
            {
                for (int cIx = 0; cIx < colorInfos.Length; cIx++)
                {
                    var (colorCode, colorName) = colorInfos[cIx];
                    var r = Convert.ToInt32(colorCode.Substring(0, 2), 16);
                    var g = Convert.ToInt32(colorCode.Substring(2, 2), 16);
                    var b = Convert.ToInt32(colorCode.Substring(4, 2), 16);
                    var iw = bmp.Width / 5;
                    var ih = bmp.Height / 3;
                    for (int sym = 0; sym < 12; sym++)
                    {
                        var newBmp = GraphicsUtil.DrawBitmap(iw, ih, gr =>
                        {
                            gr.DrawImage(bmp, -iw * (sym % 5), -ih * (2 - sym / 5));
                        });
                        unsafe
                        {
                            var data = newBmp.LockBits(new Rectangle(0, 0, iw, ih), ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
                            for (int y = 0; y < ih; y++)
                            {
                                var p = (byte*) (data.Scan0 + y * data.Stride);
                                for (int x = 0; x < iw; x++)
                                {
                                    p[4 * x] = (byte) b;
                                    p[4 * x + 1] = (byte) g;
                                    p[4 * x + 2] = (byte) r;
                                    p[4 * x + 3] = (byte) (p[4 * x + 3] >> 1);
                                }
                            }
                            newBmp.UnlockBits(data);
                        }
                        var path1 = $@"D:\c\KTANE\BlackHole\Assets\Textures\Symbol-{sym}-{cIx}-tmp.png";
                        var path2 = $@"D:\c\KTANE\BlackHole\Assets\Textures\Symbol-{sym}-{cIx}.png";
                        newBmp.Save(path1);
                        pngcr(path1, path2);
                    }
                }
            }

            foreach (var thr in thrs)
                thr.Join();
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