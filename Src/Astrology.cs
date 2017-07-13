using System;
using System.Linq;
using RT.TagSoup;

namespace KtaneStuff
{
    static class Astrology
    {
        public static void CreateCheatSheet()
        {
            var zodiacs = "Ari.|Tau.|Gem.|Can.|Leo|Vir.|Lib.|Sco.|Sag.|Cap.|Aqu.|Pis.".Split('|');
            var zodiacsFn = "aries|taurus|gemini|cancer|leo|virgo|libra|scorpio|sagittarius|capricorn|aquarius|pisces".Split('|');
            var elements = "Fire|Wat.|Ear.|Air".Split('|');
            var elementsFn = "fire|water|earth|air".Split('|');
            var planets = "Sun|Moon|Mer.|Ven.|Mar.|Jup.|Sat.|Ura.|Nep.|Plu.".Split('|');
            var planetsFn = "sun|moon|mercury|venus|mars|jupiter|saturn|uranus|neptune|pluto".Split('|');

            var ep = @"0|0|1|-1|0|1|-2|2|0|-1|-2|0|-1|0|2|0|-2|2|0|1|-1|-1|0|-1|1|2|0|2|1|-2|-1|2|-1|0|-2|-1|0|2|-2|2".Split('|').Select(int.Parse).ToArray();
            var ez = @"1|0|-1|0|0|2|2|0|1|0|1|0|2|2|-1|2|-1|-1|-2|1|2|0|0|2|-2|-1|0|0|1|0|1|2|-1|-2|1|1|1|1|-2|-2|2|0|-1|1|0|0|-1|-1".Split('|').Select(int.Parse).ToArray();
            var pz = @"-1|-1|2|0|-1|0|-1|1|0|0|-2|-2|-2|0|1|0|2|0|-1|1|2|0|1|0|-2|-2|-1|-1|1|-1|0|-2|0|0|-1|1|-2|2|-2|0|0|1|-1|0|2|-2|-1|1|-2|0|-1|-2|-2|-2|-1|1|1|1|0|-1|-1|-2|1|-1|0|0|0|1|0|-1|2|0|-1|-1|0|0|1|1|0|0|0|0|-1|-1|-1|2|0|0|1|-2|1|0|2|-1|1|0|1|0|2|1|-1|1|1|1|0|-2|2|0|-1|0|0|-1|-2|1|2|1|1|0|0|-1".Split('|').Select(int.Parse).ToArray();

            if (ep.Length != elements.Length * planets.Length)
                System.Diagnostics.Debugger.Break();
            if (ez.Length != elements.Length * zodiacs.Length)
                System.Diagnostics.Debugger.Break();
            if (pz.Length != planets.Length * zodiacs.Length)
                System.Diagnostics.Debugger.Break();

            Utils.ReplaceInFile(@"D:\c\KTANE\Public\HTML\Astrology cheat sheet (Elias + samfun123 + Timwi).html", "<!--##-->", "<!--###-->",
                new TABLE { class_ = "astrology" }._(Enumerable.Range(0, planets.Length * 2 + elements.Length * 2 + 1).Select(row => new TR(
                    Enumerable.Range(0, zodiacs.Length * 2 + elements.Length * 2 + 1).Select(col =>
                    {
                        var z = col < 25;
                        var p = row < 21;
                        var ze = (col < 25 ? col - 1 : col - 25) / 2;
                        var pe = (row < 21 ? row - 1 : row - 21) / 2;

                        if (row == 0 && col == 0)
                            return new TD { class_ = "borderless" };
                        else if (row == 0)
                            return col % 2 == 0 ? null : new TH { colspan = 2, class_ = z ? null : elementsFn[ze] }._(new IMG { src = $"img/Astrology/{(col < 25 ? "a" : "e")}_{(col < 25 ? zodiacsFn[col / 2] : elementsFn[(col - 25) / 2])}.png", class_ = "astrology-symbol" }, new BR(), col < 25 ? zodiacs[col / 2] : elements[(col - 25) / 2]);
                        else if (col == 0)
                            return row % 2 == 0 ? null : new TH { rowspan = 2, class_ = p ? null : elementsFn[pe] }._(new IMG { src = $"img/Astrology/{(row < 21 ? "p" : "e")}_{(row < 21 ? planetsFn[row / 2] : elementsFn[(row - 21) / 2])}.png", class_ = "astrology-symbol" }, new BR(), row < 21 ? planets[row / 2] : elements[(row - 21) / 2]);
                        else if (col == 25 && row == 21)
                            return new TD { colspan = 8, rowspan = 8, class_ = "borderless" };
                        else if (!z && !p)
                            return null;

                        return
                            z && p ? new TD { class_ = "number" }._(pz[ze + zodiacs.Length * pe] + (col % 2 * 2 - 1) + (row % 2 * 2 - 1)) :
                            !z && p ? new TD { class_ = "number" }._(ep[pe + planets.Length * ze] + (col % 2 * 2 - 1)) :
                            z && !p ? new TD { class_ = "number" }._(ez[ze + zodiacs.Length * pe]) : null;
                    })
                ))).ToString());
        }
    }
}