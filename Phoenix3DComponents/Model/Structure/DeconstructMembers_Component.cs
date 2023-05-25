using Grasshopper.Kernel;

using Rhino.Geometry;

using System;
using System.Collections.Generic;

using Phoenix3D.Model;
using Phoenix3D.Model.CrossSections;
using Phoenix3D.Model.Materials;

namespace Phoenix3D_Components.Model.Deconstruct
{
    public class DeconstructMembers_Component : GH_Component
    {

        public DeconstructMembers_Component() : base("Deconstruct Members", "DecMem", "deconstructs a list of Members into its parts Lines, CrossSections, Materials, and Normal forces", "Phoenix3D", "   Geometry")
        {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Members", "MB", "takes Members as a list", GH_ParamAccess.list);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddLineParameter("Lines", "LI", "Lines of Members", GH_ParamAccess.list);
            pManager.AddGenericParameter("CrossSections", "CS", "CrossSections of Members", GH_ParamAccess.list);
            pManager.AddGenericParameter("Material", "MA", "Material of Memebers", GH_ParamAccess.list);
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
            var normalForces = new List<Dictionary<LoadCase, List<double>>>();

            foreach(IMember1D member in members)
            {
                lines.Add(new Line(new Point3d(member.From.X, member.From.Y, member.From.Z), new Point3d(member.To.X, member.To.Y, member.To.Z)));
                crossSections.Add(member.CrossSection);
                materials.Add(member.Material);
            }

            // --- OUTPUT ---
            DA.SetDataList(0, lines);
            DA.SetDataList(1, crossSections);
            DA.SetDataList(2, materials);
        }

        protected override System.Drawing.Bitmap Icon => Properties.Resources.deconstruct_member;

        public override Guid ComponentGuid => new Guid("dbfaa285-f62d-4b10-9200-19ea3140bd4c");

        public override GH_Exposure Exposure
        {
            get { return GH_Exposure.secondary; }
        }
    }
}