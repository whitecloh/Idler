using System.Collections.Generic;
using Leopotam.EcsLite;
using Plinko.Scripts.Data.Common;
using Plinko.Scripts.ECS.Components;
using Plinko.Scripts.ECS.Events;
using Plinko.Scripts.ECS.Indexes;
using Plinko.Scripts.ECS.Utils;
using Plinko.Scripts.Models;
using Plinko.Scripts.Services;

namespace Plinko.Scripts.ECS.Systems
{
    public sealed class BeginRetrainingSystem : IEcsInitSystem, IEcsRunSystem
    {
        private readonly TrainingPipelineService _trainingPipelineService;
        private readonly UnitConfigService _unitConfigService;
        private readonly RunEntityIndex _runEntityIndex;

        private EcsFilter _purchasedBatchFilter;
        private EcsFilter _offerFilter;
        private EcsFilter _installedPinFilter;
        private EcsPool<CurrentPhaseComponent> _phasePool;
        private EcsPool<CurrentLocationComponent> _locationPool;
        private EcsPool<CurrentLevelComponent> _levelPool;
        private EcsPool<RetrainingPhaseStateComponent> _retrainingStatePool;
        private EcsPool<OwnedUnitComponent> _ownedUnitPool;
        private EcsPool<RetrainingPurchasedOnLevelComponent> _purchasedOnLevelPool;
        private EcsPool<RetrainingShopOfferComponent> _retrainingOfferPool;
        private EcsPool<RetrainingOfferOwnerUnitComponent> _offerOwnerUnitPool;
        private EcsPool<UnitTypeIdComponent> _unitTypeIdPool;
        private EcsPool<UnitDisplayNameComponent> _displayNamePool;
        private EcsPool<UnitStatsComponent> _unitStatsPool;
        private EcsPool<UnitManaCostComponent> _unitManaCostPool;
        private EcsPool<PassiveAbilityIdComponent> _passiveAbilityPool;
        private EcsPool<UnitLevelComponent> _unitLevelPool;
        private EcsPool<UpgradeCountComponent> _upgradeCountPool;
        private EcsPool<InstalledPinComponent> _installedPinPool;
        private EcsPool<StagedTraineeComponent> _stagedPool;
        private EcsPool<PlinkoTrainingPlaybackComponent> _playbackPool;
        private EcsPool<UnitTrainingStartedEvent> _unitTrainingStartedEventPool;
        private EcsPool<TrainingPlaybackStartedEvent> _trainingPlaybackStartedEventPool;
        private EcsPool<RetrainingShopOffersChangedEvent> _offersChangedEventPool;

        public BeginRetrainingSystem(
            TrainingPipelineService trainingPipelineService,
            UnitConfigService unitConfigService,
            RunEntityIndex runEntityIndex)
        {
            _trainingPipelineService = trainingPipelineService;
            _unitConfigService = unitConfigService;
            _runEntityIndex = runEntityIndex;
        }

        public void Init(IEcsSystems systems)
        {
            var world = systems.GetWorld();
            _purchasedBatchFilter = world.Filter<RetrainingBatchPurchasedEvent>().End();
            _offerFilter = world.Filter<RetrainingShopOfferComponent>().Inc<RetrainingOfferOwnerUnitComponent>().End();
            _installedPinFilter = world.Filter<InstalledPinComponent>().End();
            _phasePool = world.GetPool<CurrentPhaseComponent>();
            _locationPool = world.GetPool<CurrentLocationComponent>();
            _levelPool = world.GetPool<CurrentLevelComponent>();
            _retrainingStatePool = world.GetPool<RetrainingPhaseStateComponent>();
            _ownedUnitPool = world.GetPool<OwnedUnitComponent>();
            _purchasedOnLevelPool = world.GetPool<RetrainingPurchasedOnLevelComponent>();
            _retrainingOfferPool = world.GetPool<RetrainingShopOfferComponent>();
            _offerOwnerUnitPool = world.GetPool<RetrainingOfferOwnerUnitComponent>();
            _unitTypeIdPool = world.GetPool<UnitTypeIdComponent>();
            _displayNamePool = world.GetPool<UnitDisplayNameComponent>();
            _unitStatsPool = world.GetPool<UnitStatsComponent>();
            _unitManaCostPool = world.GetPool<UnitManaCostComponent>();
            _passiveAbilityPool = world.GetPool<PassiveAbilityIdComponent>();
            _unitLevelPool = world.GetPool<UnitLevelComponent>();
            _upgradeCountPool = world.GetPool<UpgradeCountComponent>();
            _installedPinPool = world.GetPool<InstalledPinComponent>();
            _stagedPool = world.GetPool<StagedTraineeComponent>();
            _playbackPool = world.GetPool<PlinkoTrainingPlaybackComponent>();
            _unitTrainingStartedEventPool = world.GetPool<UnitTrainingStartedEvent>();
            _trainingPlaybackStartedEventPool = world.GetPool<TrainingPlaybackStartedEvent>();
            _offersChangedEventPool = world.GetPool<RetrainingShopOffersChangedEvent>();
        }

        public void Run(IEcsSystems systems)
        {
            var world = systems.GetWorld();
            if (!_runEntityIndex.TryGetRunEntity(out var runEntity))
            {
                return;
            }

            foreach (var eventEntity in _purchasedBatchFilter)
            {
                if (_phasePool.Get(runEntity).Value != Enums.PhaseType.RetrainingPhase)
                {
                    world.DelEntity(eventEntity);
                    continue;
                }

                var purchasedOffers = new List<int>();
                foreach (var offerEntity in _offerFilter)
                {
                    purchasedOffers.Add(offerEntity);
                }

                purchasedOffers.Sort((left, right) =>
                    _retrainingOfferPool.Get(left).OfferSlotIndex.CompareTo(_retrainingOfferPool.Get(right).OfferSlotIndex));

                var locationId = _locationPool.Get(runEntity).LocationId;
                var levelIndex = _levelPool.Get(runEntity).LevelIndex;
                var installedPins = BuildInstalledPinSnapshots();

                var startedCount = 0;
                foreach (var offerEntity in purchasedOffers)
                {
                    var offerSlotIndex = _retrainingOfferPool.Get(offerEntity).OfferSlotIndex;
                    var runtimeId = _offerOwnerUnitPool.Get(offerEntity).RuntimeId;
                    var trainingRun = _trainingPipelineService.PrepareRetraining(
                        runtimeId,
                        _unitTypeIdPool.Get(offerEntity).Value,
                        _displayNamePool.Get(offerEntity).Value,
                        _unitStatsPool.Get(offerEntity).Attack,
                        _unitStatsPool.Get(offerEntity).Health,
                        _unitManaCostPool.Get(offerEntity).Value,
                        _passiveAbilityPool.Get(offerEntity).Value,
                        _unitLevelPool.Get(offerEntity).Value,
                        _upgradeCountPool.Get(offerEntity).Value,
                        locationId,
                        levelIndex,
                        installedPins);
                    if (trainingRun == null)
                    {
                        continue;
                    }

                    var stagedEntity = world.NewEntity();
                    _stagedPool.Add(stagedEntity) = new StagedTraineeComponent
                    {
                        RuntimeId = runtimeId,
                        IsRetraining = true,
                        SourceOfferId = offerSlotIndex
                    };
                    _unitTypeIdPool.Add(stagedEntity).Value = _unitTypeIdPool.Get(offerEntity).Value;
                    _displayNamePool.Add(stagedEntity).Value = _displayNamePool.Get(offerEntity).Value;

                    var playbackEntity = world.NewEntity();
                    ref var playback = ref _playbackPool.Add(playbackEntity);
                    playback.RuntimeId = runtimeId;
                    playback.IsRetraining = true;
                    playback.StartDelay = offerSlotIndex * 0.5f;
                    playback.HasStarted = false;
                    playback.Duration = trainingRun.PlaybackDuration;
                    playback.Elapsed = 0f;
                    playback.CurrentNodeIndex = 0;
                    playback.TotalNodeCount = trainingRun.TotalNodeCount;
                    playback.IsCompleted = false;

                    _unitTrainingStartedEventPool.Add(world.NewEntity()).RuntimeId = runtimeId;
                    startedCount++;
                }

                ref var retrainingState = ref _retrainingStatePool.Get(runEntity);
                retrainingState.ActiveTrainingCount += startedCount;

                var eligibleOwnedEntities = RetrainingPhaseUtility.CollectEligibleOwnedEntities(world, _ownedUnitPool, _purchasedOnLevelPool);
                RetrainingPhaseUtility.GenerateBatch(
                    world,
                    retrainingState.OfferCount,
                    eligibleOwnedEntities,
                    _unitConfigService,
                    _ownedUnitPool,
                    _retrainingOfferPool,
                    _offerOwnerUnitPool,
                    world.GetPool<OfferPriceComponent>(),
                    _unitTypeIdPool,
                    _displayNamePool,
                    _unitStatsPool,
                    _unitManaCostPool,
                    _passiveAbilityPool,
                    _unitLevelPool,
                    _upgradeCountPool);

                _offersChangedEventPool.Add(world.NewEntity());
                world.DelEntity(eventEntity);
            }
        }

        private List<InstalledPinSnapshotModel> BuildInstalledPinSnapshots()
        {
            var installedPins = new List<InstalledPinSnapshotModel>();
            foreach (var pinEntity in _installedPinFilter)
            {
                var installedPin = _installedPinPool.Get(pinEntity);
                installedPins.Add(new InstalledPinSnapshotModel
                {
                    SlotIndex = installedPin.SlotIndex,
                    PinTypeId = installedPin.PinTypeId
                });
            }

            return installedPins;
        }
    }
}
