using System;
using System.Collections.Generic;
using System.Linq;
using RT.Util;
using RT.Util.ExtensionMethods;

namespace KtaneStuff
{
    static class Poker
    {
        public static void Simulate()
        {
            var results = new Dictionary<int, int>();
            var highestResults = new Dictionary<int, int>();
            var numDecks = 4;

            const int iterations = 10000;
            for (int iter = 0; iter < iterations; iter++)
            {
                var decks = Enumerable.Range(0, numDecks).Select(_ => Enumerable.Range(0, 52).ToArray().Shuffle()).ToArray();
                var serialNumber = "XXNLLN".Select(ch => ch == 'X' ? "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789" : ch == 'N' ? "0123456789" : "ABCDEFGHIJKLMNOPQRSTUVWXYZ").Select(str => str[Rnd.Next(0, str.Length)]).JoinString();

                var available = new bool[9];
                var highest = -1;
                for (int deckIx = 0; deckIx < decks.Length; deckIx++)
                {
                    for (int cIx = 0; cIx < 52; cIx++)
                    {
                        var hand = new List<int> { decks[deckIx][cIx] };
                        var cIx2 = cIx;
                        for (int i = 0; i < 4; i++)
                        {
                            cIx2 = (cIx2 + serialNumber[i].Apply(ch => ch >= '0' && ch <= '9' ? ch - '0' + 1 : ch - 'A' + 1)) % 52;
                            hand.Add(decks[deckIx][cIx2]);
                        }

                        var straight = Enumerable.Range(0, 10).Any(i => Enumerable.Range(0, 5).All(j => hand.Any(h => h % 13 == (i + j) % 13)));
                        var flush = hand.All(h => h / 13 == hand[0] / 13);
                        var groups = hand.GroupBy(h => h % 13).Select(g => g.Count()).ToArray();
                        int result;
                        if (straight && flush)
                            result = 8;
                        else if (groups.Contains(4))
                            result = 7;
                        else if (groups.Contains(3) && groups.Contains(2))
                            result = 6;
                        else if (flush)
                            result = 5;
                        else if (straight)
                            result = 4;
                        else if (groups.Contains(3))
                            result = 3;
                        else if (groups.Count(g => g == 2) == 2)
                            result = 2;
                        else if (groups.Contains(2))
                            result = 1;
                        else
                            result = 0;
                        available[result] = true;
                        if (highest < result)
                            highest = result;
                    }
                }
                for (int i = 0; i < 9; i++)
                    if (available[i])
                        results.IncSafe(i);
                highestResults.IncSafe(highest);
            }
            Console.WriteLine("Available:");
            foreach (var kvp in results.OrderByDescending(p => p.Value))
                Console.WriteLine($"{"no hand,pair,two pairs,three of a kind,straight,flush,full house,four of a kind,straight flush".Split(',')[kvp.Key]} = {kvp.Value / (double) iterations * 100:0.00}%");
            Console.WriteLine();
            Console.WriteLine($"Highest result ({numDecks} decks):");
            foreach (var kvp in highestResults.OrderByDescending(p => p.Value))
                Console.WriteLine($"{"no hand,pair,two pairs,three of a kind,straight,flush,full house,four of a kind,straight flush".Split(',')[kvp.Key]} = {kvp.Value / (double) iterations * 100:0.00}%");
        }
    }
}