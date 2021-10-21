using System;
using System.Linq;
using RT.Util;
using RT.Util.ExtensionMethods;
using Words;

namespace KtaneStuff
{
    static class ForestCipher
    {
        public static string EncrSemaphoreCipher(string word, string kw)
        {
            var semaphores = Ut.NewArray(
                        /*A*/ new[] { 4, 5 },
                        /*B*/ new[] { 4, 6 },
                        /*C*/ new[] { 4, 7 },
                        /*D*/ new[] { 0, 4 },
                        /*E*/ new[] { 1, 4 },
                        /*F*/ new[] { 2, 4 },
                        /*G*/ new[] { 3, 4 },
                        /*H*/ new[] { 5, 6 },
                        /*I*/ new[] { 5, 7 },
                        /*J*/ new[] { 0, 2 },
                        /*K*/ new[] { 0, 5 },
                        /*L*/ new[] { 1, 5 },
                        /*M*/ new[] { 2, 5 },
                        /*N*/ new[] { 3, 5 },
                        /*O*/ new[] { 6, 7 },
                        /*P*/ new[] { 0, 6 },
                        /*Q*/ new[] { 1, 6 },
                        /*R*/ new[] { 2, 6 },
                        /*S*/ new[] { 3, 6 },
                        /*T*/ new[] { 0, 7 },
                        /*U*/ new[] { 1, 7 },
                        /*V*/ new[] { 0, 3 },
                        /*W*/ new[] { 1, 2 },
                        /*X*/ new[] { 1, 3 },
                        /*Y*/ new[] { 2, 7 },
                        /*Z*/ new[] { 2, 3 });

            var encrypted = "";
            for (var i = 0; i < word.Length; i++)
            {
                var rotated = semaphores[word[i] - 'A'].Select(j => (j + kw[i % kw.Length] - 'A' + 1) % 8).Order().ToArray();
                var letter = semaphores.IndexOf(s => s.SequenceEqual(rotated));
                if (letter == -1)
                    return null;
                encrypted += (char) ('A' + letter);
            }
            return encrypted;
        }

        public static void SemaphoreCipherExperiment()
        {
            var rnd = new Random(47);
            var words = new Data().allWords[2].ToArray();
            var lowest = 100d;
            foreach (var word in words)
            {
                var successes = 0;
                var fails = 0;
                foreach (var kw in words)
                {
                    var encrypted = EncrSemaphoreCipher(word, kw);
                    if (encrypted == null)
                        fails++;
                    else
                        successes++;
                }
                var perc = (double) 100 * successes / (successes + fails);
                if (perc < lowest)
                    lowest = perc;
                Console.WriteLine($"Word: {word} = {perc,4:.0}%; lowest: {lowest,4:.0}%");
            }
        }

        private static int highestBit(ulong n) => n == 0 ? -1 : 1 + highestBit(n >> 1);

        public static void ChainBitRotationExperiment()
        {
            var wordLists = new Data().allWords;
            var rnd = new Random(47);
            var rnd7letterWord = wordLists[3].PickRandom(rnd);
            var kw = wordLists.PickRandom(rnd).PickRandom(rnd);

            var number = 1ul;
            tryAgain:
            var (decrypted, residual, decryptedFull) = DecrChainBitRotation(rnd7letterWord, kw, number);
            if (decryptedFull == null)
            {
                number++;
                goto tryAgain;
            }
            Console.WriteLine($"Word: {rnd7letterWord}; KW: {kw}; number: {number}");
            Console.WriteLine($"Decrypted: {decrypted}");
            Console.WriteLine($"Residual: {residual}");
            Console.WriteLine($"Decrypted full: {decryptedFull}");
        }

        private static (string decrypted, ulong residual, string decryptedFull) DecrChainBitRotation(string encrypted, string kw, ulong number, bool debug = false)
        {
            if (debug)
            {
                Console.WriteLine();
                Console.WriteLine($"DECR: {encrypted}, KW={kw}, residual={number}");
                Console.WriteLine($"{Convert.ToString((long) number, 2),64} #={number}");
            }
            var wordLength = encrypted.Length;
            for (var i = 0; i < wordLength; i++)
            {
                // Process last letter
                var letter = encrypted[encrypted.Length - 1];
                encrypted = encrypted.Substring(0, encrypted.Length - 1);
                number = number * 26 + (letter == 'Z' ? 0ul : (ulong) (letter - 'A' + 1));
                if (debug)
                    Console.WriteLine($"{Convert.ToString((long) number, 2),64} Process {letter}");
                // Remove the top bit
                var nb = highestBit(number);
                number -= 1ul << nb;
                if (debug)
                    Console.WriteLine($"{Convert.ToString((long) number, 2),64} Remove bit {nb}");
                // Reverse the bit rotation
                if (nb > 0)
                {
                    var amt = (kw[(wordLength - 1 - i) % kw.Length] - 'A' + 1) % nb;
                    number = ((number >> (nb - amt)) | (number << amt)) & ((1ul << nb) - 1);
                    if ((number & (1ul << (nb - 1))) == 0)
                        return default;
                    if (debug)
                        Console.WriteLine($"{Convert.ToString((long) number, 2),64} Rotate bits left {amt}");
                }
            }
            var decrypted = "";
            for (var i = 0; i < wordLength; i++)
            {
                var num = number % 26;
                decrypted = (char) (num == 0 ? 'Z' : num + 'A' - 1) + decrypted;
                number /= 26;
            }
            if (debug)
                Console.WriteLine($"{Convert.ToString((long) number, 2),64} Residual={number}");
            var residual = number;
            var decryptedFull = decrypted;
            while (number > 0)
            {
                var num = number % 26;
                decryptedFull = (char) (num == 0 ? 'Z' : num + 'A' - 1) + decryptedFull;
                number /= 26;
            }
            return (decrypted, residual, decryptedFull);
        }

        private static (string encrypted, ulong number) EncrChainBitRotation(string word, string kw, ulong residual, bool debug = false)
        {
            var encrypted = "";
            var number = word.Aggregate(residual, (p, n) => p * 26 + (n == 'Z' ? 0ul : (ulong) (n - 'A' + 1)));
            if (debug)
            {
                Console.WriteLine();
                Console.WriteLine($"ENCR: {word}, KW={kw}, residual={residual}");
                Console.WriteLine($"{Convert.ToString((long) number, 2),64} residual={residual}");
            }
            for (var i = 0; i < word.Length; i++)
            {
                // Bit rotation
                var nb = highestBit(number) + 1;
                if (nb > 0)
                {
                    var amt = (kw[i % kw.Length] - 'A' + 1) % nb;
                    number = ((number >> amt) | (number << (nb - amt))) & ((1ul << nb) - 1);
                    if (debug)
                        Console.WriteLine($"{Convert.ToString((long) number, 2),64} Rotate bits right {amt}");
                }
                // Add the extra bit
                number |= 1ul << nb;
                if (debug)
                    Console.WriteLine($"{Convert.ToString((long) number, 2),64} Add bit {nb}");
                // Extract a letter
                var extracted = (int) (number % 26);
                encrypted += extracted == 0 ? 'Z' : (char) (extracted + 'A' - 1);
                number /= 26;
                if (debug)
                    Console.WriteLine($"{Convert.ToString((long) number, 2),64} Extract {encrypted.Last()}");
            }
            return (encrypted, number);
        }
    }
}