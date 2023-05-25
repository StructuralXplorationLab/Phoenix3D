using Phoenix3D.Model.Materials;

using System.Collections.Generic;


namespace Phoenix3D.Model.CrossSections
{
    
    public class EmptySection : ICrossSection
    {
        public string Name { get; private set; }
        public double Area { get; private set; }
        public double Iy { get; private set; }
        public double Iz { get; private set; }
        public double It { get; private set; }
        public double Wy { get; private set; }
        public double Wz { get; private set; }
        public double Wt { get; private set; }
        public double dy { get; private set; }
        public double dz { get; private set; }
        public double cy { get; private set; }
        public double cz { get; private set; }
        public double Avy { get; private set; }
        public double Avz { get; private set; }
        public List<int> PossibleCompounds { get; private set; }
        public List<(double, double)> Polygon { get; private set; }
        public string TypeName { get; private set; }



        public EmptySection()
        {
            Name = "ES";
            Area = 0;
            Iy = 0;
            Iz = 0;
            It = 0;
            Wy = 0;
            Wz = 0;
            Wt = 0;
            Avy = 0;
            Avz = 0;
            dy = 0;
            dz = 0;
            cy = 0;
            cz = 0;

            PossibleCompounds = new List<int>() { 0 };

            Polygon = new List<(double, double)>() { };

            TypeName = "ES";
        }

        public double GetTensionResistance(IMaterial Material)
        {
            return 0;
        }
        public List<double> GetBucklingResistance(IMaterial Material, BucklingType BucklingType, double BucklingLength)
        {
            return new List<double>() { 0 };
        }

        public void SetPolygon()
        {
            return;
        }

        public void SetPossibleCompounds(List<int> Compounds)
        {
            return;
        }

        public void SetSectionProperties()
        {
            return;
        }

        public double GetUtilization(IMaterial Material, BucklingType BucklingType, double BucklingLength, bool Plastic, double Nx, double Vy, double Vz, double My, double Mz, double Mt)
        {
            return 0;
        }
    }
}
