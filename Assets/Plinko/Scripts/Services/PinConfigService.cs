using System.Collections.Generic;
using Plinko.Scripts.Data.Levels;
using Plinko.Scripts.Data.Pins;

namespace Plinko.Scripts.Services
{
    public sealed class PinConfigService
    {
        private readonly Dictionary<string, PinTypeData> _pinsById = new();
        private readonly List<PinTypeData> _allPins = new();
        private readonly UnlocksService _unlocksService;

        public PinConfigService(IReadOnlyList<PinTypeData> pinTypes, UnlocksService unlocksService)
        {
            _unlocksService = unlocksService;
            if (pinTypes == null)
            {
                return;
            }

            foreach (var pinType in pinTypes)
            {
                if (pinType == null || string.IsNullOrWhiteSpace(pinType.Id))
                {
                    continue;
                }

                _pinsById[pinType.Id] = pinType;
                _allPins.Add(pinType);
            }
        }

        public PinTypeData GetPin(string pinTypeId)
        {
            return !string.IsNullOrWhiteSpace(pinTypeId) && _pinsById.TryGetValue(pinTypeId, out var pin)
                ? pin
                : null;
        }

        public IReadOnlyList<PinTypeData> GetUnlockedShopPool(LevelData levelData)
        {
            var result = new List<PinTypeData>();
            var explicitPool = levelData != null && levelData.PreBattlePhase != null
                ? levelData.PreBattlePhase.ExplicitPinShopPool
                : null;

            if (explicitPool != null && explicitPool.Count > 0)
            {
                foreach (var pin in explicitPool)
                {
                    if (pin != null && _unlocksService.IsUnlocked(pin.UnlockCondition))
                    {
                        result.Add(pin);
                    }
                }

                return result;
            }

            foreach (var pin in _allPins)
            {
                if (_unlocksService.IsUnlocked(pin.UnlockCondition))
                {
                    result.Add(pin);
                }
            }

            return result;
        }
    }
}