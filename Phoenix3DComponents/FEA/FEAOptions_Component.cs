using Cardinal.FEA;
using Cardinal.LinearAlgebra;

using Grasshopper.Kernel;

using System;

namespace CardinalComponents.FEA
{
    public class FEAOptions_Component : GH_Component
    {

        public FEAOptions_Component() : base("FEA Options", "FEA Options", "FEA Options", "Cardinal", "FEA")
        {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddIntegerParameter("FEA Method", "FEA Methods", "FEA Method: 0 = linear elastic. No other methods implemented so far!", GH_ParamAccess.item, 0);
            pManager.AddIntegerParameter("Matrix Solver", "Matrix Solver", "Solver for the system of linear equations: 0 = Gauss-Jordan, 1 = ConjugateGradient", GH_ParamAccess.item, 1);
            pManager.AddIntegerParameter("CG Iterations", "CG Iterations", "Maximum number of conjugate gradient solver iterations. Default = 60", GH_ParamAccess.item, 60);
            pManager.AddNumberParameter("CG Tolerance", "CG Tolerance", "Tolerance for conjugate gradient solver (0 < t < 1). Default = 0.001", GH_ParamAccess.item, 0.001);
            
            pManager[0].Optional = true;
            pManager[1].Optional = true;
            pManager[2].Optional = true;
            pManager[3].Optional = true;
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("FEA Options", "FEA Opt", "FEA options", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            int Method = 0;
            int Solver = 1;
            int CGiter = 60;
            double epsilon = 0.001;

            DA.GetData(0, ref Method);
            DA.GetData(1, ref Solver);
            DA.GetData(2, ref CGiter);
            DA.GetData(3, ref epsilon);

            if(Method != 0)
            {
                this.AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "No other method than linear-elastic FEA has been implemented yet. Linear ELastic FEA is used.");  ;
            }
            if (Solver < 0 || Solver > 1)
            {
                this.AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Please specify a correct Matrix solver. Conjugate gradient will now be used.");
            }
            if (CGiter <= 0)
            {
                this.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "The maximum number of conjugate gradient iterations should be >= 1.");
            }
            if (epsilon < 0 || epsilon > 1)
            {
                this.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "The conjugate gradient tolerance should be 0 < t < 1.");
            }

            DA.SetData(0,new FEAOptions((FEAnalysisMethod) Method, (MatrixSolver) Solver, CGiter, epsilon));
        }

        protected override System.Drawing.Bitmap Icon => null;

        public override Guid ComponentGuid => new Guid("6971dc80-b78d-4a11-aed4-4b18ce75eb6b");

    }
}