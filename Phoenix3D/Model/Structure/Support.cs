using System.Linq;


namespace Phoenix3D.Model
{
    
    public class Support
    {
        public Node Node {get; internal set;}
        public bool[] Fix { get; private set;}
        public bool FixTx { get { return Fix[0]; } set { Fix[0] = value; } }
        public bool FixTy { get { return Fix[1]; } set { Fix[1] = value; } }
        public bool FixTz { get { return Fix[2]; } set { Fix[2] = value; } }
        public bool FixRx { get { return Fix[3]; } set { Fix[3] = value; } }
        public bool FixRy { get { return Fix[4]; } set { Fix[4] = value; } }
        public bool FixRz { get { return Fix[5]; } set { Fix[5] = value; } }

        public Support(Node N) : this(N, false, false, false, false, false, false) { }
        public Support(Node N, bool FixTX, bool FixTY, bool FixTZ, bool FixRX = false, bool FixRY = false, bool FixRZ = false)
        {
            Fix = new bool[6];
            Fix[0] = FixTX;
            Fix[1] = FixTY;
            Fix[2] = FixTZ;
            Fix[3] = FixRX;
            Fix[4] = FixRY;
            Fix[5] = FixRZ;

            Node = N;
        }

        public void ToggleTx(bool value) { FixTx = value; }
        public void ToggleTy(bool value) { FixTy = value; }
        public void ToggleTz(bool value) { FixTz = value; }
        public void ToggleRx(bool value) { FixRx = value; }
        public void ToggleRy(bool value) { FixRy = value; }
        public void ToggleRz(bool value) { FixRz = value; }

        public void FixAll()
        {
            Fix = Enumerable.Repeat(true, 6).ToArray();
        }
        public void FreeAll()
        {
            Fix = new bool[6];
        }
        public void FixTranslations()
        {
            Fix[0] = true;
            Fix[1] = true;
            Fix[2] = true;
        }
        public void FixRotations()
        {
            Fix[3] = true;
            Fix[4] = true;
            Fix[5] = true;
        }
        public void FreeTranslations()
        {
            Fix[0] = false;
            Fix[1] = false;
            Fix[2] = false;
        }
        public void FreeRotations()
        {
            Fix[3] = false;
            Fix[4] = false;
            Fix[5] = false;
        }
        public Support Clone()
        {
            return new Support(Node, FixTx, FixTy, FixTz, FixRx, FixRy, FixRz);
        }

        public override bool Equals(object obj)
        {
            if ((obj is null) || !this.GetType().Equals(obj.GetType()))
            {
                return false;
            }
            else
            {
                Support s = (Support)obj;
                return this.Node.Equals(s.Node);
            }
        }
        public override int GetHashCode()
        {
            return Node.GetHashCode();
        }
    }
}
