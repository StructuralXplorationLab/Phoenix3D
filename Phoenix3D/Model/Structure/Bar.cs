using Phoenix3D.LinearAlgebra;
using Phoenix3D.Model.Materials;
using Phoenix3D.Model.CrossSections;
using Phoenix3D.Optimization;

using System;
using System.Collections.Generic;
using System.Linq;


namespace Phoenix3D.Model
{
    
    public class Bar : IMember1D
    {
        public int Number { get; set; } = -1;
        public int GroupNumber { get; set; } = -1;
        public IMaterial Material { get; set; }
        public ICrossSection CrossSection { get; set; }
        public Node From { get; set; }
        public Node To { get; set; }
        public double CosX { get; set; }
        public double CosY { get; set; }
        public double CosZ { get; set; }
        public double Length { get; set; }
        public Vector Direction { get; set; } = new Vector(3);
        public Vector Normal { get; set; } = Vector.UnitZ();
        public MatrixDense T { get; set; } = new MatrixDense(3, 1.0);
        public int MinCompound { get; set; } = 1;
        public int MaxCompound { get; set; } = 1;
        public List<string> AllowedCrossSections { get; set; } = new List<string>();
        public Dictionary<LoadCase, List<double>> Nx { get; set; } = new Dictionary<LoadCase, List<double>>();
        public (double, double) Buffer { get; set; } = (0.0, 0.0);
        public BucklingType BucklingType { get; set; } = BucklingType.Off;
        public double BucklingLength { get; set; }
        public double LBArea { get; set; } = 0;
        public double UBArea { get; set; } = double.PositiveInfinity;
        public bool TopologyFixed { get; set; } = false;
        public bool NormalUserDefined { get; set; } = false;
        public bool NormalOverwritten { get; set; } = false;
        public Assignment Assignment { get; set; } = new Assignment();

        public Bar(Node From, Node To)
        {
            this.From = From;
            this.To = To;
            SetGeometricProperties();
            SetMaterial(new Steel());
            SetCrossSection(new CircularSection());
        }

        public void SetNumber(int MemberNumber)
        {
            Number = MemberNumber;
        }
        public void SetGroupNumber(int GroupNumber)
        {
            this.GroupNumber = GroupNumber;
        }
        public void SetMaterial(IMaterial mat)
        {
            Material = mat;
        }
        public void SetCrossSection(ICrossSection sec)
        {
            CrossSection = sec;
        }
        public void SetGeometricProperties()
        {
            Direction[0] = To.X - From.X;
            Direction[1] = To.Y - From.Y;
            Direction[2] = To.Z - From.Z;

            Length = Math.Sqrt(Direction[0] * Direction[0] + Direction[1] * Direction[1] + Direction[2] * Direction[2]);

            Direction.Unitize();
            CosX = Direction[0];
            CosY = Direction[1];
            CosZ = Direction[2];

            SetT();
        }
        public void SetNormal(Vector Normal)
        {
            if (Normal.Length != 3)
            {
                throw new ArgumentException("The defined Member Normal is not a 3x1 Vector");
            }
            if (Normal.Norm() == 0)
            {
                throw new ArgumentException("The defined Member Normal has no direction [0,0,0]");
            }
            this.Normal = Normal.GetUnitizedVector();
            NormalUserDefined = true;
            SetT();
            
        }
        public void SetMinMaxCompoundSection(int MinCompound, int MaxCompound)
        {
            if(MinCompound < 1 || MaxCompound < 1)
            {
                throw new ArgumentException("Min and MaxCompound cannot be less than 1");
            }
            if(MinCompound <= MaxCompound)
            {
                this.MinCompound = MinCompound;
                this.MaxCompound = MaxCompound;
            }
            else
            {
                this.MinCompound = MaxCompound;
                this.MaxCompound = MinCompound;
            }
        }
        public void SetAllowedCrossSections(List<string> TypeList)
        {
            this.AllowedCrossSections = TypeList;
        }
        public void AddAllowedCrossSection(string CSType)
        {
            this.AllowedCrossSections.Add(CSType);
        }
        public void SetBufferLengths(double Buffer0, double Buffer1)
        {
            Buffer = (Buffer0, Buffer1);
        }
        public void SetBuckling(BucklingType BucklingType, double BucklingLength)
        {
            this.BucklingType = BucklingType;
            this.BucklingLength = BucklingLength;
        }
        internal void SetT()
        {
            if (Vector.VectorAngle(Direction, Normal) % Math.PI < 1e-4)
            {
                if (NormalUserDefined)
                    NormalOverwritten = true;

                if (!(Vector.VectorAngle(Normal, Vector.UnitZ()) % Math.PI < 1e-4))
                    Normal = Vector.UnitZ();
                else if (!(Vector.VectorAngle(Normal, Vector.UnitX()) % Math.PI < 1e-4))
                    Normal = Vector.UnitX();
                else if (!(Vector.VectorAngle(Normal, Vector.UnitY()) % Math.PI < 1e-4))
                    Normal = Vector.UnitY();
            }
            Vector x = Direction;
            Vector y = (-Vector.CrossProduct(x, Normal)).GetUnitizedVector();
            Vector z = Vector.CrossProduct(x, y).GetUnitizedVector();
            T[0, 0] = x[0];
            T[0, 1] = x[1];
            T[0, 2] = x[2];
            T[1, 0] = y[0];
            T[1, 1] = y[1];
            T[1, 2] = y[2];
            T[2, 0] = z[0];
            T[2, 1] = z[1];
            T[2, 2] = z[2];
        }
        public void SetAssignment(Assignment Assignment)
        {
            for(int a = 0; a < Assignment.ElementGroups.Count; a++)
            {
                Assignment.ElementGroups[a].AddAssignedMember(this, Assignment.ElementIndices[a]);
            }
            this.Assignment = Assignment;
        }
        public void ClearAssignment()
        {
            this.Assignment = null;
        }
        public void FixTopology(bool TopologyFixed)
        {
            this.TopologyFixed = TopologyFixed;
        }
        public void SetAreaBounds(double LBArea, double UBArea)
        {
            this.LBArea = LBArea;
            this.UBArea = UBArea;
        }
        public void ClearNormalForces()
        {
            this.Nx.Clear();
        }
        public void SetNormalForces(Dictionary<LoadCase,List<double>> NormalForces)
        {
            this.Nx = NormalForces;
        }
        public void AddNormalForce(LoadCase LC, List<double> NormalForce)
        {
            if (!Nx.ContainsKey(LC))
                this.Nx.Add(LC, NormalForce);
            else
            {
                this.Nx.Remove(LC);
                this.Nx.Add(LC, NormalForce);
            }
        }
        public List<double> GetMinMaxNormalForces()
        {
            double Min = Nx.Values.Select(x => x.Min()).Min();
            double Max = Nx.Values.Select(x => x.Max()).Max();
            return new List<double>() { Min, Max };
        }
        public override bool Equals(object obj)
        {
            if ((obj is null) || !this.GetType().Equals(obj.GetType()))
            {
                return false;
            }
            else
            {
                Bar p = (Bar)obj;

                if (p.From == From && p.To == To)
                    return true;
                else
                    return false;
            }
        }
        public override int GetHashCode()
        {
            return Number.GetHashCode();
        }
        public override string ToString()
        {
            string s = "Bar";
            s += " Nr: " + Number.ToString();
            s += " Material: " + Material.ToString();
            s += " CS: " + CrossSection.ToString();
            s += " Length: " + Length.ToString();
            s += " Normal: " + Normal.ToString();
            return s;
        }

        public static bool operator ==(Bar a, Bar b) => a.Equals(b);
        public static bool operator !=(Bar a, Bar b) => !a.Equals(b);

        public IMember1D Clone()
        {
            var new_bar = new Bar(From, To);
            new_bar.Number = Number;
            new_bar.CrossSection = CrossSection;
            new_bar.Material = Material;
            new_bar.CosX = CosX;
            new_bar.CosY = CosY;
            new_bar.CosZ = CosZ;
            new_bar.Length = Length;
            new_bar.Direction = new Vector((double[])Direction.ToDouble().Clone());
            new_bar.Normal = new Vector((double[])Normal.ToDouble().Clone());
            new_bar.T = new MatrixDense((double[,])T.ToDouble().Clone());
            new_bar.MinCompound = MinCompound;
            new_bar.MaxCompound = MaxCompound;
            new_bar.Nx = new Dictionary<LoadCase, List<double>>();
            foreach (LoadCase LC in Nx.Keys)
            {
                LoadCase newLC = LC.Clone();
                new_bar.Nx.Add(newLC, Nx[LC]);
            }
            new_bar.Buffer = (Buffer.Item1, Buffer.Item2);
            new_bar.BucklingType = BucklingType;
            new_bar.BucklingLength = BucklingLength;
            new_bar.LBArea = LBArea;
            new_bar.UBArea = UBArea;
            new_bar.TopologyFixed = TopologyFixed;
            new_bar.NormalUserDefined = NormalUserDefined;
            new_bar.NormalOverwritten = NormalOverwritten;

            new_bar.AllowedCrossSections = AllowedCrossSections;
            new_bar.GroupNumber = GroupNumber;

            if (Assignment is null)
                new_bar.Assignment = null;
            else
                new_bar.Assignment = Assignment.Clone();

            return new_bar;
        }
    }
}
