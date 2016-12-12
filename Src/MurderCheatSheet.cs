using System.Linq;
using System.Windows.Forms;
using RT.Util;
using RT.Util.ExtensionMethods;

namespace KtaneStuff
{
    static partial class Ktane
    {
        public static void DoMurderCheatSheet()
        {
            var suspectNames = "SPKGMW";
            var weaponNames = "CDPVRS";

            var tableRaw = @"D|Y|L|K|S|C
S|H|B|L|K|Y
K|B|A|Y|C|D
L|A|D|C|H|K
B|K|S|A|D|H
C|L|Y|S|B|A
A|C|K|H|Y|S
H|S|C|D|L|B
Y|D|H|B|A|L";
            var table = tableRaw.Replace("\r", "").Split('\n').Select(rowRaw => rowRaw.Split('|')).ToArray();

            var suspects = @"lit TRN|5
Dining Room|7
≥ 2×RCA|8
no D batteries|2
Study|4
≥ 5 batteries|9
unlit FRQ|1
Conservatory|3
otherwise|6".Replace("\r", "").Split('\n').Select(item => item.Split('|').Apply(arr => new { Label = arr[0], Row = int.Parse(arr[1]) - 1 })).ToArray();

            var weapons = @"Lounge|3
≥ 5 batteries|1
serial port|9
Billiard Room|4
no batteries|6
no lit indicators|5
Hall|7
≥ 2×RCA|2
otherwise|8".Replace("\r", "").Split('\n').Select(item => item.Split('|').Apply(arr => new { Label = arr[0], Row = int.Parse(arr[1]) - 1 })).ToArray();

            var contradictions = @"lit TRN|no lit indicators
Dining Room|Lounge
Dining Room|Billiard Room
Dining Room|Hall
Study|Lounge
Study|Billiard Room
Study|Hall
≥ 5 batteries|no batteries
Conservatory|Lounge
Conservatory|Billiard Room
Conservatory|Hall".Replace("\r", "").Split('\n').Select(line => line.Split('|')).ToArray();

            Clipboard.SetText($@"
<table class='solutions'>
    <tr><td>{suspects.Select(s => $"<th class='rotated'><span>{s.Label}</span>").JoinString()}</tr>
    {weapons.Select(w => $@"<tr><th>{w.Label}{
        suspects.Select(s => $@"<td>{Enumerable.Range(0, 6).SelectMany(i => Enumerable.Range(0, 6).Select(j => i != j && table[w.Row][i] == table[s.Row][j] && !contradictions.Any(c => c[1] == w.Label && c[0] == s.Label) ? $"{suspectNames[j]}{weaponNames[i]}{table[w.Row][i]}" : null).Where(str => str != null)).Order().JoinString("<br>")}").JoinString()
    }</tr>").JoinString()}
</table>
");
        }
    }
}
