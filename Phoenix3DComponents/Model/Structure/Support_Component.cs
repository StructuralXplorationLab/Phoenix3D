using System;
using System.Collections.Generic;

using Phoenix3D.Model;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;

using Rhino.Geometry;


namespace Phoenix3D_Components.Model
{
    public class Support_Component : GH_Component, IGH_PreviewObject
    {
        private Tuple<bool, bool, bool, bool, bool, bool> DOFs = new Tuple<bool, bool, bool, bool, bool, bool>(false, false, false, false, false, false);
        private List<Point3d> Locations = new List<Point3d>();
        private double scaleFactor = 1.0;

        public Support_Component() : base("Support", "Sp", "Creates a support for truss elements", "Phoenix3D", "   Geometry")
        {
        }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddPointParameter("Point", "PT", "Location of support", GH_ParamAccess.list);
            pManager.AddBooleanParameter("Fix TX", "TX", "Fix translation in X-Direction", GH_ParamAccess.item, true);
            pManager.AddBooleanParameter("Fix TY", "TY", "Fix translation in Y-Direction", GH_ParamAccess.item, true);
            pManager.AddBooleanParameter("Fix TZ", "TZ", "Fix translation in Z-Direction", GH_ParamAccess.item, true);
            pManager.AddNumberParameter("ScaleSupport", "SL", "scales displayed support", GH_ParamAccess.item, 1.0);

            pManager[0].Optional = true;
            pManager[1].Optional = true;
            pManager[2].Optional = true;
            pManager[3].Optional = true;
            pManager[4].Optional = true;

        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Support", "SP", "A support fixing selected translations and rotations", GH_ParamAccess.list);
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
            DA.GetData(4, ref scaleFactor);

            Locations.Clear();
            DOFs = new Tuple<bool, bool, bool, bool, bool, bool>(TX.Value, TY.Value, TZ.Value, true, true, true);

            // -- Solve --
            foreach(var point in points)
            {
                var Node = new Node(point.X, point.Y, point.Z);
                var Sup = new Support(Node, TX.Value, TY.Value, TZ.Value, true, true, true);

                Locations.Add(point);
                Supports.Add(Sup);
            }
            // -- Output --
            DA.SetDataList(0, Supports);
        }

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
                    coneX = new Cone(planeX, 1 * scaleFactor, 0.5 * scaleFactor);
                    args.Display.DrawCone(coneX, color);

                }
                if (FixTy)
                {
                    var planeY = new Plane(location, new Vector3d(0, -1, 0));
                    coneY = new Cone(planeY, 1 * scaleFactor, 0.5 * scaleFactor);
                    args.Display.DrawCone(coneY, color);
                }
                if (FixTz)
                {
                    var planeZ = new Plane(location, new Vector3d(0, 0, -1));
                    coneZ = new Cone(planeZ, 1 * scaleFactor, 0.5 * scaleFactor);
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
                    coneX = new Cone(planeX, 1 * scaleFactor, 0.5 * scaleFactor);
                    args.Display.DrawCone(coneX, color);

                }
                if (FixTy)
                {
                    var planeY = new Plane(location, new Vector3d(0, -1, 0));
                    coneY = new Cone(planeY, 1 * scaleFactor, 0.5 * scaleFactor);
                    args.Display.DrawCone(coneY, color);
                }
                if (FixTz)
                {
                    var planeZ = new Plane(location, new Vector3d(0, 0, -1));
                    coneZ = new Cone(planeZ, 1 * scaleFactor, 0.5 * scaleFactor);
                    args.Display.DrawCone(coneZ, color);
                }
            }
        }

        protected override System.Drawing.Bitmap Icon => Properties.Resources.support;

        public override GH_Exposure Exposure => GH_Exposure.primary;

        public override Guid ComponentGuid => new Guid("50aaa93a-9ee8-43f3-9539-2c1749d0380f");
  
    }
}