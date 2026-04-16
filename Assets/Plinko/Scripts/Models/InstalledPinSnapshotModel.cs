using System;

namespace Plinko.Scripts.Models
{
    [Serializable]
    public sealed class InstalledPinSnapshotModel
    {
        public int SlotIndex;
        public string PinTypeId;
    }
}
