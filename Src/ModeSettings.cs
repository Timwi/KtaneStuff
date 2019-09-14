using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using RT.Util;
using RT.Util.ExtensionMethods;
using RT.Util.Json;

namespace KtaneStuff
{
    static class ModeSettings
    {
        public static void Create()
        {
            var inputRaw = @"Alphabet	1		
Anagrams	1		
Digital Root	1		
Mastermind Simple	1		
Modulo	1		
Word Scramble	1		
Keypad	2		
Letter Keys	2		
The Button	2		
The Festive Jukebox	2		
The Jukebox	2		
The Plunger Button	2		
Wires	2		
Turn The Keys	3		
Caesar Cipher	3		
Combination Lock	3		
Crazy Talk	3		
Double Color	3		
Emoji Math	3		
English Test	3		
Error Codes	3		
Harmony Sequence	3		
LED Grid	3		
Mashematics	3		
Maze	3		
Memory	3		
Morse Code	3		
Party Time	3		
Poetry	3		
Simon Says	3		
Sink	3		
Text Field	3		
The Digit	3		
The Switch	3		
Wire Placement	3		
Backgrounds	4		
British Slang	4		
Calendar	4		
Colored Squares	4		
Complicated Wires	4		
Connection Check	4		
Countdown	4		
Dominoes	4		
Double-Oh	4		
Festive Piano Keys	4		
Foreign Exchange Rates	4		
Identity Parade	4		
Instructions	4		
Listening	4		
Maze Scrambler	4		
Mortal Kombat	4		
Numbers	4		
Password	4		
Piano Keys	4		
Press X	4		
Radiator	4		
Rock-Paper-Scissors-Lizard-Spock	4		
Seven Wires	4		
Simon Scrambles	4		
Skinny Wires	4		
Souvenir	4	0.4	
Subways	4		
Switches	4		
Symbolic Password	4		
The Code	4		
The Jack-O’-Lantern	4		
Timezone	4		
Who’s on First	4		
Wire Sequence	4		
Blind Alley	5		
Color Flash	5		
Color Generator	5		
Complicated Buttons	5		
Cooking	5		
Faulty Backgrounds	5		
Forget Everything	5	0.8	
Forget Me Not	5	0.6	
LED Math	5		
Mega Man 2	5		
Morse War	5		
Round Keypad	5		
Semaphore	5		
Shape Shift	5		
Simon Sounds	5		
Simon States	5		
Simon’s Stages	5	1.2	
Spinning Buttons	5		
Synonyms	5		
T-Words	5		
The Bulb	5		
The Clock	5		
Turn The Key	5		
Uncolored Squares	5		
Word Search	5		
Zoni	5		
Two Bits	6		
Astrology	6		
Big Circle	6		
Binary Puzzle	6		
Bitmaps	6		
Bitwise Operations	6		
Boolean Venn Diagram	6		
Complex Keypad	6		
Crackbox	6		
Divided Squares	6		
European Travel	6		
Extended Password	6		
Flavor Text	6		
Minesweeper	6		
Mystic Square	6		
Orientation Cube	6		
Painting	6		
Pie	6		
Point of Order	6		
Probing	6		
Question Mark	6		
Resistors	6		
Sea Shells	6		
Square Button	6		
Subscribe to Pewdiepie	6		
Symbolic Coordinates	6		
The Number Cipher	6		
The Triangle	6		
Varicolored Squares	7		
Blockbusters	7		
Boggle	7		
Broken Buttons	7		
Broken Guitar Chords	7		
Character Shift	7		
Christmas Presents	7		
Color Math	7		
Colored Switches	7		
Connection Device	7		
Cookie Jars	7		
Creation	7		
Decolored Squares	7		
Digital Cipher	7		
Flashing Lights	7		
Follow the Leader	7		
Forget This	7	0.3	
Free Parking	7		
Gadgetron Vendor	7		
Graffiti Numbers	7		
Lasers	7		
LED Encryption	7		
Mad Memory	7		
Modules Against Humanity	7		
Monsplode, Fight!	7		
Mouse In The Maze	7		
Murder	7		
Number Nimbleness	7		
Perspective Pegs	7		
Pigpen Rotations	7		
Poker	7		
Rhythms	7		
Simon Shrieks	7		
Street Fighter	7		
Tap Code	7		
Tasha Squeals	7		
The Gamepad	7		
The Hangover	7		
The Radio	7		
Unrelated Anagrams	7		
USA Maze	7		
Visual Impairment	7		
Third Base	8		
Adjacent Letters	8		
Adventure Game	8		
Alchemy	8		
Bartending	8		
Bases	8		
Battleship	8		
Benedict Cumberbatch	8		
Binary LEDs	8		
Binary Tree	8		
Blackjack	8		
Blind Maze	8		
Braille	8		
Burglar Alarm	8		
Catchphrase	8		
Challenge & Contact	8		
Chess	8		
Chord Qualities	8		
Color Morse	8		
Colorful Madness	8		
Cryptography	8		
Fast Math	8		
Flags	8		
Friendship	8		
Genetic Sequence	8		
Grid Matching	8		
Grocery Store	8		
Homophones	8		
Hunting	8		
Light Cycle	8		
Logic	8		
Modern Cipher	8		
Nonogram	8		
Passport Control	8		
Plumbing	8		
Regular Crazy Talk	8		
S.E.T.	8		
Safety Safe	8		
Scripting	8		
Simon Samples	8		
Simon’s Star	8		
Skewed Slots	8		
Snooker	8		
Sueet Wall	8		
Symbol Cycle	8		
The Number	8		
The Screw	8		
Westeros	8		
Yahtzee	8		
Timing is Everything	9		
Valves	9		
101 Dalmatians	9		
Algebra	9		
Alphabet Numbers	9		
Boolean Maze	9		
Button Sequence	9		
Colorful Insanity	9		
Discolored Squares	9		
Dr. Doctor	9		
FizzBuzz	9		
Gridlock	9		
Guitar Chords	9		
Hexamaze	9		
Horrible Memory	9		
Human Resources	9		
Logical Buttons	9		
Maintenance	9		
Manometers	9		
Microcontroller	9		
Module Homework	9		
Module Maze	9		
Neutralization	9		
Number Pad	9		
Periodic Table	9		
Playfair Cipher	9		
Purgatory	9		
Retirement	9		
Shikaku	9		
Signals	9		
Simon Screams	9		
Simon Speaks	9		
Sonic the Hedgehog	9		
Superlogic	9		
Synchronization	9		
The Labyrinth	9		
The Stare	9		
The Wire	9		
Web Design	9		
Accumulation	10		
Cheap Checkout	10		
Color Decoding	10		
Coordinates	10		
Curriculum	10		
DetoNATO	10		
Equations	10		
Font Select	10		
Functions	10		
Know Your Way	10		
Laundry	10		
Left and Right	10		
Logic Gates	10		
Maritime Flags	10		
Morse-A-Maze	10		
Morsematics	10		
Only Connect	10		
Perplexing Wires	10		
Polyhedral Maze	10		
Reverse Morse	10		
Rubik’s Cube	10		
Silly Slots	10		
Simon Spins	10		
Skyrim	10		
Sonic & Knuckles	10		
Tangrams	10		
The London Underground	10		
The Stopwatch	10		
The Sun	10		
Tic-Tac-Toe	10		
X-Ray	10		
X01	10		
Zoo	10		
Black Hole	11		
Coffeebucks	11		
Flavor Text EX	11		
Hogwarts	11		
Mafia	11		
Melody Sequencer	11		
Mineseeker	11		
Monsplode Trading Cards	11		
Quiz Buzz	11		
Rubik’s Clock	11		
Splitting The Loot	11		
Ten-Button Color Code	11		
Tennis	11		
The iPhone	11		
The Moon	11		
Waste Management	11		
Wire Spaghetti	11		
3D Maze	12		
Cruel Countdown	12		
Cruel Piano Keys	12		
Game of Life Simple	12		
Hieroglyphics	12		
Ice Cream	12		
IKEA	12		
SYNC-125 [3]	12		
The Hexabutton	12		
The Hypercube	12		
The Swan	12		
Burger Alarm	13		
Graphic Memory	13		
Greek Calculus	13		
Lightspeed	13		
Lion’s Share	13		
Mahjong	13		
Wavetapping	13		
3D Tunnels	14		
Cursed Double-Oh	14		
Krazy Talk	14		
Schlag den Bomb	14		
Simon Sends	14		
Simon Sings	14		
The Jewel Vault	14		
Turtle Robot	14		
Mastermind Cruel	15		
Kudosudoku	16		
Marble Tumble	16		
Pattern Cube	16		
Shapes And Bombs	16		
The Crystal Maze	16		
Dragon Energy	17		
The Sphere	18		
Encrypted Morse	18		
Game of Life Cruel	18		
Quintuples	18		
The Time Keeper	19		
Lombax Cubes	20		
The Stock Market	20		
LEGOs	21		
The Cube	22		
Factory Maze	23		
Tax Returns	24		
Unfair Cipher	25		
Elder Futhark	26		
Micro-Modules	27		
Colored Keys	6		
Four-Card Monte	10		!
Planets	8		!
Stack’em	8		
The Necronomicon	17		
The Troll	9		
The Witness	8		!".Replace("\r", "").Split('\n').Select(line => line.Split('\t')).ToArray();
            var clash = inputRaw.UniquePairs().FirstOrNull(tup => tup.Item1[0] == tup.Item2[0]);
            if (clash != null)
            {
                Console.WriteLine(clash.Value.Item1[0]);
                return;
            }

            var input = inputRaw.ToDictionary(arr => arr[0], arr => (score: double.Parse(arr[1]), multiplier: arr[2].Length == 0 ? null : double.Parse(arr[2]).Nullable()));

            var json = JsonDict.Parse(File.ReadAllText(@"D:\c\KTANE\Public\More\ModeSettings.json"));
            var done = new HashSet<string>();
            foreach (var module in Ktane.GetLiveJson())
            {
                if (module["Type"].GetString() != "Regular")
                    continue;
                if (!input.TryGetValue(module["Name"].GetString(), out var value))
                {
                    Console.WriteLine($"{module["Name"].GetString()} is in Live JSON but not in the input.");
                    continue;
                }

                json["ComponentValues"][module["ModuleID"].GetString()] = value.score;
                if (value.multiplier != null)
                    json["TotalModulesMultiplier"][module["ModuleID"].GetString()] = value.multiplier.Value;
                done.Add(module["ModuleID"].GetString());
            }

            var toDelete = new List<string>();
            foreach (var j in json["ComponentValues"].Keys)
                if (!done.Contains(j))
                {
                    Console.WriteLine($"{j} is in original JSON but not in the input.");
                    toDelete.Add(j);
                }
            foreach (var j in toDelete)
                json["ComponentValues"].Remove(j);

            File.WriteAllText(@"D:\c\KTANE\Public\More\ModeSettings.json", json.ToStringIndented());
            File.WriteAllText(@"C:\Users\Timwi\AppData\LocalLow\Steel Crate Games\Keep Talking and Nobody Explodes\Modsettings\ModeSettings.json", json.ToStringIndented());
        }
    }
}