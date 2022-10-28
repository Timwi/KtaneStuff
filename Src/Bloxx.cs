using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using RT.Dijkstra;
using RT.TagSoup;
using RT.Util;
using RT.Util.ExtensionMethods;

namespace KtaneStuff
{
    public class Bloxx
    {
        enum Orientation
        {
            Upright,
            Horiz,
            Vert
        }

        sealed class GameState : IEquatable<GameState>
        {
            public int curPosX;
            public int curPosY;
            public Orientation orientation;

            public GameState Move(int direction)
            {
                var newState = new GameState { curPosX = curPosX, curPosY = curPosY, orientation = orientation };
                newState.moveImpl(direction);
                return newState;
            }

            private void moveImpl(int direction)
            {
                switch (orientation)
                {
                    case Orientation.Upright:
                        switch (direction)
                        {
                            case 0: curPosY -= 2; orientation = Orientation.Vert; break;
                            case 1: curPosY++; orientation = Orientation.Vert; break;
                            case 2: curPosX -= 2; orientation = Orientation.Horiz; break;
                            case 3: curPosX++; orientation = Orientation.Horiz; break;
                        }
                        break;
                    case Orientation.Horiz:
                        switch (direction)
                        {
                            case 0: curPosY--; break;
                            case 1: curPosY++; break;
                            case 2: curPosX--; orientation = Orientation.Upright; break;
                            case 3: curPosX += 2; orientation = Orientation.Upright; break;
                        }
                        break;
                    case Orientation.Vert:
                        switch (direction)
                        {
                            case 0: curPosY--; orientation = Orientation.Upright; break;
                            case 1: curPosY += 2; orientation = Orientation.Upright; break;
                            case 2: curPosX--; break;
                            case 3: curPosX++; break;
                        }
                        break;
                }
            }

            public bool DeservesStrike(string grid, int cols)
            {
                var rows = grid.Length / cols;
                return curPosX < 0 || curPosX >= cols || curPosY < 0 || curPosY >= rows || grid[curPosX + cols * curPosY] == '-' ||
                            (orientation == Orientation.Horiz && (curPosX >= cols - 1 || grid[curPosX + 1 + cols * curPosY] == '-')) ||
                            (orientation == Orientation.Vert && (curPosY >= rows - 1 || grid[curPosX + cols * (curPosY + 1)] == '-'));
            }

            public bool Equals(GameState other) => other != null && other.curPosX == curPosX && other.curPosY == curPosY && other.orientation == orientation;
            public override bool Equals(object obj) => obj is GameState gs && Equals(gs);
            public override int GetHashCode() => Ut.ArrayHash(curPosX, curPosY, orientation);

            public void MarkUsed(char[] newGrid, int cols, char ch = '#')
            {
                newGrid[curPosX + cols * curPosY] = ch;
                switch (orientation)
                {
                    case Orientation.Horiz: newGrid[curPosX + 1 + cols * curPosY] = ch; break;
                    case Orientation.Vert: newGrid[curPosX + cols * (curPosY + 1)] = ch; break;
                }
            }

            public char posChar() => orientation switch
            {
                Orientation.Upright => 'U',
                Orientation.Horiz => 'H',
                Orientation.Vert => 'V',
                _ => throw new NotImplementedException(),
            };

            public override string ToString() => $"{curPosX}/{curPosY}/{orientation}";
        }

        sealed class BloxxNode : Node<int, (int dir, GameState state)>
        {
            public GameState GameState;
            public GameState DesiredEndState;
            public HashSet<GameState> ValidStates;
            public string ValidPositions;
            public int ValidPositionsWidth;

            public override bool IsFinal => GameState.Equals(DesiredEndState);

            public override IEnumerable<Edge<int, (int dir, GameState state)>> Edges => Enumerable.Range(0, 4)
                .Select((dir, i) => new { Dir = dir, State = GameState.Move(i) })
                .Where(inf => ValidStates != null ? ValidStates.Contains(inf.State) : !inf.State.DeservesStrike(ValidPositions, ValidPositionsWidth))
                .Select(inf => new Edge<int, (int, GameState)>(1, (inf.Dir, inf.State), new BloxxNode { GameState = inf.State, DesiredEndState = DesiredEndState, ValidStates = ValidStates, ValidPositions = ValidPositions, ValidPositionsWidth = ValidPositionsWidth }));

            public override bool Equals(Node<int, (int dir, GameState state)> other) => other is BloxxNode n && n.GameState.Equals(GameState);
            public override int GetHashCode() => GameState.GetHashCode();
        }

        public static void Experiment()
        {
            var counts = 0;
            startOver:
            var rnd2 = new Random();
            var seed = rnd2.Next();
            var rnd = new Random(seed);

            tryEverythingAgain:
            const int numCheckPoints = 6;
            var cols = 15;
            var rows = 11;
            var validPositions = "###########----###########----############---#############--#########################################################################################################";
            var validStates = new List<GameState>();
            for (var x = 0; x < cols; x++)
                for (var y = 0; y < rows; y++)
                    foreach (var or in new[] { Orientation.Horiz, Orientation.Vert, Orientation.Upright })
                    {
                        var state = new GameState { curPosX = x, curPosY = y, orientation = or };
                        if (!state.DeservesStrike(validPositions, cols))
                            validStates.Add(state);
                    }
            var allValidStates = validStates.ToHashSet();

            var checkPoints = new List<GameState>();
            var startChPtIx = rnd.Next(0, validStates.Count);
            checkPoints.Add(validStates[startChPtIx]);
            validStates.RemoveAt(startChPtIx);
            var newGrid = Ut.NewArray(validPositions.Length, _ => '-');

            for (var i = 1; i < numCheckPoints; i++)
            {
                tryAgain:
                var ix = rnd.Next(0, validStates.Count);
                if (i == numCheckPoints - 1 && validStates[ix].orientation != Orientation.Upright)
                    goto tryAgain;
                static int dist(GameState state1, GameState state2) => Math.Abs(state1.curPosX - state2.curPosX) + Math.Abs(state1.curPosY - state2.curPosY);
                if (i > 0 && dist(checkPoints.Last(), validStates[ix]) < 7)
                    goto tryAgain;
                var nextCheckPoint = validStates[ix];

                try
                {
                    var node = new BloxxNode { ValidStates = validStates.ToHashSet(), GameState = checkPoints.Last(), DesiredEndState = nextCheckPoint };
                    var path = DijkstrasAlgorithm.Run(node, 0, (a, b) => a + b, out _);
                    foreach (var step in path)
                    {
                        step.Label.state.MarkUsed(newGrid, cols);
                        validStates.Remove(step.Label.state);
                    }
                    checkPoints.Add(nextCheckPoint);
                }
                catch (DijkstraNoSolutionException<int, (int dir, GameState state)>)
                {
                    Console.WriteLine("Trying again");
                    goto tryEverythingAgain;
                }
                validStates.Remove(nextCheckPoint);
            }

            // Find shortest path
            var overallStart = new BloxxNode { GameState = checkPoints[0], DesiredEndState = checkPoints.Last(), ValidPositions = new string(newGrid), ValidPositionsWidth = cols };
            var shortestPath = DijkstrasAlgorithm.Run(overallStart, 0, (a, b) => a + b, out _).ToArray();

            var finalGrid = Ut.NewArray(validPositions.Length, _ => '-');
            // Mark reachable squares
            for (var spIx = 0; spIx < shortestPath.Length; spIx++)
                shortestPath[spIx].Label.state.MarkUsed(finalGrid, cols, spIx == shortestPath.Length - 1 ? 'X' : '#');
            // Mark start and end location
            checkPoints[0].MarkUsed(finalGrid, cols, checkPoints[0].posChar());
            checkPoints.Last().MarkUsed(finalGrid, cols, 'X');
            if (finalGrid.Count(ch => ch == '#') < 50)
                goto startOver;
            counts++;
            Console.WriteLine($"{counts}: Seed = {seed}, start = {checkPoints[0]}, end = {checkPoints.Last()}");
            Console.WriteLine(finalGrid.Split(cols).Select(row => row.JoinString()).JoinString("\n"));
            Console.WriteLine();
            Console.ReadLine();
            goto startOver;
        }
    }
}