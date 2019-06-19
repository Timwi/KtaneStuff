using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using RT.Util.ExtensionMethods;

namespace KtaneStuff
{
    static class ElderFuthark
    {
        public static void ReplaceRunesInManual()
        {
            var d = new Dictionary<string, List<string>>();
            foreach (var module in Ktane.GetLiveJson())
                if (module["Type"].GetString() != "Widget")
                {
                    var name = Regex.Replace(module["Name"].GetString(), @"^The ", "").ToUpperInvariant().Where(ch => ch >= 'A' && ch <= 'Z').JoinString();
                    var x = new HashSet<string>();
                    for (int j = 0; j < name.Length - 2; j++)
                        x.Add(name.Substring(j, 3));
                    foreach (var y in x)
                        d.AddSafe(y, name);
                }
            var i = 1;
            foreach (var kvp in d.OrderByDescending(k => k.Value.Count))
            {
                Console.WriteLine($"{i}. {kvp.Key} ({kvp.Value.Count})");
                i++;
            }
            Console.WriteLine(d.Count);
        }
    }
}