using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Xml.Linq;
using KtaneStuff.Modeling;
using RT.Util.ExtensionMethods;
using RT.Util.Geometry;

namespace KtaneStuff
{
    using static Md;

    static class Polygons
    {
        public static void GenerateSvgs()
        {
            foreach (var objFile in new DirectoryInfo(@"D:\Daten\Upload\KTANE\TasThing\Polygons\objs").EnumerateFiles("*.obj"))
            {
                var (name, polygons) = Md.ParseObjFile(objFile.FullName);
                Console.WriteLine(name);

                File.WriteAllText(Path.Combine(@"D:\Daten\Upload\KTANE\TasThing\Polygons\svgs", Path.GetFileNameWithoutExtension(objFile.FullName) + ".svg"), $@"
                    <svg xmlns=""http://www.w3.org/2000/svg"" viewBox=""-1.1 -1.1 2.2 2.2"">
                        {polygons.Select(polygon => $@"<path d=""M {polygon.Select(p => $"{p.X} {-p.Z}").JoinString(" L ")}"" />").JoinString()}
                    </svg>
                ");
            }
        }

        public static void ExtractSvgPaths()
        {
            var conditions = @"The serial number contains a vowel.
The serial number does not contain a vowel.
The serial number’s last digit is even.
The serial number’s last digit is odd.
There is a lit indicator that shares a letter with the serial number.
There is an unlit indicator that shares a letter with the serial number.
An empty port plate is present.
A parallel port is present.
A serial port is present.
A PS/2 port is present.
An RJ-45 port is present.
A Stereo RCA port is present.
A DVI port is present.
An SND indicator is present
A CLR indicator is present
A CAR indicator is present
An IND indicator is present
An FRQ indicator is present
An SIG indicator is present
An NSA indicator is present
An MSA indicator is present
A TRN indicator is present
A BOB indicator is present
An FRK indicator is present
An NLL indicator is present
The number of batteries is even.
The number of batteries is odd.
The number of battery holders is even.
The number of battery holders is odd.
The number of ports is even.
The number of ports is odd.
The number of indicators is even.
The number of indicators is odd.
The bomb was started on a weekend (Saturday or Sunday).
A “Blind Alley”, “Tap Code”, “Braille” or “A Mistake” module is present.".Replace("\r", "").Split("\n");

            var sb = new StringBuilder();
            var i = 0;
            foreach (var objFile in new DirectoryInfo(@"D:\Daten\Upload\KTANE\TasThing\Polygons\svgs-modified").EnumerateFiles("*.svg"))
            {
                var xml = XDocument.Parse(File.ReadAllText(objFile.FullName));
                var path = xml.Root.ElementI("path");
                var transform = path.AttributeI("transform");
                sb.AppendLine($"<tr><td><svg class='polygon {(Path.GetFileNameWithoutExtension(objFile.Name))}' viewBox='{(transform != null ? "0.05 0.05 2.1 2.1" : "-1.05 -1.05 2.1 2.1")}'><path d='{path.AttributeI("d").Value}' /></svg></td><td>{conditions[i]}</td></tr>");
                i++;
            }
            Clipboard.SetText(sb.ToString());
        }
    }
}