using System.Collections.Generic;
using Leopotam.EcsLite;
using Plinko.Scripts.Data.Common;
using Plinko.Scripts.ECS.Components;
using Plinko.Scripts.ECS.Events;
using Plinko.Scripts.ECS.Indexes;
using Plinko.Scripts.ECS.Requests;
using Plinko.Scripts.Models;
using Plinko.Scripts.Services;

namespace Plinko.Scripts.ECS.Systems
{
    public sealed class LaunchSignalSystem : IEcsInitSystem, IEcsRunSystem
    {
        private const float InitialSignalLaunchDelay = 1f;

        private readonly TrainingPipelineService _trainingPipelineService;
        private readonly RunEntityIndex _runEntityIndex;

        private EcsFilter _requestFilter;
        private EcsFilter _installedPinFilter;
        private EcsFilter _pendingFilter;
        private EcsPool<LaunchSignalRequest> _requestPool;
        private EcsPool<CurrentPhaseComponent> _phasePool;
        private EcsPool<CurrentLocationComponent> _locationPool;
        private EcsPool<CurrentLevelComponent> _levelPool;
        private EcsPool<SignalPurchasePhaseStateComponent> _signalPurchasePool;
        private EcsPool<InstalledPinComponent> _installedPinPool;
        private EcsPool<StagedTraineeComponent> _stagedPool;
        private EcsPool<SignalPendingSlotComponent> _signalPendingSlotPool;
        private EcsPool<UnitTypeIdComponent> _unitTypeIdPool;
        private EcsPool<UnitDisplayNameComponent> _displayNamePool;
        private EcsPool<PlinkoTrainingPlaybackComponent> _playbackPool;
        private EcsPool<SignalLaunchStartedEvent> _signalLaunchStartedEventPool;

        public LaunchSignalSystem(
            TrainingPipelineService trainingPipelineService,
            RunEntityIndex runEntityIndex)
        {
            _trainingPipelineService = trainingPipelineService;
            _runEntityIndex = runEntityIndex;
        }

        public void Init(IEcsSystems systems)
        {
            var world = systems.GetWorld();
            _requestFilter = world.Filter<LaunchSignalRequest>().End();
            _installedPinFilter = world.Filter<InstalledPinComponent>().End();
            _pendingFilter = world.Filter<SignalPendingUnitComponent>().Inc<SignalPendingSlotComponent>().Inc<StagedTraineeComponent>().Inc<UnitTypeIdComponent>().Inc<UnitDisplayNameComponent>().End();
            _requestPool = world.GetPool<LaunchSignalRequest>();
            _phasePool = world.GetPool<CurrentPhaseComponent>();
            _locationPool = world.GetPool<CurrentLocationComponent>();
            _levelPool = world.GetPool<CurrentLevelComponent>();
            _signalPurchasePool = world.GetPool<SignalPurchasePhaseStateComponent>();
            _installedPinPool = world.GetPool<InstalledPinComponent>();
            _stagedPool = world.GetPool<StagedTraineeComponent>();
            _signalPendingSlotPool = world.GetPool<SignalPendingSlotComponent>();
            _unitTypeIdPool = world.GetPool<UnitTypeIdComponent>();
            _displayNamePool = world.GetPool<UnitDisplayNameComponent>();
            _playbackPool = world.GetPool<PlinkoTrainingPlaybackComponent>();
            _signalLaunchStartedEventPool = world.GetPool<SignalLaunchStartedEvent>();
        }

        public void Run(IEcsSystems systems)
        {
            var world = systems.GetWorld();
            if (!_runEntityIndex.TryGetRunEntity(out var runEntity))
            {
                return;
            }

            foreach (var requestEntity in _requestFilter)
            {
                if (_phasePool.Get(runEntity).Value != Enums.PhaseType.SignalPurchasePhase)
                {
                    world.DelEntity(requestEntity);
                    continue;
                }

                ref var state = ref _signalPurchasePool.Get(runEntity);
                if (state.IsGeneratorBroken || state.ActiveTrainingCount > 0)
                {
                    world.DelEntity(requestEntity);
                    continue;
                }

                var pendingUnits = BuildPendingUnits();
                if (pendingUnits.Count == 0)
                {
                    world.DelEntity(requestEntity);
                    continue;
                }

                var installedPins = BuildInstalledPinSnapshots();
                var locationId = _locationPool.Get(runEntity).LocationId;
                var levelIndex = _levelPool.Get(runEntity).LevelIndex;
                var delay = InitialSignalLaunchDelay;
                var preparedCount = 0;

                for (var index = 0; index < pendingUnits.Count; index++)
                {
                    var pendingEntity = pendingUnits[index];
                    var runtimeId = _stagedPool.Get(pendingEntity).RuntimeId;
                    if (!_trainingPipelineService.TryPreparePurchaseTraining(
                            runtimeId,
                            _unitTypeIdPool.Get(pendingEntity).Value,
                            _displayNamePool.Get(pendingEntity).Value,
                            locationId,
                            levelIndex,
                            installedPins,
                            out var trainingRun))
                    {
                        continue;
                    }

                    var playbackEntity = world.NewEntity();
                    ref var playback = ref _playbackPool.Add(playbackEntity);
                    playback.RuntimeId = runtimeId;
                    playback.IsRetraining = false;
                    playback.StartDelay = delay;
                    playback.HasStarted = false;
                    playback.Duration = trainingRun.PlaybackDuration;
                    playback.Elapsed = 0f;
                    playback.CurrentNodeIndex = 0;
                    playback.TotalNodeCount = trainingRun.TotalNodeCount;
                    playback.IsCompleted = false;

                    delay += trainingRun.PlaybackDuration;
                    preparedCount++;
                }

                if (preparedCount > 0)
                {
                    state.ActiveTrainingCount = preparedCount;
                    state.SignalsLaunchedCount++;
                    state.WillBreakAfterCurrentSignal = state.SignalsLaunchedCount >= state.GeneratorBreakAfterSignalCount;
                    _signalLaunchStartedEventPool.Add(world.NewEntity()).WillBreakGenerator = state.WillBreakAfterCurrentSignal;
                }

                world.DelEntity(requestEntity);
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

        private List<int> BuildPendingUnits()
        {
            var result = new List<int>();
            foreach (var entity in _pendingFilter)
            {
                if (!_stagedPool.Get(entity).IsRetraining)
                {
                    result.Add(entity);
                }
            }

            result.Sort((left, right) => _signalPendingSlotPool.Get(left).Value.CompareTo(_signalPendingSlotPool.Get(right).Value));
            return result;
        }
    }
}
