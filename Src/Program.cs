﻿using System;
using System.Collections.Generic;
using System.Numerics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using RT.PostBuild;
using RT.Util.ExtensionMethods;

[assembly: AssemblyCopyright("Copyright © Timwi 2016–2022")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]
[assembly: ComVisible(false)]
[assembly: Guid("95055383-2e25-42be-97b7-e1411a695e1d")]

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


            Souvenir.UpdateJs();


            Console.WriteLine("Done.");
            Console.ReadLine();
            return 0;
        }
    }
}
