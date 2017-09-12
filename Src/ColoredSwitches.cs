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
            const string name = "Candidate 1";
            //*
            var graphJson = GenerateGraph();
            File.WriteAllText($@"D:\temp\Colored Switches\{name}.json", graphJson.ToStringIndented());
            /*/
            var graphJson = JsonValue.Parse(File.ReadAllText($@"D:\temp\Colored Switches\{name}.json"));
            /**/

            var edges = ClassifyJson.Deserialize<List<Edge>>(graphJson);
            var nodes = edges.Select(e => e.To).Distinct().ToArray();
            var edgesFrom = nodes.ToDictionary(node => node, node => new List<Edge>());
            foreach (var node in nodes)
                edgesFrom[node] = edges.Where(e => e.From == node).ToList();

            var covered = new bool[_numSwitches];
            foreach (var edge in edges)
            {
                var b = edge.From.SwitchStates ^ edge.To.SwitchStates;
                var bit = 0;
                while (b > 1)
                {
                    bit++;
                    b >>= 1;
                }
                if (covered[bit])
                    continue;
                if (Enumerable.Range(0, _numColors).All(c => edges.Any(e => e.Color == c && e.From == edge.From && e.To == edge.To)))
                    continue;
                covered[bit] = true;
            }
            Console.WriteLine(covered.JoinString(", "));

            Console.WriteLine(nodes.Select(n => $"{n.SwitchStates}={edgesFrom[n].Select(e => $"{e.Color}>{e.To.SwitchStates}").JoinString("|")}").JoinString("\n"));
            //JsonGraphToGraphML(graphJson, name);
        }

        private static bool allNodesReachable(Node[] nodes, IEnumerable<Edge> edges)
        {
            var edgesFrom = new Dictionary<Node, List<Edge>>();
            foreach (var node in nodes)
                edgesFrom[node] = edges.Where(e => e.From == node).ToList();

            var colorPowers = new int[_numSwitches];
            colorPowers[0] = 1;
            for (int c = 1; c < _numSwitches; c++)
                colorPowers[c] = colorPowers[c - 1] * _numColors;
            var numColorCombinations = colorPowers[_numSwitches - 1] * _numColors;

            // For each node, test that all other nodes are reachable in any color combination
            var alreadyTested = new HashSet<Node>();
            foreach (var startNode in nodes.Shuffle())
            {
                var reachable = new Dictionary<Node, bool[]>();
                foreach (var node in nodes)
                    reachable[node] = node == startNode ? Ut.NewArray(numColorCombinations, _ => true) : new bool[numColorCombinations];

                var q = new Queue<Node>();
                q.Enqueue(startNode);
                while (q.Count > 0)
                {
                    var elem = q.Dequeue();
                    if (alreadyTested.Contains(elem) && reachable[elem].All(b => b))
                        goto isTrue;

                    var reachElem = reachable[elem];
                    foreach (var edge in edgesFrom[elem])
                    {
                        var reachEdgeTo = reachable[edge.To];
                        var any = false;
                        for (int cc = 0; cc < numColorCombinations; cc++)
                            if (reachElem[cc] && !reachEdgeTo[cc] && (cc / colorPowers[edge.Switch]) % _numColors == edge.Color)
                            {
                                reachEdgeTo[cc] = true;
                                any = true;
                            }
                        if (any)
                            q.Enqueue(edge.To);
                    }
                }

                if (reachable.Any(p => p.Value.Contains(false)))
                    return false;

                isTrue:;
                alreadyTested.Add(startNode);
            }

            return true;
        }

        private static bool canHaveEdge(Node one, Node two)
        {
            var xor = one.SwitchStates ^ two.SwitchStates;
            return xor != 0 && (xor & (xor - 1)) == 0;
        }

        class Node
        {
            public int SwitchStates;
            public override string ToString() => Convert.ToString(SwitchStates, 2).PadLeft(_numSwitches, '0');
            public override int GetHashCode() => SwitchStates;
        }

        class Edge : IClassifyObjectProcessor
        {
            public Node From { get; private set; }
            public Node To { get; private set; }
            public int Color { get; private set; }

            [ClassifyIgnore]
            public int Switch { get; private set; }

            public Edge(Node from, Node to, int color)
            {
                From = from;
                To = to;
                setSwitch();
                Color = color;
            }
            private Edge() { }  // Classify

            public override string ToString() => "{0} =({1})=> {2}".Fmt(From, Color, To);

            void IClassifyObjectProcessor.BeforeSerialize() { }
            void IClassifyObjectProcessor.AfterDeserialize() { setSwitch(); }

            private void setSwitch()
            {
                var bit = From.SwitchStates ^ To.SwitchStates;
                Switch = 0;
                while (bit > 1)
                {
                    Switch++;
                    bit >>= 1;
                }
            }
        }

        const int _numSwitches = 5;
        const int _numColors = 6;

        public static JsonValue GenerateGraph()
        {
            var nodes = Ut.NewArray(1 << _numSwitches, ix => new Node { SwitchStates = ix });
            var edges = new List<Edge>();

            foreach (var node in nodes)
                foreach (var node2 in nodes)
                    if (node2 != node && isPowerOf2(node.SwitchStates ^ node2.SwitchStates))
                        for (int c = 0; c < _numColors; c++)
                            edges.Add(new Edge(node, node2, c));

            // Remove as many edges as possible without compromising reachability
            edges.Shuffle();
            var reducedEdges = Ut.ReduceRequiredSet(edges, skipConsistencyTest: true, test: state => allNodesReachable(nodes, state.SetToTest));
            return ClassifyJson.Serialize(reducedEdges.ToList());
        }

        public static void JsonGraphToGraphML(JsonValue json, string filenamePart)
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
            var colorStrs = "#BB0000|#00AA00|#0000FF|#C000DD|#FF8800|#00D2EE".Split('|');
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
            File.WriteAllText($@"D:\temp\Colored Switches\{filenamePart}.graphml", xml.ToString());
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
