using System;
using System.Collections.Generic;
using System.Linq;
using RT.Util;
using RT.Util.ExtensionMethods;

namespace KtaneStuff
{
    static class SimonStacks
    {
        public static void Do()
        {
            var hexes = Ut.NewArray(
                0b0001100111001100000,
                0b0000000011001110110,
                0b0000000001100111011,
                0b0000011001110011000,
                0b0110111001100000000,
                0b1101110011000000000,
                0b1111001100011001111,
                0b0000110010100110000
            )
                .Select(val => Enumerable.Range(0, 19).Aggregate(0, (p, n) => p | ((val & (1 << (18 - n))) != 0 ? (1 << n) : 0))).ToArray();

            var c = 0;
            // 0000000001100111111
            var bits = new[] { 0, 1, 2, 3, 4, 5, 8, 9 };

            var dic = new Dictionary<int, int>();
            for (var hexCombination = 0; hexCombination < 1 << 8; hexCombination++)
            {
                var hexResult = Enumerable.Range(0, 8).Aggregate(0, (p, n) => p ^ ((hexCombination & (1 << n)) != 0 ? hexes[n] : 0));
                var selectedBits = Enumerable.Range(0, 8).Where(bit => (hexResult & (1 << bits[bit])) != 0).Select(i => (char) ('A' + i)).JoinString();
                Console.WriteLine($"{ selectedBits} ⇒ {Enumerable.Range(0, 8).Select(bit => (hexCombination & (1 << bit)) != 0 ? (bit + 1).ToString() : "").JoinString()}");
            }

            //            Console.WriteLine($@"
            //       h
            //    d     m
            // a     i     q
            //    e     n
            // b     j     r
            //    f     o
            // c     k     s
            //    g     p
            //       l
            //".Select(ch => ch >= 'a' && ch <= 's' ? (selectBits & (1 << (ch - 'a'))) != 0 ? "●" : "·" : ch.ToString()).JoinString());
            //Console.WriteLine($"Selecting bits: {Enumerable.Range(0, 19).Select(i => (selectBits & (1 << i)) != 0 ? "█" : "░").JoinString()} is UNIQUE");
            Console.ReadLine();
        }
    }
}