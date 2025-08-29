using RT.Util;
using RT.Util.Consoles;
using RT.Util.ExtensionMethods;

namespace KtaneStuff;

internal static class LiteralMaze
{
    private static readonly string[] _wordlist = [
        "ABLE", "ACHE", "ACID", "ACNE", "ACRE", "AFAR", "AFRO", "AGED", "AIDE", "AKIN", "ALAS", "ALLY", "ALSO", "AMEN", "AMID", "APEX", "AQUA", "ARCH", "AREA", "ARID", "ARMY", "ATOM", "AUNT", "AURA", "AWAY", "AXES", "AXIS", "AXLE",
        "BABY", "BACK", "BAIL", "BAIT", "BAKE", "BALD", "BALL", "BAND", "BANG", "BANK", "BARE", "BARK", "BARN", "BASE", "BASS", "BATH", "BATS", "BEAD", "BEAM", "BEAN", "BEAR", "BEAT", "BEEF", "BEEN", "BEER", "BELL", "BELT", "BEND", "BENT", "BEST", "BIAS", "BIKE", "BILE", "BILL", "BIND", "BIRD", "BITE", "BLAH", "BLEW", "BLOW", "BLUE", "BLUR", "BOAR", "BOAT", "BODY", "BOIL", "BOLD", "BOLT", "BOMB", "BOND", "BONE", "BONY", "BOOK", "BOOM", "BOOT", "BORE", "BORN", "BOSS", "BOTH", "BOUT", "BOWL", "BROW", "BULB", "BULK", "BULL", "BUMP", "BURN", "BURY", "BUSH", "BUST", "BUSY", "BUTT", "BUZZ",
        "CAFE", "CAGE", "CAKE", "CALF", "CALL", "CALM", "CAME", "CAMP", "CANE", "CAPE", "CARD", "CARE", "CARP", "CART", "CASE", "CASH", "CAST", "CAVE", "CELL", "CHAP", "CHAT", "CHEF", "CHIN", "CHIP", "CHOP", "CITY", "CLAD", "CLAM", "CLAN", "CLAP", "CLAW", "CLAY", "CLIP", "CLOG", "CLUB", "CLUE", "COAL", "COAT", "COCK", "CODE", "COIL", "COIN", "COLD", "COMB", "COME", "CONE", "COOK", "COOL", "COPE", "COPY", "CORD", "CORE", "CORK", "CORN", "COST", "COSY", "COUP", "COZY", "CRAB", "CREW", "CRIB", "CROP", "CROW", "CUBE", "CULT", "CURB", "CURE", "CURL", "CUTE",
        "DAFT", "DAMP", "DARE", "DARK", "DART", "DASH", "DATA", "DATE", "DAWN", "DAYS", "DEAD", "DEAF", "DEAL", "DEAR", "DEBT", "DECK", "DEED", "DEEP", "DEER", "DENT", "DENY", "DESK", "DIAL", "DICE", "DIET", "DINE", "DIRE", "DIRT", "DISC", "DISH", "DISK", "DIVE", "DOCK", "DOLE", "DOLL", "DOME", "DONE", "DOOR", "DOSE", "DOVE", "DOWN", "DRAG", "DRAW", "DREW", "DRIP", "DROP", "DRUG", "DRUM", "DUAL", "DUCK", "DUEL", "DUET", "DULL", "DULY", "DUMB", "DUMP", "DUSK", "DUST", "DUTY",
        "EACH", "EARN", "EARS", "EASE", "EAST", "EASY", "EATS", "ECHO", "EDGE", "EDIT", "ELSE", "ENVY", "EPIC", "EURO", "EVEN", "EVER", "EVIL", "EXAM", "EXIT", "EYED", "EYES",
        "FACE", "FACT", "FADE", "FAIL", "FAIR", "FAKE", "FALL", "FAME", "FARE", "FARM", "FAST", "FATE", "FEAR", "FEAT", "FEED", "FEEL", "FEET", "FELL", "FELT", "FILE", "FILL", "FILM", "FIND", "FINE", "FIRE", "FIRM", "FISH", "FIST", "FIVE", "FLAG", "FLAP", "FLAT", "FLAW", "FLED", "FLEE", "FLEW", "FLEX", "FLIP", "FLOW", "FLUX", "FOAM", "FOIL", "FOLD", "FOLK", "FOND", "FONT", "FOOD", "FOOL", "FOOT", "FORD", "FORK", "FORM", "FORT", "FOUL", "FOUR", "FREE", "FROG", "FROM", "FUEL", "FULL", "FUND", "FURY", "FUSE", "FUSS",
        "GAIN", "GALA", "GALL", "GAME", "GANG", "GASP", "GATE", "GAVE", "GAZE", "GEAR", "GENE", "GERM", "GIFT", "GILL", "GILT", "GIRL", "GIVE", "GLAD", "GLEE", "GLOW", "GLUE", "GOAL", "GOAT", "GOES", "GOLD", "GOLF", "GONE", "GONG", "GOOD", "GOSH", "GOWN", "GRAB", "GRAM", "GRAY", "GREW", "GREY", "GRID", "GRIM", "GRIN", "GRIP", "GRIT", "GROW", "GUST",
        "HAIL", "HAIR", "HALF", "HALL", "HALT", "HAND", "HANG", "HARD", "HARE", "HARM", "HATE", "HAUL", "HAVE", "HAWK", "HAZE", "HEAD", "HEAL", "HEAP", "HEAR", "HEAT", "HEEL", "HEIR", "HELD", "HELL", "HELP", "HERB", "HERD", "HERE", "HERO", "HERS", "HIDE", "HIGH", "HIKE", "HILL", "HINT", "HIRE", "HOLD", "HOLE", "HOLY", "HOME", "HOOD", "HOOK", "HOPE", "HORN", "HOSE", "HOST", "HOUR", "HOWL", "HUGE", "HULL", "HUNG", "HUNT", "HURT", "HUSH", "HYMN", "HYPE",
        "ICED", "ICON", "IDEA", "IDLE", "IDOL", "INCH", "INFO", "INTO", "IRON", "ITCH", "ITEM",
        "JACK", "JAIL", "JARS", "JAZZ", "JINX", "JOBS", "JOIN", "JOKE", "JUMP", "JUNK", "JURY", "JUST",
        "KEEN", "KEEP", "KELP", "KEPT", "KICK", "KILL", "KIND", "KING", "KISS", "KITE", "KIWI", "KNEE", "KNEW", "KNIT", "KNOB", "KNOT", "KNOW",
        "LACE", "LACK", "LADY", "LAID", "LAIR", "LAKE", "LAMB", "LAMP", "LAND", "LANE", "LAST", "LATE", "LAVA", "LAWN", "LAZY", "LEAD", "LEAF", "LEAK", "LEAN", "LEAP", "LEFT", "LEND", "LENS", "LESS", "LEST", "LEVY", "LIAR", "LIED", "LIFE", "LIFT", "LIKE", "LIMB", "LIME", "LINE", "LINK", "LION", "LIST", "LIVE", "LOAD", "LOAF", "LOAN", "LOCK", "LOFT", "LOGO", "LONE", "LONG", "LOOK", "LOOP", "LORD", "LOSE", "LOSS", "LOST", "LOTS", "LOUD", "LOVE", "LUCK", "LUMP", "LUNG", "LURE", "LUSH", "LUST",
        "MADE", "MAID", "MAIL", "MAIN", "MAKE", "MALE", "MALL", "MALT", "MANY", "MARE", "MARK", "MASK", "MASS", "MAST", "MATE", "MATH", "MAZE", "MEAL", "MEAN", "MEAT", "MEET", "MELT", "MEMO", "MENU", "MERE", "MESH", "MESS", "MICE", "MILD", "MILE", "MILK", "MILL", "MIME", "MIND", "MINE", "MINI", "MINT", "MISS", "MIST", "MOAT", "MOCK", "MODE", "MOLD", "MOLE", "MONK", "MOOD", "MOON", "MOOR", "MORE", "MOSS", "MOST", "MOTH", "MOVE", "MUCH", "MULE", "MUST", "MUTE", "MYTH",
        "NAIL", "NAME", "NAVE", "NEAR", "NEAT", "NECK", "NEED", "NEON", "NEST", "NEWS", "NEWT", "NEXT", "NICE", "NICK", "NINE", "NODE", "NONE", "NOOK", "NOON", "NORM", "NOSE", "NOTE", "NOUN", "NUMB", "NUTS",
        "OATH", "OATS", "OBEY", "OBOE", "ODDS", "ODOR", "OGRE", "OILY", "OINK", "OKAY", "OMEN", "OMIT", "ONCE", "ONLY", "ONTO", "OOZE", "OPEN", "ORAL", "ORCA", "ORES", "OURS", "OVAL", "OVEN", "OVER",
        "PACE", "PACK", "PACT", "PAGE", "PAID", "PAIN", "PAIR", "PALE", "PALM", "PARK", "PART", "PASS", "PAST", "PATH", "PAWN", "PEAK", "PEAR", "PEAT", "PEEK", "PEEL", "PEER", "PERK", "PEST", "PICK", "PIER", "PILE", "PILL", "PINE", "PINK", "PINT", "PIPE", "PITY", "PLAN", "PLAY", "PLEA", "PLOT", "PLOW", "PLOY", "PLUG", "PLUM", "PLUS", "POEM", "POET", "POKE", "POLE", "POLL", "POLO", "POND", "PONY", "POOL", "POOR", "PORK", "PORT", "POSE", "POSH", "POST", "POUR", "PRAY", "PREY", "PROP", "PULL", "PUMP", "PUNK", "PURE", "PUSH",
        "QUAY", "QUID", "QUIT", "QUIZ",
        "RACE", "RACK", "RAFT", "RAGE", "RAID", "RAIL", "RAIN", "RAKE", "RAMP", "RANK", "RARE", "RASH", "RATE", "READ", "REAL", "REAR", "REEF", "RELY", "RENT", "REST", "RICE", "RICH", "RIDE", "RIFT", "RING", "RIOT", "RIPE", "RISE", "RISK", "RITE", "ROAD", "ROAM", "ROAR", "ROBE", "ROCK", "RODE", "ROLE", "ROLL", "ROOF", "ROOM", "ROOT", "ROPE", "ROSE", "ROSY", "RUBY", "RUDE", "RUIN", "RULE", "RUNG", "RUSH", "RUST",
        "SACK", "SAFE", "SAGA", "SAID", "SAIL", "SAKE", "SALE", "SALT", "SAME", "SAND", "SANE", "SANG", "SANK", "SAVE", "SCAN", "SCAR", "SCUM", "SEAL", "SEAM", "SEAT", "SEED", "SEEK", "SEEM", "SEEN", "SELF", "SELL", "SEND", "SENT", "SEXY", "SHED", "SHIP", "SHOE", "SHOP", "SHOT", "SHOW", "SHUT", "SICK", "SIDE", "SIGH", "SIGN", "SILK", "SING", "SINK", "SITE", "SIZE", "SKIN", "SKIP", "SLAB", "SLAM", "SLID", "SLIM", "SLIP", "SLOT", "SLOW", "SLUM", "SMUG", "SNAP", "SNOW", "SOAP", "SOAR", "SODA", "SOFA", "SOFT", "SOIL", "SOLD", "SOLE", "SOLO", "SOME", "SONG", "SOON", "SORE", "SORT", "SOUL", "SOUP", "SOUR", "SPAN", "SPIN", "SPOT", "SPUN", "SPUR", "STAB", "STAR", "STAY", "STEM", "STEP", "STIR", "STOP", "SUCH", "SUIT", "SUNG", "SUNK", "SURE", "SWAN", "SWAP", "SWIM",
        "TACK", "TACO", "TAIL", "TAKE", "TALE", "TALK", "TALL", "TAME", "TANK", "TAPE", "TASK", "TAUT", "TAXI", "TEAL", "TEAM", "TEAR", "TELL", "TEND", "TENT", "TERM", "TEST", "TEXT", "THAN", "THAT", "THAW", "THEE", "THEM", "THEN", "THEY", "THIN", "THIS", "THOU", "THUD", "THUS", "TIDE", "TIDY", "TIED", "TIER", "TILE", "TILL", "TILT", "TIME", "TINY", "TIRE", "TOAD", "TOIL", "TOLD", "TOLL", "TOMB", "TONE", "TOOK", "TOOL", "TORE", "TORN", "TORT", "TORY", "TOSS", "TOUR", "TOWN", "TRAM", "TRAP", "TRAY", "TREE", "TRIM", "TRIO", "TRIP", "TRUE", "TSAR", "TUBE", "TUCK", "TUNA", "TUNE", "TURF", "TURN", "TWIN", "TYPE",
        "UGLY", "UNDO", "UNIT", "UNTO", "UPON", "URGE", "USED", "USER", "USES",
        "VAIN", "VARY", "VASE", "VAST", "VEIL", "VEIN", "VENT", "VERB", "VERY", "VEST", "VETO", "VIAL", "VIEW", "VILE", "VINE", "VISA", "VOID", "VOTE",
        "WADE", "WAGE", "WAIT", "WAKE", "WALK", "WALL", "WAND", "WANT", "WARD", "WARM", "WARN", "WARP", "WARY", "WASH", "WAVE", "WAVY", "WAXY", "WEAK", "WEAR", "WEED", "WEEK", "WELD", "WELL", "WENT", "WERE", "WEST", "WHAT", "WHEN", "WHIP", "WHOM", "WIDE", "WIFE", "WILD", "WILL", "WIND", "WINE", "WING", "WIPE", "WIRE", "WISE", "WISH", "WITH", "WOLF", "WOMB", "WOOD", "WOOL", "WORD", "WORE", "WORK", "WORM", "WORN", "WRAP", "WRIT",
        "XYLO",
        "YARD", "YARN", "YAWN", "YEAH", "YEAR", "YELL", "YOGA", "YOUR",
        "ZEAL", "ZERO", "ZINC", "ZONE", "ZOOM"
    ];

    public class MazeGenerator(int size, Random rnd)
    {
        private bool[][] _visited;
        private char[] _charArr;

        public string GenerateMaze()
        {
            _visited = new bool[size][];
            for (var i = 0; i < _visited.Length; i++)
                _visited[i] = new bool[size];
            _charArr = new char[(size * 2 + 1) * (size * 2 + 1)].Select(i => '█').ToArray();
            for (var a = 0; a < size; a++)
                for (var b = 0; b < size; b++)
                    _charArr[(a * (size * 2 + 1) * 2) + (b * 2) + size * 2 + 2] = ' ';
            var x = rnd.Next(0, size);
            var y = rnd.Next(0, size);
            generate(x, y);
            return _charArr.JoinString();
        }

        private static readonly int[] _cellPositions = [10, 12, 14, 16, 28, 30, 32, 34, 46, 48, 50, 52, 64, 66, 68, 70];
        private static readonly int[] _wallVectors = [-9, 1, 9, -1];

        public static string GenerateEncodedMaze(Random rnd)
        {
            var mazeGenerator = new MazeGenerator(4, rnd);
            tryagain:
            var mazeString = mazeGenerator.GenerateMaze();
            int deadEnds = 0, attempts = 0;
            var cellWalls = new List<string>();
            var distinctWalls = new List<string>();
            var letters = "";
            for (var p = 0; p < 16; p++)
            {
                var theseWalls = "";
                for (var v = 0; v < 4; v++)
                {
                    if (mazeString[_cellPositions[p] + _wallVectors[v]].ToString() == "█")
                    {
                        theseWalls += "NESW"[v];
                    }
                }
                if (theseWalls.Length == 3) { deadEnds++; }
                cellWalls.Add(theseWalls);
                if (!distinctWalls.Any(a => a == theseWalls))
                {
                    distinctWalls.Add(theseWalls);
                    letters += (char) ('a' + distinctWalls.Count - 1);
                }
                else
                {
                    letters += (char) ('a' + distinctWalls.IndexOf(theseWalls));
                }
            }
            if (deadEnds == 2)
            {
                attempts++;
                cellWalls.Clear();
                distinctWalls.Clear();
                goto tryagain;
            }
            return letters;
        }

        private void generate(int x, int y)
        {
            _visited[x][y] = true;
            var arr = Enumerable.Range(0, 4).ToArray().Shuffle(rnd);
            var curPos = (x * (size * 2 + 1) * 2) + (y * 2) + (size * 2 + 2);
            for (var i = 0; i < 4; i++)
            {
                if (arr[i] == 0)
                    if (y != 0 && !_visited[x][y - 1])
                    {
                        _charArr[curPos - 1] = ' ';
                        generate(x, y - 1);
                    }
                if (arr[i] == 1)
                    if (x != size - 1 && !_visited[x + 1][y])
                    {
                        _charArr[curPos + (size * 2 + 1)] = ' ';
                        generate(x + 1, y);
                    }
                if (arr[i] == 2)
                    if (y != size - 1 && !_visited[x][y + 1])
                    {
                        _charArr[curPos + 1] = ' ';
                        generate(x, y + 1);
                    }
                if (arr[i] == 3)
                    if (x != 0 && !_visited[x - 1][y])
                    {
                        _charArr[curPos - (size * 2 + 1)] = ' ';
                        generate(x - 1, y);
                    }
            }
        }
    }

    private const int _w = 4;
    private const int _h = 4;

    private abstract class deduction
    {
        public abstract (int letter, int dir, bool wall)? Deduce(bool?[][] known, string maze);
        public abstract ConsoleColor Color { get; }
    }

    private class borderAtSidesDeduction : deduction
    {
        public override ConsoleColor Color => ConsoleColor.Cyan;
        public override (int letter, int dir, bool wall)? Deduce(bool?[][] known, string maze)
        {
            // Top and bottom edges
            for (var x = 0; x < _w; x++)
            {
                if (known[maze[x] - 'a'][0] == null)
                    return (maze[x] - 'a', 0, true);
                if (known[maze[x + _w * (_h - 1)] - 'a'][2] == null)
                    return (maze[x + _w * (_h - 1)] - 'a', 2, true);
            }
            // Left and right edges
            for (var y = 0; y < _h; y++)
            {
                if (known[maze[y * _w] - 'a'][3] == null)
                    return (maze[y * _w] - 'a', 3, true);
                if (known[maze[y * _w + _w - 1] - 'a'][1] == null)
                    return (maze[y * _w + _w - 1] - 'a', 1, true);
            }
            return null;
        }
    }

    private class abuttingBordersDeduction : deduction
    {
        public override ConsoleColor Color => ConsoleColor.Magenta;
        public override (int letter, int dir, bool wall)? Deduce(bool?[][] known, string maze)
        {
            // Left/right neighbors
            for (var i = 0; i < maze.Length; i++)
                if (i % _w != 0)
                {
                    var leftLtr = maze[i - 1] - 'a';
                    var rightLtr = maze[i] - 'a';
                    if (known[leftLtr][1] != null && known[rightLtr][3] == null)
                        return (rightLtr, 3, known[leftLtr][1].Value);
                    if (known[leftLtr][1] == null && known[rightLtr][3] != null)
                        return (leftLtr, 1, known[rightLtr][3].Value);
                }
            // Above/below neighbors
            for (var i = _w; i < maze.Length; i++)
            {
                var topLtr = maze[i - _w] - 'a';
                var bottomLtr = maze[i] - 'a';
                if (known[topLtr][2] != null && known[bottomLtr][0] == null)
                    return (bottomLtr, 0, known[topLtr][2].Value);
                if (known[topLtr][2] == null && known[bottomLtr][0] != null)
                    return (topLtr, 2, known[bottomLtr][0].Value);
            }
            return null;
        }
    }

    private class avoidDuplicatesDeduction : deduction
    {
        public override ConsoleColor Color => ConsoleColor.Yellow;
        public override (int letter, int dir, bool wall)? Deduce(bool?[][] known, string maze)
        {
            for (var i = 0; i < known.Length; i++)
                if (known[i].All(b => b != null))
                    for (var j = 0; j < known.Length; j++)
                        if (j != i && known[j].Count(b => b == null) == 1 && Enumerable.Range(0, 4).All(dir => known[j][dir] == null || known[j][dir].Value == known[i][dir].Value))
                        {
                            var dir = known[j].IndexOf(b => b == null);
                            return (j, dir, !known[i][dir].Value);
                        }
            return null;
        }
    }

    private class avoidEnclosingARegionDeduction : deduction
    {
        public override ConsoleColor Color => ConsoleColor.Green;
        public static int FindReachable(bool?[][] known, string maze, int startingPosition)
        {
            var visited = new HashSet<int> { startingPosition };

            IEnumerable<(int cell, int dir)> FindReachableCells(int fromCell)
            {
                if (fromCell / _w != 0 && known[maze[fromCell] - 'a'][0] != true)
                    yield return (cell: fromCell - _w, dir: 0);
                if (fromCell % _w != _w - 1 && known[maze[fromCell] - 'a'][1] != true)
                    yield return (cell: fromCell + 1, dir: 1);
                if (fromCell / _w != _h - 1 && known[maze[fromCell] - 'a'][2] != true)
                    yield return (cell: fromCell + _w, dir: 2);
                if (fromCell % _w != 0 && known[maze[fromCell] - 'a'][3] != true)
                    yield return (cell: fromCell - 1, dir: 3);
            }

            while (visited.Count < _w * _h)
            {
                var discovered = visited.SelectMany(cell => FindReachableCells(cell).Select(tup => tup.cell)).Except(visited).ToArray();
                if (discovered.Length == 0)
                    break;
                visited.AddRange(discovered);
            }
            return visited.Count;
        }

        public override (int letter, int dir, bool wall)? Deduce(bool?[][] known, string maze)
        {
            for (var ltr = 0; ltr < known.Length; ltr++)
                for (var dir = 0; dir < 4; dir++)
                    if (known[ltr][dir] == null)
                    {
                        known[ltr][dir] = true;
                        var count = FindReachable(known, maze, maze.IndexOf((char) (ltr + 'a')));
                        known[ltr][dir] = null;
                        if (count != _w * _h)
                            return (ltr, dir, false);
                    }
            return null;
        }
    }

    public static ConsoleColoredString VisualizeKnowns(bool?[][] known, string maze)
    {
        var repertoire = known.Select((walls, ix) => ConsoleColoredString.Format("{0}{4}{1}\n{5}{8}{6}\n{2}{7}{3}",
            walls[0] == true && walls[3] == true ? " ┌─" : walls[0] == true ? " ╶─" : walls[3] == true ? " ╷ " : "   ",
            walls[0] == true && walls[1] == true ? "─┐ " : walls[0] == true ? "─╴ " : walls[1] == true ? " ╷ " : "   ",
            walls[2] == true && walls[3] == true ? " └─" : walls[2] == true ? " ╶─" : walls[3] == true ? " ╵ " : "   ",
            walls[2] == true && walls[1] == true ? "─┘ " : walls[2] == true ? "─╴ " : walls[1] == true ? " ╵ " : "   ",
            walls[0] == true ? "───" : walls[0] == null ? " ? ".Color(ConsoleColor.DarkGray) : "   ",
            walls[3] == true ? " │ " : walls[3] == null ? " ? ".Color(ConsoleColor.DarkGray) : "   ",
            walls[1] == true ? " │ " : walls[1] == null ? " ? ".Color(ConsoleColor.DarkGray) : "   ",
            walls[2] == true ? "───" : walls[2] == null ? " ? ".Color(ConsoleColor.DarkGray) : "   ",
            $" {(char) ('a' + ix)} ".Color(ConsoleColor.White)).Split(["\n"])).ToArray();

        var blocks = maze.Select(ltr => repertoire[ltr - 'a']).ToArray();

        return Enumerable.Range(0, _h)
            .Select(row => Enumerable.Range(0, _w).Select(col => blocks[col + _w * row]).Aggregate((p, n) => p.Zip(n, (a, b) => a + b)).JoinColoredString("\n"))
            .JoinColoredString("\n");
    }

    public static void Experiment()
    {
        var allDeductions = new deduction[] { new borderAtSidesDeduction(), new abuttingBordersDeduction(), new avoidDuplicatesDeduction(), new avoidEnclosingARegionDeduction() };
        var disambCount = 0;
        for (var seed = 0; seed <= 0; seed++)
        {
            var rnd = new Random(seed);
            tryAgain:
            var maze = MazeGenerator.GenerateEncodedMaze(rnd);
            maze = "ababcdefcagbdhif";
            //Console.WriteLine(maze);

            IEnumerable<bool?[][]> Recurse(bool?[][] known)
            {
                keepGoing:
                var anyDeduction = false;
                for (var i = 0; i < allDeductions.Length; i++)
                {
                    var tup = allDeductions[i].Deduce(known, maze);
                    if (tup != null)
                    {
                        anyDeduction = true;
                        //ConsoleUtil.WriteLine($"{allDeductions[i].GetType().Name.Color(allDeductions[i].Color)}: {(char) (tup.Value.letter + 'a')} {"NESW"[tup.Value.dir]} = {tup.Value.wall}", null);
                        known[tup.Value.letter][tup.Value.dir] = tup.Value.wall;
                    }
                }
                if (anyDeduction)
                    goto keepGoing;

                var firstUnsolvedLtr = known.IndexOf(walls => walls.Any(w => w == null));
                if (firstUnsolvedLtr == -1)
                {
                    // Verify: Different letters correspond to different wall configurations
                    for (var i = 0; i < known.Length; i++)
                        for (var j = i + 1; j < known.Length; j++)
                            if (known[i].SequenceEqual(known[j]))
                                yield break;

                    // Verify: There is a wall around the outer edge
                    for (var cell = 0; cell < 16; cell++)
                        if ((cell / 4 == 0 && known[maze[cell] - 'a'][0] == false) || (cell % 4 == 3 && known[maze[cell] - 'a'][1] == false) || (cell / 4 == 3 && known[maze[cell] - 'a'][2] == false) || (cell % 4 == 0 && known[maze[cell] - 'a'][3] == false))
                            yield break;

                    // Verify: Adjacent cells agree on whether there is a wall between them or not
                    for (var cell = 0; cell < 16; cell++)
                        if ((cell % 4 != 0 && known[maze[cell - 1] - 'a'][1] != known[maze[cell] - 'a'][3]) || (cell / 4 != 0 && known[maze[cell - 4] - 'a'][2] != known[maze[cell] - 'a'][0]))
                            yield break;

                    // Verify: The resulting structure is a maze in which every location is reachable from every other (no isolated regions)
                    if (avoidEnclosingARegionDeduction.FindReachable(known, maze, 0) != _w * _h)
                        yield break;

                    yield return known;
                    yield break;
                }
                var firstUnsolvedDir = Enumerable.Range(0, 4).First(dir => known[firstUnsolvedLtr][dir] == null);
                foreach (var poss in new[] { true, false })
                {
                    known[firstUnsolvedLtr][firstUnsolvedDir] = poss;
                    foreach (var solution in Recurse(known.Select(k => k.ToArray()).ToArray()))
                        yield return solution;
                }
            }

            var solutions = Recurse(Ut.NewArray<bool?>(maze.Distinct().Count(), 4)).ToArray();
            foreach (var sol in solutions)
                ConsoleUtil.WriteLine(VisualizeKnowns(sol, maze));
            System.Diagnostics.Debugger.Break();
            var tileCounts = Enumerable.Range(0, 15).Select(tile => new bool?[] { (tile & 1) != 0, (tile & 2) != 0, (tile & 4) != 0, (tile & 8) != 0 }).Select(tile => solutions.Count(sol => sol.Any(t => t.SequenceEqual(tile)))).ToArray();
            var disambiguatingIxs = tileCounts.SelectIndexWhere(tc => tc == 1).ToArray();
            if (solutions.Length == 1)
                ConsoleUtil.WriteLine($"(seed {seed}) No disambiguating tile needed!".Color(ConsoleColor.Green));
            else if (disambiguatingIxs.Length == 0)
                goto tryAgain;
            else
            {
                disambCount++;
                ConsoleUtil.WriteLine($"(seed {seed}) Candidate disambiguators: {disambiguatingIxs.JoinString(", ")}".Color(ConsoleColor.Yellow));
                //var disambiguator = disambiguatingIxs.PickRandom(rnd);
                //Console.WriteLine($"Disambiguating tile: {disambiguator}");
            }
        }

        Console.WriteLine($"{disambCount} disambiguations");
    }

    public static void MakeWords(string maze)
    {
        var rows = maze.Split(4).ToArray();
        var eligibleWordsPerRow = rows.Select(row => _wordlist.Where(word =>
        {
            // row = ‘abcd’
            // word = ‘POLE’
            for (var i = 0; i < word.Length; i++)
                for (var j = i + 1; j < word.Length; j++)
                    if ((row[i] == row[j]) != (word[i] == word[j]))
                        return false;
            return true;
        }).ToArray()).ToArray();
        var maxPossible = eligibleWordsPerRow.Count(ews => ews.Length > 0);

        //Clipboard.SetText(eligibleWordsPerRow.Select((candidates, ix) => $"{rows[ix]} = {candidates.JoinString(", ")}").JoinString("\n"));

        IEnumerable<(string[] words, char?[] letterAssoc)> recurse(string[] wordsSoFar, char?[] lettersSoFar, int row)
        {
            if (row == 4)
            {
                yield return (wordsSoFar, lettersSoFar);
                yield break;
            }
            var wordOfs = Rnd.Next(0, eligibleWordsPerRow[row].Length);
            for (var wordIx = 0; wordIx < eligibleWordsPerRow[row].Length; wordIx++)
            {
                var word = eligibleWordsPerRow[row][(wordIx + wordOfs) % eligibleWordsPerRow[row].Length];

                // Check if ‘word’ satisfies the letters that have already been assigned
                for (var i = 0; i < 4; i++)
                    if (lettersSoFar[rows[row][i] - 'a'] is char letter && word[i] != letter)
                        goto busted;

                // Make sure that different cipher letters make different cleartext letters
                for (var i = 0; i < 4; i++)
                    if (lettersSoFar[rows[row][i] - 'a'] == null && lettersSoFar.Contains(word[i]))
                        goto busted;

                var newLettersSoFar = lettersSoFar.ToArray();
                for (var i = 0; i < 4; i++)
                    newLettersSoFar[rows[row][i] - 'a'] = word[i];

                foreach (var solution in recurse(wordsSoFar.Append(word), newLettersSoFar, row + 1))
                    yield return solution;

                busted:;
            }
            foreach (var solution in recurse(wordsSoFar.Append(null), lettersSoFar, row + 1))
                yield return solution;
        }
        tryAgain:
        var maxSoFar = 0;
        string preferredSolution = null;
        foreach (var solution in recurse([], new char?[maze.Distinct().Count()], 0))
        {
            var numWords = solution.words.Count(s => s != null);
            if (numWords > maxSoFar)
            {
                //Console.WriteLine(solution.Select((w, ix) => $"{rows[ix]} = {w}").JoinString(", "));
                preferredSolution = solution.words.Select((s, ix) =>
                {
                    if (s != null)
                        return s;
                    var nonWord = "";
                    var cipher = maze.Substring(4 * ix, 4);
                    for (var i = 0; i < 4; i++)
                    {
                        if (solution.letterAssoc[cipher[i] - 'a'] is char letter)
                            nonWord += letter;
                        else
                        {
                            var unassocLetter = "ABCDEFGHIJKLMNOPQRSTUVWXYZ".Where(ch => !solution.letterAssoc.Contains(ch)).PickRandom();
                            solution.letterAssoc[cipher[i] - 'a'] = unassocLetter;
                            nonWord += unassocLetter;
                        }
                    }
                    return nonWord;
                }).JoinString(", ");
                maxSoFar = numWords;
            }
            if (numWords == maxPossible)
                break;
        }
        if (maxSoFar == 4)
            goto tryAgain;
        Console.WriteLine(preferredSolution);
    }
}
