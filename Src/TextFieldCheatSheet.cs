using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using RT.TagSoup;
using RT.Util;
using RT.Util.ExtensionMethods;

namespace KtaneStuff
{
    static partial class Ktane
    {
        public static void DoTextFieldCheatSheet()
        {
            var dic = new Dictionary<char, Tuple<string, string>[]> {
                { 'A', Ut.NewArray(
                    Tuple.Create("Lit CLR", "1459"),
                    Tuple.Create("≥ 3 batteries", "BBFF"),
                    Tuple.Create("1 battery", "7F67"),
                    Tuple.Create("Lit FRK", "DC52"),
                    Tuple.Create("Otherwise", "A0C1")
                ) },
                { 'C', Ut.NewArray(
                    Tuple.Create("DVI-D port", "AA12"),
                    Tuple.Create("2 batteries", "FB01"),
                    Tuple.Create("No vowels in #", "DC52"),
                    Tuple.Create("Lit CAR", "1459"),
                    Tuple.Create("Otherwise", "7F67")
                ) },
                { 'E', Ut.NewArray(
                    Tuple.Create("≤ 2 batteries", "7F67"),
                    Tuple.Create("No RCA port", "AA12"),
                    Tuple.Create("Lit BOB", "A0C1"),
                    Tuple.Create("RJ-45 port", "BBFF"),
                    Tuple.Create("Otherwise", "DC52")
                ) },
                { 'B', Ut.NewArray(
                    Tuple.Create("No battery", "965A"),
                    Tuple.Create("Last digit odd", "1459"),
                    Tuple.Create("No serial port", "DC52"),
                    Tuple.Create("Lit TRN", "A0C1"),
                    Tuple.Create("Otherwise", "7F67")
                ) },
                { 'D', Ut.NewArray(
                    Tuple.Create("Parallel port", "FB01"),
                    Tuple.Create("≤ 1 battery", "AA12"),
                    Tuple.Create("Lit SIG", "BBFF"),
                    Tuple.Create("No PS/2 port", "965A"),
                    Tuple.Create("Otherwise", "1459")
                ) },
                { 'F', Ut.NewArray(
                    Tuple.Create("No serial port", "DC52"),
                    Tuple.Create("Vowel in #", "A0C1"),
                    Tuple.Create("Lit IND", "1459"),
                    Tuple.Create("Last digit even", "FB01"),
                    Tuple.Create("Otherwise", "AA12")
                ) }
            };

            var tables = new Dictionary<string, char[][]> {
                { "FB01",
                    new char[][] {
                        new char[] { 'D', 'C', 'F', 'A' },
                        new char[] { 'B', 'E', 'F', 'F' },
                        new char[] { 'B', 'B', 'B', 'C' },
                    }
                },
                { "965A",
                    new char[][] {
                        new char[] { 'C', 'B', 'E', 'F' },
                        new char[] { 'E', 'B', 'F', 'E' },
                        new char[] { 'D', 'C', 'A', 'A' },
                    }
                },
                { "1459",
                    new char[][] {
                        new char[] { 'B', 'A', 'B', 'B' },
                        new char[] { 'C', 'D', 'F', 'D' },
                        new char[] { 'D', 'F', 'C', 'E' },
                    }
                },
                { "BBFF",
                    new char[][] {
                        new char[] { 'D', 'A', 'B', 'F' },
                        new char[] { 'D', 'F', 'B', 'E' },
                        new char[] { 'C', 'E', 'B', 'A' },
                    }
                },
                { "DC52",
                    new char[][] {
                        new char[] { 'C', 'B', 'D', 'E' },
                        new char[] { 'A', 'F', 'D', 'C' },
                        new char[] { 'B', 'E', 'B', 'D' },
                    }
                },
                { "7F67",
                    new char[][] {
                        new char[] { 'A', 'D', 'C', 'B' },
                        new char[] { 'A', 'C', 'B', 'C' },
                        new char[] { 'A', 'E', 'F', 'A' },
                    }
                },
                { "A0C1",
                    new char[][] {
                        new char[] { 'E', 'C', 'F', 'A' },
                        new char[] { 'C', 'F', 'B', 'D' },
                        new char[] { 'F', 'F', 'B', 'C' },
                    }
                },
                { "AA12",
                    new char[][] {
                        new char[] { 'B', 'E', 'A', 'B' },
                        new char[] { 'E', 'D', 'F', 'A' },
                        new char[] { 'B', 'C', 'E', 'C' },
                    }
                }
            };

            var path = @"D:\c\KTANE\HTML\Text Field cheat sheet (Timwi).html";
            File.WriteAllText(path, Regex.Replace(File.ReadAllText(path), @"(?<=<!--##-->).*(?=<!--###-->)",
                dic.OrderBy(kvp => "ADBECF".IndexOf(kvp.Key)).Select(kvp =>
                    $@"<div class='text-field'><table class='text-field'>{
                        kvp.Value.Select((tup, ix) => $"<tr>{(ix == 0 ? $"<th class='letter' rowspan='{kvp.Value.Length}'>{kvp.Key}" : null)}<th>{tup.Item1}<td>{svg(tables[tup.Item2], kvp.Key)}").JoinString()
                    }</table></div>"
                ).JoinString(),
                RegexOptions.Singleline));
        }

        private static string svg(char[][] table, char key)
        {
            return $@"<svg class='field' viewBox='-.05 -.05 4.1 3.1'>{
                Enumerable.Range(0, 3).SelectMany(row => Enumerable.Range(0, 4).Select(col => $"<rect x='{col}' y='{row}' width='1' height='1' fill='{(table[row][col] == key ? "#888" : "none")}' stroke='#000' stroke-width='.1' />")).JoinString()
            }</svg>";
        }
    }
}
