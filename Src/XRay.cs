using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using KtaneStuff.Modeling;
using RT.TagSoup;
using RT.Util;
using RT.Util.ExtensionMethods;

namespace KtaneStuff
{
    using static Md;

    static class XRay
    {
        public static void DoModels()
        {
            File.WriteAllText(@"D:\c\KTANE\XRay\Assets\Models\Button.obj", GenerateObjFile(SymbolButton(), "Button"));
            File.WriteAllText(@"D:\c\KTANE\XRay\Assets\Models\ButtonHighlight.obj", GenerateObjFile(Disc(numVertices: 32, reverse: true), "ButtonHighlight"));
        }

        private static IEnumerable<VertexInfo[]> SymbolButton()
        {
            var f1 = .06;
            var f2 = .03;
            var h = .04;
            var steps = 36;
            return CreateMesh(false, true,
                new BevelPoint(.00, .00, normal: pt(0, -1, 0))
                    .Concat(Bézier(p(.2, 0), p(.2, f1), p(.2 - h + f2, h), p(.2 - h, h), 14).Select((p, first, last) => new BevelPoint(p.X, p.Y, first || last ? Normal.Mine : Normal.Average, first || last ? Normal.Mine : Normal.Average)))
                    .Concat(new BevelPoint(0, h, normal: pt(0, 1, 0)))
                    .Select(bi => Enumerable.Range(0, steps).Select(i => 360d * i / steps)
                        .Select(angle => bi.NormalOverride == null
                            ? pt(bi.X * cos(angle), bi.Y, bi.X * sin(angle), bi.Before, bi.After, Normal.Average, Normal.Average)
                            : pt(bi.X * cos(angle), bi.Y, bi.X * sin(angle), bi.NormalOverride.Value))
                        .ToArray())
                    .ToArray());
        }

        public static void AssignTextures()
        {
            var text = File.ReadAllLines(@"D:\c\KTANE\XRay\Assets\XRayModule.prefab");
            var indexes = text.SelectIndexWhere(l => l == "  ButtonLabels:").ToArray();
            if (indexes.Length != 1)
                System.Diagnostics.Debugger.Break();
            var index = indexes[0] + 1;

            for (int i = 0; i < 33; i++)
            {
                var yaml = File.ReadLines($@"D:\c\KTANE\XRay\Assets\Textures\Icon {"ABC"[i / 12]}{(i % 12) + 1}.png.meta").First(l => l.StartsWith("guid: "));
                text[i + index] = $"  - {{fileID: 2800000, guid: {yaml.Substring("guid: ".Length)}, type: 3}}";
            }

            File.WriteAllLines(@"D:\c\KTANE\XRay\Assets\XRayModule.prefab", text);
        }

        public unsafe static void DoManualAndIcons()
        {
            using (var src = new Bitmap(@"D:\c\KTANE\XRay\Data\Icons.png"))
            {
                var w = (src.Width / 12);
                var h = (src.Height / 3);
                var threads = new List<Thread>();
                var declarations = new string[33];
                for (int icY = 0; icY < 3; icY++)
                    for (int icX = 0; icX < (icY == 2 ? 9 : 12); icX++)
                    {
                        var code = $"{"ABC"[icY]}{icX + 1}";

                        // For the manual
                        var manPath1 = $@"D:\c\KTANE\XRay\Manual\img\X-Ray\Icon {code}_.png";
                        var manPath2 = $@"D:\c\KTANE\XRay\Manual\img\X-Ray\Icon {code}.png";
                        var manPath3 = $@"D:\c\KTANE\Public\HTML\img\X-Ray\Icon {code}.png";

                        // Textures for the module
                        var modPath = $@"D:\c\KTANE\XRay\Assets\Textures\Icon {code}.png";

                        using (var dest = new Bitmap(w, h, PixelFormat.Format32bppArgb))
                        using (var g = Graphics.FromImage(dest))
                        {
                            g.Clear(Color.Transparent);
                            g.DrawImage(src, -icX * w, -icY * h);
                            dest.Save(manPath1);
                        }

                        {
                            var thr = new Thread(() =>
                            {
                                CommandRunner.Run(@"pngcr", manPath1, manPath2).Go();
                                File.Delete(manPath1);
                                File.Copy(manPath2, manPath3, overwrite: true);
                                File.Copy(manPath2, modPath, overwrite: true);
                            });
                            thr.Start();
                            threads.Add(thr);
                        }

                        // C# declaration of the bitwise form of the icons
                        var bmpData = src.LockBits(new Rectangle(icX * w, icY * h, w, h), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
                        try
                        {
                            var numbers = new List<ulong>();
                            var pixels = Ut.NewArray<bool>(w, h);
                            for (int y = 0; y < h; y++)
                            {
                                var p = (byte*) (bmpData.Scan0 + bmpData.Stride * y);
                                for (int x = 0; x < w; x++)
                                {
                                    if (x % 64 == 0)
                                        numbers.Add(0);
                                    if (p[x * 4] < 0x80)
                                    {
                                        numbers[numbers.Count - 1] = numbers[numbers.Count - 1] | (1UL << (x % 64));
                                        pixels[x][y] = true;
                                    }
                                }
                            }
                            declarations[12 * icY + icX] = $"new ulong[] {{ {numbers.Select(n => n + "UL").JoinString(", ")} }}";

                            // Video frames (scanning from top to bottom)
                            if (true)
                            {
                                var thr = new Thread(() =>
                                {
                                    var path = @"D:\temp\XRayVideos";
                                    for (int frame = 0; frame < h + 160; frame++)
                                    {
                                        //var backgroundColor = Color.FromArgb(0x11, 0x1D, 0x2C);
                                        using (var newBmp = new Bitmap(Path.Combine(path, "Background up.png")))
                                        {
                                            for (int y = 0; y < 4; y++)
                                                for (int x = 0; x < w; x++)
                                                    if (frame < h && pixels[x][frame])
                                                        newBmp.SetPixel(x + 90, 373 + y, Color.Lime);
                                            newBmp.Save(Path.Combine(path, $"Icon {code} up bare Frame {frame}.png"));
                                            //for (int y = 0; y < frame; y++)
                                            //    for (int x = 0; x < w; x++)
                                            //        if (y < h && pixels[x][y])
                                            //            newBmp.SetPixel(x + 90, 373 - (frame - y), Color.Lime);
                                            //newBmp.Save(Path.Combine(path, $"Icon {code} up Frame {frame}.png"));
                                        }
                                    }
                                });
                                thr.Start();
                                threads.Add(thr);
                            }

                            // Video frames (scanning from bottom to top)
                            if (false)
                            {
#pragma warning disable 162
                                var thr = new Thread(() =>
#pragma warning restore 162
                                {
                                    var path = @"D:\temp\XRayVideos";
                                    for (int frame = 0; frame < h + 160; frame++)
                                    {
                                        //var backgroundColor = Color.FromArgb(0x11, 0x1D, 0x2C);
                                        using (var newBmp = new Bitmap(Path.Combine(path, "Background down.png")))
                                        {
                                            for (int y = 0; y < 4; y++)
                                                for (int x = 0; x < w; x++)
                                                    if (frame < h && pixels[x][h - 1 - frame])
                                                        newBmp.SetPixel(x + 90, 176 - y, Color.Lime);
                                            newBmp.Save(Path.Combine(path, $"Icon {code} down bare Frame {frame}.png"));
                                            //for (int y = 0; y < frame; y++)
                                            //    for (int x = 0; x < w; x++)
                                            //        if (y < h && pixels[x][h - 1 - y])
                                            //            newBmp.SetPixel(x + 90, 176 + (frame - y), Color.Lime);
                                            //newBmp.Save(Path.Combine(path, $"Icon {code} down Frame {frame}.png"));
                                        }
                                    }
                                });
                                thr.Start();
                                threads.Add(thr);
                            }
                        }
                        finally
                        {
                            src.UnlockBits(bmpData);
                        }
                    }

                foreach (var thr in threads)
                    thr.Join();

                File.WriteAllText(@"D:\c\KTANE\XRay\Assets\Data.cs", $@"namespace XRay {{
    static class RawBits {{
        public static ulong[][] Icons = new[] {{
            {declarations.JoinString(",\r\n            ")}
        }};
    }}
}}");
            }

            string iconIxToPath(int i) => i < 12 ? $"img/X-Ray/Icon A{i + 1}.png" : i < 24 ? $"img/X-Ray/Icon B{i - 12 + 1}.png" : $"img/X-Ray/Icon C{i - 24 + 1}.png";
            const int width = 38;

            var rnd = new Random(47);
            var icons = Enumerable.Repeat(Enumerable.Range(0, 33), 5).SelectMany(x => x).ToList().Shuffle(rnd);

            Utils.ReplaceInFile(@"D:\c\KTANE\Public\HTML\X-Ray.html", "<!--%%-->", "<!--%%%-->", new TABLE { class_ = "xray-table xray-table-1" }._(
                Enumerable.Range(0, 3).Select(row => new TR(Enumerable.Range(0, 3).Select(col => new TD(new IMG { src = $"img/X-Ray/Icon C{row * 3 + col + 1}.png", width = width }))))).ToString());

            Utils.ReplaceInFile(@"D:\c\KTANE\Public\HTML\X-Ray.html", "<!--##-->", "<!--###-->", new TABLE { class_ = "xray-table xray-table-2" }._(
                new TR(new TD { class_ = "corner" }, Enumerable.Range(0, 12).Select(col => new TH(new IMG { src = $"img/X-Ray/Icon A{col + 1}.png", width = width }))),
                Enumerable.Range(0, 12).Select(row => new TR(new TH(new IMG { src = $"img/X-Ray/Icon B{row + 1}.png", width = width }), icons.Skip(12 * row).Take(12).Select(ix => new TD(new IMG { src = iconIxToPath(ix), width = width }))))).ToString());

            Clipboard.SetText($@"private static int[] _table = {{ {icons.Take(12 * 12).JoinString(", ")} }};");
        }
    }
}