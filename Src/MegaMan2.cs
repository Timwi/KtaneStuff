using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace KtaneStuff
{
    static class MegaMan2
    {
        private static string[] _robotMasters = new[]
        {
            null,
            "Cold Man",
            "Magma Man",
            "Dust Man",
            "Sword Man",
            "Splash Woman",
            "Ice Man",
            "Quick Man",
            "Hard Man",
            "Pharaoh Man",
            "Charge Man",
            "Pirate Man",
            null,
            "Pump Man",
            "Galaxy Man",
            "Grenade Man",
            "Snake Man",
            "Burst Man",
            "Cut Man",
            "Air Man",
            "Magnet Man",
            "Toad Man",
            "Gyro Man",
            "Tomahawk Man",
            "Wood Man",
            "Strike Man",
            "Blade Man",
            "Aqua Man",
            "Shade Man",
            "Flash Man",
            "Flame Man",
            "Concrete Man",
            "Metal Man",
            "Needle Man",
            "Wave Man",
            "Knight Man",
            "Slash Man",
            "Shadow Man",
            "Sheep Man",
            "Ground Man",
            "Wind Man",
            "Fire Man",
            "Stone Man",
            "Tengu Man",
            null,
            null,
            null,
            "Bright Man",
            "Centaur Man",
            "Cloud Man",
            "Frost Man",
            "Dynamo Man",
            "Chill Man",
            "Turbo Man",
            "Napalm Man",
            "Jewel Man",
            "Drill Man",
            "Freeze Man",
            "Blizzard Man",
            "Gravity Man",
            "Junk Man",
            "Clown Man",
            "Hornet Man",
            "Skull Man",
            "Solar Man",
            "Commando Man",
            "Yamato Man",
            "Dive Man",
            "Search Man",
            "Gemini Man",
            "Bubble Man",
            "Guts Man",
            "Tornado Man",
            "Astro Man",
            "Plug Man",
            "Elec Man",
            "Crystal Man",
            "Nitro Man",
            null,
            "Burner Man",
            "Spark Man",
            "Spring Man",
            "Plant Man",
            "Star Man",
            "Ring Man",
            "Top Man",
            "Crash Man",
            "Bomb Man",
            "Heat Man",
            "Magic Man",
            null
        };

        public static void GenerateRobotMasters()
        {
            using (var original = new Bitmap(@"D:\c\KTANE\KtaneStuff\DataFiles\MegaMan2\OriginalGraphics.png"))
            {
                for (int x = 0; x < 13; x++)
                {
                    for (int y = 0; y < 7; y++)
                    {
                        var name = _robotMasters[x + 13 * y];
                        if (name == null)
                            continue;
                        var rect = new Rectangle(128 * x + 160, 128 * y + 106, 64, 64);
                        using (var newBitmap = new Bitmap(@"D:\c\KTANE\KtaneStuff\DataFiles\MegaMan2\Air_Man.png"))
                        using (var g = Graphics.FromImage(newBitmap))
                        {
                            g.DrawImage(original, new Rectangle(16, 16, 64, 64), 128 * x + 160, 128 * y + 106, 64, 64, GraphicsUnit.Pixel);
                            newBitmap.Save($@"D:\c\KTANE\Megaman2\Assets\Megaman 2\Images\Robot masters\{name}.png");
                        }
                    }
                }
            }
        }

        private static readonly Dictionary<string, string> _weaponInfo = new Dictionary<string, string>()
        {
            { "Cut Man", "Rolling Cutter" },
            { "Guts Man", "Super Arm" },
            { "Ice Man", "Ice Slasher" },
            { "Bomb Man", "Hyper Bomb" },
            { "Fire Man", "Fire Storm" },
            { "Elec Man", "Thunder Beam" },
            { "Metal Man", "Metal Blade" },
            { "Air Man", "Air Shooter" },
            { "Bubble Man", "Bubble Lead" },
            { "Quick Man", "Quick Boomerang" },
            { "Crash Man", "Crash Bomber" },
            { "Flash Man", "Time Stopper" },
            { "Heat Man", "Atomic Fire" },
            { "Wood Man", "Leaf Shield" },
            { "Needle Man", "Needle Cannon" },
            { "Magnet Man", "Magnet Missile" },
            { "Gemini Man", "Gemini Laser" },
            { "Hard Man", "Hard Knuckle" },
            { "Top Man", "Top Spin" },
            { "Snake Man", "Search Snake" },
            { "Spark Man", "Spark Shock" },
            { "Shadow Man", "Shadow Blade" },
            { "Bright Man", "Flash Stopper" },
            { "Toad Man", "Rain Flush" },
            { "Drill Man", "Drill Bomb" },
            { "Pharaoh Man", "Pharaoh Shot" },
            { "Ring Man", "Ring Boomerang" },
            { "Dust Man", "Dust Crusher" },
            { "Dive Man", "Dive Missile" },
            { "Skull Man", "Skull Barrier" },
            { "Gravity Man", "Gravity Hold" },
            { "Wave Man", "Water Wave" },
            { "Stone Man", "Power Stone" },
            { "Gyro Man", "Gyro Attack" },
            { "Star Man", "Star Crash" },
            { "Charge Man", "Charge Kick" },
            { "Napalm Man", "Napalm Bomb" },
            { "Crystal Man", "Crystal Eye" },
            { "Blizzard Man", "Blizzard Attack" },
            { "Centaur Man", "Centaur Flash" },
            { "Flame Man", "Flame Blast" },
            { "Knight Man", "Knight Crusher" },
            { "Plant Man", "Plant Barrier" },
            { "Tomahawk Man", "Silver Tomahawk" },
            { "Wind Man", "Wind Storm" },
            { "Yamato Man", "Yamato Spear" },
            { "Freeze Man", "Freeze Cracker" },
            { "Junk Man", "Junk Shield" },
            { "Burst Man", "Danger Wrap" },
            { "Cloud Man", "Thunder Bolt" },
            { "Spring Man", "Wild Coil" },
            { "Slash Man", "Slash Claw" },
            { "Shade Man", "Noise Crush" },
            { "Turbo Man", "Scorch Wheel" },
            { "Tengu Man", "Tengu Blade" },
            { "Astro Man", "Astro Crush" },
            { "Sword Man", "Flame Sword" },
            { "Clown Man", "Thunder Claw" },
            { "Search Man", "Homing Sniper" },
            { "Frost Man", "Ice Wave" },
            { "Grenade Man", "Flash Bomb" },
            { "Aqua Man", "Water Balloon" },
            { "Concrete Man", "Concrete Shot" },
            { "Tornado Man", "Tornado Blow" },
            { "Splash Woman", "Laser Trident" },
            { "Plug Man", "Plug Ball" },
            { "Jewel Man", "Jewel Satellite" },
            { "Hornet Man", "Hornet Chaser" },
            { "Magma Man", "Magma Bazooka" },
            { "Galaxy Man", "Black Hole Bomb" },
            { "Blade Man", "Triple Blade" },
            { "Pump Man", "Water Shield" },
            { "Commando Man", "Commando Bomb" },
            { "Chill Man", "Chill Spike" },
            { "Sheep Man", "Thunder Wool" },
            { "Strike Man", "Rebound Striker" },
            { "Nitro Man", "Wheel Cutter" },
            { "Solar Man", "Solar Blaze" },
            { "Dynamo Man", "Lightning Bolt" },
            { "Cold Man", "Ice Wall" },
            { "Ground Man", "Spread Drill" },
            { "Pirate Man", "Remote Mine" },
            { "Burner Man", "Wave Burner" },
            { "Magic Man", "Magic Card" }
        };

        private static readonly Dictionary<string, string> _weaponFilenames = new Dictionary<string, string>
        {
            { "Metal Blade", "Metalbladeicon" },
            { "Fire Storm", "MMWW-MM1-FireStorm-Icon" },
            { "Air Shooter", "Airshootericon" },
            { "Thunder Beam", "MMWW-MM1-ThunderBeam-Icon" },
            { "Quick Boomerang", "Quickboomerangicon" },
            { "Bubble Lead", "Bubbleleadicon" },
            { "Crash Bomber", "Crashbombicon" },
            { "Time Stopper", "Timestoppericon" },
            { "Atomic Fire", "Atomicfireicon" },
            { "Leaf Shield", "Leafshieldicon" },
            { "Gemini Laser", "Geminilasericon" },
            { "Top Spin", "topspinicon" },
            { "Rain Flush", "Rainflushicon" },
            { "Water Wave", "Waterwaveicon" },
            { "Chill Spike", "MM10-ChillSpike-Icon" },
            { "Wheel Cutter", "MM10-WheelCutter-Icon" },
            { "Wave Burner", "WaveBurnerIcon" },
            { "Magic Card", "MagicCardIcon" },
            { "Super Arm", "MMWW-MM1-SuperArm-Icon" },
            { "Ice Slasher", "MMWW-MM1-IceSlasher-Icon" },
            { "Hyper Bomb", "MMWW-MM1-HyperBomb-Icon" },
            { "Needle Cannon", "Needlecannonicon" },
            { "Hard Knuckle", "Hardknuckleicon" },
            { "Flash Stopper", "Flashstoppericon" },
            { "Search Snake", "Searchsnakeicon" },
            { "Magnet Missile", "Magnetmissileicon" },
            { "Shadow Blade", "Shadowbladeicon" },
            { "Spark Shock", "Sparkshockicon" },
            { "Rolling Cutter", "MMWW-MM1-RollingCutter-Icon" },
            { "Drill Bomb", "Drillbombicon" },
            { "Pharaoh Shot", "Pharaohshoticon" },
            { "Ring Boomerang", "Ringboomerangicon" },
            { "Dust Crusher", "Dustcrushericon" },
            { "Skull Barrier", "Skullbarriericon" },
            { "Dive Missile", "Divemissileicon" },
            { "Power Stone", "Powerstoneicon" },
            { "Gravity Hold", "Gravityholdicon" },
            { "Star Crash", "Starcrashicon" },
            { "Gyro Attack", "Gyroattackicon" },
            { "Charge Kick", "Chargekickicon" },
            { "Crystal Eye", "Crystaleyeicon" },
            { "Blizzard Attack", "Blizzardattackicon" },
            { "Centaur Flash", "Centaurflashicon" },
            { "Flame Blast", "Flameblasticon" },
            { "Knight Crusher", "Knightcrushicon" },
            { "Plant Barrier", "Plantbarriericon" },
            { "Silver Tomahawk", "Silvertomahawkicon" },
            { "Wind Storm", "Windstormicon" },
            { "Yamato Spear", "Yamatospearicon" },
            { "Tengu Blade", "TenguBladeIcon" },
            { "Astro Crush", "MM8-AstroCrush-Icon" },
            { "Flame Sword", "MM8-FlameSword-Icon" },
            { "Thunder Claw", "MM8-ThunderClaw-Icon" },
            { "Homing Sniper", "MM8-HomingSniper-Icon" },
            { "Ice Wave", "MM8-IceWave-Icon" },
            { "Flash Bomb", "MM8-FlashBomb-Icon" },
            { "Water Balloon", "MM8-WaterBalloon-Icon" },
            { "Concrete Shot", "MM9-ConcreteShot-Icon" },
            { "Tornado Blow", "MM9-TornadoBlow-Icon" },
            { "Laser Trident", "MM9-LaserTrident-Icon" },
            { "Plug Ball", "MM9-PlugBall-Icon" },
            { "Jewel Satellite", "MM9-JewelSatellite-Icon" },
            { "Hornet Chaser", "MM9-HornetChaser-Icon" },
            { "Magma Bazooka", "MM9-MagmaBazooka-Icon" },
            { "Black Hole Bomb", "MM9-BlackHoleBomb-Icon" },
            { "Triple Blade", "MM10-TripleBlade-Icon" },
            { "Water Shield", "MM10-WaterShield-Icon" },
            { "Commando Bomb", "MM10-CommandoBomb-Icon" },
            { "Thunder Wool", "MM10-ThunderWool-Icon" },
            { "Rebound Striker", "MM10-ReboundStriker-Icon" },
            { "Solar Blaze", "MM10-SolarBlaze-Icon" },
            { "Lightning Bolt", "LightningBoltIcon" },
            { "Ice Wall", "IceWallIcon" },
            { "Spread Drill", "SpreadDrillIcon" },
            { "Remote Mine", "RemoteMineIcon" },
            { "Thunder Bolt", "MM7-ThunderBolt-Icon" },
            { "Wild Coil", "MM7-WildCoil-Icon" },
            { "Slash Claw", "MM7-SlashClaw-Icon" },
            { "Danger Wrap", "MM7-DangerWrap-Icon" },
            { "Junk Shield", "MM7-JunkShield-Icon" },
            { "Noise Crush", "MM7-NoiseCrush-Icon" },
            { "Freeze Cracker", "MM7-FreezeCracker-Icon" },
            { "Scorch Wheel", "MM7-ScorchWheel-Icon" },
            { "Napalm Bomb", "Napalmbombicon" }
        };

        public static void GenerateWeapons()
        {
            foreach (var kvp in _weaponInfo)
            {
                var master = kvp.Key;
                var weapon = kvp.Value;
                using (var bmp = new Bitmap($@"D:\c\KTANE\KtaneStuff\DataFiles\MegaMan2\Weapon-Originals\{weapon}.png"))
                {
                    using (var newBmp = new Bitmap(96, 96, PixelFormat.Format32bppArgb))
                    using (var g = Graphics.FromImage(newBmp))
                    {
                        g.InterpolationMode = InterpolationMode.NearestNeighbor;
                        var offset = newBmp.Width / bmp.Width / 2;
                        g.DrawImage(bmp, new Rectangle(0, 0, 96, 96), -.5f, -.5f, bmp.Width, bmp.Height, GraphicsUnit.Pixel);
                        newBmp.Save($@"D:\c\KTANE\Public\HTML\img\Mega Man 2\Weapons\{master}.png");
                    }
                }
            }
        }
    }
}