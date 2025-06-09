using Editor_Custom;
using UnityEngine;

namespace Game.Data.Upgrade
{
    [CreateAssetMenu(menuName = "IdleClicker/UpgradeConfig", fileName = "UpgradeConfigData")]
    public class UpgradeConfigData : ScriptableObject
    {
        [LocalizationListPopupField]
        [SerializeField] private string localizationKey = string.Empty;
        [SerializeField] private int price;
        [SerializeField] private float incomeMultiplier;

        public string LocalizationKey => localizationKey;
        public int Price => price;
        public float IncomeMultiplier => incomeMultiplier;
    }
}