using System.Collections.Generic;
using System.IO;
using System.Linq;
using KtaneStuff.Modeling;
using RT.KitchenSink;
using RT.Util;
using RT.Util.ExtensionMethods;
using RT.Util.Geometry;

namespace KtaneStuff
{
    using static Md;

    static class Hexamaze
    {
        public static void DoModels()
        {
            File.WriteAllText(@"D:\c\KTANE\Hexamaze\Assets\Models\Screen.obj", GenerateObjFile(Screen(), "Screen"));
            File.WriteAllText(@"D:\c\KTANE\Hexamaze\Assets\Models\ScreenFrame.obj", GenerateObjFile(ScreenFrame(), "ScreenFrame"));
            File.WriteAllText(@"D:\c\KTANE\Hexamaze\Assets\Models\Button.obj", GenerateObjFile(Button(), "Button"));
            File.WriteAllText(@"D:\c\KTANE\Hexamaze\Assets\Models\Pawn.obj", GenerateObjFile(Pawn(), "Pawn"));
        }

        private static IEnumerable<Pt[]> Screen()
        {
            return Hex.LargeHexagonOutline(sideLength: 4, hexWidth: 1)
                .SelectConsecutivePairs(true, (p1, p2) => new[] { pt(0, 0, 0), pt(p2.X, 0, p2.Y), pt(p1.X, 0, p1.Y) });
        }

        private static IEnumerable<VertexInfo[]> ScreenFrame()
        {
            var arr = Ut.NewArray(
                new { Outline = Hex.LargeHexagonOutline(sideLength: 4, hexWidth: 1, expand: .0).InsertKinks(.0).ToArray(), Y = 0d },
                new { Outline = Hex.LargeHexagonOutline(sideLength: 4, hexWidth: 1, expand: .05).InsertKinks(.05).ToArray(), Y = .1 },
                new { Outline = Hex.LargeHexagonOutline(sideLength: 4, hexWidth: 1, expand: .2).InsertKinks(.35).ToArray(), Y = .1 },
                new { Outline = Hex.LargeHexagonOutline(sideLength: 4, hexWidth: 1, expand: .25).InsertKinks(.4).ToArray(), Y = 0d }
            ).Reverse().ToArray();
            return CreateMesh(false, true, Ut.NewArray(arr.Length, arr[0].Outline.Length, (x, y) => pt(arr[x].Outline[y].X, arr[x].Y, arr[x].Outline[y].Y, Normal.Mine, Normal.Mine, Normal.Mine, Normal.Mine)));
        }

        private static IEnumerable<PointD> InsertKinks(this IEnumerable<PointD> src, double expand)
        {
            expand += 3.5 * Hex.WidthToHeight;
            var i = 0;
            foreach (var elem in src)
            {
                yield return elem;
                i++;
                if (i % 7 == 0)
                    yield return new PointD(expand * cos(60 * (i / 7) - 90), expand * sin(60 * (i / 7) - 90));
            }
        }

        private static IEnumerable<VertexInfo[]> Button()
        {
            var f = .0035;
            var h = .0075;
            var w = .12;
            return CreateMesh(false, true,
                new BevelPoint(0, 0, Normal.Mine, Normal.Mine).Concat(
                Bézier(p(w, 0), p(w, f), p(w - h + f, h), p(w - h, h), 20).Select((p, first, last) => new BevelPoint(p.X, p.Y, first || last ? Normal.Mine : Normal.Average, first || last ? Normal.Mine : Normal.Average)))
                .Concat(new BevelPoint(0, h, Normal.Mine, Normal.Mine))
                .Select(bi =>
                    new[] { new { Angle = 90, Dist = .1 }, new { Angle = 190, Dist = .5 }, new { Angle = -10, Dist = .5 } }
                        .Select(info => pt(bi.Into * info.Dist * cos(info.Angle), bi.Y, bi.Into * info.Dist * sin(info.Angle), bi.Before, bi.After, Normal.Mine, Normal.Mine))
                        .ToArray()
                ).ToArray());
        }

        private static IEnumerable<VertexInfo[]> Pawn()
        {
            return CreateMesh(false, true, DecodeSvgPath.Do(@"M 13,0 C 19,0 22,3 22,9 22,15 19,14 19,20 19,28 26,30 26,48 26,58 20,57 13,57", .1)
                .FirstOrDefault()
                .SelectConsecutivePairs(true, (p1, p2) => p1 == p2 ? (PointD?) null : p1)
                .Where(p => p != null)
                .Select(p => p.Value + new PointD(-13, -57))
                .Select(p => Enumerable.Range(0, 72).Select(i => i * 360 / 72).Select(angle => pt(p.X * cos(angle), -p.Y, p.X * sin(angle))).ToArray())
                .Reverse()
                .ToArray());
        }
    }
}
