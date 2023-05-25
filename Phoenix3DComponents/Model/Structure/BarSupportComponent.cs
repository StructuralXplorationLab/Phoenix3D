using System;
using System.Collections.Generic;
using Cardinal.Model;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Grasshopper.Kernel.Special;
using Rhino.Geometry;

namespace CardinalComponents.Model.Structure
{
    public class BarSupportComponent : GH_Component
    {
        private Tuple<bool, bool, bool> DOFs = new Tuple<bool, bool, bool>(false, false, false);
        private List<Point3d> Locations = new List<Point3d>();
        public BarSupportComponent() : base("Bar Support", "BSup", "Description", "Cardinal", "Elements")
        {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddPointParameter("Location", "P", "Location of support", GH_ParamAccess.list);
            pManager.AddBooleanParameter("Fix X", "X", "Fix translation in X-Direction", GH_ParamAccess.item, true);
            pManager.AddBooleanParameter("Fix Y", "Y", "Fix translation in Y-Direction", GH_ParamAccess.item, true);
            pManager.AddBooleanParameter("Fix Z", "Z", "Fix translation in Z-Direction", GH_ParamAccess.item, true);

            pManager[1].Optional = true;
            pManager[2].Optional = true;
            pManager[3].Optional = true;
        }
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Support", "Support", "A support fixing selected translations", GH_ParamAccess.list);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // -- Input --
            var points = new List<Point3d>();
            var Supports = new List<Support>();
            GH_Boolean TX = new GH_Boolean();
            GH_Boolean TY = new GH_Boolean();
            GH_Boolean TZ = new GH_Boolean();

            DA.GetDataList(0, points);
            DA.GetData(1, ref TX);
            DA.GetData(2, ref TY);
            DA.GetData(3, ref TZ);


            Locations.Clear();
            DOFs = new Tuple<bool, bool, bool>(TX.Value, TY.Value, TZ.Value);

            // -- Solve --
            foreach (var point in points)
            {
                var Node = new Node(point.X, point.Y, point.Z);
                var Sup = new Support(Node, TX.Value, TY.Value, TZ.Value, true, true, true);

                Locations.Add(point);
                Supports.Add(Sup);
            }
            // -- Output --
            DA.SetDataList(0, Supports);
        }
        protected override System.Drawing.Bitmap Icon => null;

        public override Guid ComponentGuid => new Guid("75ede549-ef37-4b59-96e3-a30e7a9cba8a");

        public override void DrawViewportWires(IGH_PreviewArgs args)
        {
            var color = System.Drawing.Color.LimeGreen;

            for (int i = 0; i < Locations.Count; i++)
            {
                var FixTx = DOFs.Item1;
                var FixTy = DOFs.Item2;
                var FixTz = DOFs.Item3;

                var location = Locations[i];

                Cone coneX = new Cone();
                Cone coneY = new Cone();
                Cone coneZ = new Cone();

                if (FixTx)
                {
                    var planeX = new Plane(location, new Vector3d(-1, 0, 0));
                    coneX = new Cone(planeX, 1, 0.5);
                    args.Display.DrawCone(coneX, color);
                }
                if (FixTy)
                {
                    var planeY = new Plane(location, new Vector3d(0, -1, 0));
                    coneY = new Cone(planeY, 1, 0.5);
                    args.Display.DrawCone(coneY, color);
                }
                if (FixTz)
                {
                    var planeZ = new Plane(location, new Vector3d(0, 0, -1));
                    coneZ = new Cone(planeZ, 1, 0.5);
                    args.Display.DrawCone(coneZ, color);
                }
            }
        }

        public override void DrawViewportMeshes(IGH_PreviewArgs args)
        {
            var color = System.Drawing.Color.ForestGreen;

            for (int i = 0; i < Locations.Count; i++)
            {
                var FixTx = DOFs.Item1;
                var FixTy = DOFs.Item2;
                var FixTz = DOFs.Item3;

                var location = Locations[i];

                Cone coneX = new Cone();
                Cone coneY = new Cone();
                Cone coneZ = new Cone();

                if (FixTx)
                {
                    var planeX = new Plane(location, new Vector3d(-1, 0, 0));
                    coneX = new Cone(planeX, 1, 0.5);
                    args.Display.DrawCone(coneX, color);
                }
                if (FixTy)
                {
                    var planeY = new Plane(location, new Vector3d(0, -1, 0));
                    coneY = new Cone(planeY, 1, 0.5);
                    args.Display.DrawCone(coneY, color);
                }
                if (FixTz)
                {
                    var planeZ = new Plane(location, new Vector3d(0, 0, -1));
                    coneZ = new Cone(planeZ, 1, 0.5);
                    args.Display.DrawCone(coneZ, color);
                }
            }
        }

    }
}