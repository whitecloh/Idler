namespace Game.Data.Business
{
    using System.Collections.Generic;
    using Editor_Custom;
    using Upgrade;
    using UnityEngine;
    
    [CreateAssetMenu(menuName = "IdleClicker/BusinessConfig", fileName = "BusinessConfigData")]
    public class BusinessConfigData : ScriptableObject
    {
        [NameKeyPopup] 
        [SerializeField] private string nameKey = string.Empty;

        [InlineScriptableObject] 
        [SerializeField] private List<UpgradeConfigData> upgrades = new();

        [SerializeField] private int baseCost;
        [SerializeField] private int baseIncome;
        [SerializeField] private float incomeDelay;

        public string NameKey => nameKey;

        public IReadOnlyList<UpgradeConfigData> Upgrades => upgrades;

        public int BaseCost => baseCost;
        public int BaseIncome => baseIncome;
        public float IncomeDelay => incomeDelay;
    }
}