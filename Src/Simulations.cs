using System;
using System.Collections.Generic;
using System.Linq;
using RT.Util;
using RT.Util.Collections;
using RT.Util.Consoles;
using RT.Util.ExtensionMethods;
using RT.Util.Text;

namespace KtaneStuff
{
    static class Simulations
    {
        sealed class Simulatable
        {
            public bool Active;
            public string Name;
            public Func<Edgework, string[]> GetResult;
        }

        sealed class SimulatedResult
        {
            public int Count;
            public AutoDictionary<string, SimulatedResult> Children = new AutoDictionary<string, SimulatedResult>(str => new SimulatedResult());

            private int? _totalCountCache = null;
            public int TotalCount
            {
                get
                {
                    if (_totalCountCache == null)
                        _totalCountCache = Children.Values.Sum(c => c.TotalCount) + Count;
                    return _totalCountCache.Value;
                }
            }
        }

        public static void RunSimulations()
        {
            const int numIterations = 100000;

            var astrologyElements = new[] { "Fire", "Water", "Earth", "Air" };
            var astrologyPlanets = new[] { "Sun", "Jupiter", "Moon", "Saturn", "Mercury", "Uranus", "Venus", "Neptune", "Mars", "Pluto" };
            var astrologyZodiacs = new[] { "Aries", "Leo", "Sagittarius", "Taurus", "Virgo", "Capricorn", "Gemini", "Libra", "Aquarius", "Cancer", "Scorpio", "Pisces" };

            var yes = new[] { "Yes" };

            var blindMazeColorNames = "Red,Green,White,Gray,Yellow".Split(',');

            var simulatables = Ut.NewArray(
                new Simulatable
                {
                    Active = false,
                    Name = "Connection Check",
                    GetResult = ew =>
                    {
                        var numbers = Enumerable.Range(0, 8).Select(_ => Rnd.Next(1, 9)).ToArray();
                        return Ut.NewArray(
                            numbers.Distinct().Count() == 8 ? "1. all distinct" :
                            numbers.Count(c => c == 1) > 1 ? "2. more than one “1”" :
                            numbers.Count(c => c == 7) > 1 ? "3. more than one “7”" :
                            numbers.Count(c => c == 2) >= 3 ? "4. at least three “2”s" :
                            numbers.Count(c => c == 5) == 0 ? "5. no “5”s" :
                            numbers.Count(c => c == 8) == 2 ? "6. exactly two “8”s" :
                            ew.GetNumBatteries().Apply(bs => bs > 6 || bs == 0) ? "7. more than 6 or no batteries" :
                            "8. count the batteries");
                    }
                },
                new Simulatable
                {
                    Active = false,
                    Name = "Laundry: 4/2+BOB (“unicorn”)",
                    GetResult = ew => ew.GetNumAABatteries() == 4 && ew.GetNumDBatteries() == 0 && ew.HasLitIndicator("BOB") ? yes : null
                },
                new Simulatable
                {
                    Active = false,
                    Name = "Lettered Keys",
                    GetResult = ew =>
                    {
                        var number = Rnd.Next(0, 100);
                        return Ut.NewArray(
                            number == 69 ? "1. 69 → D" :
                            number % 6 == 0 ? "2. divisible by 6 → A" :
                            ew.GetNumBatteries() >= 2 && number % 3 == 0 ? "3. ≥ 2 batteries and divisible by 3 → B" :
                            ew.SerialNumber.Any("CE3".Contains) ? (number >= 22 && number <= 79 ? "4. C, E, 3 & 22 ≤ n ≤ 79 → B" : "5. C, E, 3 → C") :
                            number < 46 ? "6. n < 46 → D" : "7. otherwise → A");
                    }
                },
                new Simulatable
                {
                    Active = false,
                    Name = "Astrology",
                    GetResult = ew =>
                    {
                        var planet = Rnd.Next(astrologyPlanets.Length);
                        var zodiac = Rnd.Next(astrologyZodiacs.Length);
                        var element = Rnd.Next(astrologyElements.Length);

                        var elemPlanet = new[,] {
                            { 0,0,1,-1,0,1,-2,2,0,-1},
                            {-2,0,-1,0,2,0,-2,2,0,1},
                            {-1,-1,0,-1,1,2,0,2,1,-2},
                            {-1,2,-1,0,-2,-1,0,2,-2,2 }
                        };
                        var elemZodiac = new[,] {
                            {1,0,-1,0,0,2,2,0,1,0,1,0 },
                            {2,2,-1,2,-1,-1,-2,1,2,0,0,2 },
                            {-2,-1,0,0,1,0,1,2,-1,-2,1,1 },
                            { 1,1,-2,-2,2,0,-1,1,0,0,-1,-1 }
                        };
                        var planetZodiac = new[,] {
                            {-1,-1,2,0,-1,0,-1,1,0,0,-2,-2},
                            {-2,0,1,0,2,0,-1,1,2,0,1,0},
                            {-2,-2,-1,-1,1,-1,0,-2,0,0,-1,1},
                            {-2,2,-2,0,0,1,-1,0,2,-2,-1,1},
                            {-2,0,-1,-2,-2,-2,-1,1,1,1,0,-1},
                            {-1,-2,1,-1,0,0,0,1,0,-1,2,0},
                            {-1,-1,0,0,1,1,0,0,0,0,-1,-1},
                            {-1,2,0,0,1,-2,1,0,2,-1,1,0},
                            {1,0,2,1,-1,1,1,1,0,-2,2,0 },
                            { -1,0,0,-1,-2,1,2,1,1,0,0,-1 }
                        };

                        var omen = new[] { astrologyPlanets[planet], astrologyZodiacs[zodiac], astrologyElements[element] }.Sum(str => str.Any(ew.SerialNumber.Contains) ? 1 : -1);
                        omen += elemPlanet[element, planet] + elemZodiac[element, zodiac] + planetZodiac[planet, zodiac];
                        return omen == 0 ? new[] { "No Omen" } : omen > 0 ? new[] { "Good Omen", omen.ToString() } : new[] { "Poor Omen", (-omen).ToString() };
                    }
                },
                new Simulatable
                {
                    Active = false,
                    Name = "Plumbing",
                    GetResult = ew =>
                    {
                        // Plumbing: most inputs and outputs
                        var redInput =
                            // For: Serial contains a '1'
                            +(ew.SerialNumber.Contains('1') ? 1 : 0)
                            // For: Exactly 1 RJ45 port
                            + (ew.GetNumPorts(PortType.RJ45) == 1 ? 1 : 0)
                            // Against: Any duplicate ports
                            - (ew.HasDuplicatePorts() ? 1 : 0)
                            // Against: Any duplicate ew.SerialNumber characters
                            - (ew.SerialNumber.Order().SelectConsecutivePairs(false, (c1, c2) => c1 == c2).Any() ? 1 : 0)
                            > 0;

                        var yellowInput =
                            // For: Serial contains a '2'
                            +(ew.SerialNumber.Contains('2') ? 1 : 0)
                            // For: One or more Stereo RCA ports
                            + (ew.GetNumPorts(PortType.StereoRCA) > 0 ? 1 : 0)
                            // Against: No duplicate ports
                            - (ew.HasDuplicatePorts() ? 0 : 1)
                            // Against: Serial contains a '1' or 'L'
                            - (ew.SerialNumber.Contains('1') || ew.SerialNumber.Contains('L') ? 1 : 0)
                            > 0;

                        var greenInput =
                            // For: Serial contains 3 or more numbers
                            +(ew.SerialNumber.Count(ch => ch >= '0' && ch <= '9') >= 3 ? 1 : 0)
                            // For: One or more DVI-D ports
                            + (ew.GetNumPorts(PortType.DVI) > 0 ? 1 : 0)
                            // Against: Red Input is inactive
                            - (redInput ? 0 : 1)
                            // Against: Yellow Input is inactive
                            - (yellowInput ? 0 : 1)
                            > 0;

                        var blueInput =
                            // Note: Always active if all other inputs are inactive
                            (!redInput && !yellowInput && !greenInput) || (
                                // For: At least 4 unique ports
                                (ew.GetNumPortTypes() >= 4 ? 1 : 0)
                                // For: At least 4 batteries
                                + (ew.GetNumBatteries() >= 4 ? 1 : 0)
                                // Against: No ports
                                - (ew.GetPorts().Any() ? 0 : 1)
                                // Against: No batteries
                                - (ew.GetNumBatteries() == 0 ? 1 : 0)
                                > 0
                            );

                        var redOutput =
                            // For: One or more Serial ports
                            (ew.GetNumPorts(PortType.Serial) > 0 ? 1 : 0)
                            // For: Exactly one battery
                            + (ew.GetNumBatteries() == 1 ? 1 : 0)
                            // Against: Serial contains more than 2 numbers
                            - (ew.SerialNumber.Count(ch => ch >= '0' && ch <= '9') >= 3 ? 1 : 0)
                            // Against: More than 2 inputs are active
                            - ((redInput ? 1 : 0) + (blueInput ? 1 : 0) + (greenInput ? 1 : 0) + (yellowInput ? 1 : 0) > 2 ? 1 : 0)
                            > 0;

                        var yellowOutput =
                            // For: Any duplicate ports
                            (ew.HasDuplicatePorts() ? 1 : 0)
                            // For: Serial contains a '4' or '8'
                            + (ew.SerialNumber.Contains('4') || ew.SerialNumber.Contains('8') ? 1 : 0)
                            // Against: Serial doesn't contain a '2'
                            - (ew.SerialNumber.Contains('2') ? 0 : 1)
                            // Against: Green Input is active
                            - (greenInput ? 1 : 0)
                            > 0;

                        var greenOutput =
                            // For: Exactly 3 inputs are active
                            ((redInput ? 1 : 0) + (blueInput ? 1 : 0) + (greenInput ? 1 : 0) + (yellowInput ? 1 : 0) == 3 ? 1 : 0)
                            // For: Exactly 3 ports are present
                            + (ew.GetNumPorts() == 3 ? 1 : 0)
                            // Against: Less than 3 ports are present
                            - (ew.GetNumPorts() < 3 ? 1 : 0)
                            // Against: Serial contains more than 3 numbers
                            - (ew.SerialNumberDigits().Count() > 3 ? 1 : 0)
                            > 0;

                        var blueOutput =
                            // Note: Always active if all other outputs are inactive
                            (!redOutput && !greenOutput && !yellowOutput) ||
                            // For: All inputs are active
                            +((redInput && greenInput && yellowInput && blueInput) ? 1 : 0)
                            // For: Any other output is inactive
                            + (!redOutput || !yellowOutput || !greenOutput ? 1 : 0)
                            // Against: Less than 2 batteries
                            - (ew.GetNumBatteries() < 2 ? 1 : 0)
                            // Against: No Parallel port
                            - (ew.GetNumPorts(PortType.Parallel) == 0 ? 1 : 0)
                            > 0;

                        var numInputs = (redInput ? 1 : 0) + (blueInput ? 1 : 0) + (greenInput ? 1 : 0) + (yellowInput ? 1 : 0);
                        var numOutputs = (redOutput ? 1 : 0) + (blueOutput ? 1 : 0) + (greenOutput ? 1 : 0) + (yellowOutput ? 1 : 0);

                        return Ut.NewArray(
                            //$"{numInputs} inputs",
                            //$"{numOutputs} outputs"
                            new[] {
                                redInput ? "Red in" : null, yellowInput ? "Yellow in" : null , greenInput ? "Green in" : null, blueInput ? "Blue in" : null,
                                redOutput ? "Red out" : null, yellowOutput ? "Yellow out" : null , greenOutput ? "Green out" : null, blueOutput ? "Blue out" : null }
                            .Where(s => s != null).JoinString(", ")
                        );
                    }
                },
                new Simulatable
                {
                    Active = false,
                    Name = "Chess",
                    GetResult = ew => new[] { Chess.GetSolution(ew.SerialNumber.Last() % 2 != 0, out _, out _)[6] }
                },
                new Simulatable
                {
                    Active = true,
                    Name = "Blind Maze",
                    GetResult = ew =>
                    {
                        // Colors of the N/S/W/E buttons
                        var cols = new[] { Rnd.Next(0, 5), Rnd.Next(0, 5), Rnd.Next(0, 5), Rnd.Next(0, 5) };

                        // 0 = Red
                        // 1 = Green
                        // 2 = White
                        // 3 = Gray
                        // 4 = Yellow

                        // If there are at least two red buttons,
                        if (cols.Count(c => c == 0) >= 2)
                            // rotate the maze 90 degrees clockwise and then calculate starting position.
                            return new[] { "90", "after" };

                        // Otherwise, if there are at least 5 batteries,
                        if (ew.GetNumBatteries() >= 5)
                            // calculate starting position and then rotate the maze 90 degrees clockwise.
                            return new[] { "90", "before" };

                        // Otherwise, if there is an IND indicator,
                        if (ew.HasIndicator("IND"))
                            // rotate the maze 180 degrees and then calculate starting position.
                            return new[] { "180", "after" };

                        // Otherwise, if there are no yellow buttons and at least one red button,
                        if (cols.Count(c => c == 4) == 0 && cols.Count(c => c == 0) > 0)
                            // rotate your perspective of the maze 90 degrees clockwise and then calculate starting position.
                            return new[] { "270", "after" };

                        // Otherwise, if there are at least 2 types of maze-based modules on the bomb*,
                        if (Rnd.NextDouble() < .25)
                            // calculate starting position and then rotate the maze 180 degrees clockwise.
                            return new[] { "180", "before" };

                        // Otherwise, if there is at most 1 port type on the bomb,
                        if (ew.GetNumPortTypes() < 2)
                            // calculate starting position and then rotate your perspective of the maze 90 degrees clockwise.
                            return new[] { "270", "before" };

                        // Otherwise, keep the maze as it is.
                        return new[] { "0" };
                    }
                }
            ).Where(s => s.Active).ToArray();

            var results = new AutoDictionary<string, SimulatedResult>(str => new SimulatedResult());
            var hundredths = numIterations / 100;

            for (int attempt = 0; attempt < numIterations; attempt++)
            {
                if (attempt % hundredths == 0)
                    Console.Write($"{attempt / hundredths}%\r");

                var edgework = Edgework.Generate();

                foreach (var sim in simulatables)
                {
                    var result = sim.GetResult(edgework);
                    if (result == null)
                        continue;
                    var sr = results[sim.Name];
                    for (int i = 0; i < result.Length; i++)
                        sr = sr.Children[result[i]];
                    sr.Count++;
                }
            }
            Console.WriteLine("Done");
            Console.WriteLine();

            var tt = new TextTable { ColumnSpacing = 2 };
            var row = 0;
            foreach (var kvp in results)
            {
                tt.SetCell(0, row, kvp.Key.Color(ConsoleColor.White));
                tt.SetCell(1, row, "{0/Cyan:0.####}% (or 1 in {1/Magenta:0}){2}".Color(ConsoleColor.Gray).Fmt(
                    kvp.Value.TotalCount / (double) numIterations * 100,
                    numIterations / (double) kvp.Value.TotalCount,
                    generateTree(kvp.Value, numIterations, skipTop: true)));
                row++;
            }
            tt.WriteToConsole();
            Console.WriteLine();
        }

        private static ConsoleColoredString generateTree(SimulatedResult sr, int numIterations, bool skipTop = false)
        {
            var lst = new List<ConsoleColoredString>();
            if (!skipTop)
                lst.Add("{0/DarkCyan:0.####}% (or 1 in {1/DarkMagenta:0})".Color(ConsoleColor.DarkGray).Fmt(sr.TotalCount / (double) numIterations * 100, numIterations / (double) sr.TotalCount));
            foreach (var kvp in sr.Children.OrderBy(k => k.Key))
                lst.Add("\n  {0}: {1}".Color(ConsoleColor.DarkGray).Fmt(kvp.Key, generateTree(kvp.Value, numIterations).Replace("\n", "\n  ")));
            return new ConsoleColoredString(lst);
        }
    }
}
