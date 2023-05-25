using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Phoenix3D.Model;
using Phoenix3D.Reuse;
using Phoenix3D.Optimization;

namespace Phoenix3D.Optimization
{
    public class Utilization
    {
        public double Utilization1;

        public Utilization(IMember1D Member, Assignment Assignment)
        {
            Tuple<ElementGroup, int>[] jn_indices = Assignment.GetAssignmentIndices();
            double CapacityTension = 0;
            double CapacityCompression = 0;
            foreach (Tuple<ElementGroup, int> jn in jn_indices)
            {
                CapacityTension += jn.Item1.CrossSection.GetTensionResistance(jn.Item1.Material);
                CapacityCompression += jn.Item1.CrossSection.GetBucklingResistance(jn.Item1.Material, Member.BucklingType, Member.BucklingLength).Max();
            }


        }
    }
}
