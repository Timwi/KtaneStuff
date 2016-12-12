using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using RT.Util;
using RT.Util.ExtensionMethods;
using RT.Util.Geometry;

namespace KtaneStuff.Modeling
{
    using static Md;

    static class FollowTheLeader
    {
        public static void Do()
        {
            // HexFrame
            File.WriteAllText(@"D:\c\KTANE\FollowTheLeader\Assets\Assets\HexFrame.obj", GenerateObjFile(HexFrames(_outerHexFrame, _innerHexFrame), "Hexagonal frame"));
            // Wire connectors on the HexFrame
            File.WriteAllText(@"D:\c\KTANE\FollowTheLeader\Assets\Assets\Connectors.obj", GenerateObjFile(Connectors(_outerHexFrame, _innerHexFrame), "Connectors"));
            // Wire colliders
            for (int i = 0; i < 3; i++)
                File.WriteAllText($@"D:\c\KTANE\FollowTheLeader\Assets\Assets\WireCollider{i + 1}.obj", GenerateObjFile(WireCollider(i, _wireRadiusCollider), "WireCollider" + (i + 1)));
        }

        public const double _wireRadius = .002;
        public const double _wireRadiusHighlight = .004;
        public const double _wireRadiusCollider = .005;

        const double _hexFrameBoxOuterRadius = .004;
        const double _hexFrameBoxInnerRadius = .003;
        const double _hexFrameBoxHeight = .006;
        const double _hexFrameBoxLocationRatio = .6;

        const double _wireMaxSegmentDeviation = .005;
        const double _wireMaxBézierDeviation = .005;

        sealed class HexFrameInfo
        {
            public double InnerRadius, OuterRadius, Bevel, Depth, StartAngle;

            public double BoxCenterRadius { get { return InnerRadius * _hexFrameBoxLocationRatio + OuterRadius * (1 - _hexFrameBoxLocationRatio); } }
        }

        private static HexFrameInfo _outerHexFrame = new HexFrameInfo { InnerRadius = .058, OuterRadius = .070, Bevel = .003, Depth = .005, StartAngle = 0 };
        private static HexFrameInfo _innerHexFrame = new HexFrameInfo { InnerRadius = .032, OuterRadius = .044, Bevel = .003, Depth = .005, StartAngle = 30 };

        public static IEnumerable<VertexInfo[]> WireCollider(int lengthIndex, double thickness)
        {
            var length = getWireLength(lengthIndex);
            var start = pt(0, 0, 0);
            var startControl = pt(-length / 10, 0, .01);
            var endControl = pt(-length * 9 / 10, 0, .01);
            var end = pt(-length, 0, 0);
            var numSegments =
                lengthIndex == 0 ? 6 :
                lengthIndex == 1 ? 2 : 4;

            var bézierSteps = 8;
            var tubeRevSteps = 8;

            var interpolateStart = pt(0, 0, .007);
            var interpolateEnd = pt(-length, 0, .007);

            var intermediatePoints = Ut.NewArray(numSegments - 1, i => interpolateStart + (interpolateEnd - interpolateStart) * (i + 1) / numSegments);
            var deviations = Ut.NewArray(numSegments - 1, _ => pt(-.001, 0, 0) * _wireMaxBézierDeviation);

            var points =
                new[] { new { ControlBefore = default(Pt), Point = start, ControlAfter = startControl } }
                .Concat(intermediatePoints.Select((p, i) => new { ControlBefore = p - deviations[i], Point = p, ControlAfter = p + deviations[i] }))
                .Concat(new[] { new { ControlBefore = endControl, Point = end, ControlAfter = default(Pt) } })
                .SelectConsecutivePairs(false, (one, two) => Bézier(one.Point, one.ControlAfter, two.ControlBefore, two.Point, bézierSteps))
                .SelectMany((x, i) => i == 0 ? x : x.Skip(1))
                .ToArray();
            return tubeFromCurve(points, thickness, tubeRevSteps);
        }

        private static void getBoxCenter(int peg, out double x, out double z)
        {
            var rad = (peg % 2 == 0 ? _outerHexFrame : _innerHexFrame).BoxCenterRadius;
            x = rad * cos(30 * (4 - peg));
            z = rad * sin(30 * (4 - peg));
        }

        public static double GetWirePosAndAngle(int peg1, int peg2, out double x, out double z)
        {
            double x2, z2;
            getBoxCenter(peg1, out x, out z);
            getBoxCenter(peg2, out x2, out z2);
            return Math.Atan2(z2 - z, x2 - x) * 180 / Math.PI;
        }

        private static double getWireLength(int index)
        {
            var outer = pt(_outerHexFrame.BoxCenterRadius, _outerHexFrame.Depth, 0);
            var inner = pt(_innerHexFrame.BoxCenterRadius, _innerHexFrame.Depth, 0).RotateY(30);
            if (index == 0)
                return outer.Distance(outer.RotateY(60));
            else if (index == 1)
                return inner.Distance(inner.RotateY(60));
            else
                return outer.Distance(inner);
        }

        private static IEnumerable<VertexInfo[]> tubeFromCurve(Pt[] pts, double radius, int revSteps)
        {
            var normals = new Pt[pts.Length];
            normals[0] = ((pts[1] - pts[0]) * pt(0, 1, 0)).Normalize() * radius;
            for (int i = 1; i < pts.Length - 1; i++)
                normals[i] = normals[i - 1].ProjectOntoPlane((pts[i + 1] - pts[i]) + (pts[i] - pts[i - 1])).Normalize() * radius;
            normals[pts.Length - 1] = normals[pts.Length - 2].ProjectOntoPlane(pts[pts.Length - 1] - pts[pts.Length - 2]).Normalize() * radius;

            var axes = pts.Select((p, i) =>
                i == 0 ? new { Start = pts[0], End = pts[1] } :
                i == pts.Length - 1 ? new { Start = pts[pts.Length - 2], End = pts[pts.Length - 1] } :
                new { Start = p, End = p + (pts[i + 1] - p) + (p - pts[i - 1]) }).ToArray();

            return CreateMesh(false, true, Enumerable.Range(0, pts.Length)
                .Select(ix => new { Axis = axes[ix], Perp = pts[ix] + normals[ix], Point = pts[ix] })
                .Select(inf => Enumerable.Range(0, revSteps)
                    .Select(i => 360 * i / revSteps)
                    .Select(angle => inf.Perp.Rotate(inf.Axis.Start, inf.Axis.End, angle))
                    .Reverse().ToArray())
                .ToArray());
        }

        private static IEnumerable<Pt[]> HexFrames(params HexFrameInfo[] infs)
        {
            return infs.SelectMany(HexFrame);
        }

        private static IEnumerable<Pt[]> HexFrame(HexFrameInfo inf)
        {
            return circularSweep(new[] { p(inf.InnerRadius - inf.Bevel, 0), p(inf.InnerRadius, inf.Depth), p(inf.OuterRadius, inf.Depth), p(inf.OuterRadius + inf.Bevel, 0) }, 6, inf.StartAngle);
        }

        private static IEnumerable<Pt[]> Connectors(params HexFrameInfo[] infs)
        {
            return infs.SelectMany(Connector);
        }

        private static IEnumerable<Pt[]> Connector(HexFrameInfo inf)
        {
            var numV = 6;
            var middleRadius = inf.InnerRadius * _hexFrameBoxLocationRatio + inf.OuterRadius * (1 - _hexFrameBoxLocationRatio);
            var boxes = Enumerable.Range(0, numV)
                .Select(i => 360.0 * i / numV + inf.StartAngle)
                .Select(angle => new { Angle = angle, BoxCenter = pt(middleRadius * cos(angle), inf.Depth, middleRadius * sin(angle)) })
                .SelectMany(boxInf => circularSweep(new[] { p(0, 0), p(_hexFrameBoxInnerRadius, 0), p(_hexFrameBoxInnerRadius, _hexFrameBoxHeight), p(_hexFrameBoxOuterRadius, _hexFrameBoxHeight), p(_hexFrameBoxOuterRadius, 0) }, 4, boxInf.Angle).Move(boxInf.BoxCenter))
                .ToArray();
            return boxes;
        }

        private static Pt rotate(PointD p, double angle) => pt(p.X * cos(angle), p.Y, p.X * sin(angle));

        private static IEnumerable<Pt[]> circularSweep(IEnumerable<PointD> shape, int numSteps, double startAngle = 0, bool closed = false)
        {
            return Enumerable.Range(0, numSteps).Select(i => (360 * i / numSteps) + startAngle)
                .SelectConsecutivePairs(true, (angle1, angle2) => shape
                    .SelectConsecutivePairs(closed, (p1, p2) => new[] { rotate(p1, angle1), rotate(p1, angle2), rotate(p2, angle2), rotate(p2, angle1) }))
                .SelectMany(x => x);
        }

        static WireColor[] _whiteBlack = new[] { WireColor.White, WireColor.Black };
        static RuleInfo[] rules = Ut.NewArray(
            new RuleInfo
            {
                Name = "A or N",
                Formulation = "the previous wire is not yellow or blue or green",
                Function = (p3, p2, p1, p0) => !new[] { WireColor.Yellow, WireColor.Blue, WireColor.Green }.Contains(p1.WireColor)
            },
            new RuleInfo
            {
                Name = "B or O",
                Formulation = "the previous wire leads to an even numbered plug",
                Function = (p3, p2, p1, p0) => p1.ConnectedTo % 2 == 1
            },
            new RuleInfo
            {
                Name = "C or P",
                Formulation = "the previous wire should be cut",
                Function = (p3, p2, p1, p0) => p1.MustCut
            },
            new RuleInfo
            {
                Name = "D or Q",
                Formulation = "the previous wire is red or blue or black",
                Function = (p3, p2, p1, p0) => new[] { WireColor.Red, WireColor.Blue, WireColor.Black }.Contains(p1.WireColor)
            },
            new RuleInfo
            {
                Name = "E or R",
                Formulation = "two of the previous three wires shared a color",
                Function = (p3, p2, p1, p0) => p1.WireColor == p2.WireColor || p1.WireColor == p3.WireColor || p2.WireColor == p3.WireColor,
            },
            new RuleInfo
            {
                Name = "F or S",
                Formulation = "exactly one of the previous two wires were the same color as this wire",
                Function = (p3, p2, p1, p0) => (p0.WireColor == p1.WireColor) ^ (p0.WireColor == p2.WireColor),
            },
            new RuleInfo
            {
                Name = "G or T",
                Formulation = "the previous wire is yellow or white or green",
                Function = (p3, p2, p1, p0) => new[] { WireColor.Yellow, WireColor.White, WireColor.Green }.Contains(p1.WireColor)
            },
            new RuleInfo
            {
                Name = "H or U",
                Formulation = "the previous wire should not be cut",
                Function = (p3, p2, p1, p0) => !p1.MustCut
            },
            new RuleInfo
            {
                Name = "I or V",
                Formulation = "the previous wire skips a plug",
                Function = (p3, p2, p1, p0) => p1.DoesSkip
            },
            new RuleInfo
            {
                Name = "J or W",
                Formulation = "the previous wire is not white or black or red",
                Function = (p3, p2, p1, p0) => !new[] { WireColor.White, WireColor.Black, WireColor.Red }.Contains(p1.WireColor)
            },
            new RuleInfo
            {
                Name = "K or X",
                Formulation = "the previous two wires are different colors",
                Function = (p3, p2, p1, p0) => p1.WireColor != p2.WireColor,
            },
            new RuleInfo
            {
                Name = "L or Y",
                Formulation = "the previous wire does not lead to a position labeled 6 or less",
                Function = (p3, p2, p1, p0) => p1.ConnectedTo > 5,
            },
            new RuleInfo
            {
                Name = "M or Z",
                Formulation = "exactly one or neither of the previous two wires are white or black",
                Function = (p3, p2, p1, p0) => !(_whiteBlack.Contains(p1.WireColor) && _whiteBlack.Contains(p2.WireColor))
            }
        );

        class WireInfo
        {
            // 0–11
            public int ConnectedFrom;
            public int ConnectedTo;
            public bool DoesSkip { get { return ConnectedTo != (ConnectedFrom + 1) % 12; } }

            public WireColor WireColor;
            public bool MustCut;
            public string Justification;
            public RuleInfo Rule;

            public override string ToString()
            {
                return string.Format("Wire {0}-to-{1} ({2})", ConnectedFrom + 1, ConnectedTo + 1, WireColor.ToString());
            }

            public string ToStringFull()
            {
                return ToString() + "; " + Justification;
            }
        }

        enum WireColor { Red, Green, White, Yellow, Blue, Black }

        sealed class RuleInfo
        {
            public string Name;
            public string Formulation;
            public Func<WireInfo, WireInfo, WireInfo, WireInfo, bool> Function;
        }

        public static void SimulateFollowTheLeader()
        {
            int _numBatteries;
            string _serial;
            bool _hasRJ;
            bool _hasLitCLR;

            List<WireInfo> _wireInfos;
            List<WireInfo> _expectedCuts;

            int total = 0;
            var cutCounts = new Dictionary<int, int>();
            var wireCounts = new Dictionary<int, int>();
            var ruleApplied = new Dictionary<string, int>();
            var ruleAppliedCut = new Dictionary<RuleInfo, int>();
            var ruleAppliedNotCut = new Dictionary<RuleInfo, int>();
            var wireTotal = 0;

            for (int attempts = 0; attempts < 1000000; attempts++)
            {
                // ## START

                _expectedCuts = new List<WireInfo>();
                _wireInfos = new List<WireInfo>();
                var currentPeg = 0;
                do
                {
                    // Randomly skip a peg about 25% of the time, but we have to
                    // go back to peg #0 if we’re on peg #11. Also, we want at least
                    // 8 wires, so we need to stop skipping if we don’t have enough wires.
                    var skip = currentPeg >= 11 || currentPeg >= _wireInfos.Count + 4 ? false : Rnd.Next(0, 4) == 0;
                    var nextPeg = (currentPeg + (skip ? 2 : 1)) % 12;
                    _wireInfos.Add(new WireInfo
                    {
                        ConnectedFrom = currentPeg,
                        ConnectedTo = nextPeg,
                        WireColor = (WireColor) Rnd.Next(0, 6)
                    });
                    currentPeg = nextPeg;
                }
                while (currentPeg != 0);

                // Since the above will never skip peg #0, rotate the arrangement by a random amount
                // so that all pegs have equal chances of being skipped.
                var rotation = Rnd.Next(0, 12);
                for (int i = 0; i < _wireInfos.Count; i++)
                {
                    _wireInfos[i].ConnectedFrom = (_wireInfos[i].ConnectedFrom + rotation) % 12;
                    _wireInfos[i].ConnectedTo = (_wireInfos[i].ConnectedTo + rotation) % 12;
                }

                _serial = string.Join("", Enumerable.Range(0, 6).Select(i => Rnd.Next(0, 36)).Select(i => i < 10 ? ((char) ('0' + i)).ToString() : ((char) ('A' + i - 10)).ToString()).ToArray());
                _hasRJ = Rnd.Next(0, 2) == 0;
                _numBatteries = Rnd.Next(0, 7);
                _hasLitCLR = Rnd.Next(0, 2) == 0;

                // Figure out the starting wire (as index into wireInfos, rather than peg number)
                int startIndex;
                var serialFirstNumeral = _serial.Where(ch => ch >= '0' && ch <= '9').FirstOrNull();
                if (_hasRJ && (startIndex = _wireInfos.IndexOf(wi => wi.ConnectedFrom == 3 && wi.ConnectedTo == 4)) != -1)
                {
                }
                else if ((startIndex = _wireInfos.IndexOf(wi => wi.ConnectedFrom + 1 == _numBatteries)) != -1)
                {
                }
                else if (serialFirstNumeral != null && (startIndex = _wireInfos.IndexOf(wi => wi.ConnectedFrom + 1 == serialFirstNumeral.Value - '0')) != -1)
                {
                }
                else if (_hasLitCLR)
                {
                    foreach (var wi in _wireInfos)
                        wi.Justification = "CLR rule.";
                    _expectedCuts.Clear();
                    _expectedCuts.AddRange(_wireInfos.OrderByDescending(w => w.ConnectedFrom));
                    goto end;
                }
                else if ((startIndex = _wireInfos.IndexOf(wi => wi.ConnectedFrom == 0)) != -1)
                {
                }
                else
                    startIndex = _wireInfos.IndexOf(wi => wi.ConnectedFrom == 1);

                var curIndex = startIndex;
                // The starting step corresponds to the first letter in the serial number.
                var curStep = _serial.Where(ch => ch >= 'A' && ch <= 'Z').Select(ch => ch - 'A').FirstOrDefault() % rules.Length;
                // If the wire at the starting plug is red, green, or white, progress through the steps in reverse alphabetical order instead.
                var reverse = new[] { WireColor.Red, WireColor.Green, WireColor.White }.Contains(_wireInfos[curIndex].WireColor);

                _expectedCuts.Clear();

                // Finally, determine which wires need cutting
                for (int i = 0; i < _wireInfos.Count; i++)
                {
                    if (i == 0)
                    {
                        // Always cut the first one.
                        _wireInfos[curIndex].MustCut = true;
                        _wireInfos[curIndex].Justification = "Cut because this is the starting wire.";
                    }
                    else
                    {
                        _wireInfos[curIndex].MustCut = rules[curStep].Function(
                            _wireInfos[(curIndex + _wireInfos.Count - 3) % _wireInfos.Count],
                            _wireInfos[(curIndex + _wireInfos.Count - 2) % _wireInfos.Count],
                            _wireInfos[(curIndex + _wireInfos.Count - 1) % _wireInfos.Count],
                            _wireInfos[curIndex]
                        );
                        _wireInfos[curIndex].Justification = string.Format("Rule {0}: cut if {1} ⇒ {2}", rules[curStep].Name, rules[curStep].Formulation, _wireInfos[curIndex].MustCut ? "CUT" : "DON’T CUT");
                        _wireInfos[curIndex].Rule = rules[curStep];
                        curStep = (curStep + rules.Length + (reverse ? -1 : 1)) % rules.Length;
                    }

                    if (_wireInfos[curIndex].MustCut)
                        _expectedCuts.Add(_wireInfos[curIndex]);

                    curIndex = (curIndex + 1) % _wireInfos.Count;
                }

                end:;
                cutCounts.IncSafe(_expectedCuts.Count);
                wireCounts.IncSafe(_wireInfos.Count);
                foreach (var wi in _wireInfos)
                    if (wi.Rule != null)
                        (wi.MustCut ? ruleAppliedCut : ruleAppliedNotCut).IncSafe(wi.Rule);
                foreach (var ex in _expectedCuts)
                    ruleApplied.IncSafe(ex.Justification);
                total++;
                wireTotal += _wireInfos.Count;
            }

            Console.WriteLine("Probability of having to cut n wires in any one module:");
            foreach (var kvp in cutCounts.OrderBy(k => k.Key))
                Console.WriteLine($"{kvp.Key} wire{(kvp.Key < 2 ? "" : "s")}: {kvp.Value / (double) total * 100:0.##}% probability");
            Console.WriteLine();
            Console.WriteLine("Probability of seeing n wires in any one module:");
            foreach (var kvp in wireCounts.OrderBy(k => k.Key))
                Console.WriteLine($"{kvp.Key} wire{(kvp.Key < 2 ? "" : "s")}: {kvp.Value / (double) total * 100:0.##}% probability");
            Console.WriteLine();
            Console.WriteLine("Probability that each rule, when applied to a wire, mandates a cut:");
            foreach (var rule in rules)
                Console.WriteLine($"{ruleAppliedCut[rule] / (double) (ruleAppliedCut[rule] + ruleAppliedNotCut[rule]) * 100,-5:0.##}% probability {rule.Name}: {rule.Formulation}");
            Console.WriteLine();
            Console.WriteLine("Probability that each rule applies to any one wire in any one game:");
            foreach (var kvp in ruleApplied.OrderBy(k => k.Key))
                Console.WriteLine($"{kvp.Value / (double) wireTotal * 100,-5:0.##}% probability {kvp.Key}");
        }
    }
}
