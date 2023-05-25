using Phoenix3D.LCA;
using Phoenix3D.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gurobi;
using Phoenix3D.Optimization.SAND;
using Phoenix3D.Model.CrossSections;
using System.Windows.Forms;
using System.Drawing;

namespace Phoenix3D.Optimization.TopologyOptimization
{
    
    public class ContinuousTrussTopologyOptimization
    {
        public List<Assignment> Assignments { get; private set; } = new List<Assignment>();
        public Objective Objective { get; private set; } = Objective.MinStructureMass;
        public double ObjectiveValue { get; private set; } = double.PositiveInfinity;
        public OptimOptions Options { get; private set; }
        public Dictionary<LoadCase, double[]> MemberForces { get; private set; } = new Dictionary<LoadCase, double[]>();
        public bool TimeLimitReached { get; private set; } = false;
        public bool Interrupted { get; private set; } = false;
        public string Message { get; private set; } = "";


        public ContinuousTrussTopologyOptimization(OptimOptions Options = null)
        {
            if (Options is null)
                this.Options = new OptimOptions();
            else
                this.Options = Options;
        }

        public void Solve(Structure Structure)
        {
            this.Solve(Structure, new List<string>() { "all" });
        }
        public void Solve(Structure Structure, List<string> LoadCaseNames)
        {

            List<LoadCase> LoadCases = Structure.GetLoadCasesFromNames(LoadCaseNames);

            if(LoadCases is null ||LoadCases.Count == 0)
            {
                throw new ArgumentException("LoadCases with the provided names are not existing in the structure. Check the names or use 'all' to compute all LoadCases.");
            }

            switch(Options.LPOptimizer)
            {
                case LPOptimizer.Gurobi:
                    {
                        SolveGurobi(Structure, LoadCases); break;
                    }
                default:
                    {
                        SolveGurobi(Structure, LoadCases); break;
                    }    
            }
        }
        public void SolveGurobi(Structure Structure, List<LoadCase> LoadCases)
        {
            GRBEnv Env = new GRBEnv();
            GRBModel Model = new GRBModel(Env);
            int m = Structure.Members.OfType<Bar>().Count();

            GRBVar[] MemberAreas = Model.AddVars(m, GRB.CONTINUOUS);
            Dictionary<LoadCase, GRBVar[]> MemberForces = new Dictionary<LoadCase, GRBVar[]>();

            int i = 0;
            foreach(Bar B in Structure.Members)
            {
                MemberAreas[i].LB = B.LBArea;
                MemberAreas[i].UB = B.UBArea;
                MemberAreas[i].VarName = "a" + i.ToString();
                i++;
            }
            
            foreach(LoadCase LC in LoadCases)
            {
                MemberForces.Add(LC, Model.AddVars(Enumerable.Repeat(double.NegativeInfinity, m).ToArray(), Enumerable.Repeat(double.PositiveInfinity, m).ToArray(), new double[m], Enumerable.Repeat(GRB.CONTINUOUS, m).ToArray(), null));
                SANDGurobiContinuous.AddEquilibrium(Model, MemberAreas, MemberForces[LC], Structure, LC, Options);
                SANDGurobiContinuous.AddStress(Model, MemberAreas, MemberForces[LC], Structure, LC, Options);
            }
            SANDGurobiContinuous.SetObjective(Objective,Model, MemberAreas, MemberForces, Structure);



            // Logging of Gurobi
            FormStartPosition Pos;
            Point Location;
            CloseOpenFormsAndGetPos(out Pos, out Location);

            if (Options.LogToConsole)
            {
                Model.SetCallback(new LogCallback(Pos, Location, Options.LogFormName));
            }
            else
            {
                Model.SetCallback(new LightCallback());
            }


            try
            {
                Model.Optimize();
            }
            catch 
            {
                //TODO: IMPLEMENT catches; put everything into try
            }

            /*
            if (Options.LogToConsole)
            {
                CB.OutputForm.Close();
                CB.OutputForm.Dispose();
            }
            */


            int GurobiStatus = Model.Status;

            if (GurobiStatus == GRB.Status.TIME_LIMIT)
            {
                TimeLimitReached = true;
            }
            if(GurobiStatus == GRB.Status.OPTIMAL || GurobiStatus == GRB.Status.TIME_LIMIT || GurobiStatus == GRB.Status.INTERRUPTED)
            {
                if (GurobiStatus == GRB.Status.TIME_LIMIT)
                {
                    TimeLimitReached = true;
                }
                if (GurobiStatus == GRB.Status.INTERRUPTED)
                {
                    Interrupted = true;
                }

                try
                {
                    ObjectiveValue = Model.ObjVal;

                    i = 0;
                    foreach (Bar M in Structure.Members)
                    {
                        double A = MemberAreas[M.Number].X;
                        M.CrossSection = new CircularSection(Math.Sqrt(4 * A / Math.PI));
                        M.Nx.Clear();
                        i++;
                    }
                    foreach (LoadCase LC in LoadCases)
                    {
                        double[] mf = new double[MemberForces[LC].Length];
                        foreach (Bar M in Structure.Members)
                        {
                            mf[M.Number] = MemberForces[LC][M.Number].X;
                            M.AddNormalForce(LC, new List<double>() { MemberForces[LC][M.Number].X });
                        }
                        this.MemberForces.Add(LC, mf);
                    }
                    Structure.SetResults(new Result(Structure));
                }
                catch (GRBException e)
                {
                    Message = e.Message;
                }
            }
            else if (GurobiStatus == GRB.Status.INFEASIBLE)
            {
                throw new SystemException("Gurobi problem is infeasible");
            }
        }
        private void CloseOpenFormsAndGetPos(out FormStartPosition Pos, out Point Location)
        {
            Pos = FormStartPosition.Manual;
            Location = new Point();

            List<Form> FormsToClose = new List<Form>();
            foreach (Form f in Application.OpenForms)
            {
                if (f.Name == this.Options.LogFormName)
                {
                    FormsToClose.Add(f);
                }
            }
            foreach (Form f in FormsToClose)
            {
                Pos = f.StartPosition;
                Location = f.Location;
                f.Close();
                f.Dispose();
            }
        }
    }
}
