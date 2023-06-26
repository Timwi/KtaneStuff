using System;
using System.Linq;
using RT.TagSoup;
using RT.Util.ExtensionMethods;

namespace KtaneStuff
{
    static class SlowMath
    {
        public static void GenerateManual()
        {
            var w = 10;
            var h = w * Math.Sqrt(3) / 2;
            var ltrs = "ABCDEGKNPSTXZ".Reverse();

            var ys = new[] { 6.75, 4.75 };
            var ds = new[] { $"M0 0 5 {h} -5 {h}z", $"M -5 0 5 0 0 {h}z" };
            var svg = Enumerable.Range(0, 169).Select(i =>
            {
                var row = (int) Math.Floor(Math.Sqrt(i));
                var col = i - row * row;
                return $@"<g id='tri-{i}' transform='translate({-5 * row + 5 * col} {h * row})'><path d='{ds[col % 2]}' fill='#eee' stroke='black' stroke-width='.3' /><text x='0' y='{ys[col % 2]}'>B</text></g>";
            }).JoinString();
            var horiz = Enumerable.Range(0, 13).Select(row =>
                $"<path class='highlightable' data-mode='row' d='M{-5 * row} {h * row} {5 * row} {h * row} {5 * (row + 1)} {h * (row + 1)} {-5 * (row + 1)} {h * (row + 1)}z M {-5 * row - 10},{-5 + h * (row + .5)} l 5,5 -5,5 v -2 h -5 v -6 h 5 z' />").JoinString();
            var horizArrows = Enumerable.Range(0, 13).Select(row =>
                $"<path d='m {-5 * row - 10},{-5 + h * (row + .5)} 5,5 -5,5 v -2 h -5 v -6 h 5 z' />").JoinString();
            var bsl = Enumerable.Range(0, 13).Select(row =>
                $"<path class='highlightable' data-mode='element' d='M {-5 * (13 - row)} {h * (13 - row)} {-5 * (12 - row)} {h * (12 - row)} {-5 * 13 + 10 * (row + 1)} {13 * h} {-5 * 13 + 10 * row} {13 * h}z M {-4.7141016 - 55.5 + 10 * row},{9.5 + 112.583302491977} l 1.8301271,-6.830127 6.830127,1.8301271 -1.7320508,1 2.5,4.330127 -5.19615243,3 -2.5,-4.330127 z' />").JoinString();
            var bslArrows = Enumerable.Range(0, 13).Select(row =>
                $"<path d='m {-4.7141016 - 55.5 + 10 * row},{9.5 + 112.583302491977} 1.8301271,-6.830127 6.830127,1.8301271 -1.7320508,1 2.5,4.330127 -5.19615243,3 -2.5,-4.330127 z' />").JoinString();
            var fsl = Enumerable.Range(0, 13).Select(row =>
                $"<path class='highlightable' data-mode='column' d='M {5 * row} {h * row} {5 * (row + 1)} {h * (row + 1)} {-5 * 13 + 10 * (row + 1)} {13 * h} {-5 * 13 + 10 * row} {13 * h} M {10.5 + 5 * row},{h * row} l -6.8301271,1.8301269 -1.8301269,-6.830127 1.7320508,1 2.5,-4.33012705 5.19615241,3 -2.5,4.3301272 z' />").JoinString();
            var fslArrows = Enumerable.Range(0, 13).Select(row =>
                $"<path d='m {10.5 + 5 * row},{h * row} -6.8301271,1.8301269 -1.8301269,-6.830127 1.7320508,1 2.5,-4.33012705 5.19615241,3 -2.5,4.3301272 z' />").JoinString();

            var redLtrsSvg = ltrs.Select((ltr, row) => $"<text x='{-5 * row - 11}' y='{6.8 + h * row}'>{ltr}</text>").JoinString();
            var grnLtrsSvg = ltrs.Select((ltr, row) => $"<text x='{(13 - row) * 5 + 1.4}' y='{(13 - row) * h - 9}'>{ltr}</text>").JoinString();
            var bluLtrsSvg = ltrs.Select((ltr, row) => $"<text x='{-13 * 5 + row * 10 + 9.7}' y='{13 * h + 10}'>{ltr}</text>").JoinString();

            Utils.ReplaceInFile(@"D:\c\KTANE\Public\HTML\Slow Math.html", "<!--%-->", "<!--%%-->",
                $@"<svg viewBox=""-76 -10 148 137"" font-family=""Special Elite"" font-size=""4"" text-anchor=""middle"" stroke-linejoin=""round"">
                    {svg}
                    <g fill='#faa'>{horizArrows}</g>
                    <g fill='#aaf8aa'>{fslArrows}</g>
                    <g fill='#aaf'>{bslArrows}</g>
                    <g font-size='7'>{redLtrsSvg}{grnLtrsSvg}{bluLtrsSvg}</g>
                    {horiz}{fsl}{bsl}
                </svg>");
        }
    }
}