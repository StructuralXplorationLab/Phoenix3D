using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Phoenix3D.Model;
using Rhino.Geometry;

namespace Phoenix3D_Components.Display
{
    public class ResultsAsAListComponent : GH_Component
    {

        public ResultsAsAListComponent()
          : base("Display Results as list", "Results",
              "results of the optimization as a list",
              "Phoenix3D", "Display")
        {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Stucture", "SC", "Structure", GH_ParamAccess.item);
            pManager.AddTextParameter("LoadCase", "LC", "Name of the load case to visualize", GH_ParamAccess.item);

            pManager[1].Optional = true;
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddNumberParameter("Display Results as list", "RE", "", GH_ParamAccess.list);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            var str_tmp = new Structure();
            string lc = (default);

            DA.GetData(0, ref str_tmp);
            DA.GetData(1, ref lc);
            var str = str_tmp.Clone();

            var results = str.Results;

            DA.SetDataList(0, results.GetResultsAsAList());

        }

        protected override System.Drawing.Bitmap Icon => Properties.Resources.display_results_list;
        public override Guid ComponentGuid
        {
            get { return new Guid("28DB1014-C48F-4A7C-B645-91B0C53C49A8"); }
        }
    }
}