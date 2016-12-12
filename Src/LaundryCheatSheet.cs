using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using RT.Util;
using RT.Util.ExtensionMethods;

namespace KtaneStuff
{
    static partial class Ktane
    {
        public static void LaundryCheatSheet()
        {
            var abbrevs = @"1|110°C
2|200°C
3|30°
4|40°
5|50°
9|95°
A|Any Solvent
B|Don't Bleach
D|Don't Dryclean
F|300°F
H|Hand
J|Iron
L|Low Heat
N|No Steam
O|No Steam Finish
P|Petroleum Only
R|Reduced Moist
S|Short Cycle
T|No Tetrachlore
W|Wet Cleaning
X|No Chlorine";
            var abbreviate = Ut.Lambda((string str) => abbrevs.Replace("\r", "").Split('\n').FirstOrDefault(s => s.EndsWith("|" + str))?.Apply(s => s.Substring(0, 1)) ?? str);

            var tables = new List<string>();
            for (int material = 0; material < 6; material++)
            {
                var rows = new[] { new List<string>(), new List<string>() };
                for (int color = 0; color < 6; color++)
                {
                    var cells = new[] { new List<string>(), new List<string>() };
                    for (int item = 0; item < 6; item++)
                    {
                        var wash = abbreviate(color == 3 ? "30°" : new[] { "50°", "95°", "Hand", "30°", "40°", "30°" }[material]);
                        var drying = material == 2 ? "3" : new[] { "3", "3", "0", "X", "1", "2" }[color];
                        var ironing = abbreviate(new[] { "300°F", "No Steam", "Iron", "200°C", "300°F", "110°C" }[item]);
                        var specialForItem = abbreviate(new[] { "Bleach", "No Tetrachlore", "Reduced Moist", "Reduced Moist", "Don't Bleach", "Don't Dryclean" }[item]);
                        var specialForColor = abbreviate(new[] { "Any Solvent", "Low Heat", "Short Cycle", "No Steam Finish", "No Chlorine", "No Chlorine" }[color]);
                        var special =
                            color == 4 ? abbreviate("No Chlorine") :
                            (item == 0 || material == 4) ? abbreviate(new[] { "Petroleum Only", "Don't Dryclean", "Reduced Moist", "Low Heat", "Wet Cleaning", "No Tetrachlore" }[material]) :
                            $"<span class='material-match'>{abbreviate(specialForColor)}</span>/{abbreviate(specialForItem)}";
                        cells[material == 5 ? item / 3 : 0].Add($"<td>{wash} <span class='drying'>{drying}</span> {ironing} {special}");
                    }
                    for (int i = 0; i < (material == 5 ? 2 : 1); i++)
                        rows[i].Add($"<tr><th>{color}{cells[i].JoinString()}</tr>");
                }
                for (int i = 0; i < (material == 5 ? 2 : 1); i++)
                    tables.Add($"<div class='material material-{material}'><div class='material-head'>{material} = {new[] { "Polyester", "Cotton", "Wool", "Nylon", "Corduroy", "Leather" }[material]}</div><table class='laundry'><tr><td>I →{Enumerable.Range(3 * i, material == 5 ? 3 : 6).Select(item => $"<th rowspan=2>{item.ToString()}").JoinString()}</tr><tr><td>↓ C</tr>{rows[i].JoinString()}</table></div>");
            }
            tables.Insert(4, tables[5]);
            tables.RemoveAt(6);
            var html = $@"
<div class='abbreviations'>
<ul class='drying'>
<li><span class='drying'>1</span> = •
<li><span class='drying'>2</span> = ••
<li><span class='drying'>3</span> = •••
<li><span class='drying'>O</span> = empty
<li><span class='drying'>X</span> = crossed-out
</ul>
<ul class='main'>{abbrevs.Replace("\r", "").Split('\n').Select(str => $"<li>{str[0]} = {str.Substring(2)}").JoinString()}</ul>
<ul>
<li>I = item = Unsolved modules + indicators
<li>C = color = Last digit of the serial number + batteries</ul></div>
<h2>On the Subject of Laundry</h2>
<div class='top-head'>Material = Ports + Solved Modules − Holders</div>
{tables.JoinString()}
";
            var path = @"D:\c\KTANE\HTML\Laundry (embellished).html";
            File.WriteAllText(path, Regex.Replace(File.ReadAllText(path), @"(?<=<!--##-->).*(?=<!--###-->)", html, RegexOptions.Singleline));
        }
    }
}
