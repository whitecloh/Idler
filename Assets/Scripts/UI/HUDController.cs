namespace UI
{
    using System.Collections.Generic;
    using Game.Data.Business;
    using Game.Data.Upgrade;
    using Controllers;
    using Elements;
    using UnityEngine;
    using Game;
    using Game.Services;
    
    public sealed class HUDController : MonoBehaviour
    {
        [SerializeField] private BalanceView balanceView;
        [SerializeField] private BusinessController businessController;
        
        private ConfigService _config;
        private EcsUIEventBridge _bridge;

        public void Init(ConfigService configService, EcsUIEventBridge bridge)
        {
            _config = configService;
            _bridge = bridge;
            businessController.Init(_config, _bridge);
        }

        public void SetBalance(long value)
        {
            balanceView.SetBalance(value);
        }

        public void UpdateBusinessPanelProgress(BusinessId id, float progress)
        {
            businessController.UpdateProgress(id, progress);
        }

        public void UpdateBusinessPanelStatic(
            BusinessId id, int level, long income, bool isUnlocked,
            long buyLevelPrice, IReadOnlyList<UpgradeConfigData> upgrades,
            bool[] upgradesBought, bool canBuyLevel, bool[] canBuyUpgrade)
        {
            businessController.UpdateStatic(id, level, income, isUnlocked, buyLevelPrice,
                upgrades, upgradesBought, canBuyLevel, canBuyUpgrade);
        }
    }
}