using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using RT.TagSoup;
using RT.Util;
using RT.Util.Consoles;
using RT.Util.ExtensionMethods;
using RT.Util.Text;

namespace KtaneStuff
{
    public static class PointOfOrder
    {
        enum Suit { Spades, Hearts, Clubs, Diamonds }
        enum Rank { Ace, Two, Three, Four, Five, Six, Seven, Eight, Nine, Ten, Jack, Queen, King }
        struct PlayingCard : IEquatable<PlayingCard>
        {
            private int _card;
            public Suit Suit => (Suit) (_card % 4);
            public Rank Rank => (Rank) (_card / 4);
            public PlayingCard(int card) { _card = card; }
            public static PlayingCard GetRandom(Random rnd = null) => new PlayingCard(rnd == null ? Rnd.Next(13 * 4) : rnd.Next(13 * 4));
            public static PlayingCard[] AllCards = Enumerable.Range(0, 13 * 4).Select(i => new PlayingCard(i)).ToArray();
            public override string ToString() => "A23456789TJQK"[(int) Rank] + "" + "♠♥♣♦"[(int) Suit];
            public bool Equals(PlayingCard other) => _card == other._card;
            public override int GetHashCode() => _card;
            public override bool Equals(object obj) => obj is PlayingCard && ((PlayingCard) obj)._card == _card;
            public static bool operator ==(PlayingCard one, PlayingCard two) => one._card == two._card;
            public static bool operator !=(PlayingCard one, PlayingCard two) => one._card != two._card;
        }

        const int _numActiveRules = 2;
        const int numPlayedCards = 5;

        public static void CreateDataCs()
        {
            File.WriteAllText(@"D:\c\KTANE\PointOfOrder\Assets\Data.cs", $@"using System.Collections.Generic;

namespace PointOfOrder
{{
    static class RawPngs
    {{
        public static Dictionary<Suit, Dictionary<Rank, byte[]>> CardData = new Dictionary<Suit, Dictionary<Rank, byte[]>> {{
            {Enumerable.Range(0, 4).Select(i => (Suit) i).Select(suit => $@"{{ Suit.{suit}, new Dictionary<Rank, byte[]> {{
                {Enumerable.Range(0, 13).Select(i => (Rank) i).Select(rank => $@"{{ Rank.{rank}, new byte[] {{ {File.ReadAllBytes($@"D:\Daten\Upload\KTANE\Cards\Extracted\Cards\{rank} of {suit}.png").JoinString(", ")} }} }}")
                    .JoinString(",\r\n                ")} }} }}")
                .JoinString(",\r\n            ")}
        }};

        public static byte[][] BackData = new byte[][] {{
            {Enumerable.Range(0, 10).Select(n => $@"new byte[] {{ {File.ReadAllBytes($@"D:\Daten\Upload\KTANE\Cards\Extracted\Backs\back{n}.png").JoinString(", ")} }}").JoinString(",\r\n            ")}
        }};
    }}
}}");
        }

        public static void Test()
        {
            var ruleCombinationHistogram = new Dictionary<string, int>();
            var numCorrectCardsHistorgram = new Dictionary<int, Dictionary<string, int>>();
            var numWrongCardsHistorgram = new Dictionary<int, Dictionary<string, int>>();

            const int numIter = 2000;
            var rnd = new Random();
            var numRules = 0;
            for (int iter = 0; iter < numIter; iter++)
            {
                var rules = getRules(Edgework.Generate(5, 10, rnd));
                numRules = rules.Length;
                var puzzle = generatePuzzle(rules, rnd);

                var rulesIndexesStr = puzzle.ActiveRuleIndexes.Order().Select(r => r + 1).JoinString(",");
                ruleCombinationHistogram.IncSafe(rulesIndexesStr);
                numCorrectCardsHistorgram.IncSafe(puzzle.CorrectCards.Length, rulesIndexesStr);
                numWrongCardsHistorgram.IncSafe(puzzle.WrongCards.Length, rulesIndexesStr);
            }

            foreach (var kvp in ruleCombinationHistogram.OrderBy(k => k.Key))
                Console.WriteLine($"{kvp.Key} = {kvp.Value * 100.0 / numIter:0.0}%");
            Console.WriteLine($"Factor: {ruleCombinationHistogram.Max(k => k.Value) / (double) ruleCombinationHistogram.Min(k => k.Value):0.0}");
            Console.WriteLine();
            for (int i = 0; i < numRules; i++)
                Console.WriteLine($"Rule #{i + 1} = {ruleCombinationHistogram.Where(p => p.Key.Contains((i + 1).ToString())).Sum(p => p.Value) * 100.0 / numIter:0.0}%");
            Console.WriteLine();
            ConsoleUtil.WriteLine("Distribution of number of correct cards:".Color(ConsoleColor.White));
            var tt = new TextTable { ColumnSpacing = 2 };
            var rulesIndexesStrs = ruleCombinationHistogram.Keys.Order().ToArray();
            for (int col = 0; col < rulesIndexesStrs.Length; col++)
                tt.SetCell(col + 1, 0, rulesIndexesStrs[col].Color(ConsoleColor.White));
            for (int i = numCorrectCardsHistorgram.Keys.Max(); i >= 0; i--)
            {
                tt.SetCell(0, i + 1, $"{i} cards".Color(ConsoleColor.White), alignment: HorizontalTextAlignment.Right);
                for (int col = 0; col < rulesIndexesStrs.Length; col++)
                    tt.SetCell(col + 1, i + 1, numCorrectCardsHistorgram.Get(i, null)?.Get(rulesIndexesStrs[col]).NullOr(val => $"{val * 100.0 / numIter:0.0}%".Color(ConsoleColor.Cyan)) ?? "", alignment: HorizontalTextAlignment.Right);
                //Console.WriteLine($"{i} correct cards: {numCorrectCardsHistorgram.Get(i, 0) * 100.0 / numIter:0.0}%");
            }
            tt.WriteToConsole();
            //Console.WriteLine();
            //ConsoleUtil.WriteLine("Distribution of number of wrong cards:".Color(ConsoleColor.White));
            //for (int i = numWrongCardsHistorgram.Keys.Max(); i >= 0; i--)
            //    Console.WriteLine($"{i} wrong cards: {numWrongCardsHistorgram.Get(i, 0) * 100.0 / numIter:0.0}%");
        }

        sealed class Puzzle
        {
            public PlayingCard[] Pile;
            public PlayingCard[] CorrectCards;
            public PlayingCard[] WrongCards;
            public int[] ActiveRuleIndexes;
        }

        private static Puzzle generatePuzzle(Func<PlayingCard, List<PlayingCard>, bool>[] rules, Random rnd, int[] activeRulesIxs = null)
        {
            var specificRules = activeRulesIxs != null;

            retry:
            if (!specificRules)
                activeRulesIxs = Enumerable.Range(0, rules.Length).ToList().Shuffle(rnd).Take(_numActiveRules).ToArray();

            //Console.WriteLine("Active rules: " + activeRulesIxs.Select(r => r + 1).JoinString(", "));
            var activeRules = activeRulesIxs.Select(i => rules[i]).ToArray();
            var inactiveRulesIxs = Enumerable.Range(0, rules.Length).Except(activeRulesIxs).ToArray();
            var inactiveRules = inactiveRulesIxs.Select(i => rules[i]).ToArray();

            var pile = new List<PlayingCard> { PlayingCard.GetRandom(rnd) };
            PlayingCard[] correctCards = null;
            PlayingCard[] wrongCards = null;

            bool recurse()
            {
                // For the first 𝑛−1 cards, only make sure that they satisfy the two active rules.
                var permissibleCards = PlayingCard.AllCards.Where(c => !pile.Contains(c) && activeRules.All(rule => rule(c, pile)));

                if (pile.Count == numPlayedCards - 1)
                {
                    // For the 𝑛th card, also make sure that the pile as a whole doesn’t satisfy any inactive rule
                    permissibleCards = permissibleCards.Where(c =>
                    {
                        var newPile = new List<PlayingCard>();
                        var iar = new bool[inactiveRules.Length];
                        for (int i = 0; i < pile.Count; i++)
                        {
                            newPile.Add(pile[i]);
                            var nextCard = i == pile.Count - 1 ? c : pile[i + 1];
                            for (int j = 0; j < inactiveRules.Length; j++)
                                iar[j] = iar[j] || !inactiveRules[j](nextCard, newPile);
                        }
                        return !iar.Contains(false);
                    });
                }

                if (pile.Count == numPlayedCards)
                {
                    // Choose a “correct” card
                    correctCards = permissibleCards.ToArray();
                    if (correctCards.Length == 0)
                        return false;

                    wrongCards = PlayingCard.AllCards.Where(c => !pile.Contains(c) && !correctCards.Contains(c) && activeRules.Count(rule => rule(c, pile)) == _numActiveRules - 1).ToArray();
                    if (wrongCards.Length < 4)
                        return false;

                    return true;
                }
                else
                {
                    foreach (var pCard in permissibleCards.ToList().Shuffle(rnd))
                    {
                        pile.Add(pCard);
                        if (recurse())
                            return true;
                        pile.RemoveAt(pile.Count - 1);
                    }
                }
                return false;
            }

            if (!recurse() || correctCards.Length > 9)
                goto retry;

            return new Puzzle
            {
                Pile = pile.ToArray(),
                CorrectCards = correctCards,
                WrongCards = wrongCards,
                ActiveRuleIndexes = activeRulesIxs
            };
        }

        static Func<PlayingCard, List<PlayingCard>, bool>[] getRules(Edgework edgework)
        {
            var serial = edgework.SerialNumber;
            var serial1Letter = char.IsLetter(serial[0]);
            var serial2Letter = char.IsLetter(serial[1]);
            var allowedSuits = (serial1Letter
                    ? serial2Letter ? "01;12;23;30" : "03;10;21;32"
                    : serial2Letter ? "12;23;30;01" : "32;03;10;21").Split(';');
            ConsoleUtil.WriteLine("{0/White} Allowed suits: {1}".Color(null).Fmt("Rule 1:", allowedSuits.Select((s, i) => "♠♥♣♦"[i].Color(ConsoleColor.Cyan) + " → " + s.Select(ch => "♠♥♣♦"[ch - '0'].ToString().Color(ConsoleColor.Green)).JoinColoredString("/".Color(ConsoleColor.DarkGray))).JoinColoredString("; ".Color(ConsoleColor.DarkGray))));
            // OUTDATED Debug.LogFormat(serial1Letter
            // OUTDATED     ? serial2Letter
            // OUTDATED         ? "[Point of Order #{0}] Rule 2: No two consecutive cards of same suit."
            // OUTDATED         : "[Point of Order #{0}] Rule 2: Can’t have ♠ touch ♣ or ♥ touch ♦."
            // OUTDATED     : serial2Letter
            // OUTDATED         ? "[Point of Order #{0}] Rule 2: Can’t have ♠ touch ♥ or ♣ touch ♦."
            // OUTDATED         : "[Point of Order #{0}] Rule 2: Can’t have ♠ touch ♦ or ♣ touch ♥.", _moduleId);

            var divisibleBy = (serial[3] - 'A' + 1) % 3 + 3;
            ConsoleUtil.WriteLine("{0/White} Alternating divisibility by {1/Cyan}".Color(null).Fmt("Rule 2:", divisibleBy));
            // OUTDATED Debug.LogFormat("[Point of Order #{0}] Rule 3: Ranks must alternate between being divisible by {1} and not.", _moduleId, divisibleBy);

            var difference = (serial[4] - 'A' + 1) % 3 + 2;
            ConsoleUtil.WriteLine("{0/White} Rank difference of {1/Cyan}{2/DarkGray}{3/Cyan}".Color(null).Fmt("Rule 3:", difference, "–", difference + 1));
            // OBSOLETE Debug.LogFormat("[Point of Order #{0}] Rule 4: Consecutive ranks must have a difference between {1} and {2} (with wraparound allowed).", _moduleId, difference, difference + 3);

            // OBSOLETE var suitAssocNum = ((serial[2] - '0') % 2 == 0 ? 1 : 0) + ((serial[5] - '0') % 2 == 0 ? 2 : 0);
            // OBSOLETE var suitAssoc = new[] { "0123", "0213", "0123", "0321" }[suitAssocNum].Select(ch => (Suit) (ch - '0')).ToArray();
            // OBSOLETE var numAABatteries = edgework.GetNumAABatteries();
            // OBSOLETE var numDBatteries = edgework.GetNumDBatteries();
            // OBSOLETE var permissibleRankDifferences =
            // OBSOLETE     numAABatteries > numDBatteries ? new[] { 1, 3, 5, 7, 9, 11, 13 } :
            // OBSOLETE     numAABatteries < numDBatteries ? new[] { 0, 2, 4, 6, 8, 10, 12 } : new[] { 2, 3, 5, 7, 11 };

            // OBSOLETE Debug.LogFormat("[Point of Order #{0}] Rule 5: {1} OR {2} rank difference", _moduleId,
            // OBSOLETE     suitAssocNum == 0 ? "Same suit" : string.Format("Associated suit (♠↔{0}, {1}↔{2})", "♣♥♦"[suitAssocNum - 1], "♥♣♣"[suitAssocNum - 1], "♦♦♥"[suitAssocNum - 1]),
            // OBSOLETE     numAABatteries > numDBatteries ? "odd" : numAABatteries < numDBatteries ? "even" : "prime");

            return Ut.NewArray<Func<PlayingCard, List<PlayingCard>, bool>>(
                // OLD // Rule 1: Ranks must be 2×ascending+1×descending / 2×descending+1×ascending / alternate between descending and ascending
                // OLD (card, cards) =>
                // OLD {
                // OLD     if (card.Rank == cards.Last().Rank)
                // OLD         return false;
                // OLD     if (cards.Count < 2)
                // OLD         return true;

                // OLD     if (lit < unlit)    // 2×asc, 1×desc
                // OLD     {
                // OLD         if (cards[1].Rank < cards[0].Rank)
                // OLD             return (card.Rank > cards.Last().Rank) ^ (cards.Count % 3 == 1);
                // OLD         if (cards.Count < 3)
                // OLD             return true;
                // OLD         if (cards[2].Rank < cards[1].Rank)
                // OLD             return (card.Rank > cards.Last().Rank) ^ (cards.Count % 3 == 2);
                // OLD         return (card.Rank > cards.Last().Rank) ^ (cards.Count % 3 == 0);
                // OLD     }

                // OLD     if (lit > unlit)     // 2×desc, 1×asc
                // OLD     {
                // OLD         if (cards[1].Rank > cards[0].Rank)
                // OLD             return (card.Rank < cards.Last().Rank) ^ (cards.Count % 3 == 1);
                // OLD         if (cards.Count < 3)
                // OLD             return true;
                // OLD         if (cards[2].Rank > cards[1].Rank)
                // OLD             return (card.Rank < cards.Last().Rank) ^ (cards.Count % 3 == 2);
                // OLD         return (card.Rank < cards.Last().Rank) ^ (cards.Count % 3 == 0);
                // OLD     }

                // OLD     // alternate between desc and asc
                // OLD     return card.Rank != cards.Last().Rank && ((card.Rank > cards.Last().Rank) ^ (cards[1].Rank > cards[0].Rank) ^ (cards.Count % 2 != 0));
                // OLD },

                // OLD // Rule 2: No two consecutive cards of associated suits
                // OLD (card, cards) => serial1Letter
                // OLD     ? serial2Letter
                // OLD         ? card.Suit != cards.Last().Suit
                // OLD         : card.Suit == Suit.Spades ? cards.Last().Suit != Suit.Clubs : card.Suit == Suit.Clubs ? cards.Last().Suit != Suit.Spades : card.Suit == Suit.Diamonds ? cards.Last().Suit != Suit.Hearts : cards.Last().Suit != Suit.Diamonds
                // OLD     : serial2Letter
                // OLD         ? card.Suit == Suit.Spades ? cards.Last().Suit != Suit.Hearts : card.Suit == Suit.Hearts ? cards.Last().Suit != Suit.Spades : card.Suit == Suit.Diamonds ? cards.Last().Suit != Suit.Clubs : cards.Last().Suit != Suit.Diamonds
                // OLD         : card.Suit == Suit.Spades ? cards.Last().Suit != Suit.Diamonds : card.Suit == Suit.Diamonds ? cards.Last().Suit != Suit.Spades : card.Suit == Suit.Hearts ? cards.Last().Suit != Suit.Clubs : cards.Last().Suit != Suit.Hearts,

                // NEW Rule 1: Consecutive cards of associated suits
                (card, cards) => allowedSuits[(int) cards.Last().Suit].Contains((char) ('0' + (int) card.Suit)),

                // NEW Rule 2: Ranks must alternate between being divisible by 𝑛 and not.
                (card, cards) => (((int) card.Rank + 1) % divisibleBy == 0) ^ (((int) cards.Last().Rank + 1) % divisibleBy == 0),

                // NEW Rule 3: Consecutive ranks must have a difference of 𝑛 .. (𝑛+1) (with wraparound allowed).
                (card, cards) =>
                {
                    var thisRank = (int) card.Rank;
                    var lastRank = (int) cards.Last().Rank;
                    for (int i = 0; i < 2; i++)
                        if (thisRank == (lastRank + difference + i) % 13 || thisRank == ((lastRank - difference - i) % 13 + 13) % 13)
                            return true;
                    return false;
                }

                // OLD // Rule 5: Consecutive cards must have associated suits or ranks
                // OLD (card, cards) =>
                // OLD {
                // OLD     if (suitAssocNum == 0 && card.Suit == cards.Last().Suit)
                // OLD         return true;
                // OLD     else if (suitAssocNum > 0 && card.Suit == suitAssoc[1 ^ Array.IndexOf(suitAssoc, cards.Last().Suit)])
                // OLD         return true;
                // OLD     return permissibleRankDifferences.Contains(Math.Abs((int) card.Rank - (int) cards.Last().Rank));
                // OLD }
            );
        }

        public static void CreateRaffleHtml()
        {
            var rnd = new Random(10);

            var conditions = Ut.NewArray<Func<Edgework, bool>>(
                //ew => ew.GetNumLitIndicators() < ew.GetNumUnlitIndicators(),
                //ew => ew.GetNumLitIndicators() > ew.GetNumUnlitIndicators(),
                //ew => ew.GetNumLitIndicators() == ew.GetNumUnlitIndicators(),
                ew => char.IsLetter(ew.SerialNumber[0]) && char.IsLetter(ew.SerialNumber[1]),
                ew => !char.IsLetter(ew.SerialNumber[0]) && char.IsLetter(ew.SerialNumber[1]),
                ew => char.IsLetter(ew.SerialNumber[0]) && !char.IsLetter(ew.SerialNumber[1]),
                ew => !char.IsLetter(ew.SerialNumber[0]) && !char.IsLetter(ew.SerialNumber[1]),
                ew => (ew.SerialNumber[3] - 'A') % 3 == 0,
                ew => (ew.SerialNumber[3] - 'A') % 3 == 1,
                ew => (ew.SerialNumber[3] - 'A') % 3 == 2,
                ew => (ew.SerialNumber[4] - 'A') % 3 == 0,
                ew => (ew.SerialNumber[4] - 'A') % 3 == 1,
                ew => (ew.SerialNumber[4] - 'A') % 3 == 2
            );

            File.WriteAllText(@"D:\c\KTANE\Public\HTML\Point of Order.html", new HTML { class_ = "no-js" }._(
                new HEAD(
                    new TITLE("Point of Order"),
                    new META { httpEquiv = "Content-Type", content = "text/html; charset=UTF-8" },
                    new META { httpEquiv = "X-UA-Compatible", content = "IE=edge" },
                    new META { name = "viewport", content = "initial-scale=1" },
                    new SCRIPT { src = "js/highlighter.js" },
                    new LINK { rel = "stylesheet", type = "text/css", href = "css/font.css" },
                    new STYLELiteral($@"
                        @font-face {{
                            font-family: 'Anonymous Pro';
                            font-style: normal;
                            font-weight: normal;
                            src: local('Anonymous Pro'), url(font/AnonymousPro-Regular.ttf);
                        }}
                        @font-face {{
                            font-family: 'Anonymous Pro';
                            font-style: normal;
                            font-weight: bold;
                            src: local('Anonymous Pro Bold'), local('Anonymous Pro'), url(font/AnonymousPro-Bold.ttf);
                        }}
                        @font-face {{
                            font-family: 'Anonymous Pro';
                            font-style: italic;
                            font-weight: normal;
                            src: local('Anonymous Pro Italic'), local('Anonymous Pro'), url(font/AnonymousPro-Italic.ttf);
                        }}
                        @font-face {{
                            font-family: 'Anonymous Pro';
                            font-style: italic;
                            font-weight: bold;
                            src: local('Anonymous Pro Bold Italic'), local('Anonymous Pro'), url(font/AnonymousPro-Bold.ttf);
                        }}

                        @font-face {{
                            font-family: 'Californian FB';
                            font-style: normal;
                            font-weight: normal;
                            src: local('Californian FB'), url(font/CALIFR.TTF);
                        }}
                        @font-face {{
                            font-family: 'Californian FB';
                            font-style: normal;
                            font-weight: bold;
                            src: local('Californian FB Bold'), local('Californian FB'), url(font/CALIFB.TTF);
                        }}
                        @font-face {{
                            font-family: 'Californian FB';
                            font-style: italic;
                            font-weight: normal;
                            src: local('Californian FB Italic'), local('Californian FB'), url(font/CALIFI.TTF);
                        }}

                        @font-face {{
                            font-family: 'Ostrich';
                            src: local('Ostrich Sans'), local('Ostrich'), url(font/OstrichSans-Heavy_90.otf);
                        }}

                        * {{ box-sizing: border-box; }}

                        body {{
                            margin: 0;
                            text-align: center;
                            font-family: 'Trebuchet MS';
                            font-size: 15pt;
                        }}
                        .text, .full-phrase {{
                            font-family: 'Californian FB';
                            max-width: 40em;
                            position: relative;
                            text-align: left;
                            line-height: 1.4;
                            margin: auto;
                        }}
                            .text {{
                                margin: 2em auto;
                                padding: 0 1em;
                            }}
                            .text::before {{
                                content: '';
                                position: absolute;
                                left: -275px;
                                top: 10px;
                                width: 216px;
                                height: 300px;
                                background: url(img/Point%20of%20Order/Nine%20of%20Diamonds.png) 50% 50% no-repeat;
                                background-size: contain;
                                opacity: .5;
                                transform: rotate(-5deg);
                            }}
                        em {{
                            font-style: normal;
                            text-decoration: underline;
                        }}
                        code {{
                            font-size: 80%;
                        }}
                        p.eye-catching {{
                            font-size: 115%;
                            font-weight: bold;
                        }}
                        h2 {{
                            text-align: center;
                        }}

                        .author {{
                            font-weight: normal;
                            font-size: smaller;
                        }}
                            .author::before {{ content: '('; }}
                            .author::after {{ content: ')'; }}

                        /* Edgework */

                        .edgework {{
                            clear: both;
                            padding: 0 20px;
                            overflow: auto;
                            background: url(img/Point%20of%20Order/EdgeworkBackground.png) top left repeat;
                            background-size: 80px;
                            margin: 10px 0;
                            text-align: center;
                        }}
                            .edgework > .widget {{
                                display: inline-block;
                                vertical-align: bottom;
                                height: 50px;
                                background-size: cover;
                                background-repeat: no-repeat;
                                background-position: center center;
                                position: relative;
                                margin: 12px 15px 8px 0;
                            }}
                                .edgework > .widget.separator {{
                                    background-image: url(img/Point%20of%20Order/EdgeworkSeparator.png);
                                    width: 9px;
                                }}
                                .edgework > .widget.serial {{
                                    background-image: url(img/Point%20of%20Order/Serial%20number.png);
                                    width: 99px;
                                    padding: 23px 0 0 11px;
                                    font-family: 'Anonymous Pro';
                                    font-weight: bold;
                                    font-size: 24px;
                                    text-align: left;
                                }}
                                .edgework > .widget.indicator {{
                                    width: 115px;
                                }}
                                    .edgework > .widget.indicator > .label {{
                                        color: white;
                                        font-family: 'Ostrich';
                                        font-size: 30px;
                                        letter-spacing: .05em;
                                        position: absolute;
                                        left: 76px;
                                        top: 23px;
                                        transform: translate(-50%, -50%);
                                    }}
                                    .edgework > .widget.indicator.lit {{
                                        background-image: url(img/Point%20of%20Order/LitIndicator.png);
                                    }}
                                    .edgework > .widget.indicator.unlit {{
                                        background-image: url(img/Point%20of%20Order/UnlitIndicator.png);
                                    }}
                                .edgework > .widget.battery {{
                                }}
                                    .edgework > .widget.battery.aa {{
                                        width: 89px;
                                        background-image: url(img/Point%20of%20Order/BatteryAA.png);
                                    }}
                                    .edgework > .widget.battery.d {{
                                        width: 85px;
                                        background-image: url(img/Point%20of%20Order/BatteryD.png);
                                    }}
                                .edgework > .widget.portplate {{
                                    width: 112px;
                                    background-image: url(img/Point%20of%20Order/PortPlate.png);
                                }}
                                    .edgework > .widget.portplate > span {{
                                        position: absolute;
                                        background-size: cover;
                                        background-repeat: no-repeat;
                                        background-position: center center;
                                    }}
                                        .edgework > .widget.portplate > span.stereorca {{
                                            left: 91px;
                                            top: 13px;
                                            width: 15px;
                                            height: 30px;
                                            background-image: url(img/Point%20of%20Order/PortRCA.png);
                                        }}
                                        .edgework > .widget.portplate > span.dvi {{
                                            left: 6px;
                                            top: 23px;
                                            width: 71px;
                                            height: 23px;
                                            background-image: url(img/Point%20of%20Order/PortDVI.png);
                                        }}
                                        .edgework > .widget.portplate > span.rj45 {{
                                            left: 3px;
                                            top: 3px;
                                            width: 21px;
                                            height: 21px;
                                            background-image: url(img/Point%20of%20Order/PortRJ.png);
                                        }}
                                        .edgework > .widget.portplate > span.ps2 {{
                                            left: 67px;
                                            top: 5px;
                                            width: 21px;
                                            height: 21px;
                                            background-image: url(img/Point%20of%20Order/PortPS2.png);
                                        }}
                                        .edgework > .widget.portplate > span.parallel {{
                                            left: 6px;
                                            top: 4px;
                                            width: 98px;
                                            height: 20px;
                                            background-image: url(img/Point%20of%20Order/PortParallel.png);
                                        }}
                                        .edgework > .widget.portplate > span.serial {{
                                            left: 30px;
                                            top: 24px;
                                            width: 52px;
                                            height: 22px;
                                            background-image: url(img/Point%20of%20Order/PortSerial.png);
                                        }}

                        .example {{
                            display: inline-block;
                            width: 500px;
                            height: 125px;
                            background: #eee;
                            margin-left: 1em;
                            position: relative;
                        }}
                            .example .card {{
                                box-sizing: content-box;
                                position: absolute;
                                width: 72px;
                                height: 100px;
                                background-size: contain;
                                background-position: 50% 50%;
                                background-repeat: no-repeat;
                            }}
                            .example .card.correct {{
                                border: 2px solid #0c2;
                            }}
                            .example .card.wrong {{
                                border: 2px solid #c42;
                            }}
                            {Enumerable.Range(0, 4).SelectMany(suit => Enumerable.Range(0, 13).Select(rank => $".example .card.{(Rank) rank}.{(Suit) suit} {{ background-image: url(img/Point%20of%20Order/{(Rank) rank}%20of%20{(Suit) suit}.png); }}")).JoinString()}
                            {Enumerable.Range(0, 5).Select(ix => $".example .pile .card:nth-child({ix + 1}) {{ left: {20 + 15 * ix}px; top: {5 + 2 * ix}px; transform: rotate({-20 + 10 * ix}deg); }}").JoinString()}
                            {Enumerable.Range(0, 5).Select(ix => $".example .choices .card:nth-child({ix + 1}) {{ left: {180 + 75 * ix}px; top: 10px; }}").JoinString()}
                            {Enumerable.Range(0, 5).Select(ix => $".example .choices .card.correct:nth-child({ix + 1}), .example .choices .card.wrong:nth-child({ix + 1}) {{ left: {178 + 75 * ix}px; top: 8px; }}").JoinString()}
                    ")),

                new BODY(
                    new Func<object>(() =>
                    {
                        // Find 4 sets of edgework which between them cover all conditions
                        // for 4 edgeworks, OLD rules: seed=4049
                        var seed = 20;
                        retry:
                        seed++;
                        var conditionsSatisfied = new bool[conditions.Length];
                        var rnd2 = new Random(seed);
                        var edgeworks = Ut.NewArray(4, _ => Edgework.Generate(5, 10, rnd2));
                        for (int i = 0; i < edgeworks.Length; i++)
                            for (int j = 0; j < conditions.Length; j++)
                                conditionsSatisfied[j] = conditionsSatisfied[j] || conditions[j](edgeworks[i]);
                        if (conditionsSatisfied.Any(c => c == false))
                            goto retry;

                        var fullResults = new List<object>();
                        var counterExampleIndex = new List<object>();
                        // Generate examples (one for each rule combination) for every set of edgework
                        foreach (var ew in edgeworks)
                        {
                            Console.WriteLine();
                            Console.WriteLine("Edgework: " + ew);

                            var rules = getRules(ew);

                            // EDGEWORK HTML
                            fullResults.Add(edgeworkHtml(ew));

                            // EXAMPLES
                            foreach (var ss in Enumerable.Range(0, rules.Length).Subsequences())
                            {
                                var activeRuleIxs = ss.ToArray();
                                if (activeRuleIxs.Length != _numActiveRules)
                                    continue;

                                Console.WriteLine("Active rules: " + activeRuleIxs.Select(ix => ix + 1).JoinString(", "));
                                for (int i = 0; i < 3; i++)
                                {
                                    var puzzle = generatePuzzle(rules, rnd, activeRuleIxs);
                                    var choices = puzzle.CorrectCards.Shuffle(rnd).Take(1).Concat(puzzle.WrongCards.Shuffle(rnd).Take(3)).ToArray().Shuffle(rnd);
                                    fullResults.Add(exampleHtml(puzzle.Pile, choices, choices.IndexOf(puzzle.CorrectCards.Contains)));
                                }
                            }
                        }

                        Console.WriteLine();
                        fullResults.Add(new H1("Counter-examples"));

                        // Generate counter-examples
                        foreach (var example in Ut.NewArray(
                            new
                            {
                                Phrase = "Make a Full House from Two Pair",
                                FullPhrase = (string) null,
                                Author = "luc537#4890",
                                Id = "full-house-from-two-pair",
                                GetCounterExample = new Func<Puzzle, Edgework, Tuple<PlayingCard, PlayingCard>>((puzzle, edgework) =>
                                {
                                    var pair1 = puzzle.Pile.UniquePairs().FirstOrDefault(p => p.Item1.Rank == p.Item2.Rank);
                                    if (pair1 == null)
                                        return null;
                                    var pair2 = puzzle.Pile.UniquePairs().FirstOrDefault(p => p.Item1.Rank == p.Item2.Rank && p.Item1.Rank != pair1.Item1.Rank);
                                    if (pair2 == null)
                                        return null;
                                    return puzzle.WrongCards.Where(card => card.Rank == pair1.Item1.Rank || card.Rank == pair2.Item1.Rank).FirstOrNull().NullOr(cc => Tuple.Create(cc, puzzle.CorrectCards[0]));
                                })
                            },
                            new
                            {
                                Phrase = "Groups of ranks",
                                FullPhrase = "Consider the following groups of ranks: 5/10; 3/6/9; 4/8/Q. If the 2nd and 4th cards in the pile are from a single set and no other cards in the pile are from that set, then all ranks within the set are valid to play.",
                                Author = "Storm Vision#6438",
                                Id = "rank-sets",
                                GetCounterExample = new Func<Puzzle, Edgework, Tuple<PlayingCard, PlayingCard>>((puzzle, edgework) =>
                                {
                                    var sets = new[] { new[] { Rank.Five, Rank.Ten }, new[] { Rank.Three, Rank.Six, Rank.Nine }, new[] { Rank.Four, Rank.Eight, Rank.Queen } };
                                    var applicableSetIx = sets.IndexOf(s => !s.Contains(puzzle.Pile[0].Rank) && s.Contains(puzzle.Pile[1].Rank) && !s.Contains(puzzle.Pile[2].Rank) && s.Contains(puzzle.Pile[3].Rank) && !s.Contains(puzzle.Pile[4].Rank));
                                    if (applicableSetIx == -1)
                                        return null;
                                    return puzzle.WrongCards.Where(card => sets[applicableSetIx].Contains(card.Rank)).FirstOrNull().NullOr(cc => Tuple.Create(cc, puzzle.CorrectCards[0]));
                                })
                            }
                        ))
                        {
                            ConsoleUtil.WriteLine("Generating counter-example for: " + example.Phrase.Color(ConsoleColor.Magenta));

                            // Keep trying to find a counter-example
                            while (true)
                            {
                                var edgework = Edgework.Generate(5, 7, rnd);
                                if (edgework.Widgets.Any(w => w.Type == WidgetType.Indicator && !Indicator.WellKnown.Contains(w.Indicator.Value.Label)))
                                    continue;
                                var rules = getRules(edgework);
                                var puzzle = generatePuzzle(rules, rnd);
                                var counterExample = example.GetCounterExample(puzzle, edgework);
                                if (counterExample == null)
                                    continue;
                                // Counter-example found!
                                var counterCard = counterExample.Item1;
                                var correctCard = counterExample.Item2;

                                var choices = puzzle.WrongCards.Shuffle(rnd).Where(wc => wc != counterCard).Take(2).Concat(correctCard).Concat(counterCard).ToArray().Shuffle(rnd);
                                fullResults.Add(new DIV { class_ = "counter-example", id = example.Id }._(
                                    new H2(new SPAN { class_ = "rule" }._(example.Phrase), " ", new SPAN { class_ = "author" }._(example.Author)),
                                    example.FullPhrase == null ? null : new P { class_ = "full-phrase" }._(example.FullPhrase),
                                    edgeworkHtml(edgework),
                                    exampleHtml(puzzle.Pile, choices, choices.IndexOf(correctCard), choices.IndexOf(counterCard))));
                                counterExampleIndex.Add(new LI(new A { href = "#" + example.Id }._(new SPAN { class_ = "rule" }._(example.Phrase), " ", new SPAN { class_ = "author" }._(example.Author))));
                                goto endOfCounterExample;
                            }
                            endOfCounterExample:;
                        }

                        return Ut.NewArray<object>(
                            new DIV { class_ = "text" }._(
                                new H1("Point of Order!"),
                                new P("Welcome to the exciting world of figuring stuff out."),
                                new P(new EM("Point of Order"), @" is a new modded module for “Keep Talking and Nobody Explodes”.
                                    This module is an homage to a card game in which the fundamental premise is that the
                                    rules of the game are not explained; players merely receive hints as they play and must figure out
                                    the rules on their own through logical deduction and trial and error."),
                                new P("In keeping with the tradition of said game, the manual for ", new EM("Point of Order"), @"
                                    is withheld until the community has deduced the rules correctly."),
                                new P { class_ = "eye-catching" }._(@"The goal is to collaboratively identify all of the rules and
                                    form a full manual. Your prestige and recognition will be proportional to how many of the
                                    rules you figured out."),
                                new P(@"Below, we present several hints on which cards are legal
                                    to play. Each example shows a pile of five cards on the module, a choice of four cards to play, and
                                    a green border indicating which would be the only correct selection (out of those four). In addition,
                                    you are free to ", new A { href = "http://steamcommunity.com/sharedfiles/filedetails/?id=955137794" }._("subscribe to the mod"), @" and experiment with
                                    it on your own."),
                                new P(@"Every participant may take a “guess” by writing a manual (or fragment of a manual)
                                    and submitting it to me (", new CODE("Timwi#0551"), @" on Discord). If any part of the guess is correct, that part
                                    of the manual is published, allowing subsequent guesses to use it as a scaffold. For any part that is
                                    wrong, I will point out which example on this page contradicts it. If no contradiction is already
                                    on the page, I will add one, thus providing everyone with more hints. How exactly your proposed
                                    rules are split into “parts” is up to my own discretion."),
                                new P(@"Guesses may be made an unlimited number of times, but you cannot submit a new
                                    guess until your previous guess has been addressed. Manuals may be written in any format
                                    that I can read, including PDF, HTML, Google Docs or Microsoft Word. (Scans of
                                    handwritten pages might be a stretch, but if your handwriting is beautiful then go for it.)"),
                                new H2(new A { href = "Point of Order incomplete manual.html" }._("Incomplete manual so far")),
                                new H4("Known information:"),
                                new UL(
                                    new LI("The color or pattern on the back of the cards does not matter."),
                                    new LI("The number of strikes on the bomb does not matter."),
                                    new LI("The other modules on the bomb do not matter, nor how many of them are solved or unsolved."),
                                    new LI("Given any specific module (set of 5 exposed cards) with the same edgework, there are multiple valid cards; however, only one will show as an option in any particular set of 4 possible answers. (Credit: OceanWaves)"),
                                    new LI("You can not play the same value card that was just played. (Credit: onewingedangel30)"),
                                    new LI("Given any particular module, there is a subset of ranks and suits, such that every combination of rank/suit of those subsets is a valid answer. (Credit: OceanWaves)"),
                                    new LI("The first, second, fourth and fifth characters of the serial number matter. No other edgework matters. (Credit: samfun123)"),
                                    new LI("The cards in the pile are a sequence, and the next card to be played is determined by the previous cards. (Credit: onewingedangel30)"),
                                    new LI("The previous cards played all follow a specific set of rules which the defuser/expert must follow to play the next card. (Credit: OceanWaves)")),
                                new H4("Counter-examples:"),
                                new UL(counterExampleIndex)),
                            fullResults);
                    })
                )
            ).ToString());
        }

        private static object exampleHtml(PlayingCard[] pile, PlayingCard[] choices, int correctIndex, int? counterExampleIndex = null)
        {
            return new DIV { class_ = "example" }._(
                new DIV { class_ = "pile" }._(pile.Select(card => new DIV { class_ = $"card {card.Rank} {card.Suit}" })),
                new DIV { class_ = "choices" }._(Enumerable.Range(0, choices.Length).Select(ix => new DIV { class_ = $"card {choices[ix].Rank} {choices[ix].Suit} {(correctIndex == ix ? "correct" : counterExampleIndex == ix ? "wrong" : "")}" })));
        }

        private static object edgeworkHtml(Edgework edgework)
        {
            return new DIV { class_ = "edgework" }._(
                new DIV { class_ = "widget serial" }._(edgework.SerialNumber),
                edgework.Widgets.Any(w => w.Type == WidgetType.BatteryHolder) ? new DIV { class_ = "widget separator" } : null,
                edgework.Widgets.Where(w => w.Type == WidgetType.BatteryHolder).Select(w => new DIV { class_ = $"widget battery {(w.BatteryType == BatteryType.BatteryAA ? "aa" : "d")}" }),
                edgework.Widgets.Any(w => w.Type == WidgetType.Indicator) ? new DIV { class_ = "widget separator" } : null,
                edgework.Widgets.Where(w => w.Type == WidgetType.Indicator).Select(w => new DIV { class_ = $"widget indicator {(w.Indicator.Value.Type == IndicatorType.Lit ? "lit" : "unlit")}" }._(new SPAN { class_ = "label" }._(w.Indicator.Value.Label))),
                edgework.Widgets.Any(w => w.Type == WidgetType.PortPlate) ? new DIV { class_ = "widget separator" } : null,
                edgework.Widgets.Where(w => w.Type == WidgetType.PortPlate).Select(w => new DIV { class_ = $"widget portplate" }._(w.PortTypes.Select(p => new SPAN { class_ = p.ToString().ToLowerInvariant() }))));
        }
    }
}