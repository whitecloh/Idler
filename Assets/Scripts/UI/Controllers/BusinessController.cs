using System.Collections.Generic;
using Game.Data.Business;
using Game.Data.Upgrade;
using Game.Services;
using UI.Panels;
using UnityEngine;

namespace UI.Controllers
{
    public class BusinessController : MonoBehaviour
    {
        [SerializeField] private RectTransform businessPanelsContainer;
        [SerializeField] private BusinessPanelView businessPanelPrefab;

        private readonly Dictionary<BusinessId, BusinessPanelView> _businessPanels = new();

        public void Init()
        {
            foreach (var kvp in _businessPanels)
                Destroy(kvp.Value.gameObject);
            _businessPanels.Clear();

            var configService = ConfigService.Instance;
            var businessIds = configService.GetAllBusinessIds();
            foreach (var businessId in businessIds)
            {
                var config = configService.GetBusinessConfig(businessId);

                var panel = Instantiate(businessPanelPrefab, businessPanelsContainer);
                panel.Init(businessId, config.LocalizationKey, config.Upgrades);
                _businessPanels[businessId] = panel;
            }
        }

        public void UpdateBusiness(
            BusinessId id,
            int level,
            long income,
            float progress,
            bool isUnlocked,
            long buyLevelPrice,
            IReadOnlyList<UpgradeConfigData> upgrades,
            bool[] upgradesBought,
            bool canBuyLevel,
            bool[] canBuyUpgrade)
        {
            if (_businessPanels.TryGetValue(id, out var panel))
            {
                panel.UpdateData(level, income, progress, isUnlocked, buyLevelPrice, canBuyLevel, upgrades, upgradesBought, canBuyUpgrade);
            }
        }
    }
}