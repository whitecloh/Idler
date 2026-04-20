using System.Collections.Generic;
using Plinko.Scripts.Models.ViewData;
using UnityEngine;

namespace Plinko.Scripts.View.Items
{
    internal static class UnitStatSyncUtility
    {
        public static void Sync(
            RectTransform statsRoot,
            UnitStatEntryView statEntryPrefab,
            List<UnitStatEntryView> statViews,
            IReadOnlyList<StatDisplayViewData> stats)
        {
            if (statsRoot == null || statEntryPrefab == null || statViews == null)
            {
                return;
            }

            var targetCount = stats != null ? stats.Count : 0;
            while (statViews.Count < targetCount)
            {
                statViews.Add(Object.Instantiate(statEntryPrefab, statsRoot));
            }

            for (var index = statViews.Count - 1; index >= targetCount; index--)
            {
                Object.Destroy(statViews[index].gameObject);
                statViews.RemoveAt(index);
            }

            for (var index = 0; index < targetCount; index++)
            {
                statViews[index].Refresh(stats[index]);
            }
        }
    }
}
