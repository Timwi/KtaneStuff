using System;
using System.Collections.Generic;
using System.Linq;
using RT.Util;
using RT.Util.ExtensionMethods;

namespace KtaneStuff
{
    sealed class ExtendedPassword
    {
        private string goalword;
        private string[,] displaysText = new string[6, 6];
        public static string[] words = new string[] { "ADJUST", "ANCHOR", "BOWTIE", "BUTTON", "CIPHER", "CORNER", "DAMPEN", "DEMOTE", "ENLIST", "EVOLVE", "FORGET", "FINISH", "GEYSER", "GLOBAL", "HAMMER", "HELIUM", "INDIGO", "IGNITE", "JIGSAW", "JULIET", "KARATE", "KEYPAD", "LAMBDA", "LISTEN", "MATTER", "MEMORY", "NEBULA", "NICKEL", "OVERDO", "OXYGEN", "PEANUT", "PHOTON", "QUARTZ", "QUEBEC", "RESIST", "RIDDLE", "SIERRA", "STRIKE", "TEAPOT", "TWENTY", "UNTOLD", "ULTIMA", "VICTOR", "VIOLET", "WITHER", "WRENCH", "XENONS", "XYLOSE", "YELLOW", "YOGURT", "ZENITH", "ZODIAC" };

        private void addWordToDisplaysText(string word)
        {
            int num;
            if (word == this.goalword)
            {
                num = -1;
            }
            else
            {
                num = Rnd.Next(0, word.Length);
            }
            for (int i = 0; i < this.displaysText.GetLength(0); i++)
            {
                bool flag = false;
                int num2 = 0;
                while (num2 < this.displaysText.GetLength(1) && this.displaysText[i, num2] != null)
                {
                    if (this.displaysText[i, num2] == word.Substring(i, 1))
                    {
                        flag = true;
                        break;
                    }
                    num2++;
                }
                if (num2 < this.displaysText.GetLength(1) && !flag && num != i)
                {
                    this.displaysText[i, num2] = word.Substring(i, 1);
                }
            }
        }

        public void Init()
        {
            int num = Rnd.Next(0, words.Length);
            this.goalword = words[num];
            List<string> list = new List<string>(words);
            list.RemoveAt(num);
            this.addWordToDisplaysText(this.goalword);
            while (!this.displaysTextFull())
            {
                int index = Rnd.Next(0, list.Count);
                this.addWordToDisplaysText(list[index]);
                list.RemoveAt(index);
                this.ensureUniqueSolution();
            }
        }

        private void ensureUniqueSolution()
        {
            for (int i = 0; i < words.Length; i++)
            {
                if (words[i] != this.goalword)
                {
                    int num = 0;
                    for (int j = 0; j < this.displaysText.GetLength(0); j++)
                    {
                        for (int k = 0; k < this.displaysText.GetLength(1); k++)
                        {
                            if (words[i].Substring(j, 1) == this.displaysText[j, k])
                            {
                                num++;
                                break;
                            }
                        }
                    }
                    if (num == this.displaysText.GetLength(0))
                    {
                        int num2 = Rnd.Next(0, this.displaysText.GetLength(0));
                        while (this.goalword.Substring(num2, 1) == words[i].Substring(num2, 1))
                        {
                            num2 = Rnd.Next(0, this.displaysText.GetLength(0));
                        }
                        this.removeLetterFromPosition(num2, words[i].Substring(num2, 1));
                    }
                }
            }
        }

        private void removeLetterFromPosition(int position, string letter)
        {
            int num = -1;
            string a = string.Empty;
            int num2 = this.displaysText.GetLength(1) - 1;
            for (int i = 0; i < this.displaysText.GetLength(1); i++)
            {
                if (this.displaysText[position, i] == letter)
                {
                    num = i;
                }
                if (this.displaysText[position, i] != null)
                {
                    a = this.displaysText[position, i];
                    num2 = i;
                }
            }
            if (num != -1)
            {
                if (a != letter)
                {
                    this.displaysText[position, num] = this.displaysText[position, num2];
                }
                this.displaysText[position, num2] = null;
            }
        }

        private bool displaysTextFull()
        {
            bool result = true;
            for (int i = 0; i < this.displaysText.GetLength(0); i++)
            {
                if (this.displaysText[i, this.displaysText.GetLength(1) - 1] == null)
                {
                    result = false;
                }
            }
            return result;
        }

        public static void DoStatistics()
        {
            const int numColumns = 3;

            var permutations = new[] { 0, 1, 2, 3, 4, 5 }.Subsequences().Where(s => s.Count() == numColumns).Select(s => s.ToArray()).ToArray();
            var bestPermutationCounts = new Dictionary<string, int>();
            var wordCounts = new Dictionary<int, int>();
            for (int i = 0; i < 1000; i++)
            {
                var x = new ExtendedPassword();
                x.Init();
                var displays = Enumerable.Range(0, 6).Select(displayIx => Enumerable.Range(0, x.displaysText.GetLength(1)).Select(chIx => x.displaysText[displayIx, chIx]).JoinString()).ToArray();

                int[] bestPermutation = null;
                int bestNumSolutions = 0;
                foreach (var permutation in permutations)
                {
                    var numSolutions = words.Count(w =>
                    {
                        foreach (var ix in permutation)
                            if (!displays[ix].Contains(w[ix]))
                                return false;
                        return true;
                    });
                    if (bestPermutation == null || numSolutions < bestNumSolutions)
                    {
                        bestPermutation = permutation;
                        bestNumSolutions = numSolutions;
                    }
                }
                bestPermutationCounts.IncSafe(bestPermutation.JoinString());
                wordCounts.IncSafe(bestNumSolutions);
            }

            Console.WriteLine(bestPermutationCounts.OrderByDescending(kvp => kvp.Value).Select(kvp => $"{kvp.Key} = {kvp.Value}").JoinString("\n"));
            Console.WriteLine("---");
            Console.WriteLine(wordCounts.OrderByDescending(kvp => kvp.Value).Select(kvp => $"{kvp.Key} = {kvp.Value}").JoinString("\n"));
        }
    }
}