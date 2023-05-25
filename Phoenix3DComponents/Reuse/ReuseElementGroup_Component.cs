using System;

using Phoenix3D.Reuse;

using Grasshopper.Kernel;

using Phoenix3D.Model.Materials;
using Phoenix3D.Model.CrossSections;


namespace Phoenix3D_Components.Reuse
{
    public class ReuseElementGroup_Component : GH_Component
    {

        public ReuseElementGroup_Component() : base("Reused Element", "Reused Element", "Creates a group of identical stock elements for reuse", "Phoenix3D", "  Reuse")
        {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Material", "MA", "Element material", GH_ParamAccess.item);
            pManager.AddGenericParameter("CrossSection", "CS", "Element cross section", GH_ParamAccess.item);
            pManager.AddNumberParameter("Length", "LE", "Element length", GH_ParamAccess.item);
            pManager.AddIntegerParameter("Amount of elements", "NB", "The number of available identical elements", GH_ParamAccess.item);
            pManager.AddBooleanParameter("Elements can be cut", "CT", "True = elements can be cut, False = elements cannot be cut", GH_ParamAccess.item, true);
            pManager.AddTextParameter("Name", "NA", "Name of the element group", GH_ParamAccess.item, "REUSED");

            pManager[0].Optional = false;
            pManager[1].Optional = false;
            pManager[2].Optional = false;
            pManager[3].Optional = false;
            pManager[4].Optional = true;
            pManager[5].Optional = true;
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Reuse ElementGroup", "EL", "Group of identical stock elements", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            IMaterial Mat = new Steel();
            ICrossSection CS = new CircularSection();
            double L = 0;
            int N = 0;
            bool CBC = true;
            string Name = null;

            DA.GetData(0, ref Mat);
            DA.GetData(1, ref CS);
            DA.GetData(2, ref L);
            DA.GetData(3, ref N);
            DA.GetData(4, ref CBC);
            DA.GetData(5, ref Name);
            DA.SetData(0, new ElementGroup(ElementType.Reuse, Mat, CS, L, N, CBC, Name));
        }

        protected override System.Drawing.Bitmap Icon => Properties.Resources.reused_element;
        public override Guid ComponentGuid => new Guid("45a74b75-6eef-47b1-9f93-b34dddc660a9");

        public override GH_Exposure Exposure
        {
            get { return GH_Exposure.primary; }
        }

    }
}