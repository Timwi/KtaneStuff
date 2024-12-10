using RT.Util;
using RT.Util.Consoles;
using RT.Util.ExtensionMethods;

namespace KtaneStuff
{
    static class DiscardedExperiments
    {
        private static readonly string[] _wordlist = { "ACRONYMIZE", "AFTERSHOCK", "ANTICLERGY", "ARCHFIENDS", "BACKFIELDS", "BACKGROUND", "BIGMOUTHED", "BINOCULARS", "BYPRODUCTS", "BIRTHPLACE", "BLACKTHORN", "BLATHERING", "BLUEPOINTS", "BOULDERING", "BOUNDARIES", "BREAKDOWNS", "BRICKMASON", "BROWNFIELD", "BUCKINGHAM", "BURLINGAME", "CAFETORIUM", "CHEMOTAXIS", "CLOTHESPIN", "CLUSTERING", "COLDSTREAM", "COMPLAINED", "COMPLETING", "CORNFIELDS", "CORNFLAKES", "COUNTERBID", "CRADLESONG", "CROQUETING", "CUMBERLAND", "DECATHLONS", "DECORATING", "DESTROYING", "DISCOURAGE", "DISHWATERY", "DOCKMASTER", "DOGWATCHER", "DOUGHMAKER", "DOWNSTREAM", "DRAGONFISH", "DUMBWAITER", "EARTHBOUND", "EPISTOLARY", "EURYTHMICS", "EXHUMATION", "FARSIGHTED", "FAUNTLEROY", "FINGERHOLD", "FISHMONGER", "FITZGERALD", "FLOURISHED", "FLOWCHARTS", "FOLKSINGER", "FORGIVABLE", "FORMIDABLE", "FREAKISHLY", "GELATINOUS", "GYRFALCONS", "GODPARENTS", "GOLDENHAIR", "GUNPOWDERY", "HACKNEYISM", "HARLEQUINS", "HYDROPLANE", "HIERONYMUS", "HYPODERMAL", "HOVERINGLY", "IDEOGRAPHY", "IMPORTANCE", "IMPROVABLY", "ISOTHERMAL", "JUXTAPOSED", "KILOPARSEC", "LACQUERING", "LADYFINGER", "LARGEMOUTH", "LAWRENCIUM", "LOBSTERING", "LOGARITHMS", "LONGHAIRED", "MAYFLOWERS", "MINERALOGY", "MONKEYTAIL", "MUDSLINGER", "MULTIPHASE", "NARCOLEPSY", "OVERPAYING", "PADLOCKING", "PALMERSTON", "PARCHMENTS", "PATRONYMIC", "PISTONHEAD", "PITCHERFUL", "PLAYGROUND", "PLANETOIDS", "PLEASURING", "POLYTHEISM", "PRESOAKING", "PREVIOUSLY", "PRODUCTIVE", "PROSCENIUM", "PRUDENTIAL", "QUACKISHLY", "QUADRICEPS", "REFOCUSING", "REGULATION", "RHAPSODIZE", "RHEUMATOID", "ROUNDTABLE", "SHOPLIFTED", "SILVERBACK", "SMOLDERING", "SOUNDTRACK", "SPRINGHEAD", "SQUELCHING", "STOCKPILED", "STOMACHING", "STRICKLAND", "SUBLIMATED", "SUBMEDIANT", "SUPERYACHT", "SWITCHOVER", "TAMBOURINE", "THUMBSCREW", "TIMBERLAND", "TOURMALINE", "TRACKHOUND", "TRAMPOLINE", "TRAPEZOIDS", "TUMBLEDOWN", "UNSCRAMBLE", "UPHOLSTERY", "VESTIBULAR", "VOLKSWAGEN", "WAVEFRONTS", "WHIRLABOUT", "WINGSPREAD" };
        private static readonly string _alph = "ABCDEFGHIJKLM.NOPQRSTUVWXYZ";

        public static void CubeWithDiagonalClues()
        {
            var rnd = new Random(23687);

            // Pick a word
            var word = _wordlist[rnd.Next(0, _wordlist.Length)];

            bool adj(int c1, int c2)
            {
                var x = Math.Abs((c1 % 3) - (c2 % 3));
                var y = Math.Abs(((c1 / 3) % 3) - ((c2 / 3) % 3));
                var z = Math.Abs((c1 / 9) - (c2 / 9));
                var ones = (x == 1 ? 1 : 0) + (y == 1 ? 1 : 0) + (z == 1 ? 1 : 0);
                var zeros = (x == 0 ? 1 : 0) + (y == 0 ? 1 : 0) + (z == 0 ? 1 : 0);
                return ones == 1 && zeros == 2;
            }

            // Fill the cube with this word
            var cubes = Enumerable.Range(0, 27).ToArray();
            IEnumerable<int?[]> fillings(int?[] sofar, int ix, int? lastCube)
            {
                if (ix == word.Length + 1)
                {
                    yield return sofar.ToArray();
                    yield break;
                }

                var candidates = cubes.Where(cube => sofar[cube] == null && (lastCube == null || adj(cube, lastCube.Value))).ToArray();
                if (candidates.Length == 0)
                    yield break;
                var offset = rnd.Next(0, candidates.Length);
                for (var cIx = 0; cIx < 27; cIx++)
                {
                    var cube = candidates[(cIx + offset) % candidates.Length];
                    sofar[cube] = ix == 0 ? 0 : word[ix - 1] - 'A' + 1;
                    foreach (var filling in fillings(sofar, ix + 1, cube))
                        yield return filling;
                    sofar[cube] = null;
                }
            }
            var randomFilling = fillings(new int?[27], 0, null).First();
            var empty = randomFilling.SelectIndexWhere(rf => rf == null).ToArray();
            var alph = _alph.Select(ch => ch == '.' ? 0 : ch - 'A' + 1).ToArray();
            var remainingAlphabet = alph.Except(randomFilling.WhereNotNull()).ToList().Shuffle(rnd);
            var fullFilling = Ut.NewArray(27, ix => randomFilling[ix] ?? '_');
            for (var i = 0; i < empty.Length; i++)
                fullFilling[empty[i]] = remainingAlphabet[i];

            char ltr(int n) => n == 0 ? '·' : (char) ('A' + n - 1);

            Console.WriteLine(word);
            ConsoleUtil.WriteLine(Enumerable.Range(0, 3)
                .Select(row => Enumerable.Range(0, 3)
                    .Select(slice => Enumerable.Range(0, 3)
                        .Select(col => (col + 3 * row + 9 * slice)
                            .Apply(ix => ltr(fullFilling[ix])
                                .Color(
                                    randomFilling[ix] == null ? ConsoleColor.Gray : ConsoleColor.White,
                                    randomFilling[ix] == null ? ConsoleColor.Black : ConsoleColor.DarkBlue)))
                        .JoinColoredString(" "))
                    .JoinColoredString("    "))
                .JoinColoredString("\n"));
            Console.WriteLine();

            int? cubeInDir(int cube, int dir, int amount = 1)
            {
                var x = cube % 3;
                var y = (cube / 3) % 3;
                var z = cube / 9;

                var dx = dir % 3 - 1;
                var dy = (dir / 3) % 3 - 1;
                var dz = dir / 9 - 1;

                var nx = x + amount * dx;
                var ny = y + amount * dy;
                var nz = z + amount * dz;
                return (nx < 0 || nx > 2 || ny < 0 || ny > 2 || nz < 0 || nz > 2) ? null : (nx + ny * 3 + nz * 9).Nullable();
            }

            var clues = (
                from startCube in Enumerable.Range(0, 27)
                where startCube != 13
                from direction in Enumerable.Range(0, 27)
                where direction != 13 && cubeInDir(startCube, 26 - direction) == null
                let affectedCubes = Enumerable.Range(0, 3).Select(amt => cubeInDir(startCube, direction, amt)).WhereNotNull().ToArray()
                //where affectedCubes.Length > 1
                let sum = affectedCubes.Sum(cube => fullFilling[cube])
                select new Clue(startCube, direction, sum, affectedCubes)).ToArray();

            clues.Shuffle(rnd);

            IEnumerable<int[]> solveCube(int?[] sofar, Clue[] constraints)
            {
                var ix = -1;
                var partialSums = constraints.Select(c => c.AffectedCubes.Sum(ac => sofar[ac] ?? 0)).ToArray();
                var numUnfilled = constraints.Select(c => c.AffectedCubes.Count(ac => sofar[ac] == null)).ToArray();
                var unused = Enumerable.Range(0, 27).Except(sofar.WhereNotNull()).ToArray();
                int[] best = null;
                for (var cubeIx = 0; cubeIx < 27; cubeIx++)
                {
                    if (sofar[cubeIx] != null)
                        continue;
                    var available = unused.Where(val =>
                    {
                        if (sofar.Contains(val))
                            return false;
                        for (var constrIx = 0; constrIx < constraints.Length; constrIx++)
                        {
                            var c = constraints[constrIx];
                            if (c.AffectedCubes.Contains(cubeIx))
                            {
                                if (partialSums[constrIx] + val + unused.Where(u => u != val).Take(numUnfilled[constrIx] - 1).Sum() > c.Sum)
                                    return false;
                                if (partialSums[constrIx] + val + unused.Where(u => u != val).TakeLast(numUnfilled[constrIx] - 1).Sum() < c.Sum)
                                    return false;
                            }
                        }
                        return true;
                    }).ToArray();
                    if (available.Length == 0)
                        yield break;
                    if (best == null || available.Length < best.Length)
                    {
                        best = available;
                        ix = cubeIx;
                        if (best.Length == 1)
                            goto shortcut;
                    }
                }

                if (ix == -1)
                {
                    yield return sofar.WhereNotNull().ToArray();
                    yield break;
                }

                shortcut:
                foreach (var val in best)
                {
                    sofar[ix] = val;
                    foreach (var solution in solveCube(sofar, constraints))
                        yield return solution;
                }
                sofar[ix] = null;
            }

            var requiredClues = Ut.ReduceRequiredSet(Enumerable.Range(0, clues.Length).ToArray(), skipConsistencyTest: true, test: state =>
            {
                Console.WriteLine(Enumerable.Range(0, clues.Length).Select(i => state.SetToTest.Contains(i) ? "█" : "░").JoinString());
                return !solveCube(new int?[27], state.SetToTest.Select(i => clues[i]).ToArray()).Skip(1).Any();
            }).Select(i => clues[i]).ToArray();

            foreach (var clue in requiredClues)
                Console.WriteLine(clue);
            Console.WriteLine($"{requiredClues.Length} clues");
            Console.WriteLine();
            foreach (var clue in requiredClues)
                Console.WriteLine($"{clue.AffectedCubes.Select(c => c == 26 ? "a2" : ((char) ('a' + c)).ToString()).JoinString("+")} = {clue.Sum},");
        }

        sealed class Clue
        {
            public int StartCube { get; private set; }
            public int Direction { get; private set; }
            public int Sum { get; private set; }
            public int[] AffectedCubes { get; private set; }
            public Clue(int startCube, int direction, int sum, int[] affectedCubes)
            {
                StartCube = startCube;
                Direction = direction;
                Sum = sum;
                AffectedCubes = affectedCubes;
            }
            public override string ToString() => $"{{ Cube={StartCube}, Dir={Direction}, Sum={Sum}, Cubes={AffectedCubes.JoinString("/")} }}";
        }
    }
}