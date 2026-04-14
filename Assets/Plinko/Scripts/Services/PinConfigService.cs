using System.Collections.Generic;
using Plinko.Scripts.Data.Pins;

namespace Plinko.Scripts.Services
{
    public sealed class PinConfigService
    {
        private readonly Dictionary<string, PinTypeData> _pinsById = new();
        private readonly List<PinTypeData> _allPins = new();

        public PinConfigService(IReadOnlyList<PinTypeData> pinTypes)
        {
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

        public IReadOnlyList<PinTypeData> GetAllPins()
        {
            return _allPins;
        }
    }
}