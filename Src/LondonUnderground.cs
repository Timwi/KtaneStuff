using System;
using RT.Util.ExtensionMethods;
using System.Linq;
using RT.Util;
using System.IO;

namespace KtaneStuff
{
    static class LondonUnderground
    {
        private static readonly string[][] _stations = Ut.NewArray(
            new[] { "Stonebridge Park", "Harlesden", "Willesden Junction", "Kensal Green", "Queen’s Park", "Kilburn Park", "Maida Vale", "Warwick Avenue", "Paddington", "Edgware Road", "Marylebone", "Baker Street", "Regent’s Park", "Oxford Circus", "Piccadilly Circus", "Charing Cross", "Embankment", "Waterloo", "Lambeth North", "Elephant & Castle" },
            new[] { "Hanger Lane", "Ealing Broadway", "West Acton", "North Acton", "East Acton", "White City/Wood Lane", "Shepherd’s Bush", "Holland Park", "Notting Hill Gate", "Queensway", "Lancaster Gate", "Marble Arch", "Bond Street", "Oxford Circus", "Tottenham Court Road", "Holborn", "Chancery Lane", "St. Paul’s", "Monument/Bank", "Liverpool Street", "Bethnal Green", "Mile End", "Stratford", "Leyton", "Leytonstone" },
            new[] { "Hammersmith", "Goldhawk Road", "Shepherd’s Bush Market", "White City/Wood Lane", "Latimer Road", "Ladbroke Grove", "Westbourne Park", "Royal Oak", "Paddington", "Edgware Road", "Bayswater", "Notting Hill Gate", "High Street Kensington", "Gloucester Road", "South Kensington", "Sloane Square", "Victoria", "St. James’s Park", "Westminster", "Embankment", "Temple", "Blackfriars", "Mansion House", "Cannon Street", "Monument/Bank", "Tower Hill", "Aldgate", "Liverpool Street", "Moorgate", "Barbican", "Farringdon", "King’s Cross St. Pancras", "Euston Square", "Great Portland Street", "Baker Street" },
            new[] { "Ealing Broadway", "Ealing Common", "Acton Town", "Chiswick Park", "Turnham Green", "Stamford Brook", "Ravenscourt Park", "Hammersmith", "Barons Court", "West Kensington", "Earl’s Court", "Gloucester Road", "South Kensington", "Sloane Square", "Victoria", "St. James’s Park", "Westminster", "Embankment", "Temple", "Blackfriars", "Mansion House", "Cannon Street", "Monument/Bank", "Tower Hill", "Aldgate East", "Whitechapel", "Stepney Green", "Mile End", "Bow Road", "Bromley-by-Bow", "West Ham", "Plaistow", "Upton Park", "East Ham", "High Street Kensington", "Notting Hill Gate", "Bayswater", "Paddington", "Edgware Road" },
            new[] { "Hammersmith", "Goldhawk Road", "Shepherd’s Bush Market", "White City/Wood Lane", "Latimer Road", "Ladbroke Grove", "Westbourne Park", "Royal Oak", "Paddington", "Edgware Road", "Baker Street", "Great Portland Street", "Euston Square", "King’s Cross St. Pancras", "Farringdon", "Barbican", "Moorgate", "Liverpool Street", "Aldgate East", "Whitechapel", "Stepney Green", "Mile End", "Bow Road", "Bromley-by-Bow", "West Ham", "Plaistow", "Upton Park", "East Ham" },
            new[] { "Neasden", "Dollis Hill", "Willesden Green", "Kilburn", "West Hampstead", "Finchley Road", "Swiss Cottage", "St. John’s Wood", "Baker Street", "Bond Street", "Green Park", "Westminster", "Waterloo", "Southwark", "London Bridge", "Bermondsey", "Canada Water", "Canary Wharf", "North Greenwich", "Canning Town", "West Ham", "Stratford" },
            new[] { "Finchley Road", "Baker Street", "Great Portland Street", "Euston Square", "King’s Cross St. Pancras", "Farringdon", "Barbican", "Moorgate", "Liverpool Street", "Aldgate" },
            new[] { "Clapham South", "Clapham Common", "Clapham North", "Stockwell", "Oval", "Kennington", "Waterloo", "Embankment", "Charing Cross", "Leicester Square", "Tottenham Court Road", "Goodge Street", "Warren Street", "Euston", "Mornington Crescent", "Camden Town", "Chalk Farm", "Belsize Park", "Hampstead", "Golders Green", "Brent Cross", "Hendon Central", "Elephant & Castle", "Borough", "London Bridge", "Monument/Bank", "Moorgate", "Old Street", "Angel", "King’s Cross St. Pancras", "Kentish Town", "Tufnell Park", "Archway", "Highgate", "East Finchley" },
            new[] { "Northfields", "South Ealing", "Acton Town", "Turnham Green", "Hammersmith", "Barons Court", "Earl’s Court", "Gloucester Road", "South Kensington", "Knightsbridge", "Hyde Park Corner", "Green Park", "Piccadilly Circus", "Leicester Square", "Covent Garden", "Holborn", "Russell Square", "King’s Cross St. Pancras", "Caledonian Road", "Holloway Road", "Arsenal", "Finsbury Park", "Manor House", "Turnpike Lane", "Wood Green", "Bounds Green", "Ealing Common", "North Ealing", "Park Royal" },
            new[] { "Brixton", "Stockwell", "Vauxhall", "Pimlico", "Victoria", "Green Park", "Oxford Circus", "Warren Street", "Euston", "King’s Cross St. Pancras", "Highbury & Islington", "Finsbury Park", "Seven Sisters", "Tottenham Hale", "Blackhorse Road", "Walthamstow Central" });
        private static readonly string[] _lines = new[] { "Bakerloo Line", "Central Line", "Circle Line", "District Line", "Hammersmith & City Line", "Jubilee Line", "Metropolitan Line", "Northern Line", "Piccadilly Line", "Victoria Line" };
        private static readonly string[] _cssNames = new[] { "bakerloo", "central", "circle", "district", "hammersmith-and-city", "jubilee", "metropolitan", "northern", "piccadilly", "victoria" };
        private static readonly string[] _lineAbbrevs = new[] { "B", "Ce", "Ci", "D", "H", "J", "M", "N", "P", "V" };

        public static void CreateGoodsheet()
        {
            var lineHtmls = _lines.Order().Select(line =>
            {
                var lineIx = _lines.IndexOf(line);
                return $@"<td><ol>{_stations[lineIx].Select(station =>
                {
                    var connections = Enumerable.Range(0, _stations.Length).Where(ix => ix != lineIx && _stations[ix].Contains(station)).ToArray();
                    var name = connections.Length == 0 ? station.HtmlEscape() : $"<em>{station.HtmlEscape()}</em>";
                    var arrow = connections.Length == 0 ? null : " → ";
                    return $@"<li>{name}{arrow}{connections.Select(c => $@"<span class=""{_cssNames[c]}"">{_lineAbbrevs[c]}</span>").JoinString(" ")}</li>";
                }).JoinString()}</ol></td>";
            }).ToArray<object>();

            File.WriteAllText(@"D:\c\KTANE\Public\HTML\The London Underground optimized (Timwi & ZekNikZ).html", @"<!DOCTYPE html>
<html>
<head>
    <meta http-equiv=""content-type"" content=""text/html; charset=UTF-8"">
    <meta charset=""utf-8"">
    <title>The London Underground — Keep Talking and Nobody Explodes Module</title>
    <meta name=""viewport"" content=""initial-scale=1"">
    <link rel=""stylesheet"" type=""text/css"" href=""css/normalize.css"">
    <link rel=""stylesheet"" type=""text/css"" href=""css/main.css"">
    <link rel=""stylesheet"" type=""text/css"" href=""css/font.css"">
    <script src=""js/ktane-utils.js""></script>

    <style>
        li.strong {{
            font-weight: bold;
        }}

        .london-underground {{
            font-size: 11pt;
            font-family: 'Agency FB';
            margin: 1em auto;
        }}

            .london-underground td, .london-underground th {{
                vertical-align: top;
                white-space: nowrap;
                background: white;
            }}

                .london-underground td ol {{
                    padding: 0 0 0 1em;
                    margin: 0;
                }}

            .london-underground th {{
                font-size: 16px;
            }}

        .london-underground-page-content-1 {{
            margin: 0 0.75in;
            padding-top: 4em;
            min-height: 1in;
        }}

        .london-underground-page-content-2 {{
            margin: 0 auto;
            min-height: 8.5in;
        }}

        .bakerloo {{
            background: rgb(178, 99, 0) !important;
            color: white;
        }}
        span.bakerloo {{
            padding: 0.1em;
        }}

        .central {{
            background: rgb(220, 36, 31) !important;
            color: white;
        }}
        span.central {{
            padding: 0.1em;
        }}

        .circle {{
            background: rgb(255, 211, 41) !important;
            color: black;
        }}
        span.circle {{
            padding: 0.1em;
        }}

        .district {{
            background: rgb(0, 125, 50) !important;
            color: white;
        }}
        span.district {{
            padding: 0.1em;
        }}

        .hammersmith-and-city {{
            background: rgb(244, 169, 190) !important;
            color: black;
        }}
        span.hammersmith-and-city {{
            padding: 0.1em;
        }}

        .jubilee {{
            background: rgb(161, 165, 167) !important;
            color: black;
        }}
        span.jubilee {{
            padding: 0.1em;
        }}

        .metropolitan {{
            background: rgb(155, 0, 88) !important;
            color: white;
        }}
        span.metropolitan {{
            padding: 0.1em;
        }}

        th.northern {{
            /* I know this isn't the right color, but it is what the module uses.*/
            background: white !important;
            color: black;
        }}
        span.northern {{
            padding: 0.1em;
            background: black !important;
            color: white;
        }}

        .piccadilly {{
            background: rgb(0, 25, 168) !important;
            color: white;
        }}
        span.piccadilly {{
            padding: 0.1em;
        }}

        .victoria {{
            background: rgb(0, 152, 216) !important;
            color: white;
        }}
        span.victoria {{
            padding: 0.1em;
        }}
    </style>
</head>
<body>
    <div class=""section"">
        <div class=""page page-bg-01"">
            <div class=""page-header"">
                <span class=""page-header-doc-title"">Keep Talking and Nobody Explodes Mod</span>
                <span class=""page-header-section-title"">The London Underground</span>
            </div>
            <div class=""london-underground-page-content-1"">
                <h2>On the Subject of The London Underground</h2>
                <p class=""flavour-text"">Mind the gap!</p>
            </div>
            <div class=""london-underground-page-content-2"">
                <table class=""london-underground"">
                    <tbody>
                        <tr><th class=""bakerloo"">Bakerloo</th><th class=""central"">Central</th><th class=""circle"">Circle</th><th class=""district"">District</th></tr>
                        <tr>
                            {0}
                            {1}
                            {2}
                            {3}
                        </tr>
                    </tbody>
                </table>
            </div>
            <div class=""page-footer relative-footer"">Page 1 of 3</div>
        </div>
        <div class=""page page-bg-04"">
            <div class=""page-header"">
                <span class=""page-header-doc-title"">Keep Talking and Nobody Explodes Mod</span>
                <span class=""page-header-section-title"">The London Underground</span>
            </div>
            <div class=""page-content"">
                <table class=""london-underground"">
                    <tbody>
                        <tr><th class=""hammersmith-and-city"">Hammersmith &amp; City</th><th class=""jubilee"">Jubilee</th><th class=""metropolitan"">Metropolitan</th></tr>
                        <tr>
                            {4}
                            {5}
                            {6}
                        </tr>
                    </tbody>
                </table>
            </div>
            <div class=""page-footer relative-footer"">Page 2 of 3</div>
        </div>
        <div class=""page page-bg-03"">
            <div class=""page-header"">
                <span class=""page-header-doc-title"">Keep Talking and Nobody Explodes Mod</span>
                <span class=""page-header-section-title"">The London Underground</span>
            </div>
            <div class=""page-content"">
                <table class=""london-underground"">
                    <tbody>
                        <tr><th class=""northern"">Northern</th><th class=""piccadilly"">Piccadilly</th><th class=""victoria"">Victoria</th></tr>
                        <tr>
                            {7}
                            {8}
                            {9}
                        </tr>
                    </tbody>
                </table>
            </div>
            <div class=""page-footer relative-footer"">Page 3 of 3</div>
        </div>
    </div>
</body>
</html>".Fmt(lineHtmls));
        }
    }
}