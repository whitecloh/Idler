using UnityEngine;

namespace Plinko.Scripts.Data.Pins
{
    [CreateAssetMenu(menuName = "Session/PinType", fileName = "PinTypeData")]
    public sealed class PinTypeData : ScriptableObject
    {
        [SerializeField] private string id = string.Empty;
        [SerializeField] private string displayName = string.Empty;
        [SerializeField] private int shopPrice;
        [SerializeField] private int attackModifier;
        [SerializeField] private int healthModifier;
        [SerializeField] private int manaModifier;

        public string Id => id;
        public string DisplayName => displayName;
        public int ShopPrice => shopPrice;
        public int AttackModifier => attackModifier;
        public int HealthModifier => healthModifier;
        public int ManaModifier => manaModifier;
    }
}