using Phoenix3D.LCA;
using Phoenix3D.Model;
using Phoenix3D.Reuse;
using Gurobi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Phoenix3D.Optimization.SAND
{
    public static class SANDGurobiReuse
    {
        public static GRBVar[] GetGurobiAssignmentVariables(GRBModel Model, Structure Structure, Stock Stock, OptimOptions Options)
        {
            return Model.AddVars(Structure.Members.OfType<IMember1D>().Count() * Stock.ElementGroups.Count, GRB.BINARY);
        }
        public static GRBVar[] GetGurobiCuttingStockVariables(GRBModel Model, Stock Stock)
        {
            return Model.AddVars(Stock.ElementGroups.Count, GRB.BINARY);
        }
        public static Dictionary<LoadCase, GRBVar[]> GetGurobiMemberForceVariables(GRBModel Model, Structure Structure, List<LoadCase> LoadCases)
        {
            Dictionary<LoadCase, GRBVar[]> MemberForces = new Dictionary<LoadCase, GRBVar[]>();
            int m = Structure.Members.OfType<IMember1D>().Count();
            foreach (LoadCase LC in LoadCases)
            {
                MemberForces.Add(LC, Model.AddVars(Enumerable.Repeat(-GRB.INFINITY, m).ToArray(), Enumerable.Repeat(GRB.INFINITY, m).ToArray(), new double[m], Enumerable.Repeat(GRB.CONTINUOUS, m).ToArray(), null));
            }
            return MemberForces;
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

        public static void SetObjective(Objective Objective, GRBModel model, GRBVar[] T, Structure Structure, Stock Stock, ILCA LCA = null)
        {
            GRBLinExpr obj = new GRBLinExpr();

            switch (Objective)
            {
                case Objective.MinStructureMass:
                    {
                        foreach (IMember1D M in Structure.Members)
                        {
                            for (int j = 0; j < Stock.ElementGroups.Count; j++)
                            {
                                obj += T[M.Number * Stock.ElementGroups.Count + j] * M.Length * Stock.ElementGroups[j].Material.Density * Stock.ElementGroups[j].CrossSection.Area;
                            }
                        }
                        model.SetObjective(obj);
                        break;
                    }
                case Objective.MinStockMass:
                    {
                        for (int j = 0; j < Stock.ElementGroups.Count; j++)  
                        {
                            foreach (IMember1D M in Structure.Members)
                            {
                                if(Stock.ElementGroups[j].Type == ElementType.Reuse)
                                    obj += T[M.Number * Stock.ElementGroups.Count + j] * Stock.ElementGroups[j].Length * Stock.ElementGroups[j].Material.Density * Stock.ElementGroups[j].CrossSection.Area;
                                else
                                    obj += T[M.Number * Stock.ElementGroups.Count + j] * M.Length * Stock.ElementGroups[j].Material.Density * Stock.ElementGroups[j].CrossSection.Area;
                            }
                        }
                        model.SetObjective(obj);
                        break;
                    }
                case Objective.MinWaste:
                    {
                        for (int j = 0; j < Stock.ElementGroups.Count; j++)
                        {
                            foreach (IMember1D M in Structure.Members)
                            {
                                obj += T[M.Number * Stock.ElementGroups.Count + j] * (Stock.ElementGroups[j].Length - M.Length) * Stock.ElementGroups[j].Material.Density * Stock.ElementGroups[j].CrossSection.Area;
                            }
                        }
                        model.SetObjective(obj);
                        break;
                    }
                case Objective.MinLCA:
                    {
                        foreach (IMember1D M in Structure.Members)
                        {
                            for (int j = 0; j < Stock.ElementGroups.Count; j++)
                            {
                                obj += T[M.Number * Stock.ElementGroups.Count + j] * LCA.ReturnElementMemberImpact(Stock.ElementGroups[j], false, M);
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
        public static void SetObjectiveCuttingStock(Objective Objective, GRBModel model, GRBVar[] T, GRBVar[] Y, Structure Structure, Stock Stock, ILCA LCA = null)
        {
            GRBLinExpr obj = new GRBLinExpr();

            switch (Objective)
            {
                case Objective.MinStructureMass:
                    {
                        foreach (IMember1D M in Structure.Members)
                        {
                            for (int j = 0; j < Stock.ElementGroups.Count; j++)
                            {
                                obj += T[M.Number * Stock.ElementGroups.Count + j] * M.Length * Stock.ElementGroups[j].Material.Density * Stock.ElementGroups[j].CrossSection.Area;
                            }
                        }
                        model.SetObjective(obj);
                        break;
                    }
                case Objective.MinStockMass:
                    {
                        for (int j = 0; j < Stock.ElementGroups.Count; j++)
                        {
                            if(Stock.ElementGroups[j].Type == ElementType.Reuse)
                                obj += Y[j] * Stock.ElementGroups[j].Length * Stock.ElementGroups[j].Material.Density * Stock.ElementGroups[j].CrossSection.Area;
                        }
                        model.SetObjective(obj);
                        break;
                    }
                case Objective.MinWaste:
                    {
                        for (int j = 0; j < Stock.ElementGroups.Count; j++)
                        {
                            obj += Y[j] * Stock.ElementGroups[j].Length * Stock.ElementGroups[j].Material.Density * Stock.ElementGroups[j].CrossSection.Area;
                            foreach (IMember1D M in Structure.Members)
                            {
                                obj -= T[M.Number * Stock.ElementGroups.Count + j] * M.Length * Stock.ElementGroups[j].Material.Density * Stock.ElementGroups[j].CrossSection.Area;
                            }
                        }
                        model.SetObjective(obj);
                        break;
                    }
                case Objective.MinLCA:
                    {
                        for (int j = 0; j < Stock.ElementGroups.Count; j++)
                        {
                            obj += Y[j] * LCA.ReturnStockElementImpact(Stock.ElementGroups[j]);
                            foreach (IMember1D M in Structure.Members)
                            {
                                obj += T[M.Number * Stock.ElementGroups.Count + j] * LCA.ReturnElementMemberImpact(Stock.ElementGroups[j], true, M);
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
        public static void AddGroup(GRBModel model, GRBVar[] T, Structure Structure, Stock Stock)
        {
            foreach (KeyValuePair<int, List<IMember>> kvp in Structure.MemberGroups)
            {
                if (kvp.Key == -1)
                    continue;
                int idx0 = kvp.Value[0].Number;
                for (int i = 1; i < kvp.Value.Count; i++)
                {
                    int Midx = kvp.Value[i].Number;
                    for (int j = 0; j < Stock.ElementGroups.Count; j++)
                    {
                        model.AddConstr(T[idx0 * Stock.ElementGroups.Count + j], '=', T[Midx * Stock.ElementGroups.Count + j], null);
                    }
                }

            }
        }
        public static void AddLength(GRBModel model, GRBVar[] T, Structure Structure, Stock Stock)
        {
                foreach (IMember1D M in Structure.Members)
                {
                    GRBLinExpr l = new GRBLinExpr();
                    for (int j = 0; j < Stock.ElementGroups.Count; j++)
                    {
                        if (Stock.ElementGroups[j].Type is ElementType.Reuse)
                        {
                            l.AddTerm(Stock.ElementGroups[j].Length - M.Length, T[M.Number * Stock.ElementGroups.Count + j]);
                        }
                    }
                    model.AddConstr(l, '>', 0, "Length" + M.Number.ToString());
                }
        }
        public static void AddLengthCuttingStock(GRBModel model, GRBVar[] T, GRBVar[] Y, Structure Structure, Stock Stock)
        {
            for (int j = 0; j < Stock.ElementGroups.Count; j++)
            {
                if (Stock.ElementGroups[j].Type is ElementType.Reuse)
                {
                    GRBLinExpr l = new GRBLinExpr();
                    foreach (IMember1D M in Structure.Members)
                    {
                        l.AddTerm(M.Length, T[M.Number * Stock.ElementGroups.Count + j]);
                    }
                    model.AddConstr(l, '<', Stock.ElementGroups[j].Length * Y[j], "Length_" + Stock.ElementGroups[j].Number.ToString());
                }
            }
        }
        public static void AddAvailability(GRBModel model, GRBVar[] T, Structure Structure, Stock Stock)
        {
            for (int j = 0; j < Stock.ElementGroups.Count; j++)
            {
                if (Stock.ElementGroups[j].Type is ElementType.Reuse)
                {
                    GRBLinExpr a = new GRBLinExpr();

                    foreach (IMember1D M in Structure.Members)
                    {
                        a.AddTerm(1, T[M.Number * Stock.ElementGroups.Count + j]);
                    }
                    model.AddConstr(a, '<', Stock.ElementGroups[j].NumberOfElements, "Avail_" + Stock.ElementGroups[j].Number.ToString());
                }
            }
        }
        

    }
}
