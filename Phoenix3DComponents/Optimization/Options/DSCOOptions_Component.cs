using Grasshopper.Kernel;

using System;
using System.Collections.Generic;

using Phoenix3D.Optimization;


namespace Phoenix3D_Components.Optimization.Options
{
    public class DSCOOptions_Component : GH_Component
    {

        public DSCOOptions_Component() : base("DSCOOptions", "DSCOOptions", "Options for Discrete Stock Constrained Optimization", "Phoenix3D", " Optimization")
        {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddIntegerParameter("Optimizer", "OP", "Optimizer (0 = Gurobi (Default), 1 = ...", GH_ParamAccess.item, 0);
            pManager.AddIntegerParameter("Max Time", "MT", "Time limit for the optimization in seconds. default = No Limit", GH_ParamAccess.item, int.MaxValue);
            pManager.AddBooleanParameter("Logging", "LG", "Log Optimizer in Console Windows. default = true", GH_ParamAccess.item, true);
            pManager.AddTextParameter("LogForm Name", "LN", "Name of the Windows Form used for logging", GH_ParamAccess.item, "MILP Optimization Log");
            pManager.AddBooleanParameter("Compatibility", "CP", "Consider geometric compatibility", GH_ParamAccess.item, true);
            pManager.AddBooleanParameter("SelfWeight", "SW", "Consider self-weight lumped at member end nodes. default = false", GH_ParamAccess.item, false);
            pManager.AddBooleanParameter("CuttingStock", "CuttingStock", "Consider partition of stock elements into multiple members (cutting stock)", GH_ParamAccess.item, false);
            pManager.AddTextParameter("Parameters", "Param", "List of Optimizer parameters in the form or 'Parameter name, value'", GH_ParamAccess.list);
            pManager[0].Optional = true;
            pManager[1].Optional = true;
            pManager[2].Optional = true;
            pManager[3].Optional = true;
            pManager[4].Optional = true;
            pManager[5].Optional = true;
            pManager[6].Optional = true;
            pManager[7].Optional = true;
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("CTT Optimization Options", "OP", "CTT Optimization Options", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            char[] delimiterChars = { ' ', ',', '.', ':', ';' };

            int MILPOptimizer = 0;
            int MaxTime = int.MaxValue;
            bool LogToConsole = true;
            string LogFormName = "MILP Optimization Log";
            int Formulation = 0;
            bool Compatibility = true;
            bool SelfWeight = false;
            bool CuttingStock = false;
            List<string> OptParam = new List<string>();

            DA.GetData(0, ref MILPOptimizer);
            DA.GetData(1, ref MaxTime);
            DA.GetData(2, ref LogToConsole);
            DA.GetData(3, ref LogFormName);
            DA.GetData(4, ref Compatibility);
            DA.GetData(5, ref SelfWeight);
            DA.GetData(6, ref CuttingStock);
            DA.GetDataList(7, OptParam);

            OptimOptions Opt = new OptimOptions();
            Opt.MILPOptimizer = (MILPOptimizer) MILPOptimizer;
            Opt.MaxTime = MaxTime;
            Opt.LogToConsole = LogToConsole;
            Opt.LogFormName = LogFormName;
            Opt.MILPFormulation = (MILPFormulation)Formulation;
            Opt.Selfweight = SelfWeight;
            Opt.Compatibility = Compatibility;
            Opt.CuttingStock = CuttingStock;

            foreach (string p in OptParam)
            {
                string[] split = p.Split(delimiterChars, StringSplitOptions.RemoveEmptyEntries);
                string name = split[0];
                string value = split[1];
                Opt.GurobiParameters.Add(new Tuple<string, string>(name, value));
            }

            DA.SetData(0, Opt);
        }

        protected override System.Drawing.Bitmap Icon => Properties.Resources.DSCO_option;

        public override Guid ComponentGuid => new Guid("DE07C224-ED04-496A-9E18-B98B1EB07FC1");

        public override GH_Exposure Exposure
        {
            get { return GH_Exposure.tertiary; }
        }

    }
}