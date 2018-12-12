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

    static class MaritimeFlags
    {
        public static void DoManual()
        {
            var words = @"1STMATE;2NDMATE;3RDMATE;ABANDON;ADMIRAL;ADVANCE;AGROUND;ALLIDES;ANCHORS;ATHWART;AZIMUTH;BAILERS;BALLAST;BARRACK;BEACHED;BEACONS;BEAMEND;BEAMSEA;BEARING;BEATING;BELAYED;BERMUDA;BOBSTAY;BOILERS;BOLLARD;BONNETS;BOOMKIN;BOUNDER;BOWLINE;BRAILED;BREADTH;BRIDGES;BRIGGED;BRINGTO;BULWARK;BUMBOAT;BUMPKIN;BURTHEN;CABOOSE;CAPSIZE;CAPSTAN;CAPTAIN;CARAVEL;CAREENS;CARRACK;CARRIER;CATBOAT;CATHEAD;CHAINED;CHANNEL;CHARLEY;CHARTER;CITADEL;CLEARED;CLEATED;CLINKER;CLIPPER;COAMING;COASTED;CONSORT;CONVOYS;CORINTH;COTCHEL;COUNTER;CRANZES;CREWING;CRINGLE;CROJACK;CRUISER;CUTTERS;DANDIES;DEADRUN;DEBUNKS;DERRICK;DIPPING;DISRATE;DOGVANE;DOLDRUM;DOLPHIN;DRAUGHT;DRIFTER;DROGUES;DRYDOCK;DUNNAGE;DUNSELS;EARINGS;ECHELON;EMBAYED;ENSIGNS;ESCORTS;FAIRWAY;FALKUSA;FANTAIL;FARDAGE;FATHOMS;FENDERS;FERRIES;FITTING;FLANKED;FLARING;FLATTOP;FLEMISH;FLOATED;FLOORED;FLOTSAM;FOLDING;FOLLOWS;FORCING;FORWARD;FOULIES;FOUNDER;FRAMING;FREIGHT;FRIGATE;FUNNELS;FURLING;GALLEON;GALLEYS;GALLIOT;GANGWAY;GARBLED;GENERAL;GEORGES;GHOSTED;GINPOLE;GIVEWAY;GONDOLA;GRAVING;GRIPIES;GROUNDS;GROWLER;GUINEAS;GUNDECK;GUNPORT;GUNWALE;HALYARD;HAMMOCK;HAMPERS;HANGARS;HARBORS;HARBOUR;HAULING;HAWSERS;HEADING;HEADSEA;HEAVING;HERRING;HOGGING;HOLIDAY;HUFFLER;INBOARD;INIRONS;INSHORE;INSTAYS;INWATER;INWAYOF;JACKIES;JACKTAR;JENNIES;JETTIES;JIGGERS;JOGGLES;JOLLIES;JURYRIG;KEELSON;KELLETS;KICKING;KILLICK;KITCHEN;LANYARD;LAYDAYS;LAZARET;LEEHELM;LEESIDE;LEEWARD;LIBERTY;LIGHTER;LIZARDS;LOADING;LOCKERS;LOFTING;LOLLING;LOOKOUT;LUBBERS;LUFFING;LUGGERS;LUGSAIL;MAEWEST;MANOWAR;MARCONI;MARINER;MATELOT;MIZZENS;MOORING;MOUSING;NARROWS;NIPPERS;OFFICER;OFFPIER;OILSKIN;OLDSALT;ONBOARD;OREBOAT;OUTHAUL;OUTWARD;PAINTER;PANTING;PARCELS;PARLEYS;PARRELS;PASSAGE;PELAGIC;PENDANT;PENNANT;PICKETS;PINNACE;PINTLES;PIRATES;PIVOTED;PURSERS;PURSUED;QUARTER;QUAYING;RABBETS;RATLINE;REDUCED;REEFERS;REPAIRS;RIGGING;RIPRAPS;ROMPERS;ROWLOCK;RUDDERS;RUFFLES;RUMMAGE;SAGGING;SAILORS;SALTIES;SALVORS;SAMPANS;SAMPSON;SCULLED;SCUPPER;SCUTTLE;SEACOCK;SEALING;SEEKERS;SERVING;SEXTANT;SHELTER;SHIPPED;SHIPRIG;SICKBAY;SKIPPER;SKYSAIL;SLINGED;SLIPWAY;SNAGGED;SNOTTER;SPLICED;SPLICES;SPONSON;SPONSOR;SPRINGS;SQUARES;STACKIE;STANDON;STARTER;STATION;STEAMER;STEERED;STEEVES;STEWARD;STOPPER;STOVEIN;STOWAGE;STRIKES;SUNFISH;SWIMMIE;SYSTEMS;TACKING;THWARTS;TINCLAD;TOMPION;TONNAGE;TOPMAST;TOPSAIL;TORPEDO;TOSSERS;TRADING;TRAFFIC;TRAMPER;TRANSOM;TRAWLER;TRENAIL;TRENNEL;TRIMMER;TROOPER;TRUNNEL;TUGBOAT;TURNTWO;UNSHIPS;UPBOUND;VESSELS;VOICING;VOYAGER;WEATHER;WHALERS;WHARVES;WHELKIE;WHISTLE;WINCHES;WINDAGE;WORKING;YARDARM"
                .Split(';');
            var rnd = new Random(47);
            var values = Enumerable.Range(0, 360).Where(i => i % 8 != 0).ToList().Shuffle(rnd);
            if (words.Length != values.Count)
                Debugger.Break();

            Utils.ReplaceInFile(@"D:\c\KTANE\Public\HTML\Maritime Flags.html", "<!-- s-r-s -->", "<!-- s-r-e -->",
                values.Zip(words, (v, w) => $"<div>{w}={v}</div>").JoinString());
        }

        public static bool IsValid(string word)
        {
            for (int i = 1; i < word.Length; i++)
            {
                if (word.Substring(0, i).LastIndexOf(word[i]) >= 4)
                    return false;
            }
            return true;
        }

        public static void DoModels()
        {
            var (frame, dial) = Compass();
            File.WriteAllText(@"D:\c\KTANE\MaritimeFlags\Assets\Models\Compass.obj", GenerateObjFile(frame, "Compass"));
            File.WriteAllText(@"D:\c\KTANE\MaritimeFlags\Assets\Models\CompassBase.obj", GenerateObjFile(dial, "CompassBase"));
            File.WriteAllText(@"D:\c\KTANE\MaritimeFlags\Assets\Models\CompassNeedle.obj", GenerateObjFile(CompassNeedle(), "CompassNeedle"));
        }

        private static IEnumerable<VertexInfo[]> CompassNeedle()
        {
            var w = .3;
            var h = .7;
            yield return new[] { pt(-w, 0, 0), pt(w, 0, 0), pt(0, 0, -h) }.FlatNormals();
        }

        private static (IEnumerable<VertexInfo[]> frame, IEnumerable<VertexInfo[]> dial) Compass()
        {
            const double rO = 1; // outer radius
            const double rI = .9; // inner radius
            const double rT = .75; // tick radius
            const double hT = .1;   // height of the top of a tick
            const double hP = .2;   // height of the top of the protrusions
            const double hW = .15;   // height of the between-walls
            const double aT = 6; // tick angle
            const double v = 360.0 / 16;

            var rnd = new Random(47);

            var data = Enumerable.Range(0, 16).Select(i =>
            {
                Pt r(double angle, double rds, double y) => pt(rds * cos(angle), y, rds * sin(angle));

                VertexInfo[] addTexture(VertexInfo[] face)
                {
                    const double txRadius = .05;
                    //var mid = p(rnd.NextDouble() * (1 - 2 * txRadius) + txRadius, rnd.NextDouble() * (1 - 2 * txRadius) + txRadius);
                    var mid = p(face[0].Location.X.Clip(txRadius, 1 - txRadius), face[0].Location.Z.Clip(txRadius, 1 - txRadius));
                    var angle = rnd.NextDouble() * 360;
                    return face.Select((vt, ix) => vt.WithTexture(mid + txRadius * p(cos(angle + ix * 360 / face.Length), sin(angle + ix * 360 / face.Length)))).ToArray();
                }

                var aL = v * i - aT / 2;        // angle of the left side of the tick and protrusion
                var aMid = v * i;               // angle of the front of the tick
                var aR = v * i + aT / 2;        // angle of the right side of the tick and protrusion
                var aN = v * (i + 1) - aT / 2;  // angle of the left side of the NEXT tick, i.e. end of the between-wall

                var framePieces = Ut.NewArray(
                    // left face of the protrusion on the rim
                    new[] { r(aL, rI, hT), r(aL, rO, hT), r(aL, rO, hP), r(aL, rI, hP) },
                    // right face of the protrusion on the rim
                    new[] { r(aR, rO, hT), r(aR, rI, hT), r(aR, rI, hP), r(aR, rO, hP) },
                    // back face behind the protrusion
                    new[] { r(aR, rO, 0), r(aR, rO, hP), r(aL, rO, hP), r(aL, rO, 0) },
                    // top face of the protrusion
                    new[] { r(aL, rI, hP), r(aL, rO, hP), r(aR, rO, hP), r(aR, rI, hP) },
                    // front face of the protrusion
                    new[] { r(aL, rI, hT), r(aL, rI, hP), r(aR, rI, hP), r(aR, rI, hT) },
                    // top face of the tick
                    new[] { r(aL, rI, hT), r(aR, rI, hT), r(aMid, rT, hT) },
                    // left face of the tick
                    new[] { r(aL, rI, 0), r(aL, rI, hT), r(aMid, rT, hT), r(aMid, rT, 0) },
                    // right face of the tick
                    new[] { r(aMid, rT, 0), r(aMid, rT, hT), r(aR, rI, hT), r(aR, rI, 0) },
                    // front face of the between-wall
                    new[] { r(aR, rI, 0), r(aR, rI, hW), r(aN, rI, hW), r(aN, rI, 0) },
                    // top face of the between-wall
                    new[] { r(aR, rI, hW), r(aR, rO, hW), r(aN, rO, hW), r(aN, rI, hW) },
                    // back face of the between-wall
                    new[] { r(aN, rO, 0), r(aN, rO, hW), r(aR, rO, hW), r(aR, rO, 0) }
                )
                    .Select(f => addTexture(f.Reverse().ToArray().FlatNormals()))
                    .ToArray();

                var dialPieces = Ut.NewArray(
                    new[] { r(0, 0, 0), r(aL, rI, 0), r(aR, rI, 0) },
                    new[] { r(0, 0, 0), r(aR, rI, 0), r(aN, rI, 0) }
                )
                    .Select(f => f.Reverse().ToArray().FlatNormals().Select(p => p.WithTexture((p.Location.X + 1) / 2 * 45 / 85, (p.Location.Z + 1) / 2)).ToArray())
                    .ToArray();

                return (framePieces, dialPieces);
            });

            return (data.SelectMany(x => x.framePieces), data.SelectMany(x => x.dialPieces));
        }

        public static void DoGraphics()
        {
            var names = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789".Select(ch => ch.ToString()).Concat("R1,R2,R3,R4".Split(',')).ToArray();
            using (var bmp = new Bitmap(@"D:\c\KTANE\MaritimeFlags\Data\Flags.png"))
            {
                var w = bmp.Width / 8;
                var h = bmp.Height / 5;
                for (int x = 0; x < 8; x++)
                    for (int y = 0; y < 5; y++)
                    {
                        GraphicsUtil.DrawBitmap(w, h, g =>
                        {
                            g.Clear(Color.Transparent);
                            g.DrawImage(bmp, -x * w, -y * h);
                        }).Save($@"D:\c\KTANE\MaritimeFlags\Assets\Textures\Flag-{names[y * 8 + x]}-tmp.png");
                    }
            }
            Enumerable.Range(0, 5 * 8).ParallelForEach(i =>
            {
                CommandRunner.Run("pngcr", $@"D:\c\KTANE\MaritimeFlags\Assets\Textures\Flag-{names[i]}-tmp.png", $@"D:\c\KTANE\MaritimeFlags\Assets\Textures\Flag-{names[i]}.png").OutputNothing().Go();
                lock (names)
                    Console.WriteLine($@"D:\c\KTANE\MaritimeFlags\Assets\Textures\Flag-{names[i]}.png");
                File.Delete($@"D:\c\KTANE\MaritimeFlags\Assets\Textures\Flag-{names[i]}-tmp.png");
            });
        }
    }
}