using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using RT.Util;
using RT.Util.ExtensionMethods;
using RT.Util.Geometry;

namespace KtaneStuff.Modeling
{
    using static Md;

    static class AdjacentLetters
    {
        struct Stuff
        {
            public IEnumerable<VertexInfo[]> Button;
            public IEnumerable<Pt[]> Highlight;
            public PointD[] Outline;
        }

        public static void Do()
        {
            var buttonStuff = Button();
            File.WriteAllText(@"D:\c\KTANE\AdjacentLetters\Assets\Models\Button.obj", GenerateObjFile(buttonStuff.Button, "Button"));
            File.WriteAllText(@"D:\c\KTANE\AdjacentLetters\Assets\Models\ButtonHighlight.obj", GenerateObjFile(buttonStuff.Highlight, "ButtonHighlight"));

            var submitButtonStuff = SubmitButton();
            File.WriteAllText(@"D:\c\KTANE\AdjacentLetters\Assets\Models\SubmitButton.obj", GenerateObjFile(submitButtonStuff.Button, "SubmitButton"));
            File.WriteAllText(@"D:\c\KTANE\AdjacentLetters\Assets\Models\SubmitButtonHighlight.obj", GenerateObjFile(submitButtonStuff.Highlight, "SubmitButtonHighlight"));

            var path = @"D:\c\KTANE\AdjacentLetters\Manual\img\Component\Adjacent Letters.svg";
            File.WriteAllText(path, Regex.Replace(File.ReadAllText(path), @"(?<=<!--##-->).*(?=<!--###-->)", $@"<path d='M{buttonStuff.Outline.Select((p, i) => $"{(i == 1 ? "L" : "")}{p.X * 500},{p.Y * 500}").JoinString(" ")} z' fill='none' stroke='#000' stroke-width='1' />", RegexOptions.Singleline));
        }

        public static void GenerateRandomTable()
        {
            var width = 10;
            string[] _leftRight, _aboveBelow;
            var rnd = new Random(1);
            tryAgain:
            try
            {
                var chars = Ut.NewArray<char>(size1: 26, size2: width);
                for (int j = 0; j < width; j++)
                {
                    var available = Enumerable.Range(0, 26).Select(i => (char) ('A' + i)).ToList();
                    for (int i = 0; i < 23; i++)
                    {
                        chars[i][j] = available.Where(ch => ch != 'A' + i).Except(Enumerable.Range(0, j).Select(jj => chars[i][jj])).PickRandom(rnd);
                        available.Remove(chars[i][j]);
                    }
                    Ut.Assert(available.Count == 3);
                    var perm = available.Permutations().Where(p => p.Select((ch, ix) => ch != 'A' + 23 + ix && !Enumerable.Range(0, j).Any(jj => chars[23 + ix][jj] == ch)).All(b => b)).PickRandom(rnd).ToArray();
                    chars[23][j] = perm[0];
                    chars[24][j] = perm[1];
                    chars[25][j] = perm[2];
                }

                _leftRight = Enumerable.Range(0, 26).Select(ch => Enumerable.Range(0, width / 2).Select(j => chars[ch][j]).Order().JoinString()).ToArray();
                _aboveBelow = Enumerable.Range(0, 26).Select(ch => Enumerable.Range(width / 2, width / 2).Select(j => chars[ch][j]).Order().JoinString()).ToArray();
                var str = Ut.Lambda((int ch) => "<th>{0}<td>{1}<td>{2}".Fmt((char) ('A' + ch), _leftRight[ch], _aboveBelow[ch]));
                var strs1 = Enumerable.Range(0, 13).Select(str);
                var strs2 = Enumerable.Range(13, 13).Select(str);
                Console.WriteLine(strs1.Zip(strs2, (s1, s2) => "                <tr>{0}{1}</tr>".Fmt(s1, s2)).JoinString(Environment.NewLine));

                str = Ut.Lambda((int ch) => "{0} | {1} | {2}".Fmt((char) ('A' + ch), _leftRight[ch], _aboveBelow[ch]));
                strs1 = Enumerable.Range(0, 13).Select(str);
                strs2 = Enumerable.Range(13, 13).Select(str);
                var allStr = strs1.Zip(strs2, (s1, s2) => "{0} || {1}".Fmt(s1, s2)).JoinString(Environment.NewLine);
                Console.WriteLine(allStr);
            }
            catch
            {
                goto tryAgain;
            }

            var counts = new Dictionary<int, int>();
            for (int iter = 0; iter < 100; iter++)
            {
                var letters = Enumerable.Range(0, 26).Select(i => (char) ('A' + i)).ToList().Shuffle().Take(12).ToArray();
                var expectation = Ut.NewArray(12, i =>
                {
                    var x = i % 4;
                    var y = i / 4;
                    if ((x > 0 && _leftRight[letters[i] - 'A'].Contains(letters[i - 1]) || (x < 3 && _leftRight[letters[i] - 'A'].Contains(letters[i + 1]))))
                        return true;
                    if ((y > 0 && _aboveBelow[letters[i] - 'A'].Contains(letters[i - 4]) || (y < 2 && _aboveBelow[letters[i] - 'A'].Contains(letters[i + 4]))))
                        return true;
                    return false;
                });
                counts.IncSafe(expectation.Count(b => b));
            }
            foreach (var kvp in counts.OrderBy(k => k.Key))
                Console.WriteLine($"{kvp.Key} = {kvp.Value}");
        }

        private static Stuff Button()
        {
            var totalHeight = .05;
            var bevelHeight = .01;
            var totalWidth = .099;
            var bevelWidth = .01;
            var innerRounding = .005;
            var outerRounding = .02;

            var capHeight = totalHeight - bevelHeight;
            var width = totalWidth / 2 - bevelWidth;
            var innerF = innerRounding * .8;
            var bevelF = bevelWidth * .6;
            var outerF = outerRounding * .3;

            var bézierSteps = 10;

            var xy =
                Bézier(p(width - innerRounding, capHeight), p(width - innerRounding + innerF, capHeight), p(width, capHeight + innerRounding - innerF), p(width, capHeight + innerRounding), bézierSteps)
                    .Concat(Bézier(p(width, capHeight + bevelHeight - bevelWidth / 2), p(width, capHeight + bevelHeight - (bevelWidth - bevelF) / 2), p(width + (bevelWidth - bevelF) / 2, capHeight + bevelHeight), p(width + bevelWidth / 2, capHeight + bevelHeight), bézierSteps))
                    .Concat(Bézier(p(width + bevelWidth / 2, capHeight + bevelHeight), p(width + (bevelWidth + bevelF) / 2, capHeight + bevelHeight), p(width + bevelWidth, capHeight + bevelHeight - (bevelWidth - bevelF) / 2), p(width + bevelWidth, capHeight), bézierSteps))
                    .Concat(p(width + bevelWidth, 0))
                    .RemoveConsecutiveDuplicates(false)
                    .ToArray();

            var arr = xy.Select(q =>
                Bézier(pt(q.X, q.Y, totalWidth / 2 - outerRounding), pt(q.X, q.Y, totalWidth / 2 - outerRounding + outerF), pt(totalWidth / 2 - outerRounding + outerF, q.Y, q.X), pt(totalWidth / 2 - outerRounding, q.Y, q.X), bézierSteps)
                    .Concat(Bézier(pt(-totalWidth / 2 + outerRounding, q.Y, q.X), pt(-totalWidth / 2 + outerRounding - outerF, q.Y, q.X), pt(-q.X, q.Y, totalWidth / 2 - outerRounding + outerF), pt(-q.X, q.Y, totalWidth / 2 - outerRounding), bézierSteps))
                    .Concat(Bézier(pt(-q.X, q.Y, -totalWidth / 2 + outerRounding), pt(-q.X, q.Y, -totalWidth / 2 + outerRounding - outerF), pt(-totalWidth / 2 + outerRounding - outerF, q.Y, -q.X), pt(-totalWidth / 2 + outerRounding, q.Y, -q.X), bézierSteps))
                    .Concat(Bézier(pt(totalWidth / 2 - outerRounding, q.Y, -q.X), pt(totalWidth / 2 - outerRounding + outerF, q.Y, -q.X), pt(q.X, q.Y, -totalWidth / 2 + outerRounding - outerF), pt(q.X, q.Y, -totalWidth / 2 + outerRounding), bézierSteps))
                    .Reverse()
                    .ToArray()
                ).ToArray();

            return new Stuff
            {
                Button = CreateMesh(false, true, Ut.NewArray(arr.Length, arr[0].Length, (x, y) => x == 0
                    ? new MeshVertexInfo(arr[x][y], pt(0, 1, 0))
                    : new MeshVertexInfo(arr[x][y], x == arr.Length - 1 ? Normal.Mine : Normal.Average, Normal.Average, Normal.Average, Normal.Average)))
                    .Concat(new[] { pt(-1, 0, -1), pt(-1, 0, 1), pt(1, 0, 1), pt(1, 0, -1) }.Select(p => new VertexInfo(p * width + pt(0, capHeight, 0), pt(0, 1, 0))).ToArray()),
                Highlight = arr.Last().SelectConsecutivePairs(true, (p1, p2) => new[] { pt(0, 0, 0), p2, p1 }).ToArray(),
                Outline = arr.Last().Select(pt => p(pt.X, pt.Z)).ToArray()
            };
        }

        private static Stuff SubmitButton()
        {
            var height = .03;
            var xWidth = .299;
            var yWidth = .099;
            var bevelWidth = .03;
            var outerRounding = .01;

            var bevelF = bevelWidth * .6;
            var outerF = outerRounding * .9;

            var bézierSteps = 10;

            var xy =
                Bézier(p(0, height), p(bevelF, height), p(bevelWidth, height - bevelF), p(bevelWidth, height - bevelWidth), bézierSteps)
                    .RemoveConsecutiveDuplicates(false)
                    .ToArray();

            var arr = xy.Select(q =>
                Bézier(pt(xWidth / 2 + q.X, q.Y, yWidth / 2 - outerRounding), pt(xWidth / 2 + q.X, q.Y, yWidth / 2 - outerRounding + outerF), pt(xWidth / 2 - outerRounding + outerF, q.Y, yWidth / 2 + q.X), pt(xWidth / 2 - outerRounding, q.Y, yWidth / 2 + q.X), bézierSteps)
                    .Concat(Bézier(pt(-xWidth / 2 + outerRounding, q.Y, yWidth / 2 + q.X), pt(-xWidth / 2 + outerRounding - outerF, q.Y, yWidth / 2 + q.X), pt(-xWidth / 2 - q.X, q.Y, yWidth / 2 - outerRounding + outerF), pt(-xWidth / 2 - q.X, q.Y, yWidth / 2 - outerRounding), bézierSteps))
                    .Concat(Bézier(pt(-xWidth / 2 - q.X, q.Y, -yWidth / 2 + outerRounding), pt(-xWidth / 2 - q.X, q.Y, -yWidth / 2 + outerRounding - outerF), pt(-xWidth / 2 + outerRounding - outerF, q.Y, -yWidth / 2 - q.X), pt(-xWidth / 2 + outerRounding, q.Y, -yWidth / 2 - q.X), bézierSteps))
                    .Concat(Bézier(pt(xWidth / 2 - outerRounding, q.Y, -yWidth / 2 - q.X), pt(xWidth / 2 - outerRounding + outerF, q.Y, -yWidth / 2 - q.X), pt(xWidth / 2 + q.X, q.Y, -yWidth / 2 + outerRounding - outerF), pt(xWidth / 2 + q.X, q.Y, -yWidth / 2 + outerRounding), bézierSteps))
                    .Reverse()
                    .ToArray()
                ).ToArray();

            return new Stuff
            {
                Button = CreateMesh(false, true, Ut.NewArray(arr.Length, arr[0].Length, (x, y) => x == 0
                    ? new MeshVertexInfo(arr[x][y], pt(0, 1, 0))
                    : new MeshVertexInfo(arr[x][y], x == arr.Length - 1 ? Normal.Mine : Normal.Average, Normal.Average, Normal.Average, Normal.Average)))
                    .Concat(new[] { p(-1, -1), p(-1, 1), p(1, 1), p(1, -1) }.Select(p => new VertexInfo(pt(p.X * xWidth / 2, height, p.Y * yWidth / 2), pt(0, 1, 0))).ToArray()),
                Highlight = arr.Last().SelectConsecutivePairs(true, (p1, p2) => new[] { pt(0, 0, 0), p2, p1 }).ToArray(),
                Outline = arr.Last().Select(pt => p(pt.X, pt.Z)).ToArray()
            };
        }

        private static IEnumerable<VertexInfo[]> ButtonHighlight()
        {
            var width = .11;
            var innerWidth = .09;
            return CreateMesh(false, true, Ut.NewArray(2, 4, (i, j) => (j * 90 + 45).Apply(angle => (i == 0 ? innerWidth : width).Apply(radius => pt(radius * cos(angle), 0, radius * sin(angle))))));
        }
    }
}
