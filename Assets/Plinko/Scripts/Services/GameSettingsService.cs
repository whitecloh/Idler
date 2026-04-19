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

        public int GetStartingGold() => _settings != null ? _settings.StartingGold : 0;
        public int GetStartingBaseHealth() => _settings != null ? _settings.StartingBaseHealth : 0;
        public int GetHandSize() => _settings != null ? _settings.HandSize : 3;
        public int GetManaPerTurn() => _settings != null ? _settings.ManaPerTurn : 3;
        public int GetUnitShopOfferCount() => _settings != null ? _settings.UnitShopOfferCount : 3;
        public int GetPinShopOfferCount() => _settings != null ? _settings.PinShopOfferCount : 3;
        public int GetUnitShopRerollPrice() => _settings != null ? _settings.UnitShopRerollPrice : 1;
        public int GetPinShopRerollPrice() => _settings != null ? _settings.PinShopRerollPrice : 1;
        public int GetDefaultRetrainingOfferCount() => _settings != null ? _settings.DefaultRetrainingOfferCount : 3;
        public int GetRetrainingShopRerollPrice() => _settings != null ? _settings.RetrainingShopRerollPrice : 1;
        public int GetDefaultRetrainingSelectionLimit() => _settings != null ? _settings.DefaultRetrainingSelectionLimit : 3;
        public float GetBattleTickDuration() => _settings != null ? _settings.BattleTickDuration : 0.2f;
        public int GetPowerLineStartingMana() => _settings != null ? _settings.PowerLineStartingMana : 0;
        public int GetPowerLineMaxMana() => _settings != null ? _settings.PowerLineMaxMana : 10;
        public int GetPowerLineManaPerTick() => _settings != null ? _settings.PowerLineManaPerTick : 1;
        public int GetPowerLineManaTickInterval() => _settings != null ? _settings.PowerLineManaTickInterval : 5;
        public int GetPowerLineRerollManaCost() => _settings != null ? _settings.PowerLineRerollManaCost : 1;
        public PlinkoFieldSettingsData GetFallbackPlinkoField() => _settings != null ? _settings.FallbackPlinkoField : null;
    }
}
