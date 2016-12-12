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
    }
}
