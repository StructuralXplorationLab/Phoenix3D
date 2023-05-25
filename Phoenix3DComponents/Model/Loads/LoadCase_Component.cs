using System;
using System.Collections.Generic;

using Phoenix3D.Model;

using Grasshopper.Kernel;


namespace Phoenix3D_Components.Model.Loads
{
    public class LoadCase_Component : GH_Component
    {
        public LoadCase_Component() : base("LoadCase", "LoadCase", "Creates a LoadCase", "Phoenix3D", "  Loads")
        {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Name", "NA", "Loadcase Name", GH_ParamAccess.item, "LC1");
            pManager.AddGenericParameter("PointLoads", "PL", "AllPointLoads", GH_ParamAccess.list);
            pManager.AddGenericParameter("DisplacementBounds", "DB", "All Displacement Bounds", GH_ParamAccess.list);
            pManager.AddNumberParameter("SelfWeightFactor", "SW", "Factor for self-weight of members (lumped at nodes; 0 = off; 1.0 -> 10 kg/ms2)", GH_ParamAccess.item, 1.0);

            pManager[0].Optional = true;
            pManager[1].Optional = true;
            pManager[2].Optional = true;
            pManager[3].Optional = true;
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("LoadCase", "LC", "Load Case", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<PointLoad> PointLoads = new List<PointLoad>();
            List<DisplacementBound> DisplacementBounds = new List<DisplacementBound>();
            string Name = "";
            double SelfWeightFactor = 1.0;

            DA.GetData(0, ref Name);
            DA.GetDataList(1, PointLoads);
            DA.GetDataList(2, DisplacementBounds);
            DA.GetData(3, ref SelfWeightFactor);

            LoadCase LC = new LoadCase(Name, SelfWeightFactor);
            foreach(PointLoad PL in PointLoads)
            {
                if(LC.Loads.Contains(PL))
                {
                    this.AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Multiple force or moment vectors are defined for the same Node. Force and moment vectors are added!");
                }
                LC.AddLoad(PL);
            }
            foreach (DisplacementBound DB in DisplacementBounds)
            {
                if (LC.DisplacementBounds.Contains(DB))
                {
                    this.AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Multiple displacement bound vectors are defined for the same Node. The last item is taken!");
                }
                LC.AddDisplacementBound(DB);
            }

            DA.SetData(0, LC);

        }

        protected override System.Drawing.Bitmap Icon => Properties.Resources.loadcase;

        public override Guid ComponentGuid => new Guid("6c97a116-a198-4f10-a808-dfec935fd4d5");

        public override GH_Exposure Exposure
        {
            get { return GH_Exposure.secondary; }
        }

    }
}