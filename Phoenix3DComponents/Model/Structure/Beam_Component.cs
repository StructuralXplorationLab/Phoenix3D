using System;

using Grasshopper.Kernel;
using Rhino.Geometry;

using Cardinal.Model.Materials;
using Cardinal.Model.CrossSections;
using Cardinal.Model;
using Cardinal.LinearAlgebra;


namespace CardinalComponents.Model
{
    public class Beam_Component : GH_Component
    {

        public Beam_Component() : base("Beam", "Beam", "Create a beam element with 6 DOFs", "Cardinal", "Elements")
        {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddLineParameter("Line", "Line", "Centerline of the beam", GH_ParamAccess.item);
            pManager.AddGenericParameter("Material", "Mat", "Material of the beam", GH_ParamAccess.item);
            pManager.AddGenericParameter("CrossSection", "CS", "Beam cross section", GH_ParamAccess.item);
            pManager.AddVectorParameter("Normal", "Normal", "Normal direction of the cross section. Default (0,0,1)", GH_ParamAccess.item, Vector3d.ZAxis);

            pManager[1].Optional = true;
            pManager[2].Optional = true;
            pManager[3].Optional = true;
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Beam", "Member", "Beam member", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Line L = new Line();
            IMaterial Mat = new Steel();
            ICrossSection CS = new CircularSection();
            Vector3d N = Vector3d.ZAxis;

            DA.GetData(0, ref L);
            DA.GetData(1, ref Mat);
            DA.GetData(2, ref CS);
            DA.GetData(3, ref N);

            Beam Be = new Beam(new Node(L.FromX, L.FromY, L.FromZ), new Node(L.ToX, L.ToY, L.ToZ));

            Be.SetMaterial(Mat);
            Be.SetCrossSection(CS);
            Be.SetNormal(new Vector(new double[] { N.X, N.Y, N.Z }));

            if (Be.NormalOverwritten)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "The provided member normal " + N.ToString() + " coincides with the member direction. Normal has been overwritten to" + Be.Normal.ToString());
            }

            DA.SetData(0, Be);
        }

        protected override System.Drawing.Bitmap Icon => null;

        public override Guid ComponentGuid => new Guid("e723b0ae-9768-474c-aaf9-5854a6664cc9");

    }
}