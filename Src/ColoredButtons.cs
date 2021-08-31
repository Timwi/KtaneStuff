using System.IO;
using System.Linq;
using KtaneStuff.Modeling;
using RT.Util;
using RT.Util.ExtensionMethods;
using RT.Util.Geometry;

namespace KtaneStuff
{
    using static Md;

    static class ColoredButtons
    {
        public static void TheBlueButton_MakeSymbols()
        {
            foreach (var (name, svgPathD) in Ut.NewArray<(string name, string svg)>(
                ("triangle up", "m 5,1.25 4.3301269,7.5 -8.66025409,0 z"),
                ("triangle down", "m 5,8.75 4.3301269,-7.5 -8.66025409,0 z"),
                ("circle", "M 8.955,5 A 3.955,3.955 0 0 1 5,8.955 3.955,3.955 0 0 1 1.045,5 3.955,3.955 0 0 1 5,1.045 3.955,3.955 0 0 1 8.955,5 Z"),
                ("circle with X", "M 4.9746094 1.0449219 A 3.955 3.955 0 0 0 2.7851562 1.7226562 L 5 3.9375 L 7.2148438 1.7226562 A 3.955 3.955 0 0 0 5 1.0449219 A 3.955 3.955 0 0 0 4.9746094 1.0449219 z M 1.7226562 2.7851562 A 3.955 3.955 0 0 0 1.0449219 5 A 3.955 3.955 0 0 0 1.7226562 7.2148438 L 3.9375 5 L 1.7226562 2.7851562 z M 8.2773438 2.7851562 L 6.0625 5 L 8.2773438 7.2148438 A 3.955 3.955 0 0 0 8.9550781 5 A 3.955 3.955 0 0 0 8.2773438 2.7851562 z M 5 6.0625 L 2.7851562 8.2773438 A 3.955 3.955 0 0 0 5 8.9550781 A 3.955 3.955 0 0 0 7.2148438 8.2773438 L 5 6.0625 z"),
                ("square", "M 1.5,1.5 H 8.5 V 8.5 H 1.5 Z"),
                ("square with plus", "M 1.5 1.5 L 1.5 4.5 L 4.5 4.5 L 4.5 1.5 L 1.5 1.5 z M 5.5 1.5 L 5.5 4.5 L 8.5 4.5 L 8.5 1.5 L 5.5 1.5 z M 1.5 5.5 L 1.5 8.5 L 4.5 8.5 L 4.5 5.5 L 1.5 5.5 z M 5.5 5.5 L 5.5 8.5 L 8.5 8.5 L 8.5 5.5 L 5.5 5.5 z"),
                ("diamond", "M 5,1 9,5 5,9 1,5 Z"),
                ("star", "M 5,0.47745752 6.1067849,3.9540988 9.7552826,3.9323727 6.7908156,6.0593288 7.9389262,9.5225426 5,7.3604325 2.0610736,9.5225424 3.2091844,6.0593288 0.24471746,3.9323724 3.8932151,3.9540988 Z"),
                ("plus", "M 3.5,1 H 6.5 V 3.5 H 9 V 6.5 H 6.5 V 9 H 3.5 V 6.5 H 1 V 3.5 h 2.5 z")
            ))
            {
                Utils.SvgToPng($@"D:\c\KTANE\SingleSelectablePack\Assets\Modules\BlueButtonModule\BlueButtonMats\{name}.png",
                    $@"<svg viewBox='-.1 -.1 10.2 10.2'><path d='{svgPathD}' fill='#000544' stroke='none' /></svg>", 800);
            }
        }

        public static void TheYellowButton_MakeModels()
        {
            File.WriteAllText(@"D:\c\KTANE\SingleSelectablePack\Assets\Modules\Yellow\Assets\Annulus.obj", GenerateObjFile(LooseModels.Annulus(.5, .725, 32, reverse: true), "Annulus", AutoNormal.Flat));
        }

        public static void TheWhiteButton_MakeModels()
        {
            const int numVertices = 12;
            const double ir = .5;       // inner radius
            const double or = .7; // outer radius
            const double bw = .05;
            const double h = .03;

            var bézier = Bézier(p(0, 0), p(0, h * .5), p(bw * .5, h), p(bw, h), 6);

            File.WriteAllText(@"D:\c\KTANE\SingleSelectablePack\Assets\Modules\White\Assets\Segment.obj",
                GenerateObjFile(
                    CreateMesh(false, false,
                        Enumerable.Range(0, numVertices)
                            .Select(i => bézier.Select(pt => pt + p(ir, 0)).Concat(bézier.Reverse().Select(pt => p(or - pt.X, pt.Y))).Select(p => pt(p.X, p.Y, 0).RotateY(120.0 * i / (numVertices - 1))).ToArray())
                            .Select((pts, fst, lst) => pts.Select((pt, f, l) => pt.WithMeshInfo(fst || lst ? Normal.Mine : Normal.Average, fst || lst ? Normal.Mine : Normal.Average, f || l ? Normal.Mine : Normal.Average, f || l ? Normal.Mine : Normal.Average)).ToArray())
                            .ToArray()),
                    "Segment"));
        }
    }
}