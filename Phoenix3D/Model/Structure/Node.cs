using System;
using System.Collections.Generic;
using System.Linq;

namespace Phoenix3D.Model
{
    public struct Node
    {
        public int Number { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }
        public bool[] Fix { get; internal set; }
        public bool FixTx { get { return Fix[0]; } set { Fix[0] = value; } }
        public bool FixTy { get { return Fix[1]; } set { Fix[1] = value; } }
        public bool FixTz { get { return Fix[2]; } set { Fix[2] = value; } }
        public bool FixRx { get { return Fix[3]; } set { Fix[3] = value; } }
        public bool FixRy { get { return Fix[4]; } set { Fix[4] = value; } }
        public bool FixRz { get { return Fix[5]; } set { Fix[5] = value; } }
        public double Tolerance { get; set; }
        public int[] ReducedDofsTruss { get; internal set; }
        public int[] ReducedDofs { get; internal set; }

        public Dictionary<IMember, double> ConnectedMembers { get; private set; }
        public Dictionary<LoadCase, PointLoad> PointLoads { get; private set; } 
        public Dictionary<LoadCase, double[]> Displacements { get; private set; }
        public Dictionary<LoadCase, DisplacementBound> DisplacementBounds { get; private set; }

        public Node(double X, double Y, double Z)
        {
            this.X = X;
            this.Y = Y;
            this.Z = Z;

            Number = -1;
            Fix = new bool[6];
            Tolerance = 0.0001;
            ReducedDofsTruss = Enumerable.Repeat(-1, 3).ToArray();
            ReducedDofs = Enumerable.Repeat(-1, 6).ToArray();
            ConnectedMembers = new Dictionary<IMember, double>();
            PointLoads = new Dictionary<LoadCase, PointLoad>();
            Displacements = new Dictionary<LoadCase, double[]>();
            DisplacementBounds = new Dictionary<LoadCase, DisplacementBound>();
        }
        public void SetSupport(Support Support)
        {
            FixTx = Support.FixTx;
            FixTy = Support.FixTy;
            FixTz = Support.FixTz;
            FixRx = Support.FixRx;
            FixRy = Support.FixRy;
            FixRz = Support.FixRz;
        }
        public void SetNumber(int nb)
        {
            Number = nb;
        }
        internal void AddPointLoad(LoadCase LC, PointLoad PL)
        {
            if (!PointLoads.ContainsKey(LC))
                PointLoads.Add(LC, PL);
            else
            {
                PointLoads[LC] += PL;
                //PointLoads.Remove(LC);
                //PointLoads.Add(LC, PL);
            }
        }
        internal void AddDisplacementBound(LoadCase LC, DisplacementBound DB)
        {
            if (!DisplacementBounds.ContainsKey(LC))
                DisplacementBounds.Add(LC, DB);
            else
            {
                DisplacementBounds.Remove(LC);
                DisplacementBounds.Add(LC, DB);
            }
        }
        internal void AddDisplacement(LoadCase LC, double[] displacements)
        {
            if (!Displacements.ContainsKey(LC))
                Displacements.Add(LC, displacements);
            else
            {
                Displacements.Remove(LC);
                Displacements.Add(LC, displacements);
            }
        }
        public void RemoveAllLoads()
        {
            this.PointLoads.Clear();
        }
        public void RemoveAllDisplacementBounds()
        {
            this.DisplacementBounds.Clear();
        }
        public void FreeAllSupports()
        {
            FixTx = false;
            FixTy = false;
            FixTz = false;
            FixRx = false;
            FixRy = false;
            FixRz = false;
        }
        public void ResetReducedDofs()
        {
            this.ReducedDofs = Enumerable.Repeat(-1, 6).ToArray();
            this.ReducedDofsTruss = Enumerable.Repeat(-1, 3).ToArray();
        }
        public List<PointLoad> GetPointLoadsFromLCNames(List<string> LoadCaseNames)
        {
            

            if (LoadCaseNames.Count == 0)
            {
                throw new ArgumentException("The list of LoadCase Names is empty");
            }
            else if (LoadCaseNames.Count == 1 && LoadCaseNames[0] == "all")
            {
                return PointLoads.Values.ToList();
            }
            else
            {
                List<PointLoad> SelectedPointLoads = new List<PointLoad>();
                foreach (string LCName in LoadCaseNames)
                {
                    PointLoad PL;
                    if (PointLoads.TryGetValue(new LoadCase(LCName), out PL))
                        SelectedPointLoads.Add(PL);
                }
                return SelectedPointLoads;
            }

        }
        public int GetNumber()
        {
            return Number;
        }
        public override bool Equals(object obj)
        {
            if ((obj is null) || !this.GetType().Equals(obj.GetType()))
            {
                return false;
            }
            else
            {
                Node p = (Node)obj;

                var deltaX = p.X - X;
                var deltaY = p.Y - Y;
                var deltaZ = p.Z - Z;

                var norm = Math.Sqrt(deltaX * deltaX + deltaY * deltaY + deltaZ * deltaZ);

                if (norm < Tolerance)
                    return true;
                else
                    return false;
            }
        }
        public override int GetHashCode()
        {
            return Number.GetHashCode();
        }
        internal void AddConnectedMember(IMember Member, double Orientation)
        {
            if (!ConnectedMembers.ContainsKey(Member))
                ConnectedMembers.Add(Member,Orientation);
        }
        public static bool operator ==(Node A, Node B) => A.Equals(B);
        public static bool operator !=(Node A, Node B) => !A.Equals(B);
        public override string ToString()
        {
            return "Node " + Number.ToString() + " (" + X.ToString() + "," + Y.ToString() + "," + Z.ToString() + ") " + "Fix ["+Convert.ToInt16(FixTx)+"," + Convert.ToInt16(FixTy) + "," + Convert.ToInt16(FixTz) + "," + Convert.ToInt16(FixRx) + "," + Convert.ToInt16(FixRy) + "," + Convert.ToInt16(FixRz)+"]";
        }
    }
}