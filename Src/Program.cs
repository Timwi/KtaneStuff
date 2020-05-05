using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using KtaneStuff.Modeling;
using RT.Json;
using RT.PostBuild;
using RT.Util.ExtensionMethods;

using static KtaneStuff.Modeling.Md;

[assembly: AssemblyTitle("KtaneStuff")]
[assembly: AssemblyDescription("Contains some ancillary code used in the creation of some Keep Talking and Nobody Explodes mods.")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("")]
[assembly: AssemblyProduct("KtaneStuff")]
[assembly: AssemblyCopyright("Copyright © Timwi 2016–2019")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]
[assembly: ComVisible(false)]
[assembly: Guid("95055383-2e25-42be-97b7-e1411a695e1d")]
[assembly: AssemblyVersion("1.0.0.0")]
[assembly: AssemblyFileVersion("1.0.0.0")]

namespace KtaneStuff
{
    partial class Program
    {
        [STAThread]
        static int Main(string[] args)
        {
            try { Console.OutputEncoding = Encoding.UTF8; }
            catch { }

            if (args.Length == 2 && args[0] == "--post-build-check")
                return PostBuildChecker.RunPostBuildChecks(args[1], Assembly.GetExecutingAssembly());

            var start = DateTime.UtcNow;

            var inf = Hexamaze.GenerateHexamaze();
            inf = Hexamaze.GenerateMarkings(inf);
            var w = 6 * inf.Size - 2;
            var h = 4 * inf.Size - 1;
            var chs = new char[w, h];
            for (var x = 0; x < w; x++)
                for (var y = 0; y < h; y++)
                    chs[x, y] = ' ';
            foreach (var (hex, walls) in inf.Walls.Select(kvp => (kvp.Key, kvp.Value)))
            {
                // NW wall
                if (walls[0])
                    chs[w / 2 + 3 * hex.Q - 2, h / 2 + hex.Q + 2 * hex.R] = '/';

                // N wall
                if (walls[1])
                {
                    chs[w / 2 + 3 * hex.Q - 1, h / 2 + hex.Q + 2 * hex.R - 1] = '_';
                    chs[w / 2 + 3 * hex.Q, h / 2 + hex.Q + 2 * hex.R - 1] = '_';
                }

                // NE wall
                if (walls[2])
                    chs[w / 2 + 3 * hex.Q + 1, h / 2 + hex.Q + 2 * hex.R] = '\\';

                // Marking
                var m = inf.Markings.Get(hex, Hexamaze.Marking.None);
                if (m != Hexamaze.Marking.None)
                {
                    chs[w / 2 + 3 * hex.Q - 1, h / 2 + hex.Q + 2 * hex.R] = " (/\\<|{"[(int) m];
                    chs[w / 2 + 3 * hex.Q, h / 2 + hex.Q + 2 * hex.R] = " )\\/|>}"[(int) m];
                }
            }
            for (var y = 0; y < h; y++)
                Console.WriteLine(Enumerable.Range(0, w).Select(x => chs[x, y]).JoinString());

            Console.WriteLine($"{inf.Markings.Count(kvp => kvp.Value != Hexamaze.Marking.None)} markings");
            Console.WriteLine($"Took {(DateTime.UtcNow - start).TotalSeconds:0.#}sec.");
            Console.WriteLine("Done.");
            Console.ReadLine();
            return 0;
        }
    }
}
