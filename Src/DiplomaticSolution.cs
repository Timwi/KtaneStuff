using System;
using System.Collections.Generic;
using System.Linq;
using RT.Util;
using RT.Util.Text;

namespace KtaneStuff
{
    static class DiplomaticSolution
    {
        public static void RunStatistics()
        {
            var opinions = new List<int>[6];
            for (int i = 0; i < 6; i++)
                opinions[i] = new List<int>();
            const int iterations = 1000000;
            for (int iter = 0; iter < iterations; iter++)
            {
                var edgework = Edgework.Generate(5, 5);
                var i = edgework.GetNumIndicators();
                var b = edgework.GetNumBatteries();
                var m = Rnd.Next(7, 16);
                var h = edgework.GetNumBatteryHolders();
                var f = Rnd.NextDouble() < .02 ? 1 : 0;
                var p = edgework.GetNumPorts();
                var l = edgework.GetNumPortPlates();
                var s = edgework.SerialNumberDigits().Sum();

                opinions[0].Add((h * f - l * l) + 20 * b);
                opinions[1].Add(s * p - b * b * b);
                opinions[2].Add(m + 3 * (i + p) - f * b);
                opinions[3].Add(i * (20 + m) - p * l);
                opinions[4].Add(p * (l * b + 3) + l + b - s);
                opinions[5].Add(h * h * h + l * l + f * f * f * f - m);
            }

            var tt = new TextTable { ColumnSpacing = 2 };
            tt.SetCell(0, 0, "Faction");
            tt.SetCell(1, 0, "Mean", alignment: HorizontalTextAlignment.Right);
            tt.SetCell(2, 0, "Variance", alignment: HorizontalTextAlignment.Right);
            tt.SetCell(3, 0, "Stddev", alignment: HorizontalTextAlignment.Right);
            tt.SetCell(4, 0, "Min", alignment: HorizontalTextAlignment.Right);
            tt.SetCell(5, 0, "Max", alignment: HorizontalTextAlignment.Right);
            for (int i = 0; i < 6; i++)
            {
                tt.SetCell(0, i + 1, "red,blue,yellow,green,purple,orange".Split(',')[i]);
                var mean = (double) opinions[i].Sum() / iterations;
                var variance = opinions[i].Sum(o => (o - mean) * (o - mean)) / iterations;
                tt.SetCell(1, i + 1, $"{mean:0.0}", alignment: HorizontalTextAlignment.Right);
                tt.SetCell(2, i + 1, $"{variance:0.0}", alignment: HorizontalTextAlignment.Right);
                tt.SetCell(3, i + 1, $"{Math.Sqrt(variance):0.0}", alignment: HorizontalTextAlignment.Right);
                tt.SetCell(4, i + 1, opinions[i].Min().ToString(), alignment: HorizontalTextAlignment.Right);
                tt.SetCell(5, i + 1, opinions[i].Max().ToString(), alignment: HorizontalTextAlignment.Right);
            }
            tt.WriteToConsole();
        }
    }
}