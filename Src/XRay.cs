using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading;
using KtaneStuff.Modeling;
using RT.Util;
using RT.Util.ExtensionMethods;

namespace KtaneStuff
{
    using static Md;

    static class XRay
    {
        public static void DoModels()
        {
        }

        public unsafe static void SplitImage()
        {
            using (var src = new Bitmap(@"D:\c\KTANE\XRay\Data\Icons.png"))
            {
                var w = (src.Width / 12);
                var h = (src.Height / 3);
                var threads = new List<Thread>();
                var declarations = new string[33];
                for (int y = 0; y < 3; y++)
                    for (int x = 0; x < (y == 2 ? 9 : 12); x++)
                    {
                        // For the manual
                        var manPath1 = $@"D:\c\KTANE\XRay\Manual\img\X-Ray\Icon {"ABC"[y]}{x + 1}_.png";
                        var manPath2 = $@"D:\c\KTANE\XRay\Manual\img\X-Ray\Icon {"ABC"[y]}{x + 1}.png";
                        var manPath3 = $@"D:\c\KTANE\Public\HTML\img\X-Ray\Icon {"ABC"[y]}{x + 1}.png";

                        if (!File.Exists(manPath2) || !File.Exists(manPath3))
                        {
                            if (!File.Exists(manPath1))
                            {
                                using (var dest = new Bitmap(w, h, PixelFormat.Format32bppArgb))
                                using (var g = Graphics.FromImage(dest))
                                {
                                    g.Clear(Color.Transparent);
                                    g.DrawImage(src, -x * w, -y * h);
                                    dest.Save(manPath1);
                                }
                            }

                            var thr = new Thread(() =>
                            {
                                CommandRunner.Run(@"pngcr", manPath1, manPath2).Go();
                                File.Delete(manPath1);
                                File.Copy(manPath2, manPath3, overwrite: true);
                            });
                            thr.Start();
                            threads.Add(thr);
                        }

                        // For the module
                        var bmpData = src.LockBits(new Rectangle(x * w, y * h, w, h), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
                        try
                        {
                            var numbers = new List<ulong>();
                            for (int yy = 0; yy < h; yy++)
                            {
                                var p = (byte*) (bmpData.Scan0 + bmpData.Stride * yy);
                                for (int xx = 0; xx < w; xx++)
                                {
                                    if (xx % 64 == 0)
                                        numbers.Add(0);
                                    if (p[xx * 4] < 0x80)
                                        numbers[numbers.Count - 1] = numbers[numbers.Count - 1] | (1UL << (xx % 64));
                                }
                            }
                            declarations[12 * y + x] = $"new ulong[] {{ {numbers.Select(n => n + "UL").JoinString(", ")} }}";
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
            {declarations.JoinString(",\n            ")}
        }};
    }}
}}");
            }
        }
    }
}