using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Phoenix3D.Model;
using Phoenix3D.Reuse;
using Phoenix3D.LCA;
using Gurobi;


namespace Phoenix3D.Optimization.SAND
{
    public static class SANDGurobiDiscreteNP
    {
        public static GRBVar[] GetGurobiAssignmentVariables(GRBModel Model, Structure Structure, Stock Stock)
        {
            return Model.AddVars(Structure.Members.OfType<IMember1D>().Count() * Stock.ElementGroups.Count, GRB.BINARY);
        }
        public static Dictionary<LoadCase, GRBVar[]> GetGurobiMemberElongationVariables(GRBModel Model, Structure Structure, List<LoadCase> LoadCases, Stock Stock)
        {
            Dictionary<LoadCase, GRBVar[]> MemberForces = new Dictionary<LoadCase, GRBVar[]>();
            int m = Structure.Members.OfType<Bar>().Count();
            int g = Stock.ElementGroups.Count;
            foreach (LoadCase LC in LoadCases)
            {
                MemberForces.Add(LC, Model.AddVars(Enumerable.Repeat(-GRB.INFINITY, m * g).ToArray(), Enumerable.Repeat(GRB.INFINITY, m * g).ToArray(), new double[m * g], Enumerable.Repeat(GRB.CONTINUOUS, m * g).ToArray(), null));
            }
            return MemberForces;
        }
        public static Dictionary<LoadCase, GRBVar[]> GetGurobiDisplacementVariables(GRBModel Model, Structure Structure, List<LoadCase> LoadCases)
        {
            Dictionary<LoadCase, GRBVar[]> Displacements = new Dictionary<LoadCase, GRBVar[]>();
            foreach (LoadCase LC in LoadCases)
            {
                GRBVar[] u = new GRBVar[Structure.NFreeTranslations];
                foreach (Node N in Structure.Nodes)
                {
                    for (int d = 0; d < 3; d++)
                    {
                        if (N.Fix[d])
                            continue;
                        else if (N.DisplacementBounds.ContainsKey(LC))
                            u[N.ReducedDofsTruss[d]] = Model.AddVar(N.DisplacementBounds[LC].LB[d], N.DisplacementBounds[LC].UB[d], 0, GRB.CONTINUOUS, "u_LC_" + LC.Name + "Node" + N.Number.ToString() + "d" + d.ToString());
                        else
                            u[N.ReducedDofsTruss[d]] = Model.AddVar(-GRB.INFINITY, GRB.INFINITY, 0, GRB.CONTINUOUS, "u_LC_" + LC.Name + "Node" + N.Number.ToString() + "d" + d.ToString());
                    }
                }
                Displacements.Add(LC, u);
            }
            return Displacements;
        }
        public static void SetObjective(Objective Objective, GRBModel model, GRBVar[] T, Structure Structure, Stock Stock)
        {
            GRBLinExpr obj = new GRBLinExpr();

            switch(Objective)
            {
                case Objective.MinStructureMass:
                    {
                        foreach(IMember1D M in Structure.Members)
                        {
                            for(int j = 0; j < Stock.ElementGroups.Count; j++)
                            {
                                obj += T[M.Number*Stock.ElementGroups.Count + j] * M.Length * Stock.ElementGroups[j].Material.Density * Stock.ElementGroups[j].CrossSection.Area;
                            }
                        }
                        model.SetObjective(obj);
                        break;
                    }

                default:
                    {
                        foreach (Bar M in Structure.Members)
                        {
                            for (int j = 0; j < Stock.ElementGroups.Count; j++)
                            {
                                obj += T[M.Number * Stock.ElementGroups.Count + j] * M.Length * Stock.ElementGroups[j].Material.Density * Stock.ElementGroups[j].CrossSection.Area;
                            }
                        }
                        model.SetObjective(obj);
                        break;
                    }
            }

        }
        public static void AddAssignment(GRBModel model, GRBVar[] T, Structure Structure, Stock Stock, OptimOptions Options)
        {
            foreach(IMember1D M in Structure.Members)
            {
                GRBLinExpr assign = new GRBLinExpr();
                for(int j = 0; j < Stock.ElementGroups.Count; j++)
                {
                    if(M.TopologyFixed && Stock.ElementGroups[j].CrossSection.Area == 0)
                    {
                        assign.AddTerm(0.0, T[M.Number * Stock.ElementGroups.Count + j]);
                        T[M.Number * Stock.ElementGroups.Count + j].UB = 0;
                    }
                    else
                    {
                        assign.AddTerm(1.0, T[M.Number * Stock.ElementGroups.Count + j]);
                    }  
                }
                model.AddConstr(assign, '=', 1.0, "AssignmentBar" + M.Number.ToString());
                if (Options.SOS_Assignment)
                {
                    model.AddSOS(T.Skip(M.Number * Stock.ElementGroups.Count).Take(Stock.ElementGroups.Count).ToArray(), Enumerable.Range(1, Stock.ElementGroups.Count).Select(i => (double)i).ToArray(), GRB.SOS_TYPE1);
                }
            }
        }
        public static void AddEquilibrium(GRBModel model, GRBVar[] T, GRBVar[] MemberElongations, Structure Structure, LoadCase LC, Stock Stock, OptimOptions Options)
        {
            foreach(Node N in Structure.Nodes)
            {
                for(int d = 0; d < 3; d++)
                {
                    if (N.Fix[d])
                        continue;

                    GRBLinExpr f = 0;

                    if(N.PointLoads.ContainsKey(LC))
                        f.AddConstant(-N.PointLoads[LC].FM[d]);

                    foreach(IMember1D M in N.ConnectedMembers.Keys)
                    {
                        for (int j = 0; j < Stock.ElementGroups.Count; j++)
                        {

                            if (Options.Selfweight && d == 2)
                                f.AddTerm(Stock.ElementGroups[j].CrossSection.Area * M.Length * Stock.ElementGroups[j].Material.Density * LC.yg * 1e-5 * 0.5, T[M.Number * Stock.ElementGroups.Count + j]);

                            f.AddTerm(N.ConnectedMembers[M] * M.Direction[d] * Stock.ElementGroups[j].CrossSection.Area * M.Material.E / M.Length, MemberElongations[M.Number * Stock.ElementGroups.Count + j]);
                        }
                    }
                    model.AddConstr(f, '=', 0, "LC" + LC.Number.ToString()+"EquilibriumNode" + N.Number.ToString() + "dof" + d.ToString());
                }
            }
        }
        public static void AddCompatibility(GRBModel model, GRBVar[] MemberElongations, GRBVar[] Displacements, Structure Structure, LoadCase LC, Stock Stock)
        {
            foreach (IMember1D M in Structure.Members)
            {
                GRBLinExpr bTu = new GRBLinExpr();
                GRBLinExpr sumV = new GRBLinExpr();

                for (int d = 0; d < 3; d++)
                {
                    if (!M.From.Fix[d])
                        bTu.AddTerm(-M.Direction[d], Displacements[M.From.ReducedDofsTruss[d]]);
                    if (!M.To.Fix[d])
                        bTu.AddTerm(M.Direction[d] , Displacements[M.To.ReducedDofsTruss[d]]);
                }
                for (int j = 0; j < Stock.ElementGroups.Count; j++)
                {
                    sumV.AddTerm(1.0, MemberElongations[M.Number * Stock.ElementGroups.Count + j]);
                }
                model.AddConstr(bTu, '=', sumV, "CompatibilityLC" + LC.Name + "Member" + M.Number.ToString());
            }
        }
        public static void AddBigM(GRBModel model, GRBVar[] T, GRBVar[] MemberElongations, Structure Structure, LoadCase LC, Stock Stock, OptimOptions Options)
        {
            foreach (IMember1D M in Structure.Members)
            {
                for (int j = 0; j < Stock.ElementGroups.Count; j++)
                {
                    double emin;
                    double emax;
                    GetEminEmax(M, Stock.ElementGroups[j], LC, out emin, out emax);

                    model.AddConstr(T[M.Number*Stock.ElementGroups.Count + j] * emin, '<', MemberElongations[M.Number * Stock.ElementGroups.Count + j], "BigMemin" + LC.Name + "Member" + M.Number.ToString());
                    model.AddConstr(T[M.Number * Stock.ElementGroups.Count + j] * emax, '>', MemberElongations[M.Number * Stock.ElementGroups.Count + j], "BigMemax" + LC.Name + "Member" + M.Number.ToString());
                }
                if (Options.SOS_Continuous)
                {
                    model.AddSOS(MemberElongations.Skip(M.Number * Stock.ElementGroups.Count).Take(Stock.ElementGroups.Count).ToArray(), Enumerable.Range(1, Stock.ElementGroups.Count).Select(i => (double)i).ToArray(), GRB.SOS_TYPE1);
                }
            }
        }
        public static void AddStress(GRBModel model, GRBVar[] T, GRBVar[] MemberForces, Structure Structure, LoadCase LC, Stock Stock)
        {
            throw new NotImplementedException("Stress and buckling constraints not implemented here");
            /*
            foreach (IMember1D M in Structure.Members)
            {
                GRBLinExpr StressSumCompression = new GRBLinExpr();
                GRBLinExpr StressSumTension = new GRBLinExpr();
                for (int j = 0; j < Stock.ElementGroups.Count; j++)
                {
                    StressSumCompression += Stock.ElementGroups[j].CrossSection.GetBucklingResistance(Stock.ElementGroups[j].Material, M.BucklingType, M.BucklingLength).Max() * T[M.Number * Stock.ElementGroups.Count + j];
                    StressSumTension += Stock.ElementGroups[j].CrossSection.GetTensionResistance(Stock.ElementGroups[j].Material) * T[M.Number * Stock.ElementGroups.Count + j];
                }
                model.AddConstr(StressSumCompression <= MemberForces[M.Number], "LC" + LC.Number.ToString() + "Compression" + M.Number.ToString());
                model.AddConstr(StressSumTension >= MemberForces[M.Number], "LC" + LC.Number.ToString() + "Tension" + M.Number.ToString());
            }
            */

        }
        private static void GetEminEmax(IMember1D M, ElementGroup EG, LoadCase LC, out double emin, out double emax)
        {
            double deltamin = 0;
            double deltamax = 0;

            for (int d = 0; d < 3; d++)
            {
                if (M.To.DisplacementBounds.ContainsKey(LC) && M.From.DisplacementBounds.ContainsKey(LC))
                {
                    deltamin += M.To.DisplacementBounds[LC].LB[d] * Math.Max(M.To.ConnectedMembers[M] * M.Direction[d], 0) + M.From.DisplacementBounds[LC].UB[d] * Math.Min(M.From.ConnectedMembers[M] * M.Direction[d], 0);
                    deltamin += M.From.DisplacementBounds[LC].LB[d] * Math.Max(M.From.ConnectedMembers[M] * M.Direction[d], 0) + M.To.DisplacementBounds[LC].UB[d] * Math.Min(M.To.ConnectedMembers[M] * M.Direction[d], 0);
                }
                else
                    deltamin = -GRB.INFINITY;

                if (M.From.DisplacementBounds.ContainsKey(LC) && M.To.DisplacementBounds.ContainsKey(LC))
                {
                    deltamax += M.From.DisplacementBounds[LC].UB[d] * Math.Max(M.From.ConnectedMembers[M] * M.Direction[d], 0) + M.To.DisplacementBounds[LC].LB[d] * Math.Min(M.To.ConnectedMembers[M] * M.Direction[d], 0);
                    deltamax += M.To.DisplacementBounds[LC].UB[d] * Math.Max(M.To.ConnectedMembers[M] * M.Direction[d], 0) + M.From.DisplacementBounds[LC].LB[d] * Math.Min(M.From.ConnectedMembers[M] * M.Direction[d], 0);
                }
                else
                    deltamax = GRB.INFINITY;
            }

            //double epsilonmin = -GRB.INFINITY;
            //%double epsilonmax = GRB.INFINITY;
            double epsilonmin = -M.Length;
            double epsilonmax = M.Length;

            if (EG.CrossSection.Area > 0)
            {
                epsilonmin = -M.Length * M.Material.fc / M.Material.E;
                epsilonmax = M.Length * M.Material.ft / M.Material.E;
            }
            /*
            else
            {
                epsilonmin = -10 * M.Length * M.Material.fy / M.Material.E;
                epsilonmax = 10 * M.Length * M.Material.fy / M.Material.E;
            }
            */

            emin = Math.Max(deltamin, epsilonmin);
            emax = Math.Min(deltamax, epsilonmax);
        }
    }
}
