using System;

using Grasshopper.Kernel;

using Phoenix3D.Reuse;
using Phoenix3D.Model.Materials;
using Phoenix3D.Model.CrossSections;


namespace Phoenix3D_Components.Reuse
{
    public class NewElementGroup_Component : GH_Component
    {

        public NewElementGroup_Component() : base("New Element", "NewElement", "Creates a new element candidate", "Phoenix3D", "  Reuse")
        {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Material", "MA", "Element material", GH_ParamAccess.item);
            pManager.AddGenericParameter("CrossSection", "CS", "Element cross section", GH_ParamAccess.item);
            pManager.AddTextParameter("Name", "NA", "Element group name", GH_ParamAccess.item, "NEW");

            pManager[2].Optional = true;
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("New ElementGroup", "EL", "Group of new elements", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            IMaterial Mat = new Steel();
            ICrossSection CS = new CircularSection();
            string Name = null;

            DA.GetData(0, ref Mat);
            DA.GetData(1, ref CS);
            DA.GetData(2, ref Name);
            DA.SetData(0, new ElementGroup(Mat, CS, Name));
        }

        protected override System.Drawing.Bitmap Icon => Properties.Resources.new_element;

        public override Guid ComponentGuid => new Guid("45a74b75-6eef-47b1-9f93-b34dddc660c7");

        public override GH_Exposure Exposure
        {
            get { return GH_Exposure.primary; }
        }

    }
}