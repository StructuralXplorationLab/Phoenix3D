using System;
using System.Collections.Generic;

using Phoenix3D.Reuse;

using Grasshopper.Kernel;


namespace Phoenix3D_Components.Reuse
{
    public class Stock_Component : GH_Component
    {

        public Stock_Component() : base("Stock", "Stock", "Stock of reclaimed element and new element candidates", "Phoenix3D", "  Reuse")
        {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Elements reuse and new", "EL", "List of stock and new elements", GH_ParamAccess.list);
            pManager.AddIntegerParameter("Order elements", "OE", "Sort elements: 0 = Off, 1 = Type (Reuse then New), 2 = Type then ForceCapacity then Length (desc.), 3 = Type then Length then ForceCapacity (desc.)", GH_ParamAccess.item, 0);
            
            pManager[1].Optional = true;
        }


        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Stock", "ST", "Stock of reclaimed elements and new element candidates", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<ElementGroup> ElementGroups = new List<ElementGroup>();
            int SortBy = 0;
            DA.GetDataList(0, ElementGroups);
            DA.GetData(1, ref SortBy);
            DA.SetData(0, new Stock(ElementGroups, (SortStockElementsBy) SortBy));
        }

        protected override System.Drawing.Bitmap Icon => Properties.Resources.stock;

        public override Guid ComponentGuid => new Guid("b80eb706-ed47-4a1e-a78b-97a895515b5f");

        public override GH_Exposure Exposure
        {
            get { return GH_Exposure.secondary; }
        }

    }
}