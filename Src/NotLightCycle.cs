using System;
using System.Collections.Generic;
using System.Linq;
using RT.Util;
using RT.Util.Consoles;
using RT.Util.ExtensionMethods;

namespace KtaneStuff
{
    static class NotLightCycle
    {
        public static void DoStuff2()
        {
            var d = new Dictionary<Polyomino, int>();
            for (var seed = 0; seed < 65536; seed++)
            {
                var (hexomino, serialNumber) = GenerateSerialNumberAndHexomino(new Random(seed));
                //Console.WriteLine(serialNumber + ":");
                //Console.WriteLine(hexomino);
                //Console.WriteLine();
                d.IncSafe(hexomino);
            }

            foreach (var (hexomino, count) in d.OrderBy(kvp => kvp.Value))
            {
                Console.WriteLine($"{count}:");
                Console.WriteLine(hexomino);
                Console.WriteLine();
            }
        }

        public static void DoStuff()
        {
            IEnumerable<Polyomino> generatePolyominoes(int[] sofar, int[] forbidden)
            {
                if (sofar.Length == 6)
                {
                    var arr = new bool[6 * 6];
                    for (var i = 0; i < sofar.Length; i++)
                        arr[sofar[i]] = true;
                    var firstRow = Enumerable.Range(0, 6).First(row => Enumerable.Range(0, 6).Any(col => arr[col + 6 * row]));
                    var lastRow = Enumerable.Range(0, 6).Last(row => Enumerable.Range(0, 6).Any(col => arr[col + 6 * row])) + 1;
                    var firstCol = Enumerable.Range(0, 6).First(col => Enumerable.Range(0, 6).Any(row => arr[col + 6 * row]));
                    var lastCol = Enumerable.Range(0, 6).Last(col => Enumerable.Range(0, 6).Any(row => arr[col + 6 * row])) + 1;
                    var w = lastCol - firstCol;
                    var h = lastRow - firstRow;
                    yield return new Polyomino(w, h, Ut.NewArray(w * h, ix => arr[(ix % w) + firstCol + 6 * ((ix / w) + firstRow)]));
                    yield break;
                }

                for (var i = 0; i < 6 * 6; i++)
                {
                    if (sofar.Contains(i) || forbidden.Contains(i))
                        continue;
                    if (!sofar.Any(p => new Coord(6, 6, p).AdjacentTo(new Coord(6, 6, i))))
                        continue;
                    var newSofar = sofar.Append(i);
                    foreach (var result in generatePolyominoes(newSofar, forbidden))
                        yield return result;
                    forbidden = forbidden.Append(i);
                }
            }

            var allHexominoes = Enumerable.Range(0, 6).SelectMany(ix => generatePolyominoes([ix], Enumerable.Range(0, ix).ToArray())).Distinct().ToArray();
            Console.WriteLine(allHexominoes.Length);

            var rnd = new Random(447);
            const int gridW = 16, gridH = 16, numColors = 6;
            var grid = Enumerable.Range(0, gridW * gridH).Select(i => rnd.Next(0, numColors)).ToArray();
            ConsoleUtil.WriteLine(Enumerable.Range(0, gridH).Select(row => Enumerable.Range(0, gridW).Select(col => "██".Color((ConsoleColor) (grid[col + gridW * row] + 1))).JoinColoredString()).JoinColoredString("\n"));
            Console.WriteLine();

            var (ourHexomino, ser) = GenerateSerialNumberAndHexomino(rnd);

            var colorCombinationCounts = new Dictionary<string, int>();
            for (var x = 0; x < gridW - ourHexomino.Width; x++)
                for (var y = 0; y < gridH - ourHexomino.Height; y++)
                    colorCombinationCounts.IncSafe(ourHexomino.Cells.Select(c => grid[c.X + x + gridW * (c.Y + y)]).Order().JoinString());

            foreach (var colorComb in colorCombinationCounts.OrderBy(kvp => kvp.Value))
                ConsoleUtil.WriteLine(colorComb.Key.Select(c => "██".Color((ConsoleColor) (c - '0' + 1))).JoinColoredString() + $"   = {colorComb.Value}");

            Console.WriteLine(ourHexomino);

            //ConsoleUtil.WriteLine(Enumerable.Range(0, gridH)
            //    .Select(row => Enumerable.Range(0, gridW)
            //        .Select(col => "██".Color(ourHexomino.Cells.Any(c => c.X + hexPosX == col && c.Y + hexPosY == row) ? ConsoleColor.White : (ConsoleColor) (grid[col + gridW * row] + 1))).JoinColoredString()).JoinColoredString("\n"));
            //Console.WriteLine();

            //ConsoleUtil.WriteLine(colors.Select(c => "██".Color((ConsoleColor) (c + 1))).JoinColoredString());
        }

        private static (Polyomino hexomino, string serialNumber) GenerateSerialNumberAndHexomino(Random rnd)
        {
            var ser = $"{"0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ".PickRandom(rnd)}{"0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ".PickRandom(rnd)}{"0123456789".PickRandom(rnd)}{"ABCDEFGHIJKLMNOPQRSTUVWXYZ".PickRandom(rnd)}{"ABCDEFGHIJKLMNOPQRSTUVWXYZ".PickRandom(rnd)}{"0123456789".PickRandom(rnd)}";
            var base36 = ser.Aggregate(0L, (p, n) => (p * 36) + "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ".IndexOf(n));
            var revBinaryStr = "000";
            while (base36 > 0)
            {
                revBinaryStr += base36 & 1;
                base36 >>= 1;
            }
            var revBinary = revBinaryStr.PadLeft(25, '0').Select(ch => ch - '0').ToArray();

            var ourHexominoTiles = new List<(int x, int y)> { (0, 0) };
            for (int ti = 0, i = 0; ti < 5; ti++, i += 5)
            {
                var startingPoint = (revBinary[i] + 2 * revBinary[i + 1] + 4 * revBinary[i + 2]) % ourHexominoTiles.Count;
                var dir = revBinary[i + 3] + 2 * revBinary[i + 4];
                var (x, y) = ourHexominoTiles.Last();
                do
                    switch (dir)
                    {
                        case 0: y--; break;
                        case 1: x++; break;
                        case 2: y++; break;
                        case 3: x--; break;
                    }
                while (ourHexominoTiles.Contains((x, y)));
                ourHexominoTiles.Add((x, y));
            }
            var firstRow = ourHexominoTiles.Min(t => t.y);
            var lastRow = ourHexominoTiles.Max(t => t.y) + 1;
            var firstCol = ourHexominoTiles.Min(t => t.x);
            var lastCol = ourHexominoTiles.Max(t => t.x) + 1;
            var hexW = lastCol - firstCol;
            var hexH = lastRow - firstRow;
            var ourHexomino = new Polyomino(hexW, hexH, Ut.NewArray(hexW * hexH, ix => ourHexominoTiles.Contains((ix % hexW + firstCol, ix / hexW + firstRow))));
            return (ourHexomino, ser);
        }
    }
}