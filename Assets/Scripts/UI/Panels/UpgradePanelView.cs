using System.Collections.Generic;
using Game;
using Game.Data.Business;
using Game.Data.Upgrade;
using UI.Elements;
using UnityEngine;

namespace UI.Panels
{
    public class UpgradePanelView : MonoBehaviour
    {
        [SerializeField] private UpgradeItemView upgradeItemPrefab;
        [SerializeField] private RectTransform upgradesContainer;

        private readonly List<UpgradeItemView> _items = new();

        public void Init(IReadOnlyList<UpgradeConfigData> upgrades)
        {
            foreach (var item in _items)
                Destroy(item.gameObject);
            _items.Clear();

            for (var i = 0; i < upgrades.Count; i++)
            {
                var item = Instantiate(upgradeItemPrefab, upgradesContainer);
                _items.Add(item);
            }
        }

        public void UpdateItems(
            BusinessId businessId,
            IReadOnlyList<UpgradeConfigData> upgrades,
            bool[] isBought,
            bool[] canBuyUpgrade)
        {
            for (var i = 0; i < upgrades.Count && i < _items.Count; i++)
            {
                var index = i;
                _items[i].UpdateData(
                    upgrades[i].LocalizationKey,
                    upgrades[i].Price,
                    isBought[i],
                    canBuyUpgrade[i],
                    () => EcsUiEventBridge.Instance.SendUpgradeEvent(businessId, index)
                );
            }
        }
    }
}