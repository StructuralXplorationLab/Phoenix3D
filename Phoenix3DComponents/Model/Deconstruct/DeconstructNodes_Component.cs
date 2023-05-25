using Grasshopper.Kernel;
using Grasshopper;
using Grasshopper.Kernel.Data;

using Rhino.Geometry;

using System;
using System.Collections.Generic;


using Cardinal.Model;

namespace CardinalComponents.Model.Deconstruct
{
    public class DeconstructNodes_Component : GH_Component
    {

        public DeconstructNodes_Component() : base("Deconstruct Nodes", "DecNod", "deconstructs Nodes into its parts Points, DOFs, displacements, and Loads ", "Cardinal", "Elements")
        {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Nodes", "N", "takes Nodes of a structure", GH_ParamAccess.list);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddPointParameter("Points", "P", "Points (X,Y,Z) of Nodes", GH_ParamAccess.list);
            pManager.AddBooleanParameter("DOFs", "DOF", "Degrees of freedom for each Node", GH_ParamAccess.tree);
            pManager.AddVectorParameter("Displacements", "D", "displacements for all 6 degrees of freedom for each Node", GH_ParamAccess.tree);
            pManager.AddVectorParameter("Loads", "L", "Loads for all 6 DOFs for each Node", GH_ParamAccess.tree);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // --- INPUT ---
            var nodes = new List<Node>();

            DA.GetDataList(0, nodes);

            // --- SOLVE ---
            var points = new List<Point3d>();
            var dofs = new DataTree<bool>();
            var displacements = new DataTree<double[]>();
            var loads = new DataTree<Vector3d>();

            for (int i = 0; i < nodes.Count; ++i)
            {
                points.Add(new Point3d(nodes[i].X, nodes[i].Y, nodes[i].Z));

                foreach (var dof in nodes[i].Fix)
                {
                    dofs.Add(dof, new GH_Path(i));
                }

                foreach(var displ in nodes[i].Displacements)
                {
                    displacements.Add(displ.Value, new GH_Path(i));
                }

                foreach(var load in nodes[i].PointLoads)
                {
                    loads.Add(new Vector3d(load.Value.Fx, load.Value.Fy, load.Value.Fz), new GH_Path(i));
                }

            }

            // --- OUTPUT ---
            DA.SetDataList(0, points);
            DA.SetDataTree(1, dofs);
            DA.SetDataTree(2, displacements);
            DA.SetDataTree(3, loads);
        }

        protected override System.Drawing.Bitmap Icon => null;

        public override Guid ComponentGuid => new Guid("5153e2b6-2bb3-4530-9158-a90cec50326b");

    }
}