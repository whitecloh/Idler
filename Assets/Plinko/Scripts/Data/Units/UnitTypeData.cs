using UnityEngine;

namespace Plinko.Scripts.Data.Units
{
    [CreateAssetMenu(menuName = "Session/UnitType", fileName = "UnitTypeData")]
    public sealed class UnitTypeData : ScriptableObject
    {
        [SerializeField] private string id = string.Empty;
        [SerializeField] private string displayName = string.Empty;
        [SerializeField] private int baseAttack;
        [SerializeField] private int baseHealth;
        [SerializeField] private int manaCost;
        [SerializeField] private int shopPrice;
        [SerializeField] private string passiveAbilityId = string.Empty;

        public string Id => id;
        public string DisplayName => displayName;
        public int BaseAttack => baseAttack;
        public int BaseHealth => baseHealth;
        public int ManaCost => manaCost;
        public int ShopPrice => shopPrice;
        public string PassiveAbilityId => passiveAbilityId;
    }
}