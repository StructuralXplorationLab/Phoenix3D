using System;
using System.Collections.Generic;
using System.Linq;

using Phoenix3D.LinearAlgebra;
using Phoenix3D.Model.CrossSections;
using Phoenix3D.Model.Materials;
using Phoenix3D.Optimization;

namespace Phoenix3D.Model
{
    
    public class Beam : IMember1D
    {
        public Beam(Node a, Node b)
        {
            From = a;
            To = b;
            SetGeometricProperties();
        }


        public int Number { get; set; }
        public int GroupNumber { get; set; } = -1;
        public IMaterial Material { get; set; }
        public ICrossSection CrossSection { get; set; }
        public Node From { get; set; }
        public Node To { get; set; }
        public double Length { get; set; }
        public double CosX { get; set; }
        public double CosY { get; set; }
        public double CosZ { get; set; }
        public Vector Direction { get; set; } = new Vector(3);
        public Vector Normal { get; set; } = Vector.UnitZ();
        public MatrixDense T { get; set; } = new MatrixDense(3, 1.0);
        public Dictionary<LoadCase, List<double>> Nx { get; set; } = new Dictionary<LoadCase, List<double>>();
        public Dictionary<LoadCase, List<double>> Vy { get; set; } = new Dictionary<LoadCase, List<double>>();
        public Dictionary<LoadCase, List<double>> Vz { get; set; } = new Dictionary<LoadCase, List<double>>();
        public Dictionary<LoadCase, List<double>> My { get; set; } = new Dictionary<LoadCase, List<double>>();
        public Dictionary<LoadCase, List<double>> Mz { get; set; } = new Dictionary<LoadCase, List<double>>();
        public Dictionary<LoadCase, List<double>> Mt { get; set; } = new Dictionary<LoadCase, List<double>>();

        public (double, double) Buffer { get; set; }
        public BucklingType BucklingType { get; set; }
        public double BucklingLength { get; set; }
        public int MinCompound { get; set; }
        public int MaxCompound { get; set; }
        public List<string> AllowedCrossSections { get; set; }
        public double LBArea { get; set; } = 0;
        public double UBArea { get; set; } = double.PositiveInfinity;
        public bool TopologyFixed { get; set; } = false;
        public bool NormalUserDefined { get; set; } = false;
        public bool NormalOverwritten { get; set; } = false;
        public Assignment Assignment { get; set; } = new Assignment();


        public void SetNumber(int MemberNumber)
        {
            Number = MemberNumber;
        }
        public void SetGroupNumber(int GroupNumber)
        {
            this.GroupNumber = GroupNumber;
        }
        public void SetMaterial(IMaterial Material)
        {
            this.Material = Material;
        }
        public void SetCrossSection(ICrossSection CrossSection)
        {
            this.CrossSection = CrossSection;
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

            if (Vector.VectorAngle(Direction, Normal) < 1e-4)
            {
                Normal = new Vector(Vector.UnitX());
            }

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
            this.NormalUserDefined = true;
        }
        public List<double> GetMinMaxNormalForces()
        {
            double Min = Nx.Values.Select(x => x.Min()).Min();
            double Max = Nx.Values.Select(x => x.Max()).Max();
            return new List<double>() { Min, Max };
        }
        private void SetT()
        {
            if (Vector.VectorAngle(Direction, Normal) % Math.PI < 1e-4)
            {
                if(NormalUserDefined)
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
        public void SetMinMaxCompoundSection(int MinCompound, int MaxCompound)
        {
            if (MinCompound < 1 || MaxCompound < 1)
            {
                throw new ArgumentException("Min and MaxCompound cannot be less than 1");
            }
            if (MinCompound <= MaxCompound)
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
        public void SetAssignment(Assignment Assignment)
        {
            for (int a = 0; a < Assignment.ElementGroups.Count; a++)
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
        public void SetNormalForces(Dictionary<LoadCase, List<double>> NormalForces)
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

        public void AddInternalForces(LoadCase LC, List<double> Nx, List<double> Vy, List<double> Vz, List<double> My, List<double> Mz, List<double> Mt)
        {
            if(Nx.Count != Vy.Count || Nx.Count != Vy.Count ||Nx.Count != Vz.Count || Nx.Count != My.Count || Nx.Count != Mz.Count || Nx.Count != Mt.Count)
            {
                throw new ArgumentException("The provided lists with internal forces along the beam do not have the same length!");
            }


            if (!this.Nx.ContainsKey(LC))
            {
                this.Nx.Add(LC, Nx);
                this.Vy.Add(LC, Vy);
                this.Vz.Add(LC, Vz);
                this.My.Add(LC, My);
                this.Mz.Add(LC, Mz);
                this.Mt.Add(LC, Mt);
            }
            else
            {
                this.Nx.Remove(LC);
                this.Vy.Remove(LC);
                this.Vz.Remove(LC);
                this.My.Remove(LC);
                this.Mz.Remove(LC);
                this.Mt.Remove(LC);

                this.Nx.Add(LC, Nx);
                this.Vy.Add(LC, Vy);
                this.Vz.Add(LC, Vz);
                this.My.Add(LC, My);
                this.Mz.Add(LC, Mz);
                this.Mt.Add(LC, Mt);
            }
        }
        public void AddInternalForce(LoadCase LC, double Nx, double Vy, double Vz, double My, double Mz, double Mt)
        {
            if (!this.Nx.ContainsKey(LC))
            {
                this.Nx.Add(LC, new List<double>() { Nx });
                this.Vy.Add(LC, new List<double>() { Vy });
                this.Vz.Add(LC, new List<double>() { Vz });
                this.My.Add(LC, new List<double>() { My });
                this.Mz.Add(LC, new List<double>() { Mz });
                this.Mt.Add(LC, new List<double>() { Mt });
            }
            else
            {
                this.Nx[LC].Add(Nx);
                this.Vy[LC].Add(Vy);
                this.Vz[LC].Add(Vz);
                this.My[LC].Add(My);
                this.Mz[LC].Add(Mz);
                this.Mt[LC].Add(Mt);
            }
        }
        public void ClearInternalForces()
        {
            this.Nx.Clear();
            this.Vy.Clear();
            this.Vz.Clear();
            this.My.Clear();
            this.Mz.Clear();
            this.Mt.Clear();
        }

        public override bool Equals(object obj)
        {
            if ((obj == null) || !this.GetType().Equals(obj.GetType()))
            {
                return false;
            }
            else
            {
                Beam p = (Beam)obj;

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
        
        public static bool operator ==(Beam a, Beam b) => a.Equals(b);
        public static bool operator !=(Beam a, Beam b) => !a.Equals(b);

        public IMember1D Clone()
        {
            var new_beam = new Beam(From, To);
            new_beam.Number = Number;
            new_beam.CrossSection = CrossSection;
            new_beam.Material = Material;
            new_beam.CosX = CosX;
            new_beam.CosY = CosY;
            new_beam.CosZ = CosZ;
            new_beam.Length = Length;
            new_beam.Direction = new Vector((double[])Direction.ToDouble().Clone());
            new_beam.Normal = new Vector((double[])Normal.ToDouble().Clone());
            new_beam.T = new MatrixDense((double[,])T.ToDouble().Clone());
            new_beam.MinCompound = MinCompound;
            new_beam.MaxCompound = MaxCompound;
            new_beam.Nx = new Dictionary<LoadCase, List<double>>();
            new_beam.Vy = new Dictionary<LoadCase, List<double>>();
            new_beam.Vz = new Dictionary<LoadCase, List<double>>();
            new_beam.My = new Dictionary<LoadCase, List<double>>();
            new_beam.Mz = new Dictionary<LoadCase, List<double>>();
            new_beam.Mt = new Dictionary<LoadCase, List<double>>();
            foreach (LoadCase LC in Nx.Keys)
            {
                LoadCase newLC = LC.Clone();
                new_beam.Nx.Add(newLC, Nx[LC]);
                new_beam.Vy.Add(newLC, Vy[LC]);
                new_beam.Vz.Add(newLC, Vz[LC]);
                new_beam.My.Add(newLC, My[LC]);
                new_beam.Mz.Add(newLC, Mz[LC]);
                new_beam.Mt.Add(newLC, Mt[LC]);
            }
                
            new_beam.Buffer = (Buffer.Item1, Buffer.Item2);
            new_beam.BucklingType = BucklingType;
            new_beam.BucklingLength = BucklingLength;
            new_beam.LBArea = LBArea;
            new_beam.UBArea = UBArea;
            new_beam.TopologyFixed = TopologyFixed;
            new_beam.NormalUserDefined = NormalUserDefined;
            new_beam.NormalOverwritten = NormalOverwritten;
            new_beam.AllowedCrossSections = AllowedCrossSections;
            new_beam.GroupNumber = GroupNumber;

            if (Assignment is null)
                new_beam.Assignment = null;
            else
                new_beam.Assignment = Assignment.Clone();

            return new_beam;
        }

    }
}
