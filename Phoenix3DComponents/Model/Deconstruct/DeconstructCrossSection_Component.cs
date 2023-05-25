using Grasshopper.Kernel;

using Rhino.Geometry;

using System;
using System.Collections.Generic;

using Cardinal.Model.CrossSections;

namespace CardinalComponents.Model.Deconstruct
{
    public class DeconstructCrossSection_Component : GH_Component
    {

        public DeconstructCrossSection_Component(): base("Deconstruct CrossSection", "DecCrSec","deconstructs a CrossSection into its parts Name, Area, Iy, Iz, It, Wy, Wz, Wt ","Cardinal", "Elements")
        {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("CrossSections", "CS", "takes a list of CrossSections", GH_ParamAccess.list);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Name", "N", "Name of CrossSection", GH_ParamAccess.list);
            pManager.AddNumberParameter("Area", "A", "Area of CrossSection", GH_ParamAccess.list);
            pManager.AddNumberParameter("Iy", "Iy", "Iy of CrossSection", GH_ParamAccess.list);
            pManager.AddNumberParameter("Iz", "Iz", "Iz of CrossSection", GH_ParamAccess.list);
            pManager.AddNumberParameter("It", "It", "It of CrossSection", GH_ParamAccess.list);
            pManager.AddNumberParameter("Wy", "Wy", "Wy of CrossSection", GH_ParamAccess.list);
            pManager.AddNumberParameter("Wz", "Wz", "Wz of CrossSection", GH_ParamAccess.list);
            pManager.AddNumberParameter("Wt", "Wt", "Wt of CrossSection", GH_ParamAccess.list);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // --- INPUT ---
            var crossSections = new List<ICrossSection>();
            DA.SetDataList(0, crossSections);

            // --- SOLVE ---
            var names = new List<string>();
            var areas = new List<double>();
            var iys = new List<double>();
            var izs = new List<double>();
            var its = new List<double>();
            var wys = new List<double>();
            var wzs = new List<double>();
            var wts = new List<double>();

            foreach (var csec in crossSections)
            {
                names.Add(csec.Name);
                areas.Add(csec.Area);
                iys.Add(csec.Iy);
                izs.Add(csec.Iz);
                its.Add(csec.It);
                wys.Add(csec.Wy);
                wzs.Add(csec.Wz);
                wts.Add(csec.Wt);
            }

            // --- OUTPUT ---
            DA.SetDataList(0, names);
            DA.SetDataList(1, areas);
            DA.SetDataList(2, iys);
            DA.SetDataList(3, izs);
            DA.SetDataList(4, its);
            DA.SetDataList(5, wys);
            DA.SetDataList(6, wzs);
            DA.SetDataList(7, wts);
        }

        protected override System.Drawing.Bitmap Icon => null;

        public override Guid ComponentGuid => new Guid("b8dba0b8-5e70-4721-a911-f7f86ff8df4c");

    }
}