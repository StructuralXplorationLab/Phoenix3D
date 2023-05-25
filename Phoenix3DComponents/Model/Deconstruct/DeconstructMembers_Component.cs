using Grasshopper.Kernel;

using Rhino.Geometry;

using System;
using System.Collections.Generic;

using Cardinal.Model;
using Cardinal.Model.CrossSections;
using Cardinal.Model.Materials;

namespace CardinalComponents.Model.Deconstruct
{
    public class DeconstructMembers_Component : GH_Component
    {

        public DeconstructMembers_Component() : base("Deconstruct Members", "DecMem", "deconstructs a lsit of Members into its parts Lines, CrossSections, Materials, and Normal forces", "Cardinal", "Elements")
        {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Members", "M", "takes Members as a list", GH_ParamAccess.list);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddLineParameter("Lines", "L", "Lines of Members", GH_ParamAccess.list);
            pManager.AddGenericParameter("CrossSections", "CS", "CrossSections of Members", GH_ParamAccess.list);
            pManager.AddGenericParameter("Material", "Mat", "Material of Memebers", GH_ParamAccess.list);
            pManager.AddGenericParameter("Normal forces", "NF", "Normal forces of Members for all load cases", GH_ParamAccess.list);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // --- INPPUT ---
            var members = new List<IMember1D>();

            DA.GetDataList(0, members);

            // --- SOLVE ---
            var lines = new List<Line>();
            var crossSections = new List<ICrossSection>();
            var materials = new List<IMaterial>();
            var normalForces = new List<Dictionary<LoadCase, double>>();

            foreach(IMember1D member in members)
            {
                lines.Add(new Line(new Point3d(member.From.X, member.From.Y, member.From.Z), new Point3d(member.To.X, member.To.Y, member.To.Z)));
                crossSections.Add(member.CrossSection);
                materials.Add(member.Material);
                normalForces.Add(member.NormalForces);
            }

            // --- OUTPUT ---
            DA.SetDataList(0, lines);
            DA.SetDataList(1, crossSections);
            DA.SetDataList(2, materials);
            DA.SetDataList(3, normalForces);
        }

        protected override System.Drawing.Bitmap Icon => null;

        public override Guid ComponentGuid => new Guid("dbfaa285-f62d-4b10-9200-19ea3140bd4c");
    }
}