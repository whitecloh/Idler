using System.Collections.Generic;
using Editor_Custom;
using Game.Data.Upgrade;
using UnityEngine;

namespace Game.Data.Business
{
    [CreateAssetMenu(menuName = "IdleClicker/BusinessConfig", fileName = "BusinessConfigData")]
    public class BusinessConfigData : ScriptableObject
    {
        [LocalizationListPopupField]
        [SerializeField] private string localizationKey = string.Empty;
        
        [InlineScriptableObject]
        [SerializeField]
        private List<UpgradeConfigData> upgrades = new();
        
        [SerializeField] private int baseCost;
        [SerializeField] private int baseIncome;
        [SerializeField] private float incomeDelay;
        
        public string LocalizationKey => localizationKey;
        
        public IReadOnlyList<UpgradeConfigData> Upgrades => upgrades;
        
        public int BaseCost => baseCost;
        public int BaseIncome => baseIncome;
        public float IncomeDelay => incomeDelay;
    }
}