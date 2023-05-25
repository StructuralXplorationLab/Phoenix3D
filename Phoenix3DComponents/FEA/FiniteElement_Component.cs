using System;
using System.Collections.Generic;

using Cardinal.FEA;
using Cardinal.Model;

using Grasshopper.Kernel;
using Grasshopper;
using Grasshopper.Kernel.Data;


namespace CardinalComponents.FEA
{
    public class FiniteElementAnalysis_Component : GH_Component
    {

        public FiniteElementAnalysis_Component() : base("Finite Element Analysis", "FEA", "Analysis", "Cardinal", "FEA")
        {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Structure", "S", "", GH_ParamAccess.item);
            pManager.AddTextParameter("LoadCases", "LC", "Names of the LoadCases to be computed", GH_ParamAccess.list, new List<string>() { "all" });
            pManager.AddGenericParameter("FEA Options", "FEAOptions", "FEA options", GH_ParamAccess.item);
            pManager[1].Optional = true;
            pManager[2].Optional = true;
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Structure", "S", "Structure with FEA results", GH_ParamAccess.item);
            pManager.AddNumberParameter("MemberForces", "N", "Member forces per LoadCase", GH_ParamAccess.tree);
            pManager.AddNumberParameter("Displacements", "D", "Nodal displacements", GH_ParamAccess.tree);
            pManager.AddNumberParameter("ReactionForces", "F", "", GH_ParamAccess.tree);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // -- Input --

            var str = new Structure();
            List<string> LoadCaseNames = new List<string>();
            FEAOptions fea_opt = new FEAOptions();

            DataTree<double> MemberForces = new DataTree<double>();
            DataTree<double> Displacements = new DataTree<double>();
            DataTree<double> ReactionForces = new DataTree<double>();

            DA.GetData(0, ref str);
            DA.GetDataList(1, LoadCaseNames);
            DA.GetData(2, ref fea_opt);

            // -- Solve --

            
            var fea = new FiniteElementAnalysis(fea_opt);

            List<LoadCase> LCs = str.GetLoadCasesFromNames(LoadCaseNames);

            int path = 0;
            foreach( LoadCase LC  in LCs)
            {
                fea.Solve(str, LC);
                List<double> Nx = new List<double>();
                foreach(IMember1D M in str.Members)
                {
                    Nx.Add(M.Nx[LC][0]);
                }
                MemberForces.AddRange(Nx, new GH_Path(path));
                Displacements.AddRange(fea.Solution.u.ToDouble(), new GH_Path(path));
                path++;
            }

            // -- Output --

            DA.SetData(0, str);
            DA.SetDataTree(1, MemberForces);
            DA.SetDataTree(2, Displacements);
            DA.SetDataTree(3, ReactionForces);
        }

        protected override System.Drawing.Bitmap Icon => null;

        public override GH_Exposure Exposure => GH_Exposure.primary;

        public override Guid ComponentGuid => new Guid("1eb401fd-7c10-495c-b0d8-727467a7f101");
 
    }
}