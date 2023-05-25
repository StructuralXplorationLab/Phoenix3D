using Grasshopper.Kernel;

using Rhino.Geometry;

using System;
using System.Collections.Generic;

using Cardinal.Model;

namespace CardinalComponents.Display
{
    public class Results_Component : GH_Component
    {

        public Results_Component() : base("Results", "Res", "Results of optimization", "Cardinal", "Display")
        {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Structure", "S", "takes a structure", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Results", "R", "Results of optimization", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // --- INPUT ---
            var str = new Structure();

            DA.GetData(0, ref str);

            // --- OUTPUT ---
            DA.SetData(0, str.Results.ToString());
        }

        protected override System.Drawing.Bitmap Icon => null;

        public override Guid ComponentGuid => new Guid("825fef21-4b02-45b2-a8b4-2631c29a53eb");

    }
}