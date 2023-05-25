using System;
using System.Linq;
using System.Drawing;

using Phoenix3D.Reuse;
using Phoenix3D.Optimization;
using Phoenix3D.Model;


namespace Phoenix3D_Components.Display
{
public enum ColorStyle { ForceRedBlue = 0, ReuseNew = 1, Composite = 2, Utilization = 3 };

public static class MemberColor
{
    public static Color GetMemberColor(Structure Structure, Assignment Assignment, int i, ColorStyle ColorStyle)
    {
        Color Color = new Color();
        Tuple<ElementGroup, int>[] jn_indices = Assignment.GetAssignmentIndices();
        bool composite = jn_indices.Length > 1;

        if (ColorStyle == ColorStyle.ForceRedBlue)
        {
                if (Structure.Members[i] is IMember1D M1D)
                {
                    if (M1D.Nx.Values.Select(x => x.Min()).Min() < 0)
                        Color = ColorPalette.CCompression;
                    else
                        Color = ColorPalette.CTension;
                }
        }
        else if (ColorStyle == ColorStyle.ReuseNew)
        {
            if (jn_indices[i].Item1.Type == ElementType.Reuse)
                Color = ColorPalette.CReuseUsed;
            else if (jn_indices[i].Item1.Type == ElementType.New)
                Color = ColorPalette.CNewUsed;
        }
        else if (ColorStyle == ColorStyle.Composite && !(jn_indices.Length > 1))
        {
            if (jn_indices[i].Item1.Type == ElementType.Reuse)
                Color = ColorPalette.CReuseUsed;
            else if (jn_indices[i].Item1.Type == ElementType.New)
                Color = ColorPalette.CNewUsed;
        }
        else if (ColorStyle == ColorStyle.Composite && jn_indices.Length > 1 && jn_indices[i].Item1.Type == ElementType.Reuse)
        {
            Color = ColorPalette.BrightPastel[i % ColorPalette.BrightPastel.Count];
        }
        else if (ColorStyle == ColorStyle.Composite && jn_indices.Length > 1 && jn_indices[i].Item1.Type == ElementType.New)
        {
            Color = ColorPalette.CNewUsed;
        }
        else if (ColorStyle == ColorStyle.Utilization)
        {
                Color = Color.YellowGreen;
        }
        else
        {
            Color = Color.FromArgb(255, 255, 255);
        }
        return Color;
    }
}

}
