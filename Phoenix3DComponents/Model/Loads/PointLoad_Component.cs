using System;
using System.Collections.Generic;

using Phoenix3D.Model;

using Grasshopper.Kernel;

using Rhino.Geometry;


namespace Phoenix3D_Components.Model.Loads
{
    public class PointLoad_Component : GH_Component, IGH_PreviewObject
    {
        private List<Point3d> Locations = new List<Point3d>();
        private Vector3d Force = Vector3d.Zero;
        private double scaleFactor = 1.0;

        public PointLoad_Component() : base("Pointload", "PL", "Creates a Pointload", "Phoenix3D", "  Loads")
        {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddPointParameter("Point", "PT", "Location of load", GH_ParamAccess.list);
            pManager.AddVectorParameter("Forces [MN]", "FO [MN]", "Forces XYZ as Vector", GH_ParamAccess.item, Vector3d.Zero);
            pManager.AddNumberParameter("ScaleLoad", "SL", "scales displayed load", GH_ParamAccess.item, 1.0);

            pManager[1].Optional = true;
            pManager[2].Optional = true;
        }


        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("PointLoad", "PL", "", GH_ParamAccess.list);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // -- Input --
            var points = new List<Point3d>();
            Vector3d F = new Vector3d();

            DA.GetDataList(0, points);
            DA.GetData(1, ref F);
            DA.GetData(2, ref scaleFactor);

            // -- Solve --
            Locations.Clear();
            List<PointLoad> PL = new List<PointLoad>();

            foreach(var point in points)
            {
                var Node = new Node(point.X, point.Y, point.Z);
                var pointload = new PointLoad(Node, F.X, F.Y, F.Z, 0, 0, 0);

                Locations.Add(point);
                PL.Add(pointload);
            }

            Force = F;

            // -- Output --
            DA.SetDataList(0, PL);
        }
        

        public override void DrawViewportWires(IGH_PreviewArgs args)
        {
            var color = System.Drawing.Color.LimeGreen;

            for (int i = 0; i < Locations.Count; i++)
            {
                var location = Locations[i];
                var force = Force;

                var planeX = new Plane(location, new Vector3d(-force.X + 1, 0, 0));
                var coneX = new Cone(planeX, 1 * scaleFactor, 0.25 * scaleFactor);
                var lineX = new Line(location, new Vector3d((-force.X + 1) * scaleFactor, 0, 0));

                var planeY = new Plane(location, new Vector3d(0, -force.Y + 1, 0));
                var coneY = new Cone(planeY, 1 * scaleFactor, 0.25 * scaleFactor);
                var lineY = new Line(location, new Vector3d(0, (-force.Y + 1) * scaleFactor, 0));

                var planeZ = new Plane(location, new Vector3d(0, 0, -force.Z + 1));
                var coneZ = new Cone(planeZ, 1 * scaleFactor, 0.25 * scaleFactor);
                var lineZ = new Line(location, new Vector3d(0, 0, (-force.Z +1) * scaleFactor));

                if (force.X != 0)
                {
                    args.Display.DrawCone(coneX, color);
                    args.Display.DrawLine(lineX, color);
                }
                if (force.Y != 0)
                {
                    args.Display.DrawCone(coneY, color);
                    args.Display.DrawLine(lineY, color);
                }
                if (force.Z != 0)
                {
                    args.Display.DrawCone(coneZ, color);
                    args.Display.DrawLine(lineZ, color);
                }
            }
        }

        public override void DrawViewportMeshes(IGH_PreviewArgs args)
        {
            var color = System.Drawing.Color.ForestGreen;

            for (int i = 0; i < Locations.Count; i++)
            {
                var location = Locations[i];
                var force = Force;

                var planeX = new Plane(location, new Vector3d(-force.X + 1, 0, 0));
                var coneX = new Cone(planeX, 1 * scaleFactor, 0.25 * scaleFactor);
                var lineX = new Line(location, new Vector3d((-force.X + 1) * scaleFactor, 0, 0));

                var planeY = new Plane(location, new Vector3d(0, -force.Y + 1, 0));
                var coneY = new Cone(planeY, 1 * scaleFactor, 0.25 * scaleFactor);
                var lineY = new Line(location, new Vector3d(0, (-force.Y + 1) * scaleFactor, 0));

                var planeZ = new Plane(location, new Vector3d(0, 0, -force.Z + 1));
                var coneZ = new Cone(planeZ, 1 * scaleFactor, 0.25 * scaleFactor);
                var lineZ = new Line(location, new Vector3d(0, 0, (-force.Z + 1) * scaleFactor));

                if (force.X != 0)
                {
                    args.Display.DrawCone(coneX, color);
                    args.Display.DrawLine(lineX, color);
                }
                if (force.Y != 0)
                {
                    args.Display.DrawCone(coneY, color);
                    args.Display.DrawLine(lineY, color);
                }
                if (force.Z != 0)
                {
                    args.Display.DrawCone(coneZ, color);
                    args.Display.DrawLine(lineZ, color);
                }
            }
        }

        protected override System.Drawing.Bitmap Icon => Properties.Resources.pointload;

        public override GH_Exposure Exposure => GH_Exposure.primary;

        public override Guid ComponentGuid => new Guid("ea675e48-ec6a-4919-a228-7430f2f552b2");

    }
}