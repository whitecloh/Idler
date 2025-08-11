namespace Game.Data.Upgrade
{
    using Editor_Custom;
    using UnityEngine;
    
    [CreateAssetMenu(menuName = "IdleClicker/UpgradeConfig", fileName = "UpgradeConfigData")]
    public class UpgradeConfigData : ScriptableObject
    {
        [NameKeyPopup] 
        [SerializeField] private string nameKey = string.Empty;

        [SerializeField] private int price;
        [SerializeField] private float incomeMultiplier;

        public string NameKey => nameKey;
        public int Price => price;
        public float IncomeMultiplier => incomeMultiplier;
    }
}