

namespace Phoenix3D.Model.Materials
{

    public struct Steel : IMaterial
    {
        public MaterialType Type { get; set; }
        public string Name { get; set; }
        public double Density { get; set; }
        public double E { get; set; }
        public double G { get; set; }
        public double PoissonRatio { get; set; }
        public double ft { get; set; }
        public double fc { get; set; }
        public double gamma_0 { get; set; }
        public double gamma_1 { get; set; }
        public double kmod { get; set; }

        public Steel(double Density) : this(Density, default) { }
        public Steel(double Density, double E) : this(Density, E, default) { }
        public Steel(double Density, double E, double PoissonRatio) : this(Density, E, PoissonRatio, default) { }
        public Steel(double Density, double E, double PoissonRatio, double ft, double fc) : this(Density, E, PoissonRatio, ft, fc, default) { }
        public Steel(double Density, double E, double PoissonRatio, double ft, double fc, double gamma_0) : this(Density, E, PoissonRatio, ft, fc, gamma_0, default) { }
        public Steel(double Density = 7850, double E = 210000, double PoissonRatio = 0.3, double ft = 235, double fc = 235, double gamma_0 = 1.0, double gamma_1 = 1.1, double kmod = 1.0)
        {

            this.Type = MaterialType.Metal;
            this.Density = Density;
            this.E = E;
            this.PoissonRatio = PoissonRatio;
            this.G = this.E / (2 * (1 + PoissonRatio));
            this.ft = ft;
            this.fc = fc;
            this.gamma_0 = gamma_0;
            this.gamma_1 = gamma_1;
            this.kmod = kmod;

            this.Name = "Steel - Density: " + Density.ToString() + "kg/m3 ft: " + this.ft.ToString() + "N/mm2 fc: " + fc.ToString() + "N/mm2 E: " + E.ToString() + "N/mm2";
        }
        public override string ToString()
        {
            return Name.ToString();
        }
    }
}
