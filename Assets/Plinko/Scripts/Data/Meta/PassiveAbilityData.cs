using UnityEngine;

namespace Plinko.Scripts.Data.Meta
{
    [CreateAssetMenu(menuName = "Session/PassiveAbility", fileName = "PassiveAbilityData")]
    public sealed class PassiveAbilityData : ScriptableObject
    {
        [SerializeField] private string id = string.Empty;
        [SerializeField] private string displayName = string.Empty;
        [SerializeField] private string description = string.Empty;

        public string Id => id;
        public string DisplayName => displayName;
        public string Description => description;
    }
}