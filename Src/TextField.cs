using System.Text.RegularExpressions;
using RT.TagSoup;
using RT.Util;
using RT.Util.ExtensionMethods;

namespace KtaneStuff;

internal static class TextField
{
    public static void DoCheatSheet()
    {
        var dic = new Dictionary<char, (string condition, string code)[]>
        {
            ['A'] = Ut.NewArray(
                ("Lit CLR", "1459"),
                ("≥ 3 batteries", "BBFF"),
                ("1 battery", "7F67"),
                ("Lit FRK", "DC52"),
                ("Otherwise", "A0C1")),
            ['C'] = Ut.NewArray(
                ("DVI-D port", "AA12"),
                ("2 batteries", "FB01"),
                ("No vowels in #", "DC52"),
                ("Lit CAR", "1459"),
                ("Otherwise", "7F67")),
            ['E'] = Ut.NewArray(
                ("≤ 2 batteries", "7F67"),
                ("No RCA port", "AA12"),
                ("Lit BOB", "A0C1"),
                ("RJ-45 port", "BBFF"),
                ("Otherwise", "DC52")),
            ['B'] = Ut.NewArray(
                ("No battery", "965A"),
                ("Last digit odd", "1459"),
                ("No serial port", "DC52"),
                ("Lit TRN", "A0C1"),
                ("Otherwise", "7F67")),
            ['D'] = Ut.NewArray(
                ("Parallel port", "FB01"),
                ("≤ 1 battery", "AA12"),
                ("Lit SIG", "BBFF"),
                ("No PS/2 port", "965A"),
                ("Otherwise", "1459")),
            ['F'] = Ut.NewArray(
                ("No serial port", "DC52"),
                ("Vowel in #", "A0C1"),
                ("Lit IND", "1459"),
                ("Last digit even", "FB01"),
                ("Otherwise", "AA12"))
        };

        var tables = new Dictionary<string, string[]>
        {
            ["FB01"] = ["DCFA", "BEFF", "BBBC"],
            ["965A"] = ["CBEF", "EBFE", "DCAA"],
            ["1459"] = ["BABB", "CDFD", "DFCE"],
            ["BBFF"] = ["DABF", "DFBE", "CEBA"],
            ["DC52"] = ["CBDE", "AFDC", "BEBD"],
            ["7F67"] = ["ADCB", "ACBC", "AEFA"],
            ["A0C1"] = ["ECFA", "CFBD", "FFBC"],
            ["AA12"] = ["BEAB", "EDFA", "BCEC"]
        };

        var path = @"D:\c\KTANE\HTML\Text Field cheat sheet (Timwi).html";
        File.WriteAllText(path, File.ReadAllText(path).RegexReplace(@"(?<=<!--##-->).*(?=<!--###-->)",
            dic.OrderBy(kvp => "ADBECF".IndexOf(kvp.Key)).Select(kvp =>
                $@"<div class='text-field'><table class='text-field'>{kvp.Value.Select((tup, ix) => $"<tr>{(ix == 0 ? $"<th class='letter' rowspan='{kvp.Value.Length}'>{kvp.Key}" : null)}<th>{tup.condition}<td>{svg(tables[tup.code], kvp.Key)}").JoinString()}</table></div>"
            ).JoinString(),
            RegexOptions.Singleline));
    }

    private static string svg(string[] table, char key) => $@"<svg class='field' viewBox='-.05 -.05 4.1 3.1'>{Enumerable.Range(0, 3).SelectMany(row => Enumerable.Range(0, 4).Select(col => $"<rect x='{col}' y='{row}' width='1' height='1' fill='{(table[row][col] == key ? "#888" : "none")}' stroke='#000' stroke-width='.1' />")).JoinString()}</svg>";
}
