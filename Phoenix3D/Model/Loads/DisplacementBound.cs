using Phoenix3D.LinearAlgebra;


namespace Phoenix3D.Model
{
    
    public class DisplacementBound
    {
        public int LoadcaseNumber;
        public Node Node { get; set; }

        internal Vector LB = new Vector(6);
        internal Vector UB = new Vector(6);
        public double LBX { get { return LB[0]; } set { LB[0] = value; } }
        public double LBY { get { return LB[1]; } set { LB[1] = value; } }
        public double LBZ { get { return LB[2]; } set { LB[2] = value; } }
        public double LBRX { get { return LB[3]; } set { LB[3] = value; } }
        public double LBRY { get { return LB[4]; } set { LB[4] = value; } }
        public double LBRZ { get { return LB[5]; } set { LB[5] = value; } }

        public double UBX { get { return UB[0]; } set { UB[0] = value; } }
        public double UBY { get { return UB[1]; } set { UB[1] = value; } }
        public double UBZ { get { return UB[2]; } set { UB[2] = value; } }
        public double UBRX { get { return UB[3]; } set { UB[3] = value; } }
        public double UBRY { get { return UB[4]; } set { UB[4] = value; } }
        public double UBRZ { get { return UB[5]; } set { UB[5] = value; } }


        public DisplacementBound(Node node, double lbx, double lby, double lbz, double lbrx, double lbry, double lbrz, double ubx, double uby, double ubz, double ubrx, double ubry, double ubrz)
        {
            Node = node;
            LBX = lbx; LBY = lby; LBZ = lbz;
            LBRX = lbrx; LBRY = lbry; LBRZ = lbrz;

            UBX = ubx; UBY = uby; UBZ = ubz;
            UBRX = ubrx; UBRY = ubry; UBRZ = ubrz;
        }
        public DisplacementBound(Node node, double lbx, double lby, double lbz, double ubx, double uby, double ubz) : this( node, lbx, lby, lbz, -1E100, -1E100, -1E100, ubx, uby, ubz, 1E100, 1E100, 1E100) { }

        public override bool Equals(object obj)
        {
            if ((obj is null) || !this.GetType().Equals(obj.GetType()))
            {
                return false;
            }
            else
            {
                DisplacementBound s = (DisplacementBound)obj;
                return this.Node.Equals(s.Node);
            }
        }
        public override int GetHashCode()
        {
            return Node.GetHashCode();
        }
        public DisplacementBound Clone()
        {
            return new DisplacementBound(Node, LBX, LBY, LBZ, LBRX, LBRY, LBRZ, UBX, UBY, UBZ, UBRX, UBRY, UBRZ);
        }
    }
}
