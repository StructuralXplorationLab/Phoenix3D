using System;

using Phoenix3D.Model.CrossSections;

using Grasshopper.Kernel;


namespace Phoenix3D_Components.Model.CrossSection
{
    public class RectangularSection_Component : GH_Component
    {

        public RectangularSection_Component() : base("RectangularSection", "R", "Rectangular cross-section with specified height and width", "Phoenix3D", "  CrossSections")
        {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddNumberParameter("Height [mm]", "H [mm]", "Rectangular section height in [mm]", GH_ParamAccess.item, 50.0);
            pManager.AddNumberParameter("Width [mm]", "W [mm]", "Rectangular section width in [mm]", GH_ParamAccess.item, 30.0);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Cross Section", "CS", "Cross Section", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            
            double Height = 50;
            double Width = 30;

            DA.GetData(0, ref Height);
            DA.GetData(1, ref Width);

            DA.SetData(0, new RectangularSection(Height / 1000, Width / 1000));
        }

        protected override System.Drawing.Bitmap Icon => Properties.Resources.rectangular_section;

        public override Guid ComponentGuid => new Guid("12a218ca-4dcf-4446-999b-84cecf519004");

        public override GH_Exposure Exposure
        {
            get { return GH_Exposure.secondary; }
        }

    }
}