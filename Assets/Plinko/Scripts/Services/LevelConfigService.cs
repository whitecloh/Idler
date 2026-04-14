using Plinko.Scripts.Data.Levels;

namespace Plinko.Scripts.Services
{
    public sealed class LevelConfigService
    {
        private readonly LocationConfigService _locationConfigService;

        public LevelConfigService(LocationConfigService locationConfigService)
        {
            _locationConfigService = locationConfigService;
        }

        public LevelData GetLevel(string locationId, int levelIndex)
        {
            var location = _locationConfigService.GetLocation(locationId);
            if (location == null)
            {
                return null;
            }

            if (levelIndex < 0 || levelIndex >= location.Levels.Count)
            {
                return null;
            }

            return location.Levels[levelIndex];
        }
    }
}