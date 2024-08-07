using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using RT.Util.ExtensionMethods;
using RT.Util.Geometry;

namespace KtaneStuff
{
    public static class Flyswatting
    {
        public static void DoStuff()
        {
            var points = new[] { new PointD(250, 76), new PointD(271, 90), new PointD(290, 104), new PointD(310, 108), new PointD(333, 111), new PointD(358, 113), new PointD(365, 136), new PointD(372, 157), new PointD(386, 175), new PointD(403, 188), new PointD(423, 204), new PointD(417, 227), new PointD(414, 249), new PointD(414, 268), new PointD(417, 290), new PointD(423, 315), new PointD(403, 331), new PointD(386, 346), new PointD(378, 364), new PointD(365, 381), new PointD(358, 404), new PointD(333, 410), new PointD(310, 413), new PointD(290, 420), new PointD(271, 429), new PointD(250, 440), new PointD(229, 429), new PointD(210, 420), new PointD(190, 413), new PointD(167, 410), new PointD(142, 404), new PointD(135, 381), new PointD(122, 364), new PointD(114, 346), new PointD(97, 331), new PointD(77, 315), new PointD(83, 290), new PointD(86, 268), new PointD(86, 249), new PointD(83, 227), new PointD(77, 204), new PointD(97, 188), new PointD(114, 175), new PointD(128, 157), new PointD(135, 136), new PointD(142, 113), new PointD(167, 111), new PointD(190, 108), new PointD(210, 104), new PointD(229, 90), new PointD(250, 110), new PointD(273, 140), new PointD(301, 148), new PointD(336, 137), new PointD(338, 175), new PointD(355, 199), new PointD(390, 211), new PointD(367, 242), new PointD(367, 272), new PointD(390, 303), new PointD(355, 315), new PointD(338, 339), new PointD(336, 377), new PointD(301, 366), new PointD(273, 374), new PointD(250, 404), new PointD(227, 374), new PointD(199, 366), new PointD(164, 377), new PointD(162, 339), new PointD(145, 315), new PointD(110, 303), new PointD(133, 272), new PointD(133, 242), new PointD(110, 211), new PointD(145, 199), new PointD(162, 175), new PointD(164, 137), new PointD(199, 148), new PointD(227, 140), new PointD(250, 165), new PointD(305, 185), new PointD(338, 229), new PointD(338, 283), new PointD(305, 329), new PointD(250, 350), new PointD(195, 329), new PointD(162, 283), new PointD(162, 229), new PointD(195, 185), new PointD(250, 207), new PointD(280, 215), new PointD(300, 240), new PointD(300, 275), new PointD(280, 300), new PointD(250, 308), new PointD(220, 300), new PointD(200, 275), new PointD(200, 240), new PointD(220, 215) };
            var vertexes = new[] { new PointD(174, 26), new PointD(325, 26), new PointD(448, 115), new PointD(494, 258), new PointD(448, 401), new PointD(325, 490), new PointD(174, 490), new PointD(50, 401), new PointD(8, 258), new PointD(50, 115) };
            var sb = new StringBuilder();

            var c = 0;
            foreach (var ((p1, i1), (p2, i2), (p3, i3)) in vertexes.UniqueTriplets())
            {
                var name = $"{i1}{i2}{i3}";
                var triangle = new PolygonD(p1, p2, p3);
                var containedPoints = points.SelectIndexWhere(pt => triangle.ContainsPoint(pt)).ToList();
                if (containedPoints.Count > 0)
                {
                    sb.AppendLine($"case {c}: numbers = \"{name}\"; letters = new[] {{ {containedPoints.JoinString(", ")} }}; break;");
                    c++;
                }
            }
            Clipboard.SetText(sb.ToString());
        }

        /// <summary>
        ///     Returns an enumeration of tuples containing all unique pairs of distinct elements from the source collection. For
        ///     example, the input sequence 1, 2, 3 yields the pairs [1,2], [1,3] and [2,3] only.</summary>
        private static IEnumerable<((T, int), (T, int), (T, int))> UniqueTriplets<T>(this IEnumerable<T> source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            IEnumerable<((T, int), (T, int), (T, int))> uniquePairsIterator()
            {
                // Make sure that ‘source’ is evaluated only once
                var arr = source as IList<T> ?? source.ToArray();
                for (int i = 0; i < arr.Count - 1; i++)
                    for (int j = i + 1; j < arr.Count; j++)
                        for (int k = j + 1; k < arr.Count; k++)
                            yield return ((arr[i], i), (arr[j], j), (arr[k], k));
            }
            return uniquePairsIterator();
        }

    }
}
