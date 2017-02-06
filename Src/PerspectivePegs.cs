using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using RT.Util;
using RT.Util.ExtensionMethods;

namespace KtaneStuff
{
    sealed class PerspectivePegs
    {
        public static void Svg()
        {
            const int outerRadius = 100;
            const int innerRadius = 37;
            const int arrangementRadius = 250;
            var cos = Ut.Lambda((double angle) => Math.Cos(angle / 180.0 * Math.PI));
            var sin = Ut.Lambda((double angle) => Math.Sin(angle / 180.0 * Math.PI));
            var path = @"D:\c\KTANE\HTML\img\Component\Perspective Pegs.svg";
            File.WriteAllText(path, Regex.Replace(File.ReadAllText(path), @"(?<=<!--##-->).*(?=<!--###-->)", $@"
                <g transform='translate(174, 174) scale(.34)'>
                {
                    Enumerable.Range(0, 5).Select(i => i * 72 - 90).Select(angle2 => new { X = arrangementRadius * cos(angle2), Y = arrangementRadius * sin(angle2) }).Select(inf => $@"
                        {
                            Enumerable.Range(0, 5).Select(i => i * 72 + 90).Select(angle => $@"
                                <line stroke-width='5' stroke='#000' x1='{inf.X + outerRadius * cos(angle)}' y1='{inf.Y + outerRadius * sin(angle)}' x2='{inf.X + innerRadius * cos(angle)}' y2='{inf.Y + innerRadius * sin(angle)}' />
                                <line stroke-width='5' stroke='#000' x1='{inf.X + innerRadius * cos(angle)}' y1='{inf.Y + innerRadius * sin(angle)}' x2='{inf.X + innerRadius * cos(72 + angle)}' y2='{inf.Y + innerRadius * sin(72 + angle)}' />
                            ").JoinString()
                        }
                        <polygon stroke-width='10' stroke='#000' points='{Enumerable.Range(0, 5).Select(i => i * 72 + 90).Select(angle => $"{inf.X + outerRadius * cos(angle)} {inf.Y + outerRadius * sin(angle)}").JoinString(", ")}' />
                    ").JoinString()
                }
                </g>", RegexOptions.Singleline));
        }
    }
}
