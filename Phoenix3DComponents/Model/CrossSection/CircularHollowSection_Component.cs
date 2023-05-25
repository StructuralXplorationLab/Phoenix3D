using System;

using Phoenix3D.Model.CrossSections;

using Grasshopper.Kernel;


namespace Phoenix3D_Components.Model.CrossSection
{
    public class CircularHollowSection_Component : GH_Component
    {

        public CircularHollowSection_Component() : base("CircularHollowSection", "CHS", "Defines a circular hollow section with diameter D and wall thickness T", "Phoenix3D", "  CrossSections")
        {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddNumberParameter("Diameter [mm]", "D [mm]", "Circular section diameter in [mm]", GH_ParamAccess.item, 100);
            pManager.AddNumberParameter("WallThickness [mm]", "T [mm]", "Wall thickness in [mm]", GH_ParamAccess.item, 10);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Cross Section", "CS", "Cross Section", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            double D = 100;
            double T = 10;
            DA.GetData(0, ref D);
            DA.GetData(1, ref T);
            DA.SetData(0, new CircularHollowSection(D / 1000, T / 1000));
        }

        protected override System.Drawing.Bitmap Icon => Properties.Resources.circular_hollow_section;

        public override Guid ComponentGuid => new Guid("8e617b0c-b658-464e-958e-a27b6cea0f05");

        public override GH_Exposure Exposure
        {
            get { return GH_Exposure.secondary; }
        }

    }
}