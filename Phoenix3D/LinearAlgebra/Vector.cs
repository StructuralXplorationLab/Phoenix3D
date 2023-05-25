using System;
using System.Collections;

using System.Linq;

namespace Phoenix3D.LinearAlgebra
{
    
    public class Vector : IEnumerator, IEnumerable
    {
        private double[] Data = new double[0];
        private int position = -1;
        public int Length { get; private set; } = 0;
        public double this[int i]
        {
            get
            {
                return Data[i];
            }
            set
            {
                Data[i] = value;
            }
        }
        public Vector this[int[] indices]
        {
            get
            {
                Vector res = new Vector(indices.Length);
                for (int i = 0; i < indices.Length; i++)
                    res[i] = Data[indices[i]];
                return res;
            }
        }

        #region Constructors
        internal Vector() { }
        public Vector(int n)
        {
            if(n < 1) { throw new ArgumentException("Vector Length must be >= 1"); }
            Length = n;
            Data = new double[n];
        }
        public Vector(int n, double x)
        {
            if (n < 1) { throw new ArgumentException("Vector Length must be >= 1"); }
            Length = n;
            Data = Enumerable.Repeat(x, n).ToArray();
        }
        public Vector(double[] data)
        {
            Length = data.Length;
            Data = data;
        }
        public Vector(Vector V) : this(V.Data) { }
        public static Vector UnitX()
        {
            Vector X = new Vector(3);
            X[0] = 1;
            return X;
        }
        public static Vector UnitY()
        {
            Vector X = new Vector(3);
            X[1] = 1;
            return X;
        }
        public static Vector UnitZ()
        {
            Vector X = new Vector(3);
            X[2] = 1;
            return X;
        }
        #endregion

        #region IEnumerable Inheritance
        public IEnumerator GetEnumerator()
        {
            return (IEnumerator)this;
        }
        public bool MoveNext()
        {
            position++;
            return (position < Data.Length);
        }
        public void Reset()
        {
            position = 0;
        }
        public object Current
        {
            get { return Data[position]; }
        }
        internal void Add(double x)
        {
            double[] temp = new double[Length + 1];
            for (int i = 0; i < Length; i++)
                temp[i] = Data[i];
            temp[Length] = x;
            Data = temp;
            Length++;
        }
        #endregion

        #region Methods
        public void Scale(double s)
        {
            for (int i = 0; i < Length; i++)
                this[i] *= s;
        }
        public void AddScalar(double s)
        {
            for (int i = 0; i < Length; i++)
                this[i] += s;
        }
        public void AddVector(Vector B)
        {
            if (Length != B.Length) { throw new ArgumentException("Add To: Vectors not of same Length"); }
            for (int i = 0; i < Length; i++)
                this[i] += B[i];
        }
        public void PointwiseMultiply(Vector B)
        {
            if (Length != B.Length) { throw new ArgumentException("PointwiseMultiply by To: Vectors not of same Length"); }
            for (int i = 0; i < Length; i++)
                this[i] *= B[i];
        }
        public void Unitize()
        {
            double Norm = this.Norm();
            for (int i = 0; i < Length; i++)
                Data[i] /= Norm;
        }
        public Vector GetUnitizedVector()
        {
            Vector u = new Vector(this);
            u.Unitize();
            return u;
        }
        public double Norm()
        {
            double Norm = 0;
            for (int i = 0; i < Length; i++)
                Norm += Data[i] * Data[i];
            return Math.Sqrt(Norm);
        }
        public static Vector CrossProduct(Vector A, Vector B)
        {
            if(A.Length != 3 || B.Length != 3) { throw new ArgumentException("Cross Product Vectors are not of size 3x1"); }
            Vector C = new Vector(3);
            C[0] = A[1] * B[2] - A[2] * B[1];
            C[1] = A[2] * B[0] - A[0] * B[2];
            C[2] = A[0] * B[1] - A[1] * B[0];
            return C;
        }
        public static double DotProduct(Vector A, Vector B)
        {
            if(A.Length != B.Length) { throw new ArgumentException("Dot Product: Vectors not of same length"); }
            double dot = 0;
            for (int i = 0; i < A.Length; i++)
            {
                dot += A[i] * B[i];
            }
            return dot;
        }
        public static double VectorAngle(Vector A, Vector B)
        {
            if (A.Length != B.Length) { throw new ArgumentException("Vector Angle: Vectors not of same length"); }
            return Math.Acos(Vector.DotProduct(A,B)/(A.Norm()*B.Norm()));
        }

        public double[] ToDouble()
        { 
            return Data;
        }

        public override string ToString()
        {
            if(Data.Length <= 6)
            {
                string s = "(";
                for (int i = 0; i < Data.Length; i++)
                {
                    s += Math.Round(Data[i], 3).ToString();
                    if (i < Data.Length - 1)
                        s += ",";
                }
                s += ")";
                return s;
            }
            else
            {
                return "Phoenix3D Vector of Length " + Data.Length.ToString();
            }
        }
        #endregion

        #region Operators
        public static Vector operator +(Vector A, double s)
        {
            Vector B = new Vector(A.Length);
            for (int i = 0; i < A.Length; i++)
                B[i] = A[i] + s;
            return B;
        }
        public static Vector operator +(double s, Vector A)
        {
            Vector B = new Vector(A.Length);
            for (int i = 0; i < A.Length; i++)
                B[i] = A[i] + s;
            return B;
        }
        public static Vector operator +(Vector A, Vector B)
        {
            if(A.Length != B.Length) { throw new ArgumentException("A + To: Vectors not of same Length"); }
            Vector C = new Vector(A.Length);
            for (int i = 0; i < A.Length; i++)
                C[i] = A[i] + B[i];
            return C;
        }
        public static Vector operator -(Vector A)
        {
            Vector B = new Vector(A.Length);
            for (int i = 0; i < A.Length; i++)
                B[i] = -A[i];
            return B;
        }
        public static Vector operator -(Vector A, double s)
        {
            Vector B = new Vector(A.Length);
            for (int i = 0; i < A.Length; i++)
                B[i] = A[i] - s;
            return B;
        }
        public static Vector operator -(double s, Vector A)
        {
            Vector B = new Vector(A.Length);
            for (int i = 0; i < A.Length; i++)
                B[i] = s - A[i];
            return B;
        }
        public static Vector operator -(Vector A, Vector B)
        {
            if (A.Length != B.Length) { throw new ArgumentException("A + To: Vectors not of same Length"); }
            Vector C = new Vector(A.Length);
            for (int i = 0; i < A.Length; i++)
                C[i] = A[i] - B[i];
            return C;
        }
        public static Vector operator *(Vector A, double s)
        {
            Vector B = new Vector(A.Length);
            for (int i = 0; i < A.Length; i++)
                B[i] = A[i] * s;
            return B;
        }
        public static Vector operator *(double s, Vector A)
        {
            Vector B = new Vector(A.Length);
            for (int i = 0; i < A.Length; i++)
                B[i] = A[i] * s;
            return B;
        }
        public static double operator *(Vector A, Vector B)
        {
            if (A.Length != B.Length) { throw new ArgumentException("A * B: Vectors not of same Length"); }
            double r = 0;
            for (int i = 0; i < A.Length; i++)
                r += A[i] * B[i];
            return r;
        }
        #endregion

        public Vector Clone()
        {
            Vector clone = new Vector(this.Length);
            for (int i = 0; i < this.Length; i++)
                clone[i] = this[i];
            return clone;
        }
    }
}
