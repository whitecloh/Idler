using System.Collections.Generic;
using Game;
using Game.Data.Business;
using Game.Data.Upgrade;
using Game.Services;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Panels
{
    public class BusinessPanelView : MonoBehaviour
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

        public void Init(BusinessId businessId, string businessKey, IReadOnlyList<UpgradeConfigData> upgrades)
        {
            businessNameText.text = ConfigService.Instance.GetLocalizedText(businessKey);
            upgradePanelView.Init(upgrades, businessId);

            buyLevelButton.onClick.RemoveAllListeners();
            buyLevelButton.onClick.AddListener(() =>
            {
                EcsUiEventBridge.Instance.SendBuyLevelEvent(businessId);
            });
        }

        public void UpdateData(
            int level,
            long income,
            float progress,
            bool isUnlocked,
            long buyLevelPrice,
            bool canBuyLevel,
            IReadOnlyList<UpgradeConfigData> upgrades,
            bool[] upgradesBought,
            bool[] canBuyUpgrade)
        {
            levelText.text = level.ToString();
            incomeText.text = $"+{income}";
            progressBar.fillAmount = progress;
            buyLevelPriceText.text = buyLevelPrice.ToString();

            lockedPanel.gameObject.SetActive(!isUnlocked);
            buyLevelButton.interactable = canBuyLevel;

            upgradePanelView.UpdateItems(upgrades, upgradesBought, canBuyUpgrade);
        }
    }
}