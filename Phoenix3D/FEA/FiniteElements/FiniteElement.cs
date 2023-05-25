using Phoenix3D.LinearAlgebra;
using Phoenix3D.Model;

namespace Phoenix3D.FEA
{
    internal abstract class FiniteElement
    {
        internal abstract int[] Dofs { get; set; }
        internal abstract int[] RedDofs { get; set; }
        internal abstract MatrixDense K { get; set; }


        internal abstract void SetDofs(IMember1D M);
        internal abstract void SetRedDofs(IMember1D M);
        internal abstract void SetGlobalElementStiffnessMatrix(IMember1D Member);
        internal abstract Vector GetLocalDisplacements(Vector u, IMember1D Member);
    }
}
