using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using RT.Util;
using RT.Util.Collections;
using RT.Util.Consoles;
using RT.Util.ExtensionMethods;
using RT.Util.Text;

using static KtaneStuff.Edgework;

namespace KtaneStuff
{
    static partial class Ktane
    {
        sealed class Simulatable
        {
            public bool Active;
            public string Name;
            public Func<Widget[], string, string[]> GetResult;
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

        public static void Simulations()
        {
            const int numIterations = 1000000;

            var astrologyElements = new[] { "Fire", "Water", "Earth", "Air" };
            var astrologyPlanets = new[] { "Sun", "Jupiter", "Moon", "Saturn", "Mercury", "Uranus", "Venus", "Neptune", "Mars", "Pluto" };
            var astrologyZodiacs = new[] { "Aries", "Leo", "Sagittarius", "Taurus", "Virgo", "Capricorn", "Gemini", "Libra", "Aquarius", "Cancer", "Scorpio", "Pisces" };

            var yes = new[] { "Yes" };
            var batteryCount = Ut.Lambda((Widget[] ws) => ws.Sum(w => w.BatteryType == null ? 0 : w.BatteryType == BatteryType.BatteryAA ? 2 : 1));
            var batteryHolderCount = Ut.Lambda((Widget[] ws) => ws.Count(w => w.BatteryType != null));
            var portTypeCount = Ut.Lambda((Widget[] ws) => ws.Where(w => w.Type == WidgetType.PortPlate).SelectMany(w => w.PortTypes).Distinct().Count());

            var simulatables = Ut.NewArray(
                new Simulatable
                {
                    Active = false,
                    Name = "Connection Check",
                    GetResult = (ws, serial) =>
                    {
                        var numbers = Enumerable.Range(0, 8).Select(_ => Rnd.Next(1, 9)).ToArray();
                        return Ut.NewArray(
                            numbers.Distinct().Count() == 8 ? "1. all distinct" :
                            numbers.Count(c => c == 1) > 1 ? "2. more than one “1”" :
                            numbers.Count(c => c == 7) > 1 ? "3. more than one “7”" :
                            numbers.Count(c => c == 2) >= 3 ? "4. at least three “2”s" :
                            numbers.Count(c => c == 5) == 0 ? "5. no “5”s" :
                            numbers.Count(c => c == 8) == 2 ? "6. exactly two “8”s" :
                            batteryCount(ws).Apply(bs => bs > 6 || bs == 0) ? "7. more than 6 or no batteries" :
                            "8. count the batteries");
                    }
                },
                new Simulatable
                {
                    Active = false,
                    Name = "Laundry: 4/2+BOB (“unicorn”)",
                    GetResult = (ws, serial) => ws.Where(w => w.BatteryType == BatteryType.BatteryAA).Count() == 2 && ws.Any(w => w.Type == WidgetType.Indicator && w.Indicator.Value.Label == "BOB" && w.Indicator.Value.Type == IndicatorType.Lit) ? yes : null
                },
                new Simulatable
                {
                    Active = false,
                    Name = "Lettered Keys",
                    GetResult = (ws, serial) =>
                    {
                        var number = Rnd.Next(0, 100);
                        return Ut.NewArray(
                            number == 69 ? "1. 69 → D" :
                            number % 6 == 0 ? "2. divisible by 6 → A" :
                            batteryCount(ws) >= 2 && number % 3 == 0 ? "3. ≥ 2 batteries and divisible by 3 → B" :
                            serial.Any("CE3".Contains) ? (number >= 22 && number <= 79 ? "4. C, E, 3 & 22 ≤ n ≤ 79 → B" : "5. C, E, 3 → C") :
                            number < 46 ? "6. n < 46 → D" : "7. otherwise → A");
                    }
                },
                new Simulatable
                {
                    Active = false,
                    Name = "Orientation Cube",
                    GetResult = (ws, serial) =>
                    {
                        return Ut.NewArray(
                            serial.Contains('R') ? "1. serial contains R" :
                            ws.Any(w => w.Type == WidgetType.Indicator && (w.Indicator.Value.Label == "CAR" || (w.Indicator.Value.Type == IndicatorType.Lit && w.Indicator.Value.Label == "TRN"))) ? "2. lit TRN or lit/unlit CAR" :
                            ws.Any(w => w.Type == WidgetType.PortPlate && w.PortTypes.Contains(PortType.PS2)) ? "3. PS/2 port" :
                            serial.Contains('7') || serial.Contains('8') ? "4. serial contains 7 or 8" :
                            batteryCount(ws) > 2 || Rnd.Next(0, 4) == 0 ? "5. ≥ 2 batteries or observer on left" : "6. otherwise");
                    }
                },
                new Simulatable
                {
                    Active = false,
                    Name = "Blind Alley",
                    GetResult = (ws, serial) =>
                    {
                        var counts = new int[8];

                        if (ws.Any(w => w.Type == WidgetType.Indicator && w.Indicator.Value.Type == IndicatorType.Unlit && w.Indicator.Value.Label == "BOB")) counts[0]++;
                        if (ws.Any(w => w.Type == WidgetType.Indicator && w.Indicator.Value.Type == IndicatorType.Lit && w.Indicator.Value.Label == "CAR")) counts[0]++;
                        if (ws.Any(w => w.Type == WidgetType.Indicator && w.Indicator.Value.Type == IndicatorType.Lit && w.Indicator.Value.Label == "IND")) counts[0]++;
                        if (ws.Count(w => w.Type == WidgetType.BatteryHolder) % 2 == 0) counts[0]++;

                        if (ws.Any(w => w.Type == WidgetType.Indicator && w.Indicator.Value.Type == IndicatorType.Unlit && w.Indicator.Value.Label == "CAR")) counts[1]++;
                        if (ws.Any(w => w.Type == WidgetType.Indicator && w.Indicator.Value.Type == IndicatorType.Unlit && w.Indicator.Value.Label == "NSA")) counts[1]++;
                        if (ws.Any(w => w.Type == WidgetType.Indicator && w.Indicator.Value.Type == IndicatorType.Lit && w.Indicator.Value.Label == "FRK")) counts[1]++;
                        if (ws.Any(w => w.Type == WidgetType.PortPlate && w.PortTypes.Contains(PortType.RJ45))) counts[1]++;

                        if (ws.Any(w => w.Type == WidgetType.Indicator && w.Indicator.Value.Type == IndicatorType.Unlit && w.Indicator.Value.Label == "FRQ")) counts[2]++;
                        if (ws.Any(w => w.Type == WidgetType.Indicator && w.Indicator.Value.Type == IndicatorType.Unlit && w.Indicator.Value.Label == "IND")) counts[2]++;
                        if (ws.Any(w => w.Type == WidgetType.Indicator && w.Indicator.Value.Type == IndicatorType.Unlit && w.Indicator.Value.Label == "TRN")) counts[2]++;
                        if (ws.Any(w => w.Type == WidgetType.PortPlate && w.PortTypes.Contains(PortType.DVI))) counts[2]++;

                        if (ws.Any(w => w.Type == WidgetType.Indicator && w.Indicator.Value.Type == IndicatorType.Unlit && w.Indicator.Value.Label == "SIG")) counts[3]++;
                        if (ws.Any(w => w.Type == WidgetType.Indicator && w.Indicator.Value.Type == IndicatorType.Unlit && w.Indicator.Value.Label == "SND")) counts[3]++;
                        if (ws.Any(w => w.Type == WidgetType.Indicator && w.Indicator.Value.Type == IndicatorType.Lit && w.Indicator.Value.Label == "NSA")) counts[3]++;
                        if (ws.Sum(w => w.BatteryType == BatteryType.BatteryAA ? 2 : w.BatteryType == BatteryType.BatteryD ? 1 : 0) % 2 == 0) counts[3]++;

                        if (ws.Any(w => w.Type == WidgetType.Indicator && w.Indicator.Value.Type == IndicatorType.Lit && w.Indicator.Value.Label == "BOB")) counts[4]++;
                        if (ws.Any(w => w.Type == WidgetType.Indicator && w.Indicator.Value.Type == IndicatorType.Lit && w.Indicator.Value.Label == "CLR")) counts[4]++;
                        if (ws.Any(w => w.Type == WidgetType.PortPlate && w.PortTypes.Contains(PortType.PS2))) counts[4]++;
                        if (ws.Any(w => w.Type == WidgetType.PortPlate && w.PortTypes.Contains(PortType.Serial))) counts[4]++;

                        if (ws.Any(w => w.Type == WidgetType.Indicator && w.Indicator.Value.Type == IndicatorType.Lit && w.Indicator.Value.Label == "FRQ")) counts[5]++;
                        if (ws.Any(w => w.Type == WidgetType.Indicator && w.Indicator.Value.Type == IndicatorType.Lit && w.Indicator.Value.Label == "SIG")) counts[5]++;
                        if (ws.Any(w => w.Type == WidgetType.Indicator && w.Indicator.Value.Type == IndicatorType.Lit && w.Indicator.Value.Label == "TRN")) counts[5]++;
                        if (serial.Any("02468".Contains)) counts[5]++;

                        if (ws.Any(w => w.Type == WidgetType.Indicator && w.Indicator.Value.Type == IndicatorType.Unlit && w.Indicator.Value.Label == "FRK")) counts[6]++;
                        if (ws.Any(w => w.Type == WidgetType.Indicator && w.Indicator.Value.Type == IndicatorType.Lit && w.Indicator.Value.Label == "MSA")) counts[6]++;
                        if (ws.Any(w => w.Type == WidgetType.PortPlate && w.PortTypes.Contains(PortType.Parallel))) counts[6]++;
                        if (serial.Any("AEIOU".Contains)) counts[6]++;

                        if (ws.Any(w => w.Type == WidgetType.Indicator && w.Indicator.Value.Type == IndicatorType.Unlit && w.Indicator.Value.Label == "CLR")) counts[7]++;
                        if (ws.Any(w => w.Type == WidgetType.Indicator && w.Indicator.Value.Type == IndicatorType.Unlit && w.Indicator.Value.Label == "MSA")) counts[7]++;
                        if (ws.Any(w => w.Type == WidgetType.Indicator && w.Indicator.Value.Type == IndicatorType.Lit && w.Indicator.Value.Label == "SND")) counts[7]++;
                        if (ws.Any(w => w.Type == WidgetType.PortPlate && w.PortTypes.Contains(PortType.StereoRCA))) counts[7]++;

                        var max = counts.Max();
                        var maxCount = counts.Count(c => c == max);

                        return new[] { $"{maxCount} regions", $"{max} matches" };
                    }
                },
                new Simulatable
                {
                    Active = false,
                    Name = "Astrology",
                    GetResult = (ws, serial) =>
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

                        var omen = new[] { astrologyPlanets[planet], astrologyZodiacs[zodiac], astrologyElements[element] }.Sum(str => str.Any(serial.Contains) ? 1 : -1);
                        omen += elemPlanet[element, planet] + elemZodiac[element, zodiac] + planetZodiac[planet, zodiac];
                        return omen == 0 ? new[] { "No Omen" } : omen > 0 ? new[] { "Good Omen", omen.ToString() } : new[] { "Poor Omen", (-omen).ToString() };
                    }
                },
                new Simulatable
                {
                    Active = false,
                    Name = "Plumbing",
                    GetResult = (ws, serial) =>
                    {
                        // Plumbing: most inputs and outputs
                        var redInput =
                            // For: Serial contains a '1'
                            +(serial.Contains('1') ? 1 : 0)
                            // For: Exactly 1 RJ45 port
                            + (ws.Sum(w => w.Type == WidgetType.PortPlate ? w.PortTypes.Count(p => p == PortType.RJ45) : 0) == 1 ? 1 : 0)
                            // Against: Any duplicate ports
                            - (ws.Where(w => w.Type == WidgetType.PortPlate).SelectMany(w => w.PortTypes).Order().SelectConsecutivePairs(false, (p1, p2) => p1 == p2).Any() ? 1 : 0)
                            // Against: Any duplicate serial characters
                            - (serial.Order().SelectConsecutivePairs(false, (c1, c2) => c1 == c2).Any() ? 1 : 0)
                            > 0;

                        var yellowInput =
                            // For: Serial contains a '2'
                            +(serial.Contains('2') ? 1 : 0)
                            // For: One or more Stereo RCA ports
                            + (ws.Any(w => w.Type == WidgetType.PortPlate && w.PortTypes.Contains(PortType.StereoRCA)) ? 1 : 0)
                            // Against: No duplicate ports
                            - (ws.Where(w => w.Type == WidgetType.PortPlate).SelectMany(w => w.PortTypes).Order().SelectConsecutivePairs(false, (p1, p2) => p1 == p2).Any() ? 0 : 1)
                            // Against: Serial contains a '1' or 'L'
                            - (serial.Contains('1') || serial.Contains('L') ? 1 : 0)
                            > 0;

                        var greenInput =
                            // For: Serial contains 3 or more numbers
                            +(serial.Count(ch => ch >= '0' && ch <= '9') >= 3 ? 1 : 0)
                            // For: One or more DVI-D ports
                            + (ws.Any(w => w.Type == WidgetType.PortPlate && w.PortTypes.Contains(PortType.DVI)) ? 1 : 0)
                            // Against: Red Input is inactive
                            - (redInput ? 0 : 1)
                            // Against: Yellow Input is inactive
                            - (yellowInput ? 0 : 1)
                            > 0;

                        var blueInput =
                            // Note: Always active if all other inputs are inactive
                            (!redInput && !yellowInput && !greenInput) ||
                            // For: At least 4 unique ports
                            +(ws.Where(w => w.Type == WidgetType.PortPlate).SelectMany(w => w.PortTypes).Distinct().Count() >= 4 ? 1 : 0)
                            // For: At least 4 batteries
                            + (ws.Sum(w => w.BatteryType == BatteryType.BatteryAA ? 2 : w.BatteryType == BatteryType.BatteryD ? 1 : 0) >= 4 ? 1 : 0)
                            // Against: No ports
                            - (ws.Any(w => w.Type == WidgetType.PortPlate && w.PortTypes.Length > 0) ? 0 : 1)
                            // Against: No batteries
                            - (ws.Any(w => w.Type == WidgetType.BatteryHolder) ? 0 : 1)
                            > 0;

                        var redOutput =
                            // For: One or more Serial ports
                            +(ws.Any(w => w.Type == WidgetType.PortPlate && w.PortTypes.Contains(PortType.Serial)) ? 1 : 0)
                            // For: Exactly one battery
                            + (ws.Count(w => w.Type == WidgetType.BatteryHolder && w.BatteryType == BatteryType.BatteryD) == 1 ? 1 : 0)
                            // Against: Serial contains more than 2 numbers
                            - (serial.Count(ch => ch >= '0' && ch <= '9') >= 3 ? 1 : 0)
                            // Against: More than 2 inputs are active
                            - ((redInput ? 1 : 0) + (blueInput ? 1 : 0) + (greenInput ? 1 : 0) + (yellowInput ? 1 : 0) > 2 ? 1 : 0)
                            > 0;

                        var yellowOutput =
                            // For: Any duplicate ports
                            +(ws.Where(w => w.Type == WidgetType.PortPlate).SelectMany(w => w.PortTypes).Order().SelectConsecutivePairs(false, (p1, p2) => p1 == p2).Any() ? 1 : 0)
                            // For: Serial contains a '4' or '8'
                            + (serial.Contains('4') || serial.Contains('8') ? 1 : 0)
                            // Against: Serial doesn't contain a '2'
                            - (serial.Contains('2') ? 0 : 1)
                            // Against: Green Input is active
                            - (greenInput ? 1 : 0)
                            > 0;

                        var greenOutput =
                            // For: Exactly 3 inputs are active
                            +((redInput ? 1 : 0) + (blueInput ? 1 : 0) + (greenInput ? 1 : 0) + (yellowInput ? 1 : 0) == 3 ? 1 : 0)
                            // For: Exactly 3 ports are present
                            + (ws.Sum(w => w.Type == WidgetType.PortPlate ? w.PortTypes.Length : 0) == 3 ? 1 : 0)
                            // Against: Less than 3 ports are present
                            - (ws.Sum(w => w.Type == WidgetType.PortPlate ? w.PortTypes.Length : 0) < 3 ? 1 : 0)
                            // Against: Serial contains more than 3 numbers
                            - (serial.Count(ch => ch >= '0' && ch <= '9') > 3 ? 1 : 0)
                            > 0;

                        var blueOutput =
                            // Note: Always active if all other outputs are inactive
                            (!redOutput && !greenOutput && !yellowOutput) ||
                            // For: All inputs are active
                            +((redInput && greenInput && yellowInput && blueInput) ? 1 : 0)
                            // For: Any other output is inactive
                            + (!redOutput || !yellowOutput || !greenOutput ? 1 : 0)
                            // Against: Less than 2 batteries
                            - (batteryCount(ws) < 2 ? 1 : 0)
                            // Against: No Parallel port
                            - (ws.Any(w => w.Type == WidgetType.PortPlate && w.PortTypes.Contains(PortType.Parallel)) ? 0 : 1)
                            > 0;

                        var numInputs = (redInput ? 1 : 0) + (blueInput ? 1 : 0) + (greenInput ? 1 : 0) + (yellowInput ? 1 : 0);
                        var numOutputs = (redOutput ? 1 : 0) + (blueOutput ? 1 : 0) + (greenOutput ? 1 : 0) + (yellowOutput ? 1 : 0);

                        if (numInputs + numOutputs >= 6)
                            File.AppendAllLines(@"D:\temp\temp.txt", new[] { $@"{numInputs + numOutputs} = Widgets: [{ws.JoinString(", ")}], Serial: {serial}" });

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
                    GetResult = (ws, serial) => new[] { Chess.GetSolution(serial.Last() % 2 != 0, out _, out _)[6] }
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
                    var result = sim.GetResult(edgework.Widgets, edgework.SerialNumber);
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
