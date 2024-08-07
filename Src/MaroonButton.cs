using System.IO;
using System.Linq;
using System.Xml.Linq;
using KtaneStuff.Modeling;
using RT.KitchenSink;
using RT.Util.ExtensionMethods;

namespace KtaneStuff
{
    using static Md;

    static class MaroonButton
    {
        public static void Do()
        {
            var svg = XDocument.Parse(File.ReadAllText(@"D:\c\KTANE\KtaneStuff\DataFiles\MaroonButton\Checkmark.svg"));
            var pathD = svg.Root.ElementsI("path").Single().AttributeI("d").Value;
            var model = DecodeSvgPath.DecodePieces(pathD).Select(pc => pc.Select(pt => (-pt + p(50, 50)) / 750)).Extrude(.004, .01, true);
            File.WriteAllText($@"D:\c\KTANE\BunchOfButtons\Assets\Modules\Maroon\Assets\Checkmark.obj", GenerateObjFile(model, "Checkmark"));
        }
    }
}