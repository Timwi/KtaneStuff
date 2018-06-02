using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using KtaneStuff.Modeling;
using RT.Util.ExtensionMethods;
using RT.Util.Geometry;

namespace KtaneStuff
{
    using static Md;

    static class SimonShrieks
    {
        public static void DoModels()
        {
            File.WriteAllText($@"D:\c\KTANE\SimonShrieks\Assets\Models\Button.obj", GenerateObjFile(Button(), "Button"));
            File.WriteAllText($@"D:\c\KTANE\SimonShrieks\Assets\Models\ButtonHighlight.obj", GenerateObjFile(ButtonHighlight(), "ButtonHighlight"));
            File.WriteAllText($@"D:\c\KTANE\SimonShrieks\Assets\Models\ButtonCollider.obj", GenerateObjFile(ButtonCollider(), "ButtonCollider"));
            File.WriteAllText($@"D:\c\KTANE\SimonShrieks\Assets\Models\Arrow1.obj", GenerateObjFile(Arrow1(), "Arrow1"));
            File.WriteAllText($@"D:\c\KTANE\SimonShrieks\Assets\Models\Arrow2.obj", GenerateObjFile(Arrow2(), "Arrow2"));
            File.WriteAllText($@"D:\c\KTANE\SimonShrieks\Assets\Models\Arrow3.obj", GenerateObjFile(Arrow3(), "Arrow3"));
        }

        private static IEnumerable<VertexInfo[]> Arrow1()
        {
            yield return new[] { pt(-.5, 0, .75), pt(0, 0, 1.75), pt(.5, 0, .75) }.Select(p => p.WithNormal(0, 1, 0)).ToArray();
        }

        private static IEnumerable<VertexInfo[]> Arrow2()
        {
            yield return new[] { pt(-.5, 0, .65), pt(0, 0, 1.25), pt(.5, 0, .65) }.Select(p => p.WithNormal(0, 1, 0)).ToArray();
            yield return new[] { pt(-.5, 0, 1.25), pt(0, 0, 1.85), pt(.5, 0, 1.25) }.Select(p => p.WithNormal(0, 1, 0)).ToArray();
        }

        private static IEnumerable<VertexInfo[]> Arrow3()
        {
            yield return new[] { pt(-.5, 0, .45), pt(0, 0, .95), pt(.5, 0, .45) }.Select(p => p.WithNormal(0, 1, 0)).ToArray();
            yield return new[] { pt(-.5, 0, .95), pt(0, 0, 1.45), pt(.5, 0, .95) }.Select(p => p.WithNormal(0, 1, 0)).ToArray();
            yield return new[] { pt(-.5, 0, 1.45), pt(0, 0, 1.95), pt(.5, 0, 1.45) }.Select(p => p.WithNormal(0, 1, 0)).ToArray();
        }

        private static Pt offset(Pt p) => p + .4 * pt(-cos(360.0 / 7 / 2), 0, sin(360.0 / 7 / 2));

        private static IEnumerable<VertexInfo[]> Button()
        {
            const int steps = 8;

            var v1 = pt(0, 0, 0);
            var v2 = pt(0, 0, .5);
            var v3 = pt(0, 0, 1);
            var angle = 360.0 / 7.0;
            var h = 1.15;
            var v4 = pt(h * cos(90 - angle), 0, h * sin(90 - angle));
            var c = ((v1 + v3 + v4) / 3).Add(y: .5);

            var patchRaw = BézierPatch(
                v1, (2 * v1 + v2) / 3, (v1 + 2 * v2) / 3, v2,
                (2 * v1 + v4) / 3, (c + v1) / 2, (c + v2) / 2, (2 * v2 + v3) / 3,
                (v1 + 2 * v4) / 3, (c + v4) / 2, (c + v3) / 2, (v2 + 2 * v3) / 3,
                v4, (2 * v4 + v3) / 3, (v4 + 2 * v3) / 3, v3,
                steps
            );
            var patch = patchRaw
                .Select((arr, x) => arr.Select((p, y) => offset(p).WithMeshInfo(
                    x == 0 || x == patchRaw.Length - 1 ? Normal.Mine : Normal.Average,
                    x == 0 || x == patchRaw.Length - 1 ? Normal.Mine : Normal.Average,
                    y == 0 || y == patchRaw[0].Length - 1 ? Normal.Mine : Normal.Average,
                    y == 0 || y == patchRaw[0].Length - 1 ? Normal.Mine : Normal.Average)).ToArray()).Reverse().ToArray();

            return CreateMesh(false, false, patch);
        }

        private static IEnumerable<Pt[]> ButtonHighlight()
        {
            var v1 = pt(0, 0, 0);
            var v2 = pt(0, 0, .5);
            var v3 = pt(0, 0, 1);
            var angle = 360.0 / 7.0;
            var h = 1.15;
            var v4 = pt(h * cos(90 - angle), 0, h * sin(90 - angle));
            var c = ((v1 + v3 + v4) / 3);

            yield return new[] { v1, v4, v3 }.Select(p => offset((p - c) * 1.25 + c)).ToArray();
        }

        private static IEnumerable<VertexInfo[]> ButtonCollider()
        {
            PointD offset(PointD pt) => pt + .4 * p(-cos(360.0 / 7 / 2), sin(360.0 / 7 / 2));

            var v1 = p(0, 0);
            var v3 = p(0, 1);
            var angle = 360.0 / 7.0;
            var h = 1.15;
            var v4 = p(h * cos(90 - angle), h * sin(90 - angle));
            var c = ((v1 + v3 + v4) / 3);

            return new[] { v1, v4, v3 }.Select(p => offset(p)).Extrude(.25, true, true);
        }

        public static void CreateCheatSheet()
        {
            string[] _grid = new[]
            {
                "GMCBYRCYBWR",
                "GWCWMYRWWRC",
                "YBWGGCWBRWM",
                "BRMYCRYGMBR",
                "WCYBGBRWGYC",
                "GYRCMRMGWRB",
                "YMGCMMGBCMW",
                "MBRYGYBWYRW",
                "RCYBCBGRCBM",
                "YYMGBMCYWCW",
                "WCGMRGCMGBB"
            };
            var colorNames = "RYGCBWM";

            var table = new StringBuilder();

            for (int cx = 2; cx <= 8; cx++)
                for (int cy = 2; cy <= 8; cy++)
                {
                    var countColors = new int[7];
                    var firstOccurrence = new int[7];
                    var cells = 25;
                    for (int y = 2; y >= -2; y--)
                        for (int x = 2; x >= -2; x--)
                        {
                            countColors[colorNames.IndexOf(_grid[cy + y][cx + x])]++;
                            firstOccurrence[colorNames.IndexOf(_grid[cy + y][cx + x])] = cells;
                            cells--;
                        }

                    string answer(bool hasVowel)
                    {
                        var colorsToPress = Enumerable.Range(0, 7).Where(ix => countColors[ix] % 2 == (hasVowel ? 0 : 1)).ToArray();
                        Array.Sort(colorsToPress, (v1, v2) =>
                            countColors[v1] > countColors[v2] ? 1 :
                            countColors[v1] < countColors[v2] ? -1 :
                            firstOccurrence[v1] > firstOccurrence[v2] ? 1 :
                            firstOccurrence[v1] < firstOccurrence[v2] ? -1 : 0);
                        return colorsToPress.Select(c => colorNames[c]).JoinString(" ");
                    }

                    table.AppendLine($@"<tr><th>({cx}, {cy})</th><td>{answer(true)}</td><td>{answer(false)}</td></tr>");
                }

            Utils.ReplaceInFile(@"D:\c\KTANE\Public\HTML\Simon Shrieks optimized (Timwi).html", "<!-- #start -->", "<!-- #end -->", table.ToString());
        }
    }
}