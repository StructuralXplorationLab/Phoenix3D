using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Phoenix3D.Model;
using Gurobi;


namespace Phoenix3D.Optimization.SAND
{
    
    public static class SANDGurobiContinuous
    {
        public static void SetObjective(Objective Objective, GRBModel model, GRBVar[] Areas, Dictionary<LoadCase, GRBVar[]> MemberForces, Structure Structure)
        {
            GRBLinExpr obj = new GRBLinExpr();
            switch(Objective)
            {
                case Objective.MinStructureMass:
                    {
                        foreach(Bar M in Structure.Members)
                        {
                            obj += Areas[M.Number] * M.Length * M.Material.Density;
                        }
                        model.SetObjective(obj);
                        break;
                    }
                    
                default:
                    {
                        foreach (Bar M in Structure.Members)
                        {
                            obj += Areas[M.Number] * M.Length * M.Material.Density;
                        }
                        model.SetObjective(obj);
                        break;
                    }
            }
        }

        public static void AddEquilibrium(GRBModel model, GRBVar[] Areas, GRBVar[] MemberForces, Structure Structure, LoadCase LC, OptimOptions Options)
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
                        if (d == 2 && Options.Selfweight)
                        {
                            f += Areas[M.Number] * M.Length * M.Material.Density * LC.yg * 1e-5 * 0.5;
                        }
                        f += N.ConnectedMembers[M] * M.Direction[d] * MemberForces[M.Number];
                    }
                    model.AddConstr(f, '=', 0, "LC" + LC.Number.ToString()+"EquilibriumNode" + N.Number.ToString() + "dof" + d.ToString());
                }
            }
        }
        public static void AddStress(GRBModel model, GRBVar[] Areas, GRBVar[] MemberForces, Structure Structure, LoadCase LC, OptimOptions Options)
        {
            foreach(Bar M in Structure.Members)
            {
                model.AddConstr((-M.Material.fc / M.Material.gamma_0) * Areas[M.Number]  <= MemberForces[M.Number], "LC" + LC.Number.ToString() + "Compression" + M.Number.ToString());
                model.AddConstr((M.Material.ft / M.Material.gamma_0) * Areas[M.Number] >= MemberForces[M.Number], "LC" + LC.Number.ToString() + "Tension" + M.Number.ToString());
            }
        }
    }
}
