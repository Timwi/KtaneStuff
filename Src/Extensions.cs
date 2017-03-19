using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Text;
using System.Text.RegularExpressions;
using RT.Util.ExtensionMethods;

namespace KtaneStuff
{
    static class Extensions
    {
        public static string ToBinary(this BigInteger num, int sepAt)
        {
            if (num.IsZero)
                return "0";
            var str = new StringBuilder();
            var ix = 0;
            while (!num.IsZero)
            {
                str.Append((char) ('0' + (int) (num & 1)));
                num >>= 1;
                ix++;
                if (ix % sepAt == 0)
                    str.Append('|');
            }
            return str.ToString();
        }

        public static string Reverse(this string str)
        {
            if (str.Length == 0)
                return str;
            var arr = new char[str.Length];
            for (int i = 0; i < str.Length; i++)
                arr[str.Length - 1 - i] = str[i];
            return new string(arr);
        }

        public static void ReplaceInFile(this string path, string startMarker, string endMarker, string newText)
        {
            File.WriteAllText(path, Regex.Replace(File.ReadAllText(path), @"(?<={0})(\r?\n)?( *).*?(?=\r?\n *{1})".Fmt(Regex.Escape(startMarker), Regex.Escape(endMarker)), m => m.Groups[1].Value + newText.Indent(m.Groups[2].Length), RegexOptions.Singleline));
        }

        public static int IncSafe<K1, K2>(this IDictionary<K1, Dictionary<K2, int>> dic, K1 key1, K2 key2, int amount = 1)
        {
            if (dic == null)
                throw new ArgumentNullException("dic");
            if (key1 == null)
                throw new ArgumentNullException(nameof(key1), "Null values cannot be used for keys in dictionaries.");
            if (key2 == null)
                throw new ArgumentNullException(nameof(key2), "Null values cannot be used for keys in dictionaries.");
            if (!dic.ContainsKey(key1))
                dic[key1] = new Dictionary<K2, int>();
            if (!dic[key1].ContainsKey(key2))
                return (dic[key1][key2] = amount);
            else
                return (dic[key1][key2] = dic[key1][key2] + amount);
        }
    }
}
