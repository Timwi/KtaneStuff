using System;
using System.Collections.Generic;
using System.Linq;
using RT.Util;
using RT.Util.ExtensionMethods;

namespace KtaneStuff
{
    public class Edgework
    {
        public Widget[] Widgets { get; private set; }
        public string SerialNumber { get; private set; }

        public const int NumWidgets = 5;
        public const int NumWidgetTypes = 3;
        public const int NumBatteryTypes = 2;
        public const int NumIndicatorTypes = 2;
        public const int NumIndicators = 11;
        public const int NumPortTypes = 6;

        private Edgework(Widget[] widgets, string serialNumber) { Widgets = widgets; SerialNumber = serialNumber; }

        public static Edgework Generate(int minWidgets = 5, int? maxWidgets = null, bool allowCustomIndicators = false, Random rnd = null)
        {
            int next(int mx) => rnd == null ? Rnd.Next(0, mx) : rnd.Next(0, mx);

            var plates = Ut.NewArray(
                new[] { PortType.Parallel, PortType.Serial },
                new[] { PortType.DVI, PortType.PS2, PortType.RJ45, PortType.StereoRCA });
            PortType[] fromBits(int bits, PortType[] types)
            {
                var list = new List<PortType>();
                var ix = 0;
                while (bits > 0)
                {
                    if ((bits & 1) == 1)
                        list.Add(types[ix]);
                    ix++;
                    bits >>= 1;
                }
                return list.ToArray();
            }

            var takenIndicatorLabels = new HashSet<string>();
            return new Edgework(

                // Widgets
                Enumerable.Range(0, next((maxWidgets ?? minWidgets) - minWidgets + 1) + minWidgets)
                    .Select(i => (WidgetType) next(NumWidgetTypes))
                    .Select(wt => new Widget(
                        batteryType: wt == WidgetType.BatteryHolder ? (BatteryType?) next(NumBatteryTypes) : null,
                        indicator: wt == WidgetType.Indicator ? Indicator.Random(takenIndicatorLabels, rnd, allowCustomIndicators) : (Indicator?) null,
                        portTypes: wt == WidgetType.PortPlate
                            ? plates[next(plates.Length)].Apply(plate => fromBits(next(1 << plate.Length), plate))
                            : null))
                    .ToArray(),

                // Serial number
                "??DLLD".Select(chs => Rnd.GenerateString(1, chs == 'L' ? "ABCDEFGHIJKLMNEPQRSTUVWXZ" : chs == 'D' ? "0123456789" : "ABCDEFGHIJKLMNEPQRSTUVWXZ0123456789", rnd)).JoinString());
        }

        public override string ToString()
        {
            return $"#={SerialNumber}{(Widgets.Any() ? "; " : "")}{Widgets.GroupBy(w => w.Type).Select(g => g.JoinString(", ")).JoinString("; ")}";
        }

        public IEnumerable<int> SerialNumberDigits() => SerialNumber.Where(ch => ch >= '0' && ch <= '9').Select(ch => ch - '0');
        public IEnumerable<char> SerialNumberLetters() => SerialNumber.Where(ch => ch >= 'A' && ch <= 'Z');
        public bool SerialNumberHasVowel() => SerialNumberLetters().Any("AEIOU".Contains);

        public bool HasIndicator(string label) => Widgets.Any(e => e.Indicator != null && e.Indicator.Value.Label == label);
        public bool HasLitIndicator(string label) => Widgets.Any(e => e.Indicator != null && e.Indicator.Value.Type == IndicatorType.Lit && e.Indicator.Value.Label == label);
        public bool HasUnlitIndicator(string label) => Widgets.Any(e => e.Indicator != null && e.Indicator.Value.Type == IndicatorType.Unlit && e.Indicator.Value.Label == label);
        public int GetNumIndicators() => Widgets.Count(e => e.Type == WidgetType.Indicator);
        public int GetNumLitIndicators() => Widgets.Count(e => e.Indicator != null && e.Indicator.Value.Type == IndicatorType.Lit);
        public int GetNumUnlitIndicators() => Widgets.Count(e => e.Indicator != null && e.Indicator.Value.Type == IndicatorType.Unlit);
        public int GetNumBatteries() => Widgets.Sum(e => e.BatteryType == BatteryType.BatteryAA ? 2 : e.BatteryType == BatteryType.BatteryD ? 1 : 0);
        public int GetNumAABatteries() => Widgets.Sum(e => e.BatteryType == BatteryType.BatteryAA ? 2 : 0);
        public int GetNumDBatteries() => Widgets.Sum(e => e.BatteryType == BatteryType.BatteryD ? 1 : 0);
        public int GetNumBatteryHolders() => Widgets.Count(e => e.Type == WidgetType.BatteryHolder);
        public IEnumerable<PortType> GetPorts() => Widgets.Where(w => w.PortTypes != null).SelectMany(w => w.PortTypes);
        public IEnumerable<PortType> GetUniquePorts() => Widgets.Where(w => w.PortTypes != null).SelectMany(w => w.PortTypes).Distinct();
        public int GetNumPorts() => Widgets.Sum(e => e.PortTypes == null ? 0 : e.PortTypes.Length);
        public int GetNumPorts(PortType port) => GetPorts().Count(pt => pt == port);
        public int GetNumPortPlates() => Widgets.Count(e => e.Type == WidgetType.PortPlate);
        public int GetNumPortTypes() => GetUniquePorts().Count();
        public int GetNumEmptyPortPlates() => Widgets.Count(e => e.Type == WidgetType.PortPlate && e.PortTypes.Length == 0);

        public bool HasDuplicatePorts()
        {
            var hash = new HashSet<PortType>();
            foreach (var port in GetPorts())
                if (!hash.Add(port))
                    return true;
            return false;
        }
    }

    public enum WidgetType { BatteryHolder, Indicator, PortPlate }
    public enum BatteryType { BatteryAA, BatteryD }
    public enum IndicatorType { Lit, Unlit }
    public enum PortType { DVI, Parallel, PS2, RJ45, Serial, StereoRCA }

    public struct Indicator
    {
        public const double LitIndicatorProbability = .6;
        public string Label;
        public IndicatorType Type;
        public static string[] WellKnown = new[] { "SND", "CLR", "CAR", "IND", "FRQ", "SIG", "NSA", "MSA", "TRN", "BOB", "FRK" };
        public static Indicator Random(HashSet<string> taken, Random rnd = null, bool allowCustomIndicators = false)
        {
            var wellknown = WellKnown.Where(w => !taken.Contains(w)).ToArray();
            var result = new Indicator
            {
                Type = (rnd == null ? Rnd.NextDouble() : rnd.NextDouble()) < LitIndicatorProbability ? IndicatorType.Lit : IndicatorType.Unlit,
                Label = (wellknown.Length == 0 || (rnd == null ? Rnd.NextDouble() : rnd.NextDouble()) < .1) && allowCustomIndicators
                    ? Enumerable.Range(0, 26).SelectMany(a => Enumerable.Range(0, 26).SelectMany(b => Enumerable.Range(0, 26).Select(c => string.Concat((char) (a + 'A'), (char) (b + 'A'), (char) (c + 'A'))))).Where(l => !taken.Contains(l)).PickRandom(rnd)
                    : wellknown.Length == 0 ? "NLL" : WellKnown.Where(w => !taken.Contains(w)).PickRandom(rnd)
            };
            taken.Add(result.Label);
            return result;
        }
        public override string ToString() => $"{Type} {Label}";
    }

    public sealed class Widget
    {
        public BatteryType? BatteryType { get; private set; }
        public Indicator? Indicator { get; private set; }
        public PortType[] PortTypes { get; private set; }

        public WidgetType Type => BatteryType != null ? WidgetType.BatteryHolder : Indicator != null ? WidgetType.Indicator : WidgetType.PortPlate;

        public Widget(BatteryType? batteryType, Indicator? indicator, PortType[] portTypes)
        {
            BatteryType = batteryType;
            Indicator = indicator;
            PortTypes = portTypes;
        }

        public override string ToString()
        {
            if (BatteryType != null)
                return BatteryType.ToString();
            if (Indicator != null)
                return Indicator.Value.ToString();
            if (PortTypes == null)
                return "Invalid";
            if (PortTypes.Length == 0)
                return "Empty port plate";
            return "Port plate [" + PortTypes.JoinString(", ") + "]";
        }
    }
}
