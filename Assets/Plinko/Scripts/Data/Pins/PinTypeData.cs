using Plinko.Scripts.Data.Meta;
using UnityEngine;

namespace Plinko.Scripts.Data.Pins
{
    [CreateAssetMenu(menuName = "Session/PinType", fileName = "PinTypeData")]
    public sealed class PinTypeData : ScriptableObject
    {
        [SerializeField] private string id = string.Empty;
        [SerializeField] private string displayName = string.Empty;
        [SerializeField] private string rarity = string.Empty;
        [SerializeField] private Sprite fieldSprite;
        [SerializeField] private int shopPrice;
        [SerializeField] private int generationWeight = 1;
        [SerializeField] private int attackModifier;
        [SerializeField] private int healthModifier;
        [SerializeField] private int manaModifier;
        [SerializeField] private float moveSpeedModifier;
        [SerializeField] private int attackRangeModifier;
        [SerializeField] private float attackSpeedModifier;
        [SerializeField] private UnlockConditionData unlockCondition;

        public string Id => id;
        public string DisplayName => displayName;
        public string Rarity => rarity;
        public Sprite FieldSprite => fieldSprite;
        public int ShopPrice => shopPrice;
        public int GenerationWeight => generationWeight;
        public int AttackModifier => attackModifier;
        public int HealthModifier => healthModifier;
        public int ManaModifier => manaModifier;
        public float MoveSpeedModifier => moveSpeedModifier;
        public int AttackRangeModifier => attackRangeModifier;
        public float AttackSpeedModifier => attackSpeedModifier;
        public UnlockConditionData UnlockCondition => unlockCondition;
    }
}
