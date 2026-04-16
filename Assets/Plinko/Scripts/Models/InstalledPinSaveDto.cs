using System;

namespace Plinko.Scripts.Models
{
    [Serializable]
    public sealed class InstalledPinSaveDto
    {
        public int SlotIndex;
        public string PinTypeId;
    }
}