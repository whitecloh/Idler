using System.Collections.Generic;
using Plinko.Scripts.Data.Meta;
using Plinko.Scripts.Models;
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

        public int GetMaxCompletedLevelIndex(string locationId)
        {
            return !string.IsNullOrWhiteSpace(locationId) &&
                   _maxCompletedLevelIndexByLocation.TryGetValue(locationId, out var maxLevelIndex)
                ? maxLevelIndex
                : -1;
        }

        public bool IsLocationCompleted(string locationId)
        {
            return !string.IsNullOrWhiteSpace(locationId) && _completedLocations.Contains(locationId);
        }

        public void ImportProgress(MetaSaveDto dto)
        {
            _completedLocations.Clear();
            _maxCompletedLevelIndexByLocation.Clear();
            if (dto == null)
            {
                return;
            }

            if (dto.CompletedLocationIds != null)
            {
                foreach (var locationId in dto.CompletedLocationIds)
                {
                    if (!string.IsNullOrWhiteSpace(locationId))
                    {
                        _completedLocations.Add(locationId);
                    }
                }
            }

            if (dto.LocationProgress != null)
            {
                foreach (var progress in dto.LocationProgress)
                {
                    if (progress == null || string.IsNullOrWhiteSpace(progress.LocationId))
                    {
                        continue;
                    }

                    _maxCompletedLevelIndexByLocation[progress.LocationId] = Mathf.Max(-1, progress.MaxCompletedLevelIndex);
                }
            }
        }

        public MetaSaveDto ExportProgress()
        {
            var dto = new MetaSaveDto();
            foreach (var locationId in _completedLocations)
            {
                dto.CompletedLocationIds.Add(locationId);
            }

            foreach (var pair in _maxCompletedLevelIndexByLocation)
            {
                dto.LocationProgress.Add(new LocationProgressSaveDto
                {
                    LocationId = pair.Key,
                    MaxCompletedLevelIndex = pair.Value
                });
            }

            return dto;
        }
    }
}
