using System;

namespace Plinko.Scripts.Models.ViewData
{
    [Serializable]
    public sealed class LocationEntryViewData
    {
        public string LocationId;
        public string DisplayName;
        public bool IsUnlocked;
        public bool IsCompleted;
        public int MaxCompletedLevelIndex;
        public int TotalLevelCount;
        public string StatusText;
        public string UnlockDescription;
    }
}
