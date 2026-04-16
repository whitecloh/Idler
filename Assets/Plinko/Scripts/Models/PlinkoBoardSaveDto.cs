using System;
using System.Collections.Generic;

namespace Plinko.Scripts.Models
{
    [Serializable]
    public sealed class PlinkoBoardSaveDto
    {
        public List<InstalledPinSaveDto> InstalledPins = new();
    }
}