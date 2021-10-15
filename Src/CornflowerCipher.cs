using System;
using System.Collections.Generic;
using System.Linq;
using RT.Util;
using RT.Util.ExtensionMethods;
using Words;

namespace KtaneStuff
{
    static class CornflowerCipher
    {
        public static void Experiment()
        {
            for (var seed = 0; seed < 100000; seed++)
                foreach (var bit0 in new[] { false, true })
                {
                    var rnd = new Random(seed);

                    var originalRandomWord = Rnd.GenerateString(6, "ABCDEFGHIJKLMNOP", rnd);
                    var abc = originalRandomWord.SelectIndexWhere(c => c <= 'C').ToArray();

                    string kw3 = null;
                    bool works = false;
                    for (var rots = 0; rots < (1 << abc.Length) && !works; rots++)
                    {
                        var randomWord = originalRandomWord.Select((ch, ix) => !abc.Contains(ix) || (rots & (1 << abc.IndexOf(ix))) == 0 ? ch : (char) (ch + 13)).JoinString();

                        var wordList = new Data().allWords;

                        kw3 = FindKW3(bit0, rnd, randomWord, wordList[4]);

                        if (kw3 != null)
                        {
                            Console.WriteLine($"Seed {seed,7}/bit0={bit0,5} random word = {originalRandomWord}{(originalRandomWord != randomWord ? $", rot13ed to {randomWord}" : "")}; KW3={kw3} works");
                            works = true;
                        }
                    }
                }
        }

        private static int[] sequencing(string str)
        {
            return str.Select((ch, ix) => str.Count(c => c < ch) + str.Take(ix).Count(c => c == ch)).ToArray();
        }

        private static string FindKW3(bool bit0, Random rnd, string word, List<string> eightLetterWords)
        {
            string[] brailleDots = { "1", "12", "14", "145", "15", "124", "1245", "125", "24", "245", "13", "123", "134", "1345", "135", "1234", "12345", "1235", "234", "2345", "136", "1236", "2456", "1346", "13456", "1356" };

            while (eightLetterWords.Count > 0)
            {
                var kw3ix = rnd.Next(0, eightLetterWords.Count);
                var kw3 = eightLetterWords[kw3ix];
                eightLetterWords.RemoveAt(kw3ix);
                var colSeq = sequencing(kw3.Substring(0, 4));
                var rowSeq = sequencing(kw3.Substring(4));
                var polybius = (bit0 ? (kw3 + "ABCDEFGHIJKLMNOP") : "ABCDEFGHIJKLMNOP".Except(kw3).Concat(kw3)).Distinct().Where(ch => ch <= 'P').JoinString();

                var stunted = word.Select(ch => polybius.IndexOf(ch)).Select(i => colSeq.IndexOf(i % 4) + 4 * rowSeq.IndexOf(i / 4)).ToArray();

                var braille1 = (stunted[0] & 1) | ((stunted[0] & 4) >> 1) | ((stunted[1] & 1) << 2) | ((stunted[0] & 2) << 2) | ((stunted[0] & 8) << 1) | ((stunted[1] & 2) << 4);
                var braille1ltr = Array.IndexOf(brailleDots, Enumerable.Range(1, 6).Where(bit => (braille1 & (1 << (bit - 1))) != 0).JoinString());
                if (braille1ltr == -1)
                    goto busted;

                var braille2 = ((stunted[1] & 4) >> 2) | ((stunted[2] & 1) << 1) | ((stunted[2] & 4) << 0) | ((stunted[1] & 8) << 0) | ((stunted[2] & 2) << 3) | ((stunted[2] & 8) << 2);
                var braille2ltr = Array.IndexOf(brailleDots, Enumerable.Range(1, 6).Where(bit => (braille2 & (1 << (bit - 1))) != 0).JoinString());
                if (braille2ltr == -1)
                    goto busted;

                var braille3 = (stunted[3] & 1) | ((stunted[3] & 4) >> 1) | ((stunted[4] & 1) << 2) | ((stunted[3] & 2) << 2) | ((stunted[3] & 8) << 1) | ((stunted[4] & 2) << 4);
                var braille3ltr = Array.IndexOf(brailleDots, Enumerable.Range(1, 6).Where(bit => (braille3 & (1 << (bit - 1))) != 0).JoinString());
                if (braille3ltr == -1)
                    goto busted;

                var braille4 = ((stunted[4] & 4) >> 2) | ((stunted[5] & 1) << 1) | ((stunted[5] & 4) << 0) | ((stunted[4] & 8) << 0) | ((stunted[5] & 2) << 3) | ((stunted[5] & 8) << 2);
                var braille4ltr = Array.IndexOf(brailleDots, Enumerable.Range(1, 6).Where(bit => (braille4 & (1 << (bit - 1))) != 0).JoinString());
                if (braille4ltr == -1)
                    goto busted;

                return kw3;

                busted:;
            }
            return null;
        }
    }
}