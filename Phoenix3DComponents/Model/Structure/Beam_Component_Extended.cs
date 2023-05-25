using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

using Cardinal.Model.Materials;
using Cardinal.Model.CrossSections;
using Cardinal.Model;
using Cardinal.LinearAlgebra;
using CardinalComponents.Properties;

namespace CardinalComponents.Model
{
    public class Beam_Component_Extended : GH_Component
    {
        public Beam_Component_Extended() : base("BeamExt (Frame)", "BeamExt", "Create a beam element", "Cardinal", "Elements")
        {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddLineParameter("Line", "Line", "Centerline of the bar", GH_ParamAccess.item);
            pManager.AddGenericParameter("Material", "Mat", "Material of the bar", GH_ParamAccess.item);
            pManager.AddGenericParameter("CrossSection", "CroSec", "Bar cross section", GH_ParamAccess.item);
            pManager.AddVectorParameter("Normal", "Normal", "Normal direction of the cross section. Default (0,0,1)", GH_ParamAccess.item, Vector3d.ZAxis);
            pManager.AddIntegerParameter("BucklingType", "BucklingType", "BucklingType", GH_ParamAccess.item, 0);
            pManager.AddNumberParameter("BucklingLength", "BucklingLength", "BucklingLength", GH_ParamAccess.item);
            pManager.AddIntegerParameter("MinC", "MinC", "Min number of compound sections", GH_ParamAccess.item, 1);
            pManager.AddIntegerParameter("MaxC", "MaxC", "Max number of compound sections", GH_ParamAccess.item, 1);
            pManager.AddIntegerParameter("Group", "Group", "Group", GH_ParamAccess.item, -1);
            pManager.AddTextParameter("AllowedCrossSections", "CroSecs", "List of allowed cross-sections to be assigned", GH_ParamAccess.list);

            pManager[1].Optional = true;
            pManager[2].Optional = true;
            pManager[3].Optional = true;
            pManager[4].Optional = true;
            pManager[5].Optional = true;
            pManager[6].Optional = true;
            pManager[7].Optional = true;
            pManager[8].Optional = true;
            pManager[9].Optional = true;
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Bar", "Member", "Bar member", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Line L = new Line();
            IMaterial Mat = new Steel();
            ICrossSection CS = new CircularSection();
            Vector3d N = Vector3d.ZAxis;
            double BucklingLength = 0;
            int BucklingType = 0;
            int minC = 1;
            int maxC = 1;
            int Group = -1;
            List<string> TypeList = new List<string>();

            DA.GetData(0, ref L);
            DA.GetData(1, ref Mat);
            DA.GetData(2, ref CS);
            DA.GetData(3, ref N);
            DA.GetData(4, ref BucklingType);
            if (!DA.GetData(5, ref BucklingLength))
                BucklingLength = L.Length;
            DA.GetData(6, ref minC);
            DA.GetData(7, ref maxC);
            DA.GetData(8, ref Group);
            DA.GetDataList(9, TypeList);

            Beam B = new Beam(new Node(L.FromX, L.FromY, L.FromZ), new Node(L.ToX, L.ToY, L.ToZ));

            B.SetMaterial(Mat);
            B.SetCrossSection(CS);
            B.SetNormal(new Vector(new double[] { N.X, N.Y, N.Z }));
            B.SetBuckling((BucklingType)BucklingType, BucklingLength);
            B.SetMinMaxCompoundSection(minC, maxC);
            B.SetGroupNumber(Group);
            B.SetAllowedCrossSections(TypeList);

            if(B.NormalOverwritten)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "The provided member normal " + N.ToString() + " coincides with the member direction. Normal has been overwritten to " + B.Normal.ToString());
            }
            DA.SetData(0, B);
        }

        protected override System.Drawing.Bitmap Icon => null;

        public override Guid ComponentGuid => new Guid("ce4c1b75-be42-49ba-b34e-96cf4e41d303");

    }
}