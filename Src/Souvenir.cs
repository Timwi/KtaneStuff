using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using KtaneStuff.Modeling;
using RT.Util;
using RT.Util.ExtensionMethods;
using RT.Util.Text;

namespace KtaneStuff
{
    using static Md;

    static class Souvenir
    {
        public static void DoModels()
        {
            File.WriteAllText(@"D:\c\KTANE\Souvenir\Assets\Models\AnswerHighlightVeryLong.obj", GenerateObjFile(AnswerHighlight(.39), "AnswerHighlightVeryLong"));
            File.WriteAllText(@"D:\c\KTANE\Souvenir\Assets\Models\AnswerHighlightLong.obj", GenerateObjFile(AnswerHighlight(.21), "AnswerHighlightLong"));
            File.WriteAllText(@"D:\c\KTANE\Souvenir\Assets\Models\AnswerHighlightShort.obj", GenerateObjFile(AnswerHighlight(.15), "AnswerHighlightShort"));
        }

        private static IEnumerable<Pt[]> AnswerHighlight(double width)
        {
            var height = .08;
            var padding = .02;
            var right = width - height / 2;
            return
                Enumerable.Range(0, 37)
                    .Select(i => i * 180 / 36 + 90)
                    .Select(angle => new { Inner = pt((height - padding) / 2 * cos(angle), 0, (height - padding) / 2 * sin(angle)), Outer = pt(height / 2 * cos(angle), 0, height / 2 * sin(angle)) })
                    .SelectConsecutivePairs(false, (i1, i2) => new[] { i1.Inner, i1.Outer, i2.Outer, i2.Inner })
                .Concat(new[] { pt(0, 0, -height / 2), pt(right, 0, -height / 2), pt(right, 0, -(height - padding) / 2), pt(0, 0, -(height - padding) / 2) })
                .Concat(new[] { pt(0, 0, (height - padding) / 2), pt(right, 0, (height - padding) / 2), pt(right, 0, height / 2), pt(0, 0, height / 2) })
                .Concat(new[] { pt(right, 0, -height / 2), pt(right, 0, height / 2), pt(right - height / 3, 0, 0) });
        }

        /// <summary>Uses the list in `CONTRIBUTORS-raw.md` to generate the nicely-formatted tables in `CONTRIBUTORS.md`.</summary>
        public static void GenerateContributorsMd()
        {
            const string SouvenirPath = @"D:\c\KTANE\Souvenir";
            const int numColumns = 5;

            var groupedByAuthor = File.ReadLines($@"{SouvenirPath}\CONTRIBUTORS-raw.md")
                .Select(line => Regex.Match(line, @"^- (.*) — (.*)$"))
                .Select(m => (author: m.Groups[2].Value, module: m.Groups[1].Value.Apply(s => s.StartsWith("The ") ? $"{s.Substring(4)}, The" : s)))
                .GroupBy(t => t.author)
                .ToArray();

            var sb = new StringBuilder();
            sb.Append("# Souvenir implementors\n\nThe following is a list of modules supported by Souvenir, and the fine people who have contributed their effort to make it happen:\n\n\n");
            foreach (var group in groupedByAuthor.Where(gr => gr.Count() > numColumns).OrderByDescending(gr => gr.Count()).ThenBy(gr => gr.Key))
            {
                sb.Append($"## Implemented by {group.Key} ({group.Count()})\n\n");
                var tt = new TextTable { ColumnSpacing = 5, VerticalRules = true };
                var numItems = group.Count();
                var numRows = (numItems + numColumns - 1) / numColumns;
                var col = 0;
                foreach (var column in group.OrderBy(k => k.module).Split(numRows))
                {
                    var row = 0;
                    foreach (var (author, module) in column)
                        tt.SetCell(col, row++, module);
                    col++;
                }
                sb.Append(tt.ToString().Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries).Select(row => $"    {row.Trim().Replace("|", "│")}").JoinString("\n"));
                sb.Append("\n\n");
            }

            var remTable = new TextTable { ColumnSpacing = 5, RowSpacing = 1, VerticalRules = true, HorizontalRules = true, HeaderRows = 1 };
            remTable.SetCell(0, 0, "MODULE");
            remTable.SetCell(1, 0, "IMPLEMENTED BY");
            var remaining = groupedByAuthor.Where(gr => gr.Count() <= numColumns).SelectMany(gr => gr).OrderBy(t => t.module).ToArray();
            for (var i = 0; i < remaining.Length; i++)
            {
                remTable.SetCell(0, i + 1, remaining[i].module);
                remTable.SetCell(1, i + 1, remaining[i].author);
            }
            sb.Append($"## Others\n\n{remTable.ToString().Split('\n').Select(r => r.Trim()).Where(row => !string.IsNullOrWhiteSpace(row) && !Regex.IsMatch(row, @"^-*\|-*$")).Select(row => $"    {row.Replace("|", "│").Replace("=│=", "═╪═").Replace("=", "═")}").JoinString("\n")}\n\n");

            File.WriteAllText($@"{SouvenirPath}\CONTRIBUTORS.md", sb.ToString());
        }
    }
}
