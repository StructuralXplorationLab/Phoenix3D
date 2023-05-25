using Phoenix3D.Model;
using Phoenix3D.LCA;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Phoenix3D.Reuse;
using Phoenix3D.Model.CrossSections;

namespace Phoenix3D.Optimization
{
    
    public class Result
    {
        public double StockMass { get; private set; } = 0;
        public double StructureMass { get; private set; } = 0;
        public double ReuseMass { get; private set; } = 0;
        public double NewMass { get; private set; } = 0;
        public double Waste { get; private set; } = 0;
        public double EnvironmentalImpact { get; private set; } = 0;
        public int ReusedMembers { get; private set; } = 0;
        public int NewMembers { get; private set; } = 0;
        public int TotalMembers { get; private set; } = 0;
        public double ReuseRateMass { get; private set; } = 0;
        public double ReuseRateMembers { get; private set; } = 0;
        public double MaxMemberMass { get; private set; } = 0;
        public double MaxMemberImpact { get; private set; } = 0;
        public double[] Utilization { get; private set; }

        public Result() { }

        public Result(Structure Structure, Stock Stock, ILCA LCA = null) 
        {
            foreach (IMember1D M in Structure.Members)
            {
                double MemberMass = 0;
                Assignment A = M.Assignment;
                for (int i = 0; i < A.ElementGroups.Count; i++)
                {
                    ElementGroup EG = A.ElementGroups[i];
                    int n = A.ElementIndices[i];

                    TotalMembers++;
                    if(EG.Type == ElementType.Reuse)
                    {
                        ReusedMembers++;
                        if (!EG.AlreadyCounted[n])
                        {
                            StockMass += EG.Material.Density * EG.CrossSection.Area * EG.Length;
                            EG.AlreadyCounted[n] = true;
                        }
                        ReuseMass += EG.Material.Density * EG.CrossSection.Area * M.Length;
                    }
                    else if(EG.Type == ElementType.New)
                    {
                        NewMembers++;
                        NewMass += EG.Material.Density * EG.CrossSection.Area * M.Length;
                    }
                    StructureMass += EG.Material.Density * EG.CrossSection.Area * M.Length;
                    MemberMass += EG.Material.Density * EG.CrossSection.Area * M.Length;
                }
                MaxMemberMass = Math.Max(MaxMemberMass, MemberMass);
            }
            Stock.ResetAlreadyCounted();

            Waste = StockMass - ReuseMass;

            ReuseRateMass = ReuseMass / StructureMass;
            ReuseRateMembers = (double)ReusedMembers / TotalMembers;
            double _maxmemberimpact = 0;
            if (!(LCA is null))
            {
                EnvironmentalImpact = LCA.ReturnTotalImpact(Structure, out _maxmemberimpact);
            }
            MaxMemberImpact = _maxmemberimpact;
            Utilization = GetUtilization(Structure);
        }

        public Result(Structure Structure, ILCA LCA = null)
        {
            foreach(IMember1D M in Structure.Members)
            {
                StructureMass += M.CrossSection.Area + M.Material.Density * M.Length;
                MaxMemberMass = Math.Max(MaxMemberMass, M.Material.Density * M.CrossSection.Area * M.Length);
                if (!(LCA is null))
                {
                    double MemberImpact = LCA.ReturnMemberImpact(M);
                    EnvironmentalImpact += MemberImpact;
                    MaxMemberImpact = Math.Max(MaxMemberImpact, MemberImpact);
                }
            }

            Utilization = GetUtilization(Structure.Members.OfType<IMember1D>().ToList());
        }


        public List<double> GetResultsAsAList()
        {
            return new List<double>() { StockMass, StructureMass, ReuseMass,
                                        NewMass, Waste, ReusedMembers, NewMembers,
                                        TotalMembers, ReuseRateMass, ReuseRateMembers, EnvironmentalImpact};

        }

        public static double[] GetUtilization(Structure Structure)
        {
            double[] Utilization = new double[Structure.Members.OfType<IMember1D>().Count()];

            int i = 0;
            foreach (IMember1D M in Structure.Members)
            {
                foreach (LoadCase LC in M.Nx.Keys)
                {
                    Utilization[i] = Math.Max(Utilization[i], GetMemberUtilization(M, LC));   
                }
                i++;
            }
            return Utilization;
        }
        public static double[] GetUtilization(List<IMember1D> Members, LoadCase LC = null)
        {
            double[] Utilization = new double[Members.Count];

            int i = 0;
            foreach (IMember1D M in Members)
            {
                Utilization[i] = GetMemberUtilization(M, LC);
                i++;
            }
            return Utilization;
        }

        public static double GetMemberUtilization(IMember1D M, LoadCase LC)
        {
            switch(M)
            {
                case Bar Bar: return GetBarUtilization(Bar, LC);
                case Beam Beam: return GetBeamUtilization(Beam, LC);
                default: return GetBarUtilization(M as Bar, LC);
            }
        }

        public static double GetBarUtilization(Bar M, LoadCase LC)
        {
            double CapacityTension = 0;
            double CapacityCompression = 0;

            if (M.Nx.Count == 0)
            {
                return 0;
            }
            if (LC is null)
                return 0;
            if (!(M.Assignment is null) && M.Assignment.ElementGroups.Count != 0)
            {
                foreach (ElementGroup EG in M.Assignment.ElementGroups)
                {
                    if (EG.Type == ElementType.Zero)
                    {
                        return 0;
                    }
                    CapacityTension += EG.CrossSection.GetTensionResistance(EG.Material);
                    CapacityCompression += EG.CrossSection.GetBucklingResistance(EG.Material, M.BucklingType, M.BucklingLength).Max();
                }
                double Nt = Math.Max(0, M.Nx[LC].Max());
                double Nc = Math.Min(0, M.Nx[LC].Min());

                double UtilTension = Nt / CapacityTension;
                double UtilCompression = Nc / CapacityCompression;
                return Math.Max(UtilTension, UtilCompression);
            }
            else
            {
                double Nt = Math.Max(0, M.Nx[LC].Max());
                double Nc = Math.Min(0, M.Nx[LC].Min());

                CapacityTension = M.CrossSection.GetTensionResistance(M.Material);
                CapacityCompression = M.CrossSection.GetBucklingResistance(M.Material, M.BucklingType, M.BucklingLength).Max();

                double UtilTension = Nt / CapacityTension;
                double UtilCompression = Nc / CapacityCompression;

                return Math.Max(UtilTension, UtilCompression);
            }
        }

        public static double GetBeamUtilization(Beam Beam, LoadCase LC)
        {
            if (!(Beam.Assignment is null) && Beam.Assignment.ElementGroups.Count != 0)
            {
                double Divisor = Beam.Assignment.ElementGroups.Count;
                double max_util = 0;
                foreach (ElementGroup EG in Beam.Assignment.ElementGroups)
                {
                    for (int i = 0; i < Beam.Nx[LC].Count; i++)
                    {
                        double Nx = Beam.Nx[LC][i] / Divisor;
                        double Vy = Beam.Vy[LC][i] / Divisor;
                        double Vz = Beam.Vz[LC][i] / Divisor;
                        double My = Beam.My[LC][i] / Divisor;
                        double Mz = Beam.Mz[LC][i] / Divisor;
                        double Mt = Beam.Mt[LC][i] / Divisor;
                        max_util = Math.Max(max_util, EG.CrossSection.GetUtilization(EG.Material, Beam.BucklingType, Beam.BucklingLength, false, Nx, Vy, Vz, My, Mz, Mt));
                    }
                }
                return max_util;
            }
            else
            {
                if (LC is null)
                    return 0;
                double max_util = 0;
                for (int i = 0; i < Beam.Nx[LC].Count; i++)
                {
                    double Nx = Beam.Nx[LC][i];
                    double Vy = Beam.Vy[LC][i];
                    double Vz = Beam.Vz[LC][i];
                    double My = Beam.My[LC][i];
                    double Mz = Beam.Mz[LC][i];
                    double Mt = Beam.Mt[LC][i];
                    max_util = Math.Max(max_util, Beam.CrossSection.GetUtilization(Beam.Material, Beam.BucklingType, Beam.BucklingLength, false, Nx, Vy, Vz, My, Mz, Mt));
                }
                return max_util; 
            }
        }

        public override string ToString()
        {
            string s = "---- R E S U L T S ---- \n \n";
            s += "Stock Mass: \t \t \t \t \t \t \t \t \t" + Math.Round(StockMass,1).ToString() + "\n";
            s += "Structure Mass: \t \t \t \t " + Math.Round(StructureMass,1).ToString() + "\n";
            s += "Reuse Mass: \t \t \t \t \t \t \t \t " + Math.Round(ReuseMass,1).ToString() + "\n";
            s += "New Mass: \t \t \t \t \t \t \t \t \t \t " + Math.Round(NewMass,1).ToString() + "\n";
            s += "Waste: \t \t \t \t \t \t \t \t \t \t \t \t \t " + Math.Round(Waste,1).ToString() + "\n";
            s += "\n";
            s += "Reused Members: \t \t \t \t " + ReusedMembers.ToString() + "\n";
            s += "New Members: \t \t \t \t \t \t \t " + NewMembers.ToString() + "\n";
            s += "Total Members: \t \t \t \t \t " + TotalMembers.ToString() + "\n";
            s += "\n";
            s += "Reuse Rate Mass: \t \t \t " + Math.Round(ReuseRateMass,2).ToString() + "\n";
            s += "Reuse Rate Members: " + Math.Round(ReuseRateMembers,2).ToString() + "\n";
            s += "\n";
            s += "Environmental Impact: " + Math.Round(EnvironmentalImpact,1).ToString() + "\n";

            return s;
        }
        public Result Clone()
        {
            var new_res = new Result();
            new_res.StockMass = StockMass;
            new_res.StructureMass = StructureMass;
            new_res.ReuseMass = ReuseMass;
            new_res.NewMass = NewMass;
            new_res.Waste = Waste;
            new_res.EnvironmentalImpact = EnvironmentalImpact;
            new_res.ReusedMembers = ReusedMembers;
            new_res.NewMembers = NewMembers;
            new_res.TotalMembers = TotalMembers;
            new_res.ReuseRateMass = ReuseRateMass;
            new_res.ReuseRateMembers = ReuseRateMembers;
            new_res.MaxMemberMass = MaxMemberMass;
            new_res.MaxMemberImpact = MaxMemberImpact;
            new_res.Utilization = (double[])Utilization.Clone();

            return new_res;
        }
    }
}
