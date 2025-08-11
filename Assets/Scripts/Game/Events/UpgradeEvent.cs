namespace Game.Events
{
    using Data.Business;
    
    public struct UpgradeEvent
    {
        public BusinessId BusinessId;
        public int UpgradeIndex;
    }
}