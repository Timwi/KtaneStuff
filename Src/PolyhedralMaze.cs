using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using CsQuery;
using KtaneStuff.Modeling;
using RT.Servers;
using RT.Util;
using RT.Util.Consoles;
using RT.Util.ExtensionMethods;
using RT.Util.Geometry;
using RT.Util.Json;
using RT.Util.Serialization;

namespace KtaneStuff
{
    using static Md;

    static class PolyhedralMaze
    {
        private static string _masterJsonPath = @"D:\c\KTANE\KtaneStuff\DataFiles\PolyhedralMaze\Master.json";
        private static object _lockObj = new object();

        public enum Adjacency
        {
            /// <summary>The faces are NOT adjacent in the net, nor can you walk from one to the other.</summary>
            None,
            /// <summary>The faces are adjacent in the net, but you cannot walk from one to the other (it’s a wall).</summary>
            NetWall,
            /// <summary>The faces are adjacent in the net and you cannot walk from one to the other.</summary>
            Net,
            /// <summary>
            ///     The faces are NOT adjacent in the net, but you cannot walk from one to the other. The connection is
            ///     indicated by a letter.</summary>
            Portal,
            /// <summary>
            ///     The faces are NOT adjacent in the net, but you cannot walk from one to the other. The connection is
            ///     indicated by a curved line.</summary>
            Curve
        }

        public sealed class AdjacencyInfo
        {
            public int FromFace { get; private set; }
            public int FromEdge { get; private set; }
            public int ToFace { get; private set; }
            public int ToEdge { get; private set; }
            public Adjacency Adjacency { get; set; }
            public AdjacencyInfo(int fromFace, int fromEdge, int toFace, int toEdge, Adjacency adjacency)
            {
                FromFace = fromFace;
                FromEdge = fromEdge;
                ToFace = toFace;
                ToEdge = toEdge;
                Adjacency = adjacency;
            }
            private AdjacencyInfo() { }    // Classify
        }

        public sealed class PolyhedronInfo
        {
            public string FileCompatibleName { get; private set; }
            public string ReadableName { get; private set; }
            public Pt[][] Faces { get; private set; }

            public AdjacencyInfo[] Adjacencies { get; set; }
            public string SvgId { get; set; }
            public double[] FontSizes { get; set; }
            public double Rotation { get; set; }
            public double XOffset { get; set; }
            public double YOffset { get; set; }

            public PolyhedronInfo(string fileCompatibleName, string readableName, Pt[][] faces)
            {
                FileCompatibleName = fileCompatibleName;
                ReadableName = readableName;
                Faces = faces;
            }
            private PolyhedronInfo() { }    // Classify
            public void GenerateObjFile()
            {
                File.WriteAllText($@"D:\c\KTANE\PolyhedralMaze\Assets\Models\Polyhedra\{FileCompatibleName}.obj", Md.GenerateObjFile(Faces, ReadableName, AutoNormal.Flat));
            }

            internal char GetPortalLetter(int faceIx, int edgeIx)
            {
                char ch = 'A';
                for (int i = 0; i < Adjacencies.Length; i++)
                {
                    if (Adjacencies[i].FromFace == faceIx && Adjacencies[i].FromEdge == edgeIx)
                        return ch;
                    if (Adjacencies[i].ToFace == faceIx && Adjacencies[i].ToEdge == edgeIx)
                        return ch;
                    if (Adjacencies[i].Adjacency == Adjacency.Portal)
                        ch++;
                }
                Debugger.Break();
                throw new InvalidOperationException();
            }
        }

        public static void GeneratePolyhedronInfos()
        {
            List<PolyhedronInfo> polyhedra;
            try { polyhedra = ClassifyJson.DeserializeFile<List<PolyhedronInfo>>(_masterJsonPath); }
            catch { polyhedra = new List<PolyhedronInfo>(); }

            foreach (var file in new DirectoryInfo(@"D:\c\KTANE\KtaneStuff\DataFiles\PolyhedralMaze\Txt").GetFiles("*.txt"))
            {
                var (name, faces) = Parse(File.ReadAllText(file.FullName));
                if (faces.Length < 40 || faces.Length > 60)
                    continue;

                var info = polyhedra.FirstOrDefault(si => si.ReadableName == name);
                if (info == null)
                {
                    info = new PolyhedronInfo(Path.GetFileNameWithoutExtension(file.Name), name, faces);
                    polyhedra.Add(info);
                }
            }

            ClassifyJson.SerializeToFile(polyhedra, _masterJsonPath);
        }

        public static void RunServer()
        {
            List<PolyhedronInfo> polyhedra;
            try { polyhedra = ClassifyJson.DeserializeFile<List<PolyhedronInfo>>(_masterJsonPath); }
            catch { polyhedra = new List<PolyhedronInfo>(); }

            HttpResponse mainPage(HttpRequest req)
            {
                return HttpResponse.Html(File.ReadAllBytes(@"D:\c\KTANE\KtaneStuff\DataFiles\PolyhedralMaze\ManualTemplate.html"));
            }

            var server = new HttpServer(8991)
            {
                Handler = new UrlResolver(
                    new UrlMapping(path: "/", specificPath: true, handler: req => mainPage(req)),
                    new UrlMapping(path: "/css", handler: new FileSystemHandler(@"D:\c\KTANE\Public\HTML\css").Handle),
                    new UrlMapping(path: "/js", handler: new FileSystemHandler(@"D:\c\KTANE\Public\HTML\js").Handle),
                    new UrlMapping(path: "/font", handler: new FileSystemHandler(@"D:\c\KTANE\Public\HTML\font").Handle),
                    new UrlMapping(path: "/img", handler: new FileSystemHandler(@"D:\c\KTANE\Public\HTML\img").Handle),
                    new UrlMapping(path: "/websocket", handler: req => HttpResponse.WebSocket(new WebSocketImpl(polyhedra)))
                ).Handle
            };
            server.StartListening(blocking: true);
        }

        public sealed class WebSocketImpl : WebSocket
        {
            private List<PolyhedronInfo> _polyhedra;
            private BoundingBoxD[] _boundingBoxes;

            public WebSocketImpl(List<PolyhedronInfo> polyhedra) { _polyhedra = polyhedra; }

            protected override void onBeginConnection()
            {
                _boundingBoxes = new BoundingBoxD[_polyhedra.Count];
                for (int i = 0; i < _polyhedra.Count; i++)
                {
                    var polyhedron = _polyhedra[i];
                    SendPolyhedronSelect(polyhedron);

                    if (polyhedron.SvgId != null)
                        _boundingBoxes[i] = GenerateNet(polyhedron);
                }

                SendViewBoxes();
            }

            private void SendViewBoxes()
            {
                var boundingBoxes = new Dictionary<string, BoundingBoxD>();
                for (int i = 0; i < _polyhedra.Count; i++)
                {
                    if (_polyhedra[i].SvgId == null)
                        continue;
                    if (boundingBoxes.TryGetValue(_polyhedra[i].SvgId, out var box))
                    {
                        box.AddBoundingBox(_boundingBoxes[i]);
                        boundingBoxes[_polyhedra[i].SvgId] = box;
                    }
                    else
                        boundingBoxes[_polyhedra[i].SvgId] = _boundingBoxes[i];
                }

                foreach (var kvp in boundingBoxes)
                    SendMessage(new JsonDict { { "svg", kvp.Key }, { "viewBox", $"{kvp.Value.Xmin - .5} {kvp.Value.Ymin - .5} {kvp.Value.Width + 1} {kvp.Value.Height + 1}" } });
            }

            private void SendPolyhedronSelect(PolyhedronInfo polyhedron)
            {
                SendMessage(new JsonDict { { "polyhedron", polyhedron.FileCompatibleName }, { "select", $"[{polyhedron.SvgId ?? "absent"}] {polyhedron.ReadableName}" } });
            }

            protected override void onTextMessageReceived(string msg)
            {
                var json = JsonValue.Parse(msg);
                var edgeData = json.ContainsKey("EdgeData") && JsonValue.TryParse(json["EdgeData"].GetString(), out JsonValue result) ? result : null;
                switch (json["Command"].GetString())
                {
                    case "add": Update(json["Polyhedron"].GetString(), polyhedron => { polyhedron.SvgId = json["SvgId"].GetString(); SendPolyhedronSelect(polyhedron); }); break;
                    case "rotate": Update(edgeData["polyhedron"].GetString(), polyhedron => { polyhedron.Rotation += json["Amount"].GetDouble(NumericConversionOptions.AllowConversionFromString); }); break;
                    case "move": Update(edgeData["polyhedron"].GetString(), polyhedron => { polyhedron.XOffset += json["XAmount"].GetDouble(NumericConversionOptions.AllowConversionFromString); polyhedron.YOffset += json["YAmount"].GetDouble(NumericConversionOptions.AllowConversionFromString); }); break;

                    case "convert-to-portal": convertTo(Adjacency.Portal, edgeData); break;
                    case "convert-to-curve": convertTo(Adjacency.Curve, edgeData); break;
                    case "make-connected": convertTo(Adjacency.Net, edgeData); break;

                    default:
                        break;
                }
            }

            private void Update(string polyhedronId, Action<PolyhedronInfo> action)
            {
                var polyIx = _polyhedra.IndexOf(p => p.FileCompatibleName == polyhedronId);
                var polyhedron = _polyhedra[polyIx];
                action(polyhedron);
                _boundingBoxes[polyIx] = GenerateNet(polyhedron);
                SendViewBoxes();
                save();
            }

            private void convertTo(Adjacency adj, JsonValue edgeData)
            {
                var pIx = _polyhedra.IndexOf(poly => poly.FileCompatibleName == edgeData["polyhedron"].GetString());
                var polyhedron = _polyhedra[pIx];
                var faceIx = edgeData["face"].GetInt();
                var edgeIx = edgeData["edge"].GetInt();
                var adjInf = polyhedron.Adjacencies.Single(ad => (ad.FromFace == faceIx && ad.FromEdge == edgeIx) || (ad.ToFace == faceIx && ad.ToEdge == edgeIx));
                adjInf.Adjacency = adj;
                SendMessage(new JsonDict { { "svg", polyhedron.SvgId }, { "tag", "delete" }, { "className", $"poly-{polyhedron.FileCompatibleName}-edge" } });
                _boundingBoxes[pIx] = GenerateNet(polyhedron);
                SendViewBoxes();
                save();
            }

            private BoundingBoxD GenerateNet(PolyhedronInfo polyhedron)
            {
                void sendPath(string id, string className, string edgeData, string data, double? strokeWidth = null, string fill = null)
                {
                    var dict = new JsonDict { { "svg", polyhedron.SvgId }, { "id", $"poly-{polyhedron.FileCompatibleName}-{id}" }, { "className", $"poly-{polyhedron.FileCompatibleName}-{className}" }, { "tag", "path" }, { "attr", new JsonDict {
                        { "d", data },
                        { "stroke", strokeWidth == null ? "none" : "black" },
                        { "stroke-linejoin", "round" },
                        { "stroke-width", strokeWidth ?? 0 },
                        { "fill", fill ?? "none" }
                    } } };
                    if (edgeData != null)
                        dict["edgeData"] = edgeData;
                    SendMessage(dict);
                }

                void sendText(string id, string className, double fontSize, double x, double y, string content, string fill, string edgeData = null)
                {
                    var dict = new JsonDict { { "svg", polyhedron.SvgId }, { "id", $"poly-{polyhedron.FileCompatibleName}-{id}" }, { "className", $"poly-{polyhedron.FileCompatibleName}-{className}" }, { "tag", "text" },
                        { "attr", new JsonDict { { "x", x }, { "y", y + fontSize * .35 }, { "text-anchor", "middle" }, { "alignment-baseline", "middle" }, { "fill", fill }, { "font-size", fontSize } } },
                        { "content", content }
                    };
                    if (edgeData != null)
                        dict.Add("edgeData", edgeData);
                    SendMessage(dict);
                }

                // Numbers closer than this are considered equal
                const double closeness = .00001;
                bool sufficientlyClose(Pt p1, Pt p2) => Math.Abs(p1.X - p2.X) < closeness && Math.Abs(p1.Y - p2.Y) < closeness && Math.Abs(p1.Z - p2.Z) < closeness;

                var anyChanges = false;

                if (polyhedron.FontSizes == null)
                {
                    polyhedron.FontSizes = Ut.NewArray(polyhedron.Faces.Length, _ => .5);
                    anyChanges = true;
                }

                // Take a full copy of this
                var faces = polyhedron.Faces.Select(face => face.ToArray()).ToArray();

                void setConnection(int fromFaceIx, int fromEdgeIx, int toFaceIx, int toEdgeIx, Adjacency value)
                {
                    if (fromFaceIx > toFaceIx)
                    {
                        var t = fromFaceIx;
                        fromFaceIx = toFaceIx;
                        toFaceIx = t;
                        t = fromEdgeIx;
                        fromEdgeIx = toEdgeIx;
                        toEdgeIx = t;
                    }

                    var adjIx = polyhedron.Adjacencies == null ? -2 : polyhedron.Adjacencies.IndexOf(ad => ad.FromFace == fromFaceIx && ad.FromEdge == fromEdgeIx && ad.ToFace == toFaceIx && ad.ToEdge == toEdgeIx);
                    if (adjIx >= 0 && polyhedron.Adjacencies[adjIx].Adjacency == value)
                        return;
                    var newInf = new AdjacencyInfo(fromFaceIx, fromEdgeIx, toFaceIx, toEdgeIx, value);
                    if (adjIx == -2)
                        polyhedron.Adjacencies = new[] { newInf };
                    else if (adjIx == -1)
                        polyhedron.Adjacencies = polyhedron.Adjacencies.Concat(new[] { newInf }).ToArray();
                    else
                        polyhedron.Adjacencies[adjIx] = newInf;
                    anyChanges = true;
                }
                Adjacency? getConnection(int fromFaceIx, int fromEdgeIx, int toFaceIx, int toEdgeIx)
                {
                    if (fromFaceIx > toFaceIx)
                    {
                        var t = fromFaceIx;
                        fromFaceIx = toFaceIx;
                        toFaceIx = t;
                        t = fromEdgeIx;
                        fromEdgeIx = toEdgeIx;
                        toEdgeIx = t;
                    }
                    return polyhedron.Adjacencies?.FirstOrDefault(ad => ad.FromFace == fromFaceIx && ad.FromEdge == fromEdgeIx && ad.ToFace == toFaceIx && ad.ToEdge == toEdgeIx)?.Adjacency;
                }

                // Restricted variable scope
                {
                    var vx = faces[0][0];
                    // Put first vertex at origin and apply rotation
                    for (int i = 0; i < faces.Length; i++)
                        for (int j = 0; j < faces[i].Length; j++)
                            faces[i][j] = (faces[i][j] - vx).RotateZ(polyhedron.Rotation);

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

                    // If polyhedron is now *below* the x/y plane, rotate it 180° so it’s above
                    if (faces.Sum(f => f.Sum(p => p.Z)) < 0)
                        faces = faces.Select(f => f.Select(p => pt(-p.X, p.Y, -p.Z)).ToArray()).ToArray();

                    // Finally, apply offset
                    var offsetPt = pt(polyhedron.XOffset, polyhedron.YOffset, 0);
                    for (int i = 0; i < faces.Length; i++)
                        for (int j = 0; j < faces[i].Length; j++)
                            faces[i][j] = faces[i][j] + offsetPt;
                }

                var polyDraws = new List<Action>();
                var q = new Queue<(int newFaceIx, Pt[][] rotatedSolid)>();

                // Keeps track of the polygons in the net and also which faces have already been processed during the following algorithm (unvisited ones are null).
                var polygons = new PointD[faces.Length][];

                // Remembers which faces have already been encountered (through adjacent edges) but not yet processed.
                var seen = new HashSet<int>();

                q.Enqueue((0, faces));
                while (q.Count > 0)
                {
                    var (fromFaceIx, rotatedPolyhedron) = q.Dequeue();
                    polygons[fromFaceIx] = rotatedPolyhedron[fromFaceIx].Select(pt => p(pt.X, pt.Y)).ToArray();
                    sendText($"label-{fromFaceIx}", null, polyhedron.FontSizes[fromFaceIx], polygons[fromFaceIx].Sum(p => p.X) / polygons[fromFaceIx].Length, polygons[fromFaceIx].Sum(p => p.Y) / polygons[fromFaceIx].Length, fromFaceIx.ToString(), "#000");

                    for (int fromEdgeIx = 0; fromEdgeIx < rotatedPolyhedron[fromFaceIx].Length; fromEdgeIx++)
                    {
                        var edgeData = new JsonDict { { "polyhedron", polyhedron.FileCompatibleName }, { "face", fromFaceIx }, { "edge", fromEdgeIx } }.ToStringIndented();
                        int toEdgeIx = -1;
                        // Find another face that has the same edge
                        var toFaceIx = rotatedPolyhedron.IndexOf(fc =>
                        {
                            toEdgeIx = fc.IndexOf(p => sufficientlyClose(p, rotatedPolyhedron[fromFaceIx][(fromEdgeIx + 1) % rotatedPolyhedron[fromFaceIx].Length]));
                            return toEdgeIx != -1 && sufficientlyClose(fc[(toEdgeIx + 1) % fc.Length], rotatedPolyhedron[fromFaceIx][fromEdgeIx]);
                        });
                        if (toEdgeIx == -1 || toFaceIx == -1)
                            Debugger.Break();

                        var adj = getConnection(fromFaceIx, fromEdgeIx, toFaceIx, toEdgeIx);

                        // Make sure that this connection has an entry in the adjacency array
                        if (adj == null)
                        {
                            adj = seen.Contains(toFaceIx) ? Adjacency.Curve : Adjacency.Net;
                            setConnection(fromFaceIx, fromEdgeIx, toFaceIx, toEdgeIx, adj.Value);
                        }

                        if ((adj == Adjacency.Net || adj == Adjacency.NetWall) && seen.Add(toFaceIx))
                        {
                            // Rotate about the edge so that the new face is on the X/Y plane (i.e. “roll” the polyhedron)
                            var toFace = rotatedPolyhedron[toFaceIx];
                            var normal = (toFace[2] - toFace[1]) * (toFace[0] - toFace[1]);
                            var rot = normal.Normalize() * pt(0, 0, 1);
                            var asin = arcsin(rot.Length);
                            var newPolyhedron = Ut.NewArray(
                                rotatedPolyhedron.Select(face => face.Select(p => p.Rotate(toFace[(toEdgeIx + 1) % toFace.Length], toFace[toEdgeIx], asin)).ToArray()).ToArray(),
                                rotatedPolyhedron.Select(face => face.Select(p => p.Rotate(toFace[(toEdgeIx + 1) % toFace.Length], toFace[toEdgeIx], -asin)).ToArray()).ToArray(),
                                rotatedPolyhedron.Select(face => face.Select(p => p.Rotate(toFace[(toEdgeIx + 1) % toFace.Length], toFace[toEdgeIx], 180 + asin)).ToArray()).ToArray(),
                                rotatedPolyhedron.Select(face => face.Select(p => p.Rotate(toFace[(toEdgeIx + 1) % toFace.Length], toFace[toEdgeIx], 180 - asin)).ToArray()).ToArray())
                                .Where(sld => sld.All(fc => fc.All(p => p.Z > -closeness)))
                                .MinElement(sld => sld[toFaceIx].Sum(p => p.Z * p.Z));
                            q.Enqueue((toFaceIx, newPolyhedron));
                        }
                        else
                        {
                            var p1 = polygons[fromFaceIx][fromEdgeIx];
                            var p2 = polygons[fromFaceIx][(fromEdgeIx + 1) % polygons[fromFaceIx].Length];
                            sendPath($"edge-{fromFaceIx}-{fromEdgeIx}", null, edgeData, $"M {p1.X},{p1.Y} L {p2.X},{p2.Y}", strokeWidth: adj == Adjacency.None || adj == Adjacency.NetWall ? .05 : .025);
                            if (polygons[toFaceIx] != null)
                            {
                                var controlPointFactor = adj == Adjacency.Curve ? 1 : .6;

                                var p11 = polygons[fromFaceIx][fromEdgeIx];
                                var p12 = polygons[fromFaceIx][(fromEdgeIx + 1) % polygons[fromFaceIx].Length];
                                var p1m = p((p11.X + p12.X) / 2, (p11.Y + p12.Y) / 2);
                                var p1c = p(p1m.X - (p1m.Y - p11.Y) * controlPointFactor, p1m.Y + (p1m.X - p11.X) * controlPointFactor);
                                var p21 = polygons[toFaceIx][toEdgeIx];
                                var p22 = polygons[toFaceIx][(toEdgeIx + 1) % polygons[toFaceIx].Length];
                                var p2m = p((p21.X + p22.X) / 2, (p21.Y + p22.Y) / 2);
                                var p2c = p(p2m.X - (p2m.Y - p21.Y) * controlPointFactor, p2m.Y + (p2m.X - p21.X) * controlPointFactor);

                                var edge1 = new EdgeD(p1m, p1c);
                                var edge2 = new EdgeD(p2c, p2m);
                                Intersect.LineWithLine(ref edge1, ref edge2, out var l1, out var l2);
                                var intersect = edge1.Start + l1 * (edge1.End - edge1.Start);

                                switch (adj.Value)
                                {
                                    case Adjacency.None:
                                        break;

                                    case Adjacency.NetWall:
                                    case Adjacency.Net:
                                        //sendText($"portal-letter-{fromFaceIx}-{fromEdgeIx}", "edge", 1, p1c.X, p1c.Y, "?", "#f00", edgeData);
                                        //sendText($"portal-letter-{toFaceIx}-{toEdgeIx}", "edge", 1, p2c.X, p2c.Y, "?", "#f00", edgeData);
                                        //sendPath($"portal-marker-{fromFaceIx}-{fromEdgeIx}", "edge", edgeData, $"M {(p11.X + p1m.X) / 2},{(p11.Y + p1m.Y) / 2} {(p1c.X + p1m.X) / 2},{(p1c.Y + p1m.Y) / 2} {(p12.X + p1m.X) / 2},{(p12.Y + p1m.Y) / 2} z", strokeWidth: null, fill: "#000");
                                        //sendPath($"portal-marker-{toFaceIx}-{toEdgeIx}", "edge", edgeData, $"M {(p21.X + p2m.X) / 2},{(p21.Y + p2m.Y) / 2} {(p2c.X + p2m.X) / 2},{(p2c.Y + p2m.Y) / 2} {(p22.X + p2m.X) / 2},{(p22.Y + p2m.Y) / 2} z", strokeWidth: null, fill: "#000");
                                        break;

                                    case Adjacency.Portal:
                                        var ch = polyhedron.GetPortalLetter(fromFaceIx, fromEdgeIx);
                                        sendText($"portal-letter-{fromFaceIx}-{fromEdgeIx}", "edge", .5, p1c.X, p1c.Y, ch.ToString(), "#000", edgeData);
                                        sendText($"portal-letter-{toFaceIx}-{toEdgeIx}", "edge", .5, p2c.X, p2c.Y, ch.ToString(), "#000", edgeData);
                                        sendPath($"portal-marker-{fromFaceIx}-{fromEdgeIx}", "edge", edgeData, $"M {(p11.X + p1m.X) / 2},{(p11.Y + p1m.Y) / 2} {(p1c.X + p1m.X) / 2},{(p1c.Y + p1m.Y) / 2} {(p12.X + p1m.X) / 2},{(p12.Y + p1m.Y) / 2} z", strokeWidth: null, fill: "#000");
                                        sendPath($"portal-marker-{toFaceIx}-{toEdgeIx}", "edge", edgeData, $"M {(p21.X + p2m.X) / 2},{(p21.Y + p2m.Y) / 2} {(p2c.X + p2m.X) / 2},{(p2c.Y + p2m.Y) / 2} {(p22.X + p2m.X) / 2},{(p22.Y + p2m.Y) / 2} z", strokeWidth: null, fill: "#000");
                                        break;

                                    case Adjacency.Curve:
                                        sendPath($"curve-{fromFaceIx}-{fromEdgeIx}", "edge", edgeData,
                                            (p2m - p1m).Distance() < .5 ? $"M {p1m.X},{p1m.Y} L {p2m.X},{p2m.Y}" :
                                            l1 >= 0 && l1 <= 1 && l2 >= 0 && l2 <= 1 ? $"M {p1m.X},{p1m.Y} C {intersect.X},{intersect.Y} {intersect.X},{intersect.Y} {p2m.X},{p2m.Y}" :
                                            $"M {p1m.X},{p1m.Y} C {p1c.X},{p1c.Y} {p2c.X},{p2c.Y} {p2m.X},{p2m.Y}",
                                            strokeWidth: .01);
                                        break;

                                    default:
                                        break;
                                }

                            }
                        }
                    }
                }

                if (anyChanges)
                    save();

                return new BoundingBoxD
                {
                    Xmin = polygons.Min(pg => pg?.Min(p => p.X)).Value,
                    Ymin = polygons.Min(pg => pg?.Min(p => p.Y)).Value,
                    Xmax = polygons.Max(pg => pg?.Max(p => p.X)).Value,
                    Ymax = polygons.Max(pg => pg?.Max(p => p.Y)).Value
                };
            }

            private void save()
            {
                lock (_lockObj)
                    ClassifyJson.SerializeToFile(_polyhedra, _masterJsonPath);
            }
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
                        File.WriteAllText($@"D:\c\KTANE\KtaneStuff\DataFiles\PolyhedralMaze\Txt\{filename}", resp.DataString);
                        ConsoleUtil.WriteLine(" • {0/DarkGreen}".Color(ConsoleColor.DarkGray).Fmt(href));
                    }
                });
            }
        }

        public static (string name, Pt[][] faces) Parse(string data)
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
