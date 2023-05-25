using System;

using Grasshopper.Kernel;

using Phoenix3D.Model.CrossSections;


namespace Phoenix3D_Components.Model.CrossSection
{
    public class GenericSection_Component : GH_Component
    {

        public GenericSection_Component() : base("GenericSection", "GenericSection", "Creates a generic cross section", "Phoenix3D", "  CrossSections")
        {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddNumberParameter("Area", "A [mm2]", "Cross section area in [mm2]", GH_ParamAccess.item, 50*50);
            pManager.AddNumberParameter("Iy", "Iy [mm4]", "Area moment of inertia about y-Axis in [mm4]", GH_ParamAccess.item, 50 * 50 * 50 * 50 / 12);
            pManager.AddNumberParameter("Iz", "Iz [mm4]", "Area moment of inertia about z-Axis in [mm4]", GH_ParamAccess.item, 50 * 50 * 50 * 50 / 12);
            pManager.AddNumberParameter("Wy", "Wy [mm3]", "Section modulus about y-Axis in [mm3]", GH_ParamAccess.item, 50 * 50 * 50 / 6);
            pManager.AddNumberParameter("Wz", "Wz [mm3]", "Section modulus about z-Axis in [mm3]", GH_ParamAccess.item, 50 * 50 * 50 / 6);
            pManager.AddNumberParameter("It", "It [mm3]", "Torsional moment of inertia in [mm4]", GH_ParamAccess.item, 0.141*50*50*50*50);
            pManager.AddNumberParameter("Wt", "Wt [mm3]", "Section modulus about x-Axis in [mm3]", GH_ParamAccess.item, 50 * 50 * 50 / 6);

        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Cross Section", "CS", "Cross Section", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            double Area = 50 * 50;
            double Iy = 50 * 50 * 50 * 50 / 12;
            double Iz = Iy;
            double Wy = 50 * 50 * 50 / 6;
            double Wz = Wy;
            double It = 0.141 * 50 * 50 * 50 * 50;
            double Wt = Wy;

            DA.GetData(0, ref Area);
            DA.GetData(1, ref Iy);
            DA.GetData(2, ref Iz);
            DA.GetData(3, ref Wy);
            DA.GetData(4, ref Wz);
            DA.GetData(5, ref It);
            DA.GetData(6, ref Wt);

            Area /= 1e6;
            Iy /= 1e12;
            Iz = Iy;
            Wy /= 1e9;
            Wz = Wy;
            It /= 1e12;
            Wt /= 1e9;
            var name = "GenericSection";



            GenericSection GS = new GenericSection(name, Area, Iy, Iz, Wy, Wz, Wt, It, 0, 0);

            DA.SetData(0, GS);
        }

        protected override System.Drawing.Bitmap Icon => Properties.Resources.generic_section;

        public override Guid ComponentGuid => new Guid("75dbd239-149c-490d-953e-098502485527");

        public override GH_Exposure Exposure
        {
            get { return GH_Exposure.secondary; }
        }

    }
}