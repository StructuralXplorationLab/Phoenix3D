using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

using Phoenix3D.Model.Materials;
using Phoenix3D.Model.CrossSections;
using Phoenix3D.Model;
using Phoenix3D.LinearAlgebra;


namespace Phoenix3D_Components.Model
{
    public class Bar_Component_Extended : GH_Component
    {

        public Bar_Component_Extended()
          : base("Bar Extended", "BarExt (Truss)", "Creates a truss bar element that has pinned ends and only carries normal forces", "Phoenix3D", "   Geometry")
        {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddLineParameter("Lines", "LI", "Centerline of the bar", GH_ParamAccess.item);
            pManager.AddGenericParameter("Material", "MA", "Material of the bar", GH_ParamAccess.item);
            pManager.AddGenericParameter("CrossSection", "CS", "Bar cross section", GH_ParamAccess.item);
            pManager.AddVectorParameter("Normal", "NR", "Normal direction of the cross section. Default (0,0,1)", GH_ParamAccess.item, Vector3d.ZAxis);
            pManager.AddIntegerParameter("BucklingType", "Buckling", "BucklingType: 0 = Off;  1 = Euler; 2 = Eurocode 2", GH_ParamAccess.item, 0);
            pManager.AddNumberParameter("BucklingLength", "BLength", "BucklingLength", GH_ParamAccess.item);
            pManager.AddTextParameter("AllowedCrossSections", "allowedCS", "List of allowed cross-sections to be assigned", GH_ParamAccess.list);

            pManager[1].Optional = true;
            pManager[2].Optional = true;
            pManager[3].Optional = true;
            pManager[4].Optional = true;
            pManager[5].Optional = true;
            pManager[6].Optional = true;
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Bar", "M", "Bar member", GH_ParamAccess.item);
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
            DA.GetDataList(6, TypeList);

            Bar B = new Bar(new Node(L.FromX, L.FromY, L.FromZ), new Node(L.ToX, L.ToY, L.ToZ));

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

        protected override System.Drawing.Bitmap Icon => Properties.Resources.bar_extended;

        public override Guid ComponentGuid => new Guid("ce4c1b75-be42-49ba-b34e-96cf4e41d311");

        public override GH_Exposure Exposure
        {
            get { return GH_Exposure.primary; }
        }

    }
}