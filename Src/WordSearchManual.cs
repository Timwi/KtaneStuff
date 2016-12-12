using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using RT.Util;
using RT.Util.ExtensionMethods;

namespace KtaneStuff
{
    using static Modeling.Md;

    static partial class Ktane
    {
        public static void WordSearchManual()
        {
            var rnd = new Random(1);
            var letters = Enumerable.Range('A', 26).Select(i => (char) i).ToList().Shuffle(rnd);
            var s = "";
            var wordPairs = "Hotel/Done;Search/Quebec;Add/Check;Sierra/Find;Finish/East;Port/Color;Boom/Submit;Line/Blue;Kaboom/Echo;Panic/False;Manual/Alarm;Decoy/Call;See/Twenty;India/North;Number/Look;Zulu/Green;Victor/Xray;Delta/Yes;Help/Locate;Romeo/Beep;True/Expert;Mike/Edge;Found/Red;Bombs/Word;Work/Unique;Test/Jinx;Golf/Letter;Talk/Six;Bravo/Serial;Seven/Timer;Module/Spell;List/Tango;Yankee/Solve;Chart/Oscar;Math/Next;Read/Listen;Lima/Four;Count/Office"
                .Split(';').Select(pairStr => pairStr.Split('/').Apply(arr => new { One = arr[0], Two = arr[1] })).ToArray();
            for (int i = 0; i < wordPairs.Length; i++)
            {
                var x = (i < 5 ? i + 1 : i >= 33 ? i - 32 : (i - 5) % 7);
                var y = (i < 5 ? 0 : i >= 33 ? 5 : (i + 2) / 7);
                s += $"<div class='box' style='left: {x * 25}mm; top: {y * 25}mm;'><div class='content'>{wordPairs[i].One.ToUpperInvariant()}\n—\n{wordPairs[i].Two.ToUpperInvariant()}</div></div>";
            }

            for (int i = 0; i < letters.Count; i++)
            {
                var x = (i < 4 ? i + 1 : i >= 22 ? i - 21 : (i - 4) % 6) + 1;
                var y = (i < 4 ? 0 : i >= 22 ? 4 : (i - 4) / 6 + 1) + 1;
                s += $"<div class='letter' style='left: {x * 25}mm; top: {y * 25}mm;' data-letter='{letters[i]}'></div>";
            }

            s += $"<div class='box hint' style='left: 0; top: 0;'><div class='content'>[even]\n—\n[odd]</div></div>";

            var path = @"D:\c\KTANE\WordSearch\Manual\Word Search.html";
            File.WriteAllText(path, Regex.Replace(File.ReadAllText(path), @"(?<=<!--##-->).*(?=<!--###-->)", s, RegexOptions.Singleline));
        }
    }
}
