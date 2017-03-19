using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using RT.TagSoup;
using RT.Util;
using RT.Util.ExtensionMethods;
using RT.Util.Text;

namespace KtaneStuff
{
    static partial class Ktane
    {
        public static void DoSillySlotsCheatSheet()
        {
            var tt = new TextTable { ColumnSpacing = 2 };

            var cells = new string[4 * 4 + 1, 3 * 3 * 3 + 1];
            for (int s1s = 0; s1s < 4; s1s++)
                for (int s2s = 0; s2s < 4; s2s++)
                {
                    cells[s2s + 4 * s1s + 1, 0] = "ABCD"[s1s] + "" + "ABCD"[s2s];
                    for (int s1c = 0; s1c < 3; s1c++)
                        for (int s2c = 0; s2c < 3; s2c++)
                            for (int s3c = 0; s3c < 3; s3c++)
                            {
                                cells[0, s3c + 3 * (s2c + 3 * s1c) + 1] = "123"[s1c] + "" + "123"[s2c] + "123"[s3c];
                                var items = new Dictionary<string, string>();
                                for (int s3s = 0; s3s < 4; s3s++)
                                    AddPullInfo("123"[s1c] + "" + "ABCD"[s1s], "123"[s2c] + "" + "ABCD"[s2s], "123"[s3c] + "" + "ABCD"[s3s], items);
                                cells[s2s + 4 * s1s + 1, s3c + 3 * (s2c + 3 * s1c) + 1] = GetCell(items);
                            }
                }

            var path = @"D:\c\KTANE\HTML\Silly Slots cheat sheet (Timwi).html";
            foreach (var inf in new[] { new { Ch = '1', ColRange = Enumerable.Range(0, 2 * 4 + 1) }, new { Ch = '2', ColRange = 0.Concat(Enumerable.Range(2 * 4 + 1, 2 * 4)) } })
                File.WriteAllText(path, Regex.Replace(File.ReadAllText(path), $@"(?<=<!--##{inf.Ch}-->).*(?=<!--###{inf.Ch}-->)", new TABLE { class_ = "solutions" }._(
                    Enumerable.Range(0, 3 * 3 * 3 + 1).Select(row => new TR(
                        inf.ColRange.Select(col =>
                            (cells[col, row] ?? "").Split('\n').InsertBetween<object>(new BR()).Apply(c =>
                                row == 0 || col == 0 ? new TH(c) : (object) new TD(c)
                            )
                        )
                    ))
                ).ToString(), RegexOptions.Singleline));
        }

        private static void SetCell(TextTable tt, int col, int row, Dictionary<string, string> items)
        {
            string str = null;
            if (items.Values.Distinct().Count() == 1)
                str = items.Values.First();
            else
            {
                // Try numbers
                var strs1 = new List<string>();
                for (char n = '1'; n <= '3'; n++)
                {
                    var tItems = items.Where(p => p.Key.StartsWith(n)).ToArray();
                    if ("ABCD".All(ch => items.ContainsKey(n.ToString() + ch)))
                    {
                        var biggestGroup = tItems.GroupBy(p => p.Value).MaxElement(g => g.Count());
                        if (biggestGroup.Count() == 1)
                            biggestGroup = null;
                        foreach (var tItem in tItems)
                            if (biggestGroup == null || !biggestGroup.Any(p => p.Key == tItem.Key))
                                strs1.Add($"{tItem.Key}={tItem.Value}");
                        if (biggestGroup != null)
                            strs1.Add($"{n}={biggestGroup.First().Value}");
                    }
                    else
                        strs1.AddRange(tItems.Select(p => $"{p.Key}={p.Value}"));
                }

                // Try letters
                var strs2 = new List<string>();
                for (char l = 'A'; l <= 'D'; l++)
                {
                    var tItems = items.Where(p => p.Key.EndsWith(l)).ToArray();
                    if ("123".All(ch => items.ContainsKey(ch + l.ToString())))
                    {
                        var biggestGroup = tItems.GroupBy(p => p.Value).MaxElement(g => g.Count());
                        if (biggestGroup.Count() == 1)
                            biggestGroup = null;
                        foreach (var tItem in tItems)
                            if (biggestGroup == null || !biggestGroup.Any(p => p.Key == tItem.Key))
                                strs2.Add($"{tItem.Key}={tItem.Value}");
                        if (biggestGroup != null)
                            strs2.Add($"{l}={biggestGroup.First().Value}");
                    }
                    else
                        strs2.AddRange(tItems.Select(p => $"{p.Key}={p.Value}"));
                }

                str = (strs1.Count > strs2.Count ? strs2 : strs1).JoinString("\n");
            }
            tt.SetCell(col, row, str);
        }

        private static string GetCell(Dictionary<string, string> items)
        {
            if (items.Values.Distinct().Count() == 1)
                return items.Values.First();

            var strs = new List<string>();

            var biggestGroup = items.GroupBy(p => p.Value).MaxElement(g => g.Count());
            if (biggestGroup.Count() == 1)
                biggestGroup = null;
            foreach (var tItem in items)
                if (biggestGroup == null || !biggestGroup.Any(p => p.Key == tItem.Key))
                    strs.Add($"{tItem.Key[1]}={tItem.Value}");
            if (biggestGroup != null)
                strs.Add($"{biggestGroup.First().Value}");

            return strs.JoinString("\n");
        }

        private static void AddPullInfo(string s1, string s2, string s3, Dictionary<string, string> items)
        {
            var ss = new[] { s1, s2, s3 };
            bool? pull = false;
            var pullCondition = "";
            var setCond = Ut.Lambda((string condition) =>
            {
                if (pull == null)
                    pullCondition = pullCondition == null ? condition : $"{pullCondition}&{condition}";
                else if (pull == false)
                {
                    pull = null;
                    pullCondition = condition;
                }
            });
            if (ss.Count(x => x == "2C") == 1)
                pull = true;
            if (ss.Count(x => x == "1A") == 1)
                setCond($"{ss.IndexOf("1A") + 1}^3");
            if (ss.Count(x => x == "3D") >= 2)
                pull = true;
            if (ss.Count(x => x.EndsWith("B")) == 3 && !ss.Contains("1B"))
                pull = true;
            if (s1.EndsWith("C") && new[] { "1A", "2A" }.Contains(s2))
                pull = true;
            if (s2.EndsWith("C") && new[] { "1A", "2A" }.Contains(s1))
                pull = true;
            if (s2.EndsWith("C") && new[] { "1A", "2A" }.Contains(s3))
                pull = true;
            if (s3.EndsWith("C") && new[] { "1A", "2A" }.Contains(s2))
                pull = true;
            if (ss.Count(x => x.StartsWith("2")) == 2 && ss.Count(x => x == "2D") < 2)
                pull = true;
            if (ss.Count(x => x.StartsWith("3")) == 1)
                setCond($"<C");
            if (s2[0] == s1[0] && s3[0] == s1[0] && !ss.Any(x => x.EndsWith("A")))
                setCond($"<2D");
            if (ss.Contains("2B"))
                setCond($"«1C");
            if (s2 == s1 && s3 == s1)
                setCond($"«3D");

            items[s3] = pull == true ? "P" : pull == false ? "K" : pullCondition;
        }

        public static string TransposeTable(string html)
        {
            var rows = html.Replace("</tr>", "")
                .Split("<tr>")
                .Select(r => r.Trim())
                .Where(r => !string.IsNullOrWhiteSpace(r))
                .Select(r => Regex.Matches(r, @"(?<tag><t[dh]( class='[^']+')?>)(?<content>[^<]*)").Cast<Match>().Select(m => new { Tag = m.Groups["tag"].Value, Content = m.Groups["content"].Value }).ToArray())
                .ToArray();

            var l = rows.Max(r => r.Length);
            Console.WriteLine(l);

            return Enumerable.Range(0, l).Select(col => "<tr>" + rows.Select(row => $"{row[col].Tag}{row[col].Content.Trim()}").JoinString()).JoinString("\n");
        }
    }
}
