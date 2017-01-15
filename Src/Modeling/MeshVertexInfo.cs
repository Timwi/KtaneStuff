using RT.Util.ExtensionMethods;
using RT.Util.Geometry;

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
        public PointD? Texture { get; private set; }

        public MeshVertexInfo(Pt pt, Normal befX, Normal afX, Normal befY, Normal afY, PointD? texture = null) : this()
        {
            Location = pt;
            NormalBeforeX = befX;
            NormalAfterX = afX;
            NormalBeforeY = befY;
            NormalAfterY = afY;
            Texture = texture;
        }

        public MeshVertexInfo(Pt pt, Pt normalOverride, PointD? texture = null) : this()
        {
            Location = pt;
            NormalOverride = normalOverride;
            Texture = texture;
        }

        public override string ToString()
        {
            return "{0}, bef={1},{2}, aft={3},{4}".Fmt(Location, NormalBeforeX, NormalBeforeY, NormalAfterX, NormalAfterY);
        }

        public MeshVertexInfo WithTexture(PointD texture)
        {
            return NormalOverride == null
                ? new MeshVertexInfo(Location, NormalBeforeX, NormalAfterX, NormalBeforeY, NormalAfterY, texture)
                : new MeshVertexInfo(Location, NormalOverride.Value, texture);
        }
        public MeshVertexInfo WithTexture(double x, double y) { return WithTexture(new PointD(x, y)); }
    }
}
