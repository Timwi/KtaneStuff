using RT.Util;
using RT.Util.ExtensionMethods;

namespace KtaneStuff;

internal sealed class ExtendedPassword
{
    private string _goalword;
    private string[,] _displaysText = new string[6, 6];
    public static string[] Words = ["ADJUST", "ANCHOR", "BOWTIE", "BUTTON", "CIPHER", "CORNER", "DAMPEN", "DEMOTE", "ENLIST", "EVOLVE", "FORGET", "FINISH", "GEYSER", "GLOBAL", "HAMMER", "HELIUM", "INDIGO", "IGNITE", "JIGSAW", "JULIET", "KARATE", "KEYPAD", "LAMBDA", "LISTEN", "MATTER", "MEMORY", "NEBULA", "NICKEL", "OVERDO", "OXYGEN", "PEANUT", "PHOTON", "QUARTZ", "QUEBEC", "RESIST", "RIDDLE", "SIERRA", "STRIKE", "TEAPOT", "TWENTY", "UNTOLD", "ULTIMA", "VICTOR", "VIOLET", "WITHER", "WRENCH", "XENONS", "XYLOSE", "YELLOW", "YOGURT", "ZENITH", "ZODIAC"];

    private void addWordToDisplaysText(string word)
    {
        var num = word == _goalword ? -1 : Rnd.Next(0, word.Length);
        for (var i = 0; i < _displaysText.GetLength(0); i++)
        {
            var flag = false;
            var num2 = 0;
            while (num2 < _displaysText.GetLength(1) && _displaysText[i, num2] != null)
            {
                if (_displaysText[i, num2] == word.Substring(i, 1))
                {
                    flag = true;
                    break;
                }
                num2++;
            }
            if (num2 < _displaysText.GetLength(1) && !flag && num != i)
            {
                _displaysText[i, num2] = word.Substring(i, 1);
            }
        }
    }

    public void Init()
    {
        var num = Rnd.Next(0, Words.Length);
        _goalword = Words[num];
        var list = new List<string>(Words);
        list.RemoveAt(num);
        addWordToDisplaysText(_goalword);
        while (!displaysTextFull())
        {
            var index = Rnd.Next(0, list.Count);
            addWordToDisplaysText(list[index]);
            list.RemoveAt(index);
            ensureUniqueSolution();
        }
    }

    private void ensureUniqueSolution()
    {
        for (var i = 0; i < Words.Length; i++)
        {
            if (Words[i] != _goalword)
            {
                var num = 0;
                for (var j = 0; j < _displaysText.GetLength(0); j++)
                {
                    for (var k = 0; k < _displaysText.GetLength(1); k++)
                    {
                        if (Words[i].Substring(j, 1) == _displaysText[j, k])
                        {
                            num++;
                            break;
                        }
                    }
                }
                if (num == _displaysText.GetLength(0))
                {
                    var num2 = Rnd.Next(0, _displaysText.GetLength(0));
                    while (_goalword.Substring(num2, 1) == Words[i].Substring(num2, 1))
                    {
                        num2 = Rnd.Next(0, _displaysText.GetLength(0));
                    }
                    removeLetterFromPosition(num2, Words[i].Substring(num2, 1));
                }
            }
        }
    }

    private void removeLetterFromPosition(int position, string letter)
    {
        var num = -1;
        var a = string.Empty;
        var num2 = _displaysText.GetLength(1) - 1;
        for (var i = 0; i < _displaysText.GetLength(1); i++)
        {
            if (_displaysText[position, i] == letter)
            {
                num = i;
            }
            if (_displaysText[position, i] != null)
            {
                a = _displaysText[position, i];
                num2 = i;
            }
        }
        if (num != -1)
        {
            if (a != letter)
            {
                _displaysText[position, num] = _displaysText[position, num2];
            }
            _displaysText[position, num2] = null;
        }
    }

    private bool displaysTextFull()
    {
        var result = true;
        for (var i = 0; i < _displaysText.GetLength(0); i++)
        {
            if (_displaysText[i, _displaysText.GetLength(1) - 1] == null)
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
        for (var i = 0; i < 1000; i++)
        {
            var x = new ExtendedPassword();
            x.Init();
            var displays = Enumerable.Range(0, 6).Select(displayIx => Enumerable.Range(0, x._displaysText.GetLength(1)).Select(chIx => x._displaysText[displayIx, chIx]).JoinString()).ToArray();

            int[] bestPermutation = null;
            var bestNumSolutions = 0;
            foreach (var permutation in permutations)
            {
                var numSolutions = Words.Count(w =>
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
