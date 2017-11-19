using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using KtaneStuff.Modeling;
using RT.Util;
using RT.Util.Drawing;
using RT.Util.ExtensionMethods;

namespace KtaneStuff
{
    using static Md;

    static class PentagonalHexecontahedralMaze
    {
        public static void DoModels()
        {
            File.WriteAllText(@"D:\c\KTANE\PentagonalHexecontahedralMaze\Assets\Models\PentagonalHexecontahedron.obj", GenerateObjFile(PentagonalHexecontahedron(), "PentagonalHexecontahedron", AutoNormal.Flat));
        }

        public static IEnumerable<Pt[]> DeltoidalHexecontahedron()
        {
            const double C0 = 0.690983005625052575897706582817;
            const double C1 = 0.783457635340899531654962439488;
            const double C2 = 1.11803398874989484820458683437;
            const double C3 = 1.20601132958329828273486227812;
            const double C4 = 1.26766108272719625323969951590;
            const double C5 = 1.80901699437494742410229341718;
            const double C6 = 1.95136732208322818153792016770;
            const double C7 = 2.05111871806809578489466195539;
            const double C8 = 2.23606797749978969640917366873;

            var V0 = pt(0.0, 0.0, C8);
            var V1 = pt(0.0, 0.0, -C8);
            var V2 = pt(C8, 0.0, 0.0);
            var V3 = pt(-C8, 0.0, 0.0);
            var V4 = pt(0.0, C8, 0.0);
            var V5 = pt(0.0, -C8, 0.0);
            var V6 = pt(0.0, C1, C7);
            var V7 = pt(0.0, C1, -C7);
            var V8 = pt(0.0, -C1, C7);
            var V9 = pt(0.0, -C1, -C7);
            var V10 = pt(C7, 0.0, C1);
            var V11 = pt(C7, 0.0, -C1);
            var V12 = pt(-C7, 0.0, C1);
            var V13 = pt(-C7, 0.0, -C1);
            var V14 = pt(C1, C7, 0.0);
            var V15 = pt(C1, -C7, 0.0);
            var V16 = pt(-C1, C7, 0.0);
            var V17 = pt(-C1, -C7, 0.0);
            var V18 = pt(C3, 0.0, C6);
            var V19 = pt(C3, 0.0, -C6);
            var V20 = pt(-C3, 0.0, C6);
            var V21 = pt(-C3, 0.0, -C6);
            var V22 = pt(C6, C3, 0.0);
            var V23 = pt(C6, -C3, 0.0);
            var V24 = pt(-C6, C3, 0.0);
            var V25 = pt(-C6, -C3, 0.0);
            var V26 = pt(0.0, C6, C3);
            var V27 = pt(0.0, C6, -C3);
            var V28 = pt(0.0, -C6, C3);
            var V29 = pt(0.0, -C6, -C3);
            var V30 = pt(C0, C2, C5);
            var V31 = pt(C0, C2, -C5);
            var V32 = pt(C0, -C2, C5);
            var V33 = pt(C0, -C2, -C5);
            var V34 = pt(-C0, C2, C5);
            var V35 = pt(-C0, C2, -C5);
            var V36 = pt(-C0, -C2, C5);
            var V37 = pt(-C0, -C2, -C5);
            var V38 = pt(C5, C0, C2);
            var V39 = pt(C5, C0, -C2);
            var V40 = pt(C5, -C0, C2);
            var V41 = pt(C5, -C0, -C2);
            var V42 = pt(-C5, C0, C2);
            var V43 = pt(-C5, C0, -C2);
            var V44 = pt(-C5, -C0, C2);
            var V45 = pt(-C5, -C0, -C2);
            var V46 = pt(C2, C5, C0);
            var V47 = pt(C2, C5, -C0);
            var V48 = pt(C2, -C5, C0);
            var V49 = pt(C2, -C5, -C0);
            var V50 = pt(-C2, C5, C0);
            var V51 = pt(-C2, C5, -C0);
            var V52 = pt(-C2, -C5, C0);
            var V53 = pt(-C2, -C5, -C0);
            var V54 = pt(C4, C4, C4);
            var V55 = pt(C4, C4, -C4);
            var V56 = pt(C4, -C4, C4);
            var V57 = pt(C4, -C4, -C4);
            var V58 = pt(-C4, C4, C4);
            var V59 = pt(-C4, C4, -C4);
            var V60 = pt(-C4, -C4, C4);
            var V61 = pt(-C4, -C4, -C4);

            yield return new[] { V18, V0, V8, V32 };
            yield return new[] { V18, V32, V56, V40 };
            yield return new[] { V18, V40, V10, V38 };
            yield return new[] { V18, V38, V54, V30 };
            yield return new[] { V18, V30, V6, V0 };
            yield return new[] { V19, V1, V7, V31 };
            yield return new[] { V19, V31, V55, V39 };
            yield return new[] { V19, V39, V11, V41 };
            yield return new[] { V19, V41, V57, V33 };
            yield return new[] { V19, V33, V9, V1 };
            yield return new[] { V20, V0, V6, V34 };
            yield return new[] { V20, V34, V58, V42 };
            yield return new[] { V20, V42, V12, V44 };
            yield return new[] { V20, V44, V60, V36 };
            yield return new[] { V20, V36, V8, V0 };
            yield return new[] { V21, V1, V9, V37 };
            yield return new[] { V21, V37, V61, V45 };
            yield return new[] { V21, V45, V13, V43 };
            yield return new[] { V21, V43, V59, V35 };
            yield return new[] { V21, V35, V7, V1 };
            yield return new[] { V22, V2, V11, V39 };
            yield return new[] { V22, V39, V55, V47 };
            yield return new[] { V22, V47, V14, V46 };
            yield return new[] { V22, V46, V54, V38 };
            yield return new[] { V22, V38, V10, V2 };
            yield return new[] { V23, V2, V10, V40 };
            yield return new[] { V23, V40, V56, V48 };
            yield return new[] { V23, V48, V15, V49 };
            yield return new[] { V23, V49, V57, V41 };
            yield return new[] { V23, V41, V11, V2 };
            yield return new[] { V24, V3, V12, V42 };
            yield return new[] { V24, V42, V58, V50 };
            yield return new[] { V24, V50, V16, V51 };
            yield return new[] { V24, V51, V59, V43 };
            yield return new[] { V24, V43, V13, V3 };
            yield return new[] { V25, V3, V13, V45 };
            yield return new[] { V25, V45, V61, V53 };
            yield return new[] { V25, V53, V17, V52 };
            yield return new[] { V25, V52, V60, V44 };
            yield return new[] { V25, V44, V12, V3 };
            yield return new[] { V26, V4, V16, V50 };
            yield return new[] { V26, V50, V58, V34 };
            yield return new[] { V26, V34, V6, V30 };
            yield return new[] { V26, V30, V54, V46 };
            yield return new[] { V26, V46, V14, V4 };
            yield return new[] { V27, V4, V14, V47 };
            yield return new[] { V27, V47, V55, V31 };
            yield return new[] { V27, V31, V7, V35 };
            yield return new[] { V27, V35, V59, V51 };
            yield return new[] { V27, V51, V16, V4 };
            yield return new[] { V28, V5, V15, V48 };
            yield return new[] { V28, V48, V56, V32 };
            yield return new[] { V28, V32, V8, V36 };
            yield return new[] { V28, V36, V60, V52 };
            yield return new[] { V28, V52, V17, V5 };
            yield return new[] { V29, V5, V17, V53 };
            yield return new[] { V29, V53, V61, V37 };
            yield return new[] { V29, V37, V9, V33 };
            yield return new[] { V29, V33, V57, V49 };
            yield return new[] { V29, V49, V15, V5 };
        }

        public static IEnumerable<Pt[]> PentagonalHexecontahedron()
        {
            // face[0] is the tip, rest is counter-clockwise

            const double C0 = 0.192893711352359022108262546061;
            const double C1 = 0.218483370127321224365534157111;
            const double C2 = 0.374821658114562295266609516608;
            const double C3 = 0.567715369466921317374872062669;
            const double C4 = 0.728335176957191477360671629838;
            const double C5 = 0.755467260516595579705585253517;
            const double C6 = 0.824957552676275846265811111988;
            const double C7 = 0.921228888309550499468934175898;
            const double C8 = 0.959987701391583803994339068107;
            const double C9 = 1.13706613386050418840961998424;
            const double C10 = 1.16712343647533397917215468549;
            const double C11 = 1.22237170490362309266282747264;
            const double C12 = 1.27209628257581214613814794036;
            const double C13 = 1.52770307085850512136921113078;
            const double C14 = 1.64691794069037444140475745697;
            const double C15 = 1.74618644098582634573474528789;
            const double C16 = 1.86540131081769566577029161408;
            const double C17 = 1.88844538928366915418351670356;
            const double C18 = 1.97783896542021867236841272616;
            const double C19 = 2.097053835252087992403959052348;

            var V0 = pt(-C0, -C1, -C19);
            var V1 = pt(-C0, C1, C19);
            var V2 = pt(C0, C1, -C19);
            var V3 = pt(C0, -C1, C19);
            var V4 = pt(-C19, -C0, -C1);
            var V5 = pt(-C19, C0, C1);
            var V6 = pt(C19, C0, -C1);
            var V7 = pt(C19, -C0, C1);
            var V8 = pt(-C1, -C19, -C0);
            var V9 = pt(-C1, C19, C0);
            var V10 = pt(C1, C19, -C0);
            var V11 = pt(C1, -C19, C0);
            var V12 = pt(0.0, -C5, -C18);
            var V13 = pt(0.0, -C5, C18);
            var V14 = pt(0.0, C5, -C18);
            var V15 = pt(0.0, C5, C18);
            var V16 = pt(-C18, 0.0, -C5);
            var V17 = pt(-C18, 0.0, C5);
            var V18 = pt(C18, 0.0, -C5);
            var V19 = pt(C18, 0.0, C5);
            var V20 = pt(-C5, -C18, 0.0);
            var V21 = pt(-C5, C18, 0.0);
            var V22 = pt(C5, -C18, 0.0);
            var V23 = pt(C5, C18, 0.0);
            var V24 = pt(-C10, 0.0, -C17);
            var V25 = pt(-C10, 0.0, C17);
            var V26 = pt(C10, 0.0, -C17);
            var V27 = pt(C10, 0.0, C17);
            var V28 = pt(-C17, -C10, 0.0);
            var V29 = pt(-C17, C10, 0.0);
            var V30 = pt(C17, -C10, 0.0);
            var V31 = pt(C17, C10, 0.0);
            var V32 = pt(0.0, -C17, -C10);
            var V33 = pt(0.0, -C17, C10);
            var V34 = pt(0.0, C17, -C10);
            var V35 = pt(0.0, C17, C10);
            var V36 = pt(-C3, C6, -C16);
            var V37 = pt(-C3, -C6, C16);
            var V38 = pt(C3, -C6, -C16);
            var V39 = pt(C3, C6, C16);
            var V40 = pt(-C16, C3, -C6);
            var V41 = pt(-C16, -C3, C6);
            var V42 = pt(C16, -C3, -C6);
            var V43 = pt(C16, C3, C6);
            var V44 = pt(-C6, C16, -C3);
            var V45 = pt(-C6, -C16, C3);
            var V46 = pt(C6, -C16, -C3);
            var V47 = pt(C6, C16, C3);
            var V48 = pt(-C2, -C9, -C15);
            var V49 = pt(-C2, C9, C15);
            var V50 = pt(C2, C9, -C15);
            var V51 = pt(C2, -C9, C15);
            var V52 = pt(-C15, -C2, -C9);
            var V53 = pt(-C15, C2, C9);
            var V54 = pt(C15, C2, -C9);
            var V55 = pt(C15, -C2, C9);
            var V56 = pt(-C9, -C15, -C2);
            var V57 = pt(-C9, C15, C2);
            var V58 = pt(C9, C15, -C2);
            var V59 = pt(C9, -C15, C2);
            var V60 = pt(-C7, -C8, -C14);
            var V61 = pt(-C7, C8, C14);
            var V62 = pt(C7, C8, -C14);
            var V63 = pt(C7, -C8, C14);
            var V64 = pt(-C14, -C7, -C8);
            var V65 = pt(-C14, C7, C8);
            var V66 = pt(C14, C7, -C8);
            var V67 = pt(C14, -C7, C8);
            var V68 = pt(-C8, -C14, -C7);
            var V69 = pt(-C8, C14, C7);
            var V70 = pt(C8, C14, -C7);
            var V71 = pt(C8, -C14, C7);
            var V72 = pt(-C4, C12, -C13);
            var V73 = pt(-C4, -C12, C13);
            var V74 = pt(C4, -C12, -C13);
            var V75 = pt(C4, C12, C13);
            var V76 = pt(-C13, C4, -C12);
            var V77 = pt(-C13, -C4, C12);
            var V78 = pt(C13, -C4, -C12);
            var V79 = pt(C13, C4, C12);
            var V80 = pt(-C12, C13, -C4);
            var V81 = pt(-C12, -C13, C4);
            var V82 = pt(C12, -C13, -C4);
            var V83 = pt(C12, C13, C4);
            var V84 = pt(-C11, -C11, -C11);
            var V85 = pt(-C11, -C11, C11);
            var V86 = pt(-C11, C11, -C11);
            var V87 = pt(-C11, C11, C11);
            var V88 = pt(C11, -C11, -C11);
            var V89 = pt(C11, -C11, C11);
            var V90 = pt(C11, C11, -C11);
            var V91 = pt(C11, C11, C11);

            yield return new[] { V24, V36, V14, V2, V0 };
            yield return new[] { V24, V76, V86, V72, V36 };
            yield return new[] { V24, V52, V16, V40, V76 };
            yield return new[] { V24, V60, V84, V64, V52 };
            yield return new[] { V24, V0, V12, V48, V60 };
            yield return new[] { V25, V37, V13, V3, V1 };
            yield return new[] { V25, V77, V85, V73, V37 };
            yield return new[] { V25, V53, V17, V41, V77 };
            yield return new[] { V25, V61, V87, V65, V53 };
            yield return new[] { V25, V1, V15, V49, V61 };
            yield return new[] { V26, V38, V12, V0, V2 };
            yield return new[] { V26, V78, V88, V74, V38 };
            yield return new[] { V26, V54, V18, V42, V78 };
            yield return new[] { V26, V62, V90, V66, V54 };
            yield return new[] { V26, V2, V14, V50, V62 };
            yield return new[] { V27, V39, V15, V1, V3 };
            yield return new[] { V27, V79, V91, V75, V39 };
            yield return new[] { V27, V55, V19, V43, V79 };
            yield return new[] { V27, V63, V89, V67, V55 };
            yield return new[] { V27, V3, V13, V51, V63 };
            yield return new[] { V28, V41, V17, V5, V4 };
            yield return new[] { V28, V81, V85, V77, V41 };
            yield return new[] { V28, V56, V20, V45, V81 };
            yield return new[] { V28, V64, V84, V68, V56 };
            yield return new[] { V28, V4, V16, V52, V64 };
            yield return new[] { V29, V40, V16, V4, V5 };
            yield return new[] { V29, V80, V86, V76, V40 };
            yield return new[] { V29, V57, V21, V44, V80 };
            yield return new[] { V29, V65, V87, V69, V57 };
            yield return new[] { V29, V5, V17, V53, V65 };
            yield return new[] { V30, V42, V18, V6, V7 };
            yield return new[] { V30, V82, V88, V78, V42 };
            yield return new[] { V30, V59, V22, V46, V82 };
            yield return new[] { V30, V67, V89, V71, V59 };
            yield return new[] { V30, V7, V19, V55, V67 };
            yield return new[] { V31, V43, V19, V7, V6 };
            yield return new[] { V31, V83, V91, V79, V43 };
            yield return new[] { V31, V58, V23, V47, V83 };
            yield return new[] { V31, V66, V90, V70, V58 };
            yield return new[] { V31, V6, V18, V54, V66 };
            yield return new[] { V32, V46, V22, V11, V8 };
            yield return new[] { V32, V74, V88, V82, V46 };
            yield return new[] { V32, V48, V12, V38, V74 };
            yield return new[] { V32, V68, V84, V60, V48 };
            yield return new[] { V32, V8, V20, V56, V68 };
            yield return new[] { V33, V45, V20, V8, V11 };
            yield return new[] { V33, V73, V85, V81, V45 };
            yield return new[] { V33, V51, V13, V37, V73 };
            yield return new[] { V33, V71, V89, V63, V51 };
            yield return new[] { V33, V11, V22, V59, V71 };
            yield return new[] { V34, V44, V21, V9, V10 };
            yield return new[] { V34, V72, V86, V80, V44 };
            yield return new[] { V34, V50, V14, V36, V72 };
            yield return new[] { V34, V70, V90, V62, V50 };
            yield return new[] { V34, V10, V23, V58, V70 };
            yield return new[] { V35, V47, V23, V10, V9 };
            yield return new[] { V35, V75, V91, V83, V47 };
            yield return new[] { V35, V49, V15, V39, V75 };
            yield return new[] { V35, V69, V87, V61, V49 };
            yield return new[] { V35, V9, V21, V57, V69 };
        }

        public static void GenerateNets(IEnumerable<Pt[]> solid, string solidName)
        {
            for (int rndSeek = 0; rndSeek < 64; rndSeek++)
            {
                var rnd = new Random(rndSeek);
                const int width = 1280;
                const int height = 1024;
                GraphicsUtil.DrawBitmap(width, height, g =>
                {
                    g.Clear(Color.White);
                    g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
                    using (var tr = new GraphicsTransformer(g).Scale(60, 60).Translate(width / 2, height / 2))
                    {
                        var faces = solid.ToArray();

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
                            var newFaces1 = faces.Select(f => f.Select(p => p.Rotate(pt(0, 0, 0), rot, arcsin(rot.Length))).ToArray()).ToArray();
                            var newFaces2 = faces.Select(f => f.Select(p => p.Rotate(pt(0, 0, 0), rot, -arcsin(rot.Length))).ToArray()).ToArray();
                            faces = newFaces1[0].Sum(p => p.Z * p.Z) < newFaces2[0].Sum(p => p.Z * p.Z) ? newFaces1 : newFaces2;
                        }

                        var polyDraws = new List<Action>();

                        var q = new Queue<(int, Pt[][])>();
                        var done = new HashSet<int>();
                        var connectionMap = new List<(int face1, int edge1, int face2, int edge2)>();
                        var polygons = new PointF[faces.Length][];

                        q.Enqueue((0, faces));
                        while (q.Count > 0)
                        {
                            // At random, sometimes pick the second element instead of the first.
                            if (q.Count > 1 && rnd.Next(0, 2) == 0)
                            {
                                var x = q.Dequeue();
                                var y = q.Dequeue();
                                q.Enqueue(x);
                                q.Enqueue(y);
                            }
                            var (ix, rotatedSolid) = q.Dequeue();
                            if (!done.Add(ix))
                                continue;
                            polygons[ix] = rotatedSolid[ix].Select(pt => p(pt.X, pt.Y).ToPointF()).ToArray();

                            const double closeness = .0001;
                            bool sufficientlyClose(Pt p1, Pt p2) => Math.Abs(p1.X - p2.X) < closeness && Math.Abs(p1.Y - p2.Y) < closeness && Math.Abs(p1.Z - p2.Z) < closeness;
                            for (int i = 0; i < rotatedSolid[ix].Length; i++)
                            {
                                int vIx = -1;
                                // Find another face that has the same edge
                                var otherIx = rotatedSolid.IndexOf(fc =>
                                {
                                    vIx = fc.IndexOf(p => sufficientlyClose(p, rotatedSolid[ix][(i + 1) % rotatedSolid[ix].Length]));
                                    return vIx != -1 && sufficientlyClose(fc[(vIx + 1) % fc.Length], rotatedSolid[ix][i]);
                                });
                                if (vIx == -1 || otherIx == -1)
                                    Debugger.Break();

                                // Rotate about the edge so that the new face is on the X/Y plane (i.e. “roll” the solid)
                                var otherFace = rotatedSolid[otherIx];
                                var normal = (otherFace[2] - otherFace[1]) * (otherFace[0] - otherFace[1]);
                                var rot = normal.Normalize() * pt(0, 0, 1);
                                var newSolid = rotatedSolid.Select(face => face.Select(p => p.Rotate(otherFace[(vIx + 1) % otherFace.Length], otherFace[vIx], -arcsin(rot.Length))).ToArray()).ToArray();
                                q.Enqueue((otherIx, newSolid));
                                connectionMap.Add((ix, i, otherIx, vIx));
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
                            //g.FillPolygon(new SolidBrush(Color.CornflowerBlue), polygons[ix]);
                            g.DrawPolygon(new Pen(Color.Black, 1 / 30f), polygons[ix]);
                            g.DrawString(ix.ToString(), new Font("Calibri", .2f, FontStyle.Regular), Brushes.Black, new PointF(polygons[ix].Sum(p => p.X) / polygons[ix].Length, polygons[ix].Sum(p => p.Y) / polygons[ix].Length), new StringFormat { LineAlignment = StringAlignment.Center, Alignment = StringAlignment.Center });
                        }

                        //Console.WriteLine(connectionMap.JoinString("\n"));
                    }
                }).Save($@"D:\c\KTANE\PentagonalHexecontahedralMaze\Data\{solidName} ({rndSeek:000}).png");
                Console.WriteLine($"{solidName} ({rndSeek:000}) done.");
            }
        }
    }
}