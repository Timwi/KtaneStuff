using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using RT.Util;
using RT.Util.ExtensionMethods;

namespace KtaneStuff
{
    static class GridMatching
    {
        private static ulong[] _grids = new ulong[]
        {
            39378177601UL,
            64499872570UL,
            32484181835UL,
            47046548784UL,
            17108770296UL,
            67161369174UL,
            51985924635UL,
            14469411799UL,
            25731425460UL,
            23420243268UL,
            27236915887UL,
            44207693997UL,
            7191299719UL,
            5336900789UL,
            35778815767UL,
            68311691418UL
        };

        public static void CreateCheatSheet()
        {
            // Maps from column counts+row counts of WHITE pixels (e.g. "4234/4334" = columns are 4,2,3,4; rows are 4,3,3,4) to index in _solutionStates
            var dic = new Dictionary<string, int>();

            for (int ix = 0; ix < _grids.Length; ix++)
                for (int ox = 0; ox <= 2; ox++)
                    for (int oy = 0; oy <= 2; oy++)
                    {
                        var sol = _grids[ix];
                        var rowCounts = Enumerable.Range(oy, 4).Select(row => Enumerable.Range(ox, 4).Count(col => ((sol >> (6 * row + col)) & 1) != 0)).ToArray();
                        var colCounts = Enumerable.Range(ox, 4).Select(col => Enumerable.Range(oy, 4).Count(row => ((sol >> (6 * row + col)) & 1) != 0)).ToArray();

                        foreach (var key in Ut.NewArray(
                            colCounts.JoinString() + "/" + rowCounts.JoinString(),
                            rowCounts.Reverse().JoinString() + "/" + colCounts.JoinString(),
                            colCounts.Reverse().JoinString() + "/" + rowCounts.Reverse().JoinString(),
                            rowCounts.JoinString() + "/" + colCounts.Reverse().JoinString()
                        ))
                        {
                            if (dic.ContainsKey(key))
                            {
                                Console.WriteLine($"Duplicate: {key}");
                            }
                            dic[key] = ix;
                        }
                    }
        }
    }
}