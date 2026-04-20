using System.Collections.Generic;
using System.Text;
using Leopotam.EcsLite;
using Plinko.Scripts.Data.Common;
using Plinko.Scripts.Data.Levels;
using Plinko.Scripts.Data.Locations;
using Plinko.Scripts.Data.Pins;
using Plinko.Scripts.ECS.Components;
using Plinko.Scripts.ECS.Events;
using Plinko.Scripts.ECS.Indexes;
using Plinko.Scripts.Models.ViewData;
using Plinko.Scripts.Services;
using Plinko.Scripts.View;

namespace Plinko.Scripts.ECS.Systems.UISystems
{
    public sealed class RefreshFieldUpgradeUiSystem : IEcsInitSystem, IEcsRunSystem
    {
        private readonly GameSettingsService _gameSettingsService;
        private readonly PinConfigService _pinConfigService;
        private readonly UnitConfigService _unitConfigService;
        private readonly LocationConfigService _locationConfigService;
        private readonly LevelConfigService _levelConfigService;
        private readonly PlinkoConfigService _plinkoConfigService;
        private readonly RunEntityIndex _runEntityIndex;
        private readonly UiCompositionRoot _uiCompositionRoot;

        private EcsPool<CurrentPhaseComponent> _phasePool;
        private EcsPool<CurrentGoldComponent> _goldPool;
        private EcsPool<FieldUpgradePhaseStateComponent> _fieldUpgradeStatePool;
        private EcsPool<CurrentLocationComponent> _locationPool;
        private EcsPool<CurrentLevelComponent> _levelPool;
        private EcsPool<PlayerBaseHealthComponent> _playerBasePool;
        private EcsPool<PinShopOfferComponent> _offerPool;
        private EcsPool<OfferPriceComponent> _pricePool;
        private EcsPool<ShopOfferPinTypeIdComponent> _offerPinTypePool;
        private EcsPool<PendingPurchasedPinComponent> _pendingPinPool;
        private EcsPool<InstalledPinComponent> _installedPinPool;
        private EcsPool<OwnedUnitComponent> _ownedUnitPool;
        private EcsPool<UnitDisplayNameComponent> _displayNamePool;
        private EcsPool<UnitTypeIdComponent> _unitTypePool;
        private EcsPool<PinPurchasedEvent> _pinPurchasedEventPool;

        private EcsFilter _offerFilter;
        private EcsFilter _pendingFilter;
        private EcsFilter _installedFilter;
        private EcsFilter _ownedFilter;
        private EcsFilter _pinPurchasedFilter;

        public RefreshFieldUpgradeUiSystem(
            GameSettingsService gameSettingsService,
            PinConfigService pinConfigService,
            UnitConfigService unitConfigService,
            LocationConfigService locationConfigService,
            LevelConfigService levelConfigService,
            PlinkoConfigService plinkoConfigService,
            RunEntityIndex runEntityIndex,
            UiCompositionRoot uiCompositionRoot)
        {
            _gameSettingsService = gameSettingsService;
            _pinConfigService = pinConfigService;
            _unitConfigService = unitConfigService;
            _locationConfigService = locationConfigService;
            _levelConfigService = levelConfigService;
            _plinkoConfigService = plinkoConfigService;
            _runEntityIndex = runEntityIndex;
            _uiCompositionRoot = uiCompositionRoot;
        }

        public void Init(IEcsSystems systems)
        {
            var world = systems.GetWorld();
            _phasePool = world.GetPool<CurrentPhaseComponent>();
            _goldPool = world.GetPool<CurrentGoldComponent>();
            _fieldUpgradeStatePool = world.GetPool<FieldUpgradePhaseStateComponent>();
            _locationPool = world.GetPool<CurrentLocationComponent>();
            _levelPool = world.GetPool<CurrentLevelComponent>();
            _playerBasePool = world.GetPool<PlayerBaseHealthComponent>();
            _offerPool = world.GetPool<PinShopOfferComponent>();
            _pricePool = world.GetPool<OfferPriceComponent>();
            _offerPinTypePool = world.GetPool<ShopOfferPinTypeIdComponent>();
            _pendingPinPool = world.GetPool<PendingPurchasedPinComponent>();
            _installedPinPool = world.GetPool<InstalledPinComponent>();
            _ownedUnitPool = world.GetPool<OwnedUnitComponent>();
            _displayNamePool = world.GetPool<UnitDisplayNameComponent>();
            _unitTypePool = world.GetPool<UnitTypeIdComponent>();
            _pinPurchasedEventPool = world.GetPool<PinPurchasedEvent>();

            _offerFilter = world.Filter<PinShopOfferComponent>().Inc<OfferPriceComponent>().Inc<ShopOfferPinTypeIdComponent>().End();
            _pendingFilter = world.Filter<PendingPurchasedPinComponent>().End();
            _installedFilter = world.Filter<InstalledPinComponent>().End();
            _ownedFilter = world.Filter<OwnedUnitComponent>().End();
            _pinPurchasedFilter = world.Filter<PinPurchasedEvent>().End();
        }

        public void Run(IEcsSystems systems)
        {
            if (_uiCompositionRoot == null)
            {
                return;
            }

            if (!_runEntityIndex.TryGetRunEntity(out var runEntity) ||
                !_phasePool.Has(runEntity) ||
                _phasePool.Get(runEntity).Value != Enums.PhaseType.FieldUpgradePhase)
            {
                _uiCompositionRoot.RefreshFieldUpgradePhase(new FieldUpgradePhaseViewData());
                return;
            }

            var locationId = _locationPool.Get(runEntity).LocationId;
            var levelIndex = _levelPool.Get(runEntity).LevelIndex;
            var locationData = _locationConfigService.GetLocation(locationId);
            var levelData = _levelConfigService.GetLevel(locationId, levelIndex);
            var fieldData = _plinkoConfigService.GetField(locationData, levelData);
            var nextBattleLevel = FindNextBattleLevel(locationData, levelIndex) ?? levelData;
            var fieldState = _fieldUpgradeStatePool.Get(runEntity);
            var pendingPinData = BuildPendingPin();
            var slots = BuildSlots(fieldData, pendingPinData != null, fieldState.SelectedSlotIndex);

            var rerollPrice = _gameSettingsService.GetPinShopRerollPrice();
            var gold = _goldPool.Get(runEntity).Value;
            var playerBase = _playerBasePool.Get(runEntity);
            var selectedPin = BuildSelectedPin(slots, fieldState.SelectedSlotIndex);
            var viewData = new FieldUpgradePhaseViewData
            {
                LevelKey = $"{locationId}:{levelIndex}",
                LocationDisplayName = locationData != null && !string.IsNullOrWhiteSpace(locationData.DisplayName)
                    ? locationData.DisplayName
                    : locationId,
                FieldSignature = BuildFieldSignature(locationId, levelIndex, slots),
                FieldHorizontalSpacing = fieldData != null ? fieldData.HorizontalSpacing : 1f,
                FieldVerticalSpacing = fieldData != null ? fieldData.VerticalSpacing : 1f,
                Gold = gold,
                RerollCount = fieldState.RerollCount,
                RerollPrice = rerollPrice,
                CanReroll = gold >= rerollPrice && pendingPinData == null,
                HasPendingPin = pendingPinData != null,
                IsSelectionOverlayActive = pendingPinData != null,
                SelectedSlotIndex = fieldState.SelectedSlotIndex,
                CanReplace = pendingPinData != null && fieldState.SelectedSlotIndex >= 0,
                CanCancelSelection = pendingPinData != null && fieldState.SelectedSlotIndex >= 0,
                CanAdvance = pendingPinData == null && fieldState.SelectedSlotIndex < 0,
                PrimaryActionLabel = "Next Level",
                PlayerBaseHealth = playerBase.Value,
                PlayerBaseMaxHealth = playerBase.MaxValue,
                NextBattleBackgroundSprite = nextBattleLevel != null ? nextBattleLevel.BackgroundSprite : null,
                PlayerBaseSprite = nextBattleLevel != null ? nextBattleLevel.PlayerBaseSprite : null,
                PendingPin = pendingPinData,
                SelectedPin = selectedPin,
                Offers = BuildOffers(),
                StartedPurchases = BuildStartedPurchases(),
                Slots = slots,
                Baskets = BuildFieldBaskets(fieldData),
                Levels = BuildLevelProgress(locationData, levelIndex),
                ArmyPreviewUnits = BuildArmyPreviewUnits()
            };

            _uiCompositionRoot.RefreshFieldUpgradePhase(viewData);
        }

        private List<PinOfferViewData> BuildOffers()
        {
            var offers = new List<PinOfferViewData>();
            foreach (var offerEntity in _offerFilter)
            {
                var pinTypeId = _offerPinTypePool.Get(offerEntity).Value;
                var pinType = _pinConfigService.GetPin(pinTypeId);
                offers.Add(new PinOfferViewData
                {
                    OfferId = _offerPool.Get(offerEntity).OfferId,
                    PinTypeId = pinTypeId,
                    DisplayName = pinType != null && !string.IsNullOrWhiteSpace(pinType.DisplayName)
                        ? pinType.DisplayName
                        : pinTypeId,
                    Sprite = pinType != null ? pinType.FieldSprite : null,
                    Price = _pricePool.Get(offerEntity).Value,
                    ModifierLines = BuildModifierLines(pinType)
                });
            }

            offers.Sort((left, right) => left.OfferId.CompareTo(right.OfferId));
            return offers;
        }

        private List<BoardSlotViewData> BuildSlots(
            PlinkoFieldSettingsData fieldData,
            bool hasPendingPin,
            int selectedSlotIndex)
        {
            var authoredPinsBySlot = BuildAuthoredPinsBySlot(fieldData);
            var slots = new List<BoardSlotViewData>();
            foreach (var installedEntity in _installedFilter)
            {
                var installedPin = _installedPinPool.Get(installedEntity);
                var pinType = _pinConfigService.GetPin(installedPin.PinTypeId);
                if (pinType == null && authoredPinsBySlot.TryGetValue(installedPin.SlotIndex, out var authoredPin))
                {
                    pinType = authoredPin;
                }

                var isSelected = installedPin.SlotIndex == selectedSlotIndex;
                slots.Add(new BoardSlotViewData
                {
                    SlotIndex = installedPin.SlotIndex,
                    RowIndex = installedPin.RowIndex,
                    ColumnIndex = installedPin.ColumnIndex,
                    PinTypeId = installedPin.PinTypeId,
                    DisplayName = pinType != null && !string.IsNullOrWhiteSpace(pinType.DisplayName)
                        ? pinType.DisplayName
                        : installedPin.PinTypeId,
                    Sprite = pinType != null ? pinType.FieldSprite : null,
                    IsSelected = isSelected,
                    IsPlacementHighlighted = hasPendingPin && isSelected,
                    IsAvailableForReplacement = hasPendingPin && selectedSlotIndex < 0,
                    IsSelectedForReplacement = hasPendingPin && isSelected,
                    IsNotSelectedForReplacement = hasPendingPin && selectedSlotIndex >= 0 && !isSelected
                });
            }

            slots.Sort((left, right) => left.SlotIndex.CompareTo(right.SlotIndex));
            return slots;
        }

        private List<PurchaseFieldBasketViewData> BuildFieldBaskets(PlinkoFieldSettingsData fieldData)
        {
            var result = new List<PurchaseFieldBasketViewData>();
            if (fieldData == null || fieldData.Baskets == null)
            {
                return result;
            }

            for (var index = 0; index < fieldData.Baskets.Count; index++)
            {
                var basket = fieldData.Baskets[index];
                if (basket == null)
                {
                    continue;
                }

                result.Add(new PurchaseFieldBasketViewData
                {
                    BasketId = basket.Id,
                    BasketIndex = index,
                    DisplayName = basket.DisplayName,
                    ManaValue = basket.ManaValue,
                    Sprite = basket.FieldSprite
                });
            }

            return result;
        }

        private List<PurchaseLevelProgressEntryViewData> BuildLevelProgress(LocationData locationData, int currentLevelIndex)
        {
            var result = new List<PurchaseLevelProgressEntryViewData>();
            if (locationData == null || locationData.Levels == null)
            {
                return result;
            }

            for (var index = 0; index < locationData.Levels.Count; index++)
            {
                var level = locationData.Levels[index];
                result.Add(new PurchaseLevelProgressEntryViewData
                {
                    LevelIndex = index,
                    DisplayNumber = index + 1,
                    LevelType = level != null ? level.LevelType : Enums.LevelType.None,
                    ProgressSprite = level != null ? level.ProgressSprite : null,
                    IsCompleted = index < currentLevelIndex,
                    IsCurrent = index == currentLevelIndex,
                    IsUnlocked = index <= currentLevelIndex
                });
            }

            return result;
        }

        private List<PurchaseArmyPreviewUnitViewData> BuildArmyPreviewUnits()
        {
            var result = new List<PurchaseArmyPreviewUnitViewData>();
            var runtimeToEntity = new Dictionary<int, int>();
            foreach (var ownedEntity in _ownedFilter)
            {
                runtimeToEntity[_ownedUnitPool.Get(ownedEntity).RuntimeId] = ownedEntity;
            }

            var runtimeIds = new List<int>(runtimeToEntity.Keys);
            runtimeIds.Sort();

            for (var index = 0; index < runtimeIds.Count; index++)
            {
                var runtimeId = runtimeIds[index];
                var ownedEntity = runtimeToEntity[runtimeId];
                var unitType = _unitConfigService.GetUnit(_unitTypePool.Get(ownedEntity).Value);
                result.Add(new PurchaseArmyPreviewUnitViewData
                {
                    RuntimeId = runtimeId,
                    DisplayName = _displayNamePool.Get(ownedEntity).Value,
                    PortraitSprite = unitType != null ? unitType.PortraitSprite : null,
                    BattleAnimations = unitType != null ? unitType.BattleAnimations : null
                });
            }

            return result;
        }

        private FieldUpgradeSelectedPinViewData BuildPendingPin()
        {
            foreach (var pendingEntity in _pendingFilter)
            {
                var pendingPin = _pendingPinPool.Get(pendingEntity);
                var pinType = _pinConfigService.GetPin(pendingPin.PinTypeId);
                return BuildSelectedPin(pinType);
            }

            return null;
        }

        private List<FieldUpgradeStartedPurchaseViewData> BuildStartedPurchases()
        {
            var result = new List<FieldUpgradeStartedPurchaseViewData>();
            foreach (var eventEntity in _pinPurchasedFilter)
            {
                var pinPurchasedEvent = _pinPurchasedEventPool.Get(eventEntity);
                result.Add(new FieldUpgradeStartedPurchaseViewData
                {
                    OfferId = pinPurchasedEvent.OfferId,
                    PinTypeId = pinPurchasedEvent.PinTypeId
                });
            }

            result.Sort((left, right) => left.OfferId.CompareTo(right.OfferId));
            return result;
        }

        private FieldUpgradeSelectedPinViewData BuildSelectedPin(
            IReadOnlyList<BoardSlotViewData> slots,
            int selectedSlotIndex)
        {
            for (var index = 0; index < slots.Count; index++)
            {
                var slot = slots[index];
                if (slot.SlotIndex == selectedSlotIndex)
                {
                    var pinType = _pinConfigService.GetPin(slot.PinTypeId);
                    return BuildSelectedPin(pinType);
                }
            }

            return null;
        }

        private FieldUpgradeSelectedPinViewData BuildSelectedPin(PinTypeData pinType)
        {
            if (pinType == null)
            {
                return null;
            }

            return new FieldUpgradeSelectedPinViewData
            {
                PinTypeId = pinType.Id,
                DisplayName = pinType.DisplayName,
                Sprite = pinType.FieldSprite,
                ModifierLines = BuildModifierLines(pinType)
            };
        }

        private static List<PinModifierLineViewData> BuildModifierLines(PinTypeData pinType)
        {
            var lines = new List<PinModifierLineViewData>();
            if (pinType == null)
            {
                return lines;
            }

            if (pinType.AttackModifier != 0)
            {
                lines.Add(new PinModifierLineViewData
                {
                    Label = "ATK",
                    Value = pinType.AttackModifier
                });
            }

            if (pinType.HealthModifier != 0)
            {
                lines.Add(new PinModifierLineViewData
                {
                    Label = "HP",
                    Value = pinType.HealthModifier
                });
            }

            if (pinType.ManaModifier != 0)
            {
                lines.Add(new PinModifierLineViewData
                {
                    Label = "Mana",
                    Value = pinType.ManaModifier
                });
            }

            if (System.Math.Abs(pinType.MoveSpeedModifier) > 0.001f)
            {
                lines.Add(new PinModifierLineViewData
                {
                    Label = "Move",
                    DisplayValue = FormatSignedFloat(pinType.MoveSpeedModifier)
                });
            }

            if (pinType.AttackRangeModifier != 0)
            {
                lines.Add(new PinModifierLineViewData
                {
                    Label = "Range",
                    Value = pinType.AttackRangeModifier
                });
            }

            if (System.Math.Abs(pinType.AttackSpeedModifier) > 0.001f)
            {
                lines.Add(new PinModifierLineViewData
                {
                    Label = "ASPD",
                    DisplayValue = FormatSignedFloat(pinType.AttackSpeedModifier)
                });
            }

            return lines;
        }

        private static string FormatSignedFloat(float value)
        {
            return value > 0f ? $"+{value:0.##}" : value.ToString("0.##");
        }

        private static Dictionary<int, PinTypeData> BuildAuthoredPinsBySlot(PlinkoFieldSettingsData fieldData)
        {
            var result = new Dictionary<int, PinTypeData>();
            if (fieldData == null || fieldData.Rows == null)
            {
                return result;
            }

            var slotIndex = 0;
            for (var rowIndex = 0; rowIndex < fieldData.Rows.Count; rowIndex++)
            {
                var row = fieldData.Rows[rowIndex];
                if (row == null || row.Cells == null)
                {
                    continue;
                }

                for (var columnIndex = 0; columnIndex < row.Cells.Count; columnIndex++)
                {
                    result[slotIndex] = row.Cells[columnIndex] != null ? row.Cells[columnIndex].PinType : null;
                    slotIndex++;
                }
            }

            return result;
        }

        private static LevelData FindNextBattleLevel(LocationData locationData, int currentLevelIndex)
        {
            if (locationData == null || locationData.Levels == null)
            {
                return null;
            }

            for (var index = currentLevelIndex + 1; index < locationData.Levels.Count; index++)
            {
                var level = locationData.Levels[index];
                if (level != null && IsBattleLevel(level.LevelType))
                {
                    return level;
                }
            }

            if (currentLevelIndex + 1 >= 0 && currentLevelIndex + 1 < locationData.Levels.Count)
            {
                return locationData.Levels[currentLevelIndex + 1];
            }

            return currentLevelIndex >= 0 && currentLevelIndex < locationData.Levels.Count
                ? locationData.Levels[currentLevelIndex]
                : null;
        }

        private static string BuildFieldSignature(string locationId, int levelIndex, IReadOnlyList<BoardSlotViewData> slots)
        {
            var builder = new StringBuilder();
            builder.Append(locationId)
                .Append('|')
                .Append(levelIndex)
                .Append('|');

            for (var index = 0; index < slots.Count; index++)
            {
                var slot = slots[index];
                builder.Append(slot.SlotIndex)
                    .Append(':')
                    .Append(slot.PinTypeId)
                    .Append(';');
            }

            return builder.ToString();
        }

        private static bool IsBattleLevel(Enums.LevelType levelType)
        {
            return levelType == Enums.LevelType.StandardBattle ||
                   levelType == Enums.LevelType.DefenceBattle ||
                   levelType == Enums.LevelType.PowerLineBattle;
        }
    }
}
