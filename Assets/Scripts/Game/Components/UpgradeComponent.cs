namespace Game.Components
{
    using Data.Business;
    
    public struct UpgradeComponent
    {
        public BusinessId BusinessId;
        public int Index;
        public bool IsActive;
        public float Multiplier;
    }
}