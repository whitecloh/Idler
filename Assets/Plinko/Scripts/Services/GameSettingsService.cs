using System;
using System.Collections.Generic;
using Plinko.Scripts.Data.Pins;
using Plinko.Scripts.Data.Settings;

namespace Plinko.Scripts.Services
{
    public sealed class GameSettingsService
    {
        private readonly GameSettingsData _settings;

        public GameSettingsService(GameSettingsData settings)
        {
            _settings = settings;
        }

        public int GetStartingGold()
        {
            return _settings != null ? _settings.StartingGold : 0;
        }

        public int GetStartingBaseHealth()
        {
            return _settings != null ? _settings.StartingBaseHealth : 0;
        }

        public int GetHandSize()
        {
            return _settings != null ? _settings.HandSize : 3;
        }

        public int GetManaPerTurn()
        {
            return _settings != null ? _settings.ManaPerTurn : 3;
        }

        public int GetBoardSlotCount()
        {
            if (_settings == null)
            {
                return 5;
            }

            var totalCellCount = 0;
            if (_settings.PlinkoBoardRows != null)
            {
                foreach (var row in _settings.PlinkoBoardRows)
                {
                    totalCellCount += row != null && row.Cells != null ? row.Cells.Count : 0;
                }
            }

            return totalCellCount > 0 ? totalCellCount : _settings.BoardSlotCount;
        }

        public IReadOnlyList<PlinkoBoardRowData> GetPlinkoBoardRows()
        {
            return _settings != null ? _settings.PlinkoBoardRows : Array.Empty<PlinkoBoardRowData>();
        }

        public int GetUnitShopOfferCount()
        {
            return _settings != null ? _settings.UnitShopOfferCount : 3;
        }

        public int GetPinShopOfferCount()
        {
            return _settings != null ? _settings.PinShopOfferCount : 3;
        }

        public int GetUnitShopRerollPrice()
        {
            return _settings != null ? _settings.UnitShopRerollPrice : 1;
        }

        public int GetPinShopRerollPrice()
        {
            return _settings != null ? _settings.PinShopRerollPrice : 1;
        }

        public int GetUpgradeSelectionLimit()
        {
            return _settings != null ? _settings.UpgradeSelectionLimit : 5;
        }
    }
}