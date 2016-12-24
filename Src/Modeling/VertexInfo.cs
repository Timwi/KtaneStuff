using RT.Util.Geometry;

namespace KtaneStuff.Modeling
{
    public struct VertexInfo
    {
        public Pt Location { get; private set; }
        public Pt? Normal { get; private set; }
        public PointD? Texture { get; private set; }

        public VertexInfo(Pt loc, Pt? normal, PointD? texture = null) : this()
        {
            Location = loc;
            Normal = normal;
            Texture = texture;
        }

        public override string ToString()
        {
            if (Normal == null)
                return Texture == null ? Location.ToString() : $"{Location}/{Texture.Value}";
            return $"{Location}/{Texture}/{Normal.Value}";
        }

        public VertexInfo WithTexture(PointD texture) { return new VertexInfo(Location, Normal, texture); }
        public VertexInfo WithTexture(double x, double y) { return new VertexInfo(Location, Normal, new PointD(x, y)); }
    }
}
