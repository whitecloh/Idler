using UnityEngine;

namespace Plinko.Scripts.Data.Meta
{
    [CreateAssetMenu(menuName = "Session/UnlockCondition", fileName = "UnlockConditionData")]
    public sealed class UnlockConditionData : ScriptableObject
    {
        [SerializeField] private string id = string.Empty;
        [SerializeField] private string requiredLocationId = string.Empty;
        [SerializeField] private int requiredCompletedLevelIndex = -1;
        [SerializeField] private bool requiresCompletedLocation;

        public string Id => id;
        public string RequiredLocationId => requiredLocationId;
        public int RequiredCompletedLevelIndex => requiredCompletedLevelIndex;
        public bool RequiresCompletedLocation => requiresCompletedLocation;
    }
}