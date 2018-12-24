using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using RT.Util.ExtensionMethods;
using RT.Util.Text;

namespace KtaneStuff
{
    static class Hogwarts
    {
        public static unsafe void CreateTextures()
        {
            using (var background = new Bitmap(@"D:\c\KTANE\KtaneStuff\DataFiles\Hogwarts\ComponentBackground.png"))
            {
                foreach (var (red, green, blue, name) in new[] {
                    (0.59607846, 0.34509805, 0.36862746, "Gryffindor"),
                    (0.32941177, 0.38039216, 0.5529412, "Ravenclaw"),
                    (0.3254902, 0.5058824, 0.30980393, "Slytherin"),
                    (0.84705883, 0.7294118, 0.30980393, "Hufflepuff")
                })
                {
                    var color = Color.FromArgb((int) (red * 255), (int) (green * 255), (int) (blue * 255));
                    using (var newBmp = new Bitmap(background.Width, background.Height, PixelFormat.Format32bppArgb))
                    {
                        var reader = background.LockBits(new Rectangle(0, 0, background.Width, background.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
                        var writer = newBmp.LockBits(new Rectangle(0, 0, background.Width, background.Height), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
                        for (var y = 0; y < background.Height; y++)
                        {
                            var r = (byte*) (reader.Scan0 + y * reader.Stride);
                            var w = (byte*) (writer.Scan0 + y * writer.Stride);
                            for (int x = 0; x < background.Width; x++)
                            {
                                w[4 * x] = (byte) (r[4 * x] * blue);
                                w[4 * x + 1] = (byte) (r[4 * x + 1] * green);
                                w[4 * x + 2] = (byte) (r[4 * x + 2] * red);
                                w[4 * x + 3] = r[(4 * x) + 3];
                            }
                        }
                        background.UnlockBits(reader);
                        newBmp.UnlockBits(writer);
                        using (var g = Graphics.FromImage(newBmp))
                        {
                            var banner = new Bitmap($@"D:\c\KTANE\KtaneStuff\DataFiles\Hogwarts\{name} Banner.png");
                            g.DrawImage(banner, new Rectangle(473 - 262 / 2, 83, 262, 814));
                            var parchment = new Bitmap($@"D:\c\KTANE\KtaneStuff\DataFiles\Hogwarts\Parchment.png");
                            g.DrawImage(parchment, new Rectangle(473 - 838 / 2, 442, 838, 298));
                        }
                        newBmp.Save($@"D:\c\KTANE\Hogwarts\Assets\Textures\{name} background.png");
                    }
                }
            }
        }

        public static void Funalysis()
        {
            var modules = new[] { "101 Dalmatians", "3D Maze", "3D Tunnels", "Accumulation", "Adjacent Letters", "Adventure Game", "Algebra", "Alphabet", "Alphabet Numbers", "Anagrams", "Astrology", "Backgrounds", "Bases", "Battleship", "Benedict Cumberbatch", "Big Circle", "Binary LEDs", "Binary Tree", "Bitmaps", "Bitwise Operations", "Black Hole", "Blackjack", "Blind Alley", "Blind Maze", "Blockbusters", "Boggle", "Boolean Venn Diagram", "Boolean Maze", "Braille", "British Slang", "Broken Buttons", "The Bulb", "Burglar Alarm", "The Button", "Button Sequence", "Caesar Cipher", "Calendar", "Catchphrase", "Challenge & Contact", "Character Shift", "Cheap Checkout", "Chess", "Chord Qualities", "Christmas Presents", "The Clock", "The Code", "Coffeebucks", "Color Decoding", "Colored Squares", "Colored Switches", "Color Flash", "Colorful Madness", "Color Generator", "Color Math", "Color Morse", "Combination Lock", "Complex Keypad", "Complicated Buttons", "Complicated Wires", "Connection Check", "Connection Device", "Cooking", "Coordinates", "Countdown", "Crackbox", "Crazy Talk", "Creation", "Cruel Countdown", "Cruel Piano Keys", "Cryptography", "The Crystal Maze", "The Cube", "Curriculum", "Cursed Double-Oh", "The Digit", "Digital Root", "Divided Squares", "Double Color", "Double-Oh", "Dragon Energy", "Dr. Doctor", "Emoji Math", "Encrypted Morse", "English Test", "Equations", "Error Codes", "European Travel", "Extended Password", "Fast Math", "Faulty Backgrounds", "The Festive Jukebox", "Festive Piano Keys", "FizzBuzz", "Flags", "Flashing Lights", "Follow the Leader", "Font Select", "Foreign Exchange Rates", "Forget Everything", "Forget Me Not", "Friendship", "Functions", "Game of Life Simple", "Game of Life Cruel", "The Gamepad", "Graffiti Numbers", "Greek Calculus", "Gridlock", "Grid Matching", "Guitar Chords", "Hexamaze", "Hieroglyphics", "Horrible Memory", "Human Resources", "Hunting", "Ice Cream", "Identity Parade", "IKEA", "Instructions", "The iPhone", "The Jack-O’-Lantern", "The Jewel Vault", "The Jukebox", "Keypad", "Know Your Way", "Kudosudoku", "The Labyrinth", "Lasers", "Laundry", "LED Encryption", "LED Grid", "LEGOs", "Letter Keys", "Light Cycle", "Lightspeed", "Lion’s Share", "Listening", "Logic", "Logical Buttons", "Logic Gates", "The London Underground", "Mafia", "Mahjong", "Maintenance", "Manometers", "Marble Tumble", "Maritime Flags", "Mashematics", "Mastermind Simple", "Mastermind Cruel", "Maze", "Maze Scrambler", "Memory", "Microcontroller", "Mineseeker", "Minesweeper", "Modern Cipher", "Module Homework", "Modules Against Humanity", "Modulo", "Monsplode, Fight!", "Monsplode Trading Cards", "The Moon", "Morse-A-Maze", "Morse Code", "Morsematics", "Morse War", "Mortal Kombat", "Mouse In The Maze", "Murder", "Mystic Square", "Neutralization", "Nonogram", "The Number", "The Number Cipher", "Number Nimbleness", "Number Pad", "Only Connect", "Orientation Cube", "Painting", "Party Time", "Password", "Pattern Cube", "Periodic Table", "Perplexing Wires", "Perspective Pegs", "Piano Keys", "Pie", "Playfair Cipher", "Plumbing", "The Plunger Button", "Poetry", "Point of Order", "Poker", "Polyhedral Maze", "Press X", "Probing", "Quintuples", "Radiator", "The Radio", "Resistors", "Retirement", "Reverse Morse", "Rhythms", "Rock-Paper-Scissors-L.-Sp.", "Round Keypad", "Rubik’s Clock", "Rubik’s Cube", "Safety Safe", "Schlag den Bomb", "The Screw", "Scripting", "Sea Shells", "Semaphore", "S.E.T.", "Shape Shift", "Shikaku", "Signals", "Silly Slots", "Simon Samples", "Simon Says", "Simon Screams", "Simon Sends", "Simon Shrieks", "Simon Sings", "Simon Spins", "Simon’s Star", "Simon States", "Sink", "Skewed Slots", "Skyrim", "Snooker", "Sonic & Knuckles", "Sonic the Hedgehog", "Souvenir", "The Sphere", "Spinning Buttons", "Splitting The Loot", "Square Button", "The Stock Market", "The Stopwatch", "Street Fighter", "Subways", "Sueet Wall", "The Sun", "Superlogic", "The Swan", "The Switch", "Switches", "Symbol Cycle", "Symbolic Coordinates", "Symbolic Password", "Synchronization", "Synonyms", "Tangrams", "Tap Code", "Tax Returns", "Ten-Button Color Code", "Tennis", "Text Field", "Third Base", "Tic-Tac-Toe", "The Time Keeper", "Timezone", "The Triangle", "Turn The Key", "Turn The Keys", "Turtle Robot", "Two Bits", "T-Words", "Uncolored Squares", "USA Maze", "Valves", "Visual Impairment", "Waste Management", "Web Design", "Who’s on First", "The Wire", "Wire Placement", "Wires", "Wire Sequence", "Wire Spaghetti", "Word Scramble", "Word Search", "X01", "X-Ray", "Yahtzee", "Zoo" };
            var founders = new[] { "Godric Gryffindor", "Rowena Ravenclaw", "Salazar Slytherin", "Helga Hufflepuff" };
            var scores = modules.Select(module =>
                founders.Select(founder =>
                    founder.ToUpperInvariant().Where(ch => char.IsLetter(ch)).GroupBy(ch => ch).Sum(gr => gr.Count() * module.Count(ch => char.ToUpperInvariant(ch) == gr.Key))).ToArray()).ToArray();
            var totals = scores.Select(row => row.Max() - row.Min()).ToArray();
            var indexes = Enumerable.Range(0, modules.Length).OrderByDescending(ix => totals[ix]).ToArray();

            var tt = new TextTable { ColumnSpacing = 2 };
            for (int r = 0; r < indexes.Length; r++)
            {
                tt.SetCell(0, r + 1, modules[indexes[r]].Color(ConsoleColor.White));
                tt.SetCell(5, r + 1, totals[indexes[r]].ToString().Color(ConsoleColor.Yellow));
                for (int c = 0; c < founders.Length; c++)
                    tt.SetCell(c + 1, r + 1, scores[indexes[r]][c].ToString().Color(ConsoleColor.Green));
            }
            for (int c = 0; c < founders.Length; c++)
                tt.SetCell(c + 1, 0, founders[c].Color(ConsoleColor.Cyan));

            tt.WriteToConsole();
        }
    }
}