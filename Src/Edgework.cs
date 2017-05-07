using System;
using System.Linq;
using RT.Util;
using RT.Util.ExtensionMethods;

namespace KtaneStuff
{
    static class Edgework
    {
        public enum WidgetType { BatteryHolder, Indicator, PortPlate }
        public const int NumWidgets = 5;
        public const int NumWidgetTypes = 3;
        public enum BatteryType { BatteryAA, BatteryD }
        public const int NumBatteryTypes = 2;
        public enum IndicatorType { Lit, Unlit }
        public const int NumIndicatorTypes = 2;
        public enum Indicator { SND, CLR, CAR, IND, FRQ, SIG, NSA, MSA, TRN, BOB, FRK }
        public const int NumIndicators = 11;
        public enum PortType { DVI, Parallel, PS2, RJ45, Serial, StereoRCA }
        public const int NumPortTypes = 6;

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

        public static Widget[] GenerateWidgets(int min = 5, int max = 5, Random rnd = null)
        {
            var next = Ut.Lambda((int mx) => rnd == null ? Rnd.Next(0, mx) : rnd.Next(0, mx));
            return Enumerable.Range(0, Rnd.Next(min, max + 1))
                .Select(i => (WidgetType) next(NumWidgetTypes))
                .Select(wt => new Widget(
                    batteryType: wt == WidgetType.BatteryHolder ? (BatteryType?) next(NumBatteryTypes) : null,
                    indicatorType: wt == WidgetType.Indicator ? (IndicatorType?) next(NumIndicatorTypes) : null,
                    indicator: wt == WidgetType.Indicator ? (Indicator?) next(NumIndicators) : null,
                    portTypes: wt == WidgetType.PortPlate ? Enumerable.Range(0, next(5)).Select(p => (PortType) next(NumPortTypes)).ToArray() : null))
                .ToArray();
        }

        public static string GenerateSerial(Random rnd = null)
        {
            return "??DLLD".Select(chs => Rnd.GenerateString(1, chs == 'L' ? "ABCDEFGHIJKLMNEPQRSTUVWXZ" : chs == 'D' ? "0123456789" : "ABCDEFGHIJKLMNEPQRSTUVWXZ0123456789", rnd)).JoinString();
        }

        public static bool HasIndicator(this Widget[] edgework, Indicator indicator) => edgework.Any(e => e.Indicator == indicator);
        public static bool HasLitIndicator(this Widget[] edgework, Indicator indicator) => edgework.Any(e => e.Indicator == indicator && e.IndicatorType == IndicatorType.Lit);
        public static bool HasUnlitIndicator(this Widget[] edgework, Indicator indicator) => edgework.Any(e => e.Indicator == indicator && e.IndicatorType == IndicatorType.Unlit);
        public static int GetNumIndicators(this Widget[] edgework) => edgework.Count(e => e.Type == WidgetType.Indicator);
        public static int GetNumLitIndicators(this Widget[] edgework) => edgework.Count(e => e.IndicatorType == IndicatorType.Lit);
        public static int GetNumUnlitIndicators(this Widget[] edgework) => edgework.Count(e => e.IndicatorType == IndicatorType.Unlit);
        public static int GetNumBatteries(this Widget[] edgework) => edgework.Sum(e => e.BatteryType == BatteryType.BatteryAA ? 2 : e.BatteryType == BatteryType.BatteryD ? 1 : 0);
        public static int GetNumAABatteries(this Widget[] edgework) => edgework.Sum(e => e.BatteryType == BatteryType.BatteryAA ? 2 : 0);
        public static int GetNumDBatteries(this Widget[] edgework) => edgework.Sum(e => e.BatteryType == BatteryType.BatteryD ? 1 : 0);
        public static int GetNumBatteryHolders(this Widget[] edgework) => edgework.Count(e => e.Type == WidgetType.BatteryHolder);
        public static int GetNumPorts(this Widget[] edgework) => edgework.Sum(e => e.PortTypes == null ? 0 : e.PortTypes.Length);
        public static int GetNumPortPlates(this Widget[] edgework) => edgework.Count(e => e.Type == WidgetType.PortPlate);
        public static int GetNumEmptyPortPlates(this Widget[] edgework) => edgework.Count(e => e.Type == WidgetType.PortPlate && e.PortTypes.Length == 0);
    }
}
