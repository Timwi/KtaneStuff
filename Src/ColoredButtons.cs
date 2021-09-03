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

        private static readonly string[] _words = new[] { "ABORT", "ABOUT", "ABYSS", "ACIDS", "ACORN", "ACRES", "ACTED", "ACTOR", "ACUTE", "ADDER", "ADDLE", "ADIEU", "ADIOS", "ADMIN", "ADMIT", "ADOPT", "ADORE", "ADORN", "ADULT", "AFFIX", "AFTER", "AGILE", "AGING", "AGORA", "AGREE", "AHEAD", "AIDED", "AIMED", "AIOLI", "AIRED", "AISLE", "ALARM", "ALBUM", "ALIAS", "ALIBI", "ALIEN", "ALIGN", "ALIKE", "ALIVE", "ALLAY", "ALLEN", "ALLOT", "ALLOY", "ALOFT", "ALONG", "ALOOF", "ALOUD", "ALPHA", "ALTAR", "ALTER", "AMASS", "AMINO", "AMISH", "AMISS", "AMUSE", "ANGLO", "ANGRY", "ANGST", "ANIME", "ANION", "ANISE", "ANNEX", "ANNOY", "ANNUL", "ANTIC", "ANVIL", "AORTA", "APRON", "AREAS", "ARENA", "ARGUE", "ARISE", "ARMED", "ARMOR", "AROSE", "ASHEN", "ASHES", "ASIAN", "ASIDE", "ASSET", "ASTER", "ASTIR", "ATOLL", "ATOMS", "ATTIC", "AUDIO", "AUDIT", "AUGUR", "AUNTY", "AVAIL", "AVIAN", "AWAIT", "AWARE", "AWASH", "AXIAL", "AXION", "AZTEC", "BILGE", "BILLS", "BINGE", "BINGO", "BIRDS", "BIRTH", "BISON", "BITER", "BLIMP", "BLIND", "BLING", "BLINK", "BLISS", "BLITZ", "BLOOM", "BLOOP", "BLUES", "BLUES", "BLUNT", "BLUSH", "BOGUS", "BOGUS", "BOLTS", "BONUS", "BOOST", "BOOTH", "BOOTS", "BORAX", "BORED", "BORER", "BORNE", "BORON", "BOUGH", "BOULE", "BRACE", "BRAID", "BRAIN", "BRAKE", "BRAND", "BRASH", "BRASS", "BRAVE", "BRAWL", "BRAWN", "BRAZE", "BREAD", "BREAK", "BREAM", "BREED", "BRIAR", "BRIBE", "BRICK", "BRIDE", "BRIEF", "BRIER", "BRINE", "BRING", "BRINK", "BRINY", "BRISK", "BROIL", "BRONX", "BROOM", "BROTH", "BRUNT", "BRUSH", "BRUTE", "BUCKS", "BUDDY", "BUDGE", "BUGGY", "BUILD", "BUILT", "BULBS", "BULGE", "BULLS", "BUNNY", "BUSES", "BUZZY", "BYLAW", "CABBY", "CABIN", "CACHE", "CAIRN", "CAKES", "CALLS", "CALVE", "CALYX", "CAMPS", "CAMPY", "CANAL", "CANED", "CANNY", "CANON", "CARDS", "CARVE", "CASED", "CASES", "CASTE", "CATCH", "CAUSE", "CAVES", "CEASE", "CEDED", "CELLS", "CENTS", "CHAFE", "CHAFF", "CHAIN", "CHAIR", "CHALK", "CHAMP", "CHANT", "CHAOS", "CHAPS", "CHARM", "CHART", "CHARY", "CHASE", "CHASM", "CHEAP", "CHEAT", "CHECK", "CHEMO", "CHESS", "CHEST", "CHIDE", "CHILD", "CHILI", "CHILL", "CHIME", "CHINA", "CHINA", "CHORD", "CHORE", "CHOSE", "CHUTE", "CINCH", "CITED", "CITES", "CIVET", "CIVIC", "CIVIL", "CLADE", "CLAIM", "CLANK", "CLASH", "CLASS", "CLAWS", "CLEAN", "CLEAR", "CLEAT", "CLICK", "CLIFF", "CLIMB", "CLING", "CLOSE", "CLOTH", "CLOUD", "CLOUT", "CLUBS", "CLUCK", "CLUES", "CLUNG", "COINS", "COLIC", "COLON", "COLOR", "COMIC", "CONIC", "CORAL", "CORGI", "CORNY", "CORPS", "COSTS", "COTTA", "COUCH", "COUGH", "COUNT", "COYLY", "CRANE", "CRANK", "CRASH", "CRASS", "CRATE", "CRAVE", "CREAK", "CREAM", "CREED", "CREWS", "CRIED", "CRIES", "CRIME", "CROPS", "CROSS", "CRUDE", "CRUEL", "CRUSH", "CUBBY", "CUBIT", "CUMIN", "CUTIE", "CYCLE", "CYNIC", "CZECH", "DACHA", "DAILY", "DALLY", "DANCE", "DATED", "DATES", "DATUM", "DEALS", "DEALT", "DEATH", "DEBIT", "DEBTS", "DEBUT", "DECAF", "DECAL", "DECOR", "DECOY", "DEEDS", "DEIST", "DELVE", "DEMUR", "DENIM", "DENSE", "DESKS", "DETER", "DETOX", "DEUCE", "DEVIL", "DICED", "DIETS", "DIGIT", "DIMLY", "DINAR", "DINER", "DINGY", "DIRTY", "DISCO", "DISCS", "DISKS", "DITCH", "DITTY", "DITZY", "DIVAN", "DIVED", "DIVER", "DIVOT", "DIVVY", "DOGGY", "DOING", "DOLLS", "DONOR", "DONUT", "DOORS", "DORIC", "DOSED", "DOSES", "DOTTY", "DOUGH", "DOUSE", "DRAFT", "DRAIN", "DRAMA", "DRANK", "DREAM", "DRESS", "DRIED", "DRIER", "DRIFT", "DRILL", "DRILY", "DRINK", "DRIVE", "DROLL", "DROPS", "DRUGS", "DRUMS", "DRYER", "DUCAT", "DUCKS", "DUMMY", "DUNCE", "DUNES", "DUTCH", "DUVET", "DWARF", "DWELL", "DYING", "EARED", "EARLY", "EARTH", "EASED", "EASEL", "EATEN", "ECLAT", "EDEMA", "EDICT", "EDIFY", "EGRET", "EIDER", "EIGHT", "ELATE", "ELDER", "ELECT", "ELIDE", "ELITE", "ELUDE", "ELVES", "EMCEE", "ENACT", "ENNUI", "ENSUE", "ENTER", "ENVOY", "ETHOS", "ETUDE", "EVENT", "EVICT", "EXACT", "EXALT", "EXAMS", "EXILE", "EXIST", "EXUDE", "EXULT", "FAILS", "FAINT", "FAIRY", "FAITH", "FALLS", "FALSE", "FAMED", "FANCY", "FATAL", "FATED", "FATTY", "FATWA", "FAULT", "FAVOR", "FEAST", "FECAL", "FEINT", "FETAL", "FIBRE", "FIFTH", "FIFTY", "FIGHT", "FILCH", "FILED", "FILET", "FILLE", "FILLS", "FILLY", "FILMS", "FILMY", "FILTH", "FINAL", "FINDS", "FINED", "FINNY", "FIRED", "FIRST", "FISTS", "FLAIL", "FLAIR", "FLASK", "FLATS", "FLEET", "FLING", "FLIRT", "FLOOR", "FLORA", "FLOUT", "FLUNG", "FLYBY", "FOGGY", "FOIST", "FOLIC", "FOLIO", "FOLLY", "FONTS", "FORAY", "FORGO", "FORMS", "FORTE", "FORTH", "FORTY", "FORUM", "FOUNT", "FRAIL", "FRAUD", "FREED", "FRIED", "FRILL", "FRISK", "FRONT", "FRUIT", "FUELS", "FULLY", "FUNNY", "FUSED", "FUTON", "FUZZY", "GHOUL", "GIMPY", "GIRLS", "GIRLY", "GIRTH", "GIVEN", "GLIAL", "GLINT", "GLOOM", "GLORY", "GLUED", "GLUON", "GOING", "GOLLY", "GOOFY", "GOOPY", "GRAFT", "GRAIN", "GRAND", "GRANT", "GRASS", "GRATE", "GRAVE", "GRAVY", "GREAT", "GREED", "GREEN", "GREET", "GRILL", "GRIME", "GRIMY", "GRIND", "GRIPS", "GROIN", "GROOM", "GROSS", "GROUT", "GRUEL", "GRUNT", "GUEST", "GUILD", "GUILT", "GUISE", "GULLS", "GULLY", "GUNNY", "GUTSY", "GYRUS", "HABIT", "HAIKU", "HAIRS", "HAIRY", "HALAL", "HALVE", "HAMMY", "HANDS", "HANDY", "HANGS", "HARDY", "HAREM", "HARPY", "HARSH", "HASTE", "HATCH", "HATED", "HATES", "HAUNT", "HAVEN", "HAZEL", "HEADS", "HEADY", "HEARD", "HEARS", "HEART", "HEATH", "HEAVE", "HEAVY", "HEELS", "HEIST", "HELIX", "HELLO", "HILLS", "HILLY", "HINDI", "HINDU", "HINTS", "HITCH", "HOBBY", "HOIST", "HOLLY", "HONOR", "HORNS", "HORSE", "HOSEL", "HOTLY", "HULLO", "HUMOR", "ICHOR", "ICILY", "ICING", "ICONS", "IDEAL", "IDEAS", "IDIOM", "IDIOT", "IDLED", "IDYLL", "IGLOO", "ILIAC", "ILIUM", "IMAGO", "IMBUE", "INANE", "INCAN", "INCUS", "INDEX", "INDIA", "INDIE", "INFRA", "INGOT", "INLAY", "INLET", "INPUT", "INSET", "INTRO", "INUIT", "IONIC", "IRISH", "IRONY", "ISLET", "ISSUE", "ITEMS", "IVORY", "JOINS", "JOINT", "JUMBO", "JUNTA", "KANJI", "KARAT", "KARMA", "KILLS", "KINDA", "KINDS", "KINGS", "KITTY", "KNAVE", "KNIFE", "KNOBS", "KNOLL", "KNOTS", "KUDOS", "LABOR", "LACED", "LADLE", "LAITY", "LAMBS", "LAMPS", "LANDS", "LANES", "LAPIN", "LAPSE", "LARGE", "LARVA", "LASER", "LASSO", "LASTS", "LATCH", "LATER", "LATHE", "LATIN", "LATTE", "LAUGH", "LAWNS", "LAYER", "LAYUP", "LEACH", "LEADS", "LEAFY", "LEANT", "LEAPT", "LEARN", "LEASE", "LEASH", "LEAST", "LEAVE", "LEDGE", "LEECH", "LEGGY", "LEMMA", "LEMON", "LEMUR", "LEVEE", "LEVER", "LIANA", "LIDAR", "LIEGE", "LIFTS", "LIGHT", "LIKEN", "LIKES", "LILAC", "LIMBO", "LIMBS", "LIMIT", "LINED", "LINEN", "LINER", "LINES", "LINGO", "LINKS", "LIONS", "LISTS", "LITER", "LITRE", "LIVED", "LIVEN", "LIVER", "LIVES", "LIVID", "LLAMA", "LOBBY", "LOFTY", "LOGON", "LOLLY", "LOONY", "LOOPS", "LOOPY", "LOOSE", "LORDS", "LORRY", "LOSER", "LOSES", "LOTTO", "LOTUS", "LOUSE", "LOYAL", "LUCID", "LUCRE", "LUMEN", "LUNAR", "LUNCH", "LUNGE", "LUNGS", "LYING", "LYMPH", "LYNCH", "LYRIC", "MADAM", "MADLY", "MAINS", "MAJOR", "MALAY", "MALTA", "MAMBO", "MANGO", "MANGY", "MANIA", "MANIC", "MANLY", "MANOR", "MASKS", "MATCH", "MATED", "MATHS", "MATTE", "MAVEN", "MAYOR", "MEALS", "MEANS", "MEANT", "MEATY", "MEDAL", "MEDIA", "MEDIC", "MEETS", "MELON", "METAL", "MEZZO", "MICRO", "MIDST", "MIGHT", "MILES", "MILLS", "MIMIC", "MINCE", "MINDS", "MINED", "MINES", "MINOR", "MINTY", "MINUS", "MIRED", "MIRTH", "MITRE", "MOGUL", "MOIST", "MONTH", "MOONY", "MOORS", "MOOSE", "MORAL", "MORAY", "MORPH", "MOTIF", "MOTOR", "MOTTO", "MOUNT", "MOUSE", "MOUTH", "MOVIE", "MUCUS", "MUDDY", "MUGGY", "MULCH", "MULTI", "MUSED", "MUSIC", "MUTED", "MUZZY", "NADIR", "NAILS", "NAIVE", "NAMED", "NAMES", "NANNY", "NARCO", "NASAL", "NATAL", "NATTY", "NAVAL", "NAVEL", "NEATH", "NECKS", "NEEDS", "NEEDY", "NEIGH", "NESTS", "NEVER", "NEXUS", "NICER", "NICHE", "NIECE", "NIFTY", "NIGHT", "NIGRA", "NINTH", "NITRO", "NOISE", "NOOSE", "NORMS", "NORSE", "NORTH", "NOSES", "NUDGE", "NUTTY", "NYLON", "NYMPH", "OFFER", "OFTEN", "OILED", "OLIVE", "ONION", "ONSET", "OOMPH", "OPINE", "OPIUM", "OPTIC", "ORBIT", "ORDER", "OTHER", "OTTER", "OUGHT", "OUNCE", "OUTDO", "OUTER", "OUTER", "OVULE", "PHASE", "PHOTO", "PIGGY", "PILAF", "PILED", "PILLS", "PILOT", "PINCH", "PINTS", "PISTE", "PITCH", "PIVOT", "PIXEL", "PLOTS", "PLUMB", "PLUME", "POINT", "POLIO", "POLLS", "POOLS", "PORCH", "PORTS", "POSED", "POSIT", "POSTS", "POUCH", "PREEN", "PRICE", "PRICY", "PRIDE", "PRIMA", "PRIME", "PRIMP", "PRINT", "PRION", "PRIOR", "PRISE", "PRISM", "PRIVY", "PRIZE", "PROMO", "PRONG", "PROOF", "PROSE", "PROUD", "PRUDE", "PRUNE", "PUDGY", "PULLS", "PUNCH", "PUNIC", "PYLON", "QUEEN", "QUELL", "QUILL", "QUILT", "QUINT", "RABBI", "RADAR", "RADIO", "RAGGY", "RAIDS", "RAILS", "RAINY", "RAISE", "RALLY", "RANCH", "RANGE", "RANGY", "RATED", "RATIO", "RATTY", "RAZOR", "REACH", "REACT", "READS", "REALM", "REARM", "RECAP", "RECON", "RECTO", "REDLY", "REEDY", "REHAB", "REINS", "RELIC", "REMIT", "RENTS", "RESTS", "RHINO", "RIDGE", "RIFLE", "RIGHT", "RIGOR", "RILED", "RINGS", "RINSE", "RIOTS", "RISEN", "RISES", "RISKS", "RITZY", "RIVAL", "RIVEN", "RIVET", "ROBOT", "ROILY", "ROLLS", "ROOFS", "ROOMS", "ROOTS", "ROSIN", "ROTOR", "ROUGE", "ROUGH", "ROUTE", "ROYAL", "RUDDY", "RUGBY", "RUINS", "RULED", "RUMBA", "RUMMY", "RUMOR", "RUNIC", "RUNNY", "RUNTY", "SADLY", "SAGGY", "SAILS", "SAINT", "SALAD", "SALES", "SALLY", "SALON", "SALSA", "SALTS", "SALTY", "SALVE", "SAMBA", "SANDS", "SANDY", "SATED", "SATIN", "SATYR", "SAUCE", "SAUCY", "SAUDI", "SAUNA", "SAVED", "SAVER", "SAVES", "SAVOR", "SAVVY", "SAXON", "SCALD", "SCALE", "SCALP", "SCALY", "SCAMP", "SCANT", "SCAPE", "SCARE", "SCARF", "SCARP", "SCARS", "SCARY", "SCENE", "SCENT", "SCHMO", "SCOFF", "SCOOP", "SCOOT", "SCOPE", "SCORE", "SCORN", "SCOTS", "SCOUR", "SCOUT", "SCRAM", "SCRAP", "SCREE", "SCREW", "SCRIM", "SCRIP", "SCRUB", "SCRUM", "SCUBA", "SCULL", "SEALS", "SEAMS", "SEAMY", "SEATS", "SEDAN", "SEEDS", "SEEDY", "SEEMS", "SEGUE", "SELLS", "SENDS", "SENSE", "SETUP", "SEVEN", "SEVER", "SEXES", "SHADE", "SHADY", "SHAFT", "SHAKE", "SHALE", "SHALL", "SHAME", "SHANK", "SHAPE", "SHARD", "SHARE", "SHARP", "SHAVE", "SHAWL", "SHEAF", "SHEAR", "SHEEN", "SHEET", "SHELL", "SHILL", "SHINE", "SHINY", "SHOOT", "SHOPS", "SHORE", "SHORN", "SHORT", "SHOTS", "SHOUT", "SHUNT", "SHUSH", "SIDES", "SIDLE", "SIGHT", "SIGIL", "SIGNS", "SILLY", "SILTY", "SINCE", "SINEW", "SINGE", "SINGS", "SINUS", "SITAR", "SITES", "SIXTH", "SIZED", "SIZES", "SKALD", "SKANK", "SKATE", "SKEIN", "SKILL", "SKIMP", "SKINS", "SKULL", "SLEEP", "SLEET", "SLICE", "SLIDE", "SLIME", "SLIMY", "SLING", "SLINK", "SLOPE", "SLOSH", "SLOTH", "SLOTS", "SLUSH", "SMELL", "SMELT", "SMILE", "SMITE", "SNAIL", "SNAKE", "SNARE", "SNARL", "SNEER", "SNIDE", "SNIFF", "SNIPE", "SNOOP", "SNORE", "SNORT", "SNOUT", "SOFTY", "SOGGY", "SOILS", "SOLID", "SOLVE", "SONGS", "SONIC", "SOOTH", "SORRY", "SORTS", "SOUGH", "SOULS", "SOUTH", "STEAD", "STEAL", "STEAM", "STEEL", "STEEP", "STEER", "STEMS", "STENO", "STILE", "STILL", "STILT", "STING", "STINK", "STINT", "STOMP", "STONY", "STOOL", "STOOP", "STOPS", "STORE", "STORK", "STORM", "STORY", "STOUT", "STUDY", "STUNT", "SUAVE", "SUEDE", "SUITE", "SUITS", "SULLY", "SUNNY", "SUNUP", "SUSHI", "SWALE", "SWAMI", "SWAMP", "SWANK", "SWANS", "SWARD", "SWARM", "SWASH", "SWATH", "SWAZI", "SWEAR", "SWEAT", "SWELL", "SWILL", "SWINE", "SWING", "SWISH", "SWISS", "SWOON", "SWOOP", "SWORD", "SWORE", "SWORN", "SWUNG", "TACIT", "TACKY", "TAFFY", "TAILS", "TAINT", "TAKEN", "TALKY", "TALLY", "TALON", "TAMED", "TAMIL", "TANGO", "TANGY", "TARDY", "TAROT", "TARRY", "TASKS", "TATTY", "TAUNT", "TAXES", "TAXIS", "TAXON", "TEACH", "TEAMS", "TEARS", "TEARY", "TEASE", "TECHY", "TEDDY", "TEENS", "TEETH", "TELLS", "TELLY", "TENOR", "TENSE", "TENTH", "TENTS", "TEXAS", "THANK", "THEME", "THESE", "THETA", "THIGH", "THINE", "THING", "THINK", "THONG", "THORN", "THOSE", "THUMB", "TIARA", "TIDAL", "TIGHT", "TILDE", "TILED", "TILES", "TILTH", "TIMED", "TIMES", "TIMID", "TINES", "TINNY", "TIRED", "TITLE", "TOMMY", "TONGS", "TONIC", "TONNE", "TOOLS", "TOONS", "TOOTH", "TOQUE", "TORCH", "TORSO", "TORTE", "TORUS", "TOUCH", "TOXIN", "TRACT", "TRAIL", "TRAIN", "TRAIT", "TRAMS", "TRAWL", "TREAD", "TREAT", "TRIAD", "TRIAL", "TRIED", "TRIKE", "TRILL", "TRITE", "TROLL", "TROOP", "TROUT", "TRUCE", "TRUCK", "TRUST", "TRUTH", "TUBBY", "TULIP", "TUNIC", "TUTEE", "TUTOR", "TWANG", "TWEAK", "TWINS", "TWIST", "TYING", "UDDER", "ULCER", "ULNAR", "ULTRA", "UMBRA", "UNCLE", "UNCUT", "UNIFY", "UNION", "UNITE", "UNITS", "UNITY", "UNLIT", "UNMET", "UNTIE", "UNTIL", "USAGE", "USHER", "USING", "USUAL", "UTTER", "UVULA", "VAGUE", "VALET", "VALID", "VALOR", "VALUE", "VALVE", "VAPOR", "VAULT", "VAUNT", "VEDIC", "VEINS", "VEINY", "VENAL", "VENOM", "VENUE", "VICAR", "VIEWS", "VIGIL", "VIGOR", "VILLA", "VINES", "VINYL", "VIRUS", "VISIT", "VISOR", "VITAL", "VIVID", "VIXEN", "VOGUE", "VOUCH", "VROOM", "WAGON", "WAIST", "WAITS", "WAIVE", "WALKS", "WALLS", "WALTZ", "WANTS", "WARDS", "WARES", "WARNS", "WASTE", "WATCH", "WAVED", "WAVES", "WAXEN", "WEARS", "WEARY", "WEAVE", "WEBBY", "WELLS", "WETLY", "WHALE", "WHEAT", "WHEEL", "WHILE", "WHINE", "WHITE", "WHORL", "WHOSE", "WILLS", "WIMPY", "WINCE", "WINCH", "WINDS", "WINES", "WINGS", "WITCH", "WITTY", "WIVES", "WORDS", "WORKS", "WORLD", "WORMS", "WORMY", "WORSE", "WORST", "WORTH", "XENON", "YARDS", "YAWNS", "YEARN", "YEARS", "YEAST", "YOUNG", "YOUTH", "YUMMY", "ZILCH", "ZINGY" };

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

        const int _gw = 6;  // The Blue Button: polyomino puzzle grid size
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
            Console.WriteLine();

            var lockObj = new object();
            var dics = Enumerable.Range(0, 8 * 8).ParallelSelect(Environment.ProcessorCount, proc =>
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

            var validWords = File.ReadAllLines(@"D:\Daten\Wordlists\Nice 5-letter.txt").Order()
                .ParallelSelectMany(Environment.ProcessorCount, word =>
                {
                    lock (lockObj)
                        Console.Write($"{word}\r");
                    return TheBlueButton_GenerateRandomPolyominoSolution(word) == null ? Enumerable.Empty<string>() : new[] { word };
                }).ToArray();
            File.WriteAllLines(@"D:\temp\temp.txt", validWords.Order());
        }

        private static (int[] solution, PolyominoPlacement[] polys, int jumps)? TheBlueButton_GenerateRandomPolyominoSolution(string word, Random rnd = null)
        {
            var allPlacements = GetAllPolyominoPlacements().Shuffle(rnd);
            var allJumps = Enumerable.Range(0, 1 << 4).ToArray().Shuffle(rnd);
            foreach (var jumps in allJumps)
            {
                var letterPositions = Enumerable.Range(0, word.Length).Select(i => new Coord(_gw, _gh, i).AddYWrap((jumps & (1 << i)) != 0 ? 2 : 0)).ToArray();
                var placements = allPlacements.Where(tup => letterPositions.All((cell, ix) =>
                {
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
                    return (solutionTup.Value.solution, solutionTup.Value.polys, jumps);
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

        public static void TheBlueButton_GeneratePuzzle()
        {
            var rnd = new Random(/*47*/);

            tryAgain:
            var word = _words[rnd.Next(0, _words.Length)];
            var result = TheBlueButton_GenerateRandomPolyominoSolution(word, rnd);
            if (result == null)
                goto tryAgain;

            var (generatedGrid, generatedPolys, generatedJumps) = result.Value;
            generatedPolys.Shuffle(rnd);

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

                // Puzzle unique!
                if (solutions.Length == 1)
                    break;

                // Find a wrong solution
                var (_, wrongAllPolys) = solutions.First(s => s.polys.Any(sPl => !generatedPolys.Contains(sPl)));
                // Find all wrong polyominoes in this wrong solution
                var wrongPolys = wrongAllPolys.Where(sPl => !generatedPolys.Contains(sPl)).ToArray();
                // Find all polyominoes that touch a wrong polyomino in the wrong solution, but do not touch the corresponding correct polyomino in the correct solution
                var touchingPolys = wrongPolys.SelectMany(wPl => wrongAllPolys
                      .Where(owPl => owPl.Touches(wPl) && !generatedPolys.First(gPl => gPl.Polyomino == wPl.Polyomino).Touches(generatedPolys.First(gPl => gPl.Polyomino == owPl.Polyomino)))
                      .Select(owPl => (one: wPl.Polyomino, two: owPl.Polyomino)))
                      .ToArray();
                if (touchingPolys.Length == 0)  // We cannot disambiguate this puzzle with a no-touch constraint
                    goto tryAgain;

                // Prefer one that isn’t already constrained
                var prefIx = touchingPolys.IndexOf(tup1 =>
                    !notAllowedToTouch.Any(tup2 => tup2.one == tup1.one || tup2.two == tup1.one) &&
                    !notAllowedToTouch.Any(tup2 => tup2.one == tup1.two || tup2.two == tup1.two));
                notAllowedToTouch.Add(touchingPolys[prefIx == -1 ? 0 : prefIx]);
            }

            // Assign the polyominoes colors
            var colors = new int[generatedPolys.Length];
            for (var i = 0; i < colors.Length; i++)
            {
                var availableColors = Enumerable.Range(0, 6).ToList();
                var already = generatedPolys.Take(i).IndexOf(prev => notAllowedToTouch.Any(tup => (tup.one == prev.Polyomino && tup.two == generatedPolys[i].Polyomino) || (tup.two == prev.Polyomino && tup.one == generatedPolys[i].Polyomino)));
                if (already != -1)
                    availableColors = new List<int> { colors[already] };
                for (var j = 0; j < i; j++)
                    if (generatedPolys[j].Touches(generatedPolys[i]))
                        availableColors.Remove(colors[j]);
                if (availableColors.Count == 0)
                    goto tryAgain;
                colors[i] = availableColors[rnd.Next(availableColors.Count)];
            }

            var polyColors = Enumerable.Range(0, generatedPolys.Length).Select(ix => (poly: generatedPolys[ix], color: colors[ix])).ToArray();

            int firstKeyColorIx;
            var attempts = 0;
            var keyPoly = polyColors.First(pc => pc.poly.Polyomino.Cells.Any(c => pc.poly.Place.AddWrap(c.x, c.y).Index == 0));
            do
            {
                attempts++;
                if (attempts > 10)
                    goto tryAgain;
                polyColors.Shuffle(rnd);
                firstKeyColorIx = (polyColors.IndexOf(keyPoly) + polyColors.Length - 1) % polyColors.Length;
            }
            while (Enumerable.Range(0, polyColors.Length).Any(ix => ix != firstKeyColorIx &&
                polyColors[ix].color == polyColors[firstKeyColorIx].color &&
                polyColors[(ix + 1) % polyColors.Length].color == polyColors[(firstKeyColorIx + 1) % polyColors.Length].color &&
                polyColors[(ix + 2) % polyColors.Length].color == polyColors[(firstKeyColorIx + 2) % polyColors.Length].color));

            var eqPolyIx = keyPoly.poly.Polyomino.Cells.IndexOf(tup => keyPoly.poly.Place.AddWrap(tup.x, tup.y).Index == 0);
            var suitsTargetPermutation = Enumerable.Range(0, 4).ToArray().Shuffle();
            while (suitsTargetPermutation.IndexOf(3) == eqPolyIx)
                suitsTargetPermutation.Shuffle();
            var suitPartialPermutationColor = new[] { "012", "021", "102", "120", "201", "210" }.IndexOf(suitsTargetPermutation.Where(suit => suit != 3).JoinString());

            var eqDiamondsIx = suitsTargetPermutation.IndexOf(3);
            var eqColorExtra = Enumerable.Range(0, 5).Except(new[] { eqPolyIx, eqDiamondsIx }).PickRandom(rnd);

            Console.WriteLine("Polys:");
            foreach (var (poly, color) in polyColors)
            {
                ConsoleUtil.WriteLine(poly.Polyomino.ToString().Color((ConsoleColor) (color + 1)));
                Console.WriteLine();
            }

            Console.WriteLine("Solution:");
            ConsoleUtil.WriteLine(VisualizePolyominoGrid(generatedGrid));
            Console.WriteLine();

            ConsoleUtil.WriteLine($@"COLOR STAGE: {Ut.NewArray(
                polyColors[firstKeyColorIx].color,
                polyColors[(firstKeyColorIx + 1) % polyColors.Length].color,
                polyColors[(firstKeyColorIx + 2) % polyColors.Length].color,
                eqColorExtra,
                suitPartialPermutationColor
            ).Select(color => $" {color} ".Color(ConsoleColor.White, (ConsoleColor) (color + 1))).JoinColoredString(" ")}", null);
            Console.WriteLine($"EQUATION STAGE: {eqColorExtra}, {eqDiamondsIx}, {eqPolyIx}");

            Console.WriteLine($"Word: {word}, jumps: {Convert.ToString(generatedJumps, 2).PadLeft(4, '0').Reverse().JoinString(" ")} 0");
            Console.WriteLine($"Encoded: {word.Select(ch => Convert.ToString(_polyominoAlphabet[ch], 2).PadLeft(5, '0')).JoinString(" ")}");
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