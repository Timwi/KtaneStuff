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

        public static void DoModels()
        {
            File.WriteAllText(@"D:\c\KTANE\MaritimeFlags\Assets\Models\Compass.obj", GenerateObjFile(Compass(), "Compass"));
        }

        private static IEnumerable<Pt[]> Compass()
        {
            throw new NotImplementedException();
        }

        public static void DoGraphics()
        {
            var names = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789".Select(ch => ch.ToString()).Concat("R1,R2,R3,R4".Split(',')).ToArray();
            using (var bmp = new Bitmap(@"D:\c\KTANE\MaritimeFlags\Data\Flags.png"))
            {
                var w = (bmp.Width - 20) / 8;
                var h = (bmp.Height - 20) / 5;
                for (int x = 0; x < 8; x++)
                    for (int y = 0; y < 5; y++)
                    {
                        GraphicsUtil.DrawBitmap(w, h, g =>
                        {
                            g.Clear(Color.Transparent);
                            g.DrawImage(bmp, -x * w - 10, -y * h - 10);
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