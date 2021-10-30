using System.IO;
using System.Linq;
using System.Text;
using RT.Util.ExtensionMethods;

namespace KtaneStuff
{
    static class UltimateCipher
    {
        public static void GenerateWordlist(string[] words)
        {
            var sb = new StringBuilder();
            sb.AppendLine(@"using System.Collections.Generic;");
            sb.AppendLine();
            sb.AppendLine(@"namespace Words");
            sb.AppendLine(@"{");
            sb.AppendLine(@"    class Data");
            sb.AppendLine(@"    {");
            sb.AppendLine(@"        public readonly List<List<string>> allWords = new List<List<string>>()");
            sb.AppendLine(@"        {");
            for (var len = 4; len <= 8; len++)
            {
                sb.AppendLine(@"            new List<string>()");
                sb.AppendLine(@"            {");
                for (var firstLetter = 0; firstLetter < 26; firstLetter++)
                    sb.AppendLine(@$"                {words.Where(w => w.Length == len && w[0] - 'A' == firstLetter).Order().Select(w => $@"""{w}""").JoinString(", ")}{(firstLetter == 25 ? "" : ",")}");
                sb.AppendLine($@"            }}{(len == 8 ? "" : ",")}");
            }
            sb.AppendLine(@"        };");
            sb.AppendLine(@"    }");
            sb.AppendLine(@"}");
            File.WriteAllText(@"D:\c\KTANE\UltimateCipher\Assets\Scripts\Data.cs", sb.ToString());
        }
    }
}