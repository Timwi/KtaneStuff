using System.Xml.Linq;
using RT.Geometry;
using RT.Modeling;
using RT.Util.ExtensionMethods;
using static RT.Modeling.Md;

namespace KtaneStuff;

internal static class MaroonButton
{
    public static void Do()
    {
        var svg = XDocument.Parse(File.ReadAllText(@"D:\c\KTANE\KtaneStuff\DataFiles\MaroonButton\Checkmark.svg"));
        var pathD = svg.Root.ElementsI("path").Single().AttributeI("d").Value;
        var model = SvgPath.Decode(pathD).Select(pc => pc.Select(pt => (-pt + p(50, 50)) / 750)).Extrude(.004, .01, true);
        File.WriteAllText($@"D:\c\KTANE\BunchOfButtons\Assets\Modules\Maroon\Assets\Checkmark.obj", GenerateObjFile(model, "Checkmark"));
    }
}
