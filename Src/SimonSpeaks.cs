using System.Text.RegularExpressions;
using System.Xml.Linq;
using RT.Geometry;
using RT.Modeling;
using RT.Util.ExtensionMethods;
using static RT.Modeling.Md;

namespace KtaneStuff;

internal static class SimonSpeaks
{
    public static void DoModels()
    {
        var svg = XDocument.Parse(File.ReadAllText(@"D:\c\KTANE\KtaneStuff\DataFiles\SimonSpeaks\Bubbles-modeling.svg"));
        foreach (var elem in svg.Root.ElementsI("path"))
        {
            var name = elem.AttributeI("id").Value;
            name = string.Concat(name.Substring(0, 1).ToUpperInvariant(), name.AsSpan(1));
            Console.WriteLine(name);
            var d = elem.AttributeI("d").Value;
            var ix = name.RegexMatch(@"(\d+)$", out var m) ? int.Parse(m.Groups[1].Value) : throw new InvalidOperationException();
            var x = (ix - 1) % 3;
            var y = (ix - 1) / 3;
            var path = SvgPath.Decode(d).Smooth(.03)
                .Select(poly => poly.Reverse().Select(p => p + new PointD(-10 * x - 5, -10 * y - 5)).Select(p => new PointD(-p.X, -p.Y)))
                .Extrude(name.StartsWith("Outline") ? .2 : .1, includeBackFace: false, flatSideNormals: true);
            File.WriteAllText($@"D:\c\KTANE\SimonSpeaks\Assets\Models\{name}.obj", GenerateObjFile(path, name));
        }
    }
}
