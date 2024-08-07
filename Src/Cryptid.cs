using System.Linq;
using RT.Coordinates;
using RT.Util.ExtensionMethods;

namespace KtaneStuff
{
    static class Cryptid
    {
        public static void Do()
        {
            var grid = new Structure<Hex>(
                Enumerable.Range(0, 9).SelectMany(row => Enumerable.Range(0, 12).Select(col => new Hex(col, row - col / 2)))
            );
            var svgCode = grid.Svg(new SvgInstructions
            {
                SvgAttributes = null,
                HighlightCells = null,
                PerCellBefore = c => $"<path id='cell-{(char) ('A' + ((Hex) c).Q)}{((Hex) c).R + ((Hex) c).Q / 2 + 1}' d='M{new Hex(0, 0).GetPolygon(1).Select(p => $"{p.X} {p.Y}").JoinString(" ")}z' />"
                    + $"<path id='terr-{(char) ('A' + ((Hex) c).Q)}{((Hex) c).R + ((Hex) c).Q / 2 + 1}' d='M{new Hex(0, 0).GetPolygon(.8).Select(p => $"{p.X} {p.Y}").JoinString(" ")}z' />",
                OutlinePath = d => $"<path d='{d}' fill='none' stroke-width='.04' stroke='black' />",
                PassagesPath = d => $"<path d='{d}' fill='none' stroke-width='.02' stroke='black' />"
            });
            Utils.ReplaceInFile(@"D:\c\KTANE\Public\HTML\Cryptid.html", "<!--#-->", "<!--##-->", svgCode);
        }
    }
}