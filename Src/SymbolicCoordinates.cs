using System;
using System.Linq;
using System.Windows.Forms;
using RT.TagSoup;
using RT.Util;

namespace KtaneStuff
{
    static class SymbolicCoordinates
    {
        public static void CreateCheatSheet()
        {
            var data = @"PLA;CLA;LAP;ACE;CLE;CEL;ELA;PAL;PCL;ALP;LEC;LPE;EAL;PEC;LCP;AEL;ALE;LEP;EPC;PCE;CAL;CPE;EPL;PAE;ACL;ECP"
                .Split(';')
                .Select((str, ix) => new { Code = str, Char = (char) ('A' + ix) })
                .OrderBy(inf => inf.Code)
                .ToArray();
            var trans = "P=Stain;L=Vortex;A=Tunnel;C=Pluto;E=Flag".Split(';').Select(a => a.Split('=')).ToDictionary(a => a[0][0], a => a[1]);

            var ranges = "10,10,6".Split(',').Select(int.Parse).ToArray();
            for (int k = 0; k < 3; k++)
            {
                Utils.ReplaceInFile(@"D:\c\KTANE\Public\HTML\Symbolic Coordinates cheat sheet (Timwi).html", $"<!--%%{k + 1}-->", $"<!--%%%{k + 1}-->",
                    new TABLE(
                        Enumerable.Range(ranges.Take(k).Sum(), ranges[k]).Select(ix => data[ix].Apply(d => new TR(
                            Enumerable.Range(0, 3).Select(i =>
                                ix == 0 || Enumerable.Range(0, i + 1).Any(j => d.Code[j] != data[ix - 1].Code[j])
                                    ? new TD { rowspan = data.Skip(ix).TakeWhile(inf => inf.Code[i] == d.Code[i] && (i == 0 || inf.Code[i - 1] == d.Code[i - 1])).Count() }._(new IMG { src = $"img /Symbolic Coordinates/{trans[d.Code[i]]}.png", class_ = "image" })
                                    : null),
                                new TD(d.Char)
                           )))
                    ).ToString());
            }
        }
    }
}