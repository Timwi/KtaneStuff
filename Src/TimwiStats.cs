using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using RT.Util.ExtensionMethods;

namespace KtaneStuff
{
    static class TimwiStats
    {
        public static void GenerateModuleHistogram()
        {
            // Timwi — https://docs.google.com/spreadsheets/d/1FumPsAfpDoafiFlEUI-KXL8f_69ZVkfYGMDy2W1WoF0/edit#gid=839153156
            //var data = @"2016-10-07,2016-10-18,2016-10-24,2016-10-28,2016-11-03,2016-11-12,2016-11-21,2016-11-23,2016-11-24,2016-11-25,2016-12-02,2016-12-07,2016-12-20,2017-01-03,2017-01-06,2017-01-15,2017-01-20,2017-01-23,2017-03-17,2017-04-30,2017-05-11,2017-06-13,2017-06-25,2017-07-02,2017-07-14,2017-08-11,2017-09-03,2017-09-06,2017-09-23,2017-10-05,2017-10-31,2017-11-04,2018-01-01,2018-02-26,2018-04-17,2018-05-07,2018-05-29,2018-05-29,2018-06-01,2018-06-22,2018-07-10,2018-07-28,2018-08-08,2018-09-08,2018-10-08,2018-10-30,2018-11-12,2018-11-20,2018-11-21,2018-12-09,2018-12-27,2018-12-28,2018-12-29,2018-12-29,2018-12-30,2018-12-31,2019-01-09,2019-02-26,2019-04-08,2019-06-23,2019-11-03,2020-05-16,2020-06-02,2020-06-20,2021-05-19,2021-05-20,2021-06-07,2021-07-06,2021-07-09,2021-07-14,2021-07-17,2021-07-31,2021-08-07,2021-08-12,2021-08-26,2021-09-09,2021-09-09,2021-09-09,2021-09-09,2021-09-19,2021-09-19,2021-09-21,2021-10-10,2021-10-25,2021-10-29,2021-11-01,2021-11-06,2022-04-10,2022-08-16,2022-08-21"
            // Quinn — https://docs.google.com/spreadsheets/d/1ykG-ghYE9VR60Ll4HDhefYFc6vFnueTeTXPDwtu4FyY/edit#gid=839153156
            var data = @"2017-07-01,2017-07-31,2017-08-14,2017-09-06,2017-11-04,2020-05-03,2021-06-30,2021-07-06,2021-07-08,2021-07-21,2021-08-12,2021-08-19,2021-08-26,2021-09-09,2021-09-09,2021-09-09,2021-09-09,2021-09-17,2021-10-14,2021-10-21,2021-10-23,2021-11-03,2021-11-04,2021-11-06,2021-11-30,2021-12-02,2021-12-10,2021-12-15,2021-12-21,2021-12-27,2022-01-01,2022-01-15,2022-01-20,2022-02-04,2022-02-20,2022-02-23,2022-02-23,2022-02-23,2022-02-23,2022-02-23,2022-02-28,2022-02-28,2022-03-10,2022-04-01,2022-04-03,2022-05-10,2022-05-11,2022-05-14,2022-06-01,2022-07-06,2022-07-18"
                .Split(',').Select(DateTime.Parse).ToArray();

            var buckets = new Dictionary<int, List<DateTime>>();
            foreach (var date in data)
                buckets.AddSafe(date.Year * 12 + date.Month - 1, date);

            var barHeight = 5;
            var bars = new StringBuilder();
            foreach (var bucket in buckets)
                bars.Append($"<rect x='{bucket.Key}' y='{-bucket.Value.Count * barHeight}' width='.9' height='{bucket.Value.Count * barHeight}' />");
            var min = buckets.Keys.Min();
            min -= min % 12;
            var width = buckets.Keys.Max() - min;
            width += 12 - width % 12;
            var height = buckets.Max(b => b.Value.Count) * barHeight;

            var axis = new StringBuilder();
            axis.Append($@"<path d='M{min} 0h{width}' fill='none' stroke='black' stroke-width='.1' />");
            for (var i = 0; i < width; i += 12)
            {
                axis.Append($@"<path d='M{min + i} 0v.5' fill='none' stroke='black' stroke-width='.1' />");
                axis.Append($@"<text x='{min + i + 6}' y='1.4'>{(min + i) / 12}</text>");
            }
            axis.Append($@"<path d='M{min + width} 0v.5' fill='none' stroke='black' stroke-width='.1' />");

            File.WriteAllText(@"D:\Daten\KTANE\Quinn module histogram.svg", $@"
                <svg
                        xmlns='http://www.w3.org/2000/svg'
                        viewBox='{min - 1} {-height - 1} {width + 2} {height + 3}'
                        text-anchor='middle' font-size='1.5' font-family='Work Sans'>
                    <g fill='hsl(220, 80%, 47%)'>{axis}{bars}</g>
                </svg>");

        }
    }
}