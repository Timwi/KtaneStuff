using System.IO;
using System.Linq;
using RT.TagSoup;
using RT.Util;
using RT.Util.ExtensionMethods;
using RT.Util.Geometry;

namespace KtaneStuff
{
    sealed class Zoo
    {
        enum AnimalUse { None, InGrid, OutsideGrid }

        public static void CreateAnimalIcons()
        {
            var data = Ut.NewArray(
                new { Use = AnimalUse.InGrid, Font = "Animals 1", Ch = '%', Name = "Gorilla" },
                new { Use = AnimalUse.InGrid, Font = "Animals 1", Ch = '\'', Name = "Monkey" },
                new { Use = AnimalUse.InGrid, Font = "Animals 1", Ch = '(', Name = "Bear" },
                new { Use = AnimalUse.InGrid, Font = "Animals 1", Ch = '*', Name = "Kangaroo" },
                new { Use = AnimalUse.None, Font = "Animals 1", Ch = '+', Name = "Kangaroo" },
                new { Use = AnimalUse.None, Font = "Animals 1", Ch = ',', Name = "Rhinoceros" },
                new { Use = AnimalUse.None, Font = "Animals 1", Ch = '-', Name = "Anteater" },
                new { Use = AnimalUse.OutsideGrid, Font = "Animals 1", Ch = '/', Name = "Gazelle" },
                new { Use = AnimalUse.OutsideGrid, Font = "Animals 1", Ch = '0', Name = "Deer" },
                new { Use = AnimalUse.OutsideGrid, Font = "Animals 1", Ch = '1', Name = "Ram" },
                new { Use = AnimalUse.InGrid, Font = "Animals 1", Ch = '3', Name = "Giraffe" },
                new { Use = AnimalUse.None, Font = "Animals 1", Ch = '4', Name = "Elephant" },
                new { Use = AnimalUse.None, Font = "Animals 1", Ch = '5', Name = "Elephant" },
                new { Use = AnimalUse.InGrid, Font = "Animals 1", Ch = '6', Name = "Llama" },
                new { Use = AnimalUse.InGrid, Font = "Animals 1", Ch = '7', Name = "Camel" },
                new { Use = AnimalUse.InGrid, Font = "Animals 1", Ch = '8', Name = "Dromedary" },
                new { Use = AnimalUse.InGrid, Font = "Animals 1", Ch = '9', Name = "Lion" },
                new { Use = AnimalUse.None, Font = "Animals 1", Ch = '<', Name = "Jaguar" },
                new { Use = AnimalUse.OutsideGrid, Font = "Animals 1", Ch = '>', Name = "Alligator" },
                new { Use = AnimalUse.InGrid, Font = "Animals 1", Ch = '?', Name = "Triceratops" },
                new { Use = AnimalUse.InGrid, Font = "Animals 1", Ch = '@', Name = "Tyrannosaurus Rex" },
                new { Use = AnimalUse.InGrid, Font = "Animals 1", Ch = 'A', Name = "Apatosaurus" },
                new { Use = AnimalUse.InGrid, Font = "Animals 1", Ch = 'B', Name = "Dimetrodon" },
                new { Use = AnimalUse.InGrid, Font = "Animals 1", Ch = 'C', Name = "Stegosaurus" },
                new { Use = AnimalUse.InGrid, Font = "Animals 1", Ch = 'D', Name = "Pterodactyl" },
                new { Use = AnimalUse.OutsideGrid, Font = "Animals 1", Ch = 'E', Name = "Sea Turtle" },
                new { Use = AnimalUse.OutsideGrid, Font = "Animals 1", Ch = 'F', Name = "Orca" },
                new { Use = AnimalUse.OutsideGrid, Font = "Animals 1", Ch = 'G', Name = "Whale" },
                new { Use = AnimalUse.InGrid, Font = "Animals 1", Ch = 'H', Name = "Tortoise" },
                new { Use = AnimalUse.InGrid, Font = "Animals 1", Ch = 'J', Name = "Mouse" },
                new { Use = AnimalUse.InGrid, Font = "Animals 1", Ch = 'K', Name = "Cobra" },
                new { Use = AnimalUse.InGrid, Font = "Animals 1", Ch = 'L', Name = "Viper" },
                new { Use = AnimalUse.InGrid, Font = "Animals 1", Ch = 'M', Name = "Salamander" },
                new { Use = AnimalUse.InGrid, Font = "Animals 1", Ch = 'N', Name = "Frog" },
                new { Use = AnimalUse.InGrid, Font = "Animals 1", Ch = 'O', Name = "Dolphin" },
                new { Use = AnimalUse.InGrid, Font = "Animals 1", Ch = 'Z', Name = "Seal" },
                new { Use = AnimalUse.InGrid, Font = "Animals 1", Ch = '[', Name = "Lobster" },
                new { Use = AnimalUse.InGrid, Font = "Animals 1", Ch = '\\', Name = "Crab" },
                new { Use = AnimalUse.InGrid, Font = "Animals 1", Ch = ']', Name = "Sea Horse" },
                new { Use = AnimalUse.OutsideGrid, Font = "Animals 1", Ch = '^', Name = "Squid" },
                new { Use = AnimalUse.InGrid, Font = "Animals 1", Ch = '_', Name = "Starfish" },
                new { Use = AnimalUse.None, Font = "Animals 1", Ch = 'g', Name = "Duck" },
                new { Use = AnimalUse.InGrid, Font = "Animals 1", Ch = 'h', Name = "Duck" },
                new { Use = AnimalUse.OutsideGrid, Font = "Animals 1", Ch = 'j', Name = "Penguin" },
                new { Use = AnimalUse.InGrid, Font = "Animals 1", Ch = 'k', Name = "Owl" },
                new { Use = AnimalUse.InGrid, Font = "Animals 1", Ch = 'l', Name = "Koala" },
                new { Use = AnimalUse.InGrid, Font = "Animals 1", Ch = 'm', Name = "Woodpecker" },
                //new { Font = "Animals 1", Ch = 'n', Name = "" },
                //new { Font = "Animals 1", Ch = 'o', Name = "" },
                new { Use = AnimalUse.InGrid, Font = "Animals 1", Ch = 'p', Name = "Flamingo" },
                //new { Font = "Animals 1", Ch = 'q', Name = "Egret? Heron?" },
                //new { Font = "Animals 1", Ch = 'r', Name = "" },
                new { Use = AnimalUse.InGrid, Font = "Animals 1", Ch = 's', Name = "Swallow" },
                //new { Font = "Animals 1", Ch = 't', Name = "" },
                //new { Font = "Animals 1", Ch = 'u', Name = "" },
                new { Use = AnimalUse.None, Font = "Animals 1", Ch = 'v', Name = "Eagle" },
                new { Use = AnimalUse.InGrid, Font = "Animals 1", Ch = 'w', Name = "Eagle" },
                new { Use = AnimalUse.InGrid, Font = "Animals 1", Ch = 'x', Name = "Goose" },
                new { Use = AnimalUse.InGrid, Font = "Animals 1", Ch = 'y', Name = "Bat" },
                new { Use = AnimalUse.InGrid, Font = "Animals 2", Ch = '!', Name = "Beaver" },
                new { Use = AnimalUse.OutsideGrid, Font = "Animals 2", Ch = '"', Name = "Groundhog" },
                //new { Font = "Animals 2", Ch = '#', Name = "Otter?" },
                new { Use = AnimalUse.InGrid, Font = "Animals 2", Ch = '$', Name = "Squirrel" },
                new { Use = AnimalUse.InGrid, Font = "Animals 2", Ch = '%', Name = "Skunk" },
                new { Use = AnimalUse.InGrid, Font = "Animals 2", Ch = '&', Name = "Ferret" },
                new { Use = AnimalUse.OutsideGrid, Font = "Animals 2", Ch = '\'', Name = "Coyote" },
                new { Use = AnimalUse.InGrid, Font = "Animals 2", Ch = '(', Name = "Wolf" },
                new { Use = AnimalUse.InGrid, Font = "Animals 2", Ch = ')', Name = "Fox" },
                new { Use = AnimalUse.None, Font = "Animals 2", Ch = '*', Name = "Hyena" },
                new { Use = AnimalUse.InGrid, Font = "Animals 2", Ch = '+', Name = "Cat" },
                new { Use = AnimalUse.InGrid, Font = "Animals 2", Ch = ',', Name = "Horse" },
                new { Use = AnimalUse.OutsideGrid, Font = "Animals 2", Ch = '-', Name = "Sheep" },
                new { Use = AnimalUse.None, Font = "Animals 2", Ch = '1', Name = "Pig" },
                new { Use = AnimalUse.InGrid, Font = "Animals 2", Ch = '2', Name = "Pig" },
                new { Use = AnimalUse.InGrid, Font = "Animals 2", Ch = '3', Name = "Cow" },
                new { Use = AnimalUse.InGrid, Font = "Animals 2", Ch = '4', Name = "Rooster" },
                new { Use = AnimalUse.InGrid, Font = "Animals 2", Ch = '5', Name = "Rabbit" },
                new { Use = AnimalUse.None, Font = "Animals 2", Ch = '6', Name = "Rabbit" },
                new { Use = AnimalUse.None, Font = "Animals 2", Ch = '8', Name = "Dragonfly" },
                new { Use = AnimalUse.InGrid, Font = "Animals 2", Ch = '9', Name = "Butterfly" },
                new { Use = AnimalUse.None, Font = "Animals 2", Ch = ':', Name = "Beetle" },
                new { Use = AnimalUse.InGrid, Font = "Animals 2", Ch = '<', Name = "Fly" },
                new { Use = AnimalUse.OutsideGrid, Font = "Animals 2", Ch = '>', Name = "Caterpillar" },
                new { Use = AnimalUse.InGrid, Font = "Animals 2", Ch = '?', Name = "Dragonfly" },
                new { Use = AnimalUse.InGrid, Font = "Animals 2", Ch = '@', Name = "Snail" },
                new { Use = AnimalUse.InGrid, Font = "Animals 2", Ch = 'A', Name = "Ant" },
                //new { Font = "Animals 2", Ch = 'B', Name = "?" },
                new { Use = AnimalUse.InGrid, Font = "Animals 2", Ch = 'C', Name = "Spider" },
                new { Use = AnimalUse.OutsideGrid, Font = "Afrika Wildlife B Mammals2", Ch = '!', Name = "Baboon" },
                new { Use = AnimalUse.InGrid, Font = "Afrika Wildlife B Mammals2", Ch = '"', Name = "Rhinoceros" },
                //new { Font = "Afrika Wildlife B Mammals2", Ch = '&', Name = "?" },
                new { Use = AnimalUse.InGrid, Font = "Afrika Wildlife B Mammals2", Ch = '(', Name = "Hyena" },
                new { Use = AnimalUse.OutsideGrid, Font = "Afrika Wildlife B Mammals2", Ch = ')', Name = "Cheetah" },
                //new { Font = "Afrika Wildlife B Mammals2", Ch = '*', Name = "?" },
                //new { Font = "Afrika Wildlife B Mammals2", Ch = '+', Name = "" },
                new { Use = AnimalUse.InGrid, Font = "Afrika Wildlife B Mammals2", Ch = ',', Name = "Hippopotamus" },
                //new { Font = "Afrika Wildlife B Mammals2", Ch = '-', Name = "" },
                new { Use = AnimalUse.None, Font = "Afrika Wildlife B Mammals2", Ch = '.', Name = "Hippopotamus" },
                //new { Font = "Afrika Wildlife B Mammals2", Ch = '/', Name = "?" },
                //new { Font = "Afrika Wildlife B Mammals2", Ch = '0', Name = "" },
                //new { Font = "Afrika Wildlife B Mammals2", Ch = '1', Name = "" },
                //new { Font = "Afrika Wildlife B Mammals2", Ch = '2', Name = "" },
                //new { Font = "Afrika Wildlife B Mammals2", Ch = '3', Name = "" },
                //new { Font = "Afrika Wildlife B Mammals2", Ch = '4', Name = "" },
                //new { Font = "Afrika Wildlife B Mammals2", Ch = '5', Name = "" },
                new { Use = AnimalUse.InGrid, Font = "Afrika Wildlife B Mammals2", Ch = '6', Name = "Porcupine" },
                //new { Font = "Afrika Wildlife B Mammals2", Ch = '7', Name = "" },
                //new { Font = "Afrika Wildlife B Mammals2", Ch = '8', Name = "" },
                //new { Font = "Afrika Wildlife B Mammals2", Ch = '9', Name = "" },
                new { Use = AnimalUse.InGrid, Font = "Afrika Wildlife B Mammals2", Ch = ':', Name = "Elephant" },
                //new { Font = "Afrika Wildlife B Mammals2", Ch = ';', Name = "" },
                //new { Font = "Afrika Wildlife B Mammals2", Ch = '<', Name = "" },
                //new { Font = "Afrika Wildlife B Mammals2", Ch = '=', Name = "" },
                //new { Font = "Afrika Wildlife B Mammals2", Ch = '>', Name = "" },
                //new { Font = "Afrika Wildlife B Mammals2", Ch = '?', Name = "" },
                //new { Font = "Afrika Wildlife B Mammals2", Ch = '@', Name = "" },
                //new { Font = "Afrika Wildlife B Mammals2", Ch = 'A', Name = "" },
                new { Use = AnimalUse.None, Font = "Afrika Wildlife B Mammals2", Ch = 'B', Name = "Warthog" },
                //new { Font = "Afrika Wildlife B Mammals2", Ch = 'C', Name = "" },
                //new { Font = "Afrika Wildlife B Mammals2", Ch = 'D', Name = "" },
                new { Use = AnimalUse.None, Font = "Afrika Wildlife B Mammals2", Ch = 'E', Name = "Lion" },
                new { Use = AnimalUse.None, Font = "Afrika Wildlife B Mammals2", Ch = 'F', Name = "Anteater" },
                new { Use = AnimalUse.OutsideGrid, Font = "Afrika Wildlife B Mammals2", Ch = 'G', Name = "Armadillo" },
                new { Use = AnimalUse.None, Font = "Afrika Wildlife B Mammals2", Ch = 'H', Name = "Armadillo" },
                //new { Font = "Afrika Wildlife B Mammals2", Ch = 'I', Name = "" },
                new { Use = AnimalUse.None, Font = "Afrika Wildlife B Mammals2", Ch = 'J', Name = "Elephant" },
                new { Use = AnimalUse.None, Font = "Afrika Wildlife B Mammals2", Ch = 'K', Name = "Elephant" },
                new { Use = AnimalUse.None, Font = "Afrika Wildlife B Mammals2", Ch = 'L', Name = "Elephant" },
                //new { Font = "Afrika Wildlife B Mammals2", Ch = 'M', Name = "" },
                new { Use = AnimalUse.InGrid, Font = "Afrika Wildlife B Mammals2", Ch = 'N', Name = "Warthog" },
                //new { Font = "Afrika Wildlife B Mammals2", Ch = 'O', Name = "?" },
                new { Use = AnimalUse.None, Font = "Afrika Wildlife B Mammals2", Ch = 'P', Name = "Skunk" },
                //new { Font = "Afrika Wildlife B Mammals2", Ch = 'Q', Name = "?" },
                new { Use = AnimalUse.None, Font = "Afrika Wildlife B Mammals2", Ch = 'R', Name = "Lion" },
                new { Use = AnimalUse.InGrid, Font = "Afrika Wildlife B Mammals2", Ch = 'S', Name = "Otter" },
                new { Use = AnimalUse.OutsideGrid, Font = "Afrika Wildlife B Mammals2", Ch = 'U', Name = "Ocelot" },
                new { Use = AnimalUse.None, Font = "Afrika Wildlife B Mammals2", Ch = 'V', Name = "Rhinoceros" },
                //new { Font = "Afrika Wildlife B Mammals2", Ch = 'W', Name = "" },
                new { Use = AnimalUse.None, Font = "Afrika Wildlife B Mammals2", Ch = 'X', Name = "Rhinoceros" },
                new { Use = AnimalUse.OutsideGrid, Font = "Afrika Wildlife B Mammals2", Ch = 'Y', Name = "Caracal" },
                new { Use = AnimalUse.None, Font = "Afrika Wildlife B Mammals2", Ch = 'Z', Name = "Rhinoceros" }
            );

            var inGridIxs = Enumerable.Range(0, data.Length).Where(ix => data[ix].Use == AnimalUse.InGrid).OrderBy(ix => data[ix].Name.GetHashCode()).ToArray();
            var outsideGridIxs = Enumerable.Range(0, data.Length).Where(ix => data[ix].Use == AnimalUse.OutsideGrid).OrderBy(ix => data[ix].Name.GetHashCode()).ToArray();

            const int sideLength = 5;

            string svgForAnimal(int index, Hex hexagon, bool withHexOutline = false, double sizeFactor = .4, double? arrowAngle = null)
            {
                var d = data[index];
                var path = Utils.FontToSvgPath(d.Ch.ToString(), d.Font, 128);
                var xMin = path.Where(ps => ps.Points != null).Min(ps => ps.Points.Min(p => p.X));
                var yMin = path.Where(ps => ps.Points != null).Min(ps => ps.Points.Min(p => p.Y));
                var xMax = path.Where(ps => ps.Points != null).Max(ps => ps.Points.Max(p => p.X));
                var yMax = path.Where(ps => ps.Points != null).Max(ps => ps.Points.Max(p => p.Y));
                var maxRadius = path.Where(ps => ps.Points != null).Max(ps => ps.Points.Max(p => p.Distance(new PointD((xMin + xMax) / 2, (yMin + yMax) / 2))));
                PointD translatePoint(PointD p) => new PointD((p.X - (xMin + xMax) / 2) / maxRadius * sizeFactor, (p.Y - (yMin + yMax) / 2) / maxRadius * sizeFactor) + hexagon.GetCenter(1);
                return
                    (withHexOutline ? $"<path stroke-width='.01' stroke='#000' fill='#fff' d='M {hexagon.GetPolygon(1).Select(p => $"{p.X},{p.Y}").JoinString(", ")} z' />" : null) +
                    arrowAngle.NullOr(aa => $"<path transform='rotate({aa} {hexagon.GetCenter(1).X} {hexagon.GetCenter(1).Y})' stroke='none' fill='#ccc' d='M {"-2,-3;2,-3;2,1;4,1;0,5;-4,1;-2,1".Split(';').Select(str => str.Split(',')).Select(arr => new PointD(double.Parse(arr[0]) * .75, double.Parse(arr[1]) + 2) * .075).Select(p => p + hexagon.GetCenter(1)).Select(p => $"{p.X},{p.Y}").JoinString(", ")} z' />") +
                    $"<path d='{path.Select(pp => pp.Select(translatePoint)).JoinString(" ")}' fill='#000' />";
            }

            File.WriteAllText(@"D:\Daten\Upload\KTANE\Animals.html",
                new HTML(
                    new HEAD(
                        new TITLE("Animals!"),
                        new STYLELiteral(@"
                            table { border-collapse: collapse; }
                            td, th { border: 1px solid black; padding: .2em .5em; font-size: 20pt; text-align: center; }
                            tr.InGrid td { background: #fff; }
                            tr.OutsideGrid td { background: #ffa; }
                            tr.None td { background: #fdd; }
                            tr.duplicate:not(.None) td { border: 5px solid #f40; }
                            tr.None.duplicate td { background: #f88; }
                        ")),
                    new BODY(
                        new TABLE(
                            new TR(new TH("Char"), new TH("Shape"), new TH("Name")),
                            data.OrderBy(d => d.Use.ToString() + d.Name).Select(d => new TR { class_ = d.Use.ToString() + (data.Any(d2 => d2.Use != AnimalUse.None && d2.Name == d.Name && (d2.Font != d.Font || d2.Ch != d.Ch)) ? " duplicate" : "") }._(
                                   new TD(d.Ch),
                                   new TD(Utils.FontToSvgPath(d.Ch.ToString(), d.Font, 128).Apply(path =>
                                   {
                                       var xMin = path.Where(ps => ps.Points != null).Min(ps => ps.Points.Min(p => p.X));
                                       var yMin = path.Where(ps => ps.Points != null).Min(ps => ps.Points.Min(p => p.Y));
                                       var xMax = path.Where(ps => ps.Points != null).Max(ps => ps.Points.Max(p => p.X));
                                       var yMax = path.Where(ps => ps.Points != null).Max(ps => ps.Points.Max(p => p.Y));
                                       return new RawTag($@"<svg style='max-width: 75px; max-height: 75px;' viewBox='{xMin} {yMin} {xMax - xMin} {yMax - yMin}'><path d='{path.JoinString(" ")}' fill='#000' /></svg>");
                                   })),
                                   new TD(d.Name)))),
                        new P($"In-grid animals: {inGridIxs.Length}; need {Hex.LargeHexagon(sideLength).Count()}"),
                        new P($"Outside-grid animals: {outsideGridIxs.Length}; need 18"),
                        new RawTag(
                            $@"<svg width='1000' viewBox='-3.5 -4.6 7.5 8.7'>
                                {Hex.LargeHexagon(sideLength + 1)
                                    .Select(h =>
                                        // Along the top edge (11 & 1 o’clock)
                                        (h.R == -sideLength && h.Q < sideLength) || (h.Q + h.R == -sideLength && h.R < 0) ?
                                            //$"<path stroke='none' fill='rgba(128, 192, 255, .5)' d='M {h.GetPolygon(1).Select(p => $"{p.X},{p.Y}").JoinString(", ")} z' />" +
                                            //$"<text x='{h.GetCenter(1).X}' y='{h.GetCenter(1).Y + .3}' style='text-anchor: middle' font-family='Special Elite' font-size='.2'>{(char) (h.Q + 4 + 'A')}</text>" :
                                            svgForAnimal(outsideGridIxs[h.Q + 4], h, sizeFactor: .3, arrowAngle: 0) :
                                        // Along the south-east edge (3 & 5 o’clock)
                                        (h.Q == sideLength && h.R > -sideLength) || (h.Q + h.R == sideLength && h.Q > 0) ?
                                            //$"<path stroke='none' fill='rgba(128, 255, 192, .5)' d='M {h.GetPolygon(1).Select(p => $"{p.X},{p.Y}").JoinString(", ")} z' />" +
                                            //$"<text x='{h.GetCenter(1).X}' y='{h.GetCenter(1).Y + .3}' style='text-anchor: middle' font-family='Special Elite' font-size='.2'>{h.Q}, {h.R}</text>" :
                                            svgForAnimal(outsideGridIxs[h.R + 13], h, sizeFactor: .3, arrowAngle: 120) :
                                        null
                                    )
                                    .JoinString()}
                                <path stroke-width='.1' stroke='#000' fill='none' d='M {Hex.LargeHexagonOutline(sideLength, 1).Select(p => $"{p.X},{p.Y}").JoinString(", ")} z' />
                                {Hex.LargeHexagon(sideLength).Select((h, i) => svgForAnimal(inGridIxs[i], h, withHexOutline: true)).JoinString()}
                            </svg>"
                        )))
                    .ToString());
        }
    }
}