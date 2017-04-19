using System.IO;
using System.Text.RegularExpressions;
using RT.Util;

namespace KtaneStuff
{
    static partial class MouseInTheMaze
    {
        public static void DoManual()
        {
            var path = @"D:\c\KTANE\MouseInTheMaze\Manual\Mouse In The Maze.html";

            for (int i = 0; i < 6; i++)
            {
                var maze = getMaze(i);
                var labels = Ut.NewArray(10, 10, (x, y) => "WGBY "[x == 2 ? y == 2 ? maze.SphereColors[0] : y == 7 ? maze.SphereColors[1] : 4 : x == 7 ? y == 2 ? maze.SphereColors[3] : y == 7 ? maze.SphereColors[2] : 4 : 4]);
                var colors = new[] { "white", "green", "blue", "yellow" };

                File.WriteAllText(path, Regex.Replace(
                    File.ReadAllText(path),
                    $@"(?<=<!--##{i}-->).*(?=<!--###{i}-->)",
                    Utils.Create2DMazeSvg(maze.HoriWalls, maze.VertWalls, labels, frame: true, omitAxes: true, extra: $"<circle cx='127.5' cy='127.5' r='10' fill='#ccc' /><circle cx='127.5' cy='127.5' r='7' fill='#fff' />"),
                    RegexOptions.Singleline));
            }
        }

        sealed class MazeInfo
        {
            public bool[][] HoriWalls;
            public bool[][] VertWalls;
            public int[] SphereColors;
            public int[] GoalSphereColor;
        }

        static MazeInfo getMaze(int num)
        {
            bool[,] horiWalls = new bool[12, 12];
            bool[,] vertWalls = new bool[12, 12];
            int[] objectives = new int[4];
            int[] goals = new int[4];

            switch (num + 1)
            {
                case 1:
                    {
                        horiWalls[1, 0] = true;
                        horiWalls[4, 0] = true;
                        horiWalls[5, 0] = true;
                        horiWalls[7, 0] = true;
                        horiWalls[8, 0] = true;
                        horiWalls[9, 0] = true;
                        horiWalls[3, 1] = true;
                        horiWalls[4, 1] = true;
                        horiWalls[6, 1] = true;
                        horiWalls[7, 1] = true;
                        horiWalls[8, 1] = true;
                        horiWalls[3, 2] = true;
                        horiWalls[4, 2] = true;
                        horiWalls[5, 2] = true;
                        horiWalls[8, 2] = true;
                        horiWalls[9, 2] = true;
                        horiWalls[1, 3] = true;
                        horiWalls[2, 3] = true;
                        horiWalls[5, 3] = true;
                        horiWalls[6, 3] = true;
                        horiWalls[8, 3] = true;
                        horiWalls[7, 3] = true;
                        horiWalls[0, 4] = true;
                        horiWalls[1, 4] = true;
                        horiWalls[8, 4] = true;
                        horiWalls[9, 4] = true;
                        horiWalls[3, 5] = true;
                        horiWalls[4, 5] = true;
                        horiWalls[3, 6] = true;
                        horiWalls[4, 6] = true;
                        horiWalls[5, 6] = true;
                        horiWalls[6, 6] = true;
                        horiWalls[8, 6] = true;
                        horiWalls[8, 7] = true;
                        horiWalls[2, 7] = true;
                        horiWalls[4, 7] = true;
                        horiWalls[5, 7] = true;
                        horiWalls[9, 7] = true;
                        horiWalls[0, 8] = true;
                        horiWalls[1, 8] = true;
                        horiWalls[3, 8] = true;
                        horiWalls[5, 8] = true;
                        horiWalls[7, 8] = true;
                        horiWalls[8, 8] = true;

                        vertWalls[1, 0] = true;
                        vertWalls[2, 0] = true;
                        vertWalls[3, 0] = true;
                        vertWalls[6, 0] = true;
                        vertWalls[7, 0] = true;
                        vertWalls[8, 0] = true;
                        vertWalls[1, 1] = true;
                        vertWalls[2, 1] = true;
                        vertWalls[5, 1] = true;
                        vertWalls[6, 1] = true;
                        vertWalls[8, 1] = true;
                        vertWalls[0, 2] = true;
                        vertWalls[1, 2] = true;
                        vertWalls[4, 2] = true;
                        vertWalls[5, 2] = true;
                        vertWalls[7, 2] = true;
                        vertWalls[1, 3] = true;
                        vertWalls[3, 3] = true;
                        vertWalls[4, 3] = true;
                        vertWalls[8, 3] = true;
                        vertWalls[5, 4] = true;
                        vertWalls[9, 4] = true;
                        vertWalls[1, 5] = true;
                        vertWalls[2, 5] = true;
                        vertWalls[5, 5] = true;
                        vertWalls[6, 5] = true;
                        vertWalls[8, 5] = true;
                        vertWalls[3, 6] = true;
                        vertWalls[4, 6] = true;
                        vertWalls[5, 6] = true;
                        vertWalls[7, 6] = true;
                        vertWalls[8, 6] = true;
                        vertWalls[5, 7] = true;
                        vertWalls[6, 7] = true;
                        vertWalls[6, 8] = true;

                        objectives[0] = 1;
                        objectives[1] = 2;
                        objectives[2] = 3;
                        objectives[3] = 0;

                        goals[0] = 1;
                        goals[1] = 2;
                        goals[2] = 0;
                        goals[3] = 3;
                    }
                    break;

                case 2:
                    {
                        horiWalls[1, 0] = true;
                        horiWalls[8, 0] = true;
                        horiWalls[4, 1] = true;
                        horiWalls[5, 1] = true;
                        horiWalls[6, 1] = true;
                        horiWalls[7, 1] = true;
                        horiWalls[1, 2] = true;
                        horiWalls[2, 2] = true;
                        horiWalls[5, 2] = true;
                        horiWalls[8, 2] = true;
                        horiWalls[0, 3] = true;
                        horiWalls[1, 3] = true;
                        horiWalls[2, 3] = true;
                        horiWalls[9, 3] = true;
                        horiWalls[1, 4] = true;
                        horiWalls[2, 4] = true;
                        horiWalls[3, 4] = true;
                        horiWalls[5, 4] = true;
                        horiWalls[6, 4] = true;
                        horiWalls[8, 4] = true;
                        horiWalls[9, 4] = true;
                        horiWalls[2, 5] = true;
                        horiWalls[3, 5] = true;
                        horiWalls[5, 5] = true;
                        horiWalls[6, 5] = true;
                        horiWalls[7, 5] = true;
                        horiWalls[8, 5] = true;
                        horiWalls[1, 6] = true;
                        horiWalls[2, 6] = true;
                        horiWalls[4, 6] = true;
                        horiWalls[6, 6] = true;
                        horiWalls[9, 6] = true;
                        horiWalls[0, 7] = true;
                        horiWalls[1, 7] = true;
                        horiWalls[2, 7] = true;
                        horiWalls[7, 7] = true;
                        horiWalls[8, 7] = true;
                        horiWalls[1, 8] = true;
                        horiWalls[2, 8] = true;
                        horiWalls[6, 8] = true;
                        horiWalls[9, 8] = true;

                        vertWalls[1, 0] = true;
                        vertWalls[2, 0] = true;
                        vertWalls[5, 0] = true;
                        vertWalls[6, 0] = true;
                        vertWalls[1, 1] = true;
                        vertWalls[0, 2] = true;
                        vertWalls[1, 2] = true;
                        vertWalls[2, 2] = true;
                        vertWalls[8, 2] = true;
                        vertWalls[1, 3] = true;
                        vertWalls[2, 3] = true;
                        vertWalls[3, 3] = true;
                        vertWalls[4, 3] = true;
                        vertWalls[6, 3] = true;
                        vertWalls[7, 3] = true;
                        vertWalls[9, 3] = true;
                        vertWalls[0, 4] = true;
                        vertWalls[3, 4] = true;
                        vertWalls[4, 4] = true;
                        vertWalls[7, 4] = true;
                        vertWalls[8, 4] = true;
                        vertWalls[1, 5] = true;
                        vertWalls[3, 5] = true;
                        vertWalls[7, 5] = true;
                        vertWalls[8, 5] = true;
                        vertWalls[0, 6] = true;
                        vertWalls[2, 6] = true;
                        vertWalls[3, 6] = true;
                        vertWalls[4, 6] = true;
                        vertWalls[7, 6] = true;
                        vertWalls[1, 7] = true;
                        vertWalls[3, 7] = true;
                        vertWalls[4, 7] = true;
                        vertWalls[6, 7] = true;
                        vertWalls[7, 7] = true;
                        vertWalls[8, 7] = true;
                        vertWalls[9, 7] = true;
                        vertWalls[2, 8] = true;

                        objectives[0] = 1;
                        objectives[1] = 2;
                        objectives[2] = 0;
                        objectives[3] = 3;

                        goals[0] = 1;
                        goals[1] = 2;
                        goals[2] = 3;
                        goals[3] = 0;
                    }
                    break;

                case 3:
                    {
                        horiWalls[1, 0] = true;
                        horiWalls[2, 0] = true;
                        horiWalls[3, 0] = true;
                        horiWalls[4, 0] = true;
                        horiWalls[1, 1] = true;
                        horiWalls[2, 1] = true;
                        horiWalls[3, 1] = true;
                        horiWalls[2, 2] = true;
                        horiWalls[6, 2] = true;
                        horiWalls[8, 2] = true;
                        horiWalls[9, 2] = true;
                        horiWalls[3, 3] = true;
                        horiWalls[8, 3] = true;
                        horiWalls[2, 4] = true;
                        horiWalls[3, 4] = true;
                        horiWalls[4, 4] = true;
                        horiWalls[7, 4] = true;
                        horiWalls[2, 5] = true;
                        horiWalls[4, 5] = true;
                        horiWalls[5, 5] = true;
                        horiWalls[6, 5] = true;
                        horiWalls[7, 5] = true;
                        horiWalls[0, 6] = true;
                        horiWalls[1, 6] = true;
                        horiWalls[3, 6] = true;
                        horiWalls[4, 6] = true;
                        horiWalls[5, 6] = true;
                        horiWalls[6, 6] = true;
                        horiWalls[9, 6] = true;
                        horiWalls[1, 7] = true;
                        horiWalls[2, 7] = true;
                        horiWalls[4, 7] = true;
                        horiWalls[5, 7] = true;
                        horiWalls[7, 7] = true;
                        horiWalls[1, 8] = true;
                        horiWalls[2, 8] = true;
                        horiWalls[3, 8] = true;
                        horiWalls[7, 8] = true;
                        horiWalls[8, 8] = true;

                        vertWalls[2, 0] = true;
                        vertWalls[3, 0] = true;
                        vertWalls[4, 0] = true;
                        vertWalls[5, 0] = true;
                        vertWalls[3, 1] = true;
                        vertWalls[4, 1] = true;
                        vertWalls[6, 2] = true;
                        vertWalls[7, 2] = true;
                        vertWalls[2, 3] = true;
                        vertWalls[3, 3] = true;
                        vertWalls[5, 3] = true;
                        vertWalls[8, 3] = true;
                        vertWalls[1, 4] = true;
                        vertWalls[2, 4] = true;
                        vertWalls[3, 4] = true;
                        vertWalls[4, 4] = true;
                        vertWalls[9, 4] = true;
                        vertWalls[1, 5] = true;
                        vertWalls[2, 5] = true;
                        vertWalls[4, 5] = true;
                        vertWalls[5, 5] = true;
                        vertWalls[8, 5] = true;
                        vertWalls[9, 5] = true;
                        vertWalls[0, 6] = true;
                        vertWalls[1, 6] = true;
                        vertWalls[3, 6] = true;
                        vertWalls[4, 6] = true;
                        vertWalls[7, 6] = true;
                        vertWalls[1, 7] = true;
                        vertWalls[2, 7] = true;
                        vertWalls[3, 7] = true;
                        vertWalls[5, 7] = true;
                        vertWalls[6, 7] = true;
                        vertWalls[7, 7] = true;
                        vertWalls[0, 8] = true;
                        vertWalls[1, 8] = true;
                        vertWalls[4, 8] = true;
                        vertWalls[5, 8] = true;
                        vertWalls[7, 8] = true;
                        vertWalls[8, 8] = true;

                        objectives[0] = 3;
                        objectives[1] = 1;
                        objectives[2] = 0;
                        objectives[3] = 2;

                        goals[0] = 3;
                        goals[1] = 0;
                        goals[2] = 1;
                        goals[3] = 2;
                    }
                    break;

                case 4:
                    {
                        horiWalls[1, 0] = true;
                        horiWalls[2, 0] = true;
                        horiWalls[3, 0] = true;
                        horiWalls[4, 0] = true;
                        horiWalls[5, 0] = true;
                        horiWalls[8, 0] = true;
                        horiWalls[2, 1] = true;
                        horiWalls[3, 1] = true;
                        horiWalls[4, 1] = true;
                        horiWalls[8, 1] = true;
                        horiWalls[9, 1] = true;
                        horiWalls[0, 2] = true;
                        horiWalls[1, 2] = true;
                        horiWalls[3, 2] = true;
                        horiWalls[4, 2] = true;
                        horiWalls[6, 2] = true;
                        horiWalls[7, 2] = true;
                        horiWalls[1, 3] = true;
                        horiWalls[4, 3] = true;
                        horiWalls[6, 3] = true;
                        horiWalls[8, 3] = true;
                        horiWalls[0, 4] = true;
                        horiWalls[2, 4] = true;
                        horiWalls[3, 4] = true;
                        horiWalls[5, 4] = true;
                        horiWalls[8, 4] = true;
                        horiWalls[1, 5] = true;
                        horiWalls[3, 5] = true;
                        horiWalls[4, 5] = true;
                        horiWalls[5, 5] = true;
                        horiWalls[9, 5] = true;
                        horiWalls[2, 6] = true;
                        horiWalls[3, 6] = true;
                        horiWalls[7, 6] = true;
                        horiWalls[8, 6] = true;
                        horiWalls[0, 7] = true;
                        horiWalls[3, 7] = true;
                        horiWalls[6, 7] = true;
                        horiWalls[7, 7] = true;
                        horiWalls[9, 7] = true;
                        horiWalls[1, 8] = true;
                        horiWalls[4, 8] = true;
                        horiWalls[5, 8] = true;
                        horiWalls[6, 8] = true;
                        horiWalls[8, 8] = true;

                        vertWalls[1, 0] = true;
                        vertWalls[6, 0] = true;
                        vertWalls[7, 0] = true;
                        vertWalls[2, 1] = true;
                        vertWalls[4, 1] = true;
                        vertWalls[5, 1] = true;
                        vertWalls[7, 1] = true;
                        vertWalls[8, 1] = true;
                        vertWalls[3, 2] = true;
                        vertWalls[8, 2] = true;
                        vertWalls[9, 2] = true;
                        vertWalls[2, 4] = true;
                        vertWalls[4, 4] = true;
                        vertWalls[6, 4] = true;
                        vertWalls[7, 4] = true;
                        vertWalls[1, 5] = true;
                        vertWalls[2, 5] = true;
                        vertWalls[5, 5] = true;
                        vertWalls[6, 5] = true;
                        vertWalls[3, 5] = true;
                        vertWalls[0, 6] = true;
                        vertWalls[1, 6] = true;
                        vertWalls[4, 6] = true;
                        vertWalls[5, 6] = true;
                        vertWalls[7, 6] = true;
                        vertWalls[9, 6] = true;
                        vertWalls[1, 7] = true;
                        vertWalls[5, 7] = true;
                        vertWalls[6, 7] = true;
                        vertWalls[8, 7] = true;
                        vertWalls[2, 8] = true;
                        vertWalls[3, 8] = true;

                        objectives[0] = 0;
                        objectives[1] = 3;
                        objectives[2] = 1;
                        objectives[3] = 2;

                        goals[0] = 3;
                        goals[1] = 1;
                        goals[2] = 0;
                        goals[3] = 2;
                    }
                    break;

                case 5:
                    {
                        horiWalls[8, 0] = true;
                        horiWalls[9, 0] = true;
                        horiWalls[1, 1] = true;
                        horiWalls[2, 1] = true;
                        horiWalls[5, 1] = true;
                        horiWalls[6, 1] = true;
                        horiWalls[7, 1] = true;
                        horiWalls[8, 1] = true;
                        horiWalls[0, 2] = true;
                        horiWalls[2, 2] = true;
                        horiWalls[3, 2] = true;
                        horiWalls[4, 2] = true;
                        horiWalls[6, 2] = true;
                        horiWalls[8, 2] = true;
                        horiWalls[2, 3] = true;
                        horiWalls[3, 3] = true;
                        horiWalls[5, 3] = true;
                        horiWalls[6, 3] = true;
                        horiWalls[7, 3] = true;
                        horiWalls[1, 4] = true;
                        horiWalls[2, 4] = true;
                        horiWalls[6, 4] = true;
                        horiWalls[7, 4] = true;
                        horiWalls[8, 4] = true;
                        horiWalls[0, 5] = true;
                        horiWalls[1, 5] = true;
                        horiWalls[2, 5] = true;
                        horiWalls[3, 5] = true;
                        horiWalls[8, 5] = true;
                        horiWalls[7, 5] = true;
                        horiWalls[1, 6] = true;
                        horiWalls[2, 6] = true;
                        horiWalls[3, 6] = true;
                        horiWalls[4, 6] = true;
                        horiWalls[9, 6] = true;
                        horiWalls[2, 7] = true;
                        horiWalls[6, 7] = true;
                        horiWalls[7, 7] = true;
                        horiWalls[1, 8] = true;
                        horiWalls[2, 8] = true;
                        horiWalls[5, 8] = true;
                        horiWalls[6, 8] = true;
                        horiWalls[8, 8] = true;

                        vertWalls[1, 0] = true;
                        vertWalls[4, 0] = true;
                        vertWalls[7, 0] = true;
                        vertWalls[8, 0] = true;
                        vertWalls[0, 1] = true;
                        vertWalls[5, 1] = true;
                        vertWalls[1, 2] = true;
                        vertWalls[7, 2] = true;
                        vertWalls[0, 3] = true;
                        vertWalls[1, 3] = true;
                        vertWalls[4, 3] = true;
                        vertWalls[5, 3] = true;
                        vertWalls[8, 3] = true;
                        vertWalls[9, 3] = true;
                        vertWalls[1, 4] = true;
                        vertWalls[2, 4] = true;
                        vertWalls[3, 4] = true;
                        vertWalls[4, 4] = true;
                        vertWalls[5, 4] = true;
                        vertWalls[6, 4] = true;
                        vertWalls[7, 4] = true;
                        vertWalls[8, 4] = true;
                        vertWalls[0, 5] = true;
                        vertWalls[5, 5] = true;
                        vertWalls[6, 5] = true;
                        vertWalls[1, 6] = true;
                        vertWalls[3, 6] = true;
                        vertWalls[6, 6] = true;
                        vertWalls[7, 7] = true;
                        vertWalls[8, 7] = true;
                        vertWalls[3, 8] = true;
                        vertWalls[4, 8] = true;
                        vertWalls[6, 8] = true;
                        vertWalls[8, 8] = true;

                        objectives[0] = 3;
                        objectives[1] = 0;
                        objectives[2] = 2;
                        objectives[3] = 1;

                        goals[0] = 2;
                        goals[1] = 0;
                        goals[2] = 1;
                        goals[3] = 3;
                    }
                    break;

                case 6:
                    {
                        horiWalls[1, 0] = true;
                        horiWalls[3, 0] = true;
                        horiWalls[4, 0] = true;
                        horiWalls[7, 0] = true;
                        horiWalls[8, 0] = true;
                        horiWalls[2, 1] = true;
                        horiWalls[4, 1] = true;
                        horiWalls[5, 1] = true;
                        horiWalls[8, 1] = true;
                        horiWalls[1, 2] = true;
                        horiWalls[2, 2] = true;
                        horiWalls[3, 2] = true;
                        horiWalls[4, 2] = true;
                        horiWalls[6, 2] = true;
                        horiWalls[2, 3] = true;
                        horiWalls[5, 3] = true;
                        horiWalls[7, 3] = true;
                        horiWalls[8, 3] = true;
                        horiWalls[0, 4] = true;
                        horiWalls[6, 4] = true;
                        horiWalls[7, 4] = true;
                        horiWalls[9, 4] = true;
                        horiWalls[2, 5] = true;
                        horiWalls[3, 5] = true;
                        horiWalls[6, 5] = true;
                        horiWalls[8, 5] = true;
                        horiWalls[1, 6] = true;
                        horiWalls[2, 6] = true;
                        horiWalls[4, 6] = true;
                        horiWalls[5, 6] = true;
                        horiWalls[7, 6] = true;
                        horiWalls[3, 7] = true;
                        horiWalls[5, 7] = true;
                        horiWalls[6, 7] = true;
                        horiWalls[2, 8] = true;
                        horiWalls[4, 8] = true;
                        horiWalls[7, 8] = true;
                        horiWalls[8, 8] = true;

                        vertWalls[1, 0] = true;
                        vertWalls[2, 0] = true;
                        vertWalls[3, 0] = true;
                        vertWalls[5, 0] = true;
                        vertWalls[7, 0] = true;
                        vertWalls[8, 0] = true;
                        vertWalls[4, 1] = true;
                        vertWalls[5, 1] = true;
                        vertWalls[6, 1] = true;
                        vertWalls[8, 1] = true;
                        vertWalls[9, 1] = true;
                        vertWalls[1, 2] = true;
                        vertWalls[2, 2] = true;
                        vertWalls[4, 2] = true;
                        vertWalls[7, 2] = true;
                        vertWalls[3, 3] = true;
                        vertWalls[4, 3] = true;
                        vertWalls[8, 3] = true;
                        vertWalls[4, 4] = true;
                        vertWalls[5, 4] = true;
                        vertWalls[6, 4] = true;
                        vertWalls[0, 5] = true;
                        vertWalls[1, 5] = true;
                        vertWalls[3, 5] = true;
                        vertWalls[8, 5] = true;
                        vertWalls[9, 5] = true;
                        vertWalls[1, 6] = true;
                        vertWalls[2, 6] = true;
                        vertWalls[4, 6] = true;
                        vertWalls[6, 6] = true;
                        vertWalls[2, 7] = true;
                        vertWalls[3, 7] = true;
                        vertWalls[5, 7] = true;
                        vertWalls[7, 7] = true;
                        vertWalls[8, 7] = true;
                        vertWalls[3, 8] = true;
                        vertWalls[4, 8] = true;
                        vertWalls[6, 8] = true;
                        vertWalls[7, 8] = true;

                        objectives[0] = 2;
                        objectives[1] = 3;
                        objectives[2] = 0;
                        objectives[3] = 1;

                        goals[0] = 2;
                        goals[1] = 3;
                        goals[2] = 1;
                        goals[3] = 0;
                    }
                    break;
            }

            return new MazeInfo
            {
                HoriWalls = Ut.NewArray(10, 10, (y, x) => y > 9 ? false : horiWalls[x, 9 - y]),
                VertWalls = Ut.NewArray(10, 10, (y, x) => y > 9 || x == 0 ? false : vertWalls[9 - y, x - 1]),
                SphereColors = objectives,
                GoalSphereColor = goals
            };
        }
    }
}
