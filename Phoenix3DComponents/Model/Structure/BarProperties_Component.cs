using System;
using System.Collections.Generic;
using Phoenix3D.Model;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace Phoenix3D_Components.Model
{
    public class BarProperties_Component : GH_Component
    {

        public BarProperties_Component() : base("Assign Bar Properties", "BarProperties", "Assign additional properties of the bar useful for optimization", "Phoenix3D", " Optimization")
        {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Bar Member", "MB", "Bar Member", GH_ParamAccess.item);
            pManager.AddIntegerParameter("Buckling Type", "Buckling", "Buckling type: 0 = Off, 1 = Euler, 2 = Eurocode3", GH_ParamAccess.item, 0);
            pManager.AddIntegerParameter("Buckling Length", "BLength", "Buckling length (absolute). If not provided, bar length is taken.", GH_ParamAccess.item);
            
            pManager.AddIntegerParameter("Minimum compound sections", "MinCompound", "Minimum number of identical sections that are assigned to the bar member", GH_ParamAccess.item, 1);
            pManager.AddIntegerParameter("Maximum compound sections", "MaxCompound", "Maximum number of identical sections that are assigned to the bar member", GH_ParamAccess.item, 1);
            
            pManager.AddNumberParameter("Buffer length 0", "Buffer0", "Buffer length b0 in stock-constrained design", GH_ParamAccess.item, 0);
            pManager.AddNumberParameter("Buffer length 1", "Buffer1", "Buffer length b1 in stock-constrained design", GH_ParamAccess.item, 0);
            
            pManager.AddNumberParameter("Lower Bound CS Area", "LBArea", "Lower bound for cross section area in continuous topology optimization", GH_ParamAccess.item, 0);
            pManager.AddNumberParameter("Upper Bound CS Area", "UBArea", "Lower bound for cross section area in continuous topology optimization", GH_ParamAccess.item, double.PositiveInfinity);
            pManager.AddBooleanParameter("Topology fixed", "TopologyFix", "Define whether bar member can be removed in topology optimization", GH_ParamAccess.item, false);

          
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
            pManager.AddGenericParameter("Bar with properties", "MB", "Bar with the assigned properties", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Bar Bar = new Bar(new Node(0, 0, 0), new Node(1, 1, 1));
            int BucklingType = (int)Phoenix3D.Model.BucklingType.Off;
            double BucklingLength = 0;
            int MinCompound = 1;
            int MaxCompound = 0;
            double Buffer0 = 0;
            double Buffer1 = 0;
            double LBArea = 0;
            double UBArea = double.PositiveInfinity;
            bool TopologyFixed = false;

            DA.GetData(0, ref Bar);

            DA.GetData(1, ref BucklingType);
            if(!DA.GetData(2, ref BucklingLength))
            {
                BucklingLength = Bar.Length;
            }

            DA.GetData(3, ref MinCompound);
            DA.GetData(4, ref MaxCompound);

            DA.GetData(5, ref Buffer0);
            DA.GetData(6, ref Buffer1);

            DA.GetData(7, ref LBArea);
            DA.GetData(8, ref UBArea);
            DA.GetData(9, ref TopologyFixed);


            Bar.SetBuckling((BucklingType) BucklingType, BucklingLength);
            Bar.SetMinMaxCompoundSection(MinCompound, MaxCompound);
            Bar.SetBufferLengths(Buffer0, Buffer1);
            Bar.SetAreaBounds(LBArea, UBArea);
            Bar.FixTopology(TopologyFixed);

            DA.SetData(0, Bar);

        }

        protected override System.Drawing.Bitmap Icon => Properties.Resources.assign_bar_properties;
        public override Guid ComponentGuid => new Guid("63a025df-57d1-40fa-b2c4-c3a69cd91585");

        public override GH_Exposure Exposure
        {
            get { return GH_Exposure.tertiary; }
        }

    }
}