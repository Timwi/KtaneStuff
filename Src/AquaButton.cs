using PuzzleSolvers;
using RT.Util;
using RT.Util.Consoles;
using RT.Util.ExtensionMethods;

namespace KtaneStuff;

internal static class AquaButton
{
    public static bool[][] BrailleBits = @"1 12 14 145 15 124 1245 125 24 245 13 123 134 1345 135 1234 12345 1235 234 2345 136 1236 2456 1346 13456 1356"
        .Split(' ')
        .Select(braille => Enumerable.Range(0, 6).Select(i => braille.Contains((char) ('1' + i))).ToArray())
        .ToArray();

    private static string nonogramClue(IEnumerable<bool> src) => src.GroupConsecutive().Where(gr => gr.Key).Select(gr => gr.Count).JoinString().PadLeft(1, '0');

    private static Dictionary<string, bool[][]> getNonogramCombinations()
    {
        var dic = new Dictionary<string, List<bool[]>>();
        for (var i = 0; i < (1 << 6); i++)
        {
            var arr = Enumerable.Range(0, 6).Select(bit => (i & (1 << bit)) != 0).ToArray();
            dic.AddSafe(nonogramClue(arr), arr);
        }
        return dic.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.ToArray());
    }

    private static readonly string[] _words = ["ABACUS", "ABDUCT", "ABJECT", "ABLAZE", "ABOARD", "ABOUND", "ABROAD", "ABRUPT", "ABSENT", "ABSORB", "ABSURD", "ACACIA", "ACCEDE", "ACCENT", "ACCEPT", "ACCORD", "ACCUSE", "ACHING", "ACIDIC", "ACQUIT", "ACROSS", "ACTING", "ACTION", "ACTIVE", "ACTORS", "ACTUAL", "ACUITY", "ACUMEN", "ADAGIO", "ADDICT", "ADDING", "ADJOIN", "ADJUST", "ADMIRE", "ADMITS", "ADORED", "ADULTS", "ADVENT", "ADVERB", "ADVERT", "ADVICE", "ADVISE", "AFFECT", "AFFINE", "AFFIRM", "AFFORD", "AFGHAN", "AFIELD", "AFLAME", "AFLOAT", "AFRAID", "AFRESH", "AFRICA", "AGENCY", "AGENDA", "AGENTS", "AGREED", "AGREES", "ALASKA", "ALBEIT", "ALBINO", "ALBUMS", "ALKALI", "ALLEGE", "ALLIED", "ALLIES", "ALLOWS", "ALLUDE", "ALLURE", "ALMOND", "ALMOST", "ALPACA", "ALPINE", "ALUMNI", "ALWAYS", "AMAZED", "AMAZON", "AMBUSH", "AMENDS", "AMIDST", "AMOEBA", "AMORAL", "AMOUNT", "AMULET", "AMUSED", "ANALOG", "ANCHOR", "ANEMIA", "ANEMIC", "ANGINA", "ANGLER", "ANGORA", "ANIMAL", "ANKLES", "ANKLET", "ANNUAL", "ANOINT", "ANORAK", "ANSWER", "ANTHEM", "ANTLER", "ANYHOW", "ANYONE", "APACHE", "APATHY", "APIECE", "APOGEE", "APPALL", "APPEAL", "APPEAR", "APPEND", "APPLES", "APPLET", "ARCADE", "ARCANA", "ARCANE", "ARCHED", "ARCHER", "ARCHLY", "ARDENT", "ARGUED", "ARISEN", "ARMADA", "ARMFUL", "ARMIES", "ARMING", "ARMORY", "ARMOUR", "ARMPIT", "AROUND", "AROUSE", "ARREST", "ARRIVE", "ARROWS", "ARTERY", "ARTFUL", "ARTIST", "ASCEND", "ASCENT", "ASHORE", "ASKING", "ASLEEP", "ASPIRE", "ASSENT", "ASSERT", "ASSIGN", "ASSIST", "ASSUME", "ASSURE", "ASTHMA", "ASTRAL", "ASTRAY", "ASTUTE", "ASYLUM", "ATOMIC", "ATONAL", "ATRIUM", "ATTACH", "ATTACK", "ATTAIN", "ATTEND", "ATTIRE", "ATTUNE", "AUGUST", "AUTHOR", "AUTISM", "AUTUMN", "AVATAR", "AVOIDS", "AVOWAL", "AVOWED", "AWNING", "AZALEA", "BABOON", "BACKED", "BACKER", "BACKUP", "BADGER", "BAGGED", "BAGGER", "BAGGIE", "BAKERY", "BAKING", "BALDLY", "BALLAD", "BALLET", "BALLOT", "BAMBOO", "BANDED", "BANDIT", "BANGED", "BANGER", "BANISH", "BANKED", "BANKER", "BANNED", "BANNER", "BANZAI", "BAOBAB", "BARBED", "BARBER", "BARELY", "BARIUM", "BARKER", "BARLEY", "BARMAN", "BARNET", "BARONS", "BARRED", "BARREN", "BASICS", "BASINS", "BASQUE", "BATHER", "BATTLE", "BAUBLE", "BAZAAR", "BEACON", "BEADED", "BEAKER", "BEAMED", "BEARER", "BEAUTY", "BEAVER", "BECAME", "BECKON", "BECOME", "BEDBUG", "BEDLAM", "BEDPAN", "BEFALL", "BEFORE", "BEGGAR", "BEGGED", "BEGINS", "BEHALF", "BEHEAD", "BEHIND", "BEHOLD", "BEINGS", "BELLOW", "BELONG", "BEMOAN", "BENIGN", "BERATE", "BEREFT", "BERLIN", "BESIDE", "BETRAY", "BETTOR", "BEWARE", "BEYOND", "BIDDER", "BIGGER", "BIGWIG", "BIKING", "BIKINI", "BILLOW", "BINARY", "BINDER", "BIOGAS", "BIONIC", "BIOPIC", "BIOPSY", "BIRDER", "BISECT", "BISHOP", "BISQUE", "BISTRO", "BITING", "BLAMED", "BLANCH", "BLASTS", "BLAZER", "BLOCKS", "BLOCKY", "BLONDE", "BLOODY", "BLOTCH", "BLOWER", "BLUISH", "BLURRY", "BOARDS", "BOASTS", "BODEGA", "BODILY", "BOILED", "BOLDLY", "BOLERO", "BOLTED", "BOMBED", "BOMBER", "BONBON", "BONDED", "BONNET", "BONOBO", "BONSAI", "BOOGIE", "BOOKIE", "BOOMER", "BOOTED", "BORDER", "BORING", "BORROW", "BOSTON", "BOTANY", "BOTHER", "BOTTLE", "BOTTOM", "BOUGHT", "BOUNDS", "BOUNTY", "BOVINE", "BOWING", "BOWLER", "BOXCAR", "BOXING", "BOYISH", "BRAINS", "BRAINY", "BRAISE", "BRANCH", "BRANDS", "BRANDY", "BRASSY", "BRAWNY", "BRAZEN", "BRAZIL", "BREACH", "BREAST", "BREATH", "BREECH", "BREEDS", "BREEZE", "BREEZY", "BRIDAL", "BRIDGE", "BRINGS", "BROACH", "BROGUE", "BROKEN", "BROKER", "BRONZE", "BROOCH", "BROOKS", "BROWSE", "BRUISE", "BRUNCH", "BRUSHY", "BRUTAL", "BUBBLY", "BUFFED", "BUFFER", "BUGGER", "BUMBLE", "BUMMED", "BUNDLE", "BUNGEE", "BUNGLE", "BUNKER", "BURDEN", "BUREAU", "BURGER", "BURIAL", "BURIED", "BURLAP", "BURNED", "BURNER", "BURROW", "BURSTS", "BUSBOY", "BUSHES", "BUSILY", "BUSING", "BUSTED", "BUSTLE", "BUTANE", "BUTTER", "BUTTON", "BUYERS", "BUYING", "BUYOUT", "BUZZER", "BYGONE", "BYPASS", "CACTUS", "CALICO", "CALLED", "CALLER", "CALLUS", "CALMLY", "CALVES", "CAMPER", "CAMPUS", "CANALS", "CANARY", "CANCEL", "CANCER", "CANDID", "CANDOR", "CANINE", "CANNED", "CANNON", "CANOLA", "CANOPY", "CANVAS", "CANYON", "CAPPED", "CAPTOR", "CARBON", "CAREEN", "CAREER", "CARERS", "CARESS", "CARING", "CARNAL", "CARPAL", "CARPET", "CARROT", "CARTON", "CARVED", "CASEIN", "CASHEW", "CASING", "CASINO", "CASTLE", "CATCHY", "CATION", "CATNAP", "CATNIP", "CATTLE", "CAUGHT", "CAVEAT", "CAVERN", "CAVING", "CAVITY", "CELERY", "CEMENT", "CENSOR", "CENSUS", "CENTRE", "CEREAL", "CERVIX", "CESIUM", "CHAINS", "CHAISE", "CHAKRA", "CHALKY", "CHANCE", "CHANGE", "CHAPEL", "CHARGE", "CHATTY", "CHEEKY", "CHEERY", "CHEESY", "CHERRY", "CHERUB", "CHEWED", "CHIEFS", "CHILLY", "CHOKED", "CHOKER", "CHOPPY", "CHORAL", "CHORDS", "CHORUS", "CHROME", "CHUBBY", "CHUNKS", "CHUNKY", "CHURCH", "CICADA", "CINDER", "CIPHER", "CITING", "CITRUS", "CIVICS", "CLAIMS", "CLAMMY", "CLAMOR", "CLASSY", "CLAUSE", "CLAWED", "CLOCKS", "CLONED", "CLONES", "CLOSED", "CLOSER", "CLOSES", "CLOSET", "CLOTHE", "CLOUDS", "CLOUDY", "CLOVER", "CLUMPY", "CLUMSY", "CLUNKY", "CLUTCH", "COARSE", "COASTS", "COATED", "COBWEB", "COCOON", "CODIFY", "CODING", "COFFEE", "COFFIN", "COHERE", "COLDER", "COLDLY", "COLONY", "COLORS", "COLUMN", "COMEDY", "COMING", "COMMIT", "COMMON", "COMPEL", "COMPLY", "CONCUR", "CONDOR", "CONFER", "CONSUL", "CONVEX", "CONVEY", "CONVOY", "COOKER", "COOLER", "COPIED", "COPIER", "COPIES", "COPING", "COPPER", "CORDED", "CORDON", "CORNEA", "CORNED", "CORNER", "CORNET", "CORONA", "CORPSE", "CORPUS", "CORRAL", "CORSET", "CORTEX", "COSMIC", "COSTLY", "COTTON", "COUNTS", "COUNTY", "COUPON", "COURSE", "COURTS", "COVENT", "COVERS", "COVERT", "COWARD", "COWBOY", "COYOTE", "CRABBY", "CRAFTY", "CRANKY", "CRANNY", "CRATER", "CRAYON", "CRAZED", "CREAKY", "CREAMY", "CREASE", "CREATE", "CREDIT", "CREEPY", "CREWED", "CRINGE", "CRISPS", "CRISPY", "CROCUS", "CROTCH", "CROUCH", "CROWDS", "CRUISE", "CRUMMY", "CRUNCH", "CRUSTY", "CRUTCH", "CRYING", "CUBISM", "CUBIST", "CUCKOO", "CUPPED", "CURATE", "CURDLE", "CURFEW", "CURLED", "CURLER", "CURSED", "CURSOR", "CURTLY", "CURTSY", "CURVED", "CURVES", "CUSTOM", "CUTESY", "CUTLET", "CUTTER", "CYBORG", "CYCLIC", "CYMBAL", "DAGGER", "DAMAGE", "DAMPEN", "DAMPER", "DANCED", "DANCER", "DANCES", "DANDER", "DANGER", "DANISH", "DAPPER", "DAPPLE", "DARING", "DARKEN", "DARKER", "DARKLY", "DARNED", "DASHED", "DASHER", "DATING", "DAWDLE", "DAZZLE", "DEADEN", "DEADLY", "DEAFEN", "DEALER", "DEARLY", "DEBRIS", "DEBTOR", "DEBUNK", "DECADE", "DECAMP", "DECANT", "DECEIT", "DECENT", "DECIDE", "DECODE", "DECREE", "DEDUCT", "DEEMED", "DEEPEN", "DEEPER", "DEEPLY", "DEFAME", "DEFANG", "DEFECT", "DEFEND", "DEFINE", "DEFORM", "DEFRAY", "DEFTLY", "DEFUSE", "DEGREE", "DELAYS", "DELETE", "DELUDE", "DELUGE", "DEMAND", "DEMEAN", "DEMONS", "DEMOTE", "DEMURE", "DENIED", "DENIER", "DENOTE", "DENUDE", "DEPART", "DEPEND", "DEPICT", "DEPLOY", "DEPORT", "DEPOSE", "DEPUTY", "DERAIL", "DERIDE", "DERIVE", "DERMAL", "DESIGN", "DESIRE", "DESIST", "DETACH", "DETAIL", "DETAIN", "DETECT", "DETOUR", "DEVILS", "DEVOID", "DEVOTE", "DEVOUT", "DIADEM", "DIALOG", "DIAPER", "DIATOM", "DICTUM", "DIFFER", "DIGGER", "DIGITS", "DILATE", "DILUTE", "DIMMED", "DIMMER", "DINGHY", "DINING", "DINNER", "DIPOLE", "DISARM", "DISBAR", "DISCUS", "DISHES", "DISMAL", "DISMAY", "DISOWN", "DISPEL", "DISUSE", "DITHER", "DIVERS", "DIVERT", "DIVEST", "DIVIDE", "DIVINE", "DIVING", "DOCTOR", "DOGGIE", "DOINGS", "DOLLOP", "DOMAIN", "DOMINO", "DONATE", "DONKEY", "DONORS", "DOODAD", "DOODLE", "DOOMED", "DOSAGE", "DOTING", "DOTTED", "DOUBLE", "DOUBLY", "DOUBTS", "DOUGHY", "DOVISH", "DOWNED", "DOWNER", "DOZENS", "DRAFTY", "DRAGON", "DRAINS", "DRAPED", "DRAWER", "DRIPPY", "DRIVEL", "DRIVEN", "DRIVER", "DROOPY", "DROWSY", "DRUDGE", "DRYING", "DUFFEL", "DUGOUT", "DULLED", "DUMBLY", "DUMPED", "DUPLEX", "DURESS", "DURIAN", "DURING", "DUSTER", "DUTIES", "DYEING", "DYNAMO", "EARFUL", "EARNED", "EARNER", "EASIER", "EASILY", "EASING", "EATERY", "EATING", "ECHOED", "ECZEMA", "EDGING", "EDITOR", "EERILY", "EFFECT", "EFFIGY", "EGGNOG", "EGRESS", "EIGHTY", "EITHER", "ELAPSE", "ELATED", "ELBOWS", "ELDEST", "ELVISH", "EMBALM", "EMBARK", "EMBODY", "EMBOSS", "EMBRYO", "EMPIRE", "EMPLOY", "ENAMEL", "ENCASE", "ENCODE", "ENCORE", "ENDING", "ENDURE", "ENFOLD", "ENGAGE", "ENGINE", "ENGULF", "ENIGMA", "ENJOIN", "ENJOYS", "ENLIST", "ENMITY", "ENOUGH", "ENRAGE", "ENROLL", "ENSURE", "ENTAIL", "ENTERS", "ENTIRE", "ENTITY", "ENTOMB", "ENTRAP", "ENTREE", "ENZYME", "EQUALS", "EQUATE", "EQUINE", "EQUITY", "ERASED", "ERASER", "ERODED", "ERRAND", "ERRANT", "ERRORS", "ERSATZ", "ESCAPE", "ESCHEW", "ESSAYS", "ESTATE", "ESTEEM", "ETHICS", "ETHNIC", "EULOGY", "EUREKA", "EUROPE", "EVOLVE", "EXCEED", "EXCEPT", "EXCESS", "EXCISE", "EXCITE", "EXCUSE", "EXEMPT", "EXHUME", "EXILED", "EXISTS", "EXODUS", "EXPAND", "EXPECT", "EXPEND", "EXPERT", "EXPIRE", "EXPIRY", "EXPORT", "EXPOSE", "EXTANT", "EXTEND", "EXTENT", "EXTORT", "EXTRAS", "EYEFUL", "EYELID", "FABLED", "FABRIC", "FACADE", "FACING", "FACTOR", "FADING", "FAKERY", "FAKING", "FALCON", "FALLEN", "FAMILY", "FAMINE", "FAMOUS", "FANDOM", "FARMED", "FARMER", "FATHER", "FATHOM", "FAUCET", "FAULTS", "FAULTY", "FAVOUR", "FEARED", "FEDORA", "FEEBLY", "FEEDER", "FEELER", "FEISTY", "FELINE", "FELLED", "FELLOW", "FELONY", "FENCED", "FENCER", "FENCES", "FENDER", "FENNEL", "FERRET", "FETISH", "FIANCE", "FIASCO", "FIBULA", "FIELDS", "FIERCE", "FIGURE", "FILING", "FILLED", "FILLER", "FILLET", "FILMED", "FINALE", "FINALS", "FINDER", "FINELY", "FINERY", "FINEST", "FINGER", "FINISH", "FINITE", "FIRING", "FIRMLY", "FISCAL", "FISHER", "FIXATE", "FIXING", "FLABBY", "FLARED", "FLASHY", "FLATLY", "FLAUNT", "FLAVOR", "FLAWED", "FLOCKS", "FLOODS", "FLOORS", "FLOPPY", "FLORAL", "FLOWED", "FLOWER", "FLUENT", "FLUFFY", "FLUIDS", "FLURRY", "FLUTED", "FLYING", "FLYWAY", "FODDER", "FOETUS", "FOILED", "FOLDED", "FOLDER", "FOLLOW", "FONDLE", "FONDLY", "FONDUE", "FOODIE", "FOOTER", "FORAGE", "FORBID", "FORCED", "FORCES", "FOREGO", "FOREST", "FORGED", "FORGER", "FORGET", "FORGOT", "FORKED", "FORMAL", "FORMAT", "FORMED", "FORMER", "FOSTER", "FOUGHT", "FOULED", "FOURTH", "FOWLER", "FRAMED", "FRAMER", "FRANCE", "FRANCO", "FRANCS", "FRAYED", "FREAKY", "FREELY", "FREEZE", "FRENCH", "FRENZY", "FRESCO", "FRIDAY", "FRIDGE", "FRIEND", "FRIGID", "FRILLY", "FRINGE", "FRISKY", "FRIZZY", "FROLIC", "FRONTS", "FROSTY", "FROTHY", "FROZEN", "FRUGAL", "FRUITS", "FRUITY", "FRYING", "FULFIL", "FULHAM", "FUMBLE", "FUNDED", "FUNDER", "FUNGAL", "FUNGUS", "FUNNEL", "FURROW", "FUSION", "FUTILE", "FUTURE", "GADFLY", "GAFFER", "GALAXY", "GALLEY", "GALLON", "GALLOP", "GALORE", "GAMBIT", "GAMELY", "GAMETE", "GAMING", "GANDER", "GANGLY", "GANTRY", "GAPING", "GARAGE", "GARBLE", "GARDEN", "GARGLE", "GARISH", "GARNER", "GARNET", "GASPED", "GATHER", "GAZEBO", "GAZING", "GEARED", "GEEZER", "GELATO", "GENDER", "GENIUS", "GENTLY", "GENTRY", "GERBIL", "GERMAN", "GHETTO", "GHOSTS", "GIANTS", "GILDED", "GINGER", "GIRDER", "GIRDLE", "GIRLIE", "GIVING", "GLADLY", "GLANCE", "GLANDS", "GLARED", "GLASSY", "GLAZED", "GLOBAL", "GLOOMY", "GLOSSY", "GLOVED", "GLOVES", "GLOWER", "GLUMLY", "GLUTEN", "GLYCOL", "GNARLY", "GOALIE", "GOATEE", "GOBLIN", "GOLDEN", "GOLFER", "GOODLY", "GOPHER", "GOTHIC", "GOTTEN", "GOVERN", "GRADED", "GRADER", "GRAHAM", "GRAINS", "GRAINY", "GRANNY", "GRANTS", "GRASSY", "GRATED", "GRATER", "GRAVEL", "GRAVEN", "GRAZED", "GREASE", "GREASY", "GREECE", "GREEDY", "GREENS", "GRIMLY", "GRINCH", "GRISLY", "GRITTY", "GROCER", "GROGGY", "GROOVE", "GROOVY", "GROTTO", "GROUCH", "GROUND", "GROUPS", "GROVEL", "GROWER", "GROWTH", "GRUBBY", "GRUDGE", "GRUMPY", "GRUNGE", "GRUNGY", "GUARDS", "GUNMAN", "GUNMEN", "GUNNER", "GURGLE", "GURNEY", "GUSHER", "GUTTED", "GUTTER", "GUZZLE", "GYPSUM", "GYRATE", "HABITS", "HACKER", "HADRON", "HAGGIS", "HALVED", "HALVES", "HAMLET", "HAMMER", "HAMPER", "HANDED", "HANGAR", "HANGER", "HANKER", "HAPPEN", "HARASS", "HARBOR", "HARDEN", "HARDER", "HARDLY", "HARROW", "HASSLE", "HATRED", "HAULED", "HAULER", "HAVING", "HAZARD", "HAZING", "HEADED", "HEADER", "HEALED", "HEALER", "HEARER", "HEARTY", "HEAVED", "HEAVEN", "HELIUM", "HELMET", "HELPED", "HELPER", "HERALD", "HERBAL", "HERDER", "HEREBY", "HEREIN", "HERESY", "HERMIT", "HERNIA", "HEROES", "HEROIC", "HEYDAY", "HIATUS", "HICCUP", "HIDDEN", "HIDING", "HIGHER", "HIGHLY", "HIJACK", "HIKING", "HINDER", "HINGED", "HINGES", "HIPPIE", "HIRING", "HITHER", "HITMAN", "HOARSE", "HOBBIT", "HOBNOB", "HOLDER", "HOLDUP", "HOLLER", "HOLLOW", "HOMAGE", "HOMELY", "HOMING", "HONCHO", "HONEST", "HONOUR", "HOODED", "HOODIE", "HOOKED", "HOOKUP", "HOOPER", "HOOPLA", "HOORAY", "HOOVES", "HOPING", "HOPPER", "HORNED", "HORRID", "HORROR", "HORSES", "HORSEY", "HOTBED", "HOTDOG", "HOUNDS", "HOURLY", "HOUSED", "HOUSES", "HOWLER", "HUBBUB", "HUBRIS", "HUGELY", "HUGGED", "HUGGER", "HUMANE", "HUMANS", "HUMBLE", "HUMBLY", "HUMBUG", "HUMMUS", "HUMOUR", "HUMPED", "HUNGER", "HUNGRY", "HUNKER", "HUNTED", "HUNTER", "HURDLE", "HURRAH", "HUSHED", "HUSTLE", "HYBRID", "HYMNAL", "HYPHEN", "ICEBOX", "ICECAP", "ICONIC", "IDIOCY", "IDLING", "IGNITE", "IGNORE", "IGUANA", "IMPACT", "IMPAIR", "IMPALE", "IMPART", "IMPEDE", "IMPISH", "IMPORT", "IMPOSE", "IMPROV", "IMPURE", "INDEED", "INDENT", "INDIAN", "INDICT", "INDIGO", "INDIUM", "INDOOR", "INDUCE", "INDUCT", "INFAMY", "INFANT", "INFEST", "INFILL", "INFIRM", "INFLOW", "INFLUX", "INFORM", "INFUSE", "INGEST", "INJURE", "INJURY", "INKPOT", "INLAND", "INNATE", "INNING", "INPUTS", "INSANE", "INSERT", "INSIDE", "INSIST", "INSTEP", "INSULT", "INSURE", "INTACT", "INTAKE", "INTEND", "INTENT", "INTERN", "INTUIT", "INVENT", "INVERT", "INVEST", "INVITE", "INVOKE", "INWARD", "IODIDE", "IODINE", "IONIZE", "IRONED", "IRONIC", "ISLAND", "ISRAEL", "ISSUED", "ISSUER", "ISSUES", "ITSELF", "JERKED", "JETLAG", "JEWELS", "JEWISH", "JICAMA", "JINGLE", "JINGLY", "JINXED", "JINXES", "JIVING", "JOGGER", "JOINED", "JOINER", "JOINTS", "JOKERS", "JOKILY", "JOKING", "JOSTLE", "JOULES", "JOYFUL", "JOYOUS", "JUDGED", "JUDGER", "JUMBLE", "JUMPED", "JUNGLE", "JUNIOR", "JUNKER", "JUNKIE", "JURIED", "JURIES", "JURORS", "JUSTLY", "KABOOM", "KARATE", "KARMIC", "KELVIN", "KENNEL", "KERNEL", "KETTLE", "KEYPAD", "KHAKIS", "KIDDIE", "KIDNAP", "KIDNEY", "KILLED", "KILLER", "KINDLE", "KINDLY", "KINGLY", "KIPPER", "KISSED", "KISSER", "KISSES", "KITSCH", "KITTEN", "KLAXON", "KNOCKS", "KNOTTY", "KNOWER", "KOREAN", "KOSHER", "KRAKEN", "LABOUR", "LACKED", "LACKEY", "LACTIC", "LADDER", "LADDIE", "LAGGED", "LAGOON", "LAMELY", "LAMENT", "LANCER", "LANDED", "LAPDOG", "LAPSED", "LAPTOP", "LARDER", "LARGER", "LARVAE", "LASTED", "LASTLY", "LATELY", "LATENT", "LATHER", "LATTER", "LAUDED", "LAUGHS", "LAUNCH", "LAVISH", "LAWFUL", "LAWMAN", "LAWYER", "LAYERS", "LAYING", "LAYMAN", "LAYOFF", "LAYOUT", "LAZILY", "LAZULI", "LEADED", "LEADER", "LEAKED", "LEAKER", "LEANED", "LEAPER", "LEARNS", "LEARNT", "LEASED", "LEAVEN", "LEAVER", "LECTOR", "LEDGER", "LEGACY", "LEGATO", "LEGEND", "LEGGED", "LEGION", "LEGUME", "LEMONY", "LENDER", "LENGTH", "LENSES", "LESION", "LESSEN", "LESSER", "LESSON", "LETHAL", "LETTER", "LEVELS", "LEVITY", "LIBIDO", "LIDDED", "LIFTED", "LIFTER", "LIKELY", "LIKING", "LINGER", "LINING", "LINKED", "LIQUID", "LIQUOR", "LISTED", "LISTEN", "LITANY", "LITMUS", "LITRES", "LITTER", "LITTLE", "LIVELY", "LIVERY", "LIVING", "LIZARD", "LOADED", "LOADER", "LOANED", "LOANER", "LOATHE", "LOCATE", "LOCKED", "LOCKUP", "LOCUST", "LODGED", "LODGER", "LOGGER", "LOITER", "LONDON", "LONELY", "LONGED", "LONGER", "LOOKER", "LOOKUP", "LOOSEN", "LOOTED", "LOOTER", "LORDLY", "LOSING", "LOTION", "LOUDER", "LOUDLY", "LOUNGE", "LOVELY", "LOVERS", "LOVING", "LOWEST", "LUGGED", "LUMBAR", "LUNACY", "LUPINE", "LUSTER", "LUXURY", "LYCHEE", "LYRICS", "MADMAN", "MAGPIE", "MAKERS", "MAKING", "MALADY", "MALIGN", "MALLET", "MALTED", "MAMMAL", "MANAGE", "MANGER", "MANIAC", "MANNED", "MANNER", "MANTIS", "MANTRA", "MANUAL", "MANURE", "MARACA", "MARGIN", "MARINE", "MARKED", "MARKER", "MARKET", "MARKUP", "MARMOT", "MAROON", "MARROW", "MARSHY", "MARTIN", "MARTYR", "MASHED", "MASHER", "MASKED", "MASQUE", "MASSIF", "MASTER", "MATING", "MATRIX", "MATRON", "MATTED", "MATTER", "MATURE", "MAYHEM", "MEASLY", "MEDIAN", "MEDIUM", "MEDLEY", "MEEKLY", "MELLOW", "MELODY", "MELTED", "MEMBER", "MEMOIR", "MEMORY", "MENIAL", "MENTAL", "MENTEE", "MENTOR", "MERELY", "MERGED", "MERGER", "MERITS", "MERLOT", "METHOD", "METHYL", "METRIC", "METTLE", "MEXICO", "MIASMA", "MICRON", "MIGHTY", "MIGNON", "MILADY", "MILDLY", "MILLED", "MILLER", "MIMOSA", "MINCED", "MINERS", "MINGLE", "MINING", "MINION", "MINNOW", "MINUET", "MINUTE", "MIRAGE", "MIRROR", "MISERY", "MISFIT", "MISHAP", "MISLED", "MISSED", "MISSES", "MISSUS", "MISTER", "MISUSE", "MITTEN", "MIXING", "MOANED", "MODERN", "MODEST", "MODIFY", "MODULO", "MOLDED", "MOLDER", "MOLTEN", "MOMENT", "MONDAY", "MONGOL", "MONIES", "MONKEY", "MONTHS", "MORALE", "MORALS", "MORBID", "MORGAN", "MORGUE", "MORMON", "MORSEL", "MORTAL", "MORTAR", "MOSAIC", "MOSQUE", "MOSTLY", "MOTHER", "MOTIFS", "MOTION", "MOTIVE", "MOTLEY", "MOUSSE", "MOUTHS", "MOVIES", "MOVING", "MUFFIN", "MUGGER", "MUMBLE", "MURDER", "MURMUR", "MURPHY", "MUSCLE", "MUSEUM", "MUSING", "MUSTER", "MUTANT", "MUTATE", "MUTELY", "MUTINY", "MUTTER", "MUTTON", "MUTUAL", "MUZZLE", "MYOPIC", "MYRIAD", "MYSELF", "MYSTIC", "MYTHIC", "MYTHOS", "NAMELY", "NAMING", "NAPALM", "NAPKIN", "NARROW", "NATION", "NATIVE", "NATURE", "NAUGHT", "NEARBY", "NEARER", "NEARLY", "NEATLY", "NEBULA", "NECTAR", "NEEDED", "NELSON", "NEPHEW", "NERVES", "NESTED", "NESTER", "NESTLE", "NETHER", "NETTLE", "NEURON", "NEWBIE", "NEWEST", "NEWISH", "NEWTON", "NICELY", "NICKEL", "NIMBLY", "NIMBUS", "NINETY", "NINJAS", "NIPPLE", "NITRIC", "NITWIT", "NOBODY", "NODDED", "NOGGIN", "NOODLE", "NORDIC", "NORMAL", "NORMAN", "NOSIER", "NOSILY", "NOTARY", "NOTATE", "NOTICE", "NOTIFY", "NOTING", "NOTION", "NOUGHT", "NOVELS", "NOZZLE", "NUANCE", "NUDGER", "NUDIST", "NUDITY", "NUMBED", "NUMBLY", "NURSED", "NURSES", "NUTMEG", "NUZZLE", "OBJECT", "OBLIGE", "OBLONG", "OBOIST", "OBTAIN", "OBTUSE", "OCCUPY", "OCEANS", "OCTAVE", "OCULAR", "OCULUS", "ODDITY", "OFFEND", "OFFICE", "OILMAN", "OLDEST", "ONIONS", "ONLINE", "ONWARD", "OOZING", "OPENED", "OPENER", "OPENLY", "OPIATE", "OPIOID", "OPPOSE", "OPTICS", "OPTING", "OPTION", "ORALLY", "ORANGE", "ORATOR", "ORCHID", "ORDAIN", "ORGANS", "ORIENT", "ORIGIN", "ORNATE", "ORPHAN", "OSMIUM", "OUNCES", "OUSTED", "OUSTER", "OUTAGE", "OUTBID", "OUTCRY", "OUTDID", "OUTFIT", "OUTFOX", "OUTING", "OUTLAW", "OUTLET", "OUTPUT", "OUTRUN", "OUTSET", "OUTWIT", "OWLISH", "OWNERS", "OWNING", "OXFORD", "OXTAIL", "OXYGEN", "OYSTER", "PACIFY", "PACING", "PACKED", "PADDED", "PAGING", "PAGODA", "PALATE", "PALLET", "PALLOR", "PALTRY", "PAMPER", "PANAMA", "PANDER", "PANELS", "PANTRY", "PAPACY", "PAPAYA", "PAPERS", "PAPERY", "PARADE", "PARDON", "PARENT", "PARIAH", "PARING", "PARITY", "PARKED", "PARLAY", "PARLOR", "PARODY", "PAROLE", "PARROT", "PARSON", "PARTED", "PARTLY", "PASSED", "PASSER", "PASTOR", "PASTRY", "PATCHY", "PATENT", "PATRON", "PATTED", "PATTEN", "PAUNCH", "PAUPER", "PAUSED", "PAVING", "PAYDAY", "PAYERS", "PAYING", "PAYOFF", "PAYOUT", "PEACHY", "PEAKED", "PEANUT", "PEARCE", "PEARLY", "PECKER", "PECTIN", "PEDANT", "PEELED", "PEELER", "PEEPER", "PEERED", "PEEVED", "PELLET", "PELVIS", "PENCIL", "PEPPER", "PEPTIC", "PERIOD", "PERISH", "PERMIT", "PERSON", "PERUSE", "PESTER", "PESTLE", "PETITE", "PETROL", "PEWTER", "PHARMA", "PHASED", "PHENOL", "PHLEGM", "PHOBIA", "PHONED", "PHONES", "PHONEY", "PHOTON", "PHOTOS", "PHRASE", "PICKED", "PICKER", "PICKUP", "PICNIC", "PIDGIN", "PIERCE", "PIGEON", "PIGPEN", "PIGSTY", "PILEUP", "PILFER", "PILLOW", "PILOTS", "PINATA", "PINCER", "PINNED", "PIPING", "PIRACY", "PIRATE", "PISSED", "PISTOL", "PITTED", "PLACED", "PLACER", "PLACID", "PLAINS", "PLANAR", "PLANTS", "PLASMA", "PLAYED", "PLAYER", "PLOUGH", "PLOWED", "PLUNGE", "PLURAL", "PODIUM", "POETIC", "POETRY", "POINTS", "POINTY", "POISED", "POISON", "POLICY", "POLISH", "POLITE", "POLLEN", "POMMEL", "PONCHO", "PONDER", "PONIES", "POODLE", "POOLED", "POORER", "POORLY", "POPLAR", "POPPED", "POPPER", "POROUS", "PORTAL", "PORTER", "POSING", "POSSUM", "POSTED", "POSTER", "POTATO", "POTENT", "POTION", "POTTED", "POTTER", "POUNCE", "POUNDS", "POURED", "POWDER", "POWERS", "PRAISE", "PRANCE", "PRAXIS", "PRAYED", "PRAYER", "PREACH", "PREFAB", "PREFER", "PREFIX", "PRESTO", "PRETTY", "PRICED", "PRICEY", "PRIEST", "PRIMAL", "PRIMED", "PRIMER", "PRIMLY", "PRINCE", "PRINTS", "PRIORY", "PRISON", "PRISSY", "PRIZED", "PROBES", "PROFIT", "PROMPT", "PROPEL", "PROPER", "PROTON", "PROVED", "PROVEN", "PROVES", "PRYING", "PSEUDO", "PSYCHE", "PSYCHO", "PUBLIC", "PUFFED", "PUFFIN", "PULLED", "PULPIT", "PULSAR", "PULSED", "PULSES", "PUMMEL", "PUMPED", "PUNDIT", "PUNISH", "PUPILS", "PURELY", "PURIFY", "PURIST", "PURITY", "PURPLE", "PURSUE", "PUSHED", "PUSHER", "PUTRID", "PUTTER", "PUZZLE", "PYTHON", "QUAINT", "QUALMS", "QUARRY", "QUARTZ", "QUASAR", "QUORUM", "QUOTAS", "QUOTED", "QUOTER", "QUOTES", "RABBIT", "RACIAL", "RACING", "RADIAL", "RADIAN", "RADISH", "RADIUM", "RADIUS", "RAFTER", "RAGGED", "RAGING", "RAGTAG", "RAKING", "RANCID", "RANCOR", "RANDOM", "RANGED", "RANGES", "RANKED", "RANSOM", "RAPIER", "RAPPEL", "RAPPER", "RAPTOR", "RARELY", "RARITY", "RATHER", "RATIFY", "RATING", "RATION", "RATTLE", "RAVINE", "RAVING", "RAVISH", "READER", "REALLY", "REAPER", "REASON", "REBORN", "REBUFF", "RECALL", "RECANT", "RECEDE", "RECENT", "RECIPE", "RECITE", "RECKON", "RECODE", "RECOIL", "RECORD", "RECTAL", "RECTUM", "RECUSE", "REDDEN", "REDEEM", "REDIAL", "REDRAW", "REFILL", "REFINE", "REFLEX", "REFLUX", "REFORM", "REFUEL", "REFUGE", "REFUND", "REFUSE", "REFUTE", "REGAIN", "REGARD", "REGENT", "REGIME", "REGION", "REGROW", "REJECT", "REJOIN", "RELATE", "RELENT", "RELICS", "RELIED", "RELIEF", "RELIES", "RELISH", "RELOAD", "REMAIN", "REMARK", "REMEDY", "REMIND", "REMISS", "REMOTE", "RENDER", "RENEGE", "RENOWN", "RENTAL", "RENTED", "RENTER", "REOPEN", "REPAIR", "REPEAL", "REPEAT", "REPENT", "REPLAY", "REPORT", "REPOSE", "REPUTE", "REREAD", "RESCUE", "RESENT", "RESIDE", "RESIGN", "RESIST", "RESIZE", "RESTED", "RESUME", "RETAIL", "RETAIN", "RETAKE", "RETINA", "RETIRE", "RETURN", "REVAMP", "REVERT", "REVIEW", "REVOLT", "REWARD", "REWIND", "REWIRE", "REWORK", "RHESUS", "RHUMBA", "RHYTHM", "RIBALD", "RIBBED", "RIBBON", "RICHER", "RICHLY", "RIDDEN", "RIDGED", "RIDING", "RIGGED", "RIGGER", "RIMMED", "RINGED", "RINGER", "RINSED", "RIOTER", "RIPPED", "RIPPER", "RIPPLE", "RISING", "RISQUE", "RITUAL", "RIVALS", "RIVERS", "ROADIE", "ROARED", "ROBOTS", "ROBUST", "ROCOCO", "RODENT", "ROLLED", "ROLLER", "ROMANS", "ROOKIE", "ROOMIE", "ROOTED", "ROPING", "ROSARY", "ROSTER", "ROTARY", "ROTATE", "ROTTED", "ROTTEN", "ROTUND", "ROUNDS", "ROUTER", "ROUTES", "ROVERS", "ROVING", "ROWING", "RUBBED", "RUBBER", "RUBRIC", "RUCKUS", "RUDDER", "RUDELY", "RUDEST", "RUGGED", "RULERS", "RULING", "RUMBLE", "RUMOUR", "RUMPLE", "RUNNER", "RUNOFF", "RUNWAY", "RUSHED", "RUSHER", "RUSSIA", "RUSTED", "RUSTIC", "RUSTLE", "RUTTED", "SACKED", "SADDEN", "SAFARI", "SAFELY", "SAFETY", "SAGELY", "SALADS", "SALAMI", "SALARY", "SALINE", "SALIVA", "SALMON", "SALOON", "SALUTE", "SANDAL", "SARONG", "SASHAY", "SATIRE", "SAUCER", "SAVAGE", "SAVANT", "SAVING", "SAVIOR", "SAVORY", "SAWING", "SAYING", "SCALAR", "SCANTY", "SCARAB", "SCENIC", "SCHEMA", "SCHEME", "SCHISM", "SCHOOL", "SCONCE", "SCORED", "SCORER", "SCORES", "SCOTCH", "SCOUTS", "SCRAPE", "SCRAPS", "SCRAWL", "SCREAM", "SCREEN", "SCREWS", "SCREWY", "SCRIBE", "SCRIPT", "SCROLL", "SCRUFF", "SCULPT", "SCURRY", "SCURVY", "SCYTHE", "SEABED", "SEAMAN", "SEAMEN", "SEANCE", "SECEDE", "SECOND", "SECURE", "SEEDED", "SEEING", "SEEKER", "SEEMED", "SEEMLY", "SEIZED", "SELDOM", "SELECT", "SENATE", "SENDER", "SENIOR", "SENSOR", "SENTRY", "SEPTUM", "SEQUEL", "SEQUIN", "SERAPH", "SERENE", "SERMON", "SESAME", "SETTLE", "SEVENS", "SEWAGE", "SEWING", "SEXUAL", "SHABBY", "SHADED", "SHADOW", "SHAFTS", "SHAGGY", "SHAKEN", "SHAKER", "SHAMAN", "SHANTY", "SHAPED", "SHARED", "SHAVED", "SHAVEN", "SHEILA", "SHEKEL", "SHERRY", "SHIELD", "SHIFTS", "SHIFTY", "SHIMMY", "SHINER", "SHIVER", "SHOCKS", "SHODDY", "SHOGUN", "SHORES", "SHORTS", "SHOULD", "SHOUTS", "SHOVEL", "SHOWED", "SHOWER", "SHREWD", "SHRIEK", "SHRILL", "SHRIMP", "SHRINE", "SHRINK", "SHROUD", "SHRUBS", "SHTICK", "SHUCKS", "SICKEN", "SICKLY", "SIDING", "SIENNA", "SIERRA", "SIFTED", "SIFTER", "SIGHED", "SIGNAL", "SIGNED", "SIGNER", "SILENT", "SILKEN", "SILVER", "SIMIAN", "SIMMER", "SIMPLY", "SINFUL", "SINGED", "SINGER", "SINGLE", "SINKER", "SINNER", "SIPHON", "SISTER", "SITCOM", "SITTER", "SIZING", "SKATER", "SKYBOX", "SLALOM", "SLATER", "SLAYER", "SLOPES", "SLOPPY", "SLOUCH", "SLOUGH", "SLOWED", "SLOWER", "SLOWLY", "SLUDGE", "SLURRY", "SLUSHY", "SMARMY", "SMARTS", "SMELLY", "SMITHY", "SMOKED", "SMOKER", "SMOOCH", "SMOOTH", "SMUDGE", "SMUGLY", "SNAPPY", "SNARKY", "SNAZZY", "SNEAKY", "SNEEZE", "SNIPPY", "SNITCH", "SNOBBY", "SNOOPY", "SNOOTY", "SNOOZE", "SNUGLY", "SOAKED", "SODDEN", "SODIUM", "SODOMY", "SOFTEN", "SOFTER", "SOFTIE", "SOFTLY", "SOILED", "SOIREE", "SOLDER", "SOLELY", "SOLEMN", "SOLIDS", "SOLVED", "SOLVER", "SOMBER", "SOMBRE", "SONATA", "SONNET", "SOONER", "SOOTHE", "SORBET", "SORDID", "SORELY", "SORROW", "SORTED", "SORTER", "SOUGHT", "SOUNDS", "SOURCE", "SOURED", "SOURLY", "SOVIET", "SOWING", "SPACER", "SPEECH", "SPEEDO", "SPEEDS", "SPEEDY", "SPENDS", "SPHERE", "SPICED", "SPIDER", "SPIKED", "SPINAL", "SPIRAL", "SPIRIT", "SPLASH", "SPLEEN", "SPLICE", "SPLINT", "SPOILS", "SPOKEN", "SPONGE", "SPONGY", "SPOOKY", "SPORTS", "SPORTY", "SPOTTY", "SPOUSE", "SPRAIN", "SPRANG", "SPRAWL", "SPREAD", "SPRING", "SPRINT", "SPRITE", "SPRITZ", "SPROUT", "SPRUCE", "SPUNKY", "SPYING", "SQUADS", "SQUALL", "SQUARE", "SQUASH", "SQUAWK", "SQUEAK", "SQUEAL", "SQUINT", "SQUIRE", "SQUIRM", "SQUIRT", "SQUISH", "STAGED", "STAMEN", "STAMPS", "STANCE", "STANDS", "STANZA", "STARCH", "STARED", "STARRY", "STARTS", "STATED", "STATIC", "STATUS", "STAYED", "STEADY", "STEAMY", "STEELY", "STENCH", "STEREO", "STEWED", "STICKY", "STIGMA", "STINGY", "STINKY", "STITCH", "STOCKS", "STOCKY", "STONED", "STONES", "STOOGE", "STORED", "STORES", "STOREY", "STORMS", "STORMY", "STRAFE", "STRAND", "STRAPS", "STREAK", "STREAM", "STREET", "STRESS", "STRICT", "STRIDE", "STRIFE", "STRIKE", "STRING", "STRIPE", "STRIPS", "STRIVE", "STROBE", "STRODE", "STROKE", "STROLL", "STRONG", "STRUCK", "STRUNG", "STUBBY", "STUCCO", "STUDIO", "STUFFY", "STUMPY", "STUPOR", "STURDY", "STYLUS", "STYMIE", "SUBMIT", "SUBTLY", "SUBURB", "SUBWAY", "SUCCOR", "SUCKED", "SUDDEN", "SUFFER", "SUFFIX", "SUGARY", "SULFUR", "SULLEN", "SULTRY", "SUMMED", "SUMMIT", "SUMMON", "SUNDAE", "SUNDAY", "SUNDER", "SUNDRY", "SUNKEN", "SUNLIT", "SUNSET", "SUNTAN", "SUPERB", "SUPINE", "SUPPLE", "SUPPLY", "SURELY", "SURFER", "SURVEY", "SUTURE", "SWAMPY", "SWANKY", "SWATHE", "SWEETS", "SWITCH", "SWIVEL", "SWORDS", "SYMBOL", "SYNTAX", "SYRUPY", "SYSTEM", "TAGGED", "TAKING", "TALBOT", "TALENT", "TALKED", "TALKER", "TALLOW", "TAMERS", "TAMING", "TAMPON", "TANDEM", "TANKER", "TANNED", "TAPING", "TARGET", "TARIFF", "TARMAC", "TARTLY", "TATTLE", "TATTOO", "TAUGHT", "TAVERN", "TAWDRY", "TAXING", "TEACUP", "TECHIE", "TECHNO", "TEDIUM", "TEENSY", "TENANT", "TENDED", "TENDER", "TENDON", "TENNIS", "TENPIN", "TENSOR", "TENURE", "TERMED", "TERROR", "TETHER", "TETRIS", "THATCH", "THAWED", "THEMED", "THEORY", "THINGS", "THINGY", "THINLY", "THIRDS", "THIRTY", "THORAX", "THORNY", "THOUGH", "THRALL", "THRASH", "THREAD", "THREAT", "THRICE", "THRIFT", "THRILL", "THRIVE", "THROAT", "THROES", "THRONE", "THRONG", "THROWN", "THROWS", "THRUSH", "THRUST", "THUSLY", "THWACK", "THWART", "TICKER", "TIDBIT", "TIDING", "TIERED", "TILING", "TILTED", "TIMBER", "TIMBRE", "TIMELY", "TIMING", "TINDER", "TINGLE", "TINKER", "TINKLE", "TINNED", "TINSEL", "TINTED", "TIPPED", "TIPPET", "TIPTOE", "TIRADE", "TIRING", "TISSUE", "TITLED", "TITLES", "TITTER", "TOASTY", "TOFFEE", "TOKENS", "TOMATO", "TOMBOY", "TOMCAT", "TONGUE", "TONNES", "TONSIL", "TOOLED", "TOOTHY", "TOPICS", "TOPPED", "TOPPER", "TOPPLE", "TORPID", "TORPOR", "TORQUE", "TORRID", "TOSSED", "TOUCHY", "TOUPEE", "TOUSLE", "TOWARD", "TOWELS", "TOWERS", "TOWING", "TOXICS", "TRACED", "TRACER", "TRACTS", "TRADED", "TRADER", "TRAGIC", "TRAINS", "TRAITS", "TRANCE", "TRASHY", "TRAUMA", "TRAVEL", "TREATS", "TREATY", "TREMOR", "TRENCH", "TRENDS", "TRENDY", "TRIAGE", "TRICKY", "TRIPOD", "TRIPPY", "TROOPS", "TROPIC", "TROUGH", "TROUPE", "TROWEL", "TRUANT", "TRUCKS", "TRUDGE", "TRUISM", "TRUSTS", "TRUSTY", "TRUTHS", "TRYING", "TRYOUT", "TUBING", "TUCKED", "TUGGED", "TUMBLE", "TUMOUR", "TUMULT", "TUNDRA", "TUNING", "TUNNEL", "TURBAN", "TURBID", "TURGID", "TURKEY", "TURNED", "TURNIP", "TURRET", "TURTLE", "TUSSLE", "TUTORS", "TUXEDO", "TWENTY", "TWINGE", "TWISTY", "TWITCH", "TYCOON", "TYPING", "TYPIST", "TYRANT", "ULTIMA", "UMPIRE", "UNBEND", "UNBIND", "UNBORN", "UNCLIP", "UNCLOG", "UNCOIL", "UNCORK", "UNCURL", "UNDEAD", "UNDONE", "UNEASE", "UNEASY", "UNEVEN", "UNFAIR", "UNFOLD", "UNFURL", "UNHOLY", "UNHOOK", "UNIONS", "UNISON", "UNITED", "UNJUST", "UNKIND", "UNLESS", "UNLIKE", "UNLOAD", "UNLOCK", "UNMASK", "UNPACK", "UNPAID", "UNPLUG", "UNREAD", "UNREAL", "UNREST", "UNRIPE", "UNROLL", "UNRULY", "UNSAFE", "UNSAID", "UNSEAL", "UNSEEN", "UNSOLD", "UNSUNG", "UNSURE", "UNTIDY", "UNTIED", "UNTOLD", "UNTRUE", "UNUSED", "UNVEIL", "UNWELL", "UNWIND", "UNWISE", "UNWORN", "UNWRAP", "UPDATE", "UPHELD", "UPHILL", "UPHOLD", "UPKEEP", "UPLIFT", "UPLINK", "UPLOAD", "UPROAR", "UPROOT", "UPSHOT", "UPSIDE", "UPTAKE", "UPTICK", "UPTIME", "UPTOWN", "UPWARD", "URCHIN", "URGENT", "URGING", "URINAL", "USEFUL", "UTERUS", "UTMOST", "UTOPIA", "VACANT", "VACUUM", "VALLEY", "VALUED", "VALUES", "VALVES", "VANDAL", "VANISH", "VANITY", "VAPOUR", "VARIED", "VARIES", "VASTLY", "VAULTS", "VECTOR", "VEGGIE", "VEILED", "VEINED", "VELVET", "VENDOR", "VENTED", "VENUES", "VERBAL", "VERIFY", "VERMIN", "VERSED", "VERSES", "VERSUS", "VERTEX", "VEXING", "VIEWED", "VIEWER", "VIGOUR", "VIKING", "VILIFY", "VILLAS", "VIOLET", "VIOLIN", "VIRGIN", "VIRTUE", "VISAGE", "VISION", "VISITS", "VISUAL", "VOICED", "VOIDED", "VOLLEY", "VOODOO", "VORTEX", "VOTING", "VOWELS", "VOYAGE", "VOYEUR", "VULGAR", "WEBBED", "WEBCAM", "WEDDED", "WEEKLY", "WEENIE", "WEEVIL", "WELDED", "WELDER", "WHEEZE", "WHEEZY", "WHIMSY", "WHINER", "WHINNY", "WHISKY", "WHOLLY", "WICKED", "WIDELY", "WIDOWS", "WIENER", "WILDLY", "WILLOW", "WINDED", "WINDOW", "WINERY", "WINGED", "WINNER", "WINTRY", "WIPING", "WIRING", "WISDOM", "WISELY", "WISHED", "WISHES", "WITHER", "WITHIN", "WIZARD", "WOBBLY", "WOEFUL", "WOLVES", "WONDER", "WOODED", "WOODEN", "WOOING", "WOOLEN", "WOOLLY", "WORKED", "WORKER", "WORLDS", "WORSEN", "WORTHY", "WOUNDS", "WRAITH", "WREATH", "WRENCH", "WRISTS", "WRITER", "WRITHE", "WYVERN", "XENONS", "XYLOSE", "YEARLY", "YELLED", "YELLER", "YELLOW", "YIELDS", "YIPPEE", "YONDER", "YOUTHS", "YUPPIE", "ZAGGED", "ZAPPED", "ZAPPER", "ZENITH", "ZEROES", "ZIGGED", "ZIGZAG", "ZINGER", "ZIPPED", "ZIPPER", "ZODIAC", "ZONING", "ZOOMED", "ZYGOTE"];

    public static void Experiment_UnseparatedNonogramClues()
    {
        var dic = getNonogramCombinations();
        var possibilities = dic.Keys.ToArray();

        var rnd = new Random(6);
        while (true)
        {
            var word = _words.PickRandom(rnd);
            var bitmap = new bool[6 * 6];
            for (var i = 0; i < word.Length; i++)
                for (var dot = 0; dot < 6; dot++)
                    bitmap[2 * (i % 3) + (dot / 3) + 6 * (3 * (i / 3) + (dot % 3))] = BrailleBits[word[i] - 'A'][dot];

            //for (var row = 0; row < 6; row++)
            //    Console.WriteLine(Enumerable.Range(0, 6).Select(col => bitmap[col + 6 * row] ? "██" : "░░").JoinString());

            //Console.WriteLine();

            var nonogramClues = "";
            // columns
            for (var col = 0; col < 6; col++)
                nonogramClues += nonogramClue(Enumerable.Range(0, 6).Select(y => bitmap[col + 6 * y]));
            // rows
            for (var row = 0; row < 6; row++)
                nonogramClues += nonogramClue(Enumerable.Range(0, 6).Select(x => bitmap[x + 6 * row]));

            IEnumerable<string[]> recurse(string[] sofar, int[] remaining)
            {
                if (remaining.Length == 0 && sofar.Length == 12)
                {
                    yield return sofar;
                    yield break;
                }
                if (remaining.Length == 0 || sofar.Length == 12)
                    yield break;

                foreach (var poss in possibilities)
                    if (remaining.Take(poss.Length).SequenceEqual(poss.Select(ch => ch - '0')))
                        foreach (var result in recurse(sofar.Insert(sofar.Length, poss), remaining.Remove(0, poss.Length)))
                            yield return result;
            }

            var total = 0;
            foreach (var combinations in recurse([], nonogramClues.Select(ch => ch - '0').ToArray()))
            {
                var allSolutions = solveNonogram(combinations.Select(str => dic[str]).ToArray()).ToArray();
                var numSolutions = allSolutions.Length;
                //if (allSolutions.Length > 0)
                //{
                //    ConsoleUtil.WriteLine($"{combinations.JoinString(" | ")} = {numSolutions}".Color(numSolutions == 0 ? ConsoleColor.DarkRed : numSolutions == 1 ? ConsoleColor.Green : ConsoleColor.Magenta));
                //    ConsoleUtil.WriteLine(outputGrid(allSolutions[0]) + "\n");
                //}
                total += numSolutions;
            }
            //ConsoleUtil.WriteLine($"{word} = {total}".Color(total == 0 ? ConsoleColor.DarkRed : total == 1 ? ConsoleColor.Green : ConsoleColor.Magenta));
            if (total == 1)
            {
                Console.WriteLine(nonogramClues);
                Console.ReadLine();
                Console.WriteLine(word);
                break;
            }
        }
    }

    public static IEnumerable<int?> CreateNonogramClue(this IEnumerable<bool> source)
    {
        var any = false;
        var prevElem = false;
        var curCount = 0;
        foreach (var elem in source)
        {
            if (!any)
                any = true;
            else if (prevElem != elem)
            {
                if (prevElem)
                    yield return curCount.Nullable();
                curCount = 0;
            }
            curCount++;
            prevElem = elem;
        }
        if (any && prevElem)
            yield return curCount.Nullable();
    }

    public static void Experient_Nonogram()
    {
        var rnd = new Random(47);
        var unique = new List<string>();
        var clues = new Dictionary<string, int>();
        foreach (var word in _words)
        {
            var bitmap = new bool[6 * 6];
            for (var i = 0; i < word.Length; i++)
                for (var dot = 0; dot < 6; dot++)
                    bitmap[2 * (i % 3) + (dot / 3) + 6 * (3 * (i / 3) + (dot % 3))] = BrailleBits[word[i] - 'A'][dot];

            var colClues = Enumerable.Range(0, 6).Select(col => Enumerable.Range(0, 6).Select(y => bitmap[col + 6 * y]).CreateNonogramClue().ToArray()).ToArray();
            var rowClues = Enumerable.Range(0, 6).Select(row => Enumerable.Range(0, 6).Select(x => bitmap[x + 6 * row]).CreateNonogramClue().ToArray()).ToArray();

            var num = new int[12];
            for (var shift = 0; shift < 12; shift++)
            {
                var newColClues = shift < 6 ? (int?[][]) [.. colClues.Skip(shift), .. rowClues.Reversed().Take(shift)] : [.. rowClues.Reversed().Skip(shift - 6), .. colClues.Take(shift - 6)];
                var newRowClues = shift < 6 ? (int?[][]) [.. colClues.Take(shift).Reverse(), .. rowClues.Take(6 - shift)] : [.. rowClues.Skip(12 - shift), .. colClues.Reversed().Take(12 - shift)];

                foreach (var cl in newColClues.Concat(newRowClues))
                    clues.IncSafe(cl.JoinString(" "));

                var solutions = Nonogram.Solve(newColClues, newRowClues).Take(2).ToArray();
                num[shift] = solutions.Length;
            }
            if (num.SequenceEqual([1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0]))
                unique.Add(word);
            //ConsoleUtil.WriteLine($"{word} = {num.Select(n => n.ToString().Color(n == 0 ? ConsoleColor.Red : n == 1 ? ConsoleColor.Green : ConsoleColor.Yellow)).JoinColoredString()}", null);
        }
        //Clipboard.SetText(unique.JoinString("\n"));
        Clipboard.SetText(clues.Select(kvp => $"{kvp.Value} × {kvp.Key}").JoinString("\n"));
    }

    public static void TestNonogramSolver()
    {
        var dic = getNonogramCombinations();
        var c = 0;
        foreach (var solution in solveNonogram(new bool?[6 * 6], Ut.NewArray(
            dic["11"], dic["21"], dic["111"], dic["1"], dic["21"], dic["22"],
            dic["31"], dic["12"], dic["3"], dic["11"], dic["12"], dic["1"]
        )))
        {
            Console.WriteLine(solution.Split(6).Select(row => row.Select(c => c ? "██" : "░░").JoinString()).JoinString("\n"));
            Console.WriteLine();
            c++;
        }
        Console.WriteLine($"{c} solutions found.");
    }

    private static ConsoleColoredString outputGrid(bool[] grid) => outputGrid(grid.Select(g => g.Nullable()).ToArray());
    private static ConsoleColoredString outputGrid(bool?[] grid) => Enumerable.Range(0, 6).Select(row =>
                                                                                 Enumerable.Range(0, 6).Select(col => grid[col + 6 * row] == null ? "??".Color(ConsoleColor.Gray) : grid[col + 6 * row].Value ? "██".Color(ConsoleColor.Yellow) : "░░".Color(ConsoleColor.DarkGreen)).JoinColoredString()
        ).JoinColoredString("\n");

    private static IEnumerable<bool[]> solveNonogram(bool[][][] rowColCombinations) => solveNonogram(new bool?[6 * 6], rowColCombinations);
    private static IEnumerable<bool[]> solveNonogram(bool?[] grid, bool[][][] rowColCombinations)
    {
        if (!grid.Contains(null))
        {
            // Check that the grid consists only of valid Braille letters
            //if (Enumerable.Range(0, 6).All(ltrIx => Enumerable.Range(0, 6).Select(dot => grid[2 * (ltrIx % 3) + (dot / 3) + 6 * (3 * (ltrIx / 3) + (dot % 3))].Value).Apply(brailleDots =>
            //    BrailleBits.Any(bb => brailleDots.SequenceEqual(bb)))))
            yield return grid.WhereNotNull().ToArray();
            yield break;
        }

        // Find the row or column with the fewest remaining combinations
        var bestRowCol = rowColCombinations.IndexOf(a => a != null);
        for (var i = bestRowCol + 1; i < rowColCombinations.Length; i++)
            if (rowColCombinations[i] != null && rowColCombinations[i].Length < rowColCombinations[bestRowCol].Length)
                bestRowCol = i;

        foreach (var combination in rowColCombinations[bestRowCol])
        {
            var newRowColCombinations = rowColCombinations.ToArray();
            newRowColCombinations[bestRowCol] = null;

            // Reduce the number of combinations of all perpendicular lines
            if (bestRowCol < 6)
            {
                // We filled in a column ⇒ reduce the number of combinations in the row clues
                for (var row = 0; row < 6; row++)
                    if (rowColCombinations[row + 6] != null)
                        newRowColCombinations[row + 6] = rowColCombinations[row + 6].Where(b => b[bestRowCol] == combination[row]).ToArray();
            }
            else
            {
                // We filled in a row ⇒ reduce the number of combinations in the column clues
                for (var col = 0; col < 6; col++)
                    if (rowColCombinations[col] != null)
                        newRowColCombinations[col] = rowColCombinations[col].Where(b => b[bestRowCol - 6] == combination[col]).ToArray();
            }
            if (newRowColCombinations.Any(n => n != null && n.Length == 0))
                continue;

            // Fill the relevant row/col of ‘grid’
            var newGrid = grid.ToArray();
            for (var i = 0; i < 6; i++)
                newGrid[bestRowCol < 6 ? bestRowCol + 6 * i : i + 6 * (bestRowCol - 6)] = combination[i];

            foreach (var solution in solveNonogram(newGrid, newRowColCombinations))
                yield return solution;
        }
    }
}
