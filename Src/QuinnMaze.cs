using System;
using RT.Util.ExtensionMethods;
using System.Collections.Generic;
using System.Linq;
using CsQuery;

namespace KtaneStuff
{
    static class QuinnMaze
    {
        public static void Experiment()
        {
            var c = 0;
            foreach (var solution in FindMazes(new int?[_w * _h], new List<(int cell, int nonwallMask)[]>(), new[] { 0, 1, 3, 8, 0, 2, 12, 4, 0 }))
            //foreach (var solution in FindMazes(new int?[_w * _h], new List<(int cell, int nonwallMask)[]>(), new[] { 0xf, 0xf, 0xf, 0xf, 0xf, 0xf, 0xf, 0xf, 0xf }))
            {
                Console.WriteLine(c);
                Console.WriteLine(solution.Split(_w).Select(row => row.Select(e => "    ^  ── └─ v  │  ┌─ ├─── ─┘ ────┴──┐ ─┤ ─┬──┼─".Substring(3 * ((~e) & 0xf), 3)).JoinString()).JoinString("\n"));
                c++;
            }
            Console.WriteLine($"Found {c} mazes.");
        }

        private static string _debug_output(int?[] grid)
        {
            return grid.Split(_w).Select(row => row.Select(e => e == null ? " ? " : "    ^  ── └─ v  │  ┌─ ├─── ─┘ ────┴──┐ ─┤ ─┬──┼─".Substring(3 * ((~e.Value) & 0xf), 3)).JoinString()).JoinString("\n");
        }

        private const int _w = 3;
        private const int _h = 3;

        private static IEnumerable<int[]> FindMazes(int?[] sofar, List<(int cell, int nonwallMask)[]> regions, int[] outerWalls)
        {
            var ix = -1;
            int[] possibilities = null;

            for (var i = 0; i < sofar.Length; i++)
            {
                if (sofar[i] != null)
                    continue;
                var c = new Coord(_w, _h, i);
                var poss = Enumerable.Range(0, 1 << 4).Where(walls =>
                {
                    if (walls == 0 || sofar.Contains(walls))
                        return false;
                    for (var dir = 0; dir < 4; dir++)
                        if (c.Neighbor((GridDirection) (2 * dir)) is Coord nb ? (sofar[nb.Index] != null && ((sofar[nb.Index].Value & (1 << ((dir + 2) % 4))) != 0) != ((walls & (1 << dir)) != 0)) : ((walls & (1 << dir)) != (outerWalls[i] & (1 << dir))))
                            return false;
                    foreach (var reg in regions)
                        if (reg.Length == 1 && reg[0].cell == i && (reg[0].nonwallMask & walls) != 0)
                            return false;
                    return true;
                }).ToArray();
                if (poss.Length == 0)
                    yield break;
                if (possibilities == null || poss.Length < possibilities.Length)
                {
                    ix = i;
                    possibilities = poss;
                    if (poss.Length == 1)
                        goto shortcut;
                }
            }

            if (ix == -1)
                throw new InvalidOperationException();

            shortcut:
            var cell = new Coord(_w, _h, ix);
            foreach (var walls in possibilities)
            {
                sofar[ix] = walls;
                var newRegions = new List<(int cell, int nonwallMask)[]>();
                var myRegion = Enumerable.Range(0, 4)
                    .Where(dir => (walls & (1 << dir)) == 0 && cell.Neighbor((GridDirection) (2 * dir)) is Coord neigh && sofar[neigh.Index] == null)
                    .Select(dir => (cell: cell.Neighbor((GridDirection) (2 * dir)).Value.Index, nonwallMask: 1 << ((dir + 2) % 4)))
                    .ToList();
                foreach (var region in regions)
                {
                    if (!(region.FirstOrNull(tup => tup.cell == ix) is (int rc, int nonwallMask)))
                        newRegions.Add(region);
                    else if ((walls & nonwallMask) != 0)
                        newRegions.Add(region.Where(tup => tup.cell != ix).ToArray());
                    else
                    {
                        foreach (var tup in region)
                            if (tup.cell != ix)
                            {
                                var p = myRegion.IndexOf(t => t.cell == tup.cell);
                                if (p == -1)
                                    myRegion.Add(tup);
                                else
                                    myRegion[p] = (tup.cell, tup.nonwallMask | myRegion[p].nonwallMask);
                            }
                    }
                }
                if (myRegion.Count == 0)
                {
                    if (!sofar.Any(s => s == null))
                        yield return sofar.Select(s => s.Value).ToArray();
                    continue;
                }
                newRegions.Add(myRegion.ToArray());
                foreach (var solution in FindMazes(sofar, newRegions, outerWalls))
                    yield return solution;
            }
            sofar[ix] = null;
        }
    }
}