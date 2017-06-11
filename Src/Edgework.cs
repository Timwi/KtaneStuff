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
        public const double LitIndicatorProbability = .6;
        public const int NumIndicators = 11;
        public const int NumPortTypes = 6;

        private Edgework(Widget[] widgets, string serialNumber) { Widgets = widgets; SerialNumber = serialNumber; }

        public static Edgework Generate(int minWidgets = 5, int maxWidgets = 5, Random rnd = null)
        {
            int next(int mx) => rnd == null ? Rnd.Next(0, mx) : rnd.Next(0, mx);
            double nextDbl() => rnd == null ? Rnd.NextDouble() : rnd.NextDouble();

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

            return new Edgework(

                // Widgets
                Enumerable.Range(0, Rnd.Next(minWidgets, maxWidgets + 1))
                    .Select(i => (WidgetType) next(NumWidgetTypes))
                    .Select(wt => new Widget(
                        batteryType: wt == WidgetType.BatteryHolder ? (BatteryType?) next(NumBatteryTypes) : null,
                        indicatorType: wt == WidgetType.Indicator ? (IndicatorType?) (nextDbl() < LitIndicatorProbability ? IndicatorType.Lit : IndicatorType.Unlit) : null,
                        indicator: wt == WidgetType.Indicator ? (Indicator?) next(NumIndicators) : null,
                        portTypes: wt == WidgetType.PortPlate
                            ? plates[next(plates.Length)].Apply(plate => fromBits(next(1 << plate.Length), plate))
                            : null))
                    .ToArray(),

                // Serial number
                "??DLLD".Select(chs => Rnd.GenerateString(1, chs == 'L' ? "ABCDEFGHIJKLMNEPQRSTUVWXZ" : chs == 'D' ? "0123456789" : "ABCDEFGHIJKLMNEPQRSTUVWXZ0123456789", rnd)).JoinString());
        }

        public bool HasIndicator(Indicator indicator) => Widgets.Any(e => e.Indicator == indicator);
        public bool HasLitIndicator(Indicator indicator) => Widgets.Any(e => e.Indicator == indicator && e.IndicatorType == IndicatorType.Lit);
        public bool HasUnlitIndicator(Indicator indicator) => Widgets.Any(e => e.Indicator == indicator && e.IndicatorType == IndicatorType.Unlit);
        public int GetNumIndicators() => Widgets.Count(e => e.Type == WidgetType.Indicator);
        public int GetNumLitIndicators() => Widgets.Count(e => e.IndicatorType == IndicatorType.Lit);
        public int GetNumUnlitIndicators() => Widgets.Count(e => e.IndicatorType == IndicatorType.Unlit);
        public int GetNumBatteries() => Widgets.Sum(e => e.BatteryType == BatteryType.BatteryAA ? 2 : e.BatteryType == BatteryType.BatteryD ? 1 : 0);
        public int GetNumAABatteries() => Widgets.Sum(e => e.BatteryType == BatteryType.BatteryAA ? 2 : 0);
        public int GetNumDBatteries() => Widgets.Sum(e => e.BatteryType == BatteryType.BatteryD ? 1 : 0);
        public int GetNumBatteryHolders() => Widgets.Count(e => e.Type == WidgetType.BatteryHolder);
        public int GetNumPorts() => Widgets.Sum(e => e.PortTypes == null ? 0 : e.PortTypes.Length);
        public int GetNumPortPlates() => Widgets.Count(e => e.Type == WidgetType.PortPlate);
        public int GetNumEmptyPortPlates() => Widgets.Count(e => e.Type == WidgetType.PortPlate && e.PortTypes.Length == 0);
        public IEnumerable<PortType> GetPorts() => Widgets.Where(w => w.PortTypes != null).SelectMany(w => w.PortTypes);
        public IEnumerable<PortType> GetUniquePorts() => Widgets.Where(w => w.PortTypes != null).SelectMany(w => w.PortTypes).Distinct();
    }

    public enum WidgetType { BatteryHolder, Indicator, PortPlate }
    public enum BatteryType { BatteryAA, BatteryD }
    public enum IndicatorType { Lit, Unlit }
    public enum Indicator { SND, CLR, CAR, IND, FRQ, SIG, NSA, MSA, TRN, BOB, FRK }
    public enum PortType { DVI, Parallel, PS2, RJ45, Serial, StereoRCA }

    public sealed class Widget
    {
        public BatteryType? BatteryType { get; private set; }
        public IndicatorType? IndicatorType { get; private set; }
        public Indicator? Indicator { get; private set; }
        public PortType[] PortTypes { get; private set; }

        public WidgetType Type => BatteryType != null ? WidgetType.BatteryHolder : IndicatorType != null ? WidgetType.Indicator : WidgetType.PortPlate;

        public Widget(BatteryType? batteryType, IndicatorType? indicatorType, Indicator? indicator, PortType[] portTypes)
        {
            BatteryType = batteryType;
            IndicatorType = indicatorType;
            Indicator = indicator;
            PortTypes = portTypes;
        }

        public override string ToString()
        {
            if (BatteryType != null)
                return BatteryType.ToString();
            if (IndicatorType != null)
                return IndicatorType.ToString() + " " + Indicator.ToString();
            if (PortTypes != null)
                return "Ports: " + PortTypes.JoinString(", ");
            return "Invalid";
        }
    }
}
