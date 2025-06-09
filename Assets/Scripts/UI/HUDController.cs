using System.Collections.Generic;
using Game.Data.Business;
using Game.Data.Upgrade;
using UI.Controllers;
using UI.Elements;
using UnityEngine;

namespace UI
{
    public class HUDController : MonoBehaviour
    {
        public static HUDController Instance { get; private set; }

        [SerializeField] private BalanceView balanceView;
        [SerializeField] private BusinessController businessController;

        public void Init()
        {
            Instance = this;
            businessController.Init();
        }

        public void SetBalance(int value)
        {
            balanceView.SetBalance(value);
        }

        public void UpdateBusinessPanel(
            BusinessId id, int level, int income, float progress, bool isUnlocked,
            int buyLevelPrice, IReadOnlyList<UpgradeConfigData> upgrades, bool[] upgradesBought,  bool canBuyLevel,
            bool[] canBuyUpgrade)
        {
            businessController.UpdateBusiness(id, level, income, progress, isUnlocked, buyLevelPrice, upgrades, upgradesBought, canBuyLevel, canBuyUpgrade);
        }
    }
}