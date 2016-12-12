using RT.Util.ExtensionMethods;

namespace KtaneStuff.Modeling
{
    public struct MeshVertexInfo
    {
        public Pt Location { get; private set; }
        public Normal NormalBeforeX { get; private set; }
        public Normal NormalBeforeY { get; private set; }
        public Normal NormalAfterX { get; private set; }
        public Normal NormalAfterY { get; private set; }
        public Pt? NormalOverride { get; private set; }

        public MeshVertexInfo(Pt pt, Normal befX, Normal afX, Normal befY, Normal afY) : this()
        {
            Location = pt;
            NormalBeforeX = befX;
            NormalAfterX = afX;
            NormalBeforeY = befY;
            NormalAfterY = afY;
        }

        public MeshVertexInfo(Pt pt, Pt normalOverride) : this()
        {
            Location = pt;
            NormalOverride = normalOverride;
        }

        public override string ToString()
        {
            return "{0}, bef={1},{2}, aft={3},{4}".Fmt(Location, NormalBeforeX, NormalBeforeY, NormalAfterX, NormalAfterY);
        }
    }
}
