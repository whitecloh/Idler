using Plinko.Scripts.Data.Pins;
using UnityEngine;
using UnityEngine.Serialization;

namespace Plinko.Scripts.Data.Settings
{
    [CreateAssetMenu(menuName = "Session/GameSettings", fileName = "GameSettingsData")]
    public sealed class GameSettingsData : ScriptableObject
    {
        [SerializeField] private int startingGold;
        [SerializeField] private int startingBaseHealth;
        [SerializeField] private int handSize = 3;
        [SerializeField] private int manaPerTurn = 3;
        [SerializeField] private int unitShopOfferCount = 3;
        [SerializeField] private int pinShopOfferCount = 3;
        [SerializeField] private int unitShopRerollPrice = 1;
        [SerializeField] private int pinShopRerollPrice = 1;
        [FormerlySerializedAs("defaultRetrainingSelectionLimit")]
        [SerializeField] private int defaultRetrainingOfferCount = 3;
        [SerializeField] private int retrainingShopRerollPrice = 1;
        [SerializeField] private float battleTickDuration = 0.2f;
        [SerializeField] private int powerLineStartingMana;
        [SerializeField] private int powerLineMaxMana = 10;
        [SerializeField] private int powerLineManaPerTick = 1;
        [SerializeField] private int powerLineManaTickInterval = 5;
        [SerializeField] private int powerLineRerollManaCost = 1;
        [SerializeField] private PlinkoFieldSettingsData fallbackPlinkoField;

        public int StartingGold => startingGold;
        public int StartingBaseHealth => startingBaseHealth;
        public int HandSize => handSize;
        public int ManaPerTurn => manaPerTurn;
        public int UnitShopOfferCount => unitShopOfferCount;
        public int PinShopOfferCount => pinShopOfferCount;
        public int UnitShopRerollPrice => unitShopRerollPrice;
        public int PinShopRerollPrice => pinShopRerollPrice;
        public int DefaultRetrainingOfferCount => defaultRetrainingOfferCount;
        public int RetrainingShopRerollPrice => retrainingShopRerollPrice;
        public int DefaultRetrainingSelectionLimit => defaultRetrainingOfferCount;
        public float BattleTickDuration => battleTickDuration;
        public int PowerLineStartingMana => powerLineStartingMana;
        public int PowerLineMaxMana => powerLineMaxMana;
        public int PowerLineManaPerTick => powerLineManaPerTick;
        public int PowerLineManaTickInterval => powerLineManaTickInterval;
        public int PowerLineRerollManaCost => powerLineRerollManaCost;
        public PlinkoFieldSettingsData FallbackPlinkoField => fallbackPlinkoField;
    }
}
