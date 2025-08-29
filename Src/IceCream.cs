using RT.Modeling;
using RT.TagSoup;
using RT.Util;
using RT.Util.Drawing;
using RT.Util.ExtensionMethods;
using static RT.Modeling.Md;

namespace KtaneStuff;

internal static class IceCream
{
    [Flags]
    private enum data
    {
        // Ingredients in the recipes
        VanillaFlavour = 1 << 0,
        FruitPieces = 1 << 1,
        ChocolateFlavour = 1 << 2,
        Nuts = 1 << 3,
        Marshmallows = 1 << 4,
        RaspberrySauce = 1 << 5,
        ChocolateChips = 1 << 6,
        StrawberryFlavour = 1 << 7,
        Strawberries = 1 << 8,
        Cookies = 1 << 9,
        MintFlavour = 1 << 10,
        ChocolateSauce = 1 << 11,
        Cherry = 1 << 12,

        // Allergies
        AllergyChocolate = ChocolateChips | ChocolateSauce | ChocolateFlavour,
        AllergyStrawberry = FruitPieces | Strawberries | StrawberryFlavour,
        AllergyRaspberry = RaspberrySauce | FruitPieces,
        AllergyNuts = Nuts,
        AllergyCookies = Cookies,
        AllergyMint = MintFlavour,
        AllergyFruit = FruitPieces | RaspberrySauce | Strawberries | Cherry | StrawberryFlavour,
        AllergyCherry = Cherry | FruitPieces,
        AllergyMarshmallows = Marshmallows
    }

    public static void CreateCheatSheet()
    {
        var recipes = new Dictionary<string, data>
        {
            { "Tutti Frutti", data.VanillaFlavour | data.FruitPieces },
            { "Rocky Road", data.ChocolateFlavour | data.Nuts | data.Marshmallows },
            { "Raspberry Ripple", data.VanillaFlavour | data.RaspberrySauce },
            { "Double Chocolate", data.ChocolateFlavour | data.ChocolateChips },
            { "Double Strawberry", data.StrawberryFlavour | data.Strawberries },
            { "Cookies and Cream", data.VanillaFlavour | data.Cookies },
            { "Neapolitan", data.StrawberryFlavour | data.ChocolateFlavour | data.VanillaFlavour },
            { "Mint Chocolate Chip", data.MintFlavour | data.ChocolateChips },
            { "The Classic", data.VanillaFlavour | data.ChocolateSauce | data.Cherry }
        };

        var abbrevs = new Dictionary<string, string>
        {
            { "Tutti Frutti", "TF" },
            { "Rocky Road", "Ro" },
            { "Raspberry Ripple", "Ra" },
            { "Double Chocolate", "DC" },
            { "Double Strawberry", "DS" },
            { "Cookies and Cream", "CC" },
            { "Neapolitan", "Ne" },
            { "Mint Chocolate Chip", "Mi" },
            { "The Classic", "TC" }
        };

        var allergies = Ut.NewArray(
            data.AllergyChocolate,
            data.AllergyStrawberry,
            data.AllergyRaspberry,
            data.AllergyNuts,
            data.AllergyCookies,
            data.AllergyMint,
            data.AllergyFruit,
            data.AllergyCherry,
            data.AllergyMarshmallows);

        var customers = Ut.NewArray(
            new { Name = "Mike", Allergies = new int[][] { [1, 5, 0], [6, 8, 3], [0, 7, 1], [4, 3, 2], [3, 6, 1] } },
            new { Name = "Tim", Allergies = new int[][] { [0, 8, 3], [2, 1, 4], [4, 3, 5], [2, 6, 7], [1, 4, 3] } },
            new { Name = "Tom", Allergies = new int[][] { [8, 4, 5], [1, 6, 7], [2, 5, 6], [3, 7, 5], [3, 6, 1] } },
            new { Name = "Dave", Allergies = new int[][] { [2, 6, 7], [0, 1, 4], [8, 2, 3], [7, 8, 1], [5, 7, 3] } },
            new { Name = "Adam", Allergies = new int[][] { [3, 4, 1], [3, 6, 2], [0, 2, 1], [2, 4, 7], [8, 5, 6] } },
            new { Name = "Cheryl", Allergies = new int[][] { [1, 6, 3], [7, 5, 2], [1, 4, 5], [4, 2, 0], [3, 7, 5] } },
            new { Name = "Sean", Allergies = new int[][] { [4, 6, 1], [2, 3, 6], [1, 5, 7], [6, 8, 2], [2, 7, 4] } },
            new { Name = "Ashley", Allergies = new int[][] { [6, 2, 5], [4, 1, 7], [0, 8, 2], [1, 2, 6], [3, 6, 7] } },
            new { Name = "Jessica", Allergies = new int[][] { [4, 2, 6], [1, 2, 3], [0, 3, 4], [6, 5, 0], [4, 7, 8] } },
            new { Name = "Taylor", Allergies = new int[][] { [6, 3, 5], [5, 1, 2], [4, 2, 6], [7, 1, 0], [3, 7, 2] } },
            new { Name = "Simon", Allergies = new int[][] { [0, 3, 5], [1, 6, 4], [5, 4, 8], [2, 0, 7], [7, 3, 6] } },
            new { Name = "Sally", Allergies = new int[][] { [4, 6, 3], [1, 0, 2], [6, 7, 4], [2, 5, 8], [0, 3, 1] } },
            new { Name = "Jade", Allergies = new int[][] { [3, 7, 1], [0, 8, 2], [7, 1, 3], [6, 7, 8], [4, 5, 1] } },
            new { Name = "Sam", Allergies = new int[][] { [2, 4, 1], [7, 8, 0], [3, 4, 6], [1, 0, 3], [6, 5, 2] } },
            new { Name = "Gary", Allergies = new int[][] { [1, 2, 5], [6, 8, 0], [3, 2, 1], [7, 4, 5], [1, 8, 4] } },
            new { Name = "Victor", Allergies = new int[][] { [0, 3, 1], [2, 5, 7], [3, 4, 6], [6, 7, 1], [5, 3, 0] } },
            new { Name = "George", Allergies = new int[][] { [8, 1, 2], [6, 4, 8], [0, 4, 3], [1, 6, 4], [3, 2, 5] } },
            new { Name = "Jacob", Allergies = new int[][] { [7, 3, 2], [1, 5, 6], [5, 4, 7], [3, 4, 0], [6, 2, 1] } },
            new { Name = "Pat", Allergies = new int[][] { [5, 6, 2], [1, 3, 6], [3, 4, 7], [2, 0, 5], [8, 1, 3] } },
            new { Name = "Bob", Allergies = new int[][] { [5, 6, 8], [2, 1, 0], [4, 8, 2], [4, 2, 5], [0, 5, 1] } }
        ).Select(customer => new { customer.Name, Allergies = customer.Allergies.Select(arr => arr.Aggregate((data) 0, (p, n) => (data) p | allergies[n])).ToArray() }).ToArray();

        var tables = Ut.NewArray(
            new { Condition = "More lit than unlit indicators", Flavours = new[] { "Cookies and Cream", "Neapolitan", "Tutti Frutti", "The Classic", "Rocky Road", "Double Chocolate", "Mint Chocolate Chip", "Double Strawberry", "Raspberry Ripple" } },
            new { Condition = "Empty port plate", Flavours = new[] { "Double Chocolate", "Mint Chocolate Chip", "Neapolitan", "Rocky Road", "Tutti Frutti", "Double Strawberry", "Cookies and Cream", "Raspberry Ripple", "The Classic" } },
            new { Condition = "≥ 3 batteries", Flavours = new[] { "Neapolitan", "Tutti Frutti", "Cookies and Cream", "Raspberry Ripple", "Double Strawberry", "Mint Chocolate Chip", "Double Chocolate", "The Classic", "Rocky Road" } },
            new { Condition = "Otherwise", Flavours = new[] { "Double Strawberry", "Cookies and Cream", "Rocky Road", "The Classic", "Neapolitan", "Double Chocolate", "Tutti Frutti", "Raspberry Ripple", "Mint Chocolate Chip" } });

        var stuff = new HashSet<string>();

        File.WriteAllText(
            @"D:\c\KTANE\Public\HTML\Ice Cream cheat sheet (Timwi).html",
            Utils.CreateManualPage("Ice Cream", omitDiagram: true, onTheSubjectOf: "Impatiently Conquering an Insatiable Craving for Ice Cream", css: $@"
                    table.main-table {{ clear: both; font-size: 10pt; }}
                ", pages: tables.Select(table => Ut.NewArray<object>(
                new H3(table.Condition),
                new TABLE { class_ = "main-table" }._(
                    new TR(new TH { rowspan = 2 }._("Customer"), new TH { colspan = 5 }._("Last digit of the serial number")),
                    new TR("0 or 1|2 or 3|4 or 5|6 or 7|8 or 9".Split('|').Select(s => new TH(s))),
                    customers.OrderBy(customer => customer.Name).Select(customer => new TR(
                        new TH(customer.Name), customer.Allergies.Select(alg => new TD()._(() => { var str = table.Flavours.Where(fl => (recipes[fl] & alg) == 0).Select(fl => abbrevs[fl]).JoinString(", "); stuff.Add(str); return str; })))))))
                .Concat(Ut.NewArray<object>(new H3("Abbreviations"), new TABLE { class_ = "abbrevs-table" }._(abbrevs.OrderBy(p => p.Value).Select(p => new TR(new TH(p.Value), new TD(p.Key))))))
                .ToArray())
            .ToString());
    }

    public static void CreateIceCreamConeModel()
    {
        const double b = .1;
        const double h = 2;
        const double coneAngle = 15;
        const double w = .05;
        const double t = .1;

        const int bRev = 36;
        const int rev = 36;
        const int cuts = 4;

        var pivot = p(-h * tan(coneAngle), h);

        var curve = Enumerable.Range(0, bRev).Select(i => i * (90 - coneAngle) / (bRev - 1)).Select(angle => new { Point = p(-b * sin(angle), -b * cos(angle) + b / sin(coneAngle)), Normal = Normal.Average })
            .Concat(Enumerable.Range(0, cuts).Select(i => i / (cuts - 1.0)).Select(pr => new { Point = p(-b * sin(90 - coneAngle), -b * cos(90 - coneAngle) + b / sin(coneAngle)) * (1 - pr) + pivot * pr, Normal = Normal.Mine }));
        //.Concat(new { Point = p(-b * sin(90 - coneAngle), -b * cos(90 - coneAngle) + b / sin(coneAngle)), Normal = Normal.Mine })
        //.Concat(new { Point = pivot, Normal = Normal.Mine });

        var surface = Enumerable.Range(0, rev + 1)
            .Select(i => 360.0 * i / rev)
            .Select(angle => curve.Select(p => pt(p.Point.X * cos(angle), p.Point.Y, p.Point.X * sin(angle)).WithMeshInfo(Normal.Average, Normal.Average, p.Normal, p.Normal).WithTexture(angle / 360.0, p.Point.Y / (h + t))).ToArray());

        var rimCurve = new[] { pivot + p(-w / 2, 0), pivot + p(0, 0), pivot + p(0, t), pivot + p(-w / 2, t), pivot + p(-w, t / 2) };
        var rim = Enumerable.Range(0, rev)
            .Select(i => 360.0 * i / rev)
            .Select(angle => rimCurve.Select(p => pt(p.X * cos(angle), p.Y, p.X * sin(angle)).WithMeshInfo(Normal.Average, Normal.Average, Normal.Mine, Normal.Mine)).ToArray())
            .ToArray();

        File.WriteAllText(@"D:\c\KTANE\IceCream\Assets\Models\IceCreamCone.obj",
            GenerateObjFile(
                CreateMesh(false, false, surface.ToArray())
                    .Concat(CreateMesh(false, false, surface.Reverse().ToArray()))
                    .Concat(CreateMesh(true, true, rim)),
                "IceCreamCone"));

        static Color color(string hex) => Color.FromArgb(Convert.ToInt32(hex.Substring(0, 2), 16), Convert.ToInt32(hex.Substring(2, 2), 16), Convert.ToInt32(hex.Substring(4, 2), 16));
        const int bw = 500;
        const int bh = 400;
        GraphicsUtil.DrawBitmap(bw, bh, g =>
        {
            g.Clear(color("EAB87F"));

            using (var pen = new Pen(color("A48662"), 5f))
            using (var tr = new GraphicsTransformer(g).Translate(2.5, 0))
                for (var i = -bh; i <= bw + bh; i += 50)
                {
                    g.DrawLine(pen, i, 0, i + bh, bh);
                    g.DrawLine(pen, i + bh, 0, i, bh);
                }
            using (var pen = new Pen(color("FFD197"), 5f))
                for (var i = -bh; i <= bw + bh; i += 50)
                {
                    g.DrawLine(pen, i, 0, i + bh, bh);
                    g.DrawLine(pen, i + bh, 0, i, bh);
                }

        }).Save(@"D:\c\KTANE\IceCream\Assets\Textures\Waffle.png");
    }
}
