using System;
using Game.Services;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Elements
{
    public class UpgradeItemView : MonoBehaviour
    {
        [SerializeField] private TMP_Text upgradeNameText;
        [SerializeField] private TMP_Text upgradePriceText;
        [SerializeField] private Button buyUpgradeButton;
        [SerializeField] private RectTransform upgradeCompleteText;
        [SerializeField] private RectTransform upgradeProgressText;
        
        private Action _onBuy;

        public void Init(Action onBuy)
        {
            _onBuy = onBuy;
            buyUpgradeButton.onClick.RemoveAllListeners();
            buyUpgradeButton.onClick.AddListener(() => _onBuy?.Invoke());
        }
        
        public void UpdateData(string upgradeKey, int price, bool isBought, bool canBuy)
        {
            upgradeNameText.text = ConfigService.Instance.GetLocalizedText(upgradeKey);
            upgradePriceText.text = price.ToString();
            buyUpgradeButton.interactable = canBuy && !isBought;

            upgradeProgressText.gameObject.SetActive(!isBought);
            upgradeCompleteText.gameObject.SetActive(isBought);
        }
    }
}