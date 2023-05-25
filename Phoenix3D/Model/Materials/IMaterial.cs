

namespace Phoenix3D.Model.Materials
{
    public interface IMaterial
    {
        MaterialType Type { get; set; }
        double Density { get; set; }
        double E { get; set; }
        double G { get; set; }
        double PoissonRatio { get; set; }
        double ft { get; set; }
        double fc { get; set; }
        double gamma_0 { get; set; }
        double gamma_1 { get; set; }
        double kmod { get; set; }
        string ToString();

    }

    public enum MaterialType
    {
        Empty = 0, Metal = 1, Timber = 2
    }
}
