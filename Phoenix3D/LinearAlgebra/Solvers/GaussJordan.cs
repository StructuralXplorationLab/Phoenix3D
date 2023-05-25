using System;


namespace Phoenix3D.LinearAlgebra.Solvers
{
    
    public static class GaussJordan
    {
        public static Vector Solve(MatrixDense A, Vector b)
        {
            MatrixDense I;
            return Solve(A, b, out I);
        }
        internal static Vector Solve(MatrixDense A0, Vector b, out MatrixDense I)
        {
            MatrixDense A = A0.Clone();
            if(A.NRows != A.NColumns) { throw new ArgumentException("Matrix A is not square"); }
            if(b.Length != A.NColumns) { throw new ArgumentException("NColumns of A not equal Length of b"); }
            
            for(int i = 0; i < A.NRows; i++)
                for(int j = i+1; j < A.NRows; j++)
                {
                    double fac = -A[j, i] / A[i, i];
                    b[j] += fac * b[i];

                    for(int k = i; k < A.NRows; k++)
                    {
                        A[j, k] += fac * A[i, k];
                    }
                }
            for(int i = 0; i < A.NRows; i++)
            {
                if(A[i,i] != 1)
                {
                    double div = A[i, i]; //NaN problems
                    b[i] /= div;
                    for(int j = i; j < A.NRows; j++)
                    {
                        A[i, j] /= div; //NaN problems
                    }
                }
            }
            for(int i = A.NRows - 1; i >= 0; i--)
            {
                for(int j = i-1; j >= 0; j--)
                {
                    double fac = -A[j, i] / A[i, i];
                    b[j] += fac * b[i];
                    for(int k = A.NRows-1; k >= i-1; k--)
                    {
                        A[j, k] += fac * A[i, k];
                    }
                }
            }
            I = A;
            return b;
        }

    }
}
