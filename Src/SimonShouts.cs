using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using KtaneStuff.Modeling;
using RT.KitchenSink;
using RT.Util;
using RT.Util.ExtensionMethods;
using RT.Util.Geometry;

namespace KtaneStuff
{
    using static Md;

    static class SimonShouts
    {
        static string _grid = @"RRGRRRGRYRBRYRBRRRRGRBRYRGRRRBRYRGGGRGGGYGBGYGBGGRGGGBGYGGGRGBGYRRGRRRGRYRBRYRBRBRBGBBBYBGBRBBBYRGGGRGGGYGBGYGBGYRYGYBYYYGYRYBYYRYGYRYGYYYBYYYBYGRGGGBGYGGGRGBGYRBGBRBGBYBBBYBBBRRRGRBRYRGRRRBRYRYGYRYGYYYBYYYBYBRBGBBBYBGBRBBBYRBGBRBGBYBBBYBBBYRYGYBYYYGYRYBYY";

        public static void Experiment()
        {
            var seed = Rnd.Next();
            Console.WriteLine($"Seed: {seed}");
            var rnd = new Random(seed);

            tryAgain:
            var _curPosition = rnd.Next(0, _grid.Length);
            var _goalPosition = Enumerable.Range(0, _grid.Length).Except(new[] { _curPosition }).PickRandom(rnd);
            var valid = Ut.NewArray(256, pos => pos == _goalPosition || (
                _grid[_goalPosition] != _grid[pos] &&
                _grid[(_goalPosition + 1) % 16 + 16 * (_goalPosition / 16)] != _grid[(pos + 1) % 16 + 16 * (pos / 16)] &&
                _grid[_goalPosition % 16 + 16 * ((_goalPosition / 16 + 1) % 16)] != _grid[pos % 16 + 16 * ((pos / 16 + 1) % 16)] &&
                _grid[(_goalPosition + 1) % 16 + 16 * ((_goalPosition / 16 + 1) % 16)] != _grid[(pos + 1) % 16 + 16 * ((pos / 16 + 1) % 16)]));

            var _flashingLetters = "ABCDFGHIJKLMNOPQRSUVWXYZ".ToCharArray().Shuffle(rnd).Take(4).Select(ch => ch - 'A').ToArray();
            var availableMovements = _flashingLetters.Select(ltr => new Movement(ltr)).ToArray();

            var _optimalMovements = new Dictionary<int, Movement>();
            var q = new Queue<int>();
            q.Enqueue(_goalPosition);
            while (q.Count > 0)
            {
                var pos = q.Dequeue();
                foreach (var mv in availableMovements)
                {
                    var n = pos - mv;
                    if (n != _goalPosition && valid[n] && !_optimalMovements.ContainsKey(n))
                    {
                        _optimalMovements[n] = mv;
                        q.Enqueue(n);
                    }
                }
            }

            if (!_optimalMovements.ContainsKey(_curPosition))
                goto tryAgain;

            var solutionPath = getSolutionPath(_curPosition, _goalPosition, _optimalMovements);
            if (solutionPath.Length < 3 || solutionPath.Length > 5)
                goto tryAgain;

            var _moduleId = 0;
            Console.WriteLine(@"[Simon Shouts #{0}] Start: {1} ({2}, {3})", _moduleId, positionToGridColors(_curPosition), (char) ('A' + _curPosition % 16), _curPosition / 16 + 1);
            Console.WriteLine(@"[Simon Shouts #{0}] Goal: {1} ({2}, {3})", _moduleId, positionToGridColors(_goalPosition), (char) ('A' + _goalPosition % 16), _goalPosition / 16 + 1);
            Console.WriteLine(@"[Simon Shouts #{0}] Movements (reading order): {1}", _moduleId, new[] { 0, 3, 1, 2 }.Select(ord => string.Format("{0} ({1}, {2})", (char) ('A' + _flashingLetters[ord]), availableMovements[ord].XDist, availableMovements[ord].YDist)).JoinString(", "));
            Console.WriteLine(@"[Simon Shouts #{0}] Possible solution: {1}", _moduleId, solutionPath);

            //Console.WriteLine($"Start: {(from dy in new[] { 0, 1 } from dx in new[] { 0, 1 } select grid[(startposition % 16 + dx) % 16 + 16 * ((startposition / 16 + dy) % 16)]).JoinString()}");
            //Console.WriteLine($"End: {(from dy in new[] { 0, 1 } from dx in new[] { 0, 1 } select grid[(endposition % 16 + dx) % 16 + 16 * ((endposition / 16 + dy) % 16)]).JoinString()}");
            //Console.WriteLine($"Movements: {movements.JoinString(", ")}");
            //Console.WriteLine($"Dist: {covered[endposition]}");
            //ConsoleUtil.WriteLine(Enumerable.Range(0, 16).Select(row => Enumerable.Range(0, 16)
            //    .Select(col => col + 16 * row)
            //    .Select(ix => "██".Color(
            //            ix == _curPosition ? ConsoleColor.Green :
            //            ix == _goalPosition ? ConsoleColor.Red :
            //            valid[ix] && covered.ContainsKey(ix) ? ConsoleColor.Yellow :
            //            valid[ix] ? ConsoleColor.Magenta :
            //            ConsoleColor.Blue))
            //    .JoinColoredString()).JoinColoredString("\n"));
            //Console.WriteLine($"iter: {iter}");
            //Console.WriteLine($"Reachable points: {Enumerable.Range(0, 256).Count(i => valid[i] && covered.ContainsKey(i))}");
        }

        static string positionToGridColors(int pos)
        {
            return (from dy in new[] { 0, 1 } from dx in new[] { 0, 1 } select _grid[(pos % 16 + dx) % 16 + 16 * ((pos / 16 + dy) % 16)]).JoinString();
        }

        static string getSolutionPath(int _curPosition, int _goalPosition, Dictionary<int, Movement> _optimalMovements)
        {
            var position = _curPosition;
            var solution = new List<char>();
            while (position != _goalPosition)
            {
                var m = _optimalMovements[position];
                solution.Add((char) (m.Letter + 'A'));
                position += m;
            }
            return solution.JoinString();
        }

        struct Movement
        {
            public int XDist { get; private set; }
            public int YDist { get; private set; }
            public int Letter { get; private set; }

            private static readonly string[] _xDists = new[]
            {
                "K",
                "FLQ",
                "BGMRW",
                "ACHSXZ",
                "DINUY",
                "JOV",
                "P"
            };
            private static readonly string[] _yDists = new[]
            {
                "A",
                "BCD",
                "FGHIJ",
                "KLMNOP",
                "QRSUV",
                "WXY",
                "Z"
            };

            public Movement(int letter) : this()
            {
                var ch = ((char) ('A' + letter)).ToString();
                XDist = _xDists.IndexOf(str => str.Contains(ch)) - 3;
                YDist = _yDists.IndexOf(str => str.Contains(ch)) - 3;
                Letter = letter;
            }

            public static int operator +(int pos, Movement mv) { return (pos + 16 + mv.XDist) % 16 + 16 * ((pos / 16 + 16 + mv.YDist) % 16); }
            public static int operator -(int pos, Movement mv) { return (pos + 16 - mv.XDist) % 16 + 16 * ((pos / 16 + 16 - mv.YDist) % 16); }
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

        public static void GenerateSvgInManual()
        {
            var colorNames = "red,green,blue,yellow".Split(',');
            var squares = _grid.Select((ch, ix) => $"<rect x='{ix % 16}' y='{ix / 16}' width='1' height='1' class='{colorNames["RGBY".IndexOf(ch)]}' /><text x='{ix % 16 + .5}' y='{ix / 16 + .7}'>{ch}</text>").JoinString();
            var highlightables = Enumerable.Range(0, 16 * 16).Select(ix => $"<circle cx='{ix % 16 + 1}' cy='{ix / 16 + 1}' r='.4' class='highlightable' />").JoinString();
            Utils.ReplaceInFile(@"D:\c\KTANE\Public\HTML\Simon Shouts.html", "<!--%%-->", "<!--%%%-->", $"<svg class='grid' viewBox='-.5 -.5 17 17'><g stroke='black' stroke-width='.025' id='debruijn-torus'>{squares}</g>{highlightables}</svg>");

            var morse = new[] { ".-", "-...", "-.-.", "-..", ".", "..-.", "--.", "....", "..", ".---", "-.-", ".-..", "--", "-.", "---", ".--.", "--.-", ".-.", "...", "-", "..-", "...-", ".--", "-..-", "-.--", "--.." };
            var morseSvg = morse.Select(str =>
            {
                var sb = new StringBuilder();
                var x = 0.0;
                foreach (var ch in str)
                {
                    if (ch == '.')
                    {
                        sb.Append($"<circle cx='{x + .5}' cy='.5' r='.5' />");
                        x += 2;
                    }
                    else
                    {
                        sb.Append($"<rect x='{x}' y='0' width='3' height='1' />");
                        x += 4;
                    }
                }
                return $"<svg class='morse' viewBox='0 0 {x - 1} 1.5'>{sb}</svg>";
            }).ToArray();

            Utils.ReplaceInFile(@"D:\c\KTANE\Public\HTML\Simon Shouts.html", "<!--%%1-->", "<!--%%%1-->", Enumerable.Range(0, 13).Select(i => $"<div>{(char) ('A' + i)}{morseSvg[i]}</div>").JoinString());
            Utils.ReplaceInFile(@"D:\c\KTANE\Public\HTML\Simon Shouts.html", "<!--%%2-->", "<!--%%%2-->", Enumerable.Range(13, 13).Select(i => $"<div>{(char) ('A' + i)}{morseSvg[i]}</div>").JoinString());
        }
    }
}