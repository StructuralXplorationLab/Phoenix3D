using System;

namespace Phoenix3D.LinearAlgebra.Solvers
{
    
    public static class ConjugateGradient
    {
        public static Vector Solve(MatrixDense A, Vector b, int max_iter = 60, double epsilon = 0.001)
        {
            if(A.NRows != A.NColumns) { throw new ArgumentException("Matrix A is not square"); }
            if(b.Length != A.NColumns) { throw new ArgumentException("NColumns of A not equal Length of b"); }

            Vector x = new Vector(A.NColumns);
            Vector r = b - A * x;
            Vector d = r.Clone();
            double delta_new = r * r;
            double delta0 = delta_new;

            Vector q;
            double alpha;
            double delta_old;
            double beta;

            int i = 0;
            while (i < max_iter && delta_new > epsilon*epsilon*delta0)
            {
                q = A * d;
                alpha = delta_new / (d * q);
                x += alpha * d;

                if(i%10 == 0)
                {
                    r = b - A * x;
                }
                else
                {
                    r -= alpha * q;
                }
                delta_old = delta_new;
                delta_new = r * r;
                beta = delta_new / delta_old;
                d = r + beta * d;

                i++;
            }

            if(i < max_iter)
                Console.WriteLine("Conjugate Gradient Solver converged in " + i.ToString() + " iterations");
            else
                Console.WriteLine("Conjugate Gradient Solver converged did not converge within the maximum number of iterations: " + max_iter.ToString());
            return x;
        }

    }
}
