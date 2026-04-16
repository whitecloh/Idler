using System;
using System.Collections.Generic;

namespace Plinko.Scripts.Models
{
    [Serializable]
    public sealed class MetaSaveDto
    {
        public List<string> CompletedLocationIds = new();
        public List<LocationProgressSaveDto> LocationProgress = new();
    }
}
