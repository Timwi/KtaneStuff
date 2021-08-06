using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using RT.Json;
using RT.Util;
using RT.Util.ExtensionMethods;

namespace KtaneStuff
{
    static class CodeStatistics
    {
        public static string DataFile = @"D:\Daten\KTANE\Timwi module data.json";

        public static void Do()
        {
            var lines = @"
Tic Tac Toe/TicTacToe/2016-10-07
    D:\c\KTANE\TicTacToe\Assets\TicTacToeModule.cs

Follow the Leader/FollowtheLeader/2016-10-18
    D:\c\KTANE\FollowtheLeader\Assets\Scripts\ExtensionMethods.cs
    D:\c\KTANE\FollowtheLeader\Assets\Scripts\FollowTheLeaderModule.cs
    D:\c\KTANE\FollowtheLeader\Assets\Scripts\MeshGenerator.cs
    D:\c\KTANE\FollowtheLeader\Assets\Scripts\Pt.cs

Friendship/Friendship/2016-10-24
    D:\c\KTANE\Friendship\Assets\FriendshipModule.cs

The Bulb/TheBulb/2016-10-28
    D:\c\KTANE\TheBulb\Assets\ExtensionMethods.cs
    D:\c\KTANE\TheBulb\Assets\TheBulbModule.cs

Blind Alley/BlindAlley/2016-11-03
    D:\c\KTANE\BlindAlley\Assets\BlindAlleyModule.cs

Rock-Paper-Scissors-Lizard-Spock/RockPaperScissorsLizardSpock/2016-11-12
    D:\c\KTANE\RockPaperScissorsLizardSpock\Assets\Extensions.cs
    D:\c\KTANE\RockPaperScissorsLizardSpock\Assets\RockPaperScissorsLizardSpockModule.cs

Hexamaze/Hexamaze/2016-11-21
    D:\c\KTANE\Hexamaze\Assets\GeneratedMaze.cs
    D:\c\KTANE\Hexamaze\Assets\Hex.cs
    D:\c\KTANE\Hexamaze\Assets\HexamazeModule.cs
    D:\c\KTANE\Hexamaze\Assets\HexamazeRuleGenerator.cs
    D:\c\KTANE\Hexamaze\Assets\Marking.cs

Bitmaps/Bitmaps/2016-11-23
    D:\c\KTANE\Bitmaps\Assets\BitmapsModule.cs

Colored Squares/ColoredSquares/2016-11-24
    D:\c\KTANE\ColoredSquares\Assets\Scripts\ColoredSquaresModule.cs
    D:\c\KTANE\ColoredSquares\Assets\Scripts\ColoredSquaresModuleBase.cs
    D:\c\KTANE\ColoredSquares\Assets\Scripts\ColoredSquaresScaffold.cs
    D:\c\KTANE\ColoredSquares\Assets\Scripts\Enums.cs
    D:\c\KTANE\ColoredSquares\Assets\Scripts\Ut.cs

Adjacent Letters/AdjacentLetters/2016-11-25
    D:\c\KTANE\AdjacentLetters\Assets\AdjacentLettersModule.cs

Souvenir/Souvenir/2016-12-02
    D:\c\KTANE\Souvenir\Assets\AbandonModuleException.cs
    D:\c\KTANE\Souvenir\Assets\AnswerGenerator.cs
    D:\c\KTANE\Souvenir\Assets\Config.cs
    D:\c\KTANE\Souvenir\Assets\Enums.cs
    D:\c\KTANE\Souvenir\Assets\QandA.cs
    D:\c\KTANE\Souvenir\Assets\Question.cs
    D:\c\KTANE\Souvenir\Assets\QuestionBatch.cs
    D:\c\KTANE\Souvenir\Assets\SouvenirModule.cs
    D:\c\KTANE\Souvenir\Assets\SouvenirQuestionAttribute.cs
    D:\c\KTANE\Souvenir\Assets\Reflection\FieldInfo.cs
    D:\c\KTANE\Souvenir\Assets\Reflection\MethodInfo.cs
    D:\c\KTANE\Souvenir\Assets\Reflection\PropertyInfo.cs

Word Search/WordSearch/2016-12-07
    D:\c\KTANE\WordSearch\Assets\LetterState.cs
    D:\c\KTANE\WordSearch\Assets\WordDirection.cs
    D:\c\KTANE\WordSearch\Assets\WordSearchModule.cs

Simon Screams/SimonScreams/2016-12-20
    D:\c\KTANE\SimonScreams\Assets\Criterion.cs
    D:\c\KTANE\SimonScreams\Assets\SimonColor.cs
    D:\c\KTANE\SimonScreams\Assets\SimonScreamsModule.cs

Battleship/Battleship/2017-01-03
    D:\c\KTANE\Battleship\Assets\BattleshipModule.cs

Wire Placement/WirePlacement/2017-01-06
    D:\c\KTANE\WirePlacement\Assets\Pt.cs
    D:\c\KTANE\WirePlacement\Assets\WireInfo.cs
    D:\c\KTANE\WirePlacement\Assets\WirePlacementModule.cs

Double-Oh/DoubleOh/2017-01-15
    D:\c\KTANE\DoubleOh\Assets\ButtonFunction.cs
    D:\c\KTANE\DoubleOh\Assets\DoubleOhModule.cs

Coordinates/Coordinates/2017-01-20
    D:\c\KTANE\Coordinates\Assets\Clue.cs
    D:\c\KTANE\Coordinates\Assets\CoordinatesModule.cs

Light Cycle/LightCycle/2017-01-23
    D:\c\KTANE\LightCycle\Assets\LightCycleModule.cs

Only Connect/OnlyConnect/2017-03-17
    D:\c\KTANE\OnlyConnect\Assets\OnlyConnectModule.cs
    D:\c\KTANE\OnlyConnect\Assets\Round2ButtonInfo.cs

Rubik’s Cube/RubiksCube/2017-04-30
    D:\c\KTANE\RubiksCube\Assets\CubeletInfo.cs
    D:\c\KTANE\RubiksCube\Assets\RubiksCubeModule.cs

The Clock/TheClock/2017-05-11
    D:\c\KTANE\TheClock\Assets\HandStyle.cs
    D:\c\KTANE\TheClock\Assets\NumeralStyle.cs
    D:\c\KTANE\TheClock\Assets\TheClockModule.cs

Zoo/Zoo/2017-06-13
    D:\c\KTANE\Zoo\Assets\Enums.cs
    D:\c\KTANE\Zoo\Assets\ZooModule.cs

Point of Order/PointofOrder/2017-06-25
    D:\c\KTANE\PointofOrder\Assets\PlayingCard.cs
    D:\c\KTANE\PointofOrder\Assets\PointOfOrderModule.cs

Yahtzee/Yahtzee/2017-07-02
    D:\c\KTANE\Yahtzee\Assets\YahtzeeModule.cs

X-Ray/XRay/2017-07-14
    D:\c\KTANE\XRay\Assets\SymbolInfo.cs
    D:\c\KTANE\XRay\Assets\XRayModule.cs

Gridlock/Gridlock/2017-08-11
    D:\c\KTANE\Gridlock\Assets\GridlockModule.cs

Colored Switches/ColoredSwitches/2017-09-03
    D:\c\KTANE\ColoredSwitches\Assets\ColoredSwitchesModule.cs
    D:\c\KTANE\ColoredSwitches\Assets\SwitchColor.cs

Perplexing Wires/PerplexingWires/2017-09-06
    D:\c\KTANE\PerplexingWires\Assets\PerplexingWiresModule.cs

S.E.T./SET/2017-09-23
    D:\c\KTANE\SET\Assets\SetModule.cs

Symbol Cycle/SymbolCycle/2017-10-05
    D:\c\KTANE\SymbolCycle\Assets\SymbolCycleModule.cs

Braille/Braille/2017-10-31
    D:\c\KTANE\Braille\Assets\BrailleModule.cs

Mafia/Mafia/2017-11-04
    D:\c\KTANE\Mafia\Assets\MafiaModule.cs
    D:\c\KTANE\Mafia\Assets\Pair.cs
    D:\c\KTANE\Mafia\Assets\Suspect.cs
    D:\c\KTANE\Mafia\Assets\SuspectName.cs

Polyhedral Maze/PolyhedralMaze/2018-01-01
    D:\c\KTANE\PolyhedralMaze\Assets\PolyhedralMazeModule.cs

Human Resources/HumanResources/2018-02-26
    D:\c\KTANE\HumanResources\Assets\FindPersonResult.cs
    D:\c\KTANE\HumanResources\Assets\HumanResourcesModule.cs
    D:\c\KTANE\HumanResources\Assets\Person.cs
    D:\c\KTANE\HumanResources\Assets\TextState.cs

Superlogic/Superlogic/2018-04-17
    D:\c\KTANE\Superlogic\Assets\Expression.cs
    D:\c\KTANE\Superlogic\Assets\SuperlogicModule.cs

Marble Tumble/MarbleTumble/2018-05-07
    D:\c\KTANE\MarbleTumble\Assets\DijNode.cs
    D:\c\KTANE\MarbleTumble\Assets\MarbleTumbleModule.cs

Simon Sends/SimonSends/2018-05-29
    D:\c\KTANE\SimonSends\Assets\SimonSendsModule.cs

Simon Sings/SimonSings/2018-05-29
    D:\c\KTANE\SimonSings\Assets\SimonSingsModule.cs

Simon Shrieks/SimonShrieks/2018-06-01
    D:\c\KTANE\SimonShrieks\Assets\SimonShrieksModule.cs

Black Hole/BlackHole/2018-06-22
    D:\c\KTANE\BlackHole\Assets\BlackHoleModule.cs

Maritime Flags/MaritimeFlags/2018-07-10
    D:\c\KTANE\MaritimeModules\Assets\Callsign.cs
    D:\c\KTANE\MaritimeModules\Assets\ColorInfo.cs
    D:\c\KTANE\MaritimeModules\Assets\Flag.cs
    D:\c\KTANE\MaritimeModules\Assets\FlagDesign.cs
    D:\c\KTANE\MaritimeModules\Assets\MaritimeBase.cs
    D:\c\KTANE\MaritimeModules\Assets\MaritimeFlagsModule.cs

Pattern Cube/PatternCube/2018-07-28
    D:\c\KTANE\PatternCube\Assets\PatternCubeModule.cs

Uncolored Squares/UncoloredSquares/2018-08-08
    D:\c\KTANE\ColoredSquares\Assets\Scripts\UncoloredSquaresModule.cs

Tennis/Tennis/2018-09-08
    D:\c\KTANE\Tennis\Assets\Scores.cs
    D:\c\KTANE\Tennis\Assets\TennisModule.cs
    D:\c\KTANE\Tennis\Assets\Tournament.cs

Lion’s Share/LionsShare/2018-10-08
    D:\c\KTANE\LionsShare\Assets\LionsShareModule.cs

Divided Squares/DividedSquares/2018-10-30
    D:\c\KTANE\DividedSquares\Assets\DividedSquaresModule.cs

101 Dalmatians/101Dalmatians/2018-11-12
    D:\c\KTANE\101Dalmatians\Assets\OneHundredAndOneDalmatiansModule.cs

Mahjong/Mahjong/2018-11-20
    D:\c\KTANE\Mahjong\Assets\LayoutInfo.cs
    D:\c\KTANE\Mahjong\Assets\MahjongModule.cs
    D:\c\KTANE\Mahjong\Assets\TileInfo.cs
    D:\c\KTANE\Mahjong\Assets\TilePair.cs

Kudosudoku/Kudosudoku/2018-11-21
    D:\c\KTANE\Kudosudoku\Assets\KudosudokuModule.cs

Simon Spins/SimonSpins/2018-12-09
    D:\c\KTANE\SimonSpins\Assets\AngleRegions.cs
    D:\c\KTANE\SimonSpins\Assets\CoroutineInfo.cs
    D:\c\KTANE\SimonSpins\Assets\SimonSpinsModule.cs

Binary Puzzle/BinaryPuzzle/2018-12-27
    D:\c\KTANE\BinaryPuzzle\Assets\BinaryPuzzleModule.cs

Broken Guitar Chords/BrokenGuitarChords/2018-12-28
    D:\c\KTANE\BrokenGuitarChords\Assets\BrokenGuitarChordsModule.cs

Hogwarts/Hogwarts/2018-12-29
    D:\c\KTANE\Hogwarts\Assets\HogwartsModule.cs

Regular Crazy Talk/RegularCrazyTalk/2018-12-29
    D:\c\KTANE\RegularCrazyTalk\Assets\PhraseInfo.cs
    D:\c\KTANE\RegularCrazyTalk\Assets\RegularCrazyTalkModule.cs

Simon Speaks/SimonSpeaks/2018-12-30
    D:\c\KTANE\SimonSpeaks\Assets\SimonSpeaksModule.cs

Discolored Squares/DiscoloredSquares/2018-12-31
    D:\c\KTANE\ColoredSquares\Assets\Scripts\DiscoloredSquaresModule.cs

Decolored Squares/DecoloredSquares/2019-01-09
    D:\c\KTANE\ColoredSquares\Assets\Scripts\DecoloredSquaresModule.cs

The Hypercube/TheHypercube/2019-02-26
    D:\c\KTANE\TheHypercube\Assets\Point4D.cs
    D:\c\KTANE\TheHypercube\Assets\TheHypercubeModule.cs

Odd One Out/OddOneOut/2019-04-08
    D:\c\KTANE\OddOneOut\Assets\OddOneOutModule.cs
    D:\c\KTANE\OddOneOut\Assets\StageInfo.cs

The Ultracube/TheUltracube/2019-06-23
    D:\c\KTANE\TheUltracube\Assets\Point5D.cs
    D:\c\KTANE\TheUltracube\Assets\TheUltracubeModule.cs

Corners/Corners/2019-11-03
    D:\c\KTANE\Corners\Assets\CornersModule.cs

Color Braille/ColorBraille/2020-05-16
    D:\c\KTANE\ColorBraille\Assets\ColorBrailleModule.cs
    D:\c\KTANE\ColorBraille\Assets\Mangling.cs

DACH Maze/DACHMaze/2020-06-02
    D:\c\KTANE\WorldMazes\Assets\MoveResult.cs
    D:\c\KTANE\WorldMazes\Assets\Mazes\DACHMaze.cs

Puzzword/Puzzword/2020-06-20
    D:\c\KTANE\Puzzword\Assets\Scripts\Clue.cs
    D:\c\KTANE\Puzzword\Assets\Scripts\ClueType.cs
    D:\c\KTANE\Puzzword\Assets\Scripts\ConstantDisplay.cs
    D:\c\KTANE\Puzzword\Assets\Scripts\LayoutAttribute.cs
    D:\c\KTANE\Puzzword\Assets\Scripts\LayoutType.cs
    D:\c\KTANE\Puzzword\Assets\Scripts\PuzzwordModule.cs
    D:\c\KTANE\Puzzword\Assets\Scripts\ScreenType.cs
    D:\c\KTANE\Puzzword\Assets\Scripts\WideScreenPosition.cs
    D:\c\KTANE\Puzzword\Assets\Scripts\PuzzleSolver\BetweenConstraint.cs
    D:\c\KTANE\Puzzword\Assets\Scripts\PuzzleSolver\HasSumConstraint.cs
    D:\c\KTANE\Puzzword\Assets\Scripts\PuzzleSolver\HasXnorConstraint.cs
    D:\c\KTANE\Puzzword\Assets\Scripts\PuzzleSolver\HasXorConstraint.cs
    D:\c\KTANE\Puzzword\Assets\Scripts\PuzzleSolver\LeftOfNumberConstraint.cs
    D:\c\KTANE\Puzzword\Assets\Scripts\PuzzleSolver\MinMaxConstraint.cs
    D:\c\KTANE\Puzzword\Assets\Scripts\PuzzleSolver\MinMaxMode.cs
    D:\c\KTANE\Puzzword\Assets\Scripts\PuzzleSolver\NotMinMaxConstraint.cs
    D:\c\KTANE\Puzzword\Assets\Scripts\PuzzleSolver\NotPresentConstraint.cs
    D:\c\KTANE\Puzzword\Assets\Scripts\PuzzleSolver\RelativeToPositionConstraint.cs

Simon Shouts/SimonShouts/2021-05-19
    D:\c\KTANE\SimonShouts\Assets\Movement.cs
    D:\c\KTANE\SimonShouts\Assets\SimonShoutsModule.cs

Maritime Semaphore/MaritimeSemaphore/2021-05-20
    D:\c\KTANE\MaritimeModules\Assets\MaritimeSemaphoreModule.cs

Wire Association/WireAssociation/2021-06-07
    D:\c\KTANE\WireAssociation\Assets\Scripts\WireAssociationModule.cs
    D:\c\KTANE\WireAssociation\Assets\Scripts\WireMeshes.cs

Variety/Variety/2021-07-14
    D:\c\KTANE\Variety\Assets\General\ButtonMoveType.cs
    D:\c\KTANE\Variety\Assets\General\DummyPrefab.cs
    D:\c\KTANE\Variety\Assets\General\Item.cs
    D:\c\KTANE\Variety\Assets\General\ItemFactory.cs
    D:\c\KTANE\Variety\Assets\General\ItemSelectable.cs
    D:\c\KTANE\Variety\Assets\General\VarietyModule.cs
    D:\c\KTANE\Variety\Assets\Items\BrailleDisplay\BrailleDisplay.cs
    D:\c\KTANE\Variety\Assets\Items\BrailleDisplay\BrailleDisplayFactory.cs
    D:\c\KTANE\Variety\Assets\Items\BrailleDisplay\BrailleDisplayPrefab.cs
    D:\c\KTANE\Variety\Assets\Items\Button\Button.cs
    D:\c\KTANE\Variety\Assets\Items\Button\ButtonColor.cs
    D:\c\KTANE\Variety\Assets\Items\Button\ButtonFactory.cs
    D:\c\KTANE\Variety\Assets\Items\Button\ButtonPrefab.cs
    D:\c\KTANE\Variety\Assets\Items\ColoredKeypad\ColoredKeypad.cs
    D:\c\KTANE\Variety\Assets\Items\ColoredKeypad\ColoredKeypadColor.cs
    D:\c\KTANE\Variety\Assets\Items\ColoredKeypad\ColoredKeypadFactory.cs
    D:\c\KTANE\Variety\Assets\Items\ColoredKeypad\ColoredKeypadPrefab.cs
    D:\c\KTANE\Variety\Assets\Items\ColoredKeypad\ColoredKeypadSize.cs
    D:\c\KTANE\Variety\Assets\Items\DigitDisplay\DigitDisplay.cs
    D:\c\KTANE\Variety\Assets\Items\DigitDisplay\DigitDisplayFactory.cs
    D:\c\KTANE\Variety\Assets\Items\DigitDisplay\DigitDisplayPrefab.cs
    D:\c\KTANE\Variety\Assets\Items\Key\Key.cs
    D:\c\KTANE\Variety\Assets\Items\Key\KeyFactory.cs
    D:\c\KTANE\Variety\Assets\Items\Key\KeyPrefab.cs
    D:\c\KTANE\Variety\Assets\Items\Keypad\Keypad.cs
    D:\c\KTANE\Variety\Assets\Items\Keypad\KeypadFactory.cs
    D:\c\KTANE\Variety\Assets\Items\Keypad\KeypadPrefab.cs
    D:\c\KTANE\Variety\Assets\Items\Keypad\KeypadSize.cs
    D:\c\KTANE\Variety\Assets\Items\Knob\Knob.cs
    D:\c\KTANE\Variety\Assets\Items\Knob\KnobFactory.cs
    D:\c\KTANE\Variety\Assets\Items\Knob\KnobPrefab.cs
    D:\c\KTANE\Variety\Assets\Items\Led\Led.cs
    D:\c\KTANE\Variety\Assets\Items\Led\LedColor.cs
    D:\c\KTANE\Variety\Assets\Items\Led\LedCyclingState.cs
    D:\c\KTANE\Variety\Assets\Items\Led\LedFactory.cs
    D:\c\KTANE\Variety\Assets\Items\Led\LedPrefab.cs
    D:\c\KTANE\Variety\Assets\Items\LetterDisplay\LetterDisplay.cs
    D:\c\KTANE\Variety\Assets\Items\LetterDisplay\LetterDisplayFactory.cs
    D:\c\KTANE\Variety\Assets\Items\LetterDisplay\LetterDisplayPrefab.cs
    D:\c\KTANE\Variety\Assets\Items\Maze\Maze.cs
    D:\c\KTANE\Variety\Assets\Items\Maze\MazeFactory.cs
    D:\c\KTANE\Variety\Assets\Items\Maze\MazeLayout.cs
    D:\c\KTANE\Variety\Assets\Items\Maze\MazePrefab.cs
    D:\c\KTANE\Variety\Assets\Items\Slider\Slider.cs
    D:\c\KTANE\Variety\Assets\Items\Slider\SliderFactory.cs
    D:\c\KTANE\Variety\Assets\Items\Slider\SliderOrientation.cs
    D:\c\KTANE\Variety\Assets\Items\Slider\SliderPrefab.cs
    D:\c\KTANE\Variety\Assets\Items\Switch\Switch.cs
    D:\c\KTANE\Variety\Assets\Items\Switch\SwitchColor.cs
    D:\c\KTANE\Variety\Assets\Items\Switch\SwitchFactory.cs
    D:\c\KTANE\Variety\Assets\Items\Switch\SwitchPrefab.cs
    D:\c\KTANE\Variety\Assets\Items\Wire\Wire.cs
    D:\c\KTANE\Variety\Assets\Items\Wire\WireColor.cs
    D:\c\KTANE\Variety\Assets\Items\Wire\WireFactory.cs
    D:\c\KTANE\Variety\Assets\Items\Wire\WirePrefab.cs
".UnifyLineEndings().Split("\r\n");

            var data = new JsonList();
            (string moduleName, string moduleId, string published, List<string> files) curModule = default;
            int fairLength(string content) => Regex.Replace(content, @"\s+", " ").Length;
            int fairFileLength(string file) => fairLength(File.ReadAllText(file));

            void commit()
            {
                if (curModule.moduleName == null)
                    return;
                var dic = new JsonDict
                {
                    ["name"] = curModule.moduleName,
                    ["id"] = curModule.moduleId,
                    ["published"] = curModule.published,
                    ["cs-files"] = curModule.files.ToJsonList(),
                    ["cs-size"] = curModule.files.Sum(f => fairFileLength(f)),
                    ["ext-cs-size"] = 0,
                    ["js-size"] = Regex.Matches(File.ReadAllText($@"D:\c\KTANE\Public\HTML\{curModule.moduleName.Replace("’", "'")}.html"), @"<script>.*?</script>", RegexOptions.Singleline).Cast<Match>().Sum(m => fairLength(m.Value))
                };
                var extPath = $@"D:\c\KTANE\KtaneStuff\Src\{curModule.moduleId}.cs";
                if (File.Exists(extPath))
                {
                    dic["ext-cs-files"] = new JsonList { extPath };
                    dic["ext-cs-size"] = fairFileLength(extPath);
                }
                data.Add(dic);
            }

            for (var lineIx = 0; lineIx < lines.Length; lineIx++)
            {
                var line = lines[lineIx];
                if (string.IsNullOrWhiteSpace(line))
                    continue;
                if (line.StartsWith(" "))
                    curModule.files.Add(line.Trim());
                else
                {
                    commit();
                    var spl = line.Split('/');
                    curModule = (spl[0], spl[1], spl[2], new List<string>());
                }
            }
            commit();

            //File.WriteAllText(DataFile, data.ToStringIndented());
            Clipboard.SetText(data
                .Where(entry => entry["name"].GetString() != "Souvenir")
                .Select(entry => $"{entry["name"]}\t{entry["cs-size"]}\t{entry["ext-cs-size"]}\t{entry["js-size"]}\t{entry["cs-size"].GetLong() + entry["ext-cs-size"].GetLong() + entry["js-size"].GetLong()}\t{entry["published"]}")
                .JoinString("\n"));
        }
    }
}