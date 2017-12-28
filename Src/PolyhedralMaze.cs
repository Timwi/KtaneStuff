using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
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

        [Flags]
        public enum Adjacency
        {
            /// <summary>This edge is a wall.</summary>
            NotTraversible = 0 << 0,
            /// <summary>This edge is traversible (not a wall).</summary>
            Traversible = 1 << 0,

            /// <summary>The faces are connected in the net.</summary>
            Connected = 0 << 1,
            /// <summary>The faces are connected via a curved line (assuming the edge is traversible).</summary>
            Curved = 1 << 1,
            /// <summary>The faces are connected via a lettered reference (assuming the edge is traversible).</summary>
            Portaled = 2 << 1,

            /// <summary>A mask used to determine the connection type.</summary>
            ConnectionMask = 3 << 1,

            ///// <summary>The faces are NOT adjacent in the net, nor can you walk from one to the other.</summary>
            //None = NotTraversible | Portaled,
            ///// <summary>The faces are adjacent in the net, but you cannot walk from one to the other (it’s a wall).</summary>
            //NetWall = NotTraversible | Connected,
            ///// <summary>The faces are adjacent in the net and you cannot walk from one to the other.</summary>
            //Net = Traversible | Connected,
            ///// <summary>
            /////     The faces are NOT adjacent in the net, but you cannot walk from one to the other. The connection is
            /////     indicated by a letter.</summary>
            //Portal = Traversible | Portaled,
            ///// <summary>
            /////     The faces are NOT adjacent in the net, but you cannot walk from one to the other. The connection is
            /////     indicated by a curved line.</summary>
            //Curve = Traversible | Curved
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
            public Pt[][] Faces { get; set; }

            public AdjacencyInfo[] Adjacencies { get; set; }
            public string SvgId { get; set; }
            public double[] FontSizes { get; set; }
            public double Rotation { get; set; }
            public double XOffset { get; set; }
            public double YOffset { get; set; }
            public double ScaleFactor { get; set; }
            public double LabelX { get; set; }
            public double LabelY { get; set; }

            public PolyhedronInfo(string fileCompatibleName, string readableName, Pt[][] faces)
            {
                FileCompatibleName = fileCompatibleName;
                ReadableName = readableName;
                Faces = faces;
                ScaleFactor = 1;
            }
            private PolyhedronInfo() { }    // Classify
            public void GenerateObjFile()
            {
                Directory.CreateDirectory(@"D:\c\KTANE\PolyhedralMaze\Assets\Models\Polyhedra");
                var avg = Faces.SelectMany(p => p).Average(p => p.Length);
                File.WriteAllText($@"D:\c\KTANE\PolyhedralMaze\Assets\Models\Polyhedra\{FileCompatibleName}.obj", Md.GenerateObjFile(Faces.Select(f => f.Select(p => p / avg).ToArray()).ToArray(), FileCompatibleName, AutoNormal.Flat));
            }

            public char GetPortalLetter(int faceIx, int edgeIx)
            {
                char ch = 'A';
                for (int i = 0; i < Adjacencies.Length; i++)
                {
                    if (Adjacencies[i].FromFace == faceIx && Adjacencies[i].FromEdge == edgeIx)
                        return ch;
                    if (Adjacencies[i].ToFace == faceIx && Adjacencies[i].ToEdge == edgeIx)
                        return ch;
                    if (Adjacencies[i].Adjacency.HasFlag(Adjacency.Portaled | Adjacency.Traversible))
                        ch++;
                }
                Debugger.Break();
                throw new InvalidOperationException();
            }
    
            public string Declaration
            {
                get
                {
                    var avg = Faces.SelectMany(p => p).Average(p => p.Length);
                    var sb = new StringBuilder();
                    sb.AppendLine($@"new Polyhedron {{ Name = ""{FileCompatibleName.CLiteralEscape()}"", ReadableName = ""{ReadableName.CLiteralEscape()}"", Faces = new Face[] {{");
                    for (int fIx = 0; fIx < Faces.Length; fIx++)
                    {
                        var normal = ((Faces[fIx][2] - Faces[fIx][1]) * (Faces[fIx][0] - Faces[fIx][1])).Normalize();
                        var adjFacesInf = Enumerable.Range(0, Faces[fIx].Length).Select(eIx => Adjacencies.Select(adj => adj.FromFace == fIx && adj.FromEdge == eIx ? adj.ToFace : adj.ToFace == fIx && adj.ToEdge == eIx ? adj.FromFace : (int?) null).First(adj => adj != null).Value).JoinString(", ");
                        sb.AppendLine($@"                new Face {{ Normal = new Vector3({-normal.X}f, {normal.Y}f, {normal.Z}f), Distance = {normal.Dot(Faces[fIx][0] / avg)}f, AdjacentFaces = new int[] {{ {adjFacesInf} }}, Vertices = new Vector3[] {{");
                        for (int eIx = 0; eIx < Faces[fIx].Length; eIx++)
                            sb.AppendLine($@"                    new Vector3({-Faces[fIx][eIx].X / avg}f, {Faces[fIx][eIx].Y / avg}f, {Faces[fIx][eIx].Z / avg}f){(eIx == Faces[fIx].Length - 1 ? "" : ",")}");
                        sb.AppendLine($@"                }} }}{(fIx == Faces.Length - 1 ? "" : ",")}");
                    }
                    sb.Append($@"            }} }}");
                    return sb.ToString();
                }
            }
        }

        public static void GeneratePolyhedronInfos()
        {
            var wanted = @"4TruncatedDeltoidalIcositetrahedron2
ChamferedDodecahedron1
ChamferedIcosahedron1
DeltoidalHexecontahedron
DisdyakisDodecahedron
JoinedIcosidodecahedron
JoinedLsnubCube
JoinedRhombicuboctahedron
LpentagonalHexecontahedron
OrthokisPropelloCube
PentakisDodecahedron
RectifiedRhombicuboctahedron
TriakisIcosahedron
Rhombicosidodecahedron".Replace("\r", "").Split('\n');

            List<PolyhedronInfo> polyhedra;
            try { polyhedra = ClassifyJson.DeserializeFile<List<PolyhedronInfo>>(_masterJsonPath); }
            catch { polyhedra = new List<PolyhedronInfo>(); }

            foreach (var file in new DirectoryInfo(@"D:\c\KTANE\KtaneStuff\DataFiles\PolyhedralMaze\Txt").GetFiles("*.txt"))
            {
                var (name, faces) = Parse(File.ReadAllText(file.FullName));
                var newInfo = new PolyhedronInfo(Path.GetFileNameWithoutExtension(file.Name), name, faces);
                //if ((faces.Length < 40 || faces.Length > 60) && newInfo.FileCompatibleName != "Rhombicosidodecahedron")
                if (!wanted.Contains(newInfo.FileCompatibleName))
                    continue;

                var info = polyhedra.FirstOrDefault(si => si.FileCompatibleName == newInfo.FileCompatibleName);
                if (info == null)
                {
                    Console.WriteLine($"Adding {newInfo.ReadableName} ({newInfo.Faces.Length})");
                    polyhedra.Add(newInfo);
                }
                else
                    info.Faces = faces;
            }

            ClassifyJson.SerializeToFile(polyhedra, _masterJsonPath);
        }

        public static void GenerateModels()
        {
            List<PolyhedronInfo> polyhedra;
            try { polyhedra = ClassifyJson.DeserializeFile<List<PolyhedronInfo>>(_masterJsonPath); }
            catch { polyhedra = new List<PolyhedronInfo>(); }
            foreach (var poly in polyhedra)
                poly.GenerateObjFile();

            File.WriteAllText(@"D:\c\KTANE\PolyhedralMaze\Assets\Models\Arrow.obj", GenerateObjFile(Arrow(), "Arrow"));
            File.WriteAllText(@"D:\c\KTANE\PolyhedralMaze\Assets\Models\Frame.obj", GenerateObjFile(Frame(), "Frame"));

            Console.WriteLine("Models generated.");
        }

        private static IEnumerable<VertexInfo[]> Arrow()
        {
            var shape = new[] { p(1, 0), p(.75, 2), p(1.5, 2), p(0, 4), p(-1.5, 2), p(-.75, 2), p(-1, 0) }.Extrude(.4, includeBackFace: true, flatSideNormals: true);
            return shape;
        }

        public static void GenerateDeclaration()
        {
            List<PolyhedronInfo> polyhedra;
            try { polyhedra = ClassifyJson.DeserializeFile<List<PolyhedronInfo>>(_masterJsonPath); }
            catch { polyhedra = new List<PolyhedronInfo>(); }

            File.WriteAllText(@"D:\c\KTANE\PolyhedralMaze\Assets\Polyhedra.cs", $@"
using UnityEngine;

namespace PolyhedralMaze
{{
    sealed class Polyhedron
    {{
        public string Name;
        public string ReadableName;
        public Face[] Faces;
    }}

    sealed class Face
    {{
        public Vector3[] Vertices;
        public int[] AdjacentFaces;
        public Vector3 Normal;
        public float Distance;
    }}

    static class Data
    {{
        public static Polyhedron[] Polyhedra = new Polyhedron[] {{
            {polyhedra.Select(p => p.Declaration).JoinString(@",
            ")}
        }};
    }}
}}

");
            Console.WriteLine("Declaration file generated.");
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
                PropagateExceptions = true,
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
                    case "remove": Update(json["Polyhedron"].GetString(), polyhedron => { polyhedron.SvgId = null; SendPolyhedronSelect(polyhedron); }); break;
                    case "rotate": Update(edgeData["polyhedron"].GetString(), polyhedron => { polyhedron.Rotation += json["Amount"].GetDouble(NumericConversionOptions.AllowConversionFromString); }); break;
                    case "scale": Update(edgeData["polyhedron"].GetString(), polyhedron => { polyhedron.ScaleFactor += json["Amount"].GetDouble(NumericConversionOptions.AllowConversionFromString); }); break;
                    case "move": Update(edgeData["polyhedron"].GetString(), polyhedron => { polyhedron.XOffset += json["XAmount"].GetDouble(NumericConversionOptions.AllowConversionFromString); polyhedron.YOffset += json["YAmount"].GetDouble(NumericConversionOptions.AllowConversionFromString); }); break;
                    case "move-caption": Update(edgeData["polyhedron"].GetString(), polyhedron => { polyhedron.LabelX += json["XAmount"].GetDouble(NumericConversionOptions.AllowConversionFromString); polyhedron.LabelY += json["YAmount"].GetDouble(NumericConversionOptions.AllowConversionFromString); }); break;

                    case "convert-to-portal": ConvertTo(Adjacency.Portaled, edgeData); break;
                    case "convert-to-curve": ConvertTo(Adjacency.Curved, edgeData); break;
                    case "make-connected": ConvertTo(Adjacency.Connected, edgeData); break;

                    case "generate-maze": Update(edgeData["polyhedron"].GetString(), GenerateMaze); break;
                    case "clear-maze": Update(edgeData["polyhedron"].GetString(), ClearMaze); break;

                    default:
                        Debugger.Break();
                        break;
                }
            }

            private static void ClearMaze(PolyhedronInfo polyhedron)
            {
                foreach (var adj in polyhedron.Adjacencies)
                    adj.Adjacency = adj.Adjacency | Adjacency.Traversible;
            }

            private void GenerateMaze(PolyhedronInfo polyhedron)
            {
                SendDelete(polyhedron, "decor");

                const double wallProbability = .33;

                var iterations = 0;
                tryAgain:
                iterations++;
                if (iterations >= 50000)
                {
                    SendMessage($"Log: Giving up after {iterations} iterations.");
                    ClearMaze(polyhedron);
                    return;
                }

                // Put random walls in
                var adjs = polyhedron.Adjacencies.ToArray().Shuffle();
                for (int i = 0; i < adjs.Length; i++)
                    adjs[i].Adjacency = (adjs[i].Adjacency & ~Adjacency.Traversible) | (i <= adjs.Length * wallProbability ? 0 : Adjacency.Traversible);

                int maxDist = 0, maxFrom = -1, maxTo = -1;
                for (int startFace = 0; startFace < polyhedron.Faces.Length; startFace++)
                {
                    var visited = new bool[polyhedron.Faces.Length];
                    var qq = new Queue<(int face, int dist)>();
                    qq.Enqueue((startFace, 0));
                    while (qq.Count > 0)
                    {
                        var (face, dist) = qq.Dequeue();
                        if (visited[face])
                            continue;
                        if (maxDist < dist)
                        {
                            maxDist = dist;
                            maxFrom = startFace;
                            maxTo = face;
                        }
                        visited[face] = true;
                        for (int i = 0; i < adjs.Length; i++)
                            if (adjs[i].Adjacency.HasFlag(Adjacency.Traversible))
                            {
                                if (adjs[i].FromFace == face)
                                    qq.Enqueue((adjs[i].ToFace, dist + 1));
                                else if (adjs[i].ToFace == face)
                                    qq.Enqueue((adjs[i].FromFace, dist + 1));
                            }
                    }

                    // Make sure the maze isn’t disjoint
                    if (startFace == 0)
                        for (int i = 0; i < visited.Length; i++)
                            if (!visited[i])
                                goto tryAgain;
                }

                SendMessage($"Log: iterations = {iterations}, max distance = {maxDist} ({maxFrom} to {maxTo})");
            }

            private void Update(string polyhedronId, Action<PolyhedronInfo> action)
            {
                var polyIx = _polyhedra.IndexOf(p => p.FileCompatibleName == polyhedronId);
                var polyhedron = _polyhedra[polyIx];
                action(polyhedron);
                if (polyhedron.SvgId != null)
                    _boundingBoxes[polyIx] = GenerateNet(polyhedron);
                SendViewBoxes();
                save();
            }

            private void ConvertTo(Adjacency adj, JsonValue edgeData)
            {
                var pIx = _polyhedra.IndexOf(poly => poly.FileCompatibleName == edgeData["polyhedron"].GetString());
                var polyhedron = _polyhedra[pIx];
                var faceIx = edgeData["face"].GetInt();
                var edgeIx = edgeData["edge"].GetInt();
                var adjInf = polyhedron.Adjacencies.Single(ad => (ad.FromFace == faceIx && ad.FromEdge == edgeIx) || (ad.ToFace == faceIx && ad.ToEdge == edgeIx));
                adjInf.Adjacency = (adjInf.Adjacency & ~Adjacency.ConnectionMask) | adj;
                SendDelete(polyhedron, $"face-{faceIx}", $"edge-{faceIx}-{edgeIx}", "decor");
                _boundingBoxes[pIx] = GenerateNet(polyhedron);
                SendViewBoxes();
                save();
            }

            private void SendDelete(PolyhedronInfo polyhedron, params string[] classes)
            {
                SendMessage(new JsonDict { { "svg", polyhedron.SvgId }, { "tag", "delete" }, { "classes", classes.Select(c => $"poly-{polyhedron.FileCompatibleName}-{c}").ToJsonList() } });
            }

            private BoundingBoxD GenerateNet(PolyhedronInfo polyhedron)
            {
                // Numbers closer than this are considered equal
                const double closeness = .00001;
                bool sufficientlyClose(Pt p1, Pt p2) => Math.Abs(p1.X - p2.X) < closeness && Math.Abs(p1.Y - p2.Y) < closeness && Math.Abs(p1.Z - p2.Z) < closeness;

                void send(string id, IEnumerable<string> classes, string tag, JsonDict attr, string content, string edgeData)
                {
                    var dict = new JsonDict();
                    dict["svg"] = polyhedron.SvgId;
                    dict["id"] = $"poly-{polyhedron.FileCompatibleName}-{id}";
                    dict["classes"] = (classes ?? Enumerable.Empty<string>()).Select(c => $"poly-{polyhedron.FileCompatibleName}-{c}").Concat($"poly-{polyhedron.FileCompatibleName}").ToJsonList();
                    dict["tag"] = tag;
                    dict["attr"] = attr;
                    if (content != null)
                        dict["content"] = content;
                    if (edgeData != null)
                        dict["edgeData"] = edgeData;
                    SendMessage(dict);
                }

                void sendPath(string id, IEnumerable<string> classes, string edgeData, string data, string stroke = null, double? strokeWidth = null, string strokeDasharray = null, string fill = null) =>
                    send(id, classes, "path", new JsonDict { { "d", data }, { "stroke", stroke ?? (strokeWidth == null ? "none" : "black") }, { "stroke-linejoin", "round" }, { "stroke-width", strokeWidth }, { "stroke-dasharray", strokeDasharray }, { "fill", fill ?? "none" } }, null, edgeData);

                void sendText(string id, IEnumerable<string> classes, double fontSize, double x, double y, string content, string fill, string edgeData = null) =>
                    send(id, classes, "text", new JsonDict { { "x", x }, { "y", y + fontSize * .35 }, { "text-anchor", "middle" }, { "fill", fill }, { "font-size", fontSize }, { "style", "white-space:pre" } }, content, edgeData);

                sendText("caption", null, .6, polyhedron.LabelX + polyhedron.XOffset, polyhedron.LabelY + polyhedron.YOffset, polyhedron.ReadableName, "#000");

                var anyChanges = false;

                if (polyhedron.FontSizes == null)
                {
                    polyhedron.FontSizes = Ut.NewArray(polyhedron.Faces.Length, _ => .5);
                    anyChanges = true;
                }

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

                // Take a full copy of this and apply scale
                var faces = polyhedron.Faces.Select(face => face.Select(p => p * polyhedron.ScaleFactor).ToArray()).ToArray();

                // Restricted variable scope
                {
                    var vx = faces[0][0];
                    // Put first vertex at origin and apply rotation
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

                    // If polyhedron is now *below* the x/y plane, rotate it 180° so it’s above
                    if (faces.Sum(f => f.Sum(p => p.Z)) < 0)
                        faces = faces.Select(f => f.Select(p => pt(-p.X, p.Y, -p.Z)).ToArray()).ToArray();

                    // Finally, apply rotation and offset
                    var offsetPt = pt(polyhedron.XOffset, polyhedron.YOffset, 0);
                    for (int i = 0; i < faces.Length; i++)
                        for (int j = 0; j < faces[i].Length; j++)
                            faces[i][j] = faces[i][j].RotateZ(polyhedron.Rotation) + offsetPt;
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
                    sendText($"label-{fromFaceIx}", new[] { $"face-{fromFaceIx}" }, polyhedron.FontSizes[fromFaceIx], polygons[fromFaceIx].Sum(p => p.X) / polygons[fromFaceIx].Length, polygons[fromFaceIx].Sum(p => p.Y) / polygons[fromFaceIx].Length, fromFaceIx.ToString(), "#000");

                    for (int fromEdgeIx = 0; fromEdgeIx < rotatedPolyhedron[fromFaceIx].Length; fromEdgeIx++)
                    {
                        var edgeData = new JsonDict { { "polyhedron", polyhedron.FileCompatibleName }, { "face", fromFaceIx }, { "edge", fromEdgeIx } }.ToString();
                        int toEdgeIx = -1;
                        // Find another face that has the same edge
                        var toFaceIx = rotatedPolyhedron.IndexOf(fc =>
                        {
                            toEdgeIx = fc.IndexOf(p => sufficientlyClose(p, rotatedPolyhedron[fromFaceIx][(fromEdgeIx + 1) % rotatedPolyhedron[fromFaceIx].Length]));
                            return toEdgeIx != -1 && sufficientlyClose(fc[(toEdgeIx + 1) % fc.Length], rotatedPolyhedron[fromFaceIx][fromEdgeIx]);
                        });
                        if (toEdgeIx == -1 || toFaceIx == -1)
                            Debugger.Break();

                        // Make sure that this connection has an entry in the adjacency array
                        var adjNullable = getConnection(fromFaceIx, fromEdgeIx, toFaceIx, toEdgeIx);
                        if (adjNullable == null)
                        {
                            adjNullable = seen.Contains(toFaceIx) ? Adjacency.Curved : Adjacency.Connected;
                            setConnection(fromFaceIx, fromEdgeIx, toFaceIx, toEdgeIx, adjNullable.Value);
                        }
                        var adj = adjNullable.Value;

                        if ((adj & Adjacency.ConnectionMask) == Adjacency.Connected && seen.Add(toFaceIx))
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
                            IEnumerable<string> classes = new[] { $"face-{fromFaceIx}", $"face-{toFaceIx}", $"edge-{fromFaceIx}-{fromEdgeIx}", $"edge-{toFaceIx}-{toEdgeIx}" };
                            sendPath($"edge-{fromFaceIx}-{fromEdgeIx}", classes, edgeData, $"M {p1.X},{p1.Y} L {p2.X},{p2.Y}",
                                strokeWidth: adj.HasFlag(Adjacency.Traversible) ? .025 : .1,
                                //strokeDasharray: adj.HasFlag(Adjacency.Traversible) ? ".1,.1" : null,
                                stroke: adj.HasFlag(Adjacency.Traversible) ? "black" : null);
                            if (polygons[toFaceIx] != null && adj.HasFlag(Adjacency.Traversible))
                            {
                                var controlPointFactor = (adj & Adjacency.ConnectionMask) == Adjacency.Curved ? 1 : .6;

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

                                classes = classes.Concat("decor");
                                switch (adj & Adjacency.ConnectionMask)
                                {
                                    case Adjacency.Portaled:
                                        var ch = polyhedron.GetPortalLetter(fromFaceIx, fromEdgeIx);
                                        sendText($"portal-letter-{fromFaceIx}-{fromEdgeIx}", classes, .5, p1c.X, p1c.Y, ch.ToString(), "#000", edgeData);
                                        sendText($"portal-letter-{toFaceIx}-{toEdgeIx}", classes, .5, p2c.X, p2c.Y, ch.ToString(), "#000", edgeData);
                                        sendPath($"portal-marker-{fromFaceIx}-{fromEdgeIx}", classes, edgeData, $"M {(p11.X + p1m.X) / 2},{(p11.Y + p1m.Y) / 2} {(p1c.X + p1m.X) / 2},{(p1c.Y + p1m.Y) / 2} {(p12.X + p1m.X) / 2},{(p12.Y + p1m.Y) / 2} z", fill: "#888");
                                        sendPath($"portal-marker-{toFaceIx}-{toEdgeIx}", classes, edgeData, $"M {(p21.X + p2m.X) / 2},{(p21.Y + p2m.Y) / 2} {(p2c.X + p2m.X) / 2},{(p2c.Y + p2m.Y) / 2} {(p22.X + p2m.X) / 2},{(p22.Y + p2m.Y) / 2} z", fill: "#888");
                                        break;

                                    case Adjacency.Curved:
                                        sendPath($"curve-{fromFaceIx}-{fromEdgeIx}", classes, edgeData,
                                            (p2m - p1m).Distance() < .5 ? $"M {p1m.X},{p1m.Y} L {p2m.X},{p2m.Y}" :
                                            l1 >= 0 && l1 <= 1 && l2 >= 0 && l2 <= 1 ? $"M {p1m.X},{p1m.Y} C {intersect.X},{intersect.Y} {intersect.X},{intersect.Y} {p2m.X},{p2m.Y}" :
                                            $"M {p1m.X},{p1m.Y} C {p1c.X},{p1c.Y} {p2c.X},{p2c.Y} {p2m.X},{p2m.Y}",
                                            strokeWidth: .025);
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
                    Xmin = Math.Min(polygons.Min(pg => pg?.Min(p => p.X)).Value, polyhedron.LabelX + polyhedron.XOffset),
                    Ymin = Math.Min(polygons.Min(pg => pg?.Min(p => p.Y)).Value, polyhedron.LabelY + polyhedron.YOffset),
                    Xmax = Math.Max(polygons.Max(pg => pg?.Max(p => p.X)).Value, polyhedron.LabelX + polyhedron.XOffset),
                    Ymax = Math.Max(polygons.Max(pg => pg?.Max(p => p.Y)).Value, polyhedron.LabelY + polyhedron.YOffset)
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

        private static IEnumerable<VertexInfo[]> Frame()
        {
            var depth = .06;
            var béFac = depth * .55;
            var ratio = .825;
            var th = .2;
            const int bézierSteps = 6;

            MeshVertexInfo[] bpa(double x, double y, double z, Normal befX, Normal afX, Normal befY, Normal afY, double bx, double by, double ax, double ay) { return new[] { pt(x, y, z, befX, afX, befY, afY).WithTexture(bx, by, ax, ay) }; }

            double xf = .25;

            return CreateMesh(true, true,
                Bézier(p(th, 0), p(th, béFac), p(th - depth + béFac, depth), p(th - depth, depth), bézierSteps).Select((p, ix, first, last) => new { BP = new BevelPoint(p.X, p.Y, first || last ? Normal.Mine : Normal.Average, first || last ? Normal.Mine : Normal.Average), Y = ix / (double) (3 * bézierSteps - 1) }).Concat(
                Bézier(p(depth, depth), p(depth - béFac, depth), p(0, béFac), p(0, -.2), bézierSteps).Select((p, ix, first, last) => new { BP = new BevelPoint(p.X, p.Y, first || last ? Normal.Mine : Normal.Average, first || last ? Normal.Mine : Normal.Average), Y = (ix + 2 * bézierSteps) / (double) (3 * bézierSteps - 1) }))
                .Select((inf, ix) => inf.BP.Apply(bi => Ut.NewArray(
                    // Bottom right
                    bpa(-1 + bi.X, bi.Y, -ratio + bi.X, bi.Before, bi.After, Normal.Mine, Normal.Mine, 4 * xf, 1 - inf.Y, 0, 1 - inf.Y),

                    // Top right
                    bpa(-1 + bi.X, bi.Y, ratio - bi.X, bi.Before, bi.After, Normal.Mine, Normal.Mine, xf, 1 - inf.Y, xf, inf.Y),

                    // Top left
                    bpa(1 - bi.X, bi.Y, ratio - bi.X, bi.Before, bi.After, Normal.Mine, Normal.Mine, 2 * xf, inf.Y, 2 * xf, inf.Y),

                    // Bottom left
                    bpa(1 - bi.X, bi.Y, -ratio + bi.X, bi.Before, bi.After, Normal.Mine, Normal.Mine, 3 * xf, inf.Y, 3 * xf, 1 - inf.Y),

                    null
                )).Where(x => x != null).SelectMany(x => x).ToArray()).ToArray(), textureCoordsAreFlipped: true);
        }
    }
}
