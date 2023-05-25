using Phoenix3D.LinearAlgebra;


namespace Phoenix3D.FEA
{
    
    public class FEAOptions
    {

        public FEAOptions() : this(FEAnalysisMethod.LinearElastic) { }
        public FEAOptions(FEAnalysisMethod Method)
            : this(Method, MatrixSolver.GaussJordan) { }
        public FEAOptions(MatrixSolver Solver)
            : this(FEAnalysisMethod.LinearElastic, Solver) { }
        public FEAOptions(FEAnalysisMethod Method, MatrixSolver Solver)
            : this(Method, Solver, default) { }
        public FEAOptions(FEAnalysisMethod Method, MatrixSolver Solver, int CGIterations = 60)
            : this(Method, Solver, CGIterations, default) { }
        public FEAOptions(FEAnalysisMethod Method, MatrixSolver Solver, int CGIterations = 60, double CGTolerance = 0.001)
            : this(Method, Solver, CGIterations, CGTolerance, default) { }
        public FEAOptions(FEAnalysisMethod Method, MatrixSolver Solver, int CGIterations = 60, double CGTolerance = 0.001, int NewtonRaphsonIterations = 20)
        {
            this.Method = Method;
            this.Solver = Solver;
            this.CGIterations = CGIterations;
            this.CGTolerance = CGTolerance;
            this.NewtonRaphsonIterations = NewtonRaphsonIterations;
        }


        public FEAnalysisMethod Method { get; private set; } = FEAnalysisMethod.LinearElastic;
        public MatrixSolver Solver { get; private set; } = MatrixSolver.GaussJordan;
        public int CGIterations { get; private set; } = 60;
        public double CGTolerance { get; private set; } = 0.001;
        public int NewtonRaphsonIterations { get; private set; } = 20;

        
    }

    public enum FEAnalysisMethod { LinearElastic = 0, Theory2ndOrder = 1 }


}