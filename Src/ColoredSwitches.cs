using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using KtaneStuff.Modeling;
using RT.Util;
using RT.Util.ExtensionMethods;
using RT.Util.Json;
using RT.Util.Serialization;

namespace KtaneStuff
{
    using static Md;

    public static class ColoredSwitches
    {
        public static void DoModels()
        {
            File.WriteAllText(@"D:\c\KTANE\ColoredSwitches\Assets\Models\Switch.obj", GenerateObjFile(Switch(), "Switch"));
            File.WriteAllText(@"D:\c\KTANE\ColoredSwitches\Assets\Models\SwitchHighlight.obj", GenerateObjFile(SwitchHighlight(), "SwitchHighlight"));
            File.WriteAllText(@"D:\c\KTANE\ColoredSwitches\Assets\Models\Socket.obj", GenerateObjFile(Socket(), "Socket"));
        }

        public static IEnumerable<VertexInfo[]> Switch()
        {
            const double xa = 1.5;      // narrower bottom end
            const double xb = 3;      // wider top end
            const double ya = .5;       // how far below the point of rotation it reaches
            const double yb = 14.5;     // how far above the point of rotation it reaches (the main length of the switch)
            const double tht = 1.5;      // thickness at the top
            const double thb = .75;     // thickness at the bottom
            const double b = .5;        // bevel width

            var frontface = new[] { pt(-xa, -ya, -thb), pt(-xb, yb, -tht), pt(xb, yb, -tht), pt(xa, -ya, -thb) }.Select(p => p.WithNormal(0, 0, -1)).ToArray();
            yield return frontface;
            yield return frontface.Select(v => v.RotateY(180)).ToArray();

            foreach (var face in CreateMesh(false, false, Ut.NewArray(
                new[] { pt(-xa, -ya, -thb), pt(-xa - b, -ya, -thb + b), pt(-xa - b, -ya, thb - b), pt(-xa, -ya, thb) },
                new[] { pt(-xb, yb, -tht), pt(-xb - b, yb, -tht + b), pt(-xb - b, yb, tht - b), pt(-xb, yb, tht) },
                new[] { pt(-xb, yb, -tht), pt(-xb, yb + b, -tht + b), pt(-xb, yb + b, tht - b), pt(-xb, yb, tht) },
                new[] { pt(xb, yb, -tht), pt(xb, yb + b, -tht + b), pt(xb, yb + b, tht - b), pt(xb, yb, tht) },
                new[] { pt(xb, yb, -tht), pt(xb + b, yb, -tht + b), pt(xb + b, yb, tht - b), pt(xb, yb, tht) },
                new[] { pt(xa, -ya, -thb), pt(xa + b, -ya, -thb + b), pt(xa + b, -ya, thb - b), pt(xa, -ya, thb) }
            ).Select((arr, ix) => Ut.NewArray(
                arr[3].WithMeshInfo(0, 0, 1),
                arr[2].WithMeshInfo(ix % 2 != 0 ? Normal.Mine : Normal.Theirs, ix % 2 != 0 ? Normal.Theirs : Normal.Mine, Normal.Mine, Normal.Theirs),
                arr[1].WithMeshInfo(ix % 2 != 0 ? Normal.Mine : Normal.Theirs, ix % 2 != 0 ? Normal.Theirs : Normal.Mine, Normal.Theirs, Normal.Mine),
                arr[0].WithMeshInfo(0, 0, -1)
            )).ToArray()))
                yield return face;
        }

        public static IEnumerable<VertexInfo[]> SwitchHighlight()
        {
            const double xa = 2.5;      // narrower bottom end
            const double xb = 4.25;      // wider top end
            const double ya = 0;       // how far below the point of rotation it reaches
            const double yb = 15.5;     // how far above the point of rotation it reaches (the main length of the switch)
            const double tht = .75;      // thickness at the top
            const double thb = .55;     // thickness at the bottom

            var frontface = new[] { pt(-xa, -ya, -thb), pt(-xb, yb, -tht), pt(xb, yb, -tht), pt(xa, -ya, -thb) }.Select(p => p.WithNormal(0, 0, -1)).ToArray();
            yield return frontface;
            yield return frontface.Select(v => v.RotateY(180)).ToArray();

            foreach (var face in CreateMesh(false, false, Ut.NewArray(
                new[] { pt(-xa, -ya, thb), pt(-xa, -ya, -thb) },
                new[] { pt(-xb, yb, tht), pt(-xb, yb, -tht) },
                new[] { pt(xb, yb, tht), pt(xb, yb, -tht) },
                new[] { pt(xa, -ya, thb), pt(xa, -ya, -thb) }
            )))
                yield return face;
        }

        public static IEnumerable<VertexInfo[]> Socket()
        {
            const double r = 2.25;         // radius of the base sphere
            const int revSteps1 = 12;
            const int revStep2 = 12;
            const int slitAngle = 25;
            foreach (var face in CreateMesh(false, false, Enumerable.Range(0, revStep2).Reverse().Select(i => i * (360.0 - 2 * slitAngle) / (revStep2 - 1) + slitAngle).Select(angle1 => Enumerable.Range(0, revSteps1).Select(i => i * 180.0 / (revSteps1 - 1)).Select(angle2 => pt(r, 0, 0).RotateZ(angle2).RotateX(angle1)).Select((v, f, l) => f ? v.WithMeshInfo(1, 0, 0) : l ? v.WithMeshInfo(-1, 0, 0) : v.WithMeshInfo(Normal.Average, Normal.Average, Normal.Average, Normal.Average)).ToArray()).ToArray()))
                yield return face;
        }

        public static void DoGraph()
        {
            /*
            var graphJson = GenerateGraph();
            File.WriteAllText($@"D:\temp\Color Switches ({_numSwitches} switches, {_numColors} colors).json", graphJson.ToStringIndented());
            /*/
            var graphJson = JsonValue.Parse(File.ReadAllText($@"D:\temp\Color Switches ({_numSwitches} switches, {_numColors} colors).json"));
            /**/

            var edges = ClassifyJson.Deserialize<List<Edge>>(graphJson);
            var nodes = edges.Select(e => e.To).Distinct().ToArray();
            var edgesFrom = nodes.ToDictionary(node => node, node => new List<Edge>());
            foreach (var node in nodes)
                edgesFrom[node] = edges.Where(e => e.From == node).ToList();

            //System.Windows.Forms.Clipboard.SetText(nodes.Select(n => $"{n.SwitchStates}={edgesFrom[n].Select(e => $"{e.Color}>{e.To.SwitchStates}").JoinString("|")}").JoinString("\n"));
            JsonGraphToGraphML(graphJson);
        }

        class Node
        {
            public int SwitchStates;
            public override string ToString() => Convert.ToString(SwitchStates, 2).PadLeft(_numSwitches, '0');
        }
        class Edge
        {
            public Node From, To;
            public int Color;
            public override string ToString() => "{0} ={1}=> {2}".Fmt(From, Color, To);
        }

        const int _numSwitches = 5;
        const int _numColors = 6;

        public static JsonValue GenerateGraph()
        {
            var nodes = Ut.NewArray(1 << _numSwitches, ix => new Node { SwitchStates = ix });
            var edgesFrom = nodes.ToDictionary(node => node, node => new List<Edge>());
            var edges = new List<Edge>();

            startAgain:
            var foundUnconnectable = false;
            Console.WriteLine("Number of edges: " + edgesFrom.Values.Sum(l => l.Count));

            // Go through all color combinations in a random order and add edges whenever we find a node to be unreachable
            var numColorCombinations = Enumerable.Range(0, _numSwitches).Aggregate(1, (prev, next) => prev * _numColors);
            var colorCombinations = Enumerable.Range(0, numColorCombinations).ToList().Shuffle();
            var colors = new int[_numSwitches];
            for (int ccIx = 0; ccIx < colorCombinations.Count; ccIx++)
            {
                var cc = colorCombinations[ccIx];

                if (ccIx % 1000 == 0)
                    Console.Write($"{ccIx}/{colorCombinations.Count}\r");

                for (int i = 0; i < _numSwitches; i++)
                {
                    colors[i] = cc % _numColors;
                    cc /= _numColors;
                }

                // Check all the nodes in random order
                foreach (var startNode in nodes.Shuffle())
                {
                    // Which nodes are reachable from that node?
                    var reachable = new HashSet<Node>();
                    var q = new[] { new { Node = startNode, Distance = 0 } }.ToQueue();
                    while (q.Count > 0)
                    {
                        var elem = q.Dequeue();
                        if (reachable.Add(elem.Node))
                            foreach (var edge in edgesFrom[elem.Node])
                                if (edge.Color == colors[findBit(edge.From.SwitchStates ^ edge.To.SwitchStates)])
                                    q.Enqueue(new { Node = edge.To, Distance = elem.Distance + 1 });
                    }

                    // If all nodes are either reachable or unconnectable, move on to the next start node
                    var connectable = nodes.Where(n => !reachable.Contains(n) && isPowerOf2(n.SwitchStates ^ startNode.SwitchStates)).ToArray();
                    if (connectable.Length == 0)
                    {
                        foundUnconnectable |= reachable.Count < nodes.Length;
                        continue;
                    }

                    // Make a new connection and then start all over
                    var randomConnectable = connectable[Rnd.Next(connectable.Length)];
                    var newEdge = new Edge { From = startNode, To = randomConnectable, Color = colors[findBit(randomConnectable.SwitchStates ^ startNode.SwitchStates)] };
                    edgesFrom[startNode].Add(newEdge);
                    edges.Add(newEdge);
                    goto startAgain;
                }
            }

            if (foundUnconnectable)
                Debugger.Break();

            // See if we can remove any edges without compromising reachability
            var reducedEdges = Ut.ReduceRequiredSet(edges, skipConsistencyTest: true, test: state =>
            {
                var tEdges = state.SetToTest.ToList();
                var tEdgesFrom = tEdges.ToLookup(e => e.From);

                // Go through all color combinations to make sure that everything is still reachable
                for (int ccIx = 0; ccIx < colorCombinations.Count; ccIx++)
                {
                    var cc = colorCombinations[ccIx];

                    if (ccIx % 1000 == 0)
                        Console.Write($"{ccIx}/{colorCombinations.Count}\r");

                    for (int i = 0; i < _numSwitches; i++)
                    {
                        colors[i] = cc % _numColors;
                        cc /= _numColors;
                    }

                    // Check all the nodes
                    foreach (var startNode in nodes)
                    {
                        // Which nodes are reachable from that node?
                        var reachable = new HashSet<Node>();
                        var q = new Queue<Node>();
                        q.Enqueue(startNode);
                        while (q.Count > 0)
                        {
                            var elem = q.Dequeue();
                            if (reachable.Add(elem))
                                foreach (var edge in tEdgesFrom[elem])
                                    if (edge.Color == colors[findBit(edge.From.SwitchStates ^ edge.To.SwitchStates)])
                                        q.Enqueue(edge.To);
                        }

                        // If a node is now unreachable, that edge cannot be removed.
                        if (reachable.Count < nodes.Length)
                            return false;
                    }
                }

                return true;
            });

            return ClassifyJson.Serialize(reducedEdges.ToList());
        }

        public static void JsonGraphToGraphML(JsonValue json)
        {
            var edges = ClassifyJson.Deserialize<List<Edge>>(json);
            var nodes = edges.Select(e => e.To).Distinct().ToArray();
            var edgesFrom = nodes.ToDictionary(node => node, node => new List<Edge>());
            foreach (var node in nodes)
                edgesFrom[node] = edges.Where(e => e.From == node).ToList();

            var xml = XElement.Parse(@"<graphml xmlns='http://graphml.graphdrawing.org/xmlns' xmlns:y='http://www.yworks.com/xml/graphml'>
              <key for='node' id='d6' yfiles.type='nodegraphics'/>
              <key for='edge' id='d10' yfiles.type='edgegraphics'/>
              <graph edgedefault='directed' id='G' />
            </graphml>");

            var g = xml.Elements().FirstOrDefault(e => e.Name.LocalName == "graph");
            var nNode = XName.Get("node", g.Name.NamespaceName);
            var nEdge = XName.Get("edge", g.Name.NamespaceName);
            var nData = XName.Get("data", g.Name.NamespaceName);
            XName nm(string name) => XName.Get(name, "http://www.yworks.com/xml/graphml");
            foreach (var node in nodes)
            {
                g.Add(
                    new XElement(nNode, new XAttribute("id", "n" + node.SwitchStates),
                        new XElement(nData, new XAttribute("key", "d6"),
                            new XElement(nm("ShapeNode"),
                                new XElement(nm("Geometry"), new XAttribute("height", "30"), new XAttribute("width", "45"), new XAttribute("x", "-15"), new XAttribute("y", "-15")),
                                new XElement(nm("NodeLabel"), node.ToString()),
                                new XElement(nm("Shape"), new XAttribute("type", "rectangle"))))));
            }
            var eIx = 0;
            var colorStrs = "#BB0000|#00AA00|#0000FF|#9900AA|#FF8800|#00AAEE".Split('|');
            while (edges.Count > 0)
            {
                var sameNodes = edgesFrom[edges[0].From].Where(e => e.To == edges[0].To).ToArray();
                if (sameNodes.Length == _numColors)
                {
                    g.Add(
                        new XElement(nEdge, new XAttribute("id", "e" + eIx), new XAttribute("source", "n" + edges[0].From.SwitchStates), new XAttribute("target", "n" + edges[0].To.SwitchStates),
                            new XElement(nData, new XAttribute("key", "d10"),
                                new XElement(nm("PolyLineEdge"),
                                    new XElement(nm("Arrows"), new XAttribute("source", "none"), new XAttribute("target", "standard")),
                                    new XElement(nm("LineStyle"), new XAttribute("color", "#000000"), new XAttribute("type", "line"), new XAttribute("width", "3.0"))))));
                    edges.RemoveRange(sameNodes);
                }
                else
                {
                    g.Add(
                        new XElement(nEdge, new XAttribute("id", "e" + eIx), new XAttribute("source", "n" + edges[0].From.SwitchStates), new XAttribute("target", "n" + edges[0].To.SwitchStates),
                            new XElement(nData, new XAttribute("key", "d10"),
                                new XElement(nm("PolyLineEdge"),
                                    new XElement(nm("Arrows"), new XAttribute("source", "none"), new XAttribute("target", "standard")),
                                    new XElement(nm("LineStyle"), new XAttribute("color", colorStrs[edges[0].Color]), new XAttribute("type", "line"), new XAttribute("width", "1.0"))))));
                    edges.RemoveAt(0);
                }
                eIx++;
            }
            File.WriteAllText($@"D:\temp\Color Switches ({_numSwitches} switches, {_numColors} colors).graphml", xml.ToString());
        }

        private static int findBit(int x)
        {
            int i = 0;
            while (x > 1)
            {
                i++;
                x >>= 1;
            }
            return i;
        }

        private static bool isPowerOf2(int x) => x != 0 && (x & (x - 1)) == 0;
    }
}
