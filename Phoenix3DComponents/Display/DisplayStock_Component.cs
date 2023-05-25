using Phoenix3D.Model;
using Phoenix3D.Reuse;

using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Grasshopper.Kernel.Special;

using Rhino.Geometry;

using System;
using System.Collections.Generic;


namespace Phoenix3D_Components.Display
{
    public class DisplayStock_Component : GH_Component
    {
        private List<string> ResultType = new List<string>()
        {
            "Blank",
            "Forces",
            "Utilization",
            "Reuse New",
            "Mass",
            "Environmental Impact"
        };

        private List<string> StockType = new List<string>()
        {
            "Full Stock Blank",
            "Full Stock",
            "Used Stock",
        };

        public DisplayStock_Component() : base("Display Stock", "Disp Stock", "displays the stock", "Phoenix3D", "Display")
        {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Structure", "SC", "Structure", GH_ParamAccess.item);
            pManager.AddGenericParameter("Stock", "ST", "Stock", GH_ParamAccess.item);
            pManager.AddPlaneParameter("Plane", "Plane", " plane on which the stock elements are displayed", GH_ParamAccess.item, new Plane());
            pManager.AddVectorParameter("SpacingVector", "DX", "spacing between stock elements", GH_ParamAccess.item, new Vector3d(1, 1, 0));
            pManager.AddIntegerParameter("StockType", "TY", "connect a value list for options", GH_ParamAccess.item);
            pManager.AddIntegerParameter("ResultsType", "TP", "connect a value list for options", GH_ParamAccess.item);
            pManager.AddTextParameter("LoadCase", "LC", "", GH_ParamAccess.item);

            pManager[0].Optional = true;
            pManager[2].Optional = true;
            pManager[3].Optional = true;
            pManager[6].Optional = true;
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddMeshParameter("CutOff", "CO", "StockElements", GH_ParamAccess.tree);
            pManager.AddMeshParameter("UsedElements", "EL", "UsedElements", GH_ParamAccess.tree);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Stock temp_Stock = null;
            Structure temp_Structure = null;
            Plane Plane = new Plane();
            Vector3d Spacing = new Vector3d();
            int DSType = 2;
            int DRType = 1;
            var lc = default(string);
            LoadCase LC = null;


            DA.GetData(1, ref temp_Stock);
            bool structure_provided = DA.GetData(0, ref temp_Structure);
            DA.GetData(2, ref Plane);
            DA.GetData(3, ref Spacing);
            DA.GetData(4, ref DSType);
            DA.GetData(5, ref DRType);

            if(DA.GetData(6, ref lc))
                LC = temp_Structure.GetLoadCasesFromNames(new List<string>() { lc })[0];




            if (DRType == 0)
                DRType++;
            else
                DRType += 3;

            
            Stock Stock = temp_Stock;
            Stock.ResetNext();
            Stock.ResetStacks();
            Stock.ResetRemainLenghtsTemp();
            Stock.ResetRemainLenghts();


            DataTree<GH_Mesh> StockElements = new DataTree<GH_Mesh>();
            DataTree<GH_Mesh> StructureElements = new DataTree<GH_Mesh>();

            if (!structure_provided)
            {
                StockElements = DisplayHelper.GetStockMeshesOnly(Stock, (DisplayResultsType)DRType, Plane.Clone(), Spacing.X, Spacing.Y);
            }
            else
            {
                Structure Structure = temp_Structure;
                switch (DSType)
                {
                    case 0: StockElements = DisplayHelper.GetStockMeshesOnly(Stock, (DisplayResultsType)DRType, Plane, Spacing.X, Spacing.Y); break;
                    case 1:
                        {
                            DataTree<GH_Mesh>[] FullMeshes = DisplayHelper.GetStockMeshesFull(Stock, Structure, (DisplayResultsType)DRType, LC, Plane, Spacing.X, Spacing.Y);
                            StockElements = FullMeshes[0];
                            StructureElements = FullMeshes[1];
                            break;
                        }
                    case 2:
                        {
                            DataTree<GH_Mesh>[] FullMeshes = DisplayHelper.GetStockMeshesUsed(Stock, Structure, (DisplayResultsType)DRType, LC, Plane, Spacing.X, Spacing.Y);
                            StockElements = FullMeshes[0];
                            StructureElements = FullMeshes[1];
                            break;
                        }
                    default: StockElements = DisplayHelper.GetStockMeshesOnly(Stock, (DisplayResultsType)DRType, Plane, Spacing.X, Spacing.Y); break;
                }
            }
            DA.SetDataTree(0, StockElements);
            DA.SetDataTree(1, StructureElements);

            Stock.ResetNext();
            Stock.ResetStacks();
            Stock.ResetRemainLenghtsTemp();
            Stock.ResetRemainLenghts();
        }

        protected override System.Drawing.Bitmap Icon => Properties.Resources.display_stock;

        public override Guid ComponentGuid => new Guid("d8ef9052-8623-4b4a-a7d4-c1baa3580cee");

        protected override void BeforeSolveInstance()
        {
            // value list fpr ResultType
            if (this.Params.Input[5].SourceCount <= 0 || this.Params.Input[5].SourceCount != 1 || !(this.Params.Input[5].Sources[0] is GH_ValueList))
                return;
            GH_ValueList source = this.Params.Input[5].Sources[0] as GH_ValueList;
            source.ListMode = GH_ValueListMode.DropDown;
            if (source.ListItems.Count != this.ResultType.Count)
            {
                source.ListItems.Clear();
                for (int index = 0; index < this.ResultType.Count; ++index)
                    source.ListItems.Add(new GH_ValueListItem(this.ResultType[index], (index).ToString()));
                source.ExpireSolution(true);
            }
            else
            {
                bool flag = true;
                for (int index = 0; index < this.ResultType.Count; ++index)
                {
                    if (source.ListItems[index].Name != this.ResultType[index])
                    {
                        flag = false;
                        break;
                    }
                }
                if (!flag)
                {
                    source.ListItems.Clear();
                    for (int index = 0; index < this.ResultType.Count; ++index)
                        source.ListItems.Add(new GH_ValueListItem(this.ResultType[index], (index + 1).ToString()));
                    source.ExpireSolution(true);
                }
            }

            // value list for StockType
            if (this.Params.Input[4].SourceCount <= 0 || this.Params.Input[4].SourceCount != 1 || !(this.Params.Input[4].Sources[0] is GH_ValueList))
                return;
            GH_ValueList source2 = this.Params.Input[4].Sources[0] as GH_ValueList;
            source2.ListMode = GH_ValueListMode.DropDown;
            if (source2.ListItems.Count != this.StockType.Count)
            {
                source2.ListItems.Clear();
                for (int index = 0; index < this.StockType.Count; ++index)
                    source2.ListItems.Add(new GH_ValueListItem(this.StockType[index], (index).ToString()));
                source2.ExpireSolution(true);
            }
            else
            {
                bool flag = true;
                for (int index = 0; index < this.StockType.Count; ++index)
                {
                    if (source2.ListItems[index].Name != this.StockType[index])
                    {
                        flag = false;
                        break;
                    }
                }
                if (!flag)
                {
                    source2.ListItems.Clear();
                    for (int index = 0; index < this.StockType.Count; ++index)
                        source2.ListItems.Add(new GH_ValueListItem(this.StockType[index], (index + 0).ToString()));
                    source2.ExpireSolution(true);
                }
            }
        }
    }
}