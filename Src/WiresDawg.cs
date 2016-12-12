using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using OnlineDAWG;
using RT.TagSoup;
using RT.Util;
using RT.Util.ExtensionMethods;

namespace KtaneStuff
{
    static partial class Ktane
    {
        public static void DoWiresDawg()
        {
            var colors = "BKRWY?";
            var graph = new DawgGraph();

            var str = Ut.Lambda((int num, string cables, char? ser) => ((char) ('①' - 1 + num)).ToString() + cables + ser);

            var com3 =
                from c1 in colors
                from c2 in colors
                from c3 in colors
                let cables = c1.ToString() + c2 + c3
                select str(3, cables, null) + (
                    !cables.Contains("R") ? "2" :
                    c3 == 'W' ? "3" :
                    cables.Count(c => c == 'B') > 1 ? (cables.LastIndexOf('B') + 1).ToString() :
                    "3"
                );

            var com4 =
                from c1 in colors
                from c2 in colors
                from c3 in colors
                from c4 in colors
                from ser in "oe"    // serial number odd/even
                let cables = c1.ToString() + c2 + c3 + c4
                select str(4, cables, ser) + (
                    cables.Count(c => c == 'R') > 1 && ser == 'o' ? (cables.LastIndexOf('R') + 1).ToString() :
                    c4 == 'Y' && !cables.Contains('R') ? "1" :
                    cables.Count(c => c == 'B') == 1 ? "1" :
                    cables.Count(c => c == 'Y') > 1 ? "4" :
                    "2"
                );

            var com5 =
                from c1 in colors
                from c2 in colors
                from c3 in colors
                from c4 in colors
                from c5 in colors
                from ser in "oe"    // serial number odd/even
                let cables = c1.ToString() + c2 + c3 + c4 + c5
                select str(5, cables, ser) + (
                    // If the last wire is black and the last digit of the serial number is odd, cut the fourth wire.
                    c5 == 'K' && ser == 'o' ? "4" :
                    // Otherwise, if there is exactly one red wire and there is more than one yellow wire, cut the first wire.
                    cables.Count(c => c == 'R') == 1 && cables.Count(c => c == 'Y') > 1 ? "1" :
                    // Otherwise, if there are no black wires, cut the second wire.
                    !cables.Contains('K') ? "2" :
                    // Otherwise, cut the first wire.                    
                    "1"
                );

            var com6 =
                from c1 in colors
                from c2 in colors
                from c3 in colors
                from c4 in colors
                from c5 in colors
                from c6 in colors
                from ser in "oe"    // serial number odd/even
                let cables = c1.ToString() + c2 + c3 + c4 + c5 + c6
                select str(6, cables, ser) + (
                    // If there are no yellow wires and the last digit of the serial number is odd, cut the third wire.
                    !cables.Contains('Y') && ser == 'o' ? "3" :
                    // Otherwise, if there is exactly one yellow wire and there is more than one white wire, cut the fourth wire.
                    cables.Count(c => c == 'Y') == 1 && cables.Count(c => c == 'W') > 1 ? "4" :
                    // Otherwise, if there are no red wires, cut the last wire.
                    !cables.Contains('R') ? "6" :
                    // Otherwise, cut the fourth wire.
                    "4"
                );

            var strs = com3.Concat(com4).Concat(com5).Concat(com6).ToList();
            strs = com4.ToList();
            File.WriteAllLines(@"D:\temp\temp.txt", strs);
            foreach (var combination in strs)
                //foreach (var combination in com3)
                graph.Add(combination);

            var nodes = graph.Nodes.ToArray();
            var shortcuts = new int?[nodes.Length];
            while (true)
            {
                var anyChange = false;

                for (int i = 0; i < nodes.Length; i++)
                {
                    if (shortcuts[i] != null)
                        continue;

                    int answer;
                    if (nodes[i].Cs.Length == 1 && int.TryParse(nodes[i].Cs[0].ToString(), out answer))
                    {
                        shortcuts[i] = answer;
                        anyChange = true;
                        continue;
                    }

                    var distinct = nodes[i].Ns.Distinct().ToArray();
                    if (distinct.Length == 1)
                    {
                        var ix = nodes.IndexOf(distinct[0]);
                        if (shortcuts[ix] != null)
                        {
                            shortcuts[i] = shortcuts[ix];
                            anyChange = true;
                            continue;
                        }
                    }
                }

                if (!anyChange)
                    break;
            }

            var sb = new StringBuilder();

            var filteredNodes = nodes.Where((n, i) => !n.Accepting && shortcuts[i] == null).ToHashSet();
            var sortedNodes = new List<DawgNode>();
            var queue = new Queue<DawgNode>();
            queue.Enqueue(nodes[0]);
            while (queue.Count > 0)
            {
                var node = queue.Dequeue();
                if (!filteredNodes.Remove(node))
                    continue;
                if (sortedNodes.Contains(node))
                    continue;
                sortedNodes.Add(node);
                queue.EnqueueRange(node.Ns);
            }

            var tds = new List<TD>();
            for (int i = 0; i < sortedNodes.Count; i++)
            {
                var dic = sortedNodes[i].Cs.Select((c, j) => Ut.KeyValuePair(c == '?' ? "else" : c.ToString(), sortedNodes[i].Ns[j])).ToDictionary();
                if (dic.ContainsKey("else"))
                    dic.RemoveAllByKey(k => k != "else" && dic[k] == dic["else"]);

                tds.Add(new TD(
                    new DIV { class_ = "label" }._(i),
                    new UL(
                        dic.OrderBy(p => p.Key == "else" ? 1 : 0).Select(kvp =>
                        {
                            var shortcut = shortcuts[nodes.IndexOf(kvp.Value)];
                            return new LI("{0} → {1}".Fmt(kvp.Key == "else" && dic.Count == 1 ? null : kvp.Key,
                                shortcut.NullOr(s => "cut #" + s) ?? (object) "{0}".Fmt(sortedNodes.IndexOf(kvp.Value))));
                        }))));
            }

            var rows = 20;
            var cols = (tds.Count + rows - 1) / rows;
            var html = new HTML(
                new HEAD(
                    new TITLE("On the Subject of Wires"),
                    new META { httpEquiv = "Content-Type", content = "text/html; charset=utf-8" },
                    new STYLELiteral(@"
table {
    border-collapse: collapse;
}
td {
    vertical-align: top;
    border: 1px solid #ccc;
    position: relative;
}
.label {
    position: absolute;
    top: 0;
    left: .25em;
    font-size: 200%;
    font-weight: bold;
    color: #ccc;
}
ul {
    list-style-type: none;
}
                    ")),
                new BODY(
                    new TABLE(
                        Enumerable.Range(0, rows).Select(row => new TR(
                            Enumerable.Range(0, cols).Select(col => (rows * col + row).Apply(ix => ix < tds.Count ? tds[ix] : new TD())))))));

            File.WriteAllText(@"D:\temp\temp.html", Tag.ToString(html));
        }
    }
}
