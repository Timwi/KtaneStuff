using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Xml.Linq;
using KtaneStuff.Modeling;
using RT.Json;
using RT.KitchenSink;
using RT.Serialization;
using RT.Util;
using RT.Util.ExtensionMethods;
using RT.Util.Geometry;

namespace KtaneStuff
{
    using static Md;

    static class Puzzword
    {
        public static void CreateSymbolTextures()
        {
            var svg = @"D:\c\KTANE\KtaneStuff\DataFiles\Puzzword\Symbols.svg";
            var threads = new List<Thread>();
            void export(string area, int size, string filename)
            {
                var thread = new Thread(() =>
                {
                    var cmd = $@"D:\Inkscape\inkscape.exe ""--export-png=D:\c\KTANE\Puzzword\Assets\Symbols\{filename}"" --export-area={area} --export-width={size} --export-height={size} ""{svg}""";
                    Console.WriteLine(cmd);
                    CommandRunner.RunRaw(cmd).Go();
                });
                while (threads.Count > Environment.ProcessorCount)
                {
                    threads[0].Join();
                    threads.RemoveAt(0);
                }
                thread.Start();
                threads.Add(thread);
            }
            const int f = 50;
            for (var i = 0; i < 8; i++)
                export($"{10 * i + 1}:91:{10 * i + 9}:99", 8 * f, $"Symbol_{i}_1.png");
            for (var i = 0; i < 8; i++)
                export($"{10 * i + 3}:83:{10 * i + 7}:87", 4 * f, $"Symbol_{i}_2.png");
            for (var i = 0; i < 5; i++)
                export($"{10 * i + 2}:72:{10 * i + 8}:78", 6 * f, $"Symbol_{2 * i}_3.png");
            for (var i = 0; i < 5; i++)
                export($"{10 * i + 2}:62:{10 * i + 8}:68", 6 * f, $"Symbol_{2 * i + 1}_3.png");
            for (var i = 0; i < 4; i++)
                export($"{10 * i + 4}:54:{10 * i + 6}:56", 2 * f, $"Symbol_{i}_4.png");

            foreach (var thread in threads)
                thread.Join();
        }

        public static void DoFrontPlateTriangulation()
        {
            var polygons3d = ClassifyJson.Deserialize<List<List<Pt>>>(JsonList.Parse(@"[[
{""X"":-0.854439,""Y"":0.15,""Z"":0.854439}  ,
{""X"":-0.256273,""Y"":0.15,""Z"":0.854439}  ,
{""X"":0.256273,""Y"":0.15,""Z"":0.854439}   ,
{""X"":0.854439,""Y"":0.15,""Z"":0.854439}   ,
{""X"":0.854439,""Y"":0.15,""Z"":0.256271}   ,
{""X"":0.854439,""Y"":0.15,""Z"":-0.256271}  ,
{""X"":0.861069,""Y"":0.15,""Z"":-0.396061}  ,
{""X"":0.784569,""Y"":0.15,""Z"":-0.472279}  ,
{""X"":0.672195,""Y"":0.15,""Z"":-0.46653}   ,
{""X"":0.524693,""Y"":0.15,""Z"":-0.545002}  ,
{""X"":0.443619,""Y"":0.15,""Z"":-0.693596}  ,
{""X"":0.444206,""Y"":0.15,""Z"":-0.774025}  ,
{""X"":0.364376,""Y"":0.15,""Z"":-0.854439}  ,
{""X"":0.256273,""Y"":0.15,""Z"":-0.854439}  ,
{""X"":-0.256271,""Y"":0.15,""Z"":-0.854439} ,
{""X"":-0.854439,""Y"":0.15,""Z"":-0.854439} ,
{""X"":-0.854439,""Y"":0.15,""Z"":-0.256271} ,
{""X"":-0.854439,""Y"":0.15,""Z"":0.256271}
]]"));
            var polygons = polygons3d.Select(poly => poly.Select(pt => p(pt.X, pt.Z)).ToList()).ToList();

            foreach (var path in XDocument.Parse(File.ReadAllText(@"D:\c\KTANE\KtaneStuff\DataFiles\Puzzword\Design.svg")).Root.Descendants().Where(e => e.Name.LocalName == "path"))
                foreach (var polygon in DecodeSvgPath.Do(path.AttributeI("d").Value, .1))
                    polygons.Add(polygon.Select(pt => pt / 100 - new PointD(0, 1)).ToList());

            //var result = polygons.Triangulate();

            var result = Triangulate.DelaunayConstrained(
                polygons.SelectMany(poly => poly),
                polygons.SelectMany(poly => poly.SelectConsecutivePairs(closed: true, selector: (p1, p2) => new EdgeD(p1, p2))));

            var json = JsonDict.Parse(File.ReadAllText(@"D:\c\KTANE\KtaneStuff\DataFiles\Puzzword\MeshEdit.Settings.bak.json"));
            foreach (var polygon in result)
            {
                var face = new JsonList();
                foreach (var p in polygon.Vertices)
                {
                    face.Add(new JsonDict {
                        { "Location", new JsonDict { { "X", p.X }, { "Y", .15 }, { "Z", p.Y } } },
                        { "Normal", new JsonDict { { "X", 0 }, { "Y", 1 }, { "Z", 0 } } }
                    });
                    //Console.WriteLine(face["Vertices"].GetList().Last()["Location"]);
                }
                //Console.WriteLine();
                json["Faces"].Insert(0, new JsonDict { { "Vertices", face } });
            }
            File.WriteAllText(@"C:\Users\Timwi\AppData\Roaming\MeshEdit\MeshEdit.Settings.json", json.ToString());
        }
    }
}