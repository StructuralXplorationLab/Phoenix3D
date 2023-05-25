using System;

using Phoenix3D.Model.Materials;

using Grasshopper.Kernel;


namespace Phoenix3D_Components.Model.Materials
{
    public class Timber_Component : GH_Component
    {
        public Timber_Component() : base("Timber", "Timber", "Timber material (longitudinal only)", "Phoenix3D", "  Materials")
        {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddNumberParameter("Density", "D [kg/m3]", "Material density in [kg/m3]", GH_ParamAccess.item, 500);
            pManager.AddNumberParameter("Young's Modulus", "E0 [N/mm2]", "Material Young's Modulus = Elastic Modulus parallel to fibers in [MPa] = [N/mm2]", GH_ParamAccess.item, 12000);
            pManager.AddNumberParameter("Poisson Ratio", "nu [-]", "Material density in [kg/m3]", GH_ParamAccess.item, 0.25);
            pManager.AddNumberParameter("Tension Strength", "ft [N/mm2]", "Material tension strength (positive) in [MPa] = [N/mm2]", GH_ParamAccess.item, 12);
            pManager.AddNumberParameter("Compression Strength", "fc [N/mm2]", "Material compression strength (here positive) in [MPa] = [N/mm2]", GH_ParamAccess.item, 8);
            pManager.AddNumberParameter("Material safety factor", "y0 [-]", "Material safety factor for yield strength fd0 = fy/y0", GH_ParamAccess.item, 1.0);
            pManager.AddNumberParameter("Material safety factor for stability", "y1 [-]", "Material safety factor for stability fd1 = fy/y1", GH_ParamAccess.item, 1.1);
            pManager.AddNumberParameter("kmod", "kmod [-]", "Factor for environment", GH_ParamAccess.item, 0.8);

            pManager[0].Optional = true;
            pManager[1].Optional = true;
            pManager[2].Optional = true;
            pManager[3].Optional = true;
            pManager[4].Optional = true;
            pManager[5].Optional = true;
            pManager[6].Optional = true;
            pManager[7].Optional = true;
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Material", "MA", "Material", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            double D = 500;
            double E = 12000;
            double nu = 0.25;
            double ft = 12;
            double fc = 8;
            double y0 = 1.0;
            double y1 = 1.1;
            double kmod = 0.8;

            DA.GetData(0, ref D);
            DA.GetData(1, ref E);
            DA.GetData(2, ref nu);
            DA.GetData(3, ref ft);
            DA.GetData(4, ref fc);
            DA.GetData(5, ref y0);
            DA.GetData(6, ref y1);
            DA.GetData(7, ref kmod);

            DA.SetData(0, new Timber(Math.Abs(D),Math.Abs(E),Math.Abs(nu),Math.Abs(ft),Math.Abs(fc),Math.Abs(y0),Math.Abs(y1),Math.Abs(kmod)));

        }

        protected override System.Drawing.Bitmap Icon => Properties.Resources.timber;
        public override Guid ComponentGuid => new Guid("ca2edb42-1d35-4ca9-a18c-81213d9b1001");

    }
}