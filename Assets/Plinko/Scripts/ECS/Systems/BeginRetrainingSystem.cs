using System.Collections.Generic;
using Leopotam.EcsLite;
using Plinko.Scripts.Data.Common;
using Plinko.Scripts.ECS.Components;
using Plinko.Scripts.ECS.Events;
using Plinko.Scripts.ECS.Indexes;
using Plinko.Scripts.Models;
using Plinko.Scripts.Services;

namespace Plinko.Scripts.ECS.Systems
{
    public sealed class BeginRetrainingSystem : IEcsInitSystem, IEcsRunSystem
    {
        private readonly TrainingPipelineService _trainingPipelineService;
        private readonly RunEntityIndex _runEntityIndex;

        private EcsFilter _confirmedFilter;
        private EcsFilter _selectedFilter;
        private EcsFilter _installedPinFilter;
        private EcsPool<CurrentPhaseComponent> _phasePool;
        private EcsPool<CurrentLocationComponent> _locationPool;
        private EcsPool<CurrentLevelComponent> _levelPool;
        private EcsPool<RetrainingPhaseStateComponent> _retrainingStatePool;
        private EcsPool<SelectedForRetrainingComponent> _selectedForRetrainingPool;
        private EcsPool<OwnedUnitComponent> _ownedUnitPool;
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
        private EcsPool<RetrainingSelectionChangedEvent> _selectionChangedEventPool;

        public BeginRetrainingSystem(
            TrainingPipelineService trainingPipelineService,
            RunEntityIndex runEntityIndex)
        {
            _trainingPipelineService = trainingPipelineService;
            _runEntityIndex = runEntityIndex;
        }

        public void Init(IEcsSystems systems)
        {
            var world = systems.GetWorld();
            _confirmedFilter = world.Filter<RetrainingSelectionConfirmedEvent>().End();
            _selectedFilter = world.Filter<OwnedUnitComponent>().Inc<SelectedForRetrainingComponent>().End();
            _installedPinFilter = world.Filter<InstalledPinComponent>().End();
            _phasePool = world.GetPool<CurrentPhaseComponent>();
            _locationPool = world.GetPool<CurrentLocationComponent>();
            _levelPool = world.GetPool<CurrentLevelComponent>();
            _retrainingStatePool = world.GetPool<RetrainingPhaseStateComponent>();
            _selectedForRetrainingPool = world.GetPool<SelectedForRetrainingComponent>();
            _ownedUnitPool = world.GetPool<OwnedUnitComponent>();
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
            _selectionChangedEventPool = world.GetPool<RetrainingSelectionChangedEvent>();
        }

        public void Run(IEcsSystems systems)
        {
            var world = systems.GetWorld();
            if (!_runEntityIndex.TryGetRunEntity(out var runEntity))
            {
                return;
            }

            foreach (var eventEntity in _confirmedFilter)
            {
                if (_phasePool.Get(runEntity).Value != Enums.PhaseType.RetrainingPhase)
                {
                    world.DelEntity(eventEntity);
                    continue;
                }

                var selectedEntities = new List<int>();
                foreach (var selectedEntity in _selectedFilter)
                {
                    selectedEntities.Add(selectedEntity);
                }

                var locationId = _locationPool.Get(runEntity).LocationId;
                var levelIndex = _levelPool.Get(runEntity).LevelIndex;
                var installedPins = BuildInstalledPinSnapshots();

                var startedCount = 0;
                foreach (var selectedEntity in selectedEntities)
                {
                    var runtimeId = _ownedUnitPool.Get(selectedEntity).RuntimeId;
                    var trainingRun = _trainingPipelineService.PrepareRetraining(
                        runtimeId,
                        _unitTypeIdPool.Get(selectedEntity).Value,
                        _displayNamePool.Get(selectedEntity).Value,
                        _unitStatsPool.Get(selectedEntity).Attack,
                        _unitStatsPool.Get(selectedEntity).Health,
                        _unitManaCostPool.Get(selectedEntity).Value,
                        _passiveAbilityPool.Get(selectedEntity).Value,
                        _unitLevelPool.Get(selectedEntity).Value,
                        _upgradeCountPool.Get(selectedEntity).Value,
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
                        SourceOfferId = -1
                    };
                    _unitTypeIdPool.Add(stagedEntity).Value = _unitTypeIdPool.Get(selectedEntity).Value;
                    _displayNamePool.Add(stagedEntity).Value = _displayNamePool.Get(selectedEntity).Value;

                    var playbackEntity = world.NewEntity();
                    ref var playback = ref _playbackPool.Add(playbackEntity);
                    playback.RuntimeId = runtimeId;
                    playback.IsRetraining = true;
                    playback.Duration = trainingRun.PlaybackDuration;
                    playback.Elapsed = 0f;
                    playback.CurrentNodeIndex = 0;
                    playback.TotalNodeCount = trainingRun.TotalNodeCount;
                    playback.IsCompleted = false;

                    _selectedForRetrainingPool.Del(selectedEntity);
                    _unitTrainingStartedEventPool.Add(world.NewEntity()).RuntimeId = runtimeId;
                    _trainingPlaybackStartedEventPool.Add(world.NewEntity()).RuntimeId = runtimeId;
                    startedCount++;
                }

                ref var retrainingState = ref _retrainingStatePool.Get(runEntity);
                retrainingState.SelectedCount = 0;
                retrainingState.ActiveTrainingCount = startedCount;
                if (startedCount <= 0)
                {
                    retrainingState.IsSelectionLocked = false;
                }

                _selectionChangedEventPool.Add(world.NewEntity()).SelectedCount = 0;
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
