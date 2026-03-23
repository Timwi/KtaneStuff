using RT.Util;
using RT.Util.Consoles;
using RT.Util.ExtensionMethods;

namespace KtaneStuff;

static class TrickyTetrisPieces
{
    public static void GenerateTetrisFill()
    {
        var start = DateTime.UtcNow;
        var rnd = new Random(447);
        var fill = generateTetrisFill(rnd);
        Console.SetCursorPosition(0, 0);
        for (var row = 0; row < _gridHeight; row++)
            ConsoleUtil.WriteLine(Enumerable.Range(0, _gridWidth).Select(c => $"  ".Color(ConsoleColor.White, (ConsoleColor) (fill.Value.solution[c + _gridWidth * row] % 15 + 1))).JoinColoredString());
        ConsoleUtil.WriteLineFmt($"Took {(DateTime.UtcNow - start).Milliseconds:G/0}ms");
    }

    const int _gridWidth = 10;
    const int _gridHeight = 20;

    private static Polyomino[] getAllTetrominos()
    {
        var tetrominos = Ut.NewArray(
            "####",     // I
            "##,##",    // O
            "###,#",    // L
            "##,.##",   // S
            "###,.#"   // T
        );

        return tetrominos
            .Select(p => new Polyomino(p))
            .SelectMany(p => new[] { p, p.RotateClockwise(), p.RotateClockwise().RotateClockwise(), p.RotateClockwise().RotateClockwise().RotateClockwise() })
            .SelectMany(p => new[] { p, p.Reflect() })
            .Distinct()
            .ToArray();
    }

    private static List<PolyominoPlacement> getAllTetrominoPlacements() => (
        from poly in getAllTetrominos()
        from place in Coord.Cells(_gridWidth, _gridHeight)
        select new PolyominoPlacement(poly, place)).Where(p => p.IsInRange).ToList();

    private static (int[] solution, PolyominoPlacement[] polys)? generateTetrisFill(Random rnd)
    {
        var allPlacements = getAllTetrominoPlacements().Shuffle(rnd);
        var solutionTup = solvePolyominoPuzzle(new int?[_gridWidth * _gridHeight], 0, allPlacements, []).FirstOrNull();
        if (solutionTup != null)
            return (solutionTup.Value.solution, solutionTup.Value.polys);
        return null;
    }

    private static IEnumerable<(int[] solution, PolyominoPlacement[] polys)> solvePolyominoPuzzle(
        int?[] sofar,
        int pieceIx,
        List<PolyominoPlacement> possiblePlacements,
        List<PolyominoPlacement> polysSofar)
    {
        Coord? bestCell = null;
        int[] bestPlacementIxs = null;

        foreach (var tCell in Coord.Cells(_gridWidth, _gridHeight))
        {
            if (sofar[tCell.Index] != null)
                continue;
            var tPossiblePlacementIxs = possiblePlacements.SelectIndexWhere(pl => pl.Polyomino.Has((tCell.X - pl.Place.X + _gridWidth) % _gridWidth, (tCell.Y - pl.Place.Y + _gridHeight) % _gridHeight)).ToArray();
            if (tPossiblePlacementIxs.Length == 0)
                yield break;
            if (bestPlacementIxs == null || tPossiblePlacementIxs.Length < bestPlacementIxs.Length)
            {
                bestCell = tCell;
                bestPlacementIxs = tPossiblePlacementIxs;
            }
            if (tPossiblePlacementIxs.Length == 1)
                goto shortcut;
        }

        if (bestPlacementIxs == null)
        {
            yield return (sofar.Select(i => i.Value).ToArray(), polysSofar.ToArray());
            yield break;
        }

        shortcut:
        var cell = bestCell.Value;

        foreach (var placementIx in bestPlacementIxs.Reversed())
        {
            var placement = possiblePlacements[placementIx];
            var (poly, place) = placement;
            possiblePlacements.RemoveAt(placementIx);

            foreach (var c in poly.Cells)
                sofar[place.AddWrap(c).Index] = pieceIx;
            polysSofar.Add(new PolyominoPlacement(poly, place));

            var newPlacements = possiblePlacements
                .Where(p => p.Polyomino.Cells.All(c => sofar[p.Place.AddWrap(c).Index] == null))
                .ToList();

            foreach (var solution in solvePolyominoPuzzle(sofar, pieceIx + 1, newPlacements, polysSofar))
                yield return solution;

            polysSofar.RemoveAt(polysSofar.Count - 1);
            foreach (var c in poly.Cells)
                sofar[place.AddWrap(c).Index] = null;
        }
    }
}
