using System;
using System.Collections.Generic;
using Phoenix3D.Model;
using Grasshopper.Kernel;

namespace Phoenix3D_Components.Model
{
    public class Structure_Component : GH_Component
    {
        public Structure_Component() : base("Structure", "S", "Structure", "Phoenix3D", "   Geometry")
        {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Members", "MB", "Members of the structure", GH_ParamAccess.list);
            pManager.AddGenericParameter("Supports", "SP", "Supports of the structure", GH_ParamAccess.list);
            pManager.AddGenericParameter("LoadCases", "LC", "Load Cases acting on the structure", GH_ParamAccess.list);
            pManager[1].Optional = true;
            pManager[2].Optional = true;
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Structure", "SC", "Structure", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<IMember> Members = new List<IMember>();
            List<Support> Supports = new List<Support>();
            List<LoadCase> LoadCases = new List<LoadCase>();
            DA.GetDataList(0, Members);
            DA.GetDataList(1, Supports);
            DA.GetDataList(2, LoadCases);

            Structure S = new Structure();
            foreach(IMember M in Members)
            {
                S.AddMember(M);
            }

            foreach(LoadCase LC in LoadCases)
            {
                S.AddLoadcase(LC);
            }

            foreach (Support Fix in Supports)
            {
                S.AddSupport(Fix);
            }
            DA.SetData(0, S.Clone());
        }

        protected override System.Drawing.Bitmap Icon => Properties.Resources.structure;

        public override GH_Exposure Exposure => GH_Exposure.primary;

        public override Guid ComponentGuid => new Guid("2313f09f-392a-44ac-8322-767d1e173331");

    }
}