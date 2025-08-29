using RT.Modeling;

namespace KtaneStuff;

internal sealed class BevelPoint(double x, double y, Normal? before = null, Normal? after = null, Pt? normal = null)
{
    public double X { get; private set; } = x;
    public double Y { get; private set; } = y;
    public Normal Before { get; private set; } = before ?? Normal.Mine;
    public Normal After { get; private set; } = after ?? Normal.Mine;
    public Pt? NormalOverride { get; private set; } = normal;
}
