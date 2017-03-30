using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using KtaneStuff.Modeling;
using RT.TagSoup;
using RT.Util;
using RT.Util.ExtensionMethods;

namespace KtaneStuff
{
    using static Md;

    static class OnlyConnect
    {
        public static void DoModels()
        {
            File.WriteAllText(@"D:\c\KTANE\OnlyConnect\Assets\Misc\Button.obj", GenerateObjFile(Button(), "Button"));
            File.WriteAllText(@"D:\c\KTANE\OnlyConnect\Assets\Misc\ButtonHighlight.obj", GenerateObjFile(ButtonHighlight(), "ButtonHighlight"));
        }

        private static IEnumerable<VertexInfo[]> Button()
        {
            const double width = 1;
            const double height = .6;
            const double bf = .01;
            const double depth = .05;
            const double middleF = .25;
            const int bézierSteps = 24;

            var patch = BézierPatch(
                pt(-width / 2, 0, -height / 2), pt(-width / 2 + bf, 0, -height / 2 - bf), pt(width / 2 - bf, 0, -height / 2 - bf), pt(width / 2, 0, -height / 2),
                pt(-width / 2 - bf, 0, -height / 2 + bf), pt(-width * middleF, depth, -height * middleF), pt(width * middleF, depth, -height * middleF), pt(width / 2 + bf, 0, -height / 2 + bf),
                pt(-width / 2 - bf, 0, height / 2 - bf), pt(-width * middleF, depth, height * middleF), pt(width * middleF, depth, height * middleF), pt(width / 2 + bf, 0, height / 2 - bf),
                pt(-width / 2, 0, height / 2), pt(-width / 2 + bf, 0, height / 2 + bf), pt(width / 2 - bf, 0, height / 2 + bf), pt(width / 2, 0, height / 2),
                bézierSteps);
            return CreateMesh(false, false, patch)
                .Select(arr => arr.Select(val => val.WithTexture((-val.Location.X + width / 2 + bf) / (width + 2 * bf), (val.Location.Z + height / 2 + bf) / (height + 2 * bf))).ToArray())
                .ToArray();
        }

        private static IEnumerable<VertexInfo[]> ButtonHighlight()
        {
            const double width = 1.1;
            const double height = .7;
            const double bf = .02;
            const double depth = .01;
            const double middleF = .25;
            const int bézierSteps = 24;

            var patch = BézierPatch(
                pt(-width / 2, 0, -height / 2), pt(-width / 2 + bf, 0, -height / 2 - bf), pt(width / 2 - bf, 0, -height / 2 - bf), pt(width / 2, 0, -height / 2),
                pt(-width / 2 - bf, 0, -height / 2 + bf), pt(-width * middleF, depth, -height * middleF), pt(width * middleF, depth, -height * middleF), pt(width / 2 + bf, 0, -height / 2 + bf),
                pt(-width / 2 - bf, 0, height / 2 - bf), pt(-width * middleF, depth, height * middleF), pt(width * middleF, depth, height * middleF), pt(width / 2 + bf, 0, height / 2 - bf),
                pt(-width / 2, 0, height / 2), pt(-width / 2 + bf, 0, height / 2 + bf), pt(width / 2 - bf, 0, height / 2 + bf), pt(width / 2, 0, height / 2),
                bézierSteps);
            return CreateMesh(false, false, patch)
                .Select(arr => arr.Select(val => val.WithTexture((-val.Location.X + width / 2 + bf) / (width + 2 * bf), (val.Location.Z + height / 2 + bf) / (height + 2 * bf))).Reverse().ToArray())
                .ToArray();
        }

        private static string[] _alphabetsRaw = @"
Albanian=abcçdeëfghijklmnopqrstuvxyz
Catalan=«dóna amor que seràs feliç!». això, il·lús company geniüt, ja és un lluït rètol blavís d’onze kwh.
Croatian=gojazni đačić s biciklom drži hmelj i finu vatu u džepu nošnje.
Czech=nechť již hříšné saxofony ďáblů rozezvučí síň úděsnými tóny waltzu, tanga a quickstepu.
Danish=høj bly gom vandt fræk sexquiz på wc
Esperanto=eble ĉiu kvazaŭ-deca fuŝĥoraĵo ĝojigos homtipon.
Estonian=põdur zagrebi tšellomängija-följetonist ciqo külmetas kehvas garaažis
Finnish=törkylempijävongahdus
French=le cœur déçu mais l'âme plutôt naïve, louÿs rêva de crapaüter en canoë au delà des îles, près du mälström où brûlent les novæ. buvez de ce whisky que le patron juge fameux.
German=victor jagt zwölf boxkämpfer quer über den großen sylter deich
Hungarian=jó foxim és don quijote húszwattos lámpánál ülve egy pár bűvös cipőt készít. 
Icelandic=svo hölt, yxna kýr þegði jú um dóp í fé á bæ. 
Latvian=muļķa hipiji mēģina brīvi nogaršot celofāna žņaudzējčūsku. 
Lithuanian=įlinkdama fechtuotojo špaga sublykčiojusi pragręžė apvalų arbūzą 
Polish=jeżu klątw, spłódź finom część gry hańb
Portuguese=luís argüia à júlia que «brações, fé, chá, óxido, pôr, zângão» eram palavras do português. 
Romanian=muzicologă în bej vând whisky și tequila, preț fix.
Spanish=benjamín pidió una bebida de kiwi y fresa; noé, sin vergüenza, la más exquisita champaña del menú.
Swedish=byxfjärmat föl gick på duvshowen.
Turkish=pijamalı hasta yağız şoföre çabucak güvendi.
Welsh=parciais fy jac codi baw hud llawn dŵr ger tŷ mabon. 
".Trim().Replace("\r", "").Split('\n');
        private static string[] _alphabets;
        private static string[] _alphabetNames;

        private const int _wallSize = 3;

        static OnlyConnect()
        {
            _alphabets = _alphabetsRaw.Select(s => s.Split('=')[1].Where(char.IsLetter).Distinct().Order().JoinString()).ToArray();
            _alphabetNames = _alphabetsRaw.Select(s => s.Split('=')[0]).ToArray();
        }

        public static void UpdateFiles()
        {
            var alphabets = Enumerable.Range(0, _alphabets.Length).ToDictionary(i => _alphabetNames[i], i => _alphabets[i]);

            //var uniqueLetters = alphabets.SelectMany(a => a.Value).Distinct().Where(l => alphabets.Count(k => k.Value.Contains(l)) == 1).ToHashSet();
            var liify = Ut.Lambda((char ch) => new LI(ch));
            var ulify = Ut.Lambda((IEnumerable<char> chs) => new TD { class_ = "letters" }._(new UL(chs.OrderBy(ch => ch.ToString().Normalize(NormalizationForm.FormD)).Select(liify))));

            @"D:\c\KTANE\HTML\Only Connect.html".ReplaceInFile("<!--##-->", "<!--###-->", alphabets
                .Select(kvp => new TR(new TH(kvp.Key), ulify(kvp.Value.Except("abcdefghijklmnopqrstuvwxyz")), ulify("abcdefghijklmnopqrstuvwxyz".Except(kvp.Value))))
                .JoinString("\r\n"));
            @"D:\c\KTANE\OnlyConnect\Assets\OnlyConnectModule.cs".ReplaceInFile("//!!", "//@@", alphabets.Select(kvp => $@"""{kvp.Value.Order().JoinString()}"", // {kvp.Key}").JoinString("\r\n"));
            @"D:\c\KTANE\OnlyConnect\Assets\OnlyConnectModule.cs".ReplaceInFile("//##", "//$$", alphabets.Select(kvp => $@"""{kvp.Key.CLiteralEscape()}"",").JoinString("\r\n"));
        }

        public static void GenerateWall()
        {
            retry:
            var wall = new char[_wallSize][];
            var names = new string[_wallSize];
            var availableAlphabets = _alphabets.Select((abc, i) => new { Letters = new HashSet<char>(abc), Name = _alphabetNames[i] }).ToList();

            // Generate a possible connecting wall.
            for (var i = 0; i < _wallSize; i++)
            {
                var index = Rnd.Next(0, availableAlphabets.Count);
                var alphabet = availableAlphabets[index].Letters;
                wall[i] = alphabet.ToList().Shuffle().Take(_wallSize).ToArray();
                names[i] = availableAlphabets[index].Name;
                availableAlphabets.RemoveAt(index);
                var others = availableAlphabets.Where(a => wall[i].All(a.Letters.Contains)).ToList();
                var allLetters = alphabet.Concat(others.SelectMany(o => o.Letters)).Distinct().ToArray();
                foreach (var remaining in availableAlphabets)
                    if (wall[i].Any(remaining.Letters.Contains))
                        foreach (var letter in allLetters)
                            remaining.Letters.Remove(letter);
                availableAlphabets.RemoveAll(s => s.Letters.Count < _wallSize);
                if (availableAlphabets.Count == 0)
                    goto retry;
            }

            // Make sure that the wall has a unique solution.
            if (CheckWallUnique(wall.SelectMany(row => row).ToArray(), 0, 0, new Stack<string>(), Enumerable.Range(0, _alphabets.Length).ToDictionary(ix => _alphabetNames[ix], ix => new HashSet<char>(_alphabets[ix]))).Distinct().Skip(1).Any())
                goto retry;

            Console.WriteLine(@"Connecting Wall solution:");
            for (int i = 0; i < _wallSize; i++)
                Console.WriteLine(@"{0} ({1})".Fmt(wall[i].JoinString(" "), names[i]));
        }

        public static void ExperimentWall()
        {
            foreach (var result in CheckWallUnique("õšžäönğış".ToCharArray(), 0, 0, new Stack<string>(), Enumerable.Range(0, _alphabets.Length).ToDictionary(ix => _alphabetNames[ix], ix => new HashSet<char>(_alphabets[ix]))))
            {
                Console.WriteLine(result);
                Console.WriteLine();
            }
        }

        private static IEnumerable<string> CheckWallUnique(char[] chs, int index, int subIndex, Stack<string> already, Dictionary<string, HashSet<char>> alphabets)
        {
            if (index == _wallSize * _wallSize)
            {
                var str = new string(chs);
                yield return Enumerable.Range(0, 3).Select(i => str.Substring(3 * i, 3) + "  " + already.Skip(2 - i).First()).JoinString("\n");
                yield break;
            }

            if (index % _wallSize == 0)
            {
                foreach (var kvp in alphabets)
                    if (kvp.Value.Contains(chs[index]) && !already.Contains(kvp.Key))
                    {
                        already.Push(kvp.Key);
                        foreach (var solution in CheckWallUnique(chs, index + 1, index + 1, already, alphabets))
                            yield return solution;
                        already.Pop();
                    }
            }
            else
            {
                var curAlph = alphabets[already.Peek()];
                for (int i = subIndex; i < _wallSize * _wallSize; i++)
                    if (curAlph.Contains(chs[i]))
                    {
                        var t = chs[i];
                        chs[i] = chs[index];
                        chs[index] = t;
                        foreach (var solution in CheckWallUnique(chs, index + 1, i + 1, already, alphabets))
                            yield return solution;
                        t = chs[i];
                        chs[i] = chs[index];
                        chs[index] = t;
                    }
            }
        }
    }
}
