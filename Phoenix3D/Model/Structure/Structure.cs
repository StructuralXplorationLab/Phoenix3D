using Phoenix3D.LCA;
using Phoenix3D.Optimization;

using System;
using System.Collections.Generic;
using System.Linq;


namespace Phoenix3D.Model
{
    
    public class Structure
    {
        public List<IMember> Members { get; private set; } = new List<IMember>();
        public List<Node> Nodes { get; private set; } = new List<Node>();
        public List<LoadCase> LoadCases { get; private set; } = new List<LoadCase>();
        public List<Support> Supports { get; private set; } = new List<Support>();
        internal bool[] FixedDofs { get; private set; } = new bool[0];
        public int NFixedTranslations { get; private set; }
        public int NFixedRotations { get; private set; }
        public int NFreeTranslations { get; private set; }
        public int NFreeRotations { get; private set; }
        public SortStructureMembersBy SortBy { get; private set; } = SortStructureMembersBy.Off;
        public List<int> SortMap { get; private set; } = new List<int>();
        public Result Results { get; private set; }
        public ILCA LCA { get; private set; } = new GHGFrontiers();
        public Dictionary<int, List<IMember>> MemberGroups { get; private set; } = new Dictionary<int, List<IMember>>();

        public Structure() { }

        public void AddMember(IMember M)
        {
            if (!Members.Contains(M))
            {
                M.SetNumber(Members.Count);
                Members.Add(M);
                if(MemberGroups.ContainsKey(M.GroupNumber) && !(MemberGroups[M.GroupNumber] is null))
                {
                    MemberGroups[M.GroupNumber].Add(M);
                }
                else
                {
                    MemberGroups.Add(M.GroupNumber, new List<IMember>());
                    MemberGroups[M.GroupNumber].Add(M);
                }
            }

            else
                throw new ArgumentException($"This member already exists in the structure!");

            //AssembleStructure();

            switch(M)
            {
                case Bar Bar :
                    { 
                        AssembleIMember1D(Bar);
                        foreach (LoadCase LC in Bar.Nx.Keys)
                        {
                            this.AddLoadcase(LC);
                        }
                        break;
                    }
                case Beam Beam:
                    {
                       AssembleIMember1D(Beam);
                        foreach (LoadCase LC in Beam.Nx.Keys)
                        {
                            this.AddLoadcase(LC);
                        }
                        break;
                    }
            }
            
            SetResults(new Result(this, this.LCA));
        }
        internal void AssembleIMember1D(IMember1D Member)
        {
            if(!Nodes.Contains(Member.From))
            {
                var m = Member.From;
                m.Number = Nodes.Count;
                Member.From = m;
                Member.From.AddConnectedMember(Member, -1.0);
                Nodes.Add(Member.From);
            }
            else
            {
                Member.From = Nodes.Find(N => N.Equals(Member.From));
                Member.From.AddConnectedMember(Member, -1.0);
            }
            if (!Nodes.Contains(Member.To))
            {
                var m = Member.To;
                m.Number = Nodes.Count;
                Member.To = m;
                Member.To.AddConnectedMember(Member, +1.0);
                Nodes.Add(Member.To);
            }
            else
            {
                Member.To = Nodes.Find(N => N.Equals(Member.To));
                Member.To.AddConnectedMember(Member, +1.0);
            }
        }
        private void SetDofs()
        {
            FixedDofs = new bool[Nodes.Count * 6];

            NFixedTranslations = 0;
            NFixedRotations = 0;

            int redDof = 0;
            int redDofTruss = 0;

            for (int k = 0; k < Nodes.Count; ++k)
            {
                // Cumulate the number of fixed dofs in the structure
                NFixedTranslations += (Nodes[k].FixTx ? 1 : 0) + (Nodes[k].FixTy ? 1 : 0) + (Nodes[k].FixTz ? 1 : 0);
                NFixedRotations += (Nodes[k].FixRx ? 1 : 0) + (Nodes[k].FixRy ? 1 : 0) + (Nodes[k].FixRz ? 1 : 0);

                // Assemble FixeDof Array from Nodal fixtures
                FixedDofs[Nodes[k].Number * 6 + 0] = Nodes[k].FixTx;
                FixedDofs[Nodes[k].Number * 6 + 1] = Nodes[k].FixTy;
                FixedDofs[Nodes[k].Number * 6 + 2] = Nodes[k].FixTz;
                FixedDofs[Nodes[k].Number * 6 + 3] = Nodes[k].FixRx;
                FixedDofs[Nodes[k].Number * 6 + 4] = Nodes[k].FixRy;
                FixedDofs[Nodes[k].Number * 6 + 5] = Nodes[k].FixRz;

                Nodes[k].ResetReducedDofs();

                /*N_tmp.Dofs[0] = N.Number * 6 + 0;
                N_tmp.Dofs[1] = N.Number * 6 + 1;
                N_tmp.Dofs[2] = N.Number * 6 + 2;
                N_tmp.Dofs[3] = N.Number * 6 + 3;
                N_tmp.Dofs[4] = N.Number * 6 + 4;
                N_tmp.Dofs[5] = N.Number * 6 + 5;

                N_tmp.DofsTruss[0] = N.Number * 3 + 0;
                N_tmp.DofsTruss[1] = N.Number * 3 + 1;
                N_tmp.DofsTruss[2] = N.Number * 3 + 2;
                */


                // Create reduced Dofs Mapping
                if (!Nodes[k].FixTx)
                {
                    Nodes[k].ReducedDofs[0] = redDof;
                    Nodes[k].ReducedDofsTruss[0] = redDofTruss;
                    redDof++;
                    redDofTruss++;
                }
                if (!Nodes[k].FixTy)
                {
                    Nodes[k].ReducedDofs[1] = redDof;
                    Nodes[k].ReducedDofsTruss[1] = redDofTruss;
                    redDof++;
                    redDofTruss++;
                }
                if (!Nodes[k].FixTz)
                {
                    Nodes[k].ReducedDofs[2] = redDof;
                    Nodes[k].ReducedDofsTruss[2] = redDofTruss;
                    redDof++;
                    redDofTruss++;
                }
                if (!Nodes[k].FixRx)
                {
                    Nodes[k].ReducedDofs[3] = redDof;
                    redDof++;
                }
                if (!Nodes[k].FixRy)
                {
                    Nodes[k].ReducedDofs[4] = redDof;
                    redDof++;
                }
                if (!Nodes[k].FixRz)
                {
                    Nodes[k].ReducedDofs[5] = redDof;
                    redDof++;
                }

            }
            NFreeTranslations = this.Nodes.Count * 3 - NFixedTranslations;
            NFreeRotations = this.Nodes.Count * 3 - NFixedRotations;
        }
        public void AddLoadcase(LoadCase LC)
        {
            if(LoadCases.Contains(LC))
            {
                //throw new ArgumentException("A loadcase with the same name already exists in the Structure.");
                LoadCases[LoadCases.IndexOf(LC)].Loads.AddRange(LC.Loads);
                LoadCases[LoadCases.IndexOf(LC)].DisplacementBounds.AddRange(LC.DisplacementBounds);
                AssemblePointLoads();
                AssembleDisplacementBounds();
            }
            else
            {
                LC.Number = LoadCases.Count;
                LoadCases.Add(LC);
                AssemblePointLoads();
                AssembleDisplacementBounds();
            }
        }
        private void AssemblePointLoads()
        {
            foreach(Node N in Nodes)
            {
                N.RemoveAllLoads();
            }
            foreach (LoadCase LC in LoadCases)
            {
                foreach (PointLoad PL in LC.Loads)
                {
                    Node _N = Nodes.Find(N => N.Equals(PL.Node));
                    PL.Node = _N;
                    _N.AddPointLoad(LC, PL);
                }
            }
        }
        private void AssembleDisplacementBounds()
        {
            foreach (Node N in Nodes)
            {
                N.RemoveAllDisplacementBounds();
            }
            foreach (LoadCase LC in LoadCases)
            {
                foreach (DisplacementBound DB in LC.DisplacementBounds)
                {
                    Node _N = Nodes.Find(N => N.Equals(DB.Node));
                    DB.Node = _N;
                    _N.AddDisplacementBound(LC, DB);
                }
            }
        }
        public void AddSupport(Support S)
        {
            if(Supports.Contains(S))
            {
                Supports.Remove(S);
                Supports.Add(S);
            }
            else
            {
                Supports.Add(S);
            }

            AssembleSupports();
        }
        private void AssembleSupports()
        {
            foreach (Node N in Nodes)
            {
                N.FreeAllSupports();
            }
            foreach (Support S in Supports)
            {
                Node _N = Nodes.Find(N => N.Equals(S.Node));
                S.Node = _N;
                _N.SetSupport(S);
            }
            SetDofs();
        }
        public void SetResults(Result Results)
        {
            this.Results = Results;
        }
        public void SetLCA(ILCA LCA)
        {
            this.LCA = LCA;
        }
        public bool AllTopologyFixed()
        {
            foreach(IMember M in Members)
            {
                if (!M.TopologyFixed)
                    return false;
            }
            return true;
        }
        public List<LoadCase> GetLoadCasesFromNames(List<string> LoadCaseNames)
        {
            if (LoadCaseNames.Count == 0)
            {
                throw new ArgumentException("The list of LoadCase Names is empty");
            }
            else if (LoadCaseNames[0] == "all")
            {
                return LoadCases;
            }
            else
            {
                List<LoadCase> LCs = LoadCases.FindAll(x => LoadCaseNames.Contains(x.Name));

                if(LCs.Count == 0)
                {
                    throw new ArgumentException("No loadcases with the provided names could be found in the structure!");
                }
                return LCs;
            }
        }
        public void SortMembers1D(SortStructureMembersBy SortBy)
        {
            switch (SortBy)
            {
                case SortStructureMembersBy.Off: break;
                case SortStructureMembersBy.ForceThenLength:
                    {
                        var orderedZip = Members.OfType<IMember1D>().Zip(Enumerable.Range(0, Members.OfType<IMember1D>().Count()), (x, y) => new { x, y })
                        .OrderByDescending(pair => Math.Abs(pair.x.Nx.Values.Select(x => x.Min()).Min()))
                        .ThenByDescending(pair => Math.Abs(pair.x.Nx.Values.Select(x => x.Min()).Max()))
                        .ThenByDescending(pair => pair.x.Length)
                        .ToList();
                        Members.RemoveAll(m => m is IMember1D);
                        Members.AddRange(orderedZip.Select(pair => pair.x).ToList());
                        SortMap = orderedZip.Select(pair => pair.y).ToList();
                        break;
                    }
                case SortStructureMembersBy.LengthThenForce:
                    {
                        var orderedZip = Members.OfType<IMember1D>().Zip(Enumerable.Range(0, Members.OfType<IMember1D>().Count()), (x, y) => new { x, y })
                        .OrderByDescending(pair => pair.x.Length)
                        .ThenByDescending(pair => Math.Abs(pair.x.Nx.Values.Select(x => x.Min()).Min()))
                        .ThenByDescending(pair => Math.Abs(pair.x.Nx.Values.Select(x => x.Max()).Max()))
                        .ToList();
                        Members.RemoveAll(m => m is IMember1D);
                        Members.AddRange(orderedZip.Select(pair => pair.x).ToList());
                        SortMap = orderedZip.Select(pair => pair.y).ToList();
                        break;
                    }
            }
        }

        public Structure Clone()
        {
            var new_str = new Structure();

            new_str.Members = new List<IMember>();
            new_str.Nodes = new List<Node>();
            new_str.LoadCases = new List<LoadCase>();
            new_str.Supports = new List<Support>();
            new_str.SortMap = new List<int>();

            foreach (IMember1D IM1D in Members)
                new_str.Members.Add(IM1D.Clone());
            foreach (Node node in Nodes)
                new_str.Nodes.Add(node);
            foreach (LoadCase lc in LoadCases)
                new_str.LoadCases.Add(lc.Clone());
            foreach (Support sup in Supports)
                new_str.Supports.Add(sup.Clone());
            new_str.FixedDofs = (bool[])FixedDofs.Clone();
            new_str.NFixedTranslations = NFixedTranslations;
            new_str.NFixedRotations = NFixedRotations;
            new_str.NFreeTranslations = NFreeTranslations;
            new_str.NFreeRotations = NFreeRotations;
            new_str.SortBy = SortBy;
            foreach (int i in SortMap)
                new_str.SortMap.Add(i);
            new_str.Results = Results.Clone();
            new_str.LCA = LCA.Clone();

            return new_str;
        }
    }
}