using Phoenix3D.LinearAlgebra.Solvers;

using System;


namespace Phoenix3D.LinearAlgebra
{
    
    public class MatrixDense
    {
        private readonly double[,] Data;
        public int NRows { get; }
        public int NColumns { get; }
        public double this[int i, int j]
        {
            get
            {
                return Data[i, j];
            }
            set
            {
                Data[i, j] = value;
            }
        }

        #region Constructors
        public MatrixDense(int n)
        {
            if(n < 1) { throw new ArgumentException("Size of Matrix should be n >= 1"); }
            NRows = NColumns = n;
            Data = new double[n, n];
        }
        public MatrixDense(int n, double x)
        {
            if (n < 1) { throw new ArgumentException("Size of Matrix should be n >= 1"); }
            NRows = NColumns = n;
            Data = new double[n, n];
            FillDiagonal(x);
        }
        public MatrixDense(int m, int n)
        {
            if (m < 1) { throw new ArgumentException("Number of Rows m should be >= 1"); }
            if (n < 1) { throw new ArgumentException("Number of Columns n should be >= 1"); }
            NRows = m;
            NColumns = n;
            Data = new double[m, n];
        }
        public MatrixDense(int m, int n, double x)
        {
            if (m < 1) { throw new ArgumentException("Number of Rows m should be >= 1"); }
            if (n < 1) { throw new ArgumentException("Number of Columns n should be >= 1"); }
            NRows = m;
            NColumns = n;
            Data = new double[m, n];
            Fill(x);
        }
        public MatrixDense(double[,] data)
        {
            NRows = data.GetLength(0);
            NColumns = data.GetLength(1);
            this.Data = data;
        }
        public MatrixDense(MatrixDense A) : this(A.Data) { }
        #endregion

        #region Methods
        private void Fill(double x)
        {
            for(int i = 0; i < NRows; i++)
                for(int j = 0; j < NColumns; j++)
                {
                    Data[i, j] = x;
                }
        }
        private void FillDiagonal(double x)
        {
            if(NRows != NColumns) { throw new ArgumentException("Cannot create diagonal entries as Matrix is not square"); }
            for (int i = 0; i < NRows; i++)
                    Data[i, i] = x;
        }
        public MatrixDense Transpose()
        {
            MatrixDense T = new MatrixDense(NColumns, NRows);
            for (int i = 0; i < NRows; i++)
                for (int j = 0; j < NColumns; j++)
                    T[j, i] = this[i, j];
            return T;
        }
        public MatrixDense PointwiseMultiply(MatrixDense B)
        {
            if (NRows != B.NRows || NColumns != B.NColumns) { throw new ArgumentException("PointsiweMultiply by To: Matrices not of same size"); }
            MatrixDense C = new MatrixDense(NRows, NColumns);
            for (int i = 0; i < NRows; i++)
                for (int j = 0; j < NColumns; j++)
                    C[i, j] = this[i, j] * B[i, j];
            return C;
        }
        public void Scale(double s)
        {
            for (int i = 0; i < NRows; i++)
                for (int j = 0; j < NColumns; j++)
                    this[i, j] *= s;
        }
        public void AddScalar(double s)
        {
            for (int i = 0; i < NRows; i++)
                for (int j = 0; j < NColumns; j++)
                    this[i, j] += s;
        }
        public void AddMatrix(MatrixDense B)
        {
            if (NRows != B.NRows || NColumns != B.NColumns) { throw new ArgumentException("Add To: Matrices not of same size"); }
            for (int i = 0; i < NRows; i++)
                for (int j = 0; j < NColumns; j++)
                    this[i, j] += B[i, j];
        }
        public static MatrixDense BlockDiagonal(MatrixDense A, int n)
        {
            MatrixDense B = new MatrixDense(A.NRows * n, A.NColumns * n);
            for (int b = 0; b < n; b++)
                for (int i = 0; i < A.NRows; i++)
                    for (int j = 0; j < A.NColumns; j++)
                        B[b * A.NRows + i, b * A.NColumns + j] = A[i, j];
            return B;
        }
        public double[,] ToDouble()
        {
            return Data;
        }
        public MatrixDense Clone()
        {
            MatrixDense C = new MatrixDense(NRows, NColumns);
            for(int i = 0; i < NRows; i++)
                for(int j = 0; j < NColumns; j++)
                {
                    double entry = this.Data[i, j];
                    C[i, j] = entry;
                }
            return C;
        }
        /*
        public override bool Equals(Object obj)
        {
            //Check for null and compare run-time types.
            if ((obj is null) || !this.GetType().Equals(obj.GetType()))
            {
                return false;
            }
            else
            {
                for (int i = 0; i < NRows; i++)
                    for (int j = 0; j < NColumns; j++)
                        if (Data[i, j] != ((MatrixDense)obj)[i, j])
                            return false;
                return true;
            }
        }
        */
        #endregion

        #region Operators
        public static MatrixDense operator +(MatrixDense A, double s)
        {
            MatrixDense B = new MatrixDense(A.NRows, A.NColumns);
            for (int i = 0; i < A.NRows; i++)
                for (int j = 0; j < A.NColumns; j++)
                    B[i, j] = A[i, j] + s;
            return B;
        }
        public static MatrixDense operator +(double s, MatrixDense A)
        {
            MatrixDense B = new MatrixDense(A.NRows, A.NColumns);
            for (int i = 0; i < A.NRows; i++)
                for (int j = 0; j < A.NColumns; j++)
                    B[i, j] = A[i, j] + s;
            return B;
        }
        public static MatrixDense operator +(MatrixDense A, MatrixDense B)
        {
            if(A.NRows != B.NRows || A.NColumns != B.NColumns) { throw new ArgumentException("A plus To: Matrices not of same size"); }
            MatrixDense C = new MatrixDense(A.NRows, A.NColumns);
            for (int i = 0; i < A.NRows; i++)
                for (int j = 0; j < A.NColumns; j++)
                    C[i, j] = A[i, j] + B[i, j];
            return C;
        }
        public static MatrixDense operator -(MatrixDense A)
        {
            MatrixDense B = new MatrixDense(A.NRows, A.NColumns);
            for (int i = 0; i < A.NRows; i++)
                for (int j = 0; j < A.NColumns; j++)
                    B[i, j] = -A[i, j];
            return B;
        }
        public static MatrixDense operator -(MatrixDense A, MatrixDense B)
        {
            if (A.NRows != B.NRows || A.NColumns != B.NColumns) { throw new ArgumentException("A minus To: Matrices not of same size"); }
            MatrixDense C = new MatrixDense(A.NRows, A.NColumns);
            for (int i = 0; i < A.NRows; i++)
                for (int j = 0; j < A.NColumns; j++)
                    C[i, j] = A[i, j] - B[i, j];
            return C;
        }
        public static MatrixDense operator *(MatrixDense A, double s)
        {
            MatrixDense B = new MatrixDense(A.NRows, A.NColumns);
            for (int i = 0; i < A.NRows; i++)
                for (int j = 0; j < A.NColumns; j++)
                    B[i, j] = A[i, j] * s;
            return B;
        }
        public static MatrixDense operator *(double s, MatrixDense A)
        {
            MatrixDense B = new MatrixDense(A.NRows, A.NColumns);
            for (int i = 0; i < A.NRows; i++)
                for (int j = 0; j < A.NColumns; j++)
                    B[i, j] = A[i, j] * s;
            return B;
        }
        public static MatrixDense operator *(MatrixDense A, MatrixDense B)
        {
            if (A.NColumns != B.NRows) { throw new ArgumentException("A times To: Number of Columns in not equal to Number of rows in To"); }
            MatrixDense C = new MatrixDense(A.NRows, B.NColumns);
            for (int i = 0; i < A.NRows; i++)
                for (int j = 0; j < B.NColumns; j++)
                    for(int k = 0; k < B.NRows; k++)
                        C[i, j] += A[i, k] * B[k, j];
            return C;
        }
        public static Vector operator *(MatrixDense A, Vector x)
        {
            if (A.NColumns != x.Length) { throw new ArgumentException("A times To: Number of Columns is not equal to Number of rows in To"); }
            Vector b = new Vector(A.NRows);
            for (int i = 0; i < A.NRows; i++)
                for (int j = 0; j < A.NColumns; j++)
                        b[i] += A[i, j] * x[j];
            return b;
        }
        public static Vector operator *(Vector x, MatrixDense A)
        {
            if (A.NRows != x.Length) { throw new ArgumentException("A times To: Number of Columns in not equal to Number of rows in To"); }
            Vector b = new Vector(A.NColumns);
            for (int j = 0; j < A.NColumns; j++)
                for (int i = 0; i < A.NRows; i++)
                    b[j] += A[i, j] * x[i];
            return b;
        }
        public static MatrixDense operator /(MatrixDense A, double s)
        {
            MatrixDense B = new MatrixDense(A.NRows, A.NColumns);
            for (int i = 0; i < A.NRows; i++)
                for (int j = 0; j < A.NColumns; j++)
                    B[i, j] = A[i, j] / s;
            return B;
        }
        #endregion

        #region Solve
        public static Vector Solve(MatrixDense A, Vector b, MatrixSolver Solver = MatrixSolver.GaussJordan, int CGIterations = 60, double CGtolerance = 0.001)
        {
            switch (Solver)
            {
                case MatrixSolver.GaussJordan: return GaussJordan.Solve(A,b);
                case MatrixSolver.ConjugateGradient: return ConjugateGradient.Solve(A, b, CGIterations, CGtolerance);
                default: return GaussJordan.Solve(A, b);
            }
        }
        #endregion
    }
    public enum MatrixSolver { GaussJordan = 0, ConjugateGradient = 1}
}
