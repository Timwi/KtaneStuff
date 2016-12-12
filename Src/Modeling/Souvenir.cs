using System.Collections.Generic;
using System.IO;
using System.Linq;
using RT.Util.ExtensionMethods;

namespace KtaneStuff.Modeling
{
    using static Md;

    static class Souvenir
    {
        public static void Do()
        {
            File.WriteAllText(@"D:\c\KTANE\Souvenir\Assets\Models\AnswerHighlightLong.obj", GenerateObjFile(AnswerHighlight(.21), "AnswerHighlightLong"));
            File.WriteAllText(@"D:\c\KTANE\Souvenir\Assets\Models\AnswerHighlightShort.obj", GenerateObjFile(AnswerHighlight(.15), "AnswerHighlightShort"));
        }

        private static IEnumerable<Pt[]> AnswerHighlight(double width)
        {
            var height = .08;
            var padding = .02;
            var right = width - height / 2;
            return
                Enumerable.Range(0, 37)
                    .Select(i => i * 180 / 36 + 90)
                    .Select(angle => new { Inner = pt((height - padding) / 2 * cos(angle), 0, (height - padding) / 2 * sin(angle)), Outer = pt(height / 2 * cos(angle), 0, height / 2 * sin(angle)) })
                    .SelectConsecutivePairs(false, (i1, i2) => new[] { i1.Inner, i1.Outer, i2.Outer, i2.Inner })
                .Concat(new[] { pt(0, 0, -height / 2), pt(right, 0, -height / 2), pt(right, 0, -(height - padding) / 2), pt(0, 0, -(height - padding) / 2) })
                .Concat(new[] { pt(0, 0, (height - padding) / 2), pt(right, 0, (height - padding) / 2), pt(right, 0, height / 2), pt(0, 0, height / 2) })
                .Concat(new[] { pt(right, 0, -height / 2), pt(right, 0, height / 2), pt(right - height / 3, 0, 0) });
        }
    }
}
