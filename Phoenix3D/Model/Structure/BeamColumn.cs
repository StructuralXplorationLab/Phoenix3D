using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cardinal.LinearAlgebra;
using Cardinal.Model.Sections;
using Cardinal.Model.Materials;

namespace Cardinal.Model
{
    public class BeamColumn : IMember1D
    {
        public BeamColumn(Node a, Node b)
        {
            From = a;
            To = b;
        }


        public int Number { get; set; }
        public IMaterial Material { get; set; }
        public ICrossSection CrossSection { get; set; }
        public Node From { get; set; }
        public Node To { get; set; }
        public double Length { get; set; }
        public Vector Direction { get; set; } = new Vector(3);
        public Vector Normal { get; set; } = Vector.UnitZ();
        public MatrixDense T { get; set; } = new MatrixDense(3, 1.0);
        public List<double> NormalForces { get; set; }
        public (double, double) Buffer { get; set; }
        public BucklingType BucklingType { get; set; }
        public double BucklingLength { get; set; }
        public int MinCompound { get; set; }
        public int MaxCompound { get; set; }
        public double LBArea { get; set; } = 0;
        public double UBArea { get; set; } = double.PositiveInfinity;
        public bool TopologyFixed { get; set; } = false;
        private bool NormalUserdefined { get; set; } = false;


        public void SetNumber(int MemberNumber)
        {
            Number = MemberNumber;
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

            if(Vector.VectorAngle(Direction, Normal) < 1e-4)
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
            if (Normal[0] == 0 && Normal[1] == 0 && Normal[2] == 0)
            {
                throw new ArgumentException("The defined Member Normal has no direction [0,0,0]");
            }
            this.Normal = Normal.GetUnitizedVector();
        }
        private void SetT()
        {
            if (Vector.VectorAngle(Direction, Normal) < 1e-4)
            {
                throw new ArgumentException($"Member Direction and provided Normal Vector {Normal[0]}, {Normal[1]}, {Normal[2]} coincide");
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
        public override bool Equals(object obj)
        {
            if ((obj == null) || !this.GetType().Equals(obj.GetType()))
            {
                return false;
            }
            else
            {
                BeamColumn p = (BeamColumn)obj;

                if (p.From == From && p.To == To)
                    return true;
                else
                    return false;
            }
        }
        public override int GetHashCode()
        {
            return From.Number + To.Number;
        }


        public static bool operator ==(BeamColumn a, BeamColumn b) => a.Equals(b);
        public static bool operator !=(BeamColumn a, BeamColumn b) => !a.Equals(b);

    }
}
