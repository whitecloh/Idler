using System.Collections.Generic;
using Plinko.Scripts.Data.Locations;

namespace Plinko.Scripts.Services
{
    public sealed class LocationConfigService
    {
        private readonly Dictionary<string, LocationData> _locationsById = new();
        private readonly List<LocationData> _orderedLocations = new();

        public LocationConfigService(IReadOnlyList<LocationData> locations)
        {
            if (locations == null)
            {
                return;
            }

            foreach (var location in locations)
            {
                if (location == null || string.IsNullOrWhiteSpace(location.Id))
                {
                    continue;
                }

                _locationsById[location.Id] = location;
                _orderedLocations.Add(location);
            }
        }

        public LocationData GetLocation(string locationId)
        {
            return !string.IsNullOrWhiteSpace(locationId) && _locationsById.TryGetValue(locationId, out var location)
                ? location
                : null;
        }

        public IReadOnlyList<LocationData> GetAllLocations()
        {
            return _orderedLocations;
        }

        public LocationData GetFirstLocation()
        {
            return _orderedLocations.Count > 0 ? _orderedLocations[0] : null;
        }

        public bool IsUnlocked(string locationId, UnlocksService unlocksService)
        {
            return unlocksService != null && GetLocation(locationId) != null
                ? unlocksService.IsUnlocked(GetLocation(locationId).UnlockCondition)
                : GetLocation(locationId) != null;
        }

        public IReadOnlyList<LocationData> GetUnlockedLocations(UnlocksService unlocksService)
        {
            var result = new List<LocationData>();
            foreach (var location in _orderedLocations)
            {
                if (location == null)
                {
                    continue;
                }

                if (unlocksService == null || unlocksService.IsUnlocked(location.UnlockCondition))
                {
                    result.Add(location);
                }
            }

            return result;
        }
    }
}
