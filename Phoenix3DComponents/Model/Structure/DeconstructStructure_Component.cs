using Grasshopper.Kernel;

using System;

using Phoenix3D.Model;

namespace Phoenix3D_Components.Model.Deconstruct
{
    public class DeconstructStructure_Component : GH_Component
    {

        public DeconstructStructure_Component() : base("Deconstruct Structure", "DecStr", "deconstructs an existing structure into its parts Members, Nodes, and Supports", "Phoenix3D", "   Geometry")
        {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Structure", "SC", "takes a Structure", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Member", "MB", "Members of a Structure", GH_ParamAccess.list);
            pManager.AddGenericParameter("Nodes", "NO", "Nodes of a Structure", GH_ParamAccess.list);
            pManager.AddGenericParameter("Supports", "SP", "Supports of a Structure", GH_ParamAccess.list);
            pManager.AddGenericParameter("LoadCases", "LC", "Load cases of a Structure", GH_ParamAccess.list);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            //--- INPUT ---
            var str = new Structure();

            DA.GetData(0, ref str);

            // --- SOLVE ---

            // --- OUTPUT ---
            DA.SetDataList(0, str.Members);
            DA.SetDataList(1, str.Nodes);
            DA.SetDataList(2, str.Supports);
            DA.SetDataList(3, str.LoadCases);
        }

        protected override System.Drawing.Bitmap Icon => Properties.Resources.deconstruct_structure;

        public override Guid ComponentGuid => new Guid("6cac83a1-5d29-4092-a05c-04d6a52f55f5");

        public override GH_Exposure Exposure
        {
            get { return GH_Exposure.secondary; }
        }

    }
}