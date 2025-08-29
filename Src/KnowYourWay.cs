using RT.TagSoup;
using RT.Util.ExtensionMethods;

namespace KtaneStuff;

internal static class KnowYourWay
{
    private enum direction { Up = 0, Right = 1, Down = 2, Left = 3 }

    public static void GenerateCheatSheet()
    {
        Console.WriteLine();
        Utils.ReplaceInFile(@"D:\c\KTANE\Public\HTML\Know Your Way lookup table (Timwi).html",
            "<!--start-->", "<!--end-->", $@"<tr><td class='no-border' rowspan='2' colspan='2'></td><th colspan='4'>LED position</th></tr>
<tr><th>Up</th><th>Right</th><th>Down</th><th>Left</th></tr>
{Enumerable.Range(0, 16).Select(row => $@"<tr>{(row % 4 == 0 ? $"{Environment.NewLine}    <th rowspan='4' class='arrow'>{"↑→↓←"[row / 4]}</th>" : null)}
    <th>{new[] { "‘U’ up", "‘U’ right", "‘U’ down", "‘U’ left" }[row % 4]}</th>
    {Enumerable.Range(0, 4).Select(col =>
            {
                //if (col == 2 && row == 8)
                //    System.Diagnostics.Debugger.Break();

                var ledPos = (direction) col;
                var arrowPos = (direction) (row / 4);
                var uPos = (direction) (row % 4);

                direction opp(direction dir) => (direction) (((int) dir + 2) % 4);

                var ledInd = uPos == direction.Left ? direction.Down : arrowPos == direction.Right ? direction.Up : arrowPos != ledPos ? direction.Left : direction.Right;
                var arrowInd = arrowPos == opp(ledPos) ? direction.Down : ledPos == (direction) (((int) uPos + 1) % 4) ? direction.Up : ledPos != direction.Right ? direction.Left : direction.Right;
                var upperInd = ledPos == direction.Down ? direction.Down : (arrowPos == uPos || arrowPos == opp(uPos)) ? direction.Up : uPos != direction.Up ? direction.Left : direction.Right;
                var uInd = arrowPos == uPos ? direction.Down : (ledPos != uPos && ledPos != opp(uPos)) ? direction.Up : arrowPos != direction.Down ? direction.Left : direction.Right;

                var ledOrient = arrowInd == ledInd ? direction.Up : upperInd == ledInd ? direction.Right : uInd == ledInd ? direction.Down : direction.Left;
                var arrowOrient = upperInd == arrowInd ? direction.Right : uInd == arrowInd ? direction.Down : ledInd == arrowInd ? direction.Left : direction.Up;
                var upperOrient = uInd == upperInd ? direction.Down : ledInd == upperInd ? direction.Left : arrowInd == upperInd ? direction.Up : direction.Right;
                var uOrient = ledInd == uInd ? direction.Left : arrowInd == uInd ? direction.Up : upperInd == uInd ? direction.Right : direction.Down;

                char press(direction pos, direction ind, direction orient) => "URDL"[((int) pos + 4 - (int) ind + (int) orient + 4 - (int) uPos) % 4];

                return $@"<td>{press(ledPos, ledInd, ledOrient)}{press(arrowPos, arrowInd, arrowOrient)}{press(0, upperInd, upperOrient)}{press(uPos, uInd, uOrient)}</td>";
            }).JoinString()}
</tr>").JoinString(Environment.NewLine)}
");
    }
}
