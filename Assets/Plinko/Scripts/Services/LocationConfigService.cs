using System.Collections.Generic;
using Plinko.Scripts.Data.Locations;

namespace Plinko.Scripts.Services
{
    public sealed class LocationConfigService
    {
        private readonly Dictionary<string, LocationData> _locationsById = new();

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
            }
        }

        public LocationData GetLocation(string locationId)
        {
            return !string.IsNullOrWhiteSpace(locationId) && _locationsById.TryGetValue(locationId, out var location)
                ? location
                : null;
        }

        public IReadOnlyCollection<LocationData> GetAllLocations()
        {
            return _locationsById.Values;
        }
    }
}