using System.Numerics;
using System.Text;

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
    }
}
