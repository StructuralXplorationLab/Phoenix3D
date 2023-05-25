
using System;
using System.Collections.Generic;

using Phoenix3D.Model;

using Grasshopper;
using Grasshopper.Kernel.Types;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Special;


namespace Phoenix3D_Components.Display
{
    public class DisplayStructure_Component : GH_Component
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

        public DisplayStructure_Component() : base("Display Structure", "Disp Struct", "displays a structure", "Phoenix3D", "Display")
        {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Structure", "SC", "Structure", GH_ParamAccess.item);
            pManager.AddIntegerParameter("DisplayResultsType", "TP", "Type of results to show. Connect a ValueList component for options.", GH_ParamAccess.item);
            pManager.AddTextParameter("LoadCase", "LC", "Name of the load case to visualize", GH_ParamAccess.item);
            pManager.AddNumberParameter("DisplacementScale", "SL", "Scale factor for nodal displacement (default = 0)", GH_ParamAccess.item, 0);
            pManager[2].Optional = true;
            pManager[3].Optional = true;
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddMeshParameter("CrossSection Meshes", "ME", "Cross-sections as Meshes", GH_ParamAccess.tree);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Structure Structure = null;
            int DisplayResultsType = 0;
            string LCName = null;
            LoadCase LC = null;
            double DispScale = 0;

            
            DA.GetData(0, ref Structure);
            DA.GetData(1, ref DisplayResultsType);
            DA.GetData(2, ref LCName);
            DA.GetData(3, ref DispScale);

            //List<LoadCase> LCs = Structure.GetLoadCasesFromNames(new List<string>() { LCName });

            if (!(LCName is null) && Structure.LoadCases.Count > 0)
                LC = Structure.GetLoadCasesFromNames(new List<string>() { LCName })[0];
            else
                LC = null;

            /*
            if (DisplayResultsType != 1)
                DisplayResultsType += 2;
            */

            if (DisplayResultsType == 0)
                DisplayResultsType++;
            else
                DisplayResultsType += 3;

            DataTree<GH_Mesh> MeshTree = DisplayHelper.GetStructureMeshes(Structure, (DisplayResultsType)DisplayResultsType, LC, DispScale);

            DA.SetDataTree(0, MeshTree);
        }

        protected override System.Drawing.Bitmap Icon => Properties.Resources.display_structure;

        public override Guid ComponentGuid => new Guid("104f0eb5-92c1-4ccf-afb2-b67cd8dafe61");


        protected override void BeforeSolveInstance()
        {
            if (this.Params.Input[1].SourceCount <= 0 || this.Params.Input[1].SourceCount != 1 || !(this.Params.Input[1].Sources[0] is GH_ValueList))
                return;
            GH_ValueList source = this.Params.Input[1].Sources[0] as GH_ValueList;
            source.ListMode = GH_ValueListMode.DropDown;
            if (source.ListItems.Count != this.ResultType.Count)
            {
                source.ListItems.Clear();
                for (int index = 0; index < this.ResultType.Count; ++index)
                    source.ListItems.Add(new GH_ValueListItem(this.ResultType[index], (index + 0).ToString()));
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
                        source.ListItems.Add(new GH_ValueListItem(this.ResultType[index], (index + 0).ToString()));
                    source.ExpireSolution(true);
                }
            }
        }
    }
}