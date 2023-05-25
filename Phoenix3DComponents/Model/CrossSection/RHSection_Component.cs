using System;

using Phoenix3D.Model.CrossSections;

using Grasshopper.Kernel;


namespace Phoenix3D_Components.Model.CrossSection
{
    public class RHSection_Component : GH_Component
    {

        public RHSection_Component() : base("RectangularHollowSection", "RHS", "Standard RH-Section profile after EN 10210", "Phoenix3D", "  CrossSections")
        {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddNumberParameter("Height [mm]", "H [mm]", "RHSection height in [mm]", GH_ParamAccess.item, 50.0);
            pManager.AddNumberParameter("Width [mm]", "W [mm]", "RHSection width in [mm]", GH_ParamAccess.item, 30.0);
            pManager.AddNumberParameter("Thickness [mm]", "T [mm]", "RHSection wall thickness [mm]", GH_ParamAccess.item, 4);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Cross Section", "CS", "Cross Section", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            
            double Height = 50;
            double Width = 30;
            double Thickness = 4;

            DA.GetData(0, ref Height);
            DA.GetData(1, ref Width);
            DA.GetData(2, ref Thickness);

            DA.SetData(0, new RHSection(Height / 1000, Width / 1000, Thickness / 1000));
        }

        protected override System.Drawing.Bitmap Icon => Properties.Resources.rectangular_hollow_section;

        public override Guid ComponentGuid => new Guid("12a218ca-4dcf-4446-999b-84cecf519077");

        public override GH_Exposure Exposure
        {
            get { return GH_Exposure.secondary; }
        }

    }
}