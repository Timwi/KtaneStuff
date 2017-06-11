using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using RT.Util;
using RT.Util.ExtensionMethods;
using RT.Util.Geometry;

namespace KtaneStuff
{
    sealed class Zoo
    {
        public static void CreateManualAndPngs()
        {
            var inGrid = Ut.NewArray(
                new { Font = "Animals 2", Ch = '3', Name = "Cow" },
                new { Font = "Animals 1", Ch = '@', Name = "Tyrannosaurus Rex" },
                new { Font = "Animals 2", Ch = '5', Name = "Rabbit" },
                new { Font = "Animals 2", Ch = ',', Name = "Horse" },
                new { Font = "Animals 1", Ch = 'p', Name = "Flamingo" },
                new { Font = "Animals 2", Ch = '+', Name = "Cat" },
                new { Font = "Animals 1", Ch = 'y', Name = "Bat" },
                new { Font = "Animals 2", Ch = 'A', Name = "Ant" },
                new { Font = "Animals 2", Ch = '<', Name = "Fly" },
                new { Font = "Animals 1", Ch = '6', Name = "Llama" },
                new { Font = "Afrika Wildlife B Mammals2", Ch = '(', Name = "Hyena" },
                new { Font = "Animals 2", Ch = '2', Name = "Pig" },
                new { Font = "Animals 1", Ch = 'k', Name = "Owl" },
                new { Font = "Afrika Wildlife B Mammals2", Ch = '\"', Name = "Rhinoceros" },
                new { Font = "Animals 1", Ch = 'H', Name = "Tortoise" },
                new { Font = "Animals 1", Ch = ']', Name = "Sea Horse" },
                new { Font = "Animals 1", Ch = '7', Name = "Camel" },
                new { Font = "Animals 1", Ch = 'B', Name = "Dimetrodon" },
                new { Font = "Animals 2", Ch = 'C', Name = "Spider" },
                new { Font = "Animals 1", Ch = 'x', Name = "Goose" },
                new { Font = "Animals 2", Ch = '@', Name = "Snail" },
                new { Font = "Animals 1", Ch = '\'', Name = "Monkey" },
                new { Font = "Animals 2", Ch = '(', Name = "Wolf" },
                new { Font = "Animals 1", Ch = '*', Name = "Kangaroo" },
                new { Font = "Animals 1", Ch = '[', Name = "Lobster" },
                new { Font = "Animals 1", Ch = '8', Name = "Dromedary" },
                new { Font = "Animals 1", Ch = '(', Name = "Bear" },
                new { Font = "Animals 2", Ch = '?', Name = "Dragonfly" },
                new { Font = "Animals 2", Ch = '9', Name = "Butterfly" },
                new { Font = "Animals 2", Ch = ')', Name = "Fox" },
                new { Font = "Animals 1", Ch = 'O', Name = "Dolphin" },
                new { Font = "Animals 1", Ch = 'w', Name = "Eagle" },
                new { Font = "Afrika Wildlife B Mammals2", Ch = '6', Name = "Porcupine" },
                new { Font = "Afrika Wildlife B Mammals2", Ch = 'S', Name = "Otter" },
                new { Font = "Afrika Wildlife B Mammals2", Ch = 'N', Name = "Warthog" },
                new { Font = "Animals 2", Ch = '&', Name = "Ferret" },
                new { Font = "Animals 1", Ch = '9', Name = "Lion" },
                new { Font = "Animals 2", Ch = '$', Name = "Squirrel" },
                new { Font = "Animals 1", Ch = '3', Name = "Giraffe" },
                new { Font = "Animals 1", Ch = 'l', Name = "Koala" },
                new { Font = "Animals 1", Ch = '\\', Name = "Crab" },
                new { Font = "Animals 1", Ch = 'N', Name = "Frog" },
                new { Font = "Animals 1", Ch = 's', Name = "Swallow" },
                new { Font = "Animals 1", Ch = 'C', Name = "Stegosaurus" },
                new { Font = "Animals 1", Ch = 'D', Name = "Pterodactyl" },
                new { Font = "Animals 1", Ch = 'K', Name = "Cobra" },
                new { Font = "Afrika Wildlife B Mammals2", Ch = ',', Name = "Hippopotamus" },
                new { Font = "Animals 1", Ch = '?', Name = "Triceratops" },
                new { Font = "Animals 1", Ch = 'h', Name = "Duck" },
                new { Font = "Animals 1", Ch = '_', Name = "Starfish" },
                new { Font = "Afrika Wildlife B Mammals2", Ch = ':', Name = "Elephant" },
                new { Font = "Animals 2", Ch = '4', Name = "Rooster" },
                new { Font = "Animals 1", Ch = 'm', Name = "Woodpecker" },
                new { Font = "Animals 1", Ch = 'A', Name = "Apatosaurus" },
                new { Font = "Animals 2", Ch = '!', Name = "Beaver" },
                new { Font = "Animals 1", Ch = '%', Name = "Gorilla" },
                new { Font = "Animals 1", Ch = 'J', Name = "Mouse" },
                new { Font = "Animals 1", Ch = 'Z', Name = "Seal" },
                new { Font = "Animals 2", Ch = '%', Name = "Skunk" },
                new { Font = "Animals 1", Ch = 'L', Name = "Viper" },
                new { Font = "Animals 1", Ch = 'M', Name = "Salamander" });

            var outsideGrid = Ut.NewArray(
                new { IsQ = true, Font = "Animals 1", Ch = '/', Name = "Gazelle" },
                new { IsQ = true, Font = "Afrika Wildlife B Mammals2", Ch = 'Y', Name = "Caracal" },
                new { IsQ = true, Font = "Afrika Wildlife B Mammals2", Ch = ')', Name = "Cheetah" },
                new { IsQ = true, Font = "Afrika Wildlife B Mammals2", Ch = 'U', Name = "Ocelot" },
                new { IsQ = true, Font = "Animals 2", Ch = '-', Name = "Sheep" },
                new { IsQ = true, Font = "Animals 2", Ch = '>', Name = "Caterpillar" },
                new { IsQ = true, Font = "Animals 2", Ch = '\"', Name = "Groundhog" },
                new { IsQ = true, Font = "Afrika Wildlife B Mammals2", Ch = 'G', Name = "Armadillo" },
                new { IsQ = true, Font = "Animals 1", Ch = 'F', Name = "Orca" },
                new { IsQ = false, Font = "Animals 1", Ch = 'E', Name = "Sea Turtle" },
                new { IsQ = false, Font = "Animals 1", Ch = 'j', Name = "Penguin" },
                new { IsQ = false, Font = "Afrika Wildlife B Mammals2", Ch = '!', Name = "Baboon" },
                new { IsQ = false, Font = "Animals 1", Ch = 'G', Name = "Whale" },
                new { IsQ = false, Font = "Animals 1", Ch = '^', Name = "Squid" },
                new { IsQ = false, Font = "Animals 2", Ch = '\'', Name = "Coyote" },
                new { IsQ = false, Font = "Animals 1", Ch = '1', Name = "Ram" },
                new { IsQ = false, Font = "Animals 1", Ch = '0', Name = "Deer" },
                new { IsQ = false, Font = "Animals 1", Ch = '>', Name = "Alligator" });

            const int sideLength = 5;

            string svgArrow(double angle, double x, double y, double offsetX = 0, double offsetY = 0)
            {
                return $"<path transform='translate({x} {y}) rotate({angle})' stroke='none' fill='#ccc' d='M {"-2,-3;2,-3;2,1;4,1;0,5;-4,1;-2,1".Split(';').Select(str => str.Split(',')).Select(arr => new PointD(double.Parse(arr[0]) * 0.05625 + offsetX, double.Parse(arr[1]) * .075 + offsetY)).Select(p => $"{p.X},{p.Y}").JoinString(", ")} z' />";
            }

            string svgForAnimal(string font, char ch, Hex hexagon, bool withHexOutline = false, double sizeFactor = .4, double? arrowAngle = null)
            {
                var path = Utils.FontToSvgPath(ch.ToString(), font, 128);
                var xMin = path.Where(ps => ps.Points != null).Min(ps => ps.Points.Min(p => p.X));
                var yMin = path.Where(ps => ps.Points != null).Min(ps => ps.Points.Min(p => p.Y));
                var xMax = path.Where(ps => ps.Points != null).Max(ps => ps.Points.Max(p => p.X));
                var yMax = path.Where(ps => ps.Points != null).Max(ps => ps.Points.Max(p => p.Y));
                var maxRadius = path.Where(ps => ps.Points != null).Max(ps => ps.Points.Max(p => p.Distance(new PointD((xMin + xMax) / 2, (yMin + yMax) / 2))));
                PointD translatePoint(PointD p) => new PointD(
                    Math.Round((p.X - (xMin + xMax) / 2) / maxRadius * sizeFactor + hexagon.GetCenter(1).X, 3),
                    Math.Round((p.Y - (yMin + yMax) / 2) / maxRadius * sizeFactor + hexagon.GetCenter(1).Y, 3));
                return
                    (withHexOutline ? $"<path stroke-width='.01' stroke='#000' fill='#fff' d='M {hexagon.GetPolygon(1).Select(p => $"{p.X},{p.Y}").JoinString(", ")} z' />" : null) +
                    arrowAngle.NullOr(aa => svgArrow(aa, hexagon.GetCenter(1).X, hexagon.GetCenter(1).Y, 0, .15)) +
                    $"<path d='{path.Select(pp => pp.Select(translatePoint)).JoinString(" ")}' fill='#000' />";
            }

            Utils.ReplaceInFile(@"D:\c\KTANE\Public\HTML\Zoo.html", "<!--%%-->", "<!--%%%-->", $@"
                <svg class='full-diagram' viewBox='-3.5 -4.6 7.5 8.7'>
                    {Hex.LargeHexagon(sideLength + 1)
                        .Select(h =>
                            // Along the top edge (11 & 1 o’clock)
                            (h.R == -sideLength && h.Q < sideLength) || (h.Q + h.R == -sideLength && h.R < 0) ?
                                svgForAnimal(outsideGrid[h.Q + 4].Font, outsideGrid[h.Q + 4].Ch, h, sizeFactor: .3, arrowAngle: 0) :
                            // Along the south-east edge (3 & 5 o’clock)
                            (h.Q == sideLength && h.R > -sideLength) || (h.Q + h.R == sideLength && h.Q > 0) ?
                                svgForAnimal(outsideGrid[h.R + 13].Font, outsideGrid[h.R + 13].Ch, h, sizeFactor: .3, arrowAngle: 120) :
                            null)
                        .JoinString()}
                    <path stroke-width='.1' stroke='#000' fill='none' d='M {Hex.LargeHexagonOutline(sideLength, 1).Select(p => $"{p.X},{p.Y}").JoinString(", ")} z' />
                    {Hex.LargeHexagon(sideLength).Select((h, i) => svgForAnimal(inGrid[i].Font, inGrid[i].Ch, h, withHexOutline: true)).JoinString()}
                </svg>");

            Utils.ReplaceInFile(@"D:\c\KTANE\Public\HTML\Zoo.html", "<!--##-->", "<!--###-->", $@"
                <svg class='small-diagram' viewBox='-1.7 -1.7 3.4 3.4'>
                    <path stroke-width='.01' stroke='#000' fill='none' d='M {new Hex(0, 0).GetPolygon(1).Select(p => $"{p.X},{p.Y}").JoinString(", ")} z' />
                    {"Parallel,DVI-D,Stereo RCA,Serial,PS/2,RJ-45".Split(',')
                        .Select((p, i) => new Hex(0, 0).Neighbors[i].GetCenter(1).Apply(cnt => svgArrow(60 * (i - 1) + 180, cnt.X, cnt.Y, 0, -.15)) + $"<text x='0' y='0' transform='rotate({60 * (i - 1)}) translate(0 1.4)' font-family='Special Elite' font-size='.2' text-anchor='middle'>{p}</text>")
                        .JoinString()}
                </svg>");

            var seq = inGrid.Zip(Hex.LargeHexagon(sideLength), (ig, hex) => new { Font = ig.Font, Ch = ig.Ch, Name = ig.Name, Hex = (Hex?) hex, IsQ = (bool?) null })
                .Concat(outsideGrid.Select((og, ix) => new { Font = og.Font, Ch = og.Ch, Name = og.Name, Hex = (Hex?) null, IsQ = (bool?) og.IsQ }));

            var hexDeclarations = new List<string>();
            var qDeclarations = new Dictionary<string, string>();
            var rDeclarations = new Dictionary<string, string>();
            var pngcrs = new List<Thread>();
            foreach (var inf in seq)
            {
                //File.WriteAllText($@"D:\temp\Zoo\{inf.Name}.svg", $@"<svg xmlns='http://www.w3.org/2000/svg' viewBox='-1 -1 2 2'>{svgForAnimal(inf.Font, inf.Ch, new Hex(0, 0), sizeFactor: 1)}</svg>");
                //var stuff = CommandRunner.RunRaw($@"D:\Inkscape\InkscapePortable.exe -z -f ""D:\temp\Zoo\{inf.Name}.svg"" -w 256 -e ""D:\temp\Zoo\{inf.Name}.png""").GoGetOutputText();
                var thr = new Thread(() =>
                {
                    //CommandRunner.RunRaw($@"pngcr ""D:\temp\Zoo\{inf.Name}.png"" ""D:\temp\Zoo\{inf.Name}.cr.png""").Go();
                    lock (hexDeclarations)
                        if (inf.Hex != null)
                            hexDeclarations.Add($@"{{ new Hex({inf.Hex.Value.Q}, {inf.Hex.Value.R}), new AnimalInfo {{ Name = ""{inf.Name.CLiteralEscape()}"", Png = new byte[] {{ {File.ReadAllBytes($@"D:\temp\Zoo\{inf.Name}.cr.png").JoinString(", ")} }} }} }}");
                        else
                            (inf.IsQ.Value ? qDeclarations : rDeclarations).Add(inf.Name, $@"new AnimalInfo {{ Name = ""{inf.Name.CLiteralEscape()}"", Png = new byte[] {{ {File.ReadAllBytes($@"D:\temp\Zoo\{inf.Name}.cr.png").JoinString(", ")} }} }}");
                    //File.Delete($@"D:\temp\Zoo\{inf.Name}.png");
                    //File.Delete($@"D:\temp\Zoo\{inf.Name}.cr.png");
                });
                thr.Start();
                pngcrs.Add(thr);
            }
            foreach (var th in pngcrs)
                th.Join();

            if (hexDeclarations.Count != 61 || qDeclarations.Count != 9 || rDeclarations.Count != 9)
                System.Diagnostics.Debugger.Break();

            File.WriteAllText(@"D:\c\KTANE\Zoo\Assets\Data.cs", $@"using System.Collections.Generic;

namespace Zoo
{{
    public sealed class AnimalInfo {{ public string Name; public byte[] Png; }}
    public static class Data
    {{
        public static Dictionary<Hex, AnimalInfo> HexRawBytes = new Dictionary<Hex, AnimalInfo>
        {{
            {hexDeclarations.JoinString(",\r\n            ")}
        }};

        public static AnimalInfo[] QRawBytes = new AnimalInfo[]
        {{
            {seq.Where(i => i.IsQ == true).Select(i => qDeclarations[i.Name]).JoinString(",\r\n            ")}
        }};

        public static AnimalInfo[] RRawBytes = new AnimalInfo[]
        {{
            {seq.Where(i => i.IsQ == false).Select(i => rDeclarations[i.Name]).JoinString(",\r\n            ")}
        }};
    }}
}}");
        }

        public static void RunSimulation()
        {
            const int iterations = 100000;
            var portTypes = EnumStrong.GetValues<PortType>();
            var portTypeToDir = new int[6];
            portTypeToDir[(int) PortType.Serial] = 0;
            portTypeToDir[(int) PortType.PS2] = 1;
            portTypeToDir[(int) PortType.RJ45] = 2;
            portTypeToDir[(int) PortType.Parallel] = 3;
            portTypeToDir[(int) PortType.DVI] = 4;
            portTypeToDir[(int) PortType.StereoRCA] = 5;

            var countConds = new Dictionary<string, int>();
            for (int i = 0; i < iterations; i++)
            {
                var edgework = Edgework.Generate(5, 7);
                var counts = Ut.NewArray(edgework.GetNumPortPlates() + 1, j => j == 0 ? portTypes.ToHashSet() : new HashSet<PortType>());
                foreach (var port in edgework.Widgets.Where(w => w.PortTypes != null).SelectMany(w => w.PortTypes))
                {
                    var ix = counts.IndexOf(c => c.Contains(port));
                    counts[ix].Remove(port);
                    counts[ix + 1].Add(port);
                }

                var hexes = Hex.LargeHexagon(5).Select(hex =>
                {
                    Console.WriteLine(edgework);
                    // Check the port types in order of most common to least common.
                    for (int c = counts.Length - 1; c >= 0; c--)
                    {
                        // Check which of these port types can form a line of 5
                        var eligiblePortTypes = counts[c].Where(pt => Enumerable.Range(0, 5).All(dist => (hex + dist * Hex.GetDirection(portTypeToDir[(int) pt])).Distance < 5)).Take(2).ToArray();
                        if (eligiblePortTypes.Length != 1)
                            continue;

                        // We found an eligible port type; return the line
                        return new { Hex = hex, PortType = (PortType?) eligiblePortTypes[0], Line = Enumerable.Range(0, 5).Select(dist => hex + dist * Hex.GetDirection(portTypeToDir[(int) eligiblePortTypes[0]])).ToArray() };
                    }

                    // Check if the two-step rule works for this hex
                    for (int dir = 0; dir < 6; dir++)
                        if (Enumerable.Range(0, 5).All(dist => (hex + 2 * dist * Hex.GetDirection(dir)).Distance < 5))
                            return new { Hex = hex, PortType = (PortType?) null, Line = Enumerable.Range(0, 5).Select(dist => hex + 2 * dist * Hex.GetDirection(dir)).ToArray() };

                    return null;
                }).ToArray();
            }
            foreach (var kvp in countConds)
                Console.WriteLine($"{kvp.Key} = {kvp.Value / (double) iterations * 100:0.00}%");
        }
    }
}