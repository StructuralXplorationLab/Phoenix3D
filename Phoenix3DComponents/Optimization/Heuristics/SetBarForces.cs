using Phoenix3D.Model;

using Grasshopper.Kernel;

using System;
using System.Collections.Generic;

namespace Phoenix3D_Components.Optimization.Heuristics
{
    public class SetBarForces : GH_Component
    {

        public SetBarForces() : base("Set Bar Forces", "SetBarForces", "Set bar forces obtained from external analysis", "Phoenix3D", " Optimization")
        {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Member", "MB", "Bar", GH_ParamAccess.item);
            pManager.AddTextParameter("LoadCaseName", "LC", "Name of the LoadCase", GH_ParamAccess.list);
            pManager.AddNumberParameter("Force", "FO", "Force", GH_ParamAccess.list);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Member", "MB", "creates a member (Bar) for a structure", GH_ParamAccess.item);
        }   

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Bar temp_B = new Bar(new Node(0, 0, 0), new Node(1, 1, 1));
            List<string> LoadCaseNames = new List<string>();
            List<double> Forces = new List<double>();

            DA.GetData(0, ref temp_B);
            DA.GetDataList(1, LoadCaseNames);
            DA.GetDataList(2, Forces);

            var B = temp_B.Clone();

                for (int i = 0; i < LoadCaseNames.Count; i++)
                {
                    if(B is Bar B_bar)
                    {
                        B_bar.AddNormalForce(new LoadCase(LoadCaseNames[i]), new List<double>() { Forces[i] });
                    }
                    
                }
                DA.SetData(0, B);
           
        }

        protected override System.Drawing.Bitmap Icon => Properties.Resources.bar_nx;

        public override Guid ComponentGuid => new Guid("268a8471-1b12-4374-964b-597a3d29ae2c");

        public override GH_Exposure Exposure
        {
            get { return GH_Exposure.tertiary; }
        }

    }
}