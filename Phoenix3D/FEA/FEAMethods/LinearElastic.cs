using Phoenix3D.LinearAlgebra;
using Phoenix3D.Model;

using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;

namespace Phoenix3D.FEA
{
    
    internal class LinearElastic : FEAMethod
    {
        public double solvertime { get; private set; } = 0;
        internal LinearElastic() { }

        internal override void Solve(Structure structure, LoadCase LC, out Vector u, FEAOptions Options)
        {
            MatrixDense K_red = GetKred(structure);
            Vector f_red = Getfred(structure, LC);

            // SOLVING
            Stopwatch sw = new Stopwatch();
            sw.Start();
            Vector u_red = MatrixDense.Solve(K_red, f_red, Options.Solver, Options.CGIterations, Options.CGTolerance);
            solvertime = sw.ElapsedMilliseconds;
            sw.Stop();

            this.u = Extendu(structure, u_red);
            u = this.u;
            SetMemberForces(structure, LC, this.u);
            SetStructureDisplacements(structure, LC, this.u);
        }

        public override MatrixDense GetK(Structure Structure)
        {
            MatrixDense K = new MatrixDense(Structure.Nodes.Count * 6);
            foreach(IMember M in Structure.Members)
            {
                AddElementStiffness(ref K, M);
            }
            return K;
        }
        public MatrixDense GetKred(Structure Structure)
        {
            if (!Structure.Members.OfType<Beam>().Any())
            {
                return GetKredTruss(Structure);
            }
            else
            {
                return GetKredFull(Structure);
            }
        }
        private MatrixDense GetKredTruss(Structure Structure)
        {
            MatrixDense Kred = new MatrixDense(Structure.NFreeTranslations);

            foreach(Bar B in Structure.Members)
            {
                Bar3D B3D = new Bar3D(B);

                // 1,1 Quarter
                for(int d1 = 0; d1 < 3; d1++)
                {
                    for(int d2 = 0; d2 < 3; d2++)
                    {
                        if(!B.From.Fix[d1] && !B.From.Fix[d2])
                            Kred[B.From.ReducedDofsTruss[d1],B.From.ReducedDofsTruss[d2]] += B3D.K[d1, d2];
                    }
                }
                // 2,1 Quarter
                for (int d1 = 0; d1 < 3; d1++)
                {
                    for (int d2 = 0; d2 < 3; d2++)
                    {
                        if (!B.To.Fix[d1] && !B.From.Fix[d2])
                            Kred[B.To.ReducedDofsTruss[d1], B.From.ReducedDofsTruss[d2]] += B3D.K[6 + d1, d2];
                    }
                }
                // 1,2 Quarter
                for (int d1 = 0; d1 < 3; d1++)
                {
                    for (int d2 = 0; d2 < 3; d2++)
                    {
                        if (!B.From.Fix[d1] && !B.To.Fix[d2])
                            Kred[B.From.ReducedDofsTruss[d1], B.To.ReducedDofsTruss[d2]] += B3D.K[d1, 6 + d2];
                    }
                }
                // 2,2 Quarter
                for (int d1 = 0; d1 < 3; d1++)
                {
                    for (int d2 = 0; d2 < 3; d2++)
                    {
                        if (!B.To.Fix[d1] && !B.To.Fix[d2])
                            Kred[B.To.ReducedDofsTruss[d1], B.To.ReducedDofsTruss[d2]] += B3D.K[6 + d1, 6 + d2];
                    }
                }

            }

            return Kred;
        }
        private MatrixDense GetKredFull(Structure Structure)
        {
            MatrixDense Kred = new MatrixDense(Structure.NFreeTranslations);

            foreach (Bar B in Structure.Members)
            {
                Bar3D B3D = new Bar3D(B);

                // 1,1 Quarter
                for (int d1 = 0; d1 < 6; d1++)
                {
                    for (int d2 = 0; d2 < 6; d2++)
                    {
                        Kred[B.From.ReducedDofs[d1], B.From.ReducedDofs[d2]] += B3D.K[d1, d2];
                    }
                }
                // 2,1 Quarter
                for (int d1 = 0; d1 < 6; d1++)
                {
                    for (int d2 = 0; d2 < 6; d2++)
                    {
                        Kred[B.To.ReducedDofs[d1], B.From.ReducedDofs[d2]] += B3D.K[6 + d1, d2];
                    }
                }
                // 1,2 Quarter
                for (int d1 = 0; d1 < 6; d1++)
                {
                    for (int d2 = 0; d2 < 6; d2++)
                    {
                        Kred[B.From.ReducedDofs[d1], B.To.ReducedDofs[d2]] += B3D.K[d1, 6 + d2];
                    }
                }
                // 2,2 Quarter
                for (int d1 = 0; d1 < 6; d1++)
                {
                    for (int d2 = 0; d2 < 6; d2++)
                    {
                        Kred[B.To.ReducedDofs[d1], B.To.ReducedDofs[d2]] += B3D.K[6 + d1, 6 + d2];
                    }
                }

            }

            foreach (Beam B in Structure.Members)
            {
                Beam3D B3D = new Beam3D(B);

                // 1,1 Quarter
                for (int d1 = 0; d1 < 6; d1++)
                {
                    for (int d2 = 0; d2 < 6; d2++)
                    {
                        Kred[B.From.ReducedDofs[d1], B.From.ReducedDofs[d2]] += B3D.K[d1, d2];
                    }
                }
                // 2,1 Quarter
                for (int d1 = 0; d1 < 6; d1++)
                {
                    for (int d2 = 0; d2 < 6; d2++)
                    {
                        Kred[B.To.ReducedDofs[d1], B.From.ReducedDofs[d2]] += B3D.K[6 + d1, d2];
                    }
                }
                // 1,2 Quarter
                for (int d1 = 0; d1 < 6; d1++)
                {
                    for (int d2 = 0; d2 < 6; d2++)
                    {
                        Kred[B.From.ReducedDofs[d1], B.To.ReducedDofs[d2]] += B3D.K[d1, 6 + d2];
                    }
                }
                // 2,2 Quarter
                for (int d1 = 0; d1 < 6; d1++)
                {
                    for (int d2 = 0; d2 < 6; d2++)
                    {
                        Kred[B.To.ReducedDofs[d1], B.To.ReducedDofs[d2]] += B3D.K[6 + d1, 6 + d2];
                    }
                }

            }

            return Kred;
        }
        public override Vector Getf(Structure Structure, LoadCase LC)
        {
            Vector f = new Vector(Structure.Nodes.Count * 6);

            foreach (PointLoad pl in LC.Loads)
            {
                if (!Structure.FixedDofs[pl.Node.Number * 6 + 0])
                    f[pl.Node.Number * 6 + 0] = pl.Fx;
                if (!Structure.FixedDofs[pl.Node.Number * 6 + 1])
                    f[pl.Node.Number * 6 + 1] = pl.Fy;
                if (!Structure.FixedDofs[pl.Node.Number * 6 + 2])
                    f[pl.Node.Number * 6 + 2] = pl.Fz;
                if (!Structure.FixedDofs[pl.Node.Number * 6 + 3])
                    f[pl.Node.Number * 6 + 3] = pl.Mx;
                if (!Structure.FixedDofs[pl.Node.Number * 6 + 4])
                    f[pl.Node.Number * 6 + 4] = pl.My;
                if (!Structure.FixedDofs[pl.Node.Number * 6 + 5])
                    f[pl.Node.Number * 6 + 5] = pl.Mz;
            }
            return f;
        }
        public Vector Getfred(Structure Structure, LoadCase LC)
        {
            if (!Structure.Members.OfType<Beam>().Any())
            {
                return GetfredTruss(Structure, LC);
            }
            else
            {
                return GetfredFull(Structure, LC);
            }
        }
        private Vector GetfredTruss(Structure Structure, LoadCase LC)
        {
            Vector f = new Vector(Structure.NFreeTranslations);

            foreach (PointLoad PL in LC.Loads)
            {
                if (!PL.Node.FixTx)
                    f[PL.Node.ReducedDofsTruss[0]] = PL.Fx;
                if (!PL.Node.FixTy)
                    f[PL.Node.ReducedDofsTruss[1]] = PL.Fy;
                if (!PL.Node.FixTz)
                    f[PL.Node.ReducedDofsTruss[2]] = PL.Fz;
            }
            return f;
        }
        private Vector GetfredFull(Structure Structure, LoadCase LC)
        {
            Vector f = new Vector(Structure.NFreeTranslations);

            foreach (PointLoad PL in LC.Loads)
            {
                if (!PL.Node.FixTx)
                    f[PL.Node.ReducedDofs[0]] = PL.Fx;
                if (!PL.Node.FixTy)
                    f[PL.Node.ReducedDofs[1]] = PL.Fy;
                if (!PL.Node.FixTz)
                    f[PL.Node.ReducedDofs[2]] = PL.Fz;
                if (!PL.Node.FixRx)
                    f[PL.Node.ReducedDofs[4]] = PL.Mx;
                if (!PL.Node.FixRy)
                    f[PL.Node.ReducedDofs[5]] = PL.My;
                if (!PL.Node.FixRz)
                    f[PL.Node.ReducedDofs[6]] = PL.Mz;
            }
            return f;
        }
        private Vector Extendu(Structure Structure, Vector ured)
        {
            Vector u = new Vector(Structure.Nodes.Count * 6);
            foreach(Node N in Structure.Nodes)
            {
                if (!Structure.Members.OfType<Beam>().Any())
                {
                    for(int d = 0; d < 3; d++)
                    {
                        if(!N.Fix[d])
                            u[N.Number * 6 + d] = ured[N.ReducedDofsTruss[d]];
                    }
                }
                else
                {
                    for (int d = 0; d < 6; d++)
                    {
                        if (!N.Fix[d])
                            u[N.Number * 6 + d] = ured[N.ReducedDofs[d]];
                    }
                }
            }
            return u;
        }
        public override Vector Getu(Structure Structure, LoadCase LC)
        {
            Vector u;
            Solve(Structure, LC, out u, new FEAOptions());
            return u;
        }
        private static void AddElementStiffness(ref MatrixDense K, IMember M)
        {
            FiniteElement FE;

            if (M is IMember1D M1D)
            {
                if (M1D is Bar)
                {
                    FE = new Bar3D(M1D);

                    for (int i = 0; i < FE.Dofs.Length; i++)
                        for (int j = 0; j < FE.Dofs.Length; j++)
                            K[FE.Dofs[i], FE.Dofs[j]] += FE.K[i, j];
                }
                else if (M1D is Beam)
                {
                    FE = new Beam3D(M1D);

                    for (int i = 0; i < FE.Dofs.Length; i++)
                        for (int j = 0; j < FE.Dofs.Length; j++)
                            K[FE.Dofs[i], FE.Dofs[j]] += FE.K[i, j];
                }

            }
        }
        private static void SetMemberForces(Structure str, LoadCase LC, Vector u)
        {
            for (int i = 0; i < str.Members.Count; i++)
            {
                FiniteElement FE;

                if (str.Members[i] is IMember1D M1D)
                {
                    foreach (var node in str.Nodes)
                    {
                        if (node == M1D.From)
                            M1D.From = node;
                        if (node == M1D.To)
                            M1D.To = node;
                    }

                    if (M1D is Bar bar)
                    {
                        FE = new Bar3D(M1D);
                        var localDisplacements = FE.GetLocalDisplacements(u, M1D);

                        var d = localDisplacements[1] - localDisplacements[0];
                        bar.AddNormalForce(LC, new List<double>() { d * (M1D.Material.E * M1D.CrossSection.Area / M1D.Length) });
                    }
                    else if (M1D is Beam)
                    {
                        FE = new Beam3D(M1D);
                        var localDisplacements = FE.GetLocalDisplacements(u, M1D);
                        var fLocal = FE.K * localDisplacements;
                    }
                }
            }
        }
        private static void SetStructureDisplacements(Structure str, LoadCase LC, Vector u)
        {
            foreach(Node N in str.Nodes)
            {
                double[] displacements = new double[6];
                displacements[0] = u[N.Number * 6 + 0];
                displacements[1] = u[N.Number * 6 + 1];
                displacements[2] = u[N.Number * 6 + 2];
                displacements[3] = u[N.Number * 6 + 3];
                displacements[4] = u[N.Number * 6 + 4];
                displacements[5] = u[N.Number * 6 + 5];
                N.AddDisplacement(LC, displacements);
            }
        }
    }
}