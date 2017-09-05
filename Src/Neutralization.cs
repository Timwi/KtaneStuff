using System;
using System.Linq;
using RT.TagSoup;
using RT.Util;
using RT.Util.ExtensionMethods;

namespace KtaneStuff
{
    public static class Neutralization
    {
        public static void CreateCheatSheet()
        {
            var acids = Ut.NewArray(
                new { Color = "Red", Acid = "HBr", Anion = "Br", AtomicNumber = 35 },
                new { Color = "Yellow", Acid = "HF", Anion = "F", AtomicNumber = 9 },
                new { Color = "Green", Acid = "HCl", Anion = "Cl", AtomicNumber = 17 },
                new { Color = "Blue", Acid = "HI", Anion = "I", AtomicNumber = 53 }
            );
            var bases = Ut.NewArray(
                new { Base = "NH₃", Cation = "H", AtomicNumber = 1, ClosestConcentration = 5, FilterOff = "HBr|HI".Split('|') },
                new { Base = "LiOH", Cation = "Li", AtomicNumber = 3, ClosestConcentration = 5, FilterOff = "HCl|HI".Split('|') },
                new { Base = "NaOH", Cation = "Na", AtomicNumber = 11, ClosestConcentration = 10, FilterOff = "HBr|HF".Split('|') },
                new { Base = "KOH", Cation = "K", AtomicNumber = 19, ClosestConcentration = 20, FilterOff = "HF|HI".Split('|') }
            );

            var indicators = "SND|CLR|CAR|IND|FRQ|SIG|NSA|MSA|TRN|BOB|FRK|NLL".Split('|').Order().ToArray();

            Utils.ReplaceInFile(@"D:\c\KTANE\Public\HTML\Neutralization cheat sheet (Timwi).html", "<!--%%-->", "<!--%%%-->",
                new TABLE(
                    new TR(new TD { class_ = "corner" }, acids.Select(a => new TH(a.Color))),
                    new TR(
                        new TD { class_ = "corner" },
                        acids.Select(acid => new TD { class_ = "rules-for-base" }._(new TABLE(
                            new TR(new TH("NSA & 3b"), new TD("NH₃")),
                            new TR(new TH("lit CAR/", new WBR(), "FRQ/IND"), new TD("KOH")),
                            new TR(new TH("0 ports & vwl"), new TD("LiOH")),
                            new TR(new TH("ind.w. ", acid.Acid.ToUpperInvariant().Distinct().Order().JoinString(","), " e.g. ", indicators.Where(i => i.Any(acid.Acid.ToUpperInvariant().Contains)).InsertBetween("/").InsertBetween<object>(new WBR())), new TD("KOH")),
                            new TR(new TH("D > AA"), new TD("NH₃")),
                            new TR(new TH("else"), new TD(acid.AtomicNumber < 20 ? "NaOH" : "LiOH"))
                        )))
                    ),
                    bases.Select(base_ => new TR(
                        new TH(base_.Base),
                        acids.Select(acid =>
                        {
                            var acidConcentration = acid.AtomicNumber - base_.AtomicNumber;
                            if ((acid.Anion + base_.Cation).Any("AEIOUaeiou".Contains))
                                acidConcentration -= 4;
                            if (acid.Anion.Length == base_.Cation.Length)
                                acidConcentration *= 3;
                            acidConcentration = Math.Abs(acidConcentration) % 10;
                            Func<int, int> acidConcentrationFunc = volume => acidConcentration == 0 ? volume * 2 : acidConcentration * 5;

                            return new TD { class_ = base_.FilterOff.Contains(acid.Acid) ? "filter-off" : "filter-on" }._(() =>
                            {
                                if ((acid.Acid == "HI" && base_.Base == "KOH") || (acid.Acid == "HCl" && base_.Base == "NH₃"))
                                    return new TABLE(new TR(new TD { class_ = "corner", colspan = 2 }), new[] { 5, 10, 15, 20 }.Select(volume => new TR(new TH(volume), new TD(volume * acidConcentrationFunc(volume) / 50))));

                                return new TABLE(
                                    new TR { class_ = "criteria" }._(new TD { class_ = "corner" }, new TH("bh"), new TH("pt"), new TH("i"), new TH("=")),
                                    new[] { 5, 10, 15, 20 }.Select(volume => new TR(new TH(volume),
                                        new[] { 5, 10, 20, base_.ClosestConcentration }.Select(baseConcentration => new TD(2 * volume * acidConcentrationFunc(volume) / baseConcentration / 5))))
                                );
                            });
                        })
                    ))
                ).ToString());
        }
    }
}