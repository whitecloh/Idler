using Game.Services;

namespace UI.Panels
{
    using System.Collections.Generic;
    using Game;
    using Game.Data.Business;
    using Game.Data.Upgrade;
    using TMPro;
    using UnityEngine;
    using UnityEngine.UI;
    
    public sealed class BusinessPanelView : MonoBehaviour
    {
        [Header("Texts")] 
        [SerializeField] private TMP_Text businessNameText;
        [SerializeField] private TMP_Text levelText;
        [SerializeField] private TMP_Text incomeText;
        [SerializeField] private TMP_Text buyLevelPriceText;

        [Header("Progress")] 
        [SerializeField] private Image progressBar;

        [Header("Buttons")] 
        [SerializeField] private Button buyLevelButton;

        [Header("UI Blocks")] 
        [SerializeField] private RectTransform lockedPanel;

        [Header("Upgrades")] 
        [SerializeField] private UpgradePanelView upgradePanelView;
        
        private EcsUIEventBridge _bridge;

        public void Init(BusinessId businessId, string displayName, IReadOnlyList<UpgradeConfigData> upgrades, ConfigService config, EcsUIEventBridge bridge)
        {
            _bridge = bridge;
            
            businessNameText.text = displayName;
            upgradePanelView.Init(upgrades, businessId, config, _bridge);

            buyLevelButton.onClick.RemoveAllListeners();
            buyLevelButton.onClick.AddListener(() => { _bridge.SendBuyLevelEvent(businessId); });
        }

        public void SetProgress(float progress)
        {
            progressBar.fillAmount = progress;
        }

        public void UpdateStatic(
            int level,
            long income,
            bool isUnlocked,
            long buyLevelPrice,
            IReadOnlyList<UpgradeConfigData> upgrades,
            bool[] upgradesBought,
            bool[] canBuyUpgrade,
            bool canBuyLevel)
        {
            levelText.text = level.ToString();
            incomeText.text = $"+{income}";
            buyLevelPriceText.text = buyLevelPrice.ToString();

            lockedPanel.gameObject.SetActive(!isUnlocked);
            buyLevelButton.interactable = canBuyLevel;

            upgradePanelView.UpdateItems(upgrades, upgradesBought, canBuyUpgrade);
        }
    }
}