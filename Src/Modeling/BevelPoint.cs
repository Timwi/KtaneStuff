namespace KtaneStuff.Modeling
{
    sealed class BevelPoint
    {
        public double X { get; private set; }
        public double Y { get; private set; }
        public Normal Before { get; private set; }
        public Normal After { get; private set; }
        public Pt? NormalOverride { get; private set; }

        public BevelPoint(double x, double y, Normal? before = null, Normal? after = null, Pt? normal = null)
        {
            X = x;
            Y = y;
            Before = before ?? Normal.Mine;
            After = after ?? Normal.Mine;
            NormalOverride = normal;
        }
    }
}
