using Phoenix3D.LCA;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Phoenix3D.Optimization
{
    
    public class OptimOptions
    {
        public int MaxTime = int.MaxValue;

        public bool LogToConsole = true;
        public string LogFormName = "MyLogFormName";

        public bool Selfweight = true;
        public bool Compatibility = true;

        public double MaxMass = double.MaxValue;
        public ILCA LCA { get; private set; } = new GHGFrontiers();

        public bool SOS_Assignment = false;
        public bool SOS_Continuous = false;

        public LPOptimizer LPOptimizer = LPOptimizer.Gurobi;
        public MILPOptimizer MILPOptimizer = MILPOptimizer.Gurobi;
        public NLPOptimizer NLPOptimizer = NLPOptimizer.IPOPT;

        public MILPFormulation MILPFormulation = MILPFormulation.RasmussenStolpe;
        public List<Tuple<string, string>> GurobiParameters = new List<Tuple<string, string>>();

        public bool CuttingStock = false;

        public OptimOptions() { }

    }
    
    public enum MILPFormulation
    {
        Bruetting = 0, RasmussenStolpe = 1, GhattasGrossmann = 2, NP = 3
    }
}
