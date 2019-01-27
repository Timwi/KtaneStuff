using System;
using System.Collections.Generic;
using System.Linq;
using RT.Util;
using RT.Util.Consoles;
using RT.Util.ExtensionMethods;

namespace KtaneStuff
{
    static class ShippingChart
    {
        public static void Experiment()
        {
            var names = new[] { "John", "Rose", "Dave", "Jade", "Aradia", "Tavros", "Sollux", "Karkat", "Nepeta", "Kanaya", "Terezi", "Vriska", "Equius", "Gamzee", "Eridan", "Feferi" };
            var tables = Ut.NewArray(
                // Spades
                @";D;H;H;;S;C;;H;C;;S;H;D;;S|D;;H;S;C;H;;S;D;H;D;;;C;S;|H;H;;S;D;D;;S;C;;H;D;D;D;C;|H;S;S;;H;;H;D;S;S;;C;C;;;D|;C;D;H;;D;H;S;;;H;S;S;C;S;H|S;H;D;;D;;D;;D;;C;S;;H;;C|C;;;H;H;D;;H;;C;D;;S;;S;D|;S;S;D;S;;H;;;D;H;;S;D;;H|H;D;C;S;;D;;;;;D;H;D;S;C;|C;H;;S;;;C;D;;;D;D;H;S;D;S|;D;H;;H;C;D;H;D;D;;H;;S;H;C|S;;D;C;S;S;;;H;D;H;;C;;S;H|H;;D;C;S;;S;S;D;H;;C;;;;|D;C;D;;C;H;;D;S;S;S;;;;;|;S;C;;S;;S;;C;D;H;S;;;;H|S;;;D;H;C;D;H;;S;C;H;;;H;",
                // Hearts
                @";D;D;H;H;;;H;S;C;;S;H;S;;C|D;;H;;S;C;C;S;D;H;H;H;;S;S;D|D;H;;S;D;;D;S;D;;H;;C;;C;|H;;S;;;H;;D;S;;;C;;C;H;H|H;S;D;;;D;H;D;C;D;C;S;S;;S;H|;C;;H;D;;C;;D;;;S;S;H;;|;C;D;;H;C;;H;;H;D;;D;;S;D|H;S;S;D;D;;H;;;D;H;S;;D;D;|S;D;D;S;C;D;;;;D;C;H;D;;S;H|C;H;;;D;;H;D;D;;S;D;;S;D;C|;H;H;;C;;D;H;C;S;;H;;S;H;H|S;H;;C;S;S;;S;H;D;H;;;C;S;|H;;C;;S;S;D;;D;;;;;;C;|S;S;;C;;H;;D;;S;S;C;;;;S|;S;C;H;S;;S;D;S;D;H;S;C;;;D|C;D;;H;H;;D;;H;C;H;;;S;D;",
                // Clubs
                @";D;C;H;S;C;D;D;;H;S;S;H;;;|D;;H;C;;;S;;D;H;;H;;C;S;|C;H;;S;D;C;D;S;D;;H;;S;S;H;|H;C;S;;S;;;D;S;D;;;D;C;H;H|S;;D;S;;D;H;H;C;C;H;S;S;S;S;H|C;;C;;D;;D;S;D;;D;S;H;H;D;|D;S;D;;H;D;;H;;D;D;C;C;;S;D|D;;S;D;H;S;H;;H;D;H;;;D;C;C|;D;D;S;C;D;;H;;C;;H;D;;H;S|H;H;;D;C;;D;D;C;;;D;;S;D;|S;;H;;H;D;D;H;;;;H;;S;H;S|S;H;;;S;S;C;;H;D;H;;C;;S;|H;;S;D;S;H;C;;D;;;C;;;;|;C;S;C;S;H;;D;;S;S;;;;;|;S;H;H;S;D;S;C;H;D;H;S;;;;C|;;;H;H;;D;C;S;;S;;;;C;",
                // Diamonds
                @";D;H;H;;H;;S;;D;D;S;H;S;;|D;;H;D;C;;;C;D;H;;D;;;S;S|H;H;;S;D;D;D;S;C;;H;;;;S;C|H;D;S;;H;C;;D;S;;;D;;;C;H|;C;D;H;;D;H;C;;;S;S;S;;S;H|H;;D;C;D;;;;D;;D;S;S;H;C;|;;D;;H;;;H;S;H;D;C;D;C;S;D|S;C;S;D;C;;H;;;D;H;;;D;H;H|;D;C;S;;D;S;;;;H;H;D;S;H;C|D;H;;;;;H;D;;;C;D;C;S;D;D|D;;H;;S;D;D;H;H;C;;H;C;S;H;|S;D;;D;S;S;C;;H;D;H;;;C;S;S|H;;;;S;S;D;;D;C;C;;;H;;|S;;;;;H;C;D;S;S;S;C;H;;;|;S;S;C;S;C;S;H;H;D;H;S;;;;S|;S;C;H;H;;D;H;C;D;;S;;;S;"
            )
                .Select(str => str.Split('|').Select(row => row.Split(';').Select(c => "SHDC".IndexOf(c) + 1).ToArray()).ToArray()).ToArray();
            var suitLookup = @"-1;0;1;2|0;-1;2;3|1;2;-1;4|2;3;4;-1".Split('|').Select(row => row.Split(';').Select(cell => int.Parse(cell)).ToArray()).ToArray();

            for (int i = 0; i < 4; i++)
            {
                do
                {
                    var twoD = GenerateShippingChartCandelasVersion();
                    tables[i] = Enumerable.Range(0, 16).Select(row => Enumerable.Range(0, 16).Select(col => twoD[col, row]).ToArray()).ToArray();
                }
                while (tables[i][14][15] != i + 1);

                Console.WriteLine($"Chart for {"♠♥♣♦"[i]}:");
                Console.WriteLine(tables[i].Select(row => row.Select(n => " ♠♥♣♦"[n]).JoinString(" ")).JoinString("\n"));
                Console.WriteLine();
            }

            var solutionSuits = new List<int[]>();
            const int iterations = 10000;
            for (int i = 0; i < iterations; i++)
            {
                tryAgain:
                var corners = Enumerable.Range(0, 16).ToArray().Shuffle().Split(4).Cast<List<int>>().ToArray();
                var suits = Enumerable.Range(0, 5).Select(_ => Rnd.Next(1, 5)).ToArray();

                //Console.WriteLine(corners.Select(row => row.JoinString(", ")).JoinString("\r\n"));
                //Console.WriteLine(suits.JoinString(", "));

                // Determine which table to use based on the relationship between Eridan (#14) and Feferi (#15)
                var eridansCorner = corners.IndexOf(c => c.Contains(14));
                var feferiCorner = corners.IndexOf(c => c.Contains(15));
                if (eridansCorner == feferiCorner)
                    goto tryAgain;

                var tableIx = suits[suitLookup[eridansCorner][feferiCorner]] - 1;

                var solutions = new List<int[]>();
                for (int a = 0; a < 4; a++)
                    for (int b = 0; b < 4; b++)
                        for (int c = 0; c < 4; c++)
                            for (int d = 0; d < 4; d++)
                            {
                                if (tables[tableIx][a][b] != suits[0] || tables[tableIx][a][c] != suits[1] || tables[tableIx][a][d] != suits[2] || tables[tableIx][b][c] != suits[2] || tables[tableIx][b][d] != suits[3] || tables[tableIx][c][d] != suits[4])
                                    continue;
                                solutions.Add(new[] { a, b, c, d });
                            }
                ConsoleUtil.Write(solutions.Count.ToString().Color(solutions.Count == 0 ? ConsoleColor.Red : solutions.Count == 1 ? ConsoleColor.Green : ConsoleColor.DarkMagenta) + ", ");
                if (solutions.Count == 1)
                    solutionSuits.Add(suits);
            }
            Console.WriteLine("End");
            Console.WriteLine(solutionSuits.Select(s => s.JoinString(", ")).Distinct().JoinString("\n"));
            Console.WriteLine($"{solutionSuits.Count} solutions found (out of {iterations} iterations)");
        }

        private static int[][][] GenerateShippingChartTimwisVersion()
        {
            var tables = Ut.NewArray<int>(4, 16, 16);

            // Actually let’s generate a random manual table!
            for (int tableIx = 0; tableIx < tables.Length; tableIx++)
            {
                // We want a fair balance of suits
                var stuff = new List<int>();
                for (int i = 0; i < 25; i++)
                {
                    stuff.Add(1);
                    stuff.Add(2);
                    stuff.Add(3);
                    stuff.Add(4);
                }
                while (stuff.Count < 120)
                    stuff.Add(0);
                stuff.Shuffle();
                var stuffIx = 0;
                for (int x = 0; x < 16; x++)
                    for (int y = 0; y < 16; y++)
                    {
                        if (x == y)
                            continue;
                        else if (x >= 14 && y >= 14)
                            tables[tableIx][y][x] = tableIx + 1;
                        else if (y > x)
                            tables[tableIx][y][x] = tables[tableIx][x][y];
                        else
                            tables[tableIx][y][x] = stuff[stuffIx++];
                    }
            }
            return tables;
        }

        public static int[,] GenerateShippingChartCandelasVersion()
        {
            // Generate random ordering of card suits (minus clubs) and blank spaces
            // 0 = Blank
            // 1 = Spade
            // 2 = Heart
            // 3 = Club
            // 4 = Diamond
            // Always 23x Spades, Hearts, Diamonds; 15x Club; 36x Blank
            var array =
                Enumerable.Repeat(1, 15).Concat(
                Enumerable.Repeat(2, 15).Concat(
                Enumerable.Repeat(4, 15).Concat(
                Enumerable.Repeat(0, 60)))).ToArray();
            do
                array.Shuffle();
            while (array[array.Length - 1] == 0);

            // Create blank shipping chart
            int chartSize = 16;
            int[,] ships = new int[chartSize, chartSize];

            // Fill in chart with clubs first
            int[] clubs = Enumerable.Range(0, 16).ToArray().Shuffle();

            for (int i = 0; i < 5; i++)
            {
                int[] x = { clubs[3 * i], clubs[3 * i + 1], clubs[3 * i + 2] };
                int a = x.Max();
                int c = x.Min();
                int b = x.Sum() - a - c;

                ships[a, b] = 3;
                ships[a, c] = 3;
                ships[b, c] = 3;
            }

            // Fill in chart with other suits and blanks, making sure not to overlap clubs
            int filled = 15;
            while (filled < 120)
            {
                for (int row = 0; row < chartSize; row++)
                {
                    for (int col = 0; col < row; col++)
                    {
                        if (ships[row, col] != 3)
                        {
                            ships[row, col] = array[filled - 15];
                            filled++;
                        }
                    }
                }
            }

            // Add chart to its own transpose
            int[,] shipsCopy = (int[,]) ships.Clone();
            for (int row = 0; row < chartSize; row++)
            {
                for (int col = 0; col < chartSize; col++)
                {
                    ships[row, col] = ships[row, col] + shipsCopy[col, row];
                }
            }

            return ships;
        }

        public static void ExperimentCandelasVersion()
        {
            string[] names = new string[] { "John  ", "Rose  ", "Dave  ", "Jade  ", "Aradia", "Tavros", "Sollux", "Karkat", "Nepeta", "Kanaya", "Terezi", "Vriska", "Equius", "Gamzee", "Eridan", "Feferi" };

            // Generate random ordering of card suits (minus clubs) and blank spaces
            // 1 = Heart
            // 2 = Diamond
            // 3 = Spade
            // 4 = Club
            // 0 = Blank
            // Always 35x Heart, Diamond, Spade; 15x Club; 0x Blank
            int[] array = new int[] { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3 };

            do
                array.Shuffle();
            while (array[array.Length - 1] == 0);

            // Create blank shipping chart
            int chartSize = 16;
            int[,] ships = new int[chartSize, chartSize];
            for (int row = 0; row < chartSize; row++)
            {
                for (int col = 0; col < chartSize; col++)
                {
                    ships[row, col] = 0;
                }
            }

            // Fill in chart with clubs first
            int[] clubs = new int[16];
            for (int i = 0; i < clubs.Length; i += 1)
            {
                clubs[i] = i;
            }
            clubs.Shuffle();

            for (int i = 0; i < 5; i++)
            {
                int[] x = { clubs[3 * i], clubs[3 * i + 1], clubs[3 * i + 2] };
                int a = x.Max();
                int c = x.Min();
                int b = x.Sum() - a - c;

                ships[a, b] = 4;
                ships[a, c] = 4;
                ships[b, c] = 4;
            }

            // Fill in chart with other suits and blanks, making sure not to overlap clubs
            int filled = 15;
            while (filled < 120)
            {
                for (int row = 0; row < chartSize; row++)
                {
                    for (int col = 0; col < row; col++)
                    {
                        if (ships[row, col] != 4)
                        {
                            ships[row, col] = array[filled - 15];
                            filled++;
                        }
                    }
                }
            }

            // Add chart to its own transpose
            int[,] shipsCopy = (int[,]) ships.Clone();
            for (int row = 0; row < chartSize; row++)
            {
                for (int col = 0; col < chartSize; col++)
                {
                    ships[row, col] = ships[row, col] + shipsCopy[col, row];
                }
            }

            //// Print numeric shipping chart
            //Console.WriteLine("NUMERIC SHIPPING CHART");
            //for (int row = 0; row < chartSize; row++)
            //{
            //    for (int col = 0; col < chartSize; col++)
            //    {
            //        Console.Write(string.Format("{0}  ", ships[row, col]));
            //    }
            //    Console.WriteLine();
            //}

            // Convert numbers to card suits
            string[,] shippingChart = new string[chartSize, chartSize];
            for (int row = 0; row < chartSize; row++)
            {
                for (int col = 0; col < chartSize; col++)
                {
                    switch (ships[row, col])
                    {
                        case 1:
                            shippingChart[row, col] = "H";
                            break;
                        case 2:
                            shippingChart[row, col] = "D";
                            break;
                        case 3:
                            shippingChart[row, col] = "S";
                            break;
                        case 4:
                            shippingChart[row, col] = "C";
                            break;
                        case 0:
                            shippingChart[row, col] = " ";
                            break;

                    }
                }
            }

            //// Print final shipping chart (no labels)
            //Console.WriteLine();
            //Console.WriteLine("SHIPPING CHART WITHOUT LABELS");
            //for (int row = 0; row < chartSize; row++)
            //{
            //    for (int col = 0; col < chartSize; col++)
            //    {
            //        Console.Write(string.Format("{0}  ", shippingChart[row, col]));
            //    }
            //    Console.WriteLine();
            //}

            // Print final shipping chart (labels)
            Console.WriteLine();
            Console.WriteLine("SHIPPING CHART WITH LABELS");
            Console.Write("       ");
            for (int i = 0; i < chartSize; i++)
            {
                Console.Write(string.Format("{0} ", names[i]));
            }
            Console.WriteLine();

            for (int row = 0; row < chartSize; row++)
            {
                Console.Write(string.Format("{0} ", names[row]));
                for (int col = 0; col < chartSize; col++)
                {
                    Console.Write(string.Format("  {0}    ", shippingChart[row, col]));
                }
                Console.WriteLine();
            }

            //// Print unfolded shipping chart
            //Console.WriteLine();
            //Console.WriteLine("UNFOLDED SHIPPING CHART");
            //for (int row = 0; row < chartSize; row++)
            //{
            //    for (int col = 0; col < chartSize; col++)
            //    {
            //        Console.Write(shippingChart[row, col]);
            //    }
            //}

            Console.WriteLine();
            Console.WriteLine();

            const int iterations = 1;
            for (var iter = 0; iter < iterations; iter++)
            {
                // Create four sets of four characters
                int[] characters = new int[16];
                for (int i = 0; i < characters.Length; i += 1)
                {
                    characters[i] = i;
                }
                characters.Shuffle();

                //for (int i = 0; i < 4; i++)
                //    Console.Write(string.Format("{0}\t{1}\n", names[characters[i]], names[characters[4 + i]]));
                //Console.WriteLine();
                //for (int i = 0; i < 4; i++)
                //    Console.Write(string.Format("{0}\t{1}\n", names[characters[8 + i]], names[characters[12 + i]]));

                //Console.WriteLine();
                var moduleSuitsStringList = new List<string>();
                for (int i = 0; i < 4; i++)
                {
                    for (int j = 12; j < 16; j++)
                    {
                        if (string.IsNullOrEmpty(shippingChart[characters[i], characters[j]]))
                            continue;
                        for (int m = 4; m < 8; m++)
                        {
                            for (int n = 8; n < 12; n++)
                            {
                                if (ships[characters[i], characters[j]] == ships[characters[m], characters[n]])
                                {
                                    string[] moduleSuits = new string[5];
                                    moduleSuits[0] = shippingChart[characters[i], characters[m]];
                                    moduleSuits[1] = shippingChart[characters[i], characters[n]];
                                    moduleSuits[2] = shippingChart[characters[i], characters[j]];
                                    moduleSuits[3] = shippingChart[characters[m], characters[j]];
                                    moduleSuits[4] = shippingChart[characters[n], characters[j]];
                                    string moduleSuitsString = string.Join("", moduleSuits);
                                    moduleSuitsStringList.Add(moduleSuitsString);
                                }
                            }
                        }
                    }
                }

                //string[] allModuleSuits = moduleSuitsStringList.ToArray();
                //string[] uniqueElements = new string[allModuleSuits.Length];
                //int[] uniqueElementIndices = new int[allModuleSuits.Length];
                //for (int i = 0; i < allModuleSuits.Length; i++)
                //{
                //    uniqueElementIndices[i] = i;
                //}
                //for (int i = 0; i < allModuleSuits.Length - 1; i++)
                //{
                //    for (int j = i + 1; j < allModuleSuits.Length; j++)
                //    {
                //        if (allModuleSuits[i] == allModuleSuits[j])
                //        {
                //            uniqueElementIndices[i] = -1;
                //            continue;
                //        }
                //    }
                //}
                //for (int i = 0; i < allModuleSuits.Length; i++)
                //{
                //    if (uniqueElementIndices[i] != -1)
                //    {
                //        uniqueElements[i] = allModuleSuits[uniqueElementIndices[i]];
                //    }
                //}

                //var onlyUniqueElements = new List<string>();
                //for (int i = 0; i < uniqueElements.Length; i++)
                //{
                //    if (!string.IsNullOrEmpty(uniqueElements[i]))
                //    {
                //        onlyUniqueElements.Add(uniqueElements[i]);
                //    }
                //}
                //string[] onlyUniqueElementsArray = onlyUniqueElements.ToArray();

                //Console.WriteLine(onlyUniqueElementsArray.Length);

                var uniqueSuitStrings = moduleSuitsStringList.Where(el => moduleSuitsStringList.Count(el2 => el2 == el) == 1).ToArray();
                Console.WriteLine(uniqueSuitStrings.JoinString("\n"));

                //int suitsOnModuleIndex = Rnd.Next(onlyUniqueElementsArray.Length);
                //string suitsOnModule = onlyUniqueElementsArray[suitsOnModuleIndex];

                //string[] charactersOnModule = new string[4];
                //for (int i = 0; i < 4; i++)
                //    for (int j = 12; j < 16; j++)
                //        if (!string.IsNullOrEmpty(shippingChart[characters[i], characters[j]]))
                //            for (int m = 4; m < 8; m++)
                //                for (int n = 8; n < 12; n++)
                //                    if (ships[characters[i], characters[j]] == ships[characters[m], characters[n]])
                //                    {
                //                        string[] moduleSuits = new string[5];
                //                        moduleSuits[0] = shippingChart[characters[i], characters[m]];
                //                        moduleSuits[1] = shippingChart[characters[i], characters[n]];
                //                        moduleSuits[2] = shippingChart[characters[i], characters[j]];
                //                        moduleSuits[3] = shippingChart[characters[m], characters[j]];
                //                        moduleSuits[4] = shippingChart[characters[n], characters[j]];
                //                        string moduleSuitsString = string.Join("", moduleSuits);

                //                        if (moduleSuitsString == suitsOnModule)
                //                        {
                //                            charactersOnModule[0] = names[characters[i]];
                //                            charactersOnModule[1] = names[characters[m]];
                //                            charactersOnModule[2] = names[characters[n]];
                //                            charactersOnModule[3] = names[characters[j]];
                //                            break;
                //                        }
                //                    }

                //Console.Write("Characters: ");
                //Console.WriteLine(string.Join(" ", charactersOnModule));
                //Console.WriteLine(string.Format("Suits:      {0}", suitsOnModule));
            }
        }
    }
}