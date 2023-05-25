using Phoenix3D.Model;
using Phoenix3D.Reuse;


namespace Phoenix3D.LCA
{
    public interface ILCA
    {
        double ReturnTotalImpact(Structure Structure, out double MaxMemberImpact);
        double ReturnElementMemberImpact(ElementGroup EG, bool AlreadyCounted, IMember1D Member);
        double ReturnStockElementImpact(ElementGroup EG);
        double ReturnMemberImpact(IMember1D Member);

        ILCA Clone();
    }
}
