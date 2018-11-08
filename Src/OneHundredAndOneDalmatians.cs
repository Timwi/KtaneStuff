using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using KtaneStuff.Modeling;
using RT.Util.ExtensionMethods;

namespace KtaneStuff
{
    using static Md;

    static class OneHundredAndOneDalmatians
    {
        public static void DoGraphics()
        {
            var already = new HashSet<string>();
            var svgs = new List<string>();
            var rnd = new MonoRandom(1);
            var skip = rnd.Next(10);
            while (skip-- > 0)
                rnd.NextDouble();

            var number = 1;

            // Amount of variety
            var iv = 5;
            var v1 = 4;
            var v2 = 4;
            var v3 = 4;
            var v4 = 4;

            while (number < iv * v1 * v2 * v3 + iv * v1 * v4)
            {
                var stage2 = number >= iv * v1 * v2 * v3;
                var inner = number % iv;
                var c1 = (number / iv) % v1;
                var c2 = stage2 ? 0 : (number / (iv * v1)) % v2;
                var c3 = stage2 ? 0 : (number / (iv * v1 * v2)) % v3;
                var c4 = stage2 ? ((number - iv * v1 * v2 * v3) / (iv * v1)) % v4 : 0;
                number++;

                if (new[] { inner, c1, c2, c3, c4 }.Count(x => x != 0) < 3)
                    continue;

                var key = stage2 ? $"{inner},{c1},{c4}" : $"{inner},{c1},{c2},{c3}";
                if (already.Contains(key) ||
                    (stage2 ? already.Contains($"{inner},{c4},{c1}") : (already.Contains($"{inner},{c2},{c3},{c1}") || already.Contains($"{inner},{c3},{c1},{c2}"))))
                    continue;
                already.Add(key);

                var radiusDist = 5;
                var circles = new List<(double x, double y, int r)>();
                var rotation = 360 * rnd.NextDouble();
                if (inner != 0)
                    circles.Add((0, 0, inner));
                if (c1 != 0)
                    circles.Add((radiusDist * cos(0 + rotation), radiusDist * sin(0 + rotation), c1));
                if (c2 != 0)
                    circles.Add((radiusDist * cos(120 + rotation), radiusDist * sin(120 + rotation), c2));
                if (c3 != 0)
                    circles.Add((radiusDist * cos(240 + rotation), radiusDist * sin(240 + rotation), c3));
                if (c4 != 0)
                    circles.Add((radiusDist * cos(90 + rotation), radiusDist * sin(90 + rotation), c4));

                var mx = (circles.Min(c => c.x - c.r) + circles.Max(c => c.x + c.r)) / 2;
                var my = (circles.Min(c => c.y - c.r) + circles.Max(c => c.y + c.r)) / 2;

                svgs.Add($@"<svg xmlns='http://www.w3.org/2000/svg' viewBox='{mx-8} {my-8} 16 16'>{circles.Select(c => $"<circle cx='{c.x:0.###}' cy='{c.y:0.###}' r='{c.r}' />").JoinString()}</svg>");
            }
            rnd.ShuffleFisherYates(svgs);

            var names = File.ReadAllLines(@"D:\c\KTANE\KtaneStuff\DataFiles\101Dalmatians\Names.txt");
            var cols = 11;
            Utils.ReplaceInFile(@"D:\c\KTANE\Public\HTML\101 Dalmatians.html", "<!--%%-->", "<!--%%%-->",
                Enumerable.Range(0, (101 + cols - 1) / cols).Select(row => $@"<tr>{Enumerable.Range(0, cols)
                    .Select(col => col + cols * row >= 101 ? null : $"<td><div class='pattern'>{svgs[col + cols * row]}</div><div class='name'>{names[col + cols * row]}</div></td>")
                    .JoinString()}</tr>").JoinString(Environment.NewLine));
        }

        public static void FailedAttempt1()
        {
            var already = new HashSet<string>();
            var svgs = new List<string>();
            var rnd = new MonoRandom(1);
            var skip = rnd.Next(100);
            while (skip-- > 0)
                rnd.NextDouble();

            while (svgs.Count < 101)
            {
                var bx = 3 + rnd.Next(0, 3) - 1;
                var by = 3 + rnd.Next(0, 3) - 1;
                var br = (rnd.Next(0, 5) - 4) * 45;

                var cx = 0 + rnd.Next(0, 3) - 1;
                var cy = 6 + rnd.Next(0, 3) - 1;
                var cr = (rnd.Next(0, 5) - 2) * 45;

                var dx = -3 + rnd.Next(0, 3) - 1;
                var dy = 3 + rnd.Next(0, 3) - 1;
                var dr = rnd.Next(0, 5) * 45;

                var key = $"{bx},{by},{br},{cx},{cy},{cr},{dx},{dy},{dr}";
                if (already.Contains(key))
                    continue;

                var svg = $@"<svg xmlns='http://www.w3.org/2000/svg' viewBox='-4.5 -1.5 9 9'><path fill='#000' stroke='none' d='M0,0 C1,0 {bx + cos(br):0.###},{by + sin(br):0.###} {bx},{by} {bx - cos(br):0.###},{by - sin(br):0.###} {cx + cos(cr):0.###},{cy + sin(cr):0.###} {cx},{cy} {cx - cos(cr):0.###},{cy - sin(cr):0.###} {dx + cos(dr):0.###},{dy + sin(dr):0.###} {dx},{dy} {dx - cos(dr):0.###},{dy - sin(dr):0.###} -1,0 0,0z' /></svg>";
                already.Add(key);
                svgs.Add(svg);
            }

            var names = File.ReadAllLines(@"D:\c\KTANE\KtaneStuff\DataFiles\101Dalmatians\Names.txt");
            var cols = 11;
            Utils.ReplaceInFile(@"D:\c\KTANE\Public\HTML\101 Dalmatians.html", "<!--%%-->", "<!--%%%-->",
                Enumerable.Range(0, (101 + cols - 1) / cols).Select(row => $"<tr>{Enumerable.Range(0, cols).Select(col => col + cols * row >= 101 ? null : $"<td>{svgs[col + cols * row]}<div class='name'>{names[col + cols * row]}</div></td>").JoinString()}</tr>").JoinString(Environment.NewLine));
        }
    }
}