using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Special;


using Phoenix3D.Model;
using Phoenix3D.Optimization;
using Phoenix3D.Reuse;
using Phoenix3D.Reuse.Heuristics;
using Phoenix3D.FEA;
using Phoenix3D.LCA;

namespace Phoenix3D_Components.Optimization.Heuristics
{
    public class BestFit_Component : GH_Component
    {
        private List<string> Objectives = new List<string>()
        {
            "MinStructureMass",
            "MinStockMass",
            "MinWaste",
            "MinLCA"
        };

        public BestFit_Component() : base("Best-Fit", "Best-Fit", "Performs a heuristic search via a custom Best-Fit algorithm", "Phoenix3D", " Optimization")
        {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Structure", "SC", "", GH_ParamAccess.item);
            pManager.AddGenericParameter("Stock", "ST", "", GH_ParamAccess.item);
            pManager.AddIntegerParameter("Objective", "OB", "To Select Type of Objective add ValueList Component", GH_ParamAccess.item, 0);
            pManager.AddTextParameter("LoadCases", "LC", "Load cases as list of names", GH_ParamAccess.list, new List<string>() { "all" });
            pManager.AddBooleanParameter("PromoteFirstPart", "FI", "PromoteFirstPart", GH_ParamAccess.item, false);
            pManager.AddIntegerParameter("Iterations", "IT", "number of iterations - only required for statically indeterminate systems", GH_ParamAccess.item, 1);
            pManager.AddBooleanParameter("Run FEA", "runFEA", " (true) Run an FEA Analysis; (false) use externally computed member forces", GH_ParamAccess.item, false);
            pManager.AddGenericParameter("LCA", "LCA", "add a custom LCA", GH_ParamAccess.item);

            pManager[2].Optional = true;
            pManager[3].Optional = true;
            pManager[4].Optional = true;
            pManager[5].Optional = true;
            pManager[7].Optional = true;
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Structure", "SC", "Structure", GH_ParamAccess.list);
            pManager.AddGenericParameter("Stock", "ST", "Stock", GH_ParamAccess.list);
            //pManager.AddNumberParameter("Runtime", "T", "Runtime", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // --- INPUT ---

            var str = new Structure();
            var stock = new Stock();
            int objectiveInt = 0;
            bool PromoteFirstPart = false;
            bool runFEA = false;
            List<string> LCNames = new List<string>();
            int iterations = 1;
            var lca = new GHGFrontiers();

            DA.GetData(0, ref str);
            DA.GetData(1, ref stock);
            DA.GetData(2, ref objectiveInt);
            DA.GetDataList(3, LCNames);
            DA.GetData(4, ref PromoteFirstPart);
            DA.GetData(5, ref iterations);
            DA.GetData(6, ref runFEA);
            DA.GetData(7, ref lca);

            str = str.Clone();
            stock = stock.Clone();
            List<LoadCase> LCs = str.GetLoadCasesFromNames(LCNames);

            Stopwatch sw = new Stopwatch();
            sw.Start();

            if (runFEA && LCNames.Count > 0 && !str.Members.OfType<Beam>().Any())
            {
                var fea = new FiniteElementAnalysis();
                foreach (LoadCase LC in LCs)
                {
                    fea.Solve(str, LC);
                }
            }
            for (int i = 0; i < iterations; ++i)
            {
                // starting with 80% of capacitiy and increasing until 100% capacity by -0.2x^2 + 0.4x + 0.8
                double step = 1.0 / (iterations-1) * i;
                double allowedCapacity = -0.2 * step * step + 0.4 * step + 0.8;

                if (iterations == 1)
                    allowedCapacity = 1.0;

                if (runFEA && LCNames.Count > 0)
                {
                    var fea = new FiniteElementAnalysis();

                    foreach (LoadCase LC in LCs)
                    {
                        fea.Solve(str, LC);
                    }
                }

                stock.ResetAssignedMembers();

                var bestFit = new BestFit((Objective)objectiveInt, lca, PromoteFirstPart);
                bestFit.Solve(str, stock, LCs, allowedCapacity);

                if (runFEA)
                {
                    //checking if all utilizations <= 100%
                    var feaFinal = new FiniteElementAnalysis();

                    foreach (LoadCase LC in LCs)
                    {
                        feaFinal.Solve(str, LC);
                    }
                }
            }

            DA.SetData(0, str);
            DA.SetData(1, stock);
            //DA.SetData(2, sw.ElapsedMilliseconds);
            sw.Stop();
        }

        protected override System.Drawing.Bitmap Icon => Properties.Resources.best_fit;

        public override Guid ComponentGuid => new Guid("6b489305-6413-4f93-a028-0d0c664a5f97");

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
                        source.ListItems.Add(new GH_ValueListItem(this.Objectives[index], (index).ToString()));
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