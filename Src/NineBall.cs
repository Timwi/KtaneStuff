using RT.Coordinates;
using RT.Util;
using RT.Util.Consoles;
using RT.Util.ExtensionMethods;

namespace KtaneStuff;

static class NineBall
{
    public static void GenerateSouvenirSprites()
    {
        var hexes = new[] {
                new Hex(-1, -1),
                new Hex(-1, 0), new Hex(0, -1),
                new Hex(-1, 1), new Hex(1, -1),
                new Hex(0, 1), new Hex(1, 0),
                // Do not fill in the center and bottom one
                new Hex(0, 0), new Hex(1, 1)
            };
        for (var i = 0; i < 7; i++)
        {
            ConsoleUtil.WriteLineFmt($"Generating: {i:Y}");
            File.WriteAllText($@"D:\c\KTANE\Souvenir\DataFiles\9-Ball {(char) ('A' + i)}.svg", $"""
                <svg xmlns='http://www.w3.org/2000/svg' viewBox='-140 -205 280 410'>
                    <g fill='none' stroke-width='.1' stroke='#fff8de' transform='rotate(30) scale(100)'>
                        {hexes.Select((h, ix) => $"<circle cx='{h.Center.X}' cy='{h.Center.Y}' r='.4330127018922193'{(ix == i ? " fill='#fff8de'" : "")} />").JoinString()}
                    </g>
                </svg>
                """);
            CommandRunner.Run(@"D:\inkscape\bin\inkscape.com", $@"D:\c\KTANE\Souvenir\DataFiles\9-Ball {(char) ('A' + i)}.svg", $@"--export-filename=D:\c\KTANE\Souvenir\Assets\Sprites\9-Ball\9-Ball {(char) ('A' + i)}.png", @"--export-width=400", @"--export-area-page").Go();
            File.Delete($@"D:\c\KTANE\Souvenir\DataFiles\9-Ball {(char) ('A' + i)}.svg");
        }
    }
}
