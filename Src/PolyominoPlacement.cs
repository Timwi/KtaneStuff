namespace KtaneStuff;

public struct PolyominoPlacement(Polyomino poly, Coord place) : IEquatable<PolyominoPlacement>
{
    public Polyomino Polyomino { get; private set; } = poly;
    public Coord Place { get; private set; } = place;

    public readonly bool Equals(PolyominoPlacement other) => Polyomino == other.Polyomino && Place == other.Place;
    public override readonly bool Equals(object obj) => obj is PolyominoPlacement other && Equals(other);

    public static bool operator ==(PolyominoPlacement one, PolyominoPlacement two) => one.Equals(two);
    public static bool operator !=(PolyominoPlacement one, PolyominoPlacement two) => !one.Equals(two);

    public override readonly int GetHashCode()
    {
        var hashCode = 1291772507 + Polyomino.GetHashCode();
        hashCode = hashCode * -1521134295 + Place.GetHashCode();
        return hashCode;
    }

    public readonly void Deconstruct(out Polyomino poly, out Coord place)
    {
        poly = Polyomino;
        place = Place;
    }

    public readonly bool Touches(PolyominoPlacement other)
    {
        foreach (var c in Polyomino.Cells)
            foreach (var oc in other.Polyomino.Cells)
                if (Place.AddWrap(c).AdjacentToWrap(other.Place.AddWrap(oc)))
                    return true;
        return false;
    }

    public readonly bool Covers(Coord cell)
    {
        foreach (var c in Polyomino.Cells)
            if (Place.AddWrap(c) == cell)
                return true;
        return false;
    }
}
