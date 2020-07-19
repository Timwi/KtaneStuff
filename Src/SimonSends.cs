using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using KtaneStuff.Modeling;
using RT.Util;
using RT.Util.Consoles;
using RT.Util.ExtensionMethods;
using RT.Util.Text;

namespace KtaneStuff
{
    using static Md;

    static class SimonSends
    {
        public static string[][] _text;
        static SimonSends()
        {
            var text = File.ReadAllText(@"D:\c\KTANE\Public\HTML\Simon Sends.html");
            var m = Regex.Match(text, @"(?<=<!-- start -->)(.*)(?=<!-- end -->)", RegexOptions.Singleline);
            var p = Regex.Replace(m.Groups[1].Value, @"</?(p|em)>", "").Trim().ToUpperInvariant();
            var q = p.Where(ch => ch == ' ' || ch == '\n' || (ch >= 'A' && ch <= 'Z')).JoinString();
            _text = q.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries).Select(line => line.Trim().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)).ToArray();
        }

        public static string[] _morse = ".-|-...|-.-.|-..|.|..-.|--.|....|..|.---|-.-|.-..|--|-.|---|.--.|--.-|.-.|...|-|..-|...-|.--|-..-|-.--|--..".Split('|');

        public static void DoModels()
        {
            File.WriteAllText($@"D:\c\KTANE\SimonSends\Assets\Models\Wire.obj", GenerateObjFile(Wire(), "Wire"));
            File.WriteAllText($@"D:\c\KTANE\SimonSends\Assets\Models\Knob.obj", GenerateObjFile(Knob(), "Knob"));
            File.WriteAllText($@"D:\c\KTANE\SimonSends\Assets\Models\KnobBase.obj", GenerateObjFile(LooseModels.Cylinder(0, -.05, .1, 36).Select(f => f.Select(v => v.RotateX(90)).ToArray()), "KnobBase"));
            File.WriteAllText($@"D:\c\KTANE\SimonSends\Assets\Models\KnobHighlight.obj", GenerateObjFile(LooseModels.Disc(36, reverse: true), "KnobHighlight"));

            var (button, highlight) = ButtonAndHighlight();
            File.WriteAllText($@"D:\c\KTANE\SimonSends\Assets\Models\Button.obj", GenerateObjFile(button, "Button"));
            File.WriteAllText($@"D:\c\KTANE\SimonSends\Assets\Models\ButtonHighlight.obj", GenerateObjFile(highlight, "ButtonHighlight"));
        }

        private static IEnumerable<VertexInfo[]> Knob()
        {
            var num = 45;
            return CreateMesh(true, false, Enumerable.Range(0, num).Select(i => Ut.NewArray(
                pt(0, .5, 0).WithMeshInfo(0, 1, 0),
                pt(.35, .5, 0).RotateY(i * 360 / num).WithMeshInfo(0, 1, 0),
                pt(i % 2 != 0 ? .5 : .55, .6, 0).RotateY(i * 360 / num).WithMeshInfo(Normal.Average, Normal.Average, Normal.Mine, Normal.Mine),
                pt(i % 2 != 0 ? .85 : .9, .1, 0).RotateY(i * 360 / num).WithMeshInfo(Normal.Mine, Normal.Mine, i == 0 ? Normal.Mine : Normal.Average, i == 0 ? Normal.Mine : Normal.Average),
                pt(i == 0 ? 1.25 : i % 2 != 0 ? .85 : .9, 0, 0).RotateY(i * 360 / num).WithMeshInfo(Normal.Mine, Normal.Mine, Normal.Mine, Normal.Mine)
            )).ToArray());
        }

        public static void GenerateData()
        {
            var freqs = new Dictionary<char, int>();
            foreach (var p in _text)
                foreach (var w in p)
                    foreach (var l in w)
                        freqs.IncSafe(l);
            ConsoleUtil.WriteParagraphs(freqs.OrderByDescending(kvp => kvp.Value).Select(kvp => $"{kvp.Key}={kvp.Value}").JoinString(", "));

            Console.WriteLine(new string('─', 20));

            var lists = _text.Select(line => line.Select(word => word.Select(x => new List<(int paraIx, int wordIx, int letterIx)>()).ToArray()).ToArray()).ToArray();
            var dic = new Dictionary<char, List<(int paraIx, int wordIx, int letterIx)>>();
            for (int p = 0; p < 26; p++)
                for (int w = 0; w < 26; w++)
                    if (w != p)
                        for (int l = 0; l < 26; l++)
                            if (l != p && l != w)
                            {
                                var (paraIx, wordIx, letterIx, ch) = getPosition(p, w, l);
                                lists[paraIx][wordIx][letterIx].Add((p, w, l));
                                dic.AddSafe(ch, (p, w, l));
                            }

            ConsoleUtil.WriteParagraphs(_text.Select((para, paraIx) => para.Select((word, wordIx) => word.Select((ch, chIx) =>
            {
                var num = lists[paraIx][wordIx][chIx].Count / 2;
                return ch.Color((ConsoleColor) (num % 16), num == 0 ? ConsoleColor.Red : (ConsoleColor) (num / 16));
            }).JoinColoredString()).JoinColoredString(" ")).JoinColoredString("\n"));
            //ConsoleUtil.WriteLine("{0} paragraphs. Unused letters = {1}".Color(null)
            //    .Fmt(_text.Length, lists.Sum(one => one.Sum(two => two.Count(three => three.Count == 0))).Apply(x => x.ToString().Color(x == 0 ? ConsoleColor.Green : ConsoleColor.Magenta))));

            Utils.ReplaceInFile(@"D:\c\KTANE\SimonSends\Assets\SimonSendsModule.cs", "/*MANUAL*/", "/*!MANUAL*/", $@"@""{_text.Select(para => para.JoinString(" ")).JoinString("|")}""");

            // How many times does each combo occur? Sum of the values is equal to iterations.
            var comboStats = new Dictionary<(string rgb, string morse), List<string>>();

            // How many times does each letter (A–Z) occur? Sum is greater than iterations.
            var letterStats = Ut.NewArray(3, _ => new Dictionary<char, int>());

            // How many times does each solution length occur?
            var lengthStats = new Dictionary<int, int>();

            for (int cmb = 0; cmb < 26 * 26 * 26; cmb++)
            {
                var r = cmb % 26;
                var g = (cmb / 26) % 26;
                var b = (cmb / 26 / 26) % 26;
                if (r == g || r == b || g == b)
                    continue;

                var (_, _, _, newR) = getPosition(b, g, r);
                var (_, _, _, newG) = getPosition(r, b, g);
                var (_, _, _, newB) = getPosition(g, r, b);

                var rMorse = constructMorse(newR);
                var gMorse = constructMorse(newG);
                var bMorse = constructMorse(newB);
                var combo = Enumerable.Range(0, Ut.Max(rMorse.Length, gMorse.Length, bMorse.Length))
                    .Select(i =>
                    {
                        var isR = i >= rMorse.Length ? false : rMorse[i] == '#';
                        var isG = i >= gMorse.Length ? false : gMorse[i] == '#';
                        var isB = i >= bMorse.Length ? false : bMorse[i] == '#';
                        return "KBGCRMYW"[(isB ? 1 : 0) + (isG ? 2 : 0) + (isR ? 4 : 0)];
                    })
                    .JoinString();
                comboStats.AddSafe(($"{newR}{newG}{newB}", combo), $"{(char) (r + 'A')}{(char) (g + 'A')}{(char) (b + 'A')}");
                letterStats[0].IncSafe(newR);
                letterStats[1].IncSafe(newG);
                letterStats[2].IncSafe(newB);
                lengthStats.IncSafe(combo.Length);
            }

            Console.WriteLine(new string('─', 20));
            ConsoleUtil.WriteLine("Most common answers:".Color(ConsoleColor.White));
            foreach (var kvp in comboStats.OrderByDescending(p => p.Value.Count).Take(10))
                Console.WriteLine($"{kvp.Key.rgb} = {kvp.Key.morse,13} = {100 * kvp.Value.Count / (double) (26 * 26 * 26):0.##}%");
            Console.WriteLine(new string('─', 20));
            ConsoleUtil.WriteLine("Most common answers per color channel:".Color(ConsoleColor.White));
            var tt = new TextTable { ColumnSpacing = 2 };
            for (int i = 0; i < 3; i++)
                tt.SetCell(i, 0, letterStats[i]
                    .OrderByDescending(p => p.Value)
                    .Select(kvp => "{0} = {1,6:0.00}% {2}".Color(null).Fmt(kvp.Key.Color(new[] { ConsoleColor.Red, ConsoleColor.Green, ConsoleColor.Blue }[i]), 100 * kvp.Value / (double) (26 * 26 * 26), new string('█', (200 * kvp.Value + (26 * 26 * 26) / 2) / (26 * 26 * 26))))
                    .JoinColoredString("\n")
                    .Replace(" ", "\u00a0"));
            tt.WriteToConsole();
            Console.WriteLine(new string('─', 20));
            ConsoleUtil.WriteLine("Distribution of answer lengths:".Color(ConsoleColor.White));
            foreach (var kvp in lengthStats.OrderBy(p => p.Key))
                Console.WriteLine($"{kvp.Key,2} = {100 * kvp.Value / (double) (26 * 26 * 26),6:0.00}% {new string('█', (100 * kvp.Value + (26 * 26 * 26) / 2) / (26 * 26 * 26))}");
            //Console.WriteLine("---");
            //foreach (var kvp in comboStats)
            //    if (kvp.Value.Count == 1)
            //        Console.WriteLine($"{kvp.Key} = {kvp.Value.JoinString(", ")}");
        }

        private static (int paraIx, int wordIx, int letterIx, char ch) getPosition(int paraCount, int wordCount, int letterCount)
        {
            var para = paraCount % _text.Length;
            var w = 0;
            for (int w2 = 0; w2 < wordCount; w2++)
            {
                w++;
                if (w >= _text[para].Length)
                {
                    w = 0;
                    para = (para + 1) % _text.Length;
                }
            }
            var l = 0;
            for (int l2 = 0; l2 < letterCount; l2++)
            {
                l++;
                if (l >= _text[para][w].Length)
                {
                    l = 0;
                    w++;
                    if (w >= _text[para].Length)
                    {
                        w = 0;
                        para = (para + 1) % _text.Length;
                    }
                }
            }
            return (para, w, l, _text[para][w][l]);
        }

        private static string constructMorse(char letter) => _morse[letter - 'A'].Select(ch => ch == '.' ? "#" : "###").JoinString(" ");

        private static IEnumerable<VertexInfo[]> Wire()
        {
            var origPoints = new[] { pt(-.7, .15, -.35), pt(-1.065, .15, -.35), pt(-1.065, .075, -.35), pt(-1, 0, -.35), pt(-1, 0, -.6 - .15), pt(-.5, 0, -.6 - .15), pt(.7, 0, 0 - .2), pt(1, 0, 0 - .2), pt(1, 0, -.475), pt(1, .074, -.575), pt(1, .11, -.58), pt(.7, .11, -.58) }.Select(p => -p).ToArray();

            var newPoints = new List<Pt> { origPoints[0] };
            for (int i = 1; i < origPoints.Length - 1; i++)
            {
                const double f = .1;
                var a = origPoints[i] - Math.Min(f, (origPoints[i] - origPoints[i - 1]).Length / 2) * (origPoints[i] - origPoints[i - 1]).Normalize();
                var b = origPoints[i];
                var c = origPoints[i] - Math.Min(f, (origPoints[i] - origPoints[i + 1]).Length / 2) * (origPoints[i] - origPoints[i + 1]).Normalize();
                newPoints.AddRange(Bézier(a, b, b, c, 8));
            }
            newPoints.Add(origPoints[origPoints.Length - 1]);
            return TubeFromCurve(newPoints, .02, 36);
        }

        private static (IEnumerable<VertexInfo[]> button, IEnumerable<VertexInfo[]> highlight) ButtonAndHighlight()
        {
            const int steps = 8;
            const double bf = .45;
            const Normal n = Normal.Average;

            var data = new (double width, double depth, double bevel, Normal? normal)[] { (.6, .5, .19, null), (.85, .4, .28, n), (.95, .3, .33, n), (1, .17, .34, n), (1, 0, .35, Normal.Mine) };
            MeshVertexInfo[] rect2((double width, double depth, double bevel, Normal? normal) x)
            {
                var pts = new List<Pt>();
                pts.AddRange(Bézier(pt(-x.width + x.bevel, x.depth, -x.width), pt(-x.width + bf * x.bevel, x.depth, -x.width), pt(-x.width, x.depth, -x.width + bf * x.bevel), pt(-x.width, x.depth, -x.width + x.bevel), steps));
                pts.AddRange(Bézier(pt(-x.width, x.depth, x.width - x.bevel), pt(-x.width, x.depth, x.width - bf * x.bevel), pt(-x.width + bf * x.bevel, x.depth, x.width), pt(-x.width + x.bevel, x.depth, x.width), steps));
                pts.AddRange(Bézier(pt(x.width - x.bevel, x.depth, x.width), pt(x.width - bf * x.bevel, x.depth, x.width), pt(x.width, x.depth, x.width - bf * x.bevel), pt(x.width, x.depth, x.width - x.bevel), steps));
                pts.AddRange(Bézier(pt(x.width, x.depth, -x.width + x.bevel), pt(x.width, x.depth, -x.width + bf * x.bevel), pt(x.width - bf * x.bevel, x.depth, -x.width), pt(x.width - x.bevel, x.depth, -x.width), steps));
                return pts.Select(v => x.normal == null ? v.WithMeshInfo(0, 1, 0) : v.WithMeshInfo(x.normal.Value, x.normal.Value, n, n)).ToArray();
            }
            return (
                // Button
                CreateMesh(false, true, data.Select(rect2).ToArray()).Concat(rect2(data[0]).Select(m => m.Location.WithNormal(0, 1, 0)).ToArray()),
                // Highlight
                new[] { rect2(data.Last()).Select(m => m.Location.WithNormal(0, 1, 0)).Reverse().ToArray() }
            );
        }

        public static void MakeCheatSheet()
        {
            Utils.ReplaceInFile(@"D:\c\KTANE\Public\HTML\Simon Sends embellished (Timwi).html", "<!-- start -->", "<!-- end -->",
                $"<div class='simon-sends-text'>{_text.Select((para, ix) => $"<p class='para{(ix >= 5 && ix <= 7 ? $" para-{ix + 1}" : "")}'>{para.Select((word, wix) => $"<span class='word{(ix >= 5 && ix <= 7 && wix == 25 ? $" word-26" : "")}'>{word}<span class='len'>{word.Length}</span></span>").JoinString(" ")}</p>").JoinString("\n")}</div>");
        }
    }
}