using Phoenix3D.Model;
using Phoenix3D.Model.CrossSections;
using Phoenix3D.Optimization;
using Phoenix3D.Reuse;

using Grasshopper;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;

using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace Phoenix3D_Components.Display
{
    public static class DisplayHelper
    {
        public static DataTree<GH_Mesh> GetStructureMeshes(Structure Structure, DisplayResultsType DisplayResultsType, LoadCase LC, double LCScale)
        {
            DataTree<GH_Mesh> MeshTree = new DataTree<GH_Mesh>();

            foreach(IMember1D M in Structure.Members)
            {
                List<GH_Mesh> MeshList = GetMemberMeshes(M, Structure, DisplayResultsType, LC, LCScale);
                MeshTree.AddRange(MeshList, new GH_Path(M.Number));
            }
            foreach (IMember1D M in Structure.Members)
            {
                if (!(M.Assignment is null))
                {
                    foreach (ElementGroup EG in M.Assignment.ElementGroups)
                    {
                        EG.ResetAlreadyCounted();
                    }
                }
            }
            return MeshTree;
        }
        public static DataTree<GH_Mesh> GetStockMeshesOnly(Stock Stock, DisplayResultsType DisplayResultsType, Plane AnchorPlane = new Plane(), double dx = 1, double dy = 1)
        {
            Plane TempAnchorPlane = AnchorPlane.Clone();
            Stock.ResetNext();

            if (!TempAnchorPlane.IsValid)
            {
                TempAnchorPlane = Plane.WorldXY;
            }
            double X0 = TempAnchorPlane.OriginX;

            DataTree<GH_Mesh> MeshTree = new DataTree<GH_Mesh>();
            for (int i = 0; i < Stock.ElementGroups.Count; i++)
            {
                for (int j = 0; j < Stock.ElementGroups[i].NumberOfElements; j++)
                {
                    MeshTree.Add(GetElementMeshOnly(Stock.ElementGroups[i], TempAnchorPlane, DisplayResultsType), new GH_Path(i));
                    TempAnchorPlane.Origin += TempAnchorPlane.XAxis * dx;
                }
                TempAnchorPlane.Origin += TempAnchorPlane.YAxis * dy;
                TempAnchorPlane.OriginX = X0;
            }
            return MeshTree;
        }
        public static DataTree<GH_Mesh>[] GetStockMeshesFull(Stock Stock, Structure Structure, DisplayResultsType DisplayResultsType, LoadCase LC, Plane AnchorPlane = new Plane(), double dx = 1, double dy = 1)
        {
            Plane TempAnchorPlane = AnchorPlane.Clone();
            Stock.ResetNext();
            Stock.ResetStacks();

            if (!TempAnchorPlane.IsValid)
            {
                TempAnchorPlane = Plane.WorldXY;
            }
            double X0 = TempAnchorPlane.OriginX;
            Plane temp_Plane = TempAnchorPlane;

            DataTree<GH_Mesh> StockElements = new DataTree<GH_Mesh>();
            DataTree<GH_Mesh> StructureElements = new DataTree<GH_Mesh>();


            for (int j = 0; j < Stock.ElementGroups.Count; j++)
            {
                for (int n = 0; n < Stock.ElementGroups[j].NumberOfElements; n++)
                {
                    if (Stock.ElementGroups[j].Type == ElementType.Reuse)
                        StockElements.Add(GetElementMeshOnly(Stock.ElementGroups[j], temp_Plane, DisplayResultsType.Blank), new GH_Path(j));
                    temp_Plane.Origin += temp_Plane.XAxis * dx;
                }
                temp_Plane.Origin += temp_Plane.YAxis * dy;
                temp_Plane.OriginX = X0;
            }


            foreach(IMember1D M in Structure.Members)
            {
                for(int a = 0; a < M.Assignment.ElementGroups.Count; a++)
                {
                    ElementGroup EG = M.Assignment.ElementGroups[a];
                    int EIndex = M.Assignment.ElementIndices[a];

                    if(EG.Type is ElementType.Reuse)
                    {
                        Plane MemberPlane = TempAnchorPlane;
                        MemberPlane.Translate(MemberPlane.XAxis * EIndex * dx + MemberPlane.YAxis * EG.Number * dy + MemberPlane.ZAxis*EG.Stack[EIndex]);
                        GH_Mesh MemberMesh = GetMemberMesh(M, EG, EIndex, Structure, MemberPlane, DisplayResultsType, LC);
                        StructureElements.Add(MemberMesh, new GH_Path(EG.Number));
                        EG.Stack[EIndex] += M.Length;

                        Plane RemainPlane = MemberPlane;
                        RemainPlane.Translate(RemainPlane.ZAxis * M.Length);
                        GH_Mesh RemainMesh = GetRemainMesh(EG, EG.Length - EG.Stack[EIndex], RemainPlane, DisplayResultsType.Blank);
                        StockElements[new GH_Path(EG.Number), EIndex] = RemainMesh;
                    }
                    else if (EG.Type is ElementType.New)
                    {
                        Plane MemberPlane = TempAnchorPlane;
                        MemberPlane.Translate(new Vector3d(EG.Next * dx, EG.Number * dy, 0));
                        GH_Mesh MemberMesh = GetMemberMesh(M, EG, EIndex, Structure, MemberPlane, DisplayResultsType);
                        StructureElements.Add(MemberMesh, new GH_Path(EG.Number));
                    }
                }
            }

            Stock.ResetNext();
            Stock.ResetStacks();
            return new DataTree<GH_Mesh>[] { StockElements, StructureElements };
        }
        public static DataTree<GH_Mesh>[] GetStockMeshesUsed(Stock Stock, Structure Structure, DisplayResultsType DisplayResultsType, LoadCase LC, Plane AnchorPlane = new Plane(), double dx = 1, double dy = 1)
        {
            Plane TempAnchorPlane = AnchorPlane.Clone();
            Stock.ResetNext();

            if (!TempAnchorPlane.IsValid)
            {
                TempAnchorPlane = Plane.WorldXY;
            }
            double X0 = TempAnchorPlane.OriginX;
            Plane temp_Plane = TempAnchorPlane;

            DataTree<GH_Mesh> StockElements = new DataTree<GH_Mesh>();
            DataTree<GH_Mesh> StructureElements = new DataTree<GH_Mesh>();

            int row = 0;
            int column = 0;

            for (int j = 0; j < Stock.ElementGroups.Count; j++)
            {
                bool element_taken_in_row = false;
                ElementGroup EG = Stock.ElementGroups[j];

                Plane MemberPlane = TempAnchorPlane;
                MemberPlane.Translate(MemberPlane.YAxis * row * dy);

                for (int n = 0; n < EG.NumberOfElements; n++)
                {
                    if (EG.Type is ElementType.Reuse)
                    {
                        if(EG.AssignedMembers[n] is null)
                        {
                            continue;
                        }
                        foreach (IMember1D M in EG.AssignedMembers[n])
                        {
                            GH_Mesh MemberMesh = GetMemberMesh(M, EG, n, Structure, MemberPlane, DisplayResultsType, LC);
                            StructureElements.Add(MemberMesh, new GH_Path(EG.Number));

                            MemberPlane.Translate(MemberPlane.ZAxis * M.Length);
                            EG.Stack[n] += M.Length;
                        }
                        if (EG.AssignedMembers[n].Count > 0)
                        {
                            GH_Mesh RemainMesh = GetRemainMesh(EG, EG.Length - EG.Stack[n], MemberPlane, DisplayResultsType.Blank);
                            StockElements.Add(RemainMesh,new GH_Path(j));

                            MemberPlane.Translate(MemberPlane.ZAxis * (-EG.Stack[n]));
                            MemberPlane.Translate(MemberPlane.XAxis * dx);
                            element_taken_in_row = true;
                            column++;
                        }
                    }
                    else if(EG.Type is ElementType.New)
                    {
                        if (EG.AssignedMembers[n] is null)
                        {
                            continue;
                        }
                        foreach (IMember1D M in EG.AssignedMembers[n])
                        {
                            GH_Mesh MemberMesh = GetMemberMesh(M, EG, n, Structure, MemberPlane, DisplayResultsType, LC);
                            StructureElements.Add(MemberMesh, new GH_Path(EG.Number));
                            MemberPlane.Translate(MemberPlane.XAxis * dx);
                            column++;
                        }
                        if (EG.AssignedMembers[n].Count > 0)
                        {
                            element_taken_in_row = true;
                            column++;
                        }
                    }
                }
                MemberPlane.Translate(TempAnchorPlane.XAxis * (-dx * column));
                column = 0;
                if (element_taken_in_row)
                {
                    row++;
                }
            }

            Stock.ResetNext();
            Stock.ResetStacks();

            return new DataTree<GH_Mesh>[] { StockElements, StructureElements };
        }
        public static List<GH_Mesh> GetMemberMeshes(IMember1D M, Structure S, DisplayResultsType DisplayResultsType, LoadCase LC,  double DispScale = 0)
        {
            List<GH_Mesh> Msh = new List<GH_Mesh>();
            Vector3d N = new Vector3d(M.Normal[0], M.Normal[1], M.Normal[2]);

            Point3d PS = new Point3d(M.From.X, M.From.Y, M.From.Z);
            Point3d PE = new Point3d(M.To.X, M.To.Y, M.To.Z);

            double ProfileLength = M.Length;

            if(!(LC is null) && S.LoadCases.Contains(LC) && M.From.Displacements.ContainsKey(LC) && M.To.Displacements.ContainsKey(LC))
            {
                PS += DispScale * new Vector3d(M.From.Displacements[LC][0], M.From.Displacements[LC][1], M.From.Displacements[LC][2]);
                PE += DispScale * new Vector3d(M.To.Displacements[LC][0], M.To.Displacements[LC][1], M.To.Displacements[LC][2]);
            }

            Line L = new Line(PS,PE);
            ProfileLength = L.Length;
            Plane XZ = new Plane(L.From, L.Direction, N);
            Plane CS = new Plane(L.From, -XZ.ZAxis, XZ.YAxis);

            if (!(M.Assignment is null))
            {
                // test
                switch(M.Assignment.ElementGroups.Count)
                {
                    case 1:
                        {
                            Mesh m = GetMeshFromPolygon(M.Assignment.ElementGroups[0].CrossSection, ProfileLength);
                            ColorMemberMesh(ref m, M, M.Assignment.ElementGroups[0], M.Assignment.ElementIndices[0], S, DisplayResultsType, LC);
                            m.Transform(Transform.PlaneToPlane(Plane.WorldXY, CS));
                            m.Transform(Transform.Translation((ProfileLength - ProfileLength) / 2.0 * L.Direction / L.Direction.Length));
                            Msh.Add(new GH_Mesh(m));
                            break;
                        }

                    case 2:
                        {
                            Plane CS1 = CS;
                            CS1.Origin += new Vector3d(CS.XAxis * 0.005 / CS.XAxis.Length) + new Vector3d(CS.XAxis * M.Assignment.ElementGroups[0].CrossSection.cy / CS.XAxis.Length);

                            Plane CS2 = CS;
                            CS2.Origin -= new Vector3d(CS.XAxis * 0.005 / CS.XAxis.Length) + new Vector3d(CS.XAxis * M.Assignment.ElementGroups[0].CrossSection.cy / CS.XAxis.Length);

                            Mesh m1 = GetMeshFromPolygon(M.Assignment.ElementGroups[0].CrossSection, ProfileLength);
                            ColorMemberMesh(ref m1, M, M.Assignment.ElementGroups[0], M.Assignment.ElementIndices[0], S, DisplayResultsType, LC);
                            m1.Transform(Transform.PlaneToPlane(Plane.WorldXY, CS1 ));
                            m1.Transform(Transform.Translation((ProfileLength - ProfileLength) / 2.0 * L.Direction / L.Direction.Length));
                            Msh.Add(new GH_Mesh(m1));

                            Mesh m2 = GetMeshFromPolygon(M.Assignment.ElementGroups[1].CrossSection, ProfileLength);
                            ColorMemberMesh(ref m2, M, M.Assignment.ElementGroups[1], M.Assignment.ElementIndices[1], S, DisplayResultsType, LC);
                            m2.Transform(Transform.Mirror(new Plane(Plane.WorldYZ)));
                            m2.Transform(Transform.PlaneToPlane(Plane.WorldXY, CS2));
                            m2.Transform(Transform.Translation((ProfileLength - ProfileLength) / 2.0 * L.Direction / L.Direction.Length));
                            Msh.Add(new GH_Mesh(m2));
                            break;
                        }

                    default:
                        {
                            for (int a = 0; a < M.Assignment.ElementGroups.Count; a++)
                            {
                                Mesh m = GetMeshFromPolygon(M.Assignment.ElementGroups[a].CrossSection, ProfileLength);
                                ColorMemberMesh(ref m, M, M.Assignment.ElementGroups[a], M.Assignment.ElementIndices[a], S, DisplayResultsType, LC);
                                m.Transform(Transform.PlaneToPlane(Plane.WorldXY, CS));
                                m.Transform(Transform.Translation(new Vector3d(CS.XAxis * M.Assignment.ElementGroups[a].CrossSection.cy / CS.XAxis.Length) + new Vector3d(CS.YAxis * M.Assignment.ElementGroups[a].CrossSection.cz / CS.YAxis.Length)));
                                m.Transform(Transform.Rotation(a * Math.PI * 2 / M.Assignment.ElementGroups.Count, CS.ZAxis, CS.Origin));
                                double arad = Math.PI * 2 / (2*M.Assignment.ElementGroups.Count) + a * Math.PI * 2 / M.Assignment.ElementGroups.Count;
                                m.Transform(Transform.Translation(0.005 * Math.Cos(arad) * CS.XAxis + 0.005 * Math.Sin(arad) * CS.YAxis ));
                                m.Transform(Transform.Translation((ProfileLength - ProfileLength) / 2.0 * L.Direction / L.Direction.Length));
                                Msh.Add(new GH_Mesh(m));
                            }
                            break;
                        }

                }

            }
            else
            {
                Mesh m = GetMeshFromPolygon(M.CrossSection, ProfileLength);
                ColorMemberMesh(ref m, M, null, 0, S, DisplayResultsType, LC);
                m.Transform(Transform.PlaneToPlane(Plane.WorldXY, CS));
                //m.Transform(Transform.Translation((M.Length - ProfileLength) / 2.0 * L.Direction / L.Direction.Length));
                Msh.Add(new GH_Mesh(m));
            }

            return Msh;
        }
        public static GH_Mesh GetMemberMesh(IMember1D M, ElementGroup EG, int ElementIndex, Structure S, Plane Plane, DisplayResultsType DisplayResultsType, LoadCase LC = null)
        {
            double ProfileLength = M.Length;

            if (!(EG is null))
            {
                Mesh m = GetMeshFromPolygon(EG.CrossSection, ProfileLength);
                ColorMemberMesh(ref m, M, EG, ElementIndex, S, DisplayResultsType, LC);
                m.Transform(Transform.PlaneToPlane(Plane.WorldXY, Plane));
                return new GH_Mesh(m);
            }
            else
            {
                Mesh m = GetMeshFromPolygon(M.CrossSection, ProfileLength);
                ColorMemberMesh(ref m, M, null, 0, S, DisplayResultsType, LC);
                m.Transform(Transform.PlaneToPlane(Plane.WorldXY, Plane));
                return new GH_Mesh(m);
            }
        }
        public static GH_Mesh GetElementMeshOnly(ElementGroup EG, Plane Plane, DisplayResultsType DisplayResultsType)
        {
            Mesh m = GetMeshFromPolygon(EG.CrossSection, EG.Length);
            ColorElementMesh(ref m, EG, DisplayResultsType);
            m.Transform(Transform.PlaneToPlane(Plane.WorldXY, Plane));
            return new GH_Mesh(m);
        }
        public static GH_Mesh GetRemainMesh(ElementGroup EG, double RemainLength, Plane Plane, DisplayResultsType DisplayResultsType)
        {
            Mesh m = GetMeshFromPolygon(EG.CrossSection, RemainLength);
            ColorElementMesh(ref m, EG, DisplayResultsType);
            m.Transform(Transform.PlaneToPlane(Plane.WorldXY, Plane));
            return new GH_Mesh(m);
        }
        public static Mesh GetMeshFromPolygon(ICrossSection CroSec, double Length)
        {
            Mesh Msh = new Mesh();

            for (int i = 0; i < CroSec.Polygon.Count; i++)
            {
                Msh.Vertices.Add(CroSec.Polygon[i].Item1, CroSec.Polygon[i].Item2, 0);
            }
            for (int i = 0; i < CroSec.Polygon.Count; i++)
            {
                Msh.Vertices.Add(CroSec.Polygon[i].Item1, CroSec.Polygon[i].Item2, Length);
            }
            for (int i = 0; i < CroSec.Polygon.Count - 1; i++)
            {
                Msh.Faces.AddFace(new MeshFace(i, i+1, CroSec.Polygon.Count + i + 1, CroSec.Polygon.Count + i));
            }
            Msh.Faces.AddFace(new MeshFace(CroSec.Polygon.Count - 1, 0, CroSec.Polygon.Count, 2 * CroSec.Polygon.Count - 1));
            return Msh;
        }
        public static void ColorMemberMesh(ref Mesh Mesh, IMember1D M, ElementGroup EG, int ElementIndex, Structure S, DisplayResultsType DisplayResultsType, LoadCase LC)
        {
            Mesh.VertexColors.Clear();
            Color c = GetMemberColor(M, EG, ElementIndex, S, DisplayResultsType, LC);

            for (int i = 0; i < Mesh.Vertices.Count; i++)
            {
                Mesh.VertexColors.Add(c);
            }
        }
        public static void ColorElementMesh(ref Mesh Mesh, ElementGroup EG, DisplayResultsType DisplayResultsType)
        {
            Mesh.VertexColors.Clear();
            Color c = GetElementColor(EG, DisplayResultsType);
            for (int i = 0; i < Mesh.Vertices.Count; i++)
            {
                Mesh.VertexColors.Add(c);
            }
        }
        public static Color GetMemberColor(IMember1D M, ElementGroup EG, int ElementIndex, Structure S, DisplayResultsType DRType, LoadCase LC)
        {
            switch(DRType)
            {
                case DisplayResultsType.Blank:
                    {
                        return ColorPalette.CBlank;
                    }
                case DisplayResultsType.BarBeam:
                    {
                        if(M is Bar)
                        {
                            return Color.Aqua;
                        }
                        else
                        {
                            return Color.Khaki;
                        }
                    }
                case DisplayResultsType.Forces:
                    {
                        if(LC is null)
                        {
                            return ColorPalette.CBlank;

                        }
                        if(!S.LoadCases.Contains(LC))
                        {
                            throw new ArgumentException("LoadCase does not exist!");
                        }
                        
                        double util = Result.GetMemberUtilization(M, LC);

                        if (util == 0)
                            return Color.White;

                        if (M.Nx[LC].Min() > 0)
                        {
                            if (util > 1)
                                util = 1;
                            return ColorFromHSV(0, util, 1);
                        }
                        else
                        {
                            if (util > 1)
                                util = 1;
                            return ColorFromHSV(240, util, 1);
                        }
                    }
                case DisplayResultsType.Utilization:
                    {
                        double util = Result.GetMemberUtilization(M, LC);
                        return ColorFromHSV(120 - util * 120, 1.0, 1.0);
                    }
                case DisplayResultsType.ReuseNew:
                    {
                        if (EG is null)
                        {
                            return ColorPalette.CBlank;
                        }
                        else
                        {
                            switch (EG.Type)
                            {
                                case ElementType.Zero: return ColorPalette.CBlank;
                                case ElementType.Reuse: return ColorPalette.CReuseUsed;
                                case ElementType.New: return ColorPalette.CNewUsed;
                                default: return ColorPalette.CBlank;
                            }
                        }
                    }
                case DisplayResultsType.Mass:
                    {
                        if(!(S.Results is null))
                        {
                            if(EG is null)
                            {
                                double ratio = M.Length * M.CrossSection.Area * M.Material.Density / S.Results.MaxMemberMass;
                                return ColorFromHSV(120 - ratio * 120, 1.0, 1.0);
                            }
                            else
                            {
                                double ratio = M.Length * EG.CrossSection.Area * EG.Material.Density / S.Results.MaxMemberMass;
                                return ColorFromHSV(120 - ratio * 120, 1.0, 1.0);
                            }
                        }
                        else
                        {
                            double ratio = M.Length * M.CrossSection.Area * M.Material.Density / S.Results.MaxMemberMass;
                            return ColorFromHSV(120 - ratio * 120, 1.0, 1.0);
                        }
                    }
                case DisplayResultsType.Impact:
                    {
                        if (!(S.Results is null) && !(S.LCA is null))
                        {
                            if (EG is null)
                            {
                                double ratio = S.LCA.ReturnMemberImpact(M) / S.Results.MaxMemberImpact;
                                return ColorFromHSV(120 - ratio * 120, 1.0, 1.0);
                            }
                            else
                            {
                                double ratio = S.LCA.ReturnElementMemberImpact(EG,EG.AlreadyCounted[ElementIndex], M) / S.Results.MaxMemberImpact;
                                EG.AlreadyCounted[ElementIndex] = true;
                                return ColorFromHSV(120 - ratio * 120, 1.0, 1.0);
                            }
                        }
                        else
                        {
                            return ColorPalette.CBlank;
                        }
                    }
                default:
                    return ColorPalette.CBlank;
            }
        }
        public static Color GetElementColor(ElementGroup EG, DisplayResultsType DRType)
        {
            switch (DRType)
            {
                case DisplayResultsType.Blank:
                    {
                        return ColorPalette.CBlank;
                    }
                case DisplayResultsType.BarBeam:
                    {
                        return ColorPalette.CBlank;
                    }
                case DisplayResultsType.Forces:
                    {
                        return ColorPalette.CBlank;
                    }
                case DisplayResultsType.Utilization:
                    {
                        return ColorPalette.CBlank;
                    }
                case DisplayResultsType.ReuseNew:
                    {
                        if (EG.Type == ElementType.Reuse)
                        {
                            return ColorPalette.CReuse;
                        }
                        else if (EG.Type == ElementType.New)
                        {
                            return ColorPalette.CNew;
                        }
                        else
                        {
                            return ColorPalette.CBlank;
                        }
                    }
                case DisplayResultsType.Mass:
                    {
                        return ColorPalette.CBlank;
                    }
                case DisplayResultsType.Impact:
                    {
                        return ColorPalette.CBlank;
                    }
                default:
                    return ColorPalette.CBlank;
            }
        }
        public static Color ColorFromHSV(double hue, double saturation, double value)
        {
            int hi = Convert.ToInt32(Math.Floor(hue / 60)) % 6;
            double f = hue / 60 - Math.Floor(hue / 60);

            value = value * 255;
            int v = Convert.ToInt32(value);
            int p = Convert.ToInt32(value * (1 - saturation));
            int q = Convert.ToInt32(value * (1 - f * saturation));
            int t = Convert.ToInt32(value * (1 - (1 - f) * saturation));

            if (hi == 0)
                return Color.FromArgb(255, v, t, p);
            else if (hi == 1)
                return Color.FromArgb(255, q, v, p);
            else if (hi == 2)
                return Color.FromArgb(255, p, v, t);
            else if (hi == 3)
                return Color.FromArgb(255, p, q, v);
            else if (hi == 4)
                return Color.FromArgb(255, t, p, v);
            else
                return Color.FromArgb(255, v, p, q);
        }
    }

    public enum DisplayResultsType
    {
        Blank = 1,
        BarBeam = 2,
        Material = 3,
        Forces = 4,
        Utilization = 5,
        ReuseNew = 6,
        Mass = 7,
        Impact = 8
    }

    public static class ColorPalette
    {
        public static Color CBlank = Color.LightGray;
        public static Color CReuse = Color.LightGray;
        public static Color CNew = Color.DeepSkyBlue;
        public static Color CReuseUsed = Color.Black;
        public static Color CNewUsed = Color.DeepSkyBlue;
        public static Color CTension = Color.Red;
        public static Color CCompression = Color.Blue;

        public static Color NextColor(int i)
        {
            return BrightPastel[i % BrightPastel.Count];
        }

        public static List<Color> BrightPastel = new List<Color>()
        {
                System.Drawing.ColorTranslator.FromHtml("#418CF0"),
                System.Drawing.ColorTranslator.FromHtml("#FCB441"),
                System.Drawing.ColorTranslator.FromHtml("#E0400A"),
                System.Drawing.ColorTranslator.FromHtml("#056492"),
                System.Drawing.ColorTranslator.FromHtml("#1A3B69"),
                System.Drawing.ColorTranslator.FromHtml("#FFE382"),
                System.Drawing.ColorTranslator.FromHtml("#129CDD"),
                System.Drawing.ColorTranslator.FromHtml("#CA6B4B"),
                System.Drawing.ColorTranslator.FromHtml("#005CDB"),
                System.Drawing.ColorTranslator.FromHtml("#F3D288"),
                System.Drawing.ColorTranslator.FromHtml("#506381"),
                System.Drawing.ColorTranslator.FromHtml("#F1B9A8"),
                System.Drawing.ColorTranslator.FromHtml("#E0830A"),
        };
    }
}
