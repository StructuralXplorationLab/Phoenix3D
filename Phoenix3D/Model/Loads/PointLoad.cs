using Phoenix3D.LinearAlgebra;

using System;


namespace Phoenix3D.Model
{
    
    public class PointLoad : ILoad
    {
        public int LoadcaseNumber;
        public Node Node { get; set; }

        internal Vector FM = new Vector(6);
        public double Fx { get { return FM[0]; } set { FM[0] = value; } }
        public double Fy { get { return FM[1]; } set { FM[1] = value; } }
        public double Fz { get { return FM[2]; } set { FM[2] = value; } }
        public double Mx { get { return FM[3]; } set { FM[3] = value; } }
        public double My { get { return FM[4]; } set { FM[4] = value; } }
        public double Mz { get { return FM[5]; } set { FM[5] = value; } }

        public PointLoad(Node node, double fx, double fy, double fz, double mx, double my, double mz)
        {
            Node = node;

            Fx = fx;
            Fy = fy;
            Fz = fz;

            Mx = mx;
            My = my;
            Mz = mz;
        }
        public PointLoad(Node node, double fx, double fy, double fz) : this( node, fx, fy, fz, 0, 0, 0) { }

        public override bool Equals(object obj)
        {
            if ((obj is null) || !this.GetType().Equals(obj.GetType()))
            {
                return false;
            }
            else
            {
                PointLoad s = (PointLoad)obj;
                return this.Node.Equals(s.Node);
            }
        }
        public override int GetHashCode()
        {
            return Node.GetHashCode();
        }
        public void AddPointLoad(PointLoad PL)
        {
            if(this.Node != PL.Node)
            {
                throw new ArgumentException("The provided PointLoad cannot be added to this PointLoad because they are not acting on the same node.");
            }
            this.FM += PL.FM;
        }
        public PointLoad Clone()
        {
            return new PointLoad(Node, Fx, Fy, Fz, Mx, My, Mz);
        }
        public static PointLoad operator +(PointLoad A, PointLoad B)
        {
            if (A.Node != B.Node)
                throw new ArgumentException("The PointLoads cannot be added because they are not acting on the same node.");
            PointLoad PL = new PointLoad(A.Node, A.Fx + B.Fx, A.Fy + B.Fy, A.Fz + B.Fz, A.Mx + B.Mx, A.My + B.My, A.Mz + B.Mz);
            return PL;
        }
    }
}
