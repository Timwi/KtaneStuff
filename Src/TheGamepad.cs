using RT.TagSoup;
using RT.Util.ExtensionMethods;

namespace KtaneStuff;

internal static class TheGamepad
{
    public static void DoCheatSheet()
    {
        @"D:\c\KTANE\HTML\The Gamepad cheat sheet (Timwi).html".ReplaceInFile("<!--##-->", "<!--###-->", new TABLE { class_ = "gamepad" }._(
            new TR(new TH("x"), Enumerable.Range(0, 10).Select(i => new TH("-", i))),
            Enumerable.Range(0, 10).Select(row => new TR(
                new TH(row, "-"), Enumerable.Range(0, 10).Select(col => new TD { class_ = cssClass(10 * row + col) }._(row == 0 && col == 0 ? null : cellX(row, col)))
            ))
        ).ToString());
        @"D:\c\KTANE\HTML\The Gamepad cheat sheet (Timwi).html".ReplaceInFile("<!--%%-->", "<!--%%%-->", new TABLE { class_ = "gamepad" }._(
            new TR(new TH("y"), Enumerable.Range(0, 10).Select(i => new TH("-", i))),
            Enumerable.Range(0, 10).Select(row => new TR(
                new TH(row, "-"), Enumerable.Range(0, 10).Select(col => new TD { class_ = cssClass(10 * row + col) }._(row == 0 && col == 0 ? null : cellY(row, col)))
            ))
        ).ToString());
    }

    private static string cssClass(int num) => new[] { _squares.Contains(num) ? "square" : null, _highlyComposite.Contains(num) ? "highly-composite" : null }.Where(x => x != null).JoinString(" ");

    private static int[] _primes = [2, 3, 5, 7, 11, 13, 17, 19, 23, 29, 31, 37, 41, 43, 47, 53, 59, 61, 67, 71, 73, 79, 83, 89, 97];
    private static int[] _squares = [1, 4, 9, 16, 25, 36, 49, 64, 81];
    private static int[] _highlyComposite = [1, 2, 4, 6, 12, 24, 36, 48, 60];

    [Flags]
    private enum skip
    {
        Odd = 1 << 0,
        Y7 = 1 << 1,
        CD = 1 << 2,
        UnlitSND = 1 << 3,
        NoBatteries = 1 << 4,
        StereoRCA = 1 << 5,
        LitFRQ = 1 << 6,
        X7 = 1 << 7,
        AB = 1 << 8,
        PS2 = 1 << 9,
        Batteries2 = 1 << 10
    }

    private static object cellX(int a, int b, skip skip = 0)
    {
        var x = 10 * a + b;
        if (_primes.Contains(x))
            return new DIV { class_ = "else" }._(decorate("▲▲▼▼"));
        if (x % 12 == 0)
            return new DIV { class_ = "else" }._(decorate("▲A◀◀"));
        if (a + b == 10 && !skip.HasFlag(skip.Odd))
            return new object[] { new SPAN { class_ = "colored serial-number-odd" }._("\u00a0"), new WBR(), cellX(a, b, skip | skip.Odd) };
        if ((x - 3) % 6 == 0 || (x - 5) % 10 == 0)
            return new DIV { class_ = "else" }._(decorate("▼◀A▶"));
        if (x % 7 == 0 && !skip.HasFlag(skip.Y7))
            return new object[] { new SPAN { class_ = "colored y-not-7n" }._("\u00a0"), new WBR(), cellX(a, b, skip | skip.Y7) };
        if (!skip.HasFlag(skip.CD))
            return new object[] { new SPAN { class_ = "colored x-is-c-times-d" }._("\u00a0"), new WBR(), cellX(a, b, skip | skip.CD) };
        if (_squares.Contains(x))
            return new DIV { class_ = "else" }._(decorate("▶▶A▼"));
        if ((x + 1) % 3 == 0)
            return new DIV { class_ = "else" }._(decorate("▶AB▲"));
        if (!skip.HasFlag(skip.UnlitSND))
            return new object[] { new SPAN { class_ = "colored unlit-snd" }._("\u00a0"), new WBR(), cellX(a, b, skip | skip.UnlitSND) };
        if (x >= 60 && x < 90 && !skip.HasFlag(skip.NoBatteries))
            return new object[] { new SPAN { class_ = "colored no-batteries" }._("\u00a0"), new WBR(), cellX(a, b, skip | skip.NoBatteries) };
        if (x % 6 == 0)
            return new DIV { class_ = "else" }._(decorate("ABA▶"));
        return x % 4 == 0 ? new DIV { class_ = "else" }._(decorate("▼▼◀▲")) : new DIV { class_ = "else" }._(decorate("A◀B▶"));
    }

    private static object cellY(int c, int d, skip skip = 0)
    {
        var y = 10 * c + d;
        if (_primes.Contains(y))
            return new DIV { class_ = "else" }._(decorate("◀▶◀▶"));
        if (y % 8 == 0)
            return new DIV { class_ = "else" }._(decorate("▼▶B▲"));
        if (c - d == 4 && !skip.HasFlag(skip.StereoRCA))
            return new object[] { new SPAN { class_ = "colored stereo-rca" }._("\u00a0"), new WBR(), cellY(c, d, skip | skip.StereoRCA) };
        if ((y - 2) % 4 == 0)
            return new DIV { class_ = "else" }._(decorate("B▲▶A"));
        if (!skip.HasFlag(skip.LitFRQ))
            return new object[] { new SPAN { class_ = "colored lit-frq" }._("\u00a0"), new WBR(), cellY(c, d, skip | skip.LitFRQ) };
        if (y % 7 == 0 && !skip.HasFlag(skip.X7))
            return new object[] { new SPAN { class_ = "colored x-not-7n" }._("\u00a0"), new WBR(), cellY(c, d, skip | skip.X7) };
        if (_squares.Contains(y))
            return new DIV { class_ = "else" }._(decorate("▲▼B▶"));
        if (!skip.HasFlag(skip.AB))
            return new object[] { new SPAN { class_ = "colored y-is-a-times-b" }._("\u00a0"), new WBR(), cellY(c, d, skip | skip.AB) };
        if ((y + 1) % 4 == 0)
            return new DIV { class_ = "else" }._(decorate("▲BBB"));
        if (!skip.HasFlag(skip.PS2))
            return new object[] { new SPAN { class_ = "colored ps2" }._("\u00a0"), new WBR(), cellY(c, d, skip | skip.PS2) };
        if (c > d && !skip.HasFlag(skip.Batteries2))
            return new object[] { new SPAN { class_ = "colored two-or-more-batteries" }._("\u00a0"), new WBR(), cellY(c, d, skip | skip.Batteries2) };
        if (y % 5 == 0)
            return new DIV { class_ = "else" }._(decorate("BAB◀"));
        return y % 3 == 0 ? new DIV { class_ = "else" }._(decorate("▶▲▲◀")) : new DIV { class_ = "else" }._(decorate("B▲A▼"));
    }

    private static object decorate(string s) => s.Select(ch => "AB".Contains(ch) ? (object) ch : new SPAN { class_ = "arrow" }._(ch));
}