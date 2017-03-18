using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using RT.TagSoup;
using RT.Util;
using RT.Util.ExtensionMethods;

namespace KtaneStuff.Modeling
{
    using static Md;

    static class OnlyConnect
    {
        public static void Do()
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

        public static void UpdateFiles()
        {
            var alphabetsRaw = @"
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
".Trim();
            var alphabets = alphabetsRaw.Replace("\r", "").Split('\n')
                .ToDictionary(str => str.Split('=')[0], str => str.Split('=')[1].Where(char.IsLetter).Distinct().Order().JoinString());

            var liify = Ut.Lambda((char ch) => new LI { class_ = "letter" }._(ch));
            var ulify = Ut.Lambda((IEnumerable<char> chs) => new TD { class_ = "letters" }._(new UL { class_ = "letters" }._(chs.OrderBy(ch => ch.ToString().Normalize(NormalizationForm.FormD)).Select(liify))));
            
            @"D:\c\KTANE\HTML\Only Connect.html".ReplaceInFile("<!--##-->", "<!--###-->", alphabets
                .Select(kvp=> new TR(new TH(kvp.Key), ulify(kvp.Value.Except("abcdefghijklmnopqrstuvwxyz")), ulify("abcdefghijklmnopqrstuvwxyz".Except(kvp.Value))))
                .JoinString("\r\n"));
            @"D:\c\KTANE\OnlyConnect\Assets\OnlyConnectModule.cs".ReplaceInFile("//!!", "//@@", alphabets.Select(kvp => $@"""{kvp.Value.Order().JoinString()}"", // {kvp.Key}").JoinString("\r\n"));
            @"D:\c\KTANE\OnlyConnect\Assets\OnlyConnectModule.cs".ReplaceInFile("//##", "//$$", alphabets.Select(kvp => $@"""{kvp.Key.CLiteralEscape()}"",").JoinString("\r\n"));

            //// Big table with separate columns for all letters
            //var allLettersHash = alphabets.SelectMany(kvp => kvp.Value).Distinct().ToHashSet();
            //allLettersHash.RemoveWhere(ch => alphabets.All(a => a.Value.Contains(ch)));
            //var allLetters = allLettersHash.OrderBy(ch => ch.ToString().Normalize(NormalizationForm.FormD)).ToArray();
            //@"D:\c\KTANE\HTML\Only Connect.html".ReplaceInFile("<!--##-->", "<!--###-->", new TABLE { class_ = "only-connect-wall" }._(
            //    // Header row
            //    new TR(new TH("Language"), allLetters.Select((ch, f, l) => new TH { class_ = "letter" + (f ? " first" : "") + (l ? " last" : "") }._(ch))),
            //    alphabets.OrderBy(l => l.Key).Select(kvp => new TR(
            //        new TH(kvp.Key), allLetters.Select((ch, f, l) => new TD { class_ = "letter" + (f ? " first" : "") + (l ? " last" : "") }._(kvp.Value.Contains(ch) ? ch.ToString() : null))
            //    ))
            //).ToString());
        }
    }
}
