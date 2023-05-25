using Phoenix3D.Model;
using Phoenix3D.Model.Materials;
using Phoenix3D.Model.CrossSections;

using System;
using System.Collections.Generic;
using System.Linq;

namespace Phoenix3D.Reuse
{
    
    public class ElementGroup
    {
        public string Name { get; private set; }
        public int Number { get; private set; }
        public ElementType Type { get; private set; }
        public bool CanBeCut { get; private set; } = true;
        public ICrossSection CrossSection { get; private set; }
        public IMaterial Material { get; private set; }
        public double Length { get; private set; }
        public int NumberOfElements { get; private set; }
        public double[] RemainLengths { get; private set; }
        public double[] RemainLengthsTemp { get; private set; }
        public bool[] AlreadyCounted { get; private set; }
        public double[] Stack { get; private set; }
        public List<IMember1D>[] AssignedMembers { get; private set; }

        private int next;
        public int Next
        { 
            get 
            { 
                if(Type == ElementType.Reuse && next >= NumberOfElements)
                {
                    throw new OverflowException("next became larger than number of elements in the Element Group");
                }
                if (Type == ElementType.Reuse)
                    return next++;
                else
                    return next++;
            }
            set
            {
                next = value;
            } 
        }

        public ElementGroup(IMaterial Material, ICrossSection CrossSection, string Name = null)
        {
            this.Type = ElementType.New;
            this.Material = Material;
            this.CrossSection = CrossSection;
            this.CanBeCut = CanBeCut;
            this.Length = double.PositiveInfinity;
            this.NumberOfElements = 1;
            this.Next = 0;
            if (Name is null)
                this.Name = this.ToString();
            else
                this.Name = Name;
            RemainLengths = Enumerable.Repeat(Length, NumberOfElements).ToArray();
            RemainLengthsTemp = Enumerable.Repeat(Length, NumberOfElements).ToArray();
            AlreadyCounted = new bool[NumberOfElements];
            Stack = new double[NumberOfElements];
            AssignedMembers = new List<IMember1D>[NumberOfElements];
        }
        public ElementGroup(ElementType Type, IMaterial Material, ICrossSection CrossSection, double Length, int NumberOfElements, bool CanBeCut = true, string Name = null)
        {
            this.Name = Name;
            this.Type = Type;
            this.Material = Material;
            this.CrossSection = CrossSection;
            this.CanBeCut = CanBeCut;
            this.Next = 0;
            if (Name is null)
                this.Name = this.ToString();
            else
                this.Name = Name;

            if (Type == ElementType.Reuse)
            {
                this.Length = Length;
                this.NumberOfElements = NumberOfElements;
            }
            else if (Type == ElementType.New || Type == ElementType.Zero)
            {
                this.Length = double.MaxValue;
                this.NumberOfElements = 1;
            }

            RemainLengths = Enumerable.Repeat(Length, NumberOfElements).ToArray();
            RemainLengthsTemp = Enumerable.Repeat(Length, NumberOfElements).ToArray();
            AlreadyCounted = new bool[NumberOfElements];
            Stack = new double[NumberOfElements];
            AssignedMembers = new List<IMember1D>[NumberOfElements];
        }

        public static ElementGroup ZeroElement()
        {
            return new ElementGroup(ElementType.Zero, new EmptyMaterial(), new EmptySection(), double.MaxValue, 1, true);
        }

        public void SetNumber(int Number)
        {
            this.Number = Number;
        }
        public void ResetRemainLengths()
        {
            RemainLengths = Enumerable.Repeat(Length, NumberOfElements).ToArray();
            this.Next = 0;
        }
        public void ResetRemainLengthsTemp()
        {
            RemainLengths.CopyTo(RemainLengthsTemp, 0);
            this.Next = 0;
        }
        public void ResetAlreadyCounted()
        {
            AlreadyCounted = new bool[NumberOfElements];
            this.Next = 0;
        }
        public void ResetNext()
        {
            this.Next = 0;
        }
        public void AddAssignedMember(IMember1D M, int ElementIndex)
        {
            if (this.Type != ElementType.Reuse)
                ElementIndex = 0;

            if(AssignedMembers[ElementIndex] is null)
            {
                AssignedMembers[ElementIndex] = new List<IMember1D>();
            }

            AssignedMembers[ElementIndex].Add(M);

        }

        internal void ResetAssignedMembers()
        {
            AssignedMembers = new List<IMember1D>[NumberOfElements];
        }
        internal void ResetStack()
        {
            Stack = new double[NumberOfElements];
        }

        public override string ToString()
        {
            if (Type == ElementType.Reuse)
                return "StockElementGroup" + " Mat: " + Material.ToString() + " CS: " + CrossSection.ToString() + " L: " + Length.ToString() + " N: " + NumberOfElements.ToString();
            else if (Type == ElementType.New)
                return "NewElement" + " Mat: " + Material.ToString() + " CS: " + CrossSection.ToString();
            else if (Type == ElementType.Zero)
                return "ZeroElement";
            else
                return "No Element Type defined";
        }

        public ElementGroup Clone()
        {
            var new_eg = new ElementGroup(this.Material, this.CrossSection);

            new_eg.Number = this.Number;
            new_eg.Type = this.Type;
            new_eg.CanBeCut = this.CanBeCut;
            new_eg.CrossSection = this.CrossSection;
            new_eg.Material = this.Material;
            new_eg.Length = this.Length;
            new_eg.NumberOfElements = this.NumberOfElements;
            new_eg.RemainLengths = (double[])this.RemainLengths.Clone();
            new_eg.RemainLengthsTemp = (double[])this.RemainLengthsTemp.Clone();
            new_eg.AlreadyCounted = (bool[])this.AlreadyCounted.Clone();
            new_eg.Stack = (double[])this.Stack.Clone();
            new_eg.ResetNext();
            new_eg.next = this.next;
            new_eg.Next = this.next + 1;

            return new_eg;
        }

    }

    public enum ElementType
    {
        Zero = 0, Reuse = 1, New = 2
    }
}
