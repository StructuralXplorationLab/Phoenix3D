using System;

using Phoenix3D.Model.CrossSections;

using Grasshopper.Kernel;


namespace Phoenix3D_Components.Model.CrossSection
{
    public class SHSection_Component : GH_Component
    {

        public SHSection_Component() : base("SquareHollowSection", "SHS", "Standard SH-Section profile after EN 10210", "Phoenix3D", "  CrossSections")
        {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddNumberParameter("Width [mm]", "W [mm]", "SHSection width in [mm]", GH_ParamAccess.item, 40);
            pManager.AddNumberParameter("Thickness [mm]", "T [mm]", "SHSection wall thickness [mm]", GH_ParamAccess.item, 4);
            pManager[1].Optional = true;
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Cross Section", "CS", "Cross Section", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            double Width = 40;
            double Thickness = 4;

            DA.GetData(0, ref Width);
            DA.GetData(1, ref Thickness);

            DA.SetData(0, new SHSection(Width / 1000, Thickness / 1000));
        }

        protected override System.Drawing.Bitmap Icon => Properties.Resources.rectangular_hollow_section;

        public override Guid ComponentGuid => new Guid("12a218ca-4dcf-4446-999b-84cecf519001");

        public override GH_Exposure Exposure
        {
            get { return GH_Exposure.secondary; }
        }

    }
}