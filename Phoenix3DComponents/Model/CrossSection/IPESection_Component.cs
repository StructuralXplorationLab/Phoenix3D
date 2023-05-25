using System;

using Phoenix3D.Model.CrossSections;

using Grasshopper.Kernel;

namespace Phoenix3D_Components.Model.CrossSection
{
    public class IPESection_Component : GH_Component
    {

        public IPESection_Component() : base("IPESection", "IPE", "Standard IPE-Section profile after EN 16828; 2015-4", "Phoenix3D", "  CrossSections")
        {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddIntegerParameter("Size", "Size", "IPE-Section size", GH_ParamAccess.item, 100);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Cross Section", "CS", "Cross Section", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            
            int Size = 100;

            DA.GetData(0, ref Size);

            DA.SetData(0, new IPESection(Size));
        }

        protected override System.Drawing.Bitmap Icon => Properties.Resources.IPE_section;

        public override Guid ComponentGuid => new Guid("12a218ca-4dcf-4446-999b-84cecf519011");

        public override GH_Exposure Exposure
        {
            get { return GH_Exposure.primary; }
        }

    }
}