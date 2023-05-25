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
    
    public static class SANDGurobiDiscreteRS
    {
        public static GRBVar[] GetGurobiAssignmentVariables(GRBModel Model, Structure Structure, Stock Stock)
        {
            return Model.AddVars(Structure.Members.OfType<IMember1D>().Count() * Stock.ElementGroups.Count, GRB.BINARY);
        }
        public static Dictionary<LoadCase, GRBVar[]> GetGurobiMemberForceVariables(GRBModel Model, Structure Structure, List<LoadCase> LoadCases, Stock Stock, OptimOptions Options)
        {
            Dictionary<LoadCase, GRBVar[]> MemberForces = new Dictionary<LoadCase, GRBVar[]>();
            int m = Structure.Members.OfType<Bar>().Count();
            int g = Stock.ElementGroups.Count;
            foreach (LoadCase LC in LoadCases)
            {
                if (Options.Compatibility)
                {
                    MemberForces.Add(LC, Model.AddVars(Enumerable.Repeat(-GRB.INFINITY, m * g).ToArray(), Enumerable.Repeat(GRB.INFINITY, m * g).ToArray(), new double[m * g], Enumerable.Repeat(GRB.CONTINUOUS, m * g).ToArray(), null));
                }
                else
                {
                    MemberForces.Add(LC, Model.AddVars(Enumerable.Repeat(-GRB.INFINITY, m).ToArray(), Enumerable.Repeat(GRB.INFINITY, m).ToArray(), new double[m], Enumerable.Repeat(GRB.CONTINUOUS, m).ToArray(), null));
                }
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

                if (Options.SOS_Assignment && !M.TopologyFixed)
                {
                    model.AddSOS(T.Skip(M.Number * Stock.ElementGroups.Count).Take(Stock.ElementGroups.Count).ToArray(), Enumerable.Range(1, Stock.ElementGroups.Count).Select(i => (double)i).ToArray(), GRB.SOS_TYPE1);
                }
                else
                {
                    GRBLinExpr assign = new GRBLinExpr();
                    for (int j = 0; j < Stock.ElementGroups.Count; j++)
                    {
                        assign.AddTerm(1.0, T[M.Number * Stock.ElementGroups.Count + j]);
                    }
                    
                    if(M.TopologyFixed)
                        model.AddConstr(assign, '=', 1.0, "AssignmentBar" + M.Number.ToString());
                    else
                        model.AddConstr(assign, '<', 1.0, "AssignmentBar" + M.Number.ToString());
                }
            }
        }
        public static void AddEquilibrium(GRBModel model, GRBVar[] T, GRBVar[] MemberForces, Structure Structure, LoadCase LC, Stock Stock, OptimOptions Options)
        {
            foreach(Node N in Structure.Nodes)
            {
                for(int d = 0; d < 3; d++)
                {
                    if (N.Fix[d])
                        continue;

                    GRBLinExpr f = 0;

                    if(N.PointLoads.ContainsKey(LC))
                        f -= N.PointLoads[LC].FM[d];

                    foreach(IMember1D M in N.ConnectedMembers.Keys)
                    {
                        if (Options.Compatibility || Options.Selfweight)
                        {
                            for (int j = 0; j < Stock.ElementGroups.Count; j++)
                            {
                                if (d == 2 && Options.Selfweight)
                                {
                                    f += T[M.Number * Stock.ElementGroups.Count + j] *Stock.ElementGroups[j].CrossSection.Area * M.Length * Stock.ElementGroups[j].Material.Density * LC.yg * 1e-5 * 0.5;
                                }
                                if (Options.Compatibility)
                                {
                                    f += N.ConnectedMembers[M] * M.Direction[d] * MemberForces[M.Number * Stock.ElementGroups.Count + j];
                                }
                            }
                        }
                        if(!Options.Compatibility)
                        {
                            f += N.ConnectedMembers[M] * M.Direction[d] * MemberForces[M.Number];
                        }
                    }
                    model.AddConstr(f, '=', 0, "LC" + LC.Number.ToString()+"EquilibriumNode" + N.Number.ToString() + "dof" + d.ToString());
                }
            }
        }
        public static void AddStress(GRBModel model, GRBVar[] T, GRBVar[] MemberForces, Structure Structure, LoadCase LC, Stock Stock, OptimOptions Options)
        {
            if (Options.Compatibility)
            {
                foreach (IMember1D M in Structure.Members)
                {
                    for (int j = 0; j < Stock.ElementGroups.Count; j++)
                    {
                        //model.AddConstr(Stock.ElementGroups[j].CrossSection.GetBucklingResistance(M.BucklingType, M.Material, M.BucklingLength).Min() * T[M.Number * Stock.ElementGroups.Count + j] <= MemberForces[M.Number * Stock.ElementGroups.Count + j], "LC" + LC.Number.ToString() + "Compression" + M.Number.ToString()+ "_" + j.ToString());
                        //model.AddConstr(Stock.ElementGroups[j].CrossSection.Area * M.Material.fy / M.Material.gamma_0 * T[M.Number * Stock.ElementGroups.Count + j] >= MemberForces[M.Number * Stock.ElementGroups.Count + j], "LC" + LC.Number.ToString() + "Tension" + M.Number.ToString() + "_" + j.ToString());

                        model.AddConstr(Stock.ElementGroups[j].CrossSection.GetBucklingResistance(M.Material, M.BucklingType, M.BucklingLength).Max() * T[M.Number * Stock.ElementGroups.Count + j], '<', MemberForces[M.Number * Stock.ElementGroups.Count + j], "LC" + LC.Number.ToString() + "Compression" + M.Number.ToString() + "_" + j.ToString());
                        model.AddConstr(Stock.ElementGroups[j].CrossSection.GetTensionResistance(Stock.ElementGroups[j].Material) * T[M.Number * Stock.ElementGroups.Count + j], '>', MemberForces[M.Number * Stock.ElementGroups.Count + j], "LC" + LC.Number.ToString() + "Tension" + M.Number.ToString() + "_" + j.ToString());
                    }
                    if (Options.SOS_Continuous)
                    {
                        model.AddSOS(MemberForces.Skip(M.Number * Stock.ElementGroups.Count).Take(Stock.ElementGroups.Count).ToArray(), Enumerable.Range(1, Stock.ElementGroups.Count).Select(i => (double)i).ToArray(), GRB.SOS_TYPE1);
                    }
                }
            }
            else
            {
                foreach (IMember1D M in Structure.Members)
                {
                    GRBLinExpr StressSumCompression = new GRBLinExpr();
                    GRBLinExpr StressSumTension = new GRBLinExpr();
                    for (int j = 0; j < Stock.ElementGroups.Count; j++)
                    {
                        StressSumCompression += Stock.ElementGroups[j].CrossSection.GetBucklingResistance(M.Material, M.BucklingType, M.BucklingLength).Max() * T[M.Number * Stock.ElementGroups.Count + j];
                        StressSumTension += Stock.ElementGroups[j].CrossSection.GetTensionResistance(Stock.ElementGroups[j].Material) * T[M.Number * Stock.ElementGroups.Count + j];
                    }
                    model.AddConstr(StressSumCompression, '<', MemberForces[M.Number], "LC" + LC.Number.ToString() + "Compression" + M.Number.ToString());
                    model.AddConstr(StressSumTension, '>', MemberForces[M.Number], "LC" + LC.Number.ToString() + "Tension" + M.Number.ToString());
                }
            }
        }
        public static void AddCompatibility(GRBModel model, GRBVar[] T, GRBVar[] MemberForces, GRBVar[] Displacements, Structure Structure, LoadCase LC, Stock Stock, OptimOptions Options)
        {
            foreach (IMember1D M in Structure.Members)
            {
                for(int j = 0; j < Stock.ElementGroups.Count; j++)
                {
                    double cmin;
                    double cmax;
                    GetCminCmax(M, Stock.ElementGroups[j], LC, out cmin, out cmax);

                    GRBLinExpr bTu = new GRBLinExpr();
                    
                    for(int d = 0; d < 3; d++)
                    {
                        if (!M.From.Fix[d])
                            bTu.AddTerm(-M.Direction[d] * M.Material.E * Stock.ElementGroups[j].CrossSection.Area / M.Length, Displacements[M.From.ReducedDofsTruss[d]]);
                        if (!M.To.Fix[d])
                            bTu.AddTerm(M.Direction[d] * M.Material.E * Stock.ElementGroups[j].CrossSection.Area / M.Length, Displacements[M.To.ReducedDofsTruss[d]]);
                    }

                    model.AddConstr(cmin - T[M.Number * Stock.ElementGroups.Count + j] * cmin, '<', bTu - MemberForces[M.Number * Stock.ElementGroups.Count + j], null);
                    model.AddConstr(cmax - T[M.Number * Stock.ElementGroups.Count + j] * cmax, '>', bTu - MemberForces[M.Number * Stock.ElementGroups.Count + j], null);
                }
            }
        }
        private static void GetCminCmax(IMember1D M, ElementGroup EG, LoadCase LC, out double cmin, out double cmax)
        {
            cmin = 0;
            cmax = 0;

            for (int d = 0; d < 3; d++)
            {
                if (M.To.DisplacementBounds.ContainsKey(LC) && M.From.DisplacementBounds.ContainsKey(LC))
                {
                    cmin += M.To.DisplacementBounds[LC].LB[d] * Math.Max(M.To.ConnectedMembers[M] * M.Direction[d], 0) + M.From.DisplacementBounds[LC].UB[d] * Math.Min(M.From.ConnectedMembers[M] * M.Direction[d], 0);
                    cmin += M.From.DisplacementBounds[LC].LB[d] * Math.Max(M.From.ConnectedMembers[M] * M.Direction[d], 0) + M.To.DisplacementBounds[LC].UB[d] * Math.Min(M.To.ConnectedMembers[M] * M.Direction[d], 0);
                }
                else
                    cmin = -GRB.INFINITY;

                if (M.From.DisplacementBounds.ContainsKey(LC) && M.To.DisplacementBounds.ContainsKey(LC))
                {
                    cmax += M.From.DisplacementBounds[LC].UB[d] * Math.Max(M.From.ConnectedMembers[M] * M.Direction[d], 0) + M.To.DisplacementBounds[LC].LB[d] * Math.Min(M.To.ConnectedMembers[M] * M.Direction[d], 0);
                    cmax += M.To.DisplacementBounds[LC].UB[d] * Math.Max(M.To.ConnectedMembers[M] * M.Direction[d], 0) + M.From.DisplacementBounds[LC].LB[d] * Math.Min(M.From.ConnectedMembers[M] * M.Direction[d], 0);
                }
                else
                    cmax = GRB.INFINITY;
            }

            cmin *= M.Material.E * EG.CrossSection.Area / M.Length;
            cmax *= M.Material.E * EG.CrossSection.Area / M.Length;

        }
    }
}
