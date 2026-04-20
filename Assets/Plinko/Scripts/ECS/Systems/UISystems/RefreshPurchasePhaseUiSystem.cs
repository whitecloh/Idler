using System.Collections.Generic;
using System.Text;
using Leopotam.EcsLite;
using Plinko.Scripts.Data.Common;
using Plinko.Scripts.Data.Levels;
using Plinko.Scripts.Data.Locations;
using Plinko.Scripts.Data.Pins;
using Plinko.Scripts.Data.Units;
using Plinko.Scripts.ECS.Components;
using Plinko.Scripts.ECS.Events;
using Plinko.Scripts.ECS.Indexes;
using Plinko.Scripts.Models;
using Plinko.Scripts.Models.ViewData;
using Plinko.Scripts.Services;
using Plinko.Scripts.View;

namespace Plinko.Scripts.ECS.Systems.UISystems
{
    public sealed class RefreshPurchasePhaseUiSystem : IEcsInitSystem, IEcsRunSystem
    {
        private readonly GameSettingsService _gameSettingsService;
        private readonly UnitConfigService _unitConfigService;
        private readonly LocationConfigService _locationConfigService;
        private readonly LevelConfigService _levelConfigService;
        private readonly PlinkoConfigService _plinkoConfigService;
        private readonly PinConfigService _pinConfigService;
        private readonly PlinkoRuntimeService _plinkoRuntimeService;
        private readonly RunEntityIndex _runEntityIndex;
        private readonly UiCompositionRoot _uiCompositionRoot;

        private EcsPool<CurrentPhaseComponent> _phasePool;
        private EcsPool<CurrentGoldComponent> _goldPool;
        private EcsPool<PurchasePhaseStateComponent> _purchaseStatePool;
        private EcsPool<CurrentLocationComponent> _locationPool;
        private EcsPool<CurrentLevelComponent> _levelPool;
        private EcsPool<PlayerBaseHealthComponent> _playerBasePool;
        private EcsPool<UnitShopOfferComponent> _offerPool;
        private EcsPool<OfferPriceComponent> _pricePool;
        private EcsPool<ShopOfferUnitTypeIdComponent> _offerUnitTypePool;
        private EcsPool<OwnedUnitComponent> _ownedUnitPool;
        private EcsPool<UnitDisplayNameComponent> _displayNamePool;
        private EcsPool<UnitLevelComponent> _unitLevelPool;
        private EcsPool<UnitTypeIdComponent> _unitTypePool;
        private EcsPool<UnitStatsComponent> _statsPool;
        private EcsPool<UnitCombatStatsComponent> _unitCombatStatsPool;
        private EcsPool<UnitManaCostComponent> _manaCostPool;
        private EcsPool<UpgradeCountComponent> _upgradeCountPool;
        private EcsPool<InstalledPinComponent> _installedPinPool;
        private EcsPool<StagedTraineeComponent> _stagedPool;
        private EcsPool<PlinkoTrainingPlaybackComponent> _playbackPool;
        private EcsPool<UnitPurchasedEvent> _unitPurchasedEventPool;
        private EcsPool<OwnedUnitRegisteredEvent> _ownedUnitRegisteredEventPool;

        private EcsFilter _offerFilter;
        private EcsFilter _ownedFilter;
        private EcsFilter _installedPinFilter;
        private EcsFilter _stagedFilter;
        private EcsFilter _playbackFilter;
        private EcsFilter _unitPurchasedFilter;
        private EcsFilter _ownedUnitRegisteredFilter;

        public RefreshPurchasePhaseUiSystem(
            GameSettingsService gameSettingsService,
            UnitConfigService unitConfigService,
            LocationConfigService locationConfigService,
            LevelConfigService levelConfigService,
            PlinkoConfigService plinkoConfigService,
            PinConfigService pinConfigService,
            PlinkoRuntimeService plinkoRuntimeService,
            RunEntityIndex runEntityIndex,
            UiCompositionRoot uiCompositionRoot)
        {
            _gameSettingsService = gameSettingsService;
            _unitConfigService = unitConfigService;
            _locationConfigService = locationConfigService;
            _levelConfigService = levelConfigService;
            _plinkoConfigService = plinkoConfigService;
            _pinConfigService = pinConfigService;
            _plinkoRuntimeService = plinkoRuntimeService;
            _runEntityIndex = runEntityIndex;
            _uiCompositionRoot = uiCompositionRoot;
        }

        public void Init(IEcsSystems systems)
        {
            var world = systems.GetWorld();
            _phasePool = world.GetPool<CurrentPhaseComponent>();
            _goldPool = world.GetPool<CurrentGoldComponent>();
            _purchaseStatePool = world.GetPool<PurchasePhaseStateComponent>();
            _locationPool = world.GetPool<CurrentLocationComponent>();
            _levelPool = world.GetPool<CurrentLevelComponent>();
            _playerBasePool = world.GetPool<PlayerBaseHealthComponent>();
            _offerPool = world.GetPool<UnitShopOfferComponent>();
            _pricePool = world.GetPool<OfferPriceComponent>();
            _offerUnitTypePool = world.GetPool<ShopOfferUnitTypeIdComponent>();
            _ownedUnitPool = world.GetPool<OwnedUnitComponent>();
            _displayNamePool = world.GetPool<UnitDisplayNameComponent>();
            _unitLevelPool = world.GetPool<UnitLevelComponent>();
            _unitTypePool = world.GetPool<UnitTypeIdComponent>();
            _statsPool = world.GetPool<UnitStatsComponent>();
            _unitCombatStatsPool = world.GetPool<UnitCombatStatsComponent>();
            _manaCostPool = world.GetPool<UnitManaCostComponent>();
            _upgradeCountPool = world.GetPool<UpgradeCountComponent>();
            _installedPinPool = world.GetPool<InstalledPinComponent>();
            _stagedPool = world.GetPool<StagedTraineeComponent>();
            _playbackPool = world.GetPool<PlinkoTrainingPlaybackComponent>();
            _unitPurchasedEventPool = world.GetPool<UnitPurchasedEvent>();
            _ownedUnitRegisteredEventPool = world.GetPool<OwnedUnitRegisteredEvent>();

            _offerFilter = world.Filter<UnitShopOfferComponent>().Inc<OfferPriceComponent>().Inc<ShopOfferUnitTypeIdComponent>().End();
            _ownedFilter = world.Filter<OwnedUnitComponent>().End();
            _installedPinFilter = world.Filter<InstalledPinComponent>().End();
            _stagedFilter = world.Filter<StagedTraineeComponent>().Inc<UnitTypeIdComponent>().Inc<UnitDisplayNameComponent>().End();
            _playbackFilter = world.Filter<PlinkoTrainingPlaybackComponent>().End();
            _unitPurchasedFilter = world.Filter<UnitPurchasedEvent>().End();
            _ownedUnitRegisteredFilter = world.Filter<OwnedUnitRegisteredEvent>().End();
        }

        public void Run(IEcsSystems systems)
        {
            if (_uiCompositionRoot == null)
            {
                return;
            }

            if (!_runEntityIndex.TryGetRunEntity(out var runEntity) ||
                !_phasePool.Has(runEntity) ||
                _phasePool.Get(runEntity).Value != Enums.PhaseType.PurchasePhase)
            {
                _uiCompositionRoot.RefreshPurchasePhase(new PurchasePhaseViewData());
                return;
            }

            var locationId = _locationPool.Get(runEntity).LocationId;
            var levelIndex = _levelPool.Get(runEntity).LevelIndex;
            var locationData = _locationConfigService.GetLocation(locationId);
            var levelData = _levelConfigService.GetLevel(locationId, levelIndex);
            var fieldData = _plinkoConfigService.GetField(locationData, levelData);
            var installedPins = BuildInstalledPinLookup();
            var stagedByRuntimeId = BuildStagedLookup();
            var ownedByRuntimeId = BuildOwnedUnitLookup();
            var nextBattleLevel = FindNextBattleLevel(locationData, levelIndex) ?? levelData;

            var rerollPrice = _gameSettingsService.GetUnitShopRerollPrice();
            var gold = _goldPool.Get(runEntity).Value;
            var purchaseState = _purchaseStatePool.Get(runEntity);
            var playerBase = _playerBasePool.Get(runEntity);
            var pins = BuildFieldPins(fieldData, installedPins);
            var baskets = BuildFieldBaskets(fieldData);

            var viewData = new PurchasePhaseViewData
            {
                LevelKey = $"{locationId}:{levelIndex}",
                LocationDisplayName = locationData != null && !string.IsNullOrWhiteSpace(locationData.DisplayName)
                    ? locationData.DisplayName
                    : locationId,
                FieldSignature = BuildFieldSignature(locationId, levelIndex, pins, baskets),
                FieldHorizontalSpacing = fieldData != null ? fieldData.HorizontalSpacing : 1f,
                FieldVerticalSpacing = fieldData != null ? fieldData.VerticalSpacing : 1f,
                Gold = gold,
                RerollCount = purchaseState.RerollCount,
                RerollPrice = rerollPrice,
                CanReroll = gold >= rerollPrice,
                CanStartBattle = false,
                CanAdvance = purchaseState.ActiveTrainingCount <= 0,
                PrimaryActionLabel = "Next Level",
                ActiveTrainingCount = purchaseState.ActiveTrainingCount,
                PlayerBaseHealth = playerBase.Value,
                PlayerBaseMaxHealth = playerBase.MaxValue,
                NextBattleBackgroundSprite = nextBattleLevel != null ? nextBattleLevel.BackgroundSprite : null,
                PlayerBaseSprite = nextBattleLevel != null ? nextBattleLevel.PlayerBaseSprite : null,
                Offers = BuildOffers(),
                Levels = BuildLevelProgress(locationData, levelIndex),
                Pins = pins,
                Baskets = baskets,
                ActiveTrainings = BuildActiveTrainings(stagedByRuntimeId),
                StartedTrainings = BuildStartedTrainings(),
                CompletedTrainings = BuildCompletedTrainings(ownedByRuntimeId),
                ArmyPreviewUnits = BuildArmyPreviewUnits(ownedByRuntimeId)
            };

            _uiCompositionRoot.RefreshPurchasePhase(viewData);
        }

        private List<UnitShopOfferViewData> BuildOffers()
        {
            var offers = new List<UnitShopOfferViewData>();
            foreach (var offerEntity in _offerFilter)
            {
                var unitTypeId = _offerUnitTypePool.Get(offerEntity).Value;
                var unitType = _unitConfigService.GetUnit(unitTypeId);
                offers.Add(new UnitShopOfferViewData
                {
                    OfferId = _offerPool.Get(offerEntity).OfferId,
                    UnitTypeId = unitTypeId,
                    DisplayName = unitType != null && !string.IsNullOrWhiteSpace(unitType.DisplayName)
                        ? unitType.DisplayName
                        : unitTypeId,
                    PortraitSprite = unitType != null ? unitType.PortraitSprite : null,
                    Attack = unitType != null ? unitType.BaseAttack : 0,
                    Health = unitType != null ? unitType.BaseHealth : 0,
                    ManaCost = unitType != null ? unitType.DefaultManaCost : 0,
                    MoveSpeed = unitType != null ? unitType.BaseMoveSpeed : 0f,
                    AttackRange = unitType != null ? unitType.BattleAttackRange : 0,
                    AttackSpeed = unitType != null ? unitType.BaseAttackSpeed : 0f,
                    Price = _pricePool.Get(offerEntity).Value
                });
            }

            offers.Sort((left, right) => left.OfferId.CompareTo(right.OfferId));
            return offers;
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

        private List<PurchaseFieldPinViewData> BuildFieldPins(
            PlinkoFieldSettingsData fieldData,
            Dictionary<int, PinTypeData> installedPins)
        {
            var result = new List<PurchaseFieldPinViewData>();
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
                    var authoredPin = row.Cells[columnIndex] != null ? row.Cells[columnIndex].PinType : null;
                    var pin = installedPins.TryGetValue(slotIndex, out var runtimePin) ? runtimePin : authoredPin;
                    result.Add(new PurchaseFieldPinViewData
                    {
                        SlotIndex = slotIndex,
                        RowIndex = rowIndex,
                        ColumnIndex = columnIndex,
                        PinTypeId = pin != null ? pin.Id : string.Empty,
                        DisplayName = pin != null ? pin.DisplayName : string.Empty,
                        Sprite = pin != null ? pin.FieldSprite : null
                    });
                    slotIndex++;
                }
            }

            return result;
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

        private List<PurchaseTrainingRunViewData> BuildActiveTrainings(
            Dictionary<int, StagedSnapshot> stagedByRuntimeId)
        {
            var result = new List<PurchaseTrainingRunViewData>();
            foreach (var playbackEntity in _playbackFilter)
            {
                var playback = _playbackPool.Get(playbackEntity);
                if (playback.IsRetraining)
                {
                    continue;
                }

                if (!_plinkoRuntimeService.TryGetResult(playback.RuntimeId, out var plinkoResult) ||
                    plinkoResult == null)
                {
                    continue;
                }

                stagedByRuntimeId.TryGetValue(playback.RuntimeId, out var staged);
                var unitTypeId = staged != null && !string.IsNullOrWhiteSpace(staged.UnitTypeId)
                    ? staged.UnitTypeId
                    : plinkoResult.Result != null
                        ? plinkoResult.Result.UnitTypeId
                        : string.Empty;
                var unitType = _unitConfigService.GetUnit(unitTypeId);
                var run = new PurchaseTrainingRunViewData
                {
                    RuntimeId = playback.RuntimeId,
                    SourceOfferId = staged != null ? staged.SourceOfferId : -1,
                    DisplayName = plinkoResult.Result != null ? plinkoResult.Result.DisplayName : string.Empty,
                    TrainingFieldSprite = unitType != null ? unitType.TrainingFieldSprite : null,
                    HasStarted = playback.HasStarted,
                    Elapsed = playback.Elapsed,
                    Duration = playback.Duration,
                    CurrentNodeIndex = playback.CurrentNodeIndex,
                    TotalNodeCount = playback.TotalNodeCount,
                    FinalBasketId = plinkoResult.FinalBasketId
                };

                if (plinkoResult.Nodes != null)
                {
                    for (var index = 0; index < plinkoResult.Nodes.Count; index++)
                    {
                        var node = plinkoResult.Nodes[index];
                        if (node == null)
                        {
                            continue;
                        }

                        run.Nodes.Add(new PurchaseTrainingNodeViewData
                        {
                            RowIndex = node.RowIndex,
                            ColumnIndex = node.ColumnIndex,
                            PinTypeId = node.PinTypeId
                        });
                    }
                }

                result.Add(run);
            }

            result.Sort((left, right) => left.RuntimeId.CompareTo(right.RuntimeId));
            return result;
        }

        private List<PurchaseTrainingStartedViewData> BuildStartedTrainings()
        {
            var result = new List<PurchaseTrainingStartedViewData>();
            foreach (var eventEntity in _unitPurchasedFilter)
            {
                var purchaseEvent = _unitPurchasedEventPool.Get(eventEntity);
                result.Add(new PurchaseTrainingStartedViewData
                {
                    RuntimeId = purchaseEvent.RuntimeId,
                    SourceOfferId = purchaseEvent.OfferId
                });
            }

            result.Sort((left, right) => left.RuntimeId.CompareTo(right.RuntimeId));
            return result;
        }

        private List<PurchaseTrainedUnitCardViewData> BuildCompletedTrainings(
            Dictionary<int, int> ownedByRuntimeId)
        {
            var result = new List<PurchaseTrainedUnitCardViewData>();
            foreach (var eventEntity in _ownedUnitRegisteredFilter)
            {
                var runtimeId = _ownedUnitRegisteredEventPool.Get(eventEntity).RuntimeId;
                if (!ownedByRuntimeId.TryGetValue(runtimeId, out var ownedEntity))
                {
                    continue;
                }

                var unitTypeId = _unitTypePool.Get(ownedEntity).Value;
                var unitType = _unitConfigService.GetUnit(unitTypeId);
                result.Add(new PurchaseTrainedUnitCardViewData
                {
                    RuntimeId = runtimeId,
                    UnitTypeId = unitTypeId,
                    DisplayName = _displayNamePool.Get(ownedEntity).Value,
                    PortraitSprite = unitType != null ? unitType.PortraitSprite : null,
                    Attack = _statsPool.Get(ownedEntity).Attack,
                    Health = _statsPool.Get(ownedEntity).Health,
                    ManaCost = _manaCostPool.Get(ownedEntity).Value,
                    MoveSpeed = _unitCombatStatsPool.Get(ownedEntity).MoveSpeed,
                    AttackRange = _unitCombatStatsPool.Get(ownedEntity).AttackRange,
                    AttackSpeed = _unitCombatStatsPool.Get(ownedEntity).AttackSpeed,
                    Level = _unitLevelPool.Get(ownedEntity).Value,
                    UpgradeCount = _upgradeCountPool.Get(ownedEntity).Value
                });
            }

            result.Sort((left, right) => left.RuntimeId.CompareTo(right.RuntimeId));
            return result;
        }

        private List<PurchaseArmyPreviewUnitViewData> BuildArmyPreviewUnits(
            Dictionary<int, int> ownedByRuntimeId)
        {
            var result = new List<PurchaseArmyPreviewUnitViewData>();
            var runtimeIds = new List<int>(ownedByRuntimeId.Keys);
            runtimeIds.Sort();

            for (var index = 0; index < runtimeIds.Count; index++)
            {
                var runtimeId = runtimeIds[index];
                var ownedEntity = ownedByRuntimeId[runtimeId];
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

        private Dictionary<int, PinTypeData> BuildInstalledPinLookup()
        {
            var result = new Dictionary<int, PinTypeData>();
            foreach (var pinEntity in _installedPinFilter)
            {
                var installedPin = _installedPinPool.Get(pinEntity);
                var pinType = _pinConfigService.GetPin(installedPin.PinTypeId);
                if (pinType != null)
                {
                    result[installedPin.SlotIndex] = pinType;
                }
            }

            return result;
        }

        private Dictionary<int, StagedSnapshot> BuildStagedLookup()
        {
            var result = new Dictionary<int, StagedSnapshot>();
            foreach (var stagedEntity in _stagedFilter)
            {
                var staged = _stagedPool.Get(stagedEntity);
                result[staged.RuntimeId] = new StagedSnapshot
                {
                    RuntimeId = staged.RuntimeId,
                    SourceOfferId = staged.SourceOfferId,
                    UnitTypeId = _unitTypePool.Get(stagedEntity).Value
                };
            }

            return result;
        }

        private Dictionary<int, int> BuildOwnedUnitLookup()
        {
            var result = new Dictionary<int, int>();
            foreach (var ownedEntity in _ownedFilter)
            {
                result[_ownedUnitPool.Get(ownedEntity).RuntimeId] = ownedEntity;
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

        private static string BuildFieldSignature(
            string locationId,
            int levelIndex,
            IReadOnlyList<PurchaseFieldPinViewData> pins,
            IReadOnlyList<PurchaseFieldBasketViewData> baskets)
        {
            var builder = new StringBuilder();
            builder.Append(locationId)
                .Append('|')
                .Append(levelIndex)
                .Append('|');

            for (var index = 0; index < pins.Count; index++)
            {
                var pin = pins[index];
                builder.Append(pin.SlotIndex)
                    .Append(':')
                    .Append(pin.PinTypeId)
                    .Append(';');
            }

            builder.Append('|');
            for (var index = 0; index < baskets.Count; index++)
            {
                var basket = baskets[index];
                builder.Append(basket.BasketId)
                    .Append(':')
                    .Append(basket.ManaValue)
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

        private sealed class StagedSnapshot
        {
            public int RuntimeId;
            public int SourceOfferId;
            public string UnitTypeId;
        }
    }
}
