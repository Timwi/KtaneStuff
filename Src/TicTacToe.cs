using System.IO;
using System.Linq;
using KtaneStuff.Modeling;
using RT.Util;
using RT.Util.ExtensionMethods;

namespace KtaneStuff
{
    using static Md;

    static class TicTacToe
    {
        public static void DoModels()
        {
            // Tic-Tac-Toe Keycap
            File.WriteAllText(@"D:\c\KTANE\TicTacToe\Assets\Assets\Keycap.obj", Keycap("Keycap", slope: -3, keyWidth: 1, keyHeight: .7, bumpWidth: .2, bumpHeight: .05, keyDepth: .3));
            // Tic-Tac-Toe PASS button
            File.WriteAllText(@"D:\c\KTANE\TicTacToe\Assets\Assets\Passcap.obj", Keycap("PASS keycap", slope: -2, keyWidth: 2.5, keyHeight: .5, bumpWidth: .2, bumpHeight: .05, keyDepth: .3));
        }

        static string Keycap(string objectName, double slope, double keyWidth, double keyHeight, double bumpWidth, double bumpHeight, double keyDepth)
        {
            var bottom = -bumpHeight - keyDepth;
            var cbp0 = pt(0, 0, 0);
            var cbp1 = pt(bumpWidth * 3 / 4, 0, 0);
            var cbp2 = pt(bumpWidth + bumpHeight / slope, 0, bumpHeight);
            var cbp3 = pt(bumpWidth, 0, -bumpHeight);
            var rawPs = Ut.Range(0, 1, .02).Select(t => pow(1 - t, 3) * cbp0 + 3 * pow(1 - t, 2) * t * cbp1 + 3 * (1 - t) * t * t * cbp2 + pow(t, 3) * cbp3)
                // side slope
                .Concat(pt(bumpWidth - keyDepth / slope, 0, bottom))
                // bottom face
                .Concat(pt(0, 0, bottom));

            var sideH = rawPs.SelectConsecutivePairs(false, (p1, p2) => new[] { p1, p2, p2.Add(y: keyHeight), p1.Add(y: keyHeight) });
            var sideW = rawPs.SelectConsecutivePairs(false, (p1, p2) => new[] { p1, p2, p2.Add(y: keyWidth), p1.Add(y: keyWidth) });
            var corner = Ut.Range(0, 90, 10).SelectManyConsecutivePairs(false, (angle1, angle2) =>
                    rawPs.SelectConsecutivePairs(false, (p1, p2) => new[] { p1.RotateZ(angle1), p2.RotateZ(angle1), p2.RotateZ(angle2), p1.RotateZ(angle2) })).ToArray();

            var everything = Ut.NewArray(
                corner,
                sideH.MoveY(-keyHeight),
                sideH.RotateZ(180).MoveX(-keyWidth),
                sideW.RotateZ(90),
                sideW.RotateZ(270).MoveX(-keyWidth).MoveY(-keyHeight),
                corner.RotateZ(90).MoveX(-keyWidth),
                corner.RotateZ(180).MoveX(-keyWidth).MoveY(-keyHeight),
                corner.RotateZ(270).MoveY(-keyHeight),
                // Main key face
                new[] { new[] { pt(0, 0, 0), pt(-keyWidth, 0, 0), pt(-keyWidth, -keyHeight, 0), pt(0, -keyHeight, 0) } },
                // Bottom face
                new[] { new[] { pt(0, 0, bottom), pt(-keyWidth, 0, bottom), pt(-keyWidth, -keyHeight, bottom), pt(0, -keyHeight, bottom) }.Reverse().ToArray() }
            ).SelectMany(x => x);

            return GenerateObjFile(everything.ToArray(), objectName);
        }
    }
}
