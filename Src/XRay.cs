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

        public static unsafe void DoManualAndIcons()
        {
            //goto manualCodeOnly;
            const int nx = 11;
            const int ny = 10;
            using (var src = new Bitmap(@"D:\c\KTANE\XRay\Data\Icons.png"))
            {
                var w = src.Width / nx;
                var h = src.Height / ny;
                var threads = new List<Thread>();
                var declarations = new string[nx * ny];
                for (int icY = 0; icY < ny; icY++)
                    for (int icX = 0; icX < nx; icX++)
                    {
                        var code = $"{(char) ('A' + icX)}{icY + 1}";

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
                                    if (p[x * 4 + 3] >= 0x80)
                                    {
                                        numbers[numbers.Count - 1] = numbers[numbers.Count - 1] | (1UL << (x % 64));
                                        pixels[x][y] = true;
                                    }
                                }
                            }
                            declarations[icX + 11 * icY] = $"new ulong[] {{ {numbers.Select(n => n + "UL").JoinString(", ")} }}";

#pragma warning disable 162
                            // Video frames (scanning from top to bottom)
                            if (false)
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
                                var thr = new Thread(() =>
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

                        // Textures for the module
                        if (false)
                        {
                            var modPathTmp = $@"D:\c\KTANE\XRay\Assets\Textures\Icon {code}.png.tmp.png";
                            var modPath = $@"D:\c\KTANE\XRay\Assets\Textures\Icon {code}.png";

                            using (var dest = new Bitmap(w, h, PixelFormat.Format32bppArgb))
                            using (var g = Graphics.FromImage(dest))
                            {
                                g.Clear(Color.Transparent);
                                g.DrawImage(src, -icX * w, -icY * h);
                                dest.Save(modPathTmp);
                            }

                            {
                                var thr = new Thread(() =>
                                {
                                    CommandRunner.Run(@"pngcr", modPathTmp, modPath).Go();
                                    File.Delete(modPathTmp);
                                });
                                thr.Start();
                                threads.Add(thr);
                            }
                        }
#pragma warning restore 162
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

            manualCodeOnly:
            var rnd = new Random(47);
            var icons = Enumerable.Repeat(Enumerable.Range(0, 33), 5).SelectMany(x => x).ToList().Shuffle(rnd);

            Utils.ReplaceInFile(@"D:\c\KTANE\Public\HTML\X-Ray.html", "/*!!*/", "/*!!!*/", Enumerable.Range(0, ny).SelectMany(y => Enumerable.Range(0, nx).Select(x => $".icon.{(char) ('a' + x)}{y + 1} {{ background-position: {-x * .9}cm {-y * .9}cm; }}")).JoinString("\r\n"));

            var convertData = "a1,a1f,b1,b1f,c1,c1f,d1,d1f,e1,e1f,h2f,h2,d7,j1,h6,g1,a6,a2,k2,h1,a7,e2,d6,b3,a10,b10,c10,d10,e10,f10,i10,h9,i9".Split(',');
            string convert(int icon)
            {
                var data = convertData[icon];
                var flip = false;
                if (data.EndsWith("f"))
                {
                    flip = true;
                    data = data.Substring(0, data.Length - 1);
                }
                return "icon " + data + (flip ? " flipped" : "");
            }

            Utils.ReplaceInFile(@"D:\c\KTANE\Public\HTML\X-Ray.html", "<!--%%-->", "<!--%%%-->", new TABLE { class_ = "xray-table xray-table-1" }._(
                Enumerable.Range(0, 3).Select(row => new TR(Enumerable.Range(0, 3).Select(col => new TD(new DIV { class_ = convert(col + 3 * row + 24) }))))).ToString());

            Utils.ReplaceInFile(@"D:\c\KTANE\Public\HTML\X-Ray.html", "<!--##-->", "<!--###-->", new TABLE { class_ = "xray-table xray-table-2" }._(
                new TR(new TD { class_ = "corner" }, Enumerable.Range(0, 12).Select(col => new TH(new DIV { class_ = convert(col) }))),
                Enumerable.Range(0, 12).Select(row => new TR(new TH(new DIV { class_ = convert(row + 12) }), icons.Skip(12 * row).Take(12).Select(ix => new TD(new DIV { class_ = convert(ix) }))))).ToString());

            Clipboard.SetText($@"private static int[] _seed1Table = {{ {icons.Take(12 * 12).JoinString(", ")} }};");
        }
    }
}