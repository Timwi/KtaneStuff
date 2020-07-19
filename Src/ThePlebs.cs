using System;
using System.Collections.Generic;
using System.Linq;
using RT.TagSoup;
using RT.Util.Consoles;
using RT.Util.ExtensionMethods;
using RT.Util.Text;

namespace KtaneStuff
{
    static class ThePlebs
    {
        public static void Analyze()
        {
            var solution = @"POTATOES";
            var ponyNames = "Amethyst Star,Apple Cinnamon,Apple Fritter,Babs Seed,Berryshine,Big McIntosh,Bulk Biceps,Cadance,Golden Harvest,Celestia,Cheerilee,Cheese Sandwich,Cherry Jubilee,Coco Pommel,Coloratura,Daisy,Daring Do,Derpy,Diamond Tiara,Double Diamond,Filthy Rich,Granny Smith,Hoity Toity,Lightning Dust,Lily,Luna,Lyra,Maud Pie,Mayor Mare,Moon Dancer,Ms. Harshwhinny,Night Light,Nurse Redheart,Octavia Melody,Rose,Screwball,Shining Armor,Silver Shill,Silver Spoon,Silverstar,Spoiled Rich,Starlight Glimmer,Sunburst,Sunset Shimmer,Suri Polomare,Sweetie Drops,Thunderlane,Time Turner,Toe Tapper,Tree Hugger,Trenderhoof,Trixie,Trouble Shoes,Twilight Velvet,Twist,Vinyl Scratch".Split(',');
            var top = "JGUKV8LCH4WPMR";
            var right = "R4X7BCMDLNAPFE";
            var bottom = "YNBGWSMQK9CVDE";
            var left = "J7QDAV4GKS8WMY";
            static string canonize(string str) => str.ToUpperInvariant().Where(ch => ch >= 'A' && ch <= 'Z').JoinString();
            var coordinations =
                Enumerable.Range(0, 14).Select(ix => new { Char = top[ix], Dir = 'U', Pony = canonize(ponyNames[ix]) }).Concat(
                Enumerable.Range(0, 14).Select(ix => new { Char = right[ix], Dir = 'R', Pony = canonize(ponyNames[ix + 14]) }).Concat(
                Enumerable.Range(0, 14).Select(ix => new { Char = bottom[13 - ix], Dir = 'D', Pony = canonize(ponyNames[ix + 2 * 14]) }).Concat(
                Enumerable.Range(0, 14).Select(ix => new { Char = left[13 - ix], Dir = 'L', Pony = canonize(ponyNames[ix + 3 * 14]) }))))
                .Where(inf => inf.Char >= 'A' && inf.Char <= 'Z').ToArray();
            var buckets = new Dictionary<(int, char), List<string>>();
            var brushStrokesIndexes = new[] { 0, 1, 2, 4, 7, 10 };
            var directions = "UDLR";
            foreach (var letter in solution.Distinct())
            {
                foreach (var inf in coordinations)
                    foreach (var ix in brushStrokesIndexes.Where(ix => ix < inf.Pony.Length && inf.Pony[ix] == letter))
                        if (!directions.Contains(inf.Char))
                            buckets.AddSafe((ix, inf.Dir), $"{letter}: {inf.Char} {ix + 1} {inf.Dir} ({inf.Pony})");
            }
            var tt = new TextTable { ColumnSpacing = 3, RowSpacing = 1, VerticalRules = true, HorizontalRules = true };
            for (var col = 0; col < brushStrokesIndexes.Length; col++)
            {
                tt.SetCell(col + 1, 0, (brushStrokesIndexes[col] + 1).ToString().Color(ConsoleColor.White));
                for (var row = 0; row < directions.Length; row++)
                {
                    tt.SetCell(0, row + 1, directions[row].ToString().Color(ConsoleColor.White));
                    tt.SetCell(col + 1, row + 1, buckets.Get((brushStrokesIndexes[col], directions[row]), new List<string>()).JoinString("\n"));
                }
            }
            tt.WriteToConsole();
        }

        public static void CreateProtectionPackage()
        {
            var strings = "FORGET,INDIGO,STRIKE,HAMMER,YOGURT,WSBGKC".Split(',');
            var columns = Enumerable.Range(0, 6).Select(ix => strings.Select(str => str[ix]).Order().ToArray()).ToArray();
            var rows = Enumerable.Range(0, 6).Select(ix => columns.Select(col => col[ix]).JoinString()).ToArray();
            Console.WriteLine(rows.JoinString("\n"));
        }

        public static void AnalyzeFastMath()
        {
            for (var i = 0; i < (1 << 5); i++)
            {
                var r = 0;
                r += (i & (1 << 0)) != 0 ? 20 : 0;
                r += (i & (1 << 1)) != 0 ? 14 : 0;
                r += (i & (1 << 2)) != 0 ? -5 : 0;
                r += (i & (1 << 3)) != 0 ? 27 : 0;
                r += (i & (1 << 4)) != 0 ? -15 : 0;
                if (new[] { 14, 33, 21 }.Contains(r % 35))
                    Console.WriteLine($"{i:X} = {r} = {r % 35}");
            }
        }
    }
}