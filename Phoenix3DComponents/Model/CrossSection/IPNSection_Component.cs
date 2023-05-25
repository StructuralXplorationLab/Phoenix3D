using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Phoenix3D.Model.CrossSections;
using Rhino.Geometry;

namespace Phoenix3D_Components.Model.CrossSection
{
    public class IPNSection_Component : GH_Component
    {

        public IPNSection_Component()
          : base("IPNSection", "IPN",
              "Standard IPN-Section profile",
              "Phoenix3D", "  CrossSections")
        {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddIntegerParameter("Size", "Size", "IPN-Section size", GH_ParamAccess.item, 100);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Cross Section", "CS", "Cross Section", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            int Size = 100;

            DA.GetData(0, ref Size);

            DA.SetData(0, new IPNSection(Size));
        }

        protected override System.Drawing.Bitmap Icon => Properties.Resources.IPN_section;
        public override Guid ComponentGuid
        {
            get { return new Guid("9D246D83-AF02-48B8-BA4C-7B274AC5DC23"); }
        }

        public override GH_Exposure Exposure
        {
            get { return GH_Exposure.primary; }
        }
    }
}