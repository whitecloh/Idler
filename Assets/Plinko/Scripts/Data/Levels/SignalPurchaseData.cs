using System;
using UnityEngine;

namespace Plinko.Scripts.Data.Levels
{
    [Serializable]
    public sealed class SignalPurchaseData
    {
        [SerializeField] private int newUnitSlotCount = 3;
        [SerializeField] private int generatorBreakAfterMinSignals = 1;
        [SerializeField] private int generatorBreakAfterMaxSignals = 1;

        public int NewUnitSlotCount => Mathf.Max(1, newUnitSlotCount);
        public int GeneratorBreakAfterMinSignals => Mathf.Max(1, generatorBreakAfterMinSignals);
        public int GeneratorBreakAfterMaxSignals => Mathf.Max(GeneratorBreakAfterMinSignals, generatorBreakAfterMaxSignals);
    }
}
