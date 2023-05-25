using Phoenix3D.LinearAlgebra;
using Phoenix3D.Model;

namespace Phoenix3D.FEA
{
    
    public abstract class FEAMethod
    {
        public Vector u { get; internal set; }
        internal FEAMethod() { }
        internal abstract void Solve(Structure structure, LoadCase LC, out Vector u, FEAOptions Options);
        public abstract MatrixDense GetK(Structure Structure);
        public abstract Vector Getu(Structure Structure, LoadCase LC);
        public abstract Vector Getf(Structure Structure, LoadCase LC);
    }
}
