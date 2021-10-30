using System;
using System.Linq;
using RT.Util;
using RT.Util.Consoles;
using RT.Util.ExtensionMethods;
using Words;

namespace KtaneStuff
{
    static class ForestCipher
    {
        public static void RubiksMonoalphabeticCipherExperiment()
        {
            var wordList = new Data().allWords;
            var word = wordList[2].PickRandom();
            wordList[2].Remove(word);

            var wl = wordList.PickRandom();
            var alphaKw = wl.PickRandom();
            wl.Remove(alphaKw);

            wl = wordList.Take(3).PickRandom();
            var rotationsKw = wl.PickRandom();
            wl.Remove(rotationsKw);

            Console.WriteLine($"Word: {word}, alphaKw: {alphaKw}, rotKw: {rotationsKw}");
            var encrypted = EncrRubiksMonoalphabeticCipher(word, alphaKw, rotationsKw, debug: true);
            Console.WriteLine($"Encrypted: {encrypted}");
            Console.WriteLine($"Decrypted: {DecrRubiksMonoalphabeticCipher(encrypted, alphaKw, rotationsKw, debug: true)}");
        }

        private static string EncrRubiksMonoalphabeticCipher(string word, string alphaKw, string rotationsKw, bool debug = false)
        {
            var cube = GenerateRubiksMonoalphabeticCube(alphaKw, rotationsKw, debug);
            return word.Select(ch => cube[ch - 'A']).JoinString();
        }

        private static string DecrRubiksMonoalphabeticCipher(string word, string alphaKw, string rotationsKw, bool debug = false)
        {
            var cube = GenerateRubiksMonoalphabeticCube(alphaKw, rotationsKw, debug);
            return word.Select(ch => (char) (cube.IndexOf(ch) + 'A')).JoinString();
        }

        private static char[] GenerateRubiksMonoalphabeticCube(string alphaKw, string rotationsKw, bool debug = false)
        {
            var alphabetKey = alphaKw.Concat("ABCDEFGHIJKLMNOPQRSTUVWXYZ").Distinct().ToArray();
            var specialCubelets = new[] { 10, 4, 13, 21, 12, 15 };
            var cubelets = specialCubelets.Concat(Enumerable.Range(0, 26)).Distinct().ToArray();

            var cube = new char[26];
            for (var i = 0; i < 26; i++)
                cube[cubelets[i]] = alphabetKey[i];

            if (debug)
            {
                Console.WriteLine("Rubik’s Monoalphabetic Cube BEFORE rotations:");
                for (var row = 0; row < 3; row++)
                    Console.WriteLine($"    {Enumerable.Range(0, 3).Select(layer => Enumerable.Range(0, 3).Select(col => 9 * layer + 3 * row + col == 13 ? ' ' : cube[9 * layer + 3 * row + col - (9 * layer + 3 * row + col >= 13 ? 1 : 0)]).JoinString(" ")).JoinString("    ")}");
            }

            // order: U F R B L D
            var rotations = "0 9 17 18 19 11 2 1,0 1 2 5 8 7 6 3,2 11 19 22 25 16 8 5,19 18 17 20 23 24 25 22,17 9 0 3 6 14 23 20,6 7 8 16 25 24 23 14"
                .Split(',').Select(str => str.Split(' ').Select(int.Parse).ToArray()).ToArray();

            var rots = rotationsKw.Select(ch => (ch - 'A') % 24).Select(rotIx => new { Face = rotIx / 4, NumRot = 2 * (rotIx % 4) + 1 }).ToArray();
            foreach (var rot in rots)
            {
                var r = rotations[rot.Face];
                for (var n = 0; n < rot.NumRot; n++)
                {
                    var f = cube[r.Last()];
                    for (var i = r.Length - 1; i > 0; i--)
                        cube[r[i]] = cube[r[i - 1]];
                    cube[r[0]] = f;
                }
            }

            if (debug)
            {
                Console.WriteLine("Rubik’s Monoalphabetic Cube AFTER rotations:");
                for (var row = 0; row < 3; row++)
                    Console.WriteLine($"    {Enumerable.Range(0, 3).Select(layer => Enumerable.Range(0, 3).Select(col => 9 * layer + 3 * row + col == 13 ? ' ' : cube[9 * layer + 3 * row + col - (9 * layer + 3 * row + col >= 13 ? 1 : 0)]).JoinString(" ")).JoinString("    ")}");
            }

            return cube;
        }

        public static string EncrSemaphoreCipher(string word, string kw)
        {
            var semaphores = "45;46;47;04;14;24;34;56;57;02;05;15;25;35;67;06;16;26;36;07;17;03;12;13;27;23".Split(';').Select(str => str.Select(ch => ch - '0').ToArray()).ToArray();
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

        public static void ForwardChainBitRotationExperiment()
        {
            var wordLists = new Data().allWords;
            var rnd = new Random(47);
            var kw = wordLists.PickRandom(rnd).PickRandom(rnd);
            var word = "LETTER";
            //foreach (var word in wordLists[4])
            {
                var (decrypted, residue) = DecrChainBitRotation(word, kw, 0L, debug: true);
                Console.WriteLine($"Word: {word}; KW: {kw}");
                Console.WriteLine($"Decrypted: {decrypted}; residue: {residue}");
                var (reencrypted, residue2) = EncrChainBitRotation(decrypted, kw, residue, debug: true);
                Console.WriteLine($"Reencrypted: {reencrypted}; residue: {residue2}");

                var (redecrypted, residue3) = DecrChainBitRotation(reencrypted, kw, residue2, debug: true);
                Console.WriteLine($"Redecrypted: {redecrypted}; residue: {residue3}");
            }
        }

        public static void BackwardChainBitRotationExperiment()
        {
            var debug = false;
            var wordLists = new Data().allWords;
            var rnd = new Random(47);
            //var kw = wordLists.PickRandom(rnd).PickRandom(rnd);
            //Console.WriteLine($"KW: {kw}");
            var record = 0;
            foreach (var word in wordLists[2].ToArray().Shuffle(rnd))
            {
                var kwsTried = 0;
                foreach (var kw in wordLists.SelectMany(x => x).ToArray().Shuffle(rnd))
                {
                    kwsTried++;
                    for (var testResidue = 0; testResidue < 10; testResidue++)
                    {
                        var (encrypted, residue) = EncrChainBitRotation(word, kw, testResidue, debug);
                        if (encrypted == null)
                            continue;
                        var (decrypted, residue2) = DecrChainBitRotation(encrypted, kw, residue, debug);

                        if (kwsTried > record)
                            record = kwsTried;
                        ConsoleUtil.WriteLine($"Word = {word}; residue = {testResidue,3}; keywords tried = {kwsTried,2}; record = {record,2}".Color(ConsoleColor.Green));
                        goto good;
                    }
                }
                ConsoleUtil.WriteLine($"Word = {word} doesn’t work".Color(ConsoleColor.Red));
                good:;
            }
        }

        private static (string decrypted, long residue) DecrChainBitRotation(string encrypted, string kw, long residue, bool debug = false)
        {
            if (debug)
            {
                Console.WriteLine();
                Console.WriteLine($"DECR: {encrypted}, KW={kw}");
            }
            var number = residue;
            var wordLength = encrypted.Length;
            for (var i = 0; i < wordLength; i++)
            {
                // Process last letter
                var letter = encrypted[encrypted.Length - 1];
                encrypted = encrypted.Substring(0, encrypted.Length - 1);
                number = number * 32 + (letter - 'A' + 1);
                if (debug)
                    Console.WriteLine($"{Convert.ToString((long) number, 2),64} Process {letter}");
                // Reverse the bit rotation
                var nb = (i + 1) * 5;
                var amt = (kw[(wordLength - 1 - i) % kw.Length] - 'A' + 1) % nb;
                number = ((((number >> (nb - amt)) & ((1L << amt) - 1)) | (number << amt)) & ((1L << nb) - 1)) | (number & ~((1L << nb) - 1));
                if (debug)
                    ConsoleUtil.WriteLine($"{Convert.ToString((long) number, 2).PadLeft(nb, '0').Apply(str => str.ColorSubstring(str.Length - nb, nb, ConsoleColor.White, ConsoleColor.DarkBlue)),64} Rotate {nb} bits left {amt}", null);
            }
            var decrypted = "";
            for (var i = 0; i < wordLength; i++)
            {
                var num = number % 26;
                decrypted = (char) (num == 0 ? 'Z' : num + 'A' - 1) + decrypted;
                number /= 26;
            }
            return (decrypted, number);
        }

        private static (string encrypted, long residue) EncrChainBitRotation(string word, string kw, long residue, bool debug = false)
        {
            var encrypted = "";
            var number = word.Aggregate(residue, (p, n) => p * 26 + (n == 'Z' ? 0L : n - 'A' + 1));
            if (debug)
            {
                Console.WriteLine();
                Console.WriteLine($"ENCR: {word}, KW={kw}");
                Console.WriteLine($"{Convert.ToString(number, 2),64}");
            }
            for (var i = 0; i < word.Length; i++)
            {
                // Bit rotation
                var nb = (word.Length - i) * 5;
                var amt = (kw[i % kw.Length] - 'A' + 1) % nb;
                number = (((number >> amt) & ((1L << (nb - amt)) - 1)) | (number << (nb - amt))) & ((1L << nb) - 1) | (number & ~((1L << nb) - 1));
                if (debug)
                    ConsoleUtil.WriteLine($"{Convert.ToString(number, 2).PadLeft(nb, '0').Apply(str => str.ColorSubstring(str.Length - nb, nb, ConsoleColor.White, ConsoleColor.DarkBlue)),64} Rotate {nb} bits right {amt}", null);
                // Extract a letter
                var extracted = (int) (number % 32);
                if (extracted < 1 || extracted > 26)
                    return default;
                encrypted += (char) (extracted + 'A' - 1);
                number /= 32;
                if (debug)
                    Console.WriteLine($"{Convert.ToString(number, 2),64} Extract {encrypted.Last()}");
            }
            return (encrypted, number);
        }
    }
}