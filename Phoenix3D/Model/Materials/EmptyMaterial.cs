

namespace Phoenix3D.Model.Materials
{
    public struct EmptyMaterial : IMaterial
    {
        public MaterialType Type {get; set;}
        public double Density { get; set; }
        public double E { get; set; }
        public double G { get; set; }
        public double PoissonRatio { get;  set; }
        public double ft { get; set; }
        public double fc { get; set; }
        public double gamma_0 { get; set; }
        public double gamma_1 { get; set; }
        public double kmod { get; set; }

        public EmptyMaterial(string name)
        {
            Type = MaterialType.Empty;
            Density = 0;
            E = 0;
            G = 0;
            PoissonRatio = 0;
            ft = 0;
            fc = 0;
            gamma_0 = 1;
            gamma_1 = 1;
            kmod = 1;
        }
    }
}
