using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using CsQuery;
using KtaneStuff.Modeling;
using RT.Util;
using RT.Util.Consoles;
using RT.Util.Drawing;
using RT.Util.ExtensionMethods;

namespace KtaneStuff
{
    using System.Text;
    using RT.Util.Serialization;
    using static Md;

    static class SolidMaze
    {
        public static void ForeachSolid(Action<(string filePath, string solidName, IEnumerable<Pt[]> solid, object lockObj)> action, int maxSimultaneous = 4)
        {
            var lockObj = new object();
            new DirectoryInfo(@"D:\c\KTANE\KtaneStuff\DataFiles\SolidMaze\Txt").GetFiles("*.txt").ParallelForEach(maxSimultaneous, file =>
            {
                var (name, faces) = Parse(File.ReadAllText(file.FullName));
                action((file.FullName, name, faces, lockObj));
            });
        }

        public static void GenerateSolidObjFile(string filePath, string solidName, IEnumerable<Pt[]> solid)
        {
            File.WriteAllText($@"D:\c\KTANE\SolidMaze\Assets\Models\Solids\{Path.GetFileNameWithoutExtension(filePath)}.obj", GenerateObjFile(solid, solidName, AutoNormal.Flat));
        }

        public static void DoEverything()
        {
            var paths = new List<string>();
            ForeachSolid(maxSimultaneous: 1, action: x =>
            {
                if (x.solid.Count() < 40 || x.solid.Count() > 60)
                    return;
                var path = GenerateNet(x.filePath, x.solidName, x.solid, x.lockObj);
                paths.Add(path);
                GenerateSolidObjFile(x.filePath, x.solidName, x.solid);
            });
            Console.WriteLine(paths.Count);
            File.WriteAllText(@"D:\c\ktane\ktanestuff\DataFiles\SolidMaze\index.html", $@"
<html>
    <head>
        <title>Solid Nets</title>
        <style>
            .graphic {{ float: left; width: 500px; }}
            .graphic img {{ width: 500px; }}
        </style>
    </head>
    <body>
        {paths.Select(p => $"<div class='graphic'><img src='Svg/{Path.GetFileName(p)}' /></div>").JoinString()}
    </body>
</html>");
        }

        enum EdgeConnection { None, Portal, NetWall, Net }

        public static string GenerateNet(string filePath, string solidName, IEnumerable<Pt[]> solid, object lockObj)
        {
            // Numbers closer than this are considered equal
            const double closeness = .00001;

            var svgPath = $@"D:\c\KTANE\KtaneStuff\DataFiles\SolidMaze\Svg\{Path.GetFileNameWithoutExtension(filePath)}.svg";
            var jsonPath = $@"D:\c\KTANE\KtaneStuff\DataFiles\SolidMaze\Json\{Path.GetFileNameWithoutExtension(filePath)}.json";

            var svg = new StringBuilder();
            var faces = solid.ToArray();

            // face, edge, face, edge
            Dictionary<int, Dictionary<int, Dictionary<int, Dictionary<int, EdgeConnection>>>> connectionMap;
            try { connectionMap = ClassifyJson.DeserializeFile<Dictionary<int, Dictionary<int, Dictionary<int, Dictionary<int, EdgeConnection>>>>>(jsonPath); }
            catch { connectionMap = new Dictionary<int, Dictionary<int, Dictionary<int, Dictionary<int, EdgeConnection>>>>(); }

            void setConnection(int fromFaceIx, int fromEdgeIx, int toFaceIx, int toEdgeIx, EdgeConnection value)
            {
                if (fromFaceIx > toFaceIx)
                    connectionMap.AddSafe(toFaceIx, toEdgeIx, fromFaceIx, fromEdgeIx, value);
                else
                    connectionMap.AddSafe(fromFaceIx, fromEdgeIx, toFaceIx, toEdgeIx, value);
            }

            // Restricted variable scope
            {
                var vx = faces[0][0];
                // Put first vertex at origin
                for (int i = 0; i < faces.Length; i++)
                    for (int j = 0; j < faces[i].Length; j++)
                        faces[i][j] = faces[i][j] - vx;

                // Rotate so that first face is on the X/Y plane
                var normal = (faces[0][2] - faces[0][1]) * (faces[0][0] - faces[0][1]);
                var rot = normal.Normalize() * pt(0, 0, 1);
                if (Math.Abs(rot.X) < closeness && Math.Abs(rot.Y) < closeness && Math.Abs(rot.Z) < closeness)
                {
                    // the face is already on the X/Y plane
                }
                else
                {
                    var newFaces1 = faces.Select(f => f.Select(p => p.Rotate(pt(0, 0, 0), rot, arcsin(rot.Length))).ToArray()).ToArray();
                    var newFaces2 = faces.Select(f => f.Select(p => p.Rotate(pt(0, 0, 0), rot, -arcsin(rot.Length))).ToArray()).ToArray();
                    faces = newFaces1[0].Sum(p => p.Z * p.Z) < newFaces2[0].Sum(p => p.Z * p.Z) ? newFaces1 : newFaces2;
                }
            }

            var polyDraws = new List<Action>();
            var isAbove = faces.Sum(f => f.Sum(p => p.Z)) > 0;

            var q = new Queue<(int newFaceIx, int newEdgeIx, int oldFaceIx, int oldEdgeIx, Pt[][] solid)>();
            var done = new HashSet<int>();
            var polygons = new PointF[faces.Length][];

            q.Enqueue((0, -1, -1, -1, faces));
            while (q.Count > 0)
            {
                var (fromFaceIx, cameFromEdgeIx, oldFaceIx, oldEdgeIx, rotatedSolid) = q.Dequeue();
                if (!done.Add(fromFaceIx))
                {
                    setConnection(oldFaceIx, oldEdgeIx, fromFaceIx, cameFromEdgeIx, EdgeConnection.Portal);
                    continue;
                }
                polygons[fromFaceIx] = rotatedSolid[fromFaceIx].Select(pt => p(pt.X, pt.Y).ToPointF()).ToArray();

                bool sufficientlyClose(Pt p1, Pt p2) => Math.Abs(p1.X - p2.X) < closeness && Math.Abs(p1.Y - p2.Y) < closeness && Math.Abs(p1.Z - p2.Z) < closeness;
                for (int fromEdgeIx = 0; fromEdgeIx < rotatedSolid[fromFaceIx].Length; fromEdgeIx++)
                {
                    int toEdgeIx = -1;
                    // Find another face that has the same edge
                    var toFaceIx = rotatedSolid.IndexOf(fc =>
                    {
                        toEdgeIx = fc.IndexOf(p => sufficientlyClose(p, rotatedSolid[fromFaceIx][(fromEdgeIx + 1) % rotatedSolid[fromFaceIx].Length]));
                        return toEdgeIx != -1 && sufficientlyClose(fc[(toEdgeIx + 1) % fc.Length], rotatedSolid[fromFaceIx][fromEdgeIx]);
                    });
                    if (toEdgeIx == -1 || toFaceIx == -1)
                        Debugger.Break();

                    // Rotate about the edge so that the new face is on the X/Y plane (i.e. “roll” the solid)
                    var toFace = rotatedSolid[toFaceIx];
                    var normal = (toFace[2] - toFace[1]) * (toFace[0] - toFace[1]);
                    var rot = normal.Normalize() * pt(0, 0, 1);
                    var asin = arcsin(rot.Length);
                    var newSolid = Ut.NewArray(
                        rotatedSolid.Select(face => face.Select(p => p.Rotate(toFace[(toEdgeIx + 1) % toFace.Length], toFace[toEdgeIx], asin)).ToArray()).ToArray(),
                        rotatedSolid.Select(face => face.Select(p => p.Rotate(toFace[(toEdgeIx + 1) % toFace.Length], toFace[toEdgeIx], -asin)).ToArray()).ToArray(),
                        rotatedSolid.Select(face => face.Select(p => p.Rotate(toFace[(toEdgeIx + 1) % toFace.Length], toFace[toEdgeIx], 180 + asin)).ToArray()).ToArray(),
                        rotatedSolid.Select(face => face.Select(p => p.Rotate(toFace[(toEdgeIx + 1) % toFace.Length], toFace[toEdgeIx], 180 - asin)).ToArray()).ToArray())
                        .Where(sld => sld.All(fc => fc.All(p => isAbove ? p.Z > -.001 : p.Z < .001)))
                        .MinElement(sld => sld[toFaceIx].Sum(p => p.Z * p.Z));
                    q.Enqueue((toFaceIx, toEdgeIx, fromFaceIx, fromEdgeIx, newSolid));
                    setConnection(toFaceIx, toEdgeIx, fromFaceIx, fromEdgeIx, EdgeConnection.Net);
                }
            }

            //foreach (var (f1, e1, f2, e2) in connectionMap)
            //{
            //    var color = Color.FromArgb(Rnd.Next(0, 192), Rnd.Next(0, 192), Rnd.Next(0, 192));
            //    var p11 = polygons[f1][e1];
            //    var p12 = polygons[f1][(e1 + 1) % polygons[f1].Length];
            //    var p1m = new PointF((p11.X + p12.X) / 2, (p11.Y + p12.Y) / 2);
            //    var p1c = new PointF(p1m.X - (p1m.Y - p11.Y) / 2, p1m.Y + (p1m.X - p11.X) / 2);
            //    var p21 = polygons[f2][e2];
            //    var p22 = polygons[f2][(e2 + 1) % polygons[f2].Length];
            //    var p2m = new PointF((p21.X + p22.X) / 2, (p21.Y + p22.Y) / 2);
            //    var p2c = new PointF(p2m.X - (p2m.Y - p21.Y) / 2, p2m.Y + (p2m.X - p21.X) / 2);

            //    g.DrawBezier(new Pen(color, 1 / 50f), p1m, p1c, p2c, p2m);
            //}

            for (int ix = 0; ix < polygons.Length; ix++)
            {
                svg.Append($"<path fill='#cef' stroke='#000' stroke-width='.05' stroke-linejoin='round' d='M {polygons[ix].Select(p => $"{p.X},{p.Y}").JoinString(" ")} z' />");
                var middle = new PointF(polygons[ix].Sum(p => p.X) / polygons[ix].Length, polygons[ix].Sum(p => p.Y) / polygons[ix].Length);
                svg.Append($"<text x='{middle.X}' y='{middle.Y}' text-anchor='middle' alignment-baseline='middle' fill='#000' font-size='.5'>{ix}</text>");

                //g.FillPolygon(new SolidBrush(Color.CornflowerBlue), polygons[ix]);
                //g.DrawPolygon(new Pen(Color.Black, 1 / 30f), polygons[ix]);
                //g.DrawString(ix.ToString(), new Font("Calibri", .2f, FontStyle.Regular), Brushes.Black, new PointF(polygons[ix].Sum(p => p.X) / polygons[ix].Length, polygons[ix].Sum(p => p.Y) / polygons[ix].Length), new StringFormat { LineAlignment = StringAlignment.Center, Alignment = StringAlignment.Center });
            }

            //Console.WriteLine(connectionMap.JoinString("\n"));
            var minX = polygons.Min(poly => poly.Min(p => p.X));
            var minY = polygons.Min(poly => poly.Min(p => p.Y));
            File.WriteAllText(svgPath, $"<svg xmlns='http://www.w3.org/2000/svg' viewBox='{minX - .1} {minY - .1} {polygons.Max(poly => poly.Max(p => p.X)) - minX + .2} {polygons.Max(poly => poly.Max(p => p.Y)) - minY + .2}'>{svg.ToString()}</svg>");
            ClassifyJson.SerializeToFile(connectionMap, jsonPath);

            lock (lockObj)
                Console.WriteLine($" • {solidName} done.");
            return svgPath;
        }

        public static void DownloadFiles()
        {
            foreach (var htmlFile in Ut.NewArray(
                "http://dmccooey.com/polyhedra/Catalan.html",
                "http://dmccooey.com/polyhedra/Archimedean.html",
                "http://dmccooey.com/polyhedra/Platonic.html",
                "http://dmccooey.com/polyhedra/Propellor.html",
                "http://dmccooey.com/polyhedra/Hull.html",
                "http://dmccooey.com/polyhedra/RectifiedArchimedean.html",
                "http://dmccooey.com/polyhedra/TruncatedCatalan.html",
                "http://dmccooey.com/polyhedra/Chamfer.html",
                "http://dmccooey.com/polyhedra/Derived.html"
            ))
            {
                var response = new HClient().Get(htmlFile);
                if (response.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    ConsoleUtil.WriteLine("{0/Red} ({1/DarkRed} {2/DarkRed})".Color(ConsoleColor.DarkRed).Fmt(htmlFile, (int) response.StatusCode, response.StatusCode));
                    continue;
                }
                ConsoleUtil.WriteLine(htmlFile.Color(ConsoleColor.Green));

                var doc = CQ.CreateDocument(response.DataString);
                var lockObj = new object();
                doc["a"].ParallelForEach(elem =>
                {
                    var href = elem.Attributes["href"];
                    if (href == null || href.StartsWith("http") || !href.EndsWith(".html"))
                        return;
                    var filename = href.Replace(".html", ".txt");
                    var resp = new HClient().Get($"http://dmccooey.com/polyhedra/{filename}");
                    if (resp.StatusCode != System.Net.HttpStatusCode.OK)
                    {
                        lock (lockObj)
                            ConsoleUtil.WriteLine(" • {0/Red} ({1/DarkRed} {2/DarkRed})".Color(ConsoleColor.DarkGray).Fmt(htmlFile, (int) resp.StatusCode, resp.StatusCode));
                        return;
                    }
                    lock (lockObj)
                    {
                        File.WriteAllText($@"D:\c\KTANE\KtaneStuff\DataFiles\SolidMaze\Txt\{filename}", resp.DataString);
                        ConsoleUtil.WriteLine(" • {0/DarkGreen}".Color(ConsoleColor.DarkGray).Fmt(href));
                    }
                });
            }
        }

        public static (string name, IEnumerable<Pt[]> faces) Parse(string data)
        {
            var lines = data.Replace("\r", "").Split('\n');
            var nameMatch = Regex.Match(lines[0], @"^(.*?)(?: with|$)");
            var name = nameMatch.Groups[1].Value.Replace(" (canonical)", "");
            var matches = lines.Skip(1)
                .Select(line => new
                {
                    Line = line,
                    CoordinateMatch = Regex.Match(line, @"^C(\d+) *= *(-?\d*\.?\d+) *(?:=|$)"),
                    VertexMatch = Regex.Match(line, @"^V(\d+) *= *\( *((?<m1>-?)C(?<c1>\d+)|(?<n1>-?\d*\.\d+)) *, *((?<m2>-?)C(?<c2>\d+)|(?<n2>-?\d*\.\d+)) *, *((?<m3>-?)C(?<c3>\d+)|(?<n3>-?\d*\.\d+)) *\) *$"),
                    FaceMatch = Regex.Match(line, @"^ *\{ *(\d+ *(, *\d+ *)*)\} *$")
                });
            var coords = matches.Where(m => m.CoordinateMatch.Success).ToDictionary(m => int.Parse(m.CoordinateMatch.Groups[1].Value), m => double.Parse(m.CoordinateMatch.Groups[2].Value));
            var negCoords = coords.ToDictionary(p => p.Key, p => -p.Value);
            double resolveCoord(Group minus, Group coordIx, Group number) => number.Success ? double.Parse(number.Value) : (minus.Value == "-" ? negCoords : coords)[int.Parse(coordIx.Value)];
            var vertices = matches.Where(m => m.VertexMatch.Success).ToDictionary(m => int.Parse(m.VertexMatch.Groups[1].Value), m => m.VertexMatch.Groups.Apply(g => pt(resolveCoord(g["m1"], g["c1"], g["n1"]), resolveCoord(g["m2"], g["c2"], g["n2"]), resolveCoord(g["m3"], g["c3"], g["n3"]))));
            var faces = matches.Where(m => m.FaceMatch.Success).Select(m => m.FaceMatch.Groups[1].Value.Split(',').Select(str => vertices[int.Parse(str.Trim())]).ToArray()).ToArray();
            return (name, faces);
        }
    }
}