using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Phoenix3D.Optimization
{
    
    public enum LPOptimizer
    {
        Gurobi = 0,
        CPLEX = 1,
        GLPK = 2,
        MOSEK = 3
    }
    
    public enum MILPOptimizer
    {
        Gurobi = 0,
        CPLEX = 1,
        GLPK = 2,
        MOSEK = 3
    }
    
    public enum NLPOptimizer
    {
        IPOPT = 0
    }

}
