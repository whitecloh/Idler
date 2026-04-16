using UnityEngine;

namespace Plinko.Scripts.Data.Pins
{
    [CreateAssetMenu(menuName = "Session/BasketType", fileName = "BasketTypeData")]
    public sealed class BasketTypeData : ScriptableObject
    {
        [SerializeField] private string id = string.Empty;
        [SerializeField] private string displayName = string.Empty;
        [SerializeField] private int manaValue = 1;
        [SerializeField] private int generationWeight = 1;

        public string Id => id;
        public string DisplayName => displayName;
        public int ManaValue => manaValue;
        public int GenerationWeight => generationWeight;
    }
}