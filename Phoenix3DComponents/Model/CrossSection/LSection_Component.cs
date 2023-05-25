using System;

using Phoenix3D.Model.CrossSections;

using Grasshopper.Kernel;


namespace Phoenix3D_Components.Model.CrossSection
{
    public class LSection_Component : GH_Component
    {

        public LSection_Component() : base("LSection", "L", "Standard L-Section profile after EN 10056-1:2017", "Phoenix3D", "  CrossSections")
        {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddNumberParameter("Height [mm]", "H [mm]", "LSection height in [mm]", GH_ParamAccess.item, 50.0);
            pManager.AddNumberParameter("Width [mm]", "W [mm]", "LSection width in [mm]. If not profided, value H is taken.", GH_ParamAccess.item);
            pManager.AddNumberParameter("Thickness [mm]", "T [mm]", "LSection flange thickness [mm]", GH_ParamAccess.item, 6);
            pManager[1].Optional = true;
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Cross Section", "CS", "Cross Section", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            
            double Height = 50;
            double Width = 50;
            double Thickness = 6;

            DA.GetData(0, ref Height);
            if (!DA.GetData(1, ref Width))
            {
                Width = Height;
            }
            DA.GetData(2, ref Thickness);

            DA.SetData(0, new LSection(Height / 1000, Width / 1000, Thickness / 1000));
        }

        protected override System.Drawing.Bitmap Icon => Properties.Resources.L_section;

        public override Guid ComponentGuid => new Guid("12a218ca-4dcf-4446-999b-84cecf519080");

        public override GH_Exposure Exposure
        {
            get { return GH_Exposure.primary; }
        }

    }
}