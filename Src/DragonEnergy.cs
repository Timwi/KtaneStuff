using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using RT.Util;
using RT.Util.Drawing;
using RT.Util.ExtensionMethods;

namespace KtaneStuff
{
    static class DragonEnergy
    {
        public static void MakeSymbols()
        {
            var data = @"Angry=怒;Dragon=龍;Friend=友;Loyal=忠;Heart=心;Blessing=福;Dream=夢;Hate=懟;Magic=魔;River=河;Child=子;Energy=能;Hope=願;Male=男;Emotion=感;Curse=咒;Female=女;Kind=類;Mountain=山;Soul=魂;Heaven=天;Force=力;Longevity=壽;Night=晚;Urgency=急;Delight=喜;Forest=林;Love=愛;Pure=純;Wind=風"
                .Split(';').Select(str => str.Split('=')).OrderBy(arr => arr[0]).Select((arr, ix) => new { Name = arr[0], Character = arr[1] }).ToArray();

            foreach (var inf in data)
            {
                GraphicsUtil.DrawBitmap(340, 340, g =>
                {
                    g.Clear(Color.Transparent);
                    g.DrawString(inf.Character, new Font("cwTeX Q Fangsong", 305f, FontStyle.Regular), Brushes.Black, new Point(175, 225), new StringFormat { LineAlignment = StringAlignment.Center, Alignment = StringAlignment.Center });
                }).Save($@"D:\c\KTANE\DragonEnergy\Assets\Sprites\{inf.Name}.png");
            }
            data.ParallelForEach(inf => { CommandRunner.Run("pngcrf", $"{inf.Name}.png").WithWorkingDirectory(@"D:\c\KTANE\DragonEnergy\Assets\Sprites").Go(); });
        }
    }
}