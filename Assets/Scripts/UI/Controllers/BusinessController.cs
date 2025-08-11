using Game;

namespace UI.Controllers
{
    using System.Collections.Generic;
    using Game.Data.Business;
    using Game.Data.Upgrade;
    using Game.Services;
    using Panels;
    using UnityEngine;
    
    public sealed class BusinessController : MonoBehaviour
    {
        [SerializeField] private RectTransform businessPanelsContainer;
        [SerializeField] private BusinessPanelView businessPanelPrefab;

        private readonly Dictionary<BusinessId, BusinessPanelView> _businessPanels = new();
        
        private ConfigService _config;
        private EcsUIEventBridge _bridge;

        public void Init(ConfigService config, EcsUIEventBridge bridge)
        {
            _config = config;
            _bridge = bridge;
            
            foreach (var kvp in _businessPanels)
                Destroy(kvp.Value.gameObject);
            _businessPanels.Clear();
            
            var businessIds = _config.GetAllBusinessIds();
            foreach (var businessId in businessIds)
            {
                var configData = _config.GetBusinessConfig(businessId);

                var panel = Instantiate(businessPanelPrefab, businessPanelsContainer);
                var displayName = _config.GetName(configData.NameKey);
                panel.Init(businessId, displayName, configData.Upgrades, _config, _bridge);
                _businessPanels[businessId] = panel;
            }
        }

        public void UpdateProgress(BusinessId id, float progress)
        {
            if (_businessPanels.TryGetValue(id, out var panel))
            {
                panel.SetProgress(progress);
            }
        }

        public void UpdateStatic(
            BusinessId id,
            int level,
            long income,
            bool isUnlocked,
            long buyLevelPrice,
            IReadOnlyList<UpgradeConfigData> upgrades,
            bool[] upgradesBought,
            bool canBuyLevel,
            bool[] canBuyUpgrade)
        {
            if (_businessPanels.TryGetValue(id, out var panel))
            {
                panel.UpdateStatic(level, income, isUnlocked, buyLevelPrice, 
                    upgrades, upgradesBought, canBuyUpgrade, canBuyLevel);   
            }
        }
    }
}