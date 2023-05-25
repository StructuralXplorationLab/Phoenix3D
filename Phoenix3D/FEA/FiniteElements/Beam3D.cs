using Phoenix3D.Model;
using Phoenix3D.LinearAlgebra;

using System.Linq;

namespace Phoenix3D.FEA
{
    
    class Beam3D : FiniteElement
    {
        internal Beam3D(IMember1D M)
        {
            SetMemberStiffnessMatrices(M);
            SetDofs(M);
            SetRedDofs(M);
            SetGlobalElementStiffnessMatrix(M);
        }

        private MatrixDense k0 = new MatrixDense(12);
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
            for (int i = 0; i < 6; i++)
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
            var uElement = new Vector(12);

            for (int i = 0; i < 6; i++)
            {
                uElement[i] = u[Member.From.Number * 6 + i];
                uElement[i + 6] = u[Member.To.Number * 6 + i];
            }
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
            // u1
            k0[1, 1] = 12 * Member.Material.E * Member.CrossSection.Iz / (Member.Length * Member.Length * Member.Length);
            k0[1, 5] = 6 * Member.Material.E * Member.CrossSection.Iz / (Member.Length * Member.Length);
            k0[1, 7] = -12 * Member.Material.E * Member.CrossSection.Iz / (Member.Length * Member.Length * Member.Length);
            k0[1, 11] = 6 * Member.Material.E * Member.CrossSection.Iz / (Member.Length * Member.Length);
            // u2
            k0[2, 2] = 12 * Member.Material.E * Member.CrossSection.Iy / (Member.Length * Member.Length * Member.Length);
            k0[2, 4] = -6 * Member.Material.E * Member.CrossSection.Iy / (Member.Length * Member.Length);
            k0[2, 8] = -12 * Member.Material.E * Member.CrossSection.Iy / (Member.Length * Member.Length * Member.Length);
            k0[2, 10] = -6 * Member.Material.E * Member.CrossSection.Iy / (Member.Length * Member.Length);
            // phi3
            k0[3, 3] = Member.Material.G * Member.CrossSection.It / Member.Length;
            k0[3, 9] = -Member.Material.G * Member.CrossSection.It / Member.Length;
            // phi4
            k0[4, 4] = 4 * Member.Material.E * Member.CrossSection.Iy / Member.Length;
            k0[4, 8] = 6 * Member.Material.E * Member.CrossSection.Iy / (Member.Length * Member.Length);
            k0[4, 10] = 2 * Member.Material.E * Member.CrossSection.Iy / Member.Length;
            // phi5
            k0[5, 5] = 4 * Member.Material.E * Member.CrossSection.Iz / Member.Length;
            k0[5, 7] = -6 * Member.Material.E * Member.CrossSection.Iz / (Member.Length * Member.Length);
            k0[5, 11] = 2 * Member.Material.E * Member.CrossSection.Iz / Member.Length;

            // u6
            k0[6, 6] = Member.Material.E * Member.CrossSection.Area / Member.Length;
            // u7
            k0[7, 7] = 12 * Member.Material.E * Member.CrossSection.Iz / (Member.Length * Member.Length * Member.Length);
            k0[7, 11] = -6 * Member.Material.E * Member.CrossSection.Iz / (Member.Length * Member.Length);
            // u8
            k0[8, 8] = 12 * Member.Material.E * Member.CrossSection.Iy / (Member.Length * Member.Length * Member.Length);
            k0[8, 10] = 6 * Member.Material.E * Member.CrossSection.Iy / (Member.Length * Member.Length);
            // phi9
            k0[9, 9] = Member.Material.G * Member.CrossSection.It / Member.Length;
            // phi10
            k0[10, 10] = 4 * Member.Material.E * Member.CrossSection.Iy / Member.Length;
            // phi11
            k0[11, 11] = 4 * Member.Material.E * Member.CrossSection.Iz / Member.Length;

            for (int i = 0; i < k0.NRows; i++)
            {
                for (int j = i; j < k0.NRows; j++)
                {
                    k0[j, i] = k0[i, j];
                }
            }
        }
    }
}