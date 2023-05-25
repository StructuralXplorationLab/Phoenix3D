using Phoenix3D.LCA;
using Phoenix3D.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using Gurobi;
using Phoenix3D.Optimization.SAND;
using Phoenix3D.Model.CrossSections;
using System.Windows.Forms;
using System.Drawing;
using Phoenix3D.Reuse;
using Phoenix3D.Model.Materials;

namespace Phoenix3D.Optimization.TopologyOptimization
{
    public class DiscreteStockConstrainedOptimization
    {
        public Objective Objective { get; private set; } = Objective.MinStructureMass;
        public double ObjectiveValue { get; private set; } = double.PositiveInfinity;
        public OptimOptions Options { get; private set; }
        public bool TimeLimitReached { get; private set; } = false;
        public bool Interrupted { get; private set; } = false;
        public string Message { get; private set; } = "";

        public double Runtime { get; private set; } = 0;

        public List<Tuple<double, double>> LowerBounds;
        public List<Tuple<double, double>> UpperBounds;

        public DiscreteStockConstrainedOptimization(Objective Objective, OptimOptions Options = null)
        {
            this.Objective = Objective;

            if (Options is null)
                this.Options = new OptimOptions();
            else
                this.Options = Options;
        }

        public void Solve(Structure Structure, Stock Stock)
        {
            this.Solve(Structure, new List<string>() { "all" }, Stock, null);
        }
        public void Solve(Structure Structure, List<string> LoadCaseNames, Stock Stock, ILCA LCA = null)
        {

            List<LoadCase> LoadCases = Structure.GetLoadCasesFromNames(LoadCaseNames);

            if(LoadCases is null ||LoadCases.Count == 0)
            {
                throw new ArgumentException("LoadCases with the provided names are not existing in the structure. Check the names or use 'all' to compute all LoadCases.");
            }

            switch(Options.MILPOptimizer)
            {
                case MILPOptimizer.Gurobi:
                    {
                        SolveGurobiBR(Structure, LoadCases, Stock, LCA); break;
                    }
                default:
                    {
                        SolveGurobiBR(Structure, LoadCases, Stock, LCA); break;
                    }    
            }
        }

        public void SolveGurobiBR(Structure Structure, List<LoadCase> LoadCases, Stock Stock, ILCA LCA = null)
        {
            GRBEnv Env = new GRBEnv();
            GRBModel Model = new GRBModel(Env);
            Model.Parameters.TimeLimit = Options.MaxTime;
            foreach (var param in Options.GurobiParameters)
            {
                Model.Set(param.Item1, param.Item2);
            }

            if (!Structure.AllTopologyFixed())
                Stock.InsertElementGroup(0, ElementGroup.ZeroElement());

            Stock OriginalStock = Stock;
            if (Options.CuttingStock)
            {
                Stock = Stock.ExtendStock();
            }
            // Initialize Optimzation Variables
            // Binary assignment variables
            GRBVar[] T = SANDGurobiReuse.GetGurobiAssignmentVariables(Model, Structure, Stock, Options);

            // Continuous variables
            Dictionary<LoadCase, GRBVar[]> MemberForces = SANDGurobiReuse.GetGurobiMemberForceVariables(Model, Structure, LoadCases);
            Dictionary<LoadCase, GRBVar[]> Displacements = new Dictionary<LoadCase, GRBVar[]>();
            Dictionary<LoadCase, GRBVar[]> MemberElongations = new Dictionary<LoadCase, GRBVar[]>();
            if (Options.Compatibility)
            {
                Displacements = SANDGurobiReuse.GetGurobiDisplacementVariables(Model, Structure, LoadCases);
                MemberElongations = SANDGurobiReuse.GetGurobiMemberElongationVariables(Model, Structure, LoadCases, Stock);
            }



            // Add Constraints
            SANDGurobiDiscreteBR.AddAssignment(Model, T, Structure, Stock, Options);
            foreach (LoadCase LC in LoadCases)
            {
                SANDGurobiDiscreteBR.AddEquilibrium(Model, T, MemberForces[LC], Structure, LC, Stock, Options);
                Model.Update();
                if (Options.Compatibility)
                {
                    SANDGurobiDiscreteBR.AddCompatibility(Model, MemberElongations[LC], Displacements[LC], Structure, LC, Stock);
                    Model.Update();
                    SANDGurobiDiscreteBR.AddConstitutive(Model, MemberForces[LC], MemberElongations[LC], Structure, LC, Stock);
                    Model.Update();
                    SANDGurobiDiscreteBR.AddBigM(Model, T, MemberElongations[LC], Structure, LC, Stock, Options);
                    Model.Update();
                    SANDGurobiDiscreteBR.AddStress(Model, T, MemberForces[LC], Structure, LC, Stock);
                    Model.Update();
                }
                else
                {
                    SANDGurobiDiscreteBR.AddStress(Model, T, MemberForces[LC], Structure, LC, Stock);
                }
            }
            if (!Options.CuttingStock)
            {
                SANDGurobiReuse.SetObjective(Objective, Model, T, Structure, Stock, LCA);
                SANDGurobiReuse.AddAvailability(Model, T, Structure, Stock);
                SANDGurobiReuse.AddLength(Model, T, Structure, Stock);
            }
            else
            {
                GRBVar[] Y = SANDGurobiReuse.GetGurobiCuttingStockVariables(Model, Stock);
                SANDGurobiReuse.AddLengthCuttingStock(Model, T, Y, Structure, Stock);
                SANDGurobiReuse.SetObjectiveCuttingStock(Objective, Model, T, Y, Structure, Stock, LCA);
            }


            // Logging of Gurobi
            FormStartPosition Pos;
            Point Location;
            CloseOpenFormsAndGetPos(out Pos, out Location);
            LightCallback LightCB = null;
            LogCallback LogCB = null;

            if (Options.LogToConsole)
            {
                LogCB = new LogCallback(Pos, Location, Options.LogFormName);
                Model.SetCallback(LogCB);
            }
            else
            {
                LightCB = new LightCallback();
                Model.SetCallback(LightCB);
            }

            // Optimize
            try
            {
                Model.Optimize();
                Runtime = Model.Runtime;

                int GurobiStatus = Model.Status;

                if (GurobiStatus == GRB.Status.OPTIMAL || GurobiStatus == GRB.Status.TIME_LIMIT || GurobiStatus == GRB.Status.INTERRUPTED)
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

                        if (Options.LogToConsole)
                        {
                            LowerBounds = LogCB.LowerBounds;
                            UpperBounds = LogCB.UpperBounds;
                            LowerBounds.Add(new Tuple<double, double>(Model.Runtime, Model.ObjBound));
                            UpperBounds.Add(new Tuple<double, double>(Model.Runtime, Model.ObjVal));
                        }
                        else
                        {
                            LowerBounds = LightCB.LowerBounds;
                            UpperBounds = LightCB.UpperBounds;
                            LowerBounds.Add(new Tuple<double, double>(Model.Runtime, Model.ObjBound));
                            UpperBounds.Add(new Tuple<double, double>(Model.Runtime, Model.ObjVal));
                        }


                        foreach (Bar M in Structure.Members)
                        {
                            M.Nx.Clear();
                            Assignment Assignment = new Assignment();
                            bool is_present = false;

                            if (!Options.CuttingStock)
                            {
                                for (int j = 0; j < Stock.ElementGroups.Count; j++)
                                {
                                    if (T[M.Number * Stock.ElementGroups.Count + j].X >= 0.999)
                                    {
                                        is_present = true;
                                        M.CrossSection = Stock.ElementGroups[j].CrossSection;
                                        M.Material = Stock.ElementGroups[j].Material;
                                        if(Stock.ElementGroups[j].Type == ElementType.Reuse)
                                            Assignment.AddElementAssignment(Stock.ElementGroups[j], Stock.ElementGroups[j].Next);
                                        else
                                            Assignment.AddElementAssignment(Stock.ElementGroups[j], 0);

                                        foreach (LoadCase LC in LoadCases)
                                        {
                                            M.AddNormalForce(LC, new List<double>() { MemberForces[LC][M.Number].X } );
                                        }
                                    }
                                }
                            }
                            else
                            {
                                int EG_Counter = 0;
                                foreach (ElementGroup EG in OriginalStock.ElementGroups)
                                {
                                    for (int j = 0; j < EG.NumberOfElements; j++)
                                    {
                                        if (T[M.Number * Stock.ElementGroups.Count + EG_Counter + j].X >= 0.999)
                                        {
                                            is_present = true;
                                            M.CrossSection = EG.CrossSection;
                                            M.Material = EG.Material;
                                            
                                            if (EG.Type == ElementType.Reuse)
                                                Assignment.AddElementAssignment(EG, j);
                                            else
                                                Assignment.AddElementAssignment(EG, 0);

                                            foreach (LoadCase LC in LoadCases)
                                            {
                                                M.AddNormalForce(LC, new List<double>() { MemberForces[LC][M.Number].X } );
                                            }
                                        }
                                    }
                                    EG_Counter += EG.NumberOfElements;
                                }
                            }
                            if (!is_present)
                            {
                                Assignment.AddElementAssignment(OriginalStock.ElementGroups[0], 0);
                                M.CrossSection = new EmptySection();
                                foreach (LoadCase LC in LoadCases)
                                {
                                    M.AddNormalForce(LC, new List<double>() { 0 } );
                                }
                            }
                            M.SetAssignment(Assignment);
                        }



                        Stock.ResetRemainLenghts();
                        Stock.ResetRemainLenghtsTemp();
                        Stock.ResetAlreadyCounted();
                        Stock.ResetNext();

                        Structure.SetResults(new Result(Structure, OriginalStock, LCA));
                        Structure.SetLCA(LCA);
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
                Model.Dispose();
                Env.Dispose();
            }
            catch (GRBException e)
            {
                throw new GurobiException(e.Message);
            }
            Model.Dispose();
            Env.Dispose();
            if (!Structure.AllTopologyFixed())
            {
                Stock.RemoveElementGroup(0);
                if (Options.CuttingStock)
                {
                    OriginalStock.RemoveElementGroup(0);
                }
            }
            Stock.ResetNext();
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
