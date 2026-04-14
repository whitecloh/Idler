using Leopotam.EcsLite;
using Plinko.Scripts.Data.Common;
using Plinko.Scripts.ECS.Components;
using Plinko.Scripts.ECS.Events;
using Plinko.Scripts.ECS.Indexes;
using Plinko.Scripts.ECS.Requests;
using Plinko.Scripts.ECS.Utils;
using Plinko.Scripts.Services;

namespace Plinko.Scripts.ECS.Systems
{
    public sealed class StartNewRunSystem : IEcsInitSystem, IEcsRunSystem
    {
        private readonly GameSettingsService _gameSettingsService;
        private readonly RunEntityIndex _runEntityIndex;

        private EcsFilter _requestFilter;
        private EcsPool<StartNewRunRequest> _requestPool;
        private EcsPool<RunComponent> _runPool;
        private EcsPool<CurrentLocationComponent> _locationPool;
        private EcsPool<CurrentLevelComponent> _levelPool;
        private EcsPool<CurrentLevelTypeComponent> _levelTypePool;
        private EcsPool<CurrentPhaseComponent> _phasePool;
        private EcsPool<CurrentGoldComponent> _goldPool;
        private EcsPool<PlayerBaseHealthComponent> _playerBaseHealthPool;
        private EcsPool<RunStatusComponent> _runStatusPool;
        private EcsPool<RunStartedEvent> _runStartedPool;
        private EcsPool<GoldChangedEvent> _goldChangedPool;

        public StartNewRunSystem(GameSettingsService gameSettingsService, RunEntityIndex runEntityIndex)
        {
            _gameSettingsService = gameSettingsService;
            _runEntityIndex = runEntityIndex;
        }

        public void Init(IEcsSystems systems)
        {
            var world = systems.GetWorld();
            _requestFilter = world.Filter<StartNewRunRequest>().End();
            _requestPool = world.GetPool<StartNewRunRequest>();
            _runPool = world.GetPool<RunComponent>();
            _locationPool = world.GetPool<CurrentLocationComponent>();
            _levelPool = world.GetPool<CurrentLevelComponent>();
            _levelTypePool = world.GetPool<CurrentLevelTypeComponent>();
            _phasePool = world.GetPool<CurrentPhaseComponent>();
            _goldPool = world.GetPool<CurrentGoldComponent>();
            _playerBaseHealthPool = world.GetPool<PlayerBaseHealthComponent>();
            _runStatusPool = world.GetPool<RunStatusComponent>();
            _runStartedPool = world.GetPool<RunStartedEvent>();
            _goldChangedPool = world.GetPool<GoldChangedEvent>();
        }

        public void Run(IEcsSystems systems)
        {
            var world = systems.GetWorld();
            foreach (var requestEntity in _requestFilter)
            {
                ref var request = ref _requestPool.Get(requestEntity);

                var runEntity = GetOrCreateRunEntity(world);
                WriteRunState(runEntity, request.LocationId);

                _runStartedPool.Add(world.NewEntity());
                _goldChangedPool.Add(world.NewEntity()).Value = _goldPool.Get(runEntity).Value;
                world.DelEntity(requestEntity);
            }
        }
        
        private int GetOrCreateRunEntity(EcsWorld world)
        {
            if (_runEntityIndex.TryGetRunEntity(out var runEntity))
            {
                return runEntity;
            }

            runEntity = world.NewEntity();
            _runPool.Add(runEntity);
            _runEntityIndex.SetRunEntity(runEntity);
            return runEntity;
        }

        private void WriteRunState(int runEntity, string locationId)
        {
            _locationPool.GetOrAdd(runEntity).LocationId = locationId;
            _levelPool.GetOrAdd(runEntity).LevelIndex = 0;
            _levelTypePool.GetOrAdd(runEntity).Value = Enums.LevelType.None;
            _phasePool.GetOrAdd(runEntity).Value = Enums.PhaseType.Location;
            _goldPool.GetOrAdd(runEntity).Value = _gameSettingsService.GetStartingGold();
            _playerBaseHealthPool.GetOrAdd(runEntity).Value = _gameSettingsService.GetStartingBaseHealth();
            _runStatusPool.GetOrAdd(runEntity).Value = Enums.RunStatus.InProgress;
        }
    }
}