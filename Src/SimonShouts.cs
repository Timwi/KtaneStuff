using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using RT.KitchenSink;
using RT.Util;
using RT.Util.Consoles;
using RT.Util.ExtensionMethods;
using RT.Util.Text;
using KtaneStuff.Modeling;
using System.IO;
using RT.Util.Geometry;

namespace KtaneStuff
{
    using static Md;

    static class SimonShouts
    {
        public static void Experiment()
        {
            var grid = @"RRGRRRGRYRBRYRBRRRRGRBRYRGRRRBRYRGGGRGGGYGBGYGBGGRGGGBGYGGGRGBGYRRGRRRGRYRBRYRBRBRBGBBBYBGBRBBBYRGGGRGGGYGBGYGBGYRYGYBYYYGYRYBYYRYGYRYGYYYBYYYBYGRGGGBGYGGGRGBGYRBGBRBGBYBBBYBBBRRRGRBRYRGRRRBRYRYGYRYGYYYBYYYBYBRBGBBBYBGBRBBBYRBGBRBGBYBBBYBBBYRYGYBYYYGYRYBYY";

            var tt = new TextTable { ColumnSpacing = 1 };
            for (var ix = 0; ix < 16; ix++)
                tt.SetCell(ix + 1, 0, Enumerable.Range(0, 2).Select(i => "RGBY"[(ix >> (2 * i)) % 4]).JoinString());
            for (var ix = 0; ix < 16; ix++)
                tt.SetCell(0, ix + 1, Enumerable.Range(0, 2).Select(i => "RGBY"[(ix >> (2 * i)) % 4]).JoinString());
            for (var comboIx = 0; comboIx < 256; comboIx++)
            {
                var combo = Enumerable.Range(0, 4).Select(i => "RGBY"[(comboIx >> (2 * i)) % 4]).JoinString();

                var valid = Ut.NewArray(256, ix =>
                        combo[0] != grid[ix] &&
                        combo[1] != grid[(ix + 1) % 16 + 16 * (ix / 16)] &&
                        combo[2] != grid[ix % 16 + 16 * ((ix / 16 + 1) % 16)] &&
                        combo[3] != grid[(ix + 1) % 16 + 16 * ((ix / 16 + 1) % 16)]);

                var q = new Queue<int>();
                q.Enqueue(Enumerable.Range(0, 256).First(i => valid[i]));
                var covered = new HashSet<int>();
                var steps = new List<(int dx, int dy)> { (0, -2), (-1, -1), (0, -1), (1, -1), (-2, 0), (-1, 0), (1, 0), (2, 0), (-1, 1), (0, 1), (1, 1), (0, 2) };
                //steps.AddRange(new[] { (-3, 0), (-2, 1), (-1, 2), (0, 3), (1, 2), (2, 1), (3, 0), (2, -1), (1, -2), (0, -3), (-1, -2), (-2, -1) });
                steps.AddRange(new[] { (-2, 1), (-1, 2), (1, 2), (2, 1), (2, -1), (1, -2), (-1, -2), (-2, -1) });
                while (q.Count > 0)
                {
                    var elem = q.Dequeue();
                    if (!covered.Add(elem))
                        continue;
                    foreach (var (dx, dy) in steps)
                    {
                        var n = (elem + dx + 16) % 16 + 16 * ((elem / 16 + dy + 16) % 16);
                        if (valid[n])
                            q.Enqueue(n);
                    }
                }

                if (combo == "YYYY")
                    ConsoleUtil.WriteLine(Enumerable.Range(0, 16).Select(row => Enumerable.Range(0, 16).Select(col => "██".Color(valid[col + 16 * row] && covered.Contains(col + 16 * row) ? ConsoleColor.Yellow : valid[col + 16 * row] ? ConsoleColor.Magenta : ConsoleColor.Blue)).JoinColoredString()).JoinColoredString("\n"));

                tt.SetCell(comboIx % 16 + 1, comboIx / 16 + 1, covered.Count == valid.Count(b => b) ? "OK".Color(ConsoleColor.Green) : $"{covered.Count * 100 / valid.Count(b => b),2}".Color(ConsoleColor.Magenta));
            }
            tt.WriteToConsole();
        }

        public static void Experiment2()
        {
            var grid = @"RRGRRRGRYRBRYRBRRRRGRBRYRGRRRBRYRGGGRGGGYGBGYGBGGRGGGBGYGGGRGBGYRRGRRRGRYRBRYRBRBRBGBBBYBGBRBBBYRGGGRGGGYGBGYGBGYRYGYBYYYGYRYBYYRYGYRYGYYYBYYYBYGRGGGBGYGGGRGBGYRBGBRBGBYBBBYBBBRRRGRBRYRGRRRBRYRYGYRYGYYYBYYYBYBRBGBBBYBGBRBBBYRBGBRBGBYBBBYBBBYRYGYBYYYGYRYBYY";

            var seed = Rnd.Next();
            Console.WriteLine($"Seed: {seed}");
            var rnd = new Random(seed);
            var iter = 0;
            tryAgain:
            if (iter > 10000)
                Debugger.Break();
            var startposition = rnd.Next(0, grid.Length);
            var endposition = Enumerable.Range(0, grid.Length).Except(new[] { startposition }).PickRandom(rnd);
            var valid = Ut.NewArray(256, ix => ix == endposition || (
                grid[endposition] != grid[ix] &&
                grid[(endposition + 1) % 16 + 16 * (endposition / 16)] != grid[(ix + 1) % 16 + 16 * (ix / 16)] &&
                grid[endposition % 16 + 16 * ((endposition / 16 + 1) % 16)] != grid[ix % 16 + 16 * ((ix / 16 + 1) % 16)] &&
                grid[(endposition + 1) % 16 + 16 * ((endposition / 16 + 1) % 16)] != grid[(ix + 1) % 16 + 16 * ((ix / 16 + 1) % 16)]));

            var allPossibleMovements = (from dx in Enumerable.Range(-3, 7) from dy in Enumerable.Range(-3, 7) where (Math.Abs(dx) + Math.Abs(dy)).IsBetween(1, 3) select (dx, dy)).ToArray().Shuffle(rnd);
            var movements = allPossibleMovements.Subarray(0, 4);

            var q = new Queue<int>();
            q.Enqueue(startposition);
            var covered = new Dictionary<int, int>();    // key = grid index; value = distance from startposition
            covered[startposition] = 0;
            while (q.Count > 0)
            {
                var iad = q.Dequeue();
                foreach (var (dx, dy) in movements)
                {
                    var n = (iad + dx + 16) % 16 + 16 * ((iad / 16 + dy + 16) % 16);
                    if (valid[n] && !covered.ContainsKey(n))
                    {
                        covered[n] = covered[iad] + 1;
                        q.Enqueue(n);
                    }
                }
            }

            if (!covered.ContainsKey(endposition) || !covered[endposition].IsBetween(3, 5))
            {
                iter++;
                goto tryAgain;
            }

            Console.WriteLine($"Start: {(from dy in new[] { 0, 1 } from dx in new[] { 0, 1 } select grid[(startposition % 16 + dx) % 16 + 16 * ((startposition / 16 + dy) % 16)]).JoinString()}");
            Console.WriteLine($"End: {(from dy in new[] { 0, 1 } from dx in new[] { 0, 1 } select grid[(endposition % 16 + dx) % 16 + 16 * ((endposition / 16 + dy) % 16)]).JoinString()}");
            Console.WriteLine($"Movements: {movements.JoinString(", ")}");
            Console.WriteLine($"Dist: {covered[endposition]}");
            ConsoleUtil.WriteLine(Enumerable.Range(0, 16).Select(row => Enumerable.Range(0, 16)
                .Select(col => col + 16 * row)
                .Select(ix => "██".Color(
                        ix == startposition ? ConsoleColor.Green :
                        ix == endposition ? ConsoleColor.Red :
                        valid[ix] && covered.ContainsKey(ix) ? ConsoleColor.Yellow :
                        valid[ix] ? ConsoleColor.Magenta :
                        ConsoleColor.Blue))
                .JoinColoredString()).JoinColoredString("\n"));
            Console.WriteLine($"iter: {iter}");
            Console.WriteLine($"Reachable points: {Enumerable.Range(0, 256).Count(i => valid[i] && covered.ContainsKey(i))}");
        }

        public static void CreateModels()
        {
            // BUTTON
            static PointD[] getPath(string svg) => DecodeSvgPath.Do(svg, 1).SelectMany(ps => ps.Select(pt => p((pt.X - 50) / 1000, (100 - pt.Y) / 1000))).ToArray();
            var ys = new[] { 0, .004, .009, .01 };
            var outlines = Ut.NewArray(
                getPath(@"M 50,-10 90,60 65,85 50,70 35,85 10,60 Z"),
                getPath(@"M 50,-7.9824219 88.751953,59.832031 65,83.585938 l -15,-15 -15,15 -23.751953,-23.753907 z"),
                getPath(@"M 50,2.0957031 82.515625,58.998047 65,76.515625 l -15,-15 -15,15 -17.515625,-17.517578 z"),
                getPath(@"M 50,10.15625 77.527344,58.330078 65,70.859375 l -15,-15 -15,15 -12.527344,-12.529297 z"));

            var mesh = CreateMesh(true, false, Ut.NewArray(outlines[0].Length, xIx => Ut.NewArray(outlines.Length, yIx => outlines[yIx][xIx].Apply(p => pt(p.X, ys[yIx], p.Y)))), flatNormals: true).ToList();
            mesh.Add(outlines.Last().Select(p => pt(p.X, ys.Last(), p.Y).WithNormal(0, 1, 0)).ToArray());
            File.WriteAllText(@"D:\c\KTANE\SimonShouts\Assets\Models\Button.obj", GenerateObjFile(mesh, "Button"));

            // MODULE FRONT PLATE
            var plateY = .15;
            var frame = Ut.NewArray(
                p(0.854439, -0.256271),
                p(0.861069, -0.396061),
                p(0.784569, -0.472279),
                p(0.672195, -0.46653),
                p(0.524693, -0.545002),
                p(0.443619, -0.693596),
                p(0.444206, -0.774025),
                p(0.364376, -0.854439),
                p(0.256273, -0.854439),
                p(-0.256271, -0.854439),
                p(-0.854439, -0.854439),
                p(-0.854439, -0.256271),
                p(-0.854439, 0.256271),
                p(-0.854439, 0.854439),
                p(-0.256273, 0.854439),
                p(0.256273, 0.854439),
                p(0.854439, 0.854439),
                p(0.854439, 0.256271)).ToArray();

            var fragment = new[] { p(50, -10), p(90, 60) }.Select(pt => p((pt.X - 50) / 1000, (100 - pt.Y) / 1000)).Select(pt => p(pt.X * 7.5, pt.Y * 7.5)).ToArray();
            var buttonOutline = Enumerable.Range(0, 4).SelectMany(rot => fragment.Select(p => p.Rotated(Math.PI / 2 * rot))).Reverse().ToArray();

            var triangles = Md.Triangulate(new[] { frame, buttonOutline });
            File.WriteAllText(@"D:\c\KTANE\SimonShouts\Assets\Models\ModuleFrontPlate.obj", GenerateObjFile(triangles.Select(tri => tri.Select(p => pt(p.X, plateY, p.Y).WithTexture(.4771284794 * p.X + .46155, -.4771284794 * p.Y + .5337373145)).Reverse().ToArray()), "ModuleFrontPlate"));


            // TRAY
            const double h = .25;
            const double t = .1;
            File.WriteAllText(@"D:\c\KTANE\SimonShouts\Assets\Models\Tray.obj", GenerateObjFile(
                CreateMesh(true, false, Enumerable.Range(0, 4).Select(x => new[] { pt(0, 0, 1), pt(0, h, 1), pt(0, h, 1 - t), pt(0, 0, 1 - t) }.Select(p => p.RotateY(-90 * x)).ToArray()).ToArray(), flatNormals: true),
                "Tray", AutoNormal.Flat));


            // BUTTON COLLIDER
            var colliderOutline = new[] { p(50, -10), p(90, 60), p(65, 85), p(35, 85), p(10, 60) }.Reverse().Select(pt => p((pt.X - 50) / 1000, (100 - pt.Y) / 1000)).ToArray();
            File.WriteAllText(@"D:\c\KTANE\SimonShouts\Assets\Models\ButtonCollider.obj", GenerateObjFile(colliderOutline.Extrude(.01, true, true), "ButtonCollider"));

            // BUTTON HIGHLIGHT
            var highlightOutline = getPath("M 50,-20.078125 45.658203,-12.480469 3.7636719,60.835938 35,92.070312 l 15,-15 15,15 31.236328,-31.234374 z");
            var triangulated = Md.Triangulate(new[] { highlightOutline, outlines[0] });
            File.WriteAllText(@"D:\c\KTANE\SimonShouts\Assets\Models\ButtonHighlight.obj", GenerateObjFile(triangulated.Select(t => t.Select(p => pt(p.X, 0, p.Y)).ToArray()).ToArray(), "ButtonHighlight"));
        }
    }
}