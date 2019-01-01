using System.Linq;
using System.Windows.Forms;
using RT.Util.ExtensionMethods;

namespace KtaneStuff
{
    static class StreetFighter
    {
        public static void DoManual()
        {
            var array = @"Balrog;USA;4;Ken;USA;6
Blanka;Brazil;2;M. Bison;Unknown;11
Chun Li;China;7;Ryu;Japan;0
Dhalsim;India;9;Sagat;Thailand;10
E. Honda;Japan;1;Vega;Spain;5
Guile;USA;3;Zangief;USSR;8".Replace("\r", "").Split('\n').Select(line => line.Split(';')).SelectMany(arr => new[] { new { Name = arr[0], Country = arr[1], Index = int.Parse(arr[2]) }, new { Name = arr[3], Country = arr[4], Index = int.Parse(arr[5]) } }).ToArray();

            foreach (var inf in array.OrderBy(i => i.Name))
                System.Console.WriteLine($"{inf.Name} = {(array.Count(inf2 => inf2.Name.ToUpperInvariant().Intersect(inf.Country.ToUpperInvariant()).Any()) + inf.Name.Count(ch => char.IsLetter(ch))) % 12}");
            Clipboard.SetText(array.OrderBy(inf => inf.Index).Select(inf => $"<li>{inf.Index} = {inf.Name}</li>").JoinString("\n"));
        }
    }
}