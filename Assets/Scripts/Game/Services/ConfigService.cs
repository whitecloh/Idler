namespace Game.Services
{
    using System.Collections.Generic;
    using System.Linq;
    using Data.Business;
    using Data.Names;
    using Data.Settings;
    using Data.Upgrade;
    using UnityEngine;
    
    public class ConfigService : MonoBehaviour
    {
        [SerializeField] private BusinessesConfigsData businessesConfigsData;
        [SerializeField] private GeneralGameSettingsData generalSettingsData;
        [SerializeField] private NamesCatalog namesCatalog;

        private Dictionary<BusinessId, BusinessConfigData> _businessConfigs;

        public int GetStartBalance => generalSettingsData.StartBalance;

        public void Init()
        {
            _businessConfigs = new Dictionary<BusinessId, BusinessConfigData>();
            foreach (var item in businessesConfigsData.Items)
            {
                _businessConfigs[item.Id] = item.Data;
            }
        }

        public BusinessConfigData GetBusinessConfig(BusinessId id)
        {
            return _businessConfigs.GetValueOrDefault(id);
        }

        public IReadOnlyList<BusinessId> GetAllBusinessIds()
        {
            return _businessConfigs?.Keys.ToList() ?? new List<BusinessId>();
        }

        public IReadOnlyList<UpgradeConfigData> GetUpgradeConfigs(BusinessId id)
        {
            return GetBusinessConfig(id)?.Upgrades ?? new List<UpgradeConfigData>();
        }

        public long GetBaseIncome(BusinessId id)
        {
            return GetBusinessConfig(id)?.BaseIncome ?? 0;
        }

        public float GetIncomeDelay(BusinessId id)
        {
            return GetBusinessConfig(id)?.IncomeDelay ?? 1f;
        }

        public long GetLevelPrice(BusinessId id, int level)
        {
            return (level + 1) * GetBaseCost(id);
        }

        private int GetBaseCost(BusinessId id)
        {
            return GetBusinessConfig(id)?.BaseCost ?? 0;
        }

        public long GetUpgradePrice(BusinessId id, int upgradeIndex)
        {
            var upgrades = GetUpgradeConfigs(id);
            return upgradeIndex >= 0 && upgradeIndex < upgrades.Count ? upgrades[upgradeIndex].Price : 0;
        }

        public string GetName(string key)
        {
            return namesCatalog != null ? namesCatalog.Get(key) : key;
        }
    }
}