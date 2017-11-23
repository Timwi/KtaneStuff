using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using KtaneStuff.Modeling;
using RT.Util;
using RT.Util.Drawing;
using RT.Util.ExtensionMethods;

namespace KtaneStuff
{
    using static Md;

    static class Gridlock
    {
        public static void DoModels()
        {
            File.WriteAllText(@"D:\c\KTANE\Gridlock\Assets\Models\SquareCorner.obj", GenerateObjFile(SquareCorner(), "SquareCorner"));
            File.WriteAllText(@"D:\c\KTANE\Gridlock\Assets\Models\Square.obj", GenerateObjFile(Square(), "Square"));
            File.WriteAllText(@"D:\c\KTANE\Gridlock\Assets\Models\Screen.obj", GenerateObjFile(Screen(), "Screen"));
            File.WriteAllText(@"D:\c\KTANE\Gridlock\Assets\Models\Button.obj", GenerateObjFile(Button(), "Button"));

            var trs = new List<Thread>();
            var files = new byte[12][];
            using (var bmp = new Bitmap(@"D:\c\KTANE\Gridlock\Sources\Icons.png"))
                for (int i = 0; i < 12; i++)
                {
                    var j = i;
                    var path1 = $@"D:\Temp\Gridlock-Icon{i}a.png";
                    var path2 = $@"D:\c\KTANE\Gridlock\Assets\Images\Icon{i}.png";
                    GraphicsUtil.DrawBitmap(bmp.Width / 4, bmp.Height / 3, g =>
                    {
                        g.Clear(Color.Transparent);
                        g.DrawImage(bmp, (-bmp.Width / 4) * (i % 4), (-bmp.Height / 3) * (i / 4));
                    }).Save(path1);
                    trs.Add(new Thread(() =>
                    {
                        CommandRunner.Run(@"pngcr", path1, path2).Go();
                        files[j] = File.ReadAllBytes(path2);
                        File.Delete(path1);
                    }));
                }
            foreach (var tr in trs)
                tr.Start();
            foreach (var tr in trs)
                tr.Join();

            //// For pasting into MeshEdit
            //void jsonIntoClipboard(IEnumerable<VertexInfo[]> stuff) => Clipboard.SetText(stuff.Select(face => new JsonDict { { "Hidden", false }, { "Vertices", ClassifyJson.Serialize(face) } }).ToJsonList().ToStringIndented());
            //jsonIntoClipboard(Grid());
            //jsonIntoClipboard(ButtonHole());
            //jsonIntoClipboard(PageNumHole());
        }

        private static IEnumerable<VertexInfo[]> SquareCorner()
        {
            yield return Ut.NewArray(
                new VertexInfo(pt(.75, 0, 1), pt(0, 1, 0)),
                new VertexInfo(pt(1, 0, .75), pt(0, 1, 0)),
                new VertexInfo(pt(1, 0, -1), pt(0, 1, 0)),
                new VertexInfo(pt(-1, 0, -1), pt(0, 1, 0)),
                new VertexInfo(pt(-1, 0, 1), pt(0, 1, 0))
            );
        }

        private static IEnumerable<VertexInfo[]> Square()
        {
            yield return Ut.NewArray(
                new VertexInfo(pt(1, 0, 1), pt(0, 1, 0)),
                new VertexInfo(pt(1, 0, -1), pt(0, 1, 0)),
                new VertexInfo(pt(-1, 0, -1), pt(0, 1, 0)),
                new VertexInfo(pt(-1, 0, 1), pt(0, 1, 0))
            );
        }

        private static IEnumerable<VertexInfo[]> Screen()
        {
            yield return Ut.NewArray(
                new VertexInfo(pt(.75, 0, 3), pt(0, 1, 0)),
                new VertexInfo(pt(1, 0, 2.75), pt(0, 1, 0)),
                new VertexInfo(pt(1, 0, -.75), pt(0, 1, 0)),
                new VertexInfo(pt(.75, 0, -1), pt(0, 1, 0)),
                new VertexInfo(pt(-.75, 0, -1), pt(0, 1, 0)),
                new VertexInfo(pt(-1, 0, -.75), pt(0, 1, 0)),
                new VertexInfo(pt(-1, 0, 2.75), pt(0, 1, 0)),
                new VertexInfo(pt(-.75, 0, 3), pt(0, 1, 0))
            );
        }

        private static IEnumerable<VertexInfo[]> Grid()
        {
            const double yHeight = .025;

            VertexInfo transmogrify(VertexInfo v)
            {
                var x = -v.Location.X * 0.135 - .66;
                var z = -v.Location.Z * 0.135 - .66;

                return pt(x, v.Location.Y + .15 - yHeight, z).WithNormal(v.Normal.Value).WithTexture(.4771284794 * x + .46155, -.4771284794 * z + .5337373145);
            }

            for (int x = 0; x < 4; x++)
                for (int y = 0; y < 4; y++)
                    for (int r = 0; r < 4; r++)
                        if (!(x == 0 && y % 3 == 0 && r == 3) && !(x == 3 && y % 3 == 0 && r == 1) && !(y == 0 && x % 3 == 0 && r == 0) && !(y == 3 && x % 3 == 0 && r == 2))
                            yield return new[] { pt(-1.1, yHeight, 1.1).WithNormal(0, 1, 0), pt(1.1, yHeight, 1.1).WithNormal(0, 1, 0), pt(1, 0, 1).WithNormal(0, 1, 1), pt(-1, 0, 1).WithNormal(0, 1, 1) }
                                .Select(v => transmogrify(v.RotateY(90 * r).Move(x: -2.25 * x, z: -2.25 * y)))
                                .ToArray();

            for (int r = 0; r < 4; r++)
            {
                foreach (var face in Ut.NewArray(
                    new[] { pt(.75, 0, 1).WithNormal(0, 1, 1), pt(-1, 0, 1).WithNormal(0, 1, 1), pt(-1.1, yHeight, 1.1).WithNormal(0, 1, 0), pt(.75 + .1 * tan(22.5), yHeight, 1.1).WithNormal(0, 1, 0) },
                    new[] { pt(1.1, yHeight, -1.1).WithNormal(0, 1, 0), pt(1, 0, -1).WithNormal(1, 1, 0), pt(1, 0, .75).WithNormal(1, 1, 0), pt(1.1, yHeight, .75 + .1 * tan(22.5)).WithNormal(0, 1, 0) },
                    new[] { pt(.75 + .1 * tan(22.5), yHeight, 1.1).WithNormal(0, 1, 0), pt(1.1, yHeight, .75 + .1 * tan(22.5)).WithNormal(0, 1, 0), pt(1, 0, .75).WithNormal(1, 1, 1), pt(.75, 0, 1).WithNormal(1, 1, 1) })
                    .Select(f => f.Select(v => transmogrify(v.RotateY(90 * r).Move(x: r % 3 == 0 ? 0 : -2.25 * 3, z: r < 2 ? 0 : -2.25 * 3))).ToArray()))
                    yield return face;
            }
        }

        private static IEnumerable<VertexInfo[]> hole(double lx, double ty, double rx, double by)
        {
            const double yHeight = .025;

            VertexInfo transmogrify(VertexInfo v)
            {
                var x = v.Location.X * 0.135 - .66;
                var z = -v.Location.Z * 0.135 + .66;

                return pt(x, v.Location.Y + .15 - yHeight, z).WithNormal(v.Normal.Value).WithTexture(.4771284794 * x + .46155, -.4771284794 * z + .5337373145);
            }

            var a = pt(lx + .25 - .1 * tan(22.5), yHeight, ty - .1).WithNormal(0, 1, 0);
            var b = pt(rx - .25 + .1 * tan(22.5), yHeight, ty - .1).WithNormal(0, 1, 0);
            var c = pt(lx + .25, 0, ty).WithNormal(0, 1, -1);
            var d = pt(rx - .25, 0, ty).WithNormal(0, 1, -1);
            var e = pt(lx - .1, yHeight, ty + .25 - .1 * tan(22.5)).WithNormal(0, 1, 0);
            var f = pt(rx + .1, yHeight, ty + .25 - .1 * tan(22.5)).WithNormal(0, 1, 0);
            var g = pt(lx, 0, ty + .25).WithNormal(1, 1, 0);
            var h = pt(rx, 0, ty + .25).WithNormal(-1, 1, 0);
            var i = pt(lx, 0, by - .25).WithNormal(1, 1, 0);
            var j = pt(rx, 0, by - .25).WithNormal(-1, 1, 0);
            var k = pt(lx - .1, yHeight, by - .25 + .1 * tan(22.5)).WithNormal(0, 1, 0);
            var l = pt(rx + .1, yHeight, by - .25 + .1 * tan(22.5)).WithNormal(0, 1, 0);
            var m = pt(lx + .25, 0, by).WithNormal(0, 1, 1);
            var n = pt(rx - .25, 0, by).WithNormal(0, 1, 1);
            var o = pt(lx + .25 - .1 * tan(22.5), yHeight, by + .1).WithNormal(0, 1, 0);
            var p = pt(rx - .25 + .1 * tan(22.5), yHeight, by + .1).WithNormal(0, 1, 0);

            foreach (var face in Ut.NewArray(
                new[] { c, d, b, a },
                new[] { d, h, f, b },
                new[] { h, j, l, f },
                new[] { p, l, j, n },
                new[] { o, p, n, m },
                new[] { o, m, i, k },
                new[] { k, i, g, e },
                new[] { e, g, c, a }))
                yield return face.Select(transmogrify).Reverse().ToArray();
        }

        private static IEnumerable<VertexInfo[]> ButtonHole() => hole(-1, -1, 7.75, 1);
        private static IEnumerable<VertexInfo[]> PageNumHole() => hole(8.5, -1, 10.5, 3);

        private static IEnumerable<VertexInfo[]> Button()
        {
            const double yHeight = .05;

            VertexInfo transmogrify(VertexInfo v)
            {
                var x = -v.Location.X * 0.135 + .66;
                var z = v.Location.Z * 0.135 - .66;

                return pt(x, v.Location.Y + .15 - yHeight, z).WithNormal(v.Normal.Value).WithTexture(.4771284794 * x + .46155, -.4771284794 * z + .5337373145);
            }

            const double lx = -1;
            const double ty = -1;
            const double rx = 7.75;
            const double by = 1;
            const double bevel = .25;

            var a = pt(lx + .25 + bevel * tan(22.5), yHeight, ty + bevel).WithNormal(0, 1, 0);
            var b = pt(rx - .25 - bevel * tan(22.5), yHeight, ty + bevel).WithNormal(0, 1, 0);
            var c = pt(lx + .25, 0, ty).WithNormal(0, 1, -1);
            var d = pt(rx - .25, 0, ty).WithNormal(0, 1, -1);
            var e = pt(lx + bevel, yHeight, ty + .25 + bevel * tan(22.5)).WithNormal(0, 1, 0);
            var f = pt(rx - bevel, yHeight, ty + .25 + bevel * tan(22.5)).WithNormal(0, 1, 0);
            var g = pt(lx, 0, ty + .25).WithNormal(1, 1, 0);
            var h = pt(rx, 0, ty + .25).WithNormal(-1, 1, 0);
            var i = pt(lx, 0, by - .25).WithNormal(1, 1, 0);
            var j = pt(rx, 0, by - .25).WithNormal(-1, 1, 0);
            var k = pt(lx + bevel, yHeight, by - .25 - bevel * tan(22.5)).WithNormal(0, 1, 0);
            var l = pt(rx - bevel, yHeight, by - .25 - bevel * tan(22.5)).WithNormal(0, 1, 0);
            var m = pt(lx + .25, 0, by).WithNormal(0, 1, 1);
            var n = pt(rx - .25, 0, by).WithNormal(0, 1, 1);
            var o = pt(lx + .25 + bevel * tan(22.5), yHeight, by - bevel).WithNormal(0, 1, 0);
            var p = pt(rx - .25 - bevel * tan(22.5), yHeight, by - bevel).WithNormal(0, 1, 0);

            foreach (var face in Ut.NewArray(
                new[] { c, d, b, a },
                new[] { d, h, f, b },
                new[] { h, j, l, f },
                new[] { p, l, j, n },
                new[] { o, p, n, m },
                new[] { o, m, i, k },
                new[] { k, i, g, e },
                new[] { e, g, c, a },
                new[] { a, b, f, l, p, o, k, e }))
                yield return face.Select(transmogrify).ToArray();
        }
    }
}
