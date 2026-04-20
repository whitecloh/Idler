using UnityEngine;

namespace Plinko.Scripts.Data.Stats
{
    [CreateAssetMenu(menuName = "Session/StatType", fileName = "StatTypeData")]
    public sealed class StatTypeData : ScriptableObject
    {
        [SerializeField] private string id = string.Empty;
        [SerializeField] private string displayName = string.Empty;
        [SerializeField] private Sprite icon;

        public string Id => id;
        public string DisplayName => displayName;
        public Sprite Icon => icon;
    }
}
