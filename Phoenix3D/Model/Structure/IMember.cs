using Phoenix3D.Model.Materials;


namespace Phoenix3D.Model
{
    public interface IMember
    {
        int Number { get; set; }
        int GroupNumber { get; set; }
        IMaterial Material { get; set; }
        bool TopologyFixed { get; set; }
        void SetNumber(int MemberNumber);
        void SetMaterial(IMaterial m);
    }
}
