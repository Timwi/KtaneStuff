using System;
using System.Collections.Generic;
using System.Linq;
using RT.Util;
using RT.Util.ExtensionMethods;

namespace KtaneStuff
{
    public static class ToonEnough
    {
        private static readonly int[,] laffChart = new int[,] {
            {76, 124, 79, 107, 113, 127, 26, 75, 61, 77, 43},
            {92, 21, 125, 28, 71, 119, 82, 85, 101, 16, 69},
            {23, 44, 100, 87, 136, 105, 49, 68, 81, 89, 118},
            {56, 41, 39, 51, 37, 114, 65, 58, 18, 47, 93},
            {133, 31, 104, 73, 67, 134, 98, 34, 122, 62, 40},
            {115, 106, 128, 130, 112, 45, 63, 129, 123, 22, 57},
            {27, 48, 99, 91, 90, 55, 60, 80, 19, 116, 36},
            {15, 95, 86, 88, 72, 24, 102, 59, 53, 94, 135},
        };
        private static readonly int[,] gagChart = new int[,]
        {
            {1, 1, 2, 3, 5, 8, 13},
            {2, 3, 4, 5, 10, 15, 20},
            {1, 3, 7, 14, 20, 26, 36},
            {2, 4, 8, 16, 24, 32, 40},
            {2, 4, 8, 12, 18, 24, 32},
            {1, 3, 5, 9, 15, 21, 29},
            {2, 3, 4, 5, 10, 15, 20}
        };

        public static void CreateGoodsheet()
        {
            var toons = "Blue_Bear_Female,Blue_Bear_Male,Blue_Cat_Female,Blue_Cat_Male,Blue_Crocodile_Female,Blue_Crocodile_Male,Blue_Deer_Female,Blue_Deer_Male,Blue_Dog_Female,Blue_Dog_Male,Blue_Duck_Female,Blue_Duck_Male,Blue_Horse_Female,Blue_Horse_Male,Blue_Monkey_Female,Blue_Monkey_Male,Blue_Mouse_Female,Blue_Mouse_Male,Blue_Pig_Female,Blue_Pig_Male,Blue_Rabbit_Female,Blue_Rabbit_Male,Brown_Bear_Female,Brown_Bear_Male,Brown_Cat_Female,Brown_Cat_Male,Brown_Crocodile_Female,Brown_Crocodile_Male,Brown_Deer_Female,Brown_Deer_Male,Brown_Dog_Female,Brown_Dog_Male,Brown_Duck_Female,Brown_Duck_Male,Brown_Horse_Female,Brown_Horse_Male,Brown_Monkey_Female,Brown_Monkey_Male,Brown_Mouse_Female,Brown_Mouse_Male,Brown_Pig_Female,Brown_Pig_Male,Brown_Rabbit_Female,Brown_Rabbit_Male,Green_Bear_Female,Green_Bear_Male,Green_Cat_Female,Green_Cat_Male,Green_Crocodile_Female,Green_Crocodile_Male,Green_Deer_Female,Green_Deer_Male,Green_Dog_Female,Green_Dog_Male,Green_Duck_Female,Green_Duck_Male,Green_Horse_Female,Green_Horse_Male,Green_Monkey_Female,Green_Monkey_Male,Green_Mouse_Female,Green_Mouse_Male,Green_Pig_Female,Green_Pig_Male,Green_Rabbit_Female,Green_Rabbit_Male,Orange_Bear_Female,Orange_Bear_Male,Orange_Cat_Female,Orange_Cat_Male,Orange_Crocodile_Female,Orange_Crocodile_Male,Orange_Deer_Female,Orange_Deer_Male,Orange_Dog_Female,Orange_Dog_Male,Orange_Duck_Female,Orange_Duck_Male,Orange_Horse_Female,Orange_Horse_Male,Orange_Monkey_Female,Orange_Monkey_Male,Orange_Mouse_Female,Orange_Mouse_Male,Orange_Pig_Female,Orange_Pig_Male,Orange_Rabbit_Female,Orange_Rabbit_Male,Pink_Bear_Female,Pink_Bear_Male,Pink_Cat_Female,Pink_Cat_Male,Pink_Crocodile_Female,Pink_Crocodile_Male,Pink_Deer_Female,Pink_Deer_Male,Pink_Dog_Female,Pink_Dog_Male,Pink_Duck_Female,Pink_Duck_Male,Pink_Horse_Female,Pink_Horse_Male,Pink_Monkey_Female,Pink_Monkey_Male,Pink_Mouse_Female,Pink_Mouse_Male,Pink_Pig_Female,Pink_Pig_Male,Pink_Rabbit_Female,Pink_Rabbit_Male,Purple_Bear_Female,Purple_Bear_Male,Purple_Cat_Female,Purple_Cat_Male,Purple_Crocodile_Female,Purple_Crocodile_Male,Purple_Deer_Female,Purple_Deer_Male,Purple_Dog_Female,Purple_Dog_Male,Purple_Duck_Female,Purple_Duck_Male,Purple_Horse_Female,Purple_Horse_Male,Purple_Monkey_Female,Purple_Monkey_Male,Purple_Mouse_Female,Purple_Mouse_Male,Purple_Pig_Female,Purple_Pig_Male,Purple_Rabbit_Female,Purple_Rabbit_Male,Red_Bear_Female,Red_Bear_Male,Red_Cat_Female,Red_Cat_Male,Red_Crocodile_Female,Red_Crocodile_Male,Red_Deer_Female,Red_Deer_Male,Red_Dog_Female,Red_Dog_Male,Red_Duck_Female,Red_Duck_Male,Red_Horse_Female,Red_Horse_Male,Red_Monkey_Female,Red_Monkey_Male,Red_Mouse_Female,Red_Mouse_Male,Red_Pig_Female,Red_Pig_Male,Red_Rabbit_Female,Red_Rabbit_Male,Yellow_Bear_Female,Yellow_Bear_Male,Yellow_Cat_Female,Yellow_Cat_Male,Yellow_Crocodile_Female,Yellow_Crocodile_Male,Yellow_Deer_Female,Yellow_Deer_Male,Yellow_Dog_Female,Yellow_Dog_Male,Yellow_Duck_Female,Yellow_Duck_Male,Yellow_Horse_Female,Yellow_Horse_Male,Yellow_Monkey_Female,Yellow_Monkey_Male,Yellow_Mouse_Female,Yellow_Mouse_Male,Yellow_Pig_Female,Yellow_Pig_Male,Yellow_Rabbit_Female,Yellow_Rabbit_Male"
                .Split(',')
                .Select(str => str.Split('_'))
                .Select(arr => new { Color = arr[0], Species = arr[1], Gender = arr[2] })
                .ToArray();
            var colors = toons.Select(t => t.Color).Distinct().ToArray();
            var specieses = toons.Select(t => t.Species).Distinct().ToArray();
            var genders = toons.Select(t => t.Gender).Distinct().ToArray();

            var cogs = "1_Story_Building,2_Story_Building,3_Story_Building,4_Story_Building,5_Story_Building,The_Back_Nine,The_Cashbot_Bullion_Mint,The_Cashbot_Coin_Mint,The_Cashbot_Dollar_Mint,The_CEO,The_CFO,The_CJ,The_Front_Three,The_Lawbot_A_Office,The_Lawbot_B_Office,The_Lawbot_C_Office,The_Lawbot_D_Office,The_Middle_Six,The_Sellbot_Factory,The_VP"
                .Split(',');

            var table = new Dictionary<(string, string, string), (string[] gags, int laff)>();

            foreach (var color in colors)
                foreach (var species in specieses)
                {
                    var laff = getLaff(color, species);

                    //var cogCutOff = cogs.Length;
                    //if (laff >= 100)
                    //    cogCutOff = cogs.Length;
                    //else if (laff >= 96)
                    //    cogCutOff = 17;
                    //else if (laff >= 95)
                    //    cogCutOff = 16;
                    //else if (laff >= 90)
                    //    cogCutOff = 15;
                    //else if (laff >= 86)
                    //    cogCutOff = 14;
                    //else if (laff >= 81)
                    //    cogCutOff = 13;
                    //else if (laff >= 76)
                    //    cogCutOff = 12;
                    //else if (laff >= 71)
                    //    cogCutOff = 11;
                    //else if (laff >= 66)
                    //    cogCutOff = 9;
                    //else if (laff >= 61)
                    //    cogCutOff = 8;
                    //else
                    //    cogCutOff = 7;

                    foreach (var gender in genders)
                    //for (int i = 0; i < cogCutOff; i++)
                    {
                        //var cog = cogs[i];
                        string[] gags = getGags(color, species, gender, laff);
                        string[] gagsCarry;
                        if (laff >= 61)
                            gagsCarry = new string[6];
                        else if (laff >= 52)
                            gagsCarry = new string[5];
                        else if (laff >= 34)
                            gagsCarry = new string[4];
                        else if (laff >= 25)
                            gagsCarry = new string[3];
                        else
                            gagsCarry = new string[2];
                        for (int aa = 0; aa < gagsCarry.Length; aa++)
                            gagsCarry[aa] = gags[aa];

                        table[(color, species, gender)] = (gagsCarry, laff);

                        //int[] gagLevels = getGagLevels(gagsCarry);

                        //int toonScore = getToonScore(gagsCarry, gagLevels);
                        //int cogScore = getCogScore();
                        //Debug.Log("Final Toon Score: " + toonScore);
                        //Debug.Log("Cog Challenge Score: " + cogScore);
                        //if (toonScore >= cogScore)
                        //{
                        //    answer = 1;
                        //    Debug.Log("Answer should be YES");
                        //}

                        //else
                        //{
                        //    answer = 0;
                        //    Debug.Log("Answer should be NO");
                        //}
                    }
                }

            var htmlTable = specieses.SelectMany(s => genders.Select(g =>
            {
                var abbrev = "Throw=T,Squirt=Q,Toon-up=U,Lure=L,Trap=R,Drop=D,Sound=S"
                    .Split(',').Select(str => str.Split('=')).ToDictionary(arr => arr[0], arr => arr[1]);
                var row = colors.Order().Select(c => $"<td>{table[(c, s, g)].Apply(inf => $"{inf.gags.Select(move => abbrev.Get(move, move)).JoinString()}<br>{inf.laff}")}</td>").JoinString();
                return $"<tr><th>{(g == "Male" ? "M" : "F")} {s}</th>{row}</tr>";
            })).JoinString();
            Utils.ReplaceInFile(@"D:\c\KTANE\Public\HTML\Toon Enough optimized (Timwi).html", "<!--%%-->", "<!--%%%-->", $"<table class='toon-table'><tr><td class='corner'></td>{colors.Order().Select(c => $"<th>{c}</th>").JoinString()}</tr>{htmlTable}</table>");
        }
        static int getLaff(string color, string species)
        {
            int col = -1;
            int row = -1;
            switch (color)
            {
                case "Red":
                    row = 0;
                    break;
                case "Orange":
                    row = 1;
                    break;
                case "Yellow":
                    row = 2;
                    break;
                case "Green":
                    row = 3;
                    break;
                case "Blue":
                    row = 4;
                    break;
                case "Purple":
                    row = 5;
                    break;
                case "Pink":
                    row = 6;
                    break;
                case "Brown":
                    row = 7;
                    break;
            }
            switch (species)
            {
                case "Cat":
                    col = 0;
                    break;
                case "Dog":
                    col = 1;
                    break;
                case "Duck":
                    col = 2;
                    break;
                case "Rabbit":
                    col = 3;
                    break;
                case "Horse":
                    col = 4;
                    break;
                case "Pig":
                    col = 5;
                    break;
                case "Monkey":
                    col = 6;
                    break;
                case "Mouse":
                    col = 7;
                    break;
                case "Bear":
                    col = 8;
                    break;
                case "Deer":
                    col = 9;
                    break;
                case "Crocodile":
                    col = 10;
                    break;
            }
            return laffChart[row, col];
        }

        static bool EqualsIgnoreCase(this string str, string comparison) => str.Equals(comparison, StringComparison.InvariantCultureIgnoreCase);

        static string[] getGags(string color, string species, string gender, int laff)
        {
            string[] gags = new string[6];
            gags[1] = "Throw";
            gags[0] = "Squirt";
            if (species.EqualsIgnoreCase("dog") || species.EqualsIgnoreCase("rabbit") || species.EqualsIgnoreCase("horse") || species.EqualsIgnoreCase("monkey") || species.EqualsIgnoreCase("mouse") || species.EqualsIgnoreCase("bear"))
            {
                gags[2] = "Toon-up";
                if (color.EqualsIgnoreCase("brown") || color.EqualsIgnoreCase("purple") || color.EqualsIgnoreCase("yellow") || color.EqualsIgnoreCase("green"))
                {
                    gags[3] = "Drop";
                    if (gender.EqualsIgnoreCase("male"))
                    {
                        gags[4] = "Sound";
                        if (laff > 97)
                        {
                            gags[5] = "Trap";
                        }
                        else
                        {
                            gags[5] = "Lure";
                        }
                    }
                    else
                    {
                        gags[4] = "Trap";
                        if (laff > 97)
                        {
                            gags[5] = "Lure";
                        }
                        else
                        {
                            gags[5] = "Sound";
                        }
                    }
                }
                else
                {
                    gags[3] = "Lure";
                    if (gender.EqualsIgnoreCase("male"))
                    {
                        gags[4] = "Sound";
                        if (laff > 97)
                        {
                            gags[5] = "Drop";
                        }
                        else
                        {
                            gags[5] = "Trap";
                        }
                    }
                    else
                    {
                        gags[4] = "Trap";
                        if (laff > 97)
                        {
                            gags[5] = "Drop";
                        }
                        else
                        {
                            gags[5] = "Sound";
                        }
                    }
                }
            }
            else
            {
                gags[2] = "Sound";
                if (color.EqualsIgnoreCase("brown") || color.EqualsIgnoreCase("purple") || color.EqualsIgnoreCase("yellow") || color.EqualsIgnoreCase("green"))
                {
                    gags[3] = "Drop";
                    if (gender.EqualsIgnoreCase("male"))
                    {
                        gags[4] = "Toon-up";
                        if (laff > 97)
                        {
                            gags[5] = "Trap";
                        }
                        else
                        {
                            gags[5] = "Lure";
                        }
                    }
                    else
                    {
                        gags[4] = "Trap";
                        if (laff > 97)
                        {
                            gags[5] = "Lure";
                        }
                        else
                        {
                            gags[5] = "Toon-up";
                        }
                    }
                }
                else
                {
                    gags[3] = "Lure";
                    if (gender.EqualsIgnoreCase("male"))
                    {
                        gags[4] = "Toon-up";
                        if (laff > 97)
                        {
                            gags[5] = "Drop";
                        }
                        else
                        {
                            gags[5] = "Trap";
                        }
                    }
                    else
                    {
                        gags[4] = "Trap";
                        if (laff > 97)
                        {
                            gags[5] = "Drop";
                        }
                        else
                        {
                            gags[5] = "Toon-up";
                        }
                    }
                }
            }
            return gags;
        }

        static int getCogScore(string cog)
        {
            switch (cog)
            {
                case "1_Story_Building":
                    return 58;
                case "2_Story_Building":
                    return 60;
                case "3_Story_Building":
                    return 63;
                case "4_Story_Building":
                    return 66;
                case "5_Story_Building":
                    return 69;
                case "The_Sellbot_Factory":
                    return 65;
                case "The_Cashbot_Coin_Mint":
                    return 78;
                case "The_Cashbot_Dollar_Mint":
                    return 80;
                case "The_Cashbot_Bullion_Mint":
                    return 82;
                case "The_Lawbot_A_Office":
                    return 79;
                case "The_Lawbot_B_Office":
                    return 81;
                case "The_Lawbot_C_Office":
                    return 83;
                case "The_Lawbot_D_Office":
                    return 85;
                case "The_Front_Three":
                    return 85;
                case "The_Middle_Six":
                    return 87;
                case "The_Back_Nine":
                    return 89;
                case "The_VP":
                    return 73;
                case "The_CFO":
                    return 85;
                case "The_CJ":
                    return 87;
                case "The_CEO":
                    return 90;
                default:
                    return -1;
            }
        }
    }
}