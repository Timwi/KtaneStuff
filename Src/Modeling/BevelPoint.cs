namespace KtaneStuff.Modeling
{
    sealed class BevelPoint
    {
        public double Into { get; private set; }
        public double Y { get; private set; }
        public Normal Before { get; private set; }
        public Normal After { get; private set; }

        public BevelPoint(double into, double y, Normal before, Normal after)
        {
            Into = into;
            Y = y;
            Before = before;
            After = after;
        }
    }
}
