using Phoenix3D.Model;
using Phoenix3D.Reuse;

using System;


namespace Phoenix3D.LCA
{
    
    public class GHGFrontiers : ILCA
    {
        public double EI_Deconstruction { get; private set; } = 0.337;
        public double EI_Demolition { get; private set; } = 0.050;
        public double EI_NewSteelProduction { get; private set; } = 0.734;
        public double EI_Assembly { get; private set; } = 0.110;
        public double EI_Transport { get; private set; } = 1.1e-4; // per kg * km
        public double EI_Transport_Stock { get; private set; } = 150 * 1.1e-4;
        public double EI_Transport_NewSteel { get; private set; } = 10 * 1.1e-4;
        public double EI_Transport_Structure { get; private set; } = 10 * 1.1e-4;
        public double EI_Transport_Waste { get; private set; } = 10 * 1.1e-4;
        public double EI_Transport_Recycling { get; private set; } = 10 * 1.1e-4;

        public GHGFrontiers() : this(150, 10, 10, 10, 10) { }
        public GHGFrontiers(double TransportDistance_Stock, double TransportDistance_NewSteel, double TransportDistance_Structure, double TransportDistance_Waste, double TransportDistance_Recycling)
        { 
            this.EI_Transport_Stock = TransportDistance_Stock* EI_Transport;
            this.EI_Transport_NewSteel = TransportDistance_NewSteel* EI_Transport;
            this.EI_Transport_Structure = TransportDistance_Structure * EI_Transport;
            this.EI_Transport_Waste = TransportDistance_Waste * EI_Transport;
        }
        public GHGFrontiers(double EI_Deconstruction, double EI_Demolition, double EI_NewSteelProduction, double EI_Assembly, double EI_Transport, double TransportDistance_Stock, double TransportDistance_NewSteel, double TransportDistance_Structure, double TransportDistance_Waste, double TransportDistance_Recycling)
        {
            this.EI_NewSteelProduction = EI_NewSteelProduction;
            this.EI_Deconstruction = EI_Deconstruction;
            this.EI_Demolition = EI_Demolition;
            this.EI_Assembly = EI_Assembly;
            this.EI_Transport = EI_Transport;
            this.EI_Transport_Stock = TransportDistance_Stock * EI_Transport;
            this.EI_Transport_NewSteel = TransportDistance_NewSteel * EI_Transport;
            this.EI_Transport_Structure = TransportDistance_Structure * EI_Transport;
            this.EI_Transport_Waste = TransportDistance_Waste * EI_Transport;
            this.EI_Transport_Recycling = TransportDistance_Recycling * EI_Transport;
        }

        public double ReturnMemberImpact(IMember1D Member)
        {
            return Member.Length * Member.CrossSection.Area * Member.Material.Density * (this.EI_Demolition + this.EI_Transport_Recycling + this.EI_NewSteelProduction + this.EI_Assembly + this.EI_Transport_NewSteel + this.EI_Transport_Structure);
        }
        public double ReturnStockElementImpact(ElementGroup EG)
        {
            if (EG.Type == ElementType.Reuse)
                return EG.Material.Density * EG.CrossSection.Area * (EG.Length * (this.EI_Deconstruction + this.EI_Transport_Stock + this.EI_Transport_Waste));
            else
                return 0;
        }

        public double ReturnElementMemberImpact(ElementGroup EG, bool AlreadyCounted, IMember1D Member)
        {
            if (EG.Type == ElementType.Reuse)
                if (AlreadyCounted)
                    return EG.Material.Density * EG.CrossSection.Area * (Member.Length * (this.EI_Assembly + this.EI_Transport_Structure - EI_Transport_Waste));
                else
                    return EG.Material.Density * EG.CrossSection.Area * (EG.Length * (this.EI_Deconstruction + this.EI_Transport_Stock + this.EI_Transport_Waste) + Member.Length * (this.EI_Assembly + this.EI_Transport_Structure - EI_Transport_Waste));
            else if (EG.Type == ElementType.New)
                return (EG.Material.Density * EG.CrossSection.Area * Member.Length) * (this.EI_Demolition + this.EI_Transport_Recycling + this.EI_NewSteelProduction + this.EI_Assembly + this.EI_Transport_NewSteel + this.EI_Transport_Structure);
            else
                return 0;
        }

        public double ReturnTotalImpact(Structure Structure, out double MaxMemberImpact)
        {
            double TotalImpact = 0;
            MaxMemberImpact = 0;
            foreach (IMember1D M in Structure.Members)
            {
                double MemberImpact = 0;
                for (int i = 0; i < M.Assignment.ElementGroups.Count; i++)
                {
                    ElementGroup EG = M.Assignment.ElementGroups[i];
                    int n = M.Assignment.ElementIndices[i];
                    
                    TotalImpact += ReturnElementMemberImpact(EG, EG.AlreadyCounted[n], M);
                    MemberImpact += ReturnElementMemberImpact(EG, EG.AlreadyCounted[n], M);
                    EG.AlreadyCounted[n] = true;
                }
                MaxMemberImpact = Math.Max(MaxMemberImpact, MemberImpact);
            }
            foreach (IMember1D M in Structure.Members)
            {
                foreach(ElementGroup EG in M.Assignment.ElementGroups)
                {
                    EG.ResetAlreadyCounted();
                }
            }
            return TotalImpact;
        }
        public ILCA Clone()
        {
            var new_ghgf = new GHGFrontiers();
            new_ghgf.EI_Deconstruction = EI_Deconstruction;
            new_ghgf.EI_Demolition = EI_Demolition;
            new_ghgf.EI_NewSteelProduction = EI_NewSteelProduction;
            new_ghgf.EI_Assembly = EI_Assembly;
            new_ghgf.EI_Transport_Stock = EI_Transport_Stock;
            new_ghgf.EI_Transport_NewSteel = EI_Transport_NewSteel;
            new_ghgf.EI_Transport_Structure = EI_Transport_Structure;

            return new_ghgf;
        }
    }
}
