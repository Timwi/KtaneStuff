using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using KtaneStuff.Modeling;
using RT.Util.Drawing;

namespace KtaneStuff
{
    using RT.Util;
    using static Md;

    static class Mahjong
    {
        public static void DoModels()
        {
            File.WriteAllText(@"D:\c\KTANE\Mahjong\Assets\Models\Tile.obj", GenerateObjFile(Tile(), "Tile"));
        }

        private static IEnumerable<VertexInfo[]> Tile()
        {
            const double w = .8;
            const double h = 1;
            const double b = .05;
            const double d = .7;
            const double colR = 24d / 29;
            var octagon = new[] { p(-w, -h + b), p(-w + b, -h), p(w - b, -h), p(w, -h + b), p(w, h - b), p(w - b, h), p(-w + b, h), p(-w, h - b) };
            var extruded = octagon.Extrude(d, includeBackFace: true, flatSideNormals: true).ToArray();

            foreach (var face in extruded)
                yield return face.All(v => v.Location.Y == 0) || face.All(v => v.Location.Y == d)
                    ? face.Select(v => v.WithTexture((1 - v.Location.X / w) / 2 * colR, (v.Location.Z / h + 1) / 2)).ToArray()
                    : face.Zip(new[] { p(colR, 0), p(colR, 1), p(1, 1), p(1, 0) }, (v, t) => v.WithTexture(t)).ToArray();
        }

        public static void DoGraphics(bool doManualGraphics = false, bool doTextures = false)
        {
            var threads = new List<Thread>();
            foreach (var file in new DirectoryInfo(@"D:\c\KTANE\KtaneStuff\DataFiles\Mahjong").EnumerateFiles("*.png"))
            {
                Console.WriteLine(file.Name);
                using (var bmp = new Bitmap(file.FullName))
                {
                    var w = 80;
                    var h = 100;

                    var name = Path.GetFileNameWithoutExtension(file.Name)
                        .Replace("Flower_", "").Replace("Season_", "").Replace("Wind_", "").Replace("_", " ");
                    name = Regex.Replace(name, @"Dragon (\w+)", m => $"{m.Groups[1].Value} Dragon");

                    var path1 = $@"D:\c\KTANE\Public\HTML\img\Mahjong\{name.Replace(" normal", "")}.png.tmp.png";
                    var path2 = $@"D:\c\KTANE\Mahjong\Assets\Textures\{name}.png.tmp.png";

                    if (doManualGraphics && file.Name.Contains("normal"))
                    {
                        GraphicsUtil.DrawBitmap(w, h, gr =>
                        {
                            gr.Clear(Color.FromArgb(0xF1, 0xE9, 0xAF));
                            gr.DrawImage(GraphicsUtil.MakeSemitransparentImage(w, h,
                                initGraphics: g => { g.SetHighQuality(); },
                                drawOpaqueLayer: g =>
                                {
                                    g.DrawImage(bmp, new Rectangle(0, 0, w, h), new Rectangle(0, 0, 1112, 1390), GraphicsUnit.Pixel);
                                },
                                drawTransparencyLayer: g =>
                                {
                                    g.Clear(Color.Transparent);
                                    g.FillPath(Brushes.Black, GraphicsUtil.RoundedRectangle(0, 0, w - 1, h - 1, 15));
                                }), 0, 0);
                        })
                            .Save(path1);
                    }

                    if (doTextures)
                    {
                        w *= 3;
                        h *= 3;
                        var extraCol = 50;
                        GraphicsUtil.DrawBitmap(w + extraCol, h, gr =>
                        {
                            gr.Clear(file.Name.Contains("normal") ? Color.FromArgb(0xF1, 0xE9, 0xAF) : Color.White);
                            gr.DrawImage(GraphicsUtil.MakeSemitransparentImage(w + extraCol, h,
                                initGraphics: g => { g.SetHighQuality(); },
                                drawOpaqueLayer: g =>
                                {
                                    g.DrawImage(bmp, new Rectangle(0, 0, w, h), new Rectangle(0, 0, 1112, 1390), GraphicsUnit.Pixel);
                                    g.DrawLine(new Pen(new SolidBrush(Color.FromArgb(128, 255, 255, 255)), 10f), 0, 0, w, 0);
                                    g.DrawLine(new Pen(new SolidBrush(Color.FromArgb(128, 255, 255, 255)), 10f), 0, 0, 0, h);
                                    g.DrawLine(new Pen(new SolidBrush(Color.FromArgb(128, 0, 0, 0)), 10f), w, 0, w, h);
                                    g.DrawLine(new Pen(new SolidBrush(Color.FromArgb(128, 0, 0, 0)), 10f), 0, h, w, h);
                                },
                                drawTransparencyLayer: g =>
                                {
                                    g.Clear(Color.Transparent);
                                    g.FillPath(Brushes.Black, GraphicsUtil.RoundedRectangle(0, 0, w - 1, h - 1, 15));
                                }), 0, 0);
                            gr.FillRectangle(new SolidBrush(Color.FromArgb(0x9E, 0x99, 0x76)), w, 0, extraCol / 2, h);
                            gr.FillRectangle(new SolidBrush(Color.FromArgb(0x78, 0x48, 0x20)), w + extraCol / 2, 0, extraCol / 2, h);
                        })
                            .Save(path2);
                    }

                    var thr = new Thread(() =>
                    {
                        if (doManualGraphics && file.Name.Contains("normal"))
                        {
                            CommandRunner.Run("pngcr", path1, path1.Replace(".tmp.png", "")).OutputNothing().Go();
                            File.Delete(path1);
                        }
                        if (doTextures)
                        {
                            CommandRunner.Run("pngcr", path2, path2.Replace(".tmp.png", "")).OutputNothing().Go();
                            File.Delete(path2);
                        }
                    });
                    thr.Start();
                    threads.Add(thr);
                }
            }
            foreach (var thr in threads)
                thr.Join();
        }
    }
}