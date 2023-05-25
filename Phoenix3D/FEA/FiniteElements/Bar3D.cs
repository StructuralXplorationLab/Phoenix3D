using Phoenix3D.Model;
using Phoenix3D.LinearAlgebra;

using System.Linq;

namespace Phoenix3D.FEA
{
    
    internal class Bar3D : FiniteElement
    {
        internal Bar3D(IMember1D M)
        {
            SetMemberStiffnessMatrices(M);
            SetDofs(M);
            SetRedDofs(M);
            SetGlobalElementStiffnessMatrix(M);
        }


        private readonly MatrixDense k0 = new MatrixDense(12);
        private MatrixDense T = new MatrixDense(12);
        internal override int[] Dofs { get; set; } = Enumerable.Repeat(-1, 12).ToArray();
        internal override int[] RedDofs { get; set; } = Enumerable.Repeat(-1, 12).ToArray();
        internal override MatrixDense K { get; set; }


        internal override void SetDofs(IMember1D M)
        {
            for (int i = 0; i < 6; i++)
            {
                Dofs[i] = M.From.Number * 6 + i;
                Dofs[i + 6] = M.To.Number * 6 + i;
            }
        }
        internal override void SetRedDofs(IMember1D M)
        {
            for (int i = 0; i < 3; i++)
            {
                RedDofs[i] = M.From.ReducedDofs[i];
                RedDofs[6 + i] = M.To.ReducedDofs[i];
            }
        }
        internal override void SetGlobalElementStiffnessMatrix(IMember1D Member)
        {
            K = T.Transpose() * k0 * T;
        }
        internal override Vector GetLocalDisplacements(Vector u, IMember1D Member)
        {
            MatrixDense T = new MatrixDense(2, 6);
            T[0, 0] = Member.T[0, 0];
            T[0, 1] = Member.T[0, 1];
            T[0, 2] = Member.T[0, 2];
            T[1, 3] = Member.T[0, 0];
            T[1, 4] = Member.T[0, 1];
            T[1, 5] = Member.T[0, 2];

            var uElement = new Vector(6);
            uElement[0] = u[Member.From.Number * 6 + 0];
            uElement[1] = u[Member.From.Number * 6 + 1];
            uElement[2] = u[Member.From.Number * 6 + 2];
            uElement[3] = u[Member.To.Number * 6 + 0];
            uElement[4] = u[Member.To.Number * 6 + 1];
            uElement[5] = u[Member.To.Number * 6 + 2];

            return T * uElement;
        }

        private void SetMemberStiffnessMatrices(IMember1D Member)
        {
            var TLocal = Member.T;
            T = new MatrixDense(12, 12);

            for (int i = 0; i < T.NRows / 3; i++)
            {
                for (int j = 0; j < TLocal.NRows; j++)
                {
                    T[i * 3 + j, i * 3 + 0] = TLocal[j, 0];
                    T[i * 3 + j, i * 3 + 1] = TLocal[j, 1];
                    T[i * 3 + j, i * 3 + 2] = TLocal[j, 2];
                }
            }

            // set up local k0

            // u0
            k0[0, 0] = Member.Material.E * Member.CrossSection.Area / Member.Length;
            k0[0, 6] = -Member.Material.E * Member.CrossSection.Area / Member.Length;

            // u6
            k0[6, 6] = Member.Material.E * Member.CrossSection.Area / Member.Length;
            k0[6, 0] = -Member.Material.E * Member.CrossSection.Area / Member.Length;

        }
    }
}