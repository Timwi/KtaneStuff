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

    static class SimonScreams
    {
        public static void Do()
        {
            File.WriteAllText(@"D:\c\KTANE\SimonScreams\Assets\Models\Button.obj", GenerateObjFile(Button(), "Button"));
            File.WriteAllText(@"D:\c\KTANE\SimonScreams\Assets\Models\ButtonHighlight.obj", GenerateObjFile(ButtonHighlight(), "ButtonHighlight"));
            File.WriteAllText(@"D:\c\KTANE\SimonScreams\Assets\Models\ButtonCollider.obj", GenerateObjFile(ButtonCollider(), "ButtonCollider"));

            var flapIx = 0;
            foreach (var flap in Flaps())
            {
                File.WriteAllText($@"D:\c\KTANE\SimonScreams\Assets\Models\Flap{flapIx}.obj", GenerateObjFile(flap, $"Flap{flapIx}"));
                flapIx++;
            }
        }

        private static IEnumerable<VertexInfo[]> Button()
        {
            var height = .2;
            var fh = height * .4;
            var innerRadius = 0.4;
            var outerRadius = 1.0;
            var fr = innerRadius * .1;
            var angle = 30.0;

            var bézierSteps = 20;
            var d = Math.Sqrt(innerRadius * innerRadius + outerRadius * outerRadius - 2 * innerRadius * outerRadius * cos(angle));
            var frix = outerRadius - (outerRadius - innerRadius * cos(angle)) / d * (d - fr);
            var friy = innerRadius * sin(angle) / d * (d - fr);
            var frox = outerRadius - (outerRadius - innerRadius * cos(angle)) / d * fr;
            var froy = innerRadius * sin(angle) / d * fr;

            var patchPiece = BézierPatch(
                pt(0, 0, 0), pt(fr * cos(-angle), fh, fr * sin(-angle)), pt((innerRadius - fr) * cos(-angle), fh, (innerRadius - fr) * sin(-angle)), pt(innerRadius * cos(-angle), 0, innerRadius * sin(-angle)),
                pt(fr * cos(angle), fh, fr * sin(angle)), pt(innerRadius / 2, height, 0), pt(innerRadius * cos(angle / 2), height, innerRadius * sin(angle / 2)), pt(frix, fh, -friy),
                pt((innerRadius - fr) * cos(angle), fh, (innerRadius - fr) * sin(angle)), pt(innerRadius * cos(angle / 2), height, innerRadius * sin(angle / 2)), pt((innerRadius + outerRadius) / 2, height, 0), pt(frox, fh, -froy),
                pt(innerRadius * cos(angle), 0, innerRadius * sin(angle)), pt(frix, fh, friy), pt(frox, fh, froy), pt(outerRadius, 0, 0),
                bézierSteps);
            return CreateMesh(false, false, Ut.NewArray(bézierSteps, bézierSteps, (x, y) => new MeshVertexInfo(patchPiece[x][y].Add(x: .03), x == bézierSteps - 1 ? Normal.Mine : Normal.Average, x == 0 ? Normal.Mine : Normal.Average, y == bézierSteps - 1 ? Normal.Mine : Normal.Average, y == 0 ? Normal.Mine : Normal.Average)));
        }

        private static IEnumerable<VertexInfo[]> ButtonHighlight()
        {
            var preRadius = .03;
            var innerRadius = .44;
            var outerRadius = 1.15;
            var angle = 32.0;

            var holeInnerRadius = .4;
            var holeOuterRadius = 1.0;
            var holeAngle = 30.0;

            return
                Triangulate(Ut.NewArray<IEnumerable<PointD>>(
                    new[] { p(-preRadius, 0), p(innerRadius * cos(-angle), innerRadius * sin(-angle)), p(outerRadius, 0), p(innerRadius * cos(angle), innerRadius * sin(angle)) },
                    new[] { p(0, 0), p(holeInnerRadius * cos(holeAngle), holeInnerRadius * sin(holeAngle)), p(holeOuterRadius, 0), p(holeInnerRadius * cos(-holeAngle), holeInnerRadius * sin(-holeAngle)) }.Select(p => p + new PointD(.03, 0))
                ))
                    .Select(poly => poly.Select(p => pt(p.X, 0, p.Y).WithNormal(0, 1, 0)).ToArray());
        }

        private static IEnumerable<VertexInfo[]> ButtonCollider()
        {
            var innerRadius = .44;
            var outerRadius = 1.1;
            var angle = 30.0;

            yield return
                new[] { p(0, 0), p(innerRadius * cos(angle), innerRadius * sin(angle)), p(outerRadius, 0), p(innerRadius * cos(-angle), innerRadius * sin(-angle)) }
                    .Select(p => pt(p.X, 0, p.Y).WithNormal(0, 1, 0)).ToArray();
        }
        
        private static IEnumerable<IEnumerable<VertexInfo[]>> Flaps()
        {
            var innerRadius = 0.4;
            var outerRadius = 1.02;
            var angle = 30.0;
            var depth = .01;
            var offset = .025;

            var outline = new[] { p(0, 0), p(innerRadius * cos(angle), innerRadius * sin(angle)), p(outerRadius, 0), p(innerRadius * cos(-angle), innerRadius * sin(-angle)) };
            var midPoint = Intersect.LineWithLine(new EdgeD(outline[0], outline[2]), new EdgeD(outline[1], outline[3]));

            for (int i = 0; i < 6; i++)
            {
                foreach (var face in outline.SelectConsecutivePairs(true, (p1, p2) => new[] { .96 * midPoint + .02 * (p1 + p2), p1, p2 }.Select(p => pt(p.X + offset, 0, p.Y).RotateY(60 * i - 15))))
                {
                    var flap = new List<VertexInfo[]>();

                    // Front face
                    var frontFace = face.Select(p => p.WithNormal(0, 1, 0).WithTexture(new PointD(.4771284794 * (-p.X * .8) + .46155, -.4771284794 * (-p.Z * .8) + .5337373145))).ToArray();
                    flap.Add(frontFace);
                    // Back face
                    flap.Add(frontFace.Select(p => p.Location.Add(y: -depth).WithNormal(0, -1, 0).WithTexture(p.Texture.Value)).Reverse().ToArray());

                    // Side faces
                    flap.AddRange(face.SelectConsecutivePairs(true, (p1, p2) => new[] { p2, p1, p1.Add(y: -depth), p2.Add(y: -depth) }.Select(p => p.WithNormal((p2 - p1) * (p1 - p1.Add(y: -depth)))).ToArray()));
                    yield return flap;
                }
            }
        }
    }
}
