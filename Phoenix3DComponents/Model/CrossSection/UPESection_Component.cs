using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Phoenix3D.Model.CrossSections;
using Rhino.Geometry;

namespace Phoenix3D_Components.Model.CrossSection
{
    public class UPESection_Component : GH_Component
    {

        public UPESection_Component()
          : base("UPESection", "UPE",
              "Standard UPE-Section profile",
              "Phoenix3D", "  CrossSections")
        {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddIntegerParameter("Size", "Size", "UPE-Section size", GH_ParamAccess.item, 100);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Cross Section", "CS", "Cross Section", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            int Size = 100;

            DA.GetData(0, ref Size);

            DA.SetData(0, new UPESection(Size));
        }

        protected override System.Drawing.Bitmap Icon => Properties.Resources.UPE_section;
        public override Guid ComponentGuid
        {
            get { return new Guid("6A438023-06C4-4344-8F7A-2AD317BBA72B"); }
        }

        public override GH_Exposure Exposure
        {
            get { return GH_Exposure.primary; }
        }
    }
}