using System.Collections.Generic;
using System.Linq;
using Game.Data.Business;
using Game.Data.Localization;
using Game.Data.Settings;
using Game.Data.Upgrade;
using UnityEngine;

namespace Game.Services
{
    public class ConfigService : MonoBehaviour
    {
        public static ConfigService Instance { get; private set; }
        
        [SerializeField] private BusinessesConfigsData businessesConfigsData;
        [SerializeField] private GeneralGameSettingsData generalSettingsData;
        [SerializeField] private LocalizationData localizationData;

        private Dictionary<BusinessId, BusinessConfigData> _businessConfigs;

        public void Init()
        {
            Instance = this;
            
            _businessConfigs = new Dictionary<BusinessId, BusinessConfigData>();
            foreach (var item in businessesConfigsData.Items)
                _businessConfigs[item.Id] = item.Data;
        }

        public BusinessConfigData GetBusinessConfig(BusinessId id) => _businessConfigs.GetValueOrDefault(id);
        public IReadOnlyList<BusinessId> GetAllBusinessIds() => _businessConfigs?.Keys.ToList() ?? new List<BusinessId>();
        public IReadOnlyList<UpgradeConfigData> GetUpgradeConfigs(BusinessId id) => GetBusinessConfig(id)?.Upgrades ?? new List<UpgradeConfigData>();
        
        public float GetBaseAutoSaveInterval => generalSettingsData.AutosaveInterval;
        public int GetStartBalance => generalSettingsData.StartBalance;
        
        public int GetBaseIncome(BusinessId id) => GetBusinessConfig(id)?.BaseIncome ?? 0;
        public float GetIncomeDelay(BusinessId id) => GetBusinessConfig(id)?.IncomeDelay ?? 1f;
        public int GetLevelPrice(BusinessId id, int level) => (level + 1) * GetBaseCost(id);
        private int GetBaseCost(BusinessId id) => GetBusinessConfig(id)?.BaseCost ?? 0;
        public int GetUpgradePrice(BusinessId id, int upgradeIndex)
        {
            var upgrades = GetUpgradeConfigs(id);
            return (upgradeIndex >= 0 && upgradeIndex < upgrades.Count) ? upgrades[upgradeIndex].Price : 0;
        }
        
        public string GetLocalizedText(string key)
        {
            var lang = generalSettingsData.SelectedLanguage;
            return localizationData.TryGetText(key, lang, out var txt) ? txt : key;
        }
    }
}