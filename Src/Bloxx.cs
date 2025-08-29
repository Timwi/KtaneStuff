using RT.Dijkstra;
using RT.TagSoup;
using RT.Util;
using RT.Util.ExtensionMethods;

namespace KtaneStuff;

public class Bloxx
{
    private enum orientation
    {
        Upright,
        Horiz,
        Vert
    }

    private sealed class gameState : IEquatable<gameState>
    {
        public int CurPosX;
        public int CurPosY;
        public orientation Orientation;

        public gameState Move(int direction)
        {
            var newState = new gameState { CurPosX = CurPosX, CurPosY = CurPosY, Orientation = Orientation };
            newState.moveImpl(direction);
            return newState;
        }

        private void moveImpl(int direction)
        {
            switch (Orientation)
            {
                case orientation.Upright:
                    switch (direction)
                    {
                        case 0: CurPosY -= 2; Orientation = orientation.Vert; break;
                        case 1: CurPosY++; Orientation = orientation.Vert; break;
                        case 2: CurPosX -= 2; Orientation = orientation.Horiz; break;
                        case 3: CurPosX++; Orientation = orientation.Horiz; break;
                    }
                    break;
                case orientation.Horiz:
                    switch (direction)
                    {
                        case 0: CurPosY--; break;
                        case 1: CurPosY++; break;
                        case 2: CurPosX--; Orientation = orientation.Upright; break;
                        case 3: CurPosX += 2; Orientation = orientation.Upright; break;
                    }
                    break;
                case orientation.Vert:
                    switch (direction)
                    {
                        case 0: CurPosY--; Orientation = orientation.Upright; break;
                        case 1: CurPosY += 2; Orientation = orientation.Upright; break;
                        case 2: CurPosX--; break;
                        case 3: CurPosX++; break;
                    }
                    break;
            }
        }

        public bool DeservesStrike(string grid, int cols)
        {
            var rows = grid.Length / cols;
            return CurPosX < 0 || CurPosX >= cols || CurPosY < 0 || CurPosY >= rows || grid[CurPosX + cols * CurPosY] == '-' ||
                        (Orientation == orientation.Horiz && (CurPosX >= cols - 1 || grid[CurPosX + 1 + cols * CurPosY] == '-')) ||
                        (Orientation == orientation.Vert && (CurPosY >= rows - 1 || grid[CurPosX + cols * (CurPosY + 1)] == '-'));
        }

        public bool Equals(gameState other) => other != null && other.CurPosX == CurPosX && other.CurPosY == CurPosY && other.Orientation == Orientation;
        public override bool Equals(object obj) => obj is gameState gs && Equals(gs);
        public override int GetHashCode() => Ut.ArrayHash(CurPosX, CurPosY, Orientation);

        public void MarkUsed(char[] newGrid, int cols, char ch = '#')
        {
            newGrid[CurPosX + cols * CurPosY] = ch;
            switch (Orientation)
            {
                case orientation.Horiz: newGrid[CurPosX + 1 + cols * CurPosY] = ch; break;
                case orientation.Vert: newGrid[CurPosX + cols * (CurPosY + 1)] = ch; break;
            }
        }

        public char PosChar => Orientation switch
        {
            orientation.Upright => 'U',
            orientation.Horiz => 'H',
            orientation.Vert => 'V',
            _ => throw new NotImplementedException(),
        };

        public override string ToString() => $"{CurPosX}/{CurPosY}/{Orientation}";
    }

    private sealed class bloxxNode : Node<int, (int dir, gameState state)>
    {
        public gameState GameState;
        public gameState DesiredEndState;
        public HashSet<gameState> ValidStates;
        public string ValidPositions;
        public int ValidPositionsWidth;

        public override bool IsFinal => GameState.Equals(DesiredEndState);

        public override IEnumerable<Edge<int, (int dir, gameState state)>> Edges => Enumerable.Range(0, 4)
            .Select((dir, i) => new { Dir = dir, State = GameState.Move(i) })
            .Where(inf => ValidStates != null ? ValidStates.Contains(inf.State) : !inf.State.DeservesStrike(ValidPositions, ValidPositionsWidth))
            .Select(inf => new Edge<int, (int, gameState)>(1, (inf.Dir, inf.State), new bloxxNode { GameState = inf.State, DesiredEndState = DesiredEndState, ValidStates = ValidStates, ValidPositions = ValidPositions, ValidPositionsWidth = ValidPositionsWidth }));

        public override bool Equals(Node<int, (int dir, gameState state)> other) => other is bloxxNode n && n.GameState.Equals(GameState);
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
        var validStates = new List<gameState>();
        for (var x = 0; x < cols; x++)
            for (var y = 0; y < rows; y++)
                foreach (var or in new[] { orientation.Horiz, orientation.Vert, orientation.Upright })
                {
                    var state = new gameState { CurPosX = x, CurPosY = y, Orientation = or };
                    if (!state.DeservesStrike(validPositions, cols))
                        validStates.Add(state);
                }
        var allValidStates = validStates.ToHashSet();

        var checkPoints = new List<gameState>();
        var startChPtIx = rnd.Next(0, validStates.Count);
        checkPoints.Add(validStates[startChPtIx]);
        validStates.RemoveAt(startChPtIx);
        var newGrid = Ut.NewArray(validPositions.Length, _ => '-');

        for (var i = 1; i < numCheckPoints; i++)
        {
            tryAgain:
            var ix = rnd.Next(0, validStates.Count);
            if (i == numCheckPoints - 1 && validStates[ix].Orientation != orientation.Upright)
                goto tryAgain;
            static int dist(gameState state1, gameState state2) => Math.Abs(state1.CurPosX - state2.CurPosX) + Math.Abs(state1.CurPosY - state2.CurPosY);
            if (i > 0 && dist(checkPoints.Last(), validStates[ix]) < 7)
                goto tryAgain;
            var nextCheckPoint = validStates[ix];

            try
            {
                var node = new bloxxNode { ValidStates = validStates.ToHashSet(), GameState = checkPoints.Last(), DesiredEndState = nextCheckPoint };
                var path = DijkstrasAlgorithm.Run(node, 0, (a, b) => a + b, out _);
                foreach (var step in path)
                {
                    step.Label.state.MarkUsed(newGrid, cols);
                    validStates.Remove(step.Label.state);
                }
                checkPoints.Add(nextCheckPoint);
            }
            catch (DijkstraNoSolutionException<int, (int dir, gameState state)>)
            {
                Console.WriteLine("Trying again");
                goto tryEverythingAgain;
            }
            validStates.Remove(nextCheckPoint);
        }

        // Find shortest path
        var overallStart = new bloxxNode { GameState = checkPoints[0], DesiredEndState = checkPoints.Last(), ValidPositions = new string(newGrid), ValidPositionsWidth = cols };
        var shortestPath = DijkstrasAlgorithm.Run(overallStart, 0, (a, b) => a + b, out _).ToArray();

        var finalGrid = Ut.NewArray(validPositions.Length, _ => '-');
        // Mark reachable squares
        for (var spIx = 0; spIx < shortestPath.Length; spIx++)
            shortestPath[spIx].Label.state.MarkUsed(finalGrid, cols, spIx == shortestPath.Length - 1 ? 'X' : '#');
        // Mark start and end location
        checkPoints[0].MarkUsed(finalGrid, cols, checkPoints[0].PosChar);
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
