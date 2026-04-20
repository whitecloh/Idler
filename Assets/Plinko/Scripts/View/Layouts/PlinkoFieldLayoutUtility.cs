using System.Collections.Generic;
using Plinko.Scripts.Data.Pins;
using Plinko.Scripts.Models.ViewData;
using UnityEngine;

namespace Plinko.Scripts.View.Layouts
{
    public static class PlinkoFieldLayoutUtility
    {
        public static Vector2 BuildPinPosition(
            int rowIndex,
            int columnIndex,
            IReadOnlyDictionary<int, int> rowCounts,
            float horizontalSpacing,
            float verticalSpacing,
            float pixelsPerFieldUnit)
        {
            var rowCount = rowCounts.TryGetValue(rowIndex, out var count) ? count : 1;
            var x = (columnIndex - (rowCount - 1) * 0.5f) * horizontalSpacing * pixelsPerFieldUnit;
            var y = GetTopY(rowCounts, verticalSpacing, pixelsPerFieldUnit) - rowIndex * verticalSpacing * pixelsPerFieldUnit;
            return new Vector2(x, y);
        }

        public static Vector2 BuildBasketPosition(
            int basketIndex,
            int totalBasketCount,
            IReadOnlyDictionary<int, int> rowCounts,
            float horizontalSpacing,
            float verticalSpacing,
            float pixelsPerFieldUnit)
        {
            var x = (basketIndex - (totalBasketCount - 1) * 0.5f) * horizontalSpacing * pixelsPerFieldUnit;
            var y = GetTopY(rowCounts, verticalSpacing, pixelsPerFieldUnit) - GetTotalRowCount(rowCounts) * verticalSpacing * pixelsPerFieldUnit;
            return new Vector2(x, y);
        }

        public static float GetTopY(
            IReadOnlyDictionary<int, int> rowCounts,
            float verticalSpacing,
            float pixelsPerFieldUnit)
        {
            var totalRowCount = Mathf.Max(1, GetTotalRowCount(rowCounts));
            return (totalRowCount - 1) * verticalSpacing * pixelsPerFieldUnit * 0.5f;
        }

        public static int GetTotalRowCount(IReadOnlyDictionary<int, int> rowCounts)
        {
            var maxRowIndex = -1;
            foreach (var pair in rowCounts)
            {
                if (pair.Key > maxRowIndex)
                {
                    maxRowIndex = pair.Key;
                }
            }

            return maxRowIndex + 1;
        }

        public static Dictionary<int, int> BuildRowCounts(IReadOnlyList<PurchaseFieldPinViewData> pins)
        {
            var result = new Dictionary<int, int>();
            for (var index = 0; index < pins.Count; index++)
            {
                var pin = pins[index];
                if (!result.TryGetValue(pin.RowIndex, out var rowCount) || rowCount < pin.ColumnIndex + 1)
                {
                    result[pin.RowIndex] = pin.ColumnIndex + 1;
                }
            }

            return result;
        }

        public static Dictionary<int, int> BuildRowCounts(PlinkoFieldSettingsData fieldSettings)
        {
            var result = new Dictionary<int, int>();
            if (fieldSettings == null || fieldSettings.Rows == null)
            {
                return result;
            }

            for (var rowIndex = 0; rowIndex < fieldSettings.Rows.Count; rowIndex++)
            {
                var row = fieldSettings.Rows[rowIndex];
                result[rowIndex] = row != null && row.Cells != null ? row.Cells.Count : 0;
            }

            return result;
        }
    }
}
