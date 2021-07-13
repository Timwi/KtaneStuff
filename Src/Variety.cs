using System;
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

    static class Variety
    {
        public static void DoModels()
        {
            // Wires
            File.WriteAllText(@"D:\c\KTANE\Variety\Assets\Items\Wire\WireBase.obj", GenerateObjFile(WireBase(), "WireBase"));

            // Keys in locks
            File.WriteAllText(@"D:\c\KTANE\Variety\Assets\Items\Key\KeyBase1.obj", GenerateObjFile(KeyBase(1), "KeyBase1"));
            File.WriteAllText(@"D:\c\KTANE\Variety\Assets\Items\Key\KeyBase2.obj", GenerateObjFile(KeyBase(2), "KeyBase2"));

            // Mazes
            File.WriteAllText($@"D:\c\KTANE\Variety\Assets\Items\Maze\Frame3x3.obj", GenerateObjFile(MazeFrame(MazeComponent.Frame, 3, 3), $"Frame3x3"));
            File.WriteAllText($@"D:\c\KTANE\Variety\Assets\Items\Maze\Back3x3.obj", GenerateObjFile(MazeFrame(MazeComponent.Back, 3, 3), $"Back3x3"));
            File.WriteAllText($@"D:\c\KTANE\Variety\Assets\Items\Maze\Frame3x4.obj", GenerateObjFile(MazeFrame(MazeComponent.Frame, 3, 4), $"Frame3x4"));
            File.WriteAllText($@"D:\c\KTANE\Variety\Assets\Items\Maze\Back3x4.obj", GenerateObjFile(MazeFrame(MazeComponent.Back, 3, 4), $"Back3x4"));
            File.WriteAllText($@"D:\c\KTANE\Variety\Assets\Items\Maze\Frame4x3.obj", GenerateObjFile(MazeFrame(MazeComponent.Frame, 4, 3), $"Frame4x3"));
            File.WriteAllText($@"D:\c\KTANE\Variety\Assets\Items\Maze\Back4x3.obj", GenerateObjFile(MazeFrame(MazeComponent.Back, 4, 3), $"Back4x3"));
            File.WriteAllText($@"D:\c\KTANE\Variety\Assets\Items\Maze\Frame4x4.obj", GenerateObjFile(MazeFrame(MazeComponent.Frame, 4, 4), $"Frame4x4"));
            File.WriteAllText($@"D:\c\KTANE\Variety\Assets\Items\Maze\Back4x4.obj", GenerateObjFile(MazeFrame(MazeComponent.Back, 4, 4), $"Back4x4"));
            File.WriteAllText($@"D:\c\KTANE\Variety\Assets\Items\Maze\Button.obj", GenerateObjFile(MazeButton(), "Button"));
            File.WriteAllText($@"D:\c\KTANE\Variety\Assets\Items\Maze\ButtonHighlight.obj", GenerateObjFile(MazeButtonHighlight(), "ButtonHighlight"));

            // Knobs
            File.WriteAllText($@"D:\c\KTANE\Variety\Assets\Items\Knob\Knob.obj", GenerateObjFile(Knob(highlight: false), "Knob"));
            File.WriteAllText($@"D:\c\KTANE\Variety\Assets\Items\Knob\KnobHighlight.obj", GenerateObjFile(Knob(highlight: true), "KnobHighlight"));

            // Digit Display
            File.WriteAllText($@"D:\c\KTANE\Variety\Assets\Items\DigitDisplay\Frame.obj", GenerateObjFile(DisplayFrame(.2, 1.4), "Frame"));

            // Letter Display
            File.WriteAllText($@"D:\c\KTANE\Variety\Assets\Items\LetterDisplay\LetterFrame.obj", GenerateObjFile(DisplayFrame(.125, .61), "LetterFrame"));
            var segments = Ut.NewArray(
                "M5.104.25L2.855 2.5l2.249 2.25h29.792l2.249-2.25L34.896.25H5.104z",
                "M2.5 2.855L.25 5.104v27.292l2.25 2.249 2.25-2.249V5.104L2.5 2.855z",
                "M5.25 5.25v5.66l9.85 21.34h-.041l4.384 2.193-2.193-4.384v-3.885L7.592 5.25H5.25z",
                "M17.75 5.25v24.691L20 34.443l2.25-4.502V5.25h-4.5z",
                "M32.408 5.25L22.75 26.174v3.885l-2.193 4.384 4.384-2.193h-.04l9.849-21.34V5.25h-2.342z",
                "M37.5 2.855l-2.25 2.249v27.292l2.25 2.249 2.25-2.249V5.104L37.5 2.855z",
                "M5.104 32.75L2.855 35l2.249 2.25h9.837L19.443 35l-4.502-2.25H5.104z",
                "M25.059 32.75L20.557 35l4.5 2.25h9.84L37.144 35l-2.249-2.25H25.06z",
                "M2.5 35.355L.25 37.604v27.292l2.25 2.249 2.25-2.249V37.604L2.5 35.355z",
                "M19.443 35.557L15.06 37.75h.04L5.25 59.09v5.66h2.342l9.658-20.924v-3.885l2.193-4.384z",
                "M20 35.557l-2.25 4.502V64.75h4.5V40.059L20 35.557z",
                "M20.557 35.557l2.193 4.384v3.885l9.658 20.924h2.342v-5.66L24.9 37.75h.041l-4.384-2.193z",
                "M37.5 35.355l-2.25 2.249v27.292l2.25 2.249 2.25-2.249V37.604l-2.25-2.249z",
                "M5.104 65.25L2.855 67.5l2.249 2.25h29.792l2.249-2.25-2.249-2.25H5.104z");
            for (int i = 0; i < segments.Length; i++)
                File.WriteAllText($@"D:\c\KTANE\Variety\Assets\Items\LetterDisplay\LetterSegment{i}.obj",
                    GenerateObjFile(DecodeSvgPath.Do(DecodeSvgPath.DecodePieces(segments[i]).Select(piece => piece.Select(p => -new PointD(p.X - 20, p.Y - 35) / 260)), 1).Triangulate().Select(arr => arr.Select(p => pt(p.X, 0, p.Y).WithNormal(0, 1, 0)).Reverse().ToArray()), $"LetterSegment{i}"));

            // Buttons
            for (var vertices = 3; vertices <= 6; vertices++)
                File.WriteAllText($@"D:\c\KTANE\Variety\Assets\Items\Button\Button{vertices}.obj", GenerateObjFile(Button(vertices), $"Button{vertices}"));
        }

        private static IEnumerable<VertexInfo[]> Button(int vertices)
        {
            var f = .03;
            var h = .05;
            var h2 = .06;
            var st = .05;
            var b = .1;
            return CreateMesh(false, true,
                new BevelPoint(st, -b, normal: pt(0, -1, 0))
                    .Concat(new[] { new BevelPoint(st, -h, normal: pt(0, -1, 0)) })
                    .Concat(Bézier(p(.2 - h2, -h), p(.2 - h2 + f, -h), p(.2, -f), p(.2, 0), 20).Select((p, first, last) => new BevelPoint(p.X, p.Y, first || last ? Normal.Mine : Normal.Average, first || last ? Normal.Mine : Normal.Average)))
                    .Concat(Bézier(p(.2, 0), p(.2, f), p(.2 - h2 + f, h), p(.2 - h2, h), 20).Skip(1).Select((p, first, last) => new BevelPoint(p.X, p.Y, last ? Normal.Mine : Normal.Average, last ? Normal.Mine : Normal.Average)))
                    .Concat(new BevelPoint(0, h, normal: pt(0, 1, 0)))
                    .Select(bi => Enumerable.Range(0, vertices).Select(i => 360d * i / vertices)
                    .Select(angle => bi.NormalOverride == null
                        ? pt(bi.X * cos(angle), bi.Y, bi.X * sin(angle), bi.Before, bi.After, Normal.Mine, Normal.Mine)
                        : pt(bi.X * cos(angle), bi.Y, bi.X * sin(angle), pt(bi.NormalOverride.Value.X * cos(angle), bi.NormalOverride.Value.Y, bi.NormalOverride.Value.X * sin(angle))))
                    .ToArray())
                .ToArray());
        }

        public static void GetWordlist()
        {
            const int len = 3;
            var words = new HashSet<string>(File.ReadLines(@"D:\Daten\Wordlists\VeryCommonWords.txt").Where(line => line.Length == len && line.All(ch => ch >= 'A' && ch <= 'Z')));

            words.Remove("YER");
            words.Remove("TAE");
            words.Remove("BUM");
            words.Remove("GAY");
            words.Remove("WAN");
            words.Remove("SEX");
            words.Remove("BUD");
            words.Remove("CAB");
            words.Remove("DON");
            words.Remove("GUN");

            words.Remove("THE");
            words.Remove("BAN");

            foreach (var ind in new[] { "SND", "CLR", "CAR", "IND", "FRQ", "SIG", "NSA", "MSA", "TRN", "BOB", "FRK" })
                words.Add(ind);
            foreach (var extra in "MIC,EGG,IPA,PIE,POW,RED,ZOO,MOD,FMN,RPS,RCA,RGB,TGB,ZIG,ZAG,WIZ,BIZ,INK,ILK,NLL,NIL,QUA,QUE,QUI,QUO,LUA,DUO,LUG,FAQ,IRK".Split(','))
                words.Add(extra);

            while (true)
            {
                var prevLength = words.Count;
                words = new HashSet<string>(words.Where(word =>
                    Enumerable.Range(0, len).Count(ch => Enumerable.Range(0, 26).Select(i => (char) ('A' + i)).Count(ltr => words.Contains(word.Remove(ch, 1).Insert(ch, ltr.ToString()))) >= 2) >= 2));
                if (words.Count == prevLength)
                    break;
            }
            File.WriteAllLines(@"D:\temp\temp.txt", words.Order());
            for (var i = 0; i < 3; i++)
                Console.WriteLine($"Letter {i + 1}: {words.Select(w => w[i]).Distinct().Order().JoinString()}");
            Console.WriteLine($"Unused letters: {"ABCDEFGHIJKLMNOPQRSTUVWXYZ".Where(ch => words.All(w => !w.Contains(ch))).JoinString()}");
        }

        private static MeshVertexInfo[] bpa(double x, double y, double z, Normal befX, Normal afX, Normal befY, Normal afY) =>
            new[] { pt(x, y, z, befX, afX, befY, afY).WithTexture((x + 1) / 2, (z + 1) / 2) };
        private static IEnumerable<VertexInfo[]> DisplayFrame(double th, double ratio)
        {
            const double depth = .06;
            const double béFac = depth * .55;

            return CreateMesh(true, true,
                Bézier(p(th, -.1), p(th, béFac), p(th - depth + béFac, depth), p(th - depth, depth), 20).Select((p, first, last) => new BevelPoint(p.X, p.Y, first || last ? Normal.Mine : Normal.Average, first || last ? Normal.Mine : Normal.Average)).Concat(
                Bézier(p(depth, depth), p(depth - béFac, depth), p(0, béFac), p(0, -.1), 20).Select((p, first, last) => new BevelPoint(p.X, p.Y, first || last ? Normal.Mine : Normal.Average, first || last ? Normal.Mine : Normal.Average)))
                .Select(bi => Ut.NewArray(
                    // Bottom right
                    bpa(-1 + bi.X, bi.Y, -ratio + bi.X, bi.Before, bi.After, Normal.Mine, Normal.Mine),

                    // Top right
                    bpa(-1 + bi.X, bi.Y, ratio - bi.X, bi.Before, bi.After, Normal.Mine, Normal.Mine),

                    // Top left
                    bpa(1 - bi.X, bi.Y, ratio - bi.X, bi.Before, bi.After, Normal.Mine, Normal.Mine),

                    // Bottom left
                    bpa(1 - bi.X, bi.Y, -ratio + bi.X, bi.Before, bi.After, Normal.Mine, Normal.Mine),

                    null
                ).Where(x => x != null).SelectMany(x => x).ToArray()).ToArray());
        }

        private static IEnumerable<VertexInfo[]> Knob(bool highlight)
        {
            const double h = 1;
            const double ct = .2;
            const double cb = .7;
            const double w = .5;
            const double d = .2;
            const double ri = .8;
            const double ro = 1;
            const int rv = 10;

            var yz = (new[] { p(0, h), p(ct, h), p(w, cb), p(w, 0) }).ReverseInplace();
            var c1 = yz.Select(p => pt(0, p.Y, ri + p.X)).ToArray();
            var c2 = yz.Select(p => pt(d, p.Y, ri + p.X)).ToArray();
            var c3 = yz.Select(p => pt(-d, p.Y, ro + p.X).RotateY(-180d / rv)).ToArray();
            var c4 = yz.Select(p => pt(0, p.Y, ro + p.X).RotateY(-180d / rv)).ToArray();

            var patch = BézierPatch(new[] { c1, c2, c3, c4 }, 10);
            var all = new List<Pt[]>();
            for (var i = 0; i < rv; i++)
            {
                all.AddRange(patch.SkipLast(1).Select(c => c.Select(p => p.RotateY(-360d * (i + .5) / rv)).ToArray()));
                all.AddRange(patch.Reverse().SkipLast(1).Select(c => c.Select(p => pt(-p.X, p.Y, p.Z).RotateY(-360d * (i + 1.5) / rv)).ToArray()));
            }

            if (highlight)
            {
                return all.Select(arr => p(arr[0].X, arr[0].Z)).Triangulate().Select(tri => tri.Select(p => pt(p.X, 0, p.Y).WithNormal(0, 1, 0)).ToArray().ReverseInplace()).ToArray();
            }
            else
            {
                return CreateMesh(true, false, all.Select(arr => arr.Select((p, f, l) =>
                    l ? p.WithMeshInfo(0, 1, 0) :
                    f ? p.WithMeshInfo(Normal.Average, Normal.Average, Normal.Mine, Normal.Mine) :
                    p.WithMeshInfo(Normal.Average, Normal.Average, Normal.Average, Normal.Average)).ToArray()).ToArray())
                    .Concat(all.Select(arr => arr.Last()).Select(pt => p(pt.X, pt.Z)).Triangulate().Select(tri => tri.Select(p => pt(p.X, h, p.Y).WithNormal(0, 1, 0)).ToArray()))
                    .ToArray()
                    .Texturize(h, 1);
            }
        }

        private static IEnumerable<VertexInfo[]> MazeButton()
        {
            const double w = 8;
            const double h = 4;
            const double ow = 12;
            const double oh = 6;
            var inner = new[] { p(-w, 0), p(w, 0), p(0, -h) }.Select(p => pt(p.X, 1.5, p.Y)).ToArray();
            var outer = new[] { p(-ow, 0), p(ow, 0), p(0, -oh) }.Select(p => pt(p.X, 0, p.Y)).ToArray();
            return CreateMesh(false, true, new[] { inner, outer }, flatNormals: true).Concat(inner.Select(p => p.WithNormal(0, 1, 0)).ToArray());
        }

        private static IEnumerable<VertexInfo[]> MazeButtonHighlight()
        {
            const double w = 12;
            const double h = 6;
            yield return new[] { p(-w, 0), p(0, -h), p(0, 0) }.Select(p => pt(p.X, 1.5, p.Y)).Select(p => p.WithNormal(0, 1, 0)).ToArray();
            yield return new[] { p(0, 0), p(0, -h), p(w, 0) }.Select(p => pt(p.X, 1.5, p.Y)).Select(p => p.WithNormal(0, 1, 0)).ToArray();
        }

        enum MazeComponent { Frame, Back }

        private static IEnumerable<VertexInfo[]> MazeFrame(MazeComponent which, int mazeWidth, int mazeHeight)
        {
            MeshVertexInfo[] frameImpl(Normal xNormal, double tx, double w, double h, double y, double bevel)
            {
                var bl = bevel * Math.Sqrt(2);
                var xl = w - 2 * bevel;
                var yl = h - 2 * bevel;
                var f = 4 * bl + 2 * xl + 2 * yl;
                bl /= f;
                xl /= f;
                yl /= f;
                return Ut.NewArray<(double? ty, Pt p)>(
                        (null, pt(-w + bevel, y, h)),
                        (xl, pt(w - bevel, y, h)),
                        (xl + bl, pt(w, y, h - bevel)),
                        (xl + yl + bl, pt(w, y, -h + bevel)),
                        (xl + yl + 2 * bl, pt(w - bevel, y, -h)),
                        (2 * xl + yl + 2 * bl, pt(-w + bevel, y, -h)),
                        (2 * xl + yl + 3 * bl, pt(-w, y, -h + bevel)),
                        (2 * xl + 2 * yl + 3 * bl, pt(-w, y, h - bevel)))
                    .Select(tup => new MeshVertexInfo(tup.p, xNormal, xNormal, Normal.Mine, Normal.Mine, new PointD(tx, tup.ty ?? 1), new PointD(tx, tup.ty ?? 0)))
                    .ToArray();
            }
            MeshVertexInfo[] frame(Normal xNormal, double ty, double margin, double y, double bevel) =>
                frameImpl(xNormal, ty, mazeWidth * .5 + margin, mazeHeight * .5 + margin, y, bevel);

            return which switch
            {
                MazeComponent.Frame => CreateMesh(false, true, Ut.NewArray(
                    frame(Normal.Mine, 0, .1, 0, .3),
                    frame(Normal.Average, .6, .3, .4, .32),
                    frame(Normal.Mine, 1, .5, 0, .332))),

                MazeComponent.Back => frame(Normal.Mine, 0, .1, 0, .3).SelectConsecutivePairs(true, (v1, v2) => new[] { pt(0, 0, 0).WithNormal(0, 1, 0), v1.Location.WithNormal(0, 1, 0), v2.Location.WithNormal(0, 1, 0) }).ToArray(),

                _ => throw new InvalidOperationException(),
            };
        }

        private static IEnumerable<VertexInfo[]> KeyBase(int which)
        {
            const double h = .7;
            const double w = .5;
            const double c1 = .3;
            const double c2 = .2;
            const double c3 = .3;
            const int rev = 32;

            switch (which)
            {
                case 1:
                    var bézier = SmoothBézier(p(c3, h), p(c3 + c2, h), p(w, c1), p(w, 0), .005).ToArray();
                    return CreateMesh(true, false, Enumerable.Range(0, rev)
                        .Select(i => i * 360.0 / rev)
                        .Select((angle, fa, la) => bézier
                            .Select((p, i, first, last) => pt(p.X, p.Y, 0).RotateY(angle).Apply(pr =>
                                first ? pr.WithMeshInfo(0, 1, 0) :
                                last ? pr.WithMeshInfo(Normal.Average, Normal.Average, Normal.Mine, Normal.Theirs) :
                                pr.WithMeshInfo(Normal.Average, Normal.Average, Normal.Average, Normal.Average))
                                .WithTexture(fa ? 1 : angle / 360.0, (double) i / (bézier.Length - 1), fa ? 0 : angle / 360.0, (double) i / (bézier.Length - 1)))
                            .ToArray())
                        .ToArray())
                        .SelectMany(arr => new[] { new[] { arr[0], arr[1], arr[3] }, new[] { arr[3], arr[1], arr[2] } });

                default:
                    const double slitW = .035;
                    const double slitH = .1;

                    var outline = Enumerable.Range(0, rev).Select(i => i * 360 / rev).Select(angle => pt(c3, h, 0).RotateY(angle)).Select(pt => p(pt.X, pt.Z)).ToArray();
                    var slit = (new[] { p(-slitW, -slitH), p(slitW, -slitH), p(slitW, slitH), p(-slitW, slitH) }).ReverseInplace();
                    return new[] { outline, slit }.Triangulate().Select(tri => tri.Select(p => pt(p.X, h, p.Y).WithNormal(0, 1, 0).WithTexture((p.X + c3) / (2 * c3), (p.Y + c3) / (2 * c3))).Reverse().ToArray());
            }
        }

        private static IEnumerable<VertexInfo[]> WireBase()
        {
            const double height = .05;
            const double holeI = .03;
            const double holeO = .05;
            const double holeDepth = .05;

            var curve = new[] { p(0, height - holeDepth), p(holeI, height), p(holeO, height), p(holeO, 0) };

            return CreateMesh(true, false, Enumerable.Range(0, 4)
                .Select(i => i * 360.0 / 4)
                .Select(angle => curve
                    .Select(p => pt(p.X, p.Y, 0).RotateY(angle).Apply(pr => pr.WithMeshInfo(Normal.Average, Normal.Average, Normal.Mine, Normal.Mine)))
                    .ToArray())
                .ToArray());
        }
    }
}