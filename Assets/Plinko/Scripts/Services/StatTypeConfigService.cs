using System.Collections.Generic;
using Plinko.Scripts.Data.Stats;

namespace Plinko.Scripts.Services
{
    public sealed class StatTypeConfigService
    {
        private readonly Dictionary<string, StatTypeData> _statsById = new();

        public StatTypeConfigService(IReadOnlyList<StatTypeData> statTypes)
        {
            if (statTypes == null)
            {
                return;
            }

            for (var index = 0; index < statTypes.Count; index++)
            {
                var statType = statTypes[index];
                if (statType == null || string.IsNullOrWhiteSpace(statType.Id))
                {
                    continue;
                }

                _statsById[statType.Id] = statType;
            }
        }

        public StatTypeData GetStat(string statTypeId)
        {
            return !string.IsNullOrWhiteSpace(statTypeId) && _statsById.TryGetValue(statTypeId, out var statType)
                ? statType
                : null;
        }
    }
}
