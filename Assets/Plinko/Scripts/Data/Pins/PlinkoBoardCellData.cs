using System;
using UnityEngine;

namespace Plinko.Scripts.Data.Pins
{
    [Serializable]
    public sealed class PlinkoBoardCellData
    {
        [SerializeField] private PinTypeData pinType;

        public PinTypeData PinType => pinType;
    }
}