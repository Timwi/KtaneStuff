using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using KtaneStuff.Modeling;
using RT.Util;
using RT.Util.ExtensionMethods;

namespace KtaneStuff
{
    using static Md;

    static class SimonSings
    {
        public static void DoModels()
        {
            const double whiteKeyWidth = .17;
            const double fullHeight = 1;
            const double blackKeyWidth = .1;
            const double blackHeight = .55;
            const double blackKeyOffset = .015;

            const double woX = .01;
            const double woY = -.015;
            const double woZ = .015;

            const double boX = .01;
            const double boY = -.05;
            const double boZFront = .05;
            const double boZBack = .025;

            var wOffsets = new[] { pt(-woX, woY, -woZ), pt(woX, woY, -woZ), pt(woX, woY, woZ), pt(-woX, woY, woZ) };
            var bOffsets = new[] { pt(boX, boY, boZBack), pt(-boX, boY, boZBack), pt(-boX, boY, -boZFront), pt(boX, boY, -boZFront) };

            var x = new Dictionary<char, double>();
            var y = new Dictionary<char, double>();
            void set(Dictionary<char, double> d, string str, Func<int, double> fnc, Func<char, char> interpret = null)
            {
                for (int i = 0; i < str.Length; i++)
                    d[interpret == null ? str[i] : interpret(str[i])] = fnc(i);
            }
            void setX(string str, Func<int, double> fnc, Func<char, char> interpret = null) => set(x, str, fnc, interpret);
            void setY(string str, Func<int, double> fnc, Func<char, char> interpret = null) => set(y, str, fnc, interpret);
            void setP(string str, Func<int, double> fncX, Func<int, double> fncY, Func<char, char> interpret = null) { setX(str, fncX, interpret); setY(str, fncY, interpret); }

            /*
                a b cd e f g hi jk l m
                ┌─┬─┬┬─┬─┬─┬─┬┬─┬┬─┬─┐
                │ │ ││ │ │ │ ││ ││ │ │
                │ │o││r│ │ │u││x││á│ │
                │ └┬┘└┬┘ │ └┬┘└┬┘└┬┘ │
                │ n│pq│s │ t│vw│yz│ð │
                │  │  │  │  │  │  │  │
                └──┴──┴──┴──┴──┴──┴──┘
                é  í  ø  ñ  ó  ö  ä  þ
             */
            setP("éíøñóöäþ", i => whiteKeyWidth * i, i => fullHeight);
            setY("abcdefghijklm", i => 0);
            foreach (var pair in "aé/oí/rø/fñ/uó/xö/áä/mþ".Split('/'))
                x[pair[0]] = x[pair[1]];

            foreach (var pair in "bcpn/í/-1;desq/ø/1;ghvt/ó/-1;ijyw/ö/0;klðz/ä/1".Split(';').Select(str => str.Split('/')).Select(arr => new { str = arr[0], src = arr[1][0], offset = double.Parse(arr[2]) }))
            {
                setX("03", i => x[pair.src] - blackKeyWidth / 2 + pair.offset * blackKeyOffset, ch => pair.str[ch - '0']);
                setX("12", i => x[pair.src] + blackKeyWidth / 2 + pair.offset * blackKeyOffset, ch => pair.str[ch - '0']);
            }
            setY("oruxánpqstvwyzð", i => blackHeight);

            Pt convertToPolar(Pt p) => (-1.25 - p.Z).Apply(r => ((-p.X) * 180 / (7 * whiteKeyWidth) + 90).Apply(θ => pt(r * cos(θ), p.Y, r * sin(θ))));

            foreach (var whiteKey in @"C=a3b4n4o4í1é2/D=c3d4q4r4ø1í2o3p3/E=e3f4ñ1ø2r3s3/F=f3g4t4u4ó1ñ2/G=h3i4w4x4ö1ó2u3v3/A=j3k4z4á4ä1ö2x3y3/B=l3m4þ1ä2á3ð3".Split('/').Select(str => str.Split('=')).Select((arr, ix) => new { index = ix, key = arr[0], data = arr[1].Split(2).Select(c => new { Point = pt(x[c[0]], 0, y[c[0]]), BevelDir = c[1] - '1' }).ToArray() }))
            {
                File.WriteAllText($@"D:\c\KTANE\SimonSings\Assets\Models\Key{whiteKey.key}.obj", GenerateObjFile(
                    CreateMesh(false, true, new[] { false, true }.Select(b => whiteKey.data.Select(tup => convertToPolar(-(b ? tup.Point + wOffsets[tup.BevelDir] : tup.Point)).WithMeshInfo(Normal.Mine, Normal.Mine, Normal.Mine, Normal.Mine)).ToArray()).ToArray())
                        .Concat(whiteKey.data.Select(tup => convertToPolar(-(tup.Point + wOffsets[tup.BevelDir]))).Select(pt => p(pt.X, pt.Z)).Reverse().Triangulate().Select(f => f.Select(v => pt(v.X, -woY, v.Y).WithNormal(0, 1, 0)).ToArray())),
                    $"Key{whiteKey.key}"));
                File.WriteAllText($@"D:\c\KTANE\SimonSings\Assets\Models\Key{whiteKey.key}Highlight.obj", GenerateObjFile(FlatText("♪", "Consolas", 1).Select(f => f.Select(p => convertToPolar(pt(-p.Location.X / 35 - whiteKeyWidth * (whiteKey.index + .525), -woY + .0001, -p.Location.Z / 20 - .8)).WithNormal(0, 1, 0)).Reverse().ToArray()), $"Key{whiteKey.key}Highlight"));
            }

            foreach (var blackKey in @"bcpn/desq/ghvt/ijyw/klðz".Split('/').Select((str, ix) => new { Str = str, Index = ix }))
            {
                var name = "C#/D#/F#/G#/A#".Split('/')[blackKey.Index];
                var data = blackKey.Str.Select((ch, ix) => new { Point = pt(x[ch], 0, y[ch]), BevelDir = ix });
                File.WriteAllText($@"D:\c\KTANE\SimonSings\Assets\Models\Key{name}.obj", GenerateObjFile(
                    CreateMesh(false, true, new[] { false, true }.Select(b => data.Select(tup => convertToPolar(-(b ? tup.Point + bOffsets[tup.BevelDir] : tup.Point)).WithMeshInfo(Normal.Mine, Normal.Mine, Normal.Mine, Normal.Mine)).ToArray()).ToArray())
                        .Concat(data.Select(tup => convertToPolar(-(tup.Point + bOffsets[tup.BevelDir]))).Select(pt => p(pt.X, pt.Z)).Reverse().Triangulate().Select(f => f.Select(v => pt(v.X, -boY, v.Y).WithNormal(0, 1, 0)).ToArray())),
                    $"Key{name}"));
                File.WriteAllText($@"D:\c\KTANE\SimonSings\Assets\Models\Key{name}Highlight.obj", GenerateObjFile(FlatText("♪", "Consolas", 1).Select(f => f.Select(p => convertToPolar(pt(-p.Location.X / 70 - x[blackKey.Str[0]] - blackKeyWidth / 2 - .002, -boY + .0001, -p.Location.Z / 25 - .32)).WithNormal(0, 1, 0)).Reverse().ToArray()), $"Key{name}Highlight"));
            }

            File.WriteAllText($@"D:\c\KTANE\SimonSings\Assets\Models\TorusInner.obj", GenerateObjFile(Torus(.225, .025, 36), "TorusInner"));
            File.WriteAllText($@"D:\c\KTANE\SimonSings\Assets\Models\TorusOuter.obj", GenerateObjFile(Torus(1.275, .05, 72), "TorusOuter"));
            File.WriteAllText($@"D:\c\KTANE\SimonSings\Assets\Models\TorusSmall.obj", GenerateObjFile(Torus(.1, .025, 36), "TorusSmall"));
            File.WriteAllText($@"D:\c\KTANE\SimonSings\Assets\Models\CenterHighlight.obj", GenerateObjFile(Annulus(.225 / 0.4, .35 / 0.4, 36), "CenterHighlight"));
        }
    }
}