using System;
using System.Linq;
using RT.Util;
using RT.Util.ExtensionMethods;

namespace KtaneStuff
{
    static partial class Ktane
    {
        enum WidgetType { BatteryHolder, Indicator, PortPlate }
        const int NumWidgets = 5;
        const int NumWidgetTypes = 3;
        enum BatteryType { BatteryAA, BatteryD }
        const int NumBatteryTypes = 2;
        enum IndicatorType { Lit, Unlit }
        const int NumIndicatorTypes = 2;
        enum Indicator { SND, CLR, CAR, IND, FRQ, SIG, NSA, MSA, TRN, BOB, FRK }
        const int NumIndicators = 11;
        enum PortType { DVI, Parallel, PS2, RJ45, Serial, StereoRCA }
        const int NumPortTypes = 6;

        sealed class Widget
        {
            public WidgetType Type;
            public BatteryType? BatteryType;
            public IndicatorType? IndicatorType;
            public Indicator? Indicator;
            public PortType[] PortTypes;

            public override string ToString()
            {
                switch (Type)
                {
                    case WidgetType.BatteryHolder:
                        return BatteryType.ToString();
                    case WidgetType.Indicator:
                        return IndicatorType.ToString() + " " + Indicator.ToString();
                    case WidgetType.PortPlate:
                        return "Ports: " + PortTypes.JoinString(", ");
                    default:
                        return "Blank";
                }
            }
        }

        private static Widget[] generateRandomWidgets(Random rnd = null)
        {
            var next = Ut.Lambda((int max) => rnd == null ? Rnd.Next(0, max) : rnd.Next(0, max));
            return Enumerable.Range(0, NumWidgets)
                .Select(i => (WidgetType) next(NumWidgetTypes))
                .Select(wt => new Widget
                {
                    Type = wt,
                    BatteryType = wt == WidgetType.BatteryHolder ? (BatteryType?) next(NumBatteryTypes) : null,
                    IndicatorType = wt == WidgetType.Indicator ? (IndicatorType?) next(NumIndicatorTypes) : null,
                    Indicator = wt == WidgetType.Indicator ? (Indicator?) next(NumIndicators) : null,
                    PortTypes = wt == WidgetType.PortPlate ? Enumerable.Range(0, next(5)).Select(p => (PortType) next(NumPortTypes)).ToArray() : null
                })
                .ToArray();
        }

        private static string generateRandomSerial(Random rnd = null)
        {
            return "??DLLD".Select(chs => Rnd.GenerateString(1, chs == 'L' ? "ABCDEFGHIJKLMNOPQRSTUVWXYZ" : chs == 'D' ? "0123456789" : "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789", rnd)).JoinString();
        }
    }
}
