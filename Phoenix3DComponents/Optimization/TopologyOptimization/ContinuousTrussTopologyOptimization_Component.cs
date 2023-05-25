using System;
using System.Collections.Generic;

using Phoenix3D.Model;
using Phoenix3D.Optimization;
using Phoenix3D.Optimization.TopologyOptimization;

using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;


namespace Phoenix3D_Components.Optimization.TopologyOptimization
{
    public class ContinuousTrussTopologyOptimization_Component : GH_Component
    {

        public ContinuousTrussTopologyOptimization_Component() : base("Continuous Topology Optimization", "CTTopt", "Perform a lower-bound linear programming truss topology optimization with continous cross sections", "Phoenix3D", " Optimization")
        {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Structure", "SC", "Structure to work on", GH_ParamAccess.item);
            pManager.AddTextParameter("LoadCase Names", "LC", "Name of the loadcases to consider. Use 'all' to consider all load cases in the structure.", GH_ParamAccess.list, new List<string>() { "all" });
            pManager.AddGenericParameter("CTTO Options", "OP", "Optimization Options", GH_ParamAccess.item);

            pManager[1].Optional = true;
            pManager[2].Optional = true;
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Structure", "SC", "The optimized structure", GH_ParamAccess.item);
            pManager.AddNumberParameter("Structure Mass", "MS", "The minimum structure mass [kg]", GH_ParamAccess.item);
            pManager.AddNumberParameter("Bar areas", "AR", "The cross section areas of the optimized members", GH_ParamAccess.list);
            pManager.AddNumberParameter("Bar normal forces", "FO", "The bar normal forces for each load case", GH_ParamAccess.list);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Structure S = null;
            List<string> LoadCaseNames = new List<string>();
            OptimOptions Options = new OptimOptions();

            DA.GetData(0, ref S);
            DA.GetDataList(1, LoadCaseNames);
            DA.GetData(2, ref Options);

            ContinuousTrussTopologyOptimization CTTOpt = new ContinuousTrussTopologyOptimization(Options);
            CTTOpt.Solve(S, LoadCaseNames);

            if(CTTOpt.Interrupted)
            {
                this.AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "The optimization has been terminated by the user");
            }

            List<GH_Number> GHAreas = new List<GH_Number>();

            DataTree<GH_Number> MemberForces = new DataTree<GH_Number>();

            foreach (Bar B in S.Members)
            {
                GHAreas.Add(new GH_Number(B.CrossSection.Area));
                foreach (LoadCase LC in S.GetLoadCasesFromNames(LoadCaseNames))
                {
                    MemberForces.Add(new GH_Number(B.Nx[LC][0]), new GH_Path(B.Number));
                }
            }

            DA.SetData(0, S);
            DA.SetData(1, new GH_Number(CTTOpt.ObjectiveValue));
            DA.SetDataList(2, GHAreas);
            DA.SetDataTree(3, MemberForces);
        }

        protected override System.Drawing.Bitmap Icon => Properties.Resources.CTTO;

        public override Guid ComponentGuid => new Guid("651c8323-b797-47d3-ba81-660f2b9a9c58");

        public override GH_Exposure Exposure
        {
            get { return GH_Exposure.secondary; }
        }

    }
}