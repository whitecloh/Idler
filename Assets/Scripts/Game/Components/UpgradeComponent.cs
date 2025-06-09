using Game.Data.Business;

namespace Game.Components
{
    public struct UpgradeComponent
    {
        public BusinessId BusinessId;
        public int Index;
        public bool IsActive;
        public float Multiplier;
    }
}