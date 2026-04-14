using System;
using System.Collections.Generic;
using Plinko.Scripts.Data.Units;

namespace Plinko.Scripts.Services
{
    public sealed class UnitNamingService
    {
        private readonly IReadOnlyList<string> _names;
        private int _nextIndex;

        public UnitNamingService(UnitNamesData unitNamesData)
        {
            _names = unitNamesData != null ? unitNamesData.Names : Array.Empty<string>();
        }

        public string GetNextDisplayName(string unitTypeDisplayName)
        {
            if (_names == null || _names.Count == 0)
            {
                return unitTypeDisplayName;
            }

            var name = _names[_nextIndex % _names.Count];
            _nextIndex++;
            return $"{unitTypeDisplayName}-{name}";
        }
    }
}