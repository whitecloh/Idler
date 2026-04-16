using Plinko.Scripts.Data.Levels;
using Plinko.Scripts.Data.Locations;
using Plinko.Scripts.Data.Pins;

namespace Plinko.Scripts.Services
{
    public sealed class PlinkoConfigService
    {
        private readonly GameSettingsService _gameSettingsService;

        public PlinkoConfigService(GameSettingsService gameSettingsService)
        {
            _gameSettingsService = gameSettingsService;
        }

        public PlinkoFieldSettingsData GetField(LocationData locationData, LevelData levelData)
        {
            if (levelData != null && levelData.PreBattlePhase != null && levelData.PreBattlePhase.OverridePlinkoField != null)
            {
                return levelData.PreBattlePhase.OverridePlinkoField;
            }

            if (locationData != null && locationData.DefaultPlinkoField != null)
            {
                return locationData.DefaultPlinkoField;
            }

            return _gameSettingsService.GetFallbackPlinkoField();
        }
    }
}