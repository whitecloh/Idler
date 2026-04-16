using System;

namespace Plinko.Scripts.Models
{
    [Serializable]
    public sealed class LocationProgressSaveDto
    {
        public string LocationId;
        public int MaxCompletedLevelIndex = -1;
    }
}
