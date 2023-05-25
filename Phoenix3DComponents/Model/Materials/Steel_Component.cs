using System;

using Phoenix3D.Model.Materials;

using Grasshopper.Kernel;


namespace Phoenix3D_Components.Model.Materials
{
    public class Steel_Component : GH_Component
    {
        public Steel_Component()
          : base("Steel", "Steel", "Isotropic steel material", "Phoenix3D", "  Materials")
        {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddNumberParameter("Density", "D [kg/m3]", "Material density in [kg/m3]", GH_ParamAccess.item, 7850);
            pManager.AddNumberParameter("Young's Modulus", "E [N/mm2]", "Material Young's Modulus = Elastic Modulus in [MPa] = [N/mm2]", GH_ParamAccess.item, 210000);
            pManager.AddNumberParameter("Poisson Ratio", "nu [-]", "Material density in [kg/m3]", GH_ParamAccess.item, 0.3);
            pManager.AddNumberParameter("Tension Strength", "ft [N/mm2]", "Material tension (positive) in [MPa] = [N/mm2]", GH_ParamAccess.item, 235);
            pManager.AddNumberParameter("Compression Strength", "fc [N/mm2]", "Material compression strength (here positive) in [MPa] = [N/mm2]", GH_ParamAccess.item, 235);
            pManager.AddNumberParameter("Material safety factor", "y0 [-]", "Material safety factor for yield strength fd0 = fy/y0", GH_ParamAccess.item, 1.0);
            pManager.AddNumberParameter("Material safety factor for stability", "y1 [-]", "Material safety factor for stability fd1 = fy/y1", GH_ParamAccess.item, 1.1);

            pManager[0].Optional = true;
            pManager[1].Optional = true;
            pManager[2].Optional = true;
            pManager[3].Optional = true;
            pManager[4].Optional = true;
            pManager[5].Optional = true;
            pManager[6].Optional = true;
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Material", "MA", "Material", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            double D = 7850;
            double E = 210000;
            double nu = 0.3;
            double ft = 235;
            double fc = 235;
            double y0 = 1.0;
            double y1 = 1.1;

            DA.GetData(0, ref D);
            DA.GetData(1, ref E);
            DA.GetData(2, ref nu);
            DA.GetData(3, ref ft);
            DA.GetData(4, ref fc);
            DA.GetData(5, ref y0);
            DA.GetData(6, ref y1);

            DA.SetData(0, new Steel(Math.Abs(D), Math.Abs(E), Math.Abs(nu), Math.Abs(ft), Math.Abs(fc), Math.Abs(y0), Math.Abs(y1),1));

        }

        protected override System.Drawing.Bitmap Icon => Properties.Resources.steel;
        public override Guid ComponentGuid => new Guid("ca2edb42-1d35-4ca9-a18c-81213d9b102e");

    }
}