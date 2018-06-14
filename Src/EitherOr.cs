using System;
using System.IO;
using System.Linq;
using RT.Util.ExtensionMethods;

namespace KtaneStuff
{
    static class EitherOr
    {
        public static void MakeWordList()
        {
            var sowpods = File.ReadAllLines(@"D:\Daten\sowpods.txt").Select(l => l.ToLowerInvariant())
                .GroupBy(s => s.Order().JoinString())
                .ToDictionary(gr => gr.Key, gr => gr.ToList());

            Utils.ReplaceInFile(@"D:\c\KTANE\Public\HTML\Either-Or.html", "<!-- #start -->", "<!-- #end -->",
                File.ReadAllLines(@"D:\temp\words2.txt")
                    .Concat("yearly|wizard|poison|normal|public|labels|merits|liquid|trophy|ballot|fought|empire|strode|plight|intend|combat|gossip|coming|bosses|tricks|ironic|arisen|shells".Split('|'))
                    .Concat("kettle|sodium|marked|barrel|mining|lately|market|gazing|rotten|fisher|retire|liquid|madame|garlic|garden|corpse|phylum|wobbly|sizzle|mantle|elders|trader|smiles|landed|jargon".Split('|'))
                    .Concat("damage|breaks|gather|asylum|sticky|vacant|saving|visits|passes|spoken|insist|fitted|brutal|gloves|design|denial|bronze|voting|stress|greens|losing|hiding|gained|amidst|vastly".Split('|'))
                    .Concat("trucks|rubbed|filthy|patrol|relate|cancel|shirts|combat|emerge|shorts|crises|minute|gospel|tapped|taller|outlet|joking|syntax|marion|header|ceased|bolton|object|convoy".Split('|'))
                    .Concat("coupon|banker|coldly|aboard|burned|spouse|shrine|recall|answer|sunset|orange|cruise|spider|sacked|mosaic|covers|tossed|heroic|double|luxury|excess|builds|onions|object|monkey|grains".Split('|'))
                    .Concat("loving|derive|living|heater|effect|cracks|trauma|render|prince|bounds|dancer|canopy|ragged|floppy|edited|divide|defeat|quiche|squawk|slices|sliced".Split('|'))
                    .Concat("ascent|sipped|shores|ritual|eating|hedges|shaped|upward|unused|enzyme|circus|saving|likely|oddish|polite|rarity|jumper|ground|gifted|abrupt|stands|equals".Split("|"))
                    .Except("russia,joseph,berlin,canada,maggie,greece,brazil,morgan,marion,martin,surrey,morris,norman,equity".Split(','))
                    .Where(word =>
                    {
                        var ana = word.Order().JoinString();
                        var ret = sowpods.ContainsKey(ana) && sowpods[ana].Count == 1;
                        if (word == "sprout")
                            Console.WriteLine(sowpods[ana].JoinString(", "));
                        return ret;
                    })
                    .Order()
                    .Select(word => $"<div>{word}</div>")
                    .JoinString("\n"));
        }
    }
}