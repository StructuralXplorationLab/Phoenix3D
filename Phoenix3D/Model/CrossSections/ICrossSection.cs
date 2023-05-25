using Phoenix3D.Model.Materials;

using System.Collections.Generic;


namespace Phoenix3D.Model.CrossSections
{
    public interface ICrossSection
    {
        string TypeName { get; }
        string Name { get; }
        double Area { get; }
        double Iy { get; }
        double Iz { get; }
        double It { get; }
        double Wy { get; }
        double Wz { get; }
        double Wt { get; }
        double Avy { get; }
        double Avz { get; }
        List<int> PossibleCompounds { get; }
        List<(double, double)> Polygon { get; }
        double dy { get; }
        double dz { get; }
        double cy { get; }
        double cz { get; }
        void SetSectionProperties();
        void SetPolygon();
        void SetPossibleCompounds(List<int> Compounds);
        string ToString();
        double GetTensionResistance(IMaterial Material);
        List<double> GetBucklingResistance(IMaterial Material, BucklingType BucklingType, double BucklingLength);
        double GetUtilization(IMaterial Material, BucklingType BucklingType, double BucklingLength, bool Plastic, double Nx, double Vy, double Vz, double My, double Mz, double Mt);
        
    }

}
