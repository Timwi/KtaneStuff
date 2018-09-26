using System;
using System.Linq;
using RT.TagSoup;
using RT.Util.ExtensionMethods;

namespace KtaneStuff
{
    static class KnowYourWay
    {
        enum Direction { Up = 0, Right = 1, Down = 2, Left = 3 }

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

                    var ledPos = (Direction) col;
                    var arrowPos = (Direction) (row / 4);
                    var uPos = (Direction) (row % 4);

                    Direction opp(Direction dir) => (Direction) (((int) dir + 2) % 4);

                    var ledInd = uPos == Direction.Left ? Direction.Down : arrowPos == Direction.Right ? Direction.Up : arrowPos != ledPos ? Direction.Left : Direction.Right;
                    var arrowInd = arrowPos == opp(ledPos) ? Direction.Down : ledPos == (Direction) (((int) uPos + 1) % 4) ? Direction.Up : ledPos != Direction.Right ? Direction.Left : Direction.Right;
                    var upperInd = ledPos == Direction.Down ? Direction.Down : (arrowPos == uPos || arrowPos == opp(uPos)) ? Direction.Up : uPos != Direction.Up ? Direction.Left : Direction.Right;
                    var uInd = arrowPos == uPos ? Direction.Down : (ledPos != uPos && ledPos != opp(uPos)) ? Direction.Up : arrowPos != Direction.Down ? Direction.Left : Direction.Right;

                    var ledOrient = arrowInd == ledInd ? Direction.Up : upperInd == ledInd ? Direction.Right : uInd == ledInd ? Direction.Down : Direction.Left;
                    var arrowOrient = upperInd == arrowInd ? Direction.Right : uInd == arrowInd ? Direction.Down : ledInd == arrowInd ? Direction.Left : Direction.Up;
                    var upperOrient = uInd == upperInd ? Direction.Down : ledInd == upperInd ? Direction.Left : arrowInd == upperInd ? Direction.Up : Direction.Right;
                    var uOrient = ledInd == uInd ? Direction.Left : arrowInd == uInd ? Direction.Up : upperInd == uInd ? Direction.Right : Direction.Down;

                    char press(Direction pos, Direction ind, Direction orient) => "URDL"[((int) pos + 4 - (int) ind + (int) orient + 4 - (int) uPos) % 4];

                    return $@"<td>{press(ledPos, ledInd, ledOrient)}{press(arrowPos, arrowInd, arrowOrient)}{press(0, upperInd, upperOrient)}{press(uPos, uInd, uOrient)}</td>";
                }).JoinString()}
</tr>").JoinString(Environment.NewLine)}
");
        }
    }
}