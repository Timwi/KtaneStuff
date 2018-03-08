using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using RT.Util.ExtensionMethods;

namespace KtaneStuff
{
    static class GridMatching
    {
        private static int[] _solutionStates = new int[]
        {
            4801614,
            11514736,
            15334525,
            12676110,
            15026679,
            27395820,
            8266786,
            10065819,
            16831939,
            4766790,
            4019783,
            25639603,
            32360091,
            24475420,
            13166525,
            8785572
        };

        public static void CreateCheatSheet()
        {
            // Maps from column counts+row counts of WHITE pixels (e.g. "4234324334" = columns are 4,2,3,4,3; rows are 2,4,3,3,4) to index in _solutionStates
            var dic = new Dictionary<string, int>();

            for (int ix = 0; ix < _solutionStates.Length; ix++)
            {
                var sol = _solutionStates[ix];
                var rowCounts = Enumerable.Range(0, 5).Select(row => Enumerable.Range(0, 5).Count(col => (sol & (1 << (5 * row + col))) != 0)).ToArray();
                var colCounts = Enumerable.Range(0, 5).Select(col => Enumerable.Range(0, 5).Count(row => (sol & (1 << (5 * row + col))) != 0)).ToArray();

                foreach (var key in new[] {
                    colCounts.JoinString() + rowCounts.JoinString(),
                    rowCounts.Reverse().JoinString() + colCounts.JoinString(),
                    colCounts.Reverse().JoinString() + rowCounts.Reverse().JoinString(),
                    rowCounts.JoinString() + colCounts.Reverse().JoinString()
                })
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