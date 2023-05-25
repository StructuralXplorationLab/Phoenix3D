using Cardinal.Model;
using Grasshopper.Kernel;
using Rhino.Geometry;
using System;
using System.Collections.Generic;

namespace CardinalComponents.Optimization.Heuristics
{
    public class SetBeamForces : GH_Component
    {

        public SetBeamForces() : base("SetBeamForces", "SetBeamForces", "Set beam forces obtained from external analysis", "Cardinal", "Optimization")
        {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Beam", "Beam", "Beam", GH_ParamAccess.item);
            pManager.AddTextParameter("LoadCaseName", "LoadCase", "Name of the LoadCase", GH_ParamAccess.item);
            pManager.AddNumberParameter("Nx", "Nx", "Normal forces Nx along beam", GH_ParamAccess.list);
            pManager.AddNumberParameter("Vy", "Vy", "Shear force Vy along beam", GH_ParamAccess.list);
            pManager.AddNumberParameter("Vz", "Vz", "Shear force Vz along beam", GH_ParamAccess.list);
            pManager.AddNumberParameter("My", "My", "Bending moment My along beam", GH_ParamAccess.list);
            pManager.AddNumberParameter("Mz", "Mz", "Bending moment Mz along beam", GH_ParamAccess.list);
            pManager.AddNumberParameter("Mt", "Mt", "Torsional moment Mt along beam", GH_ParamAccess.list);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Beam", "Beam", "Beam", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Beam temp_B =  new Beam(new Node(0,0,0), new Node(1,1,1));
            string LoadCaseName = "";
            List<double> Nx = new List<double>();
            List<double> Vy = new List<double>();
            List<double> Vz = new List<double>();
            List<double> My = new List<double>();
            List<double> Mz = new List<double>();
            List<double> Mt = new List<double>();

            DA.GetData(0, ref temp_B);
            DA.GetData(1, ref LoadCaseName);
            DA.GetDataList(2, Nx);
            DA.GetDataList(3, Vy);
            DA.GetDataList(4, Vz);
            DA.GetDataList(5, My);
            DA.GetDataList(6, Mz);
            DA.GetDataList(7, Mt);

            var B = temp_B.Clone();

            temp_B.AddInternalForces(new LoadCase(LoadCaseName),Nx,Vy,Vz,My,Mz,Mt);

            DA.SetData(0, temp_B);
        }
        protected override System.Drawing.Bitmap Icon => null;

        public override Guid ComponentGuid => new Guid("268a8471-1b12-4374-964b-597a3d29ae29");

    }
}