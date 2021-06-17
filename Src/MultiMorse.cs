using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using RT.Util;

namespace KtaneStuff
{
    internal class MultiMorse
    {
        public static (string morse, string ch)[] Morse =
                    @".-|-...|-.-.|-..|.|..-.|--.|....|..|.---|-.-|.-..|--|-.|---|.--.|--.-|.-.|...|-|..-|...-|.--|-..-|-.--|--..".Split('|').Select((str, ix) => (str, ((char) ('A' + ix)).ToString()))
                        .Concat(@"-----|.----|..---|...--|....-|.....|-....|--...|---..|----.".Split('|').Select((str, ix) => (str, ((char) ('0' + ix)).ToString())))
                        .ToArray();
        public static Dictionary<string, string> MorseFromCh = Morse.ToDictionary(m => m.ch, m => m.morse);
        public static Dictionary<string, string> ChFromMorse = Morse.ToDictionary(m => m.morse, m => m.ch);

        public static void GenerateMorseWav()
        {
            var words = new[]
            {
                "ABACK", "ABBEY", "ABBOT", "ABOVE", "ABUSE", "ACIDS", "ACRES", "ACTED", "ACTOR", "ACUTE", "ADAPT", "ADDED", "ADMIT", "ADOPT", "ADULT", "AGENT", "AGONY", "AGREE",
                "AHEAD", "AIDED", "AIMED", "AISLE", "ALARM", "ALBUM", "ALERT", "ALGAE", "ALIKE", "ALIVE", "ALLEY", "ALLOW", "ALLOY", "ALONE", "ALONG", "ALOOF", "ALOUD",
                "ALPHA", "ALTAR", "ALTER", "AMEND", "AMINO", "AMONG", "AMPLE", "ANGEL", "ANGER", "ANGLE", "ANGRY", "ANKLE", "APART", "APPLE", "APPLY", "APRON", "AREAS", "ARENA",
                "ARGUE", "ARISE", "ARMED", "AROMA", "AROSE", "ARRAY", "ARROW", "ARSON", "ASHES", "ASIDE", "ASKED", "ASSAY", "ASSET", "ATOMS", "ATTIC", "AUDIO", "AUDIT", "AVOID",
                "AWAIT", "AWAKE", "AWARD", "AWARE", "AWFUL", "AWOKE", "BACKS", "BACON", "BADGE", "BADLY", "BAKED", "BAKER", "BALLS", "BANDS", "BANKS", "BARGE", "BARON", "BASAL",
                "BASED", "BASES", "BASIC", "BASIN", "BASIS", "BATCH", "BATHS", "BEACH", "BEADS", "BEAMS", "BEANS", "BEARD", "BEARS", "BEAST", "BEECH", "BEERS", "BEGAN", "BEGIN",
                "BEGUN", "BEING", "BELLS", "BELLY", "BELTS", "BENCH", "BIBLE", "BIKES", "BILLS", "BIRDS", "BIRTH", "BLACK", "BLADE", "BLAME", "BLAND", "BLANK", "BLAST",
                "BLAZE", "BLEAK", "BLEAT", "BLEND", "BLESS", "BLIND", "BLOCK", "BLOKE", "BLOND", "BLOOD", "BLOOM", "BLOWN", "BLOWS", "BLUES", "BLUNT", "BOARD", "BOATS", "BOGUS",
                "BOLTS", "BONDS", "BONES", "BONUS", "BOOKS", "BOOST", "BOOTS", "BORED", "BORNE", "BOUND", "BOWED", "BOWEL", "BOWLS", "BOXED", "BOXER",
                "BRAKE", "BRAND", "BRASS", "BRAVE", "BREAD", "BREAM", "BREED", "BRIDE", "BRIEF", "BRING", "BRINK", "BRISK", "BROAD", "BROKE", "BROOM", "BROWN",
                "BROWS", "BRUSH", "BUILD", "BUILT", "BULBS", "BULKY", "BULLS", "BUNCH", "BUNNY", "BURNS", "BURNT", "BURST", "BUSES", "BUYER", "CABIN", "CABLE", "CACHE", "CAKES",
                "CALLS", "CAMPS", "CANAL", "CANDY", "CANOE", "CANON", "CARDS", "CARED", "CARER", "CARES", "CARGO", "CARRY", "CASES", "CATCH", "CATER", "CAUSE", "CAVES", "CEASE",
                "CELLS", "CENTS", "CHAIN", "CHAIR", "CHALK", "CHAOS", "CHAPS", "CHARM", "CHART", "CHASE", "CHEAP", "CHECK", "CHEEK", "CHEER", "CHESS", "CHEST", "CHIEF", "CHILD",
                "CHILL", "CHINA", "CHIPS", "CHOIR", "CHORD", "CHOSE", "CHUNK", "CIDER", "CIGAR", "CITED", "CITES", "CIVIC", "CIVIL", "CLAIM", "CLASH", "CLASS", "CLAWS", "CLEAN",
                "CLEAR", "CLERK", "CLICK", "CLIFF", "CLIMB", "CLOAK", "CLOCK", "CLOSE", "CLOTH", "CLOUD", "CLOWN", "CLUBS", "CLUCK", "CLUES", "CLUNG", "COACH", "COAST", "COATS",
                "COCOA", "CODES", "COINS", "COLON", "COMES", "COMIC", "CORAL", "CORPS", "COSTS", "COUCH", "COUGH", "COUNT", "COURT", "COVER", "CRACK", "CRAFT", "CRANE", "CRASH",
                "CRATE", "CRAZY", "CREAM", "CREED", "CREPT", "CREST", "CREWS", "CRIED", "CRIES", "CRIME", "CRISP", "CROPS", "CROSS", "CROWD", "CROWN", "CRUDE", "CRUEL", "CRUST",
                "CRYPT", "CUBIC", "CURLS", "CURLY", "CURRY", "CURSE", "CURVE", "CYCLE", "DADDY", "DAILY", "DAIRY", "DANCE", "DARED", "DATED", "DATES", "DEALS", "DEALT", "DEATH",
                "DEBIT", "DEBTS", "DEBUT", "DECAY", "DECOR", "DECOY", "DEEDS", "DEITY", "DELAY", "DENSE", "DEPOT", "DEPTH", "DERBY", "DERRY", "DESKS", "DETER", "DEVIL", "DIARY",
                "DIETS", "DIMLY", "DIRTY", "DISCO", "DISCS", "DISKS", "DITCH", "DIVED", "DIZZY", "DOCKS", "DODGY", "DOING", "DOLLS", "DONOR", "DOORS", "DOSES", "DOUBT", "DOUGH",
                "DOWNS", "DOZEN", "DRAFT", "DRAIN", "DRAMA", "DRANK", "DRAWN", "DRAWS", "DREAD", "DREAM", "DRESS", "DRIED", "DRIFT", "DRILL", "DRILY", "DRINK", "DRIVE", "DROPS",
                "DROVE", "DROWN", "DRUGS", "DRUMS", "DRUNK", "DUCHY", "DUCKS", "DUNES", "DUSTY", "DUTCH", "DWARF", "DYING", "EAGER", "EAGLE", "EARLY", "EARTH", "EASED", "EATEN",
                "EDGES", "EERIE", "EIGHT", "ELBOW", "ELDER", "ELECT", "ELITE", "ELVES", "EMPTY", "ENDED", "ENEMY", "ENJOY", "ENTER", "ENTRY", "ENVOY", "EQUAL", "ERECT", "ERROR",
                "ESSAY", "ETHOS", "EVENT", "EXACT", "EXAMS", "EXERT", "EXILE", "EXIST", "EXTRA", "FACED", "FACES", "FACTS", "FADED", "FAILS", "FAINT", "FAIRS", "FAIRY", "FAITH",
                "FALLS", "FALSE", "FAMED", "FANCY", "FARES", "FARMS", "FATAL", "FATTY", "FAULT", "FAUNA", "FEARS", "FEAST", "FEELS", "FELLA", "FENCE", "FERET", "FERRY", "FETAL",
                "FETCH", "FEVER", "FEWER", "FIBRE", "FIELD", "FIERY", "FIFTH", "FIFTY", "FIGHT", "FILED", "FILES", "FILLS", "FILMS", "FINAL", "FINDS", "FINED", "FINER", "FINES",
                "FIRED", "FIRES", "FIRMS", "FISTS", "FIVER", "FIXED", "FLAGS", "FLAIR", "FLAME", "FLANK", "FLASH", "FLASK", "FLATS", "FLAWS", "FLEET", "FLESH", "FLIES", "FLOAT",
                "FLOCK", "FLOOD", "FLOOR", "FLORA", "FLOUR", "FLOWN", "FLOWS", "FLUID", "FLUNG", "FLUNK", "FLUSH", "FLUTE", "FOCAL", "FOCUS", "FOLDS", "FOLKS", "FOLLY", "FONTS",
                "FOODS", "FOOLS", "FORCE", "FORMS", "FORTH", "FORTY", "FORUM", "FOURS", "FOXES", "FOYER", "FRAIL", "FRAME", "FRANC", "FRANK", "FRAUD", "FREAK", "FREED", "FRESH",
                "FRIED", "FROGS", "FRONT", "FROST", "FROWN", "FROZE", "FRUIT", "FUELS", "FULLY", "FUMES", "FUNDS", "FUNNY", "GAINS", "GAMES", "GANGS", "GASES", "GATES", "GAUGE",
                "GAZED", "GEESE", "GENES", "GENRE", "GENUS", "GHOST", "GIANT", "GIFTS", "GIRLS", "GIVEN", "GIVES", "GLARE", "GLASS", "GLEAM", "GLOBE", "GLOOM", "GLORY", "GLOSS",
                "GLOVE", "GOALS", "GOATS", "GOING", "GOODS", "GOOSE", "GORGE", "GRACE", "GRADE", "GRAIN", "GRAND", "GRANT", "GRAPH", "GRASP", "GRASS", "GRAVE", "GREED", "GREEK",
                "GREEN", "GREET", "GRIEF", "GRILL", "GRIPS", "GROOM", "GROSS", "GROUP", "GROWN", "GROWS", "GUARD", "GUESS", "GUEST", "GUIDE", "GUILD", "GUILT", "GUISE",
                "GULLS", "GULLY", "GYPSY", "HABIT", "HAIRS", "HAIRY", "HANDS", "HANDY", "HANGS", "HAPPY", "HARDY", "HARSH", "HASTE", "HASTY", "HATCH", "HATED", "HATES",
                "HAVEN", "HAVOC", "HEADS", "HEADY", "HEARD", "HEARS", "HEART", "HEATH", "HEAVY", "HEDGE", "HEELS", "HEFTY", "HEIRS", "HELPS", "HENCE", "HENRY", "HERBS",
                "HERDS", "HILLS", "HINTS", "HIRED", "HOBBY", "HOLDS", "HOLES", "HOLLY", "HOMES", "HONEY", "HOOKS", "HOPED", "HOPES", "HORNS", "HORSE", "HOSTS", "HOTEL", "HOURS",
                "HUMAN", "HUMUS", "HURRY", "HURTS", "HYMNS", "ICING", "ICONS", "IDEAL", "IDEAS", "IDIOT", "IMAGE", "IMPLY", "INDEX", "INDIA", "INERT", "INFER", "INNER", "INPUT",
                "IRONY", "ISSUE", "ITEMS", "IVORY", "JAPAN", "JEANS", "JELLY", "JEWEL", "JOINS", "JOINT", "JOKER", "JOKES", "JOLLY", "JOULE", "JOUST", "JUDGE", "JUICE",
                "KEEPS", "KICKS", "KILLS", "KINDS", "KINGS", "KNEES", "KNELT", "KNIFE", "KNOBS", "KNOCK", "KNOTS", "KNOWN", "KNOWS", "LABEL", "LACKS", "LAGER", "LAKES", "LAMBS",
                "LAMPS", "LANDS", "LANES", "LASER", "LASTS", "LATER", "LAUGH", "LAWNS", "LAYER", "LEADS", "LEANT", "LEAPT", "LEASE", "LEAST", "LEAVE", "LEDGE", "LEGAL", "LEMON",
                "LEVEL", "LEVER", "LIBEL", "LIFTS", "LIGHT", "LIKED", "LIKES", "LIMBS", "LIMIT", "LINED", "LINEN", "LINER", "LINES", "LINKS", "LIONS", "LISTS", "LITRE", "LIVED",
                "LIVER", "LIVES", "LOADS", "LOANS", "LOBBY", "LOCAL", "LOCKS", "LOCUS", "LODGE", "LOFTY", "LOGIC", "LOOKS", "LOOPS", "LOOSE", "LORDS", "LORRY", "LOSER", "LOSES",
                "LOTUS", "LOVED", "LOVER", "LOVES", "LOWER", "LOYAL", "LUCKY", "LUMPS", "LUNCH", "LUNGS", "LYING", "MACHO", "MADAM", "MAGIC", "MAINS", "MAIZE", "MAJOR", "MAKER",
                "MAKES", "MALES", "MANOR", "MARCH", "MARKS", "MARRY", "MARSH", "MASKS", "MATCH", "MATES", "MATHS", "MAYBE", "MAYOR", "MEALS", "MEANS", "MEANT", "MEDAL", "MEDIA",
                "MEETS", "MENUS", "MERCY", "MERGE", "MERIT", "MERRY", "MESSY", "METAL", "METER", "METRE", "MICRO", "MIDST", "MIGHT", "MILES", "MILLS", "MINDS", "MINER", "MINES",
                "MINOR", "MINUS", "MISTY", "MIXED", "MODEL", "MODEM", "MODES", "MOIST", "MOLES", "MONEY", "MONKS", "MONTH", "MOODS", "MOORS", "MORAL", "MOTIF", "MOTOR", "MOTTO",
                "MOULD", "MOUND", "MOUNT", "MOUSE", "MOUTH", "MOVED", "MOVES", "MOVIE", "MUDDY", "MUMMY", "MUSED", "MUSIC", "MYTHS", "NAILS", "NAIVE", "NAMED", "NAMES",
                "NANNY", "NASTY", "NAVAL", "NECKS", "NEEDS", "NERVE", "NESTS", "NEWER", "NEWLY", "NICER", "NICHE", "NIECE", "NIGHT", "NINTH", "NOBLE", "NODES", "NOISE",
                "NOISY", "NOMES", "NORMS", "NORTH", "NOSES", "NOTED", "NOTES", "NOVEL", "NURSE", "NUTTY", "NYLON", "OCCUR", "OCEAN", "ODDLY", "ODOUR", "OFFER", "OFTEN", "OLDER",
                "OLIVE", "ONION", "ONSET", "OPENS", "OPERA", "ORBIT", "ORDER", "ORGAN", "OUGHT", "OUNCE", "OUTER", "OVERS", "OVERT", "OWNED", "OWNER", "OXIDE", "OZONE", "PACKS",
                "PAGES", "PAINS", "PAINT", "PAIRS", "PALMS", "PANEL", "PANIC", "PANTS", "PAPAL", "PAPER", "PARKS", "PARTS", "PARTY", "PASTA", "PASTE", "PATCH", "PATHS", "PATIO",
                "PAUSE", "PEACE", "PEAKS", "PEARL", "PEARS", "PEERS", "PENAL", "PENCE", "PENNY", "PESTS", "PETTY", "PHASE", "PHONE", "PHOTO", "PIANO", "PICKS", "PIECE", "PIERS",
                "PILED", "PILES", "PILLS", "PILOT", "PINCH", "PINTS", "PIOUS", "PIPES", "PITCH", "PIZZA", "PLAIN", "PLANE", "PLANS", "PLATE", "PLAYS", "PLEAD", "PLEAS", "PLOTS",
                "PLUMP", "POEMS", "POETS", "POLAR", "POLES", "POLLS", "PONDS", "POOLS", "PORCH", "PORES", "PORTS", "POSED", "POSES", "POSTS", "POUND", "POWER", "PRESS", "PRICE",
                "PRIDE", "PRIME", "PRINT", "PRIOR", "PRIVY", "PRIZE", "PROBE", "PRONE", "PROOF", "PROSE", "PROUD", "PROVE", "PROXY", "PULLS", "PULSE", "PUMPS", "PUNCH", "PUPIL",
                "PUPPY", "PURSE", "QUACK", "QUEEN", "QUERY", "QUEST", "QUEUE", "QUICK", "QUIET", "QUITE", "QUOTA", "QUOTE", "RACED", "RACES", "RADAR", "RADIO", "RAIDS", "RAILS",
                "RAISE", "RALLY", "RANGE", "RANKS", "RAPID", "RATED", "RATES", "RATIO", "RAZOR", "REACH", "REACT", "READS", "READY", "REALM", "REBEL", "REFER", "REIGN",
                "REINS", "RELAX", "REMIT", "RENAL", "RENEW", "RENTS", "REPAY", "REPLY", "RESIN", "RESTS", "RIDER", "RIDGE", "RIFLE", "RIGID", "RINGS", "RIOTS", "RISEN", "RISES",
                "RISKS", "RISKY", "RITES", "RIVAL", "RIVER", "ROADS", "ROBES", "ROBOT", "ROCKS", "ROCKY", "ROGUE", "ROLES", "ROLLS", "ROMAN", "ROOFS", "ROOMS", "ROOTS", "ROPES",
                "ROSES", "ROTOR", "ROUGE", "ROUGH", "ROUND", "ROUTE", "ROVER", "ROYAL", "RUGBY", "RUINS", "RULED", "RULER", "RULES", "RURAL", "RUSTY", "SADLY", "SAFER", "SAILS",
                "SAINT", "SALAD", "SALES", "SALON", "SALTS", "SANDS", "SANDY", "SATIN", "SAUCE", "SAVED", "SAVES", "SCALE", "SCALP", "SCANT", "SCARF", "SCARS", "SCENE", "SCENT",
                "SCOOP", "SCOPE", "SCORE", "SCOTS", "SCRAP", "SCREW", "SCRUM", "SEALS", "SEAMS", "SEATS", "SEEDS", "SEEKS", "SEEMS", "SEIZE", "SELLS", "SENDS", "SENSE", "SERUM",
                "SERVE", "SEVEN", "SEXES", "SHADE", "SHADY", "SHAFT", "SHAKE", "SHAKY", "SHALL", "SHAME", "SHAPE", "SHARE", "SHARP", "SHEEP", "SHEER", "SHEET", "SHELF",
                "SHIFT", "SHINY", "SHIPS", "SHIRE", "SHIRT", "SHOCK", "SHOES", "SHONE", "SHOOK", "SHOOT", "SHOPS", "SHORE", "SHORT", "SHOTS", "SHOUT", "SHOWN", "SHOWS", "SHRUG",
                "SIDES", "SIEGE", "SIGHT", "SIGNS", "SILLY", "SINCE", "SINGS", "SITES", "SIXTH", "SIXTY", "SIZES", "SKIES", "SKILL", "SKINS", "SKIRT", "SKULL", "SLABS", "SLATE",
                "SLAVE", "SLEEK", "SLEEP", "SLEPT", "SLICE", "SLIDE", "SLOPE", "SLOTS", "SLUMP", "SMART", "SMELL", "SMILE", "SMOKE", "SNAKE", "SOBER", "SOCKS", "SOILS", "SOLAR",
                "SOLID", "SOLVE", "SONGS", "SORRY", "SORTS", "SOULS", "SOUTH", "SPACE", "SPADE", "SPARE", "SPARK", "SPATE", "SPAWN", "SPEAK", "SPEED", "SPEND", "SPENT", "SPIES",
                "SPINE", "SPLAT", "SPLIT", "SPOIL", "SPOKE", "SPOON", "SPORT", "SPOTS", "SPRAY", "SPURS", "SQUAD", "STACK", "STAFF", "STAGE", "STAIN", "STAIR", "STAKE", "STALE",
                "STALL", "STAMP", "STAND", "STARE", "STARK", "STARS", "START", "STATE", "STAYS", "STEAL", "STEAM", "STEEL", "STEEP", "STEER", "STEMS", "STEPS", "STERN",
                "STICK", "STIFF", "STOCK", "STOLE", "STONE", "STONY", "STOOD", "STOOL", "STOPS", "STORE", "STORM", "STORY", "STOUT", "STOVE", "STRAP", "STRAW", "STRAY",
                "STRIP", "STUCK", "STUFF", "STYLE", "SUEDE", "SUGAR", "SUITE", "SUITS", "SUNNY", "SUPER", "SURGE", "SWANS", "SWEAR", "SWEAT", "SWEEP", "SWEET", "SWEPT", "SWIFT",
                "SWING", "SWISS", "SWORD", "SWORE", "SWORN", "SWUNG", "TABLE", "TACIT", "TAILS", "TAKEN", "TAKES", "TALES", "TALKS", "TANKS", "TAPES", "TASKS", "TASTE", "TASTY",
                "TAXED", "TAXES", "TAXIS", "TEACH", "TEAMS", "TEARS", "TEDDY", "TEENS", "TEETH", "TELLS", "TELLY", "TEMPO", "TENDS", "TENOR", "TENSE", "TENTH", "TENTS", "TERMS",
                "TESTS", "TEXAS", "TEXTS", "THANK", "THEFT", "THEME", "THICK", "THIEF", "THIGH", "THIRD", "THOSE", "THREW", "THROW", "THUMB", "TIDAL", "TIDES", "TIGER", "TIGHT",
                "TILES", "TIMES", "TIMID", "TIRED", "TITLE", "TOAST", "TODAY", "TOKEN", "TONES", "TONIC", "TONNE", "TOOLS", "TOONS", "TOOTH", "TOPIC", "TORCH", "TOTAL", "TOUCH",
                "TOUGH", "TOURS", "TOWEL", "TOWER", "TOWNS", "TOXIC", "TRACE", "TRACK", "TRACT", "TRADE", "TRAIL", "TRAIN", "TRAIT", "TRAMP", "TRAMS", "TRAYS", "TREAT", "TREES",
                "TREND", "TRIAL", "TRIBE", "TRIED", "TRIES", "TRIPS", "TROOP", "TROUT", "TRUCE", "TRUCK", "TRULY", "TRUNK", "TRUST", "TRUTH", "TUBES", "TUMMY", "TUNES",
                "TUNIC", "TURKS", "TURNS", "TUTOR", "TWICE", "TWINS", "TWIST", "TYING", "TYPES", "TYRES", "ULCER", "UNBAN", "UNCLE", "UNDER", "UNDUE", "UNFIT", "UNION", "UNITE",
                "UNITS", "UNITY", "UNTIL", "UPPER", "UPSET", "URBAN", "URGED", "URINE", "USAGE", "USERS", "USING", "USUAL", "UTTER", "VAGUE", "VALID", "VALUE", "VALVE", "VAULT",
                "VEINS", "VENUE", "VERBS", "VERGE", "VICAR", "VIDEO", "VIEWS", "VILLA", "VINES", "VINYL", "VIRUS", "VISIT", "VITAL", "VIVID", "VOCAL", "VODKA", "VOICE",
                "VOTED", "VOTER", "VOTES", "VOWED", "VOWEL", "WAGES", "WAGON", "WAIST", "WAITS", "WALKS", "WALLS", "WANTS", "WARDS", "WARES", "WARNS", "WASTE", "WATCH", "WAVED",
                "WAVES", "WEARS", "WEARY", "WEDGE", "WEEDS", "WEEKS", "WEIGH", "WEIRD", "WELLS", "WELSH", "WHALE", "WHEAT", "WHEEL", "WHILE", "WHITE", "WHOLE", "WHOSE", "WIDEN",
                "WIDER", "WIDOW", "WIDTH", "WILLS", "WINDS", "WINDY", "WINES", "WINGS", "WIPED", "WIRES", "WISER", "WITCH", "WITTY", "WIVES", "WOKEN", "WOMAN", "WOMEN", "WOODS",
                "WORDS", "WORKS", "WORMS", "WORRY", "WORSE", "WORST", "WORTH", "WOUND", "WOVEN", "WRATH", "WRECK", "WRIST", "WRONG", "WROTE", "WRYLY", "XEROX", "YACHT", "YARDS",
                "YAWNS", "YEARS", "YEAST", "YIELD", "YOUNG", "YOURS", "YOUTH", "ZILCH", "ZONES"
            };
            var word = words[Rnd.Next(0, words.Length)];
            word = "TOM";
            var letterSamples = new double[word.Length][];
            var rate = 44100;
            var amplitude = 0x1555;

            for (var i = 0; i < word.Length; i++)
            {
                var freq = 200 * Math.Pow(2.2, i);
                var morse = MorseFromCh[word[i].ToString()].Replace("-", "### ").Replace(".", "# ") + "      ";
                letterSamples[i] = Ut.NewArray(rate * morse.Length / 10, ix => morse[ix * 10 / rate] == '#' ? amplitude * Math.Sin((double) ix * 2 * Math.PI * freq / rate) : 0);
            }

            var allSamples = Ut.NewArray(rate * 30, ix => Enumerable.Range(0, word.Length).Sum(ltr => letterSamples[ltr][(ix + (word.Length - ltr) * rate / 10) % letterSamples[ltr].Length]));
            var dataAsBytes = allSamples.SelectMany(smpl => BitConverter.GetBytes((short) smpl)).ToArray();

            var file = Ut.ArrayConcat(
                new byte[] { 0x52, 0x49, 0x46, 0x46 },
                BitConverter.GetBytes(dataAsBytes.Length + 0x24),
                new byte[] { 0x57, 0x41, 0x56, 0x45, 0x66, 0x6D, 0x74, 0x20, 0x10, 0x00, 0x00, 0x00, 0x01, 0x00, 0x01, 0x00, 0x44, 0xAC, 0x00, 0x00, 0x88, 0x58, 0x01, 0x00, 0x02, 0x00, 0x10, 0x00, 0x64, 0x61, 0x74, 0x61 },
                BitConverter.GetBytes(dataAsBytes.Length),
                dataAsBytes);
            File.WriteAllBytes(@"D:\temp\temp.wav", file);
            File.WriteAllText(@"D:\temp\temp.txt", word);
        }
    }
}