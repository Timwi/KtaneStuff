using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CSharp;
using RT.Util;
using RT.Util.ExtensionMethods;

namespace KtaneStuff
{
    static class JackAttack
    {
        public static void MakeGoodsheet()
        {
            var phrasesFile = new HClient().Get(@"https://raw.githubusercontent.com/Blananas2/ktane-jackAttack/master/Assets/PhraseList.cs").DataString;
            var csc = new CSharpCodeProvider();
            var parameters = new CompilerParameters(new[] { "mscorlib.dll", "System.Core.dll" });
            parameters.GenerateInMemory = true;
            var results = csc.CompileAssemblyFromSource(parameters, phrasesFile);
            var phrases = (string[]) results.CompiledAssembly.GetType("PhraseList").GetField("phrases").GetValue(null);
            var groups = new List<(string title, string[] left, string[] right)>();
            var sb = new StringBuilder();
            for (var i = 0; i < 10; i++)
                sb.Append($@"<table class='lookup'><tr><th colspan='2'>{phrases[i * 65].Replace("\n", " ")}</th></tr>{Enumerable.Range(0, 8).Select(j => $@"<tr><td>{phrases[i * 65 + 1 + j].Replace("\n", " ")}</td><td>{phrases[i * 65 + 9 + j].Replace("\n", " ")}</td></tr>").JoinString()}</table>");
            Utils.ReplaceInFile(@"D:\c\KTANE\Public\HTML\Jack Attack lookup table (Timwi).html", @"<!--%%-->", @"<!--%%%-->", sb.ToString());
        }
    }
}