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
    
    public class DiscreteTrussTopologyOptimization
    {
        public Objective Objective { get; private set; } = Objective.MinStructureMass;
        public double ObjectiveValue { get; private set; } = double.PositiveInfinity;
        public OptimOptions Options { get; private set; }
        public bool TimeLimitReached { get; private set; } = false;
        public bool Interrupted { get; private set; } = false;
        public string Message { get; private set; } = "";

        public List<Tuple<double, double>> LowerBounds;
        public List<Tuple<double, double>> UpperBounds;

        public DiscreteTrussTopologyOptimization(Objective Objective, OptimOptions Options = null)
        {
            this.Objective = Objective;

            if (Options is null)
                this.Options = new OptimOptions();
            else
                this.Options = Options;
        }

        public void Solve(Structure Structure, Stock Stock)
        {
            this.Solve(Structure, new List<string>() { "all" }, Stock);
        }
        public void Solve(Structure Structure, List<string> LoadCaseNames, Stock Stock)
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
                        switch (Options.MILPFormulation)
                        {
                            case MILPFormulation.Bruetting:
                                SolveGurobiBR(Structure, LoadCases, Stock); break;
                            case MILPFormulation.RasmussenStolpe:
                                SolveGurobiRS(Structure, LoadCases, Stock); break;
                            case MILPFormulation.GhattasGrossmann:
                                SolveGurobiGG(Structure, LoadCases, Stock); break;
                            case MILPFormulation.NP:
                                SolveGurobiNP(Structure, LoadCases, Stock); break;
                        }
                    break;
                    }
                default:
                    {
                        SolveGurobiRS(Structure, LoadCases, Stock); break;
                    }    
            }
        }


        public void SolveGurobiRS(Structure Structure, List<LoadCase> LoadCases, Stock Stock)
        {
            GRBEnv Env = new GRBEnv();
            GRBModel Model = new GRBModel(Env);
            Model.Parameters.TimeLimit = Options.MaxTime;
            foreach (var param in Options.GurobiParameters)
            {
                Model.Set(param.Item1, param.Item2);
            }

            // Initialize Optimzation Variables
            // Binary assignment variables
            GRBVar[] T = SANDGurobiDiscreteRS.GetGurobiAssignmentVariables(Model, Structure, Stock);

            // Continuous variables
            Dictionary<LoadCase, GRBVar[]> MemberForces = SANDGurobiDiscreteRS.GetGurobiMemberForceVariables(Model, Structure, LoadCases, Stock, Options);
            Dictionary<LoadCase, GRBVar[]> Displacements = new Dictionary<LoadCase, GRBVar[]>();
            if (Options.Compatibility)
                Displacements = SANDGurobiDiscreteRS.GetGurobiDisplacementVariables(Model, Structure, LoadCases);

            // Set Objective Function
            SANDGurobiDiscreteRS.SetObjective(Objective, Model, T, Structure, Stock);

            // Add Constraints
            SANDGurobiDiscreteRS.AddAssignment(Model, T, Structure, Stock, Options);
            foreach (LoadCase LC in LoadCases)
            {
                SANDGurobiDiscreteRS.AddEquilibrium(Model, T, MemberForces[LC], Structure, LC, Stock, Options);
                SANDGurobiDiscreteRS.AddStress(Model, T, MemberForces[LC], Structure, LC, Stock, Options);

                if (Options.Compatibility)
                {
                    SANDGurobiDiscreteRS.AddCompatibility(Model, T, MemberForces[LC], Displacements[LC], Structure, LC, Stock, Options);
                }
            }

            SANDGurobiReuse.AddGroup(Model, T, Structure, Stock);

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
                        if(Options.Compatibility)
                        {
                            List<double> us = new List<double>();

                            foreach (GRBVar v in Displacements[LoadCases[0]])
                                us.Add(v.X);

                        }


                        foreach (Bar M in Structure.Members)
                        {
                            M.Nx.Clear();
                            Assignment Assignment = new Assignment();

                            bool is_present = false;
                            for (int j = 0; j < Stock.ElementGroups.Count; j++)
                            {
                                if (T[M.Number * Stock.ElementGroups.Count + j].X >= 0.999)
                                {
                                    is_present = true;
                                    M.CrossSection = Stock.ElementGroups[j].CrossSection;
                                    M.Material = Stock.ElementGroups[j].Material;
                                    Assignment.AddElementAssignment(Stock.ElementGroups[j], Stock.ElementGroups[j].Next);

                                    foreach (LoadCase LC in LoadCases)
                                    {
                                        if (Options.Compatibility)
                                            M.AddNormalForce(LC, new List<double>() { MemberForces[LC][M.Number * Stock.ElementGroups.Count + j].X } );
                                        else
                                            M.AddNormalForce(LC, new List<double>() { MemberForces[LC][M.Number].X } );
                                    }
                                }
                            }
                            if (!is_present)
                            {
                                M.CrossSection = new EmptySection();
                                M.Material = new EmptyMaterial();
                                foreach (LoadCase LC in LoadCases)
                                {
                                    M.AddNormalForce(LC, new List<double>() { 0 } );
                                }
                            }
                            M.SetAssignment(Assignment);
                        }
                        
                        Structure.SetResults(new Result(Structure, Stock));
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
        }
        public void SolveGurobiGG(Structure Structure, List<LoadCase> LoadCases, Stock Stock)
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

            // Initialize Optimzation Variables
            // Binary assignment variables
            GRBVar[] T = SANDGurobiDiscreteGG.GetGurobiAssignmentVariables(Model, Structure, Stock);

            // Continuous variables
            Dictionary<LoadCase, GRBVar[]> MemberForces = SANDGurobiDiscreteGG.GetGurobiMemberForceVariables(Model, Structure, LoadCases);
            Dictionary<LoadCase, GRBVar[]> MemberStresses = new Dictionary<LoadCase, GRBVar[]>();
            Dictionary<LoadCase, GRBVar[]> Displacements = new Dictionary<LoadCase, GRBVar[]>();
            Dictionary<LoadCase, GRBVar[]> MemberElongations = new Dictionary<LoadCase, GRBVar[]>();
            if (Options.Compatibility)
            {
                MemberStresses = SANDGurobiDiscreteGG.GetGurobiMemberStressVariables(Model, Structure, LoadCases);
                Displacements = SANDGurobiDiscreteGG.GetGurobiDisplacementVariables(Model, Structure, LoadCases);
                MemberElongations = SANDGurobiDiscreteGG.GetGurobiMemberElongationVariables(Model, Structure, LoadCases, Stock);
            }

            // Set Objective Function
            SANDGurobiDiscreteGG.SetObjective(Objective, Model, T, Structure, Stock);
            // Add Constraints
            SANDGurobiDiscreteGG.AddAssignment(Model, T, Structure, Stock, Options);
            foreach (LoadCase LC in LoadCases)
            {
                SANDGurobiDiscreteGG.AddEquilibrium(Model, T, MemberForces[LC], Structure, LC, Stock, Options);
                if (Options.Compatibility)
                {
                    SANDGurobiDiscreteGG.AddCompatibility(Model, MemberElongations[LC], Displacements[LC], Structure, LC, Stock);
                    SANDGurobiDiscreteGG.AddConstitutive(Model,MemberForces[LC], MemberElongations[LC], Structure, LC, Stock);
                    SANDGurobiDiscreteGG.AddHooke(Model, MemberStresses[LC], MemberElongations[LC], Structure, LC, Stock);
                    SANDGurobiDiscreteGG.AddBigM(Model, T, MemberElongations[LC], Structure, LC, Stock, Options);
                }
                else
                {
                    SANDGurobiDiscreteGG.AddStress(Model, T, MemberForces[LC], Structure, LC, Stock);
                }
            }

            SANDGurobiReuse.AddGroup(Model, T, Structure, Stock);

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
                            Assignment Assignment = new Assignment();
                            M.Nx.Clear();
                            bool is_present = false;
                            for (int j = 0; j < Stock.ElementGroups.Count; j++)
                            {
                                if (T[M.Number * Stock.ElementGroups.Count + j].X >= 0.999)
                                {
                                    is_present = true;
                                    M.CrossSection = Stock.ElementGroups[j].CrossSection;
                                    M.Material = Stock.ElementGroups[j].Material;
                                    Assignment.AddElementAssignment(Stock.ElementGroups[j], Stock.ElementGroups[j].Next);
                                    foreach (LoadCase LC in LoadCases)
                                    {
                                        M.AddNormalForce(LC, new List<double>() { MemberForces[LC][M.Number].X } );
                                    }
                                }
                            }
                            if (!is_present)
                            {
                                Assignment.AddElementAssignment(Stock.ElementGroups[0], 0);
                                M.CrossSection = new EmptySection();
                                foreach (LoadCase LC in LoadCases)
                                {
                                    M.AddNormalForce(LC, new List<double>() { 0 });
                                }
                            }
                            M.SetAssignment(Assignment);
                        }
                        Structure.SetResults(new Result(Structure, Stock));
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
                Stock.ElementGroups.RemoveAt(0);
            Stock.ResetNext();
        }
        public void SolveGurobiBR(Structure Structure, List<LoadCase> LoadCases, Stock Stock)
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

            // Initialize Optimzation Variables
            // Binary assignment variables
            GRBVar[] T = SANDGurobiDiscreteBR.GetGurobiAssignmentVariables(Model, Structure, Stock);

            // Continuous variables
            Dictionary<LoadCase, GRBVar[]> MemberForces = SANDGurobiDiscreteBR.GetGurobiMemberForceVariables(Model, Structure, LoadCases);
            Dictionary<LoadCase, GRBVar[]> Displacements = new Dictionary<LoadCase, GRBVar[]>();
            Dictionary<LoadCase, GRBVar[]> MemberElongations = new Dictionary<LoadCase, GRBVar[]>();
            if (Options.Compatibility)
            {
                Displacements = SANDGurobiDiscreteBR.GetGurobiDisplacementVariables(Model, Structure, LoadCases);
                MemberElongations = SANDGurobiDiscreteBR.GetGurobiMemberElongationVariables(Model, Structure, LoadCases, Stock);
            }

            // Set Objective Function
            SANDGurobiDiscreteBR.SetObjective(Objective, Model, T, Structure, Stock);

            // Add Constraints
            SANDGurobiDiscreteBR.AddAssignment(Model, T, Structure, Stock, Options);
            Model.Update();
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
            SANDGurobiReuse.AddGroup(Model, T, Structure, Stock);


            /*
            double penalty = 6000;
            foreach (LoadCase LC in LoadCases)
            {
                foreach (GRBVar u in Displacements[LC])
                {
                    Model.SetPWLObj(u, new double[] { -5, -2, 0, 2, 5 }, new double[] { penalty, 0, 0, 0, penalty });
                }
            }
            */


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
                        //List<double> Us = Displacements[Structure.LoadCases[0]].Select(x => x.X).ToList();
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
                            for (int j = 0; j < Stock.ElementGroups.Count; j++)
                            {
                                if (T[M.Number * Stock.ElementGroups.Count + j].X >= 0.999)
                                {
                                    is_present = true;
                                    M.CrossSection = Stock.ElementGroups[j].CrossSection;
                                    M.Material = Stock.ElementGroups[j].Material;
                                    Assignment.AddElementAssignment(Stock.ElementGroups[j], Stock.ElementGroups[j].Next);

                                    foreach (LoadCase LC in LoadCases)
                                    {
                                        M.AddNormalForce(LC, new List<double>() { MemberForces[LC][M.Number].X } );
                                    }
                                }
                            }
                            if (!is_present)
                            {
                                Assignment.AddElementAssignment(Stock.ElementGroups[0], 0);
                                M.CrossSection = new EmptySection();
                                //M.Material = new EmptyMaterial();
                                foreach (LoadCase LC in LoadCases)
                                {
                                    M.AddNormalForce(LC, new List<double>() { 0 } );
                                }
                            }
                            M.SetAssignment(Assignment);
                        }
                        Structure.SetResults(new Result(Structure, Stock));
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
                Stock.ElementGroups.RemoveAt(0);
            Stock.ResetNext();
        }
        public void SolveGurobiNP(Structure Structure, List<LoadCase> LoadCases, Stock Stock)
        {
            GRBEnv Env = new GRBEnv();
            GRBModel Model = new GRBModel(Env);
            Model.Parameters.TimeLimit = Options.MaxTime;
            foreach (var param in Options.GurobiParameters)
            {
                Model.Set(param.Item1, param.Item2);
            }

            if(!Structure.AllTopologyFixed())
                Stock.InsertElementGroup(0, ElementGroup.ZeroElement());

            // Initialize Optimzation Variables
            // Binary assignment variables
            GRBVar[] T = SANDGurobiDiscreteNP.GetGurobiAssignmentVariables(Model, Structure, Stock);

            // Continuous variables
            Dictionary<LoadCase, GRBVar[]> Displacements = new Dictionary<LoadCase, GRBVar[]>();
            Dictionary<LoadCase, GRBVar[]> MemberElongations = new Dictionary<LoadCase, GRBVar[]>();
            if (Options.Compatibility)
            {
                Displacements = SANDGurobiDiscreteNP.GetGurobiDisplacementVariables(Model, Structure, LoadCases);
                MemberElongations = SANDGurobiDiscreteNP.GetGurobiMemberElongationVariables(Model, Structure, LoadCases, Stock);
            }

            // Set Objective Function
            SANDGurobiDiscreteNP.SetObjective(Objective, Model, T, Structure, Stock);

            // Add Constraints
            SANDGurobiDiscreteNP.AddAssignment(Model, T, Structure, Stock, Options);
            
            foreach (LoadCase LC in LoadCases)
            {
                SANDGurobiDiscreteNP.AddEquilibrium(Model, T, MemberElongations[LC], Structure, LC, Stock, Options);
                if (Options.Compatibility)
                {
                    SANDGurobiDiscreteNP.AddCompatibility(Model, MemberElongations[LC], Displacements[LC], Structure, LC, Stock);
                    SANDGurobiDiscreteNP.AddBigM(Model, T, MemberElongations[LC], Structure, LC, Stock, Options);
                }
                else
                {
                    SANDGurobiDiscreteNP.AddStress(Model, T, MemberElongations[LC], Structure, LC, Stock);
                }
            }
            SANDGurobiReuse.AddGroup(Model, T, Structure, Stock);

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
                            for (int j = 0; j < Stock.ElementGroups.Count; j++)
                            {
                                if (T[M.Number * Stock.ElementGroups.Count + j].X >= 0.999)
                                {
                                    is_present = true;
                                    M.CrossSection = Stock.ElementGroups[j].CrossSection;
                                    M.Material = Stock.ElementGroups[j].Material;
                                    Assignment.AddElementAssignment(Stock.ElementGroups[j], Stock.ElementGroups[j].Next);

                                    foreach (LoadCase LC in LoadCases)
                                    {
                                        M.AddNormalForce(LC, new List<double>() { MemberElongations[LC][M.Number * Stock.ElementGroups.Count + j].X * M.Material.E * Stock.ElementGroups[j].CrossSection.Area / M.Length } );
                                    }
                                }
                            }
                            if (!is_present)
                            {
                                Assignment.AddElementAssignment(Stock.ElementGroups[0], 0);
                                M.CrossSection = new EmptySection();
                                //M.Material = new EmptyMaterial();
                                foreach (LoadCase LC in LoadCases)
                                {
                                    M.AddNormalForce(LC, new List<double>() { 0 } );
                                }
                            }
                            M.SetAssignment(Assignment);
                        }
                        Structure.SetResults(new Result(Structure, Stock));
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
                Stock.RemoveElementGroup(0);
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
