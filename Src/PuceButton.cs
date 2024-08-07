using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using PuzzleSolvers;
using RT.Util;
using RT.Util.Consoles;
using RT.Util.ExtensionMethods;

namespace KtaneStuff
{
    static class PuceButton
    {
        public static void GenerateDoubleSlitherlink_Abandoned()
        {
            var rnd = new Random(47);
            var solutionWord = "LINK";
            var w = 5;
            var h = solutionWord.Length;
            var desire = Enumerable.Range(0, solutionWord.Length).SelectMany(ltr => Enumerable.Range(0, w).Select(bit => ((solutionWord[ltr] - 'A' + 1) & (1 << (w - 1 - bit))) != 0)).ToArray();
            foreach (var solutionGrid in Solve(new MultiSlitherlinkState(5, solutionWord.Length, 2), null, desire, rnd))
            {
                for (var y = 0; y < h; y++)
                {
                    var gridsStrs = Enumerable.Range(0, solutionGrid.segments.Length).Select(g => Enumerable.Range(0, w).Select(bit => solutionGrid.sofar[bit + w * y][g] ? "██" : "░░").JoinString()).JoinString("   ");
                    var solutionGridStr = Enumerable.Range(0, w).Select(bit => desire[bit + w * y] ? "██" : "░░").JoinString();
                    Console.WriteLine($"{gridsStrs}   {solutionGridStr}");
                }
                Console.WriteLine();

                var (values, anyNull) = solutionGrid.GetCellValues().Value;
                var allClues = new List<(int ix, int clue)>();
                for (var i = 0; i < w * h; i++)
                    foreach (var cl in values[i])
                        allClues.Add((i, cl));
                allClues.Shuffle(rnd);

                var reqClues = Ut.ReduceRequiredSet(Enumerable.Range(0, allClues.Count), skipConsistencyTest: true, test: state =>
                {
                    var clueSet = state.SetToTest;
                    var statusStr = Enumerable.Range(0, allClues.Count).Select(ix => clueSet.Contains(ix) ? "█" : "░").JoinString();

                    var testClues = new int[w * h][];
                    foreach (var clueIx in clueSet)
                        testClues[allClues[clueIx].ix] = testClues[allClues[clueIx].ix] == null ? new[] { allClues[clueIx].clue } : testClues[allClues[clueIx].ix].Insert(0, allClues[clueIx].clue);

                    var numSolutions = Solve(new MultiSlitherlinkState(5, solutionWord.Length, 2), testClues, desiredXor: null, rnd: null).Take(2).ToArray();
                    var solutionUnique = numSolutions.Length == 1;
                    ConsoleUtil.WriteLine($@"(""{statusStr}"", {solutionUnique.ToString().ToLowerInvariant()}),".Color(solutionUnique ? ConsoleColor.Green : ConsoleColor.Red) + new string(' ', w * h + 4));
                    return solutionUnique;
                });

                foreach (var req in reqClues)
                    Console.WriteLine($"{(char) ('A' + allClues[req].ix % w)}{1 + allClues[req].ix / w} = {allClues[req].clue}");
                Debugger.Break();
            }
        }

        static IEnumerable<MultiSlitherlinkState> Solve(MultiSlitherlinkState cur, int[][] clues, bool[] desiredXor, Random rnd)
        {
            var ix = cur.sofar.IndexOf(b => b == null);
            if (ix == -1)
            {
                yield return cur;
                yield break;
            }

            var ofs = rnd == null ? 0 : rnd.Next(0, (1 << cur.segments.Length));

            for (var ir = 0; ir < (1 << cur.segments.Length); ir++)
            {
                var i = (ir + ofs) % (1 << cur.segments.Length);
                if (desiredXor != null)
                {
                    var xor = false;
                    var j = i;
                    while (j > 0)
                    {
                        xor = !xor;
                        j &= j - 1;
                    }
                    if (xor != desiredXor[ix])
                        continue;
                }

                var newState = cur.SetPixel(ix, Enumerable.Range(0, cur.segments.Length).Select(ix => (i & (1 << ix)) != 0).ToArray());
                if (newState.IsValid(clues))
                    foreach (var solution in Solve(newState, clues, desiredXor, rnd))
                        yield return solution;
            }
        }

        public sealed class MultiSlitherlinkState
        {
            public int w, h;
            public bool[][] sofar;     // indexed by cell, then grid
            public List<(int ix1, int ix2)>[] segments;     // indexed by grid
            public int[] closedLoops;   // indexed by grid

            public MultiSlitherlinkState(int width, int height, int numGrids)
            {
                w = width;
                h = height;
                sofar = new bool[width * height][];
                segments = Ut.NewArray(numGrids, _ => new List<(int ix1, int ix2)>());
                closedLoops = new int[numGrids];
            }

            private MultiSlitherlinkState() { }

            void addLine(int g, int ix1, int ix2)
            {
                var segm = segments[g];
                var e1 = segm.IndexOf(tup => tup.ix1 == ix1 || tup.ix2 == ix1);
                var e2 = segm.IndexOf(tup => tup.ix1 == ix2 || tup.ix2 == ix2);
                if (e1 != -1 && e2 == e1)
                {
                    closedLoops[g]++;
                    segm.RemoveAt(e1);
                }
                else if (e1 != -1 && e2 != -1)
                {
                    var o1 = segm[e1].ix1 == ix1 ? segm[e1].ix2 : segm[e1].ix1;
                    var o2 = segm[e2].ix1 == ix2 ? segm[e2].ix2 : segm[e2].ix1;
                    segm.RemoveAt(Math.Max(e1, e2));
                    segm.RemoveAt(Math.Min(e1, e2));
                    segm.Add((o1, o2));
                }
                else if (e1 != -1)
                    segm[e1] = (ix2, segm[e1].ix1 == ix1 ? segm[e1].ix2 : segm[e1].ix1);
                else if (e2 != -1)
                    segm[e2] = (ix1, segm[e2].ix1 == ix2 ? segm[e2].ix2 : segm[e2].ix1);
                else
                    segm.Add((ix1, ix2));
            }

            public MultiSlitherlinkState SetPixel(int ix, bool[] vs)
            {
                if (vs == null)
                    throw new ArgumentNullException(nameof(vs));
                if (vs.Length != segments.Length)
                    throw new ArgumentException($"‘vs’ array has size {vs.Length} but expected size {segments.Length}.", nameof(vs));
                var x = ix % w;
                var y = ix / w;
                if (x < 0 || x >= w || y < 0 || y >= h)
                    throw new ArgumentException($"‘ix’ is {ix} but expected 0–{w * h - 1}.", nameof(ix));
                var n = Clone();
                n.sofar[ix] = vs;

                for (var g = 0; g < segments.Length; g++)
                {
                    var v = vs[g];
                    if (x > 0 ? n.sofar[ix - 1]?[g] == !v : v)
                        n.addLine(g, x + (w + 1) * y, x + (w + 1) * (y + 1));
                    if (x < w - 1 ? n.sofar[ix + 1]?[g] == !v : v)
                        n.addLine(g, x + 1 + (w + 1) * y, x + 1 + (w + 1) * (y + 1));
                    if (y > 0 ? n.sofar[ix - w]?[g] == !v : v)
                        n.addLine(g, x + (w + 1) * y, x + 1 + (w + 1) * y);
                    if (y < h - 1 ? n.sofar[ix + w]?[g] == !v : v)
                        n.addLine(g, x + (w + 1) * (y + 1), x + 1 + (w + 1) * (y + 1));
                }
                return n;
            }

            private MultiSlitherlinkState Clone() => new()
            {
                w = w,
                h = h,
                sofar = sofar.Select(s => s?.ToArray()).ToArray(),
                segments = segments.Select(s => s.ToList()).ToArray(),
                closedLoops = closedLoops.ToArray()
            };

            public bool IsValid(int[][] clues, bool antiKnight = false, bool antiBishop = false)  // indexed by cell location; inner array is list of numbers for that cell
            {
                // Check for lexicographical ordering
                var notChecked = Enumerable.Range(1, segments.Length - 1).ToList();
                for (var cell = 0; cell < w * h; cell++)
                    if (sofar[cell] != null)
                        foreach (var g in notChecked.ToArray())
                        {
                            if (sofar[cell][g] && !sofar[cell][g - 1])
                                notChecked.Remove(g);
                            else if (!sofar[cell][g] && sofar[cell][g - 1])
                                return false;
                        }

                // Cancel if there is a prematurely closed loop
                for (var g = 0; g < segments.Length; g++)
                    if (closedLoops[g] > 1 || (closedLoops[g] == 1 && segments[g].Count > 0))
                        return false;

                // If sofar is filled, and we don’t need to check for given clues or anti-knight or anti-bishop, then we are done
                if (clues == null && !antiKnight && !antiBishop && sofar.All(s => s != null))
                    return closedLoops.All(c => c == 1);

                // Calculate the values of each cell in order to check against given clues, anti-knight or anti-bishop
                if (GetCellValues(clues) is not (int[][] values, bool anyNull))
                    return false;

                if (antiKnight)
                    for (var cell = 0; cell < w * h; cell++)
                        if (values[cell] != null)
                            foreach (var knight in AntiKnightConstraint.KnightsMoves(cell, w, h, toroidal: false))
                                for (var g = 0; g < segments.Length; g++)
                                    if (values[knight] != null && values[knight][g] == values[cell][g])
                                        return false;

                if (antiBishop)
                    for (var cell = 0; cell < w * h; cell++)
                        if (values[cell] != null)
                            foreach (var bishop in AntiBishopConstraint.BishopsMoves(cell, w, h))
                                for (var g = 0; g < segments.Length; g++)
                                    if (values[bishop] != null && values[bishop][g] == values[cell][g])
                                        return false;

                return anyNull || closedLoops.All(c => c == 1);
            }

            public (int[][] values, bool anyNull)? GetCellValues(int[][] checkClues = null)
            {
                var values = new int[w * h][];
                var anyNull = false;
                for (var cell = 0; cell < w * h; cell++)
                {
                    if (sofar[cell] == null)
                    {
                        anyNull = true;
                        continue;
                    }
                    values[cell] = new int[segments.Length];

                    var x = cell % w;
                    var y = cell / w;
                    var hasNull = false;
                    for (var g = 0; g < segments.Length; g++)
                    {
                        // No checkerboard patterns allowed
                        if (x > 0 && y > 0 &&
                                sofar[cell] != null && sofar[cell - 1] != null && sofar[cell - w] != null && sofar[cell - w - 1] != null &&
                                sofar[cell - w - 1][g] == sofar[cell][g] && sofar[cell - w][g] != sofar[cell][g] && sofar[cell - 1][g] != sofar[cell][g])
                            return null;

                        var v = sofar[cell][g];
                        var nd = 0;
                        if (x > 0) { if (sofar[cell - 1] == null) hasNull = true; else if (sofar[cell - 1][g] != v) nd++; } else if (v) nd++;
                        if (x < w - 1) { if (sofar[cell + 1] == null) hasNull = true; else if (sofar[cell + 1][g] != v) nd++; } else if (v) nd++;
                        if (y > 0) { if (sofar[cell - w] == null) hasNull = true; else if (sofar[cell - w][g] != v) nd++; } else if (v) nd++;
                        if (y < h - 1) { if (sofar[cell + w] == null) hasNull = true; else if (sofar[cell + w][g] != v) nd++; } else if (v) nd++;
                        values[cell][g] = nd;
                    }
                    if (hasNull)
                    {
                        values[cell] = null;
                        continue;
                    }
                    if (checkClues == null || checkClues[cell] == null || checkClues[cell].Length == 0)
                        continue;

                    var nDiff = values[cell].ToList();
                    for (var cl = 0; cl < checkClues[cell].Length; cl++)
                        if (!nDiff.Remove(checkClues[cell][cl]))
                            return null;
                }
                return (values, anyNull);
            }

            public string Visualize(int g) => Enumerable.Range(0, w * h).Select(c => sofar[c] == null ? "??" : sofar[c][g] ? "██" : "░░").Split(w).Select(r => r.JoinString()).JoinString("\n");

            public string VisualizeAll() => Enumerable.Range(0, segments.Length).Select(g => Visualize(g).Split('\n')).Aggregate((p, n) => p.Zip(n, (a, b) => $"{a}    {b}").ToArray()).JoinString("\n");
        }

        private static readonly string[] _words = new[] { "ABIDE", "ABORT", "ABOUT", "ABOVE", "ABYSS", "ACIDS", "ACORN", "ACRES", "ACTED", "ACTOR", "ACUTE", "ADDER", "ADDLE", "ADIEU", "ADIOS", "ADMIN", "ADMIT", "ADOPT", "ADORE", "ADORN", "ADULT", "AFFIX", "AFTER", "AGILE", "AGING", "AGORA", "AGREE", "AHEAD", "AIDED", "AIMED", "AIOLI", "AIRED", "AISLE", "ALARM", "ALBUM", "ALIAS", "ALIBI", "ALIEN", "ALIGN", "ALIKE", "ALIVE", "ALLAY", "ALLEN", "ALLOT", "ALLOY", "ALOFT", "ALONE", "ALONG", "ALOOF", "ALOUD", "ALPHA", "ALTAR", "ALTER", "AMASS", "AMAZE", "AMBLE", "AMINO", "AMISH", "AMISS", "AMONG", "AMPLE", "AMUSE", "ANGLE", "ANGLO", "ANGRY", "ANGST", "ANIME", "ANION", "ANISE", "ANKLE", "ANNEX", "ANNOY", "ANNUL", "ANTIC", "ANVIL", "AORTA", "APNEA", "APPLE", "APRON", "AREAS", "ARENA", "ARGUE", "ARISE", "ARMED", "ARMOR", "AROSE", "ASHEN", "ASHES", "ASIAN", "ASIDE", "ASSET", "ASTER", "ASTIR", "ATOLL", "ATOMS", "ATONE", "ATTIC", "AUDIO", "AUDIT", "AUGUR", "AUNTY", "AVAIL", "AVIAN", "AVOID", "AWAIT", "AWAKE", "AWARE", "AWASH", "AXIAL", "AXIOM", "AXION", "AZTEC", "BIBLE", "BIDET", "BIGHT", "BILGE", "BILLS", "BINGE", "BINGO", "BIOME", "BIRCH", "BIRDS", "BIRTH", "BISON", "BITER", "BLADE", "BLAME", "BLAND", "BLARE", "BLAZE", "BLEAT", "BLEED", "BLEEP", "BLIMP", "BLIND", "BLING", "BLINK", "BLISS", "BLITZ", "BLOND", "BLOOM", "BLOOP", "BLUES", "BLUNT", "BLUSH", "BOGGY", "BOGUS", "BOLTS", "BONDS", "BONED", "BONER", "BONES", "BONNY", "BONUS", "BOOST", "BOOTH", "BOOTS", "BORAX", "BORED", "BORER", "BORNE", "BORON", "BOTCH", "BOUGH", "BOULE", "BRACE", "BRAID", "BRAIN", "BRAKE", "BRAND", "BRASH", "BRASS", "BRAVE", "BRAWL", "BRAWN", "BRAZE", "BREAD", "BREAK", "BREAM", "BREED", "BRIAR", "BRIBE", "BRICK", "BRIDE", "BRIEF", "BRIER", "BRINE", "BRING", "BRINK", "BRINY", "BRISK", "BROIL", "BRONX", "BROOM", "BROTH", "BRUNT", "BRUSH", "BRUTE", "BUCKS", "BUDDY", "BUDGE", "BUGGY", "BUILD", "BUILT", "BULBS", "BULGE", "BULLS", "BUMPY", "BUNCH", "BUNNY", "BUSES", "BUZZY", "BYLAW", "BYWAY", "CABBY", "CABIN", "CABLE", "CACHE", "CAIRN", "CAKES", "CALLS", "CALVE", "CALYX", "CAMPS", "CAMPY", "CANAL", "CANED", "CANNY", "CANON", "CARDS", "CARVE", "CASED", "CASES", "CASTE", "CATCH", "CAULK", "CAUSE", "CAVES", "CEASE", "CEDED", "CELLS", "CENTS", "CHAFE", "CHAFF", "CHAIN", "CHAIR", "CHALK", "CHAMP", "CHANT", "CHAOS", "CHAPS", "CHARM", "CHART", "CHARY", "CHASE", "CHASM", "CHEAP", "CHEAT", "CHECK", "CHEMO", "CHESS", "CHEST", "CHICK", "CHIDE", "CHILD", "CHILI", "CHILL", "CHIME", "CHINA", "CHIPS", "CHORD", "CHORE", "CHOSE", "CHUCK", "CHUNK", "CHUTE", "CINCH", "CITED", "CITES", "CIVET", "CIVIC", "CIVIL", "CLADE", "CLAIM", "CLANK", "CLASH", "CLASS", "CLAWS", "CLEAN", "CLEAR", "CLEAT", "CLICK", "CLIFF", "CLIMB", "CLING", "CLONE", "CLOSE", "CLOTH", "CLOUD", "CLOUT", "CLOVE", "CLUBS", "CLUCK", "CLUES", "CLUNG", "CLUNK", "COINS", "COLIC", "COLON", "COLOR", "COMAL", "COMES", "COMIC", "COMMA", "CONCH", "CONIC", "CORAL", "CORGI", "CORNY", "CORPS", "COSTS", "COTTA", "COUCH", "COUGH", "COULD", "COUNT", "COVEN", "COYLY", "CRACK", "CRANE", "CRANK", "CRASH", "CRASS", "CRATE", "CRAVE", "CREAK", "CREAM", "CREED", "CREWS", "CRIED", "CRIES", "CRIME", "CRONE", "CROPS", "CROSS", "CRUDE", "CRUEL", "CRUSH", "CUBBY", "CUBIT", "CUMIN", "CUTIE", "CYCLE", "CYNIC", "CZECH", "DACHA", "DAILY", "DALLY", "DANCE", "DATED", "DATES", "DATUM", "DEALS", "DEALT", "DEATH", "DEBIT", "DEBTS", "DEBUG", "DEBUT", "DECAF", "DECAL", "DECOR", "DECOY", "DEEDS", "DEIST", "DELFT", "DELVE", "DEMUR", "DENIM", "DENSE", "DESKS", "DETER", "DETOX", "DEUCE", "DEVIL", "DICED", "DIETS", "DIGIT", "DIMLY", "DINAR", "DINER", "DINGY", "DIRTY", "DISCO", "DISCS", "DISKS", "DITCH", "DITTY", "DITZY", "DIVAN", "DIVED", "DIVER", "DIVOT", "DIVVY", "DOGGY", "DOGMA", "DOING", "DOLLS", "DOMED", "DONOR", "DONUT", "DOORS", "DORIC", "DOSED", "DOSES", "DOTTY", "DOUGH", "DOUSE", "DRAFT", "DRAIN", "DRAMA", "DRANK", "DREAM", "DRESS", "DRIED", "DRIER", "DRIFT", "DRILL", "DRILY", "DRINK", "DRIVE", "DROLL", "DRONE", "DROPS", "DROVE", "DRUGS", "DRUMS", "DRUNK", "DRYER", "DUCAT", "DUCKS", "DUMMY", "DUNCE", "DUNES", "DUTCH", "DUVET", "DWARF", "DWELL", "DYING", "EAGLE", "EARED", "EARLY", "EARTH", "EASED", "EASEL", "EATEN", "ECLAT", "EDEMA", "EDICT", "EDIFY", "EGRET", "EIDER", "EIGHT", "ELATE", "ELDER", "ELECT", "ELIDE", "ELITE", "ELUDE", "EMCEE", "EMOTE", "ENACT", "ENEMA", "ENNUI", "ENSUE", "ENTER", "ENVOY", "ETHIC", "ETHOS", "ETUDE", "EVADE", "EVICT", "EXACT", "EXALT", "EXAMS", "EXILE", "EXIST", "EXTRA", "EXUDE", "FAILS", "FAINT", "FAIRY", "FAITH", "FALLS", "FALSE", "FAMED", "FANCY", "FATAL", "FATED", "FATTY", "FATWA", "FAULT", "FAVOR", "FECAL", "FEINT", "FETAL", "FIBRE", "FIFTH", "FIFTY", "FIGHT", "FILCH", "FILED", "FILET", "FILLE", "FILLS", "FILLY", "FILMS", "FILMY", "FILTH", "FINAL", "FINDS", "FINED", "FINNY", "FIRED", "FIRMS", "FIRST", "FISTS", "FLAIL", "FLAIR", "FLATS", "FLEET", "FLING", "FLIRT", "FLOOR", "FLORA", "FLOUT", "FLUID", "FLUNG", "FLYBY", "FOGGY", "FOIST", "FOLIC", "FOLIO", "FOLLY", "FONTS", "FORAY", "FORGO", "FORMS", "FORTE", "FORTH", "FORTY", "FORUM", "FOUNT", "FOVEA", "FRAIL", "FRAUD", "FREED", "FRIED", "FRILL", "FRISK", "FRONT", "FRUIT", "FUELS", "FULLY", "FUNNY", "FUSED", "FUTON", "FUZZY", "GHOUL", "GIANT", "GIDDY", "GIFTS", "GIMPY", "GIRLS", "GIRLY", "GIRTH", "GIVEN", "GIZMO", "GLAND", "GLEAM", "GLEAN", "GLIAL", "GLINT", "GLOOM", "GLORY", "GLUED", "GLUON", "GOING", "GOLLY", "GOOFY", "GOOPY", "GRAFT", "GRAIN", "GRAND", "GRANT", "GRASS", "GRATE", "GRAVE", "GRAVY", "GREAT", "GREED", "GREEN", "GREET", "GRILL", "GRIME", "GRIMY", "GRIND", "GRIPS", "GROIN", "GROOM", "GROSS", "GROUT", "GRUEL", "GRUMP", "GRUNT", "GUANO", "GUARD", "GUAVA", "GUEST", "GUILD", "GUILT", "GUISE", "GULLS", "GULLY", "GUMMY", "GUNKY", "GUNNY", "GUSHY", "GUSTY", "GUTSY", "GYRUS", "HABIT", "HAIKU", "HAIRS", "HAIRY", "HALAL", "HALVE", "HAMMY", "HANDS", "HANDY", "HANGS", "HARDY", "HAREM", "HARPY", "HARSH", "HASTE", "HASTY", "HATCH", "HATED", "HATES", "HAUNT", "HAVEN", "HAZEL", "HEADS", "HEADY", "HEARD", "HEARS", "HEART", "HEATH", "HEAVE", "HEAVY", "HEELS", "HEIRS", "HEIST", "HELIX", "HELLO", "HENRY", "HILLS", "HILLY", "HINDI", "HINDU", "HINTS", "HIRED", "HITCH", "HOBBY", "HOIST", "HOLLY", "HOMED", "HONOR", "HORNS", "HORSE", "HOSEL", "HOTLY", "HOUND", "HUBBY", "HUGGY", "HULLO", "HUMAN", "HUMID", "HUMOR", "ICHOR", "ICILY", "ICING", "ICONS", "IDEAL", "IDEAS", "IDIOM", "IDIOT", "IDLED", "IDYLL", "IGLOO", "ILIAC", "ILIUM", "IMAGO", "IMBUE", "IMPLY", "INANE", "INCAN", "INCUS", "INDEX", "INDIA", "INDIE", "INFRA", "INGOT", "INLAY", "INLET", "INPUT", "INSET", "INTRO", "INUIT", "IONIC", "IRISH", "IRONY", "ISLET", "ISSUE", "ITEMS", "IVORY", "JOINS", "JOINT", "JOULE", "JUMBO", "JUNTA", "KANJI", "KARAT", "KARMA", "KIDDO", "KILLS", "KINDA", "KINDS", "KINGS", "KITTY", "KNAVE", "KNEES", "KNELT", "KNIFE", "KNOBS", "KNOLL", "KNOTS", "KUDOS", "KUDZU", "LABOR", "LACED", "LACKS", "LADLE", "LAITY", "LAMBS", "LAMPS", "LANDS", "LANES", "LAPIN", "LAPSE", "LARGE", "LARVA", "LASER", "LASSO", "LATCH", "LATER", "LATHE", "LATIN", "LATTE", "LAUGH", "LAWNS", "LAYER", "LAYUP", "LEACH", "LEADS", "LEAFY", "LEANT", "LEAPT", "LEARN", "LEASE", "LEASH", "LEAVE", "LEDGE", "LEECH", "LEGGY", "LEMMA", "LEMON", "LEMUR", "LIANA", "LIDAR", "LIEGE", "LIFTS", "LIGHT", "LIKEN", "LIKES", "LILAC", "LIMBO", "LIMBS", "LIMIT", "LINED", "LINEN", "LINER", "LINES", "LINGO", "LINKS", "LIONS", "LIPID", "LISTS", "LITER", "LITRE", "LIVED", "LIVEN", "LIVER", "LIVES", "LIVID", "LLAMA", "LOBBY", "LOFTY", "LOGIC", "LOGON", "LOLLY", "LONER", "LOONY", "LOOPS", "LOOPY", "LOOSE", "LORDS", "LORRY", "LOSER", "LOSES", "LOTTO", "LOTUS", "LOUSE", "LOVED", "LOVER", "LOVES", "LOYAL", "LUCID", "LUCRE", "LUMEN", "LUMPS", "LUMPY", "LUNAR", "LUNCH", "LUNGE", "LUNGS", "LUSTY", "LYING", "LYMPH", "LYNCH", "LYRIC", "MADAM", "MADLY", "MAGIC", "MAGMA", "MAINS", "MAJOR", "MALAY", "MALTA", "MAMBO", "MANGO", "MANGY", "MANIA", "MANIC", "MANLY", "MANOR", "MASKS", "MATCH", "MATED", "MATHS", "MATTE", "MAVEN", "MAXIM", "MAYAN", "MAYOR", "MEALS", "MEANS", "MEANT", "MEATY", "MEDAL", "MEDIA", "MEDIC", "MEETS", "MELON", "MESON", "METAL", "MICRO", "MIDST", "MIGHT", "MILES", "MILLS", "MIMIC", "MINCE", "MINDS", "MINED", "MINES", "MINOR", "MINTY", "MINUS", "MIRED", "MIRTH", "MISTY", "MITRE", "MOGUL", "MOIST", "MOLAR", "MOLDY", "MONTH", "MOONY", "MOORS", "MOOSE", "MORAL", "MORAY", "MORPH", "MOTEL", "MOTIF", "MOTOR", "MOTTO", "MOULD", "MOUND", "MOUNT", "MOUSE", "MOUTH", "MOVED", "MOVIE", "MUCUS", "MUDDY", "MUGGY", "MULCH", "MULTI", "MUMMY", "MUNCH", "MUSED", "MUSIC", "MUSTY", "MUTED", "MUZZY", "MYTHS", "NADIR", "NAILS", "NAIVE", "NAMED", "NAMES", "NANNY", "NARCO", "NASAL", "NATAL", "NATTY", "NAVAL", "NAVEL", "NEATH", "NECKS", "NEEDS", "NEEDY", "NEIGH", "NESTS", "NEWLY", "NEXUS", "NICER", "NICHE", "NIECE", "NIFTY", "NIGHT", "NIGRA", "NINTH", "NITRO", "NOBLE", "NOBLY", "NOISE", "NOMAD", "NOMES", "NONCE", "NOOSE", "NORMS", "NORSE", "NORTH", "NOSES", "NOTCH", "NOTED", "NOTES", "NOVEL", "NUDGE", "NUTTY", "NYLON", "NYMPH", "OFFER", "OFTEN", "OILED", "OLDER", "OLDIE", "OLIVE", "OMANI", "ONION", "ONSET", "OOMPH", "OPINE", "OPIUM", "OPTIC", "ORBIT", "ORDER", "OTHER", "OTTER", "OUGHT", "OUNCE", "OUTDO", "OUTER", "OVOID", "OVULE", "OXBOW", "OXIDE", "PHASE", "PHONE", "PHOTO", "PIANO", "PIECE", "PIGGY", "PILAF", "PILED", "PILLS", "PILOT", "PINCH", "PINTS", "PISTE", "PITCH", "PIVOT", "PIXEL", "PIXIE", "PLACE", "PLAIN", "PLAIT", "PLANE", "PLANS", "PLANT", "PLATE", "PLAYS", "PLEAS", "PLEAT", "PLOTS", "PLUMB", "PLUME", "PLUMP", "POINT", "POLAR", "POLIO", "POLLS", "POLYP", "PONDS", "POOLS", "PORCH", "PORTS", "POSED", "POSIT", "POSTS", "POUCH", "PREEN", "PRICE", "PRICY", "PRIDE", "PRIMA", "PRIME", "PRIMP", "PRINT", "PRION", "PRIOR", "PRISE", "PRISM", "PRIVY", "PRIZE", "PROMO", "PRONE", "PRONG", "PROOF", "PROSE", "PROUD", "PROVE", "PRUDE", "PRUNE", "PUDGY", "PULLS", "PUNCH", "PUNIC", "PYLON", "QUAIL", "QUALM", "QUASI", "QUEEN", "QUELL", "QUILL", "QUILT", "QUINT", "RABBI", "RADAR", "RADIO", "RAGGY", "RAIDS", "RAILS", "RAINY", "RAISE", "RALLY", "RANCH", "RANGE", "RANGY", "RATED", "RATIO", "RATTY", "RAZOR", "REACH", "REACT", "READS", "REALM", "REARM", "RECAP", "RECON", "RECTO", "REDLY", "REEDY", "REHAB", "REINS", "RELIC", "REMIT", "RENTS", "RESTS", "RETRO", "RHINO", "RIDGE", "RIFLE", "RIGHT", "RIGID", "RIGOR", "RILED", "RINGS", "RINSE", "RIOTS", "RISEN", "RISES", "RISKS", "RITZY", "RIVAL", "RIVEN", "RIVET", "ROBOT", "ROILY", "ROLLS", "ROMAN", "ROOFS", "ROOMS", "ROOTS", "ROSIN", "ROTOR", "ROUGE", "ROUGH", "ROUTE", "ROYAL", "RUDDY", "RUGBY", "RUINS", "RULED", "RUMBA", "RUMMY", "RUMOR", "RUNIC", "RUNNY", "RUNTY", "SABLE", "SADLY", "SAFER", "SAGGY", "SAILS", "SAINT", "SALAD", "SALES", "SALLY", "SALON", "SALSA", "SALTS", "SALTY", "SALVE", "SAMBA", "SANDS", "SANDY", "SATED", "SATIN", "SATYR", "SAUCE", "SAUCY", "SAUDI", "SAUNA", "SAVED", "SAVER", "SAVOR", "SAVVY", "SAXON", "SCALD", "SCALE", "SCALP", "SCALY", "SCAMP", "SCANT", "SCAPE", "SCARE", "SCARF", "SCARP", "SCARS", "SCARY", "SCENE", "SCENT", "SCHMO", "SCOFF", "SCOLD", "SCONE", "SCOOP", "SCOOT", "SCOPE", "SCORE", "SCORN", "SCOTS", "SCOUR", "SCOUT", "SCRAM", "SCRAP", "SCREE", "SCREW", "SCRIM", "SCRIP", "SCRUB", "SCRUM", "SCUBA", "SCULL", "SEALS", "SEAMS", "SEAMY", "SEATS", "SEDAN", "SEEDS", "SEEDY", "SEEMS", "SEGUE", "SEIZE", "SELLS", "SENDS", "SENSE", "SETUP", "SEXES", "SHADE", "SHADY", "SHAFT", "SHAKE", "SHALE", "SHALL", "SHAME", "SHANK", "SHAPE", "SHARD", "SHARE", "SHARP", "SHAVE", "SHAWL", "SHEAF", "SHEAR", "SHEEN", "SHEET", "SHELF", "SHELL", "SHIFT", "SHILL", "SHINE", "SHINY", "SHIPS", "SHIRE", "SHIRT", "SHONE", "SHOOT", "SHOPS", "SHORE", "SHORN", "SHORT", "SHOTS", "SHOUT", "SHOVE", "SHUNT", "SHUSH", "SIDES", "SIDLE", "SIEGE", "SIGHT", "SIGIL", "SIGNS", "SILLY", "SILTY", "SINCE", "SINEW", "SINGE", "SINGS", "SINUS", "SITAR", "SITES", "SIXTH", "SIXTY", "SIZED", "SIZES", "SKALD", "SKANK", "SKATE", "SKEIN", "SKIER", "SKIES", "SKIFF", "SKILL", "SKIMP", "SKINS", "SKIRT", "SKULL", "SLABS", "SLAIN", "SLAKE", "SLANG", "SLANT", "SLASH", "SLATE", "SLEEP", "SLEET", "SLICE", "SLIDE", "SLIME", "SLIMY", "SLING", "SLINK", "SLOPE", "SLOSH", "SLOTH", "SLOTS", "SLUMP", "SLUSH", "SLYLY", "SMALL", "SMART", "SMASH", "SMEAR", "SMELL", "SMELT", "SMILE", "SMITE", "SNAIL", "SNAKE", "SNARE", "SNARL", "SNEER", "SNIDE", "SNIFF", "SNIPE", "SNOOP", "SNORE", "SNORT", "SNOUT", "SOFTY", "SOGGY", "SOILS", "SOLAR", "SOLID", "SOLVE", "SONAR", "SONGS", "SONIC", "SOOTH", "SORRY", "SORTS", "SOUGH", "SOULS", "SOUTH", "STAFF", "STAGE", "STAIN", "STAIR", "STAKE", "STALE", "STALL", "STAMP", "STAND", "START", "STASH", "STATE", "STEAD", "STEAL", "STEAM", "STEEL", "STEEP", "STEER", "STEMS", "STENO", "STIFF", "STILE", "STILL", "STILT", "STING", "STINK", "STINT", "STOIC", "STOLE", "STOMP", "STONE", "STONY", "STOOL", "STOOP", "STOPS", "STORE", "STORK", "STORM", "STORY", "STOUT", "STOVE", "STRAP", "STRAW", "STREP", "STREW", "STRIP", "STRUM", "STRUT", "STUCK", "STUDY", "STUMP", "STUNT", "SUAVE", "SUEDE", "SUITE", "SUITS", "SULLY", "SUNNY", "SUNUP", "SUSHI", "SWALE", "SWAMI", "SWAMP", "SWANK", "SWANS", "SWARD", "SWARM", "SWASH", "SWATH", "SWAZI", "SWEAR", "SWEAT", "SWELL", "SWIFT", "SWILL", "SWINE", "SWING", "SWIPE", "SWIRL", "SWISH", "SWISS", "SWOON", "SWOOP", "SWORD", "SWORE", "SWORN", "SWUNG", "TACIT", "TAFFY", "TAILS", "TAINT", "TAKEN", "TALLY", "TALON", "TAMED", "TAMIL", "TANGO", "TANGY", "TARDY", "TAROT", "TARRY", "TASKS", "TATTY", "TAUNT", "TAWNY", "TAXES", "TAXON", "TEACH", "TEAMS", "TEARS", "TEARY", "TEASE", "TECHY", "TEDDY", "TEENS", "TEENY", "TEETH", "TELLS", "TELLY", "TENOR", "TENSE", "TENTH", "TENTS", "TEXAS", "THANK", "THEIR", "THEME", "THESE", "THETA", "THIGH", "THINE", "THING", "THINK", "THIRD", "THONG", "THORN", "THOSE", "THUMB", "TIARA", "TIBIA", "TIDAL", "TIGHT", "TILDE", "TILED", "TILES", "TILTH", "TIMED", "TIMES", "TIMID", "TINES", "TINNY", "TIPSY", "TIRED", "TITLE", "TOMMY", "TONAL", "TONED", "TONGS", "TONIC", "TONNE", "TOOLS", "TOONS", "TOOTH", "TOPIC", "TOPSY", "TOQUE", "TORCH", "TORSO", "TORTE", "TORUS", "TOTAL", "TOTEM", "TOUCH", "TOXIC", "TOXIN", "TRACT", "TRAIL", "TRAIN", "TRAIT", "TRAMS", "TRAWL", "TREAD", "TREAT", "TRIAD", "TRIAL", "TRIED", "TRIKE", "TRILL", "TRITE", "TROLL", "TROOP", "TROUT", "TRUCE", "TRUCK", "TRULY", "TRUST", "TRUTH", "TUBBY", "TULIP", "TUMMY", "TUNED", "TUNIC", "TUTEE", "TUTOR", "TWANG", "TWEAK", "TWINS", "TWIRL", "TWIST", "TYING", "UDDER", "ULCER", "ULNAR", "ULTRA", "UMBRA", "UNCAP", "UNCLE", "UNCUT", "UNDER", "UNDUE", "UNFED", "UNFIT", "UNHIP", "UNIFY", "UNION", "UNITE", "UNITS", "UNITY", "UNLIT", "UNMET", "UNSAY", "UNTIE", "UNTIL", "UNZIP", "USAGE", "USHER", "USING", "USUAL", "UTTER", "UVULA", "VAGUE", "VALET", "VALID", "VALOR", "VALUE", "VAPOR", "VAULT", "VAUNT", "VEDIC", "VEINS", "VEINY", "VENAL", "VENOM", "VICAR", "VIEWS", "VIGIL", "VIGOR", "VILLA", "VINES", "VINYL", "VIRAL", "VIRUS", "VISIT", "VISOR", "VITAL", "VIVID", "VIXEN", "VOGUE", "VOTED", "VOUCH", "VROOM", "WAGON", "WAIST", "WAITS", "WAIVE", "WALKS", "WALLS", "WALTZ", "WANTS", "WARDS", "WARES", "WARNS", "WASTE", "WATCH", "WAVED", "WAVES", "WAXEN", "WEARS", "WEARY", "WEAVE", "WEBBY", "WELLS", "WETLY", "WHALE", "WHEAT", "WHEEL", "WHICH", "WHILE", "WHINE", "WHITE", "WHORL", "WHOSE", "WIDTH", "WIELD", "WILLS", "WIMPY", "WINCE", "WINCH", "WINDS", "WINES", "WINGS", "WITCH", "WITTY", "WIVES", "WOMAN", "WORDS", "WORKS", "WORLD", "WORMS", "WORMY", "WORSE", "WORST", "WORTH", "WOULD", "WOUND", "XENON", "YARDS", "YAWNS", "YEARN", "YEARS", "YOUNG", "YOUTH", "YUCCA", "YUMMY", "ZILCH", "ZINGY", "ZONAL", "ZONES" };

        public static void TranspositionExperiment()
        {
            //0 = /
            //1 = cw
            //2 = ccw
            //3 = \

            bool[] bitsFromWord(string word) => Enumerable.Range(0, 5).SelectMany(ltr => Enumerable.Range(0, 5).Select(bit => ((word[ltr] - 'A' + 1) & (1 << (4 - bit))) != 0)).ToArray();
            string wordFromBits(bool[] bits) => Enumerable.Range(0, 5).Select(row => Enumerable.Range(0, 5).Select(bit => bits[bit + 5 * row]).Aggregate(0, (p, n) => p * 2 + (n ? 1 : 0))).Select(i => (char) ('A' + i - 1)).JoinString();
            bool[] transpose(bool[] bits, int mode) => Enumerable.Range(0, 25).Select(ix => (ix / 5).Apply(row => (ix % 5).Apply(col => bits[((mode & 1) != 0 ? row : 4 - row) + 5 * ((mode & 2) != 0 ? col : 4 - col)]))).ToArray();
            int reverseMode(int mode) => (mode >> 1) | ((mode & 1) << 1);

            List<ConsoleColoredString> outputBits(bool[] bits) => Enumerable.Range(0, 5)
                .Select(row => Enumerable.Range(0, 5).Select(bit => bits[bit + 5 * row] ? "██".Color(ConsoleColor.Green) : "░░".Color(ConsoleColor.DarkBlue)).JoinColoredString())
                .ToList();

            var words = _words.ToHashSet();
            var times = new List<double>();
            var rnd = new Random(247);

            var solWordIx = rnd.Next(0, _words.Length);
            var solWord = _words[solWordIx];
            var solRawBits = bitsFromWord(solWord);

            var start = DateTime.UtcNow;
            var reiterate = 0;
            while (true)
            {
                var xor1WordIx = rnd.Next(0, _words.Length);
                if (xor1WordIx == solWordIx)
                    continue;
                var xor1Word = _words[xor1WordIx];
                var xor1RawBits = bitsFromWord(xor1Word);

                var xor2ofs = rnd.Next(0, _words.Length);
                for (var xor2WordIxR = 0; xor2WordIxR < 1000; xor2WordIxR++)
                {
                    var xor2WordIx = (xor2WordIxR + xor2ofs) % _words.Length;
                    if (xor2WordIx == xor1WordIx || xor2WordIx == solWordIx)
                        continue;
                    var xor2Word = _words[xor2WordIx];
                    var xor2RawBits = bitsFromWord(xor2Word);
                    for (var xor1Mode = 0; xor1Mode < 4; xor1Mode++)
                    {
                        var xor1Bits = transpose(xor1RawBits, xor1Mode);
                        for (var xor2Mode = 0; xor2Mode < 4; xor2Mode++)
                        {
                            var xor2Bits = transpose(xor2RawBits, xor2Mode);
                            var xor3Bits = xor1Bits.Zip(xor2Bits, (a, b) => a ^ b).Zip(solRawBits, (a, b) => a ^ b).ToArray();
                            for (var xor3Mode = 0; xor3Mode < 4; xor3Mode++)
                            {
                                var xor3RawBits = transpose(xor3Bits, reverseMode(xor3Mode));
                                var xor3Word = wordFromBits(xor3RawBits);
                                if (xor3Word != xor1Word && xor3Word != xor2Word && xor3Word != solWord && words.Contains(xor3Word))
                                {
                                    var time = (DateTime.UtcNow - start).TotalSeconds;
                                    times.Add(time);
                                    var mean = times.Average();
                                    Console.WriteLine($"{xor1Word}/{xor1Mode} ^ {xor2Word}/{xor2Mode} ^ {xor3Word}/{xor3Mode} = ?");
                                    Console.ReadLine();
                                    Console.WriteLine($"{solWord} (took {time:0.0} sec; mean={mean:0.0} sec; std={Math.Sqrt(times.Sum(t => Math.Pow(t - mean, 2))) / Math.Sqrt(times.Count)}");
                                    ConsoleUtil.WriteLine(
                                        outputBits(xor1RawBits)
                                            .Zip(outputBits(xor1Bits), (p, n) => p + " → " + n)
                                            .Zip(outputBits(xor2RawBits), (p, n) => p + "   ^   " + n)
                                            .Zip(outputBits(xor2Bits), (p, n) => p + " → " + n)
                                            .Zip(outputBits(xor3RawBits), (p, n) => p + "   ^   " + n)
                                            .Zip(outputBits(xor3Bits), (p, n) => p + " → " + n)
                                            .Zip(outputBits(solRawBits), (p, n) => p + "   =   " + n)
                                            .JoinColoredString("\n"));
                                    Console.WriteLine();
                                    Console.ReadLine();
                                    return;
                                }
                            }
                        }
                    }
                }
                Console.Write($"{solWord} reiterate {++reiterate}\r");
            }
        }
    }
}