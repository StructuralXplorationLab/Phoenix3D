using System;
using System.Drawing;

using Rhino.Geometry;

namespace Phoenix3D_Components.Display
{
    public static class Profile
    {
        public static Mesh LSection(Point3d anchor, double w, double h, double t, double l, Color color)
        {
            Mesh m = new Mesh();
            m.Vertices.Add(anchor);
            m.Vertices.Add(anchor + new Vector3d(w, 0, 0));
            m.Vertices.Add(anchor + new Vector3d(w, t, 0));
            m.Vertices.Add(anchor + new Vector3d(t, t, 0));
            m.Vertices.Add(anchor + new Vector3d(t, h, 0));
            m.Vertices.Add(anchor + new Vector3d(0, h, 0));

            m.Vertices.Add(anchor + new Vector3d(0, 0, l));
            m.Vertices.Add(anchor + new Vector3d(w, 0, l));
            m.Vertices.Add(anchor + new Vector3d(w, t, l));
            m.Vertices.Add(anchor + new Vector3d(t, t, l));
            m.Vertices.Add(anchor + new Vector3d(t, h, l));
            m.Vertices.Add(anchor + new Vector3d(0, h, l));

            foreach (Point3f v in m.Vertices)
                m.VertexColors.Add(color);

            m.Faces.AddFace(1, 0, 3, 2);
            m.Faces.AddFace(0, 5, 4, 3);

            m.Faces.AddFace(6, 7, 8, 9);
            m.Faces.AddFace(6, 9, 10, 11);

            m.Faces.AddFace(0, 1, 7, 6);
            m.Faces.AddFace(1, 2, 8, 7);
            m.Faces.AddFace(2, 3, 9, 8);
            m.Faces.AddFace(3, 4, 10, 9);
            m.Faces.AddFace(4, 5, 11, 10);
            m.Faces.AddFace(5, 0, 6, 11);

            return m;
        }

        public static Mesh CircularSection(Point3d anchor, double r, double l, Color color)
        {
            var m = new Mesh();
            int n = 8;

            for (int i = 0; i < n; ++i)
            {
                m.Vertices.Add(anchor + new Vector3d(r * Math.Sin(i * 2 * Math.PI / n), r * Math.Cos(i * 2 * Math.PI / n), 0));
            }

            for (int i = 0; i < n; ++i)
            {
                m.Vertices.Add(anchor + new Vector3d(r * Math.Sin(i * 2 * Math.PI / n), r * Math.Cos(i * 2 * Math.PI / n), l));
            }

            foreach (Point3f v in m.Vertices)
                m.VertexColors.Add(color);

            m.Faces.AddFace(0, 1, 9, 8);
            m.Faces.AddFace(1, 2, 10, 9);
            m.Faces.AddFace(2, 3, 11, 10);
            m.Faces.AddFace(3, 4, 12, 11);
            m.Faces.AddFace(4, 5, 13, 12);
            m.Faces.AddFace(5, 6, 14, 13);
            m.Faces.AddFace(6, 7, 15, 14);
            m.Faces.AddFace(7, 0, 8, 15);

            m.Faces.AddFace(0, 1, 2, 3);
            m.Faces.AddFace(0, 3, 4, 7);
            m.Faces.AddFace(7, 4, 5, 6);
            m.Faces.AddFace(0 + n, 1 + n, 2 + n, 3 + n);
            m.Faces.AddFace(0 + n, 3 + n, 4 + n, 7 + n);
            m.Faces.AddFace(7 + n, 4 + n, 5 + n, 6 + n);

            return m;
        }

        public static Mesh CircularHollowSection(Point3d anchor, double r1, double r2, double l, Color color)
        {
            var m = new Mesh();
            int n = 8;
            int c = 2 * n;

            for (int i = 0; i < n; ++i)
            {
                m.Vertices.Add(anchor + new Vector3d(r1 * Math.Sin(i * 2 * Math.PI / n), r1 * Math.Cos(i * 2 * Math.PI / n), 0));
            }

            for (int i = 0; i < n; ++i)
            {
                m.Vertices.Add(anchor + new Vector3d(r1 * Math.Sin(i * 2 * Math.PI / n), r1 * Math.Cos(i * 2 * Math.PI / n), l));
            }

            for (int i = 0; i < n; ++i)
            {
                m.Vertices.Add(anchor + new Vector3d(r2 * Math.Sin(i * 2 * Math.PI / n), r2 * Math.Cos(i * 2 * Math.PI / n), 0));
            }

            for (int i = 0; i < n; ++i)
            {
                m.Vertices.Add(anchor + new Vector3d(r2 * Math.Sin(i * 2 * Math.PI / n), r2 * Math.Cos(i * 2 * Math.PI / n), l));
            }

            foreach (Point3f v in m.Vertices)
                m.VertexColors.Add(color);

            m.Faces.AddFace(0, 1, 9, 8);
            m.Faces.AddFace(1, 2, 10, 9);
            m.Faces.AddFace(2, 3, 11, 10);
            m.Faces.AddFace(3, 4, 12, 11);
            m.Faces.AddFace(4, 5, 13, 12);
            m.Faces.AddFace(5, 6, 14, 13);
            m.Faces.AddFace(6, 7, 15, 14);
            m.Faces.AddFace(7, 0, 8, 15);

            m.Faces.AddFace(0 + c, 1 + c, 9 + c, 8 + c);
            m.Faces.AddFace(1 + c, 2 + c, 10 + c, 9 + c);
            m.Faces.AddFace(2 + c, 3 + c, 11 + c, 10 + c);
            m.Faces.AddFace(3 + c, 4 + c, 12 + c, 11 + c);
            m.Faces.AddFace(4 + c, 5 + c, 13 + c, 12 + c);
            m.Faces.AddFace(5 + c, 6 + c, 14 + c, 13 + c);
            m.Faces.AddFace(6 + c, 7 + c, 15 + c, 14 + c);
            m.Faces.AddFace(7 + c, 0 + c, 8 + c, 15 + c);

            for (int i = 0; i < n; ++i)
            {
                m.Faces.AddFace(0 + i, 1 + i, 17 + i, 16 + i);
                m.Faces.AddFace(0 + i + n, 1 + i + n, 17 + i + n, 16 + i + n);
            }

            m.Faces.AddFace(0, 16, 23, 7);
            m.Faces.AddFace(0 + n, 16 + n, 23 + n, 7 + n);


            return m;
        }

        //public static Mesh GenericSection(Point3d anchor, )
        //{
        //    
        //}
    }
}
