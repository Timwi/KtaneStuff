using PuzzleSolvers;

namespace KtaneStuff
{
    public sealed class DeBruijnConstraint : Constraint
    {
        public DeBruijnConstraint() : base(null)
        {
        }

        public override ConstraintResult Process(SolverState state)
        {
            if (state.LastPlacedCell == null)
                return null;

            // Every digit may occur only four times
            var counts = new int[4];
            for (var c = 0; c < 16; c++)
                if (state[c] != null)
                    counts[state[c].Value]++;
            for (var v = 0; v < 4; v++)
                if (counts[v] == 4)
                    for (var c = 0; c < 16; c++)
                        state.MarkImpossible(c, v);

            // Once two digits are placed next to one another, make sure the same pair does not occur another time
            var pairsAlreadyPlaced = new bool[16];
            for (var c = 0; c < 16; c++)
                if (state[c] != null && state[(c + 1) % 16] != null)
                    pairsAlreadyPlaced[state[c].Value * 4 + state[(c + 1) % 16].Value] = true;
            for (var c = 0; c < 16; c++)
                if (state[c] != null)
                {
                    if (state[(c + 15) % 16] == null)
                        for (var v = 0; v < 4; v++)
                            if (pairsAlreadyPlaced[v * 4 + state[c].Value])
                                state.MarkImpossible((c + 15) % 16, v);
                    if (state[(c + 1) % 16] == null)
                        for (var v = 0; v < 4; v++)
                            if (pairsAlreadyPlaced[state[c].Value * 4 + v])
                                state.MarkImpossible((c + 1) % 16, v);
                }

            return null;
        }
    }
}
