using Plinko.Scripts.Data.Meta;
using Plinko.Scripts.Data.Visuals;
using UnityEngine;
using UnityEngine.Serialization;

namespace Plinko.Scripts.Data.Units
{
    [CreateAssetMenu(menuName = "Session/UnitType", fileName = "UnitTypeData")]
    public sealed class UnitTypeData : ScriptableObject
    {
        [SerializeField] private string id = string.Empty;
        [SerializeField] private string displayName = string.Empty;
        [SerializeField] private string description = string.Empty;
        [FormerlySerializedAs("icon")]
        [SerializeField] private Sprite portraitSprite;
        [SerializeField] private CharacterAnimationSetData battleAnimations = new();
        [SerializeField] private Sprite trainingFieldSprite;
        [SerializeField] private int baseAttack;
        [SerializeField] private int baseHealth;
        [SerializeField] private int defaultManaCost;
        [SerializeField] private int shopPrice;
        [SerializeField] private int generationWeight = 1;
        [SerializeField] private PassiveAbilityData passiveAbility;
        [SerializeField] private UnlockConditionData unlockCondition;

        public string Id => id;
        public string DisplayName => displayName;
        public string Description => description;
        public Sprite PortraitSprite => portraitSprite;
        public CharacterAnimationSetData BattleAnimations => battleAnimations;
        public Sprite TrainingFieldSprite => trainingFieldSprite;
        public Sprite Icon => portraitSprite;
        public int BaseAttack => baseAttack;
        public int BaseHealth => baseHealth;
        public int DefaultManaCost => defaultManaCost;
        public int ShopPrice => shopPrice;
        public int GenerationWeight => generationWeight;
        public PassiveAbilityData PassiveAbility => passiveAbility;
        public UnlockConditionData UnlockCondition => unlockCondition;
    }
}
