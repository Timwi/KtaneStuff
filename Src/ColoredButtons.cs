using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using KtaneStuff.Modeling;
using RT.Util;
using RT.Util.Consoles;
using RT.Util.ExtensionMethods;

namespace KtaneStuff
{
    using static Md;

    static class ColoredButtons
    {
        public static void TheGrayButton_MakeSymbols()
        {
            Ut.NewArray<(string name, string svgPathD)>(
                ("triangle up", "m 5,1.25 4.3301269,7.5 -8.66025409,0 z"),
                ("triangle down", "m 5,8.75 4.3301269,-7.5 -8.66025409,0 z"),
                ("circle", "M 8.955,5 A 3.955,3.955 0 0 1 5,8.955 3.955,3.955 0 0 1 1.045,5 3.955,3.955 0 0 1 5,1.045 3.955,3.955 0 0 1 8.955,5 Z"),
                ("circle with X", "M 4.9746094 1.0449219 A 3.955 3.955 0 0 0 2.7851562 1.7226562 L 5 3.9375 L 7.2148438 1.7226562 A 3.955 3.955 0 0 0 5 1.0449219 A 3.955 3.955 0 0 0 4.9746094 1.0449219 z M 1.7226562 2.7851562 A 3.955 3.955 0 0 0 1.0449219 5 A 3.955 3.955 0 0 0 1.7226562 7.2148438 L 3.9375 5 L 1.7226562 2.7851562 z M 8.2773438 2.7851562 L 6.0625 5 L 8.2773438 7.2148438 A 3.955 3.955 0 0 0 8.9550781 5 A 3.955 3.955 0 0 0 8.2773438 2.7851562 z M 5 6.0625 L 2.7851562 8.2773438 A 3.955 3.955 0 0 0 5 8.9550781 A 3.955 3.955 0 0 0 7.2148438 8.2773438 L 5 6.0625 z"),
                ("square", "M 1.5,1.5 H 8.5 V 8.5 H 1.5 Z"),
                ("square with plus", "M 1.5 1.5 L 1.5 4.5 L 4.5 4.5 L 4.5 1.5 L 1.5 1.5 z M 5.5 1.5 L 5.5 4.5 L 8.5 4.5 L 8.5 1.5 L 5.5 1.5 z M 1.5 5.5 L 1.5 8.5 L 4.5 8.5 L 4.5 5.5 L 1.5 5.5 z M 5.5 5.5 L 5.5 8.5 L 8.5 8.5 L 8.5 5.5 L 5.5 5.5 z"),
                ("diamond", "M 5,1 9,5 5,9 1,5 Z"),
                ("star", "M 5,0.47745752 6.1067849,3.9540988 9.7552826,3.9323727 6.7908156,6.0593288 7.9389262,9.5225426 5,7.3604325 2.0610736,9.5225424 3.2091844,6.0593288 0.24471746,3.9323724 3.8932151,3.9540988 Z"),
                ("plus", "M 3.5,1 H 6.5 V 3.5 H 9 V 6.5 H 6.5 V 9 H 3.5 V 6.5 H 1 V 3.5 h 2.5 z")
            ).ParallelForEach(tup =>
            {
                var (name, svgPathD) = tup;
                Utils.SvgToPng($@"D:\c\KTANE\SingleSelectablePack\Assets\Modules\Gray\Assets\{name}.png",
                    $@"<svg viewBox='-.1 -.1 10.2 10.2'><path d='{svgPathD}' fill='#1b1e21' stroke='none' /></svg>", 800);
            });
        }

        public static void TheYellowButton_MakeModels()
        {
            File.WriteAllText(@"D:\c\KTANE\SingleSelectablePack\Assets\Modules\Yellow\Assets\Annulus.obj", GenerateObjFile(LooseModels.Annulus(.5, .725, 32, reverse: true), "Annulus", AutoNormal.Flat));
        }

        public static void TheWhiteButton_MakeModels()
        {
            const int numVertices = 12;
            const double ir = .5;       // inner radius
            const double or = .7; // outer radius
            const double bw = .05;
            const double h = .03;

            var bézier = Bézier(p(0, 0), p(0, h * .5), p(bw * .5, h), p(bw, h), 6);

            File.WriteAllText(@"D:\c\KTANE\SingleSelectablePack\Assets\Modules\White\Assets\Segment.obj",
                GenerateObjFile(
                    CreateMesh(false, false,
                        Enumerable.Range(0, numVertices)
                            .Select(i => bézier.Select(pt => pt + p(ir, 0)).Concat(bézier.Reverse().Select(pt => p(or - pt.X, pt.Y))).Select(p => pt(p.X, p.Y, 0).RotateY(120.0 * i / (numVertices - 1))).ToArray())
                            .Select((pts, fst, lst) => pts.Select((pt, f, l) => pt.WithMeshInfo(fst || lst ? Normal.Mine : Normal.Average, fst || lst ? Normal.Mine : Normal.Average, f || l ? Normal.Mine : Normal.Average, f || l ? Normal.Mine : Normal.Average)).ToArray())
                            .ToArray()),
                    "Segment"));
        }

        private static readonly string[] _words = new[] { "ABIDE", "ABORT", "ABOVE", "ABYSS", "ACIDS", "ACORN", "ACTED", "ACTOR", "ACUTE", "ADDLE", "ADIEU", "ADIOS", "ADMIN", "ADMIT", "ADOPT", "ADORE", "ADORN", "ADULT", "AFFIX", "AGILE", "AGING", "AGORA", "AGREE", "AHEAD", "AIDED", "AIMED", "AIOLI", "AIRED", "AISLE", "ALARM", "ALBUM", "ALIAS", "ALIBI", "ALIEN", "ALIGN", "ALIKE", "ALIVE", "ALLAY", "ALLEN", "ALLOT", "ALLOY", "ALOFT", "ALONE", "ALONG", "ALOOF", "ALOUD", "ALPHA", "ALTAR", "AMASS", "AMAZE", "AMBLE", "AMINO", "AMISH", "AMISS", "AMONG", "AMPLE", "AMUSE", "ANGLE", "ANGLO", "ANGRY", "ANGST", "ANIME", "ANION", "ANISE", "ANKLE", "ANNEX", "ANNOY", "ANNUL", "ANTIC", "ANVIL", "AORTA", "APPLE", "APRON", "AREAS", "ARGUE", "ARISE", "ARMED", "ARMOR", "AROSE", "ASHEN", "ASHES", "ASIAN", "ASIDE", "ASSET", "ASTIR", "ATOLL", "ATOMS", "ATONE", "ATTIC", "AUDIO", "AUDIT", "AUNTY", "AVAIL", "AVIAN", "AVOID", "AWAIT", "AWAKE", "AWARE", "AWASH", "AXIAL", "AXIOM", "AXION", "AZTEC", "BIGHT", "BILLS", "BINGO", "BIRTH", "BISON", "BLAND", "BLAST", "BLEAT", "BLEED", "BLIMP", "BLIND", "BLING", "BLINK", "BLISS", "BLITZ", "BLOND", "BLOOM", "BLOOP", "BLUES", "BLUES", "BLUNT", "BOGGY", "BOLTS", "BONED", "BONNY", "BORAX", "BORED", "BORNE", "BORON", "BRAID", "BRAIN", "BRAKE", "BRAND", "BRASH", "BRASS", "BRAVE", "BRAWL", "BRAWN", "BREAD", "BREAK", "BREAM", "BREED", "BRIAR", "BRICK", "BRIDE", "BRINE", "BRING", "BRINK", "BRINY", "BRISK", "BROIL", "BRONX", "BROOM", "BROTH", "BRUNT", "BRUSH", "BRUTE", "BUDDY", "BUGGY", "BUILD", "BUILT", "BULLS", "BUMPY", "BUNNY", "BUZZY", "BYLAW", "BYWAY", "CABBY", "CABIN", "CABLE", "CACHE", "CAIRN", "CAKES", "CALLS", "CALVE", "CALYX", "CAMPS", "CAMPY", "CANAL", "CANED", "CANNY", "CANON", "CARDS", "CARVE", "CASED", "CASES", "CASTE", "CATCH", "CAULK", "CAUSE", "CAVES", "CEASE", "CEDED", "CELLS", "CENTS", "CHAFE", "CHAFF", "CHAIN", "CHAIR", "CHALK", "CHAMP", "CHANT", "CHAOS", "CHAPS", "CHARM", "CHART", "CHARY", "CHASE", "CHASM", "CHEAP", "CHEAT", "CHECK", "CHEMO", "CHESS", "CHEST", "CHICK", "CHIDE", "CHILD", "CHILI", "CHILL", "CHIME", "CHINA", "CHINA", "CHIPS", "CHORD", "CHORE", "CHOSE", "CHUCK", "CHUNK", "CHUTE", "CINCH", "CITED", "CITES", "CIVET", "CIVIC", "CIVIL", "CLADE", "CLAIM", "CLANK", "CLASH", "CLASS", "CLAWS", "CLEAN", "CLEAR", "CLEAT", "CLICK", "CLIFF", "CLIMB", "CLING", "CLONE", "CLOSE", "CLOTH", "CLOUD", "CLOUT", "CLOVE", "CLUBS", "CLUCK", "CLUES", "CLUNG", "CLUNK", "COINS", "COLIC", "COLON", "COLOR", "COMAL", "COMES", "COMIC", "COMMA", "CONCH", "CONIC", "CORAL", "CORGI", "CORNY", "CORPS", "COSTS", "COTTA", "COUCH", "COUGH", "COUNT", "COYLY", "CRANE", "CRANK", "CRASH", "CRASS", "CRATE", "CRAVE", "CREAK", "CREAM", "CREED", "CREWS", "CRIED", "CRIES", "CRIME", "CRONE", "CROPS", "CROSS", "CRUDE", "CRUEL", "CRUSH", "CUBBY", "CUBIT", "CUMIN", "CUTIE", "CYCLE", "CYNIC", "CZECH", "DACHA", "DAILY", "DALLY", "DANCE", "DATED", "DATES", "DATUM", "DEALS", "DEALT", "DEATH", "DEBTS", "DECAL", "DECOR", "DECOY", "DEEDS", "DELVE", "DENIM", "DENSE", "DESKS", "DETOX", "DEUCE", "DEVIL", "DICED", "DIETS", "DIGIT", "DIMLY", "DINAR", "DINGY", "DIRTY", "DISCO", "DISCS", "DISKS", "DITCH", "DITTY", "DITZY", "DIVAN", "DIVED", "DIVOT", "DIVVY", "DOGGY", "DOING", "DOLLS", "DOMED", "DONOR", "DONUT", "DOORS", "DORIC", "DOSED", "DOSES", "DOTTY", "DOUGH", "DOUSE", "DRAIN", "DRANK", "DREAM", "DRIED", "DRIFT", "DRILL", "DRILY", "DRINK", "DRIVE", "DROLL", "DRONE", "DROPS", "DROVE", "DRUGS", "DRUMS", "DRUNK", "DUCAT", "DUCKS", "DUMMY", "DUNCE", "DUNES", "DUTCH", "DWELL", "DYING", "EAGLE", "EARED", "EARLY", "EARTH", "EASED", "EASEL", "EATEN", "ECLAT", "EDICT", "EDIFY", "ELATE", "ELECT", "ELIDE", "ELITE", "ELUDE", "ELVES", "EMCEE", "EMOTE", "ENNUI", "ENSUE", "ENVOY", "ETHOS", "ETUDE", "EVICT", "EXACT", "EXALT", "EXAMS", "EXILE", "EXUDE", "FAILS", "FAINT", "FAITH", "FALLS", "FALSE", "FAMED", "FATAL", "FATED", "FATTY", "FATWA", "FAULT", "FAVOR", "FEAST", "FECAL", "FEINT", "FIBRE", "FIFTH", "FIFTY", "FIGHT", "FILCH", "FILED", "FILET", "FILLE", "FILLS", "FILLY", "FILMS", "FILMY", "FILTH", "FINAL", "FINDS", "FINED", "FINNY", "FIRED", "FIRMS", "FIRST", "FISTS", "FLAIL", "FLAIR", "FLASK", "FLATS", "FLEET", "FLING", "FLIRT", "FLOOR", "FLORA", "FLOUT", "FLUID", "FLUNG", "FLYBY", "FOGGY", "FOIST", "FOLIC", "FOLIO", "FOLLY", "FONTS", "FORAY", "FORGO", "FORMS", "FORTE", "FORTH", "FORTY", "FORUM", "FOUNT", "FOVEA", "FRAIL", "FRAUD", "FREED", "FRIED", "FRILL", "FRISK", "FRONT", "FRUIT", "FUELS", "FULLY", "FUNNY", "FUSED", "FUTON", "FUZZY", "GIANT", "GIFTS", "GIMPY", "GIRLS", "GIRLY", "GIRTH", "GIVEN", "GIZMO", "GLAND", "GLEAM", "GLEAN", "GLIAL", "GLINT", "GLOOM", "GLORY", "GLUED", "GLUON", "GOING", "GOLLY", "GOOFY", "GOOPY", "GRAFT", "GRAIN", "GRAND", "GRANT", "GRASS", "GRATE", "GRAVE", "GRAVY", "GREAT", "GREED", "GREEN", "GREET", "GRILL", "GRIME", "GRIMY", "GRIND", "GRIPS", "GROIN", "GROOM", "GROSS", "GROUT", "GRUEL", "GRUMP", "GRUNT", "GUANO", "GUARD", "GUAVA", "GUEST", "GUILD", "GUILT", "GUISE", "GULLS", "GULLY", "GUMMY", "GUNNY", "GYRUS", "HABIT", "HAIKU", "HAIRS", "HAIRY", "HALAL", "HALVE", "HAMMY", "HANDS", "HANDY", "HANGS", "HARDY", "HAREM", "HARPY", "HARSH", "HASTE", "HASTY", "HATCH", "HATED", "HATES", "HAUNT", "HAVEN", "HAZEL", "HEADS", "HEADY", "HEARD", "HEARS", "HEART", "HEATH", "HEAVE", "HEAVY", "HEELS", "HEIRS", "HEIST", "HELIX", "HELLO", "HENRY", "HILLS", "HILLY", "HINDI", "HINDU", "HINTS", "HIRED", "HITCH", "HOBBY", "HOIST", "HOLLY", "HOMED", "HONOR", "HORNS", "HORSE", "HOSEL", "HOTLY", "HUBBY", "HUGGY", "HULLO", "HUMAN", "HUMID", "HUMOR", "ICHOR", "ICILY", "ICING", "ICONS", "IDEAL", "IDEAS", "IDIOM", "IDIOT", "IDLED", "IDYLL", "IGLOO", "ILIAC", "ILIUM", "IMAGO", "IMBUE", "IMPLY", "INANE", "INCAN", "INCUS", "INDIA", "INDIE", "INFRA", "INGOT", "INLAY", "INLET", "INPUT", "INSET", "INTRO", "INUIT", "IONIC", "IRISH", "IRONY", "ISLET", "ISSUE", "ITEMS", "IVORY", "JOINT", "JUMBO", "KANJI", "KARAT", "KARMA", "KILLS", "KINDS", "KINGS", "KITTY", "KNAVE", "KNEES", "KNIFE", "KNOBS", "KNOLL", "KNOTS", "KUDOS", "KUDZU", "LABOR", "LACED", "LAITY", "LAMBS", "LANDS", "LAPIN", "LARVA", "LASTS", "LATCH", "LATHE", "LATIN", "LATTE", "LEACH", "LEADS", "LEAFY", "LEANT", "LEAPT", "LEARN", "LEASE", "LEASH", "LEAST", "LEAVE", "LEECH", "LEGGY", "LEMMA", "LEMON", "LIANA", "LIDAR", "LIFTS", "LIGHT", "LIKEN", "LIKES", "LILAC", "LIMBO", "LIMBS", "LIMIT", "LINED", "LINEN", "LINES", "LINGO", "LINKS", "LIONS", "LIPID", "LISTS", "LITRE", "LIVED", "LIVEN", "LIVES", "LIVID", "LLAMA", "LOBBY", "LOFTY", "LOGIC", "LOGON", "LOLLY", "LOONY", "LOOPS", "LOOPY", "LOOSE", "LORDS", "LORRY", "LOSES", "LOTTO", "LOTUS", "LOUSE", "LOVED", "LOYAL", "LUCID", "LUCRE", "LUMEN", "LUMPY", "LUNAR", "LUNCH", "LUNGS", "LUSTY", "LYING", "LYNCH", "LYRIC", "MADAM", "MADLY", "MAGIC", "MAGMA", "MAINS", "MAJOR", "MALAY", "MALTA", "MAMBO", "MANGO", "MANGY", "MANIA", "MANIC", "MANLY", "MANOR", "MASKS", "MATCH", "MATED", "MATHS", "MATTE", "MAVEN", "MAXIM", "MAYAN", "MAYOR", "MEALS", "MEANS", "MEANT", "MEATY", "MEDAL", "MEDIA", "MEDIC", "MEETS", "MELON", "METAL", "MEZZO", "MICRO", "MIDST", "MIGHT", "MILES", "MILLS", "MIMIC", "MINCE", "MINDS", "MINED", "MINES", "MINOR", "MINTY", "MINUS", "MIRED", "MIRTH", "MISTY", "MITRE", "MOGUL", "MOIST", "MOLAR", "MOLDY", "MONTH", "MOONY", "MOORS", "MOOSE", "MORAL", "MORAY", "MORPH", "MOTEL", "MOTIF", "MOTOR", "MOTTO", "MOULD", "MOUND", "MOUNT", "MOUSE", "MOUTH", "MOVED", "MOVIE", "MUCUS", "MUDDY", "MUGGY", "MULCH", "MULTI", "MUMMY", "MUNCH", "MUSED", "MUSIC", "MUSTY", "MUTED", "MUZZY", "MYTHS", "NADIR", "NAILS", "NAIVE", "NAMED", "NANNY", "NASAL", "NASTY", "NATAL", "NATTY", "NAVAL", "NAVEL", "NEATH", "NEEDS", "NEEDY", "NEWLY", "NICHE", "NIECE", "NIFTY", "NIGHT", "NIGRA", "NINTH", "NITRO", "NOBLY", "NOISE", "NOMAD", "NOOSE", "NORMS", "NORSE", "NORTH", "NOSES", "NOTCH", "NOTED", "NOVEL", "NUTTY", "NYLON", "OFTEN", "OILED", "OLIVE", "OMANI", "ONION", "ONSET", "OPINE", "OPIUM", "OPTIC", "ORBIT", "OVOID", "PHASE", "PHOTO", "PIANO", "PIGGY", "PILED", "PILLS", "PILOT", "PINTS", "PIVOT", "PIXEL", "PLAIN", "PLAIT", "PLANT", "PLEAT", "PLUMB", "PLUMP", "POINT", "POLIO", "PORTS", "POSED", "PREEN", "PRICY", "PRIMA", "PRIME", "PRIMP", "PRINT", "PRION", "PRIOR", "PRISE", "PRISM", "PRIVY", "PROMO", "PRONG", "PROOF", "PROSE", "PROUD", "PRUDE", "PUDGY", "PYGMY", "PYLON", "QUAIL", "QUALM", "QUASI", "QUEEN", "QUELL", "QUILL", "QUILT", "QUINT", "RADIO", "RAGGY", "RAILS", "RAINY", "RAISE", "RALLY", "RANGY", "RATED", "RATIO", "RATTY", "RAZOR", "REACT", "REALM", "REARM", "RECAP", "RECON", "RECTO", "REDLY", "REEDY", "REHAB", "REMIT", "RETRO", "RHINO", "RIGHT", "RIGID", "RIGOR", "RILED", "RINGS", "RINSE", "RIOTS", "RISEN", "RISES", "RISKS", "RITZY", "RIVAL", "RIVEN", "RIVET", "ROILY", "ROLLS", "ROMAN", "ROOFS", "ROSIN", "ROTOR", "ROYAL", "RUDDY", "RUGBY", "RUINS", "RULED", "RUMMY", "RUMOR", "RUNIC", "RUNNY", "RUNTY", "SADLY", "SAGGY", "SAILS", "SAINT", "SALAD", "SALLY", "SALON", "SALSA", "SALTS", "SALTY", "SALVE", "SANDS", "SANDY", "SATED", "SATIN", "SATYR", "SAUCE", "SAUCY", "SAUDI", "SAUNA", "SAVED", "SAVOR", "SAVVY", "SAXON", "SCALD", "SCALE", "SCALP", "SCALY", "SCAMP", "SCANT", "SCARF", "SCARS", "SCARY", "SCENT", "SCHMO", "SCOFF", "SCOLD", "SCONE", "SCOOP", "SCOOT", "SCORE", "SCORN", "SCOTS", "SCOUT", "SCRAM", "SCREE", "SCREW", "SCRIM", "SCRIP", "SCRUB", "SCRUM", "SCULL", "SEALS", "SEAMS", "SEAMY", "SEATS", "SEDAN", "SEEDS", "SEEDY", "SEEMS", "SEGUE", "SELLS", "SENDS", "SENSE", "SEVEN", "SEXES", "SHADY", "SHAFT", "SHAKE", "SHALE", "SHALL", "SHAME", "SHANK", "SHARD", "SHAVE", "SHAWL", "SHEAF", "SHEAR", "SHEEN", "SHEET", "SHELF", "SHELL", "SHIFT", "SHILL", "SHINY", "SHIRT", "SHORN", "SHORT", "SHUNT", "SIGHT", "SIGIL", "SIGNS", "SILLY", "SILTY", "SINEW", "SINGS", "SINUS", "SITAR", "SIXTH", "SIXTY", "SIZED", "SKALD", "SKANK", "SKATE", "SKEIN", "SKIFF", "SKILL", "SKIMP", "SKINS", "SKIRT", "SKULL", "SLAIN", "SLANG", "SLANT", "SLEET", "SLIME", "SLIMY", "SLING", "SLINK", "SLOTH", "SLOTS", "SLYLY", "SMALL", "SMART", "SMEAR", "SMELL", "SMELT", "SMILE", "SMITE", "SNAIL", "SNARL", "SNIFF", "SNOOP", "SNORE", "SNORT", "SNOUT", "SOFTY", "SOGGY", "SOILS", "SOLID", "SONIC", "SORRY", "SORTS", "STAIN", "STAIR", "STALL", "STAND", "START", "STEAD", "STEAL", "STEAM", "STEEL", "STIFF", "STILE", "STILL", "STILT", "STING", "STINK", "STINT", "STOIC", "STONY", "STOOL", "STOOP", "STOPS", "STORE", "STORK", "STORM", "STORY", "STOUT", "STRUM", "STRUT", "STUCK", "STUDY", "STUNT", "SUAVE", "SUEDE", "SUITS", "SULLY", "SUNNY", "SWALE", "SWAMI", "SWAMP", "SWANK", "SWANS", "SWARD", "SWARM", "SWASH", "SWATH", "SWAZI", "SWEAR", "SWEAT", "SWELL", "SWIFT", "SWILL", "SWINE", "SWING", "SWIRL", "SWISH", "SWISS", "SWOON", "SWOOP", "SWORD", "SWORE", "SWORN", "SWUNG", "TACIT", "TAILS", "TAINT", "TAKEN", "TALLY", "TALON", "TAMED", "TAMIL", "TANGO", "TANGY", "TAROT", "TASKS", "TASTY", "TATTY", "TAUNT", "TAXES", "TAXIS", "TAXON", "TEACH", "TEAMS", "TEARS", "TEARY", "TEASE", "TEDDY", "TEENS", "TEENY", "TEETH", "TELLS", "TELLY", "TENOR", "TENSE", "TENTH", "TENTS", "TEXAS", "THANK", "THEIR", "THEME", "THESE", "THETA", "THIGH", "THINE", "THING", "THINK", "THIRD", "THONG", "THORN", "THOSE", "THUMB", "TIARA", "TIBIA", "TIDAL", "TIGHT", "TILDE", "TILED", "TILES", "TILTH", "TIMED", "TIMES", "TIMID", "TINES", "TINNY", "TIRED", "TITLE", "TOMMY", "TONAL", "TONED", "TONGS", "TONIC", "TONNE", "TOOLS", "TOONS", "TOOTH", "TOPIC", "TOQUE", "TORCH", "TORSO", "TORTE", "TORUS", "TOTAL", "TOTEM", "TOUCH", "TOXIC", "TOXIN", "TRACT", "TRAIL", "TRAIN", "TRAIT", "TRAMS", "TREAD", "TREAT", "TRIAD", "TRIAL", "TRIED", "TRIKE", "TRILL", "TRITE", "TROLL", "TROOP", "TROUT", "TRUCE", "TRUCK", "TRULY", "TRUST", "TRUTH", "TUBBY", "TULIP", "TUMMY", "TUNED", "TUNIC", "TUTEE", "TUTOR", "TWANG", "TWEAK", "TWINS", "TWIRL", "TWIST", "TYING", "ULNAR", "ULTRA", "UMBRA", "UNCUT", "UNDUE", "UNFED", "UNFIT", "UNIFY", "UNION", "UNITE", "UNITS", "UNITY", "UNLIT", "UNMET", "UNSAY", "UNTIE", "UNTIL", "USING", "USUAL", "UVULA", "VAGUE", "VALET", "VALID", "VALOR", "VALUE", "VALVE", "VAPOR", "VAULT", "VAUNT", "VEDIC", "VEINS", "VEINY", "VENAL", "VENOM", "VENUE", "VICAR", "VIEWS", "VIGIL", "VIGOR", "VILLA", "VINES", "VINYL", "VIRAL", "VIRUS", "VISIT", "VISOR", "VITAL", "VIVID", "VIXEN", "VOGUE", "VOTED", "VOUCH", "VROOM", "WAGON", "WAIST", "WAITS", "WAIVE", "WALKS", "WALLS", "WALTZ", "WANTS", "WARDS", "WARES", "WARNS", "WASTE", "WATCH", "WAVED", "WAVES", "WAXEN", "WEARS", "WEARY", "WEAVE", "WEBBY", "WELLS", "WETLY", "WHALE", "WHEAT", "WHEEL", "WHICH", "WHILE", "WHINE", "WHITE", "WHORL", "WHOSE", "WIDTH", "WILLS", "WIMPY", "WINCE", "WINCH", "WINDS", "WINES", "WINGS", "WITCH", "WITTY", "WIVES", "WORDS", "WORKS", "WORLD", "WORMS", "WORMY", "WORSE", "WORST", "WORTH", "XENON", "YEARN", "YEAST", "YOUNG", "YUCCA", "YUMMY", "ZILCH", "ZINGY", "ZONAL", "ZONES" };

        class Polyomino : IEquatable<Polyomino>
        {
            private readonly int _w;
            private readonly int _h;
            private readonly bool[] _arr;

            private Polyomino() { }
            private Polyomino(int w, int h, bool[] arr) { _w = w; _h = h; _arr = arr; }

            public Polyomino(string description)
            {
                var strs = description.Split(',');
                _w = strs.Max(s => s.Length);
                _h = strs.Length;
                _arr = new bool[_w * _h];
                for (var y = 0; y < _h; y++)
                    for (var x = 0; x < _w; x++)
                        _arr[x + _w * y] = strs[y].Length > x && strs[y][x] == '#';
            }

            public Polyomino RotateClockwise() => new Polyomino(_h, _w, Ut.NewArray(_h * _w, ix => _arr[(ix / _h) + _w * (_h - 1 - (ix % _h))]));
            public Polyomino Reflect() => new Polyomino(_w, _h, Ut.NewArray(_w * _h, ix => _arr[_w - 1 - (ix % _w) + _w * (ix / _w)]));

            public bool Has(int x, int y) => x >= 0 && x < _w && y >= 0 && y < _h && _arr[x + _w * y];
            public IEnumerable<(int x, int y)> Cells => _arr.SelectIndexWhere(b => b).Select(ix => (x: ix % _w, y: ix / _w));

            public bool Equals(Polyomino other) => other._w == _w && other._h == _h && other._arr.SequenceEqual(_arr);
            public override bool Equals(object obj) => obj is Polyomino other && Equals(other);
            public override int GetHashCode() => _arr.Aggregate(_w * 37 + _h, (p, n) => unchecked((p << 1) | (n ? 1 : 0)));
            public static bool operator ==(Polyomino one, Polyomino two) => one.Equals(two);
            public static bool operator !=(Polyomino one, Polyomino two) => !one.Equals(two);

            public override string ToString() => Enumerable.Range(0, _h).Select(row => Enumerable.Range(0, _w).Select(col => _arr[col + _w * row] ? "██" : "░░").JoinString()).JoinString("|\n");
        }

        const int _gw = 8;  // The Blue Button: polyomino puzzle grid size
        const int _gh = 4;

        private static IEnumerable<(int[] solution, PolyominoPlacement[] polys)> TheBlueButton_SolvePolyominoPuzzle(
            int?[] sofar,
            int pieceIx,
            List<PolyominoPlacement> possiblePlacements,
            IEnumerable<(Polyomino one, Polyomino two)> notAllowedToTouch = null,
            List<PolyominoPlacement> polysSofar = null,
            bool debug = false)
        {
            polysSofar ??= new List<PolyominoPlacement>();
            Coord? bestCell = null;
            int[] bestPlacementIxs = null;

            foreach (var tCell in Coord.Cells(_gw, _gh))
            {
                if (sofar[tCell.Index] != null)
                    continue;
                var tPossiblePlacementIxs = possiblePlacements.SelectIndexWhere(pl => pl.Polyomino.Has((tCell.X - pl.Place.X + _gw) % _gw, (tCell.Y - pl.Place.Y + _gh) % _gh)).ToArray();
                if (tPossiblePlacementIxs.Length == 0)
                    yield break;
                if (bestPlacementIxs == null || tPossiblePlacementIxs.Length < bestPlacementIxs.Length)
                {
                    bestCell = tCell;
                    bestPlacementIxs = tPossiblePlacementIxs;
                }
                if (tPossiblePlacementIxs.Length == 1)
                    goto shortcut;
            }

            if (bestPlacementIxs == null)
            {
                yield return (sofar.Select(i => i.Value).ToArray(), polysSofar.ToArray());
                yield break;
            }

            shortcut:
            var cell = bestCell.Value;

            foreach (var placementIx in bestPlacementIxs.Reverse())
            {
                var placement = possiblePlacements[placementIx];
                var (poly, place) = placement;
                possiblePlacements.RemoveAt(placementIx);

                foreach (var (dx, dy) in poly.Cells)
                    sofar[place.AddWrap(dx, dy).Index] = pieceIx;
                polysSofar.Add(new PolyominoPlacement(poly, place));

                if (debug)
                {
                    Console.WriteLine($"Placing at {place}:");
                    Console.WriteLine(poly);
                    ConsoleUtil.WriteLine(VisualizePolyominoGrid(sofar));
                    Console.ReadLine();
                }

                var newPlacements = possiblePlacements
                    .Where(p => p.Polyomino != poly && p.Polyomino.Cells.All(c => sofar[p.Place.AddWrap(c.x, c.y).Index] == null))
                    .ToList();
                if (notAllowedToTouch != null)
                {
                    foreach (var (one, two) in notAllowedToTouch)
                        if (one == poly)
                            newPlacements.RemoveAll(pl => pl.Polyomino == two && pl.Touches(placement));
                        else if (two == poly)
                            newPlacements.RemoveAll(pl => pl.Polyomino == one && pl.Touches(placement));
                }

                foreach (var solution in TheBlueButton_SolvePolyominoPuzzle(sofar, pieceIx + 1, newPlacements, notAllowedToTouch, polysSofar, debug: debug))
                    yield return solution;

                polysSofar.RemoveAt(polysSofar.Count - 1);
                foreach (var (dx, dy) in poly.Cells)
                    sofar[place.AddWrap(dx, dy).Index] = null;
            }
        }

        public static void TheBlueButton_GatherEdgeStatistics()
        {
            var freq = new Dictionary<char, int>();
            foreach (var word in _words)
                foreach (var ltr in word)
                    freq.IncSafe(ltr);
            Console.WriteLine(freq.OrderByDescending(p => p.Value).Select(p => p.Key).JoinString("\n"));
            Debugger.Break();

            var lockObj = new object();
            var dics = Enumerable.Range(0, 8 * 4).ParallelSelect(Environment.ProcessorCount, proc =>
            {
                var rnd = new Random(8472 + proc);
                var allPlacements = GetAllPolyominoPlacements();
                var dic = new Dictionary<int, int>();
                for (var i = 0; i < 100; i++)
                {
                    if (i % 10 == 0)
                        lock (lockObj)
                            ConsoleUtil.WriteLine($"Proc {proc} = {i}".Color((ConsoleColor) (proc % 8 + 8)));
                    allPlacements.Shuffle(rnd);
                    var solutionTup = TheBlueButton_SolvePolyominoPuzzle(new int?[_gw * _gh], 1, allPlacements).FirstOrNull();
                    if (solutionTup == null)
                    {
                        Console.WriteLine("dud");
                        continue;
                    }
                    var (solution, _) = solutionTup.Value;
                    foreach (var cell in Coord.Cells(_gw, _gh))
                    {
                        var shape =
                            (solution[cell.Index] != solution[cell.AddYWrap(-1).Index] ? 8 : 0) |
                            (solution[cell.Index] != solution[cell.AddXWrap(1).Index] ? 4 : 0) |
                            (solution[cell.Index] != solution[cell.AddYWrap(1).Index] ? 2 : 0) |
                            (solution[cell.Index] != solution[cell.AddXWrap(-1).Index] ? 1 : 0) |
                            (solution.Count(i => i == solution[cell.Index]) == 5 ? 16 : 0);
                        dic.IncSafe(shape);
                    }
                }
                lock (lockObj)
                    ConsoleUtil.WriteLine($"Proc {proc} done".Color((ConsoleColor) (proc % 8 + 8), ConsoleColor.DarkGreen));
                return dic;
            });
            var overallDic = new Dictionary<int, int>();
            foreach (var dic in dics)
                foreach (var key in dic.Keys)
                    overallDic.IncSafe(key, dic[key]);

            for (var i = 0; i < (1 << 5); i++)
                Console.WriteLine($"{overallDic.Get(i, 0),5} × {Convert.ToString(i, 2).PadLeft(5, '0')}");
        }

        private static Polyomino[] GetAllPolyominoes()
        {
            var basePolyominoes = Ut.NewArray(
                // domino
                "##",

                // triominoes
                "###",
                "##,#",

                // tetrominoes
                "####",     // I
                "##,##",    // O
                "###,#",    // L
                "##,.##",   // S
                "###,.#",   // T

                // pentominoes
                ".##,##,.#",    // F
                "#####",        // I
                "####,#",       // L
                "##,.###",      // N
                "##,###",       // P
                "###,.#,.#",    // T
                "###,#.#",      // U
                "###,#,#",      // V
                ".##,##,#",     // W
                ".#,###,.#",    // X
                "####,.#",      // Y
                "##,.#,.##"     // Z
            );

            return basePolyominoes
                .Select(p => new Polyomino(p))
                .SelectMany(p => new[] { p, p.RotateClockwise(), p.RotateClockwise().RotateClockwise(), p.RotateClockwise().RotateClockwise().RotateClockwise() })
                .SelectMany(p => new[] { p, p.Reflect() })
                .Distinct()
                .Where(poly => !poly.Cells.Any(c => c.x >= _gw || c.y >= _gh)
                    && !poly.Cells.Any(c =>
                        (!poly.Has(c.x + 1, c.y) && poly.Has((c.x + 1) % _gw, c.y)) ||
                        (!poly.Has(c.x - 1, c.y) && poly.Has((c.x + _gw - 1) % _gw, c.y)) ||
                        (!poly.Has(c.x, c.y + 1) && poly.Has(c.x, (c.y + 1) % _gh)) ||
                        (!poly.Has(c.x, c.y - 1) && poly.Has(c.x, (c.y + _gh - 1) % _gh))))
                .ToArray();
        }

        private static List<PolyominoPlacement> GetAllPolyominoPlacements() =>
            (from poly in GetAllPolyominoes() from place in Enumerable.Range(0, _gw * _gh) select new PolyominoPlacement(poly, new Coord(_gw, _gh, place))).ToList();

        private static readonly Dictionary<char, int> _polyominoAlphabet = new Dictionary<char, int>
        {
            ['E'] = 0b11010,
            ['S'] = 0b11011,
            ['A'] = 0b11110,
            ['R'] = 0b01110,
            ['O'] = 0b01011,
            ['T'] = 0b01101,
            ['L'] = 0b00111,
            ['I'] = 0b11101,
            ['N'] = 0b10111,
            ['D'] = 0b10110,
            ['C'] = 0b11100,
            ['U'] = 0b10011,
            ['H'] = 0b11001,
            ['P'] = 0b01010,
            ['M'] = 0b10101,
            ['G'] = 0b01001,
            ['Y'] = 0b00110,
            ['B'] = 0b00011,
            ['F'] = 0b01100,
            ['W'] = 0b11000,
            ['K'] = 0b10010,
            ['V'] = 0b00101,
            ['X'] = 0b10001,
            ['Z'] = 0b10100,
            ['J'] = 0b00010,
            ['Q'] = 0b01000
        };

        public static void TheBlueButton_FilterWords()
        {
            var rnd = new Random(447);
            var allPlacements = GetAllPolyominoPlacements().Shuffle(rnd);
            Console.WriteLine($"Total number of placements: {allPlacements.Count}");
            Console.WriteLine();
            var lockObj = new object();

            var validWords = _words.Order()
                .ParallelSelectMany(Environment.ProcessorCount, word =>
                {
                    lock (lockObj)
                        Console.Write($"{word}\r");
                    return TheBlueButton_GenerateRandomPolyominoSolution(word) == null ? Enumerable.Empty<string>() : new[] { word };
                }).ToArray();
            File.WriteAllLines(@"D:\temp\temp.txt", validWords.Order());
        }

        private static (int[] solution, PolyominoPlacement[] polys, int gaps)? TheBlueButton_GenerateRandomPolyominoSolution(string word, Random rnd = null)
        {
            var allPlacements = GetAllPolyominoPlacements().Shuffle(rnd);
            var allGaps = Enumerable.Range(0, 1 << 4).ToArray().Shuffle(rnd);
            foreach (var gaps in allGaps)
            {
                var letterPositions = Enumerable.Range(0, word.Length).Select(i => i + Enumerable.Range(0, i).Count(bit => (gaps & (1 << bit)) != 0)).ToArray();
                if (letterPositions.Last() >= _gw)
                    continue;
                var placements = allPlacements.Where(tup => letterPositions.All((pos, ix) =>
                {
                    var cell = new Coord(_gw, _gh, pos);
                    var (poly, place) = tup;
                    var dx = (cell.X - place.X + _gw) % _gw;
                    var dy = (cell.Y - place.Y + _gh) % _gh;
                    if (!poly.Has(dx, dy))
                        return true;
                    var encoding = _polyominoAlphabet[word[ix]];
                    return
                        poly.Has(dx, dy - 1) != ((encoding & 8) != 0) &&
                        poly.Has(dx + 1, dy) != ((encoding & 4) != 0) &&
                        poly.Has(dx, dy + 1) != ((encoding & 2) != 0) &&
                        poly.Has(dx - 1, dy) != ((encoding & 1) != 0) &&
                        (poly.Cells.Count() == 5) == ((encoding & 16) != 0);
                })).ToList();

                var solutionTup = TheBlueButton_SolvePolyominoPuzzle(new int?[_gw * _gh], 1, placements).FirstOrNull();
                if (solutionTup != null)
                    return (solutionTup.Value.solution, solutionTup.Value.polys, gaps);
            }
            return null;
        }

        private static ConsoleColoredString VisualizePolyominoGrid(int[] grid) => VisualizePolyominoGrid(grid.Select(i => (int?) i).ToArray());
        private static ConsoleColoredString VisualizePolyominoGrid(int?[] grid) =>
            Enumerable.Range(0, _gh)
                .Select(row => Enumerable.Range(0, _gw)
                    .Select(col => grid[col + _gw * row] == null ? "· " : "  ".Color(null, (ConsoleColor) grid[col + _gw * row].Value))
                    .JoinColoredString(""))
                .JoinColoredString("\n");

        public static void TheBlueButton_TestUniqueness()
        {
            var rnd = new Random(84727);
            var word = _words[rnd.Next(0, _words.Length)];
            var result = TheBlueButton_GenerateRandomPolyominoSolution(word, rnd);
            if (result == null)
                Debugger.Break();
            var (generatedGrid, generatedPolys, generatedGaps) = result.Value;
            Console.WriteLine($"Word: {word}, gaps: {Convert.ToString(generatedGaps, 2).PadLeft(4, '0').Reverse().JoinString(" ")}");
            Console.WriteLine("Solution:");
            ConsoleUtil.WriteLine(VisualizePolyominoGrid(generatedGrid));

            Console.WriteLine("Polys:");
            foreach (var (poly, place) in generatedPolys)
            {
                Console.WriteLine(poly);
                Console.WriteLine(place);
                Console.WriteLine();
            }
            generatedPolys.Shuffle(rnd);

            Console.WriteLine("───────────────────────────────────────────────────────────────────");

            // Without loss of generality, assume the first polyomino is a given
            var givenPolyominoPlacement = generatedPolys[0];
            var givenGrid = new int?[_gw * _gh];
            foreach (var (x, y) in givenPolyominoPlacement.Polyomino.Cells)
                givenGrid[givenPolyominoPlacement.Place.AddWrap(x, y).Index] = 1;
            var possiblePlacements = GetAllPolyominoPlacements()
                .Where(pl =>
                    pl.Polyomino.Cells.All(c => givenGrid[pl.Place.AddWrap(c.x, c.y).Index] == null) &&
                    generatedPolys.Skip(1).Any(tup => tup.Polyomino == pl.Polyomino))
                .ToArray();
            Console.WriteLine("Given:");
            ConsoleUtil.WriteLine(VisualizePolyominoGrid(givenGrid));

            IEnumerable<(int[] solution, PolyominoPlacement[] polys)> GenerateSolutions(List<(Polyomino one, Polyomino two)> noAllowTouch)
            {
                var grid = givenGrid.ToArray();
                var placements = possiblePlacements.ToList();
                foreach (var (one, two) in noAllowTouch)
                    if (one == givenPolyominoPlacement.Polyomino)
                        placements.RemoveAll(pl => pl.Polyomino == two && pl.Touches(givenPolyominoPlacement));
                    else if (two == givenPolyominoPlacement.Polyomino)
                        placements.RemoveAll(pl => pl.Polyomino == one && pl.Touches(givenPolyominoPlacement));

                return TheBlueButton_SolvePolyominoPuzzle(grid, 2, placements, noAllowTouch, debug: false);
            }

            var notAllowedToTouch = new List<(Polyomino one, Polyomino two)>();
            while (true)
            {
                var solutions = GenerateSolutions(notAllowedToTouch).Take(2).ToArray();

                // Puzzle still ambiguous?
                if (solutions.Length > 1)
                {
                    // Find a wrong solution
                    var (_, wrongAllPolys) = solutions.First(s => s.polys.Any(sPl => !generatedPolys.Contains(sPl)));
                    // Find all wrong polyominoes in this wrong solution
                    var wrongPolys = wrongAllPolys.Where(sPl => !generatedPolys.Contains(sPl)).ToArray();
                    // Find all polyominoes that touch a wrong polyomino in the wrong solution, but do not touch the corresponding correct polyomino in the correct solution
                    var touchingPolys = wrongPolys.SelectMany(wPl => wrongAllPolys
                          .Where(owPl => owPl.Touches(wPl) && !generatedPolys.First(gPl => gPl.Polyomino == wPl.Polyomino).Touches(generatedPolys.First(gPl => gPl.Polyomino == owPl.Polyomino)))
                          .Select(owPl => (one: wPl.Polyomino, two: owPl.Polyomino)))
                          .ToArray();

                    // Prefer one that isn’t already constrained
                    var prefIx = touchingPolys.IndexOf(tup1 =>
                        !notAllowedToTouch.Any(tup2 => tup2.one == tup1.one || tup2.two == tup1.one) &&
                        !notAllowedToTouch.Any(tup2 => tup2.one == tup1.two || tup2.two == tup1.two));
                    notAllowedToTouch.Add(touchingPolys[prefIx == -1 ? 0 : prefIx]);
                    continue;
                }

                // Puzzle valid!
                break;
            }

            Console.WriteLine("Forbidden touches:");
            foreach (var (one, two) in notAllowedToTouch)
            {
                Console.WriteLine(one);
                Console.WriteLine("vs.");
                Console.WriteLine(two);
                Console.WriteLine();
            }
        }

        private struct PolyominoPlacement : IEquatable<PolyominoPlacement>
        {
            public Polyomino Polyomino { get; private set; }
            public Coord Place { get; private set; }

            public PolyominoPlacement(Polyomino poly, Coord place)
            {
                Polyomino = poly;
                Place = place;
            }

            public bool Equals(PolyominoPlacement other) => Polyomino == other.Polyomino && Place == other.Place;
            public override bool Equals(object obj) => obj is PolyominoPlacement other && Equals(other);

            public static bool operator ==(PolyominoPlacement one, PolyominoPlacement two) => one.Equals(two);
            public static bool operator !=(PolyominoPlacement one, PolyominoPlacement two) => !one.Equals(two);

            public override int GetHashCode()
            {
                var hashCode = 1291772507 + Polyomino.GetHashCode();
                hashCode = hashCode * -1521134295 + Place.GetHashCode();
                return hashCode;
            }

            public void Deconstruct(out Polyomino poly, out Coord place)
            {
                poly = Polyomino;
                place = Place;
            }

            public bool Touches(PolyominoPlacement other)
            {
                foreach (var (x, y) in Polyomino.Cells)
                    foreach (var (ox, oy) in other.Polyomino.Cells)
                        if (Place.AddWrap(x, y).AdjacentToWrap(other.Place.AddWrap(ox, oy)))
                            return true;
                return false;
            }
        }
    }
}