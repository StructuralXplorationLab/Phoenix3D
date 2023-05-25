using System;
using System.Collections.Generic;

using Cardinal.Model;
using Cardinal.FEA;

using Grasshopper.Kernel;

using Rhino.Geometry;

namespace CardinalComponents.Display
{
    public class DisplacementsComponent : GH_Component
    {
        public DisplacementsComponent() : base("Displacements", "Displ", "Description", "Cardinal", "Display")
        {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Structure", "S", "takes a Structure", GH_ParamAccess.item);
            pManager.AddNumberParameter("Scale", "Sc", "scales displayed displacements", GH_ParamAccess.item, 10000);

            pManager[1].Optional = true;
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddLineParameter("Lines", "L", "", GH_ParamAccess.list);
            pManager.AddNumberParameter("Displacements", "D", "", GH_ParamAccess.list);
            pManager.AddNumberParameter("Forces", "F", "", GH_ParamAccess.list);
            pManager.AddMatrixParameter("Stiffnes matrix", "K", "", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // -- Input --

            var scaleFactor = new double();
            var str_tmp = new Structure();

            DA.GetData(0, ref str_tmp);
            DA.GetData(1, ref scaleFactor);

            // -- Solve --

            var str = str_tmp.Clone();

            var fea = new FiniteElementAnalysis();

            foreach (var lc in str.LoadCases)
            {
                fea.Solve(str, lc.Number);
            }

            var u = fea.Solution.Getu();
            var f = fea.Solution.Getf();
            var K = fea.Solution.GetK();

            var stiffnesMatrix = new Matrix(K.NRows, K.NColumns);

            for (int i = 0; i < K.NRows; ++i)
                for (int j = 0; j < K.NColumns; ++j)
                    stiffnesMatrix[i, j] = K[i, j];

            var displacements = new List<double>();
            var forces = new List<double>();

            for (int i = 0; i < u.Length; i++)
            {
                displacements.Add(u[i]);
                forces.Add(f[i]);
            }

            var Lines = new List<Line>();

            for (int i = 0; i < str.Nodes.Count; ++i)
            {
                str.Nodes[i] = new Node(str.Nodes[i].X + displacements[str.Nodes[i].GetNumber() * 6 + 0] * scaleFactor, str.Nodes[i].Y + displacements[str.Nodes[i].GetNumber() * 6 + 1] * scaleFactor, str.Nodes[i].Z + displacements[str.Nodes[i].GetNumber() * 6 + 2] * scaleFactor);
            }

            foreach (var member in str.Members)
            {
                if (member is IMember1D member1D)
                {
                    var nbAtStart = member1D.From.GetNumber();
                    var nbAtEnd = member1D.To.GetNumber();
                    var A = new Point3d(str.Nodes[nbAtStart].X, str.Nodes[nbAtStart].Y, str.Nodes[nbAtStart].Z);
                    var B = new Point3d(str.Nodes[nbAtEnd].X, str.Nodes[nbAtEnd].Y, str.Nodes[nbAtEnd].Z);
                    var line = new Line(A, B);
                    Lines.Add(line);
                }
            }

            // -- Output --

            DA.SetDataList(0, Lines);
            DA.SetDataList(1, displacements);
            DA.SetDataList(2, forces);
            DA.SetData(3, stiffnesMatrix);

        }

        protected override System.Drawing.Bitmap Icon => null;

        public override GH_Exposure Exposure => GH_Exposure.primary;

        public override Guid ComponentGuid => new Guid("d62838de-f5c9-4013-9394-0a9dfc225000");

    }
}