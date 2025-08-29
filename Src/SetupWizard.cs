using PuzzleSolvers;
using RT.Util.ExtensionMethods;

namespace KtaneStuff;

internal static class SetupWizard
{
    public enum Operator
    {
        Add,
        Subtract,
        Multiply,
        Divide,
        Concatenate
    }

    public class Equation(int left, Operator op, int right, int result)
    {
        public int Left { get; private set; } = left;
        public Operator Op { get; private set; } = op;
        public int Right { get; private set; } = right;
        public int Result { get; set; } = result;
        public override string ToString() => $"{(char) ('A' + Left)} {Op switch { Operator.Add => "+", Operator.Subtract => "-", Operator.Multiply => "*", Operator.Divide => "/", _ => "||" }} {(char) ('A' + Right)} = {Result}";
        public Constraint Constraint => Op switch
        {
            Operator.Add => new TwoCellLambdaConstraint(Left, Right, (a, b) => a + b == Result),
            Operator.Subtract => new TwoCellLambdaConstraint(Left, Right, (a, b) => a - b == Result),
            Operator.Multiply => new TwoCellLambdaConstraint(Left, Right, (a, b) => a * b == Result),
            Operator.Divide => new TwoCellLambdaConstraint(Left, Right, (a, b) => a == b * Result),
            _ => new TwoCellLambdaConstraint(Left, Right, (a, b) => a * 10 + b == Result)
        };
    }

    public static (int[] password, Equation[] equations) GeneratePasswordPuzzle()
    {
        const int numDigits = 6;
        const int numEqs = 6;

        tryAgain:
        var rnd = new Random();
        var password = Enumerable.Range(0, numDigits).Select(i => rnd.Next(0, 10)).ToArray();
        var allEquations = new List<Equation>();

        for (var left = 0; left < numDigits; left++)
            for (var right = 0; right < numDigits; right++)
            {
                // Commutative operators
                if (right > left)
                {
                    allEquations.Add(new Equation(left, Operator.Add, right, password[left] + password[right]));
                    allEquations.Add(new Equation(left, Operator.Multiply, right, password[left] * password[right]));
                }
                // Non-commutative operators
                if (right != left)
                {
                    allEquations.Add(new Equation(left, Operator.Subtract, right, password[left] - password[right]));
                    if (password[right] != 0 && password[left] % password[right] == 0)
                        allEquations.Add(new Equation(left, Operator.Divide, right, password[left] / password[right]));
                    allEquations.Add(new Equation(left, Operator.Concatenate, right, password[left] * 10 + password[right]));
                }
            }

        // Pick 6 equations with different variables and different results
        var generatedEquations = new List<Equation>();
        while (generatedEquations.Count < numEqs)
        {
            if (allEquations.Count == 0)
                goto tryAgain;
            var pick = rnd.Next(0, allEquations.Count);
            var newEq = allEquations[pick];
            generatedEquations.Add(newEq);
            allEquations.RemoveAll(eq => (eq.Left == newEq.Left && eq.Right == newEq.Right) || (eq.Left == newEq.Right && eq.Right == newEq.Left) || eq.Result == newEq.Result);
        }

        // Make sure that the puzzle is unique at this point
        Puzzle makePuzzle(IEnumerable<Equation> equations)
        {
            var puzzle = new Puzzle(numDigits, 0, 9);
            foreach (var eq in equations)
                puzzle.AddConstraint(eq.Constraint);
            return puzzle;
        }

        if (makePuzzle(generatedEquations).Solve().Skip(1).Any())
            goto tryAgain;

        // Swap the results of two equations
        var swEq1 = rnd.Next(0, numEqs);
        var swEq2 = rnd.Next(0, numEqs - 1);
        if (swEq2 >= swEq1)
            swEq2++;
        void swap(int eqIx1, int eqIx2) => (generatedEquations[eqIx2].Result, generatedEquations[eqIx1].Result) = (generatedEquations[eqIx1].Result, generatedEquations[eqIx2].Result);
        swap(swEq1, swEq2);

        // Make sure that the puzzle is still unique (no other swaps produce a solution)
        for (var eq1 = 0; eq1 < numEqs; eq1++)
            for (var eq2 = eq1 + 1; eq2 < numEqs; eq2++)
                if (eq1 != swEq1 || eq2 != swEq2)
                {
                    swap(eq1, eq2);
                    if (makePuzzle(generatedEquations).Solve().Any())
                        goto tryAgain;
                    swap(eq1, eq2);
                }

        return (password, generatedEquations.ToArray());
    }
}
