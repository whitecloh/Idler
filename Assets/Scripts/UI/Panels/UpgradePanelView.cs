namespace UI.Panels
{
    using System.Collections.Generic;
    using Game;
    using Game.Data.Business;
    using Game.Data.Upgrade;
    using Game.Services;
    using Elements;
    using UnityEngine;
    
    public class UpgradePanelView : MonoBehaviour
    {
        [SerializeField] private UpgradeItemView upgradeItemPrefab;
        [SerializeField] private RectTransform upgradesContainer;

        private readonly List<UpgradeItemView> _items = new();
        
        private ConfigService _config;
        private EcsUIEventBridge _bridge;
        private BusinessId _businessId;

        public void Init(IReadOnlyList<UpgradeConfigData> upgrades, BusinessId businessId, ConfigService config, EcsUIEventBridge bridge)
        {
            _config = config;
            _bridge = bridge;
            _businessId = businessId;
            
            foreach (var item in _items)
            {
                Destroy(item.gameObject);
            }
            _items.Clear();

            for (var i = 0; i < upgrades.Count; i++)
            {
                var item = Instantiate(upgradeItemPrefab, upgradesContainer);
                var index = i;
                item.Init(() => _bridge.SendUpgradeEvent(_businessId, index));
                _items.Add(item);
            }
        }

        public void UpdateItems(
            IReadOnlyList<UpgradeConfigData> upgrades,
            bool[] isBought,
            bool[] canBuyUpgrade)
        {
            for (var i = 0; i < upgrades.Count && i < _items.Count; i++)
            {
                var displayName = _config.GetName(upgrades[i].NameKey);
                _items[i].UpdateData(displayName, upgrades[i].Price, isBought[i], canBuyUpgrade[i]);
            }
        }
    }
}