using Phoenix3D.LCA;
using Phoenix3D.Model;
using Phoenix3D.Optimization;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;


namespace Phoenix3D.Reuse.Heuristics
{
    
    public class BestFit
    {
        public Objective Objective { get; private set; } = Objective.MinStructureMass;
        public double Runtime { get; private set; } = 0;

        public Result Result { get; private set; }

        public bool PromoteFirstPart { get; private set; } = false;

        private ILCA LCA;

        public BestFit(Objective Objective, ILCA LCA = null, bool PromoteFirstPart = false)
        {
            this.Objective = Objective;
            this.PromoteFirstPart = PromoteFirstPart;
            if (LCA is null)
                this.LCA = new GHGFrontiers();
            else
                this.LCA = LCA;
        }

        public void Solve(Structure Structure, Stock Stock, List<LoadCase> LoadCases, double allowedCapacity)
        {
            Stock.ResetRemainLenghts();
            Stock.ResetRemainLenghtsTemp();
            Stopwatch sw = new Stopwatch();
            sw.Start();
            foreach (IMember1D Member in Structure.Members)
            {
                Assignment Assignment = new Assignment();
                for (int NumberOfCompounds = Member.MinCompound; NumberOfCompounds <= Member.MaxCompound; NumberOfCompounds++)
                {
                    Assignment tempAssignment = GetBestAssignment(NumberOfCompounds, Member, Stock, LoadCases, allowedCapacity);
                    if (tempAssignment.Feasible && tempAssignment.ObjectiveValue < Assignment.ObjectiveValue)
                    {
                        Assignment = tempAssignment;
                    }
                    foreach (ElementGroup EG in tempAssignment.ElementGroups)
                        EG.ResetRemainLengthsTemp();
                    //Stock.ResetRemainLenghtsTemp();
                }

                if (Assignment.Feasible)
                {
                    Member.SetAssignment(Assignment);
                    Member.SetCrossSection(Assignment.ElementGroups[0].CrossSection);
                    for (int a = 0; a < Assignment.ElementGroups.Count; a++)
                    {
                        if (Assignment.ElementGroups[a].Type == ElementType.Reuse)
                        {
                            if (Assignment.ElementGroups[a].CanBeCut)
                            {
                                Assignment.ElementGroups[a].RemainLengths[Assignment.ElementIndices[a]] -= Member.Length - Member.Buffer.Item1;
                                Assignment.ElementGroups[a].RemainLengthsTemp[Assignment.ElementIndices[a]] = Assignment.ElementGroups[a].RemainLengths[Assignment.ElementIndices[a]];
                            }
                            else
                            {
                                Assignment.ElementGroups[a].RemainLengths[Assignment.ElementIndices[a]] = 0;
                                Assignment.ElementGroups[a].RemainLengthsTemp[Assignment.ElementIndices[a]] = 0;
                            }
                        }
                    }
                }
                else if (allowedCapacity < 1.0)
                {
                    Stock.ResetRemainLenghts();
                    Stock.ResetRemainLenghtsTemp();
                    Stock.ResetAlreadyCounted();
                    Stock.ResetNext();
                }
                else
                {
                    Stock.ResetRemainLenghts();
                    Stock.ResetRemainLenghtsTemp();
                    Stock.ResetAlreadyCounted();
                    Stock.ResetNext();
                    Runtime = sw.ElapsedMilliseconds / 1000;
                    sw.Stop();
                    throw new Exception("Infeasible problem");
                }
            }
            Runtime = sw.ElapsedMilliseconds / 1000;
            sw.Stop();

            Stock.ResetAlreadyCounted();

            Stock.ResetRemainLenghts();
            Structure.SetResults(new Result(Structure, Stock, LCA));
            Structure.SetLCA(LCA);
        }

        private Assignment GetBestAssignment(int NumberOfCompounds, IMember1D Member, Stock Stock, List<LoadCase> LoadCases, double allowedCapacity)
        {
            Assignment Assignment = new Assignment();
            Assignment.SetFeasible(true);
            double total_obj = 0;

            for (int i = 0; i < NumberOfCompounds; i++)
            {
                var tempj = 0;
                var tempn = 0;
                bool ifeasible = false;
                double iobjective = double.PositiveInfinity;

                for (int j = 0; j < Stock.ElementGroups.Count; j++)
                {
                    ElementGroup EG = Stock.ElementGroups[j];
                    if (!EG.CrossSection.PossibleCompounds.Contains(NumberOfCompounds))
                        continue;
                    if (Member.AllowedCrossSections.Any() && !Member.AllowedCrossSections.Contains(EG.CrossSection.TypeName))
                        continue;
                    if (Member.Nx.Count != 0 && !CheckForceCapacity(Member, EG, NumberOfCompounds, LoadCases, allowedCapacity))
                        continue;

                    for (int n = 0; n < EG.NumberOfElements; n++)
                    {
                        double TempLength = EG.RemainLengthsTemp[n];
                        bool length_check = false;
                        switch (EG.CanBeCut)
                        {
                            case true: length_check = Member.Length - Member.Buffer.Item1 <= TempLength; break;
                            case false: length_check = Member.Length - Member.Buffer.Item1 <= TempLength && TempLength <= Member.Length - Member.Buffer.Item2; break;
                        }

                        if (EG.Type == ElementType.New || length_check)
                        {
                            ifeasible = true;
                            double obj = GetObjectiveValue(this.Objective, Member, EG, TempLength, TempLength < EG.Length);
                            if (obj < iobjective)
                            {
                                tempj = j;
                                tempn = n;
                                iobjective = obj;
                                if (Math.Abs(TempLength - EG.Length) < 1e-3)
                                {
                                    break;
                                }
                            }
                        }
                        else if (!length_check && Math.Abs(TempLength - EG.Length) < 1e-3)
                        {
                            break;
                        }
                    }
                }

                if (ifeasible)
                {
                    total_obj += iobjective;
                    Assignment.AddElementAssignment(Stock.ElementGroups[tempj], tempn);
                    if (Stock.ElementGroups[tempj].CanBeCut)
                    {
                        Stock.ElementGroups[tempj].RemainLengthsTemp[tempn] -= Member.Length - Member.Buffer.Item1;
                    }
                    else
                    {
                        Stock.ElementGroups[tempj].RemainLengthsTemp[tempn] = 0;
                    }
                }
                else
                {
                    Assignment.SetFeasible(false);
                    break;
                }
            }
            if (Assignment.Feasible)
            {
                Assignment.SetObjectiveValue(total_obj);
            }
            return Assignment;
        }
        private bool CheckForceCapacity(IMember1D Member, ElementGroup EG, int Divisor, List<LoadCase> LoadCases, double allowedCapacity)
        {
            switch (Member)
            {
                case Bar Bar: return CheckBarForceCapacity(Bar, EG, Divisor, LoadCases, allowedCapacity);
                case Beam Beam: return CheckBeamForceCapacity(Beam, EG, Divisor, LoadCases, allowedCapacity);
                default: return CheckBarForceCapacity(Member, EG, Divisor, LoadCases, allowedCapacity);
            }
        }
        private bool CheckBarForceCapacity(IMember1D Bar, ElementGroup EG, int Divisor, List<LoadCase> LoadCases, double allowedCapacity)
        {
            double Mtension = Math.Max(0, LoadCases.Where(Bar.Nx.ContainsKey).Select(x => Bar.Nx[x].Max()).Max() / Divisor);
            double Mcompression = Math.Min(0, LoadCases.Where(Bar.Nx.ContainsKey).Select(x => Bar.Nx[x].Min()).Min() / Divisor);

            //double Mtension = Math.Max(0, Bar.Nx.Values.Max() / (double)Divisor);
            //double Mcompression = Math.Min(0, Bar.Nx.Values.Min() / (double)Divisor);

            double EGtension = EG.CrossSection.GetTensionResistance(EG.Material) * allowedCapacity;
            double EGcompression = EG.CrossSection.GetBucklingResistance(EG.Material, Bar.BucklingType, Bar.BucklingLength).Max() * allowedCapacity;

            if (EGcompression <= Mcompression && Mtension <= EGtension)
                return true;
            else
                return false;
        }
        private bool CheckBeamForceCapacity(Beam Beam, ElementGroup EG, int Divisor, List<LoadCase> LoadCases, double allowedCapacity)
        {
            double NRt = EG.CrossSection.GetTensionResistance(EG.Material) * allowedCapacity;
            double NRk = EG.CrossSection.GetBucklingResistance(EG.Material, Beam.BucklingType, Beam.BucklingLength).Max() * allowedCapacity;

            foreach (LoadCase LC in LoadCases)
            {
                if (Beam.Nx[LC][0] < NRk || Beam.Nx[LC][0] > NRt)
                    return false;

                for (int i = 0; i < Beam.Nx[LC].Count; i++)
                {
                    double Nx = Beam.Nx[LC][i] / Divisor;
                    double Vy = Beam.Vy[LC][i] / Divisor;
                    double Vz = Beam.Vz[LC][i] / Divisor;
                    double My = Beam.My[LC][i] / Divisor;
                    double Mz = Beam.Mz[LC][i] / Divisor;
                    double Mt = Beam.Mt[LC][i] / Divisor;

                    if (EG.CrossSection.GetUtilization(EG.Material, Beam.BucklingType, Beam.BucklingLength, false, Nx, Vy, Vz, My, Mz, Mt) > 1)
                        return false;
                }
            }
            return true;
        }
        private double GetObjectiveValue(Objective objective, IMember1D Member, ElementGroup EG, double L_remain, bool alreadyused)
        {
            switch (objective)
            {
                case Objective.MinStockMass:
                    {
                        if (EG.Type == ElementType.Reuse)
                            if (alreadyused)
                                return 0;
                            else
                                return EG.Material.Density * EG.Length * EG.CrossSection.Area;
                        else if (EG.Type == ElementType.New)
                            return EG.Material.Density * Member.Length * EG.CrossSection.Area;
                        else
                            return 0;
                    }
                case Objective.MinStructureMass:
                    {
                        if (EG.Type == ElementType.Reuse)
                            return EG.Material.Density * EG.CrossSection.Area * Member.Length;
                        else if (EG.Type == ElementType.New)
                            return EG.Material.Density * EG.CrossSection.Area * Member.Length;
                        else
                            return 0;
                    }
                case Objective.MinWaste:
                    {
                        if (EG.Type == ElementType.Reuse)
                        {
                            if (EG.CanBeCut && L_remain >= Member.Length)
                                if (alreadyused)
                                    return -EG.Material.Density * EG.CrossSection.Area * Member.Length;
                                else
                                    return EG.Material.Density * EG.CrossSection.Area * (L_remain - Member.Length);
                            else if (EG.CanBeCut && L_remain < Member.Length)
                                return 0.5 * EG.Material.Density * EG.CrossSection.Area * (-L_remain + Member.Length);
                            else if (!EG.CanBeCut && EG.Length < Member.Length)
                                return 0;
                            else
                                return 0.5 * EG.Material.Density * EG.CrossSection.Area * (EG.Length - Member.Length);
                        }
                        else if (EG.Type == ElementType.New)
                            return 0;
                        else
                            return 0;
                    }
                case Objective.MinLCA:
                    {
                        if (L_remain < EG.Length)
                            return LCA.ReturnElementMemberImpact(EG, true, Member);
                        else
                        {
                            if (PromoteFirstPart)
                                return LCA.ReturnElementMemberImpact(EG, true, Member);
                            else
                                return LCA.ReturnElementMemberImpact(EG, false, Member);
                        }
                    }
                default:
                    {
                        if (EG.Type == ElementType.Reuse)
                            return EG.Material.Density * EG.CrossSection.Area * Member.Length;
                        else if (EG.Type == ElementType.New)
                            return EG.Material.Density * EG.CrossSection.Area * Member.Length;
                        else
                            return 0;
                    }
            }
        }
    }
}
