using Game.Data.Business;

namespace Game.Events
{
    public struct UpgradeEvent
    {
        public BusinessId BusinessId;
        public int UpgradeIndex;
    }
}