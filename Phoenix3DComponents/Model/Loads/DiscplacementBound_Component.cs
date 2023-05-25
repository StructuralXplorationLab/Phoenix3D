using System;

using Phoenix3D.Model;

using Grasshopper.Kernel;

using Rhino.Geometry;


namespace Phoenix3D_Components.Model.Loads
{
    public class DiscplacementBound_Component : GH_Component
    {

        public DiscplacementBound_Component() : base("Displacement Bounds", "Displacements", "Set a limit on allowable node displacements for optimization", "Phoenix3D", "  Loads")
        {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddPointParameter("Location", "PT", "Position of the constrained node", GH_ParamAccess.item);
            pManager.AddVectorParameter("LowerBound", "LB", "Lower bound for the node displacements (translations XYZ, usually negative number or 0)", GH_ParamAccess.item, new Vector3d(-1E100,-1E100,-1E100));
            pManager.AddVectorParameter("UpperBound", "UB", "Upper bound for the node displacements (translations XYZ, usually positive number or 0)", GH_ParamAccess.item, new Vector3d(1E100, 1E100, 1E100));
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("DisplacementBound", "DB", "Displacement bound", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Point3d P = new Point3d();
            Vector3d LB = new Vector3d(-1E100, -1E100, -1E100);
            Vector3d UB = new Vector3d(1E100, 1E100, 1E100);

            DA.GetData(0, ref P);
            DA.GetData(1, ref LB);
            DA.GetData(2, ref UB);

            DA.SetData(0, new DisplacementBound(new Node(P.X, P.Y, P.Z), LB.X, LB.Y, LB.Z, UB.X, UB.Y, UB.Z));
        }

        protected override System.Drawing.Bitmap Icon => Properties.Resources.displacement_bounds;

        public override Guid ComponentGuid => new Guid("54e9919d-2147-47f0-b885-7d84c8887a56");

        public override GH_Exposure Exposure
        {
            get { return GH_Exposure.primary; }
        }

    }
}