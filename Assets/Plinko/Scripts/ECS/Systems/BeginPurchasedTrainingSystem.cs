using System.Collections.Generic;
using Leopotam.EcsLite;
using Plinko.Scripts.ECS.Components;
using Plinko.Scripts.ECS.Events;
using Plinko.Scripts.ECS.Indexes;
using Plinko.Scripts.Models;
using Plinko.Scripts.Services;

namespace Plinko.Scripts.ECS.Systems
{
    public sealed class BeginPurchasedTrainingSystem : IEcsInitSystem, IEcsRunSystem
    {
        private readonly TrainingPipelineService _trainingPipelineService;
        private readonly RunEntityIndex _runEntityIndex;

        private EcsFilter _eventFilter;
        private EcsFilter _installedPinFilter;
        private EcsFilter _stagedFilter;
        private EcsPool<UnitPurchasedEvent> _unitPurchasedEventPool;
        private EcsPool<CurrentLocationComponent> _locationPool;
        private EcsPool<CurrentLevelComponent> _levelPool;
        private EcsPool<InstalledPinComponent> _installedPinPool;
        private EcsPool<StagedTraineeComponent> _stagedPool;
        private EcsPool<UnitTypeIdComponent> _unitTypeIdPool;
        private EcsPool<UnitDisplayNameComponent> _displayNamePool;
        private EcsPool<PlinkoTrainingPlaybackComponent> _playbackPool;
        private EcsPool<UnitTrainingStartedEvent> _unitTrainingStartedEventPool;
        private EcsPool<TrainingPlaybackStartedEvent> _trainingPlaybackStartedEventPool;

        public BeginPurchasedTrainingSystem(
            TrainingPipelineService trainingPipelineService,
            RunEntityIndex runEntityIndex)
        {
            _trainingPipelineService = trainingPipelineService;
            _runEntityIndex = runEntityIndex;
        }
        
        public void Init(IEcsSystems systems)
        {
            var world = systems.GetWorld();
            _eventFilter = world.Filter<UnitPurchasedEvent>().End();
            _installedPinFilter = world.Filter<InstalledPinComponent>().End();
            _stagedFilter = world.Filter<StagedTraineeComponent>().Inc<UnitTypeIdComponent>().Inc<UnitDisplayNameComponent>().End();
            _unitPurchasedEventPool = world.GetPool<UnitPurchasedEvent>();
            _locationPool = world.GetPool<CurrentLocationComponent>();
            _levelPool = world.GetPool<CurrentLevelComponent>();
            _installedPinPool = world.GetPool<InstalledPinComponent>();
            _stagedPool = world.GetPool<StagedTraineeComponent>();
            _unitTypeIdPool = world.GetPool<UnitTypeIdComponent>();
            _displayNamePool = world.GetPool<UnitDisplayNameComponent>();
            _playbackPool = world.GetPool<PlinkoTrainingPlaybackComponent>();
            _unitTrainingStartedEventPool = world.GetPool<UnitTrainingStartedEvent>();
            _trainingPlaybackStartedEventPool = world.GetPool<TrainingPlaybackStartedEvent>();
        }
        
        public void Run(IEcsSystems systems)
        {
            var world = systems.GetWorld();
            if (!_runEntityIndex.TryGetRunEntity(out var runEntity))
            {
                return;
            }

            var locationId = _locationPool.Get(runEntity).LocationId;
            var levelIndex = _levelPool.Get(runEntity).LevelIndex;
            var installedPins = BuildInstalledPinSnapshots();

            foreach (var eventEntity in _eventFilter)
            {
                var runtimeId = _unitPurchasedEventPool.Get(eventEntity).RuntimeId;
                var stagedEntity = -1;
                foreach (var entity in _stagedFilter)
                {
                    if (_stagedPool.Get(entity).RuntimeId == runtimeId && !_stagedPool.Get(entity).IsRetraining)
                    {
                        stagedEntity = entity;
                        break;
                    }
                }

                if (stagedEntity < 0)
                {
                    world.DelEntity(eventEntity);
                    continue;
                }

                if (!_trainingPipelineService.TryPreparePurchaseTraining(
                        runtimeId,
                        _unitTypeIdPool.Get(stagedEntity).Value,
                        _displayNamePool.Get(stagedEntity).Value,
                        locationId,
                        levelIndex,
                        installedPins,
                        out var trainingRun))
                {
                    world.DelEntity(eventEntity);
                    continue;
                }

                var playbackEntity = world.NewEntity();
                ref var playback = ref _playbackPool.Add(playbackEntity);
                playback.RuntimeId = runtimeId;
                playback.IsRetraining = false;
                playback.StartDelay = 0f;
                playback.HasStarted = true;
                playback.Duration = trainingRun.PlaybackDuration;
                playback.Elapsed = 0f;
                playback.CurrentNodeIndex = 0;
                playback.TotalNodeCount = trainingRun.TotalNodeCount;
                playback.IsCompleted = false;

                _unitTrainingStartedEventPool.Add(world.NewEntity()).RuntimeId = runtimeId;
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
