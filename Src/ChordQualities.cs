using System.Linq;
using System.Text;
using System.Windows.Forms;
using KtaneStuff.Modeling;
using RT.Util.ExtensionMethods;

namespace KtaneStuff
{
    public static class ChordQualities
    {
        public static void CheatSheet()
        {
            var data = @"
7|4332|G
-7|3432|G♯
Δ7|4341|A♯
-Δ7|3441|F
7♯9|3162|A
ø|3342|C♯
add9|2235|D♯
-add9|2145|E
7♯5|4422|F♯
Δ7♯5|4431|C
7sus|5232|D
-Δ7♯5|3531|B
"
                .Trim().Replace("\r", "").Split('\n').Select(l => l.Split('|')).Select(arr => new { Chord = arr[0], Gaps = arr[1], Note = arr[2] });

            var newInf = data.SelectMany(inf => Enumerable.Range(0, 4).Select(i => new
            {
                Gaps = inf.Gaps.Substring(i) + inf.Gaps.Substring(0, i),
                RootIndex = (4 - i) % 4,
                NewRoot = inf.Note
            })).ToArray();

            var sb = new StringBuilder();
            foreach (var inf in newInf.OrderBy(nf => nf.Gaps))
                sb.AppendLine(@"<tr><th>{0}<span class='root'>{1}</span>{2}<td>{3}</tr>".Fmt(inf.Gaps.Substring(0, inf.RootIndex), inf.Gaps.Substring(inf.RootIndex, 1), inf.Gaps.Substring(inf.RootIndex + 1), inf.NewRoot));
            Clipboard.SetText(sb.ToString());

            var data2 = @"
A|-Δ7♯5
A♯|Δ7♯5
B|-7
C|ø
C♯|-add9
D|Δ7
D♯|7♯9
E|7sus
F|add9
F♯|7
G|-Δ7
G♯|7♯5".Trim().Replace("\r", "").Split('\n').Select(l => l.Split('|')).Select(arr => new { Root = arr[0], Gaps = data.First(d => d.Chord.Equals(arr[1])).Gaps }).ToArray();

            sb = new StringBuilder();
            foreach (var inf in data2)
                sb.AppendLine(@"<tr><th>{0}<td>{1}</tr>".Fmt(inf.Root, inf.Gaps));
            Clipboard.SetText(sb.ToString());
        }
    }
}
