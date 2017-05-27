using System;
using System.IO;
using System.Linq;
using System.Text;
using RT.TagSoup;
using RT.Util;
using RT.Util.ExtensionMethods;

namespace KtaneStuff
{
    static class CheapCheckout
    {
        [Flags]
        enum Categories
        {
            None = 0,
            FixedPrice = 1 << 0,
            Sweet = 1 << 1,
            Fruit = 1 << 2,
            ContainsS = 1 << 3
        }

        public static void DoCheatSheet()
        {
            var items = Ut.NewArray(
                new { Name = "Candy Canes", Price = 3.51m },
                new { Name = "Socks", Price = 6.97m },
                new { Name = "Lotion", Price = 7.97m },
                new { Name = "Cheese", Price = 4.49m },
                new { Name = "Mints", Price = 6.39m },
                new { Name = "Grape Jelly", Price = 2.98m },
                new { Name = "Honey", Price = 8.25m },
                new { Name = "Sugar", Price = 2.08m },
                new { Name = "Soda", Price = 2.05m },
                new { Name = "Tissues", Price = 3.94m },
                new { Name = "White Bread", Price = 2.43m },
                new { Name = "Canola Oil", Price = 2.28m },
                new { Name = "Mustard", Price = 2.36m },
                new { Name = "Deodorant", Price = 3.97m },
                new { Name = "White Milk", Price = 3.62m },
                new { Name = "Pasta Sauce", Price = 2.30m },
                new { Name = "Lollipops", Price = 2.61m },
                new { Name = "Cookies", Price = 2.00m },
                new { Name = "Paper Towels", Price = 9.46m },
                new { Name = "Tea", Price = 2.35m },
                new { Name = "Coffee Beans", Price = 7.85m },
                new { Name = "Mayonnaise", Price = 3.99m },
                new { Name = "Chocolate Milk", Price = 5.68m },
                new { Name = "Fruit Punch", Price = 2.08m },
                new { Name = "Potato Chips", Price = 3.25m },
                new { Name = "Shampoo", Price = 4.98m },
                new { Name = "Toothpaste", Price = 2.50m },
                new { Name = "Peanut Butter", Price = 5.00m },
                new { Name = "Gum", Price = 1.12m },
                new { Name = "Water Bottles", Price = 9.37m },
                new { Name = "Spaghetti", Price = 2.92m },
                new { Name = "Chocolate Bar", Price = 2.10m },
                new { Name = "Ketchup", Price = 3.59m },
                new { Name = "Cereal", Price = 4.19m });

            var itemsLb = Ut.NewArray(
                new { Name = "Turkey", Price = 2.98m },
                new { Name = "Chicken", Price = 1.99m },
                new { Name = "Steak", Price = 4.97m },
                new { Name = "Pork", Price = 4.14m },
                new { Name = "Lettuce", Price = 1.10m },
                new { Name = "Potatoes", Price = 0.68m },
                new { Name = "Tomatoes", Price = 1.80m },
                new { Name = "Broccoli", Price = 1.39m },
                new { Name = "Oranges", Price = 0.80m },
                new { Name = "Lemons", Price = 1.74m },
                new { Name = "Bananas", Price = 0.87m },
                new { Name = "Grapefruit", Price = 1.08m });

            var fruits = new[] { "Bananas", "Grapefruit", "Lemons", "Oranges", "Tomatoes" };
            var sweets = new[] { "Candy Canes", "Mints", "Honey", "Soda", "Lollipops", "Gum", "Chocolate Bar", "Fruit Punch", "Cookies", "Sugar", "Grape Jelly" };

            var days = Ut.NewArray(
                new
                {
                    Name = "Monday",
                    FlavourText = "Meddle with murderous money on Malleable Monday!",
                    Rules = Ut.NewArray(
                        new { Name = "1/3|5", Rule = Ut.Lambda((decimal d, Categories categories) => !categories.HasFlag(Categories.FixedPrice) ? decimal.Round(d * .85m, 2, MidpointRounding.AwayFromZero) : d) },
                        new { Name = "2/4|6", Rule = Ut.Lambda((decimal d, Categories categories) => categories.HasFlag(Categories.FixedPrice) ? decimal.Round(d * .85m, 2, MidpointRounding.AwayFromZero) : d) })
                },
                new
                {
                    Name = "Tuesday",
                    FlavourText = "Tickle your tots throwing tremendous tantrums on Troublesome Tuesday!",
                    Rules = Ut.NewArray(new { Name = "", Rule = Ut.Lambda((decimal d, Categories categories) => categories.HasFlag(Categories.FixedPrice) ? d + ((int) (d * 100) - 1) % 9 + 1 : d) })
                },
                new
                {
                    Name = "Wednesday",
                    FlavourText = "Wander the winds in the Wild West on Wacky Wednesday!",
                    Rules = Ut.NewArray(new
                    {
                        Name = "",
                        Rule = Ut.Lambda((decimal d, Categories categories) =>
                        {
                            int val = (int) (d % 10m);
                            int val2 = (int) (d * 10m) % 10;
                            int val3 = (int) (d * 100m) % 10;
                            int highest = Math.Max(Math.Max(val, val2), val3);
                            int lowest = Math.Min(Math.Min(val, val2), val3);
                            return decimal.Parse(d.ToString("N2").Where(ch => ch != '.').Select(ch => ch - '0').Select(dgt => dgt == highest ? lowest : dgt == lowest ? highest : dgt).JoinString()) * .01m;
                        })
                    })
                },
                new
                {
                    Name = "Thursday",
                    FlavourText = "Throw out your thunderous things throughout Thrilling Thursday!",
                    Rules = Ut.NewArray(
                        new { Name = "1/3|5", Rule = Ut.Lambda((decimal d, Categories categories) => decimal.Round(d / 2, 2)) },
                        new { Name = "2/4|6", Rule = Ut.Lambda((decimal d, Categories categories) => d) })
                },
                new
                {
                    Name = "Friday",
                    FlavourText = "Fancy your fellow fleshy friends on Fruity Friday!",
                    Rules = Ut.NewArray(new { Name = "", Rule = Ut.Lambda((decimal d, Categories categories) => categories.HasFlag(Categories.Fruit) ? decimal.Round(d * 1.25m, 2) : d) })
                },
                new
                {
                    Name = "Saturday",
                    FlavourText = "Stock up your supply of satisfying sugary surprises on Sweet Saturday!",
                    Rules = Ut.NewArray(new { Name = "", Rule = Ut.Lambda((decimal d, Categories categories) => categories.HasFlag(Categories.Sweet) ? decimal.Round(d * .65m, 2) : d) })
                },
                new
                {
                    Name = "Sunday",
                    FlavourText = "Set the scene for seeing smashing stuff on Special Sunday!",
                    Rules = Ut.NewArray(new { Name = "", Rule = Ut.Lambda((decimal d, Categories categories) => categories.HasFlag(Categories.FixedPrice) && categories.HasFlag(Categories.ContainsS) ? d + 2.15m : d) })
                }
            );

            File.WriteAllText(@"D:\c\KTANE\Public\HTML\Cheap Checkout cheat sheet (Timwi).html", $@"<!DOCTYPE html>
<html class='no-js'>
<head>
    <meta content='text/html; charset=utf-8' http-equiv='Content-Type'>
    <meta content='IE=edge' http-equiv='X-UA-Compatible'>
    <title>Cheap Checkout — Keep Talking and Nobody Explodes</title>
    <meta content='initial-scale=1' name='viewport'>
    <link href='css/font.css' rel='stylesheet' type='text/css'>
    <link href='css/normalize.css' rel='stylesheet' type='text/css'>
    <link href='css/main.css' rel='stylesheet' type='text/css'>
    <script src='js/highlighter.js'></script>
    <style>
        table {{ font-size: 90%; }}
        table.fixed-price-items {{ float: left; }}
        table.non-fixed-price-items {{ float: right; margin-top: 2em; }}
        div.clear {{ clear: both; }}
        th.item {{ text-align: left; }}
    </style>
</head>
<body>
<div class='section'>
    {days.Select((day, ix) => $@"
        <div class='page page-bg-0{ix + 1}'>
            <div class='page-header'>
			    <span class='page-header-doc-title'>Keep Talking and Nobody Explodes Mod</span>
			    <span class='page-header-section-title'>Cheap Checkout</span>
		    </div>
            <div class='page-content'>
                <h2>On the Subject of Cheap Checkouts on {day.Name}</h2>
                <p class='flavour-text'>{day.FlavourText}

                {new[] { 0, 1 }.Select(tblIx => new TABLE { class_ = $"fixed-price-items fixed-price-items-{tblIx + 1}" }._(
                    new TR(new TH("Item"), day.Rules.Select(r => new TH(r.Name.Contains('|') ? r.Name.Split('|')[0] : r.Name))),
                    items.OrderBy(item => item.Name).Skip(23 * tblIx).Take(23).Select(item => new TR(new TH { class_ = "item" }._(item.Name), day.Rules.Select(r => new TD(r.Rule(item.Price,
                        (item.Name.ToLowerInvariant().Contains('s') ? Categories.ContainsS : 0) |
                        (fruits.Contains(item.Name) ? Categories.Fruit : 0) |
                        (sweets.Contains(item.Name) ? Categories.Sweet : 0)).ToString("N2")))))).ToString()).JoinString()}

                {new TABLE { class_ = "non-fixed-price-items" }._(
                    day.Rules.Length == 1
                        ? new TR(new TH("Item"), day.Rules.Select(r => new[] { "½ lb", "1 lb", "1½ lb" }.Select(x => new TH(x))))
                        : (object) Ut.NewArray(
                            new TR(new TH { rowspan = 2 }._("Item"), day.Rules.Select(r => new TH { colspan = 3 }._(r.Name.Contains('|') ? r.Name.Split('|')[1] : r.Name))),
                            new TR(day.Rules.Select(r => new[] { "½ lb", "1 lb", "1½ lb" }.Select(x => new TH(x))))
                        ),
                    itemsLb.OrderBy(item => item.Name).Select(item => new TR(new TH { class_ = "item" }._(item.Name), day.Rules.SelectMany(r => new[] { .5m, 1m, 1.5m }.Select(x => new TD(r.Rule(decimal.Round(item.Price * x, 2),
                        (item.Name.ToLowerInvariant().Contains('s') ? Categories.ContainsS : 0) |
                        (fruits.Contains(item.Name) ? Categories.Fruit : 0) |
                        (sweets.Contains(item.Name) ? Categories.Sweet : 0) | Categories.FixedPrice).ToString("N2"))))))).ToString()}

                <div class='clear'></div>
            </div>
		    <div class='page-footer relative-footer'>Page {ix + 1} of 7 ({day.Name})</div>
        </div>
    ").JoinString()}
</div>
</body></html>
");
        }
    }
}