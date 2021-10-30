using System;
using System.CodeDom;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Xml.Linq;
using BlueButtonLib;
using KtaneStuff.Modeling;
using RT.KitchenSink;
using RT.Serialization;
using RT.Util;
using RT.Util.Consoles;
using RT.Util.ExtensionMethods;
using RT.Util.Geometry;
using RT.Util.Text;
using Words;

namespace KtaneStuff
{
    static class NavyButton
    {
        private static int _sz => NavyButtonPuzzle._sz;

        private static readonly HashSet<string> _words4 = "ABLE,ACHE,ACID,ACNE,ACRE,AFRO,AGED,AIDE,AKIN,ALAS,ALLY,ALSO,AMEN,AMID,APEX,APPS,AQUA,ARAB,ARCH,AREA,ARID,ARMY,ASKS,ATOM,AUKS,AUNT,AURA,AWAY,AXES,AXIS,AXLE,BABY,BACK,BAIL,BAIT,BAKE,BALD,BALL,BAND,BANG,BANK,BARE,BARK,BARN,BASE,BASS,BATH,BATS,BEAD,BEAM,BEAN,BEAR,BEAT,BEEF,BEEN,BEER,BELL,BELT,BEND,BENT,BERG,BEST,BIAS,BIKE,BILE,BILL,BIND,BIRD,BITE,BLAH,BLED,BLEW,BLOC,BLOG,BLOT,BLOW,BLUE,BLUR,BOAR,BOAT,BODY,BOIL,BOLD,BOLT,BOMB,BOND,BONE,BONY,BOOK,BOOM,BOOT,BORE,BORN,BOSS,BOTH,BOUT,BOWL,BRED,BREW,BRIG,BRIT,BROW,BULB,BULK,BULL,BUMP,BURN,BURY,BUSH,BUST,BUSY,BUTT,BUZZ,CAFE,CAGE,CAKE,CALF,CALL,CALM,CAME,CAMP,CANE,CAPE,CAPO,CARD,CARE,CARP,CART,CASE,CASH,CAST,CAVE,CEIL,CELL,CENT,CHAI,CHAO,CHAP,CHAT,CHEF,CHIA,CHIN,CHIP,CHIS,CHOP,CIEL,CITY,CLAD,CLAM,CLAN,CLAP,CLAW,CLAY,CLIP,CLOG,CLUB,CLUE,COAL,COAT,COCK,COCO,CODE,COIL,COIN,COLD,COLE,COMB,COME,CONE,COOK,COOL,COPE,COPS,COPY,CORD,CORE,CORK,CORN,COST,COSY,COUP,COVE,COZY,CRAB,CREE,CREW,CRIB,CROP,CROW,CUBE,CULT,CURB,CURE,CURL,CUTE,DAFT,DAMP,DARE,DARK,DART,DASH,DATA,DATE,DAWN,DAYS,DEAD,DEAF,DEAL,DEAR,DEBT,DECK,DEED,DEEP,DEER,DENT,DENY,DESK,DIAL,DICE,DIET,DINE,DIRE,DIRT,DISC,DISH,DISK,DIVE,DOCK,DOLE,DOLL,DOME,DONE,DOOR,DORM,DORY,DOSE,DOVE,DOWN,DRAG,DRAW,DREW,DREY,DRIB,DRIP,DROP,DRUG,DRUM,DUAL,DUCK,DUEL,DUET,DULL,DULY,DUMB,DUMP,DUSK,DUST,DUTY,DYER,EACH,EARN,EARS,EASE,EAST,EASY,EATS,ECHO,EDGE,EDIT,ELSE,ENVY,EPIC,ERIC,EURO,EVEN,EVER,EVIL,EXAM,EXIT,EYED,EYES,FACE,FACT,FADE,FAIL,FAIR,FAKE,FALL,FAME,FANG,FARE,FARM,FAST,FATE,FAVE,FAWN,FEAR,FEAT,FEED,FEEL,FEET,FELL,FELT,FILE,FILL,FILM,FIND,FINE,FIRE,FIRM,FIRS,FISH,FIST,FIVE,FLAG,FLAP,FLAT,FLAW,FLAX,FLEA,FLED,FLEE,FLEW,FLEX,FLIP,FLOW,FLUX,FOAL,FOAM,FOIL,FOLD,FOLK,FOND,FONT,FOOD,FOOL,FOOT,FORA,FORD,FORK,FORM,FORT,FOUL,FOUR,FREE,FROG,FROM,FUEL,FULL,FUND,FURY,FUSE,FUSS,GAIN,GALA,GALL,GAME,GANG,GAOL,GASP,GATE,GAVE,GAZE,GEAR,GENE,GERM,GIFT,GILL,GILT,GIRL,GIVE,GLAD,GLEE,GLEN,GLIB,GLOW,GLUE,GOAL,GOAT,GOES,GOLD,GOLF,GONE,GONG,GOOD,GORY,GOSH,GOWN,GRAB,GRAM,GRAY,GREW,GREY,GRID,GRIM,GRIN,GRIP,GRIT,GROW,GUMP,GUST,GYRE,GYRO,HAHA,HAIL,HAIR,HALF,HALL,HALO,HALT,HAND,HANG,HARD,HARE,HARM,HATE,HAUL,HAVE,HAWK,HAZE,HEAD,HEAL,HEAP,HEAR,HEAT,HECK,HEEL,HEIR,HELD,HELL,HELP,HERB,HERD,HERE,HERO,HERS,HIDE,HIGH,HIKE,HILL,HINT,HIRE,HOLD,HOLE,HOLY,HOME,HOOD,HOOK,HOPE,HORN,HOSE,HOST,HOUR,HOWL,HUGE,HULL,HUNG,HUNT,HURT,HUSH,HYMN,HYPE,ICED,ICER,ICON,IDEA,IDLE,IDLY,IDOL,IDYL,INCH,INFO,INTO,IRON,ITCH,ITEM,JACK,JAIL,JARS,JAZZ,JINX,JOBS,JOIN,JOKE,JOSH,JUMP,JUNK,JURY,JUST,KEEN,KEEP,KELP,KEPT,KICK,KILL,KIND,KING,KISS,KITE,KIWI,KNEE,KNEW,KNIT,KNOB,KNOT,KNOW,LACE,LACK,LADY,LAID,LAIR,LAKE,LAMB,LAMP,LAND,LANE,LAST,LATE,LAVA,LAWN,LAZE,LAZY,LEAD,LEAF,LEAK,LEAN,LEAP,LEER,LEFT,LEND,LENS,LENT,LESS,LEST,LEVO,LEVY,LIAR,LICE,LIED,LIFE,LIFT,LIKE,LILO,LIMB,LIME,LINE,LINK,LINT,LION,LIPS,LIRE,LISP,LIST,LIVE,LOAD,LOAF,LOAN,LOCI,LOCK,LOCO,LOFT,LOGO,LONE,LONG,LOOK,LOOP,LOPS,LORD,LORE,LOSE,LOSS,LOST,LOTS,LOUD,LOUP,LOVE,LUCK,LUMP,LUNG,LURE,LUSH,LUST,MADE,MAID,MAIL,MAIN,MAKE,MALE,MALI,MALL,MALT,MANY,MARE,MARK,MASK,MASS,MAST,MATE,MATH,MATT,MAZE,MEAL,MEAN,MEAT,MEET,MELD,MELT,MEMO,MENU,MERE,MESH,MESS,MICE,MILD,MILE,MILK,MILL,MIME,MIND,MINE,MINI,MINT,MISS,MIST,MOAT,MOCK,MODE,MOLD,MOLE,MOLT,MONK,MOOD,MOON,MOOR,MORE,MOSS,MOST,MOTH,MOVE,MUCH,MULE,MUST,MUTE,MYTH,NAIL,NAME,NAVE,NEAR,NEAT,NECK,NEED,NEON,NERD,NEST,NEWS,NEWT,NEXT,NICE,NICK,NINE,NODE,NONE,NOOK,NOON,NORM,NOSE,NOTE,NOUN,NUMB,NUTS,OATH,OATS,OBEY,OBOE,ODDS,ODOR,OGRE,OILY,OINK,OKAY,OMEN,OMIT,ONCE,ONLY,ONTO,OOZE,OPAL,OPEN,ORAL,ORCA,ORES,ORGY,OUCH,OURS,OVAL,OVEN,OVER,OWLY,PACE,PACK,PACT,PAGE,PAID,PAIL,PAIN,PAIR,PALE,PALM,PANG,PANT,PAPA,PAPS,PARA,PARE,PARK,PART,PASS,PAST,PATH,PAVE,PAWN,PEAK,PEAL,PEAR,PEAT,PECK,PECS,PEEK,PEEL,PEER,PERK,PEST,PICA,PICK,PICS,PIER,PILE,PILL,PINE,PINK,PINT,PIPE,PITY,PLAN,PLAY,PLEA,PLOT,PLOW,PLOY,PLUG,PLUM,PLUS,POEM,POET,POKE,POLE,POLK,POLL,POLO,POND,PONY,POOL,POOR,PORK,PORT,POSE,POSH,POST,POUR,PRAY,PREY,PROP,PROS,PUCE,PULL,PUMP,PUNK,PUNT,PUPA,PUPS,PURE,PUSH,QUAY,QUID,QUIT,QUIZ,RACE,RACK,RAFT,RAGE,RAID,RAIL,RAIN,RAKE,RAMP,RANK,RARE,RASH,RATE,RAZE,READ,REAL,REAP,REAR,REEF,REEL,RELY,REND,RENT,REPS,REST,RHEA,RHOS,RICE,RICH,RIDE,RIFT,RILE,RIND,RING,RIOT,RIPE,RIPS,RISE,RISK,RITE,ROAD,ROAM,ROAR,ROBE,ROCK,RODE,ROLE,ROLL,ROOF,ROOM,ROOT,ROPE,ROSE,ROSY,ROVE,RUBY,RUDE,RUIN,RULE,RUNG,RUSH,RUST,SACK,SAFE,SAGA,SAID,SAIL,SAKE,SALE,SALT,SAME,SAND,SANE,SANG,SANK,SASS,SAVE,SCAN,SCAR,SCUM,SEAL,SEAM,SEAT,SEED,SEEK,SEEM,SEEN,SELF,SELL,SEND,SENT,SERF,SEXY,SHAH,SHED,SHIP,SHOE,SHOP,SHOT,SHOW,SHUT,SICK,SIDE,SIGH,SIGN,SILK,SING,SINK,SITE,SIZE,SKAS,SKIN,SKIP,SKUA,SLAB,SLAM,SLID,SLIM,SLIP,SLOP,SLOT,SLOW,SLUM,SMUG,SNAP,SNOW,SOAP,SOAR,SODA,SOFA,SOFT,SOIL,SOLD,SOLE,SOLO,SOME,SONG,SOON,SORE,SORT,SOUL,SOUP,SOUR,SPAN,SPEC,SPIN,SPOT,SPUN,SPUR,STAB,STAR,STAY,STEM,STEP,STIR,STOP,SUCH,SUIT,SUNG,SUNK,SURE,SUSS,SWAN,SWAP,SWIM,TACK,TACO,TAIL,TAKE,TALE,TALK,TALL,TAME,TANK,TAPE,TASK,TAUT,TAXI,TEAL,TEAM,TEAR,TELL,TEND,TENT,TERM,TERN,TEST,TEXT,THAI,THAN,THAT,THAW,THEE,THEM,THEN,THEY,THIN,THIS,THOU,THUD,THUS,TIDE,TIDY,TIED,TIER,TILE,TILL,TILT,TIME,TINY,TIRE,TOAD,TOIL,TOLD,TOLL,TOMB,TONE,TOOK,TOOL,TORE,TORN,TORT,TORY,TOSS,TOUR,TOWN,TRAM,TRAP,TRAY,TREE,TREY,TRIM,TRIO,TRIP,TRON,TROY,TRUE,TSAR,TUBE,TUCK,TUNA,TUNE,TURF,TURN,TWIN,TYPE,TYRE,UGLY,UNDO,UNIT,UNTO,UPON,URGE,USED,USER,USES,VAIN,VARY,VASE,VAST,VEER,VEIL,VEIN,VEND,VENT,VERB,VERY,VEST,VETO,VIAL,VICE,VIEW,VILE,VINE,VIOL,VISA,VIVO,VOID,VOLE,VOTE,WADE,WAGE,WAIT,WAKE,WALK,WALL,WAND,WANT,WARD,WARM,WARN,WARP,WARY,WASH,WAVE,WAVY,WAXY,WEAK,WEAR,WEED,WEEK,WELD,WELL,WENT,WERE,WEST,WHAM,WHAT,WHEN,WHIP,WHOM,WIDE,WIFE,WILD,WILL,WILY,WIND,WINE,WING,WIPE,WIRE,WISE,WISH,WITH,WOLF,WOMB,WOOD,WOOL,WORD,WORE,WORK,WORM,WORN,WRAP,WREN,WRIT,XYLO,YARD,YARN,YAWN,YEAH,YEAR,YELL,YOGA,YOUR,YOWL,ZAPS,ZEAL,ZERO,ZINC,ZONE,ZOOM"
            .Split(',').ToHashSet();

        public static void GenerateLatinSquaresAndStencils()
        {
            var allLatinSquares = FindSolutions(new int?[NavyButtonPuzzle._sz * NavyButtonPuzzle._sz], null, null).ToList();
            Console.WriteLine(allLatinSquares.Count);

            (int sm, int la, bool big)[] generateConstraints(int[] grid)
            {
                var upConstraints = Enumerable.Range(0, _sz * _sz).Where(ix => ix / _sz > 0 && grid[ix - _sz] < grid[ix]).Select(ix => (sm: ix - _sz, la: ix, false));
                var rightConstraints = Enumerable.Range(0, _sz * _sz).Where(ix => ix % _sz < _sz - 1 && grid[ix + 1] < grid[ix]).Select(ix => (sm: ix + 1, la: ix, false));
                var downConstraints = Enumerable.Range(0, _sz * _sz).Where(ix => ix / _sz < _sz - 1 && grid[ix + _sz] < grid[ix]).Select(ix => (sm: ix + _sz, la: ix, false));
                var leftConstraints = Enumerable.Range(0, _sz * _sz).Where(ix => ix % _sz > 0 && grid[ix - 1] < grid[ix]).Select(ix => (sm: ix - 1, la: ix, false));

                var upConstraints2 = Enumerable.Range(0, _sz * _sz).Where(ix => ix / _sz > 0 && grid[ix - _sz] < grid[ix] - 1).Select(ix => (sm: ix - _sz, la: ix, true));
                var rightConstraints2 = Enumerable.Range(0, _sz * _sz).Where(ix => ix % _sz < _sz - 1 && grid[ix + 1] < grid[ix] - 1).Select(ix => (sm: ix + 1, la: ix, true));
                var downConstraints2 = Enumerable.Range(0, _sz * _sz).Where(ix => ix / _sz < _sz - 1 && grid[ix + _sz] < grid[ix] - 1).Select(ix => (sm: ix + _sz, la: ix, true));
                var leftConstraints2 = Enumerable.Range(0, _sz * _sz).Where(ix => ix % _sz > 0 && grid[ix - 1] < grid[ix] - 1).Select(ix => (sm: ix - 1, la: ix, true));

                return upConstraints.Concat(rightConstraints).Concat(downConstraints).Concat(leftConstraints)
                    .Concat(upConstraints2).Concat(rightConstraints2).Concat(downConstraints2).Concat(leftConstraints2)
                    .ToArray();
            }

            bool evaluateConstraint(int[] grid, (int sm, int la, bool big) constraint) => constraint.big ? grid[constraint.sm] < grid[constraint.la] - 1 : grid[constraint.sm] < grid[constraint.la];

            allLatinSquares.RemoveAll(lsq =>
            {
                var constraints = generateConstraints(lsq);
                var num = allLatinSquares.Count(lsq2 => constraints.All(constr => evaluateConstraint(lsq2, constr)));
                return num > 1;
            });
            Console.WriteLine(allLatinSquares.Count);

            Utils.ReplaceInFile(@"D:\c\KTANE\BunchOfButtons\Lib\NavyButtonPuzzle.cs", "/*LSQstart*/", "/*LSQend*/",
                $@"""{allLatinSquares.Select(ia => ia.JoinString()).JoinString(",")}""");
            Utils.ReplaceInFile(@"D:\c\KTANE\BunchOfButtons\Lib\NavyButtonPuzzle.cs", "/*stencils-start*/", "/*stencils-end*/",
                FindRequiredStencilsForSeed(2, allLatinSquares, _words4).Select(ta => $"new[] {{ {ta.Select(tup => $"({tup.dx}, {tup.dy})").JoinString(", ")} }}").JoinString(", "));
        }

        private static IEnumerable<int[]> FindSolutions(int?[] sofar, (int sm, int la)[] constraints, Random rnd)
        {
            var ix = -1;
            int[] best = null;
            for (var i = 0; i < sofar.Length; i++)
            {
                var x = i % _sz;
                var y = i / _sz;
                if (sofar[i] != null)
                    continue;
                var taken = new bool[_sz];
                // Same row
                for (var c = 0; c < _sz; c++)
                    if (sofar[c + _sz * y] is int v)
                        taken[v] = true;
                // Same column
                for (var r = 0; r < _sz; r++)
                    if (sofar[x + _sz * r] is int v)
                        taken[v] = true;
                // Constraints
                if (constraints != null)
                {
                    foreach (var (sm, la) in constraints)
                    {
                        if (i == sm && sofar[la] != null) // i is the cell with the smaller value, so it can’t be anything larger than la
                            for (var ov = sofar[la].Value; ov < _sz; ov++)
                                taken[ov] = true;
                        else if (i == la && sofar[sm] != null)  // i is the cell with the larger value, so it can’t be anything smaller than sm
                            for (var ov = sofar[sm].Value; ov >= 0; ov--)
                                taken[ov] = true;
                    }
                }
                var values = taken.SelectIndexWhere(b => !b).ToArray();
                if (values.Length == 0)
                    yield break;
                if (best == null || values.Length < best.Length)
                {
                    ix = i;
                    best = values;
                    if (values.Length == 1)
                        goto shortcut;
                }
            }

            if (ix == -1)
            {
                yield return sofar.Select(i => i.Value).ToArray();
                yield break;
            }

            shortcut:
            var offset = rnd == null ? 0 : rnd.Next(0, best.Length);
            for (var i = 0; i < best.Length; i++)
            {
                var value = best[(i + offset) % best.Length];
                sofar[ix] = value;
                foreach (var solution in FindSolutions(sofar, constraints, rnd))
                    yield return solution;
            }
            sofar[ix] = null;
        }

        public static void GeneratePuzzle()
        {
            var puzzle = NavyButtonPuzzle.GeneratePuzzle(1);
            Console.WriteLine(puzzle.GreekLetterIxs.Select(ix => "ΑΒΓΔΕΖΗΘΙΚΛΜΝΞΟΠΡΣΤΥΦΧΨΩαβγδεζηθικλμνξοπρστυφχψω"[ix]).JoinString());
            var tt = new TextTable { ColumnSpacing = 1 };
            for (var i = 0; i < 7 * 7; i++)
                tt.SetCell(i % 7, i / 7, " ");
            for (var i = 0; i < 16; i++)
                tt.SetCell(2 * (i % 4), 2 * (i / 4), i == puzzle.GivenIndex ? puzzle.GivenValue.ToString() : "·");

            var offset = Rnd.Next(0, puzzle.StencilIxs.Length);
            for (var stIx = 0; stIx < puzzle.StencilIxs.Length; stIx++)
            {
                var stencilIx = puzzle.StencilIxs[(stIx + offset) % puzzle.StencilIxs.Length];
                var stencil = NavyButtonData.Stencils[stencilIx];
                Console.WriteLine(Enumerable.Range(0, 3).Select(row => Enumerable.Range(0, 3).Select(col => col == 1 && row == 1 ? "░░" : stencil.Contains((col - 1, row - 1)) ? "██" : "  ").JoinString()).JoinString("\n"));
                Console.WriteLine();
            }
            //foreach (var (sm, la) in puzzle.Constraints)
            //{
            //    var x = sm % 4 + la % 4;
            //    var y = sm / 4 + la / 4;
            //    tt.SetCell(x, y, sm % 4 < la % 4 ? "<" : sm % 4 > la % 4 ? ">" : sm / 4 < la / 4 ? "∧" : "∨");
            //}
            tt.WriteToConsole();
            Console.WriteLine(puzzle.Answer);
        }

        public static void Stencil_FindPossibleWords()
        {
            Stencil_FindPossibleWords(FindSolutions(new int?[NavyButtonPuzzle._sz * NavyButtonPuzzle._sz], null, null).ToArray());
        }

        private static void Stencil_FindPossibleWords(int[][] latinSquares)
        {
            int numBits(int n) => n == 0 ? 0 : 1 + numBits(n & (n - 1));
            var directions = Enumerable.Range(0, 9).Select(i => (dx: i % 3 - 1, dy: i / 3 - 1)).Where(tup => tup.dx != 0 || tup.dy != 0).ToArray();
            var allStencils = Enumerable.Range(0, 1 << 8).Where(n => numBits(n) == 3).Select(bits => Enumerable.Range(0, 8).Where(bit => (bits & (1 << bit)) != 0).Select(bit => directions[bit]).ToArray()).ToArray();

            var validWords = new HashSet<string>();
            for (var lsqIx = 0; lsqIx < latinSquares.Length; lsqIx++)
            {
                Console.WriteLine($"LSq {lsqIx}/{latinSquares.Length}");
                var latinSquare = latinSquares[lsqIx];
                var threes = latinSquare.SelectIndexWhere(val => val == 3);
                var letters = threes.Select(ix => allStencils.Select(stencil =>
                {
                    var x = ix % 4;
                    var y = ix / 4;
                    var values = stencil.Select(tup => latinSquare[(x + tup.dx + 4) % 4 + 4 * ((y + tup.dy + 4) % 4)]);
                    return values.Contains(3) ? null : (int?) values.Aggregate(0, (p, n) => 3 * p + n);
                }).Where(i => i != null && i != 0).Select(i => (char) ('A' + i.Value - 1)).Distinct().Order().JoinString()).ToArray();

                validWords.AddRange(
                    from a in letters[0]
                    from b in letters[1]
                    from c in letters[2]
                    from d in letters[3]
                    select $"{a}{b}{c}{d}");
            }

            var invalidWords = (from a in Enumerable.Range('A', 26)
                                from b in Enumerable.Range('A', 26)
                                from c in Enumerable.Range('A', 26)
                                from d in Enumerable.Range('A', 26)
                                select $"{(char) a}{(char) b}{(char) c}{(char) d}").Except(validWords).ToArray();
            Console.WriteLine(invalidWords.JoinString("\n"));
            //var wordlist = new Data().allWords[0].ToHashSet();
            Console.WriteLine("---");
            Console.WriteLine(invalidWords.Where(iv => _words4.Contains(iv)).JoinString("\n"));
        }

        private static void Stencil_ReduceRequired(int[][] latinSquares)
        {
            var currentBest = int.MaxValue;
            var lockObj = new object();

            Console.Clear();
            Enumerable.Range(0, 1000).ParallelForEach(Environment.ProcessorCount, (seed, proc) =>
            {
                lock (lockObj)
                {
                    Console.CursorLeft = 0;
                    Console.CursorTop = proc;
                    Console.WriteLine($"Proc {proc}: seed {seed}");
                }
                var requiredStencils = FindRequiredStencilsForSeed(seed, latinSquares, _words4);

                lock (lockObj)
                {
                    var req = requiredStencils.Count();
                    if (req < currentBest)
                    {
                        Console.CursorLeft = 0;
                        Console.CursorTop = Environment.ProcessorCount + 2;
                        Console.WriteLine($"Seed {seed} requires only {req}  ");
                        currentBest = req;
                    }
                }
            });
        }

        public static void List21Stencils()
        {
            var allLatinSquares = FindSolutions(new int?[NavyButtonPuzzle._sz * NavyButtonPuzzle._sz], null, null).ToArray();
            var stencils = FindRequiredStencilsForSeed(2, allLatinSquares, _words4).OrderBy(tup => tup[0].dy).ThenBy(tup => tup[0].dx).ToArray();
            foreach (var stencil in stencils)
            {
                Console.WriteLine(Enumerable.Range(0, 3).Select(row => Enumerable.Range(0, 3).Select(col => col == 1 && row == 1 ? "░░" : stencil.Contains((col - 1, row - 1)) ? "██" : "  ").JoinString()).JoinString("\n"));
                Console.WriteLine();
            }
            Console.WriteLine(stencils.Length);
        }

        private static (int dx, int dy)[][] FindRequiredStencilsForSeed(int seed, IEnumerable<int[]> allLatinSquares, HashSet<string> allWords)
        {
            int numBits(int n) => n == 0 ? 0 : 1 + numBits(n & (n - 1));
            var directions = Enumerable.Range(0, 9).Select(i => (dx: i % 3 - 1, dy: i / 3 - 1)).Where(tup => tup.dx != 0 || tup.dy != 0).ToArray();
            var stencils = Enumerable.Range(0, 1 << 8).Where(n => /*!new[] { 69, 49, 162, 140 }.Contains(n) &&*/ numBits(n) == 3).Select(bits => Enumerable.Range(0, 8).Where(bit => (bits & (1 << bit)) != 0).Select(bit => directions[bit]).ToArray()).ToArray();
            var rnd = new Random(seed);

            var requiredStencils = Ut.ReduceRequiredSet(Enumerable.Range(0, stencils.Length).ToArray().Shuffle(rnd), test: state =>
            {
                var words = allWords.ToHashSet();
                var validWords = new HashSet<string>();
                foreach (var latinSquare in allLatinSquares)
                {
                    var threes = latinSquare.SelectIndexWhere(val => val == 3);
                    var letters = threes.Select(ix => state.SetToTest.Select(stencilIx =>
                    {
                        var x = ix % 4;
                        var y = ix / 4;
                        var values = stencils[stencilIx].Select(tup => latinSquare[(x + tup.dx + 4) % 4 + 4 * ((y + tup.dy + 4) % 4)]);
                        return values.Contains(3) ? null : (int?) values.Aggregate(0, (p, n) => 3 * p + n);
                    }).Where(i => i != null && i != 0).Select(i => (char) ('A' + i.Value - 1)).Distinct().Order().JoinString()).ToArray();

                    foreach (var word in words.ToArray())
                        if (Enumerable.Range(0, 4).All(ix => letters[ix].Contains(word[ix])))
                        {
                            validWords.Add(word);
                            words.Remove(word);
                        }
                }
                return validWords.Count == allWords.Count;
            });
            return requiredStencils.Select(req => stencils[req]).ToArray();
        }

        public static void LatinSquareUniquenessExperiment()
        {
            var groupedByClues = new Dictionary<int, List<int[]>>();
            foreach (var latinSq in FindSolutions(new int?[_sz * _sz], null, null))
            {
                int val(int v1, int v2) => v1 < v2 ? 1 : v1 > v2 ? 0 : throw new InvalidOperationException();
                var constr1 = Enumerable.Range(0, _sz * _sz).Where(ix => ix / _sz > 0).Select(ix => val(latinSq[ix - _sz], latinSq[ix])).Aggregate(0, (p, n) => p * 2 + n);
                var constr2 = Enumerable.Range(0, _sz * _sz).Where(ix => ix % _sz > 0).Select(ix => val(latinSq[ix - 1], latinSq[ix])).Aggregate(constr1, (p, n) => p * 2 + n);
                groupedByClues.AddSafe(constr2, latinSq);
            }
            var validLatinSquares = new List<int[]>();
            foreach (var gr in groupedByClues)
            {
                foreach (var latinSquare in gr.Value)
                {
                    // Latin Square is valid if it can be made unique with one given
                    if (Enumerable.Range(0, _sz * _sz).Any(ix => gr.Value.Count(lsq => lsq[ix] == latinSquare[ix]) == 1))
                        validLatinSquares.Add(latinSquare);
                }
            }
            Console.WriteLine(validLatinSquares.Count);
            Console.WriteLine(FindSolutions(new int?[_sz * _sz], null, null).ToArray().Length);
            var stencils = FindRequiredStencilsForSeed(143, validLatinSquares, _words4);
            foreach (var stencil in stencils)
            {
                Console.WriteLine(Enumerable.Range(0, 3).Select(row => Enumerable.Range(0, 3).Select(col => col == 1 && row == 1 ? "░░" : stencil.Contains((col - 1, row - 1)) ? "██" : "  ").JoinString()).JoinString("\n"));
                Console.WriteLine();
            }
            Console.WriteLine(stencils.Length);
            Console.WriteLine("--");
            foreach (var latinSquare in validLatinSquares)
                Console.WriteLine($"{latinSquare.JoinString("")}");
        }

        public static void GatherData()
        {
            var stencils = Ut.NewArray<(int dx, int dy)[]>(
                new[] { (1, -1), (1, 0), (-1, 1) },
                new[] { (0, -1), (1, 0), (0, 1) },
                new[] { (0, -1), (1, 0), (-1, 1) },
                new[] { (-1, -1), (-1, 0), (0, 1) },
                new[] { (1, 0), (-1, 1), (1, 1) },
                new[] { (1, -1), (-1, 0), (-1, 1) },
                new[] { (0, -1), (1, -1), (0, 1) },
                new[] { (1, 0), (-1, 1), (0, 1) },
                new[] { (1, -1), (-1, 0), (0, 1) },
                new[] { (-1, -1), (-1, 0), (1, 1) },
                new[] { (-1, -1), (-1, 0), (-1, 1) },
                new[] { (0, -1), (1, -1), (-1, 0) },
                new[] { (1, -1), (0, 1), (1, 1) },
                new[] { (-1, 0), (1, 0), (0, 1) },
                new[] { (1, -1), (-1, 1), (0, 1) },
                new[] { (0, -1), (-1, 0), (1, 0) },
                new[] { (-1, -1), (1, 0), (0, 1) },
                new[] { (-1, -1), (0, -1), (1, 0) },
                new[] { (0, -1), (-1, 0), (1, 1) },
                new[] { (0, -1), (1, 0), (1, 1) },
                new[] { (0, -1), (-1, 0), (0, 1) });

            var latinSquares = "0123130220313210,0123130232102031,0123230132101032,0123103223013210,0123103223103201,0123103232012310,0123103232102301,0123203132101302,0123123023013012,0123123030122301,0123203113023210,0123230110323210,0123230112303012,0123230130121230,0123231010323201,0123231032011032,0123301212302301,0123301223011230,0123320110322310,0123320123101032,0123321010322301,0123321013022031,0123321020311302,0123321023011032,0132120323103021,0132120330212310,0132102323013210,0132102323103201,0231102323103102,1230201331020321,0132102332102301,0132132020133201,0231132020133102,0132201313203201,0132230110233210,0132230132101023,0132231010233201,0231231010233102,0132231012033021,0132231030211203,0132302112032310,0132302123101203,0132320110232310,0132320113202013,0132320120131320,0132320123101023,0132321010232301,0132321023011023,0213103223013120,0213103231202301,0213130220313120,0213130221303021,0213130230212130,0213130231202031,0213230131201032,0213132020313102,0213132031022031,0213230110323120,2301312002131032,0213203113203102,0213203131021320,0213310213202031,0213310220311320,0213312010322301,2301312010320213,0213312023011032,0231130220133120,0231130231202013,0231132021033012,0231132030122103,0231210330121320,1230210330120321,0231201313023120,0231201331201302,0231301221031320,1230301221030321,0231310210232310,0231310220131320,1230310220130321,0231310223101023,0231312013022013,0231312020131302,0312102321303201,0312102332012130,0312120320313120,0312120321303021,0312120330212130,0312120331202031,0312123021033021,0312123030212103,0312210312303021,0312210330211230,0312213010233201,0312213032011023,0312320110232130,0312320121301023,0312302112302103,0312302121031230,0321103221033210,0321103232102103,0321120321303012,0321120330122130,0321123020133102,0321123021033012,0321123030122103,0321123031022013,0321201331021230,1320201331020231,0321210310323210,2310310202311023,0321210330121230,1320210330120231,0321210332101032,0321213012033012,0321213030121203,0321310220131230,0321301212032130,0321301221031230,1320301221030231,0321301221301203,0321321010322103,0321321021031032,1023031221303201,1023031232012130,1023013223013210,3012023113202103,1023013232012310,2013013232011320,1023013232102301,1023023131022310,1023213003123201,1023213032010312,1023230101323210,1023230132100132,1023231001323201,1023231002313102,1023231031020231,1023231032010132,1023310202312310,2013310202311320,1023320101322310,2013320101321320,1023320103122130,1023320121300312,1023321001322301,1023321023010132,1032021323013120,1032021331202301,1032012323013210,2031012313023210,1032012323103201,1032012332012310,1032012332102301,2031012332101302,1032032121033210,1032032132102103,1032210303213210,1032210332100321,1032230101233210,1032230102133120,1032230131200213,1032230132100123,1032231001233201,1032231032010123,1032312002132301,2031312002131302,1032312023010213,2031312013020213,1032320101232310,1032320123100123,1032321001232301,2031321001231302,1032321003212103,1032321021030321,1032321023010123,2031321013020123,1203013223103021,1203013230212310,1203031220313120,1203031221303021,1203031230212130,1203031231202031,1203032121303012,1203032130122130,1203231001323021,1203231030210132,1203213003213012,1203213030120321,1203301203212130,1203301221300321,1203302101322310,1203302123100132,1230012323013012,1230012330122301,1230031221033021,1230031230212103,1230032120133102,1230032121033012,1230032130122103,1230032131022013,1230210303123021,1230210330210312,1230230101233012,1230230130120123,1230301201232301,1230301223010123,1230302103122103,1230302121030312,1302012320313210,2301012310323210,1302012332102031,2301012332101032,1302021320313120,2301021310323120,1302021321303021,1302021330212130,1302021331202031,2301021331201032,1302023120133120,1302023131202013,1302201302313120,1302201331200231,1302203101233210,1302203132100123,1302321001232031,2301321001231032,1302321020310123,2301321010320123,1302312002312013,1302312020130231,1320013232012013,2310013232011023,1320021320313102,1320021331022031,1320023121033012,1320023130122103,1320023131022013,2310023131021023,1320201301323201,1320201332010132,1320203102133102,1320203131020213,1320310202132031,1320310220310213,1320320101322013,2310320101321023,2013032112303102,3012032112302103,2013023113023120,2013023131201302,2013123003213102,3012123003212103,2013130202313120,2013130231200231,2013132001323201,2013132002313102,3012132002312103,2013132032010132,2013310203211230,2013310212300321,2013312002311302,2013312013020231,2031021313203102,2031021331021320,2031130201233210,3120230102131032,2031130232100123,2031132002133102,2031132031020213,2031310202131320,2031310213200213,2031312003121203,2031312012030312,2103023113203012,3102023113202013,2103031212303021,2103031230211230,2103032110323210,2103032112303012,3102032112302013,3201132020130132,2103032132101032,2103132002313012,2103103203213210,2103103232100321,2103123003123021,2103123003213012,3102123003212013,2103123030210312,2103301202311320,2103301203211230,2103301212300321,2103301213200231,2103302103121230,2103302112300312,2103321003211032,2103321010320321,2130031210233201,2130031232011023,2130032112033012,2130032130121203,2130120303213012,2130120330120321,2130102303123201,2130102332010312,2130301203211203,2130301212030321,2130302102131302,2130302103121203,2130302112030312,2130302113020213,2130320103121023,2130320110230312,2301012312303012,2301012330121230,2301013210233210,2301013232101023,2301102301323210,2301102332100132,2301103201233210,2301103202133120,2301103231200213,2301103232100123,2301123001233012,2301123030120123,2301301201231230,2301301212300123,2301321001321023,2301321010230132,2310012310323201,2310012332011032,2310013212033021,2310013230211203,2310102301323201,2310102302313102,2310102331020231,2310102332010132,2310103201233201,2310103232010123,2310120301323021,2310120330210132,2310320101231032,2310320110320123,2310302101321203,2310302112030132,3012012312302301,3012012323011230,3012032112032130,3012032121301203,3012120303212130,3012120321300321,3012123001232301,3012123023010123,3012210302311320,3012210303211230,3012210312300321,3012210313200231,3012213003211203,3012213012030321,3012230101231230,3012230112300123,3021031212302103,3021031221031230,3021013212032310,3021013223101203,3021120301322310,3021120323100132,3021123003122103,3021123021030312,3021210303121230,3021210312300312,3021213002131302,3021213003121203,3021213012030312,3021213013020213,3021231001321203,3021231012030132,3102021313202031,3102021320311320,3102023110232310,3102023123101023,3102102323100231,3201102323100132,3102132002132031,3102132020310213,3102201303211230,3102201312300321,3102201313200231,3201201313200132,3102203102131320,3102203113200213,3102231010230231,3201231010230132,3120021310322301,3120021323011032,3120023113022013,3120023120131302,3120130202312013,3120130220130231,3120103202132301,3120203102131302,3120103223010213,3120201302311302,3120201313020231,3120203103121203,3120203112030312,3120203113020213,3120230110320213,3201012310322310,3201012323101032,3201013210232310,3201013213202013,3201013220131320,3201013223101023,3201031210232130,3201031221301023,3201102303122130,3201102321300312,3201103201232310,3201103223100123,3201231001231032,3201231010320123,3201213003121023,3201213010230312,3210012310322301,3210012313022031,3210012320311302,3210012323011032,3210013210232301,3210013223011023,3210032110322103,3210032121031032,3210102301322301,3210102323010132,3210103201232301,3210203101231302,3210103203212103,3210103221030321,3210103223010123,3210130201232031,3210230101231032,3210130220310123,3210210303211032,3210210310320321,3210230101321023,3210230110230132,3210230110320123,3210203113020123"
                .Split(',').Select(str => str.Select(ch => ch - '0').ToArray()).ToArray();

            var dic = new Dictionary<string, List<(string lsq, string stencilIxs)>>();
            Enumerable.Range(0, latinSquares.Length).ParallelForEach(Environment.ProcessorCount, lsqIx =>
            {
                lock (dic)
                    Console.WriteLine($"{lsqIx}/{latinSquares.Length}");
                var lsq = latinSquares[lsqIx];
                var threes = lsq.SelectIndexWhere(v => v == 3).Select(coord => new Coord(_sz, _sz, coord)).ToArray();
                IEnumerable<int[]> findStencilCombinations(int[] sofar, int ix)
                {
                    if (ix == _sz)
                    {
                        yield return sofar.ToArray();
                        yield break;
                    }

                    for (var stIx = 0; stIx < stencils.Length; stIx++)
                    {
                        if (stencils[stIx].Any(tup => lsq[threes[ix].AddWrap(tup.dx, tup.dy).Index] == 3))
                            continue;
                        if (stencils[stIx].All(tup => lsq[threes[ix].AddWrap(tup.dx, tup.dy).Index] == 0))
                            continue;
                        sofar[ix] = stIx;
                        foreach (var result in findStencilCombinations(sofar, ix + 1))
                            yield return result;
                    }
                }
                foreach (var comb in findStencilCombinations(new int[_sz], 0))
                {
                    var word = comb.Select((stIx, ix) => (char) (stencils[stIx].Aggregate(0, (p, n) => p * 3 + lsq[threes[ix].AddWrap(n.dx, n.dy).Index]) + 'A' - 1)).JoinString();
                    var addedExtraStencil = stencils.IndexOf(extraSt => "1234,2340,3401,4012".Split(',')
                            .Select(str => str.Select(ch => ch - '0').Select(ix => ix == 4 ? extraSt : stencils[comb[ix]]))
                            .All(stencils => stencils.Any((stencil, thrIx) => stencil.Any(tup => lsq[threes[thrIx].AddWrap(tup.dx, tup.dy).Index] == 3))));
                    if (addedExtraStencil == -1)
                        continue;
                    if (_words4.Contains(word))
                        lock (dic)
                            dic.AddSafe(word, (lsq.JoinString(), comb.Select(ix => (char) ('A' + ix)).JoinString()));
                }
            });
            Console.WriteLine($"Total data: {dic.Sum(kvp => kvp.Value.Count)}");
            Utils.ReplaceInFile(@"D:\c\KTANE\BunchOfButtons\Lib\NavyButtonData.cs", "/*data-start*/", "/*data-end*/", @"""" + dic
                .OrderBy(kvp => kvp.Key)
                .Select(kvp => $"{kvp.Key}={kvp.Value.GroupBy(inf => inf.lsq).Select(gr => $"{gr.Key}>{gr.Select(v => v.stencilIxs).JoinString("|")}").JoinString(",")}")
                .JoinString(";") + @"""");
            Utils.ReplaceInFile(@"D:\c\KTANE\BunchOfButtons\Lib\NavyButtonData.cs", "/*stencils-start*/", "/*stencils-end*/",
               stencils.Select(ta => $"new[] {{ {ta.Select(tup => $"({tup.dx}, {tup.dy})").JoinString(", ")} }}").JoinString(", "));
        }

        public static void GenerateSvg()
        {
            File.WriteAllText(@"D:\temp\temp.svg", $"<svg font-family='Avrile Serif' text-anchor='start'>{"ΑαΒβΓγΔδΕεΖζΗηΘθΙιΚκΛλΜμΝνΞξΟοΠπΡρΣσΤτΥυΦφΧχΨψΩω".Select(ch => $"<text id='{ch}'>{ch}</text>").JoinString()}</svg>");
        }

        public static void GenerateShapeModels()
        {
            File.WriteAllText(@"D:\c\KTANE\BunchOfButtons\Assets\Modules\Navy\Assets\Pyramid.obj", Md.GenerateObjFile(LooseModels.Cone(-.5, .5, .5, 4), "Pyramid", AutoNormal.FlatOverride));
            File.WriteAllText(@"D:\c\KTANE\BunchOfButtons\Assets\Modules\Navy\Assets\Torus.obj", Md.GenerateObjFile(LooseModels.Torus(.4, .2 * .4 / .5, 36), "Torus"));
            File.WriteAllText(@"D:\c\KTANE\BunchOfButtons\Assets\Modules\Navy\Assets\Cylinder.obj", Md.GenerateObjFile(LooseModels.Cylinder(-.5, .5, .4, 36), "Cylinder"));
            File.WriteAllText(@"D:\c\KTANE\BunchOfButtons\Assets\Modules\Navy\Assets\Cone.obj", Md.GenerateObjFile(LooseModels.Cone(-.5, .5, .4, 36), "Cone"));
            File.WriteAllText(@"D:\c\KTANE\BunchOfButtons\Assets\Modules\Navy\Assets\Prism.obj", Md.GenerateObjFile(LooseModels.Cylinder(-.5, .5, .5, 3), "Prism", AutoNormal.FlatOverride));
        }

        public static void GenerateGreekLetterModels()
        {
            var alphabet = "ΑαΒβΓγΔδΕεΖζΗηΘθΙιΚκΛλΜμΝνΞξΟοΠπΡρΣσΤτΥυΦφΧχΨψΩω".ToHashSet();
            var svg = XDocument.Parse(File.ReadAllText(@"D:\temp\temp.svg"));
            var widths = new double[48];
            foreach (var group in svg.Root.ElementsI("g"))
            {
                var id = group.AttributeI("id").Value.Single();
                if (!alphabet.Remove(id))
                    System.Diagnostics.Debugger.Break();
                var pathD = group.ElementsI("path").Single().AttributeI("d").Value;
                var name = $"{id}{(id < 'α' ? "u" : "c")}";
                var model = DecodeSvgPath.DecodePieces(pathD).Extrude(2, .01, true);
                widths["ΑΒΓΔΕΖΗΘΙΚΛΜΝΞΟΠΡΣΤΥΦΧΨΩαβγδεζηθικλμνξοπρστυφχψω".IndexOf(id)] = model.Max(face => face.Max(v => v.Location.X)) - model.Min(face => face.Min(v => v.Location.X));
                File.WriteAllText($@"D:\c\KTANE\BunchOfButtons\Assets\Modules\Navy\Assets\{name}.obj", Md.GenerateObjFile(model, name));
            }
            if (alphabet.Any())
                System.Diagnostics.Debugger.Break();
            Utils.ReplaceInFile(@"D:\c\KTANE\BunchOfButtons\Assets\Modules\Navy\NavyButtonScript.cs", "/*widths-start*/", "/*widths-end*/", $@"new[] {{ {widths.Select(w => $"{w}f").JoinString(", ")} }}");
        }

        public static void GenerateManualGraphics()
        {
            var shapeNames = new[] { "Sphere", "Cube", "Cone", "Prism", "Cylinder", "Pyramid", "Torus" };
            Utils.ReplaceInFile(@"D:\c\KTANE\Public\HTML\The Navy Button.html", "<!--%%-->", "<!--%%%-->",
                Enumerable.Range(0, 7).Select(row => $@"<tr><th><img src='img/The Navy Button/{shapeNames[row]}.png' /></th>{Enumerable.Range(0, 3).Select(col =>
                    $"<td><svg viewBox='0 0 3 3' fill='none' stroke='black' stroke-width='.05'><path d='M1 1h1v1H1z' />{NavyButtonData.Stencils[col + 3 * row].Select(tup => $"<circle cx='{tup.dx + 1.5}' cy='{tup.dy + 1.5}' r='.4' />").JoinString()}</svg></td>").JoinString()}</tr>").JoinString());
        }
    }
}