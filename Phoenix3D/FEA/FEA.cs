using Phoenix3D.Model;


namespace Phoenix3D.FEA
{
    
    public class FiniteElementAnalysis
    {
        public FiniteElementAnalysis()
            : this(new FEAOptions()) { }
        public FiniteElementAnalysis(FEAOptions Options)
        {
            this.Options = Options;
        }



        public FEAMethod Solution { get; private set; }
        public FEAOptions Options { get; private set; } = new FEAOptions();


        public void Solve(Structure structure, LoadCase LC)
        {
            switch (Options.Method)
            {
                case FEAnalysisMethod.LinearElastic:
                    {
                        Solution = new LinearElastic();
                        Solution.Solve(structure, LC, out _, Options); break;
                    }
                default:
                    {
                        Solution = new LinearElastic();
                        Solution.Solve(structure, LC, out _, Options); break;
                    }
            }
        }

    }
}
