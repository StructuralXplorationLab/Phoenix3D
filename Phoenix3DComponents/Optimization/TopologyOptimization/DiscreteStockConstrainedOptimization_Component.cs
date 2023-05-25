using System;
using System.Collections.Generic;
using System.Diagnostics;

using Phoenix3D.LCA;
using Phoenix3D.Model;
using Phoenix3D.Optimization;
using Phoenix3D.Optimization.TopologyOptimization;
using Phoenix3D.Reuse;

using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using Grasshopper.Kernel.Special;

using Rhino.Geometry;


namespace Phoenix3D_Components.Optimization.TopologyOptimization
{
    public class DiscreteStockConstrainedOptimization_Component : GH_Component
    {
        private List<string> Objectives = new List<string>()
        {
            "MinStructureMass",
            "MinStockMass",
            "MinWaste",
            "MinLCA"
        };

        public DiscreteStockConstrainedOptimization_Component() : base("Discrete Stock Constrained Optimization", "DSCopt", "Perform a MILP truss stock constrained optimization with discrete stock", "Phoenix3D", " Optimization")
        {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Structure", "SC", "Structure to work on", GH_ParamAccess.item);
            pManager.AddGenericParameter("Stock", "ST", "Cross Section Catalog", GH_ParamAccess.item);
            pManager.AddIntegerParameter("Objective", "OB", "Objective function: 0 = Min structure mass, 1 = compliance",GH_ParamAccess.item, 0);
            pManager.AddTextParameter("LoadCase Names", "LC", "Name of the loadcases to consider. Use 'all' to consider all load cases in the structure.", GH_ParamAccess.list, new List<string>() { "all" });
            pManager.AddGenericParameter("DSCO Options", "OP", "Discret Stock Constrained Optimization Options", GH_ParamAccess.item);
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
            pManager.AddNumberParameter("Bar normal forces", "FO", "The bar normal forces for each load case", GH_ParamAccess.list);
            pManager.AddPointParameter("LowerBounds", "LB", "LowerBounds", GH_ParamAccess.list);
            pManager.AddPointParameter("UpperBounds", "UB", "UpperBounds", GH_ParamAccess.list);
            pManager.AddGenericParameter("Stock", "ST", "The stock", GH_ParamAccess.item);
            pManager.AddNumberParameter("Runtime", "RT", "Runtime", GH_ParamAccess.item);
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
            Stock = Stock.Clone();

            Objective -= 1;

            Stopwatch sw = new Stopwatch();
            sw.Start();

            DiscreteStockConstrainedOptimization DSCOpt = new DiscreteStockConstrainedOptimization((Objective)Objective, Options);


            if(run)
            {
                DSCOpt.Solve(S, LoadCaseNames, Stock, new GHGFrontiers());
            } 
            else
            {
                DA.SetData(0, S);
                DA.SetData(6, Stock);
                return;
            }

            if(DSCOpt.Interrupted)
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

            for (int i = 0; i < DSCOpt.LowerBounds.Count; i++)
            {
                if(DSCOpt.UpperBounds[i].Item2 > 1e99 && DSCOpt.LowerBounds[i].Item2 > -1e99)
                {
                    LB.Add(new GH_Point(new Point3d(DSCOpt.LowerBounds[i].Item1, DSCOpt.LowerBounds[i].Item2, 0)));
                }
                else if (DSCOpt.LowerBounds[i].Item2 < -1e99 && DSCOpt.UpperBounds[i].Item2 < 1e99)
                {
                    UB.Add(new GH_Point(new Point3d(DSCOpt.UpperBounds[i].Item1, DSCOpt.UpperBounds[i].Item2, 0)));
                }
                else
                {
                    LB.Add(new GH_Point(new Point3d(DSCOpt.LowerBounds[i].Item1, DSCOpt.LowerBounds[i].Item2, 0)));
                    UB.Add(new GH_Point(new Point3d(DSCOpt.UpperBounds[i].Item1, DSCOpt.UpperBounds[i].Item2, 0)));
                }
            }

            DA.SetData(0, S);
            DA.SetData(1, new GH_Number(DSCOpt.ObjectiveValue));
            DA.SetDataList(2, GHAreas);
            DA.SetDataTree(3, MemberForces);
            DA.SetDataList(4, LB);
            DA.SetDataList(5, UB);
            DA.SetData(6, Stock);
            DA.SetData(7, sw.ElapsedMilliseconds);
            sw.Stop();
        }

        protected override System.Drawing.Bitmap Icon => Properties.Resources.DSCO;

        public override Guid ComponentGuid => new Guid("651c8323-b797-47d3-ba81-660f2b9a9c03");

        protected override void BeforeSolveInstance()
        {
            if (this.Params.Input[2].SourceCount <= 0 || this.Params.Input[2].SourceCount != 1 || !(this.Params.Input[2].Sources[0] is GH_ValueList))
                return;
            GH_ValueList source = this.Params.Input[2].Sources[0] as GH_ValueList;
            source.ListMode = GH_ValueListMode.DropDown;
            if (source.ListItems.Count != this.Objectives.Count)
            {
                source.ListItems.Clear();
                for (int index = 0; index < this.Objectives.Count; ++index)
                    source.ListItems.Add(new GH_ValueListItem(this.Objectives[index], (index).ToString()));
                source.ExpireSolution(true);
            }
            else
            {
                bool flag = true;
                for (int index = 0; index < this.Objectives.Count; ++index)
                {
                    if (source.ListItems[index].Name != this.Objectives[index])
                    {
                        flag = false;
                        break;
                    }
                }
                if (!flag)
                {
                    source.ListItems.Clear();
                    for (int index = 0; index < this.Objectives.Count; ++index)
                        source.ListItems.Add(new GH_ValueListItem(this.Objectives[index], (index + 1).ToString()));
                    source.ExpireSolution(true);
                }
            }
        }

        public override GH_Exposure Exposure
        {
            get { return GH_Exposure.secondary; }
        }

    }
}