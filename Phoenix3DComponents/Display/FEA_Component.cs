using Grasshopper.Kernel;
using Grasshopper;
using Grasshopper.Kernel.Types;

using Rhino.Geometry;

using System;
using System.Collections.Generic;

using Phoenix3D.Model;
using Phoenix3D.FEA;
using Phoenix3D.Optimization;
using Phoenix3D.Reuse;
using Phoenix3D.Reuse.Heuristics;
using Phoenix3D.LCA;


namespace Phoenix3D_Components.Display
{
    public class FEA_Component : GH_Component
    {

        public FEA_Component()
          : base("FEA", "FEA", "runs a linear elastic FEA", "Phoenix3D", " Optimization")
        {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Structure", "SC", "takes a Structure", GH_ParamAccess.item);
            pManager.AddTextParameter("LoadCases", "LC", "Load cases as list of names", GH_ParamAccess.list);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Structure", "SC", "Structure", GH_ParamAccess.list);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            var lc = new List<string>();
            Structure str = (default);

            DA.GetData(0, ref str);
            DA.GetDataList(1, lc);

            str = str.Clone();

            List<LoadCase> LCs = str.GetLoadCasesFromNames(lc);

            if (lc.Count > 0)
            {
                var fea = new FiniteElementAnalysis();

                foreach (LoadCase LC in LCs)
                {
                    fea.Solve(str, LC);
                }
            }

            var egs = new List<ElementGroup>();
            foreach(Bar bar in str.Members)
            {
                egs.Add(new ElementGroup((ElementType)1, bar.Material, bar.CrossSection, bar.Length, 1, false));
            }

            var stock = new Stock(egs);

            stock.ResetAssignedMembers();

            var bestFit = new BestFit((Objective)0, new GHGFrontiers(), false);
            bestFit.Solve(str, stock, LCs, 1);

            DA.SetData(0, str);
        }

        protected override System.Drawing.Bitmap Icon => Properties.Resources.fea;

        public override Guid ComponentGuid => new Guid("12d3df0c-73c6-483b-a60d-4e16381c470b");

        public override GH_Exposure Exposure
        {
            get { return GH_Exposure.primary; }
        }

    }
}