using Grasshopper.Kernel;

using System;

using Phoenix3D.Optimization;


namespace Phoenix3D_Components.Optimization.Options
{
    public class CCTOOptions_Component : GH_Component
    {
         
        public CCTOOptions_Component() : base("CTTOOptions", "CTTOOptions", "Options for Continuous Truss Topology Optimization", "Phoenix3D", " Optimization")
        {
        }

         
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddIntegerParameter("Optimizer", "OP", "Optimizer (0 = Gurobi (Default), 1 = ...", GH_ParamAccess.item, 0);
            pManager.AddIntegerParameter("Max Time", "MT", "Time limit for the optimization in seconds. default = No Limit", GH_ParamAccess.item, int.MaxValue);
            pManager.AddBooleanParameter("Logging", "LG", "Log Optimizer in Console Windows. default = true", GH_ParamAccess.item, true);
            pManager.AddTextParameter("LogForm Name", "LN", "Name of the Windows Form used for logging", GH_ParamAccess.item, "LP Optimization Log");
            pManager.AddBooleanParameter("SelfWeight", "SW", "Consider self-weight lumped at member end nodes. default = false", GH_ParamAccess.item, false);
            pManager[0].Optional = false;
            pManager[1].Optional = false;
            pManager[2].Optional = false;
            pManager[3].Optional = false;
            pManager[4].Optional = false;
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("CTT Optimization Options", "OP", "CTT Optimization Options", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            int LPOptimizer = 0;
            int MaxTime = int.MaxValue;
            bool LogToConsole = true;
            string LogFormName = "LP Optimization Log";
            bool SelfWeight = false;

            DA.GetData(0, ref LPOptimizer);
            DA.GetData(1, ref MaxTime);
            DA.GetData(2, ref LogToConsole);
            DA.GetData(3, ref LogFormName);
            DA.GetData(4, ref SelfWeight);

            OptimOptions Opt = new OptimOptions();
            Opt.LPOptimizer = (LPOptimizer) LPOptimizer;
            Opt.MaxTime = MaxTime;
            Opt.LogToConsole = LogToConsole;
            Opt.LogFormName = LogFormName;
            Opt.Selfweight = SelfWeight;

            DA.SetData(0, Opt);
        }

        protected override System.Drawing.Bitmap Icon => Properties.Resources.CTTO_option;

        public override Guid ComponentGuid => new Guid("3e6f5818-e5fb-4b22-95fb-2ddbd9fea34f");

        public override GH_Exposure Exposure
        {
            get { return GH_Exposure.tertiary; }
        }

    }
}