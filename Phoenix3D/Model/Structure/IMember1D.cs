using Phoenix3D.LinearAlgebra;
using Phoenix3D.Model.CrossSections;
using Phoenix3D.Optimization;

using System.Collections.Generic;


namespace Phoenix3D.Model
{
    public interface IMember1D : IMember
    {
        Node From { get; set; }
        Node To { get; set; }
        ICrossSection CrossSection { get; set; }
        double Length { get; set; }
        Vector Direction { get; set; }
        Vector Normal { get; set; }
        MatrixDense T { get; set; }

        Dictionary<LoadCase, List<double>> Nx { get; set; }
        (double,double) Buffer { get; set; }
        BucklingType BucklingType { get; set; }
        double BucklingLength { get; set; }
        int MinCompound { get; set; }
        int MaxCompound { get; set; }
        List<string> AllowedCrossSections { get; set; }
        Assignment Assignment { get; set; }
        void SetCrossSection(ICrossSection c);
        void SetGeometricProperties();
        void SetNormal(Vector n);
        void SetAssignment(Assignment Assignment);
        void SetGroupNumber(int GroupNumber);
        void ClearAssignment();
        IMember1D Clone();
        double LBArea { get; set; }
        double UBArea { get; set; }
        bool NormalUserDefined { get; set; }
        bool NormalOverwritten { get; set; }
        
    }

}
