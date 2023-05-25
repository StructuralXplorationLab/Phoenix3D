using Grasshopper.Kernel;
using Grasshopper;

using Rhino.Geometry;

using System;
using System.Collections.Generic;

using Phoenix3D.Model;


namespace Phoenix3D_Components.Display
{
    public class ResultsComponent : GH_Component
    {
        private List<double> utilization = new List<double>();
        private DataTree<double> normalForces = new DataTree<double>();
        private bool showOnStructure = true;
        private Structure Structure = new Structure();
        private int loadcaseNumber = 0;

        public ResultsComponent(): base("Display Results", "Disp Res", "shows results of optimization", "Phoenix3D", "Display")
        {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Stucture", "SC", "Structure", GH_ParamAccess.item);
            pManager.AddBooleanParameter("ShowResultsOnStructure", "DI", "displays information of members on the members in the structure", GH_ParamAccess.item, true);
            pManager.AddTextParameter("LoadCase", "LC", "Name of the load case to visualize", GH_ParamAccess.item);

            pManager[1].Optional = true;
            pManager[2].Optional = true;
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("OptimizationResults", "RE", "key facts of optimization", GH_ParamAccess.item);
            pManager.AddNumberParameter("Utilization", "UT", "Utilization of each member in the structure", GH_ParamAccess.list);
            pManager.AddNumberParameter("Normal Forces", "NX", "Normal Forces of members of the structure", GH_ParamAccess.tree);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // --- INPUT ---
            var str_tmp = new Structure();
            string results;
            string lc = (default);

            DA.GetData(0, ref str_tmp);
            DA.GetData(1, ref showOnStructure);
            DA.GetData(2, ref lc);
            var str = str_tmp.Clone();
            Structure = str;

            // --- SOLVE ---
            results = str.Results.ToString();

            //List<LoadCase> LCs = Structure.GetLoadCasesFromNames(new List<string>() { lc });

            if (!(lc is null) && Structure.LoadCases.Count > 0)
                loadcaseNumber = Structure.GetLoadCasesFromNames(new List<string>() { lc })[0].Number;
            else
                loadcaseNumber = -1;

            //loadcaseNumber = LCs[0].Number;

            if (loadcaseNumber != -1 && (loadcaseNumber < 0 || loadcaseNumber > str.LoadCases.Count))
                throw new ArgumentException("Choose a valid LoadCase!");

            utilization.Clear();

            foreach (var util in str.Results.Utilization)
                utilization.Add(util);

            int i = 0;

            normalForces.Clear();

            foreach (IMember1D member in str.Members)
            {
                foreach (KeyValuePair<LoadCase, List<double>> nx in member.Nx)
                    normalForces.Add(nx.Value[0], new Grasshopper.Kernel.Data.GH_Path(i));

                ++i;
            }
                

            // --- OUTPUT ---
            DA.SetData(0, results);
            DA.SetDataList(1, utilization);
            DA.SetDataTree(2, normalForces);
        }

        public override void DrawViewportWires(IGH_PreviewArgs args)
        {
            if(showOnStructure)
            {
                var color = System.Drawing.Color.Black;
                int i = 0;
                foreach (IMember1D member in Structure.Members)
                {
                    var line = new Line(new Point3d(member.From.X, member.From.Y, member.From.Z), new Point3d(member.To.X, member.To.Y, member.To.Z));
                    var location = new Point3d(line.PointAt(0.5));
                    double force = 0;
                    if (loadcaseNumber != -1)
                        force = member.Nx[Structure.LoadCases[loadcaseNumber]][0];

                    decimal force_round = Math.Round(new decimal(force), 3);
                    decimal util = Math.Round(new decimal(Structure.Results.Utilization[i]), 2) * 100;
                    util = Math.Round(util, 0);
                    var text = "N = " + force_round + "\n";
                    text += "ID: " + member.Number + "\n";
                    text += "Util: " + util + "%" + "\n";
                    text += "Sec: " + member.Assignment.ElementGroups[0].CrossSection;
                    args.Display.Draw2dText(text, color, location, true);
                    ++i;
                }
            }
            
        }
        public override void DrawViewportMeshes(IGH_PreviewArgs args)
        {
            if (showOnStructure)
            {
                var color = System.Drawing.Color.Black;

                int i = 0;
                foreach (IMember1D member in Structure.Members)
                {
                    var line = new Line(new Point3d(member.From.X, member.From.Y, member.From.Z), new Point3d(member.To.X, member.To.Y, member.To.Z));
                    var location = new Point3d(line.PointAt(0.5));
                    //var force = member.Nx[Structure.LoadCases[loadcaseNumber]][0];

                    double force = 0;
                    if (loadcaseNumber != -1)
                        force = member.Nx[Structure.LoadCases[loadcaseNumber]][0];

                    decimal force_round = Math.Round(new decimal(force), 3);
                    decimal util = Math.Round(new decimal(Structure.Results.Utilization[i]), 2) * 100;
                    util = Math.Round(util, 0);
                    var text = "N = " + force_round + "\n";
                    text += "ID: " + member.Number + "\n";
                    text += "Util: " + util + "%" + "\n";
                    text += "Sec: " + member.Assignment.ElementGroups[0].CrossSection;
                    args.Display.Draw2dText(text, color, location, true);
                    ++i;
                }
            }
        }

        protected override System.Drawing.Bitmap Icon => Properties.Resources.display_results;

        public override Guid ComponentGuid => new Guid("94e4dcf9-f958-41ab-baef-5fe805a8141b");

    }
}