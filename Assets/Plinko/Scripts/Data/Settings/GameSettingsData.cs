    using System.Collections.Generic;
    using Plinko.Scripts.Data.Pins;
    using UnityEngine;

namespace Plinko.Scripts.Data.Settings
{
    [CreateAssetMenu(menuName = "Session/GameSettings", fileName = "GameSettingsData")]
    public sealed class GameSettingsData : ScriptableObject
    {
        [SerializeField] private int startingGold;
        [SerializeField] private int startingBaseHealth;
        [SerializeField] private int handSize = 3;
        [SerializeField] private int manaPerTurn = 3;
        [SerializeField] private int boardSlotCount = 5;
        [SerializeField] private int unitShopOfferCount = 3;
        [SerializeField] private int pinShopOfferCount = 3;
        [SerializeField] private int unitShopRerollPrice = 1;
        [SerializeField] private int pinShopRerollPrice = 1;
        [SerializeField] private int upgradeSelectionLimit = 5;
        [SerializeField] private List<PlinkoBoardRowData> plinkoBoardRows = new();

        public int StartingGold => startingGold;
        public int StartingBaseHealth => startingBaseHealth;
        public int HandSize => handSize;
        public int ManaPerTurn => manaPerTurn;
        public int BoardSlotCount => boardSlotCount;
        public int UnitShopOfferCount => unitShopOfferCount;
        public int PinShopOfferCount => pinShopOfferCount;
        public int UnitShopRerollPrice => unitShopRerollPrice;
        public int PinShopRerollPrice => pinShopRerollPrice;
        public int UpgradeSelectionLimit => upgradeSelectionLimit;
        public IReadOnlyList<PlinkoBoardRowData> PlinkoBoardRows => plinkoBoardRows;
    }
}