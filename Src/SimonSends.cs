using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using KtaneStuff.Modeling;
using RT.Util.Consoles;
using RT.Util.ExtensionMethods;

namespace KtaneStuff
{
    using RT.Util;
    using static Md;

    static class SimonSends
    {
        public static string[][] _text = @"This is the “first” word for the purposes of counting words and paragraphs in this text.

Hyphenated and apostrophized words equate as just one word; punctuation marks do not count as letters.

The module contains three colorized LEDs (red, green and blue) which flash three unique letters in Morse code simultaneously.

However, due to their proximity, the lit LEDs’ light mixes according to additive color mixing.

Acquire the letters by disjoining the colors, then convert each letter into a number by using its alphabetic position. Call these numbers R, G and B.

Construct a new color sequence by combining (using the same additive color mixing) a red, green and blue Morse code letter determined as follows:

The R’th letter from the start of the G’th word from the start of the B’th paragraph on this page is the new red letter.

The G’th letter from the start of the B’th word from the start of the R’th paragraph on this page is the new green letter.

The B’th letter from the start of the R’th word from the start of the G’th paragraph on this page is the new blue letter.

The size of a dot in Morse code is one unit. A dash is three. The gap between dashes and dots is one unit in size.

Enter the result using the eight colored buttons.

A mistake results in an immediate strike. Continue entering the code where you left off.

Jump back to the “first” word if while counting letters you advance beyond the “last” word, which is this. ".Trim().ToUpperInvariant().Where(ch => ch == ' ' || ch == '\n' || (ch >= 'A' && ch <= 'Z')).JoinString().Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries).Select(line => line.Split(' ')).ToArray();
        public static string[] _morse = ".-|-...|-.-.|-..|.|..-.|--.|....|..|.---|-.-|.-..|--|-.|---|.--.|--.-|.-.|...|-|..-|...-|.--|-..-|-.--|--..".Split('|');

        public static void DoModels()
        {
            File.WriteAllText($@"D:\c\KTANE\SimonSends\Assets\Models\Wire.obj", GenerateObjFile(Wire(), "Wire"));

            var (button, highlight) = ButtonAndHighlight();
            File.WriteAllText($@"D:\c\KTANE\SimonSends\Assets\Models\Button.obj", GenerateObjFile(button, "Button"));
            File.WriteAllText($@"D:\c\KTANE\SimonSends\Assets\Models\ButtonHighlight.obj", GenerateObjFile(highlight, "ButtonHighlight"));
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
            Console.WriteLine(freqs.Count);

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
            Console.WriteLine($"All letters covered = {lists.All(one => one.All(two => two.All(three => three.Count > 0)))}");

            var possible = new bool[26, 26, 26];
            var possibleCount = 0;
            for (char r = 'A'; r <= 'Z'; r++)
            {
                var poss = new HashSet<string>();
                for (char g = 'A'; g <= 'Z'; g++)
                    for (char b = 'A'; b <= 'Z'; b++)
                    {
                        // Looking for R,G,B such that
                        // (B, G, R) = r
                        // (R, B, G) = g
                        // (G, R, B) = b
                        var isPossible = dic[r]
                            .Where(tup => dic[g].Any(gtup => gtup.paraIx == tup.letterIx && gtup.wordIx == tup.paraIx && gtup.letterIx == tup.wordIx))
                            .Where(tup => dic[b].Any(btup => btup.paraIx == tup.wordIx && btup.wordIx == tup.letterIx && btup.letterIx == tup.paraIx))
                            .Any();
                        if (isPossible)
                        {
                            possible[r - 'A', g - 'A', b - 'A'] = true;
                            poss.Add(g + "" + b);
                            possibleCount++;
                        }
                    }
                ConsoleUtil.Write("{1,3/Green}/{2,3/Magenta}={0/White}".Color(null).Fmt(r, poss.Count, 26 * 26 - poss.Count));
                if (poss.Count < 100)
                    ConsoleUtil.Write($" ({poss.JoinString("/")})".Color(ConsoleColor.DarkGray));
                Console.WriteLine();
            }
            Console.WriteLine();
            Console.WriteLine($"Possible combinations: {possibleCount}");
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

        public static void DoStatistics()
        {
            const int iterations = 1000000;

            var stats = new Dictionary<string, int>();
            for (int iter = 0; iter < iterations; iter++)
            {
                var r = Rnd.Next(0, 26);
                var g = Rnd.Next(0, 26);
                var b = Rnd.Next(0, 26);
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
                stats.IncSafe(combo);
            }

            Console.WriteLine(stats.Count);
            foreach (var kvp in stats.OrderByDescending(p => p.Value).Take(20))
                Console.WriteLine($"{kvp.Key,13} = {100 * kvp.Value / (double) stats.Count:0.##}%");
        }

        private static IEnumerable<VertexInfo[]> Wire()
        {
            var origPoints = new[] { pt(-.7, .15, -.35), pt(-1.065, .15, -.35), pt(-1.065, .075, -.35), pt(-1, 0, -.35), pt(-1, 0, -.6 - .15), pt(-.6, 0, -.6 - .15), pt(.6, 0, 0 - .2), pt(1, 0, 0 - .2), pt(1, 0, -.475), pt(1, .074, -.575), pt(1, .11, -.58), pt(.7, .11, -.58) }.Select(p => -p).ToArray();

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

            var data = new(double width, double depth, double bevel, Normal? normal)[] { (.6, .5, .19, null), (.85, .4, .28, n), (.95, .3, .33, n), (1, .17, .34, n), (1, 0, .35, Normal.Mine) };
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
    }
}