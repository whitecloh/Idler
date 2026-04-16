using System.Collections.Generic;
using Plinko.Scripts.Data.Meta;
using UnityEngine;

namespace Plinko.Scripts.Services
{
    public sealed class UnlocksService
    {
        private readonly HashSet<string> _completedLocations = new();
        private readonly Dictionary<string, int> _maxCompletedLevelIndexByLocation = new();

        public bool IsUnlocked(UnlockConditionData condition)
        {
            if (condition == null)
            {
                return true;
            }

            if (condition.RequiresCompletedLocation && !_completedLocations.Contains(condition.RequiredLocationId))
            {
                return false;
            }

            if (string.IsNullOrWhiteSpace(condition.RequiredLocationId))
            {
                return true;
            }

            return _maxCompletedLevelIndexByLocation.TryGetValue(condition.RequiredLocationId, out var maxLevel) &&
                   maxLevel >= condition.RequiredCompletedLevelIndex;
        }

        public void MarkLevelCompleted(string locationId, int levelIndex)
        {
            if (string.IsNullOrWhiteSpace(locationId))
            {
                return;
            }

            if (_maxCompletedLevelIndexByLocation.TryGetValue(locationId, out var currentMax))
            {
                _maxCompletedLevelIndexByLocation[locationId] = Mathf.Max(currentMax, levelIndex);
            }
            else
            {
                _maxCompletedLevelIndexByLocation[locationId] = levelIndex;
            }
        }

        public void MarkLocationCompleted(string locationId)
        {
            if (!string.IsNullOrWhiteSpace(locationId))
            {
                _completedLocations.Add(locationId);
            }
        }
    }
}