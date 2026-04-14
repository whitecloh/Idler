using System;
using System.Collections.Generic;
using UnityEngine;

namespace Plinko.Scripts.Data.Pins
{
    [Serializable]
    public sealed class PlinkoBoardRowData
    {
        [SerializeField] private List<PlinkoBoardCellData> cells = new();

        public IReadOnlyList<PlinkoBoardCellData> Cells => cells;
    }
}