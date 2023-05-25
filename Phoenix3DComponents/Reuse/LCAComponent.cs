using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

using Phoenix3D.LCA;

namespace Phoenix3D_Components.Reuse
{
    public class LCAComponent : GH_Component
    {
        public LCAComponent()
          : base("Life Cycle Asessment", "LCA",
              "add custom LCA values computed accorting to GHGFrontiers paper",
              "Phoenix3D", "  Reuse")
        {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddNumberParameter("Deconstruction", "Deconstruction", "", GH_ParamAccess.item, 0.337);
            pManager.AddNumberParameter("Demolition", "Demolition", "", GH_ParamAccess.item, 0.050);
            pManager.AddNumberParameter("New Steel Production", "New Production", "", GH_ParamAccess.item, 0.734);
            pManager.AddNumberParameter("Assembly", "Assembly", "", GH_ParamAccess.item, 0.110);
            pManager.AddNumberParameter("Transport", "Transport", "", GH_ParamAccess.item, 1.1e-4); // per kg + km
            pManager.AddNumberParameter("Transport Stock", "Transport Stock", "", GH_ParamAccess.item, 150 * 1.1e-4);
            pManager.AddNumberParameter("Transport new Steel", "Transport new Steel", "", GH_ParamAccess.item, 10 * 1.1e-4);
            pManager.AddNumberParameter("Transport Structure", "Transport Structure", "", GH_ParamAccess.item, 10 * 1.1e-4);
            pManager.AddNumberParameter("Transport Waste", "Transport Waste", "", GH_ParamAccess.item, 10 * 1.1e-4);
            pManager.AddNumberParameter("Transport Recycling", "Transport Recycling", "", GH_ParamAccess.item, 10 * 1.1e-4);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("LCA", "LCA", "", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            var values = new double[10];

            for (int i = 0; i < values.Length; ++i)
                DA.GetData(i, ref values[i]);

            DA.SetData(0, new GHGFrontiers(values[0], values[1], values[2], values[3], values[4],
                                           values[5], values[6], values[7], values[8], values[9]));

        }

        protected override System.Drawing.Bitmap Icon => Properties.Resources.lca;
        public override Guid ComponentGuid
        {
            get { return new Guid("04E848B2-305E-42FC-AE44-FAD855E33FBA"); }
        }
        public override GH_Exposure Exposure
        {
            get { return GH_Exposure.secondary; }
        }
    }
}