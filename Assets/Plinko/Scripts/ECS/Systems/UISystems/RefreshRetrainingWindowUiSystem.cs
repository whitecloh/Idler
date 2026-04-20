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
    public sealed class RefreshRetrainingWindowUiSystem : IEcsInitSystem, IEcsRunSystem
    {
        private readonly GameSettingsService _gameSettingsService;
        private readonly UnitConfigService _unitConfigService;
        private readonly StatTypeConfigService _statTypeConfigService;
        private readonly LocationConfigService _locationConfigService;
        private readonly LevelConfigService _levelConfigService;
        private readonly PlinkoConfigService _plinkoConfigService;
        private readonly PinConfigService _pinConfigService;
        private readonly PlinkoRuntimeService _plinkoRuntimeService;
        private readonly RunEntityIndex _runEntityIndex;
        private readonly UiCompositionRoot _uiCompositionRoot;

        private EcsPool<CurrentPhaseComponent> _phasePool;
        private EcsPool<CurrentGoldComponent> _goldPool;
        private EcsPool<RetrainingPhaseStateComponent> _retrainingStatePool;
        private EcsPool<CurrentLocationComponent> _locationPool;
        private EcsPool<CurrentLevelComponent> _currentLevelPool;
        private EcsPool<PlayerBaseHealthComponent> _playerBasePool;
        private EcsPool<OwnedUnitComponent> _ownedUnitPool;
        private EcsPool<RetrainingPurchasedOnLevelComponent> _purchasedOnLevelPool;
        private EcsPool<RetrainingShopOfferComponent> _retrainingOfferPool;
        private EcsPool<RetrainingOfferOwnerUnitComponent> _offerOwnerPool;
        private EcsPool<UnitDisplayNameComponent> _displayNamePool;
        private EcsPool<UnitLevelComponent> _unitLevelPool;
        private EcsPool<UnitTypeIdComponent> _unitTypePool;
        private EcsPool<UnitStatsComponent> _statsPool;
        private EcsPool<UnitCombatStatsComponent> _unitCombatStatsPool;
        private EcsPool<UnitManaCostComponent> _manaCostPool;
        private EcsPool<UpgradeCountComponent> _upgradeCountPool;
        private EcsPool<OfferPriceComponent> _pricePool;
        private EcsPool<InstalledPinComponent> _installedPinPool;
        private EcsPool<StagedTraineeComponent> _stagedPool;
        private EcsPool<PlinkoTrainingPlaybackComponent> _playbackPool;
        private EcsPool<TrainingPlaybackStartedEvent> _trainingPlaybackStartedEventPool;
        private EcsPool<OwnedUnitReplacedEvent> _ownedUnitReplacedEventPool;

        private EcsFilter _ownedFilter;
        private EcsFilter _offerFilter;
        private EcsFilter _installedPinFilter;
        private EcsFilter _stagedFilter;
        private EcsFilter _playbackFilter;
        private EcsFilter _trainingPlaybackStartedFilter;
        private EcsFilter _ownedUnitReplacedFilter;

        public RefreshRetrainingWindowUiSystem(
            GameSettingsService gameSettingsService,
            UnitConfigService unitConfigService,
            StatTypeConfigService statTypeConfigService,
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
            _statTypeConfigService = statTypeConfigService;
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
            _retrainingStatePool = world.GetPool<RetrainingPhaseStateComponent>();
            _locationPool = world.GetPool<CurrentLocationComponent>();
            _currentLevelPool = world.GetPool<CurrentLevelComponent>();
            _playerBasePool = world.GetPool<PlayerBaseHealthComponent>();
            _ownedUnitPool = world.GetPool<OwnedUnitComponent>();
            _purchasedOnLevelPool = world.GetPool<RetrainingPurchasedOnLevelComponent>();
            _retrainingOfferPool = world.GetPool<RetrainingShopOfferComponent>();
            _offerOwnerPool = world.GetPool<RetrainingOfferOwnerUnitComponent>();
            _displayNamePool = world.GetPool<UnitDisplayNameComponent>();
            _unitLevelPool = world.GetPool<UnitLevelComponent>();
            _unitTypePool = world.GetPool<UnitTypeIdComponent>();
            _statsPool = world.GetPool<UnitStatsComponent>();
            _unitCombatStatsPool = world.GetPool<UnitCombatStatsComponent>();
            _manaCostPool = world.GetPool<UnitManaCostComponent>();
            _upgradeCountPool = world.GetPool<UpgradeCountComponent>();
            _pricePool = world.GetPool<OfferPriceComponent>();
            _installedPinPool = world.GetPool<InstalledPinComponent>();
            _stagedPool = world.GetPool<StagedTraineeComponent>();
            _playbackPool = world.GetPool<PlinkoTrainingPlaybackComponent>();
            _trainingPlaybackStartedEventPool = world.GetPool<TrainingPlaybackStartedEvent>();
            _ownedUnitReplacedEventPool = world.GetPool<OwnedUnitReplacedEvent>();

            _ownedFilter = world.Filter<OwnedUnitComponent>().End();
            _offerFilter = world.Filter<RetrainingShopOfferComponent>().Inc<RetrainingOfferOwnerUnitComponent>().End();
            _installedPinFilter = world.Filter<InstalledPinComponent>().End();
            _stagedFilter = world.Filter<StagedTraineeComponent>().Inc<UnitTypeIdComponent>().Inc<UnitDisplayNameComponent>().End();
            _playbackFilter = world.Filter<PlinkoTrainingPlaybackComponent>().End();
            _trainingPlaybackStartedFilter = world.Filter<TrainingPlaybackStartedEvent>().End();
            _ownedUnitReplacedFilter = world.Filter<OwnedUnitReplacedEvent>().End();
        }

        public void Run(IEcsSystems systems)
        {
            if (!_runEntityIndex.TryGetRunEntity(out var runEntity) ||
                !_phasePool.Has(runEntity) ||
                _phasePool.Get(runEntity).Value != Enums.PhaseType.RetrainingPhase)
            {
                _uiCompositionRoot.RefreshRetrainingPhase(new RetrainingPhaseViewData());
                return;
            }

            var locationId = _locationPool.Get(runEntity).LocationId;
            var levelIndex = _currentLevelPool.Get(runEntity).LevelIndex;
            var locationData = _locationConfigService.GetLocation(locationId);
            var levelData = _levelConfigService.GetLevel(locationId, levelIndex);
            var fieldData = _plinkoConfigService.GetField(locationData, levelData);
            var installedPins = BuildInstalledPinLookup();
            var stagedByRuntimeId = BuildStagedLookup();
            var ownedByRuntimeId = BuildOwnedUnitLookup();
            var ownedEntries = BuildOwnedEntries(ownedByRuntimeId);
            var offers = BuildOffers();
            var offerRuntimeIds = BuildOfferRuntimeIdSet(offers);
            var activeTrainings = BuildActiveTrainings(stagedByRuntimeId);
            var activeTrainingRuntimeIds = BuildActiveTrainingRuntimeIdSet(activeTrainings);
            var nextBattleLevel = FindNextBattleLevel(locationData, levelIndex) ?? levelData;
            var retrainingState = _retrainingStatePool.Get(runEntity);
            var gold = _goldPool.Get(runEntity).Value;
            var rerollPrice = _gameSettingsService.GetRetrainingShopRerollPrice();
            var playerBase = _playerBasePool.Get(runEntity);
            var pins = BuildFieldPins(fieldData, installedPins);
            var baskets = BuildFieldBaskets(fieldData);
            var batchPrice = 0;
            for (var index = 0; index < offers.Count; index++)
            {
                batchPrice += offers[index].Price;
            }

            var eligibleCount = 0;
            for (var index = 0; index < ownedEntries.Count; index++)
            {
                if (!_purchasedOnLevelPool.Has(ownedEntries[index].Entity))
                {
                    eligibleCount++;
                }
            }

            var viewData = new RetrainingPhaseViewData
            {
                LevelKey = $"{locationId}:{levelIndex}",
                LocationDisplayName = locationData != null && !string.IsNullOrWhiteSpace(locationData.DisplayName) ? locationData.DisplayName : locationId,
                FieldSignature = BuildFieldSignature(locationId, levelIndex, pins, baskets),
                FieldHorizontalSpacing = fieldData != null ? fieldData.HorizontalSpacing : 1f,
                FieldVerticalSpacing = fieldData != null ? fieldData.VerticalSpacing : 1f,
                OfferCount = retrainingState.OfferCount,
                EligibleCount = eligibleCount,
                CurrentOfferCount = offers.Count,
                CurrentGold = gold,
                BatchPrice = batchPrice,
                RerollCount = retrainingState.RerollCount,
                RerollPrice = rerollPrice,
                CanBuyBatch = retrainingState.ActiveTrainingCount <= 0 && offers.Count > 0 && gold >= batchPrice,
                CanReroll = retrainingState.ActiveTrainingCount <= 0 && eligibleCount > offers.Count && gold >= rerollPrice,
                CanAdvance = retrainingState.ActiveTrainingCount <= 0,
                IsInteractionLocked = retrainingState.ActiveTrainingCount > 0,
                ActiveTrainingCount = retrainingState.ActiveTrainingCount,
                PlayerBaseHealth = playerBase.Value,
                PlayerBaseMaxHealth = playerBase.MaxValue,
                PrimaryActionLabel = "Next Level",
                NextBattleBackgroundSprite = nextBattleLevel != null ? nextBattleLevel.BackgroundSprite : null,
                PlayerBaseSprite = nextBattleLevel != null ? nextBattleLevel.PlayerBaseSprite : null,
                Offers = offers,
                Levels = BuildLevelProgress(locationData, levelIndex),
                Pins = pins,
                Baskets = baskets,
                ActiveTrainings = activeTrainings,
                StartedTrainings = BuildStartedTrainings(stagedByRuntimeId),
                CompletedTrainings = BuildCompletedTrainings(ownedByRuntimeId),
                AllOwnedArmyPreviewUnits = BuildArmyPreviewUnits(ownedEntries, null, null, null),
                PendingArmyPreviewUnits = BuildArmyPreviewUnits(ownedEntries, offerRuntimeIds, activeTrainingRuntimeIds, false),
                RetrainedArmyPreviewUnits = BuildArmyPreviewUnits(ownedEntries, offerRuntimeIds, activeTrainingRuntimeIds, true)
            };

            _uiCompositionRoot.RefreshRetrainingPhase(viewData);
        }

        private List<RetrainingOfferViewData> BuildOffers()
        {
            var offers = new List<RetrainingOfferViewData>();
            foreach (var offerEntity in _offerFilter)
            {
                var unitTypeId = _unitTypePool.Get(offerEntity).Value;
                var unitType = _unitConfigService.GetUnit(unitTypeId);
                offers.Add(new RetrainingOfferViewData
                {
                    OfferSlotIndex = _retrainingOfferPool.Get(offerEntity).OfferSlotIndex,
                    RuntimeId = _offerOwnerPool.Get(offerEntity).RuntimeId,
                    DisplayName = _displayNamePool.Get(offerEntity).Value,
                    UnitTypeId = unitTypeId,
                    PortraitSprite = unitType != null ? unitType.PortraitSprite : null,
                    Level = _unitLevelPool.Get(offerEntity).Value,
                    Attack = _statsPool.Get(offerEntity).Attack,
                    Health = _statsPool.Get(offerEntity).Health,
                      ManaCost = _manaCostPool.Get(offerEntity).Value,
                      MoveSpeed = _unitCombatStatsPool.Get(offerEntity).MoveSpeed,
                      AttackRange = _unitCombatStatsPool.Get(offerEntity).AttackRange,
                      AttackSpeed = _unitCombatStatsPool.Get(offerEntity).AttackSpeed,
                      UpgradeCount = _upgradeCountPool.Get(offerEntity).Value,
                      Price = _pricePool.Get(offerEntity).Value,
                      Stats = StatViewDataFactory.BuildUnitStats(
                          _statTypeConfigService,
                          unitType,
                          _statsPool.Get(offerEntity).Attack,
                          _statsPool.Get(offerEntity).Health,
                          _manaCostPool.Get(offerEntity).Value,
                          _unitCombatStatsPool.Get(offerEntity).MoveSpeed,
                          _unitCombatStatsPool.Get(offerEntity).AttackRange,
                          _unitCombatStatsPool.Get(offerEntity).AttackSpeed)
                  });
            }

            offers.Sort((left, right) => left.OfferSlotIndex.CompareTo(right.OfferSlotIndex));
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

        private List<PurchaseFieldPinViewData> BuildFieldPins(PlinkoFieldSettingsData fieldData, Dictionary<int, PinTypeData> installedPins)
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

        private List<PurchaseTrainingRunViewData> BuildActiveTrainings(Dictionary<int, StagedSnapshot> stagedByRuntimeId)
        {
            var result = new List<PurchaseTrainingRunViewData>();
            foreach (var playbackEntity in _playbackFilter)
            {
                var playback = _playbackPool.Get(playbackEntity);
                if (!playback.IsRetraining || !_plinkoRuntimeService.TryGetResult(playback.RuntimeId, out var plinkoResult) || plinkoResult == null)
                {
                    continue;
                }

                stagedByRuntimeId.TryGetValue(playback.RuntimeId, out var staged);
                var unitTypeId = staged != null ? staged.UnitTypeId : plinkoResult.Result != null ? plinkoResult.Result.UnitTypeId : string.Empty;
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

            result.Sort((left, right) => left.SourceOfferId.CompareTo(right.SourceOfferId));
            return result;
        }

        private List<PurchaseTrainingStartedViewData> BuildStartedTrainings(Dictionary<int, StagedSnapshot> stagedByRuntimeId)
        {
            var result = new List<PurchaseTrainingStartedViewData>();
            foreach (var eventEntity in _trainingPlaybackStartedFilter)
            {
                var runtimeId = _trainingPlaybackStartedEventPool.Get(eventEntity).RuntimeId;
                if (!stagedByRuntimeId.TryGetValue(runtimeId, out var staged))
                {
                    continue;
                }

                result.Add(new PurchaseTrainingStartedViewData
                {
                    RuntimeId = runtimeId,
                    SourceOfferId = staged.SourceOfferId
                });
            }

            result.Sort((left, right) => left.SourceOfferId.CompareTo(right.SourceOfferId));
            return result;
        }

        private List<PurchaseTrainedUnitCardViewData> BuildCompletedTrainings(Dictionary<int, int> ownedByRuntimeId)
        {
            var result = new List<PurchaseTrainedUnitCardViewData>();
            foreach (var eventEntity in _ownedUnitReplacedFilter)
            {
                var runtimeId = _ownedUnitReplacedEventPool.Get(eventEntity).RuntimeId;
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
                      UpgradeCount = _upgradeCountPool.Get(ownedEntity).Value,
                      Stats = StatViewDataFactory.BuildUnitStats(
                          _statTypeConfigService,
                          unitType,
                          _statsPool.Get(ownedEntity).Attack,
                          _statsPool.Get(ownedEntity).Health,
                          _manaCostPool.Get(ownedEntity).Value,
                          _unitCombatStatsPool.Get(ownedEntity).MoveSpeed,
                          _unitCombatStatsPool.Get(ownedEntity).AttackRange,
                          _unitCombatStatsPool.Get(ownedEntity).AttackSpeed)
                  });
            }

            result.Sort((left, right) => left.RuntimeId.CompareTo(right.RuntimeId));
            return result;
        }

        private List<PurchaseArmyPreviewUnitViewData> BuildArmyPreviewUnits(
            IReadOnlyList<OwnedEntry> ownedEntries,
            HashSet<int> offerRuntimeIds,
            HashSet<int> activeTrainingRuntimeIds,
            bool? purchasedOnLevel)
        {
            var result = new List<PurchaseArmyPreviewUnitViewData>();
            for (var index = 0; index < ownedEntries.Count; index++)
            {
                var ownedEntry = ownedEntries[index];
                if (offerRuntimeIds != null && offerRuntimeIds.Contains(ownedEntry.RuntimeId))
                {
                    continue;
                }

                if (activeTrainingRuntimeIds != null && activeTrainingRuntimeIds.Contains(ownedEntry.RuntimeId))
                {
                    continue;
                }

                var isPurchasedOnLevel = _purchasedOnLevelPool.Has(ownedEntry.Entity);
                if (purchasedOnLevel.HasValue && isPurchasedOnLevel != purchasedOnLevel.Value)
                {
                    continue;
                }

                var unitType = _unitConfigService.GetUnit(_unitTypePool.Get(ownedEntry.Entity).Value);
                result.Add(new PurchaseArmyPreviewUnitViewData
                {
                    RuntimeId = ownedEntry.RuntimeId,
                    DisplayName = _displayNamePool.Get(ownedEntry.Entity).Value,
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
                if (!staged.IsRetraining)
                {
                    continue;
                }

                result[staged.RuntimeId] = new StagedSnapshot
                {
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

        private List<OwnedEntry> BuildOwnedEntries(Dictionary<int, int> ownedByRuntimeId)
        {
            var runtimeIds = new List<int>(ownedByRuntimeId.Keys);
            runtimeIds.Sort();
            var result = new List<OwnedEntry>(runtimeIds.Count);
            for (var index = 0; index < runtimeIds.Count; index++)
            {
                result.Add(new OwnedEntry
                {
                    RuntimeId = runtimeIds[index],
                    Entity = ownedByRuntimeId[runtimeIds[index]]
                });
            }

            return result;
        }

        private static HashSet<int> BuildOfferRuntimeIdSet(IReadOnlyList<RetrainingOfferViewData> offers)
        {
            var result = new HashSet<int>();
            for (var index = 0; index < offers.Count; index++)
            {
                result.Add(offers[index].RuntimeId);
            }

            return result;
        }

        private static HashSet<int> BuildActiveTrainingRuntimeIdSet(IReadOnlyList<PurchaseTrainingRunViewData> activeTrainings)
        {
            var result = new HashSet<int>();
            for (var index = 0; index < activeTrainings.Count; index++)
            {
                result.Add(activeTrainings[index].RuntimeId);
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

            return currentLevelIndex >= 0 && currentLevelIndex < locationData.Levels.Count ? locationData.Levels[currentLevelIndex] : null;
        }

        private static string BuildFieldSignature(string locationId, int levelIndex, IReadOnlyList<PurchaseFieldPinViewData> pins, IReadOnlyList<PurchaseFieldBasketViewData> baskets)
        {
            var builder = new StringBuilder();
            builder.Append(locationId).Append('|').Append(levelIndex).Append('|');
            for (var index = 0; index < pins.Count; index++)
            {
                builder.Append(pins[index].SlotIndex).Append(':').Append(pins[index].PinTypeId).Append(';');
            }

            builder.Append('|');
            for (var index = 0; index < baskets.Count; index++)
            {
                builder.Append(baskets[index].BasketId).Append(':').Append(baskets[index].ManaValue).Append(';');
            }

            return builder.ToString();
        }

        private sealed class StagedSnapshot
        {
            public int SourceOfferId;
            public string UnitTypeId;
        }

        private static bool IsBattleLevel(Enums.LevelType levelType)
        {
            return levelType == Enums.LevelType.StandardBattle ||
                   levelType == Enums.LevelType.DefenceBattle ||
                   levelType == Enums.LevelType.PowerLineBattle;
        }

        private sealed class OwnedEntry
        {
            public int RuntimeId;
            public int Entity;
        }
    }
}
