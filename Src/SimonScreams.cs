using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using KtaneStuff.Modeling;
using RT.Util;
using RT.Util.Consoles;
using RT.Util.ExtensionMethods;
using RT.Util.Geometry;
using RT.Util.Text;

namespace KtaneStuff
{
    using static Md;

    static class SimonScreams
    {
        public static void DoModels()
        {
            File.WriteAllText(@"D:\c\KTANE\SimonScreams\Assets\Models\Button.obj", GenerateObjFile(Button(), "Button"));
            File.WriteAllText(@"D:\c\KTANE\SimonScreams\Assets\Models\ButtonHighlight.obj", GenerateObjFile(ButtonHighlight(), "ButtonHighlight"));
            File.WriteAllText(@"D:\c\KTANE\SimonScreams\Assets\Models\ButtonCollider.obj", GenerateObjFile(ButtonCollider(), "ButtonCollider"));

            var flapIx = 0;
            foreach (var flap in Flaps())
            {
                File.WriteAllText($@"D:\c\KTANE\SimonScreams\Assets\Models\Flap{flapIx}.obj", GenerateObjFile(flap, $"Flap{flapIx}"));
                flapIx++;
            }
        }

        private static IEnumerable<VertexInfo[]> Button()
        {
            var height = .2;
            var fh = height * .4;
            var innerRadius = 0.4;
            var outerRadius = 1.0;
            var fr = innerRadius * .1;
            var angle = 30.0;

            var bézierSteps = 20;
            var d = Math.Sqrt(innerRadius * innerRadius + outerRadius * outerRadius - 2 * innerRadius * outerRadius * cos(angle));
            var frix = outerRadius - (outerRadius - innerRadius * cos(angle)) / d * (d - fr);
            var friy = innerRadius * sin(angle) / d * (d - fr);
            var frox = outerRadius - (outerRadius - innerRadius * cos(angle)) / d * fr;
            var froy = innerRadius * sin(angle) / d * fr;

            var patchCoords = Ut.NewArray(
                new[] { pt(0, 0, 0), pt(fr * cos(-angle), fh, fr * sin(-angle)), pt((innerRadius - fr) * cos(-angle), fh, (innerRadius - fr) * sin(-angle)), pt(innerRadius * cos(-angle), 0, innerRadius * sin(-angle)) },
                new[] { pt(fr * cos(angle), fh, fr * sin(angle)), pt(innerRadius / 2, height, 0), pt(innerRadius * cos(angle / 2), height, innerRadius * sin(angle / 2)), pt(frix, fh, -friy) },
                new[] { pt((innerRadius - fr) * cos(angle), fh, (innerRadius - fr) * sin(angle)), pt(innerRadius * cos(angle / 2), height, innerRadius * sin(angle / 2)), pt((innerRadius + outerRadius) / 2, height, 0), pt(frox, fh, -froy) },
                new[] { pt(innerRadius * cos(angle), 0, innerRadius * sin(angle)), pt(frix, fh, friy), pt(frox, fh, froy), pt(outerRadius, 0, 0) }
            )
                .Select(arr => arr.Select(p => p.Add(x: .03)).ToArray())
                .ToArray();
            var patch = BézierPatch(patchCoords, bézierSteps);

            var extendedPatch = Ut.NewArray(bézierSteps + 2, bézierSteps + 2, (i, j) =>
            {
                var ii = i == 0 ? 1 : i == bézierSteps + 1 ? bézierSteps - 1 : i - 1;
                var jj = j == 0 ? 1 : j == bézierSteps + 1 ? bézierSteps - 1 : j - 1;
                return new MeshVertexInfo(
                    patch[ii][jj].Set(y: i == 0 || i == bézierSteps + 1 || j == 0 || j == bézierSteps + 1 ? -.03 : (double?) null),
                    i == bézierSteps + 1 ? Normal.Mine : Normal.Average, i == 0 ? Normal.Mine : Normal.Average,
                    j == bézierSteps + 1 ? Normal.Mine : Normal.Average, j == 0 ? Normal.Mine : Normal.Average
                );
            });

            return CreateMesh(false, false, extendedPatch);
        }

        private static IEnumerable<VertexInfo[]> ButtonHighlight()
        {
            var preRadius = .03;
            var innerRadius = .44;
            var outerRadius = 1.15;
            var angle = 32.0;

            var holeInnerRadius = .4;
            var holeOuterRadius = 1.0;
            var holeAngle = 30.0;

            return
                Triangulate(Ut.NewArray<IEnumerable<PointD>>(
                    new[] { p(-preRadius, 0), p(innerRadius * cos(-angle), innerRadius * sin(-angle)), p(outerRadius, 0), p(innerRadius * cos(angle), innerRadius * sin(angle)) },
                    new[] { p(0, 0), p(holeInnerRadius * cos(holeAngle), holeInnerRadius * sin(holeAngle)), p(holeOuterRadius, 0), p(holeInnerRadius * cos(-holeAngle), holeInnerRadius * sin(-holeAngle)) }.Select(p => p + new PointD(.03, 0))
                ))
                    .Select(poly => poly.Select(p => pt(p.X, 0, p.Y).WithNormal(0, 1, 0)).ToArray());
        }

        private static IEnumerable<Pt[]> ButtonCollider()
        {
            var innerRadius = .44;
            var outerRadius = 1.1;
            var angle = 30.0;
            var vertices = new[] { pt(0, 0, 0), pt(innerRadius * cos(-angle), 0, innerRadius * sin(-angle)), pt(outerRadius, 0, 0), pt(innerRadius * cos(angle), 0, innerRadius * sin(angle)) };

            // Front face
            yield return vertices.ToArray();
            // Back face
            yield return vertices.Select(v => v.Add(y: .05)).Reverse().ToArray();

            // Walls
            foreach (var face in vertices.SelectConsecutivePairs(true, (p1, p2) => new[] { p1, p1.Add(y: .05), p2.Add(y: .05), p2 }))
                yield return face;
        }

        private static IEnumerable<IEnumerable<VertexInfo[]>> Flaps()
        {
            var innerRadius = 0.4;
            var outerRadius = 1.02;
            var angle = 30.0;
            var depth = .01;
            var offset = .025;

            var outline = new[] { p(0, 0), p(innerRadius * cos(angle), innerRadius * sin(angle)), p(outerRadius, 0), p(innerRadius * cos(-angle), innerRadius * sin(-angle)) };
            var midPoint = Intersect.LineWithLine(new EdgeD(outline[0], outline[2]), new EdgeD(outline[1], outline[3]));

            for (int i = 0; i < 6; i++)
            {
                foreach (var face in outline.SelectConsecutivePairs(true, (p1, p2) => new[] { .96 * midPoint + .02 * (p1 + p2), p1, p2 }.Select(p => pt(p.X + offset, 0, p.Y).RotateY(60 * i - 15))))
                {
                    var flap = new List<VertexInfo[]>();

                    // Front face
                    var frontFace = face.Select(p => p.WithNormal(0, 1, 0).WithTexture(new PointD(.4771284794 * (-p.X * .8) + .46155, -.4771284794 * (-p.Z * .8) + .5337373145))).ToArray();
                    flap.Add(frontFace);
                    // Back face
                    flap.Add(frontFace.Select(p => p.Location.Add(y: -depth).WithNormal(0, -1, 0).WithTexture(p.Texture.Value)).Reverse().ToArray());

                    // Side faces
                    flap.AddRange(face.SelectConsecutivePairs(true, (p1, p2) => new[] { p2, p1, p1.Add(y: -depth), p2.Add(y: -depth) }.Select(p => p.WithNormal((p2 - p1) * (p1 - p1.Add(y: -depth)))).ToArray()));
                    yield return flap;
                }
            }
        }

        public static void GenerateLargeTable()
        {
            var grids = new List<int[][]>();
            for (int i = 0; i < 3; i++)
            {
                var grid = Ut.NewArray(6, 6, (_, __) => Rnd.Next(6));
                fill(grid, 0, 0);
                grids.Add(grid);
            }

            Console.WriteLine(Enumerable.Range(0, 6).Select(row => Enumerable.Range(0, 6).Select(col => $"<td>{grids.Select(g => "ACDEFH"[g[col][row]]).JoinString()}").JoinString()).JoinString(Environment.NewLine));
        }

        public static void GenerateSmallTable()
        {
            var grid = Ut.NewArray(6, 6, (_, __) => Rnd.Next(6));
            fill(grid, 0, 0);
            Console.WriteLine(Enumerable.Range(0, 6).Select(row => Enumerable.Range(0, 6).Select(col => $"<td>{"ROYGBP"[grid[col][row]]}").JoinString()).JoinString(Environment.NewLine));
        }

        public static void DoSvg()
        {
            var path = @"D:\c\KTANE\HTML\img\Component\Simon Screams.svg";
            File.WriteAllText(path, Regex.Replace(File.ReadAllText(path), @"(?<=<!--##-->).*(?=<!--###-->)",
                $@"<g transform='translate(174,174) scale(50)' fill='none' stroke='#000' stroke-width='.04' stroke-linejoin='round'>{
                    Enumerable.Range(0, 6).Select(i => i * 360 / 6 - 15).Select(angle =>
                        $"<path d='M0,0 L{1.25 * cos(30)},{1.25 * sin(30)} 3,0 {1.25 * cos(-30)},{1.25 * sin(-30)} z' transform='rotate({angle})' />"
                    ).JoinString()
                }</g>",
                RegexOptions.Singleline));
        }

        private static bool fill(int[][] grid, int x, int y)
        {
            if (x == 6)
            {
                x = 0;
                y++;
            }
            if (y == 6)
                return true;

            var offset = grid[x][y];
            for (int j = 0; j < 6; j++)
            {
                var i = (j + offset) % 6;
                for (int xx = x - 1; xx >= 0; xx--)
                    if (grid[xx][y] == i)
                        goto nope;
                for (int yy = y - 1; yy >= 0; yy--)
                    if (grid[x][yy] == i)
                        goto nope;
                grid[x][y] = i;
                if (fill(grid, x + 1, y))
                    return true;
                nope:;
            }
            return false;
        }

        public static void Simulation()
        {
            /* Version 1 (too hard)
            var criteria1 = Ut.NewArray(
                new { Name = "If three adjacent colors flashed in counter clockwise order", Criterion = Ut.Lambda((int[] seq, int[] rgb, int orange, int purple) => Enumerable.Range(0, seq.Length - 2).Any(ix => seq[ix + 1] == (seq[ix] + 5) % 6 && seq[ix + 2] == (seq[ix] + 4) % 6)) },
                new { Name = "Otherwise, if orange flashed more than twice", Criterion = Ut.Lambda((int[] seq, int[] rgb, int orange, int purple) => seq.Count(n => n == orange) > 2) },
                new { Name = "Otherwise, if two adjacent colors didn’t flash", Criterion = Ut.Lambda((int[] seq, int[] rgb, int orange, int purple) => Enumerable.Range(0, 6).Any(color => !seq.Contains(color) && !seq.Contains((color + 1) % 6))) },
                new { Name = "Otherwise, if exactly two colors flashed exactly twice", Criterion = Ut.Lambda((int[] seq, int[] rgb, int orange, int purple) => seq.GroupBy(n => n).Where(g => g.Count() == 2).Count() == 2) },
                new { Name = "Otherwise, if the number of colors that flashed is even", Criterion = Ut.Lambda((int[] seq, int[] rgb, int orange, int purple) => seq.Distinct().Count() % 2 == 0) },
                new { Name = "Otherwise", Criterion = Ut.Lambda((int[] seq, int[] rgb, int orange, int purple) => true) }
            );
            var criteria2 = Ut.NewArray(
                new { Name = "If two opposite colors didn’t flash", Criterion = Ut.Lambda((int[] seq, int[] rgb, int orange, int purple) => Enumerable.Range(0, 3).Any(col => !seq.Contains(col) && !seq.Contains(col + 3))) },
                new { Name = "Otherwise, if at most one of red, green and blue flashed", Criterion = Ut.Lambda((int[] seq, int[] rgb, int orange, int purple) => rgb.Count(color => seq.Contains(color)) <= 1) },
                new { Name = "Otherwise, if three adjacent colors flashed in clockwise order", Criterion = Ut.Lambda((int[] seq, int[] rgb, int orange, int purple) => Enumerable.Range(0, seq.Length - 2).Any(ix => seq[ix + 1] == (seq[ix] + 1) % 6 && seq[ix + 2] == (seq[ix] + 2) % 6)) },
                new { Name = "Otherwise, if a color flashed, then an adjacent color, then the first again", Criterion = Ut.Lambda((int[] seq, int[] rgb, int orange, int purple) => Enumerable.Range(0, seq.Length - 2).Any(ix => seq[ix + 2] == seq[ix] && (seq[ix + 1] == (seq[ix] + 1) % 6 || seq[ix + 1] == (seq[ix] + 5) % 6))) },
                new { Name = "Otherwise, if two adjacent colors flashed in clockwise order", Criterion = Ut.Lambda((int[] seq, int[] rgb, int orange, int purple) => Enumerable.Range(0, seq.Length - 1).Any(ix => seq[ix + 1] == (seq[ix] + 1) % 6)) },
                new { Name = "Otherwise", Criterion = Ut.Lambda((int[] seq, int[] rgb, int orange, int purple) => true) }
            );
            int numStages=3,minFirstStageLength=6,maxFirstStageLength=6,minStageExtra=1,maxStageExtra=3;
            bool allowSameConsecutive = false;
            // Version 2 /*/
            var criteria1 = Ut.NewArray(
                new { Name = "If the nth color is red", Criterion = Ut.Lambda((int[] seq, int[] rgb, int orange, int purple) => seq[0] == 0) },
                new { Name = "If the nth color is orange", Criterion = Ut.Lambda((int[] seq, int[] rgb, int orange, int purple) => seq[0] == 1) },
                new { Name = "If the nth color is yellow", Criterion = Ut.Lambda((int[] seq, int[] rgb, int orange, int purple) => seq[0] == 2) },
                new { Name = "If the nth color is green", Criterion = Ut.Lambda((int[] seq, int[] rgb, int orange, int purple) => seq[0] == 3) },
                new { Name = "If the nth color is blue", Criterion = Ut.Lambda((int[] seq, int[] rgb, int orange, int purple) => seq[0] == 4) },
                new { Name = "If the nth color is purple", Criterion = Ut.Lambda((int[] seq, int[] rgb, int orange, int purple) => seq[0] == 5) }
            );
            var criteria2 = Ut.NewArray(
                new { Name = "Otherwise, if three adjacent colors flashed in clockwise order", Criterion = Ut.Lambda((int[] seq, int[] rgb, int orange, int purple) => Enumerable.Range(0, seq.Length - 2).Any(ix => seq[ix + 1] == (seq[ix] + 1) % 6 && seq[ix + 2] == (seq[ix] + 2) % 6)) },
                new { Name = "Otherwise, if a color flashed, then an adjacent color, then the first again", Criterion = Ut.Lambda((int[] seq, int[] rgb, int orange, int purple) => Enumerable.Range(0, seq.Length - 2).Any(ix => seq[ix + 2] == seq[ix] && (seq[ix + 1] == (seq[ix] + 1) % 6 || seq[ix + 1] == (seq[ix] + 5) % 6))) },
                new { Name = "Otherwise, if at least two of red, yellow, and blue didn’t flash", Criterion = Ut.Lambda((int[] seq, int[] rgb, int orange, int purple) => rgb.Count(color => seq.Contains(color)) <= 1) },
                new { Name = "Otherwise, if there are two colors opposite each other that didn’t flash", Criterion = Ut.Lambda((int[] seq, int[] rgb, int orange, int purple) => Enumerable.Range(0, 3).Any(col => !seq.Contains(col) && !seq.Contains(col + 3))) },
                new { Name = "Otherwise, if two adjacent colors flashed in clockwise order", Criterion = Ut.Lambda((int[] seq, int[] rgb, int orange, int purple) => Enumerable.Range(0, seq.Length - 1).Any(ix => seq[ix + 1] == (seq[ix] + 1) % 6)) },
                new { Name = "Otherwise", Criterion = Ut.Lambda((int[] seq, int[] rgb, int orange, int purple) => true) }
            );
            int numStages = 3, minFirstStageLength = 3, maxFirstStageLength = 5, minStageExtra = 1, maxStageExtra = 2;
            bool allowSameConsecutive = false;
            /**/

            const int numIterations = 100000;

            var dic = new Dictionary<string, Dictionary<int, int>>();
            for (int i = 0; i < numIterations; i++)
            {
                var colors = Enumerable.Range(0, 6).ToList().Shuffle();
                var rgb = colors.Take(3).ToArray();
                var orange = colors[3];
                var purple = colors[4];
                var seqs = generateSequences(numStages, minFirstStageLength, maxFirstStageLength, minStageExtra, maxStageExtra, allowSameConsecutive);
                for (int seqIx = 0; seqIx < seqs.Length; seqIx++)
                {
                    var seq = seqs[seqIx];
                    string c1 = "Otherwise";
                    foreach (var cr in criteria1)
                        if (cr.Criterion(seq, rgb, orange, purple))
                        {
                            c1 = cr.Name;
                            break;
                        }

                    //*
                    string c2 = "Otherwise";
                    foreach (var cr in criteria2)
                        if (cr.Criterion(seq, rgb, orange, purple))
                        {
                            c2 = cr.Name;
                            break;
                        }
                    dic.IncSafe($"{c1}×{c2}", seqIx);
                    /*/
                    foreach (var cr in criteria2)
                        if (cr.Criterion(seq, rgb, orange, purple))
                            dic.IncSafe($"{c1}×{cr.Name}", seqIx);
                    /**/
                }
            }

            for (int stage = 0; stage < numStages; stage++)
            {
                var tt = new TextTable { ColumnSpacing = 3, RowSpacing = 1, HorizontalRules = true, VerticalRules = true, MaxWidth = 125 };
                tt.SetCell(0, 0, $"STAGE {stage + 1}".Color(ConsoleColor.White));
                var col = 1;
                foreach (var cri1 in criteria1)
                    tt.SetCell(col++, 0, cri1.Name.Color(ConsoleColor.Cyan));
                tt.SetCell(col++, 0, "Total".Color(ConsoleColor.White));

                var row = 1;
                foreach (var cri2 in criteria2)
                {
                    tt.SetCell(0, row, cri2.Name.Color(ConsoleColor.Cyan));
                    col = 1;
                    foreach (var cri1 in criteria1)
                        tt.SetCell(col++, row, dic.Get($"{cri1.Name}×{cri2.Name}", stage, 0).Apply(num => $"{(num / (double) numIterations) * 100:0.00}%".Color(num == 0 ? ConsoleColor.Red : ConsoleColor.Green)), alignment: HorizontalTextAlignment.Right);
                    tt.SetCell(col, row, criteria1.Sum(cri1 => dic.Get($"{cri1.Name}×{cri2.Name}", stage, 0)).Apply(num => $"{(num / (double) numIterations) * 100:0.00}%".Color(num == 0 ? ConsoleColor.Magenta : ConsoleColor.Yellow)), alignment: HorizontalTextAlignment.Right);
                    row++;
                }

                tt.SetCell(0, row, "Total".Color(ConsoleColor.Yellow));
                col = 1;
                foreach (var cri1 in criteria1)
                    tt.SetCell(col++, row, criteria2.Sum(cri2 => dic.Get($"{cri1.Name}×{cri2.Name}", stage, 0)).Apply(num => $"{(num / (double) numIterations) * 100:0.00}%".Color(num == 0 ? ConsoleColor.Magenta : ConsoleColor.Yellow)), alignment: HorizontalTextAlignment.Right);

                tt.WriteToConsole();
                ConsoleUtil.WriteLine(new string('═', 125).Color(ConsoleColor.White));
            }
        }

        private static int[][] generateSequences(int numStages, int minFirstStageLength, int maxFirstStageLength, int minStageExtra, int maxStageExtra, bool allowSameConsecutive)
        {
            var firstStageLength = Rnd.Next(minFirstStageLength, maxFirstStageLength + 1);
            var seq = generateSequence(firstStageLength + numStages * maxStageExtra, allowSameConsecutive).ToArray();
            var arr = new int[numStages][];
            var len = firstStageLength;
            for (int stage = 0; stage < numStages; stage++)
            {
                arr[stage] = seq.Subarray(0, len);
                len += Rnd.Next(minStageExtra, maxStageExtra + 1);
            }
            return arr;
        }

        private static IEnumerable<int> generateSequence(int num, bool allowSameConsecutive)
        {
            var last = Rnd.Next(6);
            yield return last;
            for (int i = 1; i < num; i++)
            {
                int next;
                if (allowSameConsecutive)
                    next = Rnd.Next(6);
                else
                {
                    next = Rnd.Next(5);
                    if (next >= last)
                        next++;
                }
                yield return next;
                last = next;
            }
        }
    }
}
