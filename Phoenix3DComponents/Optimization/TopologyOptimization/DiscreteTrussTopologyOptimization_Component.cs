using System;
using System.Collections.Generic;

using Phoenix3D.Model;
using Phoenix3D.Optimization;
using Phoenix3D.Optimization.TopologyOptimization;
using Phoenix3D.Reuse;

using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;

using Rhino.Geometry;


namespace Phoenix3D_Components.Optimization.TopologyOptimization
{
    public class DiscreteTrussTopologyOptimization_Component : GH_Component
    {

        public DiscreteTrussTopologyOptimization_Component() : base("Discrete Topology Optimization", "DTTopt", "Perform a MILP truss topology optimization with discrete cross sections", "Phoenix3D", " Optimization")
        {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Structure", "SC", "Structure to work on", GH_ParamAccess.item);
            pManager.AddGenericParameter("Stock", "ST", "Cross Section Catalog", GH_ParamAccess.item);
            pManager.AddIntegerParameter("Objective", "OB", "Objective function: 0 = Min structure mass, 1 = compliance",GH_ParamAccess.item, 0);
            pManager.AddTextParameter("LoadCase Names", "LC", "Name of the loadcases to consider. Use 'all' to consider all load cases in the structure.", GH_ParamAccess.list, new List<string>() { "all" });
            pManager.AddGenericParameter("DTTO Options", "OP", "Discret Truss Topology Optimization Options", GH_ParamAccess.item);
            pManager.AddBooleanParameter("Run", "Run", "Run", GH_ParamAccess.item, false);
            pManager[2].Optional = true;
            pManager[3].Optional = true;
            pManager[4].Optional = true;
            pManager[5].Optional = true;
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Structure", "SC", "The optimized structure", GH_ParamAccess.item);
            pManager.AddNumberParameter("Objective value", "OB", "The optimal objective function value", GH_ParamAccess.item);
            pManager.AddNumberParameter("Bar areas", "AR", "The cross section areas of the optimized members", GH_ParamAccess.list);
            pManager.AddNumberParameter("Bar normal Forces", "FO", "The bar normal forces for each load case", GH_ParamAccess.list);
            pManager.AddPointParameter("LowerBounds", "LB", "LowerBounds", GH_ParamAccess.list);
            pManager.AddPointParameter("UpperBounds", "UB", "UpperBounds", GH_ParamAccess.list);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            bool run = false;

            Structure S = null;
            Stock Stock = null;
            List<string> LoadCaseNames = new List<string>();
            int Objective = 0;
            OptimOptions Options = new OptimOptions();
            List<GH_Point> LB = new List<GH_Point>();
            List<GH_Point> UB = new List<GH_Point>();


            DA.GetData(0, ref S);
            DA.GetData(1, ref Stock);
            DA.GetData(2, ref Objective);
            DA.GetDataList(3, LoadCaseNames);
            DA.GetData(4, ref Options);
            DA.GetData(5, ref run);

            S = S.Clone();

            DiscreteTrussTopologyOptimization DTTOpt = new DiscreteTrussTopologyOptimization((Objective)Objective, Options);

            if(run)
            {
                DTTOpt.Solve(S, LoadCaseNames, Stock);
            } 
            else
            {
                DA.SetData(0, S);
                return;
            }

            if(DTTOpt.Interrupted)
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

            for (int i = 0; i < DTTOpt.LowerBounds.Count; i++)
            {
                if(DTTOpt.UpperBounds[i].Item2 > 1e99)
                {
                    LB.Add(new GH_Point(new Point3d(DTTOpt.LowerBounds[i].Item1, DTTOpt.LowerBounds[i].Item2, 0)));
                }
                else
                {
                    LB.Add(new GH_Point(new Point3d(DTTOpt.LowerBounds[i].Item1, DTTOpt.LowerBounds[i].Item2, 0)));
                    UB.Add(new GH_Point(new Point3d(DTTOpt.UpperBounds[i].Item1, DTTOpt.UpperBounds[i].Item2, 0)));
                }
            }

            DA.SetData(0, S);
            DA.SetData(1, new GH_Number(DTTOpt.ObjectiveValue));
            DA.SetDataList(2, GHAreas);
            DA.SetDataTree(3, MemberForces);
            DA.SetDataList(4, LB);
            DA.SetDataList(5, UB);
        }

        protected override System.Drawing.Bitmap Icon => Properties.Resources.DTTO;
        public override Guid ComponentGuid => new Guid("651c8323-b797-47d3-ba81-660f2b9a9c11");

        public override GH_Exposure Exposure
        {
            get { return GH_Exposure.secondary; }
        }

    }
}