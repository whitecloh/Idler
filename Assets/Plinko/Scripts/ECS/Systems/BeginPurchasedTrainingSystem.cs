using System.Collections.Generic;
using Leopotam.EcsLite;
using Plinko.Scripts.Data.Pins;
using Plinko.Scripts.ECS.Components;
using Plinko.Scripts.ECS.Events;
using Plinko.Scripts.ECS.Indexes;
using Plinko.Scripts.Services;
using UnityEngine;

namespace Plinko.Scripts.ECS.Systems
{
    public sealed class BeginPurchasedTrainingSystem : IEcsInitSystem, IEcsRunSystem
    {
        private readonly UnitConfigService _unitConfigService;
        private readonly PinConfigService _pinConfigService;
        private readonly LocationConfigService _locationConfigService;
        private readonly LevelConfigService _levelConfigService;
        private readonly PlinkoConfigService _plinkoConfigService;
        private readonly PlinkoPathFactory _plinkoPathFactory;
        private readonly PlinkoRuntimeService _plinkoRuntimeService;
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
            UnitConfigService unitConfigService,
            PinConfigService pinConfigService,
            LocationConfigService locationConfigService,
            LevelConfigService levelConfigService,
            PlinkoConfigService plinkoConfigService,
            PlinkoPathFactory plinkoPathFactory,
            PlinkoRuntimeService plinkoRuntimeService,
            RunEntityIndex runEntityIndex)
        {
            _unitConfigService = unitConfigService;
            _pinConfigService = pinConfigService;
            _locationConfigService = locationConfigService;
            _levelConfigService = levelConfigService;
            _plinkoConfigService = plinkoConfigService;
            _plinkoPathFactory = plinkoPathFactory;
            _plinkoRuntimeService = plinkoRuntimeService;
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

                var location = _locationConfigService.GetLocation(_locationPool.Get(runEntity).LocationId);
                var levelData = _levelConfigService.GetLevel(_locationPool.Get(runEntity).LocationId, _levelPool.Get(runEntity).LevelIndex);
                var field = _plinkoConfigService.GetField(location, levelData);
                var installedPins = new Dictionary<int, PinTypeData>();
                foreach (var pinEntity in _installedPinFilter)
                {
                    var installedPin = _installedPinPool.Get(pinEntity);
                    var pinType = _pinConfigService.GetPin(installedPin.PinTypeId);
                    if (pinType != null)
                    {
                        installedPins[installedPin.SlotIndex] = pinType;
                    }
                }

                var unitType = _unitConfigService.GetUnit(_unitTypeIdPool.Get(stagedEntity).Value);
                var result = _plinkoPathFactory.GeneratePurchaseResult(
                    runtimeId,
                    unitType,
                    _displayNamePool.Get(stagedEntity).Value,
                    field,
                    installedPins);
                _plinkoRuntimeService.SetResult(runtimeId, result);

                var playbackEntity = world.NewEntity();
                ref var playback = ref _playbackPool.Add(playbackEntity);
                playback.RuntimeId = runtimeId;
                playback.IsRetraining = false;
                playback.Duration = Mathf.Max(0.75f, result != null && result.Nodes != null ? result.Nodes.Count * 0.2f : 0.75f);
                playback.Elapsed = 0f;
                playback.CurrentNodeIndex = 0;
                playback.TotalNodeCount = result != null && result.Nodes != null ? result.Nodes.Count : 0;
                playback.IsCompleted = false;

                _unitTrainingStartedEventPool.Add(world.NewEntity()).RuntimeId = runtimeId;
                _trainingPlaybackStartedEventPool.Add(world.NewEntity()).RuntimeId = runtimeId;
                world.DelEntity(eventEntity);
            }
        }
    }
}