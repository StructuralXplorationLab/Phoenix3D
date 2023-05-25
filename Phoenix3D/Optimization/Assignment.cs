using Phoenix3D.Model;
using Phoenix3D.Reuse;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Phoenix3D.Optimization
{
    
    public class Assignment
    {
        public bool Feasible { get; private set; } = false;
        public double ObjectiveValue { get; private set; } = double.PositiveInfinity;
        public List<ElementGroup> ElementGroups { get; private set; } = new List<ElementGroup>();
        public List<int> ElementIndices { get; private set; } = new List<int>();

        public Assignment() {}
        public void SetFeasible(bool Feasible) { this.Feasible = Feasible; }
        public void SetObjectiveValue(double Obj) { this.ObjectiveValue = Obj; }
        public void AddElementAssignment(ElementGroup ElementGroup, int ElementIndex)
        {
            ElementGroups.Add(ElementGroup);
            ElementIndices.Add(ElementIndex);
        }
        public void ClearElementAssignments()
        {
            this.ElementGroups.Clear();
            this.ElementIndices.Clear();
        }
        public Tuple<ElementGroup, int>[] GetAssignmentIndices()
        {
            Tuple<ElementGroup, int>[] AssignmentIndices = new Tuple<ElementGroup, int>[ElementGroups.Count];
            for (int i = 0; i < ElementGroups.Count; i++)
                AssignmentIndices[i] = new Tuple<ElementGroup, int>(ElementGroups[i], ElementIndices[i]);
            return AssignmentIndices;
        }
        public Assignment Clone()
        {
            var new_A = new Assignment();
            new_A.ElementGroups = new List<ElementGroup>();
            new_A.ElementIndices = new List<int>();


            for (int i = 0; i < ElementGroups.Count; ++i)
            {
                var new_eg = ElementGroups[i].Clone();
                new_A.ElementGroups.Add(new_eg);
            }
            for (int i = 0; i < ElementIndices.Count; ++i)
            {
                new_A.ElementIndices.Add(ElementIndices[i]);
            }

            new_A.Feasible = Feasible;
            new_A.ObjectiveValue = ObjectiveValue;

            return new_A;
        }
    }
}
