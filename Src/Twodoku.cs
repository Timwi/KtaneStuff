using RT.Util;

namespace KtaneStuff;

static class Twodoku
{
    public static void Do()
    {
        for (var i = 0; i < 20; i++)
        {
            Console.WriteLine(i);
            var x = i % 5;
            var y = i / 5;
            var cmd = $"""
                    D:\Inkscape\bin\inkscape.exe
                    --export-type=png
                    --export-filename=D:\c\KTANE\Twodoku\Assets\Images\{(i == 8 ? "Numbers-5.png" : y >= 3 ? $"Numbers-{x}.png" : $@"Symbol-{i}.png")}
                    --export-width=512
                    --export-height=512
                    --export-area={110 * x}:{110 * y}:{110 * (x + 1)}:{110 * (y + 1)}
                    --export-background=#201e13
                    D:\c\KTANE\Twodoku\DataFiles\design.svg
                    """.Replace("\r", "").Split('\n');
            CommandRunner.Run(cmd).Go();
        }
    }
}
